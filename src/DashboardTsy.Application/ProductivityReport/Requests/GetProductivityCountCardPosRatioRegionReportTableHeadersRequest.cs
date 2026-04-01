namespace DashboardTsy.Application.ProductivityReport.Requests;

public class GetProductivityCountCardPosRatioRegionReportTableHeadersRequest
{
    public string SessionId { get; set; }
    /// <summary>
    /// 1 = KrediKartı, 2 = POS
    /// </summary>
    public int TabId { get; set; }
    public DateTime ReportDate { get; set; }
}
