using System.Globalization;
using DashboardTsy.Application.SalaryCustomerReport.Requests;
using DashboardTsy.Application.SalaryCustomerReport.Responses;

namespace DashboardTsy.Infrastructure.SalaryCustomerReport;

/// <summary>
/// SalaryCustomerReport için mock veri kaynağı.
/// SalaryCustomerReportProvider, ReportMock:Enabled true olduğunda buraya delege eder.
/// </summary>
public static class MockSalaryCustomerReportData
{
    public static IReadOnlyList<GetSalaryCustomerReportTabItem> GetSalaryCustomerReportTabs(GetSalaryCustomerReportTabsRequest request)
    {
        return new List<GetSalaryCustomerReportTabItem>
        {
            new() { TabId = 1, TabName = "Hacim", ParentId = 0, TabLevel = 1 },
            new() { TabId = 2, TabName = "Çapraz Satış Gelişimi", ParentId = 0, TabLevel = 1 },
            new() { TabId = 3, TabName = "Banka Payı", ParentId = 0, TabLevel = 1 },

            new() { TabId = 10, TabName = "Tümü", ParentId = 1, TabLevel = 2 },
            new() { TabId = 11, TabName = "Kurumsal", ParentId = 1, TabLevel = 2 },
            new() { TabId = 12, TabName = "Ticari", ParentId = 1, TabLevel = 2 },
            new() { TabId = 13, TabName = "KOBİ", ParentId = 1, TabLevel = 2 },
            new() { TabId = 14, TabName = "Tarım", ParentId = 1, TabLevel = 2 },
            new() { TabId = 15, TabName = "Bireysel", ParentId = 1, TabLevel = 2 }
        };
    }

    public static GetSalaryCustomerVolumeReportHeadersResponse GetSalaryCustomerVolumeReportHeaders(DateTime reportDate)
    {
        var t = (reportDate == default ? DateTime.Today : reportDate).Date;

        return new GetSalaryCustomerVolumeReportHeadersResponse
        {
            ProductColumnName = "Ürün",

            LastYearColumnName = "Geçen Yıl",
            LastYearColumnDate = t.AddYears(-1),
            LastYearDifferenceColumnName = "Geçen Yıla Göre",

            LastWeekColumnName = "Geçen Hafta",
            LastWeekColumnDate = t.AddDays(-7),
            LastWeekDifferenceColumnName = "Geçen Haftaya Göre",

            PreviousDayColumnName = "Önceki Gün",
            PreviousDayColumnDate = t.AddDays(-2),
            PreviousDayDifferenceColumnName = "Önceki Güne Göre",

            YesterdayColumnName = "Dün",
            YesterdayColumnDate = t.AddDays(-1),

            SalaryLabel = "Maaş",
            RetiredLabel = "Emekli"
        };
    }

