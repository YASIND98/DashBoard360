using DashboardTsy.Application.BranchMap.Requests;
using DashboardTsy.Application.BranchMap.Responses;

namespace DashboardTsy.Infrastructure.BranchMap;

/// <summary>
/// BranchMap için mock veri kaynağı.
/// BranchMapProvider, ReportMock:Enabled true olduğunda buraya delege eder.
///
/// İstanbul'un iki bölgesinden gerçek adres/koordinat yaklaşıklığı ile toplam 8 şube üretir;
/// FE harita katmanının kümeleme, filtreleme ve popup davranışını test edebilmesi için farklı
/// noktalara dağıtılmıştır. Değerler tamamen mock — gerçek şube verisi değildir.
/// </summary>
public static class MockBranchMapData
{
    private static readonly GetBranchMapInfoItem[] Branches =
    {
        new()
        {
            RegionCode = 10,
            RegionName = "İstanbul Avrupa 1. Bölge",
            BranchCode = 1201,
            BranchName = "Levent Şubesi",
            BranchAddress = "Büyükdere Cad. No:123 Levent / İstanbul",
            Longitude = 29.011234m,
            Latitude = 41.081234m
        },
        new()
        {
            RegionCode = 10,
            RegionName = "İstanbul Avrupa 1. Bölge",
            BranchCode = 1202,
            BranchName = "Mecidiyeköy Şubesi",
            BranchAddress = "Büyükdere Cad. No:45 Mecidiyeköy / İstanbul",
            Longitude = 28.996875m,
            Latitude = 41.066789m
        },
        new()
        {
            RegionCode = 10,
            RegionName = "İstanbul Avrupa 1. Bölge",
            BranchCode = 1203,
            BranchName = "Şişli Şubesi",
            BranchAddress = "Halaskargazi Cad. No:200 Şişli / İstanbul",
            Longitude = 28.987654m,
            Latitude = 41.058901m
        },
        new()
        {
            RegionCode = 10,
            RegionName = "İstanbul Avrupa 1. Bölge",
            BranchCode = 1204,
            BranchName = "Nişantaşı Şubesi",
            BranchAddress = "Teşvikiye Cad. No:88 Nişantaşı / İstanbul",
            Longitude = 28.994321m,
            Latitude = 41.049876m
        },
        new()
        {
            RegionCode = 20,
            RegionName = "İstanbul Anadolu 1. Bölge",
            BranchCode = 2201,
            BranchName = "Kadıköy Şubesi",
            BranchAddress = "Bağdat Cad. No:150 Kadıköy / İstanbul",
            Longitude = 29.026543m,
            Latitude = 40.987654m
        },
        new()
        {
            RegionCode = 20,
            RegionName = "İstanbul Anadolu 1. Bölge",
            BranchCode = 2202,
            BranchName = "Bostancı Şubesi",
            BranchAddress = "Bağdat Cad. No:512 Bostancı / İstanbul",
            Longitude = 29.093210m,
            Latitude = 40.964321m
        },
        new()
        {
            RegionCode = 20,
            RegionName = "İstanbul Anadolu 1. Bölge",
            BranchCode = 2203,
            BranchName = "Ataşehir Şubesi",
            BranchAddress = "Barbaros Mah. Halk Cad. No:12 Ataşehir / İstanbul",
            Longitude = 29.104567m,
            Latitude = 40.987012m
        },
        new()
        {
            RegionCode = 20,
            RegionName = "İstanbul Anadolu 1. Bölge",
            BranchCode = 2204,
            BranchName = "Ümraniye Şubesi",
            BranchAddress = "Alemdağ Cad. No:340 Ümraniye / İstanbul",
            Longitude = 29.121098m,
            Latitude = 41.021543m
        }
    };

    public static IReadOnlyList<GetBranchMapInfoItem> GetBranchMapInfo(GetBranchMapInfoRequest request)
    {
        IEnumerable<GetBranchMapInfoItem> query = Branches;

        if (request.RegionCode.HasValue)
            query = query.Where(b => b.RegionCode == request.RegionCode.Value);

        if (request.BranchCode.HasValue)
            query = query.Where(b => b.BranchCode == request.BranchCode.Value);

        return query.ToList();
    }
}
