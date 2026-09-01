using System.Text;
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
/// Logging:
///   Her istek için iki satır log basılır — request body ve response body. Amaç prod'da hata olursa
///   iOS'un ne gönderdiğini ve Kutup'un ne döndüğünü görmek. Hassas alanlar (password/otp/token/…)
///   plaintext gitmez, "***" ile maskelenir.
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

    /// <summary>
    /// Loglarken plaintext gitmemesi gereken JSON property isimleri (case-insensitive).
    /// Kutup body'sinde bunlardan biri geçerse değeri "***" ile değiştirilir. Yeni alan çıkarsa buraya eklenir.
    /// </summary>
    private static readonly HashSet<string> SensitiveFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "password", "pass", "pwd",
        "otp", "otpcode", "smscode", "code",
        "token", "accesstoken", "refreshtoken", "idtoken",
        "encryptdata", "encrypteddata",
        "tckn", "tcno", "identityno", "identitynumber",
        "customerno", "customernumber",
        "cardno", "cardnumber", "cvv", "cvc"
    };

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly MockMobileAuthScenario _mockScenario;
    private readonly ILogger<MobileAuthController> _logger;

    public MobileAuthController(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        MockMobileAuthScenario mockScenario,
        ILogger<MobileAuthController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _mockScenario = mockScenario;
        _logger = logger;
    }

    private bool AuthMockEnabled => _configuration.GetValue<bool>("AuthMock:Enabled", false);

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
    /// Ortak handler — mock enabled ise mock'a, değilse Kutup'a yönlendirir. Her iki modda da
    /// request ve response body loglanır. Body iki modda da aynı şekilde okunur (buffer'a alınır) —
    /// mock JsonDocument olarak, proxy raw bytes olarak kullanır.
    /// </summary>
    private async Task<IActionResult> HandleAsync(
        string kutupPath,
        Func<JsonElement?, MockResult> mockHandler,
        CancellationToken cancellationToken)
    {
        var bodyBytes = await ReadBodyAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogInformation(
            "MobileAuth request. endpoint={Endpoint} body={RequestBody}",
            kutupPath, MaskJsonForLog(bodyBytes));

        var (result, responseBytes) = AuthMockEnabled
            ? HandleMock(bodyBytes, mockHandler)
            : await ForwardToKutupAsync(kutupPath, bodyBytes, cancellationToken).ConfigureAwait(false);

        _logger.LogInformation(
            "MobileAuth response. endpoint={Endpoint} body={ResponseBody}",
            kutupPath, MaskJsonForLog(responseBytes));

        return result;
    }

    private (IActionResult Result, byte[] ResponseBytes) HandleMock(byte[] bodyBytes, Func<JsonElement?, MockResult> mockHandler)
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
        var payloadJson = result.Payload is null
            ? string.Empty
            : JsonSerializer.Serialize(result.Payload, ResponseJsonOptions);
        var payloadBytes = Encoding.UTF8.GetBytes(payloadJson);

        return (new ContentResult
        {
            StatusCode = result.StatusCode,
            ContentType = "application/json; charset=utf-8",
            Content = payloadJson
        }, payloadBytes);
    }

    private async Task<(IActionResult Result, byte[] ResponseBytes)> ForwardToKutupAsync(
        string kutupPath,
        byte[] bodyBytes,
        CancellationToken cancellationToken)
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

        using var upstreamResponse = await client.SendAsync(
            upstreamRequest,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken).ConfigureAwait(false);

        var responseBytes = await upstreamResponse.Content
            .ReadAsByteArrayAsync(cancellationToken)
            .ConfigureAwait(false);

        return (new ContentResult
        {
            Content = Encoding.UTF8.GetString(responseBytes),
            ContentType = "application/json; charset=utf-8",
            StatusCode = (int)upstreamResponse.StatusCode
        }, responseBytes);
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

    /// <summary>
    /// JSON body'yi loga uygun hale getirir: hassas alanları maskeler, JSON değilse güvenli bir özet döner.
    /// </summary>
    private static string MaskJsonForLog(byte[]? bytes)
    {
        if (bytes is null || bytes.Length == 0) return "<empty>";

        try
        {
            using var doc = JsonDocument.Parse(bytes);
            using var output = new MemoryStream();
            using (var writer = new Utf8JsonWriter(output, new JsonWriterOptions { Indented = false }))
            {
                WriteMasked(doc.RootElement, writer);
            }
            return Encoding.UTF8.GetString(output.ToArray());
        }
        catch (JsonException)
        {
            // Non-JSON body — plaintext yayma, sadece uzunluğu belirt.
            return $"<non-json len={bytes.Length}>";
        }
    }

    private static void WriteMasked(JsonElement element, Utf8JsonWriter writer)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                writer.WriteStartObject();
                foreach (var prop in element.EnumerateObject())
                {
                    writer.WritePropertyName(prop.Name);
                    if (SensitiveFields.Contains(prop.Name) && prop.Value.ValueKind == JsonValueKind.String)
                    {
                        writer.WriteStringValue("***");
                    }
                    else
                    {
                        WriteMasked(prop.Value, writer);
                    }
                }
                writer.WriteEndObject();
                break;
            case JsonValueKind.Array:
                writer.WriteStartArray();
                foreach (var item in element.EnumerateArray())
                    WriteMasked(item, writer);
                writer.WriteEndArray();
                break;
            default:
                element.WriteTo(writer);
                break;
        }
    }
}
