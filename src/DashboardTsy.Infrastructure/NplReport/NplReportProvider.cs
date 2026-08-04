using DashboardTsy.Application.NplReport;
using DashboardTsy.Application.NplReport.Requests;
using DashboardTsy.Application.NplReport.Responses;
using DashboardTsy.Infrastructure.Data;
using Microsoft.Extensions.Configuration;

namespace DashboardTsy.Infrastructure.NplReport;

/// <summary>
/// INplReportProvider implementasyonu: sp_NPL_Bakiye_Oran çağrısını yapar ve long→wide pivot uygular.
/// ReportMock:Enabled true iken MockNplReportData'ya delege eder.
/// </summary>
public class NplReportProvider : INplReportProvider
{
    private const string ConnectionKey = "YoneticiRaporu";
    private const string ProcedureName = "sp_NPL_Bakiye_Oran";
    private const string ProcedureFilters = "SP_RP_GetNplFilters";
    private const string ProcedureProducts = "SP_RP_GetNplProducts";

    private readonly IStoredProcedureExecutor _spExecutor;
    private readonly IConfiguration _configuration;

    public NplReportProvider(IStoredProcedureExecutor spExecutor, IConfiguration configuration)
    {
        _spExecutor = spExecutor;
        _configuration = configuration;
    }

    private bool MockEnabled => _configuration["ReportMock:Enabled"] is string v && bool.TryParse(v, out var b) && b;

    public IReadOnlyList<GetNplBalanceRatioItem> GetNplBalanceRatio(GetNplBalanceRatioRequest request)
    {
        request ??= new GetNplBalanceRatioRequest();

        if (MockEnabled)
            return MockNplReportData.GetNplBalanceRatio(request);

        var parameters = NplBalanceRatioSpParameters.Bind(request.Parameters);
        parameters["@SUBE_KODU"] = request.SubeKodu;
        parameters["@BOLGE_KODU"] = request.BolgeKodu;
        parameters["@Yıl"] = request.Yil;

        var ds = _spExecutor.ExecuteDataSet(ConnectionKey, ProcedureName, parameters);
        if (ds.Tables.Count == 0)
            return Array.Empty<GetNplBalanceRatioItem>();

        return NplBalanceRatioPivot.Pivot(ds.Tables[0]);
    }

    public IReadOnlyList<GetNplFiltersItem> GetNplFilters(GetNplFiltersRequest request)
    {
        request ??= new GetNplFiltersRequest();

        if (MockEnabled)
            return MockNplReportData.GetNplFilters(request);

        var ds = _spExecutor.ExecuteDataSet(ConnectionKey, ProcedureFilters, null);
        if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
            return Array.Empty<GetNplFiltersItem>();

        return DataTableHelper.ToList<GetNplFiltersItem>(ds.Tables[0]);
    }

    public IReadOnlyList<GetNplProductsItem> GetNplProducts(GetNplProductsRequest request)
    {
        request ??= new GetNplProductsRequest();

        if (MockEnabled)
            return MockNplReportData.GetNplProducts(request);

        var ds = _spExecutor.ExecuteDataSet(ConnectionKey, ProcedureProducts, null);
        if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
            return Array.Empty<GetNplProductsItem>();

        return DataTableHelper.ToList<GetNplProductsItem>(ds.Tables[0]);
    }
}
