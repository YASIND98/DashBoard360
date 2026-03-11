using System.Net;
using System.Text.Json;
using DashboardTsy.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DashboardTsy.Web.Controllers;

[Authorize]
public class AuthController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public AuthController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    [HttpGet]
    public async Task<IActionResult> Login(CancellationToken cancellationToken)
    {
        var userName = User?.Identity?.Name;
        if (string.IsNullOrEmpty(userName))
            return View();

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
                result = JsonSerializer.Deserialize<ApiResponse<UsersDto>>(json);
            }
        }
        catch
        {
            // ignore
        }

        if (result?.Result != null && result.Result.UserId > 0 && result.Result.IsBlock != true)
        {
            SetSession(result.Result);

            return RedirectToAction("Index", "Home");
        }

        return View();
    }

    [HttpGet]
    public async Task<JsonResult> LoginDomain(CancellationToken cancellationToken)
    {
        var userName = User?.Identity?.Name ?? string.Empty;

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
                result = JsonSerializer.Deserialize<ApiResponse<UsersDto>>(json);
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
        }

        return Json(result);
    }

    [HttpPost]
    public async Task<JsonResult> LoginUser([FromForm] LoginModel model, CancellationToken cancellationToken)
    {
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
                result = JsonSerializer.Deserialize<ApiResponse<UsersDto>>(json);
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
        }

        return Json(result);
    }

    [HttpGet]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Index", "Home");
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
}
