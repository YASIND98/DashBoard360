namespace DashboardTsy.Web.Models.ProductivityReport.Request;

public class GetProductivityReportTabsRequest
{
    public string SessionId { get; set; } = string.Empty;
    public int FilterType { get; set; }
}
