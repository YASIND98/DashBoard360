namespace DashboardTsy.Application.ProductivityReport.Responses;

public class GetProductivityReportTabItem
{
    public int TabId { get; set; }

    /// <summary>Stabil, i18n'den bağımsız tanımlayıcı — kök tab'dan bu tab'a kadarki yolu nokta ile birleştirir
    /// (örn. "count.customer.all"). Aynı TabName birden çok parent altında tekrar edebildiği için
    /// (örn. "Tümü"), tekil ayrım için parent zinciri key'e dahil edilir.</summary>
    public string Key { get; set; } = string.Empty;

    public string TabName { get; set; } = string.Empty;
    public int ParentId { get; set; }
    public int TabLevel { get; set; }
}
