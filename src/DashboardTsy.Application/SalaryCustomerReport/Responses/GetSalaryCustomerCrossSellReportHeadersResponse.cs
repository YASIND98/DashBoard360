namespace DashboardTsy.Application.SalaryCustomerReport.Responses;

public class GetSalaryCustomerCrossSellReportHeadersResponse
{
    public string ProductColumnName { get; set; } = string.Empty;

    public string LastYearColumnName { get; set; } = string.Empty;
    public DateTime LastYearColumnDate { get; set; }
    public string LastYearDifferenceColumnName { get; set; } = string.Empty;

    public string TwoMonthsAgoColumnName { get; set; } = string.Empty;
    public DateTime TwoMonthsAgoColumnDate { get; set; }
    public string TwoMonthsAgoDifferenceColumnName { get; set; } = string.Empty;

    public string LastMonthColumnName { get; set; } = string.Empty;
    public DateTime LastMonthColumnDate { get; set; }

    public string SalaryLabel { get; set; } = string.Empty;
    public string RetiredLabel { get; set; } = string.Empty;
}
