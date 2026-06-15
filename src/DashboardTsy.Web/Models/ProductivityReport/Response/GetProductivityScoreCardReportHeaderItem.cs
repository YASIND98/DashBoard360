namespace DashboardTsy.Web.Models.ProductivityReport.Response;

public class GetProductivityScoreCardReportHeaderItem
{
    public int Id { get; set; }
    public string HeaderName { get; set; } = string.Empty;
    public int ParentId { get; set; }
    public int OrderNo { get; set; }
}
