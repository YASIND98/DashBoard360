namespace DashboardTsy.Web.Models.ExchangeRate.Response;

public class GetUsdExchangeRatesResponse
{
    public DateTime YesterdayDate { get; set; }
    public decimal YesterdayRate { get; set; }

    public DateTime PreviousDayDate { get; set; }
    public decimal PreviousDayRate { get; set; }

    public DateTime PreviousWeekDate { get; set; }
    public decimal PreviousWeekRate { get; set; }

    public DateTime PreviousYearDate { get; set; }
    public decimal PreviousYearRate { get; set; }
}
