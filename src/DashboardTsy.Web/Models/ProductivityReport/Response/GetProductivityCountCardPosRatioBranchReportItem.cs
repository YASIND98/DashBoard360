namespace DashboardTsy.Web.Models.ProductivityReport.Response;

public class GetProductivityCountCardPosRatioBranchReportItem
{
    public int Id { get; set; }
    public string RatioName { get; set; } = string.Empty;
    public decimal PreviousQuarterBranchValue { get; set; }
    public decimal CurrentBranchValue { get; set; }
    public decimal CurrentRegionAverageValue { get; set; }
    public decimal CurrentBankAverageValue { get; set; }
    public decimal? CurrentBranchValueDiff { get; set; }
    public decimal? CurrentRegionAverageValueDiff { get; set; }
    public decimal? CurrentBankAverageValueDiff { get; set; }
}
