namespace DashboardTsy.Application.ProductivityReport.Responses;

public class GetProductivityCountCardPosRatioRegionReportItem
{
    public int Id { get; set; }
    public string RatioName { get; set; } = string.Empty;
    public decimal PreviousQuarterRegionValue { get; set; }
    public decimal CurrentRegionValue { get; set; }
    public decimal CurrentBankAverageValue { get; set; }
    public decimal? CurrentRegionDiff { get; set; }
    public decimal? CurrentBankAverageDiff { get; set; }
}
