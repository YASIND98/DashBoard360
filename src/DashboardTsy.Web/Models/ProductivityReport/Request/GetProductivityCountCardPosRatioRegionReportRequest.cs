namespace DashboardTsy.Web.Models.ProductivityReport.Request;

public class GetProductivityCountCardPosRatioRegionReportRequest
{
    public string SessionId { get; set; } = string.Empty;
    public string RegionCode { get; set; } = string.Empty;
    /// <summary>
    /// 1 = KrediKartı, 2 = POS
    /// </summary>
    public int TabId { get; set; }
    public DateTime ReportDate { get; set; }
}
