namespace DashboardTsy.Application.TargetReport.Responses;

/// <summary>
/// GetTargetReportMenuTexts'in aynı verisinin mobil istemciler için hiyerarşik (tabs/subTabs) görünümü.
/// Mobil ekip Swift tarafında sabit alan adları yerine dizi üzerinde iterate etmek istediği için eklendi.
/// Web tarafı hâlâ düz alanlı GetTargetReportMenuTextsResponse'u kullanır — bu response onu değiştirmez,
/// aynı kaynağı (SP_RP_GetTargetReportMenuTexts) farklı bir şekilde sunar.
/// </summary>
public class GetTargetReportMenuResponse
{
    public string ScreenTitle { get; set; } = string.Empty;
    public IReadOnlyList<TargetReportMenuTab> Tabs { get; set; } = Array.Empty<TargetReportMenuTab>();
}

public class TargetReportMenuTab
{
    /// <summary>Tabs dizisindeki sırayı yansıtan sabit id (0'dan başlar) — mobilde index/eşleme için.</summary>
    public int TabId { get; set; }

    /// <summary>Stabil, i18n'den bağımsız tanımlayıcı — örn. "sme", "retail". Title değişse bile sabit kalır.</summary>
    public string Key { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public IReadOnlyList<TargetReportMenuSubTab> SubTabs { get; set; } = Array.Empty<TargetReportMenuSubTab>();
}

public class TargetReportMenuSubTab
{
    /// <summary>Kendi tab'ının SubTabs dizisindeki sırayı yansıtan id (her tab'da 0'dan başlar; global unique değildir).</summary>
    public int SubTabId { get; set; }

    /// <summary>Stabil tanımlayıcı — örn. "sme.kbi", "retail.private".</summary>
    public string Key { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
}
