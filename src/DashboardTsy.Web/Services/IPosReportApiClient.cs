using DashboardTsy.Web.Models.PosReport.Request;
using DashboardTsy.Web.Models.PosReport.Response;

namespace DashboardTsy.Web.Services;

public interface IPosReportApiClient
{
    Task<IReadOnlyList<GetPosReportItem>> GetPosReportAsync(
        GetPosReportRequest request,
        CancellationToken cancellationToken = default);
}
