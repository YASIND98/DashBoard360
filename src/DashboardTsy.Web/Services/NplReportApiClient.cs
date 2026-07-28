using System.Text.Json;
using DashboardTsy.Web.Models.NplReport.Request;
using DashboardTsy.Web.Models.NplReport.Response;
using Microsoft.Extensions.Options;

namespace DashboardTsy.Web.Services;

public class NplReportApiClient : INplReportApiClient
{
    private const string BasePath = "NplReport/";

    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public NplReportApiClient(HttpClient httpClient, IOptions<DashboardApiOptions> options)
    {
        _httpClient = httpClient;
        var baseUrl = options.Value.BaseUrl?.TrimEnd('/') ?? "http://localhost:5219";
        _httpClient.BaseAddress = new Uri(baseUrl + "/");
    }

    public async Task<IReadOnlyList<GetNplBalanceRatioItem>> GetNplBalanceRatioAsync(
        GetNplBalanceRatioRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient
            .PostAsJsonAsync(BasePath + "GetNplBalanceRatio", request, cancellationToken)
            .ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
            return Array.Empty<GetNplBalanceRatioItem>();

        var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        var list = JsonSerializer.Deserialize<List<GetNplBalanceRatioItem>>(json, _jsonOptions);
        return list ?? (IReadOnlyList<GetNplBalanceRatioItem>)Array.Empty<GetNplBalanceRatioItem>();
    }
}
