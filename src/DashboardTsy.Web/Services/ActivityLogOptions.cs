namespace DashboardTsy.Web.Services;

public sealed class ActivityLogOptions
{
    public const string SectionName = "ActivityLog";

    /// <summary>Açık değilse veya bağlantı yoksa log yazılmaz.</summary>
    public bool Enabled { get; set; } = true;
}
