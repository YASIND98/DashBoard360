namespace DashboardTsy.Web.Models.NplReport.Request;

public class GetNplBalanceRatioRequest
{
    public string SessionId { get; set; } = string.Empty;
    public string? KatDonem { get; set; }
    public string? IsKolu { get; set; }
    public string? TahsisKolu { get; set; }
    public int? SubeKodu { get; set; }
    public int? BolgeKodu { get; set; }
    public string? Urun { get; set; }
    public int? Yil { get; set; }
    public string? YetkiKodu { get; set; }

    public bool? BonusBusinessFlag { get; set; }
    public bool? BireyselMikroFlag { get; set; }
    public int? IrsFlag { get; set; }
    public bool? YapilandirmaFlag { get; set; }
    public string? YapilandirmaFlagKredi { get; set; }
    public bool? IhtiyacTicariFlag { get; set; }
    public bool? KgfliKrediFlag { get; set; }
    public bool? KgfliMustFlag { get; set; }
    public bool? EmekliFlag { get; set; }
    public bool? DbMaasOdemesiFlag { get; set; }
    public string? Ob { get; set; }
}
