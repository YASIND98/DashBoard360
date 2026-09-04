namespace DashboardTsy.Application.PosReport.Responses;

/// <summary>
/// SP_RP_POS_Report çıktısının satır modeli.
/// Her satır: bir metrik (POS ürün/adet göstergesi), o metriğin belirli bir aya ait değeri (Value)
/// ve önceki aya göre farkı (DiffValue).
/// Gruplama (metric bazında birleştirme) client tarafında yapılır — projedeki diğer rapor
/// endpoint'leriyle (NPL, SalaryCustomer) tutarlı olsun diye flat liste dönülür.
/// </summary>
public class GetPosReportItem
{
    /// <summary>Metrik adı — SP "Metrics" kolonu. Örn. "Sahip Müşteri Adedi", "Aktif Müşteri Adedi".</summary>
    public string Metrics { get; set; } = string.Empty;

    /// <summary>Metriğin ait olduğu tarih — SP "Date" kolonu (ay sonu).</summary>
    public DateTime Date { get; set; }

    /// <summary>O tarihteki değer — SP "Value" kolonu (BIGINT, NULL olabilir).</summary>
    public long? Value { get; set; }

    /// <summary>Bir önceki aya göre fark — SP "DiffValue" kolonu (BIGINT, ilk ayda NULL olabilir).</summary>
    public long? DiffValue { get; set; }
}
