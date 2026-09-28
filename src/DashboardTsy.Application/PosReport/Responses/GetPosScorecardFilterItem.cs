namespace DashboardTsy.Application.PosReport.Responses;

/// <summary>
/// SP_RP_POS_Report_Skorkart_Filter çıktısının satır modeli.
/// Skorkart ekranındaki ürün seçimini besler; seçilen <see cref="ProductCode"/>
/// GetPosScorecardRequest.ProductCode olarak SP_RP_POS_Report_Skorkart'a gönderilir.
/// </summary>
public class GetPosScorecardFilterItem
{
    /// <summary>Ürün kodu — SP "ProductCode" kolonu (INT). Örn: 536.</summary>
    public int ProductCode { get; set; }

    /// <summary>Ürün adı — SP "URUN_ADI" kolonu. Örn: "Aktif Üye İşyeri Müşteri Stok Adedi".</summary>
    public string ProductName { get; set; } = string.Empty;
}
