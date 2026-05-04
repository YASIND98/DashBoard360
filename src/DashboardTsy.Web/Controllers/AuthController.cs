using System.Net;
using System.Text.Json;
using DashboardTsy.Web.Models;
using DashboardTsy.Web.Models.Activity;
using DashboardTsy.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace DashboardTsy.Web.Controllers;

/// <summary>
/// [Authorize] açıkken: IIS Express + Windows Auth ile çalıştırıldığında ilk istekte tarayıcı Windows login popup'ı açılır (DBRapor gibi).
/// Kestrel (dotnet run) ile çalıştırıyorsan popup çıkmaz; Login sayfası doğrudan açılır.
/// </summary>
public class AuthController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly IUserActivityLogService _activityLog;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public AuthController(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        IUserActivityLogService activityLog)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _activityLog = activityLog;
    }

    private bool AuthMockEnabled => _configuration.GetValue<bool>("AuthMock:Enabled");

    private static ApiResponse<UsersDto> MockOkUser(string? userNameOrEmail)
    {
        var user = new UsersDto
        {
            UserId = 999,
            NameSurname = "Mock Kullanıcı",
            Email = userNameOrEmail,
            DomainName = userNameOrEmail,
            Department = "Mock Departman",
            BranchCode = 1001,
            BranchName = "Mock Şube",
            RegionCode = 1,
            Authority = "Admin",
            Password = "mock-token",
            IsBlock = false,
            UpdateSeen = true,
            CreatedDate = DateTime.Now,
            ProfilePhoto = "https://cdn.pixabay.com/photo/2023/02/18/11/00/icon-7797704_640.png"
        };

        return new ApiResponse<UsersDto>
        {
            Result = user,
            Message = new MessageResult { message = "OK", message2 = "Mock login başarılı." }
        };
    }

    private static ApiResponse<UsersDto> MockNotLoggedIn(string message2)
    {
        return new ApiResponse<UsersDto>
        {
            Result = new UsersDto { UserId = 0 },
            Message = new MessageResult { message = "Mock", message2 = message2 }
        };
    }

    [HttpGet]
    public async Task<IActionResult> Login(CancellationToken cancellationToken)
    {
        var userName = User?.Identity?.Name;
        if (string.IsNullOrEmpty(userName))
            return View();

        if (AuthMockEnabled)
        {
            // In mock mode, do NOT auto-login. Keep showing the login screen so UI can be tested.
            return View();
        }

        var baseUrl = _configuration["DashboardApi:BaseUrl"]?.TrimEnd('/') + "/";
        var url = $"{baseUrl}Public/WindowsLogin?username={WebUtility.UrlEncode(userName)}";

        ApiResponse<UsersDto>? result = null;
        try
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync(url, cancellationToken).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                result = JsonSerializer.Deserialize<ApiResponse<UsersDto>>(json, JsonOptions);
            }
        }
        catch
        {
            // ignore
        }

        if (result?.Result != null && result.Result.UserId > 0 && result.Result.IsBlock != true)
        {
            SetSession(result.Result);
            await LogLoginSuccessAsync("Windows", result.Result, cancellationToken).ConfigureAwait(false);

            return RedirectToAction("Index", "Home");
        }

        return View();
    }

    [HttpGet]
    public async Task<JsonResult> LoginDomain(CancellationToken cancellationToken)
    {
        var userName = User?.Identity?.Name ?? string.Empty;

        if (AuthMockEnabled)
        {
            // Login page calls this on load; return "not logged in" so it falls back to local login UI.
            return Json(MockNotLoggedIn("Mock mod aktif: otomatik domain login kapalı."));
        }

        var baseUrl = _configuration["DashboardApi:BaseUrl"]?.TrimEnd('/') + "/";
        var url = $"{baseUrl}Public/DomainLogin?domainName={WebUtility.UrlEncode(userName)}";

        ApiResponse<UsersDto>? result = null;
        try
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync(url, cancellationToken).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                result = JsonSerializer.Deserialize<ApiResponse<UsersDto>>(json, JsonOptions);
            }
        }
        catch
        {
            // ignore
        }

        result ??= new ApiResponse<UsersDto> { Result = new UsersDto(), Message = new MessageResult { message = "Hata", message2 = "Servis yanıtı alınamadı." } };

        if (result.Result != null && result.Result.UserId > 0 && result.Result.IsBlock != true)
        {
            SetSession(result.Result);
            await LogLoginSuccessAsync("Domain", result.Result, cancellationToken).ConfigureAwait(false);
        }

        return Json(result);
    }

    [HttpPost]
    public async Task<JsonResult> LoginUser([FromForm] LoginModel model, CancellationToken cancellationToken)
    {
        if (AuthMockEnabled)
        {
            var mock = MockOkUser(model.Email);
            if (mock.Result != null)
            {
                SetSession(mock.Result);
                await LogLoginSuccessAsync("Form(Mock)", mock.Result, cancellationToken).ConfigureAwait(false);
            }

            return Json(mock);
        }

        var baseUrl = _configuration["DashboardApi:BaseUrl"]?.TrimEnd('/') + "/";
        var url = $"{baseUrl}Public/Login?username={WebUtility.UrlEncode(model.Email ?? string.Empty)}&password={WebUtility.UrlEncode(model.Pass ?? string.Empty)}";

        ApiResponse<UsersDto>? result = null;
        try
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync(url, cancellationToken).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                result = JsonSerializer.Deserialize<ApiResponse<UsersDto>>(json, JsonOptions);
            }
        }
        catch
        {
            // ignore
        }

        result ??= new ApiResponse<UsersDto> { Result = new UsersDto(), Message = new MessageResult { message = "Hata", message2 = "Servis yanıtı alınamadı." } };

        if (result.Result != null && result.Result.UserId > 0 && result.Result.IsBlock != true)
        {
            SetSession(result.Result);
            await LogLoginSuccessAsync("Form", result.Result, cancellationToken).ConfigureAwait(false);
        }

        return Json(result);
    }

    [HttpGet]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        var userId = HttpContext.Session.GetInt32("UserId") ?? 0;
        if (userId > 0)
        {
            await _activityLog.LogAsync(new UserActivityLogEntry
            {
                UserId = userId,
                UserDisplayName = HttpContext.Session.GetString("NameSurname"),
                EventType = "Logout",
                ActionName = "SessionEnd"
            }, cancellationToken).ConfigureAwait(false);
        }

        HttpContext.Session.Clear();
        return RedirectToAction("Login", "Auth");
    }

    private void SetSession(UsersDto user)
    {
        HttpContext.Session.SetInt32("UserId", user.UserId);
        HttpContext.Session.SetString("NameSurname", user.NameSurname ?? string.Empty);

        if (!string.IsNullOrEmpty(user.Authority))
            HttpContext.Session.SetString("Authority", user.Authority);
        if (!string.IsNullOrEmpty(user.ProfilePhoto))
            HttpContext.Session.SetString("ProfilePhoto", user.ProfilePhoto);
        if (user.BranchCode.HasValue)
            HttpContext.Session.SetInt32("BranchCode", user.BranchCode.Value);
        if (!string.IsNullOrEmpty(user.BranchName))
            HttpContext.Session.SetString("BranchName", user.BranchName);
        if (!string.IsNullOrEmpty(user.Department))
            HttpContext.Session.SetString("Department", user.Department);
        if (user.RegionCode.HasValue)
            HttpContext.Session.SetInt32("RegionCode", user.RegionCode.Value);
        if (user.UpdateSeen.HasValue)
            HttpContext.Session.SetString("UpdateSeen", user.UpdateSeen.Value.ToString());

        HttpContext.Session.SetString("User", JsonSerializer.Serialize(user));
        if (!string.IsNullOrEmpty(user.Password))
            HttpContext.Session.SetString("token", user.Password);
    }

    private Task LogLoginSuccessAsync(string loginMethod, UsersDto user, CancellationToken cancellationToken)
    {
        return _activityLog.LogAsync(new UserActivityLogEntry
        {
            UserId = user.UserId,
            UserDisplayName = user.NameSurname,
            EventType = "Login",
            ActionName = loginMethod
        }, cancellationToken);
    }
}
