namespace DashboardTsy.Web.Models.TargetReport;

public class ProductTop10DifferencesResponse
{
    public List<Top10Item> First10 { get; set; } = new();
    public List<Top10Item> Last10 { get; set; } = new();
}

public class Top10Item
{
    public long CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public decimal Value { get; set; }
}
