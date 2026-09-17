using System.Text.Json;
using DashboardTsy.Web.Models.BranchMap.Request;
using DashboardTsy.Web.Models.BranchMap.Response;
using Microsoft.Extensions.Options;

namespace DashboardTsy.Web.Services;

public class BranchMapApiClient : IBranchMapApiClient
{
    private const string BasePath = "BranchMap/";

    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public BranchMapApiClient(HttpClient httpClient, IOptions<DashboardApiOptions> options)
    {
        _httpClient = httpClient;
        var baseUrl = options.Value.BaseUrl?.TrimEnd('/') ?? "http://localhost:5219";
        _httpClient.BaseAddress = new Uri(baseUrl + "/");
    }

    public async Task<IReadOnlyList<GetBranchMapInfoItem>> GetBranchMapInfoAsync(
        GetBranchMapInfoRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient
            .PostAsJsonAsync(BasePath + "GetBranchMapInfo", request, cancellationToken)
            .ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
            return Array.Empty<GetBranchMapInfoItem>();

        var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        var list = JsonSerializer.Deserialize<List<GetBranchMapInfoItem>>(json, _jsonOptions);
        return list ?? (IReadOnlyList<GetBranchMapInfoItem>)Array.Empty<GetBranchMapInfoItem>();
    }
}
