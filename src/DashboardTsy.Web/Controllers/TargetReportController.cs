using DashboardTsy.Web.Models.TargetReport;
using DashboardTsy.Web.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;

namespace DashboardTsy.Web.Controllers;

[Route("[controller]")]
[ApiController]
public class TargetReportController : ControllerBase
{
    private readonly ITargetReportApiClient _apiClient;
    private readonly HttpClient _pupaClient;
    private readonly IScoreCardTokenService _tokenService;

    public TargetReportController(ITargetReportApiClient apiClient, IHttpClientFactory httpClientFactory, IScoreCardTokenService tokenService)
    {
        _apiClient = apiClient;
        _pupaClient = httpClientFactory.CreateClient("PupaApi");
        _tokenService = tokenService;
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

    // ── ScoreCard Proxy ──────────────────────────────────────────────────────

    [HttpPost("scorecard/authorities")]
    public Task<IActionResult> ScoreCardAuthorities([FromBody] JsonElement body, CancellationToken ct)
        => ProxyPost("scorecard/authorities", body, ct);

    [HttpGet("scorecard/periods")]
    public Task<IActionResult> ScoreCardPeriods([FromQuery] int periodTypes, CancellationToken ct)
        => ProxyGet($"scorecard/periods?periodTypes={periodTypes}", ct);

    [HttpPost("scorecard/pupa-types")]
    public Task<IActionResult> ScoreCardPupaTypes([FromBody] JsonElement body, CancellationToken ct)
        => ProxyPost("scorecard/pupa-types", body, ct);

    [HttpPost("scorecard/score-cards")]
    public Task<IActionResult> ScoreCardScoreCards([FromBody] JsonElement body, CancellationToken ct)
        => ProxyPost("scorecard/score-cards", body, ct);

    [HttpPost("scorecard/regions")]
    public Task<IActionResult> ScoreCardRegions([FromBody] JsonElement body, CancellationToken ct)
        => ProxyPost("scorecard/regions", body, ct);

    [HttpPost("scorecard/branches")]
    public Task<IActionResult> ScoreCardBranches([FromBody] JsonElement body, CancellationToken ct)
        => ProxyPost("scorecard/branches", body, ct);

    [HttpPost("scorecard/registers")]
    public Task<IActionResult> ScoreCardRegisters([FromBody] JsonElement body, CancellationToken ct)
        => ProxyPost("scorecard/registers", body, ct);

    [HttpPost("scorecard/cumulatives")]
    public Task<IActionResult> ScoreCardCumulatives([FromBody] JsonElement body, CancellationToken ct)
        => ProxyPost("scorecard/cumulatives", body, ct);

    [HttpPost("scorecard/main-view-regions")]
    public Task<IActionResult> ScoreCardMainViewRegions([FromBody] JsonElement body, CancellationToken ct)
        => ProxyPost("scorecard/main-view-regions", body, ct);

    [HttpPost("scorecard/main-view-branches")]
    public Task<IActionResult> ScoreCardMainViewBranches([FromBody] JsonElement body, CancellationToken ct)
        => ProxyPost("scorecard/main-view-branches", body, ct);

    [HttpPost("scorecard/employee-order-summaries")]
    public Task<IActionResult> ScoreCardEmployeeOrderSummaries([FromBody] JsonElement body, CancellationToken ct)
        => ProxyPost("scorecard/employee-order-summaries", body, ct);

    [HttpPost("scorecard/details")]
    public Task<IActionResult> ScoreCardDetails([FromBody] JsonElement body, CancellationToken ct)
        => ProxyPost("scorecard/details", body, ct);

    [HttpPost("scorecard/trends/product-sale-realized")]
    public Task<IActionResult> ScoreCardTrendsProductSaleRealized([FromBody] JsonElement body, CancellationToken ct)
        => ProxyPost("scorecard/trends/product-sale-realized", body, ct);

    private async Task<IActionResult> ProxyPost(string path, JsonElement body, CancellationToken ct)
    {
        if (!HasSession()) return Unauthorized();

        var token = await _tokenService.GetAccessTokenAsync(ct).ConfigureAwait(false);

        using var request = new HttpRequestMessage(HttpMethod.Post, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = new StringContent(body.GetRawText(), System.Text.Encoding.UTF8, "application/json");

        using var upstream = await _pupaClient.SendAsync(request, ct).ConfigureAwait(false);
        var content = await upstream.Content.ReadAsStringAsync(ct).ConfigureAwait(false);

        if (!upstream.IsSuccessStatusCode)
            return StatusCode((int)upstream.StatusCode, content);

        return Content(content, "application/json");
    }

    private async Task<IActionResult> ProxyGet(string pathAndQuery, CancellationToken ct)
    {
        if (!HasSession()) return Unauthorized();

        var token = await _tokenService.GetAccessTokenAsync(ct).ConfigureAwait(false);

        using var request = new HttpRequestMessage(HttpMethod.Get, pathAndQuery);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var upstream = await _pupaClient.SendAsync(request, ct).ConfigureAwait(false);
        var content = await upstream.Content.ReadAsStringAsync(ct).ConfigureAwait(false);

        if (!upstream.IsSuccessStatusCode)
            return StatusCode((int)upstream.StatusCode, content);

        return Content(content, "application/json");
    }
}
