namespace DashboardTsy.Application.AiInsight.Requests;

public class GetBranchAiInsightRequest
{
    public string RegionCode { get; set; } = string.Empty;
    public string BranchCode { get; set; } = string.Empty;
}
