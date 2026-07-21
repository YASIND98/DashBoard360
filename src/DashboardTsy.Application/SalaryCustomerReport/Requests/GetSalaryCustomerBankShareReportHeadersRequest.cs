namespace DashboardTsy.Application.SalaryCustomerReport.Requests;

public class GetSalaryCustomerBankShareReportHeadersRequest
{
    public string SessionId { get; set; } = string.Empty;
    public DateTime ReportDate { get; set; }
}
