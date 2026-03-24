namespace DashboardTsy.Application.ProductivityReport.Responses;

public class GetProductivityBranchScoreCardReportItem
{
    public string ManagerName { get; set; } = string.Empty;
    public decimal FirstMonthScore { get; set; }
    public decimal SecondMonthScore { get; set; }
    public decimal ThirdMonthScore { get; set; }
    public decimal? CorporateScore { get; set; }
    public decimal? CommercialScore { get; set; }
    public decimal? KobiScore { get; set; }
    public decimal? ObiScore { get; set; }
    public decimal? AgricultureScore { get; set; }
    public decimal? MassScore { get; set; }
    public decimal? AffluentScore { get; set; }
    public decimal? PrivateBankingScore { get; set; }
    public decimal BranchNpsScore { get; set; }
    public decimal BankNpsScore { get; set; }
}
