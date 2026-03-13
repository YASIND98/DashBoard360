using DashboardTsy.Application.TargetReport;

namespace DashboardTsy.Infrastructure.TargetReport;

public static class MockTargetReportData
{
    public static GetTargetReportMenuTextsResponse GetMenuTexts()
        => new()
        {
            ScreenTitle = "Hedef Raporları",
            TabAllTitle = "Tümü",
            TabCorporateTitle = "Kurumsal",
            TabCommercialTitle = "Ticari",
            TabSmeTitle = "KOBİ",
            TabAgricultureTitle = "Tarım",
            TabRetailTitle = "Bireysel",
            SmeSubTabAllTitle = "Tümü",
            SmeSubTabKbiTitle = "KBİ",
            SmeSubTabObiTitle = "OBİ",
            RetailSubTabAllTitle = "Tümü",
            RetailSubTabGeneralTitle = "Genel Kitle",
            RetailSubTabAffiliateTitle = "Afili",
            RetailSubTabPrivateTitle = "Özel Bankacılık"
        };

    public static IReadOnlyList<GetTargetReportFiltersItem> GetFilters(int filterId)
        => filterId switch
        {
            0 => new List<GetTargetReportFiltersItem>
            {
                new() { Code = "1", Name = "Marmara" },
                new() { Code = "2", Name = "Ege" },
                new() { Code = "3", Name = "İç Anadolu" }
            },
            1 => new List<GetTargetReportFiltersItem>
            {
                new() { Code = "1001", Name = "İstanbul - Merkez" },
                new() { Code = "1002", Name = "Ankara - Kızılay" },
                new() { Code = "1003", Name = "İzmir - Konak" }
            },
            _ => new List<GetTargetReportFiltersItem>
            {
                new() { Code = "501", Name = "Portföy A" },
                new() { Code = "502", Name = "Portföy B" },
                new() { Code = "503", Name = "Portföy C" }
            }
        };

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

    public static GetDailyTargetReportTableHeadersResponse GetDailyHeaders(DateTime today)
    {
        var t = today.Date;
        return new GetDailyTargetReportTableHeadersResponse
        {
            ProductNameTitle = "Ürün Adı",
            LastYearTitle = "Geçen Yıl",
            LastYearDate = t.AddYears(-1),
            LastWeekTitle = "Geçen Hafta",
            LastWeekDate = t.AddDays(-7),
            PrevDayTitle = "Önceki Gün (T-2)",
            PrevDayDate = t.AddDays(-2),
            YesterdayTitle = "Dün (T-1)",
            YesterdayDate = t.AddDays(-1),
            TodayTitle = "Bugün (T)",
            TodayDate = t,
            DiffByPrevDayTitle = "T-2'ye Göre",
            DiffByLastYearTitle = "Yıla Göre",
            DiffByLastWeekTitle = "Haftaya Göre"
        };
    }

