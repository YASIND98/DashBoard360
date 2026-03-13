using DashboardTsy.Application.TargetReport;
using Microsoft.AspNetCore.Mvc;

namespace DashboardTsy.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class ProductivityReportController : ControllerBase
{
    private readonly IReportDataProvider _reportDataProvider;

    public ProductivityReportController(IReportDataProvider reportDataProvider)
    {
        _reportDataProvider = reportDataProvider;
    }

    /// <summary>
    /// POST /ProductivityReport/GetProductivityReportTabs
    /// Verimlilik ekranı için sekme ağaç yapısını döner.
    /// Şu an sadece mock veri üzerinden çalışır.
    /// </summary>
    [HttpPost("GetProductivityReportTabs")]
    public ActionResult<IReadOnlyList<GetProductivityReportTabItem>> GetProductivityReportTabs(
        [FromBody] GetProductivityReportTabsRequest request)
    {
        if (request == null)
            return BadRequest();

        var result = _reportDataProvider.GetProductivityReportTabs(request);
        return Ok(result);
    }

    /// <summary>
    /// POST /ProductivityReport/GetProductivityReportTableHeaders
    /// Verimlilik raporu için tablo kolon başlıklarını (çok seviyeli header yapısı) döner.
    /// Şu an SP tanımlanmadığı için mock veri üzerinden çalışır.
    /// </summary>
    [HttpPost("GetProductivityReportTableHeaders")]
    public ActionResult<IReadOnlyList<GetProductivityReportTableHeaderItem>> GetProductivityReportTableHeaders(
        [FromBody] GetProductivityReportTableHeadersRequest request)
    {
        if (request == null)
            return BadRequest();

        var result = _reportDataProvider.GetProductivityReportTableHeaders(request);
        return Ok(result);
    }

    /// <summary>
    /// POST /ProductivityReport/GetReportRegionFilters
    /// Verimlilik raporu ekranı için bölge filtre seçeneklerini döner.
    /// Şu an SP adı/parametreleri belli olmadığı için mock veri üzerinden çalışır.
    /// </summary>
    [HttpPost("GetReportRegionFilters")]
    public ActionResult<IReadOnlyList<GetReportRegionFilterItem>> GetReportRegionFilters(
        [FromBody] GetReportRegionFiltersRequest request)
    {
        if (request == null)
            return BadRequest();

        var result = _reportDataProvider.GetReportRegionFilters(request);
        return Ok(result);
    }

    /// <summary>
    /// POST /ProductivityReport/GetReportBranchFilters
    /// Verimlilik raporu ekranı için şube filtre seçeneklerini döner (bölge kodu ile birlikte).
    /// Şu an SP adı/parametreleri belli olmadığı için mock veri üzerinden çalışır.
    /// </summary>
    [HttpPost("GetReportBranchFilters")]
    public ActionResult<IReadOnlyList<GetReportBranchFilterItem>> GetReportBranchFilters(
        [FromBody] GetReportBranchFiltersRequest request)
    {
        if (request == null)
            return BadRequest();

        var result = _reportDataProvider.GetReportBranchFilters(request);
        return Ok(result);
    }

    /// <summary>
    /// POST /ProductivityReport/GetProductivityGeneralRegionReport
    /// Verimlilik ekranı için genel bölge bazlı raporu döner.
    /// Şu an SP tanımlanmadığı için mock veri üzerinden çalışır.
    /// </summary>
    [HttpPost("GetProductivityGeneralRegionReport")]
    public ActionResult<IReadOnlyList<GetProductivityGeneralRegionReportItem>> GetProductivityGeneralRegionReport(
        [FromBody] GetProductivityGeneralRegionReportRequest request)
    {
        if (request == null)
            return BadRequest();

        var result = _reportDataProvider.GetProductivityGeneralRegionReport(request);
        return Ok(result);
    }

    /// <summary>
    /// POST /ProductivityReport/GetProductivityCountCardPosRegionReport
    /// Verimlilik ekranı için KrediKartı/POS sayısal bölge raporunu döner.
    /// Şu an SP tanımlanmadığı için mock veri üzerinden çalışır.
    /// </summary>
    [HttpPost("GetProductivityCountCardPosRegionReport")]
    public ActionResult<IReadOnlyList<GetProductivityCountCardPosRegionReportItem>> GetProductivityCountCardPosRegionReport(
        [FromBody] GetProductivityCountCardPosRegionReportRequest request)
    {
        if (request == null)
            return BadRequest();

        var result = _reportDataProvider.GetProductivityCountCardPosRegionReport(request);
        return Ok(result);
    }

