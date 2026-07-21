namespace DashboardTsy.Application.SalaryCustomerReport.Requests;

public class GetSalaryCustomerVolumeReportRequest
{
    public string SessionId { get; set; } = string.Empty;
    public string? RegionCode { get; set; }
    public string? BranchCode { get; set; }
    public int SubTabId { get; set; }
    public DateTime ReportDate { get; set; }
    public bool ShowDifferences { get; set; } = false;

    /// <summary>1=Ürün Adı, 2=Geçen Yıl, 3=Geçen Hafta, ...</summary>
    public int? SortBy { get; set; }

    /// <summary>Default: false (büyükten küçüğe / DESC)</summary>
    public bool IsAscending { get; set; } = false;
}
