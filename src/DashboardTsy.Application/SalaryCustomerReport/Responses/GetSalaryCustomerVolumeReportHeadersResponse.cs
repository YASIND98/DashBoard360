namespace DashboardTsy.Application.SalaryCustomerReport.Responses;

public class GetSalaryCustomerVolumeReportHeadersResponse
{
    public string ProductColumnName { get; set; } = string.Empty;

    public string LastYearColumnName { get; set; } = string.Empty;
    public DateTime LastYearColumnDate { get; set; }
    public string LastYearDifferenceColumnName { get; set; } = string.Empty;

    public string LastWeekColumnName { get; set; } = string.Empty;
    public DateTime LastWeekColumnDate { get; set; }
    public string LastWeekDifferenceColumnName { get; set; } = string.Empty;

    public string PreviousDayColumnName { get; set; } = string.Empty;
    public DateTime PreviousDayColumnDate { get; set; }
    public string PreviousDayDifferenceColumnName { get; set; } = string.Empty;

    public string YesterdayColumnName { get; set; } = string.Empty;
    public DateTime YesterdayColumnDate { get; set; }

    public string SalaryLabel { get; set; } = string.Empty;
    public string RetiredLabel { get; set; } = string.Empty;
}
