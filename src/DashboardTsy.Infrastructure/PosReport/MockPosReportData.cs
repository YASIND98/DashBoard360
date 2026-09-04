using DashboardTsy.Application.PosReport.Requests;
using DashboardTsy.Application.PosReport.Responses;

namespace DashboardTsy.Infrastructure.PosReport;

/// <summary>
/// PosReport için mock veri kaynağı.
/// PosReportProvider, ReportMock:Enabled true olduğunda buraya delege eder.
///
/// Mock; ekrandaki tabloya paralel olarak 5 metrik x 7 ay (güncel + son 6 ay) = 35 satır döner.
/// Değerler ve farklar, client tarafında pozitif/negatif ayrımının test edilebilmesi için karışık dağıtılmıştır.
/// </summary>
public static class MockPosReportData
{
    private static readonly string[] Metrics =
    {
        "Sahip Müşteri Adedi",
        "Aktif Müşteri Adedi",
        "Ciro Müşteri Adedi",
        "Yeni Kazanım Müşteri Adedi",
        "İptal Müşteri Adedi"
    };

    // Ekrandaki başlıklara birebir: Güncel Dönem + son 6 ayın son günü.
    // Bir önceki ay farkı hesaplanabilsin diye tarihler eskiden yeniye sıralı üretiliyor.
    private static readonly DateTime[] Dates =
    {
        new(2026, 7, 30),
        new(2026, 8, 31),
        new(2026, 9, 30),
        new(2026, 10, 31),
        new(2026, 11, 30),
        new(2026, 12, 31),
        new(2027, 1, 12)  // Güncel Dönem (rapor tarihi)
    };

    public static IReadOnlyList<GetPosReportItem> GetPosReport(GetPosReportRequest request)
    {
        var items = new List<GetPosReportItem>(Metrics.Length * Dates.Length);

        for (var m = 0; m < Metrics.Length; m++)
        {
            long? previousValue = null;

            for (var d = 0; d < Dates.Length; d++)
            {
                // Her metrik için deterministik ama birbirinden farklı bir seri üret;
                // gerçek SP çıktısını taklit etmek değil, ekranda anlamlı bir tablo göstermek amaç.
                long value = 123_456L + (m * 10_000L) + (d * 1_500L);
                long? diff = previousValue.HasValue ? value - previousValue.Value : (long?)null;

                items.Add(new GetPosReportItem
                {
                    Metrics = Metrics[m],
                    Date = Dates[d],
                    Value = value,
                    DiffValue = diff
                });

                previousValue = value;
            }
        }

        return items;
    }
}
