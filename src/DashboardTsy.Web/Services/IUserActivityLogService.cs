using DashboardTsy.Web.Models.Activity;

namespace DashboardTsy.Web.Services;

public interface IUserActivityLogService
{
    Task LogAsync(UserActivityLogEntry entry, CancellationToken cancellationToken = default);
}
