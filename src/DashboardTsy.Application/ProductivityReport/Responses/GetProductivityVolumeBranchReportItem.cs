namespace DashboardTsy.Application.ProductivityReport.Responses;

public class GetProductivityVolumeBranchReportItem
{
    public int Id { get; set; }
    public int? ParentId { get; set; }
    public int LevelNo { get; set; }
    public int SortOrder { get; set; }

    public string ProductName { get; set; } = string.Empty;
    public decimal RealizationBranchValue { get; set; }
    public decimal RealizationRegionAverageValue { get; set; }
    public decimal? RealizationRegionAverageValueDiff { get; set; }
    public decimal RealizationBankAverageValue { get; set; }
    public decimal? RealizationBankAverageValueDiff { get; set; }
    public decimal TargetValue { get; set; }
    public decimal HgRate { get; set; }
    public decimal NetGrowthBranchValue { get; set; }
    public decimal NetGrowthRegionAverageValue { get; set; }
    public decimal? NetGrowthRegionAverageValueDiff { get; set; }
    public decimal NetGrowthBankAverageValue { get; set; }
    public decimal? NetGrowthBankAverageValueDiff { get; set; }
    public decimal YtdBranchValue { get; set; }
    public decimal YtdRegionValue { get; set; }
    public decimal? YtdRegionValueDiff { get; set; }
    public decimal YtdBankValue { get; set; }
    public decimal? YtdBankValueDiff { get; set; }
    public decimal QtdBranchValue { get; set; }
    public decimal QtdRegionValue { get; set; }
    public decimal? QtdRegionValueDiff { get; set; }
    public decimal QtdBankValue { get; set; }
    public decimal? QtdBankValueDiff { get; set; }
}
