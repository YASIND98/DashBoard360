using DashboardTsy.Web.Models.NplReport.Request;
using DashboardTsy.Web.Models.NplReport.Response;
using DashboardTsy.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace DashboardTsy.Web.Controllers;

[Route("NplReport")]
[ApiController]
public class NplReportApiController : ControllerBase
{
    private readonly INplReportApiClient _apiClient;

    public NplReportApiController(INplReportApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    private static string GetSessionId(string? requestSessionId, ISession session)
    {
        if (!string.IsNullOrEmpty(requestSessionId)) return requestSessionId;
        return session.GetString("UserId") ?? string.Empty;
    }

    private bool HasSession() => (HttpContext.Session.GetInt32("UserId") ?? 0) > 0;

    [HttpPost("GetNplBalanceRatio")]
    public async Task<ActionResult<IReadOnlyList<GetNplBalanceRatioItem>>> GetNplBalanceRatio(
        [FromBody] GetNplBalanceRatioRequest? request,
        CancellationToken cancellationToken)
    {
        if (request == null) return BadRequest();
        if (!HasSession()) return Unauthorized();

        request.SessionId = GetSessionId(request.SessionId, HttpContext.Session);
        var result = await _apiClient
            .GetNplBalanceRatioAsync(request, cancellationToken)
            .ConfigureAwait(false);

        return Ok(result);
    }
}
