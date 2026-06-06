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

        return new List<GetProductivityScoreCardReportHeaderItem>
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
    }

    public static IReadOnlyList<GetProductivityReportTabItem> GetProductivityReportTabs(GetProductivityReportTabsRequest request)
    {
        return new List<GetProductivityReportTabItem>
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
    }

    public static GetProductivityGeneralRegionReportResponse GetProductivityGeneralRegionReport(GetProductivityGeneralRegionReportRequest request)
    {
        var root = new GetProductivityGeneralRegionReportResponse.GetProductivityGeneralRegionReportItem
        {
            Id = 1,
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
        };

        root.SubProducts.Add(new GetProductivityGeneralRegionReportResponse.GetProductivityGeneralRegionReportItem
        {
            Id = 2,
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
        });

        root.SubProducts.Add(new GetProductivityGeneralRegionReportResponse.GetProductivityGeneralRegionReportItem
        {
            Id = 3,
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
        });

        var roots = new List<GetProductivityGeneralRegionReportResponse.GetProductivityGeneralRegionReportItem> { root };
        roots = SortGeneralRegionTree(roots, request.SortBy, request.IsAscending);

        return new GetProductivityGeneralRegionReportResponse
        {
            GetProductivityGeneralRegionReports = roots
        };
    }

    public static GetProductivityCountCardPosRegionReportResponse GetProductivityCountCardPosRegionReport(GetProductivityCountCardPosRegionReportRequest request)
    {
        // TabId: 1=KrediKartı, 2=POS
        var isCard = request.TabId == 1;
        var prefix = isCard ? "Kredi Kartı" : "POS";

        var root = new GetProductivityCountCardPosRegionReportResponse.GetProductivityCountCardPosRegionReportItem
        {
            Id = 1,
            ProductName = $"{prefix} - Toplam",
            CurrentMonthRegionValue = 120m,
            CurrentMonthBankAverage = 100m,
            CurrentMonthBankAverageDiff = 20m,
            ThreeMonthHgRegion = 0.90m,
            ThreeMonthHgBankAverage = 0.85m,
            ThreeMonthHgBankAverageDiff = 0.05m
        };

        root.SubProducts.Add(new GetProductivityCountCardPosRegionReportResponse.GetProductivityCountCardPosRegionReportItem
        {
            Id = 2,
            ProductName = $"{prefix} - Ürün A",
            CurrentMonthRegionValue = 60m,
            CurrentMonthBankAverage = 55m,
            CurrentMonthBankAverageDiff = 5m,
            ThreeMonthHgRegion = 0.92m,
            ThreeMonthHgBankAverage = 0.86m,
            ThreeMonthHgBankAverageDiff = 0.06m
        });

        root.SubProducts.Add(new GetProductivityCountCardPosRegionReportResponse.GetProductivityCountCardPosRegionReportItem
        {
            Id = 3,
            ProductName = $"{prefix} - Ürün B",
            CurrentMonthRegionValue = 60m,
            CurrentMonthBankAverage = 45m,
            CurrentMonthBankAverageDiff = 15m,
            ThreeMonthHgRegion = 0.88m,
            ThreeMonthHgBankAverage = 0.83m,
            ThreeMonthHgBankAverageDiff = 0.05m
        });

        var roots = new List<GetProductivityCountCardPosRegionReportResponse.GetProductivityCountCardPosRegionReportItem> { root };
        roots = SortCountCardPosRegionTree(roots, request.SortBy, request.IsAscending);

        return new GetProductivityCountCardPosRegionReportResponse
        {
            GetProductivityCountCardPosRegionReports = roots
        };
    }

    public static IReadOnlyList<GetProductivityCountCardPosRatioRegionReportItem> GetProductivityCountCardPosRatioRegionReport(GetProductivityCountCardPosRatioRegionReportRequest request)
    {
        var items = new List<GetProductivityCountCardPosRatioRegionReportItem>
        {
            new()
            {
                Id = 1,
                RatioName = request.TabId == 1 ? "Toplam Kredi Kartı" : "Toplam Satış",
                PreviousQuarterRegionValue = 35m,
                CurrentRegionValue = 24m,
                CurrentRegionDiff = 11m,
                CurrentBankAverageValue = 20m,
                CurrentBankAverageDiff = 15m
            },
            new()
            {
                Id = 2,
                RatioName = request.TabId == 1 ? "Aktif Kredi Kartı" : "Aktif Satış",
                PreviousQuarterRegionValue = 35m,
                CurrentRegionValue = 24m,
                CurrentRegionDiff = 11m,
                CurrentBankAverageValue = 20m,
                CurrentBankAverageDiff = 15m
            },
            new()
            {
                Id = 3,
                RatioName = "Aktiflik Oranı %",
                PreviousQuarterRegionValue = 35m,
                CurrentRegionValue = 24m,
                CurrentRegionDiff = 11m,
                CurrentBankAverageValue = 20m,
                CurrentBankAverageDiff = 15m
            }
        };

        if (request.TabId == 1)
        {
            items.Add(new GetProductivityCountCardPosRatioRegionReportItem
            {
                Id = 4,
                RatioName = "Anında Kart Şifre Alım %",
                PreviousQuarterRegionValue = 35m,
                CurrentRegionValue = 24m,
                CurrentRegionDiff = 11m,
                CurrentBankAverageValue = 20m,
                CurrentBankAverageDiff = 15m
            });
        }

        return items;
    }

    public static GetProductivityCountCardPosRatioRegionReportTableHeadersItem GetProductivityCountCardPosRatioRegionReportTableHeaders(GetProductivityCountCardPosRatioRegionReportTableHeadersRequest request)
    {
        return new GetProductivityCountCardPosRatioRegionReportTableHeadersItem
        {
            RowNumberTitle = "#",
            RatioNameTitle = "Oran Adı",
            PreviousQuarterRegionTitle = "Önceki Çeyrek (Bölge)",
            CurrentRegionTitle = "Bölge",
            CurrentBankAverageTitle = "Banka"
        };
    }

    public static GetProductivityCountCardPosBranchReportResponse GetProductivityCountCardPosBranchReport(GetProductivityCountCardPosBranchReportRequest request)
    {
        var isCard = request.TabId == 1;
        var prefix = isCard ? "Kredi Kartı" : "POS";

        var root = new GetProductivityCountCardPosBranchReportResponse.GetProductivityCountCardPosBranchReportItem
        {
            Id = 1,
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
        };

        root.SubProducts.Add(new GetProductivityCountCardPosBranchReportResponse.GetProductivityCountCardPosBranchReportItem
        {
            Id = 2,
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
        });

        root.SubProducts.Add(new GetProductivityCountCardPosBranchReportResponse.GetProductivityCountCardPosBranchReportItem
        {
            Id = 3,
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
        });

        var roots = new List<GetProductivityCountCardPosBranchReportResponse.GetProductivityCountCardPosBranchReportItem> { root };
        roots = SortCountCardPosBranchTree(roots, request.SortBy, request.IsAscending);

        return new GetProductivityCountCardPosBranchReportResponse
        {
            GetProductivityCountCardPosBranchReports = roots
        };
    }

    public static IReadOnlyList<GetProductivityCountCardPosRatioBranchReportItem> GetProductivityCountCardPosRatioBranchReport(GetProductivityCountCardPosRatioBranchReportRequest request)
    {
        var items = new List<GetProductivityCountCardPosRatioBranchReportItem>
        {
            new()
            {
                Id = 1,
                RatioName = request.TabId == 1 ? "Toplam Kredi Kartı" : "Toplam Satış",
                PreviousQuarterBranchValue = 35m,
                CurrentBranchValue = 24m,
                CurrentBranchValueDiff = 11m,
                CurrentRegionAverageValue = 24m,
                CurrentRegionAverageValueDiff = 11m,
                CurrentBankAverageValue = 20m,
                CurrentBankAverageValueDiff = 15m
            },
            new()
            {
                Id = 2,
                RatioName = request.TabId == 1 ? "Aktif Kredi Kartı" : "Aktif Satış",
                PreviousQuarterBranchValue = 35m,
                CurrentBranchValue = 24m,
                CurrentBranchValueDiff = 11m,
                CurrentRegionAverageValue = 24m,
                CurrentRegionAverageValueDiff = 11m,
                CurrentBankAverageValue = 20m,
                CurrentBankAverageValueDiff = 15m
            },
            new()
            {
                Id = 3,
                RatioName = "Aktiflik Oranı %",
                PreviousQuarterBranchValue = 35m,
                CurrentBranchValue = 24m,
                CurrentBranchValueDiff = 11m,
                CurrentRegionAverageValue = 24m,
                CurrentRegionAverageValueDiff = 11m,
                CurrentBankAverageValue = 20m,
                CurrentBankAverageValueDiff = 15m
            }
        };

        if (request.TabId == 1)
        {
            items.Add(new GetProductivityCountCardPosRatioBranchReportItem
            {
                Id = 4,
                RatioName = "Anında Kart Şifre Alım %",
                PreviousQuarterBranchValue = 35m,
                CurrentBranchValue = 24m,
                CurrentBranchValueDiff = 11m,
                CurrentRegionAverageValue = 24m,
                CurrentRegionAverageValueDiff = 11m,
                CurrentBankAverageValue = 20m,
                CurrentBankAverageValueDiff = 15m
            });
        }

        return items;
    }

    public static GetProductivityCountCardPosRatioBranchReportTableHeadersItem GetProductivityCountCardPosRatioBranchReportTableHeaders(GetProductivityCountCardPosRatioBranchReportTableHeadersRequest request)
    {
        return new GetProductivityCountCardPosRatioBranchReportTableHeadersItem
        {
            RowNumberTitle = "#",
            RatioNameTitle = "Oran Adı",
            PreviousQuarterBranchTitle = "Önceki Çeyrek (Şube)",
            CurrentBranchTitle = "Şube",
            CurrentRegionAverageTitle = "Bölge",
            CurrentBankAverageTitle = "Banka"
        };
    }

    public static GetProductivityCountCustomerRegionReportResponse GetProductivityCountCustomerRegionReport(GetProductivityCountCustomerRegionReportRequest request)
    {
        var segmentName = GetCustomerSegmentName(request.SubTabId);

        var root = new GetProductivityCountCustomerRegionReportResponse.GetProductivityCountCustomerRegionReportItem
        {
            Id = 1,
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
        };

        root.SubProducts.Add(new GetProductivityCountCustomerRegionReportResponse.GetProductivityCountCustomerRegionReportItem
        {
            Id = 2,
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
        });

        root.SubProducts.Add(new GetProductivityCountCustomerRegionReportResponse.GetProductivityCountCustomerRegionReportItem
        {
            Id = 3,
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
        });

        var roots = new List<GetProductivityCountCustomerRegionReportResponse.GetProductivityCountCustomerRegionReportItem> { root };
        roots = SortCountCustomerRegionTree(roots, request.SortBy, request.IsAscending);

        return new GetProductivityCountCustomerRegionReportResponse
        {
            GetProductivityCountCustomerRegionReports = roots
        };
    }

    public static GetProductivityCountCustomerBranchReportResponse GetProductivityCountCustomerBranchReport(GetProductivityCountCustomerBranchReportRequest request)
    {
        var segmentName = GetCustomerSegmentName(request.SubTabId);

        var root = new GetProductivityCountCustomerBranchReportResponse.GetProductivityCountCustomerBranchReportItem
        {
            Id = 1,
            ProductName = $"{segmentName} - Toplam",
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
        };

        root.SubProducts.Add(new GetProductivityCountCustomerBranchReportResponse.GetProductivityCountCustomerBranchReportItem
        {
            Id = 2,
            ProductName = $"{segmentName} - Ürün A",
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
        });

        root.SubProducts.Add(new GetProductivityCountCustomerBranchReportResponse.GetProductivityCountCustomerBranchReportItem
        {
            Id = 3,
            ProductName = $"{segmentName} - Ürün B",
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
        });

        var roots = new List<GetProductivityCountCustomerBranchReportResponse.GetProductivityCountCustomerBranchReportItem> { root };
        roots = SortCountCustomerBranchTree(roots, request.SortBy, request.IsAscending);

        return new GetProductivityCountCustomerBranchReportResponse
        {
            GetProductivityCountCustomerBranchReports = roots
        };
    }

    public static GetProductivityVolumeRegionReportResponse GetProductivityVolumeRegionReport(GetProductivityVolumeRegionReportRequest request)
    {
        var segmentName = GetCustomerSegmentName(request.SubTabId);

        var root = new GetProductivityVolumeRegionReportResponse.GetProductivityVolumeRegionReportItem
        {
            Id = 1,
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
        };

        root.SubProducts.Add(new GetProductivityVolumeRegionReportResponse.GetProductivityVolumeRegionReportItem
        {
            Id = 2,
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
        });

        root.SubProducts.Add(new GetProductivityVolumeRegionReportResponse.GetProductivityVolumeRegionReportItem
        {
            Id = 3,
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
        });

        var roots = new List<GetProductivityVolumeRegionReportResponse.GetProductivityVolumeRegionReportItem> { root };
        roots = SortVolumeRegionTree(roots, request.SortBy, request.IsAscending);

        return new GetProductivityVolumeRegionReportResponse
        {
            GetProductivityVolumeRegionReports = roots
        };
    }

    public static GetProductivityVolumeBranchReportResponse GetProductivityVolumeBranchReport(GetProductivityVolumeBranchReportRequest request)
    {
        var segmentName = GetCustomerSegmentName(request.SubTabId);

        var root = new GetProductivityVolumeBranchReportResponse.GetProductivityVolumeBranchReportItem
        {
            Id = 1,
            ProductName = $"{segmentName} - Toplam",
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
        };

        root.SubProducts.Add(new GetProductivityVolumeBranchReportResponse.GetProductivityVolumeBranchReportItem
        {
            Id = 2,
            ProductName = $"{segmentName} - Ürün A",
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
        });

        root.SubProducts.Add(new GetProductivityVolumeBranchReportResponse.GetProductivityVolumeBranchReportItem
        {
            Id = 3,
            ProductName = $"{segmentName} - Ürün B",
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
        });

        var roots = new List<GetProductivityVolumeBranchReportResponse.GetProductivityVolumeBranchReportItem> { root };
        roots = SortVolumeBranchTree(roots, request.SortBy, request.IsAscending);

        return new GetProductivityVolumeBranchReportResponse
        {
            GetProductivityVolumeBranchReports = roots
        };
    }

    public static GetProductivityProfitRatioRegionReportResponse GetProductivityProfitRatioRegionReport(GetProductivityProfitRatioRegionReportRequest request)
    {
        var root = new GetProductivityProfitRatioRegionReportResponse.GetProductivityProfitRatioRegionReportItem
        {
            Id = 1,
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
        };

        root.SubProducts.Add(new GetProductivityProfitRatioRegionReportResponse.GetProductivityProfitRatioRegionReportItem
        {
            Id = 2,
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
        });

        root.SubProducts.Add(new GetProductivityProfitRatioRegionReportResponse.GetProductivityProfitRatioRegionReportItem
        {
            Id = 3,
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
        });

        var roots = new List<GetProductivityProfitRatioRegionReportResponse.GetProductivityProfitRatioRegionReportItem> { root };
        roots = SortProfitRatioRegionTree(roots, request.SortBy, request.IsAscending);

        return new GetProductivityProfitRatioRegionReportResponse
        {
            GetProductivityProfitRatioRegionReports = roots
        };
    }

    public static GetProductivityProfitRatioBranchReportResponse GetProductivityProfitRatioBranchReport(GetProductivityProfitRatioBranchReportRequest request)
    {
        var root = new GetProductivityProfitRatioBranchReportResponse.GetProductivityProfitRatioBranchReportItem
        {
            Id = 1,
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
        };

        root.SubProducts.Add(new GetProductivityProfitRatioBranchReportResponse.GetProductivityProfitRatioBranchReportItem
        {
            Id = 2,
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
        });

        root.SubProducts.Add(new GetProductivityProfitRatioBranchReportResponse.GetProductivityProfitRatioBranchReportItem
        {
            Id = 3,
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
        });

        var roots = new List<GetProductivityProfitRatioBranchReportResponse.GetProductivityProfitRatioBranchReportItem> { root };
        roots = SortProfitRatioBranchTree(roots, request.SortBy, request.IsAscending);

        return new GetProductivityProfitRatioBranchReportResponse
        {
            GetProductivityProfitRatioBranchReports = roots
        };
    }

    public static GetProductivityProfitTotalRegionReportResponse GetProductivityProfitTotalRegionReport(GetProductivityProfitTotalRegionReportRequest request)
    {
        var root = new GetProductivityProfitTotalRegionReportResponse.GetProductivityProfitTotalRegionReportItem
        {
            Id = 1,
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
        };

        root.SubProducts.Add(new GetProductivityProfitTotalRegionReportResponse.GetProductivityProfitTotalRegionReportItem
        {
            Id = 2,
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
        });

        root.SubProducts.Add(new GetProductivityProfitTotalRegionReportResponse.GetProductivityProfitTotalRegionReportItem
        {
            Id = 3,
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
        });

        var roots = new List<GetProductivityProfitTotalRegionReportResponse.GetProductivityProfitTotalRegionReportItem> { root };
        roots = SortProfitTotalRegionTree(roots, request.SortBy, request.IsAscending);

        return new GetProductivityProfitTotalRegionReportResponse
        {
            GetProductivityProfitTotalRegionReports = roots
        };
    }

    public static GetProductivityProfitTotalBranchReportResponse GetProductivityProfitTotalBranchReport(GetProductivityProfitTotalBranchReportRequest request)
    {
        var root = new GetProductivityProfitTotalBranchReportResponse.GetProductivityProfitTotalBranchReportItem
        {
            Id = 1,
            Description = "Toplam Karlılık (Şube)",
            TargetValue = 4200m,
            RealizationBranchValue = 3980m,
            RealizationRegionAverageValue = 3850m,
            RealizationRegionAverageValueDiff = 250m,
            RealizationBankAverageValue = 3600m,
            RealizationBankAverageValueDiff = 0m,
            HgBranchValue = 0.948m,
            HgBranchValueDiff = 0m,
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
        };

        root.SubProducts.Add(new GetProductivityProfitTotalBranchReportResponse.GetProductivityProfitTotalBranchReportItem
        {
            Id = 2,
            Description = "Net Faiz Geliri",
            TargetValue = 2200m,
            RealizationBranchValue = 2080m,
            RealizationRegionAverageValue = 2000m,
            RealizationRegionAverageValueDiff = 100m,
            RealizationBankAverageValue = 1900m,
            RealizationBankAverageValueDiff = 0m,
            HgBranchValue = 0.945m,
            HgBranchValueDiff = 0m,
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
        });

        root.SubProducts.Add(new GetProductivityProfitTotalBranchReportResponse.GetProductivityProfitTotalBranchReportItem
        {
            Id = 3,
            Description = "Komisyon Geliri",
            TargetValue = 800m,
            RealizationBranchValue = 760m,
            RealizationRegionAverageValue = 720m,
            RealizationRegionAverageValueDiff = 40m,
            RealizationBankAverageValue = 680m,
            RealizationBankAverageValueDiff = 0m,
            HgBranchValue = 0.95m,
            HgBranchValueDiff = 0m,
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
        });

        var roots = new List<GetProductivityProfitTotalBranchReportResponse.GetProductivityProfitTotalBranchReportItem> { root };
        roots = SortProfitTotalBranchTree(roots, request.SortBy, request.IsAscending);

        return new GetProductivityProfitTotalBranchReportResponse
        {
            GetProductivityProfitTotalBranchReports = roots
        };
    }

    public static GetProductivityProfitSpreadManagementRegionReportResponse GetProductivityProfitSpreadManagementRegionReport(GetProductivityProfitSpreadManagementRegionReportRequest request)
    {
        var root = new GetProductivityProfitSpreadManagementRegionReportResponse.GetProductivityProfitSpreadManagementRegionReportItem
        {
            Id = 1,
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
        };

        root.SubProducts.Add(new GetProductivityProfitSpreadManagementRegionReportResponse.GetProductivityProfitSpreadManagementRegionReportItem
        {
            Id = 2,
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
        });

        root.SubProducts.Add(new GetProductivityProfitSpreadManagementRegionReportResponse.GetProductivityProfitSpreadManagementRegionReportItem
        {
            Id = 3,
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
        });

        var roots = new List<GetProductivityProfitSpreadManagementRegionReportResponse.GetProductivityProfitSpreadManagementRegionReportItem> { root };
        roots = SortProfitSpreadManagementRegionTree(roots, request.SortBy, request.IsAscending);

        return new GetProductivityProfitSpreadManagementRegionReportResponse
        {
            GetProductivityProfitSpreadManagementRegionReports = roots
        };
    }

    public static GetProductivityProfitSpreadManagementBranchReportResponse GetProductivityProfitSpreadManagementBranchReport(GetProductivityProfitSpreadManagementBranchReportRequest request)
    {
        var root = new GetProductivityProfitSpreadManagementBranchReportResponse.GetProductivityProfitSpreadManagementBranchReportItem
        {
            Id = 1,
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
        };

        root.SubProducts.Add(new GetProductivityProfitSpreadManagementBranchReportResponse.GetProductivityProfitSpreadManagementBranchReportItem
        {
            Id = 2,
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
        });

        root.SubProducts.Add(new GetProductivityProfitSpreadManagementBranchReportResponse.GetProductivityProfitSpreadManagementBranchReportItem
        {
            Id = 3,
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
        });

        var roots = new List<GetProductivityProfitSpreadManagementBranchReportResponse.GetProductivityProfitSpreadManagementBranchReportItem> { root };
        roots = SortProfitSpreadManagementBranchTree(roots, request.SortBy, request.IsAscending);

        return new GetProductivityProfitSpreadManagementBranchReportResponse
        {
            GetProductivityProfitSpreadManagementBranchReports = roots
        };
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
            BankNpsScore = 68.0m,
            PhoneGreetingScore = 91m
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

    public static IReadOnlyList<GetProductivityReportTableHeaderItem> GetProductivityReportTableHeaders(GetProductivityReportTableHeadersRequest request)
    {
        return new List<GetProductivityReportTableHeaderItem>
        {
            new() { Id = 1, HeaderName = "Genel Bilgiler", ParentId = 0, OrderNo = 1, Sortable = false },
            new() { Id = 2, HeaderName = "Müşteri Adı", ParentId = 1, OrderNo = 1, Sortable = true },
            new() { Id = 3, HeaderName = "Şube Adı", ParentId = 1, OrderNo = 2, Sortable = true },

            new() { Id = 4, HeaderName = "Performans", ParentId = 0, OrderNo = 2, Sortable = false },
            new() { Id = 5, HeaderName = "Adet", ParentId = 4, OrderNo = 1, Sortable = true },
            new() { Id = 6, HeaderName = "Hacim", ParentId = 4, OrderNo = 2, Sortable = true },
            new() { Id = 7, HeaderName = "Karlılık", ParentId = 4, OrderNo = 3, Sortable = true }
        };
    }

    public static IReadOnlyList<GetReportSidebarItem> GetReportSidebarItems(GetReportSidebarItemsRequest request)
    {
        return new List<GetReportSidebarItem>
        {
            new()
            {
                Code = "ProductivityReport",
                Name = "Verim Raporları",
                Url = "/verim-raporlari",
                IsVisible = true,
                OrderNo = 2
            },
            new()
            {
                Code = "ScoreCard",
                Name = "Skor Kart",
                Url = "/skor-kart",
                IsVisible = true,
                OrderNo = 3
            },
            new()
            {
                Code = "TargetReport",
                Name = "Hedef Raporları",
                Url = "/",
                IsVisible = true,
                OrderNo = 1
            },
            new()
            {
                Code = "FinancialMap",
                Name = "Finalsal Harita",
                Url = "https://dashboard-iam.v3.dev.intertech.com.tr/",
                IsVisible = true,
                OrderNo = 4
            },
        };
    }

    public static IReadOnlyList<GetReportDatesItem> GetReportDates()
    {
        return new List<GetReportDatesItem>
        {
            new()
            {
                ReportDate = DateTime.Today,
                IsDefault = true
            },
            new()
            {
                ReportDate = DateTime.Today.AddDays(-1),
                IsDefault = false
            }
        };
    }

    #region Helpers

    private static string GetCustomerSegmentName(int subTabId) => subTabId switch
    {
        20 or 0 => "Tümü",
        21 or 1 => "Kurumsal",
        22 or 2 => "Ticari",
        23 or 3 => "KOBİ",
        24 or 4 => "Tarım",
        25 or 5 => "Bireysel",
        _ => "Tümü"
    };

    private static List<GetProductivityGeneralRegionReportResponse.GetProductivityGeneralRegionReportItem> SortGeneralRegionTree(
        List<GetProductivityGeneralRegionReportResponse.GetProductivityGeneralRegionReportItem> nodes, int? sortBy, bool asc)
    {
        Func<GetProductivityGeneralRegionReportResponse.GetProductivityGeneralRegionReportItem, object> key = sortBy switch
        {
            1 => p => p.BranchName ?? string.Empty,
            2 => p => p.FirstMonthRealizationRate,
            3 => p => p.SecondMonthRealizationRate,
            4 => p => p.ThirdMonthRealizationRate,
            5 => p => p.CorporateRate,
            6 => p => p.CommercialRate,
            7 => p => p.KbiRate,
            8 => p => p.ObiRate,
            9 => p => p.AgricultureRate,
            10 => p => p.MassRate,
            11 => p => p.AffluentRate,
            12 => p => p.PrivateBankingRate,
            _ => p => p.Id
        };

        var ordered = asc ? nodes.OrderBy(key).ToList() : nodes.OrderByDescending(key).ToList();

        foreach (var n in ordered)
            if (n.SubProducts != null && n.SubProducts.Count > 0)
                n.SubProducts = SortGeneralRegionTree(n.SubProducts, sortBy, asc);

        return ordered;
    }

    private static List<GetProductivityCountCardPosRegionReportResponse.GetProductivityCountCardPosRegionReportItem> SortCountCardPosRegionTree(
        List<GetProductivityCountCardPosRegionReportResponse.GetProductivityCountCardPosRegionReportItem> nodes, int? sortBy, bool asc)
    {
        Func<GetProductivityCountCardPosRegionReportResponse.GetProductivityCountCardPosRegionReportItem, object> key = sortBy switch
        {
            1 => p => p.ProductName ?? string.Empty,
            2 => p => p.CurrentMonthRegionValue,
            3 => p => p.CurrentMonthBankAverage,
            4 => p => p.ThreeMonthHgRegion,
            5 => p => p.ThreeMonthHgBankAverage,
            _ => p => p.Id
        };

        var ordered = asc ? nodes.OrderBy(key).ToList() : nodes.OrderByDescending(key).ToList();

        foreach (var n in ordered)
            if (n.SubProducts != null && n.SubProducts.Count > 0)
                n.SubProducts = SortCountCardPosRegionTree(n.SubProducts, sortBy, asc);

        return ordered;
    }

    private static List<GetProductivityCountCardPosBranchReportResponse.GetProductivityCountCardPosBranchReportItem> SortCountCardPosBranchTree(
        List<GetProductivityCountCardPosBranchReportResponse.GetProductivityCountCardPosBranchReportItem> nodes, int? sortBy, bool asc)
    {
        Func<GetProductivityCountCardPosBranchReportResponse.GetProductivityCountCardPosBranchReportItem, object> key = sortBy switch
        {
            1 => p => p.ProductName ?? string.Empty,
            2 => p => p.CurrentPeriodBranchValue,
            3 => p => p.CurrentPeriodRegionAverageValue,
            4 => p => p.CurrentPeriodBankAverageValue,
            5 => p => p.ThreeMonthHgBranchValue,
            6 => p => p.ThreeMonthHgRegionAverageValue,
            7 => p => p.ThreeMonthHgBankAverageValue,
            _ => p => p.Id
        };

        var ordered = asc ? nodes.OrderBy(key).ToList() : nodes.OrderByDescending(key).ToList();

        foreach (var n in ordered)
            if (n.SubProducts != null && n.SubProducts.Count > 0)
                n.SubProducts = SortCountCardPosBranchTree(n.SubProducts, sortBy, asc);

        return ordered;
    }

    private static List<GetProductivityCountCustomerRegionReportResponse.GetProductivityCountCustomerRegionReportItem> SortCountCustomerRegionTree(
        List<GetProductivityCountCustomerRegionReportResponse.GetProductivityCountCustomerRegionReportItem> nodes, int? sortBy, bool asc)
    {
        Func<GetProductivityCountCustomerRegionReportResponse.GetProductivityCountCustomerRegionReportItem, object> key = sortBy switch
        {
            1 => p => p.ProductName ?? string.Empty,
            2 => p => p.RealizationRegion,
            3 => p => p.RealizationBankAverage,
            4 => p => p.YtdChangeRegion,
            5 => p => p.YtdChangeBankAverage,
            6 => p => p.QtdChangeRegion,
            7 => p => p.QtdChangeBankAverage,
            _ => p => p.Id
        };

        var ordered = asc ? nodes.OrderBy(key).ToList() : nodes.OrderByDescending(key).ToList();

        foreach (var n in ordered)
            if (n.SubProducts != null && n.SubProducts.Count > 0)
                n.SubProducts = SortCountCustomerRegionTree(n.SubProducts, sortBy, asc);

        return ordered;
    }

    private static List<GetProductivityCountCustomerBranchReportResponse.GetProductivityCountCustomerBranchReportItem> SortCountCustomerBranchTree(
        List<GetProductivityCountCustomerBranchReportResponse.GetProductivityCountCustomerBranchReportItem> nodes, int? sortBy, bool asc)
    {
        Func<GetProductivityCountCustomerBranchReportResponse.GetProductivityCountCustomerBranchReportItem, object> key = sortBy switch
        {
            1 => p => p.ProductName ?? string.Empty,
            2 => p => p.RealizationBranchValue,
            3 => p => p.RealizationRegionAverageValue,
            4 => p => p.RealizationBankAverageValue,
            5 => p => p.YtdNominalChangeBranchValue,
            6 => p => p.YtdNominalChangeRegionAverageValue,
            7 => p => p.YtdNominalChangeBankAverageValue,
            8 => p => p.QtdNominalChangeBranchValue,
            9 => p => p.QtdNominalChangeRegionAverageValue,
            10 => p => p.QtdNominalChangeBankAverageValue,
            _ => p => p.Id
        };

        var ordered = asc ? nodes.OrderBy(key).ToList() : nodes.OrderByDescending(key).ToList();

        foreach (var n in ordered)
            if (n.SubProducts != null && n.SubProducts.Count > 0)
                n.SubProducts = SortCountCustomerBranchTree(n.SubProducts, sortBy, asc);

        return ordered;
    }

    private static List<GetProductivityVolumeRegionReportResponse.GetProductivityVolumeRegionReportItem> SortVolumeRegionTree(
        List<GetProductivityVolumeRegionReportResponse.GetProductivityVolumeRegionReportItem> nodes, int? sortBy, bool asc)
    {
        Func<GetProductivityVolumeRegionReportResponse.GetProductivityVolumeRegionReportItem, object> key = sortBy switch
        {
            1 => p => p.ProductName ?? string.Empty,
            2 => p => p.RealizationRegionValue,
            3 => p => p.RealizationBankAverageValue,
            4 => p => p.TargetValue,
            5 => p => p.HgRate,
            6 => p => p.NetGrowthRegionValue,
            7 => p => p.NetGrowthBankAverageValue,
            8 => p => p.YtdRegionValue,
            9 => p => p.YtdBankAverageValue,
            10 => p => p.QtdRegionValue,
            11 => p => p.QtdBankAverageValue,
            _ => p => p.Id
        };

        var ordered = asc ? nodes.OrderBy(key).ToList() : nodes.OrderByDescending(key).ToList();

        foreach (var n in ordered)
            if (n.SubProducts != null && n.SubProducts.Count > 0)
                n.SubProducts = SortVolumeRegionTree(n.SubProducts, sortBy, asc);

        return ordered;
    }

    private static List<GetProductivityVolumeBranchReportResponse.GetProductivityVolumeBranchReportItem> SortVolumeBranchTree(
        List<GetProductivityVolumeBranchReportResponse.GetProductivityVolumeBranchReportItem> nodes, int? sortBy, bool asc)
    {
        Func<GetProductivityVolumeBranchReportResponse.GetProductivityVolumeBranchReportItem, object> key = sortBy switch
        {
            1 => p => p.ProductName ?? string.Empty,
            2 => p => p.RealizationBranchValue,
            3 => p => p.RealizationRegionAverageValue,
            4 => p => p.RealizationBankAverageValue,
            5 => p => p.TargetValue,
            6 => p => p.HgRate,
            7 => p => p.NetGrowthBranchValue,
            8 => p => p.NetGrowthRegionAverageValue,
            9 => p => p.NetGrowthBankAverageValue,
            10 => p => p.YtdBranchValue,
            11 => p => p.YtdRegionValue,
            12 => p => p.YtdBankValue,
            13 => p => p.QtdBranchValue,
            14 => p => p.QtdRegionValue,
            15 => p => p.QtdBankValue,
            _ => p => p.Id
        };

        var ordered = asc ? nodes.OrderBy(key).ToList() : nodes.OrderByDescending(key).ToList();

        foreach (var n in ordered)
            if (n.SubProducts != null && n.SubProducts.Count > 0)
                n.SubProducts = SortVolumeBranchTree(n.SubProducts, sortBy, asc);

        return ordered;
    }

    private static List<GetProductivityProfitRatioRegionReportResponse.GetProductivityProfitRatioRegionReportItem> SortProfitRatioRegionTree(
        List<GetProductivityProfitRatioRegionReportResponse.GetProductivityProfitRatioRegionReportItem> nodes, int? sortBy, bool asc)
    {
        Func<GetProductivityProfitRatioRegionReportResponse.GetProductivityProfitRatioRegionReportItem, object> key = sortBy switch
        {
            1 => p => p.RatioName ?? string.Empty,
            2 => p => p.TargetValue,
            3 => p => p.RegionValue,
            4 => p => p.BankValue,
            5 => p => p.RetailValue,
            6 => p => p.KobiValue,
            7 => p => p.AgricultureValue,
            8 => p => p.CommercialValue,
            _ => p => p.Id
        };

        var ordered = asc ? nodes.OrderBy(key).ToList() : nodes.OrderByDescending(key).ToList();

        foreach (var n in ordered)
            if (n.SubProducts != null && n.SubProducts.Count > 0)
                n.SubProducts = SortProfitRatioRegionTree(n.SubProducts, sortBy, asc);

        return ordered;
    }

    private static List<GetProductivityProfitRatioBranchReportResponse.GetProductivityProfitRatioBranchReportItem> SortProfitRatioBranchTree(
        List<GetProductivityProfitRatioBranchReportResponse.GetProductivityProfitRatioBranchReportItem> nodes, int? sortBy, bool asc)
    {
        Func<GetProductivityProfitRatioBranchReportResponse.GetProductivityProfitRatioBranchReportItem, object> key = sortBy switch
        {
            1 => p => p.RatioName ?? string.Empty,
            2 => p => p.TargetValue,
            3 => p => p.RegionValue,
            4 => p => p.BankValue,
            5 => p => p.RetailValue,
            6 => p => p.KobiValue,
            7 => p => p.AgricultureValue,
            8 => p => p.CommercialValue,
            _ => p => p.Id
        };

        var ordered = asc ? nodes.OrderBy(key).ToList() : nodes.OrderByDescending(key).ToList();

        foreach (var n in ordered)
            if (n.SubProducts != null && n.SubProducts.Count > 0)
                n.SubProducts = SortProfitRatioBranchTree(n.SubProducts, sortBy, asc);

        return ordered;
    }

    private static List<GetProductivityProfitTotalRegionReportResponse.GetProductivityProfitTotalRegionReportItem> SortProfitTotalRegionTree(
        List<GetProductivityProfitTotalRegionReportResponse.GetProductivityProfitTotalRegionReportItem> nodes, int? sortBy, bool asc)
    {
        Func<GetProductivityProfitTotalRegionReportResponse.GetProductivityProfitTotalRegionReportItem, object> key = sortBy switch
        {
            1 => p => p.Description ?? string.Empty,
            2 => p => p.TargetValue,
            3 => p => p.RealizationRegionValue,
            4 => p => p.RealizationBankAverageValue,
            5 => p => p.HgRegionValue,
            6 => p => p.HgBankAverageValue,
            7 => p => p.RetailValue,
            8 => p => p.KobiValue,
            9 => p => p.AgricultureValue,
            10 => p => p.CommercialValue,
            11 => p => p.PartnerValue,
            _ => p => p.Id
        };

        var ordered = asc ? nodes.OrderBy(key).ToList() : nodes.OrderByDescending(key).ToList();

        foreach (var n in ordered)
            if (n.SubProducts != null && n.SubProducts.Count > 0)
                n.SubProducts = SortProfitTotalRegionTree(n.SubProducts, sortBy, asc);

        return ordered;
    }

    private static List<GetProductivityProfitTotalBranchReportResponse.GetProductivityProfitTotalBranchReportItem> SortProfitTotalBranchTree(
        List<GetProductivityProfitTotalBranchReportResponse.GetProductivityProfitTotalBranchReportItem> nodes, int? sortBy, bool asc)
    {
        Func<GetProductivityProfitTotalBranchReportResponse.GetProductivityProfitTotalBranchReportItem, object> key = sortBy switch
        {
            1 => p => p.Description ?? string.Empty,
            2 => p => p.TargetValue,
            3 => p => p.RealizationBranchValue,
            4 => p => p.RealizationRegionAverageValue,
            5 => p => p.RealizationBankAverageValue,
            6 => p => p.HgBranchValue,
            7 => p => p.HgRegionAverageValue,
            8 => p => p.HgBankAverageValue,
            9 => p => p.RetailValue,
            10 => p => p.KobiValue,
            11 => p => p.AgricultureValue,
            12 => p => p.CommercialValue,
            13 => p => p.PartnerValue,
            _ => p => p.Id
        };

        var ordered = asc ? nodes.OrderBy(key).ToList() : nodes.OrderByDescending(key).ToList();

        foreach (var n in ordered)
            if (n.SubProducts != null && n.SubProducts.Count > 0)
                n.SubProducts = SortProfitTotalBranchTree(n.SubProducts, sortBy, asc);

        return ordered;
    }

    private static List<GetProductivityProfitSpreadManagementRegionReportResponse.GetProductivityProfitSpreadManagementRegionReportItem> SortProfitSpreadManagementRegionTree(
        List<GetProductivityProfitSpreadManagementRegionReportResponse.GetProductivityProfitSpreadManagementRegionReportItem> nodes, int? sortBy, bool asc)
    {
        Func<GetProductivityProfitSpreadManagementRegionReportResponse.GetProductivityProfitSpreadManagementRegionReportItem, object> key = sortBy switch
        {
            1 => p => p.Description ?? string.Empty,
            2 => p => p.SpreadValue,
            3 => p => p.RatioRegionValue,
            4 => p => p.RatioBankAverageValue,
            5 => p => p.NetReturnRegionValue,
            6 => p => p.NetReturnBankAverageValue,
            7 => p => p.NetReturnHgRegionValue,
            8 => p => p.NetReturnHgBankAverageValue,
            _ => p => p.Id
        };

        var ordered = asc ? nodes.OrderBy(key).ToList() : nodes.OrderByDescending(key).ToList();

        foreach (var n in ordered)
            if (n.SubProducts != null && n.SubProducts.Count > 0)
                n.SubProducts = SortProfitSpreadManagementRegionTree(n.SubProducts, sortBy, asc);

        return ordered;
    }

    private static List<GetProductivityProfitSpreadManagementBranchReportResponse.GetProductivityProfitSpreadManagementBranchReportItem> SortProfitSpreadManagementBranchTree(
        List<GetProductivityProfitSpreadManagementBranchReportResponse.GetProductivityProfitSpreadManagementBranchReportItem> nodes, int? sortBy, bool asc)
    {
        Func<GetProductivityProfitSpreadManagementBranchReportResponse.GetProductivityProfitSpreadManagementBranchReportItem, object> key = sortBy switch
        {
            1 => p => p.Description ?? string.Empty,
            2 => p => p.SpreadValue,
            3 => p => p.RatioBranchValue,
            4 => p => p.RatioRegionAverageValue,
            5 => p => p.RatioBankAverageValue,
            6 => p => p.NetReturnBranchValue,
            7 => p => p.NetReturnRegionAverageValue,
            8 => p => p.NetReturnBankAverageValue,
            9 => p => p.NetReturnHgBranchValue,
            10 => p => p.NetReturnHgRegionAverageValue,
            11 => p => p.NetReturnHgBankAverageValue,
            _ => p => p.Id
        };

        var ordered = asc ? nodes.OrderBy(key).ToList() : nodes.OrderByDescending(key).ToList();

        foreach (var n in ordered)
            if (n.SubProducts != null && n.SubProducts.Count > 0)
                n.SubProducts = SortProfitSpreadManagementBranchTree(n.SubProducts, sortBy, asc);

        return ordered;
    }

    #endregion
}