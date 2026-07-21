using DashboardTsy.Application.SalaryCustomerReport.Requests;
using DashboardTsy.Application.SalaryCustomerReport.Responses;

namespace DashboardTsy.Application.SalaryCustomerReport;

/// <summary>
/// SalaryCustomerReport için application katmanı kontratı.
/// Stored procedure çağrılarını soyutlar.
/// </summary>
public interface ISalaryCustomerReportProvider
{
    /// <summary>
    /// Maaş Müşterileri Raporları ekranında üstte bulunan ana tab menülerini ve alt tab yapılarını döner.
    /// </summary>
    IReadOnlyList<GetSalaryCustomerReportTabItem> GetSalaryCustomerReportTabs(GetSalaryCustomerReportTabsRequest request);

    /// <summary>
    /// Hacim sekmesindeki tablonun kolon header bilgilerini döner (kolon adları, tarihleri, Maaş/Emekli etiketleri).
    /// </summary>
    GetSalaryCustomerVolumeReportHeadersResponse? GetSalaryCustomerVolumeReportHeaders(GetSalaryCustomerVolumeReportHeadersRequest request);

    /// <summary>
    /// Ana tabdan Hacim seçildiğinde gösterilen tablo verilerini döner (ürün satırları + Maaş/Emekli kırılımı).
    /// </summary>
    IReadOnlyList<GetSalaryCustomerVolumeReportItem> GetSalaryCustomerVolumeReport(GetSalaryCustomerVolumeReportRequest request);

    /// <summary>
    /// Çapraz Satış Gelişimi sekmesindeki tablonun kolon header bilgilerini döner (kolon adları, tarihleri, Maaş/Emekli etiketleri).
    /// </summary>
    GetSalaryCustomerCrossSellReportHeadersResponse? GetSalaryCustomerCrossSellReportHeaders(GetSalaryCustomerCrossSellReportHeadersRequest request);

    /// <summary>
    /// Ana tabdan Çapraz Satış Gelişimi seçildiğinde gösterilen tablo verilerini döner (ürün satırları + Maaş/Emekli kırılımı).
    /// </summary>
    IReadOnlyList<GetSalaryCustomerCrossSellReportItem> GetSalaryCustomerCrossSellReport(GetSalaryCustomerCrossSellReportRequest request);

    /// <summary>
    /// Banka Payı sekmesindeki tablonun kolon header bilgilerini döner (kolon grubu adları, ay etiketleri, Maaş/Emekli toggle etiketleri).
    /// </summary>
    GetSalaryCustomerBankShareReportHeadersResponse? GetSalaryCustomerBankShareReportHeaders(GetSalaryCustomerBankShareReportHeadersRequest request);

    /// <summary>
    /// Ana tabdan Banka Payı seçildiğinde gösterilen tablo verilerini döner (ürün-metrik satırları, CustomerType'a göre değişir).
    /// </summary>
    IReadOnlyList<GetSalaryCustomerBankShareReportItem> GetSalaryCustomerBankShareReport(GetSalaryCustomerBankShareReportRequest request);
}
