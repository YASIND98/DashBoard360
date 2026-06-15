namespace DashboardTsy.Web.Models.ProductivityReport.Request;

public class GetProductivityCountCardPosRatioRegionReportTableHeadersRequest
{
    public string SessionId { get; set; } = string.Empty;
    /// <summary>
    /// 1 = KrediKartı, 2 = POS
    /// </summary>
    public int TabId { get; set; }
    public DateTime ReportDate { get; set; }
}
