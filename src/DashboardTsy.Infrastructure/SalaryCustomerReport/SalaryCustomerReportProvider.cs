using DashboardTsy.Application.SalaryCustomerReport;
using DashboardTsy.Application.SalaryCustomerReport.Requests;
using DashboardTsy.Application.SalaryCustomerReport.Responses;
using DashboardTsy.Infrastructure.Data;
using Microsoft.Extensions.Configuration;

namespace DashboardTsy.Infrastructure.SalaryCustomerReport;

/// <summary>
/// ISalaryCustomerReportProvider implementasyonu: SP ve mock veriyi soyutlar.
/// DataLayer pattern from ReportDataProvider: calls stored procedures via IStoredProcedureExecutor.
/// </summary>
public class SalaryCustomerReportProvider : ISalaryCustomerReportProvider
{
    private readonly IStoredProcedureExecutor _spExecutor;
    private readonly IConfiguration _configuration;

    public SalaryCustomerReportProvider(IStoredProcedureExecutor spExecutor, IConfiguration configuration)
    {
        _spExecutor = spExecutor;
        _configuration = configuration;
    }

    private bool MockEnabled => _configuration["ReportMock:Enabled"] is string v && bool.TryParse(v, out var b) && b;

    public IReadOnlyList<GetSalaryCustomerReportTabItem> GetSalaryCustomerReportTabs(GetSalaryCustomerReportTabsRequest request)
    {
        request ??= new GetSalaryCustomerReportTabsRequest();

        if (MockEnabled)
            return MockSalaryCustomerReportData.GetSalaryCustomerReportTabs(request);

        var parameters = new Dictionary<string, object?>
        {
            ["@SessionId"] = request.SessionId ?? string.Empty
        };

        var ds = _spExecutor.ExecuteDataSet("YoneticiRaporu", "SP_RP_GetSalaryCustomerReportTabs", parameters);
        if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
            return Array.Empty<GetSalaryCustomerReportTabItem>();

        return DataTableHelper.ToList<GetSalaryCustomerReportTabItem>(ds.Tables[0]);
    }

    public GetSalaryCustomerVolumeReportHeadersResponse? GetSalaryCustomerVolumeReportHeaders(GetSalaryCustomerVolumeReportHeadersRequest request)
    {
        request ??= new GetSalaryCustomerVolumeReportHeadersRequest();

        if (MockEnabled)
            return MockSalaryCustomerReportData.GetSalaryCustomerVolumeReportHeaders(request.ReportDate);

        var parameters = new Dictionary<string, object?>
        {
            ["@SessionId"] = request.SessionId ?? string.Empty,
            ["@ReportDate"] = request.ReportDate
        };

        var ds = _spExecutor.ExecuteDataSet("YoneticiRaporu", "SP_RP_GetSalaryCustomerVolumeReportHeaders", parameters);
        if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
            return null;

        return DataTableHelper.ToObject<GetSalaryCustomerVolumeReportHeadersResponse>(ds.Tables[0].Rows[0]);
    }

    public IReadOnlyList<GetSalaryCustomerVolumeReportItem> GetSalaryCustomerVolumeReport(GetSalaryCustomerVolumeReportRequest request)
    {
        request ??= new GetSalaryCustomerVolumeReportRequest();

        if (MockEnabled)
            return MockSalaryCustomerReportData.GetSalaryCustomerVolumeReport(request);

        var parameters = new Dictionary<string, object?>
        {
            ["@SessionId"] = request.SessionId ?? string.Empty,
            ["@RegionCode"] = string.IsNullOrWhiteSpace(request.RegionCode) ? (object)DBNull.Value : request.RegionCode,
            ["@BranchCode"] = string.IsNullOrWhiteSpace(request.BranchCode) ? (object)DBNull.Value : request.BranchCode,
            ["@SubTabId"] = request.SubTabId,
            ["@ReportDate"] = request.ReportDate,
            ["@ShowDifferences"] = request.ShowDifferences,
            ["@SortBy"] = request.SortBy ?? (object)DBNull.Value,
            ["@IsAscending"] = request.IsAscending
        };

        var ds = _spExecutor.ExecuteDataSet("YoneticiRaporu", "SP_RP_GetSalaryCustomerVolumeReport", parameters);
        if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
            return Array.Empty<GetSalaryCustomerVolumeReportItem>();

        var items = DataTableHelper.ToList<GetSalaryCustomerVolumeReportItem>(ds.Tables[0]);

        if (!request.ShowDifferences)
            ClearVolumeReportDifferences(items);

        return items;
    }

    private static void ClearVolumeReportDifferences(IEnumerable<GetSalaryCustomerVolumeReportItem> items)
    {
        foreach (var i in items)
        {
            i.LastYearTotalDifference = null; i.LastYearTotalDifferenceStatus = null;
            i.LastYearSalaryDifference = null; i.LastYearSalaryDifferenceStatus = null;
            i.LastYearRetiredDifference = null; i.LastYearRetiredDifferenceStatus = null;

            i.LastWeekTotalDifference = null; i.LastWeekTotalDifferenceStatus = null;
            i.LastWeekSalaryDifference = null; i.LastWeekSalaryDifferenceStatus = null;
            i.LastWeekRetiredDifference = null; i.LastWeekRetiredDifferenceStatus = null;

            i.PreviousDayTotalDifference = null; i.PreviousDayTotalDifferenceStatus = null;
            i.PreviousDaySalaryDifference = null; i.PreviousDaySalaryDifferenceStatus = null;
            i.PreviousDayRetiredDifference = null; i.PreviousDayRetiredDifferenceStatus = null;
        }
    }

