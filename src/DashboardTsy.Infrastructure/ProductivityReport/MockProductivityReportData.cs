using System.Globalization;
using DashboardTsy.Application.ProductivityReport.Requests;
using DashboardTsy.Application.ProductivityReport.Responses;

namespace DashboardTsy.Infrastructure.ProductivityReport;

public static class MockProductivityReportData
{
    public static IReadOnlyList<GetReportRegionFilterItem> GetReportRegionFilters()
        => new List<GetReportRegionFilterItem>
        {
            new() { Code = "1", Name = "Marmara" },
            new() { Code = "2", Name = "Ege" },
            new() { Code = "3", Name = "İç Anadolu" },
            new() { Code = "4", Name = "Akdeniz" },
            new() { Code = "5", Name = "Karadeniz" }
        };

    public static IReadOnlyList<GetReportBranchFilterItem> GetReportBranchFilters()
        => new List<GetReportBranchFilterItem>
        {
            new() { Code = "1001", Name = "İstanbul - Merkez", RegionCode = "1" },
            new() { Code = "1002", Name = "Ankara - Kızılay", RegionCode = "3" },
            new() { Code = "1003", Name = "İzmir - Konak", RegionCode = "2" },
            new() { Code = "1004", Name = "Antalya - Merkez", RegionCode = "4" },
            new() { Code = "1005", Name = "Trabzon - Merkez", RegionCode = "5" }
        };

    public static IReadOnlyList<GetProductivityScoreCardReportHeaderItem> GetProductivityScoreCardReportHeaders(GetProductivityScoreCardReportHeadersRequest request)
    {
        var culture = CultureInfo.GetCultureInfo("tr-TR");
        var baseDate = request.ReportDate == default ? DateTime.Today : request.ReportDate.Date;

        // Son üç ay: ReportDate, -1 ay, -2 ay
        string MonthLabel(DateTime d) => d.ToString("MMMM yyyy", culture);

        var month1 = MonthLabel(baseDate);
        var month2 = MonthLabel(baseDate.AddMonths(-1));
        var month3 = MonthLabel(baseDate.AddMonths(-2));

        // FilterType'a göre ana başlığın Region/Branch olması dışında yapı aynı
        var isBranch = request.FilterType == 2;
        var mainTitle = isBranch ? "Şube Müdürü Skor Kartı" : "Bölge Müdürü Skor Kartı";
        var rolesTitle = isBranch ? "Şube Rolleri Skor Kartı" : "Bölge Rolleri Skor Kartı";

        var headers = new List<GetProductivityScoreCardReportHeaderItem>
        {
            new() { Id = 1,  HeaderName = mainTitle,      ParentId = 0,  OrderNo = 1 },
            new() { Id = 2,  HeaderName = "Adı Soyadı",   ParentId = 1,  OrderNo = 2 },
            new() { Id = 3,  HeaderName = month1,         ParentId = 1,  OrderNo = 3 },
            new() { Id = 4,  HeaderName = month2,         ParentId = 1,  OrderNo = 4 },
            new() { Id = 5,  HeaderName = month3,         ParentId = 1,  OrderNo = 5 },

            new() { Id = 6,  HeaderName = rolesTitle,     ParentId = 0,  OrderNo = 6 },
            new() { Id = 7,  HeaderName = "Kurumsal",     ParentId = 6,  OrderNo = 7 },
            new() { Id = 8,  HeaderName = "Ticari",       ParentId = 6,  OrderNo = 8 },
            new() { Id = 9,  HeaderName = "KBİ",          ParentId = 6,  OrderNo = 9 },
            new() { Id = 10, HeaderName = "OBİ",          ParentId = 6,  OrderNo = 10 },
            new() { Id = 11, HeaderName = "Tarım",        ParentId = 6,  OrderNo = 11 },
            new() { Id = 12, HeaderName = "Kitle",        ParentId = 6,  OrderNo = 12 },
            new() { Id = 13, HeaderName = "Afili",        ParentId = 6,  OrderNo = 13 },
            new() { Id = 14, HeaderName = "ÖB",           ParentId = 6,  OrderNo = 14 },

            new() { Id = 15, HeaderName = "NPS",          ParentId = 0,  OrderNo = 15 },
            new() { Id = 16, HeaderName = isBranch ? "Şube" : "Bölge", ParentId = 15, OrderNo = 16 },
            new() { Id = 17, HeaderName = "Banka",        ParentId = 15, OrderNo = 17 }
        };

        return headers;
    }
    public static IReadOnlyList<GetProductivityReportTabItem> GetProductivityReportTabs(GetProductivityReportTabsRequest request)
    {
        var tabs = new List<GetProductivityReportTabItem>
        {
            new() { TabId = 1, TabName = "Genel", ParentId = 0, TabLevel = 1 },
            new() { TabId = 2, TabName = "Adet", ParentId = 0, TabLevel = 1 },
            new() { TabId = 3, TabName = "Hacim", ParentId = 0, TabLevel = 1 },
            new() { TabId = 4, TabName = "Karlılık", ParentId = 0, TabLevel = 1 },
            new() { TabId = 10, TabName = "Müşteri", ParentId = 2, TabLevel = 2 },
            new() { TabId = 11, TabName = "Kredi Kartı", ParentId = 2, TabLevel = 2 },
            new() { TabId = 12, TabName = "POS", ParentId = 2, TabLevel = 2 },
            new() { TabId = 20, TabName = "Tümü", ParentId = 10, TabLevel = 3 },
            new() { TabId = 21, TabName = "Kurumsal", ParentId = 10, TabLevel = 3 },
            new() { TabId = 22, TabName = "Ticari", ParentId = 10, TabLevel = 3 },
            new() { TabId = 23, TabName = "KOBİ", ParentId = 10, TabLevel = 3 },
            new() { TabId = 24, TabName = "Tarım", ParentId = 10, TabLevel = 3 },
            new() { TabId = 25, TabName = "Bireysel", ParentId = 10, TabLevel = 3 },
            new() { TabId = 30, TabName = "Tümü", ParentId = 3, TabLevel = 2 },
            new() { TabId = 31, TabName = "Kurumsal", ParentId = 3, TabLevel = 2 },
            new() { TabId = 32, TabName = "Ticari", ParentId = 3, TabLevel = 2 },
            new() { TabId = 33, TabName = "KOBİ", ParentId = 3, TabLevel = 2 },
            new() { TabId = 34, TabName = "Tarım", ParentId = 3, TabLevel = 2 },
            new() { TabId = 35, TabName = "Bireysel", ParentId = 3, TabLevel = 2 },
            new() { TabId = 40, TabName = "Toplam", ParentId = 4, TabLevel = 2 },
            new() { TabId = 41, TabName = "Spread Yönetimi", ParentId = 4, TabLevel = 2 }
        };

        return tabs;
    }

