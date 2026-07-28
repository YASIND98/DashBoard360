namespace DashboardTsy.Application.NplReport.Responses;

/// <summary>
/// sp_NPL_Bakiye_Oran çıktısının tarih başına pivotlanmış hâli.
/// SP long-format döner (Tur=BAKIYE|ORAN, Kolon=Anapara|KOF|Toplam); pivotlama Infrastructure'da yapılır.
/// </summary>
public class GetNplBalanceRatioItem
{
    public DateTime ReportDate { get; set; }

    public decimal? BalanceAnapara { get; set; }
    public decimal? BalanceKof { get; set; }
    public decimal? BalanceToplam { get; set; }

    public decimal? RatioAnapara { get; set; }
    public decimal? RatioKof { get; set; }
}
