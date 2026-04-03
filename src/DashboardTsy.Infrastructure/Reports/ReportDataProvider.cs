using System.Data;
using DashboardTsy.Application;
using DashboardTsy.Application.TargetReport.Requests;
using DashboardTsy.Application.TargetReport.Responses;
using DashboardTsy.Application.ProductivityReport.Requests;
using DashboardTsy.Application.ProductivityReport.Responses;
using DashboardTsy.Infrastructure.Data;
using DashboardTsy.Infrastructure.TargetReport;
using DashboardTsy.Infrastructure.ProductivityReport;
using Microsoft.Extensions.Configuration;

namespace DashboardTsy.Infrastructure.Reports;

/// <summary>
/// IReportDataProvider implementasyonu: hem TargetReport hem ProductivityReport için SP ve mock veriyi soyutlar.
/// DataLayer pattern from DBRapor: calls stored procedures via IStoredProcedureExecutor.
/// </summary>
public class ReportDataProvider : IReportDataProvider
{
    private readonly IStoredProcedureExecutor _spExecutor;
    private readonly IConfiguration _configuration;

    public ReportDataProvider(IStoredProcedureExecutor spExecutor, IConfiguration configuration)
    {
        _spExecutor = spExecutor;
        _configuration = configuration;
    }

    private bool MockEnabled => _configuration["ReportMock:Enabled"] is string v && bool.TryParse(v, out var b) && b;


    public GetTargetReportMenuTextsResponse? GetTargetReportMenuTexts(string sessionId)
    {
        if (MockEnabled)
            return MockTargetReportData.GetMenuTexts();

        var parameters = new Dictionary<string, object?>
        {
            ["@SessionId"] = sessionId ?? string.Empty
        };
        var ds = _spExecutor.ExecuteDataSet("YoneticiRaporu", "SP_RP_GetTargetReportMenuTexts", parameters);
        if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
            return null;
        return DataTableHelper.ToObject<GetTargetReportMenuTextsResponse>(ds.Tables[0].Rows[0]);
    }

    public IReadOnlyList<GetTargetReportFiltersItem> GetTargetReportFilters(string sessionId, int filterId, List<string>? filterCode)
    {
        if (MockEnabled)
            return MockTargetReportData.GetFilters(filterId);

        var filterCodeStr = filterCode != null && filterCode.Count > 0 ? string.Join(",", filterCode) : null;
        var parameters = new Dictionary<string, object?>
        {
            ["@SessionId"] = sessionId ?? string.Empty,
            ["@FilterId"] = filterId,
            ["@FilterCode"] = filterCodeStr ?? (object)DBNull.Value
        };
        var ds = _spExecutor.ExecuteDataSet("YoneticiRaporu", "SP_RP_GetTargetReportFilters", parameters);
        if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
            return Array.Empty<GetTargetReportFiltersItem>();
        return DataTableHelper.ToList<GetTargetReportFiltersItem>(ds.Tables[0]);
    }

    public GetDailyTargetReportResponse GetDailyTargetReport(GetDailyTargetReportRequest request)
    {
        request ??= new GetDailyTargetReportRequest();

        if (MockEnabled)
            return MockTargetReportData.GetDailyReport(request);

        var parameters = new Dictionary<string, object?>
        {
            ["@SessionId"] = request.SessionId ?? string.Empty,
            ["@TabId"] = request.TabId,
            ["@SubTabId"] = request.SubTabId ?? (object)DBNull.Value,
            ["@ReportDate"] = request.ReportDate,
            ["@RegionId"] = ToCsv(request.RegionId) ?? (object)DBNull.Value,
            ["@BranchId"] = ToCsv(request.BranchId) ?? (object)DBNull.Value,
            ["@PortfolioId"] = ToCsv(request.PortfolioId) ?? (object)DBNull.Value,
            ["@SearchText"] = string.IsNullOrWhiteSpace(request.SearchText) ? (object)DBNull.Value : request.SearchText.Trim(),
            ["@ShowDifferences"] = request.ShowDifferences,
            ["@SortBy"] = request.SortBy ?? (object)DBNull.Value,
            ["@IsAscending"] = request.IsAscending
        };

        var ds = _spExecutor.ExecuteDataSet("YoneticiRaporu", "SP_RP_GetDailyTargetReport", parameters);
        var response = new GetDailyTargetReportResponse();

        if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
            return response;

        var roots = BuildProductTree(ds, request.ReportDate);

        if (!string.IsNullOrWhiteSpace(request.SearchText))
        {
            var q = request.SearchText.Trim();
            roots = FilterTreeByName(roots, q);
        }

        if (!request.ShowDifferences)
            ClearDiffs(roots);

        roots = SortTree(roots, request.SortBy, request.IsAscending);

        response.Products = roots;
        return response;
    }

    public GetDailyQuantityTargetReportResponse GetDailyQuantityTargetReport(GetDailyQuantityTargetReportRequest request)
    {
        request ??= new GetDailyQuantityTargetReportRequest();

        if (MockEnabled)
            return MockTargetReportData.GetDailyQuantityReport(request);

        var parameters = new Dictionary<string, object?>
        {
            ["@SessionId"] = request.SessionId ?? string.Empty,
            ["@TabId"] = request.TabId,
            ["@SubTabId"] = request.SubTabId ?? (object)DBNull.Value,
            ["@ReportDate"] = request.ReportDate,
            ["@RegionId"] = ToCsv(request.RegionId) ?? (object)DBNull.Value,
            ["@BranchId"] = ToCsv(request.BranchId) ?? (object)DBNull.Value,
            ["@PortfolioId"] = ToCsv(request.PortfolioId) ?? (object)DBNull.Value,
            ["@SearchText"] = string.IsNullOrWhiteSpace(request.SearchText) ? (object)DBNull.Value : request.SearchText.Trim(),
            ["@ShowDifferences"] = request.ShowDifferences,
            ["@SortBy"] = request.SortBy ?? (object)DBNull.Value,
            ["@IsAscending"] = request.IsAscending
        };

        var ds = _spExecutor.ExecuteDataSet("YoneticiRaporu", "SP_RP_GetDailyQuantityTargetReport", parameters);
        var response = new GetDailyQuantityTargetReportResponse();

        if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
            return response;

        var roots = BuildDailyQuantityProductTree(ds, request.ReportDate);

        if (!string.IsNullOrWhiteSpace(request.SearchText))
        {
            var q = request.SearchText.Trim();
            roots = FilterDailyQuantityTreeByName(roots, q);
        }

        if (!request.ShowDifferences)
            ClearDailyQuantityDiffs(roots);

        roots = SortDailyQuantityTree(roots, request.SortBy, request.IsAscending);

        response.Products = roots;
        return response;
    }

    public GetDailyQuantityTargetReportTableHeadersResponse? GetDailyQuantityTargetReportTableHeaders(GetDailyQuantityTargetReportTableHeadersRequest request)
    {
        if (MockEnabled)
            return MockTargetReportData.GetDailyQuantityHeaders(DateTime.Today);

        var parameters = new Dictionary<string, object?>
        {
            ["@SessionId"] = request.SessionId ?? string.Empty
        };

        var ds = _spExecutor.ExecuteDataSet("YoneticiRaporu", "SP_RP_GetDailyQuantityTargetReportTableHeaders", parameters);

        if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
            return null;

        return DataTableHelper.ToObject<GetDailyQuantityTargetReportTableHeadersResponse>(ds.Tables[0].Rows[0]);
    }

    public ProductTop10DifferencesResponse GetProductTop10DailyAndWeeklyDifferences(GetProductTop10DailyAndWeeklyDifferencesRequest request)
    {
        request ??= new GetProductTop10DailyAndWeeklyDifferencesRequest();

        if (MockEnabled)
            return MockTargetReportData.GetProductTop10DailyAndWeeklyDifferences(request);

        var parameters = new Dictionary<string, object?>
        {
            ["@ProductId"] = request.ProductId,
            ["@FilterType"] = request.FilterType
        };

        var ds = _spExecutor.ExecuteDataSet("YoneticiRaporu", "SP_RP_GetProductTop10DailyAndWeeklyDifferences", parameters);

        var response = new ProductTop10DifferencesResponse
        {
            First10 = new List<Top10Item>(),
            Last10 = new List<Top10Item>()
        };

        if (ds.Tables.Count == 0)
            return response;

        if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            response.First10 = DataTableHelper.ToList<Top10Item>(ds.Tables[0]);

        if (ds.Tables.Count > 1 && ds.Tables[1].Rows.Count > 0)
            response.Last10 = DataTableHelper.ToList<Top10Item>(ds.Tables[1]);

        return response;
    }

    public GetDailyTargetReportTableHeadersResponse? GetDailyTargetReportTableHeaders(string sessionId)
    {
        if (MockEnabled)
            return MockTargetReportData.GetDailyHeaders(DateTime.Today);

        var parameters = new Dictionary<string, object?>
        {
            ["@SessionId"] = sessionId ?? string.Empty
        };

        var ds = _spExecutor.ExecuteDataSet("YoneticiRaporu", "SP_RP_GetDailyTargetReportTableHeaders", parameters);
        if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
            return null;

        return DataTableHelper.ToObject<GetDailyTargetReportTableHeadersResponse>(ds.Tables[0].Rows[0]);
    }