    public static IReadOnlyList<GetProductivityGeneralRegionReportItem> GetProductivityGeneralRegionReport(GetProductivityGeneralRegionReportRequest request)
    {
        // Çok basit örnek mock hiyerarşi:
        // Level 0: Bölge
        // Level 1: Şubeler

        var items = new List<GetProductivityGeneralRegionReportItem>
        {
            new()
            {
                Id = 1,
                ParentId = null,
                LevelNo = 0,
                SortOrder = 1,
                BranchName = "Marmara Bölgesi",
                FirstMonthRealizationRate = 0.85m,
                SecondMonthRealizationRate = 0.88m,
                ThirdMonthRealizationRate = 0.90m,
                CorporateRate = 0.82m,
                CommercialRate = 0.86m,
                KbiRate = 0.80m,
                ObiRate = 0.78m,
                AgricultureRate = 0.75m,
                MassRate = 0.88m,
                AffluentRate = 0.92m,
                PrivateBankingRate = 0.95m
            },
            new()
            {
                Id = 2,
                ParentId = 1,
                LevelNo = 1,
                SortOrder = 1,
                BranchName = "İstanbul - Merkez",
                FirstMonthRealizationRate = 0.87m,
                SecondMonthRealizationRate = 0.89m,
                ThirdMonthRealizationRate = 0.91m,
                CorporateRate = 0.84m,
                CommercialRate = 0.88m,
                KbiRate = 0.81m,
                ObiRate = 0.79m,
                AgricultureRate = 0.00m,
                MassRate = 0.89m,
                AffluentRate = 0.93m,
                PrivateBankingRate = 0.96m
            },
            new()
            {
                Id = 3,
                ParentId = 1,
                LevelNo = 1,
                SortOrder = 2,
                BranchName = "Bursa - Nilüfer",
                FirstMonthRealizationRate = 0.82m,
                SecondMonthRealizationRate = 0.85m,
                ThirdMonthRealizationRate = 0.88m,
                CorporateRate = 0.80m,
                CommercialRate = 0.83m,
                KbiRate = 0.78m,
                ObiRate = 0.76m,
                AgricultureRate = 0.72m,
                MassRate = 0.86m,
                AffluentRate = 0.90m,
                PrivateBankingRate = 0.92m
            }
        };

        // İleride RegionCode / SortBy / IsAscending'e göre filtre/sırala.
        return items;
    }

    public static IReadOnlyList<GetProductivityCountCardPosRegionReportItem> GetProductivityCountCardPosRegionReport(GetProductivityCountCardPosRegionReportRequest request)
    {
        // TabId: 1=KrediKartı, 2=POS
        var isCard = request.TabId == 1;
        var prefix = isCard ? "Kredi Kartı" : "POS";

        var items = new List<GetProductivityCountCardPosRegionReportItem>
        {
            new()
            {
                Id = 1,
                ParentId = null,
                LevelNo = 0,
                SortOrder = 1,
                ProductName = $"{prefix} - Toplam",
                CurrentMonthRegionValue = 120m,
                CurrentMonthBankAverage = 100m,
                CurrentMonthBankAverageDiff = 20m,
                ThreeMonthHgRegion = 0.90m,
                ThreeMonthHgBankAverage = 0.85m,
                ThreeMonthHgBankAverageDiff = 0.05m
            },
            new()
            {
                Id = 2,
                ParentId = 1,
                LevelNo = 1,
                SortOrder = 1,
                ProductName = $"{prefix} - Ürün A",
                CurrentMonthRegionValue = 60m,
                CurrentMonthBankAverage = 55m,
                CurrentMonthBankAverageDiff = 5m,
                ThreeMonthHgRegion = 0.92m,
                ThreeMonthHgBankAverage = 0.86m,
                ThreeMonthHgBankAverageDiff = 0.06m
            },
            new()
            {
                Id = 3,
                ParentId = 1,
                LevelNo = 1,
                SortOrder = 2,
                ProductName = $"{prefix} - Ürün B",
                CurrentMonthRegionValue = 60m,
                CurrentMonthBankAverage = 45m,
                CurrentMonthBankAverageDiff = 15m,
                ThreeMonthHgRegion = 0.88m,
                ThreeMonthHgBankAverage = 0.83m,
                ThreeMonthHgBankAverageDiff = 0.05m
            }
        };

        // İleride RegionCode / SortBy / IsAscending'e göre filtre/sırala.
        return items;
    }

