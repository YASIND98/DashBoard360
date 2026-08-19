using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using DashboardTsy.Api.Models;
using Microsoft.IdentityModel.Tokens;

namespace DashboardTsy.Api.Services;

/// <summary>
/// AuthMock:Enabled=true iken MobileAuthController'ın Token/Login2/SendSmsCode akışını
/// KutupYıldızı'na gitmeden burada tamamlar. iOS ekibi Kutup'a bağımlı kalmadan geliştirme yapar.
///
/// Kural özeti:
///   Token       → parametreler doluysa anonim JWT üret; boşsa 400.
///   Login2      → parameters[0] doluysa OTP challenge (sabit smsGuid + key) döner; boşsa hata payload'ı.
///   SendSmsCode → password="111111" ise kullanıcıya özel JWT + kimlik döner; başka her şey "Sms kodu doğru değil."
///
/// Response şeması Kutup'un pass-through modundaki gerçek şemasıyla birebir aynıdır — iOS mock ile prod arasında
/// kod farkı görmez, sadece appsettings flag'i değişir.
/// </summary>
public sealed class MockMobileAuthScenario
{
    private const string MockSmsGuid = "mock-sms-guid-11111111-1111-1111-1111-111111111111";
    private const string MockKey = "mock-verify-key";
    private const string MockCustomerNo = "1000000001";
    private const string MockCustomerIdentity = "9000000001";
    private const string MockChannelSessionId = "mock-channel-session-1234567890abcdef";
    private const string ValidTestOtp = "111111";

    private readonly byte[] _jwtKey;
    private readonly int _tokenExpiresMinutes;

    public MockMobileAuthScenario(IConfiguration configuration)
    {
        var base64 = configuration["MobileJwt:SymmetricKey"]
            ?? throw new InvalidOperationException("AuthMock için MobileJwt:SymmetricKey config'i zorunludur — mock JWT üretiminde kullanılır.");
        _jwtKey = Convert.FromBase64String(base64);

        // Kutup convention: dev'de 480dk, prod'da 30dk. Mock ekseriyet dev'de çalışır, uzun süre veriyoruz.
        _tokenExpiresMinutes = configuration.GetValue<int?>("MobileJwt:ExpiresMinutesDevelopment") ?? 480;
    }

    /// <summary>
    /// /api/Token — anonim JWT.
    /// Kutup ClientId/ClientSecret doğrulaması yapıyor; mock'ta sadece deviceId dolu mu kontrol ediyoruz.
    /// </summary>
    public MockResult HandleToken(JsonElement? body)
    {
        var parameter = TryGetFirstParameter(body);
        if (parameter is null)
            return MockResult.BadRequest();

        var deviceId = TryGetString(parameter.Value, "deviceId") ?? TryGetString(parameter.Value, "DeviceId");
        if (string.IsNullOrEmpty(deviceId))
            return MockResult.BadRequest();

        var token = IssueJwt(new[]
        {
            new Claim("DeviceId", deviceId),
            new Claim(ClaimTypes.Role, "Anonymous"),
            new Claim("CustomerNo", "-1"),
            new Claim("ChannelSessionId", MockChannelSessionId),
            new Claim("DeviceToken", "0"),
            new Claim("UserId", string.Empty)
        });

        return MockResult.Ok(new { Token = token });
    }

    /// <summary>
    /// /api/Login2 — OTP challenge dönüşü.
    /// Kredensiyel doğrulaması YAPMIYOR (mock). Parametreler dolu ise başarılı sayar.
    /// </summary>
    public MockResult HandleLogin2(JsonElement? body)
    {
        var parameter = TryGetFirstParameter(body);
        if (parameter is null)
        {
            // Kutup davranışı: hatalarda da 200 OK + IsError=true döner.
            return MockResult.Ok(new
            {
                IsError = true,
                ErrorMessage = "Parameters gerekli."
            });
        }

        // Kutup response şeması ile birebir aynı. EncryptData normalde RSA-şifreli CustomerNo — mock'ta düz string.
        // iOS bunu opaque olarak SendSmsCode'a geri gönderiyor; içeriğe bakmadığı için sorun olmaz.
        return MockResult.Ok(new
        {
            IsError = false,
            ErrorMessage = (string?)null,
            SmsLength = 6,
            IsAlphaNumericCode = false,
            EncryptData = MockCustomerNo,
            SmsGuid = MockSmsGuid,
            Key = MockKey
        });
    }

