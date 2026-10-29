using System.Text.RegularExpressions;

namespace DashboardTsy.Web.Security;

// Allow-list rule for the username / e-mail typed into the login form.
// Only letters, digits and . _ - + @ are accepted; markup, quotes, whitespace
// and control characters are rejected before the value reaches any sink
// (upstream URL, session, JSON response). Fortify: Cross-Site Scripting: Reflected.
public static partial class LoginIdentifier
{
    public const int MaxLength = 254;

    public static bool IsWellFormed(string? value) =>
        value is not null && value.Length <= MaxLength && AllowedPattern().IsMatch(value);

    [GeneratedRegex(@"^[\p{L}\p{Nd}._+@-]+$", RegexOptions.CultureInvariant)]
    private static partial Regex AllowedPattern();
}
