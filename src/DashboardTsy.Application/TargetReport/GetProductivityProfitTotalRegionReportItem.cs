namespace DashboardTsy.Application.TargetReport;

public class GetProductivityProfitTotalRegionReportItem
{
    public int Id { get; set; }
    public int? ParentId { get; set; }
    public int LevelNo { get; set; }
    public int SortOrder { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal TargetValue { get; set; }
    public decimal RealizationRegionValue { get; set; }
    public decimal? RealizationRegionValueDiff { get; set; }
    public decimal RealizationBankAverageValue { get; set; }
    public decimal? RealizationBankAverageValueDiff { get; set; }
    public decimal HgRegionValue { get; set; }
    public decimal? HgRegionValueDiff { get; set; }
    public decimal HgBankAverageValue { get; set; }
    public decimal? HgBankAverageValueDiff { get; set; }
    public decimal RetailValue { get; set; }
    public decimal KobiValue { get; set; }
    public decimal AgricultureValue { get; set; }
    public decimal CommercialValue { get; set; }
    public decimal? CommercialValueDiff { get; set; }
    public decimal PartnerValue { get; set; }
}