    public static IReadOnlyList<GetProductivityCountCardPosBranchReportItem> GetProductivityCountCardPosBranchReport(GetProductivityCountCardPosBranchReportRequest request)
    {
        var isCard = request.TabId == 1;
        var prefix = isCard ? "Kredi Kartı" : "POS";

        var items = new List<GetProductivityCountCardPosBranchReportItem>
        {
            new()
            {
                Id = 1,
                ParentId = null,
                LevelNo = 0,
                SortOrder = 1,
                ProductName = $"{prefix} - Toplam",
                CurrentPeriodBranchValue = 75m,
                CurrentPeriodRegionAverageValue = 60m,
                CurrentPeriodRegionAverageValueDiff = 15m,
                CurrentPeriodBankAverageValue = 55m,
                CurrentPeriodBankAverageValueDiff = 0m,
                ThreeMonthHgBranchValue = 0.91m,
                ThreeMonthHgRegionAverageValue = 0.88m,
                ThreeMonthHgRegionAverageValueDiff = 0.03m,
                ThreeMonthHgBankAverageValue = 0.85m,
                ThreeMonthHgBankAverageValueDiff = 0m
            },
            new()
            {
                Id = 2,
                ParentId = 1,
                LevelNo = 1,
                SortOrder = 1,
                ProductName = $"{prefix} - Ürün A",
                CurrentPeriodBranchValue = 40m,
                CurrentPeriodRegionAverageValue = 32m,
                CurrentPeriodRegionAverageValueDiff = 8m,
                CurrentPeriodBankAverageValue = 28m,
                CurrentPeriodBankAverageValueDiff = 0m,
                ThreeMonthHgBranchValue = 0.93m,
                ThreeMonthHgRegionAverageValue = 0.89m,
                ThreeMonthHgRegionAverageValueDiff = 0.04m,
                ThreeMonthHgBankAverageValue = 0.86m,
                ThreeMonthHgBankAverageValueDiff = 0m
            },
            new()
            {
                Id = 3,
                ParentId = 1,
                LevelNo = 1,
                SortOrder = 2,
                ProductName = $"{prefix} - Ürün B",
                CurrentPeriodBranchValue = 35m,
                CurrentPeriodRegionAverageValue = 28m,
                CurrentPeriodRegionAverageValueDiff = 7m,
                CurrentPeriodBankAverageValue = 27m,
                CurrentPeriodBankAverageValueDiff = 0m,
                ThreeMonthHgBranchValue = 0.89m,
                ThreeMonthHgRegionAverageValue = 0.87m,
                ThreeMonthHgRegionAverageValueDiff = 0.02m,
                ThreeMonthHgBankAverageValue = 0.84m,
                ThreeMonthHgBankAverageValueDiff = 0m
            }
        };

        return items;
    }

    public static IReadOnlyList<GetProductivityCountCustomerRegionReportItem> GetProductivityCountCustomerRegionReport(GetProductivityCountCustomerRegionReportRequest request)
    {
        // SubTabId: Tümü / Kurumsal / Ticari / KOBİ / Tarım / Bireysel
        var segmentNames = new[] { "Tümü", "Kurumsal", "Ticari", "KOBİ", "Tarım", "Bireysel" };
        var segmentName = request.SubTabId >= 0 && request.SubTabId < segmentNames.Length
            ? segmentNames[request.SubTabId]
            : "Tümü";

        var items = new List<GetProductivityCountCustomerRegionReportItem>
        {
            new()
            {
                Id = 1,
                ParentId = null,
                LevelNo = 0,
                SortOrder = 1,
                ProductName = $"{segmentName} - Toplam",
                RealizationRegion = 0.88m,
                RealizationRegionDiff = 0.06m,
                RealizationBankAverage = 0.82m,
                RealizationBankAverageDiff = 0.00m,
                YtdChangeRegion = 0.05m,
                YtdChangeRegionDiff = 0.02m,
                YtdChangeBankAverage = 0.03m,
                YtdChangeBankAverageDiff = 0.00m,
                QtdChangeRegion = 0.02m,
                QtdChangeRegionDiff = 0.01m,
                QtdChangeBankAverage = 0.01m,
                QtdChangeBankAverageDiff = 0.00m
            },
            new()
            {
                Id = 2,
                ParentId = 1,
                LevelNo = 1,
                SortOrder = 1,
                ProductName = $"{segmentName} - Ürün A",
                RealizationRegion = 0.90m,
                RealizationRegionDiff = 0.05m,
                RealizationBankAverage = 0.85m,
                RealizationBankAverageDiff = 0.00m,
                YtdChangeRegion = 0.06m,
                YtdChangeRegionDiff = 0.02m,
                YtdChangeBankAverage = 0.04m,
                YtdChangeBankAverageDiff = 0.00m,
                QtdChangeRegion = 0.03m,
                QtdChangeRegionDiff = 0.01m,
                QtdChangeBankAverage = 0.02m,
                QtdChangeBankAverageDiff = 0.00m
            },
            new()
            {
                Id = 3,
                ParentId = 1,
                LevelNo = 1,
                SortOrder = 2,
                ProductName = $"{segmentName} - Ürün B",
                RealizationRegion = 0.86m,
                RealizationRegionDiff = 0.06m,
                RealizationBankAverage = 0.80m,
                RealizationBankAverageDiff = 0.00m,
                YtdChangeRegion = 0.04m,
                YtdChangeRegionDiff = 0.02m,
                YtdChangeBankAverage = 0.02m,
                YtdChangeBankAverageDiff = 0.00m,
                QtdChangeRegion = 0.01m,
                QtdChangeRegionDiff = 0.01m,
                QtdChangeBankAverage = 0.00m,
                QtdChangeBankAverageDiff = 0.00m
            }
        };

        return items;
    }

