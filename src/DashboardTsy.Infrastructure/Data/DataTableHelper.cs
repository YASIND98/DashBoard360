using System.Data;
using System.Reflection;
using System.Linq;

namespace DashboardTsy.Infrastructure.Data;

public static class DataTableHelper
{
    public static List<T> ToList<T>(DataTable table) where T : new()
    {
        if (table == null || table.Rows.Count == 0)
            return new List<T>();

        var list = new List<T>(table.Rows.Count);
        var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanWrite)
            .ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);

        foreach (DataRow row in table.Rows)
        {
            var item = new T();
            foreach (DataColumn col in table.Columns)
            {
                var colName = col.ColumnName;
                if (string.IsNullOrEmpty(colName)) continue;

                if (!props.TryGetValue(colName.Trim(), out var prop))
                    continue;

                if (row[col] is DBNull or null)
                {
                    if (Nullable.GetUnderlyingType(prop.PropertyType) != null || !prop.PropertyType.IsValueType)
                        prop.SetValue(item, null);
                    continue;
                }

                try
                {
                    var value = row[col];
                    var targetType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                    if (value != null && value.GetType() != targetType)
                        value = Convert.ChangeType(value, targetType);
                    prop.SetValue(item, value);
                }
                catch
                {
                    // skip invalid conversion
                }
            }

            list.Add(item);
        }

        return list;
    }

    public static T? ToObject<T>(DataTable table) where T : class, new()
    {
        var list = ToList<T>(table);
        return list.Count > 0 ? list[0] : null;
    }

    public static T? ToObject<T>(DataRow row) where T : class, new()
    {
        if (row == null) return null;
        var table = row.Table;
        if (table == null) return null;

        var item = new T();
        var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanWrite)
            .ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);

        foreach (DataColumn col in table.Columns)
        {
            var colName = col.ColumnName;
            if (string.IsNullOrEmpty(colName) || !props.TryGetValue(colName.Trim(), out var prop))
                continue;

            if (row[col] is DBNull or null)
            {
                if (Nullable.GetUnderlyingType(prop.PropertyType) != null || !prop.PropertyType.IsValueType)
                    prop.SetValue(item, null);
                continue;
            }

            try
            {
                var value = row[col];
                var targetType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                if (value != null && value.GetType() != targetType)
                    value = Convert.ChangeType(value, targetType);
                prop.SetValue(item, value);
            }
            catch { /* skip */ }
        }

        return item;
    }
}
