using System.Text.Json;
using DashboardTsy.Web.Models.PosReport.Request;
using DashboardTsy.Web.Models.PosReport.Response;
using Microsoft.Extensions.Options;

namespace DashboardTsy.Web.Services;

public class PosReportApiClient : IPosReportApiClient
{
    private const string BasePath = "PosReport/";

    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public PosReportApiClient(HttpClient httpClient, IOptions<DashboardApiOptions> options)
    {
        _httpClient = httpClient;
        var baseUrl = options.Value.BaseUrl?.TrimEnd('/') ?? "http://localhost:5219";
        _httpClient.BaseAddress = new Uri(baseUrl + "/");
    }

    public Task<IReadOnlyList<GetPosReportItem>> GetPosReportAsync(
        GetPosReportRequest request,
        CancellationToken cancellationToken = default)
        => PostForListAsync<GetPosReportItem>("GetPosReport", request, cancellationToken);

    public Task<IReadOnlyList<GetPosScorecardItem>> GetPosScorecardAsync(
        GetPosScorecardRequest request,
        CancellationToken cancellationToken = default)
        => PostForListAsync<GetPosScorecardItem>("GetPosScorecard", request, cancellationToken);

    private async Task<IReadOnlyList<TItem>> PostForListAsync<TItem>(
        string endpoint,
        object request,
        CancellationToken cancellationToken)
    {
        var response = await _httpClient
            .PostAsJsonAsync(BasePath + endpoint, request, cancellationToken)
            .ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
            return Array.Empty<TItem>();

        var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        var list = JsonSerializer.Deserialize<List<TItem>>(json, _jsonOptions);
        return list ?? (IReadOnlyList<TItem>)Array.Empty<TItem>();
    }
}