    public static IReadOnlyList<GetProductivityVolumeRegionReportItem> GetProductivityVolumeRegionReport(GetProductivityVolumeRegionReportRequest request)
    {
        var segmentNames = new[] { "Tümü", "Kurumsal", "Ticari", "KOBİ", "Tarım", "Bireysel" };
        var segmentName = request.SubTabId >= 0 && request.SubTabId < segmentNames.Length
            ? segmentNames[request.SubTabId]
            : "Tümü";

        var items = new List<GetProductivityVolumeRegionReportItem>
        {
            new()
            {
                Id = 1,
                ParentId = null,
                LevelNo = 0,
                SortOrder = 1,
                ProductName = $"{segmentName} - Hacim Toplam",
                RealizationRegionValue = 1250m,
                RealizationRegionDiff = 150m,
                RealizationBankAverageValue = 1100m,
                RealizationBankAverageDiff = 0m,
                TargetValue = 1200m,
                HgRate = 0.92m,
                NetGrowthRegionValue = 85m,
                NetGrowthRegionDiff = 15m,
                NetGrowthBankAverageValue = 70m,
                NetGrowthBankAverageDiff = 0m,
                YtdRegionValue = 14200m,
                YtdRegionDiff = 1400m,
                YtdBankAverageValue = 12800m,
                YtdBankAverageDiff = 0m,
                QtdRegionValue = 3800m,
                QtdRegionDiff = 400m,
                QtdBankAverageValue = 3400m,
                QtdBankAverageDiff = 0m
            },
            new()
            {
                Id = 2,
                ParentId = 1,
                LevelNo = 1,
                SortOrder = 1,
                ProductName = $"{segmentName} - Ürün A",
                RealizationRegionValue = 650m,
                RealizationRegionDiff = 70m,
                RealizationBankAverageValue = 580m,
                RealizationBankAverageDiff = 0m,
                TargetValue = 600m,
                HgRate = 0.93m,
                NetGrowthRegionValue = 45m,
                NetGrowthRegionDiff = 7m,
                NetGrowthBankAverageValue = 38m,
                NetGrowthBankAverageDiff = 0m,
                YtdRegionValue = 7200m,
                YtdRegionDiff = 700m,
                YtdBankAverageValue = 6500m,
                YtdBankAverageDiff = 0m,
                QtdRegionValue = 1950m,
                QtdRegionDiff = 200m,
                QtdBankAverageValue = 1750m,
                QtdBankAverageDiff = 0m
            },
            new()
            {
                Id = 3,
                ParentId = 1,
                LevelNo = 1,
                SortOrder = 2,
                ProductName = $"{segmentName} - Ürün B",
                RealizationRegionValue = 600m,
                RealizationRegionDiff = 80m,
                RealizationBankAverageValue = 520m,
                RealizationBankAverageDiff = 0m,
                TargetValue = 600m,
                HgRate = 0.90m,
                NetGrowthRegionValue = 40m,
                NetGrowthRegionDiff = 8m,
                NetGrowthBankAverageValue = 32m,
                NetGrowthBankAverageDiff = 0m,
                YtdRegionValue = 7000m,
                YtdRegionDiff = 700m,
                YtdBankAverageValue = 6300m,
                YtdBankAverageDiff = 0m,
                QtdRegionValue = 1850m,
                QtdRegionDiff = 200m,
                QtdBankAverageValue = 1650m,
                QtdBankAverageDiff = 0m
            }
        };

        return items;
    }

    public static IReadOnlyList<GetProductivityProfitRatioRegionReportItem> GetProductivityProfitRatioRegionReport(GetProductivityProfitRatioRegionReportRequest request)
    {
        var items = new List<GetProductivityProfitRatioRegionReportItem>
        {
            new()
            {
                Id = 1,
                ParentId = null,
                LevelNo = 0,
                SortOrder = 1,
                RatioName = "Karlılık Oranı - Toplam",
                TargetValue = 0.25m,
                RegionValue = 0.24m,
                RegionValueDiff = 0.01m,
                BankValue = 0.23m,
                BankValueDiff = 0m,
                RetailValue = 0.22m,
                KobiValue = 0.26m,
                AgricultureValue = 0.20m,
                AgricultureValueDiff = 0.03m,
                CommercialValue = 0.28m,
                CommercialValueDiff = 0.05m
            },
            new()
            {
                Id = 2,
                ParentId = 1,
                LevelNo = 1,
                SortOrder = 1,
                RatioName = "Net Faiz Marjı",
                TargetValue = 0.12m,
                RegionValue = 0.115m,
                RegionValueDiff = 0.005m,
                BankValue = 0.11m,
                BankValueDiff = 0m,
                RetailValue = 0.10m,
                KobiValue = 0.13m,
                AgricultureValue = 0.09m,
                AgricultureValueDiff = 0.02m,
                CommercialValue = 0.14m,
                CommercialValueDiff = 0.03m
            },
            new()
            {
                Id = 3,
                ParentId = 1,
                LevelNo = 1,
                SortOrder = 2,
                RatioName = "Komisyon Geliri Oranı",
                TargetValue = 0.08m,
                RegionValue = 0.078m,
                RegionValueDiff = 0.003m,
                BankValue = 0.075m,
                BankValueDiff = 0m,
                RetailValue = 0.082m,
                KobiValue = 0.07m,
                AgricultureValue = 0.06m,
                AgricultureValueDiff = 0.01m,
                CommercialValue = 0.09m,
                CommercialValueDiff = 0.015m
            }
        };

        return items;
    }

