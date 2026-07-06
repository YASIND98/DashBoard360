using DashboardTsy.Web.Models.TargetReport;
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
    public async Task<ActionResult<GetTargetReportMenuTextsResponse>> GetTargetReportMenuTexts(
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
    public async Task<ActionResult<IReadOnlyList<GetTargetReportFiltersItem>>> GetTargetReportFilters(
        [FromBody] GetTargetReportFiltersRequest? request,
        CancellationToken cancellationToken)
    {
        if (request == null)
            return BadRequest();
        if (!HasSession()) return Unauthorized();
        var sid = request.SessionId;
        if (string.IsNullOrEmpty(sid))
            sid = HttpContext.Session.GetString("UserId") ?? string.Empty;
        var req = new GetTargetReportFiltersRequest
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
    public async Task<ActionResult<GetDailyTargetReportResponse>> GetDailyTargetReport(
        [FromBody] GetDailyTargetReportRequest? request,
        CancellationToken cancellationToken)
    {
        if (request == null)
            return BadRequest();

        var sid = request.SessionId;
        if (string.IsNullOrEmpty(sid))
            sid = HttpContext.Session.GetString("UserId") ?? string.Empty;

        var req = new GetDailyTargetReportRequest
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
    public async Task<ActionResult<GetDailyQuantityTargetReportTableHeadersResponse>> GetDailyQuantityTargetReportTableHeaders(
        [FromBody] GetDailyQuantityTargetReportTableHeadersRequest? request,
        CancellationToken cancellationToken)
    {
        if (request == null)
            return BadRequest();

        var sid = request.SessionId;
        if (string.IsNullOrEmpty(sid))
            sid = HttpContext.Session.GetString("UserId") ?? string.Empty;

        var req = new GetDailyQuantityTargetReportTableHeadersRequest
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
    public async Task<ActionResult<GetDailyQuantityTargetReportResponse>> GetDailyQuantityTargetReport(
        [FromBody] GetDailyQuantityTargetReportRequest? request,
        CancellationToken cancellationToken)
    {
        if (request == null)
            return BadRequest();

        var sid = request.SessionId;
        if (string.IsNullOrEmpty(sid))
            sid = HttpContext.Session.GetString("UserId") ?? string.Empty;

        var req = new GetDailyQuantityTargetReportRequest
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
    public async Task<ActionResult<ProductTop10DifferencesResponse>> GetProductTop10DailyAndWeeklyDifferences(
        [FromBody] GetProductTop10DailyAndWeeklyDifferencesRequest? request,
        CancellationToken cancellationToken)
    {
        if (request == null)
            return BadRequest();

        var req = new GetProductTop10DailyAndWeeklyDifferencesRequest
        {
            ProductId = request.ProductId,
            FilterType = request.FilterType,
            RegionId = request.RegionId,
            BranchId = request.BranchId,
            TabId = request.TabId,
            SubTabId = request.SubTabId
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
    public async Task<ActionResult<GetDailyTargetReportTableHeadersResponse>> GetDailyTargetReportTableHeaders(
        [FromBody] GetDailyTargetReportTableHeadersRequest? request,
        CancellationToken cancellationToken)
    {
        if (request == null)
            return BadRequest();

        var sid = request.SessionId;
        if (string.IsNullOrEmpty(sid))
            sid = HttpContext.Session.GetString("UserId") ?? string.Empty;

        var req = new GetDailyTargetReportTableHeadersRequest { SessionId = sid };
        var result = await _apiClient.GetDailyTargetReportTableHeadersAsync(req, cancellationToken).ConfigureAwait(false);
        if (result == null)
            return StatusCode(502);
        return Ok(result);
    }

    /// <summary>
    /// POST /api/TargetReport/GetMonthlyTargetReport - proxies to Dashboard API SP_RP_GetMonthlyTargetReport.
    /// </summary>
    [HttpPost("GetMonthlyTargetReport")]
    public async Task<ActionResult<GetMonthlyTargetReportResponse>> GetMonthlyTargetReport(
        [FromBody] GetMonthlyTargetReportRequest? request,
        CancellationToken cancellationToken)
    {
        if (request == null)
            return BadRequest();

        var sid = request.SessionId;
        if (string.IsNullOrEmpty(sid))
            sid = HttpContext.Session.GetString("UserId") ?? string.Empty;

        var req = new GetMonthlyTargetReportRequest
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
    public async Task<ActionResult<GetMonthlyTargetReportTableHeadersResponse>> GetMonthlyTargetReportTableHeaders(
        [FromBody] GetMonthlyTargetReportTableHeadersRequest? request,
        CancellationToken cancellationToken)
    {
        if (request == null)
            return BadRequest();

        var sid = request.SessionId;
        if (string.IsNullOrEmpty(sid))
            sid = HttpContext.Session.GetString("UserId") ?? string.Empty;

        var req = new GetMonthlyTargetReportTableHeadersRequest
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
