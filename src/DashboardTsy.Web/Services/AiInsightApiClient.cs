using DashboardTsy.Web.Models.AiInsight.Request;
using DashboardTsy.Web.Models.AiInsight.Response;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace DashboardTsy.Web.Services;

public class AiInsightApiClient : IAiInsightApiClient
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };
    private const string BasePath = "AiInsight/";

    public AiInsightApiClient(HttpClient httpClient, IOptions<DashboardApiOptions> options)
    {
        _httpClient = httpClient;
        var baseUrl = options.Value.BaseUrl?.TrimEnd('/') ?? "http://localhost:5219";
        _httpClient.BaseAddress = new Uri(baseUrl + "/");
    }

    public async Task<GetBranchAiInsightResponse?> GetBranchAiInsightsAsync(GetBranchAiInsightRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync(BasePath + "GetBranchAiInsights", request, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode) return null;
        var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        return JsonSerializer.Deserialize<GetBranchAiInsightResponse>(json, _jsonOptions);
    }
}
