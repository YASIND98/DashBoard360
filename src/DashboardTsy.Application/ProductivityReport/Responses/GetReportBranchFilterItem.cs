namespace DashboardTsy.Application.ProductivityReport.Responses;

public class GetReportBranchFilterItem
{
    public string Code { get; set; } = string.Empty;      // Seçenek kodu
    public string Name { get; set; } = string.Empty;      // Seçenek adı
    public string RegionCode { get; set; } = string.Empty;
}
