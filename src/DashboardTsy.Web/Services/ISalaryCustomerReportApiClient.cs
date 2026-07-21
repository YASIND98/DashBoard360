using DashboardTsy.Web.Models.SalaryCustomerReport.Request;
using DashboardTsy.Web.Models.SalaryCustomerReport.Response;

namespace DashboardTsy.Web.Services;

public interface ISalaryCustomerReportApiClient
{
    Task<IReadOnlyList<GetSalaryCustomerReportTabItem>> GetSalaryCustomerReportTabsAsync(GetSalaryCustomerReportTabsRequest request, CancellationToken cancellationToken = default);
    Task<GetSalaryCustomerVolumeReportHeadersResponse?> GetSalaryCustomerVolumeReportHeadersAsync(GetSalaryCustomerVolumeReportHeadersRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<GetSalaryCustomerVolumeReportItem>> GetSalaryCustomerVolumeReportAsync(GetSalaryCustomerVolumeReportRequest request, CancellationToken cancellationToken = default);
    Task<GetSalaryCustomerCrossSellReportHeadersResponse?> GetSalaryCustomerCrossSellReportHeadersAsync(GetSalaryCustomerCrossSellReportHeadersRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<GetSalaryCustomerCrossSellReportItem>> GetSalaryCustomerCrossSellReportAsync(GetSalaryCustomerCrossSellReportRequest request, CancellationToken cancellationToken = default);
    Task<GetSalaryCustomerBankShareReportHeadersResponse?> GetSalaryCustomerBankShareReportHeadersAsync(GetSalaryCustomerBankShareReportHeadersRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<GetSalaryCustomerBankShareReportItem>> GetSalaryCustomerBankShareReportAsync(GetSalaryCustomerBankShareReportRequest request, CancellationToken cancellationToken = default);
}
