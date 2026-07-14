namespace DashboardTsy.Application.TargetReport.Requests;

public class GetDailyQuantityTargetReportRequest
{
    public string SessionId { get; set; } = string.Empty;

    /// <summary>0=Tümü, 1=Kurumsal, 2=Ticari, 3=KOBİ, 4=Tarım, 5=Bireysel</summary>
    public int TabId { get; set; }

    /// <summary>KOBİ: 0=Tümü,1=KBİ,2=OBİ; Bireysel: 0=Tümü,1=Genel Kitle,2=Afili,3=Özel Bankacılık</summary>
    public int? SubTabId { get; set; }

    /// <summary>Günlük seçilen tarih</summary>
    public DateTime ReportDate { get; set; }

    public List<int>? RegionId { get; set; }
    public List<int>? BranchId { get; set; }
    public List<int>? PortfolioId { get; set; }

    /// <summary>Ürün adı araması (opsiyonel)</summary>
    public string? SearchText { get; set; }

    /// <summary>Toggle: Farkları Göster</summary>
    public bool ShowDifferences { get; set; } = false;

    /// <summary>0=#, 1=Ürün Adı, 2=Geçen Yıl, 3=Geçen Hafta, 4=Önceki Gün(T-2), 5=Dün(T-1)</summary>
    public int? SortBy { get; set; }

    /// <summary>Default: yeniden -> eskiye</summary>
    public bool IsAscending { get; set; } = false;
}
