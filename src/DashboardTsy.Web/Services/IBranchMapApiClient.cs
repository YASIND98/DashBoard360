using DashboardTsy.Web.Models.BranchMap.Request;
using DashboardTsy.Web.Models.BranchMap.Response;

namespace DashboardTsy.Web.Services;

public interface IBranchMapApiClient
{
    Task<IReadOnlyList<GetBranchMapInfoItem>> GetBranchMapInfoAsync(
        GetBranchMapInfoRequest request,
        CancellationToken cancellationToken = default);
}
