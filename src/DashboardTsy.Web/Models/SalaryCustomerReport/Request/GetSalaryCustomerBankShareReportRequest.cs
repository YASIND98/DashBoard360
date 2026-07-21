namespace DashboardTsy.Web.Models.SalaryCustomerReport.Request;

public class GetSalaryCustomerBankShareReportRequest
{
    public string SessionId { get; set; } = string.Empty;
    public string? RegionCode { get; set; }
    public string? BranchCode { get; set; }
    public int SubTabId { get; set; }
    public DateTime ReportDate { get; set; }
    public int CustomerType { get; set; }
}
