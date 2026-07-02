namespace DashboardTsy.Web.Services;

public interface IScoreCardTokenService
{
    Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default);
}
