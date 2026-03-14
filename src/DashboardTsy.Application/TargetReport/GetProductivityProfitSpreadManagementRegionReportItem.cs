namespace DashboardTsy.Application.TargetReport;

public class GetProductivityProfitSpreadManagementRegionReportItem
{
    public int Id { get; set; }
    public int? ParentId { get; set; }
    public int LevelNo { get; set; }
    public int SortOrder { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal SpreadValue { get; set; }
    public decimal RatioRegionValue { get; set; }
    public decimal? RatioRegionValueDiff { get; set; }
    public decimal RatioBankAverageValue { get; set; }
    public decimal? RatioBankAverageValueDiff { get; set; }
    public decimal NetReturnRegionValue { get; set; }
    public decimal? NetReturnRegionValueDiff { get; set; }
    public decimal NetReturnBankAverageValue { get; set; }
    public decimal? NetReturnBankAverageValueDiff { get; set; }
    public decimal NetReturnHgRegionValue { get; set; }
    public decimal? NetReturnHgRegionValueDiff { get; set; }
    public decimal NetReturnHgBankAverageValue { get; set; }
    public decimal? NetReturnHgBankAverageValueDiff { get; set; }
}
