using DashboardTsy.Web.Models.Activity;

namespace DashboardTsy.Web.Services;

public sealed class NoOpUserActivityLogService : IUserActivityLogService
{
    public Task LogAsync(UserActivityLogEntry entry, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}
