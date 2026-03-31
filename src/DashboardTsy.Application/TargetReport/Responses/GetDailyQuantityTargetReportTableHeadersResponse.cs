namespace DashboardTsy.Application.TargetReport.Responses;

public class GetDailyQuantityTargetReportTableHeadersResponse
{
    public string ProductNameTitle { get; set; } = string.Empty;

    public string LastYearTitle { get; set; } = string.Empty;
    public DateTime LastYearDate { get; set; }

    public string LastMonthTitle { get; set; } = string.Empty;
    public DateTime LastMonthDate { get; set; }

    public string LastTwoMonthEarlierTitle { get; set; } = string.Empty;
    public DateTime LastTwoMonthEarlierDate { get; set; }

    public string TodayTitle { get; set; } = string.Empty;
    public DateTime TodayDate { get; set; }

    public string DiffByLastTwoMonthEarlierTitle { get; set; } = string.Empty;
    public string DiffByLastYearTitle { get; set; } = string.Empty;
    public string DiffByLastMonthTitle { get; set; } = string.Empty;
}
