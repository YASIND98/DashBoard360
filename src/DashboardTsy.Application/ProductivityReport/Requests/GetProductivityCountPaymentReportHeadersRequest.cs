namespace DashboardTsy.Application.ProductivityReport.Requests;

public class GetProductivityCountPaymentReportHeadersRequest
{
    public string SessionId { get; set; } = string.Empty;
    public int FilterType { get; set; } = 1;     // 1=Region 2=Branch
}
