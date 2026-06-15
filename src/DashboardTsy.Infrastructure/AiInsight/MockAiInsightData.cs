using DashboardTsy.Application.AiInsight.Responses;

namespace DashboardTsy.Infrastructure.AiInsight;

public static class MockAiInsightData
{
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
                    Summary = "Şube, Haziran 2025 itibarıyla bireysel kredi ürünlerinde bölge ortalamasının %12 üzerinde performans sergilemiştir. Mevduat tarafında ise kısa vadeli ürünlerde hedefin %95'ine ulaşılmıştır. Müşteri memnuniyeti skoru 4.3/5 olarak ölçülmüştür.",
                    ModelName = "gpt-4o",
                    CreatedAt = new DateTime(2025, 6, 2, 8, 0, 0)
                },
                new()
                {
                    Id = 2,
                    Region = "Marmara",
                    BranchName = "İstanbul Merkez Şubesi",
                    BranchCode = branchCode,
                    SummaryDate = new DateTime(2025, 5, 1),
                    Summary = "Mayıs 2025 döneminde ticari kredi portföyü bir önceki aya göre %8 büyümüştür. KOBİ segmentinde yeni müşteri edinimi hedefin üzerinde gerçekleşmiş, toplam 47 yeni müşteri kazanılmıştır. Operasyonel verimlilik göstergelerinde iyileşme gözlemlenmiştir.",
                    ModelName = "gpt-4o",
                    CreatedAt = new DateTime(2025, 5, 2, 8, 0, 0)
                }
            }
        };
}