    public GetMonthlyTargetReportResponse? GetMonthlyTargetReport(GetMonthlyTargetReportRequest request)
    {
        request ??= new GetMonthlyTargetReportRequest();

        if (MockEnabled)
            return MockTargetReportData.GetMonthlyReport(request);

        var parameters = new Dictionary<string, object?>
        {
            ["@SessionId"] = request.SessionId ?? string.Empty,
            ["@TabId"] = request.TabId,
            ["@SubTabId"] = request.SubTabId ?? (object)DBNull.Value,
            ["@ReportDate"] = request.ReportDate,
            ["@RegionId"] = ToCsv(request.RegionId) ?? (object)DBNull.Value,
            ["@BranchId"] = ToCsv(request.BranchId) ?? (object)DBNull.Value,
            ["@PortfolioId"] = ToCsv(request.PortfolioId) ?? (object)DBNull.Value,
            ["@SearchText"] = string.IsNullOrWhiteSpace(request.SearchText) ? (object)DBNull.Value : request.SearchText.Trim(),
            ["@SortBy"] = request.SortBy ?? (object)DBNull.Value,
            ["@IsAscending"] = request.IsAscending
        };

        var ds = _spExecutor.ExecuteDataSet("YoneticiRaporu", "SP_RP_GetMonthlyTargetReport", parameters);
        if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
            return null;

        var response = new GetMonthlyTargetReportResponse();

        var roots = BuildMonthlyProductTree(ds);

        if (!string.IsNullOrWhiteSpace(request.SearchText))
            roots = FilterMonthlyTreeByName(roots, request.SearchText.Trim());

        roots = SortMonthlyTree(roots, request.SortBy, request.IsAscending);

        response.Products = roots;
        return response;
    }

    public GetMonthlyTargetReportTableHeadersResponse? GetMonthlyTargetReportTableHeaders(GetMonthlyTargetReportTableHeadersRequest request)
    {
        request ??= new GetMonthlyTargetReportTableHeadersRequest();

        if (MockEnabled)
            return MockTargetReportData.GetMonthlyHeaders(request.ReportDate);

        var parameters = new Dictionary<string, object?>
        {
            ["@SessionId"] = request.SessionId ?? string.Empty,
            ["@ReportDate"] = request.ReportDate
        };

        var ds = _spExecutor.ExecuteDataSet("YoneticiRaporu", "SP_RP_GetMonthlyTargetReportTableHeaders", parameters);
        if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
            return null;

        return DataTableHelper.ToObject<GetMonthlyTargetReportTableHeadersResponse>(ds.Tables[0].Rows[0]);
    }

    public IReadOnlyList<GetProductivityReportTabItem> GetProductivityReportTabs(GetProductivityReportTabsRequest request)
    {
        request ??= new GetProductivityReportTabsRequest();

        if (MockEnabled)
            return MockProductivityReportData.GetProductivityReportTabs(request);

        var parameters = new Dictionary<string, object?>
        {
            ["@SessionId"] = request.SessionId ?? string.Empty,
            ["@FilterType"] = request.FilterType
        };

        var ds = _spExecutor.ExecuteDataSet("YoneticiRaporu", "GetProductivityReportTabs", parameters);
        if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
            return Array.Empty<GetProductivityReportTabItem>();

        return DataTableHelper.ToList<GetProductivityReportTabItem>(ds.Tables[0]);
    }

    public IReadOnlyList<GetProductivityReportTableHeaderItem> GetProductivityReportTableHeaders(GetProductivityReportTableHeadersRequest request)
    {
        request ??= new GetProductivityReportTableHeadersRequest();

        if (MockEnabled)
            return MockProductivityReportData.GetProductivityReportTableHeaders(request);

        var parameters = new Dictionary<string, object?>
        {
            ["@SessionId"] = request.SessionId ?? string.Empty,
            ["@MainTabId"] = request.MainTabId,
            ["@MidTabId"] = request.MidTabId ?? (object)DBNull.Value,
            ["@SubTabId"] = request.SubTabId ?? (object)DBNull.Value,
            ["@FilterType"] = request.FilterType,
            ["@ReportDate"] = request.ReportDate == default ? DateTime.Today : request.ReportDate
        };

        var ds = _spExecutor.ExecuteDataSet("YoneticiRaporu", "SP_GetProductivityReportTableHeaders", parameters);
        if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
            return Array.Empty<GetProductivityReportTableHeaderItem>();

        return DataTableHelper.ToList<GetProductivityReportTableHeaderItem>(ds.Tables[0]);
    }

    public IReadOnlyList<GetProductivityScoreCardReportHeaderItem> GetProductivityScoreCardReportHeaders(GetProductivityScoreCardReportHeadersRequest request)
    {
        request ??= new GetProductivityScoreCardReportHeadersRequest();

        if (MockEnabled)
            return MockProductivityReportData.GetProductivityScoreCardReportHeaders(request);

        var parameters = new Dictionary<string, object?>
        {
            ["@SessionId"] = request.SessionId ?? string.Empty,
            ["@FilterType"] = request.FilterType,
            ["@ReportDate"] = request.ReportDate == default ? DateTime.Today : request.ReportDate
        };

        var ds = _spExecutor.ExecuteDataSet("YoneticiRaporu", "SP_RP_GetProductivityScoreCardReportHeaders", parameters);
        if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
            return Array.Empty<GetProductivityScoreCardReportHeaderItem>();

        return DataTableHelper.ToList<GetProductivityScoreCardReportHeaderItem>(ds.Tables[0]);
    }

    public IReadOnlyList<GetReportRegionFilterItem> GetReportRegionFilters(GetReportRegionFiltersRequest request)
    {
        request ??= new GetReportRegionFiltersRequest();

        if (MockEnabled)
            return MockProductivityReportData.GetReportRegionFilters();

        const string sql = @"
SELECT DISTINCT BOLGE_KODU, BOLGE
FROM DW_BOLGELER
ORDER BY BOLGE;";

        var ds = _spExecutor.ExecuteQueryDataSet("Referans", sql);
        if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
            return Array.Empty<GetReportRegionFilterItem>();

        return ds.Tables[0].AsEnumerable()
            .Select(r => new GetReportRegionFilterItem
            {
                Code = (r["BOLGE_KODU"]?.ToString() ?? string.Empty).Trim(),
                Name = (r["BOLGE"]?.ToString() ?? string.Empty).Trim()
            })
            .Where(x => !string.IsNullOrWhiteSpace(x.Code))
            .ToList();
    }

    public IReadOnlyList<GetReportBranchFilterItem> GetReportBranchFilters(GetReportBranchFiltersRequest request)
    {
        request ??= new GetReportBranchFiltersRequest();

        if (MockEnabled)
            return MockProductivityReportData.GetReportBranchFilters();

        const string sql = @"
SELECT SUBE_KODU, SUBE_ADI, BOLGE_KODU
FROM DW_BOLGELER
ORDER BY SUBE_ADI;";

        var ds = _spExecutor.ExecuteQueryDataSet("Referans", sql);
        if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
            return Array.Empty<GetReportBranchFilterItem>();

        return ds.Tables[0].AsEnumerable()
            .Select(r => new GetReportBranchFilterItem
            {
                Code = (r["SUBE_KODU"]?.ToString() ?? string.Empty).Trim(),
                Name = (r["SUBE_ADI"]?.ToString() ?? string.Empty).Trim(),
                RegionCode = (r["BOLGE_KODU"]?.ToString() ?? string.Empty).Trim()
            })
            .Where(x => !string.IsNullOrWhiteSpace(x.Code))
            .ToList();
    }

    public IReadOnlyList<GetProductivityGeneralRegionReportItem> GetProductivityGeneralRegionReport(GetProductivityGeneralRegionReportRequest request)
    {
        request ??= new GetProductivityGeneralRegionReportRequest();

        if (MockEnabled)
            return MockProductivityReportData.GetProductivityGeneralRegionReport(request);

        return MockProductivityReportData.GetProductivityGeneralRegionReport(request);
    }

    public IReadOnlyList<GetProductivityCountCardPosRegionReportItem> GetProductivityCountCardPosRegionReport(GetProductivityCountCardPosRegionReportRequest request)
    {
        request ??= new GetProductivityCountCardPosRegionReportRequest();

        if (MockEnabled)
            return MockProductivityReportData.GetProductivityCountCardPosRegionReport(request);

        return MockProductivityReportData.GetProductivityCountCardPosRegionReport(request);
    }

    public IReadOnlyList<GetProductivityCountCardPosRatioRegionReportItem> GetProductivityCountCardPosRatioRegionReport(GetProductivityCountCardPosRatioRegionReportRequest request)
    {
        request ??= new GetProductivityCountCardPosRatioRegionReportRequest();

        if (MockEnabled)
            return MockProductivityReportData.GetProductivityCountCardPosRatioRegionReport(request);

        return MockProductivityReportData.GetProductivityCountCardPosRatioRegionReport(request);
    }

