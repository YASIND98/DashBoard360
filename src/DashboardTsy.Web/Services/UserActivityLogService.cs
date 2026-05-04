using DashboardTsy.Web.Data;
using DashboardTsy.Web.Models.Activity;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DashboardTsy.Web.Services;

public sealed class UserActivityLogService : IUserActivityLogService
{
    private readonly UserActivityLogDbContext _db;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<UserActivityLogService> _logger;
    private readonly ActivityLogOptions _options;

    public UserActivityLogService(
        UserActivityLogDbContext db,
        IHttpContextAccessor httpContextAccessor,
        IOptions<ActivityLogOptions> options,
        ILogger<UserActivityLogService> logger)
    {
        _db = db;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
        _options = options.Value;
    }

    public async Task LogAsync(UserActivityLogEntry entry, CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled || string.IsNullOrWhiteSpace(entry.EventType))
            return;

        var http = _httpContextAccessor.HttpContext;
        var ip = entry.IpAddress ?? GetClientIp(http);
        var ua = entry.UserAgent ?? TruncateUserAgent(http?.Request.Headers.UserAgent.ToString());

        try
        {
            _db.UserActivityLogs.Add(new UserActivityLog
            {
                UserId = entry.UserId,
                UserDisplayName = entry.UserDisplayName,
                EventType = entry.EventType.Trim(),
                ActionName = entry.ActionName,
                Route = entry.Route,
                PageUrl = entry.PageUrl,
                Details = entry.Details,
                IpAddress = ip,
                UserAgent = ua,
                CreatedAtUtc = DateTime.UtcNow
            });

            await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "User activity log yazılamadı. EventType={EventType}, UserId={UserId}", entry.EventType, entry.UserId);
        }
    }

    private static string? GetClientIp(HttpContext? ctx)
    {
        if (ctx == null)
            return null;

        var fwd = ctx.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(fwd))
        {
            var first = fwd.Split(',')[0].Trim();
            if (first.Length > 0)
                return first.Length > 45 ? first[..45] : first;
        }

        var ip = ctx.Connection.RemoteIpAddress?.ToString();
        return ip is { Length: > 45 } ? ip[..45] : ip;
    }

    private static string? TruncateUserAgent(string? ua)
    {
        if (string.IsNullOrEmpty(ua))
            return null;
        return ua.Length > 500 ? ua[..500] : ua;
    }
}
