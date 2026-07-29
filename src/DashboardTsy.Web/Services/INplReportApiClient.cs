using DashboardTsy.Web.Models.NplReport.Request;
using DashboardTsy.Web.Models.NplReport.Response;

namespace DashboardTsy.Web.Services;

public interface INplReportApiClient
{
    Task<IReadOnlyList<GetNplBalanceRatioItem>> GetNplBalanceRatioAsync(
        GetNplBalanceRatioRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<GetNplFiltersItem>> GetNplFiltersAsync(
        GetNplFiltersRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<GetNplProductsItem>> GetNplProductsAsync(
        GetNplProductsRequest request,
        CancellationToken cancellationToken = default);
}
