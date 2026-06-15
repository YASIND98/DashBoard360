namespace DashboardTsy.Web.Models.ProductivityReport;

public class GetProductivityRegionScoreCardReportRequest
{
    public string SessionId { get; set; } = string.Empty;
    public string RegionCode { get; set; } = string.Empty;
    public string? BranchCode { get; set; }
    public DateTime ReportDate { get; set; }
}
