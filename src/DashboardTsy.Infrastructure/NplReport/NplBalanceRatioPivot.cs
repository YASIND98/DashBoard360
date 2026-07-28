using System.Data;
using DashboardTsy.Application.NplReport.Responses;

namespace DashboardTsy.Infrastructure.NplReport;

/// <summary>
/// sp_NPL_Bakiye_Oran çıktısını (Tarih/Tur/Kolon/Deger) tarih başına tek satıra pivotlar.
/// Saf dönüşüm — I/O yok, bu sayede test edilebilir.
/// </summary>
internal static class NplBalanceRatioPivot
{
    private const string CategoryBalance = "BAKIYE";
    private const string CategoryRatio = "ORAN";

    private const string ColumnAnapara = "Anapara";
    private const string ColumnKof = "KOF";
    private const string ColumnToplam = "Toplam";

    public static IReadOnlyList<GetNplBalanceRatioItem> Pivot(DataTable? table)
    {
        if (table == null || table.Rows.Count == 0)
            return Array.Empty<GetNplBalanceRatioItem>();

        var byDate = new Dictionary<DateTime, GetNplBalanceRatioItem>();
        var order = new List<DateTime>();

        foreach (DataRow row in table.Rows)
        {
            if (!TryReadDate(row, out var date)) continue;

            var category = ReadString(row, "Tur");
            var column = ReadString(row, "Kolon");
            var value = ReadDecimal(row, "Deger");

            if (!byDate.TryGetValue(date, out var item))
            {
                item = new GetNplBalanceRatioItem { ReportDate = date };
                byDate[date] = item;
                order.Add(date);
            }

            Assign(item, category, column, value);
        }

        var result = new List<GetNplBalanceRatioItem>(order.Count);
        foreach (var d in order)
            result.Add(byDate[d]);

        return result;
    }

    private static void Assign(GetNplBalanceRatioItem item, string? category, string? column, decimal? value)
    {
        if (string.IsNullOrEmpty(category) || string.IsNullOrEmpty(column))
            return;

        if (string.Equals(category, CategoryBalance, StringComparison.OrdinalIgnoreCase))
        {
            if (string.Equals(column, ColumnAnapara, StringComparison.OrdinalIgnoreCase)) item.BalanceAnapara = value;
            else if (string.Equals(column, ColumnKof, StringComparison.OrdinalIgnoreCase)) item.BalanceKof = value;
            else if (string.Equals(column, ColumnToplam, StringComparison.OrdinalIgnoreCase)) item.BalanceToplam = value;
        }
        else if (string.Equals(category, CategoryRatio, StringComparison.OrdinalIgnoreCase))
        {
            if (string.Equals(column, ColumnAnapara, StringComparison.OrdinalIgnoreCase)) item.RatioAnapara = value;
            else if (string.Equals(column, ColumnKof, StringComparison.OrdinalIgnoreCase)) item.RatioKof = value;
        }
    }

    private static bool TryReadDate(DataRow row, out DateTime date)
    {
        date = default;
        if (!row.Table.Columns.Contains("Tarih")) return false;
        var raw = row["Tarih"];
        if (raw is DBNull or null) return false;
        if (raw is DateTime dt) { date = dt.Date; return true; }
        return DateTime.TryParse(raw.ToString(), out date);
    }

    private static string? ReadString(DataRow row, string column)
    {
        if (!row.Table.Columns.Contains(column)) return null;
        var raw = row[column];
        return raw is DBNull or null ? null : raw.ToString();
    }

    private static decimal? ReadDecimal(DataRow row, string column)
    {
        if (!row.Table.Columns.Contains(column)) return null;
        var raw = row[column];
        if (raw is DBNull or null) return null;
        if (raw is decimal d) return d;
        return decimal.TryParse(raw.ToString(), out var parsed) ? parsed : null;
    }
}
