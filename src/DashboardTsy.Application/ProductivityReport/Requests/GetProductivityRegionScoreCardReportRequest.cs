namespace DashboardTsy.Application.ProductivityReport.Requests;

public class GetProductivityRegionScoreCardReportRequest
{
    public string SessionId { get; set; } = string.Empty;
    public string RegionCode { get; set; } = string.Empty;
    public string? BranchCode { get; set; }
    public DateTime ReportDate { get; set; }
}
