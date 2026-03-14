namespace DashboardTsy.Application.TargetReport;

public class GetProductivityCountCardPosBranchReportItem
{
    public int Id { get; set; }
    public int? ParentId { get; set; }
    public int LevelNo { get; set; }
    public int SortOrder { get; set; }

    public string ProductName { get; set; } = string.Empty;
    public decimal CurrentPeriodBranchValue { get; set; }
    public decimal CurrentPeriodRegionAverageValue { get; set; }
    public decimal? CurrentPeriodRegionAverageValueDiff { get; set; }
    public decimal CurrentPeriodBankAverageValue { get; set; }
    public decimal? CurrentPeriodBankAverageValueDiff { get; set; }
    public decimal ThreeMonthHgBranchValue { get; set; }
    public decimal ThreeMonthHgRegionAverageValue { get; set; }
    public decimal? ThreeMonthHgRegionAverageValueDiff { get; set; }
    public decimal ThreeMonthHgBankAverageValue { get; set; }
    public decimal? ThreeMonthHgBankAverageValueDiff { get; set; }
}