    public static GetDailyTargetReportResponse GetDailyReport(GetDailyTargetReportRequest request)
    {
        var t = (request.ReportDate == default ? DateTime.Today : request.ReportDate).Date;

        var root1 = new GetDailyTargetReportResponse.Product
        {
            ProductId = 10,
            ProductName = "Kredi",
            LastYearAmount = 120,
            LastYearDate = t.AddYears(-1),
            LastWeekAmount = 130,
            LastWeekDate = t.AddDays(-7),
            PrevDayAmount = 140,
            PrevDayDate = t.AddDays(-2),
            YesterdayAmount = 150,
            YesterdayDate = t.AddDays(-1),
            TodayDate = t
        };
        var sub11 = new GetDailyTargetReportResponse.Product
        {
            ProductId = 11,
            ProductName = "İhtiyaç Kredisi",
            LastYearAmount = 40,
            LastYearDate = t.AddYears(-1),
            LastWeekAmount = 45,
            LastWeekDate = t.AddDays(-7),
            PrevDayAmount = 47,
            PrevDayDate = t.AddDays(-2),
            YesterdayAmount = 49,
            YesterdayDate = t.AddDays(-1),
            TodayDate = t
        };
        var sub12 = new GetDailyTargetReportResponse.Product
        {
            ProductId = 12,
            ProductName = "Konut Kredisi",
            LastYearAmount = 80,
            LastYearDate = t.AddYears(-1),
            LastWeekAmount = 85,
            LastWeekDate = t.AddDays(-7),
            PrevDayAmount = 93,
            PrevDayDate = t.AddDays(-2),
            YesterdayAmount = 101,
            YesterdayDate = t.AddDays(-1),
            TodayDate = t
        };
        root1.SubProducts.Add(sub11);
        root1.SubProducts.Add(sub12);

        var root2 = new GetDailyTargetReportResponse.Product
        {
            ProductId = 20,
            ProductName = "Mevduat",
            LastYearAmount = 200,
            LastYearDate = t.AddYears(-1),
            LastWeekAmount = 210,
            LastWeekDate = t.AddDays(-7),
            PrevDayAmount = 215,
            PrevDayDate = t.AddDays(-2),
            YesterdayAmount = 220,
            YesterdayDate = t.AddDays(-1),
            TodayDate = t
        };

        // Simulate Today amounts by reusing Yesterday + some delta
        SetDailyTodayAndDiffs(root1, todayAmount: 155);
        SetDailyTodayAndDiffs(sub11, todayAmount: 52);
        SetDailyTodayAndDiffs(sub12, todayAmount: 103);
        SetDailyTodayAndDiffs(root2, todayAmount: 218);

        var roots = new List<GetDailyTargetReportResponse.Product> { root1, root2 };

        if (!string.IsNullOrWhiteSpace(request.SearchText))
        {
            var q = request.SearchText.Trim();
            roots = FilterDailyTreeByName(roots, q);
        }

        if (!request.ShowDifferences)
            ClearDailyDiffs(roots);

        roots = SortDailyTree(roots, request.SortBy, request.IsAscending);

        return new GetDailyTargetReportResponse { Products = roots };
    }

    public static GetMonthlyTargetReportTableHeadersResponse GetMonthlyHeaders(DateTime reportDate)
    {
        var dt = (reportDate == default ? DateTime.Today : reportDate).Date;
        return new GetMonthlyTargetReportTableHeadersResponse
        {
            ProductNameTitle = "Ürün Adı",
            MonthGroupTitle = dt.ToString("MMMM") + " Ayı",
            YearGroupTitle = "Yıllık",
            MonthActualTitle = "Gerçekleşen",
            MonthTargetTitle = "Hedef",
            MonthHGTitle = "H/G",
            YearActualTitle = "Gerçekleşen",
            YearTargetTitle = "Hedef",
            YearHGTitle = "H/G"
        };
    }

    public static GetMonthlyTargetReportResponse GetMonthlyReport(GetMonthlyTargetReportRequest request)
    {
        var root1 = new GetMonthlyTargetReportResponse.Product
        {
            ProductId = 100,
            ProductName = "Kredi",
            MonthActualAmount = 850,
            MonthTargetAmount = 1000,
            MonthRatio = 0.85,
            YearActualAmount = 4200,
            YearTargetAmount = 5000,
            YearRatio = 0.84
        };
        root1.SubProducts.Add(new GetMonthlyTargetReportResponse.Product
        {
            ProductId = 101,
            ProductName = "İhtiyaç Kredisi",
            MonthActualAmount = 300,
            MonthTargetAmount = 350,
            MonthRatio = 0.857,
            YearActualAmount = 1400,
            YearTargetAmount = 1700,
            YearRatio = 0.823
        });
        root1.SubProducts.Add(new GetMonthlyTargetReportResponse.Product
        {
            ProductId = 102,
            ProductName = "Konut Kredisi",
            MonthActualAmount = 550,
            MonthTargetAmount = 650,
            MonthRatio = 0.846,
            YearActualAmount = 2800,
            YearTargetAmount = 3300,
            YearRatio = 0.848
        });

        var root2 = new GetMonthlyTargetReportResponse.Product
        {
            ProductId = 200,
            ProductName = "Mevduat",
            MonthActualAmount = 1100,
            MonthTargetAmount = 1000,
            MonthRatio = 1.10,
            YearActualAmount = 5100,
            YearTargetAmount = 4800,
            YearRatio = 1.0625
        };

        var roots = new List<GetMonthlyTargetReportResponse.Product> { root1, root2 };

        if (!string.IsNullOrWhiteSpace(request.SearchText))
        {
            var q = request.SearchText.Trim();
            roots = FilterMonthlyTreeByName(roots, q);
        }

        roots = SortMonthlyTree(roots, request.SortBy, request.IsAscending);

        return new GetMonthlyTargetReportResponse { Products = roots };
    }

