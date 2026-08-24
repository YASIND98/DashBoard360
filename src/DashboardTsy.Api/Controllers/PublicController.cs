using DashboardTsy.Api.Models;
using DashboardTsy.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DashboardTsy.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class PublicController(IWindowsAuthService authService) : ControllerBase
{
    /// <summary>
    /// Kutup JWT'sinin business session tanımlayıcısını taşıdığı claim adı.
    /// KutupYıldızı /api/SendSmsCode dönüşünde AccessToken'a bu isimle koyuyor
    /// (bkz. MockMobileAuthScenario.HandleSendSmsCode) ve UserLogin.SessionId ile
    /// aynı değere karşılık geliyor.
    /// </summary>
    private const string ChannelSessionClaim = "ChannelSessionId";

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

    /// <summary>
    /// Web login akışı için: sessionId query parametresi ile kullanıcıyı çözüp UsersDto döner.
    /// Web tarafı sonucu sunucu-tarafı session'a yazar (bkz. DashboardTsy.Web MobileAuthController.SessionLogin).
    /// Mobil taraf bu endpoint yerine <see cref="GetCurrentUser"/>'ı çağırır — orada sessionId JWT'den okunur.
    /// </summary>
    [HttpGet("GetUserBySession")]
    public async Task<ActionResult<ApiResponse<UsersDto>>> GetUserBySession([FromQuery] string sessionId, CancellationToken cancellationToken)
    {
        var result = await authService.GetUserBySessionAsync(sessionId, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    /// <summary>
    /// Mobil için: geçerli Kutup JWT'sinden ChannelSessionId claim'ini okuyup UsersDto döner.
    /// Client'ın sessionId taşıması gerekmez — token içinde geldiği için spoof edilemez ve tek kaynaktır.
    /// Token yoksa/geçersizse JwtBearer middleware'i 401 döner; token var ama claim yoksa aynı UsersDto zarfında
    /// "Geçersiz session bilgisi" mesajı döner (mevcut GetUserBySession davranışıyla tutarlı).
    /// </summary>
    [Authorize]
    [HttpGet("GetCurrentUser")]
    public async Task<ActionResult<ApiResponse<UsersDto>>> GetCurrentUser(CancellationToken cancellationToken)
    {
        var sessionId = User.FindFirst(ChannelSessionClaim)?.Value;
        var result = await authService.GetUserBySessionAsync(sessionId ?? string.Empty, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }
}
