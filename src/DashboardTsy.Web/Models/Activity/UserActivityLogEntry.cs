namespace DashboardTsy.Web.Models.Activity;

public sealed class UserActivityLogEntry
{
    public int UserId { get; init; }

    public string? UserDisplayName { get; init; }

    public string EventType { get; init; } = string.Empty;

    public string? ActionName { get; init; }

    public string? Route { get; init; }

    public string? PageUrl { get; init; }

    public string? Details { get; init; }

    public string? IpAddress { get; init; }

    public string? UserAgent { get; init; }
}
