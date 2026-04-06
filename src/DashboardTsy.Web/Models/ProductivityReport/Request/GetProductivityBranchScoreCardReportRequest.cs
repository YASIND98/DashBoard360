namespace DashboardTsy.Web.Models.ProductivityReport.Request;

public class GetProductivityBranchScoreCardReportRequest
{
    public string SessionId { get; set; } = string.Empty;
    public string BranchCode { get; set; } = string.Empty;
    public DateTime ReportDate { get; set; }
}
