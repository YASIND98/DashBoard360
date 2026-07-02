using DashboardTsy.Api.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;

namespace DashboardTsy.Api.Controllers;

[ApiController]
[Route("scorecard")]
public class ScoreCardProxyController : ControllerBase
{
    private readonly HttpClient _pupaClient;
    private readonly IScoreCardTokenService _tokenService;

    public ScoreCardProxyController(IHttpClientFactory httpClientFactory, IScoreCardTokenService tokenService)
    {
        _pupaClient = httpClientFactory.CreateClient("PupaApi");
        _tokenService = tokenService;
    }

    // POST /scorecard/authorities
    [HttpPost("authorities")]
    public Task<IActionResult> Authorities([FromBody] JsonElement body, CancellationToken ct)
        => ProxyPost("scorecard/authorities", body, ct);

    // GET /scorecard/periods?periodTypes={1|2|3}
    [HttpGet("periods")]
    public Task<IActionResult> Periods([FromQuery] int periodTypes, CancellationToken ct)
        => ProxyGet($"scorecard/periods?periodTypes={periodTypes}", ct);

    // POST /scorecard/pupa-types
    [HttpPost("pupa-types")]
    public Task<IActionResult> PupaTypes([FromBody] JsonElement body, CancellationToken ct)
        => ProxyPost("scorecard/pupa-types", body, ct);

    // POST /scorecard/score-cards
    [HttpPost("score-cards")]
    public Task<IActionResult> ScoreCards([FromBody] JsonElement body, CancellationToken ct)
        => ProxyPost("scorecard/score-cards", body, ct);

    // POST /scorecard/regions
    [HttpPost("regions")]
    public Task<IActionResult> Regions([FromBody] JsonElement body, CancellationToken ct)
        => ProxyPost("scorecard/regions", body, ct);

    // POST /scorecard/branches
    [HttpPost("branches")]
    public Task<IActionResult> Branches([FromBody] JsonElement body, CancellationToken ct)
        => ProxyPost("scorecard/branches", body, ct);

    // POST /scorecard/registers
    [HttpPost("registers")]
    public Task<IActionResult> Registers([FromBody] JsonElement body, CancellationToken ct)
        => ProxyPost("scorecard/registers", body, ct);

    // POST /scorecard/cumulatives
    [HttpPost("cumulatives")]
    public Task<IActionResult> Cumulatives([FromBody] JsonElement body, CancellationToken ct)
        => ProxyPost("scorecard/cumulatives", body, ct);

    // POST /scorecard/main-view-regions
    [HttpPost("main-view-regions")]
    public Task<IActionResult> MainViewRegions([FromBody] JsonElement body, CancellationToken ct)
        => ProxyPost("scorecard/main-view-regions", body, ct);

    // POST /scorecard/main-view-branches
    [HttpPost("main-view-branches")]
    public Task<IActionResult> MainViewBranches([FromBody] JsonElement body, CancellationToken ct)
        => ProxyPost("scorecard/main-view-branches", body, ct);

    // POST /scorecard/employee-order-summaries
    [HttpPost("employee-order-summaries")]
    public Task<IActionResult> EmployeeOrderSummaries([FromBody] JsonElement body, CancellationToken ct)
        => ProxyPost("scorecard/employee-order-summaries", body, ct);

    // POST /scorecard/details
    [HttpPost("details")]
    public Task<IActionResult> Details([FromBody] JsonElement body, CancellationToken ct)
        => ProxyPost("scorecard/details", body, ct);

    // POST /scorecard/trends/product-sale-realized
    [HttpPost("trends/product-sale-realized")]
    public Task<IActionResult> TrendsProductSaleRealized([FromBody] JsonElement body, CancellationToken ct)
        => ProxyPost("scorecard/trends/product-sale-realized", body, ct);

    // ── helpers ──────────────────────────────────────────────────────────────

    private async Task<IActionResult> ProxyPost(string path, JsonElement body, CancellationToken ct)
    {
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
