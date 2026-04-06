using DashboardTsy.Web.Models.ProductivityReport;
using DashboardTsy.Web.Models.ProductivityReport.Request;
using DashboardTsy.Web.Models.ProductivityReport.Response;
using DashboardTsy.Web.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading;

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

    private static string GetRegionCode(ISession session)
    {
        int? regionInt = session.GetInt32("RegionCode");
        return regionInt?.ToString() ?? string.Empty;
    }

    private bool HasSession() => (HttpContext.Session.GetInt32("UserId") ?? 0) > 0;

    [HttpPost("GetProductivityReportTabs")]
    public async Task<ActionResult<IReadOnlyList<GetProductivityReportTabItem>>> GetProductivityReportTabs(
        [FromBody] GetProductivityReportTabsRequest? request,
        CancellationToken cancellationToken)
    {
        if (request == null) return BadRequest();
        if (!HasSession()) return Unauthorized();
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
        if (!HasSession()) return Unauthorized();
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
        if (!HasSession()) return Unauthorized();
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
        if (!HasSession()) return Unauthorized();
        //request.SessionId = GetSessionId(request.SessionId, HttpContext.Session);
        request.SessionId = GetRegionCode(HttpContext.Session);
        var result = await _apiClient.GetReportRegionFiltersAsync(request, cancellationToken).ConfigureAwait(false);

        var department = (HttpContext.Session.GetString("Department") ?? string.Empty).Trim().ToUpperInvariant();
        var regionCode = HttpContext.Session.GetInt32("RegionCode");

        // GM: all regions
        if (department == "GM")
            return Ok(result);

        // BOLGE / SUBE: only user's region
        if (department is "BOLGE" or "SUBE")
        {
            var rc = regionCode?.ToString() ?? string.Empty;
            var filtered = result.Where(x => string.Equals(x.Code, rc, StringComparison.OrdinalIgnoreCase)).ToList();
            return Ok(filtered);
        }

        // Unknown department: be safe (no overexposure)
        return Ok(Array.Empty<GetReportRegionFilterItem>());
    }

    [HttpPost("GetReportBranchFilters")]
    public async Task<ActionResult<IReadOnlyList<GetReportBranchFilterItem>>> GetReportBranchFilters(
        [FromBody] GetReportBranchFiltersRequest? request,
        CancellationToken cancellationToken)
    {
        if (request == null) return BadRequest();
        if (!HasSession()) return Unauthorized();
        //request.SessionId = GetSessionId(request.SessionId, HttpContext.Session);
        request.SessionId = GetRegionCode(HttpContext.Session);
        var result = await _apiClient.GetReportBranchFiltersAsync(request, cancellationToken).ConfigureAwait(false);

        var department = (HttpContext.Session.GetString("Department") ?? string.Empty).Trim().ToUpperInvariant();
        var regionCode = HttpContext.Session.GetInt32("RegionCode");
        var branchCode = HttpContext.Session.GetInt32("BranchCode");

        // GM: all branches
        if (department == "GM")
            return Ok(result);

        // BOLGE: branches in user's region
        if (department == "BOLGE")
        {
            var rc = regionCode?.ToString() ?? string.Empty;
            var filtered = result.Where(x => string.Equals(x.RegionCode, rc, StringComparison.OrdinalIgnoreCase)).ToList();
            return Ok(filtered);
        }

        // SUBE: only user's branch (and region)
        if (department == "SUBE")
        {
            var bc = branchCode?.ToString() ?? string.Empty;
            var filtered = result.Where(x => string.Equals(x.Code, bc, StringComparison.OrdinalIgnoreCase)).ToList();
            return Ok(filtered);
        }

        // Unknown department: be safe (no overexposure)
        return Ok(Array.Empty<GetReportBranchFilterItem>());
    }

    [HttpPost("GetProductivityGeneralRegionReport")]
    public async Task<ActionResult<GetProductivityGeneralRegionReportResponse>> GetProductivityGeneralRegionReport(
        [FromBody] GetProductivityGeneralRegionReportRequest? request,
        CancellationToken cancellationToken)
    {
        if (request == null) return BadRequest();
        request.SessionId = GetSessionId(request.SessionId, HttpContext.Session);
        var result = await _apiClient.GetProductivityGeneralRegionReportAsync(request, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpPost("GetProductivityCountCardPosRegionReport")]
    public async Task<ActionResult<GetProductivityCountCardPosRegionReportResponse>> GetProductivityCountCardPosRegionReport(
        [FromBody] GetProductivityCountCardPosRegionReportRequest? request,
        CancellationToken cancellationToken)
    {
        if (request == null) return BadRequest();
        request.SessionId = GetSessionId(request.SessionId, HttpContext.Session);
        var result = await _apiClient.GetProductivityCountCardPosRegionReportAsync(request, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpPost("GetProductivityCountCardPosRatioRegionReport")]
    public async Task<ActionResult<IReadOnlyList<GetProductivityCountCardPosRatioRegionReportItem>>> GetProductivityCountCardPosRatioRegionReport(
    [FromBody] GetProductivityCountCardPosRatioRegionReportRequest? request,
    CancellationToken cancellationToken)
    {
        if (request == null) return BadRequest();
        request.SessionId = GetSessionId(request.SessionId, HttpContext.Session);
        var result = await _apiClient.GetProductivityCountCardPosRatioRegionReportAsync(request, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpPost("GetProductivityCountCardPosRatioRegionReportTableHeaders")]
    public async Task<ActionResult<GetProductivityCountCardPosRatioRegionReportTableHeadersItem>> GetProductivityCountCardPosRatioRegionReportTableHeaders(
    [FromBody] GetProductivityCountCardPosRatioRegionReportTableHeadersRequest? request,
    CancellationToken cancellationToken)
    {
        if (request == null) return BadRequest();
        request.SessionId = GetSessionId(request.SessionId, HttpContext.Session);
        var result = await _apiClient.GetProductivityCountCardPosRatioRegionReportTableHeadersAsync(request, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpPost("GetProductivityCountCustomerRegionReport")]
    public async Task<ActionResult<GetProductivityCountCustomerRegionReportResponse>> GetProductivityCountCustomerRegionReport(
        [FromBody] GetProductivityCountCustomerRegionReportRequest? request,
        CancellationToken cancellationToken)
    {
        if (request == null) return BadRequest();
        request.SessionId = GetSessionId(request.SessionId, HttpContext.Session);
        var result = await _apiClient.GetProductivityCountCustomerRegionReportAsync(request, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpPost("GetProductivityVolumeRegionReport")]
    public async Task<ActionResult<GetProductivityVolumeRegionReportResponse>> GetProductivityVolumeRegionReport(
        [FromBody] GetProductivityVolumeRegionReportRequest? request,
        CancellationToken cancellationToken)
    {
        if (request == null) return BadRequest();
        request.SessionId = GetSessionId(request.SessionId, HttpContext.Session);
        var result = await _apiClient.GetProductivityVolumeRegionReportAsync(request, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpPost("GetProductivityProfitRatioRegionReport")]
    public async Task<ActionResult<GetProductivityProfitRatioRegionReportResponse>> GetProductivityProfitRatioRegionReport(
        [FromBody] GetProductivityProfitRatioRegionReportRequest? request,
        CancellationToken cancellationToken)
    {
        if (request == null) return BadRequest();
        request.SessionId = GetSessionId(request.SessionId, HttpContext.Session);
        var result = await _apiClient.GetProductivityProfitRatioRegionReportAsync(request, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpPost("GetProductivityProfitTotalRegionReport")]
    public async Task<ActionResult<GetProductivityProfitTotalRegionReportResponse>> GetProductivityProfitTotalRegionReport(
        [FromBody] GetProductivityProfitTotalRegionReportRequest? request,
        CancellationToken cancellationToken)
    {
        if (request == null) return BadRequest();
        request.SessionId = GetSessionId(request.SessionId, HttpContext.Session);
        var result = await _apiClient.GetProductivityProfitTotalRegionReportAsync(request, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpPost("GetProductivityProfitSpreadManagementRegionReport")]
    public async Task<ActionResult<GetProductivityProfitSpreadManagementRegionReportResponse>> GetProductivityProfitSpreadManagementRegionReport(
        [FromBody] GetProductivityProfitSpreadManagementRegionReportRequest? request,
        CancellationToken cancellationToken)
    {
        if (request == null) return BadRequest();
        request.SessionId = GetSessionId(request.SessionId, HttpContext.Session);
        var result = await _apiClient.GetProductivityProfitSpreadManagementRegionReportAsync(request, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpPost("GetProductivityProfitSpreadManagementBranchReport")]
    public async Task<ActionResult<GetProductivityProfitSpreadManagementBranchReportResponse>> GetProductivityProfitSpreadManagementBranchReport(
        [FromBody] GetProductivityProfitSpreadManagementBranchReportRequest? request,
        CancellationToken cancellationToken)
    {
        if (request == null) return BadRequest();
        request.SessionId = GetSessionId(request.SessionId, HttpContext.Session);
        var result = await _apiClient.GetProductivityProfitSpreadManagementBranchReportAsync(request, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpPost("GetProductivityCountCardPosBranchReport")]
    public async Task<ActionResult<GetProductivityCountCardPosBranchReportResponse>> GetProductivityCountCardPosBranchReport(
        [FromBody] GetProductivityCountCardPosBranchReportRequest? request,
        CancellationToken cancellationToken)
    {
        if (request == null) return BadRequest();
        request.SessionId = GetSessionId(request.SessionId, HttpContext.Session);
        var result = await _apiClient.GetProductivityCountCardPosBranchReportAsync(request, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpPost("GetProductivityCountCardPosRatioBranchReport")]
    public async Task<ActionResult<IReadOnlyList<GetProductivityCountCardPosRatioBranchReportItem>>> GetProductivityCountCardPosRatioBranchReport(
    [FromBody] GetProductivityCountCardPosRatioBranchReportRequest? request,
    CancellationToken cancellationToken)
    {
        if (request == null) return BadRequest();
        request.SessionId = GetSessionId(request.SessionId, HttpContext.Session);
        var result = await _apiClient.GetProductivityCountCardPosRatioBranchReportAsync(request, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpPost("GetProductivityCountCardPosRatioBranchReportTableHeaders")]
    public async Task<ActionResult<GetProductivityCountCardPosRatioBranchReportTableHeadersItem>> GetProductivityCountCardPosRatioBranchReportTableHeaders(
    [FromBody] GetProductivityCountCardPosRatioBranchReportTableHeadersRequest? request,
    CancellationToken cancellationToken)
    {
        if (request == null) return BadRequest();
        request.SessionId = GetSessionId(request.SessionId, HttpContext.Session);
        var result = await _apiClient.GetProductivityCountCardPosRatioBranchReportTableHeadersAsync(request, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpPost("GetProductivityProfitRatioBranchReport")]
    public async Task<ActionResult<GetProductivityProfitRatioBranchReportResponse>> GetProductivityProfitRatioBranchReport(
        [FromBody] GetProductivityProfitRatioBranchReportRequest? request,
        CancellationToken cancellationToken)
    {
        if (request == null) return BadRequest();
        request.SessionId = GetSessionId(request.SessionId, HttpContext.Session);
        var result = await _apiClient.GetProductivityProfitRatioBranchReportAsync(request, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpPost("GetProductivityProfitTotalBranchReport")]
    public async Task<ActionResult<GetProductivityProfitTotalBranchReportResponse>> GetProductivityProfitTotalBranchReport(
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
    public async Task<ActionResult<GetProductivityCountCustomerBranchReportResponse>> GetProductivityCountCustomerBranchReport(
        [FromBody] GetProductivityCountCustomerBranchReportRequest? request,
        CancellationToken cancellationToken)
    {
        if (request == null) return BadRequest();
        request.SessionId = GetSessionId(request.SessionId, HttpContext.Session);
        var result = await _apiClient.GetProductivityCountCustomerBranchReportAsync(request, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpPost("GetProductivityVolumeBranchReport")]
    public async Task<ActionResult<GetProductivityVolumeBranchReportResponse>> GetProductivityVolumeBranchReport(
        [FromBody] GetProductivityVolumeBranchReportRequest? request,
        CancellationToken cancellationToken)
    {
        if (request == null) return BadRequest();
        request.SessionId = GetSessionId(request.SessionId, HttpContext.Session);
        var result = await _apiClient.GetProductivityVolumeBranchReportAsync(request, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }
}
