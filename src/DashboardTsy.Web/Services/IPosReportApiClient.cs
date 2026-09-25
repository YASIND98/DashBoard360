using DashboardTsy.Web.Models.PosReport.Request;
using DashboardTsy.Web.Models.PosReport.Response;

namespace DashboardTsy.Web.Services;

public interface IPosReportApiClient
{
    Task<IReadOnlyList<GetPosReportItem>> GetPosReportAsync(
        GetPosReportRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<GetPosScorecardItem>> GetPosScorecardAsync(
        GetPosScorecardRequest request,
        CancellationToken cancellationToken = default);
}
