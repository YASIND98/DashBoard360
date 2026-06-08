using System.Data;
using DashboardTsy.Application.TargetReport.Requests;
using DashboardTsy.Application.TargetReport.Responses;
using DashboardTsy.Application.ProductivityReport.Requests;
using DashboardTsy.Application.ProductivityReport.Responses;
using DashboardTsy.Application.AiInsight.Requests;
using DashboardTsy.Application.AiInsight.Responses;

namespace DashboardTsy.Application;

/// <summary>
/// TargetReport ve ProductivityReport için application katmanı kontratı.
/// Stored procedure çağrılarını soyutlar.
/// </summary>
public interface IReportDataProvider
{

    GetTargetReportMenuTextsResponse? GetTargetReportMenuTexts(string sessionId);

    IReadOnlyList<GetTargetReportFiltersItem> GetTargetReportFilters(string sessionId, int filterId, List<string>? filterCode);

    GetDailyTargetReportResponse GetDailyTargetReport(GetDailyTargetReportRequest request);

    GetDailyQuantityTargetReportTableHeadersResponse? GetDailyQuantityTargetReportTableHeaders(GetDailyQuantityTargetReportTableHeadersRequest request);
    GetDailyQuantityTargetReportResponse GetDailyQuantityTargetReport(GetDailyQuantityTargetReportRequest request);
    ProductTop10DifferencesResponse GetProductTop10DailyAndWeeklyDifferences(GetProductTop10DailyAndWeeklyDifferencesRequest request);

    GetDailyTargetReportTableHeadersResponse? GetDailyTargetReportTableHeaders(string sessionId);

    GetMonthlyTargetReportResponse? GetMonthlyTargetReport(GetMonthlyTargetReportRequest request);

    GetMonthlyTargetReportTableHeadersResponse? GetMonthlyTargetReportTableHeaders(GetMonthlyTargetReportTableHeadersRequest request);

    /// <summary>
    /// Verimlilik raporu için sekme hiyerarşisini döner.
    /// SP henüz yok; şimdilik mock veri üzerinden çalışıyor.
    /// </summary>
    IReadOnlyList<GetProductivityReportTabItem> GetProductivityReportTabs(GetProductivityReportTabsRequest request);

    /// <summary>
    /// Verimlilik raporu tablo başlıklarını döner (çok seviyeli header yapısı).
    /// Şu an SP belli olmadığı için mock veri üzerinden çalışır.
    /// </summary>
    IReadOnlyList<GetProductivityReportTableHeaderItem> GetProductivityReportTableHeaders(GetProductivityReportTableHeadersRequest request);

    /// <summary>
    /// Verimlilik raporu için bölge filtre seçeneklerini döner.
    /// Şu an SP adı belli olmadığı için mock veri + SP taslağı ile çalışır.
    /// </summary>
    IReadOnlyList<GetReportRegionFilterItem> GetReportRegionFilters(GetReportRegionFiltersRequest request);

    /// <summary>
    /// Verimlilik raporu için şube filtre seçeneklerini döner.
    /// Şu an SP adı belli olmadığı için mock veri + SP taslağı ile çalışır.
    /// </summary>
    IReadOnlyList<GetReportBranchFilterItem> GetReportBranchFilters(GetReportBranchFiltersRequest request);

    /// <summary>
    /// Verimlilik ekranı için genel bölge bazlı raporu döner.
    /// Şu an SP adı belli olmadığı için mock veri + SP taslağı ile çalışır.
    /// </summary>
    GetProductivityGeneralRegionReportResponse? GetProductivityGeneralRegionReport(GetProductivityGeneralRegionReportRequest request);

    /// <summary>
    /// Verimlilik ekranı için KrediKartı / POS sayısal bölge raporunu döner.
    /// Şu an SP adı belli olmadığı için mock veri + SP taslağı ile çalışır.
    /// </summary>
    GetProductivityCountCardPosRegionReportResponse? GetProductivityCountCardPosRegionReport(GetProductivityCountCardPosRegionReportRequest request);
    IReadOnlyList<GetProductivityCountCardPosRatioRegionReportItem> GetProductivityCountCardPosRatioRegionReport(GetProductivityCountCardPosRatioRegionReportRequest request);
    GetProductivityCountCardPosRatioRegionReportTableHeadersItem? GetProductivityCountCardPosRatioRegionReportTableHeaders(GetProductivityCountCardPosRatioRegionReportTableHeadersRequest request);

    /// <summary>
    /// Verimlilik ekranı için müşteri sayısı bölge raporunu döner (Tümü/Kurumsal/Ticari/KOBİ/Tarım/Bireysel).
    /// Şu an SP adı belli olmadığı için mock veri + SP taslağı ile çalışır.
    /// </summary>
    GetProductivityCountCustomerRegionReportResponse? GetProductivityCountCustomerRegionReport(GetProductivityCountCustomerRegionReportRequest request);

    /// <summary>
    /// Verimlilik ekranı için hacim bölge raporunu döner.
    /// Şu an SP adı belli olmadığı için mock veri + SP taslağı ile çalışır.
    /// </summary>
    GetProductivityVolumeRegionReportResponse GetProductivityVolumeRegionReport(GetProductivityVolumeRegionReportRequest request);

