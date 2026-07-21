namespace DashboardTsy.Application.SalaryCustomerReport.Responses;

public class GetSalaryCustomerBankShareReportHeadersResponse
{
    public string ProductColumnName { get; set; } = string.Empty;

    public string DenizbankCreditGroupName { get; set; } = string.Empty;
    public string OtherBanksCreditGroupName { get; set; } = string.Empty;
    public string WalletShareGroupName { get; set; } = string.Empty;

    public string FirstMonthName { get; set; } = string.Empty;
    public string SecondMonthName { get; set; } = string.Empty;

    public string SalaryCustomersLabel { get; set; } = string.Empty;
    public string RetiredCustomersLabel { get; set; } = string.Empty;
}
