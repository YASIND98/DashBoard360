namespace DashboardTsy.Application.ProductivityReport.Requests;

public class GetProductivityVolumeRegionReportRequest
{
    public string SessionId { get; set; } = string.Empty;
    public string RegionCode { get; set; } = string.Empty;
    public int SubTabId { get; set; }
    public DateTime ReportDate { get; set; }
    /// <summary>1=Ürün Adı</summary>
    public int? SortBy { get; set; }
    /// <summary>Default: yeniden -> eskiye</summary>
    public bool IsAscending { get; set; } = false;
}