    public GetProductivityCountCardPosRatioRegionReportTableHeadersItem GetProductivityCountCardPosRatioRegionReportTableHeaders(GetProductivityCountCardPosRatioRegionReportTableHeadersRequest request)
    {
        request ??= new GetProductivityCountCardPosRatioRegionReportTableHeadersRequest();

        if (MockEnabled)
            return MockProductivityReportData.GetProductivityCountCardPosRatioRegionReportTableHeaders(request);

        return MockProductivityReportData.GetProductivityCountCardPosRatioRegionReportTableHeaders(request);
    }

    public IReadOnlyList<GetProductivityCountCustomerRegionReportItem> GetProductivityCountCustomerRegionReport(GetProductivityCountCustomerRegionReportRequest request)
    {
        request ??= new GetProductivityCountCustomerRegionReportRequest();

        if (MockEnabled)
            return MockProductivityReportData.GetProductivityCountCustomerRegionReport(request);

        return MockProductivityReportData.GetProductivityCountCustomerRegionReport(request);
    }

    public GetProductivityVolumeRegionReportResponse GetProductivityVolumeRegionReport(GetProductivityVolumeRegionReportRequest request)
    {
        request ??= new GetProductivityVolumeRegionReportRequest();

        if (MockEnabled)
            return MockProductivityReportData.GetProductivityVolumeRegionReport(request);

        var parameters = new Dictionary<string, object?>
        {
            ["@SessionId"] = request.SessionId ?? string.Empty,
            ["@RegionCode"] = request.RegionCode ?? string.Empty,
            ["@SubTabId"] = request.SubTabId,
            ["@ReportDate"] = request.ReportDate,
            ["@SortBy"] = request.SortBy ?? (object)DBNull.Value,
            ["@IsAscending"] = request.IsAscending
        };

        var ds = _spExecutor.ExecuteDataSet(
            "YoneticiRaporu",
            "SP_RP_GetProductivityVolumeRegionReport",
            parameters);

        var response = new GetProductivityVolumeRegionReportResponse();

        if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
            return response;

        var roots = BuildProductivityVolumeRegionReportTree(ds, request.ReportDate);
        roots = SortProductivityVolumeRegionTree(roots, request.SortBy, request.IsAscending);

        response.GetProductivityVolumeRegionReports = roots;
        return response;
    }

    public GetProductivityProfitRatioRegionReportResponse GetProductivityProfitRatioRegionReport(GetProductivityProfitRatioRegionReportRequest request)
    {
        request ??= new GetProductivityProfitRatioRegionReportRequest();

        if (MockEnabled)
            return MockProductivityReportData.GetProductivityProfitRatioRegionReport(request);

        var parameters = new Dictionary<string, object?>
        {
            ["@SessionId"] = request.SessionId ?? string.Empty,
            ["@RegionCode"] = request.RegionCode ?? string.Empty,
            ["@ReportDate"] = request.ReportDate,
            ["@SortBy"] = request.SortBy ?? (object)DBNull.Value,
            ["@IsAscending"] = request.IsAscending
        };

        var ds = _spExecutor.ExecuteDataSet(
            "YoneticiRaporu",
            "SP_RP_GetProductivityProfitRatioRegionReport",
            parameters);

        var response = new GetProductivityProfitRatioRegionReportResponse();

        if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
            return response;

        var roots = BuildProductivityProfitRatioRegionReportTree(ds, request.ReportDate);
        roots = SortProductivityProfitRatioRegionTree(roots, request.SortBy, request.IsAscending);

        response.GetProductivityProfitRatioRegionReports = roots;
        return response;
    }

    public GetProductivityProfitTotalRegionReportResponse GetProductivityProfitTotalRegionReport(GetProductivityProfitTotalRegionReportRequest request)
    {
        request ??= new GetProductivityProfitTotalRegionReportRequest();

        if (MockEnabled)
            return MockProductivityReportData.GetProductivityProfitTotalRegionReport(request);

        var parameters = new Dictionary<string, object?>
        {
            ["@SessionId"] = request.SessionId ?? string.Empty,
            ["@RegionCode"] = request.RegionCode ?? string.Empty,
            ["@ReportDate"] = request.ReportDate,
            ["@SortBy"] = request.SortBy ?? (object)DBNull.Value,
            ["@IsAscending"] = request.IsAscending
        };

        var ds = _spExecutor.ExecuteDataSet(
            "YoneticiRaporu",
            "SP_RP_GetProductivityProfitTotalRegionReport",
            parameters);

        var response = new GetProductivityProfitTotalRegionReportResponse();

        if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
            return response;

        var roots = BuildProductivityProfitTotalRegionReportTree(ds, request.ReportDate);
        roots = SortProductivityProfitTotalRegionTree(roots, request.SortBy, request.IsAscending);

        response.GetProductivityProfitTotalRegionReports = roots;
        return response;
    }

    public GetProductivityProfitSpreadManagementRegionReportResponse GetProductivityProfitSpreadManagementRegionReport(GetProductivityProfitSpreadManagementRegionReportRequest request)
    {
        request ??= new GetProductivityProfitSpreadManagementRegionReportRequest();

        if (MockEnabled)
            return MockProductivityReportData.GetProductivityProfitSpreadManagementRegionReport(request);

        var parameters = new Dictionary<string, object?>
        {
            ["@SessionId"] = request.SessionId ?? string.Empty,
            ["@RegionCode"] = request.RegionCode ?? string.Empty,
            ["@ReportDate"] = request.ReportDate,
            ["@SortBy"] = request.SortBy ?? (object)DBNull.Value,
            ["@IsAscending"] = request.IsAscending
        };

        var ds = _spExecutor.ExecuteDataSet(
            "YoneticiRaporu",
            "SP_RP_GetProductivityProfitSpreadManagementRegionReport",
            parameters);

        var response = new GetProductivityProfitSpreadManagementRegionReportResponse();

        if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
            return response;

        var roots = BuildProductivityProfitSpreadManagementRegionReportTree(ds, request.ReportDate);
        roots = SortProductivityProfitSpreadManagementRegionTree(roots, request.SortBy, request.IsAscending);

        response.GetProductivityProfitSpreadManagementRegionReports = roots;
        return response;
    }

    public IReadOnlyList<GetProductivityProfitSpreadManagementBranchReportItem> GetProductivityProfitSpreadManagementBranchReport(GetProductivityProfitSpreadManagementBranchReportRequest request)
    {
        request ??= new GetProductivityProfitSpreadManagementBranchReportRequest();

        if (MockEnabled)
            return MockProductivityReportData.GetProductivityProfitSpreadManagementBranchReport(request);

        return MockProductivityReportData.GetProductivityProfitSpreadManagementBranchReport(request);
    }

    public IReadOnlyList<GetProductivityCountCardPosBranchReportItem> GetProductivityCountCardPosBranchReport(GetProductivityCountCardPosBranchReportRequest request)
    {
        request ??= new GetProductivityCountCardPosBranchReportRequest();

        if (MockEnabled)
            return MockProductivityReportData.GetProductivityCountCardPosBranchReport(request);

        return MockProductivityReportData.GetProductivityCountCardPosBranchReport(request);
    }

    public IReadOnlyList<GetProductivityCountCardPosRatioBranchReportItem> GetProductivityCountCardPosRatioBranchReport(GetProductivityCountCardPosRatioBranchReportRequest request)
    {
        request ??= new GetProductivityCountCardPosRatioBranchReportRequest();

        if (MockEnabled)
            return MockProductivityReportData.GetProductivityCountCardPosRatioBranchReport(request);

        return MockProductivityReportData.GetProductivityCountCardPosRatioBranchReport(request);
    }

    public GetProductivityCountCardPosRatioBranchReportTableHeadersItem GetProductivityCountCardPosRatioBranchReportTableHeaders(GetProductivityCountCardPosRatioBranchReportTableHeadersRequest request)
    {
        request ??= new GetProductivityCountCardPosRatioBranchReportTableHeadersRequest();

        if (MockEnabled)
            return MockProductivityReportData.GetProductivityCountCardPosRatioBranchReportTableHeaders(request);

        return MockProductivityReportData.GetProductivityCountCardPosRatioBranchReportTableHeaders(request);
    }

    public IReadOnlyList<GetProductivityProfitRatioBranchReportItem> GetProductivityProfitRatioBranchReport(GetProductivityProfitRatioBranchReportRequest request)
    {
        request ??= new GetProductivityProfitRatioBranchReportRequest();

        if (MockEnabled)
            return MockProductivityReportData.GetProductivityProfitRatioBranchReport(request);

        return MockProductivityReportData.GetProductivityProfitRatioBranchReport(request);
    }

    public IReadOnlyList<GetProductivityProfitTotalBranchReportItem> GetProductivityProfitTotalBranchReport(GetProductivityProfitTotalBranchReportRequest request)
    {
        request ??= new GetProductivityProfitTotalBranchReportRequest();

        if (MockEnabled)
            return MockProductivityReportData.GetProductivityProfitTotalBranchReport(request);

        return MockProductivityReportData.GetProductivityProfitTotalBranchReport(request);
    }

