using EduOnline.Core.ControleDeAcesso;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace EduOnline.WebApps.UnitTest.Identidade;

public class AspNetUserCoverageTest
{
    [Fact]
    public void Name_SemHttpContext_DeveRetornarVazio()
    {
        var sut = new AspNetUser(new HttpContextAccessor());

        Assert.Equal(string.Empty, sut.Name);
    }

    [Fact]
    public void GetUserId_UsuarioNaoAutenticado_DeveRetornarGuidVazio()
    {
        var sut = new AspNetUser(new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity())
            }
        });

        Assert.Equal(Guid.Empty, sut.GetUserId());
    }

    [Fact]
    public void GetUserId_ClaimInvalida_DeveRetornarGuidVazio()
    {
        var sut = new AspNetUser(new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext
            {
                User = CriarPrincipal(autenticado: true, claimId: "invalido")
            }
        });

        Assert.Equal(Guid.Empty, sut.GetUserId());
    }

    [Fact]
    public void GetUserId_ClaimValida_DeveRetornarGuid()
    {
        var id = Guid.NewGuid();
        var sut = new AspNetUser(new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext
            {
                User = CriarPrincipal(autenticado: true, claimId: id.ToString())
            }
        });

        Assert.Equal(id, sut.GetUserId());
    }

    [Fact]
    public void GetUserEmail_EClaims_RoleEHttpContext_DevemRefletirUsuarioAtual()
    {
        var httpContext = new DefaultHttpContext
        {
            User = CriarPrincipal(
                autenticado: true,
                claimId: Guid.NewGuid().ToString(),
                email: "aluno@eduonline.com",
                role: "Administrador",
                name: "Aluno Teste")
        };
        var sut = new AspNetUser(new HttpContextAccessor { HttpContext = httpContext });

        Assert.Equal("Aluno Teste", sut.Name);
        Assert.Equal("aluno@eduonline.com", sut.GetUserEmail());
        Assert.True(sut.IsAuthenticated());
        Assert.True(sut.IsInRole("Administrador"));
        Assert.NotEmpty(sut.GetClaimsIdentity());
        Assert.Same(httpContext, sut.ObterHttpContext());
    }

    [Fact]
    public void ObterHttpContext_SemContexto_DeveLancarExcecao()
    {
        var sut = new AspNetUser(new HttpContextAccessor());

        var ex = Assert.Throws<InvalidOperationException>(() => sut.ObterHttpContext());

        Assert.Equal("HttpContext não disponível.", ex.Message);
    }

    private static ClaimsPrincipal CriarPrincipal(bool autenticado, string claimId, string? email = null, string? role = null, string? name = null)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, claimId)
        };

        if (!string.IsNullOrWhiteSpace(email))
            claims.Add(new Claim(ClaimTypes.Email, email));

        if (!string.IsNullOrWhiteSpace(role))
            claims.Add(new Claim(ClaimTypes.Role, role));

        var identity = new ClaimsIdentity(claims, autenticado ? "Teste" : null, ClaimTypes.Name, ClaimTypes.Role);

        if (!string.IsNullOrWhiteSpace(name))
            identity.AddClaim(new Claim(ClaimTypes.Name, name));

        return new ClaimsPrincipal(identity);
    }
}
