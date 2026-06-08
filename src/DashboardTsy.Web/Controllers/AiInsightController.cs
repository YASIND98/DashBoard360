using DashboardTsy.Web.Models.AiInsight.Request;
using DashboardTsy.Web.Models.AiInsight.Response;
using DashboardTsy.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace DashboardTsy.Web.Controllers;

[Route("[controller]")]
[ApiController]
public class AiInsightController : ControllerBase
{
    private readonly IAiInsightApiClient _apiClient;

    public AiInsightController(IAiInsightApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    private bool HasSession() => (HttpContext.Session.GetInt32("UserId") ?? 0) > 0;

    [HttpPost("GetBranchAiInsights")]
    public async Task<ActionResult<GetBranchAiInsightResponse>> GetBranchAiInsights(
        [FromBody] GetBranchAiInsightRequest? request,
        CancellationToken cancellationToken)
    {
        if (request == null) return BadRequest();
        if (!HasSession()) return Unauthorized();
        if (string.IsNullOrWhiteSpace(request.RegionCode) || string.IsNullOrWhiteSpace(request.BranchCode))
            return BadRequest("RegionCode and BranchCode are required.");

        var result = await _apiClient.GetBranchAiInsightsAsync(request, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }
}
