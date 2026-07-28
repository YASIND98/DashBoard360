using DashboardTsy.Application.NplReport;
using DashboardTsy.Application.NplReport.Requests;
using DashboardTsy.Application.NplReport.Responses;
using Microsoft.AspNetCore.Mvc;

namespace DashboardTsy.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class NplReportController : ControllerBase
{
    private readonly INplReportProvider _reportProvider;

    public NplReportController(INplReportProvider reportProvider)
    {
        _reportProvider = reportProvider;
    }

    /// <summary>
    /// POST /NplReport/GetNplBalanceRatio
    /// NPL Bakiye ve Oran raporu — tarih başına Anapara/KOF/Toplam bakiyeleri ve Anapara/KOF oranlarını döner.
    /// </summary>
    [HttpPost("GetNplBalanceRatio")]
    public ActionResult<IReadOnlyList<GetNplBalanceRatioItem>> GetNplBalanceRatio(
        [FromBody] GetNplBalanceRatioRequest request)
    {
        if (request == null)
            return BadRequest();

        var result = _reportProvider.GetNplBalanceRatio(request);
        return Ok(result);
    }
}
