using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace EduOnline.Core.ControleDeAcesso;

public class AspNetUser(IHttpContextAccessor accessor) : IAspNetUser
{
    private readonly IHttpContextAccessor _accessor = accessor;

    public string Name => _accessor.HttpContext?.User?.Identity?.Name ?? string.Empty;

    public Guid GetUserId()
    {
        if (!IsAuthenticated()) return Guid.Empty;

        var userId = _accessor.HttpContext?.User?.GetUserId();
        return Guid.TryParse(userId, out var guid) ? guid : Guid.Empty;
    }

    public string GetUserEmail()
    {
        return IsAuthenticated() ? _accessor.HttpContext?.User?.GetUserEmail() ?? string.Empty : string.Empty;
    }

    public bool IsAuthenticated()
    {
        return _accessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
    }

    public bool IsInRole(string role)
    {
        return _accessor.HttpContext?.User?.IsInRole(role) ?? false;
    }

    public IEnumerable<Claim> GetClaimsIdentity()
    {
        return _accessor.HttpContext?.User?.Claims ?? [];
    }

    public HttpContext ObterHttpContext()
    {
        return _accessor.HttpContext ?? throw new InvalidOperationException("HttpContext não disponível.");
    }
}
