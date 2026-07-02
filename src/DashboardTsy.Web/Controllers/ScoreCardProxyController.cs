using DashboardTsy.Web.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;

namespace DashboardTsy.Web.Controllers;

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

    private bool HasSession() => (HttpContext.Session.GetInt32("UserId") ?? 0) > 0;

    [HttpPost("authorities")]
    public Task<IActionResult> Authorities([FromBody] JsonElement body, CancellationToken ct)
        => ProxyPost("scorecard/authorities", body, ct);

    [HttpGet("periods")]
    public Task<IActionResult> Periods([FromQuery] int periodTypes, CancellationToken ct)
        => ProxyGet($"scorecard/periods?periodTypes={periodTypes}", ct);

    [HttpPost("pupa-types")]
    public Task<IActionResult> PupaTypes([FromBody] JsonElement body, CancellationToken ct)
        => ProxyPost("scorecard/pupa-types", body, ct);

    [HttpPost("score-cards")]
    public Task<IActionResult> ScoreCards([FromBody] JsonElement body, CancellationToken ct)
        => ProxyPost("scorecard/score-cards", body, ct);

    [HttpPost("regions")]
    public Task<IActionResult> Regions([FromBody] JsonElement body, CancellationToken ct)
        => ProxyPost("scorecard/regions", body, ct);

    [HttpPost("branches")]
    public Task<IActionResult> Branches([FromBody] JsonElement body, CancellationToken ct)
        => ProxyPost("scorecard/branches", body, ct);

    [HttpPost("registers")]
    public Task<IActionResult> Registers([FromBody] JsonElement body, CancellationToken ct)
        => ProxyPost("scorecard/registers", body, ct);

    [HttpPost("cumulatives")]
    public Task<IActionResult> Cumulatives([FromBody] JsonElement body, CancellationToken ct)
        => ProxyPost("scorecard/cumulatives", body, ct);

    [HttpPost("main-view-regions")]
    public Task<IActionResult> MainViewRegions([FromBody] JsonElement body, CancellationToken ct)
        => ProxyPost("scorecard/main-view-regions", body, ct);

    [HttpPost("main-view-branches")]
    public Task<IActionResult> MainViewBranches([FromBody] JsonElement body, CancellationToken ct)
        => ProxyPost("scorecard/main-view-branches", body, ct);

    [HttpPost("employee-order-summaries")]
    public Task<IActionResult> EmployeeOrderSummaries([FromBody] JsonElement body, CancellationToken ct)
        => ProxyPost("scorecard/employee-order-summaries", body, ct);

    [HttpPost("details")]
    public Task<IActionResult> Details([FromBody] JsonElement body, CancellationToken ct)
        => ProxyPost("scorecard/details", body, ct);

    [HttpPost("trends/product-sale-realized")]
    public Task<IActionResult> TrendsProductSaleRealized([FromBody] JsonElement body, CancellationToken ct)
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
