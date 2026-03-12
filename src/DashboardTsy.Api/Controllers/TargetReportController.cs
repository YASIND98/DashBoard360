using DashboardTsy.Api.DataLayer;
using DashboardTsy.Api.Models.TargetReport;
using Microsoft.AspNetCore.Mvc;

namespace DashboardTsy.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TargetReportController : ControllerBase
{
    private readonly IReportDataProvider _reportDataProvider;

    public TargetReportController(IReportDataProvider reportDataProvider)
    {
        _reportDataProvider = reportDataProvider;
    }

    /// <summary>
    /// GET /api/TargetReport/GetTargetReportMenuTexts?sessionId=...
    /// EXEC [dbo].[SP_RP_GetTargetReportMenuTexts] @SessionId = ''
    /// </summary>
    [HttpGet("GetTargetReportMenuTexts")]
    public ActionResult<GetTargetReportMenuTextsResponse> GetTargetReportMenuTexts([FromQuery] string? sessionId)
    {
        var result = _reportDataProvider.GetTargetReportMenuTexts(sessionId ?? string.Empty);
        if (result == null)
            return NotFound();
        return Ok(result);
    }

    /// <summary>
    /// POST /api/TargetReport/GetTargetReportFilters
    /// EXEC dbo.SP_RP_GetTargetReportFilters @SessionId='', @FilterId=0, @FilterCode=NULL (FilterCode as "12,23,45")
    /// </summary>
    [HttpPost("GetTargetReportFilters")]
    public ActionResult<IReadOnlyList<GetTargetReportFiltersItem>> GetTargetReportFilters([FromBody] GetTargetReportFiltersRequest request)
    {
        if (request == null)
            return BadRequest();
        var result = _reportDataProvider.GetTargetReportFilters(
            request.SessionId ?? string.Empty,
            request.FilterId,
            request.FilterCode);
        return Ok(result);
    }

    /// <summary>
    /// POST /api/TargetReport/GetDailyTargetReport
    /// EXEC dbo.SP_RP_GetDailyTargetReport ...
    /// </summary>
    [HttpPost("GetDailyTargetReport")]
    public ActionResult<GetDailyTargetReportResponse> GetDailyTargetReport([FromBody] GetDailyTargetReportRequest request)
    {
        if (request == null)
            return BadRequest();
        var result = _reportDataProvider.GetDailyTargetReport(request);
        return Ok(result);
    }

    /// <summary>
    /// POST /api/TargetReport/GetDailyTargetReportTableHeaders
    /// EXEC dbo.SP_RP_GetDailyTargetReportTableHeaders @SessionId=''
    /// </summary>
    [HttpPost("GetDailyTargetReportTableHeaders")]
    public ActionResult<GetDailyTargetReportTableHeadersResponse> GetDailyTargetReportTableHeaders(
        [FromBody] GetDailyTargetReportTableHeadersRequest request)
    {
        if (request == null)
            return BadRequest();

        var result = _reportDataProvider.GetDailyTargetReportTableHeaders(request.SessionId ?? string.Empty);
        if (result == null)
            return NotFound();
        return Ok(result);
    }

    /// <summary>
    /// POST /api/TargetReport/GetMonthlyTargetReport
    /// EXEC dbo.SP_RP_GetMonthlyTargetReport @SessionId='', @ReportDate=...
    /// </summary>
    [HttpPost("GetMonthlyTargetReport")]
    public ActionResult<GetMonthlyTargetReportResponse> GetMonthlyTargetReport([FromBody] GetMonthlyTargetReportRequest request)
    {
        if (request == null)
            return BadRequest();

        var result = _reportDataProvider.GetMonthlyTargetReport(request);
        if (result == null)
            return NotFound();
        return Ok(result);
    }

    /// <summary>
    /// POST /api/TargetReport/GetMonthlyTargetReportTableHeaders
    /// EXEC dbo.SP_RP_GetMonthlyTargetReportTableHeaders @SessionId='', @ReportDate=...
    /// </summary>
    [HttpPost("GetMonthlyTargetReportTableHeaders")]
    public ActionResult<GetMonthlyTargetReportTableHeadersResponse> GetMonthlyTargetReportTableHeaders(
        [FromBody] GetMonthlyTargetReportTableHeadersRequest request)
    {
        if (request == null)
            return BadRequest();

        var result = _reportDataProvider.GetMonthlyTargetReportTableHeaders(request);
        if (result == null)
            return NotFound();
        return Ok(result);
    }
}
