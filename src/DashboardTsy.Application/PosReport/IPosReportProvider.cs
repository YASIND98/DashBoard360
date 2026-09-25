using DashboardTsy.Application.PosReport.Requests;
using DashboardTsy.Application.PosReport.Responses;

namespace DashboardTsy.Application.PosReport;

/// <summary>
/// PosReport için application katmanı kontratı. SP_RP_POS_Report ve SP_RP_POS_Report_Skorkart çağrılarını soyutlar.
/// </summary>
public interface IPosReportProvider
{
    /// <summary>
    /// POS raporu — verilen bölge / şube / iş kolu kırılımında her metrik için ay bazlı
    /// değer ve önceki aya göre farkı döner.
    /// </summary>
    IReadOnlyList<GetPosReportItem> GetPosReport(GetPosReportRequest request);

    /// <summary>
    /// POS skorkartı — verilen bölge ya da şube için ürün bazlı gerçekleşen, hedef,
    /// gerçekleşme oranı ve hedefe kalan farkı döner.
    /// </summary>
    IReadOnlyList<GetPosScorecardItem> GetPosScorecard(GetPosScorecardRequest request);
}