    public static IReadOnlyList<GetProductivityProfitRatioBranchReportItem> GetProductivityProfitRatioBranchReport(GetProductivityProfitRatioBranchReportRequest request)
    {
        var items = new List<GetProductivityProfitRatioBranchReportItem>
        {
            new()
            {
                Id = 1,
                ParentId = null,
                LevelNo = 0,
                SortOrder = 1,
                RatioName = "Şube Karlılık Oranı - Toplam",
                TargetValue = 0.24m,
                RegionValue = 0.235m,
                RegionValueDiff = 0.005m,
                BankValue = 0.23m,
                BankValueDiff = 0m,
                RetailValue = 0.22m,
                KobiValue = 0.25m,
                AgricultureValue = 0.19m,
                AgricultureValueDiff = 0.04m,
                CommercialValue = 0.27m,
                CommercialValueDiff = 0.04m
            },
            new()
            {
                Id = 2,
                ParentId = 1,
                LevelNo = 1,
                SortOrder = 1,
                RatioName = "Net Faiz Marjı (Şube)",
                TargetValue = 0.11m,
                RegionValue = 0.108m,
                RegionValueDiff = 0.001m,
                BankValue = 0.107m,
                BankValueDiff = 0m,
                RetailValue = 0.10m,
                KobiValue = 0.115m,
                AgricultureValue = 0.09m,
                AgricultureValueDiff = 0.017m,
                CommercialValue = 0.12m,
                CommercialValueDiff = 0.013m
            },
            new()
            {
                Id = 3,
                ParentId = 1,
                LevelNo = 1,
                SortOrder = 2,
                RatioName = "Komisyon Geliri Oranı (Şube)",
                TargetValue = 0.07m,
                RegionValue = 0.069m,
                RegionValueDiff = 0.001m,
                BankValue = 0.068m,
                BankValueDiff = 0m,
                RetailValue = 0.072m,
                KobiValue = 0.065m,
                AgricultureValue = 0.055m,
                AgricultureValueDiff = 0.013m,
                CommercialValue = 0.08m,
                CommercialValueDiff = 0.012m
            }
        };

        return items;
    }

    public static IReadOnlyList<GetProductivityProfitTotalBranchReportItem> GetProductivityProfitTotalBranchReport(GetProductivityProfitTotalBranchReportRequest request)
    {
        var items = new List<GetProductivityProfitTotalBranchReportItem>
        {
            new()
            {
                Id = 1,
                ParentId = null,
                LevelNo = 0,
                SortOrder = 1,
                Description = "Toplam Karlılık (Şube)",
                TargetValue = 4200m,
                RealizationBranchValue = 3980m,
                RealizationRegionAverageValue = 3850m,
                RealizationRegionAverageValueDiff = 250m,
                RealizationBankAverageValue = 3600m,
                RealizationBankAverageValueDiff = 0m,
                HgBranchValue = 0.948m,
                HgRegionAverageValue = 0.917m,
                HgRegionAverageValueDiff = 0.06m,
                HgBankAverageValue = 0.857m,
                HgBankAverageValueDiff = 0m,
                RetailValue = 1800m,
                KobiValue = 1200m,
                AgricultureValue = 450m,
                CommercialValue = 380m,
                CommercialValueDiff = 30m,
                PartnerValue = 150m
            },
            new()
            {
                Id = 2,
                ParentId = 1,
                LevelNo = 1,
                SortOrder = 1,
                Description = "Net Faiz Geliri",
                TargetValue = 2200m,
                RealizationBranchValue = 2080m,
                RealizationRegionAverageValue = 2000m,
                RealizationRegionAverageValueDiff = 100m,
                RealizationBankAverageValue = 1900m,
                RealizationBankAverageValueDiff = 0m,
                HgBranchValue = 0.945m,
                HgRegionAverageValue = 0.909m,
                HgRegionAverageValueDiff = 0.045m,
                HgBankAverageValue = 0.864m,
                HgBankAverageValueDiff = 0m,
                RetailValue = 950m,
                KobiValue = 620m,
                AgricultureValue = 240m,
                CommercialValue = 200m,
                CommercialValueDiff = 15m,
                PartnerValue = 70m
            },
            new()
            {
                Id = 3,
                ParentId = 1,
                LevelNo = 1,
                SortOrder = 2,
                Description = "Komisyon Geliri",
                TargetValue = 800m,
                RealizationBranchValue = 760m,
                RealizationRegionAverageValue = 720m,
                RealizationRegionAverageValueDiff = 40m,
                RealizationBankAverageValue = 680m,
                RealizationBankAverageValueDiff = 0m,
                HgBranchValue = 0.95m,
                HgRegionAverageValue = 0.90m,
                HgRegionAverageValueDiff = 0.05m,
                HgBankAverageValue = 0.85m,
                HgBankAverageValueDiff = 0m,
                RetailValue = 350m,
                KobiValue = 250m,
                AgricultureValue = 80m,
                CommercialValue = 60m,
                CommercialValueDiff = 5m,
                PartnerValue = 20m
            }
        };

        return items;
    }

    public static GetProductivityBranchScoreCardReportItem GetProductivityBranchScoreCardReport(GetProductivityBranchScoreCardReportRequest request)
    {
        return new GetProductivityBranchScoreCardReportItem
        {
            ManagerName = "Ahmet Yılmaz",
            FirstMonthScore = 78.5m,
            SecondMonthScore = 82.0m,
            ThirdMonthScore = 85.0m,
            CorporateScore = 80m,
            CommercialScore = 76m,
            KobiScore = 84m,
            ObiScore = 72m,
            AgricultureScore = 88m,
            MassScore = 79m,
            AffluentScore = 86m,
            PrivateBankingScore = 90m,
            BranchNpsScore = 72.5m,
            BankNpsScore = 68.0m
        };
    }

    public static GetProductivityRegionScoreCardReportItem GetProductivityRegionScoreCardReport(GetProductivityRegionScoreCardReportRequest request)
    {
        return new GetProductivityRegionScoreCardReportItem
        {
            ManagerName = "Bölge Müdürü - Ege",
            FirstMonthScore = 80.0m,
            SecondMonthScore = 83.5m,
            ThirdMonthScore = 87.0m,
            CorporateScore = 82m,
            CommercialScore = 79m,
            KobiScore = 85m,
            ObiScore = 74m,
            AgricultureScore = 90m,
            MassScore = 81m,
            AffluentScore = 88m,
            PrivateBankingScore = 91m,
            RegionNpsScore = 75.0m,
            BankNpsScore = 68.0m
        };
    }

