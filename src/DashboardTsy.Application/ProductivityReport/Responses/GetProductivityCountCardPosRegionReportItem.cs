namespace DashboardTsy.Application.ProductivityReport.Responses;

public class GetProductivityCountCardPosRegionReportItem
{
    public int Id { get; set; }
    public int? ParentId { get; set; }
    public int LevelNo { get; set; }
    public int SortOrder { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal CurrentMonthRegionValue { get; set; }
    public decimal CurrentMonthBankAverage { get; set; }
    public decimal? CurrentMonthBankAverageDiff { get; set; }
    public decimal ThreeMonthHgRegion { get; set; }
    public decimal ThreeMonthHgBankAverage { get; set; }
    public decimal? ThreeMonthHgBankAverageDiff { get; set; }
}
