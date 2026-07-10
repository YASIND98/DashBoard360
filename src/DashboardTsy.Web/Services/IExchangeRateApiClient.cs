using DashboardTsy.Web.Models.ExchangeRate.Request;
using DashboardTsy.Web.Models.ExchangeRate.Response;

namespace DashboardTsy.Web.Services;

public interface IExchangeRateApiClient
{
    Task<GetUsdExchangeRatesResponse?> GetUsdExchangeRatesAsync(GetUsdExchangeRatesRequest request, CancellationToken cancellationToken = default);
}
