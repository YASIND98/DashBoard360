using DashboardTsy.Application.SalaryCustomerReport;
using DashboardTsy.Application.SalaryCustomerReport.Requests;
using DashboardTsy.Application.SalaryCustomerReport.Responses;
using Microsoft.AspNetCore.Mvc;

namespace DashboardTsy.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class SalaryCustomerReportController : ControllerBase
{
    private readonly ISalaryCustomerReportProvider _reportProvider;

    public SalaryCustomerReportController(ISalaryCustomerReportProvider reportProvider)
    {
        _reportProvider = reportProvider;
    }

    /// <summary>
    /// POST /SalaryCustomerReport/GetSalaryCustomerReportTabs
    /// Maaş Müşterileri Raporları ekranında üstte bulunan ana tab menülerini ve alt tab yapılarını döner.
    /// </summary>
    [HttpPost("GetSalaryCustomerReportTabs")]
    public ActionResult<IReadOnlyList<GetSalaryCustomerReportTabItem>> GetSalaryCustomerReportTabs(
        [FromBody] GetSalaryCustomerReportTabsRequest request)
    {
        if (request == null)
            return BadRequest();

        var result = _reportProvider.GetSalaryCustomerReportTabs(request);
        return Ok(result);
    }

    /// <summary>
    /// POST /SalaryCustomerReport/GetSalaryCustomerVolumeReportHeaders
    /// Hacim sekmesindeki tablonun kolon header bilgilerini döner (kolon adları, tarihleri, Maaş/Emekli etiketleri).
    /// </summary>
    [HttpPost("GetSalaryCustomerVolumeReportHeaders")]
    public ActionResult<GetSalaryCustomerVolumeReportHeadersResponse> GetSalaryCustomerVolumeReportHeaders(
        [FromBody] GetSalaryCustomerVolumeReportHeadersRequest request)
    {
        if (request == null)
            return BadRequest();

        var result = _reportProvider.GetSalaryCustomerVolumeReportHeaders(request);
        return Ok(result);
    }

    /// <summary>
    /// POST /SalaryCustomerReport/GetSalaryCustomerVolumeReport
    /// Ana tabdan Hacim seçildiğinde gösterilen tablo verilerini döner (ürün satırları + Maaş/Emekli kırılımı).
    /// </summary>
    [HttpPost("GetSalaryCustomerVolumeReport")]
    public ActionResult<IReadOnlyList<GetSalaryCustomerVolumeReportItem>> GetSalaryCustomerVolumeReport(
        [FromBody] GetSalaryCustomerVolumeReportRequest request)
    {
        if (request == null)
            return BadRequest();

        var result = _reportProvider.GetSalaryCustomerVolumeReport(request);
        return Ok(result);
    }

    /// <summary>
    /// POST /SalaryCustomerReport/GetSalaryCustomerCrossSellReportHeaders
    /// Çapraz Satış Gelişimi sekmesindeki tablonun kolon header bilgilerini döner (kolon adları, tarihleri, Maaş/Emekli etiketleri).
    /// </summary>
    [HttpPost("GetSalaryCustomerCrossSellReportHeaders")]
    public ActionResult<GetSalaryCustomerCrossSellReportHeadersResponse> GetSalaryCustomerCrossSellReportHeaders(
        [FromBody] GetSalaryCustomerCrossSellReportHeadersRequest request)
    {
        if (request == null)
            return BadRequest();

        var result = _reportProvider.GetSalaryCustomerCrossSellReportHeaders(request);
        return Ok(result);
    }

    /// <summary>
    /// POST /SalaryCustomerReport/GetSalaryCustomerCrossSellReport
    /// Ana tabdan Çapraz Satış Gelişimi seçildiğinde gösterilen tablo verilerini döner (ürün satırları + Maaş/Emekli kırılımı).
    /// </summary>
    [HttpPost("GetSalaryCustomerCrossSellReport")]
    public ActionResult<IReadOnlyList<GetSalaryCustomerCrossSellReportItem>> GetSalaryCustomerCrossSellReport(
        [FromBody] GetSalaryCustomerCrossSellReportRequest request)
    {
        if (request == null)
            return BadRequest();

        var result = _reportProvider.GetSalaryCustomerCrossSellReport(request);
        return Ok(result);
    }

    /// <summary>
    /// POST /SalaryCustomerReport/GetSalaryCustomerBankShareReportHeaders
    /// Banka Payı sekmesindeki tablonun kolon header bilgilerini döner (kolon grubu adları, ay etiketleri, Maaş/Emekli toggle etiketleri).
    /// </summary>
    [HttpPost("GetSalaryCustomerBankShareReportHeaders")]
    public ActionResult<GetSalaryCustomerBankShareReportHeadersResponse> GetSalaryCustomerBankShareReportHeaders(
        [FromBody] GetSalaryCustomerBankShareReportHeadersRequest request)
    {
        if (request == null)
            return BadRequest();

        var result = _reportProvider.GetSalaryCustomerBankShareReportHeaders(request);
        return Ok(result);
    }

    /// <summary>
    /// POST /SalaryCustomerReport/GetSalaryCustomerBankShareReport
    /// Ana tabdan Banka Payı seçildiğinde gösterilen tablo verilerini döner (ürün-metrik satırları, CustomerType'a göre değişir).
    /// </summary>
    [HttpPost("GetSalaryCustomerBankShareReport")]
    public ActionResult<IReadOnlyList<GetSalaryCustomerBankShareReportItem>> GetSalaryCustomerBankShareReport(
        [FromBody] GetSalaryCustomerBankShareReportRequest request)
    {
        if (request == null)
            return BadRequest();

        var result = _reportProvider.GetSalaryCustomerBankShareReport(request);
        return Ok(result);
    }
}
