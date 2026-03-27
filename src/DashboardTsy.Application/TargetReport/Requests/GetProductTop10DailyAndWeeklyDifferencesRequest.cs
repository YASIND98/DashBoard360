namespace DashboardTsy.Application.TargetReport.Requests;

public class GetProductTop10DailyAndWeeklyDifferencesRequest
{
    public long ProductId { get; set; }
    public int FilterType { get; set; }
}
