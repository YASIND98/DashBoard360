namespace DashboardTsy.Application.SalaryCustomerReport.Responses;

public class GetSalaryCustomerBankShareReportItem
{
    public int Id { get; set; }
    public int SortOrder { get; set; }
    public string ProductName { get; set; } = string.Empty;

    /// <summary>1=Adet, 2=Hacim</summary>
    public int ValueType { get; set; }

    public decimal DenizbankFirstMonthValue { get; set; }
    public decimal DenizbankSecondMonthValue { get; set; }

    public decimal OtherBanksFirstMonthValue { get; set; }
    public decimal OtherBanksSecondMonthValue { get; set; }

    public decimal WalletShareFirstMonthRate { get; set; }
    public decimal WalletShareSecondMonthRate { get; set; }
    public int WalletShareSecondMonthRateStatus { get; set; }
}
