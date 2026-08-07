using System.Text.Json.Serialization;

namespace DashboardTsy.Api.Models.Mobile;

/// <summary>
/// Mobil client'ın beklediği ortak response zarfı: { status, message, data }.
/// /mobile/* prefix'i altında dönen tüm yanıtlar MobileEnvelopeMiddleware tarafından bu tipe sarılır.
/// </summary>
/// <remarks>
/// Property'ler mobil ekibin talep ettiği isimlerle (camelCase JSON) çıksın diye açıkça belirtildi;
/// projenin global JSON ayarları değişirse burası etkilenmez.
/// </remarks>
public sealed class MobileEnvelope<T>
{
    [JsonPropertyName("status")]
    public bool Status { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public T? Data { get; set; }

    public static MobileEnvelope<T> Success(T? data, string message = "OK")
        => new() { Status = true, Message = message, Data = data };

    public static MobileEnvelope<T> Failure(string message)
        => new() { Status = false, Message = message, Data = default };
}
