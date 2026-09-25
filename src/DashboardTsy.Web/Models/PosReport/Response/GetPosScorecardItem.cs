namespace DashboardTsy.Web.Models.PosReport.Response;

/// <summary>
/// Web katmanının POS skorkart response DTO'su — Application katmanındaki eşleniğinden
/// bağımsız bir kopya. API'den dönen JSON buraya deserialize edilir ve view/JS'e verilir.
/// </summary>
public class GetPosScorecardItem
{
    public string Metrics { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public long? Achievement { get; set; }
    public long? Target { get; set; }
    public decimal? TaRate { get; set; }
    public long? DiffValue { get; set; }
}
