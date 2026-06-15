using DashboardTsy.Web.Models.AiInsight.Request;
using DashboardTsy.Web.Models.AiInsight.Response;

namespace DashboardTsy.Web.Services;

public interface IAiInsightApiClient
{
    Task<GetBranchAiInsightResponse?> GetBranchAiInsightsAsync(GetBranchAiInsightRequest request, CancellationToken cancellationToken = default);
}
