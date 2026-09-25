namespace DashboardTsy.Application.PosReport.Responses;

/// <summary>
/// SP_RP_POS_Report_Skorkart çıktısının satır modeli.
/// Her satır: bir ürün (Metrics), o ürünün belirli bir tarihteki gerçekleşeni (Achievement), hedefi (Target),
/// gerçekleşme oranı (TaRate) ve hedefe kalan fark (DiffValue).
/// PosReport ile tutarlı olsun diye flat liste dönülür; gruplama client tarafında yapılır.
/// </summary>
public class GetPosScorecardItem
{
    /// <summary>Ürün adı — SP "Metrics" kolonu (URUN_ADI).</summary>
    public string Metrics { get; set; } = string.Empty;

    /// <summary>Değerin ait olduğu tarih — SP "Date" kolonu (TARIH).</summary>
    public DateTime Date { get; set; }

    /// <summary>Gerçekleşen — SP "Achievement" kolonu (BIGINT, SUM_GERCEKLESEN).</summary>
    public long? Achievement { get; set; }

    /// <summary>Hedef — SP "Target" kolonu (BIGINT, SUM_HEDEF).</summary>
    public long? Target { get; set; }

    /// <summary>Gerçekleşme oranı (Achievement / Target) — SP "TA_Rate" kolonu (DECIMAL(10,2)).</summary>
    public decimal? TaRate { get; set; }

    /// <summary>Hedefe kalan (Target - Achievement) — SP "DiffValue" kolonu (BIGINT). Negatif = hedef aşıldı.</summary>
    public long? DiffValue { get; set; }
}
