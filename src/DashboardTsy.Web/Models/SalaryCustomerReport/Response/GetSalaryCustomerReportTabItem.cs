namespace DashboardTsy.Web.Models.SalaryCustomerReport.Response;

public class GetSalaryCustomerReportTabItem
{
    public int TabId { get; set; }
    public string TabName { get; set; } = string.Empty;
    public int ParentId { get; set; }
    public int TabLevel { get; set; }
}
