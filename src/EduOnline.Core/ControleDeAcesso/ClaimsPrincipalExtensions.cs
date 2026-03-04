using System.Security.Claims;

namespace EduOnline.Core.ControleDeAcesso;

public static class ClaimsPrincipalExtensions
{
    public static string GetUserId(this ClaimsPrincipal principal)
    {
        if (principal == null)
        {
            throw new ArgumentException(null, nameof(principal));
        }

        var claim = principal.FindFirst(ClaimTypes.NameIdentifier);
        return claim?.Value ?? string.Empty;
    }

    public static string GetUserEmail(this ClaimsPrincipal principal)
    {
        if (principal == null)
        {
            throw new ArgumentException(null, nameof(principal));
        }

        var claim = principal.FindFirst(ClaimTypes.Email);
        return claim?.Value ?? string.Empty;
    }
}