    public static IReadOnlyList<GetProductivityReportTabItem> GetProductivityReportTabs(GetProductivityReportTabsRequest request)
    {
        // Basit bir örnek hiyerarşi: 2 ana sekme + her birinin altında alt sekmeler.
        var tabs = new List<GetProductivityReportTabItem>
        {
            new() { TabId = 1, TabName = "Genel Verimlilik", ParentId = 0, TabLevel = 0 },
            new() { TabId = 2, TabName = "Rol Bazlı Verimlilik", ParentId = 0, TabLevel = 0 },

            new() { TabId = 3, TabName = "Şube Bazlı", ParentId = 1, TabLevel = 1 },
            new() { TabId = 4, TabName = "Bölge Bazlı", ParentId = 1, TabLevel = 1 },

            new() { TabId = 5, TabName = "Müdür", ParentId = 2, TabLevel = 1 },
            new() { TabId = 6, TabName = "Portföy Yöneticisi", ParentId = 2, TabLevel = 1 }
        };

        // İleride FilterType'a göre filtrelenebilir.
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
                ThreeMonthHgRegion = 0.90m,
                ThreeMonthHgBankAverage = 0.85m
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
                ThreeMonthHgRegion = 0.92m,
                ThreeMonthHgBankAverage = 0.86m
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
                ThreeMonthHgRegion = 0.88m,
                ThreeMonthHgBankAverage = 0.83m
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
                CurrentPeriodBankAverageValue = 55m,
                ThreeMonthHgBranchValue = 0.91m,
                ThreeMonthHgRegionAverageValue = 0.88m,
                ThreeMonthHgBankAverageValue = 0.85m
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
                CurrentPeriodBankAverageValue = 28m,
                ThreeMonthHgBranchValue = 0.93m,
                ThreeMonthHgRegionAverageValue = 0.89m,
                ThreeMonthHgBankAverageValue = 0.86m
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
                CurrentPeriodBankAverageValue = 27m,
                ThreeMonthHgBranchValue = 0.89m,
                ThreeMonthHgRegionAverageValue = 0.87m,
                ThreeMonthHgBankAverageValue = 0.84m
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
                RealizationBankAverage = 0.82m,
                YtdChangeRegion = 0.05m,
                YtdChangeBankAverage = 0.03m,
                QtdChangeRegion = 0.02m,
                QtdChangeBankAverage = 0.01m
            },
            new()
            {
                Id = 2,
                ParentId = 1,
                LevelNo = 1,
                SortOrder = 1,
                ProductName = $"{segmentName} - Ürün A",
                RealizationRegion = 0.90m,
                RealizationBankAverage = 0.85m,
                YtdChangeRegion = 0.06m,
                YtdChangeBankAverage = 0.04m,
                QtdChangeRegion = 0.03m,
                QtdChangeBankAverage = 0.02m
            },
            new()
            {
                Id = 3,
                ParentId = 1,
                LevelNo = 1,
                SortOrder = 2,
                ProductName = $"{segmentName} - Ürün B",
                RealizationRegion = 0.86m,
                RealizationBankAverage = 0.80m,
                YtdChangeRegion = 0.04m,
                YtdChangeBankAverage = 0.02m,
                QtdChangeRegion = 0.01m,
                QtdChangeBankAverage = 0.00m
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
                RealizationBankAverageValue = 1100m,
                TargetValue = 1200m,
                HgRate = 0.92m,
                NetGrowthRegionValue = 85m,
                NetGrowthBankAverageValue = 70m,
                YtdRegionValue = 14200m,
                YtdBankAverageValue = 12800m,
                QtdRegionValue = 3800m,
                QtdBankAverageValue = 3400m
            },
            new()
            {
                Id = 2,
                ParentId = 1,
                LevelNo = 1,
                SortOrder = 1,
                ProductName = $"{segmentName} - Ürün A",
                RealizationRegionValue = 650m,
                RealizationBankAverageValue = 580m,
                TargetValue = 600m,
                HgRate = 0.93m,
                NetGrowthRegionValue = 45m,
                NetGrowthBankAverageValue = 38m,
                YtdRegionValue = 7200m,
                YtdBankAverageValue = 6500m,
                QtdRegionValue = 1950m,
                QtdBankAverageValue = 1750m
            },
            new()
            {
                Id = 3,
                ParentId = 1,
                LevelNo = 1,
                SortOrder = 2,
                ProductName = $"{segmentName} - Ürün B",
                RealizationRegionValue = 600m,
                RealizationBankAverageValue = 520m,
                TargetValue = 600m,
                HgRate = 0.90m,
                NetGrowthRegionValue = 40m,
                NetGrowthBankAverageValue = 32m,
                YtdRegionValue = 7000m,
                YtdBankAverageValue = 6300m,
                QtdRegionValue = 1850m,
                QtdBankAverageValue = 1650m
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
                BankValue = 0.23m,
                RetailValue = 0.22m,
                KobiValue = 0.26m,
                AgricultureValue = 0.20m,
                CommercialValue = 0.28m
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
                BankValue = 0.11m,
                RetailValue = 0.10m,
                KobiValue = 0.13m,
                AgricultureValue = 0.09m,
                CommercialValue = 0.14m
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
                BankValue = 0.075m,
                RetailValue = 0.082m,
                KobiValue = 0.07m,
                AgricultureValue = 0.06m,
                CommercialValue = 0.09m
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
                BankValue = 0.23m,
                RetailValue = 0.22m,
                KobiValue = 0.25m,
                AgricultureValue = 0.19m,
                CommercialValue = 0.27m
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
                BankValue = 0.107m,
                RetailValue = 0.10m,
                KobiValue = 0.115m,
                AgricultureValue = 0.09m,
                CommercialValue = 0.12m
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
                BankValue = 0.068m,
                RetailValue = 0.072m,
                KobiValue = 0.065m,
                AgricultureValue = 0.055m,
                CommercialValue = 0.08m
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
                RealizationBankAverageValue = 3600m,
                HgBranchValue = 0.948m,
                HgRegionAverageValue = 0.917m,
                HgBankAverageValue = 0.857m,
                RetailValue = 1800m,
                KobiValue = 1200m,
                AgricultureValue = 450m,
                CommercialValue = 380m,
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
                RealizationBankAverageValue = 1900m,
                HgBranchValue = 0.945m,
                HgRegionAverageValue = 0.909m,
                HgBankAverageValue = 0.864m,
                RetailValue = 950m,
                KobiValue = 620m,
                AgricultureValue = 240m,
                CommercialValue = 200m,
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
                RealizationBankAverageValue = 680m,
                HgBranchValue = 0.95m,
                HgRegionAverageValue = 0.90m,
                HgBankAverageValue = 0.85m,
                RetailValue = 350m,
                KobiValue = 250m,
                AgricultureValue = 80m,
                CommercialValue = 60m,
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
                RealizationBankAverageValue = 1100m,
                YtdNominalChangeBranchValue = 85m,
                YtdNominalChangeRegionAverageValue = 72m,
                YtdNominalChangeBankAverageValue = 65m,
                QtdNominalChangeBranchValue = 22m,
                QtdNominalChangeRegionAverageValue = 18m,
                QtdNominalChangeBankAverageValue = 15m
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
                RealizationBankAverageValue = 860m,
                YtdNominalChangeBranchValue = 62m,
                YtdNominalChangeRegionAverageValue = 55m,
                YtdNominalChangeBankAverageValue = 48m,
                QtdNominalChangeBranchValue = 18m,
                QtdNominalChangeRegionAverageValue = 14m,
                QtdNominalChangeBankAverageValue = 12m
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
                RealizationBankAverageValue = 240m,
                YtdNominalChangeBranchValue = 23m,
                YtdNominalChangeRegionAverageValue = 17m,
                YtdNominalChangeBankAverageValue = 17m,
                QtdNominalChangeBranchValue = 4m,
                QtdNominalChangeRegionAverageValue = 4m,
                QtdNominalChangeBankAverageValue = 3m
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
                RealizationBankAverageValue = 110_000_000m,
                TargetValue = 120_000_000m,
                HgRate = 1.042m,
                NetGrowthBranchValue = 8_500_000m,
                NetGrowthRegionAverageValue = 7_200_000m,
                NetGrowthBankAverageValue = 6_500_000m,
                YtdBranchValue = 1_250_000_000m,
                YtdRegionValue = 1_180_000_000m,
                YtdBankValue = 1_100_000_000m,
                QtdBranchValue = 380_000_000m,
                QtdRegionValue = 355_000_000m,
                QtdBankValue = 330_000_000m
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
                RealizationBankAverageValue = 75_000_000m,
                TargetValue = 82_000_000m,
                HgRate = 1.037m,
                NetGrowthBranchValue = 5_200_000m,
                NetGrowthRegionAverageValue = 4_500_000m,
                NetGrowthBankAverageValue = 4_000_000m,
                YtdBranchValue = 850_000_000m,
                YtdRegionValue = 800_000_000m,
                YtdBankValue = 750_000_000m,
                QtdBranchValue = 258_000_000m,
                QtdRegionValue = 242_000_000m,
                QtdBankValue = 228_000_000m
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
                RealizationBankAverageValue = 35_000_000m,
                TargetValue = 38_000_000m,
                HgRate = 1.053m,
                NetGrowthBranchValue = 3_300_000m,
                NetGrowthRegionAverageValue = 2_700_000m,
                NetGrowthBankAverageValue = 2_500_000m,
                YtdBranchValue = 400_000_000m,
                YtdRegionValue = 380_000_000m,
                YtdBankValue = 350_000_000m,
                QtdBranchValue = 122_000_000m,
                QtdRegionValue = 113_000_000m,
                QtdBankValue = 102_000_000m
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
                RealizationBankAverageValue = 4600m,
                HgRegionValue = 0.96m,
                HgBankAverageValue = 0.92m,
                RetailValue = 2200m,
                KobiValue = 1200m,
                AgricultureValue = 400m,
                CommercialValue = 800m,
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
                RealizationBankAverageValue = 2600m,
                HgRegionValue = 0.96m,
                HgBankAverageValue = 0.93m,
                RetailValue = 1300m,
                KobiValue = 700m,
                AgricultureValue = 250m,
                CommercialValue = 400m,
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
                RealizationBankAverageValue = 1100m,
                HgRegionValue = 0.96m,
                HgBankAverageValue = 0.92m,
                RetailValue = 600m,
                KobiValue = 300m,
                AgricultureValue = 100m,
                CommercialValue = 250m,
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
                RatioBankAverageValue = 0.032m,
                NetReturnRegionValue = 420m,
                NetReturnBankAverageValue = 380m,
                NetReturnHgRegionValue = 0.97m,
                NetReturnHgBankAverageValue = 0.92m
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
                RatioBankAverageValue = 0.0165m,
                NetReturnRegionValue = 210m,
                NetReturnBankAverageValue = 195m,
                NetReturnHgRegionValue = 0.97m,
                NetReturnHgBankAverageValue = 0.92m
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
                RatioBankAverageValue = 0.0155m,
                NetReturnRegionValue = 210m,
                NetReturnBankAverageValue = 185m,
                NetReturnHgRegionValue = 0.97m,
                NetReturnHgBankAverageValue = 0.91m
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
                RatioBankAverageValue = 0.032m,
                NetReturnBranchValue = 435m,
                NetReturnRegionAverageValue = 420m,
                NetReturnBankAverageValue = 380m,
                NetReturnHgBranchValue = 0.98m,
                NetReturnHgRegionAverageValue = 0.97m,
                NetReturnHgBankAverageValue = 0.92m
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
                RatioBankAverageValue = 0.0165m,
                NetReturnBranchValue = 220m,
                NetReturnRegionAverageValue = 210m,
                NetReturnBankAverageValue = 195m,
                NetReturnHgBranchValue = 0.98m,
                NetReturnHgRegionAverageValue = 0.97m,
                NetReturnHgBankAverageValue = 0.92m
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
                RatioBankAverageValue = 0.0155m,
                NetReturnBranchValue = 215m,
                NetReturnRegionAverageValue = 210m,
                NetReturnBankAverageValue = 185m,
                NetReturnHgBranchValue = 0.98m,
                NetReturnHgRegionAverageValue = 0.97m,
                NetReturnHgBankAverageValue = 0.91m
            }
        };

        return items;
    }

    public static IReadOnlyList<GetProductivityReportTableHeaderItem> GetProductivityReportTableHeaders(GetProductivityReportTableHeadersRequest request)
    {
        // Çok basit, 2 seviyeli örnek header yapısı:
        // [Genel Bilgiler] - parent
        //   [Müşteri Adı]
        //   [Şube Adı]
        // [Performans] - parent
        //   [Adet]
        //   [Hacim]
        //   [Karlılık]

        var headers = new List<GetProductivityReportTableHeaderItem>
        {
            new() { Id = 1, HeaderName = "Genel Bilgiler", ParentId = 0, OrderNo = 1 },
            new() { Id = 2, HeaderName = "Müşteri Adı", ParentId = 1, OrderNo = 1 },
            new() { Id = 3, HeaderName = "Şube Adı", ParentId = 1, OrderNo = 2 },

            new() { Id = 4, HeaderName = "Performans", ParentId = 0, OrderNo = 2 },
            new() { Id = 5, HeaderName = "Adet", ParentId = 4, OrderNo = 1 },
            new() { Id = 6, HeaderName = "Hacim", ParentId = 4, OrderNo = 2 },
            new() { Id = 7, HeaderName = "Karlılık", ParentId = 4, OrderNo = 3 }
        };

        // İleride MainTabId / MidTabId / SubTabId / FilterType'a göre filtrelenebilir.
        return headers;
    }

    private static void SetDailyTodayAndDiffs(GetDailyTargetReportResponse.Product p, double todayAmount)
    {
        p.DiffByPrevDayAmount = todayAmount - p.PrevDayAmount;
        p.DiffByLastYearAmount = todayAmount - p.LastYearAmount;
        p.DiffByLastWeekAmount = todayAmount - p.LastWeekAmount;
    }

    private static List<GetDailyTargetReportResponse.Product> FilterDailyTreeByName(List<GetDailyTargetReportResponse.Product> nodes, string q)
    {
        bool Matches(GetDailyTargetReportResponse.Product p)
            => (p.ProductName ?? string.Empty).Contains(q, StringComparison.OrdinalIgnoreCase);

        List<GetDailyTargetReportResponse.Product> Recurse(IEnumerable<GetDailyTargetReportResponse.Product> list)
        {
            var result = new List<GetDailyTargetReportResponse.Product>();
            foreach (var n in list)
            {
                var filteredChildren = Recurse(n.SubProducts ?? new List<GetDailyTargetReportResponse.Product>());
                if (Matches(n) || filteredChildren.Count > 0)
                {
                    n.SubProducts = filteredChildren;
                    result.Add(n);
                }
            }
            return result;
        }

        return Recurse(nodes);
    }

    private static void ClearDailyDiffs(IEnumerable<GetDailyTargetReportResponse.Product> nodes)
    {
        foreach (var n in nodes)
        {
            n.DiffByPrevDayAmount = null;
            n.DiffByLastYearAmount = null;
            n.DiffByLastWeekAmount = null;
            if (n.SubProducts != null && n.SubProducts.Count > 0)
                ClearDailyDiffs(n.SubProducts);
        }
    }

    private static List<GetDailyTargetReportResponse.Product> SortDailyTree(List<GetDailyTargetReportResponse.Product> nodes, int? sortBy, bool asc)
    {
        Func<GetDailyTargetReportResponse.Product, object> key = sortBy switch
        {
            1 => p => p.ProductName ?? string.Empty,
            2 => p => p.LastYearAmount,
            3 => p => p.LastWeekAmount,
            4 => p => p.PrevDayAmount,
            5 => p => p.YesterdayAmount,
            _ => p => p.ProductId
        };

        var ordered = (asc ? nodes.OrderBy(key) : nodes.OrderByDescending(key)).ToList();
        foreach (var n in ordered)
            if (n.SubProducts != null && n.SubProducts.Count > 0)
                n.SubProducts = SortDailyTree(n.SubProducts, sortBy, asc);
        return ordered;
    }

    private static List<GetMonthlyTargetReportResponse.Product> FilterMonthlyTreeByName(List<GetMonthlyTargetReportResponse.Product> nodes, string q)
    {
        bool Matches(GetMonthlyTargetReportResponse.Product p)
            => (p.ProductName ?? string.Empty).Contains(q, StringComparison.OrdinalIgnoreCase);

        List<GetMonthlyTargetReportResponse.Product> Recurse(IEnumerable<GetMonthlyTargetReportResponse.Product> list)
        {
            var result = new List<GetMonthlyTargetReportResponse.Product>();
            foreach (var n in list)
            {
                var filteredChildren = Recurse(n.SubProducts ?? new List<GetMonthlyTargetReportResponse.Product>());
                if (Matches(n) || filteredChildren.Count > 0)
                {
                    n.SubProducts = filteredChildren;
                    result.Add(n);
                }
            }
            return result;
        }

        return Recurse(nodes);
    }

    private static List<GetMonthlyTargetReportResponse.Product> SortMonthlyTree(List<GetMonthlyTargetReportResponse.Product> nodes, int? sortBy, bool asc)
    {
        Func<GetMonthlyTargetReportResponse.Product, object> key = sortBy switch
        {
            1 => p => p.ProductName ?? string.Empty,
            2 => p => p.MonthActualAmount,
            3 => p => p.MonthTargetAmount,
            4 => p => p.MonthRatio,
            5 => p => p.YearActualAmount,
            6 => p => p.YearTargetAmount,
            7 => p => p.YearRatio,
            _ => p => p.ProductId
        };

        var ordered = (asc ? nodes.OrderBy(key) : nodes.OrderByDescending(key)).ToList();
        foreach (var n in ordered)
            if (n.SubProducts != null && n.SubProducts.Count > 0)
                n.SubProducts = SortMonthlyTree(n.SubProducts, sortBy, asc);
        return ordered;
    }
}

