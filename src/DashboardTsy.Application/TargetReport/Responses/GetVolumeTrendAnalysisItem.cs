namespace DashboardTsy.Application.TargetReport.Responses;

/// <summary>
/// RP_Hacimler_Trend_Analizi çıktısı — ürün + tarih başına hacim (TL) satırı.
/// </summary>
public class GetVolumeTrendAnalysisItem
{
    public string ProductName { get; set; } = string.Empty;   // SP: URUN
    public DateTime ReportDate { get; set; }                   // SP: TARIH_DETAY
    public decimal Amount { get; set; }                        // SP: HACIM
}
