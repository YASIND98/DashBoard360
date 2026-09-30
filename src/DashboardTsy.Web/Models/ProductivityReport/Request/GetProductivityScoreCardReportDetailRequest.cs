namespace DashboardTsy.Web.Models.ProductivityReport.Request;

public class GetProductivityScoreCardReportDetailRequest
{
    public string SessionId { get; set; } = string.Empty;
    public string? RegionCode { get; set; }
    public string? BranchCode { get; set; }
    public DateTime ReportDate { get; set; }
}
