using DashboardTsy.Api.Models;
using DashboardTsy.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace DashboardTsy.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class PublicController(IWindowsAuthService authService) : ControllerBase
{
    [HttpGet("WindowsLogin")]
    public async Task<ActionResult<ApiResponse<UsersDto>>> WindowsLogin([FromQuery] string username, CancellationToken cancellationToken)
    {
        var result = await authService.WindowsLoginAsync(username, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpGet("DomainLogin")]
    public async Task<ActionResult<ApiResponse<UsersDto>>> DomainLogin([FromQuery] string domainName, CancellationToken cancellationToken)
    {
        var result = await authService.DomainLoginAsync(domainName, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpGet("Login")]
    public async Task<ActionResult<ApiResponse<UsersDto>>> Login([FromQuery] string username, [FromQuery] string password, CancellationToken cancellationToken)
    {
        var result = await authService.LoginAsync(username, password, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }
}
