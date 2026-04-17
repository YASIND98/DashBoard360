namespace DashboardTsy.Web.Models.ProductivityReport.Response;

public class GetReportSidebarItem
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public bool IsVisible { get; set; }
    public int OrderNo { get; set; }
}
