using System.Text.Json;
using DashboardTsy.Api.Models.Mobile;

namespace DashboardTsy.Api.Middleware;

/// <summary>
/// /mobile/* path'i altındaki tüm istekleri işler:
///   1. Path'ten "/mobile" prefix'ini kaldırır — böylece mevcut controller route'ları aynen match olur.
///   2. Controller'ın döndüğü JSON body'yi yakalar, MobileEnvelope&lt;T&gt; içine sararak client'a yazar.
///   3. Beklenmedik exception yakalarsa HTTP 500 + Failure envelope döner.
///   4. HTTP status code (200/400/404/500) korunur, sadece body sarılır.
///
/// Web (/mobile prefix'i olmayan) istekler bu middleware'i şeffaf olarak geçer — mevcut davranış değişmez.
/// </summary>
public sealed class MobileEnvelopeMiddleware
{
    private const string MobileSegment = "/mobile";
    private const string SuccessMessage = "OK";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never
    };

    private readonly RequestDelegate _next;
    private readonly ILogger<MobileEnvelopeMiddleware> _logger;

    public MobileEnvelopeMiddleware(RequestDelegate next, ILogger<MobileEnvelopeMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // /mobile prefix'i yoksa şeffaf geçir; web tarafı hiç etkilenmez.
        if (!context.Request.Path.StartsWithSegments(MobileSegment, out var remainingPath))
        {
            await _next(context);
            return;
        }

        // Path'i rewrite et: controller route'ları /mobile öneki olmadan tanımlı, mevcut match'lerin çalışması için.
        // Bu middleware UseRouting'den ÖNCE çalışmalıdır; aksi halde endpoint çoktan bind edilmiş olur ve
        // rewrite yeni route match'i tetiklemez.
        var originalPath = context.Request.Path;
        context.Request.Path = remainingPath.HasValue ? remainingPath : "/";

        // Response body'yi geçici bir buffer'a yönlendir; sarma sonrası gerçek stream'e yazacağız.
        var originalBody = context.Response.Body;
        using var buffer = new MemoryStream();
        context.Response.Body = buffer;

        try
        {
            await _next(context);
            await WriteEnvelopeAsync(context, buffer, originalBody);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception in /mobile pipeline for {Path}", originalPath);
            await WriteFailureAsync(context, originalBody, StatusCodes.Status500InternalServerError, "Sunucu hatası");
        }
        finally
        {
            // Response.Body'yi eski haline getir; sonraki middleware'ler için kritik.
            context.Response.Body = originalBody;
            context.Request.Path = originalPath;
        }
    }

    private static async Task WriteEnvelopeAsync(HttpContext context, MemoryStream buffer, Stream originalBody)
    {
        buffer.Seek(0, SeekOrigin.Begin);

        // Ham byte'lar üzerinden çalış — bazı controller'lar (özellikle UTF-8 default encoding kullananlar) response'a
        // BOM (EF BB BF) ekleyebiliyor; JsonDocument.Parse(string) BOM'u tolere etmez, sessizce sıyırmak gerek.
        var bodyBytes = buffer.ToArray();
        var jsonBytes = TrimUtf8Bom(bodyBytes);

        var statusCode = context.Response.StatusCode;
        var isSuccess = statusCode >= 200 && statusCode < 300;

        // JSON olmayan (HTML view, plain text, dosya indirme) yanıtları sarma; olduğu gibi geçir.
        // Bu, ScoreCard/AppSettings/Public gibi controller'lardan farklı content-type dönebilme ihtimalini de kapsar.
        var contentType = context.Response.ContentType ?? string.Empty;
        if (!contentType.Contains("application/json", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(contentType))
        {
            await WriteRawBytesAsync(originalBody, bodyBytes, context);
            return;
        }

        // Envelope'a saracağımız içerik: data alanı bir JSON değeri olarak kalsın (string escape'lenmesin).
        // MobileEnvelope<object?> ile serialize ederken data'ya JsonElement geçirmek en temiz yol.
        JsonElement? dataElement = null;
        string? failureMessageFromBody = null;

        if (jsonBytes.Length > 0)
        {
            try
            {
                using var doc = JsonDocument.Parse(jsonBytes);
                if (isSuccess)
                {
                    dataElement = doc.RootElement.Clone();
                }
                else
                {
                    // Hata payload'unda anlamlı bir "message" veya "title" varsa yakalayıp öne çıkar.
                    failureMessageFromBody = TryExtractErrorMessage(doc.RootElement);
                }
            }
            catch (JsonException)
            {
                // Beklenmedik non-JSON body — sarmadan olduğu gibi ilet.
                await WriteRawBytesAsync(originalBody, bodyBytes, context);
                return;
            }
        }

        var envelope = isSuccess
            ? new MobileEnvelope<JsonElement?> { Status = true, Message = SuccessMessage, Data = dataElement }
            : new MobileEnvelope<JsonElement?>
            {
                Status = false,
                Message = failureMessageFromBody ?? DefaultFailureMessage(statusCode),
                Data = null
            };

        var payload = JsonSerializer.SerializeToUtf8Bytes(envelope, SerializerOptions);

        // Content-Length'i yeni body'ye göre güncelle; aksi halde reverse proxy/compression bozar.
        context.Response.ContentType = "application/json; charset=utf-8";
        context.Response.ContentLength = payload.LongLength;

        await originalBody.WriteAsync(payload);
    }

    private static async Task WriteFailureAsync(HttpContext context, Stream originalBody, int statusCode, string message)
    {
        // Response headers henüz gönderilmemişse status'u da düzelt; gönderildiyse (nadir) sadece body yaz.
        if (!context.Response.HasStarted)
            context.Response.StatusCode = statusCode;

        var envelope = new MobileEnvelope<object?> { Status = false, Message = message, Data = null };
        var payload = JsonSerializer.SerializeToUtf8Bytes(envelope, SerializerOptions);

        context.Response.ContentType = "application/json; charset=utf-8";
        context.Response.ContentLength = payload.LongLength;

        await originalBody.WriteAsync(payload);
    }

    private static async Task WriteRawBytesAsync(Stream originalBody, byte[] rawBytes, HttpContext context)
    {
        // Content-Length'i orijinal byte uzunluğuna göre yaz — encoding roundtrip'i yok, byte'lar aynen iletilir.
        context.Response.ContentLength = rawBytes.LongLength;
        await originalBody.WriteAsync(rawBytes);
    }

    /// <summary>
    /// UTF-8 BOM (EF BB BF) varsa temizler. StringContent(Encoding.UTF8, ...) veya bazı content negotiator'lar
    /// response'un başına BOM koyabilir; JsonDocument.Parse bunu invalid start of value olarak reddeder.
    /// </summary>
    private static byte[] TrimUtf8Bom(byte[] bytes)
    {
        if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
        {
            var trimmed = new byte[bytes.Length - 3];
            Buffer.BlockCopy(bytes, 3, trimmed, 0, trimmed.Length);
            return trimmed;
        }
        return bytes;
    }

    /// <summary>
    /// ASP.NET Core ProblemDetails / ValidationProblemDetails yanıtlarından okunabilir bir mesaj çıkarır.
    /// Bulamazsa null döner ve çağıran taraf status koduna göre generic bir mesaj kullanır.
    /// </summary>
    private static string? TryExtractErrorMessage(JsonElement root)
    {
        if (root.ValueKind != JsonValueKind.Object) return null;

        // ProblemDetails: { "title": "...", "detail": "..." }
        if (root.TryGetProperty("detail", out var detail) && detail.ValueKind == JsonValueKind.String)
            return detail.GetString();

        if (root.TryGetProperty("title", out var title) && title.ValueKind == JsonValueKind.String)
            return title.GetString();

        // Basit { "error": "..." } veya { "message": "..." } patternleri
        if (root.TryGetProperty("error", out var error) && error.ValueKind == JsonValueKind.String)
            return error.GetString();

        if (root.TryGetProperty("message", out var msg) && msg.ValueKind == JsonValueKind.String)
            return msg.GetString();

        return null;
    }

    private static string DefaultFailureMessage(int statusCode) => statusCode switch
    {
        400 => "Geçersiz istek",
        401 => "Yetkisiz",
        403 => "Erişim yok",
        404 => "Bulunamadı",
        408 => "Zaman aşımı",
        409 => "Çakışma",
        _ when statusCode >= 500 => "Sunucu hatası",
        _ => "Hata"
    };
}
