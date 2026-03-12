namespace DashboardTsy.Web.Models.TargetReport;

public class GetMonthlyTargetReportRequest
{
    public string SessionId { get; set; } = string.Empty;
    public int TabId { get; set; }
    public int? SubTabId { get; set; }
    public DateTime ReportDate { get; set; }

    public List<int>? RegionId { get; set; }
    public List<int>? BranchId { get; set; }
    public List<int>? PortfolioId { get; set; }

    public string? SearchText { get; set; }

    public int? SortBy { get; set; }
    public bool IsAscending { get; set; } = false;
}

