namespace DashboardTsy.Application.TargetReport;

public class GetProductivityCountCardPosBranchReportRequest
{
    public string SessionId { get; set; } = string.Empty;
    public string BranchCode { get; set; } = string.Empty;
    /// <summary>1=KrediKartı, 2=POS vb.</summary>
    public int TabId { get; set; }
    public DateTime ReportDate { get; set; }
    /// <summary>1=Ürün Adı</summary>
    public int? SortBy { get; set; }
    /// <summary>Default: yeniden -> eskiye</summary>
    public bool IsAscending { get; set; } = false;
}

