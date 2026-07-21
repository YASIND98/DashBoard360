namespace DashboardTsy.Web.Models.SalaryCustomerReport.Request;

public class GetSalaryCustomerCrossSellReportHeadersRequest
{
    public string SessionId { get; set; } = string.Empty;
    public DateTime ReportDate { get; set; }
}
