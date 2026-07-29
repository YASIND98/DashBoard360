namespace DashboardTsy.Web.Models.NplReport.Response;

public class GetNplProductsItem
{
    public int ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
}
