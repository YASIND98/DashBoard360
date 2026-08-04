namespace DashboardTsy.Web.Models.NplReport.Request;

public class GetNplBalanceRatioRequest
{
    public string SessionId { get; set; } = string.Empty;

    public Dictionary<string, object?> Parameters { get; set; } = new();
}
