using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using DashboardTsy.Api.Data;
using DashboardTsy.Api.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace DashboardTsy.Api.Services;

public class WindowsAuthService : IWindowsAuthService
{
    private readonly DashboardTsyDbContext _db;
    private readonly ReferansDbOptions _referansOptions;
    private const string JwtSecret = "YourKey-2374-OFFKDI940NG7:56753253-tyuw-5769-0921-kfirox29zoxv";

    public WindowsAuthService(DashboardTsyDbContext db, ReferansDbOptions referansOptions)
    {
        _db = db;
        _referansOptions = referansOptions;
    }

    public async Task<ApiResponse<UsersDto>> WindowsLoginAsync(string username, CancellationToken cancellationToken = default)
    {
        var response = NewResponse();

        if (string.IsNullOrWhiteSpace(username))
        {
            response.Message!.message = "Hata";
            response.Message.message2 = "Kullanıcı adı boş olamaz.";
            return response;
        }

        try
        {
            var windowsUser = await GetWindowsUserAsync(username, cancellationToken).ConfigureAwait(false);
            if (windowsUser == null || string.IsNullOrEmpty(windowsUser.EMAIL) || string.IsNullOrEmpty(windowsUser.FIRSTNAME))
            {
                response.Message!.message = "Hata";
                response.Message.message2 = "Kullanıcı kaydı bulunamadı.";
                return response;
            }

            if (string.IsNullOrEmpty(windowsUser.REGIONCODE) || string.IsNullOrEmpty(windowsUser.BRANCHCODE))
            {
                response.Message!.message = "Hata";
                response.Message.message2 = "Kullanıcı kaydı bulunamadı.";
                return response;
            }

            // SSO_USERVIEW is the source of truth. Do not touch _db.Users for SSO flows.
            var dto = MapToDtoFromWindowsUser(username, windowsUser);
            // Generate a JWT for downstream calls; UserId is a stable per-user surrogate (not persisted).
            dto.Password = GenerateJwt(new User { UserId = dto.UserId, Email = dto.Email });
            response.Result = dto;

            response.Message!.message = "Başarılı";
            response.Message.message2 = "Giriş başarılı.";
        }
        catch (Exception)
        {
            response.Message!.message = "Hata";
            response.Message.message2 = "İşlemler sırasında bir hata oluştu.";
        }

        return response;
    }

    public async Task<ApiResponse<UsersDto>> DomainLoginAsync(string domainName, CancellationToken cancellationToken = default)
    {
        var response = NewResponse();

        if (string.IsNullOrWhiteSpace(domainName))
        {
            response.Message!.message = "Hata";
            response.Message.message2 = "Domain adı boş olamaz.";
            return response;
        }

        try
        {
            var windowsUser = await GetWindowsUserAsync(domainName, cancellationToken).ConfigureAwait(false);
            if (windowsUser == null || string.IsNullOrEmpty(windowsUser.EMAIL) || string.IsNullOrEmpty(windowsUser.FIRSTNAME))
            {
                response.Message!.message = "Hata";
                response.Message.message2 = "Kullanıcı kaydı bulunamadı.";
                return response;
            }

            var dto = MapToDtoFromWindowsUser(domainName, windowsUser);
            dto.Password = GenerateJwt(new User { UserId = dto.UserId, Email = dto.Email });
            response.Result = dto;
            response.Message!.message = "Başarılı";
            response.Message.message2 = "Giriş başarılı.";
        }
        catch (Exception)
        {
            response.Message!.message = "Hata";
            response.Message.message2 = "İşlemler sırasında bir hata oluştu.";
        }

        return response;
    }

    public async Task<ApiResponse<UsersDto>> LoginAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        var response = NewResponse();

