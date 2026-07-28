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
}
