using DashboardTsy.Application.Common.Requests;

namespace DashboardTsy.Application.TargetReport.Requests;

/// <summary>
/// Hedef Raporları > Hacim ekranında ürün detayından açılan Trend Analizi filtreleri.
/// Aynı DTO hem hacim (RP_Hacimler_Trend_Analizi) hem adet (RP_Adetler_Trend_Analizi) çağrılarında kullanılır.
/// </summary>
public class GetTrendAnalysisRequest : BaseReportRequest
{
    public string SessionId { get; set; } = string.Empty;

    public string? Bolge { get; set; }
    public string? SubeKodu { get; set; }
    public string? IsKolu { get; set; }
    public string? Segment { get; set; }
    public string? Urun { get; set; }
}
