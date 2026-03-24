namespace DashboardTsy.Api.Models.TargetReport;

public class GetMonthlyTargetReportTableHeadersRequest
{
    public string SessionId { get; set; } = string.Empty;
    public DateTime ReportDate { get; set; }
}