    public GetProductivityBranchScoreCardReportItem GetProductivityBranchScoreCardReport(GetProductivityBranchScoreCardReportRequest request)
    {
        request ??= new GetProductivityBranchScoreCardReportRequest();

        if (MockEnabled)
            return MockProductivityReportData.GetProductivityBranchScoreCardReport(request);

        return MockProductivityReportData.GetProductivityBranchScoreCardReport(request);
    }

    public GetProductivityRegionScoreCardReportItem? GetProductivityRegionScoreCardReport(GetProductivityRegionScoreCardReportRequest request)
    {
        request ??= new GetProductivityRegionScoreCardReportRequest();

        if (MockEnabled)
            return MockProductivityReportData.GetProductivityRegionScoreCardReport(request);

        var parameters = new Dictionary<string, object?>
        {
            ["@SessionId"] = request.SessionId ?? string.Empty,
            ["@RegionCode"] = request.RegionCode ?? string.Empty,
            ["@BranchCode"] = string.IsNullOrWhiteSpace(request.BranchCode) ? (object)DBNull.Value : request.BranchCode,
            ["@ReportDate"] = request.ReportDate == default ? DateTime.Today : request.ReportDate
        };

        var ds = _spExecutor.ExecuteDataSet( "YoneticiRaporu", "SP_RP_GetProductivityRegionScoreCardReport", parameters);

        if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
            return new GetProductivityRegionScoreCardReportItem();

        return DataTableHelper.ToObject<GetProductivityRegionScoreCardReportItem>(ds.Tables[0].Rows[0]);
    }

    public IReadOnlyList<GetProductivityCountCustomerBranchReportItem> GetProductivityCountCustomerBranchReport(GetProductivityCountCustomerBranchReportRequest request)
    {
        request ??= new GetProductivityCountCustomerBranchReportRequest();

        if (MockEnabled)
            return MockProductivityReportData.GetProductivityCountCustomerBranchReport(request);

        return MockProductivityReportData.GetProductivityCountCustomerBranchReport(request);
    }

    public IReadOnlyList<GetProductivityVolumeBranchReportItem> GetProductivityVolumeBranchReport(GetProductivityVolumeBranchReportRequest request)
    {
        request ??= new GetProductivityVolumeBranchReportRequest();

        if (MockEnabled)
            return MockProductivityReportData.GetProductivityVolumeBranchReport(request);

        return MockProductivityReportData.GetProductivityVolumeBranchReport(request);
    }

    #region Helpers

    private static string? ToCsv(List<int>? list)
        => list != null && list.Count > 0 ? string.Join(",", list) : null;

    private sealed class MonthlyTargetRow
    {
        public long ProductId { get; set; }
        public long? ParentProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;

        public double MonthActualAmount { get; set; }
        public double MonthTargetAmount { get; set; }
        public double MonthRatio { get; set; }

        public double YearActualAmount { get; set; }
        public double YearTargetAmount { get; set; }
        public double YearRatio { get; set; }
    }

    private static List<GetMonthlyTargetReportResponse.Product> BuildMonthlyProductTree(DataSet ds)
    {
        var t0 = ds.Tables[0];
        var hasParentInT0 = t0.Columns.Contains("ParentProductId");

        if (hasParentInT0)
        {
            var rows = DataTableHelper.ToList<MonthlyTargetRow>(t0);
            return MonthlyTreeFromRows(rows);
        }

        if (ds.Tables.Count > 1 && ds.Tables[1].Columns.Contains("ParentProductId"))
        {
            var roots = DataTableHelper.ToList<MonthlyTargetRow>(ds.Tables[0]);
            var children = DataTableHelper.ToList<MonthlyTargetRow>(ds.Tables[1]);
            var all = new List<MonthlyTargetRow>(roots.Count + children.Count);
            all.AddRange(roots);
            all.AddRange(children);
            return MonthlyTreeFromRows(all);
        }

        var flat = DataTableHelper.ToList<MonthlyTargetRow>(t0);
        return flat.Select(MapMonthlyProduct).ToList();
    }

    private static List<GetMonthlyTargetReportResponse.Product> MonthlyTreeFromRows(List<MonthlyTargetRow> rows)
    {
        var byId = rows
            .GroupBy(r => r.ProductId)
            .ToDictionary(g => g.Key, g => MapMonthlyProduct(g.First()));

        foreach (var r in rows)
        {
            if (!byId.TryGetValue(r.ProductId, out var node))
            {
                node = MapMonthlyProduct(r);
                byId[r.ProductId] = node;
            }

            var parentId = r.ParentProductId;
            if (parentId.HasValue && parentId.Value != 0 && byId.TryGetValue(parentId.Value, out var parent))
                parent.SubProducts.Add(node);
        }

        var rootIds = rows
            .Where(r => !r.ParentProductId.HasValue || r.ParentProductId.Value == 0 || !byId.ContainsKey(r.ParentProductId.Value))
            .Select(r => r.ProductId)
            .Distinct()
            .ToList();

        return rootIds.Select(id => byId[id]).ToList();
    }

    private static GetMonthlyTargetReportResponse.Product MapMonthlyProduct(MonthlyTargetRow r)
    {
        return new GetMonthlyTargetReportResponse.Product
        {
            ProductId = r.ProductId,
            ProductName = r.ProductName ?? string.Empty,
            ParentProductId = r.ParentProductId,
            MonthActualAmount = r.MonthActualAmount,
            MonthTargetAmount = r.MonthTargetAmount,
            MonthRatio = r.MonthRatio,
            YearActualAmount = r.YearActualAmount,
            YearTargetAmount = r.YearTargetAmount,
            YearRatio = r.YearRatio,
            SubProducts = new List<GetMonthlyTargetReportResponse.Product>()
        };
    }

    private sealed class DailyTargetRow
    {
        public long ProductId { get; set; }
        public long? ParentProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;

        public double LastYearAmount { get; set; }
        public DateTime LastYearDate { get; set; }

        public double LastWeekAmount { get; set; }
        public DateTime LastWeekDate { get; set; }

        public double PrevDayAmount { get; set; }
        public DateTime PrevDayDate { get; set; }

        public double YesterdayAmount { get; set; }
        public DateTime YesterdayDate { get; set; }

        public DateTime TodayDate { get; set; }

        public double? DiffByPrevDayAmount { get; set; }
        public double? DiffByLastYearAmount { get; set; }
        public double? DiffByLastWeekAmount { get; set; }
    }

    private static List<GetDailyTargetReportResponse.Product> BuildProductTree(DataSet ds, DateTime reportDate)
    {
        var t0 = ds.Tables[0];
        var hasParentInT0 = t0.Columns.Contains("ParentProductId");

        if (hasParentInT0)
        {
            var rows = DataTableHelper.ToList<DailyTargetRow>(t0);
            foreach (var r in rows)
                if (r.TodayDate == default) r.TodayDate = reportDate;
            return TreeFromRows(rows);
        }

        if (ds.Tables.Count > 1 && ds.Tables[1].Columns.Contains("ParentProductId"))
        {
            var roots = DataTableHelper.ToList<DailyTargetRow>(ds.Tables[0]);
            var children = DataTableHelper.ToList<DailyTargetRow>(ds.Tables[1]);
            foreach (var r in roots.Concat(children))
                if (r.TodayDate == default) r.TodayDate = reportDate;

            var all = new List<DailyTargetRow>(roots.Count + children.Count);
            all.AddRange(roots);
            all.AddRange(children);
            return TreeFromRows(all);
        }

        var flat = DataTableHelper.ToList<DailyTargetRow>(t0);
        foreach (var r in flat)
            if (r.TodayDate == default) r.TodayDate = reportDate;
        return flat.Select(MapProduct).ToList();
    }

    private static List<GetDailyTargetReportResponse.Product> TreeFromRows(List<DailyTargetRow> rows)
    {
        var byId = rows
            .GroupBy(r => r.ProductId)
            .ToDictionary(g => g.Key, g => MapProduct(g.First()));

        foreach (var r in rows)
        {
            if (!byId.TryGetValue(r.ProductId, out var node))
            {
                node = MapProduct(r);
                byId[r.ProductId] = node;
            }

            var parentId = r.ParentProductId;
            if (parentId.HasValue && parentId.Value != 0 && byId.TryGetValue(parentId.Value, out var parent))
                parent.SubProducts.Add(node);
        }

        var rootIds = rows
            .Where(r => !r.ParentProductId.HasValue || r.ParentProductId.Value == 0 || !byId.ContainsKey(r.ParentProductId.Value))
            .Select(r => r.ProductId)
            .Distinct()
            .ToList();

        return rootIds.Select(id => byId[id]).ToList();
    }

