namespace DashboardTsy.Web.Models.NplReport.Request;

public class GetNplBalanceRatioRequest
{
    public string SessionId { get; set; } = string.Empty;
    public string? KatDonem { get; set; }
    public string? IsKolu { get; set; }
    public string? TahsisKolu { get; set; }
    public int? SubeKodu { get; set; }
    public int? BolgeKodu { get; set; }
    public string? Urun { get; set; }
    public int? Yil { get; set; }
    public string? YetkiKodu { get; set; }
}
