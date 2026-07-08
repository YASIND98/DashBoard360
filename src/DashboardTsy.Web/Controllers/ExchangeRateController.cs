using DashboardTsy.Web.Models.ExchangeRate.Request;
using DashboardTsy.Web.Models.ExchangeRate.Response;
using DashboardTsy.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace DashboardTsy.Web.Controllers;

[Route("[controller]")]
[ApiController]
public class ExchangeRateController : ControllerBase
{
    private readonly IExchangeRateApiClient _apiClient;

    public ExchangeRateController(IExchangeRateApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    private static string GetSessionId(string? requestSessionId, ISession session)
    {
        if (!string.IsNullOrEmpty(requestSessionId)) return requestSessionId;
        return session.GetString("UserId") ?? string.Empty;
    }

    private bool HasSession() => (HttpContext.Session.GetInt32("UserId") ?? 0) > 0;

    [HttpPost("GetUsdExchangeRates")]
    public async Task<ActionResult<GetUsdExchangeRatesResponse>> GetUsdExchangeRates(
        [FromBody] GetUsdExchangeRatesRequest? request,
        CancellationToken cancellationToken)
    {
        if (request == null) return BadRequest();
        if (!HasSession()) return Unauthorized();
        request.SessionId = GetSessionId(request.SessionId, HttpContext.Session);
        var result = await _apiClient.GetUsdExchangeRatesAsync(request, cancellationToken).ConfigureAwait(false);
        if (result == null) return StatusCode(502);
        return Ok(result);
    }
}
