using DashboardTsy.Web.Models.Activity;
using DashboardTsy.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace DashboardTsy.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ActivityLogController : ControllerBase
{
    private readonly IUserActivityLogService _activityLog;

    public ActivityLogController(IUserActivityLogService activityLog)
    {
        _activityLog = activityLog;
    }

    /// <summary>Tarayıcıdan gelen sayfa görüntüleme / tıklama / navigasyon kayıtları.</summary>
    [HttpPost("client")]
    public async Task<IActionResult> PostClientAsync([FromBody] ClientActivityLogRequest? body, CancellationToken cancellationToken)
    {
        var userId = HttpContext.Session.GetInt32("UserId") ?? 0;
        if (userId <= 0)
            return Unauthorized();

        if (body == null || string.IsNullOrWhiteSpace(body.EventType))
            return BadRequest();

        var displayName = HttpContext.Session.GetString("NameSurname");

        await _activityLog.LogAsync(new UserActivityLogEntry
        {
            UserId = userId,
            UserDisplayName = displayName,
            EventType = body.EventType.Trim(),
            ActionName = string.IsNullOrWhiteSpace(body.ActionName) ? null : body.ActionName.Trim(),
            Route = null,
            PageUrl = string.IsNullOrWhiteSpace(body.PageUrl) ? null : body.PageUrl.Trim(),
            Details = string.IsNullOrWhiteSpace(body.Details) ? null : body.Details.Trim()
        }, cancellationToken).ConfigureAwait(false);

        return Ok();
    }
}
