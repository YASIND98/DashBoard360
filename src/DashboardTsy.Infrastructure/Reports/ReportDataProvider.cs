using System.Data;
using System;
using System.Collections.Generic;
using System.Linq;
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

    public IReadOnlyList<GetProductivityCountCustomerRegionReportItem> GetProductivityCountCustomerRegionReport(GetProductivityCountCustomerRegionReportRequest request)
    {
        request ??= new GetProductivityCountCustomerRegionReportRequest();

        if (MockEnabled)
            return MockProductivityReportData.GetProductivityCountCustomerRegionReport(request);

        return MockProductivityReportData.GetProductivityCountCustomerRegionReport(request);
    }

    public IReadOnlyList<GetProductivityVolumeRegionReportItem> GetProductivityVolumeRegionReport(GetProductivityVolumeRegionReportRequest request)
    {
        request ??= new GetProductivityVolumeRegionReportRequest();

        if (MockEnabled)
            return MockProductivityReportData.GetProductivityVolumeRegionReport(request);

        return MockProductivityReportData.GetProductivityVolumeRegionReport(request);
    }

    public IReadOnlyList<GetProductivityProfitRatioRegionReportItem> GetProductivityProfitRatioRegionReport(GetProductivityProfitRatioRegionReportRequest request)
    {
        request ??= new GetProductivityProfitRatioRegionReportRequest();

        if (MockEnabled)
            return MockProductivityReportData.GetProductivityProfitRatioRegionReport(request);

        return MockProductivityReportData.GetProductivityProfitRatioRegionReport(request);
    }

    public IReadOnlyList<GetProductivityProfitTotalRegionReportItem> GetProductivityProfitTotalRegionReport(GetProductivityProfitTotalRegionReportRequest request)
    {
        request ??= new GetProductivityProfitTotalRegionReportRequest();

        if (MockEnabled)
            return MockProductivityReportData.GetProductivityProfitTotalRegionReport(request);

        return MockProductivityReportData.GetProductivityProfitTotalRegionReport(request);
    }

    public IReadOnlyList<GetProductivityProfitSpreadManagementRegionReportItem> GetProductivityProfitSpreadManagementRegionReport(GetProductivityProfitSpreadManagementRegionReportRequest request)
    {
        request ??= new GetProductivityProfitSpreadManagementRegionReportRequest();

        if (MockEnabled)
            return MockProductivityReportData.GetProductivityProfitSpreadManagementRegionReport(request);

        return MockProductivityReportData.GetProductivityProfitSpreadManagementRegionReport(request);
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

    public GetProductivityRegionScoreCardReportItem GetProductivityRegionScoreCardReport(GetProductivityRegionScoreCardReportRequest request)
    {
        request ??= new GetProductivityRegionScoreCardReportRequest();

        if (MockEnabled)
            return MockProductivityReportData.GetProductivityRegionScoreCardReport(request);

        return MockProductivityReportData.GetProductivityRegionScoreCardReport(request);
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
}
