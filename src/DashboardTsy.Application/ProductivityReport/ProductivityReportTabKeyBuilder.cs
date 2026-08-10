using DashboardTsy.Application.ProductivityReport.Responses;

namespace DashboardTsy.Application.ProductivityReport;

/// <summary>
/// GetProductivityReportTabItem listesindeki her satıra, kök tab'dan kendisine kadarki yolu
/// nokta ile birleştiren stabil bir Key atar (örn. "count.customer.all").
/// Aynı TabName birden çok parent altında tekrar edebildiği için (örn. "Tümü" hem Hacim hem
/// Müşteri hem Nakit Yönetimi altında var), tek başına TabName->key sözlüğü yeterli değildir;
/// parent zinciri key'e dahil edilerek tekillik sağlanır.
///
/// Girdi mock veya SP fark etmeksizin çalışır: TabId/ParentId/TabName üzerinden yürür, veri
/// kaynağına bağımlı değildir. Sözlükte karşılığı olmayan (bilinmeyen) bir TabName gelirse
/// atlamak yerine TabId'yi fallback key olarak kullanır — böylece yeni bir SP alanı sessizce
/// boş key ile gelmez.
/// </summary>
public static class ProductivityReportTabKeyBuilder
{
    // TabName -> kısa segment key'i. Yeni bir sekme eklendiğinde sadece burava bir satır eklemek yeterli.
    private static readonly IReadOnlyDictionary<string, string> SegmentKeys = new Dictionary<string, string>
    {
        ["Genel"] = "general",
        ["Hacim"] = "volume",
        ["Karlılık"] = "profit",
        ["Adet"] = "count",
        ["Müşteri"] = "customer",
        ["Ödeme Sistemleri"] = "payment-systems",
        ["Nakit Yönetimi"] = "cash-management",
        ["Toplam"] = "total",
        ["Spread Yönetimi"] = "spread-management",
        ["Tümü"] = "all",
        ["Kurumsal"] = "corporate",
        ["Ticari"] = "commercial",
        ["KOBİ"] = "sme",
        ["Tarım"] = "agriculture",
        ["Bireysel"] = "retail"
    };

    public static IReadOnlyList<GetProductivityReportTabItem> Build(IReadOnlyList<GetProductivityReportTabItem> tabs)
    {
        var byId = tabs.ToDictionary(t => t.TabId);

        foreach (var tab in tabs)
            tab.Key = BuildKey(tab, byId);

        return tabs;
    }

    private static string BuildKey(GetProductivityReportTabItem tab, IReadOnlyDictionary<int, GetProductivityReportTabItem> byId)
    {
        var ownSegment = SegmentKeys.TryGetValue(tab.TabName, out var mapped) ? mapped : tab.TabId.ToString();

        if (tab.ParentId == 0 || !byId.TryGetValue(tab.ParentId, out var parent))
            return ownSegment;

        return $"{BuildKey(parent, byId)}.{ownSegment}";
    }
}
