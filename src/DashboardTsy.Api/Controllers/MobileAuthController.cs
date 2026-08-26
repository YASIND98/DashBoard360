using System.Text.Json;
using DashboardTsy.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace DashboardTsy.Api.Controllers;

/// <summary>
/// Mobil (iOS) client için login akışı. İki modda çalışır:
///   1) AuthMock:Enabled=false (default) → KutupYıldızı /api/Token, /api/Login2, /api/SendSmsCode, /api/GetAuth
///      endpoint'lerine PASS-THROUGH proxy. Body/authorization byte-identical forward edilir; Kutup'ta
///      hiçbir değişiklik yapılmadan iOS'a Kutup'un mevcut login akışı servis edilir.
///   2) AuthMock:Enabled=true → MockMobileAuthScenario devreye girer, Kutup'a hiç gidilmez.
///      iOS ekibi Kutup'a bağımlı kalmadan geliştirebilir. Response şeması pass-through modu ile birebir aynıdır.
///
/// Akış (her iki modda):
///   iOS → /mobile/api/Login2  → MobileEnvelopeMiddleware (/mobile prefix'i kaldırır) → /api/Login2 (bu controller)
///
/// GetAuth farkı: Kutup'ta [Authorize(Roles = "User", AuthenticationSchemes = "ApplicationSchema")] ile korunur —
/// yani SendSmsCode'dan alınan access token ile (Authorization header) çağrılması zorunludur. Bu yüzden
/// MobileEnvelopeMiddleware.AnonymousPaths listesine EKLENMEMİŞTİR: /mobile/api/GetAuth çağrısı önce
/// DashboardTsy'nin kendi JWT kontrolünden geçer, sonra ForwardToKutupAsync ile Authorization header'ı
/// Kutup'a da forward edilir (Kutup kendi JWT'sini ayrıca doğrular).
/// </summary>
[ApiController]
[Route("api")]
public sealed class MobileAuthController : ControllerBase
{
    public const string HttpClientName = "KutupYildizi";

    private static readonly JsonSerializerOptions ResponseJsonOptions = new()
    {
        // Kutup PascalCase döner (IsError, SmsGuid, EncryptData, …); mock da aynı isimlerle döndürüyor.
        // Envelope middleware'in property-name policy'sinden bağımsız olsun diye burada explicit tanımlıyoruz.
        PropertyNamingPolicy = null
    };

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly MockMobileAuthScenario _mockScenario;

    public MobileAuthController(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        MockMobileAuthScenario mockScenario)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _mockScenario = mockScenario;
    }

    private bool AuthMockEnabled => _configuration.GetValue<bool>("AuthMock:Enabled",false);

    [HttpPost("Token")]
    public Task<IActionResult> Token(CancellationToken cancellationToken)
        => HandleAsync("/api/Token", body => _mockScenario.HandleToken(body), cancellationToken);

    [HttpPost("Login2")]
    public Task<IActionResult> Login2(CancellationToken cancellationToken)
        => HandleAsync("/api/Login2", body => _mockScenario.HandleLogin2(body), cancellationToken);

    [HttpPost("SendSmsCode")]
    public Task<IActionResult> SendSmsCode(CancellationToken cancellationToken)
        => HandleAsync("/api/SendSmsCode", body => _mockScenario.HandleSendSmsCode(body), cancellationToken);

    [HttpPost("GetAuth")]
    public Task<IActionResult> GetAuth(CancellationToken cancellationToken)
        => HandleAsync("/api/GetAuth", body => _mockScenario.HandleGetAuth(body), cancellationToken);

    /// <summary>
    /// Ortak handler — mock enabled ise mock'a, değilse Kutup'a yönlendirir. Body iki modda da
    /// aynı şekilde okunur (buffer'a alınır) — mock JsonDocument olarak, proxy raw bytes olarak kullanır.
    /// </summary>
    private async Task<IActionResult> HandleAsync(
        string kutupPath,
        Func<JsonElement?, MockResult> mockHandler,
        CancellationToken cancellationToken)
    {
        var bodyBytes = await ReadBodyAsync(cancellationToken).ConfigureAwait(false);

        if (AuthMockEnabled)
        {
            return HandleMock(bodyBytes, mockHandler);
        }

        return await ForwardToKutupAsync(kutupPath, bodyBytes, cancellationToken).ConfigureAwait(false);
    }

    private IActionResult HandleMock(byte[] bodyBytes, Func<JsonElement?, MockResult> mockHandler)
    {
        JsonElement? bodyElement = null;
        if (bodyBytes.Length > 0)
        {
            try
            {
                using var doc = JsonDocument.Parse(bodyBytes);
                // Clone gerekli — using bloğu bitince JsonDocument dispose olur, JsonElement geçersizleşir.
                bodyElement = doc.RootElement.Clone();
            }
            catch (JsonException)
            {
                bodyElement = null;
            }
        }

        var result = mockHandler(bodyElement);
        return new ContentResult
        {
            StatusCode = result.StatusCode,
            ContentType = "application/json; charset=utf-8",
            Content = result.Payload is null ? string.Empty : JsonSerializer.Serialize(result.Payload, ResponseJsonOptions)
        };
    }

    private async Task<IActionResult> ForwardToKutupAsync(string kutupPath, byte[] bodyBytes, CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient(HttpClientName);

        using var upstreamRequest = new HttpRequestMessage(HttpMethod.Post, kutupPath);
        // ByteArrayContent + Content-Length otomatik — chunked encoding sorunu olmaz;
        // ayrıca body'yi zaten okuduk, StreamContent(Request.Body) burada boş kalırdı.
        upstreamRequest.Content = new ByteArrayContent(bodyBytes);
        upstreamRequest.Content.Headers.ContentType =
            new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

        // Authorization header'ı da forward et — Kutup Login2/SendSmsCode Anonymous JWT bekliyor.
        if (Request.Headers.TryGetValue("Authorization", out var auth))
        {
            upstreamRequest.Headers.TryAddWithoutValidation("Authorization", auth.ToArray());
        }

        var upstreamResponse = await client.SendAsync(
            upstreamRequest,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken).ConfigureAwait(false);

        var responseBody = await upstreamResponse.Content
            .ReadAsStringAsync(cancellationToken)
            .ConfigureAwait(false);

        return new ContentResult
        {
            Content = responseBody,
            ContentType = "application/json; charset=utf-8",
            StatusCode = (int)upstreamResponse.StatusCode
        };
    }

    /// <summary>
    /// Request body'sini tamamen buffer'a okur. İki gerekçe: (a) hem mock hem proxy aynı byte'lara ihtiyaç duyar;
    /// (b) StreamContent(Request.Body) + Content-Length hesaplamama sorunu buradan çözülür — ByteArrayContent
    /// uzunluğu otomatik biliyor.
    /// </summary>
    private async Task<byte[]> ReadBodyAsync(CancellationToken cancellationToken)
    {
        Request.EnableBuffering();
        using var ms = new MemoryStream();
        await Request.Body.CopyToAsync(ms, cancellationToken).ConfigureAwait(false);
        return ms.ToArray();
    }
}