    /// <summary>
    /// POST /ProductivityReport/GetProductivityCountCustomerRegionReport
    /// Verimlilik ekranı için müşteri sayısı bölge raporunu döner (Tümü/Kurumsal/Ticari/KOBİ/Tarım/Bireysel).
    /// Şu an SP tanımlanmadığı için mock veri üzerinden çalışır.
    /// </summary>
    [HttpPost("GetProductivityCountCustomerRegionReport")]
    public ActionResult<IReadOnlyList<GetProductivityCountCustomerRegionReportItem>> GetProductivityCountCustomerRegionReport(
        [FromBody] GetProductivityCountCustomerRegionReportRequest request)
    {
        if (request == null)
            return BadRequest();

        var result = _reportDataProvider.GetProductivityCountCustomerRegionReport(request);
        return Ok(result);
    }

    /// <summary>
    /// POST /ProductivityReport/GetProductivityVolumeRegionReport
    /// Verimlilik ekranı için hacim bölge raporunu döner.
    /// Şu an SP tanımlanmadığı için mock veri üzerinden çalışır.
    /// </summary>
    [HttpPost("GetProductivityVolumeRegionReport")]
    public ActionResult<IReadOnlyList<GetProductivityVolumeRegionReportItem>> GetProductivityVolumeRegionReport(
        [FromBody] GetProductivityVolumeRegionReportRequest request)
    {
        if (request == null)
            return BadRequest();

        var result = _reportDataProvider.GetProductivityVolumeRegionReport(request);
        return Ok(result);
    }

    /// <summary>
    /// POST /ProductivityReport/GetProductivityProfitRatioRegionReport
    /// Verimlilik ekranı için karlılık oranı bölge raporunu döner.
    /// Şu an SP tanımlanmadığı için mock veri üzerinden çalışır.
    /// </summary>
    [HttpPost("GetProductivityProfitRatioRegionReport")]
    public ActionResult<IReadOnlyList<GetProductivityProfitRatioRegionReportItem>> GetProductivityProfitRatioRegionReport(
        [FromBody] GetProductivityProfitRatioRegionReportRequest request)
    {
        if (request == null)
            return BadRequest();

        var result = _reportDataProvider.GetProductivityProfitRatioRegionReport(request);
        return Ok(result);
    }

    /// <summary>
    /// POST /ProductivityReport/GetProductivityProfitTotalRegionReport
    /// Verimlilik ekranı için toplam karlılık bölge raporunu döner.
    /// Şu an SP tanımlanmadığı için mock veri üzerinden çalışır.
    /// </summary>
    [HttpPost("GetProductivityProfitTotalRegionReport")]
    public ActionResult<IReadOnlyList<GetProductivityProfitTotalRegionReportItem>> GetProductivityProfitTotalRegionReport(
        [FromBody] GetProductivityProfitTotalRegionReportRequest request)
    {
        if (request == null)
            return BadRequest();

        var result = _reportDataProvider.GetProductivityProfitTotalRegionReport(request);
        return Ok(result);
    }

    /// <summary>
    /// POST /ProductivityReport/GetProductivityProfitSpreadManagementRegionReport
    /// Verimlilik ekranı için spread yönetimi karlılık bölge raporunu döner.
    /// Şu an SP tanımlanmadığı için mock veri üzerinden çalışır.
    /// </summary>
    [HttpPost("GetProductivityProfitSpreadManagementRegionReport")]
    public ActionResult<IReadOnlyList<GetProductivityProfitSpreadManagementRegionReportItem>> GetProductivityProfitSpreadManagementRegionReport(
        [FromBody] GetProductivityProfitSpreadManagementRegionReportRequest request)
    {
        if (request == null)
            return BadRequest();

        var result = _reportDataProvider.GetProductivityProfitSpreadManagementRegionReport(request);
        return Ok(result);
    }

    /// <summary>
    /// POST /ProductivityReport/GetProductivityProfitSpreadManagementBranchReport
    /// Verimlilik ekranı için spread yönetimi karlılık şube raporunu döner.
    /// Şu an SP tanımlanmadığı için mock veri üzerinden çalışır.
    /// </summary>
    [HttpPost("GetProductivityProfitSpreadManagementBranchReport")]
    public ActionResult<IReadOnlyList<GetProductivityProfitSpreadManagementBranchReportItem>> GetProductivityProfitSpreadManagementBranchReport(
        [FromBody] GetProductivityProfitSpreadManagementBranchReportRequest request)
    {
        if (request == null)
            return BadRequest();

        var result = _reportDataProvider.GetProductivityProfitSpreadManagementBranchReport(request);
        return Ok(result);
    }

