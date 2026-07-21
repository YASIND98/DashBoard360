namespace DashboardTsy.Web.Models.SalaryCustomerReport.Request;

public class GetSalaryCustomerVolumeReportRequest
{
    public string SessionId { get; set; } = string.Empty;
    public string? RegionCode { get; set; }
    public string? BranchCode { get; set; }
    public int SubTabId { get; set; }
    public DateTime ReportDate { get; set; }
    public bool ShowDifferences { get; set; } = false;
    public int? SortBy { get; set; }
    public bool IsAscending { get; set; } = false;
}