    private static GetDailyTargetReportResponse.Product MapProduct(DailyTargetRow r)
    {
        return new GetDailyTargetReportResponse.Product
        {
            ProductId = r.ProductId,
            ProductName = r.ProductName ?? string.Empty,
            ParentProductId = r.ParentProductId,
            LastYearAmount = r.LastYearAmount,
            LastYearDate = r.LastYearDate,
            LastWeekAmount = r.LastWeekAmount,
            LastWeekDate = r.LastWeekDate,
            PrevDayAmount = r.PrevDayAmount,
            PrevDayDate = r.PrevDayDate,
            YesterdayAmount = r.YesterdayAmount,
            YesterdayDate = r.YesterdayDate,
            TodayDate = r.TodayDate,
            DiffByPrevDayAmount = r.DiffByPrevDayAmount,
            DiffByLastYearAmount = r.DiffByLastYearAmount,
            DiffByLastWeekAmount = r.DiffByLastWeekAmount,
            SubProducts = new List<GetDailyTargetReportResponse.Product>()
        };
    }

    private static List<GetDailyTargetReportResponse.Product> FilterTreeByName(List<GetDailyTargetReportResponse.Product> nodes, string searchText)
    {
        bool Matches(GetDailyTargetReportResponse.Product p)
            => (p.ProductName ?? string.Empty).Contains(searchText, StringComparison.OrdinalIgnoreCase);

        List<GetDailyTargetReportResponse.Product> Recurse(IEnumerable<GetDailyTargetReportResponse.Product> list)
        {
            var result = new List<GetDailyTargetReportResponse.Product>();
            foreach (var n in list)
            {
                var filteredChildren = Recurse(n.SubProducts ?? new List<GetDailyTargetReportResponse.Product>());
                if (Matches(n) || filteredChildren.Count > 0)
                {
                    n.SubProducts = filteredChildren;
                    result.Add(n);
                }
            }

            return result;
        }

        return Recurse(nodes);
    }

    private static void ClearDiffs(IEnumerable<GetDailyTargetReportResponse.Product> nodes)
    {
        foreach (var n in nodes)
        {
            n.DiffByPrevDayAmount = null;
            n.DiffByLastYearAmount = null;
            n.DiffByLastWeekAmount = null;
            if (n.SubProducts != null && n.SubProducts.Count > 0)
                ClearDiffs(n.SubProducts);
        }
    }

    private static List<GetDailyTargetReportResponse.Product> SortTree(List<GetDailyTargetReportResponse.Product> nodes, int? sortBy, bool isAscending)
    {
        Func<GetDailyTargetReportResponse.Product, object> keySelector = sortBy switch
        {
            1 => p => p.ProductName ?? string.Empty,
            2 => p => p.LastYearAmount,
            3 => p => p.LastWeekAmount,
            4 => p => p.PrevDayAmount,
            5 => p => p.YesterdayAmount,
            _ => p => p.ProductId
        };

        var ordered = (isAscending ? nodes.OrderBy(keySelector) : nodes.OrderByDescending(keySelector)).ToList();

        foreach (var n in ordered)
        {
            if (n.SubProducts != null && n.SubProducts.Count > 0)
                n.SubProducts = SortTree(n.SubProducts, sortBy, isAscending);
        }

        return ordered;
    }

    private static List<GetMonthlyTargetReportResponse.Product> FilterMonthlyTreeByName(List<GetMonthlyTargetReportResponse.Product> nodes, string searchText)
    {
        bool Matches(GetMonthlyTargetReportResponse.Product p)
            => (p.ProductName ?? string.Empty).Contains(searchText, StringComparison.OrdinalIgnoreCase);

        List<GetMonthlyTargetReportResponse.Product> Recurse(IEnumerable<GetMonthlyTargetReportResponse.Product> list)
        {
            var result = new List<GetMonthlyTargetReportResponse.Product>();
            foreach (var n in list)
            {
                var filteredChildren = Recurse(n.SubProducts ?? new List<GetMonthlyTargetReportResponse.Product>());
                if (Matches(n) || filteredChildren.Count > 0)
                {
                    n.SubProducts = filteredChildren;
                    result.Add(n);
                }
            }
            return result;
        }

        return Recurse(nodes);
    }

    private static List<GetMonthlyTargetReportResponse.Product> SortMonthlyTree(List<GetMonthlyTargetReportResponse.Product> nodes, int? sortBy, bool isAscending)
    {
        Func<GetMonthlyTargetReportResponse.Product, object> keySelector = sortBy switch
        {
            1 => p => p.ProductName ?? string.Empty,
            2 => p => p.MonthActualAmount,
            3 => p => p.MonthTargetAmount,
            4 => p => p.MonthRatio,
            5 => p => p.YearActualAmount,
            6 => p => p.YearTargetAmount,
            7 => p => p.YearRatio,
            _ => p => p.ProductId
        };

        var ordered = (isAscending ? nodes.OrderBy(keySelector) : nodes.OrderByDescending(keySelector)).ToList();

        foreach (var n in ordered)
        {
            if (n.SubProducts != null && n.SubProducts.Count > 0)
                n.SubProducts = SortMonthlyTree(n.SubProducts, sortBy, isAscending);
        }

        return ordered;
    }

    private sealed class DailyQuantityTargetRow
    {
        public long ProductId { get; set; }
        public long? ParentProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;

        public double LastYearAmount { get; set; }
        public DateTime LastYearDate { get; set; }

        public double LastMonthAmount { get; set; }
        public DateTime LastMonthDate { get; set; }

        public double LastTwoMonthEarlierAmount { get; set; }
        public DateTime LastTwoMonthEarlierDate { get; set; }

        public DateTime TodayDate { get; set; }

        public double? DiffByLastTwoMonthEarlierAmount { get; set; }
        public double? DiffByLastYearAmount { get; set; }
        public double? DiffByLastMonthAmount { get; set; }
    }

    private static List<GetDailyQuantityTargetReportResponse.Product> BuildDailyQuantityProductTree(DataSet ds, DateTime reportDate)
    {
        var t0 = ds.Tables[0];
        var hasParentInT0 = t0.Columns.Contains("ParentProductId");

        if (hasParentInT0)
        {
            var rows = DataTableHelper.ToList<DailyQuantityTargetRow>(t0);
            foreach (var r in rows)
                if (r.TodayDate == default) r.TodayDate = reportDate;
            return DailyQuantityTreeFromRows(rows);
        }

        if (ds.Tables.Count > 1 && ds.Tables[1].Columns.Contains("ParentProductId"))
        {
            var roots = DataTableHelper.ToList<DailyQuantityTargetRow>(ds.Tables[0]);
            var children = DataTableHelper.ToList<DailyQuantityTargetRow>(ds.Tables[1]);

            foreach (var r in roots.Concat(children))
                if (r.TodayDate == default) r.TodayDate = reportDate;

            var all = new List<DailyQuantityTargetRow>(roots.Count + children.Count);
            all.AddRange(roots);
            all.AddRange(children);

            return DailyQuantityTreeFromRows(all);
        }

        var flat = DataTableHelper.ToList<DailyQuantityTargetRow>(t0);
        foreach (var r in flat)
            if (r.TodayDate == default) r.TodayDate = reportDate;
        return flat.Select(MapDailyQuantityProduct).ToList();
    }

    private static List<GetDailyQuantityTargetReportResponse.Product> DailyQuantityTreeFromRows(List<DailyQuantityTargetRow> rows)
    {
        var byId = rows
            .GroupBy(r => r.ProductId)
            .ToDictionary(g => g.Key, g => MapDailyQuantityProduct(g.First()));

        foreach (var r in rows)
        {
            if (!byId.TryGetValue(r.ProductId, out var node))
            {
                node = MapDailyQuantityProduct(r);
                byId[r.ProductId] = node;
            }

            var parentId = r.ParentProductId;
            if (parentId.HasValue && parentId.Value != 0 && byId.TryGetValue(parentId.Value, out var parent))
                parent.SubProducts.Add(node);
        }

        var rootIds = rows
            .Where(r => !r.ParentProductId.HasValue || r.ParentProductId.Value == 0 || !byId.ContainsKey(r.ParentProductId.Value))
            .Select(r => r.ProductId)
            .Distinct()
            .ToList();

        return rootIds.Select(id => byId[id]).ToList();
    }

    private static GetDailyQuantityTargetReportResponse.Product MapDailyQuantityProduct(DailyQuantityTargetRow r)
    {
        return new GetDailyQuantityTargetReportResponse.Product
        {
            ProductId = r.ProductId,
            ProductName = r.ProductName ?? string.Empty,
            ParentProductId = r.ParentProductId,
            LastYearAmount = r.LastYearAmount,
            LastYearDate = r.LastYearDate,
            LastMonthAmount = r.LastMonthAmount,
            LastMonthDate = r.LastMonthDate,
            LastTwoMonthEarlierAmount = r.LastTwoMonthEarlierAmount,
            LastTwoMonthEarlierDate = r.LastTwoMonthEarlierDate,
            TodayDate = r.TodayDate,
            DiffByLastTwoMonthEarlierAmount = r.DiffByLastTwoMonthEarlierAmount,
            DiffByLastYearAmount = r.DiffByLastYearAmount,
            DiffByLastMonthAmount = r.DiffByLastMonthAmount,
            SubProducts = new List<GetDailyQuantityTargetReportResponse.Product>()
        };
    }

