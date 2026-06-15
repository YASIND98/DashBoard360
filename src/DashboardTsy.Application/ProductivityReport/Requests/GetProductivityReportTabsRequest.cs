namespace DashboardTsy.Application.ProductivityReport.Requests;

public class GetProductivityReportTabsRequest
{
    public string SessionId { get; set; } = string.Empty;
    public int FilterType { get; set; }
}
