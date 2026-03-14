namespace DashboardTsy.Application.TargetReport;

public class GetProductivityProfitSpreadManagementBranchReportItem
{
    public int Id { get; set; }
    public int? ParentId { get; set; }
    public int LevelNo { get; set; }
    public int SortOrder { get; set; }

    public string Description { get; set; } = string.Empty;
    public decimal SpreadValue { get; set; }
    public decimal RatioBranchValue { get; set; }
    public decimal RatioRegionAverageValue { get; set; }
    public decimal? RatioRegionAverageValueDiff { get; set; }
    public decimal RatioBankAverageValue { get; set; }
    public decimal? RatioBankAverageValueDiff { get; set; }
    public decimal NetReturnBranchValue { get; set; }
    public decimal NetReturnRegionAverageValue { get; set; }
    public decimal? NetReturnRegionAverageValueDiff { get; set; }
    public decimal NetReturnBankAverageValue { get; set; }
    public decimal? NetReturnBankAverageValueDiff { get; set; }
    public decimal NetReturnHgBranchValue { get; set; }
    public decimal NetReturnHgRegionAverageValue { get; set; }
    public decimal? NetReturnHgRegionAverageValueDiff { get; set; }
    public decimal NetReturnHgBankAverageValue { get; set; }
    public decimal? NetReturnHgBankAverageValueDiff { get; set; }
}
