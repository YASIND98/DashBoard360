namespace DashboardTsy.Web.Models;

public class ReportViewModel
{
    public string KullaniciAdi { get; set; } = "Ayşe Deniz Yılmaz";
    public string Tarih { get; set; } = "19 Şubat 2026";
    public List<DailyProduct> DailySatirlari { get; set; } = MockData.DailyProducts();
    public List<MonthlyProduct> MonthlySatirlari { get; set; } = MockData.MonthlyProducts();

    public TargetReportMenuTexts MenuTexts { get; set; } = new();
    public DailyTargetReportTableHeaders DailyHeaders { get; set; } = new();
    public MonthlyTargetReportTableHeaders MonthlyHeaders { get; set; } = new();

    public List<FilterItem> BolgeFilters { get; set; } = MockData.BolgeFilters();
    public List<FilterItem> SubeFilters { get; set; } = MockData.SubeFilters();
    public List<FilterItem> PortfoyFilters { get; set; } = MockData.PortfoyFilters();
}

public class FilterItem
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
}

public class TargetReportMenuTexts
{
    // Sayfa
    public string ScreenTitle { get; set; } = "Hedef Raporları";
    // Üst Tab Menü
    public string TabAllTitle { get; set; } = "Tümü";
    public string TabCorporateTitle { get; set; } = "Kurumsal";
    public string TabCommercialTitle { get; set; } = "Ticari";
    public string TabSmeTitle { get; set; } = "KOBİ";
    public string TabAgricultureTitle { get; set; } = "Tarım";
    public string TabRetailTitle { get; set; } = "Bireysel";
    // KOBİ Alt Menü
    public string SmeSubTabAllTitle { get; set; } = "Tümü";
    public string SmeSubTabKbiTitle { get; set; } = "KBİ";
    public string SmeSubTabObiTitle { get; set; } = "OBİ";
    // Bireysel Alt Menü
    public string RetailSubTabAllTitle { get; set; } = "Tümü";
    public string RetailSubTabGeneralTitle { get; set; } = "Genel Kitle";
    public string RetailSubTabAffiliateTitle { get; set; } = "Afili";
    public string RetailSubTabPrivateTitle { get; set; } = "Özel Bankacılık";
}

public class DailyTargetReportTableHeaders
{
    public string ProductNameTitle { get; set; } = "Ürün Adı";
    public string LastYearTitle { get; set; } = "Geçen Yıl";
    public DateTime LastYearDate { get; set; } = new(2025, 12, 31);
    public string LastWeekTitle { get; set; } = "Geçen Hafta";
    public DateTime LastWeekDate { get; set; } = new(2026, 2, 13);
    public string PrevDayTitle { get; set; } = "Önceki Gün (T-2)";
    public DateTime PrevDayDate { get; set; } = new(2026, 2, 17);
    public string YesterdayTitle { get; set; } = "Dün (T-1)";
    public DateTime YesterdayDate { get; set; } = new(2026, 2, 18);
    public string DiffByPrevDayTitle { get; set; } = "T-2'ye Göre";
    public string DiffByLastYearTitle { get; set; } = "Yıla Göre";
    public string DiffByLastWeekTitle { get; set; } = "Haftaya Göre";
}

public class MonthlyTargetReportTableHeaders
{
    public string ProductNameTitle { get; set; } = "Ürün Adı";
    public string MonthGroupTitle { get; set; } = "Şubat Ayı";
    public string YearGroupTitle { get; set; } = "Yıllık";
    public string MonthActualTitle { get; set; } = "Gerçekleşen";
    public string MonthTargetTitle { get; set; } = "Hedef";
    public string MonthHGTitle { get; set; } = "H/G";
    public string YearActualTitle { get; set; } = "Gerçekleşen";
    public string YearTargetTitle { get; set; } = "Hedef";
    public string YearHGTitle { get; set; } = "H/G";
}

