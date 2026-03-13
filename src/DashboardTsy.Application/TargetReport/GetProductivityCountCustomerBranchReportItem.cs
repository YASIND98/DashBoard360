namespace DashboardTsy.Application.TargetReport;

public class GetProductivityCountCustomerBranchReportItem
{
    public int Id { get; set; }
    public int? ParentId { get; set; }
    public int LevelNo { get; set; }
    public int SortOrder { get; set; }

    public string ProductName { get; set; } = string.Empty;
    public decimal RealizationBranchValue { get; set; }
    public decimal RealizationRegionAverageValue { get; set; }
    public decimal RealizationBankAverageValue { get; set; }
    public decimal YtdNominalChangeBranchValue { get; set; }
    public decimal YtdNominalChangeRegionAverageValue { get; set; }
    public decimal YtdNominalChangeBankAverageValue { get; set; }
    public decimal QtdNominalChangeBranchValue { get; set; }
    public decimal QtdNominalChangeRegionAverageValue { get; set; }
    public decimal QtdNominalChangeBankAverageValue { get; set; }
}
