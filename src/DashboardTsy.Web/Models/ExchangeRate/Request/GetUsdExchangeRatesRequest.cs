namespace DashboardTsy.Web.Models.ExchangeRate.Request;

public class GetUsdExchangeRatesRequest
{
    public string SessionId { get; set; } = string.Empty;
    public DateTime ReportDate { get; set; }
}
