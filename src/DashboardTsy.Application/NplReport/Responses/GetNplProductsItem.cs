namespace DashboardTsy.Application.NplReport.Responses;

/// <summary>
/// SP_RP_GetNplProducts çıktısı — NPL rapor filtrelerinde ve ürün seçimlerinde kullanılan ürün lookup listesi.
/// </summary>
public class GetNplProductsItem
{
    public int ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
}
