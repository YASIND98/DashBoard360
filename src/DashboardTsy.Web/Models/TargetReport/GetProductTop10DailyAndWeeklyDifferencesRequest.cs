namespace DashboardTsy.Web.Models.TargetReport;

public class GetProductTop10DailyAndWeeklyDifferencesRequest
{
    public long ProductId { get; set; }
    public int FilterType { get; set; }

    /// <summary>
    /// Bölge & Þube filtresi
    /// </summary>
    public List<int>? RegionId { get; set; }
    public List<int>? BranchId { get; set; }
}
