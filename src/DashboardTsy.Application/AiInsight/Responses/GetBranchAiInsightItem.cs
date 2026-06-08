namespace DashboardTsy.Application.AiInsight.Responses;

public class GetBranchAiInsightItem
{
    public int Id { get; set; }
    public string Region { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public string BranchCode { get; set; } = string.Empty;
    public DateTime SummaryDate { get; set; }
    public string Summary { get; set; } = string.Empty;
    public string ModelName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
