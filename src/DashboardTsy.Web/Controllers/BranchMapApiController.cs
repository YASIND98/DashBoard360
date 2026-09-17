using DashboardTsy.Web.Models.BranchMap.Request;
using DashboardTsy.Web.Models.BranchMap.Response;
using DashboardTsy.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace DashboardTsy.Web.Controllers;

[Route("BranchMap")]
[ApiController]
public class BranchMapApiController : ControllerBase
{
    private readonly IBranchMapApiClient _apiClient;

    public BranchMapApiController(IBranchMapApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    private bool HasSession() => (HttpContext.Session.GetInt32("UserId") ?? 0) > 0;

    [HttpPost("GetBranchMapInfo")]
    public async Task<ActionResult<IReadOnlyList<GetBranchMapInfoItem>>> GetBranchMapInfo(
        [FromBody] GetBranchMapInfoRequest? request,
        CancellationToken cancellationToken)
    {
        if (request == null) return BadRequest();
        if (!HasSession()) return Unauthorized();

        var result = await _apiClient
            .GetBranchMapInfoAsync(request, cancellationToken)
            .ConfigureAwait(false);

        return Ok(result);
    }
}
