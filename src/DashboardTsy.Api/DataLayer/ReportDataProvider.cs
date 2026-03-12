using System.Data;
using System.Linq;
using DashboardTsy.Api.Models.TargetReport;
using DashboardTsy.Api.Services;
using Microsoft.Extensions.Configuration;

namespace DashboardTsy.Api.DataLayer;

/// <summary>
/// DataLayer pattern from DBRapor: calls stored procedures via IStoredProcedureExecutor.
/// Use connection keys from config (e.g. Main, SubeDashboard, PMO). Add more methods per SP.
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

    private bool MockEnabled => _configuration.GetValue<bool>("ReportMock:Enabled");

    /// <summary>
    /// Example: execute sp_GetRaporTarihi on Main (or SubeDashboard if you use that DB).
    /// Configure DbConnectionStrings in appsettings and ensure the SP exists in that database.
    /// </summary>
    public DataSet GetRaporTarihi()
    {
        return _spExecutor.ExecuteDataSet("Main", "sp_GetRaporTarihi");
    }

    /// <summary>
    /// Example with parameters (mirrors DBRapor SubeDataProvider.GetNavBarNPSGelisimi).
    /// Uncomment and adjust SP name + connection key when you have the procedure.
    /// </summary>
    // public DataSet GetNavBarNPSGelisimi(int? sube, int? bolge, int? iskolu)
    // {
    //     var parameters = new Dictionary<string, object?>();
    //     if (sube.HasValue && sube != 0) parameters["@SUBE_KODU"] = sube;
    //     if (bolge.HasValue && bolge != 0) parameters["@BOLGE_KODU"] = bolge;
    //     if (iskolu.HasValue && iskolu != 0) parameters["@IS_KOLU"] = iskolu;
    //     return _spExecutor.ExecuteDataSet("SubeDashboard", "sp_GetNavBarNPSGelisimi", parameters);
    // }

    /// <summary>
    /// EXEC [dbo].[SP_RP_GetTargetReportMenuTexts] @SessionId = ''
    /// </summary>
    public GetTargetReportMenuTextsResponse? GetTargetReportMenuTexts(string sessionId)
    {
        if (MockEnabled)
            return MockTargetReportData.GetMenuTexts();

        var parameters = new Dictionary<string, object?>
        {
            ["@SessionId"] = sessionId ?? string.Empty
        };
        var ds = _spExecutor.ExecuteDataSet("Main", "SP_RP_GetTargetReportMenuTexts", parameters);
        if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
            return null;
        return DataTableHelper.ToObject<GetTargetReportMenuTextsResponse>(ds.Tables[0].Rows[0]);
    }

    /// <summary>
    /// EXEC dbo.SP_RP_GetTargetReportFilters @SessionId='', @FilterId=0, @FilterCode=NULL
    /// </summary>
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
        var ds = _spExecutor.ExecuteDataSet("Main", "SP_RP_GetTargetReportFilters", parameters);
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

        var ds = _spExecutor.ExecuteDataSet("Main", "SP_RP_GetDailyTargetReport", parameters);
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

    public GetDailyTargetReportTableHeadersResponse? GetDailyTargetReportTableHeaders(string sessionId)
    {
        if (MockEnabled)
            return MockTargetReportData.GetDailyHeaders(DateTime.Today);

        var parameters = new Dictionary<string, object?>
        {
            ["@SessionId"] = sessionId ?? string.Empty
        };

        var ds = _spExecutor.ExecuteDataSet("Main", "SP_RP_GetDailyTargetReportTableHeaders", parameters);
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

        var ds = _spExecutor.ExecuteDataSet("Main", "SP_RP_GetMonthlyTargetReport", parameters);
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

        var ds = _spExecutor.ExecuteDataSet("Main", "SP_RP_GetMonthlyTargetReportTableHeaders", parameters);
        if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
            return null;

        return DataTableHelper.ToObject<GetMonthlyTargetReportTableHeadersResponse>(ds.Tables[0].Rows[0]);
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
            MonthActualAmount = r.MonthActualAmount,
            MonthTargetAmount = r.MonthTargetAmount,
            MonthRatio = r.MonthRatio,
            YearActualAmount = r.YearActualAmount,
            YearTargetAmount = r.YearTargetAmount,
            YearRatio = r.YearRatio,
            SubProducts = new List<GetMonthlyTargetReportResponse.Product>()
        };
    }

    private static List<GetMonthlyTargetReportResponse.Product> FilterMonthlyTreeByName(
        List<GetMonthlyTargetReportResponse.Product> nodes,
        string searchText)
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

    private static List<GetMonthlyTargetReportResponse.Product> SortMonthlyTree(
        List<GetMonthlyTargetReportResponse.Product> nodes,
        int? sortBy,
        bool isAscending)
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
        // Strategy:
        // - Prefer a single table with ParentProductId (hierarchical in one result set)
        // - Otherwise, if 2 tables exist and table[1] has ParentProductId, treat table[0]=roots, table[1]=children.
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

            // Ensure roots have null parent, children have parent set (SP responsibility).
            var all = new List<DailyTargetRow>(roots.Count + children.Count);
            all.AddRange(roots);
            all.AddRange(children);
            return TreeFromRows(all);
        }

        // Flat list fallback (no nesting info).
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

        // Ensure all nodes exist even if duplicated rows happen.
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

        // Roots are those without a resolvable parent.
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

    private static List<GetDailyTargetReportResponse.Product> FilterTreeByName(
        List<GetDailyTargetReportResponse.Product> nodes,
        string searchText)
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

    private static List<GetDailyTargetReportResponse.Product> SortTree(
        List<GetDailyTargetReportResponse.Product> nodes,
        int? sortBy,
        bool isAscending)
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

        var ordered = (isAscending
                ? nodes.OrderBy(keySelector)
                : nodes.OrderByDescending(keySelector))
            .ToList();

        foreach (var n in ordered)
        {
            if (n.SubProducts != null && n.SubProducts.Count > 0)
                n.SubProducts = SortTree(n.SubProducts, sortBy, isAscending);
        }

        return ordered;
    }
}
