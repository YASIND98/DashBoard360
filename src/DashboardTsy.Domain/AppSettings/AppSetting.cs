using System.Globalization;

namespace DashboardTsy.Domain.AppSettings;

public sealed class AppSetting
{
    public string Key { get; }
    public string? RawValue { get; }
    public AppSettingType Type { get; }
    public string? Description { get; }

    public AppSetting(string key, string? rawValue, AppSettingType type, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key required.", nameof(key));

        Key = key;
        RawValue = rawValue;
        Type = type;
        Description = description;
    }

    public object? GetTypedValue()
    {
        if (RawValue is null) return null;

        return Type switch
        {
            AppSettingType.String => RawValue,
            AppSettingType.Int => int.Parse(RawValue, CultureInfo.InvariantCulture),
            AppSettingType.Decimal => decimal.Parse(RawValue, CultureInfo.InvariantCulture),
            AppSettingType.Boolean => ParseBoolean(RawValue),
            _ => throw new InvalidOperationException($"Unknown AppSettingType: {Type}")
        };
    }

    private static bool ParseBoolean(string raw)
    {
        if (bool.TryParse(raw, out var b)) return b;
        if (raw == "1") return true;
        if (raw == "0") return false;
        throw new FormatException($"Cannot parse '{raw}' as boolean.");
    }
}
