namespace DashboardTsy.Web.Models.ProductivityReport.Request;

public class GetProductivityCountCardPosRatioBranchReportTableHeadersRequest
{
    public string SessionId { get; set; } = string.Empty;
    public int TabId { get; set; }
    public DateTime ReportDate { get; set; }
}
