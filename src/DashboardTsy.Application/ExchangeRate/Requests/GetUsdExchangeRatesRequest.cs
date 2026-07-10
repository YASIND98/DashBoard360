namespace DashboardTsy.Application.ExchangeRate.Requests;

public class GetUsdExchangeRatesRequest
{
    public string SessionId { get; set; } = string.Empty;
    public DateTime ReportDate { get; set; }
}