public class DailyProduct
{
    public long ProductId { get; set; }
    public string ProductName { get; set; } = "";
    public double LastYearAmount { get; set; }
    public double LastWeekAmount { get; set; }
    public double PrevDayAmount { get; set; }
    public double YesterdayAmount { get; set; }
    public double? DiffByPrevDayAmount { get; set; }
    public double? DiffByLastYearAmount { get; set; }
    public double? DiffByLastWeekAmount { get; set; }
    public List<DailyProduct> SubProducts { get; set; } = new();
}

public class MonthlyProduct
{
    public long ProductId { get; set; }
    public string ProductName { get; set; } = "";
    public double MonthActualAmount { get; set; }
    public double MonthTargetAmount { get; set; }
    public double MonthRatio { get; set; }
    public double YearActualAmount { get; set; }
    public double YearTargetAmount { get; set; }
    public double YearRatio { get; set; }
    public List<MonthlyProduct> SubProducts { get; set; } = new();
}

public static class MockData
{
    public static List<DailyProduct> DailyProducts() => new()
    {
        new()
        {
            ProductId = 1, ProductName = "Toplam Mevduat",
            LastYearAmount = 125_000_000, LastWeekAmount = 132_500_000,
            PrevDayAmount = 133_200_000, YesterdayAmount = 133_800_000,
            DiffByPrevDayAmount = 600_000, DiffByLastYearAmount = 8_800_000, DiffByLastWeekAmount = 1_300_000,
            SubProducts = new()
            {
                new() { ProductId = 11, ProductName = "TL Mevduat", LastYearAmount = 75_000_000, LastWeekAmount = 80_200_000, PrevDayAmount = 80_800_000, YesterdayAmount = 81_100_000, DiffByPrevDayAmount = 300_000, DiffByLastYearAmount = 6_100_000, DiffByLastWeekAmount = 900_000 },
                new() { ProductId = 12, ProductName = "YP Mevduat", LastYearAmount = 50_000_000, LastWeekAmount = 52_300_000, PrevDayAmount = 52_400_000, YesterdayAmount = 52_700_000, DiffByPrevDayAmount = 300_000, DiffByLastYearAmount = 2_700_000, DiffByLastWeekAmount = 400_000 }
            }
        },
        new()
        {
            ProductId = 2, ProductName = "Toplam Kredi",
            LastYearAmount = 98_000_000, LastWeekAmount = 101_500_000,
            PrevDayAmount = 102_100_000, YesterdayAmount = 102_400_000,
            DiffByPrevDayAmount = 300_000, DiffByLastYearAmount = 4_400_000, DiffByLastWeekAmount = 900_000,
            SubProducts = new()
            {
                new() { ProductId = 21, ProductName = "Ticari Kredi", LastYearAmount = 45_000_000, LastWeekAmount = 46_800_000, PrevDayAmount = 47_000_000, YesterdayAmount = 47_200_000, DiffByPrevDayAmount = 200_000, DiffByLastYearAmount = 2_200_000, DiffByLastWeekAmount = 400_000 },
                new() { ProductId = 22, ProductName = "Bireysel Kredi", LastYearAmount = 53_000_000, LastWeekAmount = 54_700_000, PrevDayAmount = 55_100_000, YesterdayAmount = 55_200_000, DiffByPrevDayAmount = 100_000, DiffByLastYearAmount = 2_200_000, DiffByLastWeekAmount = 500_000 }
            }
        },
        new()
        {
            ProductId = 3, ProductName = "Toplam Fon",
            LastYearAmount = 42_000_000, LastWeekAmount = 44_100_000,
            PrevDayAmount = 44_300_000, YesterdayAmount = 44_500_000,
            DiffByPrevDayAmount = 200_000, DiffByLastYearAmount = 2_500_000, DiffByLastWeekAmount = 400_000
        },
        new()
        {
            ProductId = 4, ProductName = "Sigorta",
            LastYearAmount = 18_500_000, LastWeekAmount = 19_200_000,
            PrevDayAmount = 19_350_000, YesterdayAmount = 19_400_000,
            DiffByPrevDayAmount = 50_000, DiffByLastYearAmount = 900_000, DiffByLastWeekAmount = 200_000
        },
        new()
        {
            ProductId = 5, ProductName = "DBS",
            LastYearAmount = 8_200_000, LastWeekAmount = 8_600_000,
            PrevDayAmount = 8_650_000, YesterdayAmount = 8_700_000,
            DiffByPrevDayAmount = 50_000, DiffByLastYearAmount = 500_000, DiffByLastWeekAmount = 100_000
        },
        new()
        {
            ProductId = 6, ProductName = "Kredi Kartı",
            LastYearAmount = 31_000_000, LastWeekAmount = 32_500_000,
            PrevDayAmount = 32_700_000, YesterdayAmount = 32_900_000,
            DiffByPrevDayAmount = 200_000, DiffByLastYearAmount = 1_900_000, DiffByLastWeekAmount = 400_000,
            SubProducts = new()
            {
                new() { ProductId = 61, ProductName = "Yeni Kart", LastYearAmount = 12_000_000, LastWeekAmount = 12_800_000, PrevDayAmount = 12_900_000, YesterdayAmount = 13_000_000, DiffByPrevDayAmount = 100_000, DiffByLastYearAmount = 1_000_000, DiffByLastWeekAmount = 200_000 },
                new() { ProductId = 62, ProductName = "Ciro", LastYearAmount = 19_000_000, LastWeekAmount = 19_700_000, PrevDayAmount = 19_800_000, YesterdayAmount = 19_900_000, DiffByPrevDayAmount = 100_000, DiffByLastYearAmount = 900_000, DiffByLastWeekAmount = 200_000 }
            }
        },
        new()
        {
            ProductId = 7, ProductName = "POS",
            LastYearAmount = 5_400_000, LastWeekAmount = 5_650_000,
            PrevDayAmount = 5_680_000, YesterdayAmount = 5_710_000,
            DiffByPrevDayAmount = 30_000, DiffByLastYearAmount = 310_000, DiffByLastWeekAmount = 60_000
        },
        new()
        {
            ProductId = 8, ProductName = "Müşteri Sayısı",
            LastYearAmount = 2_450_000, LastWeekAmount = 2_520_000,
            PrevDayAmount = 2_525_000, YesterdayAmount = 2_528_000,
            DiffByPrevDayAmount = 3_000, DiffByLastYearAmount = 78_000, DiffByLastWeekAmount = 8_000
        }
    };

