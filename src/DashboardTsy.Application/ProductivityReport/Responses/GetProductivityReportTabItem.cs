namespace DashboardTsy.Application.ProductivityReport.Responses;

public class GetProductivityReportTabItem
{
    public int TabId { get; set; }
    public string TabName { get; set; } = string.Empty;
    public int ParentId { get; set; }
    public int TabLevel { get; set; }
}
