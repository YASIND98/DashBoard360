namespace DashboardTsy.Web.Models.ProductivityReport;

public class GetProductivityCountCardPosRatioBranchReportTableHeadersRequest
{
    public string SessionId { get; set; } = string.Empty;
    public int TabId { get; set; }
    public DateTime ReportDate { get; set; }
}