    public GetSalaryCustomerCrossSellReportHeadersResponse? GetSalaryCustomerCrossSellReportHeaders(GetSalaryCustomerCrossSellReportHeadersRequest request)
    {
        request ??= new GetSalaryCustomerCrossSellReportHeadersRequest();

        if (MockEnabled)
            return MockSalaryCustomerReportData.GetSalaryCustomerCrossSellReportHeaders(request.ReportDate);

        var parameters = new Dictionary<string, object?>
        {
            ["@SessionId"] = request.SessionId ?? string.Empty,
            ["@ReportDate"] = request.ReportDate
        };

        var ds = _spExecutor.ExecuteDataSet("YoneticiRaporu", "SP_RP_GetSalaryCustomerCrossSellReportHeaders", parameters);
        if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
            return null;

        return DataTableHelper.ToObject<GetSalaryCustomerCrossSellReportHeadersResponse>(ds.Tables[0].Rows[0]);
    }

    public IReadOnlyList<GetSalaryCustomerCrossSellReportItem> GetSalaryCustomerCrossSellReport(GetSalaryCustomerCrossSellReportRequest request)
    {
        request ??= new GetSalaryCustomerCrossSellReportRequest();

        if (MockEnabled)
            return MockSalaryCustomerReportData.GetSalaryCustomerCrossSellReport(request);

        var parameters = new Dictionary<string, object?>
        {
            ["@SessionId"] = request.SessionId ?? string.Empty,
            ["@RegionCode"] = string.IsNullOrWhiteSpace(request.RegionCode) ? (object)DBNull.Value : request.RegionCode,
            ["@BranchCode"] = string.IsNullOrWhiteSpace(request.BranchCode) ? (object)DBNull.Value : request.BranchCode,
            ["@SubTabId"] = request.SubTabId,
            ["@ReportDate"] = request.ReportDate,
            ["@ShowDifferences"] = request.ShowDifferences,
            ["@SortBy"] = request.SortBy ?? (object)DBNull.Value,
            ["@IsAscending"] = request.IsAscending
        };

        var ds = _spExecutor.ExecuteDataSet("YoneticiRaporu", "SP_RP_GetSalaryCustomerCrossSellReport", parameters);
        if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
            return Array.Empty<GetSalaryCustomerCrossSellReportItem>();

        var items = DataTableHelper.ToList<GetSalaryCustomerCrossSellReportItem>(ds.Tables[0]);

        if (!request.ShowDifferences)
            ClearCrossSellReportDifferences(items);

        return items;
    }

    private static void ClearCrossSellReportDifferences(IEnumerable<GetSalaryCustomerCrossSellReportItem> items)
    {
        foreach (var i in items)
        {
            i.LastYearTotalDifference = null; i.LastYearTotalDifferenceStatus = null;
            i.LastYearSalaryDifference = null; i.LastYearSalaryDifferenceStatus = null;
            i.LastYearRetiredDifference = null; i.LastYearRetiredDifferenceStatus = null;

            i.TwoMonthsAgoTotalDifference = null; i.TwoMonthsAgoTotalDifferenceStatus = null;
            i.TwoMonthsAgoSalaryDifference = null; i.TwoMonthsAgoSalaryDifferenceStatus = null;
            i.TwoMonthsAgoRetiredDifference = null; i.TwoMonthsAgoRetiredDifferenceStatus = null;
        }
    }

    public GetSalaryCustomerBankShareReportHeadersResponse? GetSalaryCustomerBankShareReportHeaders(GetSalaryCustomerBankShareReportHeadersRequest request)
    {
        request ??= new GetSalaryCustomerBankShareReportHeadersRequest();

        if (MockEnabled)
            return MockSalaryCustomerReportData.GetSalaryCustomerBankShareReportHeaders(request.ReportDate);

        var parameters = new Dictionary<string, object?>
        {
            ["@SessionId"] = request.SessionId ?? string.Empty,
            ["@ReportDate"] = request.ReportDate
        };

        var ds = _spExecutor.ExecuteDataSet("YoneticiRaporu", "SP_RP_GetSalaryCustomerBankShareReportHeaders", parameters);
        if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
            return null;

        return DataTableHelper.ToObject<GetSalaryCustomerBankShareReportHeadersResponse>(ds.Tables[0].Rows[0]);
    }

    public IReadOnlyList<GetSalaryCustomerBankShareReportItem> GetSalaryCustomerBankShareReport(GetSalaryCustomerBankShareReportRequest request)
    {
        request ??= new GetSalaryCustomerBankShareReportRequest();

        if (MockEnabled)
            return MockSalaryCustomerReportData.GetSalaryCustomerBankShareReport(request);

        var parameters = new Dictionary<string, object?>
        {
            ["@SessionId"] = request.SessionId ?? string.Empty,
            ["@RegionCode"] = string.IsNullOrWhiteSpace(request.RegionCode) ? (object)DBNull.Value : request.RegionCode,
            ["@BranchCode"] = string.IsNullOrWhiteSpace(request.BranchCode) ? (object)DBNull.Value : request.BranchCode,
            ["@SubTabId"] = request.SubTabId,
            ["@ReportDate"] = request.ReportDate,
            ["@CustomerType"] = request.CustomerType
        };

        var ds = _spExecutor.ExecuteDataSet("YoneticiRaporu", "SP_RP_GetSalaryCustomerBankShareReport", parameters);
        if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
            return Array.Empty<GetSalaryCustomerBankShareReportItem>();

        return DataTableHelper.ToList<GetSalaryCustomerBankShareReportItem>(ds.Tables[0]);
    }
}