    /// <summary>
    /// /api/SendSmsCode — OTP doğrulama + user JWT.
    /// Zorunlu alanlar boşsa 400; kod 111111 değilse "Sms kodu doğru değil"; doğruysa access token döner.
    /// </summary>
    public MockResult HandleSendSmsCode(JsonElement? body)
    {
        if (body is null || body.Value.ValueKind != JsonValueKind.Object)
            return MockResult.BadRequest("Body gerekli.");

        var root = body.Value;
        var smsGuid = TryGetString(root, "smsGuid") ?? TryGetString(root, "SmsGuid");
        var password = TryGetString(root, "password") ?? TryGetString(root, "Password");
        var key = TryGetString(root, "key") ?? TryGetString(root, "Key");
        var customerNo = TryGetString(root, "customerNo") ?? TryGetString(root, "CustomerNo");
        var userName = TryGetString(root, "userName") ?? TryGetString(root, "UserName");
        var deviceId = TryGetString(root, "deviceId") ?? TryGetString(root, "DeviceId");
        var deviceToken = TryGetString(root, "deviceToken") ?? TryGetString(root, "DeviceToken");

        if (string.IsNullOrEmpty(smsGuid) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(key) ||
            string.IsNullOrEmpty(customerNo) || string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(deviceId))
        {
            return MockResult.BadRequest("DeviceId, UserName, SmsGuid, Password, Key ve CustomerNo zorunludur.");
        }

        // Kutup dev-mode davranışı: sabit test kodu (LoginController.IsSMSCodeCorrect).
        if (password != ValidTestOtp)
        {
            // Kutup 200 OK + IsError=1 döndürüyor bu senaryoda; mock aynen taklit ediyor.
            return MockResult.Ok(new
            {
                IsError = 1,
                ErrorMessage = "Sms kodu doğru değil.",
                CustomerNo = customerNo
            });
        }

        var accessToken = IssueJwt(new[]
        {
            new Claim("DeviceId", deviceId),
            new Claim(ClaimTypes.Role, "User"),
            new Claim(ClaimTypes.Name, userName),
            new Claim("CustomerNo", MockCustomerNo),
            new Claim("ChannelSessionId", MockChannelSessionId),
            new Claim("DeviceToken", deviceToken ?? string.Empty),
            new Claim("UserId", MockCustomerIdentity)
        });

        return MockResult.Ok(new
        {
            State = "Success",
            CustomerNo = customerNo,
            CustomerIdentity = MockCustomerIdentity,
            AccessToken = accessToken
        });
    }

    /// <summary>
    /// /Public/GetCurrentUser mock cevabı. AuthMock:Enabled=true iken controller DB'ye hiç gitmez;
    /// bu UsersDto'yu ApiResponse zarfına sarıp döner. Değerler Web/MobileAuthController.BuildMockUser
    /// ile aynı — mobil ve web mock akışları aynı kimliği görür.
    /// </summary>
    public ApiResponse<UsersDto> BuildCurrentUserResponse() =>
        new()
        {
            Result = new UsersDto
            {
                UserId = 999,
                NameSurname = "Mock Kullanici",
                Email = "mock-session-user",
                DomainName = "mock-session-user",
                Department = "Mock Departman",
                BranchCode = 1001,
                BranchName = "Mock Sube",
                RegionCode = 1,
                Authority = "Admin",
                Password = "mock-token",
                IsBlock = false,
                UpdateSeen = true,
                CreatedDate = DateTime.UtcNow,
                ProfilePhoto = "https://cdn.pixabay.com/photo/2023/02/18/11/00/icon-7797704_640.png"
            },
            Message = new MessageResult
            {
                message = "Başarılı",
                message2 = "Giriş başarılı."
            }
        };

    /// <summary>
    /// JWT üretici — Kutup'un JwtManager'ıyla aynı algoritma (HS256) ve aynı symmetric key kullanır.
    /// Böylece DashboardTsy'nin JWT Bearer middleware'i bu token'ı geçerli sayar; iOS mock modda diğer
    /// korunmuş /mobile/* endpoint'lerine de erişebilir.
    /// </summary>
    private string IssueJwt(Claim[] claims)
    {
        var handler = new JwtSecurityTokenHandler();
        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_tokenExpiresMinutes),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(_jwtKey), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = handler.CreateToken(descriptor);
        return handler.WriteToken(token);
    }

    private static JsonElement? TryGetFirstParameter(JsonElement? body)
    {
        if (body is null || body.Value.ValueKind != JsonValueKind.Object)
            return null;
        if (!body.Value.TryGetProperty("parameters", out var parameters) &&
            !body.Value.TryGetProperty("Parameters", out parameters))
            return null;
        if (parameters.ValueKind != JsonValueKind.Array || parameters.GetArrayLength() == 0)
            return null;
        return parameters[0];
    }

    private static string? TryGetString(JsonElement element, string propertyName)
    {
        if (element.ValueKind != JsonValueKind.Object) return null;
        if (!element.TryGetProperty(propertyName, out var prop)) return null;
        return prop.ValueKind == JsonValueKind.String ? prop.GetString() : null;
    }
}

/// <summary>
/// Mock senaryonun controller'a döndürdüğü nötr sonuç. Controller bunu ActionResult'a çevirir;
/// böylece bu sınıf ASP.NET Core primitive'lerinden bağımsız kalır ve test etmek de kolaydır.
/// </summary>
public sealed record MockResult(int StatusCode, object? Payload)
{
    public static MockResult Ok(object payload) => new(200, payload);
    public static MockResult BadRequest(string? message = null) =>
        new(400, message is null ? null : new { IsError = true, ErrorMessage = message });
}
