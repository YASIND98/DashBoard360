using DashboardTsy.Domain.AppSettings;

namespace DashboardTsy.Application.AppSettings;

public sealed class AppSettingDto
{
    public string Key { get; set; } = string.Empty;
    public object? Value { get; set; }
    public AppSettingType Type { get; set; }
    public string? Description { get; set; }
}
