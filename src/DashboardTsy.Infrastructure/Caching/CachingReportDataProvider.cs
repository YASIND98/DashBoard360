using DashboardTsy.Application;
using DashboardTsy.Application.AiInsight.Requests;
using DashboardTsy.Application.AiInsight.Responses;
using DashboardTsy.Application.Caching;
using DashboardTsy.Application.ExchangeRate.Requests;
using DashboardTsy.Application.ExchangeRate.Responses;
using DashboardTsy.Application.ProductivityReport.Requests;
using DashboardTsy.Application.ProductivityReport.Responses;
using DashboardTsy.Application.TargetReport.Requests;
using DashboardTsy.Application.TargetReport.Responses;
using Microsoft.Extensions.Configuration;

namespace DashboardTsy.Infrastructure.Caching;

/// <summary>
/// IReportDataProvider decorator'ı: SP çağrılarını read-through cache ile sarar.
/// - Target* metotları bir sonraki 08:50 Türkiye saatine kadar cache'lenir (prefix: "Target").
///   Yani cache girdisi her gün 08:50'de otomatik düşer; sabah ilk çağrı SP'yi tetikler,
///   gün içindeki tekrar çağrılar cache'ten döner.
/// - Productivity* metotları yazıldığı andan itibaren 30 gün cache'lenir (prefix: "Productivity").
/// - Filtre/lookup/AI/kur metotları cache'lenmeden inner'a delege edilir (kapsam dışı).
/// - Mock mod (ReportMock:Enabled=true) aktifken decorator devre dışıdır; inner mock veriyi doğrudan döner.
/// </summary>
public sealed class CachingReportDataProvider : IReportDataProvider
{
    public const string TargetPrefix = "Target";
    public const string ProductivityPrefix = "Productivity";

    // Target cache'inin her gün sıfırlanma zamanı (Türkiye saati).
    private static readonly TimeSpan TargetDailyResetTime = new(8, 50, 0);

    private static readonly TimeSpan ProductivityTtl = TimeSpan.FromDays(30);

    // Türkiye saatini explicit çöz: sunucu farklı bir zaman dilimindeyse de doğru davransın.
    // .NET 8 hem Windows ("Turkey Standard Time") hem IANA ("Europe/Istanbul") anahtarını tanır,
    // ama TZ verisi eksik bir imaj olasılığına karşı ikisini birden deniyoruz.
    private static readonly TimeZoneInfo TurkeyTimeZone = ResolveTurkeyTimeZone();

    private readonly IReportDataProvider _inner;
    private readonly ICacheStore _cache;
    private readonly ICacheKeyBuilder _keyBuilder;
    private readonly IConfiguration _configuration;

    public CachingReportDataProvider(
        IReportDataProvider inner,
        ICacheStore cache,
        ICacheKeyBuilder keyBuilder,
        IConfiguration configuration)
    {
        _inner = inner;
        _cache = cache;
        _keyBuilder = keyBuilder;
        _configuration = configuration;
    }

    // Mock mod inner ile aynı kuralı kullanır. Mock veri deterministik olduğundan cache'lemenin faydası yok.
    private bool MockEnabled => _configuration["ReportMock:Enabled"] is string v
                                && bool.TryParse(v, out var b) && b;

    // Ortak read-through helper: mock aktifse doğrudan inner, aksi halde cache'e sor / miss'te yaz.
    // TTL bir Func — set anında taze hesaplanır. Böylece Target gibi "sonraki 08:50'ye kadar" gibi
    // dinamik hedef zamanlar dogal olarak ifade edilir.
    private T Cached<T>(string prefix, Func<TimeSpan> ttlProvider, string methodName, object? arguments, Func<T> loader)
    {
        if (MockEnabled)
            return loader();

        var key = _keyBuilder.Build(prefix, methodName, arguments);
        if (_cache.TryGet<T>(key, out var cached) && cached is not null)
            return cached;

        var value = loader();
        if (value is not null)
            _cache.Set(key, value, ttlProvider());
        return value;
    }

