using DashboardTsy.Application.ProductivityReport.Requests;
using DashboardTsy.Application.ProductivityReport.Responses;
using DashboardTsy.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace DashboardTsy.Web.Controllers;

[Route("[controller]")]
[ApiController]
public class ProductivityReportController : ControllerBase
{
    private readonly IProductivityReportApiClient _apiClient;

    public ProductivityReportController(IProductivityReportApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    private static string GetSessionId(string? requestSessionId, ISession session)
    {
        if (!string.IsNullOrEmpty(requestSessionId)) return requestSessionId;
        return session.GetString("UserId") ?? string.Empty;
    }

    [HttpPost("GetProductivityReportTabs")]
    public async Task<ActionResult<IReadOnlyList<GetProductivityReportTabItem>>> GetProductivityReportTabs(
        [FromBody] GetProductivityReportTabsRequest? request,
        CancellationToken cancellationToken)
    {
        if (request == null) return BadRequest();
        request.SessionId = GetSessionId(request.SessionId, HttpContext.Session);
        var result = await _apiClient.GetProductivityReportTabsAsync(request, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpPost("GetProductivityReportTableHeaders")]
    public async Task<ActionResult<IReadOnlyList<GetProductivityReportTableHeaderItem>>> GetProductivityReportTableHeaders(
        [FromBody] GetProductivityReportTableHeadersRequest? request,
        CancellationToken cancellationToken)
    {
        if (request == null) return BadRequest();
        request.SessionId = GetSessionId(request.SessionId, HttpContext.Session);
        var result = await _apiClient.GetProductivityReportTableHeadersAsync(request, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpPost("GetProductivityScoreCardReportHeaders")]
    public async Task<ActionResult<IReadOnlyList<GetProductivityScoreCardReportHeaderItem>>> GetProductivityScoreCardReportHeaders(
        [FromBody] GetProductivityScoreCardReportHeadersRequest? request,
        CancellationToken cancellationToken)
    {
        if (request == null) return BadRequest();
        request.SessionId = GetSessionId(request.SessionId, HttpContext.Session);
        var result = await _apiClient.GetProductivityScoreCardReportHeadersAsync(request, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpPost("GetReportRegionFilters")]
    public async Task<ActionResult<IReadOnlyList<GetReportRegionFilterItem>>> GetReportRegionFilters(
        [FromBody] GetReportRegionFiltersRequest? request,
        CancellationToken cancellationToken)
    {
        if (request == null) return BadRequest();
        request.SessionId = GetSessionId(request.SessionId, HttpContext.Session);
        var result = await _apiClient.GetReportRegionFiltersAsync(request, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpPost("GetReportBranchFilters")]
    public async Task<ActionResult<IReadOnlyList<GetReportBranchFilterItem>>> GetReportBranchFilters(
        [FromBody] GetReportBranchFiltersRequest? request,
        CancellationToken cancellationToken)
    {
        if (request == null) return BadRequest();
        request.SessionId = GetSessionId(request.SessionId, HttpContext.Session);
        var result = await _apiClient.GetReportBranchFiltersAsync(request, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpPost("GetProductivityGeneralRegionReport")]
    public async Task<ActionResult<IReadOnlyList<GetProductivityGeneralRegionReportItem>>> GetProductivityGeneralRegionReport(
        [FromBody] GetProductivityGeneralRegionReportRequest? request,
        CancellationToken cancellationToken)
    {
        if (request == null) return BadRequest();
        request.SessionId = GetSessionId(request.SessionId, HttpContext.Session);
        var result = await _apiClient.GetProductivityGeneralRegionReportAsync(request, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpPost("GetProductivityCountCardPosRegionReport")]
    public async Task<ActionResult<IReadOnlyList<GetProductivityCountCardPosRegionReportItem>>> GetProductivityCountCardPosRegionReport(
        [FromBody] GetProductivityCountCardPosRegionReportRequest? request,
        CancellationToken cancellationToken)
    {
        if (request == null) return BadRequest();
        request.SessionId = GetSessionId(request.SessionId, HttpContext.Session);
        var result = await _apiClient.GetProductivityCountCardPosRegionReportAsync(request, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpPost("GetProductivityCountCustomerRegionReport")]
    public async Task<ActionResult<IReadOnlyList<GetProductivityCountCustomerRegionReportItem>>> GetProductivityCountCustomerRegionReport(
        [FromBody] GetProductivityCountCustomerRegionReportRequest? request,
        CancellationToken cancellationToken)
    {
        if (request == null) return BadRequest();
        request.SessionId = GetSessionId(request.SessionId, HttpContext.Session);
        var result = await _apiClient.GetProductivityCountCustomerRegionReportAsync(request, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpPost("GetProductivityVolumeRegionReport")]
    public async Task<ActionResult<IReadOnlyList<GetProductivityVolumeRegionReportItem>>> GetProductivityVolumeRegionReport(
        [FromBody] GetProductivityVolumeRegionReportRequest? request,
        CancellationToken cancellationToken)
    {
        if (request == null) return BadRequest();
        request.SessionId = GetSessionId(request.SessionId, HttpContext.Session);
        var result = await _apiClient.GetProductivityVolumeRegionReportAsync(request, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpPost("GetProductivityProfitRatioRegionReport")]
    public async Task<ActionResult<IReadOnlyList<GetProductivityProfitRatioRegionReportItem>>> GetProductivityProfitRatioRegionReport(
        [FromBody] GetProductivityProfitRatioRegionReportRequest? request,
        CancellationToken cancellationToken)
    {
        if (request == null) return BadRequest();
        request.SessionId = GetSessionId(request.SessionId, HttpContext.Session);
        var result = await _apiClient.GetProductivityProfitRatioRegionReportAsync(request, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpPost("GetProductivityProfitTotalRegionReport")]
    public async Task<ActionResult<IReadOnlyList<GetProductivityProfitTotalRegionReportItem>>> GetProductivityProfitTotalRegionReport(
        [FromBody] GetProductivityProfitTotalRegionReportRequest? request,
        CancellationToken cancellationToken)
    {
        if (request == null) return BadRequest();
        request.SessionId = GetSessionId(request.SessionId, HttpContext.Session);
        var result = await _apiClient.GetProductivityProfitTotalRegionReportAsync(request, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpPost("GetProductivityProfitSpreadManagementRegionReport")]
    public async Task<ActionResult<IReadOnlyList<GetProductivityProfitSpreadManagementRegionReportItem>>> GetProductivityProfitSpreadManagementRegionReport(
        [FromBody] GetProductivityProfitSpreadManagementRegionReportRequest? request,
        CancellationToken cancellationToken)
    {
        if (request == null) return BadRequest();
        request.SessionId = GetSessionId(request.SessionId, HttpContext.Session);
        var result = await _apiClient.GetProductivityProfitSpreadManagementRegionReportAsync(request, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpPost("GetProductivityProfitSpreadManagementBranchReport")]
    public async Task<ActionResult<IReadOnlyList<GetProductivityProfitSpreadManagementBranchReportItem>>> GetProductivityProfitSpreadManagementBranchReport(
        [FromBody] GetProductivityProfitSpreadManagementBranchReportRequest? request,
        CancellationToken cancellationToken)
    {
        if (request == null) return BadRequest();
        request.SessionId = GetSessionId(request.SessionId, HttpContext.Session);
        var result = await _apiClient.GetProductivityProfitSpreadManagementBranchReportAsync(request, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpPost("GetProductivityCountCardPosBranchReport")]
    public async Task<ActionResult<IReadOnlyList<GetProductivityCountCardPosBranchReportItem>>> GetProductivityCountCardPosBranchReport(
        [FromBody] GetProductivityCountCardPosBranchReportRequest? request,
        CancellationToken cancellationToken)
    {
        if (request == null) return BadRequest();
        request.SessionId = GetSessionId(request.SessionId, HttpContext.Session);
        var result = await _apiClient.GetProductivityCountCardPosBranchReportAsync(request, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpPost("GetProductivityProfitRatioBranchReport")]
    public async Task<ActionResult<IReadOnlyList<GetProductivityProfitRatioBranchReportItem>>> GetProductivityProfitRatioBranchReport(
        [FromBody] GetProductivityProfitRatioBranchReportRequest? request,
        CancellationToken cancellationToken)
    {
        if (request == null) return BadRequest();
        request.SessionId = GetSessionId(request.SessionId, HttpContext.Session);
        var result = await _apiClient.GetProductivityProfitRatioBranchReportAsync(request, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpPost("GetProductivityProfitTotalBranchReport")]
    public async Task<ActionResult<IReadOnlyList<GetProductivityProfitTotalBranchReportItem>>> GetProductivityProfitTotalBranchReport(
        [FromBody] GetProductivityProfitTotalBranchReportRequest? request,
        CancellationToken cancellationToken)
    {
        if (request == null) return BadRequest();
        request.SessionId = GetSessionId(request.SessionId, HttpContext.Session);
        var result = await _apiClient.GetProductivityProfitTotalBranchReportAsync(request, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpPost("GetProductivityBranchScoreCardReport")]
    public async Task<ActionResult<GetProductivityBranchScoreCardReportItem>> GetProductivityBranchScoreCardReport(
        [FromBody] GetProductivityBranchScoreCardReportRequest? request,
        CancellationToken cancellationToken)
    {
        if (request == null) return BadRequest();
        request.SessionId = GetSessionId(request.SessionId, HttpContext.Session);
        var result = await _apiClient.GetProductivityBranchScoreCardReportAsync(request, cancellationToken).ConfigureAwait(false);
        if (result == null) return StatusCode(502);
        return Ok(result);
    }

    [HttpPost("GetProductivityRegionScoreCardReport")]
    public async Task<ActionResult<GetProductivityRegionScoreCardReportItem>> GetProductivityRegionScoreCardReport(
        [FromBody] GetProductivityRegionScoreCardReportRequest? request,
        CancellationToken cancellationToken)
    {
        if (request == null) return BadRequest();
        request.SessionId = GetSessionId(request.SessionId, HttpContext.Session);
        var result = await _apiClient.GetProductivityRegionScoreCardReportAsync(request, cancellationToken).ConfigureAwait(false);
        if (result == null) return StatusCode(502);
        return Ok(result);
    }

    [HttpPost("GetProductivityCountCustomerBranchReport")]
    public async Task<ActionResult<IReadOnlyList<GetProductivityCountCustomerBranchReportItem>>> GetProductivityCountCustomerBranchReport(
        [FromBody] GetProductivityCountCustomerBranchReportRequest? request,
        CancellationToken cancellationToken)
    {
        if (request == null) return BadRequest();
        request.SessionId = GetSessionId(request.SessionId, HttpContext.Session);
        var result = await _apiClient.GetProductivityCountCustomerBranchReportAsync(request, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpPost("GetProductivityVolumeBranchReport")]
    public async Task<ActionResult<IReadOnlyList<GetProductivityVolumeBranchReportItem>>> GetProductivityVolumeBranchReport(
        [FromBody] GetProductivityVolumeBranchReportRequest? request,
        CancellationToken cancellationToken)
    {
        if (request == null) return BadRequest();
        request.SessionId = GetSessionId(request.SessionId, HttpContext.Session);
        var result = await _apiClient.GetProductivityVolumeBranchReportAsync(request, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }
}
