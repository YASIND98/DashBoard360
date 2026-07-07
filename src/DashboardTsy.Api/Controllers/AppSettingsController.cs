using DashboardTsy.Application.AppSettings;
using Microsoft.AspNetCore.Mvc;

namespace DashboardTsy.Api.Controllers;

[ApiController]
[Route("app-settings")]
public class AppSettingsController : ControllerBase
{
    private readonly IAppSettingsService _appSettings;

    public AppSettingsController(IAppSettingsService appSettings)
    {
        _appSettings = appSettings;
    }

    [HttpGet]
    public ActionResult<IReadOnlyList<AppSettingDto>> GetAll()
        => Ok(_appSettings.GetAll());

    [HttpGet("{key}")]
    public ActionResult<AppSettingDto> Get(string key)
    {
        var dto = _appSettings.Get(key);
        return dto is null ? NotFound() : Ok(dto);
    }
}
