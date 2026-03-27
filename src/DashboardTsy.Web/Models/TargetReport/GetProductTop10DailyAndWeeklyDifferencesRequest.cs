namespace DashboardTsy.Web.Models.TargetReport;

public class GetProductTop10DailyAndWeeklyDifferencesRequest
{
    public long ProductId { get; set; }
    public int FilterType { get; set; }
}