    // Primitive parametreleri anonim nesneye sararak deterministik key üretimi.
    private T CachedTarget<T>(string methodName, object? args, Func<T> loader)
        => Cached(TargetPrefix, TimeUntilNextTargetReset, methodName, args, loader);

    private T CachedProductivity<T>(string methodName, object? args, Func<T> loader)
        => Cached(ProductivityPrefix, () => ProductivityTtl, methodName, args, loader);

    /// <summary>
    /// Şu andan itibaren bir sonraki Türkiye saati 08:50'ye kadar geçen süreyi döner.
    /// 08:50'den önce çağrılırsa aynı günün 08:50'sine, sonra çağrılırsa ertesi günün 08:50'sine hesaplar.
    /// TTL asla 0 olmaz — 08:49:59.999'da cache'lenen bir girdi bile en az birkaç milisaniye yaşar.
    /// </summary>
    internal static TimeSpan TimeUntilNextTargetReset()
        => ComputeTimeUntilNextReset(DateTimeOffset.UtcNow, TargetDailyResetTime, TurkeyTimeZone);

    // Saf, deterministik yardımcı — test edilebilir olması için "now" parametreli.
    // DateTimeOffset.UtcNow'ı üstteki wrapper'da veriyoruz; burada dış dünyaya bağımlılık yok.
    internal static TimeSpan ComputeTimeUntilNextReset(DateTimeOffset utcNow, TimeSpan resetTimeOfDay, TimeZoneInfo timeZone)
    {
        var localNow = TimeZoneInfo.ConvertTime(utcNow, timeZone);
        var todayReset = new DateTimeOffset(
            localNow.Year, localNow.Month, localNow.Day,
            resetTimeOfDay.Hours, resetTimeOfDay.Minutes, resetTimeOfDay.Seconds,
            localNow.Offset);

        var nextReset = localNow < todayReset ? todayReset : todayReset.AddDays(1);
        return nextReset - localNow;
    }

    private static TimeZoneInfo ResolveTurkeyTimeZone()
    {
        // Windows kimliği, sonra IANA. .NET 8 tarafında cross-mapping var ama bir imajda TZ verisi eksik
        // olabileceğinden ikisini de deniyoruz; her ikisi de patlarsa UTC+3 fallback'i (Türkiye 2016'dan beri sabit +03:00).
        foreach (var id in new[] { "Turkey Standard Time", "Europe/Istanbul" })
        {
            try { return TimeZoneInfo.FindSystemTimeZoneById(id); }
            catch (TimeZoneNotFoundException) { }
            catch (InvalidTimeZoneException) { }
        }

        return TimeZoneInfo.CreateCustomTimeZone(
            id: "Turkey UTC+3 fallback",
            baseUtcOffset: TimeSpan.FromHours(3),
            displayName: "Turkey (fixed +03:00)",
            standardDisplayName: "Turkey");
    }

    // ============================================================
    // TARGET — 24 saat
    // ============================================================

    public GetTargetReportMenuTextsResponse? GetTargetReportMenuTexts(string sessionId)
        => CachedTarget(nameof(GetTargetReportMenuTexts),
            args: null, // sessionId cache key'ine dahil edilmiyor (SessionId hariç kuralı)
            loader: () => _inner.GetTargetReportMenuTexts(sessionId));

    public IReadOnlyList<GetTargetReportFiltersItem> GetTargetReportFilters(string sessionId, int filterId, List<string>? filterCode)
        => CachedTarget(nameof(GetTargetReportFilters),
            new { filterId, filterCode }, // sessionId hariç
            () => _inner.GetTargetReportFilters(sessionId, filterId, filterCode));

    public GetDailyTargetReportResponse GetDailyTargetReport(GetDailyTargetReportRequest request)
        => CachedTarget(nameof(GetDailyTargetReport), request,
            () => _inner.GetDailyTargetReport(request));

