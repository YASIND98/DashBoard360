using DashboardTsy.Web.Models.NplReport.Request;
using DashboardTsy.Web.Models.NplReport.Response;

namespace DashboardTsy.Web.Services;

public interface INplReportApiClient
{
    Task<IReadOnlyList<GetNplBalanceRatioItem>> GetNplBalanceRatioAsync(
        GetNplBalanceRatioRequest request,
        CancellationToken cancellationToken = default);
}
