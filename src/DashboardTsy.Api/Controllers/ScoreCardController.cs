using DashboardTsy.Api.Services;
using DashboardTsy.Application.ScoreCard;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text.Json;

namespace DashboardTsy.Api.Controllers;

[ApiController]
[Route("scorecard")]
public class ScoreCardController : ControllerBase
{
    private readonly HttpClient _pupaClient;
    private readonly IScoreCardTokenService _tokenService;
    private readonly IScoreCardRequestLogQueue _requestLogQueue;
    private readonly ILogger<ScoreCardController> _logger;

    public ScoreCardController(
        IHttpClientFactory httpClientFactory,
        IScoreCardTokenService tokenService,
        IScoreCardRequestLogQueue requestLogQueue,
        ILogger<ScoreCardController> logger)
    {
        _pupaClient = httpClientFactory.CreateClient("PupaApi");
        _tokenService = tokenService;
        _requestLogQueue = requestLogQueue;
        _logger = logger;
    }

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

    private string? ReadExternalContextHeader()
        => Request.Headers.TryGetValue("ExternalContext", out var v) ? v.ToString() : null;

    private Task<IActionResult> ProxyPost(string path, JsonElement body, CancellationToken ct)
        => ProxyToPupa(HttpMethod.Post, path, body.GetRawText(), ct);

    private Task<IActionResult> ProxyGet(string pathAndQuery, CancellationToken ct)
        => ProxyToPupa(HttpMethod.Get, pathAndQuery, null, ct);

    // Her istek (başarılı, hatalı ya da exception) ScoreCardRequestLogs tablosuna yazılmak üzere kuyruğa atılır.
    // DurationMs yalnızca skor kart (Pupa) servisinin yanıt süresidir; token alma süresi dahil değildir.
    private async Task<IActionResult> ProxyToPupa(HttpMethod method, string pathAndQuery, string? body, CancellationToken ct)
    {
        var externalContext = ReadExternalContextHeader();
        var requestLog = ScoreCardRequestLogFactory.Create(method.Method, pathAndQuery, body, externalContext);
        Stopwatch? pupaCall = null;

        try
        {
            _logger.LogWarning("[ScoreCard] {Method} {PathAndQuery} -> token alınıyor", method.Method, pathAndQuery);
            string token;
            try
            {
                token = await _tokenService.GetAccessTokenAsync(ct).ConfigureAwait(false);
                _logger.LogWarning("[ScoreCard] Token alındı");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ScoreCard] Token alınamadı");
                requestLog.StatusCode = 502;
                requestLog.ErrorMessage = "Token alınamadı: " + ex.Message;
                return StatusCode(502, requestLog.ErrorMessage);
            }

            using var request = new HttpRequestMessage(method, pathAndQuery);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            if (!string.IsNullOrEmpty(externalContext))
                request.Headers.TryAddWithoutValidation("ExternalContext", externalContext);
            if (body != null)
                request.Content = new StringContent(body, System.Text.Encoding.UTF8, "application/json");

            _logger.LogWarning("[ScoreCard] Pupa isteği gönderiliyor: {BaseAddress}{PathAndQuery} | Body: {Body} | ExternalContext: {ExternalContext}", _pupaClient.BaseAddress, pathAndQuery, body ?? "(yok)", externalContext ?? "(yok)");
            pupaCall = Stopwatch.StartNew();
            using var upstream = await _pupaClient.SendAsync(request, ct).ConfigureAwait(false);
            var content = await upstream.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
            requestLog.DurationMs = pupaCall.ElapsedMilliseconds;
            _logger.LogWarning("[ScoreCard] Pupa yanıtı: {StatusCode} | Body: {Body}", (int)upstream.StatusCode, content);

            requestLog.StatusCode = (int)upstream.StatusCode;
            requestLog.ResponseBody = content;
            requestLog.IsSuccess = upstream.IsSuccessStatusCode;

            if (!upstream.IsSuccessStatusCode)
            {
                _logger.LogWarning("[ScoreCard] Pupa hata döndü: {StatusCode} {Body}", (int)upstream.StatusCode, content);
                requestLog.ErrorMessage = string.IsNullOrEmpty(content) ? upstream.ReasonPhrase : content;
                return StatusCode((int)upstream.StatusCode, content);
            }

            return Content(content, "application/json");
        }
        catch (Exception ex)
        {
            // Bağlantı hatası, timeout, iptal: kayıt hata mesajıyla yazılır, mevcut davranış (exception) korunur.
            requestLog.DurationMs ??= pupaCall?.ElapsedMilliseconds;
            requestLog.ErrorMessage = ex.Message;
            throw;
        }
        finally
        {
            _requestLogQueue.Enqueue(requestLog);
        }
    }
}
