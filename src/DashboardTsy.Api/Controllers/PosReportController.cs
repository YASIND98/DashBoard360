using DashboardTsy.Application.PosReport;
using DashboardTsy.Application.PosReport.Requests;
using DashboardTsy.Application.PosReport.Responses;
using Microsoft.AspNetCore.Mvc;

namespace DashboardTsy.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class PosReportController : ControllerBase
{
    private readonly IPosReportProvider _reportProvider;

    public PosReportController(IPosReportProvider reportProvider)
    {
        _reportProvider = reportProvider;
    }

    /// <summary>
    /// POST /PosReport/GetPosReport
    /// POS raporu — SP_RP_POS_Report çıktısını satır bazlı döner.
    /// Her satır: metrik (POS ürün göstergesi), ay bazlı değer (Value) ve bir önceki aya göre fark (DiffValue).
    /// Aynı endpoint web ve mobil taraf tarafından kullanılır (mobil için /mobile/PosReport/GetPosReport).
    /// </summary>
    [HttpPost("GetPosReport")]
    public ActionResult<IReadOnlyList<GetPosReportItem>> GetPosReport(
        [FromBody] GetPosReportRequest request)
    {
        if (request == null)
            return BadRequest();

        var result = _reportProvider.GetPosReport(request);
        return Ok(result);
    }

    /// <summary>
    /// POST /PosReport/GetPosScorecard
    /// POS skorkartı — SP_RP_POS_Report_Skorkart çıktısını satır bazlı döner.
    /// Her satır: ürün (Metrics), tarih, gerçekleşen, hedef, gerçekleşme oranı (TaRate) ve hedefe kalan fark (DiffValue).
    /// Bölge görünümü için RegionCode, şube görünümü için BranchCode dolu gönderilir.
    /// Aynı endpoint web ve mobil taraf tarafından kullanılır (mobil için /mobile/PosReport/GetPosScorecard).
    /// </summary>
    [HttpPost("GetPosScorecard")]
    public ActionResult<IReadOnlyList<GetPosScorecardItem>> GetPosScorecard(
        [FromBody] GetPosScorecardRequest request)
    {
        if (request == null)
            return BadRequest();

        var result = _reportProvider.GetPosScorecard(request);
        return Ok(result);
    }
}
