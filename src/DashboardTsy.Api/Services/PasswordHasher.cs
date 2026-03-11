using System.Security.Cryptography;

namespace DashboardTsy.Api.Services;

public static class PasswordHasher
{
    // DBRapor-compatible PBKDF2 format: base64(salt[16] + hash[20]), 10k iterations.
    public static bool CheckPassword(string hashstr, string input)
    {
        try
        {
            var hashBytes = Convert.FromBase64String(hashstr);
            if (hashBytes.Length < 36)
                return false;

            var salt = new byte[16];
            Array.Copy(hashBytes, 0, salt, 0, 16);

            var pbkdf2 = new Rfc2898DeriveBytes(input, salt, 10000, HashAlgorithmName.SHA1);
            var hash = pbkdf2.GetBytes(20);

            for (var i = 0; i < 20; i++)
            {
                if (hashBytes[i + 16] != hash[i])
                    return false;
            }

            return true;
        }
        catch
        {
            return false;
        }
    }
}

