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

    /// <summary>
    /// 0=Tümü, 1=Kurumsal, 2=Ticari, 3=KOBÝ, 4=Tarým, 5=Bireysel
    /// </summary>
    public int TabId { get; set; }
    /// <summary>
    /// // KOBÝ ise 0=Tümü, 1=KBÝ, 2=OBÝ; Bireysel ise 0=Tümü, 1=Genel Kitle, 2=Afili, 3=Özel Bankacýlýk
    /// </summary>
    public int? SubTabId { get; set; }
}