    public static IReadOnlyList<GetProductivityCountCustomerBranchReportItem> GetProductivityCountCustomerBranchReport(GetProductivityCountCustomerBranchReportRequest request)
    {
        var items = new List<GetProductivityCountCustomerBranchReportItem>
        {
            new()
            {
                Id = 1,
                ParentId = null,
                LevelNo = 0,
                SortOrder = 1,
                ProductName = "Müşteri Sayısı - Toplam",
                RealizationBranchValue = 1250m,
                RealizationRegionAverageValue = 1180m,
                RealizationRegionAverageValueDiff = 80m,
                RealizationBankAverageValue = 1100m,
                RealizationBankAverageValueDiff = 0m,
                YtdNominalChangeBranchValue = 85m,
                YtdNominalChangeRegionAverageValue = 72m,
                YtdNominalChangeRegionAverageValueDiff = 13m,
                YtdNominalChangeBankAverageValue = 65m,
                YtdNominalChangeBankAverageValueDiff = 0m,
                QtdNominalChangeBranchValue = 22m,
                QtdNominalChangeRegionAverageValue = 18m,
                QtdNominalChangeRegionAverageValueDiff = 4m,
                QtdNominalChangeBankAverageValue = 15m,
                QtdNominalChangeBankAverageValueDiff = 0m
            },
            new()
            {
                Id = 2,
                ParentId = 1,
                LevelNo = 1,
                SortOrder = 1,
                ProductName = "Bireysel Müşteri",
                RealizationBranchValue = 980m,
                RealizationRegionAverageValue = 920m,
                RealizationRegionAverageValueDiff = 60m,
                RealizationBankAverageValue = 860m,
                RealizationBankAverageValueDiff = 0m,
                YtdNominalChangeBranchValue = 62m,
                YtdNominalChangeRegionAverageValue = 55m,
                YtdNominalChangeRegionAverageValueDiff = 10m,
                YtdNominalChangeBankAverageValue = 48m,
                YtdNominalChangeBankAverageValueDiff = 0m,
                QtdNominalChangeBranchValue = 18m,
                QtdNominalChangeRegionAverageValue = 14m,
                QtdNominalChangeRegionAverageValueDiff = 3m,
                QtdNominalChangeBankAverageValue = 12m,
                QtdNominalChangeBankAverageValueDiff = 0m
            },
            new()
            {
                Id = 3,
                ParentId = 1,
                LevelNo = 1,
                SortOrder = 2,
                ProductName = "Kurumsal Müşteri",
                RealizationBranchValue = 270m,
                RealizationRegionAverageValue = 260m,
                RealizationRegionAverageValueDiff = 20m,
                RealizationBankAverageValue = 240m,
                RealizationBankAverageValueDiff = 0m,
                YtdNominalChangeBranchValue = 23m,
                YtdNominalChangeRegionAverageValue = 17m,
                YtdNominalChangeRegionAverageValueDiff = 3m,
                YtdNominalChangeBankAverageValue = 17m,
                YtdNominalChangeBankAverageValueDiff = 0m,
                QtdNominalChangeBranchValue = 4m,
                QtdNominalChangeRegionAverageValue = 4m,
                QtdNominalChangeRegionAverageValueDiff = 1m,
                QtdNominalChangeBankAverageValue = 3m,
                QtdNominalChangeBankAverageValueDiff = 0m
            }
        };

        return items;
    }

    public static IReadOnlyList<GetProductivityVolumeBranchReportItem> GetProductivityVolumeBranchReport(GetProductivityVolumeBranchReportRequest request)
    {
        var items = new List<GetProductivityVolumeBranchReportItem>
        {
            new()
            {
                Id = 1,
                ParentId = null,
                LevelNo = 0,
                SortOrder = 1,
                ProductName = "Hacim - Toplam",
                RealizationBranchValue = 125_000_000m,
                RealizationRegionAverageValue = 118_000_000m,
                RealizationRegionAverageValueDiff = 15_000_000m,
                RealizationBankAverageValue = 110_000_000m,
                RealizationBankAverageValueDiff = 0m,
                TargetValue = 120_000_000m,
                HgRate = 1.042m,
                NetGrowthBranchValue = 8_500_000m,
                NetGrowthRegionAverageValue = 7_200_000m,
                NetGrowthRegionAverageValueDiff = 1_300_000m,
                NetGrowthBankAverageValue = 6_500_000m,
                NetGrowthBankAverageValueDiff = 0m,
                YtdBranchValue = 1_250_000_000m,
                YtdRegionValue = 1_180_000_000m,
                YtdRegionValueDiff = 150_000_000m,
                YtdBankValue = 1_100_000_000m,
                YtdBankValueDiff = 0m,
                QtdBranchValue = 380_000_000m,
                QtdRegionValue = 355_000_000m,
                QtdRegionValueDiff = 50_000_000m,
                QtdBankValue = 330_000_000m,
                QtdBankValueDiff = 0m
            },
            new()
            {
                Id = 2,
                ParentId = 1,
                LevelNo = 1,
                SortOrder = 1,
                ProductName = "Mevduat",
                RealizationBranchValue = 85_000_000m,
                RealizationRegionAverageValue = 80_000_000m,
                RealizationRegionAverageValueDiff = 10_000_000m,
                RealizationBankAverageValue = 75_000_000m,
                RealizationBankAverageValueDiff = 0m,
                TargetValue = 82_000_000m,
                HgRate = 1.037m,
                NetGrowthBranchValue = 5_200_000m,
                NetGrowthRegionAverageValue = 4_500_000m,
                NetGrowthRegionAverageValueDiff = 700_000m,
                NetGrowthBankAverageValue = 4_000_000m,
                NetGrowthBankAverageValueDiff = 0m,
                YtdBranchValue = 850_000_000m,
                YtdRegionValue = 800_000_000m,
                YtdRegionValueDiff = 100_000_000m,
                YtdBankValue = 750_000_000m,
                YtdBankValueDiff = 0m,
                QtdBranchValue = 258_000_000m,
                QtdRegionValue = 242_000_000m,
                QtdRegionValueDiff = 30_000_000m,
                QtdBankValue = 228_000_000m,
                QtdBankValueDiff = 0m
            },
            new()
            {
                Id = 3,
                ParentId = 1,
                LevelNo = 1,
                SortOrder = 2,
                ProductName = "Kredi",
                RealizationBranchValue = 40_000_000m,
                RealizationRegionAverageValue = 38_000_000m,
                RealizationRegionAverageValueDiff = 5_000_000m,
                RealizationBankAverageValue = 35_000_000m,
                RealizationBankAverageValueDiff = 0m,
                TargetValue = 38_000_000m,
                HgRate = 1.053m,
                NetGrowthBranchValue = 3_300_000m,
                NetGrowthRegionAverageValue = 2_700_000m,
                NetGrowthRegionAverageValueDiff = 600_000m,
                NetGrowthBankAverageValue = 2_500_000m,
                NetGrowthBankAverageValueDiff = 0m,
                YtdBranchValue = 400_000_000m,
                YtdRegionValue = 380_000_000m,
                YtdRegionValueDiff = 50_000_000m,
                YtdBankValue = 350_000_000m,
                YtdBankValueDiff = 0m,
                QtdBranchValue = 122_000_000m,
                QtdRegionValue = 113_000_000m,
                QtdRegionValueDiff = 20_000_000m,
                QtdBankValue = 102_000_000m,
                QtdBankValueDiff = 0m
            }
        };

        return items;
    }

