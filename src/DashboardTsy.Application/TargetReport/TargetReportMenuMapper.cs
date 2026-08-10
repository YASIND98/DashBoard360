using DashboardTsy.Application.TargetReport.Responses;

namespace DashboardTsy.Application.TargetReport;

/// <summary>
/// GetTargetReportMenuTextsResponse (düz alanlar) -> GetTargetReportMenuResponse (tabs/subTabs) dönüşümü.
/// Tek kaynaktan iki view üretir; SP veya mock veri değişse bile tab/subTab hiyerarşisi burada tanımlı kalır.
/// </summary>
public static class TargetReportMenuMapper
{
    public static GetTargetReportMenuResponse ToTabsResponse(GetTargetReportMenuTextsResponse texts)
    {
        var tabs = new List<TargetReportMenuTab>
        {
            BuildTab("all", texts.TabAllTitle),
            BuildTab("corporate", texts.TabCorporateTitle),
            BuildTab("commercial", texts.TabCommercialTitle),
            BuildTab("sme", texts.TabSmeTitle,
                BuildSubTab("sme.all", texts.SmeSubTabAllTitle),
                BuildSubTab("sme.kbi", texts.SmeSubTabKbiTitle),
                BuildSubTab("sme.obi", texts.SmeSubTabObiTitle)),
            BuildTab("agriculture", texts.TabAgricultureTitle),
            BuildTab("retail", texts.TabRetailTitle,
                BuildSubTab("retail.all", texts.RetailSubTabAllTitle),
                BuildSubTab("retail.general", texts.RetailSubTabGeneralTitle),
                BuildSubTab("retail.affiliate", texts.RetailSubTabAffiliateTitle),
                BuildSubTab("retail.private", texts.RetailSubTabPrivateTitle))
        };

        // TabId/SubTabId, dizideki sırayı yansıtır: elle numaralamaya gerek kalmaz, tab eklenip
        // çıkarıldığında da otomatik doğru kalır. SubTabId her tab kendi listesinde 0'dan başlar (global değil).
        for (var i = 0; i < tabs.Count; i++)
            tabs[i].TabId = i;

        return new GetTargetReportMenuResponse { ScreenTitle = texts.ScreenTitle, Tabs = tabs };
    }

    private static TargetReportMenuTab BuildTab(string key, string title, params TargetReportMenuSubTab[] subTabs)
    {
        for (var i = 0; i < subTabs.Length; i++)
            subTabs[i].SubTabId = i;

        return new TargetReportMenuTab { Key = key, Title = title, SubTabs = subTabs };
    }

    private static TargetReportMenuSubTab BuildSubTab(string key, string title)
        => new() { Key = key, Title = title };
}
