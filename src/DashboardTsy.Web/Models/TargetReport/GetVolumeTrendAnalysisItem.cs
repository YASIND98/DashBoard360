namespace DashboardTsy.Web.Models.TargetReport;

public class GetVolumeTrendAnalysisItem
{
    public string ProductName { get; set; } = string.Empty;
    public DateTime ReportDate { get; set; }
    public decimal Amount { get; set; }
}
