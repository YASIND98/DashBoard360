using DashboardTsy.Web.Models.Common.Request;

namespace DashboardTsy.Web.Models.TargetReport;

public class GetTrendAnalysisRequest : BaseReportRequest
{
    public string SessionId { get; set; } = string.Empty;

    public string? Bolge { get; set; }
    public string? SubeKodu { get; set; }
    public string? IsKolu { get; set; }
    public string? Segment { get; set; }
    public string? Urun { get; set; }
}
