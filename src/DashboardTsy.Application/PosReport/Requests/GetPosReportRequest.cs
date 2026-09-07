namespace DashboardTsy.Application.PosReport.Requests;

/// <summary>
/// SP_RP_POS_Report için request modeli.
/// SP üç parametre alır: @RegionCode (VARCHAR), @BranchCode (VARCHAR), @TabId (INT).
/// Üçü de opsiyoneldir; boş gönderilirse SP'ye NULL geçilir.
/// </summary>
public class GetPosReportRequest
{
    /// <summary>Bölge kodu — SP @RegionCode parametresi.</summary>
    public string? RegionCode { get; set; }

    /// <summary>Şube kodu — SP @BranchCode parametresi.</summary>
    public string? BranchCode { get; set; }

    /// <summary>Ekrandaki tab seçimi — SP @TabId parametresi.</summary>
    public int? TabId { get; set; }
}
