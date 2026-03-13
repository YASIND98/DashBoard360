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