    public static List<MonthlyProduct> MonthlyProducts() => new()
    {
        new()
        {
            ProductId = 1, ProductName = "Toplam Mevduat",
            MonthActualAmount = 133_800_000, MonthTargetAmount = 140_000_000, MonthRatio = 0.956,
            YearActualAmount = 133_800_000, YearTargetAmount = 150_000_000, YearRatio = 0.892,
            SubProducts = new()
            {
                new() { ProductId = 11, ProductName = "TL Mevduat", MonthActualAmount = 81_100_000, MonthTargetAmount = 85_000_000, MonthRatio = 0.954, YearActualAmount = 81_100_000, YearTargetAmount = 90_000_000, YearRatio = 0.901 },
                new() { ProductId = 12, ProductName = "YP Mevduat", MonthActualAmount = 52_700_000, MonthTargetAmount = 55_000_000, MonthRatio = 0.958, YearActualAmount = 52_700_000, YearTargetAmount = 60_000_000, YearRatio = 0.878 }
            }
        },
        new()
        {
            ProductId = 2, ProductName = "Toplam Kredi",
            MonthActualAmount = 102_400_000, MonthTargetAmount = 105_000_000, MonthRatio = 0.975,
            YearActualAmount = 102_400_000, YearTargetAmount = 120_000_000, YearRatio = 0.853,
            SubProducts = new()
            {
                new() { ProductId = 21, ProductName = "Ticari Kredi", MonthActualAmount = 47_200_000, MonthTargetAmount = 48_000_000, MonthRatio = 0.983, YearActualAmount = 47_200_000, YearTargetAmount = 55_000_000, YearRatio = 0.858 },
                new() { ProductId = 22, ProductName = "Bireysel Kredi", MonthActualAmount = 55_200_000, MonthTargetAmount = 57_000_000, MonthRatio = 0.968, YearActualAmount = 55_200_000, YearTargetAmount = 65_000_000, YearRatio = 0.849 }
            }
        },
        new()
        {
            ProductId = 3, ProductName = "Toplam Fon",
            MonthActualAmount = 44_500_000, MonthTargetAmount = 45_000_000, MonthRatio = 0.989,
            YearActualAmount = 44_500_000, YearTargetAmount = 50_000_000, YearRatio = 0.890
        },
        new()
        {
            ProductId = 4, ProductName = "Sigorta",
            MonthActualAmount = 19_400_000, MonthTargetAmount = 20_000_000, MonthRatio = 0.970,
            YearActualAmount = 19_400_000, YearTargetAmount = 24_000_000, YearRatio = 0.808
        },
        new()
        {
            ProductId = 5, ProductName = "DBS",
            MonthActualAmount = 8_700_000, MonthTargetAmount = 9_000_000, MonthRatio = 0.967,
            YearActualAmount = 8_700_000, YearTargetAmount = 10_000_000, YearRatio = 0.870
        },
        new()
        {
            ProductId = 6, ProductName = "Kredi Kartı",
            MonthActualAmount = 32_900_000, MonthTargetAmount = 30_000_000, MonthRatio = 1.097,
            YearActualAmount = 32_900_000, YearTargetAmount = 35_000_000, YearRatio = 0.940,
            SubProducts = new()
            {
                new() { ProductId = 61, ProductName = "Yeni Kart", MonthActualAmount = 13_000_000, MonthTargetAmount = 12_000_000, MonthRatio = 1.083, YearActualAmount = 13_000_000, YearTargetAmount = 14_000_000, YearRatio = 0.929 },
                new() { ProductId = 62, ProductName = "Ciro", MonthActualAmount = 19_900_000, MonthTargetAmount = 18_000_000, MonthRatio = 1.106, YearActualAmount = 19_900_000, YearTargetAmount = 21_000_000, YearRatio = 0.948 }
            }
        },
        new()
        {
            ProductId = 7, ProductName = "POS",
            MonthActualAmount = 5_710_000, MonthTargetAmount = 6_000_000, MonthRatio = 0.952,
            YearActualAmount = 5_710_000, YearTargetAmount = 7_000_000, YearRatio = 0.816
        },
        new()
        {
            ProductId = 8, ProductName = "Müşteri Sayısı",
            MonthActualAmount = 2_528_000, MonthTargetAmount = 2_600_000, MonthRatio = 0.972,
            YearActualAmount = 2_528_000, YearTargetAmount = 2_800_000, YearRatio = 0.903
        }
    };

