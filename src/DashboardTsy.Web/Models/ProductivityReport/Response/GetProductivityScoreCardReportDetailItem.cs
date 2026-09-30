namespace DashboardTsy.Web.Models.ProductivityReport.Response;

public class GetProductivityScoreCardReportDetailItem
{
    public string Status { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string PositionName { get; set; } = string.Empty;
    public DateTime? PositionStartDate { get; set; }
    /// <summary>Örn. "1 Yıl 8 Ay 1 Gün"</summary>
    public string PositionDuration { get; set; } = string.Empty;
}
