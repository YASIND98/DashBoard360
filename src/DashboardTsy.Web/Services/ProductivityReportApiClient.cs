using DashboardTsy.Web.Models.ProductivityReport;
using DashboardTsy.Web.Models.ProductivityReport.Request;
using DashboardTsy.Web.Models.ProductivityReport.Response;
using Microsoft.Extensions.Options;
using System.Text.Json;

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

    public async Task<IReadOnlyList<GetProductivityScoreCardReportHeaderItem>> GetProductivityScoreCardReportHeadersAsync(GetProductivityScoreCardReportHeadersRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync(BasePath + "GetProductivityScoreCardReportHeaders", request, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode) return Array.Empty<GetProductivityScoreCardReportHeaderItem>();
        var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        var list = JsonSerializer.Deserialize<List<GetProductivityScoreCardReportHeaderItem>>(json, _jsonOptions);
        return list ?? (IReadOnlyList<GetProductivityScoreCardReportHeaderItem>)Array.Empty<GetProductivityScoreCardReportHeaderItem>();
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

    public async Task<GetProductivityGeneralRegionReportResponse?> GetProductivityGeneralRegionReportAsync(GetProductivityGeneralRegionReportRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync(BasePath + "GetProductivityGeneralRegionReport", request, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode) return null;
        var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        return JsonSerializer.Deserialize<GetProductivityGeneralRegionReportResponse>(json, _jsonOptions);
    }

    public async Task<GetProductivityCountCardPosRegionReportResponse?> GetProductivityCountCardPosRegionReportAsync(GetProductivityCountCardPosRegionReportRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync(BasePath + "GetProductivityCountCardPosRegionReport", request, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode) return null;
        var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        return JsonSerializer.Deserialize<GetProductivityCountCardPosRegionReportResponse>(json, _jsonOptions);
    }

    public async Task<IReadOnlyList<GetProductivityCountCardPosRatioRegionReportItem>> GetProductivityCountCardPosRatioRegionReportAsync(GetProductivityCountCardPosRatioRegionReportRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync(BasePath + "GetProductivityCountCardPosRatioRegionReport", request, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode) return Array.Empty<GetProductivityCountCardPosRatioRegionReportItem>();
        var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        var list = JsonSerializer.Deserialize<List<GetProductivityCountCardPosRatioRegionReportItem>>(json, _jsonOptions);
        return list ?? (IReadOnlyList<GetProductivityCountCardPosRatioRegionReportItem>)Array.Empty<GetProductivityCountCardPosRatioRegionReportItem>();
    }

    public async Task<GetProductivityCountCardPosRatioRegionReportTableHeadersItem?> GetProductivityCountCardPosRatioRegionReportTableHeadersAsync(GetProductivityCountCardPosRatioRegionReportTableHeadersRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync(BasePath + "GetProductivityCountCardPosRatioRegionReportTableHeaders", request, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode) return null;
        var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        return JsonSerializer.Deserialize<GetProductivityCountCardPosRatioRegionReportTableHeadersItem>(json, _jsonOptions);
    }

    public async Task<GetProductivityCountCustomerRegionReportResponse?> GetProductivityCountCustomerRegionReportAsync(GetProductivityCountCustomerRegionReportRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync(BasePath + "GetProductivityCountCustomerRegionReport", request, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode) return null;
        var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        return JsonSerializer.Deserialize<GetProductivityCountCustomerRegionReportResponse>(json, _jsonOptions);
    }

    public async Task<GetProductivityVolumeRegionReportResponse?> GetProductivityVolumeRegionReportAsync(GetProductivityVolumeRegionReportRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync(BasePath + "GetProductivityVolumeRegionReport", request, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode) return null;
        var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        return JsonSerializer.Deserialize<GetProductivityVolumeRegionReportResponse>(json, _jsonOptions);
    }

    public async Task<GetProductivityProfitRatioRegionReportResponse?> GetProductivityProfitRatioRegionReportAsync(GetProductivityProfitRatioRegionReportRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync(BasePath + "GetProductivityProfitRatioRegionReport", request, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode) return null;
        var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        return JsonSerializer.Deserialize<GetProductivityProfitRatioRegionReportResponse>(json, _jsonOptions);
    }

    public async Task<GetProductivityProfitTotalRegionReportResponse?> GetProductivityProfitTotalRegionReportAsync(GetProductivityProfitTotalRegionReportRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync(BasePath + "GetProductivityProfitTotalRegionReport", request, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode) return null;
        var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        return JsonSerializer.Deserialize<GetProductivityProfitTotalRegionReportResponse>(json, _jsonOptions);
    }

    public async Task<GetProductivityProfitSpreadManagementRegionReportResponse?> GetProductivityProfitSpreadManagementRegionReportAsync(GetProductivityProfitSpreadManagementRegionReportRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync(BasePath + "GetProductivityProfitSpreadManagementRegionReport", request, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode) return null;
        var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        return JsonSerializer.Deserialize<GetProductivityProfitSpreadManagementRegionReportResponse>(json, _jsonOptions);
    }

    public async Task<GetProductivityProfitSpreadManagementBranchReportResponse?> GetProductivityProfitSpreadManagementBranchReportAsync(GetProductivityProfitSpreadManagementBranchReportRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync(BasePath + "GetProductivityProfitSpreadManagementBranchReport", request, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode) return null;
        var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        return JsonSerializer.Deserialize<GetProductivityProfitSpreadManagementBranchReportResponse>(json, _jsonOptions);
    }

    public async Task<GetProductivityCountCardPosBranchReportResponse?> GetProductivityCountCardPosBranchReportAsync(GetProductivityCountCardPosBranchReportRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync(BasePath + "GetProductivityCountCardPosBranchReport", request, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode) return null;
        var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        return JsonSerializer.Deserialize<GetProductivityCountCardPosBranchReportResponse>(json, _jsonOptions);
    }

    public async Task<IReadOnlyList<GetProductivityCountCardPosRatioBranchReportItem>> GetProductivityCountCardPosRatioBranchReportAsync(GetProductivityCountCardPosRatioBranchReportRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync(BasePath + "GetProductivityCountCardPosRatioBranchReport", request, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode) return Array.Empty<GetProductivityCountCardPosRatioBranchReportItem>();
        var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        var list = JsonSerializer.Deserialize<List<GetProductivityCountCardPosRatioBranchReportItem>>(json, _jsonOptions);
        return list ?? (IReadOnlyList<GetProductivityCountCardPosRatioBranchReportItem>)Array.Empty<GetProductivityCountCardPosRatioBranchReportItem>();
    }

    public async Task<GetProductivityCountCardPosRatioBranchReportTableHeadersItem?> GetProductivityCountCardPosRatioBranchReportTableHeadersAsync(GetProductivityCountCardPosRatioBranchReportTableHeadersRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync(BasePath + "GetProductivityCountCardPosRatioBranchReportTableHeaders", request, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode) return null;
        var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        return JsonSerializer.Deserialize<GetProductivityCountCardPosRatioBranchReportTableHeadersItem>(json, _jsonOptions);
    }

    public async Task<GetProductivityProfitRatioBranchReportResponse?> GetProductivityProfitRatioBranchReportAsync(GetProductivityProfitRatioBranchReportRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync(BasePath + "GetProductivityProfitRatioBranchReport", request, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode) return null;
        var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        return JsonSerializer.Deserialize<GetProductivityProfitRatioBranchReportResponse>(json, _jsonOptions);
    }

    public async Task<GetProductivityProfitTotalBranchReportResponse?> GetProductivityProfitTotalBranchReportAsync(GetProductivityProfitTotalBranchReportRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync(BasePath + "GetProductivityProfitTotalBranchReport", request, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode) return null;
        var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        return JsonSerializer.Deserialize<GetProductivityProfitTotalBranchReportResponse>(json, _jsonOptions);
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

    public async Task<GetProductivityCountCustomerBranchReportResponse?> GetProductivityCountCustomerBranchReportAsync(GetProductivityCountCustomerBranchReportRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync(BasePath + "GetProductivityCountCustomerBranchReport", request, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode) return null;
        var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        return JsonSerializer.Deserialize<GetProductivityCountCustomerBranchReportResponse>(json, _jsonOptions);
    }

    public async Task<GetProductivityVolumeBranchReportResponse?> GetProductivityVolumeBranchReportAsync(GetProductivityVolumeBranchReportRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync(BasePath + "GetProductivityVolumeBranchReport", request, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode) return null;
        var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        return JsonSerializer.Deserialize<GetProductivityVolumeBranchReportResponse>(json, _jsonOptions);
    }

    public async Task<IReadOnlyList<GetReportSidebarItem>> GetReportSidebarItemsAsync(GetReportSidebarItemsRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync(BasePath + "GetReportSidebarItems", request, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode) return Array.Empty<GetReportSidebarItem>();
        var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        var list = JsonSerializer.Deserialize<List<GetReportSidebarItem>>(json, _jsonOptions);
        return list ?? (IReadOnlyList<GetReportSidebarItem>)Array.Empty<GetReportSidebarItem>();
    }

    public async Task<IReadOnlyList<GetReportDatesItem>> GetReportDatesAsync(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync(BasePath + "GetReportDates", cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode) return Array.Empty<GetReportDatesItem>();
        var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        var list = JsonSerializer.Deserialize<List<GetReportDatesItem>>(json, _jsonOptions);
        return list ?? (IReadOnlyList<GetReportDatesItem>)Array.Empty<GetReportDatesItem>();
    }
}
