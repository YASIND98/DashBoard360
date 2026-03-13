using System.Data;

namespace DashboardTsy.Application.TargetReport;

/// <summary>
/// TargetReport için application katmanı kontratı.
/// Stored procedure çağrılarını soyutlar.
/// </summary>
public interface IReportDataProvider
{
    DataSet GetRaporTarihi();

    GetTargetReportMenuTextsResponse? GetTargetReportMenuTexts(string sessionId);

    IReadOnlyList<GetTargetReportFiltersItem> GetTargetReportFilters(string sessionId, int filterId, List<string>? filterCode);

    GetDailyTargetReportResponse GetDailyTargetReport(GetDailyTargetReportRequest request);

    GetDailyTargetReportTableHeadersResponse? GetDailyTargetReportTableHeaders(string sessionId);

    GetMonthlyTargetReportResponse? GetMonthlyTargetReport(GetMonthlyTargetReportRequest request);

    GetMonthlyTargetReportTableHeadersResponse? GetMonthlyTargetReportTableHeaders(GetMonthlyTargetReportTableHeadersRequest request);
}

