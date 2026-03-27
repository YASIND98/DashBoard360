using DashboardTsy.Web.Models.TargetReport;

namespace DashboardTsy.Web.Services;

public interface ITargetReportApiClient
{
    Task<GetTargetReportMenuTextsResponse?> GetTargetReportMenuTextsAsync(string sessionId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<GetTargetReportFiltersItem>> GetTargetReportFiltersAsync(GetTargetReportFiltersRequest request, CancellationToken cancellationToken = default);
    Task<GetDailyTargetReportResponse?> GetDailyTargetReportAsync(GetDailyTargetReportRequest request, CancellationToken cancellationToken = default);
    Task<GetDailyQuantityTargetReportResponse?> GetDailyQuantityTargetReportAsync(GetDailyQuantityTargetReportRequest request, CancellationToken cancellationToken = default);
    Task<GetDailyTargetReportTableHeadersResponse?> GetDailyTargetReportTableHeadersAsync(GetDailyTargetReportTableHeadersRequest request, CancellationToken cancellationToken = default);
    Task<GetMonthlyTargetReportResponse?> GetMonthlyTargetReportAsync(GetMonthlyTargetReportRequest request, CancellationToken cancellationToken = default);
    Task<GetMonthlyTargetReportTableHeadersResponse?> GetMonthlyTargetReportTableHeadersAsync(GetMonthlyTargetReportTableHeadersRequest request, CancellationToken cancellationToken = default);
}
