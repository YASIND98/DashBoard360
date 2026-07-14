namespace DashboardTsy.Application.ProductivityReport.Requests;

public class GetProductivityCountCustomerBranchReportRequest
{
    public string SessionId { get; set; } = string.Empty;
    public string BranchCode { get; set; } = string.Empty;
    public int SubTabId { get; set; }
    public DateTime ReportDate { get; set; }
    /// <summary>1=Ürün Adı</summary>
    public int? SortBy { get; set; }
    /// <summary>Default: yeniden -> eskiye</summary>
    public bool IsAscending { get; set; } = false;
}
