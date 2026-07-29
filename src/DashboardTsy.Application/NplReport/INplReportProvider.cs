using DashboardTsy.Application.NplReport.Requests;
using DashboardTsy.Application.NplReport.Responses;

namespace DashboardTsy.Application.NplReport;

/// <summary>
/// NplReport için application katmanı kontratı. Stored procedure çağrılarını soyutlar.
/// </summary>
public interface INplReportProvider
{
    /// <summary>
    /// NPL Bakiye ve Oran raporu — tarih başına Anapara/KOF/Toplam bakiyeleri ve Anapara/KOF oranlarını döner.
    /// </summary>
    IReadOnlyList<GetNplBalanceRatioItem> GetNplBalanceRatio(GetNplBalanceRatioRequest request);

    /// <summary>
    /// NPL rapor ekranının filter dropdown/multiselect seçeneklerini döner (Ürün, Yetki, İş Kolu, Tahsis Kolu vs.).
    /// </summary>
    IReadOnlyList<GetNplFiltersItem> GetNplFilters(GetNplFiltersRequest request);

    /// <summary>
    /// NPL raporlarında kullanılan ürün lookup listesini döner (Ihtiyaç, KMH, KK, ÜK, Traktör, Diğer).
    /// </summary>
    IReadOnlyList<GetNplProductsItem> GetNplProducts(GetNplProductsRequest request);
}
