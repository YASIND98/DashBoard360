namespace DashboardTsy.Application.NplReport.Responses;

/// <summary>
/// sp_RP_GetNplFilters çıktısının satır bazlı (flat) hâli.
/// Her satır bir filter-item ilişkisidir; UI groupBy(FilterGroupId) ile gruplayarak render eder.
/// </summary>
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
