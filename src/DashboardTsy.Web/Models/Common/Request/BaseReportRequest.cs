namespace DashboardTsy.Web.Models.Common.Request;

public abstract class BaseReportRequest
{
    public long? ProductId { get; set; }

    public string? UserCode { get; set; }
}
