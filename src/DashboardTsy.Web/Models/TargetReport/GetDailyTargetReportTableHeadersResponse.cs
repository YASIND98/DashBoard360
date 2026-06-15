namespace DashboardTsy.Web.Models.TargetReport;

public class GetDailyTargetReportTableHeadersResponse
{
    public string ProductNameTitle { get; set; } = string.Empty;

    public string LastYearTitle { get; set; } = string.Empty;
    public DateTime LastYearDate { get; set; }

    public string LastWeekTitle { get; set; } = string.Empty;
    public DateTime LastWeekDate { get; set; }

    public string PrevDayTitle { get; set; } = string.Empty;
    public DateTime PrevDayDate { get; set; }

    public string YesterdayTitle { get; set; } = string.Empty;
    public DateTime YesterdayDate { get; set; }

    public string TodayTitle { get; set; } = string.Empty;
    public DateTime TodayDate { get; set; }

    public string DiffByPrevDayTitle { get; set; } = string.Empty;
    public string DiffByLastYearTitle { get; set; } = string.Empty;
    public string DiffByLastWeekTitle { get; set; } = string.Empty;
}

