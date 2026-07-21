namespace DashboardTsy.Web.Models.SalaryCustomerReport.Request;

public class GetSalaryCustomerBankShareReportHeadersRequest
{
    public string SessionId { get; set; } = string.Empty;
    public DateTime ReportDate { get; set; }
}
