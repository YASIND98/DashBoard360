using System.Text.Json;
using DashboardTsy.Application.TargetReport;
using Microsoft.Extensions.Options;

namespace DashboardTsy.Web.Services;

public class ProductivityReportApiClient : IProductivityReportApiClient
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };
    private const string BasePath = "ProductivityReport/";

    public ProductivityReportApiClient(HttpClient httpClient, IOptions<DashboardApiOptions> options)
    {
        _httpClient = httpClient;
        var baseUrl = options.Value.BaseUrl?.TrimEnd('/') ?? "http://localhost:5219";
        _httpClient.BaseAddress = new Uri(baseUrl + "/");
    }

    public async Task<IReadOnlyList<GetProductivityReportTabItem>> GetProductivityReportTabsAsync(GetProductivityReportTabsRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync(BasePath + "GetProductivityReportTabs", request, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode) return Array.Empty<GetProductivityReportTabItem>();
        var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        var list = JsonSerializer.Deserialize<List<GetProductivityReportTabItem>>(json, _jsonOptions);
        return list ?? (IReadOnlyList<GetProductivityReportTabItem>)Array.Empty<GetProductivityReportTabItem>();
    }

    public async Task<IReadOnlyList<GetProductivityReportTableHeaderItem>> GetProductivityReportTableHeadersAsync(GetProductivityReportTableHeadersRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync(BasePath + "GetProductivityReportTableHeaders", request, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode) return Array.Empty<GetProductivityReportTableHeaderItem>();
        var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        var list = JsonSerializer.Deserialize<List<GetProductivityReportTableHeaderItem>>(json, _jsonOptions);
        return list ?? (IReadOnlyList<GetProductivityReportTableHeaderItem>)Array.Empty<GetProductivityReportTableHeaderItem>();
    }

    public async Task<IReadOnlyList<GetReportRegionFilterItem>> GetReportRegionFiltersAsync(GetReportRegionFiltersRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync(BasePath + "GetReportRegionFilters", request, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode) return Array.Empty<GetReportRegionFilterItem>();
        var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        var list = JsonSerializer.Deserialize<List<GetReportRegionFilterItem>>(json, _jsonOptions);
        return list ?? (IReadOnlyList<GetReportRegionFilterItem>)Array.Empty<GetReportRegionFilterItem>();
    }

    public async Task<IReadOnlyList<GetReportBranchFilterItem>> GetReportBranchFiltersAsync(GetReportBranchFiltersRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync(BasePath + "GetReportBranchFilters", request, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode) return Array.Empty<GetReportBranchFilterItem>();
        var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        var list = JsonSerializer.Deserialize<List<GetReportBranchFilterItem>>(json, _jsonOptions);
        return list ?? (IReadOnlyList<GetReportBranchFilterItem>)Array.Empty<GetReportBranchFilterItem>();
    }

    public async Task<IReadOnlyList<GetProductivityGeneralRegionReportItem>> GetProductivityGeneralRegionReportAsync(GetProductivityGeneralRegionReportRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync(BasePath + "GetProductivityGeneralRegionReport", request, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode) return Array.Empty<GetProductivityGeneralRegionReportItem>();
        var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        var list = JsonSerializer.Deserialize<List<GetProductivityGeneralRegionReportItem>>(json, _jsonOptions);
        return list ?? (IReadOnlyList<GetProductivityGeneralRegionReportItem>)Array.Empty<GetProductivityGeneralRegionReportItem>();
    }

    public async Task<IReadOnlyList<GetProductivityCountCardPosRegionReportItem>> GetProductivityCountCardPosRegionReportAsync(GetProductivityCountCardPosRegionReportRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync(BasePath + "GetProductivityCountCardPosRegionReport", request, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode) return Array.Empty<GetProductivityCountCardPosRegionReportItem>();
        var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        var list = JsonSerializer.Deserialize<List<GetProductivityCountCardPosRegionReportItem>>(json, _jsonOptions);
        return list ?? (IReadOnlyList<GetProductivityCountCardPosRegionReportItem>)Array.Empty<GetProductivityCountCardPosRegionReportItem>();
    }

    public async Task<IReadOnlyList<GetProductivityCountCustomerRegionReportItem>> GetProductivityCountCustomerRegionReportAsync(GetProductivityCountCustomerRegionReportRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync(BasePath + "GetProductivityCountCustomerRegionReport", request, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode) return Array.Empty<GetProductivityCountCustomerRegionReportItem>();
        var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        var list = JsonSerializer.Deserialize<List<GetProductivityCountCustomerRegionReportItem>>(json, _jsonOptions);
        return list ?? (IReadOnlyList<GetProductivityCountCustomerRegionReportItem>)Array.Empty<GetProductivityCountCustomerRegionReportItem>();
    }

    public async Task<IReadOnlyList<GetProductivityVolumeRegionReportItem>> GetProductivityVolumeRegionReportAsync(GetProductivityVolumeRegionReportRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync(BasePath + "GetProductivityVolumeRegionReport", request, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode) return Array.Empty<GetProductivityVolumeRegionReportItem>();
        var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        var list = JsonSerializer.Deserialize<List<GetProductivityVolumeRegionReportItem>>(json, _jsonOptions);
        return list ?? (IReadOnlyList<GetProductivityVolumeRegionReportItem>)Array.Empty<GetProductivityVolumeRegionReportItem>();
    }

    public async Task<IReadOnlyList<GetProductivityProfitRatioRegionReportItem>> GetProductivityProfitRatioRegionReportAsync(GetProductivityProfitRatioRegionReportRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync(BasePath + "GetProductivityProfitRatioRegionReport", request, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode) return Array.Empty<GetProductivityProfitRatioRegionReportItem>();
        var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        var list = JsonSerializer.Deserialize<List<GetProductivityProfitRatioRegionReportItem>>(json, _jsonOptions);
        return list ?? (IReadOnlyList<GetProductivityProfitRatioRegionReportItem>)Array.Empty<GetProductivityProfitRatioRegionReportItem>();
    }

    public async Task<IReadOnlyList<GetProductivityProfitTotalRegionReportItem>> GetProductivityProfitTotalRegionReportAsync(GetProductivityProfitTotalRegionReportRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync(BasePath + "GetProductivityProfitTotalRegionReport", request, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode) return Array.Empty<GetProductivityProfitTotalRegionReportItem>();
        var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        var list = JsonSerializer.Deserialize<List<GetProductivityProfitTotalRegionReportItem>>(json, _jsonOptions);
        return list ?? (IReadOnlyList<GetProductivityProfitTotalRegionReportItem>)Array.Empty<GetProductivityProfitTotalRegionReportItem>();
    }

    public async Task<IReadOnlyList<GetProductivityProfitSpreadManagementRegionReportItem>> GetProductivityProfitSpreadManagementRegionReportAsync(GetProductivityProfitSpreadManagementRegionReportRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync(BasePath + "GetProductivityProfitSpreadManagementRegionReport", request, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode) return Array.Empty<GetProductivityProfitSpreadManagementRegionReportItem>();
        var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        var list = JsonSerializer.Deserialize<List<GetProductivityProfitSpreadManagementRegionReportItem>>(json, _jsonOptions);
        return list ?? (IReadOnlyList<GetProductivityProfitSpreadManagementRegionReportItem>)Array.Empty<GetProductivityProfitSpreadManagementRegionReportItem>();
    }

    public async Task<IReadOnlyList<GetProductivityProfitSpreadManagementBranchReportItem>> GetProductivityProfitSpreadManagementBranchReportAsync(GetProductivityProfitSpreadManagementBranchReportRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync(BasePath + "GetProductivityProfitSpreadManagementBranchReport", request, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode) return Array.Empty<GetProductivityProfitSpreadManagementBranchReportItem>();
        var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        var list = JsonSerializer.Deserialize<List<GetProductivityProfitSpreadManagementBranchReportItem>>(json, _jsonOptions);
        return list ?? (IReadOnlyList<GetProductivityProfitSpreadManagementBranchReportItem>)Array.Empty<GetProductivityProfitSpreadManagementBranchReportItem>();
    }

    public async Task<IReadOnlyList<GetProductivityCountCardPosBranchReportItem>> GetProductivityCountCardPosBranchReportAsync(GetProductivityCountCardPosBranchReportRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync(BasePath + "GetProductivityCountCardPosBranchReport", request, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode) return Array.Empty<GetProductivityCountCardPosBranchReportItem>();
        var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        var list = JsonSerializer.Deserialize<List<GetProductivityCountCardPosBranchReportItem>>(json, _jsonOptions);
        return list ?? (IReadOnlyList<GetProductivityCountCardPosBranchReportItem>)Array.Empty<GetProductivityCountCardPosBranchReportItem>();
    }

    public async Task<IReadOnlyList<GetProductivityProfitRatioBranchReportItem>> GetProductivityProfitRatioBranchReportAsync(GetProductivityProfitRatioBranchReportRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync(BasePath + "GetProductivityProfitRatioBranchReport", request, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode) return Array.Empty<GetProductivityProfitRatioBranchReportItem>();
        var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        var list = JsonSerializer.Deserialize<List<GetProductivityProfitRatioBranchReportItem>>(json, _jsonOptions);
        return list ?? (IReadOnlyList<GetProductivityProfitRatioBranchReportItem>)Array.Empty<GetProductivityProfitRatioBranchReportItem>();
    }

    public async Task<IReadOnlyList<GetProductivityProfitTotalBranchReportItem>> GetProductivityProfitTotalBranchReportAsync(GetProductivityProfitTotalBranchReportRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync(BasePath + "GetProductivityProfitTotalBranchReport", request, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode) return Array.Empty<GetProductivityProfitTotalBranchReportItem>();
        var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        var list = JsonSerializer.Deserialize<List<GetProductivityProfitTotalBranchReportItem>>(json, _jsonOptions);
        return list ?? (IReadOnlyList<GetProductivityProfitTotalBranchReportItem>)Array.Empty<GetProductivityProfitTotalBranchReportItem>();
    }

    public async Task<GetProductivityBranchScoreCardReportItem?> GetProductivityBranchScoreCardReportAsync(GetProductivityBranchScoreCardReportRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync(BasePath + "GetProductivityBranchScoreCardReport", request, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode) return null;
        var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        return JsonSerializer.Deserialize<GetProductivityBranchScoreCardReportItem>(json, _jsonOptions);
    }

    public async Task<GetProductivityRegionScoreCardReportItem?> GetProductivityRegionScoreCardReportAsync(GetProductivityRegionScoreCardReportRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync(BasePath + "GetProductivityRegionScoreCardReport", request, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode) return null;
        var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        return JsonSerializer.Deserialize<GetProductivityRegionScoreCardReportItem>(json, _jsonOptions);
    }

    public async Task<IReadOnlyList<GetProductivityCountCustomerBranchReportItem>> GetProductivityCountCustomerBranchReportAsync(GetProductivityCountCustomerBranchReportRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync(BasePath + "GetProductivityCountCustomerBranchReport", request, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode) return Array.Empty<GetProductivityCountCustomerBranchReportItem>();
        var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        var list = JsonSerializer.Deserialize<List<GetProductivityCountCustomerBranchReportItem>>(json, _jsonOptions);
        return list ?? (IReadOnlyList<GetProductivityCountCustomerBranchReportItem>)Array.Empty<GetProductivityCountCustomerBranchReportItem>();
    }

    public async Task<IReadOnlyList<GetProductivityVolumeBranchReportItem>> GetProductivityVolumeBranchReportAsync(GetProductivityVolumeBranchReportRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync(BasePath + "GetProductivityVolumeBranchReport", request, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode) return Array.Empty<GetProductivityVolumeBranchReportItem>();
        var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        var list = JsonSerializer.Deserialize<List<GetProductivityVolumeBranchReportItem>>(json, _jsonOptions);
        return list ?? (IReadOnlyList<GetProductivityVolumeBranchReportItem>)Array.Empty<GetProductivityVolumeBranchReportItem>();
    }
}
