using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using DashboardTsy.Web.Models.TargetReport;
using Microsoft.Extensions.Options;

namespace DashboardTsy.Web.Services;

public class TargetReportApiClient : ITargetReportApiClient
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public TargetReportApiClient(HttpClient httpClient, IOptions<DashboardApiOptions> options)
    {
        _httpClient = httpClient;
        var baseUrl = options.Value.BaseUrl?.TrimEnd('/') ?? "https://localhost:5001";
        _httpClient.BaseAddress = new Uri(baseUrl + "/");
    }

    public async Task<GetTargetReportMenuTextsResponse?> GetTargetReportMenuTextsAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        var url = $"TargetReport/GetTargetReportMenuTexts?sessionId={WebUtility.UrlEncode(sessionId)}";
        var response = await _httpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
            return null;
        var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        return JsonSerializer.Deserialize<GetTargetReportMenuTextsResponse>(json, _jsonOptions);
    }

    public async Task<IReadOnlyList<GetTargetReportFiltersItem>> GetTargetReportFiltersAsync(GetTargetReportFiltersRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync("TargetReport/GetTargetReportFilters", request, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
            return Array.Empty<GetTargetReportFiltersItem>();
        var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        var list = JsonSerializer.Deserialize<List<GetTargetReportFiltersItem>>(json, _jsonOptions);
        return list ?? (IReadOnlyList<GetTargetReportFiltersItem>)Array.Empty<GetTargetReportFiltersItem>();
    }

    public async Task<GetDailyTargetReportResponse?> GetDailyTargetReportAsync(GetDailyTargetReportRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync("TargetReport/GetDailyTargetReport", request, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
            return null;
        var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        return JsonSerializer.Deserialize<GetDailyTargetReportResponse>(json, _jsonOptions);
    }

    public async Task<GetDailyQuantityTargetReportTableHeadersResponse?> GetDailyQuantityTargetReportTableHeadersAsync(
    GetDailyQuantityTargetReportTableHeadersRequest request,
    CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync("TargetReport/GetDailyQuantityTargetReportTableHeaders", request, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
            return null;
        var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        return JsonSerializer.Deserialize<GetDailyQuantityTargetReportTableHeadersResponse>(json, _jsonOptions);
    }

    public async Task<GetDailyQuantityTargetReportResponse?> GetDailyQuantityTargetReportAsync(
        GetDailyQuantityTargetReportRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync("TargetReport/GetDailyQuantityTargetReport", request, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
            return null;
        var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        return JsonSerializer.Deserialize<GetDailyQuantityTargetReportResponse>(json, _jsonOptions);
    }

    public async Task<ProductTop10DifferencesResponse?> GetProductTop10DailyAndWeeklyDifferencesAsync(
        GetProductTop10DailyAndWeeklyDifferencesRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync("TargetReport/GetProductTop10DailyAndWeeklyDifferences", request, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
            return null;
        var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        return JsonSerializer.Deserialize<ProductTop10DifferencesResponse>(json, _jsonOptions);
    }

    public async Task<GetDailyTargetReportTableHeadersResponse?> GetDailyTargetReportTableHeadersAsync(
        GetDailyTargetReportTableHeadersRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync("TargetReport/GetDailyTargetReportTableHeaders", request, cancellationToken)
            .ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
            return null;
        var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        return JsonSerializer.Deserialize<GetDailyTargetReportTableHeadersResponse>(json, _jsonOptions);
    }

    public async Task<GetMonthlyTargetReportResponse?> GetMonthlyTargetReportAsync(
        GetMonthlyTargetReportRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync("TargetReport/GetMonthlyTargetReport", request, cancellationToken)
            .ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
            return null;
        var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        return JsonSerializer.Deserialize<GetMonthlyTargetReportResponse>(json, _jsonOptions);
    }

    public async Task<GetMonthlyTargetReportTableHeadersResponse?> GetMonthlyTargetReportTableHeadersAsync(
        GetMonthlyTargetReportTableHeadersRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync("TargetReport/GetMonthlyTargetReportTableHeaders", request, cancellationToken)
            .ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
            return null;
        var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        return JsonSerializer.Deserialize<GetMonthlyTargetReportTableHeadersResponse>(json, _jsonOptions);
    }
}
