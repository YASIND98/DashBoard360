namespace DashboardTsy.Web.Models.ProductivityReport.Request;

public class GetProductivityCountCardPosRegionReportRequest
{
    public string SessionId { get; set; } = string.Empty;
    public string RegionCode { get; set; } = string.Empty;
    /// <summary>1=KrediKartı, 2=POS</summary>
    public int TabId { get; set; }
    public DateTime ReportDate { get; set; }
    /// <summary>1=Ürün Adı</summary>
    public int? SortBy { get; set; }
    public bool IsAscending { get; set; } = false;
}
