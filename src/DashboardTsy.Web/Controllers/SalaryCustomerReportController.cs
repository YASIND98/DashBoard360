using DashboardTsy.Web.Models.SalaryCustomerReport.Request;
using DashboardTsy.Web.Models.SalaryCustomerReport.Response;
using DashboardTsy.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace DashboardTsy.Web.Controllers;

[Route("[controller]")]
[ApiController]
public class SalaryCustomerReportController : ControllerBase
{
    private readonly ISalaryCustomerReportApiClient _apiClient;

    public SalaryCustomerReportController(ISalaryCustomerReportApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    private static string GetSessionId(string? requestSessionId, ISession session)
    {
        if (!string.IsNullOrEmpty(requestSessionId)) return requestSessionId;
        return session.GetString("UserId") ?? string.Empty;
    }

    private bool HasSession() => (HttpContext.Session.GetInt32("UserId") ?? 0) > 0;

    [HttpPost("GetSalaryCustomerReportTabs")]
    public async Task<ActionResult<IReadOnlyList<GetSalaryCustomerReportTabItem>>> GetSalaryCustomerReportTabs(
        [FromBody] GetSalaryCustomerReportTabsRequest? request,
        CancellationToken cancellationToken)
    {
        if (request == null) return BadRequest();
        if (!HasSession()) return Unauthorized();
        request.SessionId = GetSessionId(request.SessionId, HttpContext.Session);
        var result = await _apiClient.GetSalaryCustomerReportTabsAsync(request, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpPost("GetSalaryCustomerVolumeReportHeaders")]
    public async Task<ActionResult<GetSalaryCustomerVolumeReportHeadersResponse?>> GetSalaryCustomerVolumeReportHeaders(
        [FromBody] GetSalaryCustomerVolumeReportHeadersRequest? request,
        CancellationToken cancellationToken)
    {
        if (request == null) return BadRequest();
        if (!HasSession()) return Unauthorized();
        request.SessionId = GetSessionId(request.SessionId, HttpContext.Session);
        var result = await _apiClient.GetSalaryCustomerVolumeReportHeadersAsync(request, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpPost("GetSalaryCustomerVolumeReport")]
    public async Task<ActionResult<IReadOnlyList<GetSalaryCustomerVolumeReportItem>>> GetSalaryCustomerVolumeReport(
        [FromBody] GetSalaryCustomerVolumeReportRequest? request,
        CancellationToken cancellationToken)
    {
        if (request == null) return BadRequest();
        if (!HasSession()) return Unauthorized();
        request.SessionId = GetSessionId(request.SessionId, HttpContext.Session);
        var result = await _apiClient.GetSalaryCustomerVolumeReportAsync(request, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpPost("GetSalaryCustomerCrossSellReportHeaders")]
    public async Task<ActionResult<GetSalaryCustomerCrossSellReportHeadersResponse?>> GetSalaryCustomerCrossSellReportHeaders(
        [FromBody] GetSalaryCustomerCrossSellReportHeadersRequest? request,
        CancellationToken cancellationToken)
    {
        if (request == null) return BadRequest();
        if (!HasSession()) return Unauthorized();
        request.SessionId = GetSessionId(request.SessionId, HttpContext.Session);
        var result = await _apiClient.GetSalaryCustomerCrossSellReportHeadersAsync(request, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpPost("GetSalaryCustomerCrossSellReport")]
    public async Task<ActionResult<IReadOnlyList<GetSalaryCustomerCrossSellReportItem>>> GetSalaryCustomerCrossSellReport(
        [FromBody] GetSalaryCustomerCrossSellReportRequest? request,
        CancellationToken cancellationToken)
    {
        if (request == null) return BadRequest();
        if (!HasSession()) return Unauthorized();
        request.SessionId = GetSessionId(request.SessionId, HttpContext.Session);
        var result = await _apiClient.GetSalaryCustomerCrossSellReportAsync(request, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpPost("GetSalaryCustomerBankShareReportHeaders")]
    public async Task<ActionResult<GetSalaryCustomerBankShareReportHeadersResponse?>> GetSalaryCustomerBankShareReportHeaders(
        [FromBody] GetSalaryCustomerBankShareReportHeadersRequest? request,
        CancellationToken cancellationToken)
    {
        if (request == null) return BadRequest();
        if (!HasSession()) return Unauthorized();
        request.SessionId = GetSessionId(request.SessionId, HttpContext.Session);
        var result = await _apiClient.GetSalaryCustomerBankShareReportHeadersAsync(request, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpPost("GetSalaryCustomerBankShareReport")]
    public async Task<ActionResult<IReadOnlyList<GetSalaryCustomerBankShareReportItem>>> GetSalaryCustomerBankShareReport(
        [FromBody] GetSalaryCustomerBankShareReportRequest? request,
        CancellationToken cancellationToken)
    {
        if (request == null) return BadRequest();
        if (!HasSession()) return Unauthorized();
        request.SessionId = GetSessionId(request.SessionId, HttpContext.Session);
        var result = await _apiClient.GetSalaryCustomerBankShareReportAsync(request, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }
}
