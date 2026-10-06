using System.Text.Json;
using DashboardTsy.Application.ScoreCard;

namespace DashboardTsy.Api.Services;

/// <summary>
/// Gelen skor kart isteğinden log kaydının bağlamını çıkarır: kullanıcı ExternalContext header'ından,
/// seçili sicil/bölge/şube/skor kart request body'sinden okunur. Alan yoksa ya da JSON okunamazsa null kalır.
/// </summary>
public static class ScoreCardRequestLogFactory
{
    public static ScoreCardRequestLog Create(string httpMethod, string endpoint, string? requestBody, string? externalContext)
    {
        using var body = TryParse(requestBody);
        using var context = TryParse(externalContext);

        return new ScoreCardRequestLog
        {
            RequestedAt = DateTime.Now,
            Endpoint = endpoint,
            HttpMethod = httpMethod,
            RequestBody = requestBody,
            RequesterUserCode = ReadValue(context, "userCode"),
            RequesterBranchCode = ReadValue(context, "branchCode"),
            RegisterId = ReadValue(body, "registerId"),
            RegionCode = ReadValue(body, "regionCode"),
            BranchCode = ReadValue(body, "branchCode"),
            ScoreCardId = ReadValue(body, "scoreCardId"),
            ScoreCardTypeId = ReadValue(body, "scoreCardTypeId"),
            PupaType = ReadValue(body, "pupaType")
        };
    }

    private static JsonDocument? TryParse(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        try { return JsonDocument.Parse(json); }
        catch (JsonException) { return null; }
    }

    // Ekranlar alan adlarını farklı casing ile gönderebiliyor (ör. scoreCardId / scorecardId); eşleşme büyük/küçük harf duyarsızdır.
    private static string? ReadValue(JsonDocument? document, string propertyName)
    {
        if (document is null || document.RootElement.ValueKind != JsonValueKind.Object) return null;

        foreach (var property in document.RootElement.EnumerateObject())
        {
            if (!property.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase)) continue;

            return property.Value.ValueKind switch
            {
                JsonValueKind.String => property.Value.GetString(),
                JsonValueKind.Number or JsonValueKind.True or JsonValueKind.False => property.Value.GetRawText(),
                _ => null
            };
        }
        return null;
    }
}
