namespace DashboardTsy.Application.SalaryCustomerReport.Responses;

public class GetSalaryCustomerVolumeReportItem
{
    public int Id { get; set; }
    public int SortOrder { get; set; }
    public string ProductName { get; set; } = string.Empty;

    public decimal LastYearTotalAmount { get; set; }
    public decimal LastYearSalaryAmount { get; set; }
    public decimal LastYearSalaryRate { get; set; }
    public decimal LastYearRetiredAmount { get; set; }
    public decimal LastYearRetiredRate { get; set; }

    public decimal? LastYearTotalDifference { get; set; }
    public int? LastYearTotalDifferenceStatus { get; set; }
    public decimal? LastYearSalaryDifference { get; set; }
    public int? LastYearSalaryDifferenceStatus { get; set; }
    public decimal? LastYearRetiredDifference { get; set; }
    public int? LastYearRetiredDifferenceStatus { get; set; }

    public decimal LastWeekTotalAmount { get; set; }
    public decimal LastWeekSalaryAmount { get; set; }
    public decimal LastWeekSalaryRate { get; set; }
    public decimal LastWeekRetiredAmount { get; set; }
    public decimal LastWeekRetiredRate { get; set; }

    public decimal? LastWeekTotalDifference { get; set; }
    public int? LastWeekTotalDifferenceStatus { get; set; }
    public decimal? LastWeekSalaryDifference { get; set; }
    public int? LastWeekSalaryDifferenceStatus { get; set; }
    public decimal? LastWeekRetiredDifference { get; set; }
    public int? LastWeekRetiredDifferenceStatus { get; set; }

    public decimal PreviousDayTotalAmount { get; set; }
    public decimal PreviousDaySalaryAmount { get; set; }
    public decimal PreviousDaySalaryRate { get; set; }
    public decimal PreviousDayRetiredAmount { get; set; }
    public decimal PreviousDayRetiredRate { get; set; }

    public decimal? PreviousDayTotalDifference { get; set; }
    public int? PreviousDayTotalDifferenceStatus { get; set; }
    public decimal? PreviousDaySalaryDifference { get; set; }
    public int? PreviousDaySalaryDifferenceStatus { get; set; }
    public decimal? PreviousDayRetiredDifference { get; set; }
    public int? PreviousDayRetiredDifferenceStatus { get; set; }

    public decimal YesterdayTotalAmount { get; set; }
    public decimal YesterdaySalaryAmount { get; set; }
    public decimal YesterdaySalaryRate { get; set; }
    public decimal YesterdayRetiredAmount { get; set; }
    public decimal YesterdayRetiredRate { get; set; }
}
