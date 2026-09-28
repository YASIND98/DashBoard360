namespace DashboardTsy.Web.Models.PosReport.Response;

/// <summary>
/// Web katmanının POS skorkart ürün filtresi response DTO'su — Application katmanındaki eşleniğinden
/// bağımsız bir kopya. Seçilen ProductCode, GetPosScorecardRequest.ProductCode olarak geri gönderilir.
/// </summary>
public class GetPosScorecardFilterItem
{
    public int ProductCode { get; set; }
    public string ProductName { get; set; } = string.Empty;
}
