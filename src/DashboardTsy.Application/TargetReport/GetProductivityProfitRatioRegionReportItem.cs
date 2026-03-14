namespace DashboardTsy.Application.TargetReport;

public class GetProductivityProfitRatioRegionReportItem
{
    public int Id { get; set; }
    public int? ParentId { get; set; }
    public int LevelNo { get; set; }
    public int SortOrder { get; set; }
    public string RatioName { get; set; } = string.Empty;
    public decimal TargetValue { get; set; }
    public decimal RegionValue { get; set; }
    public decimal? RegionValueDiff { get; set; }
    public decimal BankValue { get; set; }
    public decimal? BankValueDiff { get; set; }
    public decimal RetailValue { get; set; }
    public decimal KobiValue { get; set; }
    public decimal AgricultureValue { get; set; }
    public decimal? AgricultureValueDiff { get; set; }
    public decimal CommercialValue { get; set; }
    public decimal? CommercialValueDiff { get; set; }
}
