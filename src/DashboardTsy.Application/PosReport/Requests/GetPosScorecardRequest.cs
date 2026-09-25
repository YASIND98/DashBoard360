namespace DashboardTsy.Application.PosReport.Requests;

/// <summary>
/// SP_RP_POS_Report_Skorkart için request modeli.
/// SP üç parametre alır: @RegionCode (NVARCHAR(100)), @BranchCode (NVARCHAR(50)), @ProductCode (INT).
///
/// Aynı SP hem bölge hem şube görünümünde kullanılır:
///   - RegionCode NULL, BranchCode dolu → şube değeri
///   - RegionCode dolu, BranchCode NULL → bölge değeri
/// </summary>
public class GetPosScorecardRequest
{
    /// <summary>
    /// ProductCode gönderilmezse kullanılacak ürün. Şu an SP tablosunda yalnızca bu ürün var.
    /// SP'nin kendi default'u sadece parametre hiç geçilmediğinde devreye girer; NULL geçildiğinde değil —
    /// bu yüzden default provider tarafında uygulanır.
    /// </summary>
    public const int DefaultProductCode = 536;

    /// <summary>Bölge kodu — SP @RegionCode parametresi. Bölge görünümünde dolu gelir.</summary>
    public string? RegionCode { get; set; }

    /// <summary>Şube kodu — SP @BranchCode parametresi. Şube görünümünde dolu gelir.</summary>
    public string? BranchCode { get; set; }

    /// <summary>Ürün kodu — SP @ProductCode parametresi. Boşsa <see cref="DefaultProductCode"/> kullanılır.</summary>
    public int? ProductCode { get; set; }
}
