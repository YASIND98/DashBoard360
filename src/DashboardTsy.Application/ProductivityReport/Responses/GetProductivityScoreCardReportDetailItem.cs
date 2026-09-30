namespace DashboardTsy.Application.ProductivityReport.Responses;

public class GetProductivityScoreCardReportDetailItem
{
    /// <summary>SP: STATU</summary>
    public string Status { get; set; } = string.Empty;
    /// <summary>SP: PERSONEL_ADI</summary>
    public string EmployeeName { get; set; } = string.Empty;
    /// <summary>SP: GOREV_ADI</summary>
    public string PositionName { get; set; } = string.Empty;
    /// <summary>SP: GOREVE_BASLANGIC_TARIHI</summary>
    public DateTime? PositionStartDate { get; set; }
    /// <summary>SP: GOREV_SURESI (örn. "1 Yıl 8 Ay 1 Gün")</summary>
    public string PositionDuration { get; set; } = string.Empty;
}
