namespace DashboardTsy.Web.Models.NplReport.Response;

public class GetNplFiltersItem
{
    public int FilterGroupId { get; set; }
    public string FilterCode { get; set; } = string.Empty;
    public string FilterName { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool IsMultiSelect { get; set; }

    public int FilterItemId { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public int ItemOrder { get; set; }
    public bool IsDefault { get; set; }
}
