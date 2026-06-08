namespace DashboardTsy.Api.Models.TargetReport;

public class GetTargetReportMenuTextsResponse
{
    public string ScreenTitle { get; set; } = string.Empty;           // "Hedef Raporları"
    public string TabAllTitle { get; set; } = string.Empty;            // "Tümü"
    public string TabCorporateTitle { get; set; } = string.Empty;      // "Kurumsal"
    public string TabCommercialTitle { get; set; } = string.Empty;     // "Ticari"
    public string TabSmeTitle { get; set; } = string.Empty;            // "KOBİ"
    public string TabAgricultureTitle { get; set; } = string.Empty;    // "Tarım"
    public string TabRetailTitle { get; set; } = string.Empty;        // "Bireysel"
    public string SmeSubTabAllTitle { get; set; } = string.Empty;     // "Tümü"
    public string SmeSubTabKbiTitle { get; set; } = string.Empty;     // "KBİ"
    public string SmeSubTabObiTitle { get; set; } = string.Empty;     // "OBİ"
    public string RetailSubTabAllTitle { get; set; } = string.Empty;  // "Tümü"
    public string RetailSubTabGeneralTitle { get; set; } = string.Empty;   // "Genel Kitle"
    public string RetailSubTabAffiliateTitle { get; set; } = string.Empty; // "Afili"
    public string RetailSubTabPrivateTitle { get; set; } = string.Empty;    // "Özel Bankacılık"
    public string AmountSubTabTitle { get; set; } = string.Empty;    // "Özel Bankacılık"
}
