namespace DashboardTsy.Application.TargetReport.Responses;

public class GetMonthlyTargetReportTableHeadersResponse
{
    public string ProductNameTitle { get; set; } = string.Empty;

    public string MonthGroupTitle { get; set; } = string.Empty;
    public string YearGroupTitle { get; set; } = string.Empty;

    public string MonthActualTitle { get; set; } = string.Empty;
    public string MonthTargetTitle { get; set; } = string.Empty;
    public string MonthHGTitle { get; set; } = string.Empty;

    public string YearActualTitle { get; set; } = string.Empty;
    public string YearTargetTitle { get; set; } = string.Empty;
    public string YearHGTitle { get; set; } = string.Empty;
}
