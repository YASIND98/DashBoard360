using DashboardTsy.Web.Models.PosReport.Request;
using DashboardTsy.Web.Models.PosReport.Response;
using DashboardTsy.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace DashboardTsy.Web.Controllers;

/// <summary>
/// Web'in browser tarafından çağrılan PosReport proxy controller'ı.
/// Session guard'ından geçtikten sonra isteği HttpClient üzerinden asıl API'ye forward eder.
/// PosReport request'inde SessionId alanı yoktur — API endpoint'i onu beklemez;
/// erişim korumasını sadece Web session (Windows auth üzerinden set edilen UserId) sağlar.
/// </summary>
[Route("PosReport")]
[ApiController]
public class PosReportApiController : ControllerBase
{
    private readonly IPosReportApiClient _apiClient;

    public PosReportApiController(IPosReportApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    private bool HasSession() => (HttpContext.Session.GetInt32("UserId") ?? 0) > 0;

    [HttpPost("GetPosReport")]
    public async Task<ActionResult<IReadOnlyList<GetPosReportItem>>> GetPosReport(
        [FromBody] GetPosReportRequest? request,
        CancellationToken cancellationToken)
    {
        if (request == null) return BadRequest();
        if (!HasSession()) return Unauthorized();

        var result = await _apiClient
            .GetPosReportAsync(request, cancellationToken)
            .ConfigureAwait(false);

        return Ok(result);
    }
}
