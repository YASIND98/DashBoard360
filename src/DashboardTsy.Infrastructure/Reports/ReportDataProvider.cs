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
            ["@FilterType"] = request.FilterType,
            ["@RegionId"] = ToCsv(request.RegionId) ?? (object)DBNull.Value,
            ["@BranchId"] = ToCsv(request.BranchId) ?? (object)DBNull.Value,
            ["@TabId"] = request.TabId,
            ["@SubTabId"] = request.SubTabId ?? (object)DBNull.Value,
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

        var ds = _spExecutor.ExecuteDataSet("YoneticiRaporu", "SP_RP_GetProductivityReportTabs", parameters);
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

    //public IReadOnlyList<GetReportRegionFilterItem> GetReportRegionFilters(GetReportRegionFiltersRequest request)
    //{
    //    request ??= new GetReportRegionFiltersRequest();

    //    if (MockEnabled)
    //        return MockProductivityReportData.GetReportRegionFilters();

    //    const string sql = @"
    //    SELECT DISTINCT BOLGE_KODU, BOLGE
    //    FROM DW_BOLGELER
    //    ORDER BY BOLGE;";

    //    var ds = _spExecutor.ExecuteQueryDataSet("Referans", sql);
    //    if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
    //        return Array.Empty<GetReportRegionFilterItem>();

    //    return ds.Tables[0].AsEnumerable()
    //        .Select(r => new GetReportRegionFilterItem
    //        {
    //            Code = (r["BOLGE_KODU"]?.ToString() ?? string.Empty).Trim(),
    //            Name = (r["BOLGE"]?.ToString() ?? string.Empty).Trim()
    //        })
    //        .Where(x => !string.IsNullOrWhiteSpace(x.Code))
    //        .ToList();
    //}

    //public IReadOnlyList<GetReportBranchFilterItem> GetReportBranchFilters(GetReportBranchFiltersRequest request)
    //{
    //    request ??= new GetReportBranchFiltersRequest();

    //    if (MockEnabled)
    //        return MockProductivityReportData.GetReportBranchFilters();

    //    const string sql = @"
    //    SELECT SUBE_KODU, SUBE_ADI, BOLGE_KODU
    //    FROM DW_BOLGELER
    //    ORDER BY SUBE_ADI;";

    //    var ds = _spExecutor.ExecuteQueryDataSet("Referans", sql);
    //    if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
    //        return Array.Empty<GetReportBranchFilterItem>();

    //    return ds.Tables[0].AsEnumerable()
    //        .Select(r => new GetReportBranchFilterItem
    //        {
    //            Code = (r["SUBE_KODU"]?.ToString() ?? string.Empty).Trim(),
    //            Name = (r["SUBE_ADI"]?.ToString() ?? string.Empty).Trim(),
    //            RegionCode = (r["BOLGE_KODU"]?.ToString() ?? string.Empty).Trim()
    //        })
    //        .Where(x => !string.IsNullOrWhiteSpace(x.Code))
    //        .ToList();
    //}

    public IReadOnlyList<GetReportRegionFilterItem> GetReportRegionFilters(GetReportRegionFiltersRequest request)
    {
        request ??= new GetReportRegionFiltersRequest();

        if (MockEnabled)
            return MockProductivityReportData.GetReportRegionFilters();

        var parameters = new Dictionary<string, object?>
        {
            ["@SessionId"] = request.SessionId ?? string.Empty
        };

        var ds = _spExecutor.ExecuteDataSet(
            "NorthStarMobile",
            "RP_SP_GetReportRegionFilters",
            parameters);

        if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
            return Array.Empty<GetReportRegionFilterItem>();

        return DataTableHelper.ToList<GetReportRegionFilterItem>(ds.Tables[0]);
    }

    public IReadOnlyList<GetReportBranchFilterItem> GetReportBranchFilters(GetReportBranchFiltersRequest request)
    {
        request ??= new GetReportBranchFiltersRequest();

        if (MockEnabled)
            return MockProductivityReportData.GetReportBranchFilters();

        var parameters = new Dictionary<string, object?>
        {
            ["@SessionId"] = request.SessionId ?? string.Empty
        };

        var ds = _spExecutor.ExecuteDataSet(
            "NorthStarMobile",
            "RP_SP_GetReportBranchFilters",
            parameters);

        if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
            return Array.Empty<GetReportBranchFilterItem>();

        return DataTableHelper.ToList<GetReportBranchFilterItem>(ds.Tables[0]);
    }

    public GetProductivityGeneralRegionReportResponse? GetProductivityGeneralRegionReport(GetProductivityGeneralRegionReportRequest request)
    {
        request ??= new GetProductivityGeneralRegionReportRequest();

        if (MockEnabled)
            return MockProductivityReportData.GetProductivityGeneralRegionReport(request);

        var parameters = new Dictionary<string, object?>
        {
            ["@SessionId"] = request.SessionId ?? string.Empty,
            ["@RegionCode"] = string.IsNullOrWhiteSpace(request.RegionCode) ? (object)DBNull.Value : request.RegionCode,
            ["@ReportDate"] = request.ReportDate,
            ["@SortBy"] = request.SortBy ?? (object)DBNull.Value,
            ["@IsAscending"] = request.IsAscending
        };

        var ds = _spExecutor.ExecuteDataSet(
            "YoneticiRaporu",
            "SP_RP_GetProductivityGeneralRegionReport",
            parameters);

        var response = new GetProductivityGeneralRegionReportResponse();

        if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
            return response;

        var roots = BuildProductivityGeneralRegionReportTree(ds, request.ReportDate);
        roots = SortProductivityGeneralRegionTree(roots, request.SortBy, request.IsAscending);

        response.GetProductivityGeneralRegionReports = roots;
        return response;
    }

    public GetProductivityCountCardPosRegionReportResponse? GetProductivityCountCardPosRegionReport(GetProductivityCountCardPosRegionReportRequest request)
    {
        request ??= new GetProductivityCountCardPosRegionReportRequest();

        if (MockEnabled)
            return MockProductivityReportData.GetProductivityCountCardPosRegionReport(request);

        var parameters = new Dictionary<string, object?>
        {
            ["@SessionId"] = request.SessionId ?? string.Empty,
            ["@RegionCode"] = string.IsNullOrWhiteSpace(request.RegionCode) ? (object)DBNull.Value : request.RegionCode,
            ["@TabId"] = request.TabId,
            ["@ReportDate"] = request.ReportDate,
            ["@SortBy"] = request.SortBy ?? (object)DBNull.Value,
            ["@IsAscending"] = request.IsAscending
        };

        var ds = _spExecutor.ExecuteDataSet(
            "YoneticiRaporu",
            "SP_RP_GetProductivityCountCardPosRegionReport",
            parameters);

        var response = new GetProductivityCountCardPosRegionReportResponse();

        if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
            return response;

        var roots = BuildProductivityCountCardPosRegionReportTree(ds, request.ReportDate);
        roots = SortProductivityCountCardPosRegionTree(roots, request.SortBy, request.IsAscending);

        response.GetProductivityCountCardPosRegionReports = roots;
        return response;
    }

    public IReadOnlyList<GetProductivityCountCardPosRatioRegionReportItem> GetProductivityCountCardPosRatioRegionReport(GetProductivityCountCardPosRatioRegionReportRequest request)
    {
        request ??= new GetProductivityCountCardPosRatioRegionReportRequest();

        if (MockEnabled)
            return MockProductivityReportData.GetProductivityCountCardPosRatioRegionReport(request);

        var parameters = new Dictionary<string, object?>
        {
            ["@SessionId"] = request.SessionId ?? string.Empty,
            ["@RegionCode"] = request.RegionCode ?? string.Empty,
            ["@TabId"] = request.TabId,
            ["@ReportDate"] = request.ReportDate
        };

        var ds = _spExecutor.ExecuteDataSet(
            "YoneticiRaporu",
            "SP_RP_GetProductivityCountCardPosRatioRegionReport",
            parameters);

        if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
            return Array.Empty<GetProductivityCountCardPosRatioRegionReportItem>();

        return DataTableHelper.ToList<GetProductivityCountCardPosRatioRegionReportItem>(ds.Tables[0]);
    }

    public GetProductivityCountCardPosRatioRegionReportTableHeadersItem? GetProductivityCountCardPosRatioRegionReportTableHeaders(GetProductivityCountCardPosRatioRegionReportTableHeadersRequest request)
    {
        request ??= new GetProductivityCountCardPosRatioRegionReportTableHeadersRequest();

        if (MockEnabled)
            return MockProductivityReportData.GetProductivityCountCardPosRatioRegionReportTableHeaders(request);

        var parameters = new Dictionary<string, object?>
        {
            ["@SessionId"] = request.SessionId ?? string.Empty,
            ["@TabId"] = request.TabId,
            ["@ReportDate"] = request.ReportDate
        };

        var ds = _spExecutor.ExecuteDataSet(
            "YoneticiRaporu",
            "SP_RP_GetProductivityCountCardPosRatioRegionReportTableHeaders",
            parameters);

        if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
            return new GetProductivityCountCardPosRatioRegionReportTableHeadersItem();

        return DataTableHelper.ToObject<GetProductivityCountCardPosRatioRegionReportTableHeadersItem>(ds.Tables[0].Rows[0]);
    }

    public GetProductivityCountCustomerRegionReportResponse? GetProductivityCountCustomerRegionReport(GetProductivityCountCustomerRegionReportRequest request)
    {
        request ??= new GetProductivityCountCustomerRegionReportRequest();

        if (MockEnabled)
            return MockProductivityReportData.GetProductivityCountCustomerRegionReport(request);

        var parameters = new Dictionary<string, object?>
        {
            ["@SessionId"] = request.SessionId ?? string.Empty,
            ["@RegionCode"] = string.IsNullOrWhiteSpace(request.RegionCode) ? (object)DBNull.Value : request.RegionCode,
            ["@SubTabId"] = request.SubTabId,
            ["@ReportDate"] = request.ReportDate,
            ["@SortBy"] = request.SortBy ?? (object)DBNull.Value,
            ["@IsAscending"] = request.IsAscending
        };

        var ds = _spExecutor.ExecuteDataSet(
            "YoneticiRaporu",
            "SP_RP_GetProductivityCountCustomerRegionReport",
            parameters);

        var response = new GetProductivityCountCustomerRegionReportResponse();

        if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
            return response;

        var roots = BuildProductivityCountCustomerRegionReportTree(ds, request.ReportDate);
        roots = SortProductivityCountCustomerRegionTree(roots, request.SortBy, request.IsAscending);

        response.GetProductivityCountCustomerRegionReports = roots;
        return response;
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

    public GetProductivityProfitSpreadManagementBranchReportResponse? GetProductivityProfitSpreadManagementBranchReport(GetProductivityProfitSpreadManagementBranchReportRequest request)
    {
        request ??= new GetProductivityProfitSpreadManagementBranchReportRequest();

        if (MockEnabled)
            return MockProductivityReportData.GetProductivityProfitSpreadManagementBranchReport(request);

        var parameters = new Dictionary<string, object?>
        {
            ["@SessionId"] = request.SessionId ?? string.Empty,
            ["@BranchCode"] = string.IsNullOrWhiteSpace(request.BranchCode) ? (object)DBNull.Value : request.BranchCode,
            ["@ReportDate"] = request.ReportDate,
            ["@SortBy"] = request.SortBy ?? (object)DBNull.Value,
            ["@IsAscending"] = request.IsAscending
        };

        var ds = _spExecutor.ExecuteDataSet(
            "YoneticiRaporu",
            "SP_RP_GetProductivityProfitSpreadManagementBranchReport",
            parameters);

        var response = new GetProductivityProfitSpreadManagementBranchReportResponse();

        if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
            return response;

        var roots = BuildProductivityProfitSpreadManagementBranchReportTree(ds, request.ReportDate);
        roots = SortProductivityProfitSpreadManagementBranchTree(roots, request.SortBy, request.IsAscending);

        response.GetProductivityProfitSpreadManagementBranchReports = roots;
        return response;
    }

    public GetProductivityCountCardPosBranchReportResponse? GetProductivityCountCardPosBranchReport(GetProductivityCountCardPosBranchReportRequest request)
    {
        request ??= new GetProductivityCountCardPosBranchReportRequest();

        if (MockEnabled)
            return MockProductivityReportData.GetProductivityCountCardPosBranchReport(request);

        var parameters = new Dictionary<string, object?>
        {
            ["@SessionId"] = request.SessionId ?? string.Empty,
            ["@BranchCode"] = string.IsNullOrWhiteSpace(request.BranchCode) ? (object)DBNull.Value : request.BranchCode,
            ["@TabId"] = request.TabId,
            ["@ReportDate"] = request.ReportDate,
            ["@SortBy"] = request.SortBy ?? (object)DBNull.Value,
            ["@IsAscending"] = request.IsAscending
        };

        var ds = _spExecutor.ExecuteDataSet(
            "YoneticiRaporu",
            "SP_RP_GetProductivityCountCardPosBranchReport",
            parameters);

        var response = new GetProductivityCountCardPosBranchReportResponse();

        if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
            return response;

        var roots = BuildProductivityCountCardPosBranchReportTree(ds, request.ReportDate);
        roots = SortProductivityCountCardPosBranchTree(roots, request.SortBy, request.IsAscending);

        response.GetProductivityCountCardPosBranchReports = roots;
        return response;
    }

    public IReadOnlyList<GetProductivityCountCardPosRatioBranchReportItem> GetProductivityCountCardPosRatioBranchReport(GetProductivityCountCardPosRatioBranchReportRequest request)
    {
        request ??= new GetProductivityCountCardPosRatioBranchReportRequest();

        if (MockEnabled)
            return MockProductivityReportData.GetProductivityCountCardPosRatioBranchReport(request);

        var parameters = new Dictionary<string, object?>
        {
            ["@SessionId"] = request.SessionId ?? string.Empty,
            ["@BranchCode"] = string.IsNullOrWhiteSpace(request.BranchCode) ? (object)DBNull.Value : request.BranchCode,
            ["@TabId"] = request.TabId,
            ["@ReportDate"] = request.ReportDate
        };

        var ds = _spExecutor.ExecuteDataSet(
            "YoneticiRaporu",
            "SP_RP_GetProductivityCountCardPosRatioBranchReport",
            parameters);

        if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
            return Array.Empty<GetProductivityCountCardPosRatioBranchReportItem>();

        return DataTableHelper.ToList<GetProductivityCountCardPosRatioBranchReportItem>(ds.Tables[0]);
    }

    public GetProductivityCountCardPosRatioBranchReportTableHeadersItem? GetProductivityCountCardPosRatioBranchReportTableHeaders(GetProductivityCountCardPosRatioBranchReportTableHeadersRequest request)
    {
        request ??= new GetProductivityCountCardPosRatioBranchReportTableHeadersRequest();

        if (MockEnabled)
            return MockProductivityReportData.GetProductivityCountCardPosRatioBranchReportTableHeaders(request);

        var parameters = new Dictionary<string, object?>
        {
            ["@SessionId"] = request.SessionId ?? string.Empty,
            ["@TabId"] = request.TabId,
            ["@ReportDate"] = request.ReportDate
        };

        var ds = _spExecutor.ExecuteDataSet(
            "YoneticiRaporu",
            "SP_RP_GetProductivityCountCardPosRatioBranchReportTableHeaders",
            parameters);

        if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
            return new GetProductivityCountCardPosRatioBranchReportTableHeadersItem();

        return DataTableHelper.ToObject<GetProductivityCountCardPosRatioBranchReportTableHeadersItem>(ds.Tables[0].Rows[0]);
    }

    public GetProductivityProfitRatioBranchReportResponse? GetProductivityProfitRatioBranchReport(GetProductivityProfitRatioBranchReportRequest request)
    {
        request ??= new GetProductivityProfitRatioBranchReportRequest();

        if (MockEnabled)
            return MockProductivityReportData.GetProductivityProfitRatioBranchReport(request);

        var parameters = new Dictionary<string, object?>
        {
            ["@SessionId"] = request.SessionId ?? string.Empty,
            ["@BranchCode"] = string.IsNullOrWhiteSpace(request.BranchCode) ? (object)DBNull.Value : request.BranchCode,
            ["@ReportDate"] = request.ReportDate,
            ["@SortBy"] = request.SortBy ?? (object)DBNull.Value,
            ["@IsAscending"] = request.IsAscending
        };

        var ds = _spExecutor.ExecuteDataSet(
            "YoneticiRaporu",
            "SP_RP_GetProductivityProfitRatioBranchReport",
            parameters);

        var response = new GetProductivityProfitRatioBranchReportResponse();

        if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
            return response;

        var roots = BuildProductivityProfitRatioBranchReportTree(ds, request.ReportDate);
        roots = SortProductivityProfitRatioBranchTree(roots, request.SortBy, request.IsAscending);

        response.GetProductivityProfitRatioBranchReports = roots;
        return response;
    }

    public GetProductivityProfitTotalBranchReportResponse? GetProductivityProfitTotalBranchReport(GetProductivityProfitTotalBranchReportRequest request)
    {
        request ??= new GetProductivityProfitTotalBranchReportRequest();

        if (MockEnabled)
            return MockProductivityReportData.GetProductivityProfitTotalBranchReport(request);

        var parameters = new Dictionary<string, object?>
        {
            ["@SessionId"] = request.SessionId ?? string.Empty,
            ["@BranchCode"] = string.IsNullOrWhiteSpace(request.BranchCode) ? (object)DBNull.Value : request.BranchCode,
            ["@ReportDate"] = request.ReportDate,
            ["@SortBy"] = request.SortBy ?? (object)DBNull.Value,
            ["@IsAscending"] = request.IsAscending
        };

        var ds = _spExecutor.ExecuteDataSet(
            "YoneticiRaporu",
            "SP_RP_GetProductivityProfitTotalBranchReport",
            parameters);

        var response = new GetProductivityProfitTotalBranchReportResponse();

        if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
            return response;

        var roots = BuildProductivityProfitTotalBranchReportTree(ds, request.ReportDate);
        roots = SortProductivityProfitTotalBranchTree(roots, request.SortBy, request.IsAscending);

        response.GetProductivityProfitTotalBranchReports = roots;
        return response;
    }

    public GetProductivityBranchScoreCardReportItem? GetProductivityBranchScoreCardReport(GetProductivityBranchScoreCardReportRequest request)
    {
        request ??= new GetProductivityBranchScoreCardReportRequest();

        if (MockEnabled)
            return MockProductivityReportData.GetProductivityBranchScoreCardReport(request);

        var parameters = new Dictionary<string, object?>
        {
            ["@SessionId"] = request.SessionId ?? string.Empty,
            ["@BranchCode"] = string.IsNullOrWhiteSpace(request.BranchCode) ? (object)DBNull.Value : request.BranchCode,
            ["@ReportDate"] = request.ReportDate
        };

        var ds = _spExecutor.ExecuteDataSet(
            "YoneticiRaporu",
            "SP_RP_GetProductivityBranchScoreCardReport",
            parameters);

        if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
            return new GetProductivityBranchScoreCardReportItem();

        return DataTableHelper.ToObject<GetProductivityBranchScoreCardReportItem>(ds.Tables[0].Rows[0]);
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

        var ds = _spExecutor.ExecuteDataSet("YoneticiRaporu", "SP_RP_GetProductivityRegionScoreCardReport", parameters);

        if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
            return new GetProductivityRegionScoreCardReportItem();

        return DataTableHelper.ToObject<GetProductivityRegionScoreCardReportItem>(ds.Tables[0].Rows[0]);
    }

    public GetProductivityCountCustomerBranchReportResponse? GetProductivityCountCustomerBranchReport(GetProductivityCountCustomerBranchReportRequest request)
    {
        request ??= new GetProductivityCountCustomerBranchReportRequest();

        if (MockEnabled)
            return MockProductivityReportData.GetProductivityCountCustomerBranchReport(request);

        var parameters = new Dictionary<string, object?>
        {
            ["@SessionId"] = request.SessionId ?? string.Empty,
            ["@BranchCode"] = string.IsNullOrWhiteSpace(request.BranchCode) ? (object)DBNull.Value : request.BranchCode,
            ["@SubTabId"] = request.SubTabId,
            ["@ReportDate"] = request.ReportDate,
            ["@SortBy"] = request.SortBy ?? (object)DBNull.Value,
            ["@IsAscending"] = request.IsAscending
        };

        var ds = _spExecutor.ExecuteDataSet(
            "YoneticiRaporu",
            "SP_RP_GetProductivityCountCustomerBranchReport",
            parameters);

        var response = new GetProductivityCountCustomerBranchReportResponse();

        if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
            return response;

        var roots = BuildProductivityCountCustomerBranchReportTree(ds, request.ReportDate);
        roots = SortProductivityCountCustomerBranchTree(roots, request.SortBy, request.IsAscending);

        response.GetProductivityCountCustomerBranchReports = roots;
        return response;
    }

    public GetProductivityVolumeBranchReportResponse? GetProductivityVolumeBranchReport(GetProductivityVolumeBranchReportRequest request)
    {
        request ??= new GetProductivityVolumeBranchReportRequest();

        if (MockEnabled)
            return MockProductivityReportData.GetProductivityVolumeBranchReport(request);

        var parameters = new Dictionary<string, object?>
        {
            ["@SessionId"] = request.SessionId ?? string.Empty,
            ["@BranchCode"] = string.IsNullOrWhiteSpace(request.BranchCode) ? (object)DBNull.Value : request.BranchCode,
            ["@SubTabId"] = request.SubTabId,
            ["@ReportDate"] = request.ReportDate,
            ["@SortBy"] = request.SortBy ?? (object)DBNull.Value,
            ["@IsAscending"] = request.IsAscending
        };

        var ds = _spExecutor.ExecuteDataSet(
            "YoneticiRaporu",
            "SP_RP_GetProductivityVolumeBranchReport",
            parameters);

        var response = new GetProductivityVolumeBranchReportResponse();

        if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
            return response;

        var roots = BuildProductivityVolumeBranchReportTree(ds, request.ReportDate);
        roots = SortProductivityVolumeBranchTree(roots, request.SortBy, request.IsAscending);

        response.GetProductivityVolumeBranchReports = roots;
        return response;
    }

    #region Helpers

    private static string? ToCsv(List<int>? list)
        => list != null && list.Count > 0 ? string.Join(",", list) : null;

    private static string? ToCsv(List<string>? list)
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

    private sealed class ProductivityGeneralRegionRow
    {
        public int Id { get; set; }
        public int? ParentProductId { get; set; }

        public string BranchName { get; set; } = string.Empty;

        public decimal FirstMonthRealizationRate { get; set; }
        public decimal SecondMonthRealizationRate { get; set; }
        public decimal ThirdMonthRealizationRate { get; set; }

        public decimal CorporateRate { get; set; }
        public decimal CommercialRate { get; set; }
        public decimal KbiRate { get; set; }
        public decimal ObiRate { get; set; }
        public decimal AgricultureRate { get; set; }
        public decimal MassRate { get; set; }
        public decimal AffluentRate { get; set; }
        public decimal PrivateBankingRate { get; set; }
    }

    private static List<GetProductivityGeneralRegionReportResponse.GetProductivityGeneralRegionReportItem> BuildProductivityGeneralRegionReportTree(DataSet ds, DateTime reportDate)
    {
        var t0 = ds.Tables[0];
        var hasParentInT0 = t0.Columns.Contains("ParentProductId");

        if (hasParentInT0)
        {
            var rows = DataTableHelper.ToList<ProductivityGeneralRegionRow>(t0);
            return ProductivityGeneralRegionReportTreeFromRows(rows);
        }

        if (ds.Tables.Count > 1 && ds.Tables[1].Columns.Contains("ParentProductId"))
        {
            var roots = DataTableHelper.ToList<ProductivityGeneralRegionRow>(ds.Tables[0]);
            var children = DataTableHelper.ToList<ProductivityGeneralRegionRow>(ds.Tables[1]);

            var all = new List<ProductivityGeneralRegionRow>(roots.Count + children.Count);
            all.AddRange(roots);
            all.AddRange(children);

            return ProductivityGeneralRegionReportTreeFromRows(all);
        }

        var flat = DataTableHelper.ToList<ProductivityGeneralRegionRow>(t0);
        return flat.Select(MapProductivityGeneralRegionReportItem).ToList();
    }

    private static List<GetProductivityGeneralRegionReportResponse.GetProductivityGeneralRegionReportItem> ProductivityGeneralRegionReportTreeFromRows(List<ProductivityGeneralRegionRow> rows)
    {
        var byId = rows
            .GroupBy(r => r.Id)
            .ToDictionary(g => g.Key, g => MapProductivityGeneralRegionReportItem(g.First()));

        foreach (var r in rows)
        {
            if (!byId.TryGetValue(r.Id, out var node))
            {
                node = MapProductivityGeneralRegionReportItem(r);
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

    private static GetProductivityGeneralRegionReportResponse.GetProductivityGeneralRegionReportItem MapProductivityGeneralRegionReportItem(ProductivityGeneralRegionRow r)
    {
        return new GetProductivityGeneralRegionReportResponse.GetProductivityGeneralRegionReportItem
        {
            Id = r.Id,
            BranchName = r.BranchName ?? string.Empty,
            FirstMonthRealizationRate = r.FirstMonthRealizationRate,
            SecondMonthRealizationRate = r.SecondMonthRealizationRate,
            ThirdMonthRealizationRate = r.ThirdMonthRealizationRate,
            CorporateRate = r.CorporateRate,
            CommercialRate = r.CommercialRate,
            KbiRate = r.KbiRate,
            ObiRate = r.ObiRate,
            AgricultureRate = r.AgricultureRate,
            MassRate = r.MassRate,
            AffluentRate = r.AffluentRate,
            PrivateBankingRate = r.PrivateBankingRate,
            SubProducts = new List<GetProductivityGeneralRegionReportResponse.GetProductivityGeneralRegionReportItem>()
        };
    }

    private static List<GetProductivityGeneralRegionReportResponse.GetProductivityGeneralRegionReportItem> SortProductivityGeneralRegionTree(List<GetProductivityGeneralRegionReportResponse.GetProductivityGeneralRegionReportItem> nodes, int? sortBy, bool isAscending)
    {
        Func<GetProductivityGeneralRegionReportResponse.GetProductivityGeneralRegionReportItem, object> keySelector = sortBy switch
        {
            1 => p => p.BranchName ?? string.Empty,
            2 => p => p.FirstMonthRealizationRate,
            3 => p => p.SecondMonthRealizationRate,
            4 => p => p.ThirdMonthRealizationRate,
            5 => p => p.CorporateRate,
            6 => p => p.CommercialRate,
            7 => p => p.KbiRate,
            8 => p => p.ObiRate,
            9 => p => p.AgricultureRate,
            10 => p => p.MassRate,
            11 => p => p.AffluentRate,
            12 => p => p.PrivateBankingRate,
            _ => p => p.Id
        };

        var ordered = (isAscending ? nodes.OrderBy(keySelector) : nodes.OrderByDescending(keySelector)).ToList();

        foreach (var n in ordered)
        {
            if (n.SubProducts != null && n.SubProducts.Count > 0)
                n.SubProducts = SortProductivityGeneralRegionTree(n.SubProducts, sortBy, isAscending);
        }

        return ordered;
    }

    private sealed class ProductivityCountCardPosRegionRow
    {
        public int Id { get; set; }
        public int? ParentProductId { get; set; }

        public string ProductName { get; set; } = string.Empty;

        public decimal CurrentMonthRegionValue { get; set; }
        public decimal CurrentMonthBankAverage { get; set; }
        public decimal? CurrentMonthBankAverageDiff { get; set; }

        public decimal ThreeMonthHgRegion { get; set; }
        public decimal ThreeMonthHgBankAverage { get; set; }
        public decimal? ThreeMonthHgBankAverageDiff { get; set; }
    }

    private static List<GetProductivityCountCardPosRegionReportResponse.GetProductivityCountCardPosRegionReportItem> BuildProductivityCountCardPosRegionReportTree(DataSet ds, DateTime reportDate)
    {
        var t0 = ds.Tables[0];
        var hasParentInT0 = t0.Columns.Contains("ParentProductId");

        if (hasParentInT0)
        {
            var rows = DataTableHelper.ToList<ProductivityCountCardPosRegionRow>(t0);
            return ProductivityCountCardPosRegionReportTreeFromRows(rows);
        }

        if (ds.Tables.Count > 1 && ds.Tables[1].Columns.Contains("ParentProductId"))
        {
            var roots = DataTableHelper.ToList<ProductivityCountCardPosRegionRow>(ds.Tables[0]);
            var children = DataTableHelper.ToList<ProductivityCountCardPosRegionRow>(ds.Tables[1]);

            var all = new List<ProductivityCountCardPosRegionRow>(roots.Count + children.Count);
            all.AddRange(roots);
            all.AddRange(children);

            return ProductivityCountCardPosRegionReportTreeFromRows(all);
        }

        var flat = DataTableHelper.ToList<ProductivityCountCardPosRegionRow>(t0);
        return flat.Select(MapProductivityCountCardPosRegionReportItem).ToList();
    }

    private static List<GetProductivityCountCardPosRegionReportResponse.GetProductivityCountCardPosRegionReportItem> ProductivityCountCardPosRegionReportTreeFromRows(List<ProductivityCountCardPosRegionRow> rows)
    {
        var byId = rows
            .GroupBy(r => r.Id)
            .ToDictionary(g => g.Key, g => MapProductivityCountCardPosRegionReportItem(g.First()));

        foreach (var r in rows)
        {
            if (!byId.TryGetValue(r.Id, out var node))
            {
                node = MapProductivityCountCardPosRegionReportItem(r);
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

    private static GetProductivityCountCardPosRegionReportResponse.GetProductivityCountCardPosRegionReportItem MapProductivityCountCardPosRegionReportItem(ProductivityCountCardPosRegionRow r)
    {
        return new GetProductivityCountCardPosRegionReportResponse.GetProductivityCountCardPosRegionReportItem
        {
            Id = r.Id,
            ProductName = r.ProductName ?? string.Empty,
            CurrentMonthRegionValue = r.CurrentMonthRegionValue,
            CurrentMonthBankAverage = r.CurrentMonthBankAverage,
            CurrentMonthBankAverageDiff = r.CurrentMonthBankAverageDiff,
            ThreeMonthHgRegion = r.ThreeMonthHgRegion,
            ThreeMonthHgBankAverage = r.ThreeMonthHgBankAverage,
            ThreeMonthHgBankAverageDiff = r.ThreeMonthHgBankAverageDiff,
            SubProducts = new List<GetProductivityCountCardPosRegionReportResponse.GetProductivityCountCardPosRegionReportItem>()
        };
    }

    private static List<GetProductivityCountCardPosRegionReportResponse.GetProductivityCountCardPosRegionReportItem> SortProductivityCountCardPosRegionTree(
    List<GetProductivityCountCardPosRegionReportResponse.GetProductivityCountCardPosRegionReportItem> nodes,
    int? sortBy,
    bool isAscending)
    {
        Func<GetProductivityCountCardPosRegionReportResponse.GetProductivityCountCardPosRegionReportItem, object> keySelector = sortBy switch
        {
            1 => p => p.ProductName ?? string.Empty,
            2 => p => p.CurrentMonthRegionValue,
            3 => p => p.CurrentMonthBankAverage,
            4 => p => p.ThreeMonthHgRegion,
            5 => p => p.ThreeMonthHgBankAverage,
            _ => p => p.Id
        };

        var ordered = (isAscending ? nodes.OrderBy(keySelector) : nodes.OrderByDescending(keySelector)).ToList();

        foreach (var n in ordered)
        {
            if (n.SubProducts != null && n.SubProducts.Count > 0)
                n.SubProducts = SortProductivityCountCardPosRegionTree(n.SubProducts, sortBy, isAscending);
        }

        return ordered;
    }

    private sealed class ProductivityCountCustomerRegionRow
    {
        public int Id { get; set; }
        public int? ParentProductId { get; set; }

        public string ProductName { get; set; } = string.Empty;

        public decimal RealizationRegion { get; set; }
        public decimal? RealizationRegionDiff { get; set; }
        public decimal RealizationBankAverage { get; set; }
        public decimal? RealizationBankAverageDiff { get; set; }

        public decimal YtdChangeRegion { get; set; }
        public decimal? YtdChangeRegionDiff { get; set; }
        public decimal YtdChangeBankAverage { get; set; }
        public decimal? YtdChangeBankAverageDiff { get; set; }

        public decimal QtdChangeRegion { get; set; }
        public decimal? QtdChangeRegionDiff { get; set; }
        public decimal QtdChangeBankAverage { get; set; }
        public decimal? QtdChangeBankAverageDiff { get; set; }
    }

    private static List<GetProductivityCountCustomerRegionReportResponse.GetProductivityCountCustomerRegionReportItem> BuildProductivityCountCustomerRegionReportTree(DataSet ds, DateTime reportDate)
    {
        var t0 = ds.Tables[0];
        var hasParentInT0 = t0.Columns.Contains("ParentProductId");

        if (hasParentInT0)
        {
            var rows = DataTableHelper.ToList<ProductivityCountCustomerRegionRow>(t0);
            return ProductivityCountCustomerRegionReportTreeFromRows(rows);
        }

        if (ds.Tables.Count > 1 && ds.Tables[1].Columns.Contains("ParentProductId"))
        {
            var roots = DataTableHelper.ToList<ProductivityCountCustomerRegionRow>(ds.Tables[0]);
            var children = DataTableHelper.ToList<ProductivityCountCustomerRegionRow>(ds.Tables[1]);

            var all = new List<ProductivityCountCustomerRegionRow>(roots.Count + children.Count);
            all.AddRange(roots);
            all.AddRange(children);

            return ProductivityCountCustomerRegionReportTreeFromRows(all);
        }

        var flat = DataTableHelper.ToList<ProductivityCountCustomerRegionRow>(t0);
        return flat.Select(MapProductivityCountCustomerRegionReportItem).ToList();
    }

    private static List<GetProductivityCountCustomerRegionReportResponse.GetProductivityCountCustomerRegionReportItem> ProductivityCountCustomerRegionReportTreeFromRows(List<ProductivityCountCustomerRegionRow> rows)
    {
        var byId = rows
            .GroupBy(r => r.Id)
            .ToDictionary(g => g.Key, g => MapProductivityCountCustomerRegionReportItem(g.First()));

        foreach (var r in rows)
        {
            if (!byId.TryGetValue(r.Id, out var node))
            {
                node = MapProductivityCountCustomerRegionReportItem(r);
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

    private static GetProductivityCountCustomerRegionReportResponse.GetProductivityCountCustomerRegionReportItem MapProductivityCountCustomerRegionReportItem(ProductivityCountCustomerRegionRow r)
    {
        return new GetProductivityCountCustomerRegionReportResponse.GetProductivityCountCustomerRegionReportItem
        {
            Id = r.Id,
            ProductName = r.ProductName ?? string.Empty,

            RealizationRegion = r.RealizationRegion,
            RealizationRegionDiff = r.RealizationRegionDiff,
            RealizationBankAverage = r.RealizationBankAverage,
            RealizationBankAverageDiff = r.RealizationBankAverageDiff,

            YtdChangeRegion = r.YtdChangeRegion,
            YtdChangeRegionDiff = r.YtdChangeRegionDiff,
            YtdChangeBankAverage = r.YtdChangeBankAverage,
            YtdChangeBankAverageDiff = r.YtdChangeBankAverageDiff,

            QtdChangeRegion = r.QtdChangeRegion,
            QtdChangeRegionDiff = r.QtdChangeRegionDiff,
            QtdChangeBankAverage = r.QtdChangeBankAverage,
            QtdChangeBankAverageDiff = r.QtdChangeBankAverageDiff,

            SubProducts = new List<GetProductivityCountCustomerRegionReportResponse.GetProductivityCountCustomerRegionReportItem>()
        };
    }

    private static List<GetProductivityCountCustomerRegionReportResponse.GetProductivityCountCustomerRegionReportItem> SortProductivityCountCustomerRegionTree(List<GetProductivityCountCustomerRegionReportResponse.GetProductivityCountCustomerRegionReportItem> nodes, int? sortBy, bool isAscending)
    {
        Func<GetProductivityCountCustomerRegionReportResponse.GetProductivityCountCustomerRegionReportItem, object> keySelector = sortBy switch
        {
            1 => p => p.ProductName ?? string.Empty,
            2 => p => p.RealizationRegion,
            3 => p => p.RealizationBankAverage,
            4 => p => p.YtdChangeRegion,
            5 => p => p.YtdChangeBankAverage,
            6 => p => p.QtdChangeRegion,
            7 => p => p.QtdChangeBankAverage,
            _ => p => p.Id
        };

        var ordered = (isAscending ? nodes.OrderBy(keySelector) : nodes.OrderByDescending(keySelector)).ToList();

        foreach (var n in ordered)
        {
            if (n.SubProducts != null && n.SubProducts.Count > 0)
                n.SubProducts = SortProductivityCountCustomerRegionTree(n.SubProducts, sortBy, isAscending);
        }

        return ordered;
    }

    private sealed class ProductivityCountCardPosBranchRow
    {
        public int Id { get; set; }
        public int? ParentProductId { get; set; }

        public string ProductName { get; set; } = string.Empty;

        public decimal CurrentPeriodBranchValue { get; set; }
        public decimal CurrentPeriodRegionAverageValue { get; set; }
        public decimal? CurrentPeriodRegionAverageValueDiff { get; set; }
        public decimal CurrentPeriodBankAverageValue { get; set; }
        public decimal? CurrentPeriodBankAverageValueDiff { get; set; }

        public decimal ThreeMonthHgBranchValue { get; set; }
        public decimal ThreeMonthHgRegionAverageValue { get; set; }
        public decimal? ThreeMonthHgRegionAverageValueDiff { get; set; }
        public decimal ThreeMonthHgBankAverageValue { get; set; }
        public decimal? ThreeMonthHgBankAverageValueDiff { get; set; }
    }

    private static List<GetProductivityCountCardPosBranchReportResponse.GetProductivityCountCardPosBranchReportItem> BuildProductivityCountCardPosBranchReportTree(DataSet ds, DateTime reportDate)
    {
        var t0 = ds.Tables[0];
        var hasParentInT0 = t0.Columns.Contains("ParentProductId");

        if (hasParentInT0)
        {
            var rows = DataTableHelper.ToList<ProductivityCountCardPosBranchRow>(t0);
            return ProductivityCountCardPosBranchReportTreeFromRows(rows);
        }

        if (ds.Tables.Count > 1 && ds.Tables[1].Columns.Contains("ParentProductId"))
        {
            var roots = DataTableHelper.ToList<ProductivityCountCardPosBranchRow>(ds.Tables[0]);
            var children = DataTableHelper.ToList<ProductivityCountCardPosBranchRow>(ds.Tables[1]);

            var all = new List<ProductivityCountCardPosBranchRow>(roots.Count + children.Count);
            all.AddRange(roots);
            all.AddRange(children);

            return ProductivityCountCardPosBranchReportTreeFromRows(all);
        }

        var flat = DataTableHelper.ToList<ProductivityCountCardPosBranchRow>(t0);
        return flat.Select(MapProductivityCountCardPosBranchReportItem).ToList();
    }

    private static List<GetProductivityCountCardPosBranchReportResponse.GetProductivityCountCardPosBranchReportItem> ProductivityCountCardPosBranchReportTreeFromRows(List<ProductivityCountCardPosBranchRow> rows)
    {
        var byId = rows
            .GroupBy(r => r.Id)
            .ToDictionary(g => g.Key, g => MapProductivityCountCardPosBranchReportItem(g.First()));

        foreach (var r in rows)
        {
            if (!byId.TryGetValue(r.Id, out var node))
            {
                node = MapProductivityCountCardPosBranchReportItem(r);
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

    private static GetProductivityCountCardPosBranchReportResponse.GetProductivityCountCardPosBranchReportItem MapProductivityCountCardPosBranchReportItem(ProductivityCountCardPosBranchRow r)
    {
        return new GetProductivityCountCardPosBranchReportResponse.GetProductivityCountCardPosBranchReportItem
        {
            Id = r.Id,
            ProductName = r.ProductName ?? string.Empty,
            CurrentPeriodBranchValue = r.CurrentPeriodBranchValue,
            CurrentPeriodRegionAverageValue = r.CurrentPeriodRegionAverageValue,
            CurrentPeriodRegionAverageValueDiff = r.CurrentPeriodRegionAverageValueDiff,
            CurrentPeriodBankAverageValue = r.CurrentPeriodBankAverageValue,
            CurrentPeriodBankAverageValueDiff = r.CurrentPeriodBankAverageValueDiff,
            ThreeMonthHgBranchValue = r.ThreeMonthHgBranchValue,
            ThreeMonthHgRegionAverageValue = r.ThreeMonthHgRegionAverageValue,
            ThreeMonthHgRegionAverageValueDiff = r.ThreeMonthHgRegionAverageValueDiff,
            ThreeMonthHgBankAverageValue = r.ThreeMonthHgBankAverageValue,
            ThreeMonthHgBankAverageValueDiff = r.ThreeMonthHgBankAverageValueDiff,
            SubProducts = new List<GetProductivityCountCardPosBranchReportResponse.GetProductivityCountCardPosBranchReportItem>()
        };
    }

    private static List<GetProductivityCountCardPosBranchReportResponse.GetProductivityCountCardPosBranchReportItem> SortProductivityCountCardPosBranchTree(
    List<GetProductivityCountCardPosBranchReportResponse.GetProductivityCountCardPosBranchReportItem> nodes,
    int? sortBy,
    bool isAscending)
    {
        Func<GetProductivityCountCardPosBranchReportResponse.GetProductivityCountCardPosBranchReportItem, object> keySelector = sortBy switch
        {
            1 => p => p.ProductName ?? string.Empty,
            2 => p => p.CurrentPeriodBranchValue,
            3 => p => p.CurrentPeriodRegionAverageValue,
            4 => p => p.CurrentPeriodBankAverageValue,
            5 => p => p.ThreeMonthHgBranchValue,
            6 => p => p.ThreeMonthHgRegionAverageValue,
            7 => p => p.ThreeMonthHgBankAverageValue,
            _ => p => p.Id
        };

        var ordered = (isAscending ? nodes.OrderBy(keySelector) : nodes.OrderByDescending(keySelector)).ToList();

        foreach (var n in ordered)
        {
            if (n.SubProducts != null && n.SubProducts.Count > 0)
                n.SubProducts = SortProductivityCountCardPosBranchTree(n.SubProducts, sortBy, isAscending);
        }

        return ordered;
    }

    private sealed class ProductivityCountCustomerBranchRow
    {
        public int Id { get; set; }
        public int? ParentProductId { get; set; }

        public string ProductName { get; set; } = string.Empty;

        public decimal RealizationBranchValue { get; set; }
        public decimal RealizationRegionAverageValue { get; set; }
        public decimal? RealizationRegionAverageValueDiff { get; set; }
        public decimal RealizationBankAverageValue { get; set; }
        public decimal? RealizationBankAverageValueDiff { get; set; }

        public decimal YtdNominalChangeBranchValue { get; set; }
        public decimal YtdNominalChangeRegionAverageValue { get; set; }
        public decimal? YtdNominalChangeRegionAverageValueDiff { get; set; }
        public decimal YtdNominalChangeBankAverageValue { get; set; }
        public decimal? YtdNominalChangeBankAverageValueDiff { get; set; }

        public decimal QtdNominalChangeBranchValue { get; set; }
        public decimal QtdNominalChangeRegionAverageValue { get; set; }
        public decimal? QtdNominalChangeRegionAverageValueDiff { get; set; }
        public decimal QtdNominalChangeBankAverageValue { get; set; }
        public decimal? QtdNominalChangeBankAverageValueDiff { get; set; }
    }

    private static List<GetProductivityCountCustomerBranchReportResponse.GetProductivityCountCustomerBranchReportItem> BuildProductivityCountCustomerBranchReportTree(DataSet ds, DateTime reportDate)
    {
        var t0 = ds.Tables[0];
        var hasParentInT0 = t0.Columns.Contains("ParentProductId");

        if (hasParentInT0)
        {
            var rows = DataTableHelper.ToList<ProductivityCountCustomerBranchRow>(t0);
            return ProductivityCountCustomerBranchReportTreeFromRows(rows);
        }

        if (ds.Tables.Count > 1 && ds.Tables[1].Columns.Contains("ParentProductId"))
        {
            var roots = DataTableHelper.ToList<ProductivityCountCustomerBranchRow>(ds.Tables[0]);
            var children = DataTableHelper.ToList<ProductivityCountCustomerBranchRow>(ds.Tables[1]);

            var all = new List<ProductivityCountCustomerBranchRow>(roots.Count + children.Count);
            all.AddRange(roots);
            all.AddRange(children);

            return ProductivityCountCustomerBranchReportTreeFromRows(all);
        }

        var flat = DataTableHelper.ToList<ProductivityCountCustomerBranchRow>(t0);
        return flat.Select(MapProductivityCountCustomerBranchReportItem).ToList();
    }

    private static List<GetProductivityCountCustomerBranchReportResponse.GetProductivityCountCustomerBranchReportItem> ProductivityCountCustomerBranchReportTreeFromRows(List<ProductivityCountCustomerBranchRow> rows)
    {
        var byId = rows
            .GroupBy(r => r.Id)
            .ToDictionary(g => g.Key, g => MapProductivityCountCustomerBranchReportItem(g.First()));

        foreach (var r in rows)
        {
            if (!byId.TryGetValue(r.Id, out var node))
            {
                node = MapProductivityCountCustomerBranchReportItem(r);
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

    private static GetProductivityCountCustomerBranchReportResponse.GetProductivityCountCustomerBranchReportItem MapProductivityCountCustomerBranchReportItem(ProductivityCountCustomerBranchRow r)
    {
        return new GetProductivityCountCustomerBranchReportResponse.GetProductivityCountCustomerBranchReportItem
        {
            Id = r.Id,
            ProductName = r.ProductName ?? string.Empty,

            RealizationBranchValue = r.RealizationBranchValue,
            RealizationRegionAverageValue = r.RealizationRegionAverageValue,
            RealizationRegionAverageValueDiff = r.RealizationRegionAverageValueDiff,
            RealizationBankAverageValue = r.RealizationBankAverageValue,
            RealizationBankAverageValueDiff = r.RealizationBankAverageValueDiff,

            YtdNominalChangeBranchValue = r.YtdNominalChangeBranchValue,
            YtdNominalChangeRegionAverageValue = r.YtdNominalChangeRegionAverageValue,
            YtdNominalChangeRegionAverageValueDiff = r.YtdNominalChangeRegionAverageValueDiff,
            YtdNominalChangeBankAverageValue = r.YtdNominalChangeBankAverageValue,
            YtdNominalChangeBankAverageValueDiff = r.YtdNominalChangeBankAverageValueDiff,

            QtdNominalChangeBranchValue = r.QtdNominalChangeBranchValue,
            QtdNominalChangeRegionAverageValue = r.QtdNominalChangeRegionAverageValue,
            QtdNominalChangeRegionAverageValueDiff = r.QtdNominalChangeRegionAverageValueDiff,
            QtdNominalChangeBankAverageValue = r.QtdNominalChangeBankAverageValue,
            QtdNominalChangeBankAverageValueDiff = r.QtdNominalChangeBankAverageValueDiff,

            SubProducts = new List<GetProductivityCountCustomerBranchReportResponse.GetProductivityCountCustomerBranchReportItem>()
        };
    }

    private static List<GetProductivityCountCustomerBranchReportResponse.GetProductivityCountCustomerBranchReportItem> SortProductivityCountCustomerBranchTree( List<GetProductivityCountCustomerBranchReportResponse.GetProductivityCountCustomerBranchReportItem> nodes, int? sortBy, bool isAscending)
    {
        Func<GetProductivityCountCustomerBranchReportResponse.GetProductivityCountCustomerBranchReportItem, object> keySelector = sortBy switch
        {
            1 => p => p.ProductName ?? string.Empty,
            2 => p => p.RealizationBranchValue,
            3 => p => p.RealizationRegionAverageValue,
            4 => p => p.RealizationBankAverageValue,
            5 => p => p.YtdNominalChangeBranchValue,
            6 => p => p.YtdNominalChangeRegionAverageValue,
            7 => p => p.YtdNominalChangeBankAverageValue,
            8 => p => p.QtdNominalChangeBranchValue,
            9 => p => p.QtdNominalChangeRegionAverageValue,
            10 => p => p.QtdNominalChangeBankAverageValue,
            _ => p => p.Id
        };

        var ordered = (isAscending ? nodes.OrderBy(keySelector) : nodes.OrderByDescending(keySelector)).ToList();

        foreach (var n in ordered)
        {
            if (n.SubProducts != null && n.SubProducts.Count > 0)
                n.SubProducts = SortProductivityCountCustomerBranchTree(n.SubProducts, sortBy, isAscending);
        }

        return ordered;
    }

    private sealed class ProductivityProfitRatioBranchRow
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

    private static List<GetProductivityProfitRatioBranchReportResponse.GetProductivityProfitRatioBranchReportItem> BuildProductivityProfitRatioBranchReportTree(DataSet ds, DateTime reportDate)
    {
        var t0 = ds.Tables[0];
        var hasParentInT0 = t0.Columns.Contains("ParentProductId");

        if (hasParentInT0)
        {
            var rows = DataTableHelper.ToList<ProductivityProfitRatioBranchRow>(t0);
            return ProductivityProfitRatioBranchReportTreeFromRows(rows);
        }

        if (ds.Tables.Count > 1 && ds.Tables[1].Columns.Contains("ParentProductId"))
        {
            var roots = DataTableHelper.ToList<ProductivityProfitRatioBranchRow>(ds.Tables[0]);
            var children = DataTableHelper.ToList<ProductivityProfitRatioBranchRow>(ds.Tables[1]);

            var all = new List<ProductivityProfitRatioBranchRow>(roots.Count + children.Count);
            all.AddRange(roots);
            all.AddRange(children);

            return ProductivityProfitRatioBranchReportTreeFromRows(all);
        }

        var flat = DataTableHelper.ToList<ProductivityProfitRatioBranchRow>(t0);
        return flat.Select(MapProductivityProfitRatioBranchReportItem).ToList();
    }

    private static List<GetProductivityProfitRatioBranchReportResponse.GetProductivityProfitRatioBranchReportItem> ProductivityProfitRatioBranchReportTreeFromRows(List<ProductivityProfitRatioBranchRow> rows)
    {
        var byId = rows
            .GroupBy(r => r.Id)
            .ToDictionary(g => g.Key, g => MapProductivityProfitRatioBranchReportItem(g.First()));

        foreach (var r in rows)
        {
            if (!byId.TryGetValue(r.Id, out var node))
            {
                node = MapProductivityProfitRatioBranchReportItem(r);
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

    private static GetProductivityProfitRatioBranchReportResponse.GetProductivityProfitRatioBranchReportItem MapProductivityProfitRatioBranchReportItem(ProductivityProfitRatioBranchRow r)
    {
        return new GetProductivityProfitRatioBranchReportResponse.GetProductivityProfitRatioBranchReportItem
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
            SubProducts = new List<GetProductivityProfitRatioBranchReportResponse.GetProductivityProfitRatioBranchReportItem>()
        };
    }

    private static List<GetProductivityProfitRatioBranchReportResponse.GetProductivityProfitRatioBranchReportItem> SortProductivityProfitRatioBranchTree(
    List<GetProductivityProfitRatioBranchReportResponse.GetProductivityProfitRatioBranchReportItem> nodes,
    int? sortBy,
    bool isAscending)
    {
        Func<GetProductivityProfitRatioBranchReportResponse.GetProductivityProfitRatioBranchReportItem, object> keySelector = sortBy switch
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
                n.SubProducts = SortProductivityProfitRatioBranchTree(n.SubProducts, sortBy, isAscending);
        }

        return ordered;
    }

    private sealed class ProductivityProfitTotalBranchRow
    {
        public int Id { get; set; }
        public int? ParentProductId { get; set; }

        public string Description { get; set; } = string.Empty;

        public decimal TargetValue { get; set; }

        public decimal RealizationBranchValue { get; set; }
        public decimal RealizationRegionAverageValue { get; set; }
        public decimal? RealizationRegionAverageValueDiff { get; set; }
        public decimal RealizationBankAverageValue { get; set; }
        public decimal? RealizationBankAverageValueDiff { get; set; }

        public decimal HgBranchValue { get; set; }
        public decimal? HgBranchValueDiff { get; set; }
        public decimal HgRegionAverageValue { get; set; }
        public decimal? HgRegionAverageValueDiff { get; set; }
        public decimal HgBankAverageValue { get; set; }
        public decimal? HgBankAverageValueDiff { get; set; }

        public decimal RetailValue { get; set; }
        public decimal KobiValue { get; set; }
        public decimal AgricultureValue { get; set; }
        public decimal CommercialValue { get; set; }
        public decimal? CommercialValueDiff { get; set; }
        public decimal PartnerValue { get; set; }
    }

    private static List<GetProductivityProfitTotalBranchReportResponse.GetProductivityProfitTotalBranchReportItem> BuildProductivityProfitTotalBranchReportTree(DataSet ds, DateTime reportDate)
    {
        var t0 = ds.Tables[0];
        var hasParentInT0 = t0.Columns.Contains("ParentProductId");

        if (hasParentInT0)
        {
            var rows = DataTableHelper.ToList<ProductivityProfitTotalBranchRow>(t0);
            return ProductivityProfitTotalBranchReportTreeFromRows(rows);
        }

        if (ds.Tables.Count > 1 && ds.Tables[1].Columns.Contains("ParentProductId"))
        {
            var roots = DataTableHelper.ToList<ProductivityProfitTotalBranchRow>(ds.Tables[0]);
            var children = DataTableHelper.ToList<ProductivityProfitTotalBranchRow>(ds.Tables[1]);

            var all = new List<ProductivityProfitTotalBranchRow>(roots.Count + children.Count);
            all.AddRange(roots);
            all.AddRange(children);

            return ProductivityProfitTotalBranchReportTreeFromRows(all);
        }

        var flat = DataTableHelper.ToList<ProductivityProfitTotalBranchRow>(t0);
        return flat.Select(MapProductivityProfitTotalBranchReportItem).ToList();
    }

    private static List<GetProductivityProfitTotalBranchReportResponse.GetProductivityProfitTotalBranchReportItem> ProductivityProfitTotalBranchReportTreeFromRows(List<ProductivityProfitTotalBranchRow> rows)
    {
        var byId = rows
            .GroupBy(r => r.Id)
            .ToDictionary(g => g.Key, g => MapProductivityProfitTotalBranchReportItem(g.First()));

        foreach (var r in rows)
        {
            if (!byId.TryGetValue(r.Id, out var node))
            {
                node = MapProductivityProfitTotalBranchReportItem(r);
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

    private static GetProductivityProfitTotalBranchReportResponse.GetProductivityProfitTotalBranchReportItem MapProductivityProfitTotalBranchReportItem(ProductivityProfitTotalBranchRow r)
    {
        return new GetProductivityProfitTotalBranchReportResponse.GetProductivityProfitTotalBranchReportItem
        {
            Id = r.Id,
            Description = r.Description ?? string.Empty,
            TargetValue = r.TargetValue,

            RealizationBranchValue = r.RealizationBranchValue,
            RealizationRegionAverageValue = r.RealizationRegionAverageValue,
            RealizationRegionAverageValueDiff = r.RealizationRegionAverageValueDiff,
            RealizationBankAverageValue = r.RealizationBankAverageValue,
            RealizationBankAverageValueDiff = r.RealizationBankAverageValueDiff,

            HgBranchValue = r.HgBranchValue,
            HgBranchValueDiff = r.HgBranchValueDiff,
            HgRegionAverageValue = r.HgRegionAverageValue,
            HgRegionAverageValueDiff = r.HgRegionAverageValueDiff,
            HgBankAverageValue = r.HgBankAverageValue,
            HgBankAverageValueDiff = r.HgBankAverageValueDiff,

            RetailValue = r.RetailValue,
            KobiValue = r.KobiValue,
            AgricultureValue = r.AgricultureValue,
            CommercialValue = r.CommercialValue,
            CommercialValueDiff = r.CommercialValueDiff,
            PartnerValue = r.PartnerValue,

            SubProducts = new List<GetProductivityProfitTotalBranchReportResponse.GetProductivityProfitTotalBranchReportItem>()
        };
    }

    private static List<GetProductivityProfitTotalBranchReportResponse.GetProductivityProfitTotalBranchReportItem> SortProductivityProfitTotalBranchTree( List<GetProductivityProfitTotalBranchReportResponse.GetProductivityProfitTotalBranchReportItem> nodes, int? sortBy, bool isAscending)
    {
        Func<GetProductivityProfitTotalBranchReportResponse.GetProductivityProfitTotalBranchReportItem, object> keySelector = sortBy switch
        {
            1 => p => p.Description ?? string.Empty,
            2 => p => p.TargetValue,
            3 => p => p.RealizationBranchValue,
            4 => p => p.RealizationRegionAverageValue,
            5 => p => p.RealizationBankAverageValue,
            6 => p => p.HgBranchValue,
            7 => p => p.HgRegionAverageValue,
            8 => p => p.HgBankAverageValue,
            9 => p => p.RetailValue,
            10 => p => p.KobiValue,
            11 => p => p.AgricultureValue,
            12 => p => p.CommercialValue,
            13 => p => p.PartnerValue,
            _ => p => p.Id
        };

        var ordered = (isAscending ? nodes.OrderBy(keySelector) : nodes.OrderByDescending(keySelector)).ToList();

        foreach (var n in ordered)
        {
            if (n.SubProducts != null && n.SubProducts.Count > 0)
                n.SubProducts = SortProductivityProfitTotalBranchTree(n.SubProducts, sortBy, isAscending);
        }

        return ordered;
    }

    private sealed class ProductivityVolumeBranchRow
    {
        public int Id { get; set; }
        public int? ParentProductId { get; set; }

        public string ProductName { get; set; } = string.Empty;

        public decimal RealizationBranchValue { get; set; }
        public decimal RealizationRegionAverageValue { get; set; }
        public decimal? RealizationRegionAverageValueDiff { get; set; }
        public decimal RealizationBankAverageValue { get; set; }
        public decimal? RealizationBankAverageValueDiff { get; set; }

        public decimal TargetValue { get; set; }
        public decimal HgRate { get; set; }

        public decimal NetGrowthBranchValue { get; set; }
        public decimal NetGrowthRegionAverageValue { get; set; }
        public decimal? NetGrowthRegionAverageValueDiff { get; set; }
        public decimal NetGrowthBankAverageValue { get; set; }
        public decimal? NetGrowthBankAverageValueDiff { get; set; }

        public decimal YtdBranchValue { get; set; }
        public decimal YtdRegionValue { get; set; }
        public decimal? YtdRegionValueDiff { get; set; }
        public decimal YtdBankValue { get; set; }
        public decimal? YtdBankValueDiff { get; set; }

        public decimal QtdBranchValue { get; set; }
        public decimal QtdRegionValue { get; set; }
        public decimal? QtdRegionValueDiff { get; set; }
        public decimal QtdBankValue { get; set; }
        public decimal? QtdBankValueDiff { get; set; }
    }

    private static List<GetProductivityVolumeBranchReportResponse.GetProductivityVolumeBranchReportItem> BuildProductivityVolumeBranchReportTree(DataSet ds, DateTime reportDate)
    {
        var t0 = ds.Tables[0];
        var hasParentInT0 = t0.Columns.Contains("ParentProductId");

        if (hasParentInT0)
        {
            var rows = DataTableHelper.ToList<ProductivityVolumeBranchRow>(t0);
            return ProductivityVolumeBranchReportTreeFromRows(rows);
        }

        if (ds.Tables.Count > 1 && ds.Tables[1].Columns.Contains("ParentProductId"))
        {
            var roots = DataTableHelper.ToList<ProductivityVolumeBranchRow>(ds.Tables[0]);
            var children = DataTableHelper.ToList<ProductivityVolumeBranchRow>(ds.Tables[1]);

            var all = new List<ProductivityVolumeBranchRow>(roots.Count + children.Count);
            all.AddRange(roots);
            all.AddRange(children);

            return ProductivityVolumeBranchReportTreeFromRows(all);
        }

        var flat = DataTableHelper.ToList<ProductivityVolumeBranchRow>(t0);
        return flat.Select(MapProductivityVolumeBranchReportItem).ToList();
    }

    private static List<GetProductivityVolumeBranchReportResponse.GetProductivityVolumeBranchReportItem> ProductivityVolumeBranchReportTreeFromRows(List<ProductivityVolumeBranchRow> rows)
    {
        var byId = rows
            .GroupBy(r => r.Id)
            .ToDictionary(g => g.Key, g => MapProductivityVolumeBranchReportItem(g.First()));

        foreach (var r in rows)
        {
            if (!byId.TryGetValue(r.Id, out var node))
            {
                node = MapProductivityVolumeBranchReportItem(r);
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

    private static GetProductivityVolumeBranchReportResponse.GetProductivityVolumeBranchReportItem MapProductivityVolumeBranchReportItem(ProductivityVolumeBranchRow r)
    {
        return new GetProductivityVolumeBranchReportResponse.GetProductivityVolumeBranchReportItem
        {
            Id = r.Id,
            ProductName = r.ProductName ?? string.Empty,

            RealizationBranchValue = r.RealizationBranchValue,
            RealizationRegionAverageValue = r.RealizationRegionAverageValue,
            RealizationRegionAverageValueDiff = r.RealizationRegionAverageValueDiff,
            RealizationBankAverageValue = r.RealizationBankAverageValue,
            RealizationBankAverageValueDiff = r.RealizationBankAverageValueDiff,

            TargetValue = r.TargetValue,
            HgRate = r.HgRate,

            NetGrowthBranchValue = r.NetGrowthBranchValue,
            NetGrowthRegionAverageValue = r.NetGrowthRegionAverageValue,
            NetGrowthRegionAverageValueDiff = r.NetGrowthRegionAverageValueDiff,
            NetGrowthBankAverageValue = r.NetGrowthBankAverageValue,
            NetGrowthBankAverageValueDiff = r.NetGrowthBankAverageValueDiff,

            YtdBranchValue = r.YtdBranchValue,
            YtdRegionValue = r.YtdRegionValue,
            YtdRegionValueDiff = r.YtdRegionValueDiff,
            YtdBankValue = r.YtdBankValue,
            YtdBankValueDiff = r.YtdBankValueDiff,

            QtdBranchValue = r.QtdBranchValue,
            QtdRegionValue = r.QtdRegionValue,
            QtdRegionValueDiff = r.QtdRegionValueDiff,
            QtdBankValue = r.QtdBankValue,
            QtdBankValueDiff = r.QtdBankValueDiff,

            SubProducts = new List<GetProductivityVolumeBranchReportResponse.GetProductivityVolumeBranchReportItem>()
        };
    }

    private static List<GetProductivityVolumeBranchReportResponse.GetProductivityVolumeBranchReportItem> SortProductivityVolumeBranchTree( List<GetProductivityVolumeBranchReportResponse.GetProductivityVolumeBranchReportItem> nodes, int? sortBy, bool isAscending)
    {
        Func<GetProductivityVolumeBranchReportResponse.GetProductivityVolumeBranchReportItem, object> keySelector = sortBy switch
        {
            1 => p => p.ProductName ?? string.Empty,
            2 => p => p.RealizationBranchValue,
            3 => p => p.RealizationRegionAverageValue,
            4 => p => p.RealizationBankAverageValue,
            5 => p => p.TargetValue,
            6 => p => p.HgRate,
            7 => p => p.NetGrowthBranchValue,
            8 => p => p.NetGrowthRegionAverageValue,
            9 => p => p.NetGrowthBankAverageValue,
            10 => p => p.YtdBranchValue,
            11 => p => p.YtdRegionValue,
            12 => p => p.YtdBankValue,
            13 => p => p.QtdBranchValue,
            14 => p => p.QtdRegionValue,
            15 => p => p.QtdBankValue,
            _ => p => p.Id
        };

        var ordered = (isAscending ? nodes.OrderBy(keySelector) : nodes.OrderByDescending(keySelector)).ToList();

        foreach (var n in ordered)
        {
            if (n.SubProducts != null && n.SubProducts.Count > 0)
                n.SubProducts = SortProductivityVolumeBranchTree(n.SubProducts, sortBy, isAscending);
        }

        return ordered;
    }

    private sealed class ProductivityProfitSpreadManagementBranchRow
    {
        public int Id { get; set; }
        public int? ParentProductId { get; set; }

        public string Description { get; set; } = string.Empty;

        public decimal SpreadValue { get; set; }

        public decimal RatioBranchValue { get; set; }
        public decimal RatioRegionAverageValue { get; set; }
        public decimal? RatioRegionAverageValueDiff { get; set; }
        public decimal RatioBankAverageValue { get; set; }
        public decimal? RatioBankAverageValueDiff { get; set; }

        public decimal NetReturnBranchValue { get; set; }
        public decimal NetReturnRegionAverageValue { get; set; }
        public decimal? NetReturnRegionAverageValueDiff { get; set; }
        public decimal NetReturnBankAverageValue { get; set; }
        public decimal? NetReturnBankAverageValueDiff { get; set; }

        public decimal NetReturnHgBranchValue { get; set; }
        public decimal NetReturnHgRegionAverageValue { get; set; }
        public decimal? NetReturnHgRegionAverageValueDiff { get; set; }
        public decimal NetReturnHgBankAverageValue { get; set; }
        public decimal? NetReturnHgBankAverageValueDiff { get; set; }
    }

    private static List<GetProductivityProfitSpreadManagementBranchReportResponse.GetProductivityProfitSpreadManagementBranchReportItem> BuildProductivityProfitSpreadManagementBranchReportTree(DataSet ds, DateTime reportDate)
    {
        var t0 = ds.Tables[0];
        var hasParentInT0 = t0.Columns.Contains("ParentProductId");

        if (hasParentInT0)
        {
            var rows = DataTableHelper.ToList<ProductivityProfitSpreadManagementBranchRow>(t0);
            return ProductivityProfitSpreadManagementBranchReportTreeFromRows(rows);
        }

        if (ds.Tables.Count > 1 && ds.Tables[1].Columns.Contains("ParentProductId"))
        {
            var roots = DataTableHelper.ToList<ProductivityProfitSpreadManagementBranchRow>(ds.Tables[0]);
            var children = DataTableHelper.ToList<ProductivityProfitSpreadManagementBranchRow>(ds.Tables[1]);

            var all = new List<ProductivityProfitSpreadManagementBranchRow>(roots.Count + children.Count);
            all.AddRange(roots);
            all.AddRange(children);

            return ProductivityProfitSpreadManagementBranchReportTreeFromRows(all);
        }

        var flat = DataTableHelper.ToList<ProductivityProfitSpreadManagementBranchRow>(t0);
        return flat.Select(MapProductivityProfitSpreadManagementBranchReportItem).ToList();
    }

    private static List<GetProductivityProfitSpreadManagementBranchReportResponse.GetProductivityProfitSpreadManagementBranchReportItem> ProductivityProfitSpreadManagementBranchReportTreeFromRows(List<ProductivityProfitSpreadManagementBranchRow> rows)
    {
        var byId = rows
            .GroupBy(r => r.Id)
            .ToDictionary(g => g.Key, g => MapProductivityProfitSpreadManagementBranchReportItem(g.First()));

        foreach (var r in rows)
        {
            if (!byId.TryGetValue(r.Id, out var node))
            {
                node = MapProductivityProfitSpreadManagementBranchReportItem(r);
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

    private static GetProductivityProfitSpreadManagementBranchReportResponse.GetProductivityProfitSpreadManagementBranchReportItem MapProductivityProfitSpreadManagementBranchReportItem(ProductivityProfitSpreadManagementBranchRow r)
    {
        return new GetProductivityProfitSpreadManagementBranchReportResponse.GetProductivityProfitSpreadManagementBranchReportItem
        {
            Id = r.Id,
            Description = r.Description ?? string.Empty,
            SpreadValue = r.SpreadValue,

            RatioBranchValue = r.RatioBranchValue,
            RatioRegionAverageValue = r.RatioRegionAverageValue,
            RatioRegionAverageValueDiff = r.RatioRegionAverageValueDiff,
            RatioBankAverageValue = r.RatioBankAverageValue,
            RatioBankAverageValueDiff = r.RatioBankAverageValueDiff,

            NetReturnBranchValue = r.NetReturnBranchValue,
            NetReturnRegionAverageValue = r.NetReturnRegionAverageValue,
            NetReturnRegionAverageValueDiff = r.NetReturnRegionAverageValueDiff,
            NetReturnBankAverageValue = r.NetReturnBankAverageValue,
            NetReturnBankAverageValueDiff = r.NetReturnBankAverageValueDiff,

            NetReturnHgBranchValue = r.NetReturnHgBranchValue,
            NetReturnHgRegionAverageValue = r.NetReturnHgRegionAverageValue,
            NetReturnHgRegionAverageValueDiff = r.NetReturnHgRegionAverageValueDiff,
            NetReturnHgBankAverageValue = r.NetReturnHgBankAverageValue,
            NetReturnHgBankAverageValueDiff = r.NetReturnHgBankAverageValueDiff,

            SubProducts = new List<GetProductivityProfitSpreadManagementBranchReportResponse.GetProductivityProfitSpreadManagementBranchReportItem>()
        };
    }

    private static List<GetProductivityProfitSpreadManagementBranchReportResponse.GetProductivityProfitSpreadManagementBranchReportItem> SortProductivityProfitSpreadManagementBranchTree( List<GetProductivityProfitSpreadManagementBranchReportResponse.GetProductivityProfitSpreadManagementBranchReportItem> nodes, int? sortBy, bool isAscending)
    {
        Func<GetProductivityProfitSpreadManagementBranchReportResponse.GetProductivityProfitSpreadManagementBranchReportItem, object> keySelector = sortBy switch
        {
            1 => p => p.Description ?? string.Empty,
            2 => p => p.SpreadValue,
            3 => p => p.RatioBranchValue,
            4 => p => p.RatioRegionAverageValue,
            5 => p => p.RatioBankAverageValue,
            6 => p => p.NetReturnBranchValue,
            7 => p => p.NetReturnRegionAverageValue,
            8 => p => p.NetReturnBankAverageValue,
            9 => p => p.NetReturnHgBranchValue,
            10 => p => p.NetReturnHgRegionAverageValue,
            11 => p => p.NetReturnHgBankAverageValue,
            _ => p => p.Id
        };

        var ordered = (isAscending ? nodes.OrderBy(keySelector) : nodes.OrderByDescending(keySelector)).ToList();

        foreach (var n in ordered)
        {
            if (n.SubProducts != null && n.SubProducts.Count > 0)
                n.SubProducts = SortProductivityProfitSpreadManagementBranchTree(n.SubProducts, sortBy, isAscending);
        }

        return ordered;
    }

    #endregion
}
