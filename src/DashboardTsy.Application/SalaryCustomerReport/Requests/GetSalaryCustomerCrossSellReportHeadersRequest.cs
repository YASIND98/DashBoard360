namespace DashboardTsy.Application.SalaryCustomerReport.Requests;

public class GetSalaryCustomerCrossSellReportHeadersRequest
{
    public string SessionId { get; set; } = string.Empty;
    public DateTime ReportDate { get; set; }
}
