using DashboardTsy.Application;
using DashboardTsy.Application.ExchangeRate.Requests;
using DashboardTsy.Application.ExchangeRate.Responses;
using Microsoft.AspNetCore.Mvc;

namespace DashboardTsy.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class ExchangeRateController : ControllerBase
{
    private readonly IReportDataProvider _reportDataProvider;

    public ExchangeRateController(IReportDataProvider reportDataProvider)
    {
        _reportDataProvider = reportDataProvider;
    }

    /// <summary>
    /// POST /ExchangeRate/GetUsdExchangeRates
    /// Yabancı Para hesaplamalarında kullanılan USD kur bilgilerini döner.
    /// Seçilen tarih baz alınarak: dün (T-1), önceki gün (T-2), geçen hafta (T-7), geçen yıl (T-365).
    /// Şu an SP tanımlanmadığı için mock veri üzerinden çalışır.
    /// </summary>
    [HttpPost("GetUsdExchangeRates")]
    public ActionResult<GetUsdExchangeRatesResponse> GetUsdExchangeRates(
        [FromBody] GetUsdExchangeRatesRequest request)
    {
        if (request == null)
            return BadRequest();

        var result = _reportDataProvider.GetUsdExchangeRates(request);
        return Ok(result);
    }
}
