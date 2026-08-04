namespace DashboardTsy.Web.Models.NplReport.Request;

public class GetNplBalanceRatioRequest
{
    public string SessionId { get; set; } = string.Empty;

    public int? SubeKodu { get; set; }
    public int? BolgeKodu { get; set; }
    public int? Yil { get; set; }

    public Dictionary<string, object?> Parameters { get; set; } = new();
}
