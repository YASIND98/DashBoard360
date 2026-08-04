namespace DashboardTsy.Application.NplReport.Requests;

/// <summary>
/// SP_RP_NPL_Bakiye_Oran için dinamik parametre taşıyıcı request.
/// Sabit filtreler (SubeKodu, BolgeKodu, Yil) typed alanlardır — Parameters
/// sözlüğünden gönderilemez. Kalan SP parametreleri Parameters sözlüğü
/// üzerinden key/value olarak taşınır; kabul edilen anahtarlar için bkz.
/// NplBalanceRatioSpParameters.AllowedParameters.
/// </summary>
public class GetNplBalanceRatioRequest
{
    public string SessionId { get; set; } = string.Empty;

    public int? SubeKodu { get; set; }
    public int? BolgeKodu { get; set; }
    public int? Yil { get; set; }

    public Dictionary<string, object?> Parameters { get; set; } = new();
}
