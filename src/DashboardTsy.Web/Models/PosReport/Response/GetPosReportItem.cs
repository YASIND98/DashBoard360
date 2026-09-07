namespace DashboardTsy.Web.Models.PosReport.Response;

/// <summary>
/// Web katmanının PosReport response DTO'su — Application katmanındaki eşleniğinden
/// bağımsız bir kopya. API'den dönen JSON buraya deserialize edilir ve view/JS'e verilir.
/// </summary>
public class GetPosReportItem
{
    public string Metrics { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public long? Value { get; set; }
    public long? DiffValue { get; set; }
}
