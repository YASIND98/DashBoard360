using DashboardTsy.Application.BranchMap;
using DashboardTsy.Application.BranchMap.Requests;
using DashboardTsy.Application.BranchMap.Responses;
using Microsoft.AspNetCore.Mvc;

namespace DashboardTsy.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class BranchMapController : ControllerBase
{
    private readonly IBranchMapProvider _branchMapProvider;

    public BranchMapController(IBranchMapProvider branchMapProvider)
    {
        _branchMapProvider = branchMapProvider;
    }

    /// <summary>
    /// POST /BranchMap/GetBranchMapInfo
    /// Bölge/şube harita bilgisi — şube adı, adresi ve konumunu (enlem/boylam) döner.
    /// </summary>
    [HttpPost("GetBranchMapInfo")]
    public ActionResult<IReadOnlyList<GetBranchMapInfoItem>> GetBranchMapInfo(
        [FromBody] GetBranchMapInfoRequest request)
    {
        if (request == null)
            return BadRequest();

        var result = _branchMapProvider.GetBranchMapInfo(request);
        return Ok(result);
    }
}
