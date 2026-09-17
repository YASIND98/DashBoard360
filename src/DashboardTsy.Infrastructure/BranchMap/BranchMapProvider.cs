using System.Data;
using DashboardTsy.Application.BranchMap;
using DashboardTsy.Application.BranchMap.Requests;
using DashboardTsy.Application.BranchMap.Responses;
using DashboardTsy.Infrastructure.Data;
using Microsoft.Extensions.Configuration;

namespace DashboardTsy.Infrastructure.BranchMap;

/// <summary>
/// IBranchMapProvider implementasyonu: SP_RP_BRANCHMAPINFO çağrısını yapar.
/// SP çıktısındaki kolon adları (BRANCNAME, BRANCHADRESS, LN, LT) DTO adlarıyla
/// birebir örtüşmediği için DataTableHelper yerine elle eşleme kullanılır.
/// ReportMock:Enabled true iken MockBranchMapData'ya delege eder.
/// </summary>
public class BranchMapProvider : IBranchMapProvider
{
    private const string ConnectionKey = "YoneticiRaporu";
    private const string ProcedureName = "SP_RP_BRANCHMAPINFO";

    private readonly IStoredProcedureExecutor _spExecutor;
    private readonly IConfiguration _configuration;

    public BranchMapProvider(IStoredProcedureExecutor spExecutor, IConfiguration configuration)
    {
        _spExecutor = spExecutor;
        _configuration = configuration;
    }

    private bool MockEnabled =>
        _configuration["ReportMock:Enabled"] is string v && bool.TryParse(v, out var b) && b;

    public IReadOnlyList<GetBranchMapInfoItem> GetBranchMapInfo(GetBranchMapInfoRequest request)
    {
        request ??= new GetBranchMapInfoRequest();

        if (MockEnabled)
            return MockBranchMapData.GetBranchMapInfo(request);

        var parameters = new Dictionary<string, object?>
        {
            ["@REGIONCODE"] = request.RegionCode,
            ["@BRANCOCDE"] = request.BranchCode
        };

        var ds = _spExecutor.ExecuteDataSet(ConnectionKey, ProcedureName, parameters);
        if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
            return Array.Empty<GetBranchMapInfoItem>();

        return MapRows(ds.Tables[0]);
    }

    private static List<GetBranchMapInfoItem> MapRows(DataTable table)
    {
        var list = new List<GetBranchMapInfoItem>(table.Rows.Count);

        foreach (DataRow row in table.Rows)
        {
            list.Add(new GetBranchMapInfoItem
            {
                RegionCode = row["REGIONCODE"] as short? ?? 0,
                RegionName = row["REGIONNAME"] as string ?? string.Empty,
                BranchCode = row["BRANCHCODE"] as short? ?? 0,
                BranchName = row["BRANCNAME"] as string ?? string.Empty,
                BranchAddress = row["BRANCHADRESS"] as string ?? string.Empty,
                Longitude = row["LN"] as decimal? ?? 0m,
                Latitude = row["LT"] as decimal? ?? 0m
            });
        }

        return list;
    }
}
