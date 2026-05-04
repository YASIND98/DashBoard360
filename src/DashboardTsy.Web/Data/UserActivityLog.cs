namespace DashboardTsy.Web.Data;

public class UserActivityLog
{
    public long Id { get; set; }

    public int UserId { get; set; }

    public string? UserDisplayName { get; set; }

    /// <summary>Login, Logout, PageView, ButtonClick, Navigation, ClientAction, …</summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>Örn. giriş yöntemi (Windows, Domain, Form), buton kodu, menü etiketi.</summary>
    public string? ActionName { get; set; }

    public string? Route { get; set; }

    public string? PageUrl { get; set; }

    public string? Details { get; set; }

    public string? IpAddress { get; set; }

    public string? UserAgent { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}
