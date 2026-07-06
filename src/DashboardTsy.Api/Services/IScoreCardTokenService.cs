namespace DashboardTsy.Api.Services;

public interface IScoreCardTokenService
{
    Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default);
}