    /// <summary>
    /// Verimlilik ekranı için karlılık oranı bölge raporunu döner.
    /// Şu an SP adı belli olmadığı için mock veri + SP taslağı ile çalışır.
    /// </summary>
    GetProductivityProfitRatioRegionReportResponse GetProductivityProfitRatioRegionReport(GetProductivityProfitRatioRegionReportRequest request);

    /// <summary>
    /// Verimlilik ekranı için toplam karlılık bölge raporunu döner.
    /// Şu an SP adı belli olmadığı için mock veri + SP taslağı ile çalışır.
    /// </summary>
    GetProductivityProfitTotalRegionReportResponse GetProductivityProfitTotalRegionReport(GetProductivityProfitTotalRegionReportRequest request);

    /// <summary>
    /// Verimlilik ekranı için spread yönetimi karlılık bölge raporunu döner.
    /// Şu an SP adı belli olmadığı için mock veri + SP taslağı ile çalışır.
    /// </summary>
    GetProductivityProfitSpreadManagementRegionReportResponse GetProductivityProfitSpreadManagementRegionReport(GetProductivityProfitSpreadManagementRegionReportRequest request);

    /// <summary>
    /// Verimlilik ekranı için spread yönetimi karlılık şube raporunu döner.
    /// Şu an SP adı belli olmadığı için mock veri + SP taslağı ile çalışır.
    /// </summary>
    GetProductivityProfitSpreadManagementBranchReportResponse? GetProductivityProfitSpreadManagementBranchReport(GetProductivityProfitSpreadManagementBranchReportRequest request);

    /// <summary>
    /// Verimlilik ekranı için KrediKartı/POS sayısal şube raporunu döner.
    /// Şu an SP adı belli olmadığı için mock veri + SP taslağı ile çalışır.
    /// </summary>
    GetProductivityCountCardPosBranchReportResponse? GetProductivityCountCardPosBranchReport(GetProductivityCountCardPosBranchReportRequest request);
    IReadOnlyList<GetProductivityCountCardPosRatioBranchReportItem> GetProductivityCountCardPosRatioBranchReport(GetProductivityCountCardPosRatioBranchReportRequest request);
    GetProductivityCountCardPosRatioBranchReportTableHeadersItem? GetProductivityCountCardPosRatioBranchReportTableHeaders(GetProductivityCountCardPosRatioBranchReportTableHeadersRequest request);

    /// <summary>
    /// Verimlilik ekranı için karlılık oranı şube raporunu döner.
    /// Şu an SP adı belli olmadığı için mock veri + SP taslağı ile çalışır.
    /// </summary>
    GetProductivityProfitRatioBranchReportResponse? GetProductivityProfitRatioBranchReport(GetProductivityProfitRatioBranchReportRequest request);

    /// <summary>
    /// Verimlilik ekranı için karlılık toplam şube raporunu döner.
    /// Şu an SP adı belli olmadığı için mock veri + SP taslağı ile çalışır.
    /// </summary>
    GetProductivityProfitTotalBranchReportResponse? GetProductivityProfitTotalBranchReport(GetProductivityProfitTotalBranchReportRequest request);

    /// <summary>
    /// Verimlilik ekranı için şube skor kartı raporunu döner (tek satır).
    /// Şu an SP adı belli olmadığı için mock veri + SP taslağı ile çalışır.
    /// </summary>
    GetProductivityBranchScoreCardReportItem? GetProductivityBranchScoreCardReport(GetProductivityBranchScoreCardReportRequest request);

    /// <summary>
    /// Verimlilik ekranı için bölge skor kartı raporunu döner (tek satır).
    /// Şu an SP adı belli olmadığı için mock veri + SP taslağı ile çalışır.
    /// </summary>
    GetProductivityRegionScoreCardReportItem? GetProductivityRegionScoreCardReport(GetProductivityRegionScoreCardReportRequest request);

    /// <summary>
    /// Verimlilik ekranı için müşteri sayısı şube raporunu döner.
    /// Şu an SP adı belli olmadığı için mock veri + SP taslağı ile çalışır.
    /// </summary>
    GetProductivityCountCustomerBranchReportResponse? GetProductivityCountCustomerBranchReport(GetProductivityCountCustomerBranchReportRequest request);

    /// <summary>
    /// Verimlilik ekranı için hacim şube raporunu döner.
    /// Şu an SP adı belli olmadığı için mock veri + SP taslağı ile çalışır.
    /// </summary>
    GetProductivityVolumeBranchReportResponse? GetProductivityVolumeBranchReport(GetProductivityVolumeBranchReportRequest request);

    /// <summary>
    /// Skor kartı ekranı için header hiyerarşisini döner (bölge/şube, roller, NPS).
    /// Şu an SP tanımlı olmadığı için mock veri üzerinden çalışır.
    /// </summary>
    IReadOnlyList<GetProductivityScoreCardReportHeaderItem> GetProductivityScoreCardReportHeaders(GetProductivityScoreCardReportHeadersRequest request);
    IReadOnlyList<GetReportSidebarItem> GetReportSidebarItems(GetReportSidebarItemsRequest request);
    IReadOnlyList<GetReportDatesItem> GetReportDates();

    GetBranchAiInsightResponse GetBranchAiInsights(GetBranchAiInsightRequest request);
}
