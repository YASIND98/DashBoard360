namespace DashboardTsy.Application.ProductivityReport.Requests;

public class GetProductivityCountCardPosRatioBranchReportRequest
{
    public string SessionId { get; set; } = string.Empty;
    public string BranchCode { get; set; } = string.Empty;
    public int TabId { get; set; }
    public DateTime ReportDate { get; set; }
}
