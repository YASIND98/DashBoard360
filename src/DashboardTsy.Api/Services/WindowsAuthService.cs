using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
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

            var existing = await _db.Users.FirstOrDefaultAsync(x => x.DomainName == username, cancellationToken).ConfigureAwait(false);

            if (existing != null)
            {
                existing.NameSurname = $"{windowsUser.FIRSTNAME} {windowsUser.SURNAME}".Trim();
                existing.RegionCode = int.Parse(windowsUser.REGIONCODE);
                existing.BranchCode = int.Parse(windowsUser.BRANCHCODE);
                existing.Email = windowsUser.EMAIL;
                existing.ProfilePhoto = windowsUser.Resim;
                existing.Department = MapDepartment(windowsUser.GROUPNAME);
                await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                existing.Password = GenerateJwt(existing);
                response.Result = MapToDto(existing);
            }
            else
            {
                var newUser = new User
                {
                    DomainName = username,
                    NameSurname = $"{windowsUser.FIRSTNAME} {windowsUser.SURNAME}".Trim(),
                    RegionCode = int.Parse(windowsUser.REGIONCODE),
                    BranchCode = int.Parse(windowsUser.BRANCHCODE),
                    Email = windowsUser.EMAIL,
                    ProfilePhoto = windowsUser.Resim,
                    Department = MapDepartment(windowsUser.GROUPNAME),
                    CreatedDate = DateTime.UtcNow
                };
                _db.Users.Add(newUser);
                await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                newUser.Password = GenerateJwt(newUser);
                response.Result = MapToDto(newUser);
            }

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
            var user = await _db.Users.FirstOrDefaultAsync(x => x.DomainName == domainName, cancellationToken).ConfigureAwait(false);
            if (user == null)
            {
                response.Message!.message = "Hata";
                response.Message.message2 = "Kullanıcı kaydı bulunamadı.";
                return response;
            }

            response.Result = MapToDto(user);
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

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            response.Message!.message = "Hata";
            response.Message.message2 = "Hatalı kullanıcı adı veya şifre.";
            return response;
        }

        try
        {
            var dbUser = await _db.Users.FirstOrDefaultAsync(x => x.Email == username, cancellationToken).ConfigureAwait(false);
            if (dbUser == null)
            {
                response.Message!.message = "Hata";
                response.Message.message2 = "Hatalı kullanıcı adı veya şifre.";
                return response;
            }

            if (dbUser.IsBlock == true)
            {
                response.Message!.message = "Hata";
                response.Message.message2 = "Platforma giriş yetkiniz bulunmamaktadır. Lütfen yöneticinizle iletişime geçiniz.";
                return response;
            }

            var verified = PasswordHasher.CheckPassword(dbUser.Password ?? string.Empty, password);
            if (!verified)
            {
                response.Message!.message = "Hata";
                response.Message.message2 = "Hatalı kullanıcı adı veya şifre.";
                return response;
            }

            // Return a JWT for downstream calls but DO NOT overwrite stored password hash.
            var dto = MapToDto(dbUser);
            dto.Password = GenerateJwt(dbUser);
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