    public static List<FilterItem> BolgeFilters() => new()
    {
        new() { Code = "B001", Name = "İstanbul Anadolu" },
        new() { Code = "B002", Name = "İstanbul Avrupa" },
        new() { Code = "B003", Name = "Ankara" },
        new() { Code = "B004", Name = "İzmir" },
        new() { Code = "B005", Name = "Antalya" },
        new() { Code = "B006", Name = "Bursa" },
        new() { Code = "B007", Name = "Adana" },
        new() { Code = "B008", Name = "Trabzon" }
    };

    public static List<FilterItem> SubeFilters() => new()
    {
        new() { Code = "S001", Name = "Kadıköy Şubesi" },
        new() { Code = "S002", Name = "Ataşehir Şubesi" },
        new() { Code = "S003", Name = "Maltepe Şubesi" },
        new() { Code = "S004", Name = "Üsküdar Şubesi" },
        new() { Code = "S005", Name = "Beşiktaş Şubesi" },
        new() { Code = "S006", Name = "Şişli Şubesi" }
    };

    public static List<FilterItem> PortfoyFilters() => new()
    {
        new() { Code = "P001", Name = "Portföy A" },
        new() { Code = "P002", Name = "Portföy B" },
        new() { Code = "P003", Name = "Portföy C" },
        new() { Code = "P004", Name = "Portföy D" }
    };
}