    public static IReadOnlyList<GetSalaryCustomerVolumeReportItem> GetSalaryCustomerVolumeReport(GetSalaryCustomerVolumeReportRequest request)
    {
        var products = new[]
        {
            "Çalışma Büyüklüğü", "Aktif Büyüklük", "Vadesiz TL", "Vadeli TL", "Vadeli YP", "Vadeli YP", "Aktif Büyüklük"
        };

        var items = new List<GetSalaryCustomerVolumeReportItem>();
        for (var i = 0; i < products.Length; i++)
        {
            var item = new GetSalaryCustomerVolumeReportItem
            {
                Id = i + 1,
                SortOrder = i + 1,
                ProductName = products[i],

                LastYearTotalAmount = 32609591.452m,
                LastYearSalaryAmount = 30609591.452m,
                LastYearSalaryRate = 67m,
                LastYearRetiredAmount = 2609591.452m,
                LastYearRetiredRate = 12m,

                LastWeekTotalAmount = 32609591.452m,
                LastWeekSalaryAmount = 30609591.452m,
                LastWeekSalaryRate = 67m,
                LastWeekRetiredAmount = 2609591.452m,
                LastWeekRetiredRate = 12m,

                PreviousDayTotalAmount = 32609591.452m,
                PreviousDaySalaryAmount = 30609591.452m,
                PreviousDaySalaryRate = 67m,
                PreviousDayRetiredAmount = 2609591.452m,
                PreviousDayRetiredRate = 12m,

                YesterdayTotalAmount = 32609591.452m,
                YesterdaySalaryAmount = 30609591.452m,
                YesterdaySalaryRate = 67m,
                YesterdayRetiredAmount = 2609591.452m,
                YesterdayRetiredRate = 12m
            };

            if (request.ShowDifferences)
            {
                item.LastYearTotalDifference = 2.456m; item.LastYearTotalDifferenceStatus = 1;
                item.LastYearSalaryDifference = 0.456m; item.LastYearSalaryDifferenceStatus = 1;
                item.LastYearRetiredDifference = 1.456m; item.LastYearRetiredDifferenceStatus = 1;

                item.LastWeekTotalDifference = -2.456m; item.LastWeekTotalDifferenceStatus = 2;
                item.LastWeekSalaryDifference = -0.456m; item.LastWeekSalaryDifferenceStatus = 2;
                item.LastWeekRetiredDifference = -1.456m; item.LastWeekRetiredDifferenceStatus = 2;

                item.PreviousDayTotalDifference = 2.456m; item.PreviousDayTotalDifferenceStatus = 1;
                item.PreviousDaySalaryDifference = 0.456m; item.PreviousDaySalaryDifferenceStatus = 1;
                item.PreviousDayRetiredDifference = 1.456m; item.PreviousDayRetiredDifferenceStatus = 1;
            }

            items.Add(item);
        }

        return items;
    }

    public static GetSalaryCustomerCrossSellReportHeadersResponse GetSalaryCustomerCrossSellReportHeaders(DateTime reportDate)
    {
        var t = (reportDate == default ? DateTime.Today : reportDate).Date;

        return new GetSalaryCustomerCrossSellReportHeadersResponse
        {
            ProductColumnName = "Ürün",

            LastYearColumnName = "Geçen Yıl",
            LastYearColumnDate = t.AddYears(-1),
            LastYearDifferenceColumnName = "Geçen Yıla Göre",

            TwoMonthsAgoColumnName = "İki Ay Önce",
            TwoMonthsAgoColumnDate = t.AddMonths(-2),
            TwoMonthsAgoDifferenceColumnName = "İki Ay Önceye Göre",

            LastMonthColumnName = "Geçen Ay",
            LastMonthColumnDate = t.AddMonths(-1),

            SalaryLabel = "Maaş",
            RetiredLabel = "Emekli"
        };
    }