    public GetDailyQuantityTargetReportTableHeadersResponse? GetDailyQuantityTargetReportTableHeaders(GetDailyQuantityTargetReportTableHeadersRequest request)
        => CachedTarget(nameof(GetDailyQuantityTargetReportTableHeaders), request,
            () => _inner.GetDailyQuantityTargetReportTableHeaders(request));

    public GetDailyQuantityTargetReportResponse GetDailyQuantityTargetReport(GetDailyQuantityTargetReportRequest request)
        => CachedTarget(nameof(GetDailyQuantityTargetReport), request,
            () => _inner.GetDailyQuantityTargetReport(request));

    public ProductTop10DifferencesResponse GetProductTop10DailyAndWeeklyDifferences(GetProductTop10DailyAndWeeklyDifferencesRequest request)
        => CachedTarget(nameof(GetProductTop10DailyAndWeeklyDifferences), request,
            () => _inner.GetProductTop10DailyAndWeeklyDifferences(request));

    public GetDailyTargetReportTableHeadersResponse? GetDailyTargetReportTableHeaders(string sessionId)
        => CachedTarget(nameof(GetDailyTargetReportTableHeaders),
            args: null,
            loader: () => _inner.GetDailyTargetReportTableHeaders(sessionId));

    public GetMonthlyTargetReportResponse? GetMonthlyTargetReport(GetMonthlyTargetReportRequest request)
        => CachedTarget(nameof(GetMonthlyTargetReport), request,
            () => _inner.GetMonthlyTargetReport(request));

    public GetMonthlyTargetReportTableHeadersResponse? GetMonthlyTargetReportTableHeaders(GetMonthlyTargetReportTableHeadersRequest request)
        => CachedTarget(nameof(GetMonthlyTargetReportTableHeaders), request,
            () => _inner.GetMonthlyTargetReportTableHeaders(request));

    public IReadOnlyList<GetVolumeTrendAnalysisItem> GetVolumeTrendAnalysis(GetTrendAnalysisRequest request)
        => CachedTarget(nameof(GetVolumeTrendAnalysis), request,
            () => _inner.GetVolumeTrendAnalysis(request));

    public IReadOnlyList<GetQuantityTrendAnalysisItem> GetQuantityTrendAnalysis(GetTrendAnalysisRequest request)
        => CachedTarget(nameof(GetQuantityTrendAnalysis), request,
            () => _inner.GetQuantityTrendAnalysis(request));

    // ============================================================
    // PRODUCTIVITY — 30 gün
    // ============================================================

    public IReadOnlyList<GetProductivityReportTabItem> GetProductivityReportTabs(GetProductivityReportTabsRequest request)
        => CachedProductivity(nameof(GetProductivityReportTabs), request,
            () => _inner.GetProductivityReportTabs(request));

    public IReadOnlyList<GetProductivityReportTableHeaderItem> GetProductivityReportTableHeaders(GetProductivityReportTableHeadersRequest request)
        => CachedProductivity(nameof(GetProductivityReportTableHeaders), request,
            () => _inner.GetProductivityReportTableHeaders(request));

    public GetProductivityGeneralRegionReportResponse? GetProductivityGeneralRegionReport(GetProductivityGeneralRegionReportRequest request)
        => CachedProductivity(nameof(GetProductivityGeneralRegionReport), request,
            () => _inner.GetProductivityGeneralRegionReport(request));

    public GetProductivityCountCardPosRegionReportResponse? GetProductivityCountCardPosRegionReport(GetProductivityCountCardPosRegionReportRequest request)
        => CachedProductivity(nameof(GetProductivityCountCardPosRegionReport), request,
            () => _inner.GetProductivityCountCardPosRegionReport(request));

