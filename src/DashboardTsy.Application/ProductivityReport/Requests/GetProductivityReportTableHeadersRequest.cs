namespace DashboardTsy.Application.ProductivityReport.Requests;

public class GetProductivityReportTableHeadersRequest
{
    public string SessionId { get; set; } = string.Empty;
    public int MainTabId { get; set; }
    public int? MidTabId { get; set; }
    public int? SubTabId { get; set; }
    public int FilterType { get; set; }     // 1=Region 2=Branch
    public DateTime ReportDate { get; set; }
}
