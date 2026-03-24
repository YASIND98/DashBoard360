namespace DashboardTsy.Web.Models.TargetReport;

public class GetTargetReportFiltersRequest
{
    public string SessionId { get; set; } = string.Empty;
    public int FilterId { get; set; }
    public List<string>? FilterCode { get; set; }
}
