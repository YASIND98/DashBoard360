namespace DashboardTsy.Application.TargetReport;

public class GetProductivityCountCustomerRegionReportItem
{
    public int Id { get; set; }
    public int? ParentId { get; set; }
    public int LevelNo { get; set; }
    public int SortOrder { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal RealizationRegion { get; set; }
    public decimal RealizationBankAverage { get; set; }
    public decimal YtdChangeRegion { get; set; }
    public decimal YtdChangeBankAverage { get; set; }
    public decimal QtdChangeRegion { get; set; }
    public decimal QtdChangeBankAverage { get; set; }
}
