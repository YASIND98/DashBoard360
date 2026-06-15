namespace DashboardTsy.Web.Models.ProductivityReport.Request;

public class GetProductivityScoreCardReportHeadersRequest
{
    public string SessionId { get; set; } = string.Empty;
    /// <summary>1=Region, 2=Branch</summary>
    public int FilterType { get; set; }
    public DateTime ReportDate { get; set; }
}