    private static List<GetDailyQuantityTargetReportResponse.Product> FilterDailyQuantityTreeByName(List<GetDailyQuantityTargetReportResponse.Product> nodes, string searchText)
    {
        bool Matches(GetDailyQuantityTargetReportResponse.Product p)
            => (p.ProductName ?? string.Empty).Contains(searchText, StringComparison.OrdinalIgnoreCase);

        List<GetDailyQuantityTargetReportResponse.Product> Recurse(IEnumerable<GetDailyQuantityTargetReportResponse.Product> list)
        {
            var result = new List<GetDailyQuantityTargetReportResponse.Product>();
            foreach (var n in list)
            {
                var filteredChildren = Recurse(n.SubProducts ?? new List<GetDailyQuantityTargetReportResponse.Product>());
                if (Matches(n) || filteredChildren.Count > 0)
                {
                    n.SubProducts = filteredChildren;
                    result.Add(n);
                }
            }

            return result;
        }

        return Recurse(nodes);
    }

    private static void ClearDailyQuantityDiffs(IEnumerable<GetDailyQuantityTargetReportResponse.Product> nodes)
    {
        foreach (var n in nodes)
        {
            n.DiffByLastTwoMonthEarlierAmount = null;
            n.DiffByLastYearAmount = null;
            n.DiffByLastMonthAmount = null;
            if (n.SubProducts != null && n.SubProducts.Count > 0)
                ClearDailyQuantityDiffs(n.SubProducts);
        }
    }

    private static List<GetDailyQuantityTargetReportResponse.Product> SortDailyQuantityTree(List<GetDailyQuantityTargetReportResponse.Product> nodes, int? sortBy, bool isAscending)
    {
        Func<GetDailyQuantityTargetReportResponse.Product, object> keySelector = sortBy switch
        {
            1 => p => p.ProductName ?? string.Empty,
            2 => p => p.LastYearAmount,
            3 => p => p.LastMonthAmount,
            4 => p => p.LastTwoMonthEarlierAmount,
            _ => p => p.ProductId
        };

        var ordered = (isAscending ? nodes.OrderBy(keySelector) : nodes.OrderByDescending(keySelector)).ToList();

        foreach (var n in ordered)
        {
            if (n.SubProducts != null && n.SubProducts.Count > 0)
                n.SubProducts = SortDailyQuantityTree(n.SubProducts, sortBy, isAscending);
        }

        return ordered;
    }

    private sealed class ProductivityProfitSpreadManagementRegionRow
    {
        public int Id { get; set; }
        public int? ParentProductId { get; set; }

        public string Description { get; set; } = string.Empty;

        public decimal SpreadValue { get; set; }

        public decimal RatioRegionValue { get; set; }
        public decimal? RatioRegionValueDiff { get; set; }
        public decimal RatioBankAverageValue { get; set; }
        public decimal? RatioBankAverageValueDiff { get; set; }

        public decimal NetReturnRegionValue { get; set; }
        public decimal? NetReturnRegionValueDiff { get; set; }
        public decimal NetReturnBankAverageValue { get; set; }
        public decimal? NetReturnBankAverageValueDiff { get; set; }

        public decimal NetReturnHgRegionValue { get; set; }
        public decimal? NetReturnHgRegionValueDiff { get; set; }
        public decimal NetReturnHgBankAverageValue { get; set; }
        public decimal? NetReturnHgBankAverageValueDiff { get; set; }
    }

    private static List<GetProductivityProfitSpreadManagementRegionReportResponse.GetProductivityProfitSpreadManagementRegionReportItem>
        BuildProductivityProfitSpreadManagementRegionReportTree(DataSet ds, DateTime reportDate)
    {
        var t0 = ds.Tables[0];
        var hasParentInT0 = t0.Columns.Contains("ParentProductId");

        if (hasParentInT0)
        {
            var rows = DataTableHelper.ToList<ProductivityProfitSpreadManagementRegionRow>(t0);
            return ProductivityProfitSpreadManagementRegionReportTreeFromRows(rows);
        }

        if (ds.Tables.Count > 1 && ds.Tables[1].Columns.Contains("ParentProductId"))
        {
            var roots = DataTableHelper.ToList<ProductivityProfitSpreadManagementRegionRow>(ds.Tables[0]);
            var children = DataTableHelper.ToList<ProductivityProfitSpreadManagementRegionRow>(ds.Tables[1]);

            var all = new List<ProductivityProfitSpreadManagementRegionRow>(roots.Count + children.Count);
            all.AddRange(roots);
            all.AddRange(children);

            return ProductivityProfitSpreadManagementRegionReportTreeFromRows(all);
        }

        var flat = DataTableHelper.ToList<ProductivityProfitSpreadManagementRegionRow>(t0);
        return flat.Select(MapProductivityProfitSpreadManagementRegionReportItem).ToList();
    }

    private static List<GetProductivityProfitSpreadManagementRegionReportResponse.GetProductivityProfitSpreadManagementRegionReportItem>
        ProductivityProfitSpreadManagementRegionReportTreeFromRows(List<ProductivityProfitSpreadManagementRegionRow> rows)
    {
        var byId = rows
            .GroupBy(r => r.Id)
            .ToDictionary(g => g.Key, g => MapProductivityProfitSpreadManagementRegionReportItem(g.First()));

        foreach (var r in rows)
        {
            if (!byId.TryGetValue(r.Id, out var node))
            {
                node = MapProductivityProfitSpreadManagementRegionReportItem(r);
                byId[r.Id] = node;
            }

            var parentId = r.ParentProductId;
            if (parentId.HasValue && parentId.Value != 0 && byId.TryGetValue(parentId.Value, out var parent))
                parent.SubProducts.Add(node);
        }

        var rootIds = rows
            .Where(r => !r.ParentProductId.HasValue || r.ParentProductId.Value == 0 || !byId.ContainsKey(r.ParentProductId.Value))
            .Select(r => r.Id)
            .Distinct()
            .ToList();

        return rootIds.Select(id => byId[id]).ToList();
    }

    private static GetProductivityProfitSpreadManagementRegionReportResponse.GetProductivityProfitSpreadManagementRegionReportItem
        MapProductivityProfitSpreadManagementRegionReportItem(ProductivityProfitSpreadManagementRegionRow r)
    {
        return new GetProductivityProfitSpreadManagementRegionReportResponse.GetProductivityProfitSpreadManagementRegionReportItem
        {
            Id = r.Id,
            Description = r.Description ?? string.Empty,
            SpreadValue = r.SpreadValue,
            RatioRegionValue = r.RatioRegionValue,
            RatioRegionValueDiff = r.RatioRegionValueDiff,
            RatioBankAverageValue = r.RatioBankAverageValue,
            RatioBankAverageValueDiff = r.RatioBankAverageValueDiff,
            NetReturnRegionValue = r.NetReturnRegionValue,
            NetReturnRegionValueDiff = r.NetReturnRegionValueDiff,
            NetReturnBankAverageValue = r.NetReturnBankAverageValue,
            NetReturnBankAverageValueDiff = r.NetReturnBankAverageValueDiff,
            NetReturnHgRegionValue = r.NetReturnHgRegionValue,
            NetReturnHgRegionValueDiff = r.NetReturnHgRegionValueDiff,
            NetReturnHgBankAverageValue = r.NetReturnHgBankAverageValue,
            NetReturnHgBankAverageValueDiff = r.NetReturnHgBankAverageValueDiff,
            SubProducts = new List<GetProductivityProfitSpreadManagementRegionReportResponse.GetProductivityProfitSpreadManagementRegionReportItem>()
        };
    }

    private static List<GetProductivityProfitSpreadManagementRegionReportResponse.GetProductivityProfitSpreadManagementRegionReportItem>
        SortProductivityProfitSpreadManagementRegionTree(
            List<GetProductivityProfitSpreadManagementRegionReportResponse.GetProductivityProfitSpreadManagementRegionReportItem> nodes,
            int? sortBy,
            bool isAscending)
    {
        Func<GetProductivityProfitSpreadManagementRegionReportResponse.GetProductivityProfitSpreadManagementRegionReportItem, object> keySelector = sortBy switch
        {
            1 => p => p.Description ?? string.Empty,
            2 => p => p.SpreadValue,
            3 => p => p.RatioRegionValue,
            4 => p => p.RatioBankAverageValue,
            5 => p => p.NetReturnRegionValue,
            6 => p => p.NetReturnBankAverageValue,
            7 => p => p.NetReturnHgRegionValue,
            8 => p => p.NetReturnHgBankAverageValue,
            _ => p => p.Id
        };

        var ordered = (isAscending ? nodes.OrderBy(keySelector) : nodes.OrderByDescending(keySelector)).ToList();

        foreach (var n in ordered)
        {
            if (n.SubProducts != null && n.SubProducts.Count > 0)
                n.SubProducts = SortProductivityProfitSpreadManagementRegionTree(n.SubProducts, sortBy, isAscending);
        }

        return ordered;
    }

    private sealed class ProductivityProfitTotalRegionRow
    {
        public int Id { get; set; }
        public int? ParentProductId { get; set; }

