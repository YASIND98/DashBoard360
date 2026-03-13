namespace DashboardTsy.Application.TargetReport;

public class GetMonthlyTargetReportRequest
{
    public string SessionId { get; set; } = string.Empty;

    /// <summary>0=Tümü, 1=Kurumsal, 2=Ticari, 3=KOBİ, 4=Tarım, 5=Bireysel</summary>
    public int TabId { get; set; }

    /// <summary>KOBİ: 0=Tümü,1=KBİ,2=OBİ | Bireysel: 0=Tümü,1=GenelKitle,2=Afili,3=ÖzelBankacılık | diğerlerinde null</summary>
    public int? SubTabId { get; set; }

    public DateTime ReportDate { get; set; }

    public List<int>? RegionId { get; set; }
    public List<int>? BranchId { get; set; }
    public List<int>? PortfolioId { get; set; }

    public string? SearchText { get; set; }

    /// <summary>0=#, 1=Ürün Adı, 2=Gerçekleşen, 3=Hedef, 4=H/G, 5=Gerçekleşen, 6=Hedef, 7=H/G</summary>
    public int? SortBy { get; set; }

    public bool IsAscending { get; set; } = false;
}

