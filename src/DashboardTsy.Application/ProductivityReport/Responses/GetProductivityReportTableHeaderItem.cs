namespace DashboardTsy.Application.ProductivityReport.Responses;

public class GetProductivityReportTableHeaderItem
{
    public int Id { get; set; }
    public string HeaderName { get; set; } = string.Empty;
    public int ParentId { get; set; }
    public int OrderNo { get; set; }
    public bool Sortable { get; set; }
}