        public string Description { get; set; } = string.Empty;

        public decimal TargetValue { get; set; }

        public decimal RealizationRegionValue { get; set; }
        public decimal? RealizationRegionValueDiff { get; set; }
        public decimal RealizationBankAverageValue { get; set; }
        public decimal? RealizationBankAverageValueDiff { get; set; }

        public decimal HgRegionValue { get; set; }
        public decimal? HgRegionValueDiff { get; set; }
        public decimal HgBankAverageValue { get; set; }
        public decimal? HgBankAverageValueDiff { get; set; }

        public decimal RetailValue { get; set; }
        public decimal KobiValue { get; set; }
        public decimal AgricultureValue { get; set; }
        public decimal CommercialValue { get; set; }
        public decimal? CommercialValueDiff { get; set; }
        public decimal PartnerValue { get; set; }
    }

    private static List<GetProductivityProfitTotalRegionReportResponse.GetProductivityProfitTotalRegionReportItem>
    BuildProductivityProfitTotalRegionReportTree(DataSet ds, DateTime reportDate)
    {
        var t0 = ds.Tables[0];
        var hasParentInT0 = t0.Columns.Contains("ParentProductId");

        if (hasParentInT0)
        {
            var rows = DataTableHelper.ToList<ProductivityProfitTotalRegionRow>(t0);
            return ProductivityProfitTotalRegionReportTreeFromRows(rows);
        }

        if (ds.Tables.Count > 1 && ds.Tables[1].Columns.Contains("ParentProductId"))
        {
            var roots = DataTableHelper.ToList<ProductivityProfitTotalRegionRow>(ds.Tables[0]);
            var children = DataTableHelper.ToList<ProductivityProfitTotalRegionRow>(ds.Tables[1]);

            var all = new List<ProductivityProfitTotalRegionRow>(roots.Count + children.Count);
            all.AddRange(roots);
            all.AddRange(children);

            return ProductivityProfitTotalRegionReportTreeFromRows(all);
        }

        var flat = DataTableHelper.ToList<ProductivityProfitTotalRegionRow>(t0);
        return flat.Select(MapProductivityProfitTotalRegionReportItem).ToList();
    }

    private static List<GetProductivityProfitTotalRegionReportResponse.GetProductivityProfitTotalRegionReportItem>
    ProductivityProfitTotalRegionReportTreeFromRows(List<ProductivityProfitTotalRegionRow> rows)
    {
        var byId = rows
            .GroupBy(r => r.Id)
            .ToDictionary(g => g.Key, g => MapProductivityProfitTotalRegionReportItem(g.First()));

        foreach (var r in rows)
        {
            if (!byId.TryGetValue(r.Id, out var node))
            {
                node = MapProductivityProfitTotalRegionReportItem(r);
                byId[r.Id] = node;
            }

            var parentId = r.ParentProductId;
            if (parentId.HasValue && parentId.Value != 0 && byId.TryGetValue(parentId.Value, out var parent))
                parent.SubProducts.Add(node);
        }

        var rootIds = rows
            .Where(r => !r.ParentProductId.HasValue || r.ParentProductId.Value == 0 || !byId.ContainsKey(r.ParentProductId.Value))
            .Select(r => r.Id)
            .Distinct()
            .ToList();

        return rootIds.Select(id => byId[id]).ToList();
    }

    private static GetProductivityProfitTotalRegionReportResponse.GetProductivityProfitTotalRegionReportItem
    MapProductivityProfitTotalRegionReportItem(ProductivityProfitTotalRegionRow r)
    {
        return new GetProductivityProfitTotalRegionReportResponse.GetProductivityProfitTotalRegionReportItem
        {
            Id = r.Id,
            Description = r.Description ?? string.Empty,
            TargetValue = r.TargetValue,
            RealizationRegionValue = r.RealizationRegionValue,
            RealizationRegionValueDiff = r.RealizationRegionValueDiff,
            RealizationBankAverageValue = r.RealizationBankAverageValue,
            RealizationBankAverageValueDiff = r.RealizationBankAverageValueDiff,
            HgRegionValue = r.HgRegionValue,
            HgRegionValueDiff = r.HgRegionValueDiff,
            HgBankAverageValue = r.HgBankAverageValue,
            HgBankAverageValueDiff = r.HgBankAverageValueDiff,
            RetailValue = r.RetailValue,
            KobiValue = r.KobiValue,
            AgricultureValue = r.AgricultureValue,
            CommercialValue = r.CommercialValue,
            CommercialValueDiff = r.CommercialValueDiff,
            PartnerValue = r.PartnerValue,
            SubProducts = new List<GetProductivityProfitTotalRegionReportResponse.GetProductivityProfitTotalRegionReportItem>()
        };
    }

    private static List<GetProductivityProfitTotalRegionReportResponse.GetProductivityProfitTotalRegionReportItem>
    SortProductivityProfitTotalRegionTree(
        List<GetProductivityProfitTotalRegionReportResponse.GetProductivityProfitTotalRegionReportItem> nodes,
        int? sortBy,
        bool isAscending)
    {
        Func<GetProductivityProfitTotalRegionReportResponse.GetProductivityProfitTotalRegionReportItem, object> keySelector = sortBy switch
        {
            1 => p => p.Description ?? string.Empty,
            2 => p => p.TargetValue,
            3 => p => p.RealizationRegionValue,
            4 => p => p.RealizationBankAverageValue,
            5 => p => p.HgRegionValue,
            6 => p => p.HgBankAverageValue,
            7 => p => p.RetailValue,
            8 => p => p.KobiValue,
            9 => p => p.AgricultureValue,
            10 => p => p.CommercialValue,
            11 => p => p.PartnerValue,
            _ => p => p.Id
        };

        var ordered = (isAscending ? nodes.OrderBy(keySelector) : nodes.OrderByDescending(keySelector)).ToList();

        foreach (var n in ordered)
        {
            if (n.SubProducts != null && n.SubProducts.Count > 0)
                n.SubProducts = SortProductivityProfitTotalRegionTree(n.SubProducts, sortBy, isAscending);
        }

        return ordered;
    }

    private sealed class ProductivityProfitRatioRegionRow
    {
        public int Id { get; set; }
        public int? ParentProductId { get; set; }

        public string RatioName { get; set; } = string.Empty;

        public decimal TargetValue { get; set; }

        public decimal RegionValue { get; set; }
        public decimal? RegionValueDiff { get; set; }

        public decimal BankValue { get; set; }
        public decimal? BankValueDiff { get; set; }

        public decimal RetailValue { get; set; }
        public decimal KobiValue { get; set; }

        public decimal AgricultureValue { get; set; }
        public decimal? AgricultureValueDiff { get; set; }

        public decimal CommercialValue { get; set; }
        public decimal? CommercialValueDiff { get; set; }
    }

    private static List<GetProductivityProfitRatioRegionReportResponse.GetProductivityProfitRatioRegionReportItem>
    BuildProductivityProfitRatioRegionReportTree(DataSet ds, DateTime reportDate)
    {
        var t0 = ds.Tables[0];
        var hasParentInT0 = t0.Columns.Contains("ParentProductId");

        if (hasParentInT0)
        {
            var rows = DataTableHelper.ToList<ProductivityProfitRatioRegionRow>(t0);
            return ProductivityProfitRatioRegionReportTreeFromRows(rows);
        }

        if (ds.Tables.Count > 1 && ds.Tables[1].Columns.Contains("ParentProductId"))
        {
            var roots = DataTableHelper.ToList<ProductivityProfitRatioRegionRow>(ds.Tables[0]);
            var children = DataTableHelper.ToList<ProductivityProfitRatioRegionRow>(ds.Tables[1]);

            var all = new List<ProductivityProfitRatioRegionRow>(roots.Count + children.Count);
            all.AddRange(roots);
            all.AddRange(children);

            return ProductivityProfitRatioRegionReportTreeFromRows(all);
        }

        var flat = DataTableHelper.ToList<ProductivityProfitRatioRegionRow>(t0);
        return flat.Select(MapProductivityProfitRatioRegionReportItem).ToList();
    }

    private static List<GetProductivityProfitRatioRegionReportResponse.GetProductivityProfitRatioRegionReportItem>
    ProductivityProfitRatioRegionReportTreeFromRows(List<ProductivityProfitRatioRegionRow> rows)
    {
        var byId = rows
            .GroupBy(r => r.Id)
            .ToDictionary(g => g.Key, g => MapProductivityProfitRatioRegionReportItem(g.First()));

        foreach (var r in rows)
        {
            if (!byId.TryGetValue(r.Id, out var node))
            {
                node = MapProductivityProfitRatioRegionReportItem(r);
                byId[r.Id] = node;
            }

            var parentId = r.ParentProductId;
            if (parentId.HasValue && parentId.Value != 0 && byId.TryGetValue(parentId.Value, out var parent))
                parent.SubProducts.Add(node);
        }

        var rootIds = rows
            .Where(r => !r.ParentProductId.HasValue || r.ParentProductId.Value == 0 || !byId.ContainsKey(r.ParentProductId.Value))
            .Select(r => r.Id)
            .Distinct()
            .ToList();

        return rootIds.Select(id => byId[id]).ToList();
    }

