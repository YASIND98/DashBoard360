namespace DashboardTsy.Application.ProductivityReport.Requests;

public class GetProductivityBranchScoreCardReportRequest
{
    public string SessionId { get; set; } = string.Empty;
    public string BranchCode { get; set; } = string.Empty;
    public DateTime ReportDate { get; set; }
}
