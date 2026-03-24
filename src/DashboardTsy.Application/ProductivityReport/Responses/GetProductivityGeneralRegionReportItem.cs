namespace DashboardTsy.Application.ProductivityReport.Responses;

public class GetProductivityGeneralRegionReportItem
{
    public int Id { get; set; }
    public int? ParentId { get; set; }
    public int LevelNo { get; set; }
    public int SortOrder { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public decimal FirstMonthRealizationRate { get; set; }
    public decimal SecondMonthRealizationRate { get; set; }
    public decimal ThirdMonthRealizationRate { get; set; }
    public decimal CorporateRate { get; set; }
    public decimal CommercialRate { get; set; }
    public decimal KbiRate { get; set; }
    public decimal ObiRate { get; set; }
    public decimal AgricultureRate { get; set; }
    public decimal MassRate { get; set; }
    public decimal AffluentRate { get; set; }
    public decimal PrivateBankingRate { get; set; }
}
