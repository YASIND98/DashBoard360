namespace DashboardTsy.Application.SalaryCustomerReport.Requests;

public class GetSalaryCustomerBankShareReportRequest
{
    public string SessionId { get; set; } = string.Empty;
    public string? RegionCode { get; set; }
    public string? BranchCode { get; set; }
    public int SubTabId { get; set; }
    public DateTime ReportDate { get; set; }

    /// <summary>1=Maaş Müşterileri, 2=Emekli Müşterileri</summary>
    public int CustomerType { get; set; }
}