    public static IReadOnlyList<GetProductivityProfitTotalRegionReportItem> GetProductivityProfitTotalRegionReport(GetProductivityProfitTotalRegionReportRequest request)
    {
        var items = new List<GetProductivityProfitTotalRegionReportItem>
        {
            new()
            {
                Id = 1,
                ParentId = null,
                LevelNo = 0,
                SortOrder = 1,
                Description = "Toplam Karlılık",
                TargetValue = 5000m,
                RealizationRegionValue = 4800m,
                RealizationRegionValueDiff = 200m,
                RealizationBankAverageValue = 4600m,
                RealizationBankAverageValueDiff = 0m,
                HgRegionValue = 0.96m,
                HgRegionValueDiff = 0.04m,
                HgBankAverageValue = 0.92m,
                HgBankAverageValueDiff = 0m,
                RetailValue = 2200m,
                KobiValue = 1200m,
                AgricultureValue = 400m,
                CommercialValue = 800m,
                CommercialValueDiff = 50m,
                PartnerValue = 200m
            },
            new()
            {
                Id = 2,
                ParentId = 1,
                LevelNo = 1,
                SortOrder = 1,
                Description = "Net Faiz Geliri",
                TargetValue = 2800m,
                RealizationRegionValue = 2700m,
                RealizationRegionValueDiff = 100m,
                RealizationBankAverageValue = 2600m,
                RealizationBankAverageValueDiff = 0m,
                HgRegionValue = 0.96m,
                HgRegionValueDiff = 0.03m,
                HgBankAverageValue = 0.93m,
                HgBankAverageValueDiff = 0m,
                RetailValue = 1300m,
                KobiValue = 700m,
                AgricultureValue = 250m,
                CommercialValue = 400m,
                CommercialValueDiff = 25m,
                PartnerValue = 50m
            },
            new()
            {
                Id = 3,
                ParentId = 1,
                LevelNo = 1,
                SortOrder = 2,
                Description = "Komisyon Geliri",
                TargetValue = 1200m,
                RealizationRegionValue = 1150m,
                RealizationRegionValueDiff = 50m,
                RealizationBankAverageValue = 1100m,
                RealizationBankAverageValueDiff = 0m,
                HgRegionValue = 0.96m,
                HgRegionValueDiff = 0.04m,
                HgBankAverageValue = 0.92m,
                HgBankAverageValueDiff = 0m,
                RetailValue = 600m,
                KobiValue = 300m,
                AgricultureValue = 100m,
                CommercialValue = 250m,
                CommercialValueDiff = 15m,
                PartnerValue = 100m
            }
        };

        return items;
    }

    public static IReadOnlyList<GetProductivityProfitSpreadManagementRegionReportItem> GetProductivityProfitSpreadManagementRegionReport(GetProductivityProfitSpreadManagementRegionReportRequest request)
    {
        var items = new List<GetProductivityProfitSpreadManagementRegionReportItem>
        {
            new()
            {
                Id = 1,
                ParentId = null,
                LevelNo = 0,
                SortOrder = 1,
                Description = "Spread Yönetimi - Toplam",
                SpreadValue = 0.035m,
                RatioRegionValue = 0.034m,
                RatioRegionValueDiff = 0.002m,
                RatioBankAverageValue = 0.032m,
                RatioBankAverageValueDiff = 0m,
                NetReturnRegionValue = 420m,
                NetReturnRegionValueDiff = 40m,
                NetReturnBankAverageValue = 380m,
                NetReturnBankAverageValueDiff = 0m,
                NetReturnHgRegionValue = 0.97m,
                NetReturnHgRegionValueDiff = 0.05m,
                NetReturnHgBankAverageValue = 0.92m,
                NetReturnHgBankAverageValueDiff = 0m
            },
            new()
            {
                Id = 2,
                ParentId = 1,
                LevelNo = 1,
                SortOrder = 1,
                Description = "Mevduat Spread",
                SpreadValue = 0.018m,
                RatioRegionValue = 0.0175m,
                RatioRegionValueDiff = 0.001m,
                RatioBankAverageValue = 0.0165m,
                RatioBankAverageValueDiff = 0m,
                NetReturnRegionValue = 210m,
                NetReturnRegionValueDiff = 15m,
                NetReturnBankAverageValue = 195m,
                NetReturnBankAverageValueDiff = 0m,
                NetReturnHgRegionValue = 0.97m,
                NetReturnHgRegionValueDiff = 0.05m,
                NetReturnHgBankAverageValue = 0.92m,
                NetReturnHgBankAverageValueDiff = 0m
            },
            new()
            {
                Id = 3,
                ParentId = 1,
                LevelNo = 1,
                SortOrder = 2,
                Description = "Kredi Spread",
                SpreadValue = 0.017m,
                RatioRegionValue = 0.0165m,
                RatioRegionValueDiff = 0.001m,
                RatioBankAverageValue = 0.0155m,
                RatioBankAverageValueDiff = 0m,
                NetReturnRegionValue = 210m,
                NetReturnRegionValueDiff = 25m,
                NetReturnBankAverageValue = 185m,
                NetReturnBankAverageValueDiff = 0m,
                NetReturnHgRegionValue = 0.97m,
                NetReturnHgRegionValueDiff = 0.06m,
                NetReturnHgBankAverageValue = 0.91m,
                NetReturnHgBankAverageValueDiff = 0m
            }
        };

        return items;
    }

