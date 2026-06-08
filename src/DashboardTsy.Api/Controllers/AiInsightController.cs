using DashboardTsy.Application;
using DashboardTsy.Application.AiInsight.Requests;
using DashboardTsy.Application.AiInsight.Responses;
using Microsoft.AspNetCore.Mvc;

namespace DashboardTsy.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class AiInsightController : ControllerBase
{
    private readonly IReportDataProvider _reportDataProvider;

    public AiInsightController(IReportDataProvider reportDataProvider)
    {
        _reportDataProvider = reportDataProvider;
    }

    [HttpPost("GetBranchAiInsights")]
    public ActionResult<GetBranchAiInsightResponse> GetBranchAiInsights([FromBody] GetBranchAiInsightRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.RegionCode) || string.IsNullOrWhiteSpace(request.BranchCode))
            return BadRequest("RegionCode and BranchCode are required.");

        var result = _reportDataProvider.GetBranchAiInsights(request);
        return Ok(result);
    }
}