    private static GetProductivityProfitRatioRegionReportResponse.GetProductivityProfitRatioRegionReportItem
    MapProductivityProfitRatioRegionReportItem(ProductivityProfitRatioRegionRow r)
    {
        return new GetProductivityProfitRatioRegionReportResponse.GetProductivityProfitRatioRegionReportItem
        {
            Id = r.Id,
            RatioName = r.RatioName ?? string.Empty,
            TargetValue = r.TargetValue,
            RegionValue = r.RegionValue,
            RegionValueDiff = r.RegionValueDiff,
            BankValue = r.BankValue,
            BankValueDiff = r.BankValueDiff,
            RetailValue = r.RetailValue,
            KobiValue = r.KobiValue,
            AgricultureValue = r.AgricultureValue,
            AgricultureValueDiff = r.AgricultureValueDiff,
            CommercialValue = r.CommercialValue,
            CommercialValueDiff = r.CommercialValueDiff,
            SubProducts = new List<GetProductivityProfitRatioRegionReportResponse.GetProductivityProfitRatioRegionReportItem>()
        };
    }

    private static List<GetProductivityProfitRatioRegionReportResponse.GetProductivityProfitRatioRegionReportItem>
    SortProductivityProfitRatioRegionTree(
        List<GetProductivityProfitRatioRegionReportResponse.GetProductivityProfitRatioRegionReportItem> nodes,
        int? sortBy,
        bool isAscending)
    {
        Func<GetProductivityProfitRatioRegionReportResponse.GetProductivityProfitRatioRegionReportItem, object> keySelector = sortBy switch
        {
            1 => p => p.RatioName ?? string.Empty,
            2 => p => p.TargetValue,
            3 => p => p.RegionValue,
            4 => p => p.BankValue,
            5 => p => p.RetailValue,
            6 => p => p.KobiValue,
            7 => p => p.AgricultureValue,
            8 => p => p.CommercialValue,
            _ => p => p.Id
        };

        var ordered = (isAscending ? nodes.OrderBy(keySelector) : nodes.OrderByDescending(keySelector)).ToList();

        foreach (var n in ordered)
        {
            if (n.SubProducts != null && n.SubProducts.Count > 0)
                n.SubProducts = SortProductivityProfitRatioRegionTree(n.SubProducts, sortBy, isAscending);
        }

        return ordered;
    }

    private sealed class ProductivityVolumeRegionRow
    {
        public int Id { get; set; }
        public int? ParentProductId { get; set; }

        public string ProductName { get; set; } = string.Empty;

        public decimal RealizationRegionValue { get; set; }
        public decimal? RealizationRegionDiff { get; set; }

        public decimal RealizationBankAverageValue { get; set; }
        public decimal? RealizationBankAverageDiff { get; set; }

        public decimal TargetValue { get; set; }
        public decimal HgRate { get; set; }

        public decimal NetGrowthRegionValue { get; set; }
        public decimal? NetGrowthRegionDiff { get; set; }

        public decimal NetGrowthBankAverageValue { get; set; }
        public decimal? NetGrowthBankAverageDiff { get; set; }

        public decimal YtdRegionValue { get; set; }
        public decimal? YtdRegionDiff { get; set; }

        public decimal YtdBankAverageValue { get; set; }
        public decimal? YtdBankAverageDiff { get; set; }

        public decimal QtdRegionValue { get; set; }
        public decimal? QtdRegionDiff { get; set; }

        public decimal QtdBankAverageValue { get; set; }
        public decimal? QtdBankAverageDiff { get; set; }
    }

    private static List<GetProductivityVolumeRegionReportResponse.GetProductivityVolumeRegionReportItem>
    BuildProductivityVolumeRegionReportTree(DataSet ds, DateTime reportDate)
    {
        var t0 = ds.Tables[0];
        var hasParentInT0 = t0.Columns.Contains("ParentProductId");

        if (hasParentInT0)
        {
            var rows = DataTableHelper.ToList<ProductivityVolumeRegionRow>(t0);
            return ProductivityVolumeRegionReportTreeFromRows(rows);
        }

        if (ds.Tables.Count > 1 && ds.Tables[1].Columns.Contains("ParentProductId"))
        {
            var roots = DataTableHelper.ToList<ProductivityVolumeRegionRow>(ds.Tables[0]);
            var children = DataTableHelper.ToList<ProductivityVolumeRegionRow>(ds.Tables[1]);

            var all = new List<ProductivityVolumeRegionRow>(roots.Count + children.Count);
            all.AddRange(roots);
            all.AddRange(children);

            return ProductivityVolumeRegionReportTreeFromRows(all);
        }

        var flat = DataTableHelper.ToList<ProductivityVolumeRegionRow>(t0);
        return flat.Select(MapProductivityVolumeRegionReportItem).ToList();
    }

    private static List<GetProductivityVolumeRegionReportResponse.GetProductivityVolumeRegionReportItem>
    ProductivityVolumeRegionReportTreeFromRows(List<ProductivityVolumeRegionRow> rows)
    {
        var byId = rows
            .GroupBy(r => r.Id)
            .ToDictionary(g => g.Key, g => MapProductivityVolumeRegionReportItem(g.First()));

        foreach (var r in rows)
        {
            if (!byId.TryGetValue(r.Id, out var node))
            {
                node = MapProductivityVolumeRegionReportItem(r);
                byId[r.Id] = node;
            }

            var parentId = r.ParentProductId;
            if (parentId.HasValue && parentId.Value != 0 && byId.TryGetValue(parentId.Value, out var parent))
                parent.SubProducts.Add(node);
        }

        var rootIds = rows
            .Where(r => !r.ParentProductId.HasValue || r.ParentProductId.Value == 0 || !byId.ContainsKey(r.ParentProductId.Value))
            .Select(r => r.Id)
            .Distinct()
            .ToList();

        return rootIds.Select(id => byId[id]).ToList();
    }

    private static GetProductivityVolumeRegionReportResponse.GetProductivityVolumeRegionReportItem
    MapProductivityVolumeRegionReportItem(ProductivityVolumeRegionRow r)
    {
        return new GetProductivityVolumeRegionReportResponse.GetProductivityVolumeRegionReportItem
        {
            Id = r.Id,
            ProductName = r.ProductName ?? string.Empty,
            RealizationRegionValue = r.RealizationRegionValue,
            RealizationRegionDiff = r.RealizationRegionDiff,
            RealizationBankAverageValue = r.RealizationBankAverageValue,
            RealizationBankAverageDiff = r.RealizationBankAverageDiff,
            TargetValue = r.TargetValue,
            HgRate = r.HgRate,
            NetGrowthRegionValue = r.NetGrowthRegionValue,
            NetGrowthRegionDiff = r.NetGrowthRegionDiff,
            NetGrowthBankAverageValue = r.NetGrowthBankAverageValue,
            NetGrowthBankAverageDiff = r.NetGrowthBankAverageDiff,
            YtdRegionValue = r.YtdRegionValue,
            YtdRegionDiff = r.YtdRegionDiff,
            YtdBankAverageValue = r.YtdBankAverageValue,
            YtdBankAverageDiff = r.YtdBankAverageDiff,
            QtdRegionValue = r.QtdRegionValue,
            QtdRegionDiff = r.QtdRegionDiff,
            QtdBankAverageValue = r.QtdBankAverageValue,
            QtdBankAverageDiff = r.QtdBankAverageDiff,
            SubProducts = new List<GetProductivityVolumeRegionReportResponse.GetProductivityVolumeRegionReportItem>()
        };
    }

    private static List<GetProductivityVolumeRegionReportResponse.GetProductivityVolumeRegionReportItem>
    SortProductivityVolumeRegionTree(
        List<GetProductivityVolumeRegionReportResponse.GetProductivityVolumeRegionReportItem> nodes,
        int? sortBy,
        bool isAscending)
    {
        Func<GetProductivityVolumeRegionReportResponse.GetProductivityVolumeRegionReportItem, object> keySelector = sortBy switch
        {
            1 => p => p.ProductName ?? string.Empty,
            2 => p => p.RealizationRegionValue,
            3 => p => p.RealizationBankAverageValue,
            4 => p => p.TargetValue,
            5 => p => p.HgRate,
            6 => p => p.NetGrowthRegionValue,
            7 => p => p.NetGrowthBankAverageValue,
            8 => p => p.YtdRegionValue,
            9 => p => p.YtdBankAverageValue,
            10 => p => p.QtdRegionValue,
            11 => p => p.QtdBankAverageValue,
            _ => p => p.Id
        };

        var ordered = (isAscending ? nodes.OrderBy(keySelector) : nodes.OrderByDescending(keySelector)).ToList();

        foreach (var n in ordered)
        {
            if (n.SubProducts != null && n.SubProducts.Count > 0)
                n.SubProducts = SortProductivityVolumeRegionTree(n.SubProducts, sortBy, isAscending);
        }

        return ordered;
    }

    #endregion
}
