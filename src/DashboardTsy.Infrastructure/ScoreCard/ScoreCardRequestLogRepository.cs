using DashboardTsy.Application.ScoreCard;
using DashboardTsy.Infrastructure.Data;

namespace DashboardTsy.Infrastructure.ScoreCard;

/// <summary>
/// Tablo DB'de elle oluşturulur (<c>DashboardTsy.Api/Scripts/ScoreCardRequestLogs.sql</c>).
/// </summary>
public sealed class ScoreCardRequestLogRepository : IScoreCardRequestLogRepository
{
    private const string ConnectionKey = "YoneticiRaporu";
    private const string Sql = @"
INSERT INTO dbo.ScoreCardRequestLogs
    (RequestedAt, Endpoint, HttpMethod, RequestBody, ResponseBody, StatusCode, IsSuccess, ErrorMessage, DurationMs,
     RequesterUserCode, RequesterBranchCode, RegisterId, RegionCode, BranchCode, ScoreCardId, ScoreCardTypeId, PupaType)
VALUES
    (@RequestedAt, @Endpoint, @HttpMethod, @RequestBody, @ResponseBody, @StatusCode, @IsSuccess, @ErrorMessage, @DurationMs,
     @RequesterUserCode, @RequesterBranchCode, @RegisterId, @RegionCode, @BranchCode, @ScoreCardId, @ScoreCardTypeId, @PupaType)";

    private readonly IStoredProcedureExecutor _executor;

    public ScoreCardRequestLogRepository(IStoredProcedureExecutor executor)
    {
        _executor = executor;
    }

    public void Insert(ScoreCardRequestLog log)
    {
        // Sınırlı kolonlar tablo boyutlarına kırpılır; aksi halde SQL "would be truncated" hatasıyla kaydı reddeder.
        var parameters = new Dictionary<string, object?>
        {
            ["@RequestedAt"] = log.RequestedAt,
            ["@Endpoint"] = Truncate(log.Endpoint, 500),
            ["@HttpMethod"] = Truncate(log.HttpMethod, 10),
            ["@RequestBody"] = log.RequestBody,
            ["@ResponseBody"] = log.ResponseBody,
            ["@StatusCode"] = log.StatusCode,
            ["@IsSuccess"] = log.IsSuccess,
            ["@ErrorMessage"] = log.ErrorMessage,
            ["@DurationMs"] = log.DurationMs,
            ["@RequesterUserCode"] = Truncate(log.RequesterUserCode, 100),
            ["@RequesterBranchCode"] = Truncate(log.RequesterBranchCode, 50),
            ["@RegisterId"] = Truncate(log.RegisterId, 50),
            ["@RegionCode"] = Truncate(log.RegionCode, 50),
            ["@BranchCode"] = Truncate(log.BranchCode, 50),
            ["@ScoreCardId"] = Truncate(log.ScoreCardId, 50),
            ["@ScoreCardTypeId"] = Truncate(log.ScoreCardTypeId, 50),
            ["@PupaType"] = Truncate(log.PupaType, 50)
        };

        _executor.ExecuteQueryDataSet(ConnectionKey, Sql, parameters);
    }

    private static string? Truncate(string? value, int maxLength)
        => value is not null && value.Length > maxLength ? value[..maxLength] : value;
}