    /// <summary>
    /// POST /ProductivityReport/GetProductivityCountCardPosBranchReport
    /// Verimlilik ekranı için KrediKartı/POS sayısal şube raporunu döner.
    /// Şu an SP tanımlanmadığı için mock veri üzerinden çalışır.
    /// </summary>
    [HttpPost("GetProductivityCountCardPosBranchReport")]
    public ActionResult<IReadOnlyList<GetProductivityCountCardPosBranchReportItem>> GetProductivityCountCardPosBranchReport(
        [FromBody] GetProductivityCountCardPosBranchReportRequest request)
    {
        if (request == null)
            return BadRequest();

        var result = _reportDataProvider.GetProductivityCountCardPosBranchReport(request);
        return Ok(result);
    }

    /// <summary>
    /// POST /ProductivityReport/GetProductivityProfitRatioBranchReport
    /// Verimlilik ekranı için karlılık oranı şube raporunu döner.
    /// Şu an SP tanımlanmadığı için mock veri üzerinden çalışır.
    /// </summary>
    [HttpPost("GetProductivityProfitRatioBranchReport")]
    public ActionResult<IReadOnlyList<GetProductivityProfitRatioBranchReportItem>> GetProductivityProfitRatioBranchReport(
        [FromBody] GetProductivityProfitRatioBranchReportRequest request)
    {
        if (request == null)
            return BadRequest();

        var result = _reportDataProvider.GetProductivityProfitRatioBranchReport(request);
        return Ok(result);
    }

    /// <summary>
    /// POST /ProductivityReport/GetProductivityProfitTotalBranchReport
    /// Verimlilik ekranı için karlılık toplam şube raporunu döner.
    /// Şu an SP tanımlanmadığı için mock veri üzerinden çalışır.
    /// </summary>
    [HttpPost("GetProductivityProfitTotalBranchReport")]
    public ActionResult<IReadOnlyList<GetProductivityProfitTotalBranchReportItem>> GetProductivityProfitTotalBranchReport(
        [FromBody] GetProductivityProfitTotalBranchReportRequest request)
    {
        if (request == null)
            return BadRequest();

        var result = _reportDataProvider.GetProductivityProfitTotalBranchReport(request);
        return Ok(result);
    }

    /// <summary>
    /// POST /ProductivityReport/GetProductivityBranchScoreCardReport
    /// Verimlilik ekranı için şube skor kartı raporunu döner (tek obje).
    /// Şu an SP tanımlanmadığı için mock veri üzerinden çalışır.
    /// </summary>
    [HttpPost("GetProductivityBranchScoreCardReport")]
    public ActionResult<GetProductivityBranchScoreCardReportItem> GetProductivityBranchScoreCardReport(
        [FromBody] GetProductivityBranchScoreCardReportRequest request)
    {
        if (request == null)
            return BadRequest();

        var result = _reportDataProvider.GetProductivityBranchScoreCardReport(request);
        return Ok(result);
    }

    /// <summary>
    /// POST /ProductivityReport/GetProductivityRegionScoreCardReport
    /// Verimlilik ekranı için bölge skor kartı raporunu döner (tek obje).
    /// Şu an SP tanımlanmadığı için mock veri üzerinden çalışır.
    /// </summary>
    [HttpPost("GetProductivityRegionScoreCardReport")]
    public ActionResult<GetProductivityRegionScoreCardReportItem> GetProductivityRegionScoreCardReport(
        [FromBody] GetProductivityRegionScoreCardReportRequest request)
    {
        if (request == null)
            return BadRequest();

        var result = _reportDataProvider.GetProductivityRegionScoreCardReport(request);
        return Ok(result);
    }

    /// <summary>
    /// POST /ProductivityReport/GetProductivityCountCustomerBranchReport
    /// Verimlilik ekranı için müşteri sayısı şube raporunu döner.
    /// Şu an SP tanımlanmadığı için mock veri üzerinden çalışır.
    /// </summary>
    [HttpPost("GetProductivityCountCustomerBranchReport")]
    public ActionResult<IReadOnlyList<GetProductivityCountCustomerBranchReportItem>> GetProductivityCountCustomerBranchReport(
        [FromBody] GetProductivityCountCustomerBranchReportRequest request)
    {
        if (request == null)
            return BadRequest();

        var result = _reportDataProvider.GetProductivityCountCustomerBranchReport(request);
        return Ok(result);
    }

    /// <summary>
    /// POST /ProductivityReport/GetProductivityVolumeBranchReport
    /// Verimlilik ekranı için hacim şube raporunu döner.
    /// Şu an SP tanımlanmadığı için mock veri üzerinden çalışır.
    /// </summary>
    [HttpPost("GetProductivityVolumeBranchReport")]
    public ActionResult<IReadOnlyList<GetProductivityVolumeBranchReportItem>> GetProductivityVolumeBranchReport(
        [FromBody] GetProductivityVolumeBranchReportRequest request)
    {
        if (request == null)
            return BadRequest();

        var result = _reportDataProvider.GetProductivityVolumeBranchReport(request);
        return Ok(result);
    }
}

