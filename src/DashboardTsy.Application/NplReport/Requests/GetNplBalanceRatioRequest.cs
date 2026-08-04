namespace DashboardTsy.Application.NplReport.Requests;

/// <summary>
/// SP_RP_NPL_Bakiye_Oran için dinamik parametre taşıyıcı request.
/// Frontend, SP parametre adı → değer eşleşmesini Parameters sözlüğünde gönderir.
/// Kabul edilen anahtarlar için bkz. NplBalanceRatioSpParameters.AllowedParameters.
/// </summary>
public class GetNplBalanceRatioRequest
{
    public string SessionId { get; set; } = string.Empty;

    public Dictionary<string, object?> Parameters { get; set; } = new();
}
