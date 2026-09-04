using DashboardTsy.Application.PosReport.Requests;
using DashboardTsy.Application.PosReport.Responses;

namespace DashboardTsy.Application.PosReport;

/// <summary>
/// PosReport için application katmanı kontratı. SP_RP_POS_Report çağrısını soyutlar.
/// </summary>
public interface IPosReportProvider
{
    /// <summary>
    /// POS raporu — verilen bölge / şube / iş kolu kırılımında her metrik için ay bazlı
    /// değer ve önceki aya göre farkı döner.
    /// </summary>
    IReadOnlyList<GetPosReportItem> GetPosReport(GetPosReportRequest request);
}