    public static IReadOnlyList<GetProductivityProfitSpreadManagementBranchReportItem> GetProductivityProfitSpreadManagementBranchReport(GetProductivityProfitSpreadManagementBranchReportRequest request)
    {
        var items = new List<GetProductivityProfitSpreadManagementBranchReportItem>
        {
            new()
            {
                Id = 1,
                ParentId = null,
                LevelNo = 0,
                SortOrder = 1,
                Description = "Spread Yönetimi - Toplam (Şube)",
                SpreadValue = 0.036m,
                RatioBranchValue = 0.035m,
                RatioRegionAverageValue = 0.034m,
                RatioRegionAverageValueDiff = 0.002m,
                RatioBankAverageValue = 0.032m,
                RatioBankAverageValueDiff = 0m,
                NetReturnBranchValue = 435m,
                NetReturnRegionAverageValue = 420m,
                NetReturnRegionAverageValueDiff = 55m,
                NetReturnBankAverageValue = 380m,
                NetReturnBankAverageValueDiff = 0m,
                NetReturnHgBranchValue = 0.98m,
                NetReturnHgRegionAverageValue = 0.97m,
                NetReturnHgRegionAverageValueDiff = 0.06m,
                NetReturnHgBankAverageValue = 0.92m,
                NetReturnHgBankAverageValueDiff = 0m
            },
            new()
            {
                Id = 2,
                ParentId = 1,
                LevelNo = 1,
                SortOrder = 1,
                Description = "Mevduat Spread",
                SpreadValue = 0.019m,
                RatioBranchValue = 0.0185m,
                RatioRegionAverageValue = 0.0175m,
                RatioRegionAverageValueDiff = 0.001m,
                RatioBankAverageValue = 0.0165m,
                RatioBankAverageValueDiff = 0m,
                NetReturnBranchValue = 220m,
                NetReturnRegionAverageValue = 210m,
                NetReturnRegionAverageValueDiff = 25m,
                NetReturnBankAverageValue = 195m,
                NetReturnBankAverageValueDiff = 0m,
                NetReturnHgBranchValue = 0.98m,
                NetReturnHgRegionAverageValue = 0.97m,
                NetReturnHgRegionAverageValueDiff = 0.06m,
                NetReturnHgBankAverageValue = 0.92m,
                NetReturnHgBankAverageValueDiff = 0m
            },
            new()
            {
                Id = 3,
                ParentId = 1,
                LevelNo = 1,
                SortOrder = 2,
                Description = "Kredi Spread",
                SpreadValue = 0.017m,
                RatioBranchValue = 0.0165m,
                RatioRegionAverageValue = 0.0165m,
                RatioRegionAverageValueDiff = 0.001m,
                RatioBankAverageValue = 0.0155m,
                RatioBankAverageValueDiff = 0m,
                NetReturnBranchValue = 215m,
                NetReturnRegionAverageValue = 210m,
                NetReturnRegionAverageValueDiff = 30m,
                NetReturnBankAverageValue = 185m,
                NetReturnBankAverageValueDiff = 0m,
                NetReturnHgBranchValue = 0.98m,
                NetReturnHgRegionAverageValue = 0.97m,
                NetReturnHgRegionAverageValueDiff = 0.06m,
                NetReturnHgBankAverageValue = 0.91m,
                NetReturnHgBankAverageValueDiff = 0m
            }
        };

        return items;
    }

    public static IReadOnlyList<GetProductivityReportTableHeaderItem> GetProductivityReportTableHeaders(GetProductivityReportTableHeadersRequest request)
    {
        // Çok basit, 2 seviyeli örnek header yapısı:
        // [Genel Bilgiler] - parent (sortable = false)
        //   [Müşteri Adı]  (sortable = true)
        //   [Şube Adı]     (sortable = true)
        // [Performans] - parent (sortable = false)
        //   [Adet]         (sortable = true)
        //   [Hacim]        (sortable = true)
        //   [Karlılık]     (sortable = true)

        var headers = new List<GetProductivityReportTableHeaderItem>
        {
            new() { Id = 1, HeaderName = "Genel Bilgiler", ParentId = 0, OrderNo = 1, Sortable = false },
            new() { Id = 2, HeaderName = "Müşteri Adı",    ParentId = 1, OrderNo = 1, Sortable = true },
            new() { Id = 3, HeaderName = "Şube Adı",       ParentId = 1, OrderNo = 2, Sortable = true },

            new() { Id = 4, HeaderName = "Performans",     ParentId = 0, OrderNo = 2, Sortable = false },
            new() { Id = 5, HeaderName = "Adet",           ParentId = 4, OrderNo = 1, Sortable = true },
            new() { Id = 6, HeaderName = "Hacim",          ParentId = 4, OrderNo = 2, Sortable = true },
            new() { Id = 7, HeaderName = "Karlılık",       ParentId = 4, OrderNo = 3, Sortable = true }
        };

        // İleride MainTabId / MidTabId / SubTabId / FilterType'a göre filtrelenebilir.
        return headers;
    }
}