        // This application uses SSO_USERVIEW as the source of truth.
        // Username/password verification cannot be performed without _db.Users (stored password hash),
        // so this flow is intentionally disabled to avoid hitting the Users table.
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            response.Message!.message = "Hata";
            response.Message.message2 = "Kullanıcı adı ve şifre zorunludur.";
            return response;
        }

        response.Message!.message = "Hata";
        response.Message.message2 = "Kullanıcı adı/şifre ile giriş devre dışı. Lütfen domain/SSO ile giriş yapınız.";

        return response;
    }

    private async Task<WindowsUserInfo?> GetWindowsUserAsync(string fullLoginName, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_referansOptions.ConnectionString))
            return null;

        const string sql = "SELECT FIRSTNAME, SURNAME, EMAIL, BRANCHCODE, REGIONCODE, GROUPNAME, Resim FROM SSO_USERVIEW WHERE FULLLOGINNAME = @fullName";
        await using var conn = new SqlConnection(_referansOptions.ConnectionString);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@fullName", fullLoginName);
        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            return null;

        return new WindowsUserInfo
        {
            FIRSTNAME = reader.IsDBNull(0) ? null : reader.GetString(0),
            SURNAME = reader.IsDBNull(1) ? null : reader.GetString(1),
            EMAIL = reader.IsDBNull(2) ? null : reader.GetString(2),
            BRANCHCODE = reader.IsDBNull(3) ? null : reader.GetValue(3)?.ToString(),
            REGIONCODE = reader.IsDBNull(4) ? null : reader.GetValue(4)?.ToString(),
            GROUPNAME = reader.IsDBNull(5) ? null : reader.GetString(5),
            Resim = reader.IsDBNull(6) ? null : reader.GetString(6)
        };
    }

    private static string MapDepartment(string? groupName)
    {
        if (string.IsNullOrEmpty(groupName)) return "SUBE";
        if (groupName.Contains("Genel Müdürlük")) return "GM";
        if (groupName.Contains("Bölge")) return "BOLGE";
        return "SUBE";
    }

    private static int StableUserId(string domainNameOrLogin)
    {
        // Produce a stable positive 32-bit int from the user's login.
        // This avoids relying on _db.Users for an identity value.
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(domainNameOrLogin.Trim().ToUpperInvariant()));
        var value = BitConverter.ToInt32(bytes, 0) & 0x7fffffff;
        return value == 0 ? 1 : value;
    }

    private static UsersDto MapToDtoFromWindowsUser(string domainName, WindowsUserInfo windowsUser)
    {
        return new UsersDto
        {
            UserId = StableUserId(domainName),
            NameSurname = $"{windowsUser.FIRSTNAME} {windowsUser.SURNAME}".Trim(),
            Email = windowsUser.EMAIL,
            DomainName = domainName,
            Department = MapDepartment(windowsUser.GROUPNAME),
            BranchCode = int.TryParse(windowsUser.BRANCHCODE, out var bc) ? bc : null,
            RegionCode = int.TryParse(windowsUser.REGIONCODE, out var rc) ? rc : null,
            ProfilePhoto = windowsUser.Resim,
            CreatedDate = DateTime.UtcNow
        };
    }

    private static string GenerateJwt(User user)
    {
        var key = Encoding.ASCII.GetBytes(JwtSecret);
        var claims = new List<Claim>
        {
            new("Authorization", "Authorized"),
            new("Email", user.Email ?? string.Empty),
            new("UserId", user.UserId.ToString())
        };
        var jwt = new JwtSecurityToken(
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddDays(1),
            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        );
        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }

    private static ApiResponse<UsersDto> NewResponse()
    {
        return new ApiResponse<UsersDto>
        {
            Result = new UsersDto(),
            Message = new MessageResult()
        };
    }

    private static UsersDto MapToDto(User u)
    {
        return new UsersDto
        {
            UserId = u.UserId,
            NameSurname = u.NameSurname,
            Email = u.Email,
            Password = u.Password,
            IsAdmin = u.IsAdmin,
            CreatedDate = u.CreatedDate,
            Type = u.Type,
            PassChange = u.PassChange,
            ProfilePhoto = u.ProfilePhoto,
            Department = u.Department,
            BranchCode = u.BranchCode,
            BranchName = u.BranchName,
            Authority = u.Authority,
            RegionCode = u.RegionCode,
            DomainName = u.DomainName,
            IsBlock = u.IsBlock,
            UpdateSeen = u.UpdateSeen
        };
    }
}