    public IReadOnlyList<GetProductivityCountCardPosRatioRegionReportItem> GetProductivityCountCardPosRatioRegionReport(GetProductivityCountCardPosRatioRegionReportRequest request)
        => CachedProductivity(nameof(GetProductivityCountCardPosRatioRegionReport), request,
            () => _inner.GetProductivityCountCardPosRatioRegionReport(request));

    public GetProductivityCountCardPosRatioRegionReportTableHeadersItem? GetProductivityCountCardPosRatioRegionReportTableHeaders(GetProductivityCountCardPosRatioRegionReportTableHeadersRequest request)
        => CachedProductivity(nameof(GetProductivityCountCardPosRatioRegionReportTableHeaders), request,
            () => _inner.GetProductivityCountCardPosRatioRegionReportTableHeaders(request));

    public GetProductivityCountCustomerRegionReportResponse? GetProductivityCountCustomerRegionReport(GetProductivityCountCustomerRegionReportRequest request)
        => CachedProductivity(nameof(GetProductivityCountCustomerRegionReport), request,
            () => _inner.GetProductivityCountCustomerRegionReport(request));

    public GetProductivityCountCashManagementRegionReportResponse? GetProductivityCountCashManagementRegionReport(GetProductivityCountCashManagementRegionReportRequest request)
        => CachedProductivity(nameof(GetProductivityCountCashManagementRegionReport), request,
            () => _inner.GetProductivityCountCashManagementRegionReport(request));

    public GetProductivityVolumeRegionReportResponse GetProductivityVolumeRegionReport(GetProductivityVolumeRegionReportRequest request)
        => CachedProductivity(nameof(GetProductivityVolumeRegionReport), request,
            () => _inner.GetProductivityVolumeRegionReport(request));

    public GetProductivityProfitRatioRegionReportResponse GetProductivityProfitRatioRegionReport(GetProductivityProfitRatioRegionReportRequest request)
        => CachedProductivity(nameof(GetProductivityProfitRatioRegionReport), request,
            () => _inner.GetProductivityProfitRatioRegionReport(request));

    public GetProductivityProfitTotalRegionReportResponse GetProductivityProfitTotalRegionReport(GetProductivityProfitTotalRegionReportRequest request)
        => CachedProductivity(nameof(GetProductivityProfitTotalRegionReport), request,
            () => _inner.GetProductivityProfitTotalRegionReport(request));

    public GetProductivityProfitSpreadManagementRegionReportResponse GetProductivityProfitSpreadManagementRegionReport(GetProductivityProfitSpreadManagementRegionReportRequest request)
        => CachedProductivity(nameof(GetProductivityProfitSpreadManagementRegionReport), request,
            () => _inner.GetProductivityProfitSpreadManagementRegionReport(request));

    public GetProductivityProfitSpreadManagementBranchReportResponse? GetProductivityProfitSpreadManagementBranchReport(GetProductivityProfitSpreadManagementBranchReportRequest request)
        => CachedProductivity(nameof(GetProductivityProfitSpreadManagementBranchReport), request,
            () => _inner.GetProductivityProfitSpreadManagementBranchReport(request));

    public GetProductivityCountCardPosBranchReportResponse? GetProductivityCountCardPosBranchReport(GetProductivityCountCardPosBranchReportRequest request)
        => CachedProductivity(nameof(GetProductivityCountCardPosBranchReport), request,
            () => _inner.GetProductivityCountCardPosBranchReport(request));

    public IReadOnlyList<GetProductivityCountCardPosRatioBranchReportItem> GetProductivityCountCardPosRatioBranchReport(GetProductivityCountCardPosRatioBranchReportRequest request)
        => CachedProductivity(nameof(GetProductivityCountCardPosRatioBranchReport), request,
            () => _inner.GetProductivityCountCardPosRatioBranchReport(request));

