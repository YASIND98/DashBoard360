using System.Threading.Channels;
using DashboardTsy.Application.ScoreCard;

namespace DashboardTsy.Api.Services;

/// <summary>
/// Skor kart istek loglarını bellekte sıraya alır; <see cref="ScoreCardRequestLogWriter"/> sıradan okuyup DB'ye yazar.
/// Böylece log yazımı skor kart isteğini bekletmez. Kuyruk dolarsa yeni kayıt atılır ve uyarı loglanır.
/// </summary>
public sealed class ScoreCardRequestLogQueue : IScoreCardRequestLogQueue
{
    private const int Capacity = 10_000;

    private readonly Channel<ScoreCardRequestLog> _channel = Channel.CreateBounded<ScoreCardRequestLog>(
        new BoundedChannelOptions(Capacity) { FullMode = BoundedChannelFullMode.Wait, SingleReader = true });

    private readonly ILogger<ScoreCardRequestLogQueue> _logger;

    public ScoreCardRequestLogQueue(ILogger<ScoreCardRequestLogQueue> logger)
    {
        _logger = logger;
    }

    public ChannelReader<ScoreCardRequestLog> Reader => _channel.Reader;

    public void Enqueue(ScoreCardRequestLog log)
    {
        if (!_channel.Writer.TryWrite(log))
            _logger.LogWarning("[ScoreCard] İstek log kuyruğu dolu, kayıt atıldı: {Endpoint}", log.Endpoint);
    }
}
