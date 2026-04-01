namespace DashboardTsy.Application.ProductivityReport.Requests;

public class GetProductivityCountCardPosRatioRegionReportRequest
{
    public string SessionId { get; set; }
    public string RegionCode { get; set; }
    /// <summary>
    /// 1 = KrediKartı, 2 = POS
    /// </summary>
    public int TabId { get; set; }
    public DateTime ReportDate { get; set; }
}
