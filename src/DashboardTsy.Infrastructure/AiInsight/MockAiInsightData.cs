using DashboardTsy.Application.AiInsight.Responses;

namespace DashboardTsy.Infrastructure.AiInsight;

public static class MockAiInsightData
{
    private const string BranchSummary = """
        # YÖNETİCİ ÖZETİ — İstanbul Merkez Şubesi

        **Tarih:** 2025-06-01

        ---

        ## 1. Yönetici Özeti

        Şube, Haziran 2025 itibarıyla bireysel kredi ürünlerinde bölge ortalamasının <span style="color:green">**%12**</span> üzerinde performans sergilemiştir. Mevduat tarafında kısa vadeli ürünlerde hedefin **%95**'ine ulaşılmıştır.

        ---

        ## 2. Ürünler

        ### 2.1. Özet

        Bireysel kredi portföyü bir önceki aya göre **%8** büyümüştür.

        ### 2.2. Özet Metrik Tablo

        | Ürün Adı | Şube | Bölge Ort. | Banka Ort. |
        |---|---|---|---|
        | İhtiyaç Kredisi | **1.240** | **1.105** | **1.080** |
        | Konut Kredisi | **860** | **910** | **845** |
        | Vadesiz Mevduat | **2.310** | **2.180** | **2.240** |

        ### 2.3. ⚠️ Zayıf Yönler

        Konut kredisinde bölge ortalamasının <span style="color:red">**-50**</span> birim gerisindedir.

        ---

        ## 3. Karlılık

        ### 3.1. Özet

        Net komisyon geliri bölge ortalamasının üzerinde, net faiz geliri gerisindedir.
        """;

    public static GetBranchAiInsightResponse GetBranchAiInsights(string regionCode, string branchCode)
        => new()
        {
            Items = new List<GetBranchAiInsightItem>
            {
                new()
                {
                    Id = 1,
                    Region = "Marmara",
                    BranchName = "İstanbul Merkez Şubesi",
                    BranchCode = branchCode,
                    SummaryDate = new DateTime(2025, 6, 1),
                    Summary = BranchSummary,
                    ModelName = "gpt-4o",
                    CreatedAt = new DateTime(2025, 6, 2, 8, 0, 0)
                }
            }
        };
}
