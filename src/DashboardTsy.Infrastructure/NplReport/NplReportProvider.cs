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

        var parameters = new Dictionary<string, object?>
        {
            ["@KAT_DONEM"] = string.IsNullOrWhiteSpace(request.KatDonem) ? null : request.KatDonem,
            ["@ISKOLU"] = string.IsNullOrWhiteSpace(request.IsKolu) ? null : request.IsKolu,
            ["@TAHSIS_KOLU"] = string.IsNullOrWhiteSpace(request.TahsisKolu) ? null : request.TahsisKolu,
            ["@SUBE_KODU"] = request.SubeKodu,
            ["@BOLGE_KODU"] = request.BolgeKodu,
            ["@URUN"] = string.IsNullOrWhiteSpace(request.Urun) ? null : request.Urun,
            ["@Yıl"] = request.Yil,
            ["@YETKI_KODU"] = string.IsNullOrWhiteSpace(request.YetkiKodu) ? null : request.YetkiKodu
        };

        var ds = _spExecutor.ExecuteDataSet(ConnectionKey, ProcedureName, parameters);
        if (ds.Tables.Count == 0)
            return Array.Empty<GetNplBalanceRatioItem>();

        return NplBalanceRatioPivot.Pivot(ds.Tables[0]);
    }
}
