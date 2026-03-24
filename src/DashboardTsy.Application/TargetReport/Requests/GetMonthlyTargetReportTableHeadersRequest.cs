namespace DashboardTsy.Application.TargetReport.Requests;

public class GetMonthlyTargetReportTableHeadersRequest
{
    public string SessionId { get; set; } = string.Empty;
    public DateTime ReportDate { get; set; }
}
