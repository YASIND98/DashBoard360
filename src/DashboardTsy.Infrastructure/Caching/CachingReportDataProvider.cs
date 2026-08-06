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
/// - Target* metotları 24 saat sliding TTL ile cache'lenir (prefix: "Target").
/// - Productivity* metotları 30 gün sliding TTL ile cache'lenir (prefix: "Productivity").
/// - Filtre/lookup/AI/kur metotları cache'lenmeden inner'a delege edilir (kapsam dışı).
/// - Mock mod (ReportMock:Enabled=true) aktifken decorator devre dışıdır; inner mock veriyi doğrudan döner.
/// </summary>
public sealed class CachingReportDataProvider : IReportDataProvider
{
    public const string TargetPrefix = "Target";
    public const string ProductivityPrefix = "Productivity";

    private static readonly TimeSpan TargetTtl = TimeSpan.FromHours(24);
    private static readonly TimeSpan ProductivityTtl = TimeSpan.FromDays(30);

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
    private T Cached<T>(string prefix, TimeSpan ttl, string methodName, object? arguments, Func<T> loader)
    {
        if (MockEnabled)
            return loader();

        var key = _keyBuilder.Build(prefix, methodName, arguments);
        if (_cache.TryGet<T>(key, out var cached) && cached is not null)
            return cached;

        var value = loader();
        if (value is not null)
            _cache.Set(key, value, ttl);
        return value;
    }

    // Primitive parametreleri anonim nesneye sararak deterministik key üretimi.
    private T CachedTarget<T>(string methodName, object? args, Func<T> loader)
        => Cached(TargetPrefix, TargetTtl, methodName, args, loader);

    private T CachedProductivity<T>(string methodName, object? args, Func<T> loader)
        => Cached(ProductivityPrefix, ProductivityTtl, methodName, args, loader);

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
