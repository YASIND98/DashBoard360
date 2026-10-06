namespace DashboardTsy.Application.ScoreCard;

/// <summary>
/// Skor kart istek loglarını isteği bekletmeden kalıcı depoya yazılmak üzere sıraya alır.
/// </summary>
public interface IScoreCardRequestLogQueue
{
    void Enqueue(ScoreCardRequestLog log);
}
