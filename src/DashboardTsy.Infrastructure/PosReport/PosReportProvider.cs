using System.Data;
using DashboardTsy.Application.PosReport;
using DashboardTsy.Application.PosReport.Requests;
using DashboardTsy.Application.PosReport.Responses;
using DashboardTsy.Infrastructure.Data;
using Microsoft.Extensions.Configuration;

namespace DashboardTsy.Infrastructure.PosReport;

/// <summary>
/// IPosReportProvider implementasyonu: SP_RP_POS_Report çağrısını yapar ve satırları GetPosReportItem'a map eder.
/// ReportMock:Enabled true iken MockPosReportData'ya delege eder.
///
/// SP imzası:
///   @RegionCode   VARCHAR(100)
///   @BranchCode   VARCHAR(...)
///   @TabId        INT
///
/// SP çıktı kolonları: Metrics (VARCHAR), Date (DATE), Value (BIGINT), DiffValue (BIGINT).
/// Kolon adları DTO property adlarıyla birebir olduğu için DataTableHelper.ToList<T>() yeterlidir.
///
/// Skorkart SP imzası (SP_RP_POS_Report_Skorkart):
///   @RegionCode   NVARCHAR(100)
///   @BranchCode   NVARCHAR(50)
///   @ProductCode  INT
///
/// Skorkart çıktı kolonları: Metrics, Date, Achievement, Target, TA_Rate, DiffValue.
/// "TA_Rate" DTO'da "TaRate" olarak tutulur; map öncesi kolon adı yeniden adlandırılır.
/// </summary>
public class PosReportProvider : IPosReportProvider
{
    private const string ConnectionKey = "YoneticiRaporu";
    private const string ProcedureName = "SP_RP_POS_Report";
    private const string ScorecardProcedureName = "SP_RP_POS_Report_Skorkart";
    private const string ScorecardRateColumn = "TA_Rate";

    private readonly IStoredProcedureExecutor _spExecutor;
    private readonly IConfiguration _configuration;

    public PosReportProvider(IStoredProcedureExecutor spExecutor, IConfiguration configuration)
    {
        _spExecutor = spExecutor;
        _configuration = configuration;
    }

    private bool MockEnabled =>
        _configuration["ReportMock:Enabled"] is string v && bool.TryParse(v, out var b) && b;

    public IReadOnlyList<GetPosReportItem> GetPosReport(GetPosReportRequest request)
    {
        request ??= new GetPosReportRequest();

        if (MockEnabled)
            return MockPosReportData.GetPosReport(request);

        var parameters = new Dictionary<string, object?>
        {
            ["@RegionCode"]   = string.IsNullOrWhiteSpace(request.RegionCode) ? (object?)DBNull.Value : request.RegionCode,
            ["@BranchCode"]   = string.IsNullOrWhiteSpace(request.BranchCode) ? (object?)DBNull.Value : request.BranchCode,
            ["@TabId"]      = request.TabId.HasValue ? (object?)request.TabId.Value : DBNull.Value
        };

        var ds = _spExecutor.ExecuteDataSet(ConnectionKey, ProcedureName, parameters);
        if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
            return Array.Empty<GetPosReportItem>();

        return DataTableHelper.ToList<GetPosReportItem>(ds.Tables[0]);
    }

    public IReadOnlyList<GetPosScorecardItem> GetPosScorecard(GetPosScorecardRequest request)
    {
        request ??= new GetPosScorecardRequest();

        if (MockEnabled)
            return MockPosReportData.GetPosScorecard(request);

        var parameters = new Dictionary<string, object?>
        {
            ["@RegionCode"]  = string.IsNullOrWhiteSpace(request.RegionCode) ? (object?)DBNull.Value : request.RegionCode,
            ["@BranchCode"]  = string.IsNullOrWhiteSpace(request.BranchCode) ? (object?)DBNull.Value : request.BranchCode,
            ["@ProductCode"] = request.ProductCode ?? GetPosScorecardRequest.DefaultProductCode
        };

        var ds = _spExecutor.ExecuteDataSet(ConnectionKey, ScorecardProcedureName, parameters);
        if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
            return Array.Empty<GetPosScorecardItem>();

        var table = ds.Tables[0];
        if (table.Columns.Contains(ScorecardRateColumn))
            table.Columns[ScorecardRateColumn]!.ColumnName = nameof(GetPosScorecardItem.TaRate);

        return DataTableHelper.ToList<GetPosScorecardItem>(table);
    }
}
