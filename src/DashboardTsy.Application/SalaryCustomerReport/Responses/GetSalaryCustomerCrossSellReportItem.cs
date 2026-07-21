namespace DashboardTsy.Application.SalaryCustomerReport.Responses;

public class GetSalaryCustomerCrossSellReportItem
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

    public decimal TwoMonthsAgoTotalAmount { get; set; }
    public decimal TwoMonthsAgoSalaryAmount { get; set; }
    public decimal TwoMonthsAgoSalaryRate { get; set; }
    public decimal TwoMonthsAgoRetiredAmount { get; set; }
    public decimal TwoMonthsAgoRetiredRate { get; set; }

    public decimal? TwoMonthsAgoTotalDifference { get; set; }
    public int? TwoMonthsAgoTotalDifferenceStatus { get; set; }
    public decimal? TwoMonthsAgoSalaryDifference { get; set; }
    public int? TwoMonthsAgoSalaryDifferenceStatus { get; set; }
    public decimal? TwoMonthsAgoRetiredDifference { get; set; }
    public int? TwoMonthsAgoRetiredDifferenceStatus { get; set; }

    public decimal LastMonthTotalAmount { get; set; }
    public decimal LastMonthSalaryAmount { get; set; }
    public decimal LastMonthSalaryRate { get; set; }
    public decimal LastMonthRetiredAmount { get; set; }
    public decimal LastMonthRetiredRate { get; set; }
}
