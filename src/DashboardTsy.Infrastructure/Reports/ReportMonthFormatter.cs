using System.Globalization;

namespace DashboardTsy.Infrastructure.Reports;

/// <summary>
/// SP'lerden gelen rapor tarihini ekranda gösterilecek "Ağustos 2026" formatına çevirir.
/// </summary>
internal static class ReportMonthFormatter
{
    private static readonly CultureInfo TurkishCulture = CultureInfo.GetCultureInfo("tr-TR");

    public static string? Format(DateTime? reportDate)
        => reportDate is { } date && date != default
            ? date.ToString("MMMM yyyy", TurkishCulture)
            : null;
}
