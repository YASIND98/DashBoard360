using Microsoft.AspNetCore.Mvc;

namespace DashboardTsy.Web.Controllers;

[ApiController]
[Route("app-settings")]
public class AppSettingsProxyController : ControllerBase
{
    private readonly HttpClient _apiClient;

    public AppSettingsProxyController(IHttpClientFactory httpClientFactory)
    {
        _apiClient = httpClientFactory.CreateClient("DashboardApi");
    }

    [HttpGet]
    public Task<IActionResult> GetAll(CancellationToken ct)
        => ProxyGet("app-settings", ct);

    [HttpGet("{key}")]
    public Task<IActionResult> Get(string key, CancellationToken ct)
        => ProxyGet($"app-settings/{Uri.EscapeDataString(key)}", ct);

    private async Task<IActionResult> ProxyGet(string pathAndQuery, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, pathAndQuery);
        using var upstream = await _apiClient.SendAsync(request, ct).ConfigureAwait(false);
        var content = await upstream.Content.ReadAsStringAsync(ct).ConfigureAwait(false);

        if (!upstream.IsSuccessStatusCode)
            return StatusCode((int)upstream.StatusCode, content);

        return Content(content, "application/json");
    }
}
