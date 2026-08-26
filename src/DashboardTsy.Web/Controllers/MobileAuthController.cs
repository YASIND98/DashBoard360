using System.Net;
using System.Text.Json;
using DashboardTsy.Web.Models;
using DashboardTsy.Web.Models.Activity;
using DashboardTsy.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DashboardTsy.Web.Controllers;

// Windows/Negotiate authentication mobil WebView'de calismadigi icin
// mobil istemcilere ozel, sadece form (username/password) login destekleyen
// ayri bir controller. Route: /mobile/*
[AllowAnonymous]
[Route("mobile/[action]")]
public class MobileAuthController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly IUserActivityLogService _activityLog;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public MobileAuthController(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        IUserActivityLogService activityLog)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _activityLog = activityLog;
    }

    private bool AuthMockEnabled => _configuration.GetValue<bool>("AuthMock:Enabled");

    [HttpGet]
    public IActionResult Login()
    {
        if (HttpContext.Session.GetInt32("UserId") is int userId && userId > 0)
            return Redirect("/");

        return View();
    }

    [HttpPost]
    public async Task<JsonResult> LoginUser([FromForm] LoginModel model, CancellationToken cancellationToken)
    {
        if (AuthMockEnabled)
        {
            var mock = BuildMockUser(model.Email);
            SetSession(mock, model.Email);
            await LogLoginSuccessAsync("MobileForm(Mock)", mock, cancellationToken).ConfigureAwait(false);
            return Json(new ApiResponse<UsersDto>
            {
                Result = mock,
                Message = new MessageResult { message = "OK", message2 = "Mock login basarili." }
            });
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

        result ??= new ApiResponse<UsersDto>
        {
            Result = new UsersDto(),
            Message = new MessageResult { message = "Hata", message2 = "Servis yaniti alinamadi." }
        };

        if (result.Result != null && result.Result.UserId > 0 && result.Result.IsBlock != true)
        {
            SetSession(result.Result, model.Email);
            await LogLoginSuccessAsync("MobileForm", result.Result, cancellationToken).ConfigureAwait(false);
        }

        return Json(result);
    }

    [HttpGet]
    public async Task<IActionResult> SessionLogin([FromQuery] string sessionId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            return BadRequest(new ApiResponse<UsersDto>
            {
                Result = new UsersDto(),
                Message = new MessageResult { message = "Hata", message2 = "Session bilgisi boş olamaz." }
            });

        if (AuthMockEnabled)
        {
            var mock = BuildMockUser("mock-session-user");
            SetSession(mock, mock.DomainName);
            await LogLoginSuccessAsync("MobileSession(Mock)", mock, cancellationToken).ConfigureAwait(false);
            return Redirect("/");
        }

        var baseUrl = _configuration["DashboardApi:BaseUrl"]?.TrimEnd('/') + "/";
        var url = $"{baseUrl}Public/GetUserBySession?sessionId={WebUtility.UrlEncode(sessionId)}";

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

        if (result?.Result == null || result.Result.UserId <= 0 || result.Result.IsBlock == true)
            return Unauthorized(result ?? new ApiResponse<UsersDto>
            {
                Result = new UsersDto(),
                Message = new MessageResult { message = "Hata", message2 = "Session dogrulanamadi." }
            });

        SetSession(result.Result, result.Result.DomainName);
        await LogLoginSuccessAsync("MobileSession", result.Result, cancellationToken).ConfigureAwait(false);

        return Redirect("/");
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
                ActionName = "MobileSessionEnd"
            }, cancellationToken).ConfigureAwait(false);
        }

        HttpContext.Session.Clear();
        return RedirectToAction(nameof(Login));
    }

    private static UsersDto BuildMockUser(string? userNameOrEmail) => new()
    {
        UserId = 999,
        NameSurname = "Mock Kullanici",
        Email = userNameOrEmail,
        DomainName = userNameOrEmail,
        Department = "Mock Departman",
        BranchCode = 1001,
        BranchName = "Mock Sube",
        RegionCode = 1,
        Authority = "Admin",
        Password = "mock-token",
        IsBlock = false,
        UpdateSeen = true,
        CreatedDate = DateTime.Now,
        ProfilePhoto = "https://cdn.pixabay.com/photo/2023/02/18/11/00/icon-7797704_640.png"
    };

    private void SetSession(UsersDto user, string? username = null)
    {
        var sessionUsername = !string.IsNullOrWhiteSpace(username)
            ? username
            : user.DomainName ?? user.Email ?? string.Empty;

        HttpContext.Session.SetInt32("UserId", user.UserId);
        HttpContext.Session.SetString("Username", sessionUsername);
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
