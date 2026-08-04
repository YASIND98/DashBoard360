namespace DashboardTsy.Web.Models.TargetReport;

public class GetQuantityTrendAnalysisItem
{
    public string ProductName { get; set; } = string.Empty;
    public DateTime ReportDate { get; set; }
    public decimal Count { get; set; }
}
