using System.Data;
using System.Linq;
using DashboardTsy.Application.TargetReport;
using DashboardTsy.Infrastructure.Data;
using Microsoft.Extensions.Configuration;

namespace DashboardTsy.Infrastructure.TargetReport;

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

    private bool MockEnabled => _configuration["ReportMock:Enabled"] is string v && bool.TryParse(v, out var b) && b;

    public DataSet GetRaporTarihi()
    {
        return _spExecutor.ExecuteDataSet("Main", "sp_GetRaporTarihi");
    }

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

    public IReadOnlyList<GetProductivityReportTabItem> GetProductivityReportTabs(GetProductivityReportTabsRequest request)
    {
        request ??= new GetProductivityReportTabsRequest();

        if (MockEnabled)
            return MockTargetReportData.GetProductivityReportTabs(request);

        // Gerçek SP ismi ve parametreleri netleştiğinde burası aktif edilecek.
        // Örnek taslak (SP adı sadece örnek):
        //
        // var parameters = new Dictionary<string, object?>
        // {
        //     ["@SessionId"] = request.SessionId ?? string.Empty,
        //     ["@FilterType"] = request.FilterType
        // };
        //
        // var ds = _spExecutor.ExecuteDataSet("Main", "SP_RP_GetProductivityReportTabs", parameters);
        // if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
        //     return Array.Empty<GetProductivityReportTabItem>();
        //
        // return DataTableHelper.ToList<GetProductivityReportTabItem>(ds.Tables[0]);

        // Şimdilik her durumda mock veri dönmeye devam ediyoruz.
        return MockTargetReportData.GetProductivityReportTabs(request);
    }

    public IReadOnlyList<GetProductivityReportTableHeaderItem> GetProductivityReportTableHeaders(GetProductivityReportTableHeadersRequest request)
    {
        // SP henüz belli değil; şu an sadece mock veriyi döndürüyoruz.
        request ??= new GetProductivityReportTableHeadersRequest();

        if (MockEnabled)
            return MockTargetReportData.GetProductivityReportTableHeaders(request);

        // Gerçek SP ismi ve parametreleri netleştiğinde burası güncellenecek.
        // Örnek taslak:
        // var parameters = new Dictionary<string, object?>
        // {
        //     ["@SessionId"] = request.SessionId ?? string.Empty,
        //     ["@MainTabId"] = request.MainTabId,
        //     ["@MidTabId"] = request.MidTabId ?? (object)DBNull.Value,
        //     ["@SubTabId"] = request.SubTabId ?? (object)DBNull.Value,
        //     ["@FilterType"] = request.FilterType,
        //     ["@ReportDate"] = request.ReportDate
        // };
        //
        // var ds = _spExecutor.ExecuteDataSet("Main", "SP_RP_GetProductivityReportTableHeaders", parameters);
        // if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
        //     return Array.Empty<GetProductivityReportTableHeaderItem>();
        //
        // return DataTableHelper.ToList<GetProductivityReportTableHeaderItem>(ds.Tables[0]);

        return MockTargetReportData.GetProductivityReportTableHeaders(request);
    }

    public IReadOnlyList<GetReportRegionFilterItem> GetReportRegionFilters(GetReportRegionFiltersRequest request)
    {
        request ??= new GetReportRegionFiltersRequest();

        if (MockEnabled)
            return MockTargetReportData.GetReportRegionFilters();

        // Gerçek SP ismi ve parametreleri netleştiğinde burası güncellenecek.
        // Örnek taslak (SP adı sadece örnek):
        //
        // var parameters = new Dictionary<string, object?>
        // {
        //     ["@SessionId"] = request.SessionId ?? string.Empty
        // };
        //
        // var ds = _spExecutor.ExecuteDataSet("Main", "SP_RP_GetReportRegionFilters", parameters);
        // if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
        //     return Array.Empty<GetReportRegionFilterItem>();
        //
        // return DataTableHelper.ToList<GetReportRegionFilterItem>(ds.Tables[0]);

        // Şimdilik SP tarafı hazır değilken de mock veri dön.
        return MockTargetReportData.GetReportRegionFilters();
    }

    public IReadOnlyList<GetReportBranchFilterItem> GetReportBranchFilters(GetReportBranchFiltersRequest request)
    {
        request ??= new GetReportBranchFiltersRequest();

        if (MockEnabled)
            return MockTargetReportData.GetReportBranchFilters();

        // Gerçek SP ismi ve parametreleri netleştiğinde burası güncellenecek.
        // Örnek taslak (SP adı sadece örnek):
        //
        // var parameters = new Dictionary<string, object?>
        // {
        //     ["@SessionId"] = request.SessionId ?? string.Empty
        // };
        //
        // var ds = _spExecutor.ExecuteDataSet("Main", "SP_RP_GetReportBranchFilters", parameters);
        // if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
        //     return Array.Empty<GetReportBranchFilterItem>();
        //
        // return DataTableHelper.ToList<GetReportBranchFilterItem>(ds.Tables[0]);

        // Şimdilik SP tarafı hazır değilken de mock veri dön.
        return MockTargetReportData.GetReportBranchFilters();
    }

    public IReadOnlyList<GetProductivityGeneralRegionReportItem> GetProductivityGeneralRegionReport(GetProductivityGeneralRegionReportRequest request)
    {
        request ??= new GetProductivityGeneralRegionReportRequest();

        if (MockEnabled)
            return MockTargetReportData.GetProductivityGeneralRegionReport(request);

        // Gerçek SP ismi ve parametreleri netleştiğinde burası güncellenecek.
        // Örnek taslak (SP adı sadece örnek):
        //
        // var parameters = new Dictionary<string, object?>
        // {
        //     ["@SessionId"] = request.SessionId ?? string.Empty,
        //     ["@RegionCode"] = string.IsNullOrWhiteSpace(request.RegionCode) ? (object)DBNull.Value : request.RegionCode,
        //     ["@ReportDate"] = request.ReportDate,
        //     ["@SortBy"] = request.SortBy ?? (object)DBNull.Value,
        //     ["@IsAscending"] = request.IsAscending
        // };
        //
        // var ds = _spExecutor.ExecuteDataSet("Main", "SP_RP_GetProductivityGeneralRegionReport", parameters);
        // if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
        //     return Array.Empty<GetProductivityGeneralRegionReportItem>();
        //
        // return DataTableHelper.ToList<GetProductivityGeneralRegionReportItem>(ds.Tables[0]);

        // Şimdilik SP tarafı hazır değilken de mock veri dön.
        return MockTargetReportData.GetProductivityGeneralRegionReport(request);
    }

    public IReadOnlyList<GetProductivityCountCardPosRegionReportItem> GetProductivityCountCardPosRegionReport(GetProductivityCountCardPosRegionReportRequest request)
    {
        request ??= new GetProductivityCountCardPosRegionReportRequest();

        if (MockEnabled)
            return MockTargetReportData.GetProductivityCountCardPosRegionReport(request);

        // Gerçek SP ismi ve parametreleri netleştiğinde burası güncellenecek.
        // Örnek taslak (SP adı sadece örnek):
        //
        // var parameters = new Dictionary<string, object?>
        // {
        //     ["@SessionId"] = request.SessionId ?? string.Empty,
        //     ["@RegionCode"] = request.RegionCode,
        //     ["@TabId"] = request.TabId,
        //     ["@ReportDate"] = request.ReportDate,
        //     ["@SortBy"] = request.SortBy ?? (object)DBNull.Value,
        //     ["@IsAscending"] = request.IsAscending
        // };
        //
        // var ds = _spExecutor.ExecuteDataSet("Main", "SP_RP_GetProductivityCountCardPosRegionReport", parameters);
        // if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
        //     return Array.Empty<GetProductivityCountCardPosRegionReportItem>();
        //
        // return DataTableHelper.ToList<GetProductivityCountCardPosRegionReportItem>(ds.Tables[0]);

        // Şimdilik SP tarafı hazır değilken de mock veri dön.
        return MockTargetReportData.GetProductivityCountCardPosRegionReport(request);
    }

    public IReadOnlyList<GetProductivityCountCustomerRegionReportItem> GetProductivityCountCustomerRegionReport(GetProductivityCountCustomerRegionReportRequest request)
    {
        request ??= new GetProductivityCountCustomerRegionReportRequest();

        if (MockEnabled)
            return MockTargetReportData.GetProductivityCountCustomerRegionReport(request);

        // Gerçek SP ismi ve parametreleri netleştiğinde burası güncellenecek.
        // Örnek taslak (SP adı sadece örnek):
        //
        // var parameters = new Dictionary<string, object?>
        // {
        //     ["@SessionId"] = request.SessionId ?? string.Empty,
        //     ["@RegionCode"] = request.RegionCode,
        //     ["@SubTabId"] = request.SubTabId,
        //     ["@ReportDate"] = request.ReportDate,
        //     ["@SortBy"] = request.SortBy ?? (object)DBNull.Value,
        //     ["@IsAscending"] = request.IsAscending
        // };
        //
        // var ds = _spExecutor.ExecuteDataSet("Main", "SP_RP_GetProductivityCountCustomerRegionReport", parameters);
        // if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
        //     return Array.Empty<GetProductivityCountCustomerRegionReportItem>();
        //
        // return DataTableHelper.ToList<GetProductivityCountCustomerRegionReportItem>(ds.Tables[0]);

        return MockTargetReportData.GetProductivityCountCustomerRegionReport(request);
    }

    public IReadOnlyList<GetProductivityVolumeRegionReportItem> GetProductivityVolumeRegionReport(GetProductivityVolumeRegionReportRequest request)
    {
        request ??= new GetProductivityVolumeRegionReportRequest();

        if (MockEnabled)
            return MockTargetReportData.GetProductivityVolumeRegionReport(request);

        // Gerçek SP ismi ve parametreleri netleştiğinde burası güncellenecek.
        // Örnek taslak (SP adı sadece örnek):
        //
        // var parameters = new Dictionary<string, object?>
        // {
        //     ["@SessionId"] = request.SessionId ?? string.Empty,
        //     ["@RegionCode"] = request.RegionCode,
        //     ["@SubTabId"] = request.SubTabId,
        //     ["@ReportDate"] = request.ReportDate,
        //     ["@SortBy"] = request.SortBy ?? (object)DBNull.Value,
        //     ["@IsAscending"] = request.IsAscending
        // };
        //
        // var ds = _spExecutor.ExecuteDataSet("Main", "SP_RP_GetProductivityVolumeRegionReport", parameters);
        // if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
        //     return Array.Empty<GetProductivityVolumeRegionReportItem>();
        //
        // return DataTableHelper.ToList<GetProductivityVolumeRegionReportItem>(ds.Tables[0]);

        return MockTargetReportData.GetProductivityVolumeRegionReport(request);
    }

    public IReadOnlyList<GetProductivityProfitRatioRegionReportItem> GetProductivityProfitRatioRegionReport(GetProductivityProfitRatioRegionReportRequest request)
    {
        request ??= new GetProductivityProfitRatioRegionReportRequest();

        if (MockEnabled)
            return MockTargetReportData.GetProductivityProfitRatioRegionReport(request);

        // Gerçek SP ismi ve parametreleri netleştiğinde burası güncellenecek.
        // Örnek taslak (SP adı sadece örnek):
        //
        // var parameters = new Dictionary<string, object?>
        // {
        //     ["@SessionId"] = request.SessionId ?? string.Empty,
        //     ["@RegionCode"] = request.RegionCode,
        //     ["@ReportDate"] = request.ReportDate,
        //     ["@SortBy"] = request.SortBy ?? (object)DBNull.Value,
        //     ["@IsAscending"] = request.IsAscending
        // };
        //
        // var ds = _spExecutor.ExecuteDataSet("Main", "SP_RP_GetProductivityProfitRatioRegionReport", parameters);
        // if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
        //     return Array.Empty<GetProductivityProfitRatioRegionReportItem>();
        //
        // return DataTableHelper.ToList<GetProductivityProfitRatioRegionReportItem>(ds.Tables[0]);

        return MockTargetReportData.GetProductivityProfitRatioRegionReport(request);
    }

    public IReadOnlyList<GetProductivityProfitTotalRegionReportItem> GetProductivityProfitTotalRegionReport(GetProductivityProfitTotalRegionReportRequest request)
    {
        request ??= new GetProductivityProfitTotalRegionReportRequest();

        if (MockEnabled)
            return MockTargetReportData.GetProductivityProfitTotalRegionReport(request);

        // Gerçek SP ismi ve parametreleri netleştiğinde burası güncellenecek.
        // Örnek taslak (SP adı sadece örnek):
        //
        // var parameters = new Dictionary<string, object?>
        // {
        //     ["@SessionId"] = request.SessionId ?? string.Empty,
        //     ["@RegionCode"] = request.RegionCode,
        //     ["@ReportDate"] = request.ReportDate,
        //     ["@SortBy"] = request.SortBy ?? (object)DBNull.Value,
        //     ["@IsAscending"] = request.IsAscending
        // };
        //
        // var ds = _spExecutor.ExecuteDataSet("Main", "SP_RP_GetProductivityProfitTotalRegionReport", parameters);
        // if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
        //     return Array.Empty<GetProductivityProfitTotalRegionReportItem>();
        //
        // return DataTableHelper.ToList<GetProductivityProfitTotalRegionReportItem>(ds.Tables[0]);

        return MockTargetReportData.GetProductivityProfitTotalRegionReport(request);
    }

    public IReadOnlyList<GetProductivityProfitSpreadManagementRegionReportItem> GetProductivityProfitSpreadManagementRegionReport(GetProductivityProfitSpreadManagementRegionReportRequest request)
    {
        request ??= new GetProductivityProfitSpreadManagementRegionReportRequest();

        if (MockEnabled)
            return MockTargetReportData.GetProductivityProfitSpreadManagementRegionReport(request);

        // Gerçek SP ismi ve parametreleri netleştiğinde burası güncellenecek.
        // Örnek taslak (SP adı sadece örnek):
        //
        // var parameters = new Dictionary<string, object?>
        // {
        //     ["@SessionId"] = request.SessionId ?? string.Empty,
        //     ["@RegionCode"] = request.RegionCode,
        //     ["@ReportDate"] = request.ReportDate,
        //     ["@SortBy"] = request.SortBy ?? (object)DBNull.Value,
        //     ["@IsAscending"] = request.IsAscending
        // };
        //
        // var ds = _spExecutor.ExecuteDataSet("Main", "SP_RP_GetProductivityProfitSpreadManagementRegionReport", parameters);
        // if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
        //     return Array.Empty<GetProductivityProfitSpreadManagementRegionReportItem>();
        //
        // return DataTableHelper.ToList<GetProductivityProfitSpreadManagementRegionReportItem>(ds.Tables[0]);

        return MockTargetReportData.GetProductivityProfitSpreadManagementRegionReport(request);
    }

    public IReadOnlyList<GetProductivityProfitSpreadManagementBranchReportItem> GetProductivityProfitSpreadManagementBranchReport(GetProductivityProfitSpreadManagementBranchReportRequest request)
    {
        request ??= new GetProductivityProfitSpreadManagementBranchReportRequest();

        if (MockEnabled)
            return MockTargetReportData.GetProductivityProfitSpreadManagementBranchReport(request);

        // Gerçek SP ismi ve parametreleri netleştiğinde burası güncellenecek.
        // Örnek taslak (SP adı sadece örnek):
        //
        // var parameters = new Dictionary<string, object?>
        // {
        //     ["@SessionId"] = request.SessionId ?? string.Empty,
        //     ["@BranchCode"] = request.BranchCode,
        //     ["@ReportDate"] = request.ReportDate,
        //     ["@SortBy"] = request.SortBy ?? (object)DBNull.Value,
        //     ["@IsAscending"] = request.IsAscending
        // };
        //
        // var ds = _spExecutor.ExecuteDataSet("Main", "SP_RP_GetProductivityProfitSpreadManagementBranchReport", parameters);
        // if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
        //     return Array.Empty<GetProductivityProfitSpreadManagementBranchReportItem>();
        //
        // return DataTableHelper.ToList<GetProductivityProfitSpreadManagementBranchReportItem>(ds.Tables[0]);

        return MockTargetReportData.GetProductivityProfitSpreadManagementBranchReport(request);
    }

    public IReadOnlyList<GetProductivityCountCardPosBranchReportItem> GetProductivityCountCardPosBranchReport(GetProductivityCountCardPosBranchReportRequest request)
    {
        request ??= new GetProductivityCountCardPosBranchReportRequest();

        if (MockEnabled)
            return MockTargetReportData.GetProductivityCountCardPosBranchReport(request);

        // Gerçek SP ismi ve parametreleri netleştiğinde burası güncellenecek.
        // Örnek taslak (SP adı sadece örnek):
        //
        // var parameters = new Dictionary<string, object?>
        // {
        //     ["@SessionId"] = request.SessionId ?? string.Empty,
        //     ["@BranchCode"] = request.BranchCode,
        //     ["@TabId"] = request.TabId,
        //     ["@ReportDate"] = request.ReportDate,
        //     ["@SortBy"] = request.SortBy ?? (object)DBNull.Value,
        //     ["@IsAscending"] = request.IsAscending
        // };
        //
        // var ds = _spExecutor.ExecuteDataSet("Main", "SP_RP_GetProductivityCountCardPosBranchReport", parameters);
        // if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
        //     return Array.Empty<GetProductivityCountCardPosBranchReportItem>();
        //
        // return DataTableHelper.ToList<GetProductivityCountCardPosBranchReportItem>(ds.Tables[0]);

        return MockTargetReportData.GetProductivityCountCardPosBranchReport(request);
    }

    public IReadOnlyList<GetProductivityProfitRatioBranchReportItem> GetProductivityProfitRatioBranchReport(GetProductivityProfitRatioBranchReportRequest request)
    {
        request ??= new GetProductivityProfitRatioBranchReportRequest();

        if (MockEnabled)
            return MockTargetReportData.GetProductivityProfitRatioBranchReport(request);

        // Gerçek SP ismi ve parametreleri netleştiğinde burası güncellenecek.
        // Örnek taslak (SP adı sadece örnek):
        //
        // var parameters = new Dictionary<string, object?>
        // {
        //     ["@SessionId"] = request.SessionId ?? string.Empty,
        //     ["@BranchCode"] = request.BranchCode,
        //     ["@ReportDate"] = request.ReportDate,
        //     ["@SortBy"] = request.SortBy ?? (object)DBNull.Value,
        //     ["@IsAscending"] = request.IsAscending
        // };
        //
        // var ds = _spExecutor.ExecuteDataSet("Main", "SP_RP_GetProductivityProfitRatioBranchReport", parameters);
        // if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
        //     return Array.Empty<GetProductivityProfitRatioBranchReportItem>();
        //
        // return DataTableHelper.ToList<GetProductivityProfitRatioBranchReportItem>(ds.Tables[0]);

        return MockTargetReportData.GetProductivityProfitRatioBranchReport(request);
    }

    public IReadOnlyList<GetProductivityProfitTotalBranchReportItem> GetProductivityProfitTotalBranchReport(GetProductivityProfitTotalBranchReportRequest request)
    {
        request ??= new GetProductivityProfitTotalBranchReportRequest();

        if (MockEnabled)
            return MockTargetReportData.GetProductivityProfitTotalBranchReport(request);

        // Gerçek SP ismi ve parametreleri netleştiğinde burası güncellenecek.
        // Örnek taslak (SP adı sadece örnek):
        //
        // var parameters = new Dictionary<string, object?>
        // {
        //     ["@SessionId"] = request.SessionId ?? string.Empty,
        //     ["@BranchCode"] = request.BranchCode,
        //     ["@ReportDate"] = request.ReportDate,
        //     ["@SortBy"] = request.SortBy ?? (object)DBNull.Value,
        //     ["@IsAscending"] = request.IsAscending
        // };
        //
        // var ds = _spExecutor.ExecuteDataSet("Main", "SP_RP_GetProductivityProfitTotalBranchReport", parameters);
        // if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
        //     return Array.Empty<GetProductivityProfitTotalBranchReportItem>();
        //
        // return DataTableHelper.ToList<GetProductivityProfitTotalBranchReportItem>(ds.Tables[0]);

        return MockTargetReportData.GetProductivityProfitTotalBranchReport(request);
    }

    public GetProductivityBranchScoreCardReportItem GetProductivityBranchScoreCardReport(GetProductivityBranchScoreCardReportRequest request)
    {
        request ??= new GetProductivityBranchScoreCardReportRequest();

        if (MockEnabled)
            return MockTargetReportData.GetProductivityBranchScoreCardReport(request);

        // Gerçek SP ismi ve parametreleri netleştiğinde burası güncellenecek.
        // Örnek taslak (SP adı sadece örnek, tek satır döner):
        //
        // var parameters = new Dictionary<string, object?>
        // {
        //     ["@SessionId"] = request.SessionId ?? string.Empty,
        //     ["@BranchCode"] = request.BranchCode,
        //     ["@ReportDate"] = request.ReportDate
        // };
        //
        // var ds = _spExecutor.ExecuteDataSet("Main", "SP_RP_GetProductivityBranchScoreCardReport", parameters);
        // if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
        //     return new GetProductivityBranchScoreCardReportItem();
        //
        // return DataTableHelper.ToList<GetProductivityBranchScoreCardReportItem>(ds.Tables[0])[0];

        return MockTargetReportData.GetProductivityBranchScoreCardReport(request);
    }

    public GetProductivityRegionScoreCardReportItem GetProductivityRegionScoreCardReport(GetProductivityRegionScoreCardReportRequest request)
    {
        request ??= new GetProductivityRegionScoreCardReportRequest();

        if (MockEnabled)
            return MockTargetReportData.GetProductivityRegionScoreCardReport(request);

        // Gerçek SP ismi ve parametreleri netleştiğinde burası güncellenecek.
        // Örnek taslak (SP adı sadece örnek, tek satır döner):
        //
        // var parameters = new Dictionary<string, object?>
        // {
        //     ["@SessionId"] = request.SessionId ?? string.Empty,
        //     ["@RegionCode"] = request.RegionCode,
        //     ["@BranchCode"] = request.BranchCode,
        //     ["@ReportDate"] = request.ReportDate
        // };
        //
        // var ds = _spExecutor.ExecuteDataSet("Main", "SP_RP_GetProductivityRegionScoreCardReport", parameters);
        // if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
        //     return new GetProductivityRegionScoreCardReportItem();
        //
        // return DataTableHelper.ToList<GetProductivityRegionScoreCardReportItem>(ds.Tables[0])[0];

        return MockTargetReportData.GetProductivityRegionScoreCardReport(request);
    }

    public IReadOnlyList<GetProductivityCountCustomerBranchReportItem> GetProductivityCountCustomerBranchReport(GetProductivityCountCustomerBranchReportRequest request)
    {
        request ??= new GetProductivityCountCustomerBranchReportRequest();

        if (MockEnabled)
            return MockTargetReportData.GetProductivityCountCustomerBranchReport(request);

        // Gerçek SP ismi ve parametreleri netleştiğinde burası güncellenecek.
        // Örnek taslak (SP adı sadece örnek):
        //
        // var parameters = new Dictionary<string, object?>
        // {
        //     ["@SessionId"] = request.SessionId ?? string.Empty,
        //     ["@BranchCode"] = request.BranchCode,
        //     ["@SubTabId"] = request.SubTabId,
        //     ["@ReportDate"] = request.ReportDate,
        //     ["@SortBy"] = request.SortBy ?? (object)DBNull.Value,
        //     ["@IsAscending"] = request.IsAscending
        // };
        //
        // var ds = _spExecutor.ExecuteDataSet("Main", "SP_RP_GetProductivityCountCustomerBranchReport", parameters);
        // if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
        //     return Array.Empty<GetProductivityCountCustomerBranchReportItem>();
        //
        // return DataTableHelper.ToList<GetProductivityCountCustomerBranchReportItem>(ds.Tables[0]);

        return MockTargetReportData.GetProductivityCountCustomerBranchReport(request);
    }

    public IReadOnlyList<GetProductivityVolumeBranchReportItem> GetProductivityVolumeBranchReport(GetProductivityVolumeBranchReportRequest request)
    {
        request ??= new GetProductivityVolumeBranchReportRequest();

        if (MockEnabled)
            return MockTargetReportData.GetProductivityVolumeBranchReport(request);

        // Gerçek SP ismi ve parametreleri netleştiğinde burası güncellenecek.
        // Örnek taslak (SP adı sadece örnek):
        //
        // var parameters = new Dictionary<string, object?>
        // {
        //     ["@SessionId"] = request.SessionId ?? string.Empty,
        //     ["@BranchCode"] = request.BranchCode,
        //     ["@SubTabId"] = request.SubTabId,
        //     ["@ReportDate"] = request.ReportDate,
        //     ["@SortBy"] = request.SortBy ?? (object)DBNull.Value,
        //     ["@IsAscending"] = request.IsAscending
        // };
        //
        // var ds = _spExecutor.ExecuteDataSet("Main", "SP_RP_GetProductivityVolumeBranchReport", parameters);
        // if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
        //     return Array.Empty<GetProductivityVolumeBranchReportItem>();
        //
        // return DataTableHelper.ToList<GetProductivityVolumeBranchReportItem>(ds.Tables[0]);

        return MockTargetReportData.GetProductivityVolumeBranchReport(request);
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
}

