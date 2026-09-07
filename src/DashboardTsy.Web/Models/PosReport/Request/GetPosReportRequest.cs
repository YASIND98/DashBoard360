namespace DashboardTsy.Web.Models.PosReport.Request;

/// <summary>
/// Web katmanının PosReport request DTO'su — Application katmanındaki eşleniğinden
/// bağımsız bir kopya. Browser'dan gelen isteği API'ye forward etmek için kullanılır.
/// </summary>
public class GetPosReportRequest
{
    public string? RegionCode { get; set; }
    public string? BranchCode { get; set; }
    public int? TabId { get; set; }
}
