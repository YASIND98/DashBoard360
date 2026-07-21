namespace DashboardTsy.Application.SalaryCustomerReport.Requests;

public class GetSalaryCustomerVolumeReportHeadersRequest
{
    public string SessionId { get; set; } = string.Empty;
    public DateTime ReportDate { get; set; }
}
