using DashboardTsy.Application.BranchMap.Requests;
using DashboardTsy.Application.BranchMap.Responses;

namespace DashboardTsy.Application.BranchMap;

/// <summary>
/// BranchMap için application katmanı kontratı. Stored procedure çağrısını soyutlar.
/// </summary>
public interface IBranchMapProvider
{
    /// <summary>
    /// Bölge/şube harita bilgisi — şube adı, adresi ve konumunu (enlem/boylam) döner.
    /// </summary>
    IReadOnlyList<GetBranchMapInfoItem> GetBranchMapInfo(GetBranchMapInfoRequest request);
}
