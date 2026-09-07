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

    public async Task<IReadOnlyList<GetPosReportItem>> GetPosReportAsync(
        GetPosReportRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient
            .PostAsJsonAsync(BasePath + "GetPosReport", request, cancellationToken)
            .ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
            return Array.Empty<GetPosReportItem>();

        var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        var list = JsonSerializer.Deserialize<List<GetPosReportItem>>(json, _jsonOptions);
        return list ?? (IReadOnlyList<GetPosReportItem>)Array.Empty<GetPosReportItem>();
    }
}
