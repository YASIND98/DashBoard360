namespace DashboardTsy.Application.Common.Requests;

public abstract class BaseReportRequest
{
    public long? ProductId { get; set; }

    public string? UserCode { get; set; }
}
