namespace DashboardTsy.Web.Models.PosReport.Request;

/// <summary>
/// Web katmanının POS skorkart request DTO'su — Application katmanındaki eşleniğinden
/// bağımsız bir kopya. Browser'dan gelen isteği API'ye forward etmek için kullanılır.
/// ProductCode boş bırakılırsa API tarafında default ürün (536) uygulanır.
/// </summary>
public class GetPosScorecardRequest
{
    public string? RegionCode { get; set; }
    public string? BranchCode { get; set; }
    public int? ProductCode { get; set; }
}
