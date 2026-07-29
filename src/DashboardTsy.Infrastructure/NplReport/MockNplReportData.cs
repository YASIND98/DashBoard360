using DashboardTsy.Application.NplReport.Requests;
using DashboardTsy.Application.NplReport.Responses;

namespace DashboardTsy.Infrastructure.NplReport;

/// <summary>
/// NplReport için mock veri kaynağı.
/// NplReportProvider, ReportMock:Enabled true olduğunda buraya delege eder.
/// </summary>
public static class MockNplReportData
{
    public static IReadOnlyList<GetNplBalanceRatioItem> GetNplBalanceRatio(GetNplBalanceRatioRequest request)
    {
        return new List<GetNplBalanceRatioItem>
        {
            new()
            {
                ReportDate = new DateTime(2025, 5, 31),
                BalanceAnapara = 31342321.37m,
                BalanceKof = 82433434.38m,
                BalanceToplam = 396346610.75m,
                RatioAnapara = 0.9m,
                RatioKof = 0.1m
            },
            new()
            {
                ReportDate = new DateTime(2025, 6, 30),
                BalanceAnapara = 31343065.76m,
                BalanceKof = 91633437.85m,
                BalanceToplam = 4194343569.61m,
                RatioAnapara = 0.8m,
                RatioKof = 0.2m
            },
            new()
            {
                ReportDate = new DateTime(2025, 7, 31),
                BalanceAnapara = 3342651.53m
            }
        };
    }

    public static IReadOnlyList<GetNplFiltersItem> GetNplFilters(GetNplFiltersRequest request)
    {
        return new List<GetNplFiltersItem>
        {
            new() { FilterGroupId = 1, FilterCode = "PRODUCT",   FilterName = "Ürün",         DisplayOrder = 1, IsMultiSelect = false, FilterItemId = 1,  ItemCode = "IHTIYAC",  ItemName = "İhtiyaç",     ItemOrder = 1, IsDefault = true },
            new() { FilterGroupId = 1, FilterCode = "PRODUCT",   FilterName = "Ürün",         DisplayOrder = 1, IsMultiSelect = false, FilterItemId = 2,  ItemCode = "KMH",      ItemName = "KMH",         ItemOrder = 1, IsDefault = true },
            new() { FilterGroupId = 1, FilterCode = "PRODUCT",   FilterName = "Ürün",         DisplayOrder = 1, IsMultiSelect = false, FilterItemId = 3,  ItemCode = "KK",       ItemName = "KK",          ItemOrder = 1, IsDefault = true },
            new() { FilterGroupId = 1, FilterCode = "PRODUCT",   FilterName = "Ürün",         DisplayOrder = 1, IsMultiSelect = false, FilterItemId = 4,  ItemCode = "UK",       ItemName = "ÜK",          ItemOrder = 1, IsDefault = true },
            new() { FilterGroupId = 1, FilterCode = "PRODUCT",   FilterName = "Ürün",         DisplayOrder = 1, IsMultiSelect = false, FilterItemId = 5,  ItemCode = "TRAKTOR",  ItemName = "Traktör",     ItemOrder = 1, IsDefault = true },
            new() { FilterGroupId = 1, FilterCode = "PRODUCT",   FilterName = "Ürün",         DisplayOrder = 1, IsMultiSelect = false, FilterItemId = 6,  ItemCode = "DIGER",    ItemName = "Diğer",       ItemOrder = 1, IsDefault = true },

            new() { FilterGroupId = 2, FilterCode = "AUTHORITY", FilterName = "Yetki Kodu",   DisplayOrder = 1, IsMultiSelect = false, FilterItemId = 7,  ItemCode = "BY",       ItemName = "BY",          ItemOrder = 1, IsDefault = true },
            new() { FilterGroupId = 2, FilterCode = "AUTHORITY", FilterName = "Yetki Kodu",   DisplayOrder = 1, IsMultiSelect = false, FilterItemId = 8,  ItemCode = "SY",       ItemName = "SY",          ItemOrder = 1, IsDefault = true },
            new() { FilterGroupId = 2, FilterCode = "AUTHORITY", FilterName = "Yetki Kodu",   DisplayOrder = 1, IsMultiSelect = false, FilterItemId = 9,  ItemCode = "GM",       ItemName = "GM",          ItemOrder = 1, IsDefault = true },
            new() { FilterGroupId = 2, FilterCode = "AUTHORITY", FilterName = "Yetki Kodu",   DisplayOrder = 1, IsMultiSelect = false, FilterItemId = 10, ItemCode = "DIGER",    ItemName = "Diğer",       ItemOrder = 1, IsDefault = true },

            new() { FilterGroupId = 3, FilterCode = "BUSINESS",  FilterName = "İş Kolu",      DisplayOrder = 1, IsMultiSelect = true,  FilterItemId = 11, ItemCode = "TUMU",     ItemName = "Tümü",        ItemOrder = 1, IsDefault = true },
            new() { FilterGroupId = 3, FilterCode = "BUSINESS",  FilterName = "İş Kolu",      DisplayOrder = 1, IsMultiSelect = true,  FilterItemId = 12, ItemCode = "BIREYSEL", ItemName = "Bireysel",    ItemOrder = 1, IsDefault = true },
            new() { FilterGroupId = 3, FilterCode = "BUSINESS",  FilterName = "İş Kolu",      DisplayOrder = 1, IsMultiSelect = true,  FilterItemId = 13, ItemCode = "ISLETME",  ItemName = "İşletme",     ItemOrder = 1, IsDefault = true },
            new() { FilterGroupId = 3, FilterCode = "BUSINESS",  FilterName = "İş Kolu",      DisplayOrder = 1, IsMultiSelect = true,  FilterItemId = 14, ItemCode = "KREDI",    ItemName = "Kredi Kartı", ItemOrder = 1, IsDefault = true },
            new() { FilterGroupId = 3, FilterCode = "BUSINESS",  FilterName = "İş Kolu",      DisplayOrder = 1, IsMultiSelect = true,  FilterItemId = 15, ItemCode = "TARIM",    ItemName = "Tarım",       ItemOrder = 1, IsDefault = true },

            new() { FilterGroupId = 4, FilterCode = "ALLOCATION", FilterName = "Tahsis Kolu", DisplayOrder = 1, IsMultiSelect = true,  FilterItemId = 16, ItemCode = "TUMU",     ItemName = "Tümü",        ItemOrder = 1, IsDefault = true },
            new() { FilterGroupId = 4, FilterCode = "ALLOCATION", FilterName = "Tahsis Kolu", DisplayOrder = 1, IsMultiSelect = true,  FilterItemId = 17, ItemCode = "BIREYSEL", ItemName = "Bireysel",    ItemOrder = 1, IsDefault = true }
        };
    }

    public static IReadOnlyList<GetNplProductsItem> GetNplProducts(GetNplProductsRequest request)
    {
        return new List<GetNplProductsItem>
        {
            new() { ProductId = 1, ProductCode = "IHTIYAC", ProductName = "İhtiyaç" },
            new() { ProductId = 2, ProductCode = "KMH",     ProductName = "KMH" },
            new() { ProductId = 3, ProductCode = "KK",      ProductName = "KK" },
            new() { ProductId = 4, ProductCode = "UK",      ProductName = "ÜK" },
            new() { ProductId = 5, ProductCode = "TRAKTOR", ProductName = "Traktör" },
            new() { ProductId = 6, ProductCode = "DIGER",   ProductName = "Diğer" }
        };
    }
}
