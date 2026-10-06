using DashboardTsy.Application.ScoreCard;

namespace DashboardTsy.Api.Services;

/// <summary>
/// Kuyruktaki skor kart istek loglarını sırayla ScoreCardRequestLogs tablosuna yazar.
/// Yazılamayan kayıt yalnızca uygulama loguna düşer; skor kart isteklerini etkilemez.
/// </summary>
public sealed class ScoreCardRequestLogWriter : BackgroundService
{
    private readonly ScoreCardRequestLogQueue _queue;
    private readonly IScoreCardRequestLogRepository _repository;
    private readonly ILogger<ScoreCardRequestLogWriter> _logger;

    public ScoreCardRequestLogWriter(
        ScoreCardRequestLogQueue queue,
        IScoreCardRequestLogRepository repository,
        ILogger<ScoreCardRequestLogWriter> logger)
    {
        _queue = queue;
        _repository = repository;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var log in _queue.Reader.ReadAllAsync(stoppingToken).ConfigureAwait(false))
        {
            try
            {
                _repository.Insert(log);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[ScoreCard] İstek logu DB'ye yazılamadı: {Endpoint}", log.Endpoint);
            }
        }
    }
}
