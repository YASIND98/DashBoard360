namespace DashboardTsy.Application.ScoreCard;

/// <summary>
/// Skor kart (Pupa) servisine yapılan tek bir isteğin kaydı; DenizBank raporlaması için saklanır.
/// İstek bağlamı (input, kullanıcı, seçili sicil/bölge/şube/skor kart) istek başında, sonuç alanları istek bitince dolar.
/// </summary>
public sealed class ScoreCardRequestLog
{
    public DateTime RequestedAt { get; init; }
    public string Endpoint { get; init; } = string.Empty;
    public string HttpMethod { get; init; } = string.Empty;
    public string? RequestBody { get; init; }

    public string? RequesterUserCode { get; init; }
    public string? RequesterBranchCode { get; init; }

    public string? RegisterId { get; init; }
    public string? RegionCode { get; init; }
    public string? BranchCode { get; init; }
    public string? ScoreCardId { get; init; }
    public string? ScoreCardTypeId { get; init; }
    public string? PupaType { get; init; }

    public string? ResponseBody { get; set; }
    public int? StatusCode { get; set; }
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }

    /// <summary>Skor kart servisinin yanıt süresi (ms). Servise gidilmediyse (ör. token alınamadı) null.</summary>
    public long? DurationMs { get; set; }
}
