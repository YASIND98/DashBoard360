namespace DashboardTsy.Web.Models.SalaryCustomerReport.Request;

public class GetSalaryCustomerVolumeReportHeadersRequest
{
    public string SessionId { get; set; } = string.Empty;
    public DateTime ReportDate { get; set; }
}
