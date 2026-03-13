namespace DashboardTsy.Application.TargetReport;

public class GetProductivityReportTabsRequest
{
    public string SessionId { get; set; } = string.Empty;
    public int FilterType { get; set; }
}

