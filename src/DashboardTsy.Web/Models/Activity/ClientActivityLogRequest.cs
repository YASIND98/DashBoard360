namespace DashboardTsy.Web.Models.Activity;

public sealed class ClientActivityLogRequest
{
    public string EventType { get; set; } = string.Empty;

    public string? ActionName { get; set; }

    public string? PageUrl { get; set; }

    public string? Details { get; set; }
}
