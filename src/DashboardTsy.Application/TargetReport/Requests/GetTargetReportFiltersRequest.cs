namespace DashboardTsy.Application.TargetReport.Requests;

public class GetTargetReportFiltersRequest
{
    public string SessionId { get; set; } = string.Empty;
    /// <summary>0=Region, 1=Branch, 2=Portfolio</summary>
    public int FilterId { get; set; }
    public List<string>? FilterCode { get; set; }
}