    public static IReadOnlyList<GetSalaryCustomerCrossSellReportItem> GetSalaryCustomerCrossSellReport(GetSalaryCustomerCrossSellReportRequest request)
    {
        var products = new[]
        {
            "Toplam Müşteri", "Aktif Büyüklük", "Vadesiz TL", "Vadeli TL", "Vadesiz YP", "Vadeli YP"
        };

        var items = new List<GetSalaryCustomerCrossSellReportItem>();
        for (var i = 0; i < products.Length; i++)
        {
            var item = new GetSalaryCustomerCrossSellReportItem
            {
                Id = i + 1,
                SortOrder = i + 1,
                ProductName = products[i],

                LastYearTotalAmount = 32609591.452m,
                LastYearSalaryAmount = 30609591.452m,
                LastYearSalaryRate = 67m,
                LastYearRetiredAmount = 2609591.452m,
                LastYearRetiredRate = 12m,

                TwoMonthsAgoTotalAmount = 32609591.452m,
                TwoMonthsAgoSalaryAmount = 30609591.452m,
                TwoMonthsAgoSalaryRate = 67m,
                TwoMonthsAgoRetiredAmount = 2609591.452m,
                TwoMonthsAgoRetiredRate = 12m,

                LastMonthTotalAmount = 32609591.452m,
                LastMonthSalaryAmount = 30609591.452m,
                LastMonthSalaryRate = 67m,
                LastMonthRetiredAmount = 2609591.452m,
                LastMonthRetiredRate = 12m
            };

            if (request.ShowDifferences)
            {
                item.LastYearTotalDifference = 2.456m; item.LastYearTotalDifferenceStatus = 1;
                item.LastYearSalaryDifference = 0.456m; item.LastYearSalaryDifferenceStatus = 1;
                item.LastYearRetiredDifference = 1.456m; item.LastYearRetiredDifferenceStatus = 1;

                item.TwoMonthsAgoTotalDifference = -2.456m; item.TwoMonthsAgoTotalDifferenceStatus = 2;
                item.TwoMonthsAgoSalaryDifference = -0.456m; item.TwoMonthsAgoSalaryDifferenceStatus = 2;
                item.TwoMonthsAgoRetiredDifference = -1.456m; item.TwoMonthsAgoRetiredDifferenceStatus = 2;
            }

            items.Add(item);
        }

        return items;
    }

    public static GetSalaryCustomerBankShareReportHeadersResponse GetSalaryCustomerBankShareReportHeaders(DateTime reportDate)
    {
        var culture = CultureInfo.GetCultureInfo("tr-TR");
        var reportMonth = new DateTime((reportDate == default ? DateTime.Today : reportDate).Year, (reportDate == default ? DateTime.Today : reportDate).Month, 1);
        var previousMonth = reportMonth.AddMonths(-1);

        string MonthLabel(DateTime d) => $"{d.ToString("MMMM", culture)}'{d:yy}";

        return new GetSalaryCustomerBankShareReportHeadersResponse
        {
            ProductColumnName = "Ürün",

            DenizbankCreditGroupName = "Denizbank Kredisi Olan",
            OtherBanksCreditGroupName = "Kredisi Diğer Bankalarda Olan",
            WalletShareGroupName = "Cüzdan Payı",

            FirstMonthName = MonthLabel(previousMonth),
            SecondMonthName = MonthLabel(reportMonth),

            SalaryCustomersLabel = "Maaş Müşterileri",
            RetiredCustomersLabel = "Emekli Müşterileri"
        };
    }

    public static IReadOnlyList<GetSalaryCustomerBankShareReportItem> GetSalaryCustomerBankShareReport(GetSalaryCustomerBankShareReportRequest request)
    {
        var rows = new (string Name, int ValueType)[]
        {
            ("Tüketici Kredisi Müşteri (Adet)", 1),
            ("Tüketici Kredisi Müşteri (Hacim)", 2),
            ("Kredi Kartı (Adet)", 1),
            ("Kredi Kartı (Hacim)", 2),
            ("KMH (Adet)", 1),
            ("KMH (Hacim)", 2)
        };

        var items = new List<GetSalaryCustomerBankShareReportItem>();
        for (var i = 0; i < rows.Length; i++)
        {
            var (name, valueType) = rows[i];
            var isVolume = valueType == 2;

            items.Add(new GetSalaryCustomerBankShareReportItem
            {
                Id = i + 1,
                SortOrder = i + 1,
                ProductName = name,
                ValueType = valueType,

                DenizbankFirstMonthValue = 123.456m,
                DenizbankSecondMonthValue = 123.456m,

                OtherBanksFirstMonthValue = 123.456m,
                OtherBanksSecondMonthValue = 123.456m,

                WalletShareFirstMonthRate = 4.98m,
                WalletShareSecondMonthRate = isVolume ? 7.456m : 0.29m,
                WalletShareSecondMonthRateStatus = isVolume ? 1 : 2
            });
        }

        return items;
    }
}
