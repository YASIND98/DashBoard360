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
                BalanceToplam = 113775755.75m,
                RatioAnapara = 0.9m,
                RatioKof = 0.1m
            },
            new()
            {
                ReportDate = new DateTime(2025, 6, 30),
                BalanceAnapara = 31343065.76m,
                BalanceKof = 91633437.85m,
                BalanceToplam = 122976503.61m,
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
            new() { FilterGroupId = 1,   FilterCode = "URUN",                    FilterName = "Ürün",                 DisplayOrder = 1, IsMultiSelect = false,  FilterItemId = 1,   ItemCode = "İhtiyaç",       ItemName = "İhtiyaç",        ItemOrder = 1, IsDefault = true },
            new() { FilterGroupId = 1,   FilterCode = "URUN",                    FilterName = "Ürün",                 DisplayOrder = 1, IsMultiSelect = false,  FilterItemId = 2,   ItemCode = "KMH",           ItemName = "KMH",            ItemOrder = 1, IsDefault = true },
            new() { FilterGroupId = 1,   FilterCode = "URUN",                    FilterName = "Ürün",                 DisplayOrder = 1, IsMultiSelect = false,  FilterItemId = 3,   ItemCode = "KK",            ItemName = "Kredi Kartı",    ItemOrder = 1, IsDefault = true },
            new() { FilterGroupId = 1,   FilterCode = "URUN",                    FilterName = "Ürün",                 DisplayOrder = 1, IsMultiSelect = false,  FilterItemId = 4,   ItemCode = "Üretici Kart",  ItemName = "Üretici Kart",   ItemOrder = 1, IsDefault = true },
            new() { FilterGroupId = 1,   FilterCode = "URUN",                    FilterName = "Ürün",                 DisplayOrder = 1, IsMultiSelect = false,  FilterItemId = 5,   ItemCode = "Traktör",       ItemName = "Traktör",        ItemOrder = 1, IsDefault = true },
            new() { FilterGroupId = 1,   FilterCode = "URUN",                    FilterName = "Ürün",                 DisplayOrder = 1, IsMultiSelect = false,  FilterItemId = 6,   ItemCode = "Diğer",         ItemName = "Diğer",          ItemOrder = 1, IsDefault = true },

            new() { FilterGroupId = 2,   FilterCode = "YETKI_KODU",              FilterName = "Yetki Kodu",           DisplayOrder = 1, IsMultiSelect = false,  FilterItemId = 7,   ItemCode = "BY",            ItemName = "BY",             ItemOrder = 1, IsDefault = true },
            new() { FilterGroupId = 2,   FilterCode = "YETKI_KODU",              FilterName = "Yetki Kodu",           DisplayOrder = 1, IsMultiSelect = false,  FilterItemId = 8,   ItemCode = "SY",            ItemName = "SY",             ItemOrder = 1, IsDefault = true },
            new() { FilterGroupId = 2,   FilterCode = "YETKI_KODU",              FilterName = "Yetki Kodu",           DisplayOrder = 1, IsMultiSelect = false,  FilterItemId = 9,   ItemCode = "GM",            ItemName = "GM",             ItemOrder = 1, IsDefault = true },
            new() { FilterGroupId = 2,   FilterCode = "YETKI_KODU",              FilterName = "Yetki Kodu",           DisplayOrder = 1, IsMultiSelect = false,  FilterItemId = 10,  ItemCode = "Diğer",         ItemName = "Diğer",          ItemOrder = 1, IsDefault = true },

            new() { FilterGroupId = 3,   FilterCode = "ISKOLU",                  FilterName = "İş Kolu",              DisplayOrder = 1, IsMultiSelect = true,   FilterItemId = 11,  ItemCode = null,            ItemName = "Tümü",           ItemOrder = 1, IsDefault = true },
            new() { FilterGroupId = 3,   FilterCode = "ISKOLU",                  FilterName = "İş Kolu",              DisplayOrder = 1, IsMultiSelect = true,   FilterItemId = 12,  ItemCode = "Bireysel",      ItemName = "Bireysel",       ItemOrder = 1, IsDefault = true },
            new() { FilterGroupId = 3,   FilterCode = "ISKOLU",                  FilterName = "İş Kolu",              DisplayOrder = 1, IsMultiSelect = true,   FilterItemId = 13,  ItemCode = "İşletme",       ItemName = "İşletme",        ItemOrder = 1, IsDefault = true },
            new() { FilterGroupId = 3,   FilterCode = "ISKOLU",                  FilterName = "İş Kolu",              DisplayOrder = 1, IsMultiSelect = true,   FilterItemId = 14,  ItemCode = "Kredi Kartı",   ItemName = "Kredi Kartı",    ItemOrder = 1, IsDefault = true },
            new() { FilterGroupId = 3,   FilterCode = "ISKOLU",                  FilterName = "İş Kolu",              DisplayOrder = 1, IsMultiSelect = true,   FilterItemId = 15,  ItemCode = "TARIM",         ItemName = "Tarım",          ItemOrder = 1, IsDefault = true },

            new() { FilterGroupId = 4,   FilterCode = "TAHSIS_KOLU",             FilterName = "Tahsis Kodu",          DisplayOrder = 1, IsMultiSelect = true,   FilterItemId = 16,  ItemCode = null,            ItemName = "Tümü",           ItemOrder = 1, IsDefault = true },
            new() { FilterGroupId = 4,   FilterCode = "TAHSIS_KOLU",             FilterName = "Tahsis Kodu",          DisplayOrder = 1, IsMultiSelect = true,   FilterItemId = 17,  ItemCode = "BIREYSEL",      ItemName = "Bireysel",       ItemOrder = 1, IsDefault = true },
            new() { FilterGroupId = 4,   FilterCode = "TAHSIS_KOLU",             FilterName = "Tahsis Kodu",          DisplayOrder = 1, IsMultiSelect = true,   FilterItemId = 18,  ItemCode = "ISLETME",       ItemName = "İşletme",        ItemOrder = 1, IsDefault = true },
            new() { FilterGroupId = 4,   FilterCode = "TAHSIS_KOLU",             FilterName = "Tahsis Kodu",          DisplayOrder = 1, IsMultiSelect = true,   FilterItemId = 19,  ItemCode = "KREDIKARTI",    ItemName = "Kredi Kartı",    ItemOrder = 1, IsDefault = true },
            new() { FilterGroupId = 4,   FilterCode = "TAHSIS_KOLU",             FilterName = "Tahsis Kodu",          DisplayOrder = 1, IsMultiSelect = true,   FilterItemId = 20,  ItemCode = "TARIM",         ItemName = "Tarım",          ItemOrder = 1, IsDefault = true },

            new() { FilterGroupId = 5,   FilterCode = "BONUS_BUSINESS_FLAG",     FilterName = "Bonus Business",       DisplayOrder = 1, IsMultiSelect = false,  FilterItemId = 21,  ItemCode = null,            ItemName = "Tümü",           ItemOrder = 1, IsDefault = true },
            new() { FilterGroupId = 5,   FilterCode = "BONUS_BUSINESS_FLAG",     FilterName = "Bonus Business",       DisplayOrder = 1, IsMultiSelect = false,  FilterItemId = 22,  ItemCode = "1",             ItemName = "Evet",           ItemOrder = 1, IsDefault = true },
            new() { FilterGroupId = 5,   FilterCode = "BONUS_BUSINESS_FLAG",     FilterName = "Bonus Business",       DisplayOrder = 1, IsMultiSelect = false,  FilterItemId = 23,  ItemCode = "0",             ItemName = "Hayır",          ItemOrder = 1, IsDefault = true },

            new() { FilterGroupId = 6,   FilterCode = "BIREYSEL_MIKRO_FLAG",     FilterName = "Bireysel Mikro",       DisplayOrder = 1, IsMultiSelect = false,  FilterItemId = 24,  ItemCode = null,            ItemName = "Tümü",           ItemOrder = 1, IsDefault = true },
            new() { FilterGroupId = 6,   FilterCode = "BIREYSEL_MIKRO_FLAG",     FilterName = "Bireysel Mikro",       DisplayOrder = 1, IsMultiSelect = false,  FilterItemId = 25,  ItemCode = "1",             ItemName = "Evet",           ItemOrder = 1, IsDefault = true },
            new() { FilterGroupId = 6,   FilterCode = "BIREYSEL_MIKRO_FLAG",     FilterName = "Bireysel Mikro",       DisplayOrder = 1, IsMultiSelect = false,  FilterItemId = 26,  ItemCode = "0",             ItemName = "Hayır",          ItemOrder = 1, IsDefault = true },

            new() { FilterGroupId = 7,   FilterCode = "IRS_Flag",                FilterName = "IRS",                  DisplayOrder = 1, IsMultiSelect = false,  FilterItemId = 27,  ItemCode = null,            ItemName = "Tümü",           ItemOrder = 1, IsDefault = true },
            new() { FilterGroupId = 7,   FilterCode = "IRS_Flag",                FilterName = "IRS",                  DisplayOrder = 1, IsMultiSelect = false,  FilterItemId = 28,  ItemCode = "1",             ItemName = "Evet",           ItemOrder = 1, IsDefault = true },
            new() { FilterGroupId = 7,   FilterCode = "IRS_Flag",                FilterName = "IRS",                  DisplayOrder = 1, IsMultiSelect = false,  FilterItemId = 29,  ItemCode = "0",             ItemName = "Hayır",          ItemOrder = 1, IsDefault = true },

            new() { FilterGroupId = 8,   FilterCode = "YAPILANDIRMA_FLAG",       FilterName = "Yapılandırma Müşteri", DisplayOrder = 1, IsMultiSelect = false,  FilterItemId = 30,  ItemCode = null,            ItemName = "Tümü",           ItemOrder = 1, IsDefault = true },
            new() { FilterGroupId = 8,   FilterCode = "YAPILANDIRMA_FLAG",       FilterName = "Yapılandırma Müşteri", DisplayOrder = 1, IsMultiSelect = false,  FilterItemId = 31,  ItemCode = "1",             ItemName = "Evet",           ItemOrder = 1, IsDefault = true },
            new() { FilterGroupId = 8,   FilterCode = "YAPILANDIRMA_FLAG",       FilterName = "Yapılandırma Müşteri", DisplayOrder = 1, IsMultiSelect = false,  FilterItemId = 32,  ItemCode = "0",             ItemName = "Hayır",          ItemOrder = 1, IsDefault = true },

            new() { FilterGroupId = 9,   FilterCode = "YAPILANDIRMA_FLAG_KREDI", FilterName = "Yapılandırma Kredi",   DisplayOrder = 1, IsMultiSelect = false,  FilterItemId = 33,  ItemCode = "YAPILANDIRMA",  ItemName = "Yapılandırma",   ItemOrder = 1, IsDefault = true },
            new() { FilterGroupId = 9,   FilterCode = "YAPILANDIRMA_FLAG_KREDI", FilterName = "Yapılandırma Kredi",   DisplayOrder = 1, IsMultiSelect = false,  FilterItemId = 34,  ItemCode = "MODIFIKASYON",  ItemName = "Modifikasyon ",  ItemOrder = 1, IsDefault = true },
            new() { FilterGroupId = 9,   FilterCode = "YAPILANDIRMA_FLAG_KREDI", FilterName = "Yapılandırma Kredi",   DisplayOrder = 1, IsMultiSelect = false,  FilterItemId = 35,  ItemCode = "0",             ItemName = "Hayır",          ItemOrder = 1, IsDefault = true },

            new() { FilterGroupId = 10,  FilterCode = "IHTIYAC_TICARI_FLAG",     FilterName = "İhtiyaç Ticari",       DisplayOrder = 1, IsMultiSelect = false,  FilterItemId = 36,  ItemCode = null,            ItemName = "Tümü",           ItemOrder = 1, IsDefault = true },
            new() { FilterGroupId = 10,  FilterCode = "IHTIYAC_TICARI_FLAG",     FilterName = "İhtiyaç Ticari",       DisplayOrder = 1, IsMultiSelect = false,  FilterItemId = 37,  ItemCode = "1",             ItemName = "Evet",           ItemOrder = 1, IsDefault = true },
            new() { FilterGroupId = 10,  FilterCode = "IHTIYAC_TICARI_FLAG",     FilterName = "İhtiyaç Ticari",       DisplayOrder = 1, IsMultiSelect = false,  FilterItemId = 38,  ItemCode = "0",             ItemName = "Hayır",          ItemOrder = 1, IsDefault = true },

            new() { FilterGroupId = 11,  FilterCode = "KGFLI_KREDI_FLAG",        FilterName = "KGF'li Kredi",         DisplayOrder = 1, IsMultiSelect = false,  FilterItemId = 39,  ItemCode = null,            ItemName = "Tümü",           ItemOrder = 1, IsDefault = true },
            new() { FilterGroupId = 11,  FilterCode = "KGFLI_KREDI_FLAG",        FilterName = "KGF'li Kredi",         DisplayOrder = 1, IsMultiSelect = false,  FilterItemId = 40,  ItemCode = "1",             ItemName = "Evet",           ItemOrder = 1, IsDefault = true },
            new() { FilterGroupId = 11,  FilterCode = "KGFLI_KREDI_FLAG",        FilterName = "KGF'li Kredi",         DisplayOrder = 1, IsMultiSelect = false,  FilterItemId = 41,  ItemCode = "0",             ItemName = "Hayır",          ItemOrder = 1, IsDefault = true },

            new() { FilterGroupId = 12,  FilterCode = "EMEKLI_FLAG",             FilterName = "Emekli",               DisplayOrder = 1, IsMultiSelect = false,  FilterItemId = 42,  ItemCode = null,            ItemName = "Tümü",           ItemOrder = 1, IsDefault = true },
            new() { FilterGroupId = 12,  FilterCode = "EMEKLI_FLAG",             FilterName = "Emekli",               DisplayOrder = 1, IsMultiSelect = false,  FilterItemId = 43,  ItemCode = "1",             ItemName = "Evet",           ItemOrder = 1, IsDefault = true },
            new() { FilterGroupId = 12,  FilterCode = "EMEKLI_FLAG",             FilterName = "Emekli",               DisplayOrder = 1, IsMultiSelect = false,  FilterItemId = 44,  ItemCode = "0",             ItemName = "Hayır",          ItemOrder = 1, IsDefault = true },

            new() { FilterGroupId = 13,  FilterCode = "DB_MAAS_ODEMESI_FLAG",    FilterName = "Maaş Müşteri",         DisplayOrder = 1, IsMultiSelect = false,  FilterItemId = 45,  ItemCode = null,            ItemName = "Tümü",           ItemOrder = 1, IsDefault = true },
            new() { FilterGroupId = 13,  FilterCode = "DB_MAAS_ODEMESI_FLAG",    FilterName = "Maaş Müşteri",         DisplayOrder = 1, IsMultiSelect = false,  FilterItemId = 46,  ItemCode = "1",             ItemName = "Evet",           ItemOrder = 1, IsDefault = true },
            new() { FilterGroupId = 13,  FilterCode = "DB_MAAS_ODEMESI_FLAG",    FilterName = "Maaş Müşteri",         DisplayOrder = 1, IsMultiSelect = false,  FilterItemId = 47,  ItemCode = "0",             ItemName = "Hayır",          ItemOrder = 1, IsDefault = true }
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
