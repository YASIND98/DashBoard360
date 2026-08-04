namespace DashboardTsy.Application.TargetReport.Responses;

/// <summary>
/// RP_Adetler_Trend_Analizi çıktısı — ürün + tarih başına adet satırı.
/// </summary>
public class GetQuantityTrendAnalysisItem
{
    public string ProductName { get; set; } = string.Empty;   // SP: URUN
    public DateTime ReportDate { get; set; }                   // SP: TARIH_DETAY
    public decimal Count { get; set; }                         // SP: ADET
}
