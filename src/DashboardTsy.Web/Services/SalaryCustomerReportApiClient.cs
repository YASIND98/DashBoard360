using DashboardTsy.Web.Models.SalaryCustomerReport.Request;
using DashboardTsy.Web.Models.SalaryCustomerReport.Response;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace DashboardTsy.Web.Services;

public class SalaryCustomerReportApiClient : ISalaryCustomerReportApiClient
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };
    private const string BasePath = "SalaryCustomerReport/";

    public SalaryCustomerReportApiClient(HttpClient httpClient, IOptions<DashboardApiOptions> options)
    {
        _httpClient = httpClient;
        var baseUrl = options.Value.BaseUrl?.TrimEnd('/') ?? "http://localhost:5219";
        _httpClient.BaseAddress = new Uri(baseUrl + "/");
    }

    public async Task<IReadOnlyList<GetSalaryCustomerReportTabItem>> GetSalaryCustomerReportTabsAsync(GetSalaryCustomerReportTabsRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync(BasePath + "GetSalaryCustomerReportTabs", request, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode) return Array.Empty<GetSalaryCustomerReportTabItem>();
        var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        var list = JsonSerializer.Deserialize<List<GetSalaryCustomerReportTabItem>>(json, _jsonOptions);
        return list ?? (IReadOnlyList<GetSalaryCustomerReportTabItem>)Array.Empty<GetSalaryCustomerReportTabItem>();
    }

    public async Task<GetSalaryCustomerVolumeReportHeadersResponse?> GetSalaryCustomerVolumeReportHeadersAsync(GetSalaryCustomerVolumeReportHeadersRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync(BasePath + "GetSalaryCustomerVolumeReportHeaders", request, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode) return null;
        var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        return JsonSerializer.Deserialize<GetSalaryCustomerVolumeReportHeadersResponse>(json, _jsonOptions);
    }

    public async Task<IReadOnlyList<GetSalaryCustomerVolumeReportItem>> GetSalaryCustomerVolumeReportAsync(GetSalaryCustomerVolumeReportRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync(BasePath + "GetSalaryCustomerVolumeReport", request, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode) return Array.Empty<GetSalaryCustomerVolumeReportItem>();
        var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        var list = JsonSerializer.Deserialize<List<GetSalaryCustomerVolumeReportItem>>(json, _jsonOptions);
        return list ?? (IReadOnlyList<GetSalaryCustomerVolumeReportItem>)Array.Empty<GetSalaryCustomerVolumeReportItem>();
    }

    public async Task<GetSalaryCustomerCrossSellReportHeadersResponse?> GetSalaryCustomerCrossSellReportHeadersAsync(GetSalaryCustomerCrossSellReportHeadersRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync(BasePath + "GetSalaryCustomerCrossSellReportHeaders", request, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode) return null;
        var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        return JsonSerializer.Deserialize<GetSalaryCustomerCrossSellReportHeadersResponse>(json, _jsonOptions);
    }

    public async Task<IReadOnlyList<GetSalaryCustomerCrossSellReportItem>> GetSalaryCustomerCrossSellReportAsync(GetSalaryCustomerCrossSellReportRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync(BasePath + "GetSalaryCustomerCrossSellReport", request, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode) return Array.Empty<GetSalaryCustomerCrossSellReportItem>();
        var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        var list = JsonSerializer.Deserialize<List<GetSalaryCustomerCrossSellReportItem>>(json, _jsonOptions);
        return list ?? (IReadOnlyList<GetSalaryCustomerCrossSellReportItem>)Array.Empty<GetSalaryCustomerCrossSellReportItem>();
    }

    public async Task<GetSalaryCustomerBankShareReportHeadersResponse?> GetSalaryCustomerBankShareReportHeadersAsync(GetSalaryCustomerBankShareReportHeadersRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync(BasePath + "GetSalaryCustomerBankShareReportHeaders", request, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode) return null;
        var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        return JsonSerializer.Deserialize<GetSalaryCustomerBankShareReportHeadersResponse>(json, _jsonOptions);
    }

    public async Task<IReadOnlyList<GetSalaryCustomerBankShareReportItem>> GetSalaryCustomerBankShareReportAsync(GetSalaryCustomerBankShareReportRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync(BasePath + "GetSalaryCustomerBankShareReport", request, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode) return Array.Empty<GetSalaryCustomerBankShareReportItem>();
        var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        var list = JsonSerializer.Deserialize<List<GetSalaryCustomerBankShareReportItem>>(json, _jsonOptions);
        return list ?? (IReadOnlyList<GetSalaryCustomerBankShareReportItem>)Array.Empty<GetSalaryCustomerBankShareReportItem>();
    }
}