    public GetProductivityCountCardPosRatioBranchReportTableHeadersItem? GetProductivityCountCardPosRatioBranchReportTableHeaders(GetProductivityCountCardPosRatioBranchReportTableHeadersRequest request)
        => CachedProductivity(nameof(GetProductivityCountCardPosRatioBranchReportTableHeaders), request,
            () => _inner.GetProductivityCountCardPosRatioBranchReportTableHeaders(request));

    public GetProductivityProfitRatioBranchReportResponse? GetProductivityProfitRatioBranchReport(GetProductivityProfitRatioBranchReportRequest request)
        => CachedProductivity(nameof(GetProductivityProfitRatioBranchReport), request,
            () => _inner.GetProductivityProfitRatioBranchReport(request));

    public GetProductivityProfitTotalBranchReportResponse? GetProductivityProfitTotalBranchReport(GetProductivityProfitTotalBranchReportRequest request)
        => CachedProductivity(nameof(GetProductivityProfitTotalBranchReport), request,
            () => _inner.GetProductivityProfitTotalBranchReport(request));

    public GetProductivityBranchScoreCardReportItem? GetProductivityBranchScoreCardReport(GetProductivityBranchScoreCardReportRequest request)
        => CachedProductivity(nameof(GetProductivityBranchScoreCardReport), request,
            () => _inner.GetProductivityBranchScoreCardReport(request));

    public GetProductivityRegionScoreCardReportItem? GetProductivityRegionScoreCardReport(GetProductivityRegionScoreCardReportRequest request)
        => CachedProductivity(nameof(GetProductivityRegionScoreCardReport), request,
            () => _inner.GetProductivityRegionScoreCardReport(request));

    public GetProductivityCountCustomerBranchReportResponse? GetProductivityCountCustomerBranchReport(GetProductivityCountCustomerBranchReportRequest request)
        => CachedProductivity(nameof(GetProductivityCountCustomerBranchReport), request,
            () => _inner.GetProductivityCountCustomerBranchReport(request));

    public GetProductivityCountCashManagementBranchReportResponse? GetProductivityCountCashManagementBranchReport(GetProductivityCountCashManagementBranchReportRequest request)
        => CachedProductivity(nameof(GetProductivityCountCashManagementBranchReport), request,
            () => _inner.GetProductivityCountCashManagementBranchReport(request));

    public GetProductivityVolumeBranchReportResponse? GetProductivityVolumeBranchReport(GetProductivityVolumeBranchReportRequest request)
        => CachedProductivity(nameof(GetProductivityVolumeBranchReport), request,
            () => _inner.GetProductivityVolumeBranchReport(request));

    public IReadOnlyList<GetProductivityScoreCardReportHeaderItem> GetProductivityScoreCardReportHeaders(GetProductivityScoreCardReportHeadersRequest request)
        => CachedProductivity(nameof(GetProductivityScoreCardReportHeaders), request,
            () => _inner.GetProductivityScoreCardReportHeaders(request));

    // ============================================================
    // PASS-THROUGH — cache YOK (ortak filtre/lookup/AI/kur)
    // ============================================================

    public IReadOnlyList<GetReportRegionFilterItem> GetReportRegionFilters(GetReportRegionFiltersRequest request)
        => _inner.GetReportRegionFilters(request);

    public IReadOnlyList<GetReportBranchFilterItem> GetReportBranchFilters(GetReportBranchFiltersRequest request)
        => _inner.GetReportBranchFilters(request);

    public IReadOnlyList<GetReportSidebarItem> GetReportSidebarItems(GetReportSidebarItemsRequest request)
        => _inner.GetReportSidebarItems(request);

    public IReadOnlyList<GetReportDatesItem> GetReportDates()
        => _inner.GetReportDates();

    public GetBranchAiInsightResponse GetBranchAiInsights(GetBranchAiInsightRequest request)
        => _inner.GetBranchAiInsights(request);

    public GetUsdExchangeRatesResponse? GetUsdExchangeRates(GetUsdExchangeRatesRequest request)
        => _inner.GetUsdExchangeRates(request);
}
