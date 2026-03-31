using DashboardTsy.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace DashboardTsy.Web.Controllers;

[Route("[controller]")]
[ApiController]
public class TargetReportController : ControllerBase
{
    private readonly ITargetReportApiClient _apiClient;

    public TargetReportController(ITargetReportApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    private bool HasSession() => (HttpContext.Session.GetInt32("UserId") ?? 0) > 0;

    /// <summary>
    /// GET /api/TargetReport/GetTargetReportMenuTexts?sessionId=...
    /// Proxies to Dashboard API SP_RP_GetTargetReportMenuTexts. SessionId can come from query or current session.
    /// </summary>
    [HttpGet("GetTargetReportMenuTexts")]
    public async Task<ActionResult<Models.TargetReport.GetTargetReportMenuTextsResponse>> GetTargetReportMenuTexts(
        [FromQuery] string? sessionId,
        CancellationToken cancellationToken)
    {
        if (!HasSession()) return Unauthorized();
        var sid = sessionId ?? HttpContext.Session.GetString("UserId")?.ToString() ?? string.Empty;
        var result = await _apiClient.GetTargetReportMenuTextsAsync(sid, cancellationToken).ConfigureAwait(false);
        if (result == null)
            return NotFound();
        return Ok(result);
    }

    /// <summary>
    /// POST /api/TargetReport/GetTargetReportFilters - proxies to Dashboard API SP_RP_GetTargetReportFilters.
    /// </summary>
    [HttpPost("GetTargetReportFilters")]
    public async Task<ActionResult<IReadOnlyList<Models.TargetReport.GetTargetReportFiltersItem>>> GetTargetReportFilters(
        [FromBody] Models.TargetReport.GetTargetReportFiltersRequest? request,
        CancellationToken cancellationToken)
    {
        if (request == null)
            return BadRequest();
        if (!HasSession()) return Unauthorized();
        var sid = request.SessionId;
        if (string.IsNullOrEmpty(sid))
            sid = HttpContext.Session.GetString("UserId") ?? string.Empty;
        var req = new Models.TargetReport.GetTargetReportFiltersRequest
        {
            SessionId = sid,
            FilterId = request.FilterId,
            FilterCode = request.FilterCode
        };
        var result = await _apiClient.GetTargetReportFiltersAsync(req, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    /// <summary>
    /// POST /api/TargetReport/GetDailyTargetReport - proxies to Dashboard API SP_RP_GetDailyTargetReport.
    /// </summary>
    [HttpPost("GetDailyTargetReport")]
    public async Task<ActionResult<Models.TargetReport.GetDailyTargetReportResponse>> GetDailyTargetReport(
        [FromBody] Models.TargetReport.GetDailyTargetReportRequest? request,
        CancellationToken cancellationToken)
    {
        if (request == null)
            return BadRequest();

        var sid = request.SessionId;
        if (string.IsNullOrEmpty(sid))
            sid = HttpContext.Session.GetString("UserId") ?? string.Empty;

        var req = new Models.TargetReport.GetDailyTargetReportRequest
        {
            SessionId = sid,
            TabId = request.TabId,
            SubTabId = request.SubTabId,
            ReportDate = request.ReportDate,
            RegionId = request.RegionId,
            BranchId = request.BranchId,
            PortfolioId = request.PortfolioId,
            SearchText = request.SearchText,
            ShowDifferences = request.ShowDifferences,
            SortBy = request.SortBy,
            IsAscending = request.IsAscending
        };

        var result = await _apiClient.GetDailyTargetReportAsync(req, cancellationToken).ConfigureAwait(false);
        if (result == null)
            return StatusCode(502);
        return Ok(result);
    }

    /// <summary>
    /// POST /api/TargetReport/GetDailyQuantityTargetReportTableHeaders - proxies to Dashboard API SP_RP_GetDailyQuantityTargetReportTableHeaders.
    /// </summary>
    [HttpPost("GetDailyQuantityTargetReportTableHeaders")]
    public async Task<ActionResult<Models.TargetReport.GetDailyQuantityTargetReportTableHeadersResponse>> GetDailyQuantityTargetReportTableHeaders(
        [FromBody] Models.TargetReport.GetDailyQuantityTargetReportTableHeadersRequest? request,
        CancellationToken cancellationToken)
    {
        if (request == null)
            return BadRequest();

        var sid = request.SessionId;
        if (string.IsNullOrEmpty(sid))
            sid = HttpContext.Session.GetString("UserId") ?? string.Empty;

        var req = new Models.TargetReport.GetDailyQuantityTargetReportTableHeadersRequest
        {
            SessionId = sid
        };

        var result = await _apiClient.GetDailyQuantityTargetReportTableHeadersAsync(req, cancellationToken).ConfigureAwait(false);
        if (result == null)
            return StatusCode(502);
        return Ok(result);
    }

    /// <summary>
    /// POST /api/TargetReport/GetDailyQuantityTargetReport - proxies to Dashboard API SP_RP_GetDailyQuantityTargetReport.
    /// </summary>
    [HttpPost("GetDailyQuantityTargetReport")]
    public async Task<ActionResult<Models.TargetReport.GetDailyQuantityTargetReportResponse>> GetDailyQuantityTargetReport(
        [FromBody] Models.TargetReport.GetDailyQuantityTargetReportRequest? request,
        CancellationToken cancellationToken)
    {
        if (request == null)
            return BadRequest();

        var sid = request.SessionId;
        if (string.IsNullOrEmpty(sid))
            sid = HttpContext.Session.GetString("UserId") ?? string.Empty;

        var req = new Models.TargetReport.GetDailyQuantityTargetReportRequest
        {
            SessionId = sid,
            TabId = request.TabId,
            SubTabId = request.SubTabId,
            ReportDate = request.ReportDate,
            RegionId = request.RegionId,
            BranchId = request.BranchId,
            PortfolioId = request.PortfolioId,
            SearchText = request.SearchText,
            ShowDifferences = request.ShowDifferences,
            SortBy = request.SortBy,
            IsAscending = request.IsAscending
        };

        var result = await _apiClient.GetDailyQuantityTargetReportAsync(req, cancellationToken).ConfigureAwait(false);
        if (result == null)
            return StatusCode(502);
        return Ok(result);
    }

    /// <summary>
    /// POST /api/TargetReport/GetProductTop10DailyAndWeeklyDifferences - proxies to Dashboard API SP_RP_GetProductTop10DailyAndWeeklyDifferences.
    /// </summary>
    [HttpPost("GetProductTop10DailyAndWeeklyDifferences")]
    public async Task<ActionResult<Models.TargetReport.ProductTop10DifferencesResponse>> GetProductTop10DailyAndWeeklyDifferences(
        [FromBody] Models.TargetReport.GetProductTop10DailyAndWeeklyDifferencesRequest? request,
        CancellationToken cancellationToken)
    {
        if (request == null)
            return BadRequest();

        var req = new Models.TargetReport.GetProductTop10DailyAndWeeklyDifferencesRequest
        {
            ProductId = request.ProductId,
            FilterType = request.FilterType
        };

        var result = await _apiClient.GetProductTop10DailyAndWeeklyDifferencesAsync(req, cancellationToken).ConfigureAwait(false);
        if (result == null)
            return StatusCode(502);
        return Ok(result);
    }

    /// <summary>
    /// POST /api/TargetReport/GetDailyTargetReportTableHeaders - proxies to Dashboard API SP_RP_GetDailyTargetReportTableHeaders.
    /// </summary>
    [HttpPost("GetDailyTargetReportTableHeaders")]
    public async Task<ActionResult<Models.TargetReport.GetDailyTargetReportTableHeadersResponse>> GetDailyTargetReportTableHeaders(
        [FromBody] Models.TargetReport.GetDailyTargetReportTableHeadersRequest? request,
        CancellationToken cancellationToken)
    {
        if (request == null)
            return BadRequest();

        var sid = request.SessionId;
        if (string.IsNullOrEmpty(sid))
            sid = HttpContext.Session.GetString("UserId") ?? string.Empty;

        var req = new Models.TargetReport.GetDailyTargetReportTableHeadersRequest { SessionId = sid };
        var result = await _apiClient.GetDailyTargetReportTableHeadersAsync(req, cancellationToken).ConfigureAwait(false);
        if (result == null)
            return StatusCode(502);
        return Ok(result);
    }

    /// <summary>
    /// POST /api/TargetReport/GetMonthlyTargetReport - proxies to Dashboard API SP_RP_GetMonthlyTargetReport.
    /// </summary>
    [HttpPost("GetMonthlyTargetReport")]
    public async Task<ActionResult<Models.TargetReport.GetMonthlyTargetReportResponse>> GetMonthlyTargetReport(
        [FromBody] Models.TargetReport.GetMonthlyTargetReportRequest? request,
        CancellationToken cancellationToken)
    {
        if (request == null)
            return BadRequest();

        var sid = request.SessionId;
        if (string.IsNullOrEmpty(sid))
            sid = HttpContext.Session.GetString("UserId") ?? string.Empty;

        var req = new Models.TargetReport.GetMonthlyTargetReportRequest
        {
            SessionId = sid,
            TabId = request.TabId,
            SubTabId = request.SubTabId,
            ReportDate = request.ReportDate,
            RegionId = request.RegionId,
            BranchId = request.BranchId,
            PortfolioId = request.PortfolioId,
            SearchText = request.SearchText,
            SortBy = request.SortBy,
            IsAscending = request.IsAscending
        };

        var result = await _apiClient.GetMonthlyTargetReportAsync(req, cancellationToken).ConfigureAwait(false);
        if (result == null)
            return StatusCode(502);
        return Ok(result);
    }

    /// <summary>
    /// POST /api/TargetReport/GetMonthlyTargetReportTableHeaders - proxies to Dashboard API SP_RP_GetMonthlyTargetReportTableHeaders.
    /// </summary>
    [HttpPost("GetMonthlyTargetReportTableHeaders")]
    public async Task<ActionResult<Models.TargetReport.GetMonthlyTargetReportTableHeadersResponse>> GetMonthlyTargetReportTableHeaders(
        [FromBody] Models.TargetReport.GetMonthlyTargetReportTableHeadersRequest? request,
        CancellationToken cancellationToken)
    {
        if (request == null)
            return BadRequest();

        var sid = request.SessionId;
        if (string.IsNullOrEmpty(sid))
            sid = HttpContext.Session.GetString("UserId") ?? string.Empty;

        var req = new Models.TargetReport.GetMonthlyTargetReportTableHeadersRequest
        {
            SessionId = sid,
            ReportDate = request.ReportDate
        };

        var result = await _apiClient.GetMonthlyTargetReportTableHeadersAsync(req, cancellationToken).ConfigureAwait(false);
        if (result == null)
            return StatusCode(502);
        return Ok(result);
    }
}
