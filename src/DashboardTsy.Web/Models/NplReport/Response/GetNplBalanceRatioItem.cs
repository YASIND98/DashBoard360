namespace DashboardTsy.Web.Models.NplReport.Response;

public class GetNplBalanceRatioItem
{
    public DateTime ReportDate { get; set; }

    public decimal? BalanceAnapara { get; set; }
    public decimal? BalanceKof { get; set; }
    public decimal? BalanceToplam { get; set; }

    public decimal? RatioAnapara { get; set; }
    public decimal? RatioKof { get; set; }
}
