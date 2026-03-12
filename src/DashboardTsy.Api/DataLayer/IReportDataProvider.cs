using System.Data;
using DashboardTsy.Api.Models.TargetReport;

namespace DashboardTsy.Api.DataLayer;

/// <summary>
/// Sample interface for report/data providers that call stored procedures (DBRapor DataLayer pattern).
/// Add methods per SP and return DataSet or typed DTOs via DataTableHelper.ToList&lt;T&gt;.
/// </summary>
public interface IReportDataProvider
{
    DataSet GetRaporTarihi();

    /// <summary>
    /// EXEC [dbo].[SP_RP_GetTargetReportMenuTexts] @SessionId = ''
    /// </summary>
    GetTargetReportMenuTextsResponse? GetTargetReportMenuTexts(string sessionId);

    /// <summary>
    /// EXEC dbo.SP_RP_GetTargetReportFilters @SessionId='', @FilterId=0, @FilterCode=NULL
    /// FilterCode sent as "12,23,45" when list provided.
    /// </summary>
    IReadOnlyList<GetTargetReportFiltersItem> GetTargetReportFilters(string sessionId, int filterId, List<string>? filterCode);

    /// <summary>
    /// EXEC dbo.SP_RP_GetDailyTargetReport ...
    /// </summary>
    GetDailyTargetReportResponse GetDailyTargetReport(GetDailyTargetReportRequest request);

    /// <summary>
    /// EXEC dbo.SP_RP_GetDailyTargetReportTableHeaders @SessionId=''
    /// </summary>
    GetDailyTargetReportTableHeadersResponse? GetDailyTargetReportTableHeaders(string sessionId);

    /// <summary>
    /// EXEC dbo.SP_RP_GetMonthlyTargetReport @SessionId='', @ReportDate=...
    /// </summary>
    GetMonthlyTargetReportResponse? GetMonthlyTargetReport(GetMonthlyTargetReportRequest request);

    /// <summary>
    /// EXEC dbo.SP_RP_GetMonthlyTargetReportTableHeaders @SessionId='', @ReportDate=...
    /// </summary>
    GetMonthlyTargetReportTableHeadersResponse? GetMonthlyTargetReportTableHeaders(GetMonthlyTargetReportTableHeadersRequest request);
}
