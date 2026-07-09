using DashboardTsy.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace DashboardTsy.Web.Controllers;

[ApiController]
[Route("scorecard")]
public class ScoreCardProxyController : ControllerBase
{
    private readonly HttpClient _apiClient;

    public ScoreCardProxyController(IHttpClientFactory httpClientFactory, IOptions<DashboardApiOptions> apiOptions)
    {
        _apiClient = httpClientFactory.CreateClient("DashboardApi");
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

    [HttpPost("types")]
    public Task<IActionResult> Types([FromBody] JsonElement body, CancellationToken ct)
        => ProxyPost("scorecard/types", body, ct);

    private string? BuildExternalContext(JsonElement body)
    {
        var userCode = body.ValueKind == JsonValueKind.Object
            && body.TryGetProperty("userCode", out var el)
            && el.ValueKind == JsonValueKind.String
                ? el.GetString()
                : HttpContext.Session.GetString("Username");
        if (string.IsNullOrEmpty(userCode))
            return null;

        var branchCode = HttpContext.Session.GetInt32("BranchCode") ?? 0;

        return JsonSerializer.Serialize(new
        {
            BranchCode = branchCode,
            ChannelCode = "BATCH",
            UserCode = userCode,
            TranCode = "BATCH"
        });
    }

    private string? BuildExternalContextFromSession()
    {
        var userCode = HttpContext.Session.GetString("Username");
        if (string.IsNullOrEmpty(userCode))
            return null;

        var branchCode = HttpContext.Session.GetInt32("BranchCode") ?? 0;

        return JsonSerializer.Serialize(new
        {
            BranchCode = branchCode,
            ChannelCode = "BATCH",
            UserCode = userCode,
            TranCode = "BATCH"
        });
    }

    private async Task<IActionResult> ProxyPost(string path, JsonElement body, CancellationToken ct)
    {
        //if (!HasSession()) return Unauthorized();

        using var request = new HttpRequestMessage(HttpMethod.Post, path);
        var externalContext = BuildExternalContext(body);
        if (!string.IsNullOrEmpty(externalContext))
            request.Headers.TryAddWithoutValidation("ExternalContext", externalContext);
        request.Content = new StringContent(body.GetRawText(), System.Text.Encoding.UTF8, "application/json");

        using var upstream = await _apiClient.SendAsync(request, ct).ConfigureAwait(false);
        var content = await upstream.Content.ReadAsStringAsync(ct).ConfigureAwait(false);

        if (!upstream.IsSuccessStatusCode)
            return StatusCode((int)upstream.StatusCode, content);

        return Content(content, "application/json");
    }

    private async Task<IActionResult> ProxyGet(string pathAndQuery, CancellationToken ct)
    {
        //if (!HasSession()) return Unauthorized();

        using var request = new HttpRequestMessage(HttpMethod.Get, pathAndQuery);
        var externalContext = BuildExternalContextFromSession();
        if (!string.IsNullOrEmpty(externalContext))
            request.Headers.TryAddWithoutValidation("ExternalContext", externalContext);

        using var upstream = await _apiClient.SendAsync(request, ct).ConfigureAwait(false);
        var content = await upstream.Content.ReadAsStringAsync(ct).ConfigureAwait(false);

        if (!upstream.IsSuccessStatusCode)
            return StatusCode((int)upstream.StatusCode, content);

        return Content(content, "application/json");
    }
}
