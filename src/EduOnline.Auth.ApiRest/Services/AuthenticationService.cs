using EduOnline.Auth.ApiRest.Data;
using EduOnline.Auth.ApiRest.Extensions;
using EduOnline.Auth.ApiRest.Models;
using EduOnline.Core.ControleDeAcesso;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EduOnline.Auth.ApiRest.Services;

public class AuthenticationService
{
    public readonly SignInManager<EduOnlineUser> SignInManager;
    public readonly UserManager<EduOnlineUser> UserManager;
    private readonly AppTokenSettings _appTokenSettingsSettings;
    private readonly ApplicationDbContext _context;

    private readonly IAspNetUser _aspNetUser;

    public AuthenticationService(
        SignInManager<EduOnlineUser> signInManager,
        UserManager<EduOnlineUser> userManager,
        IOptions<AppTokenSettings> appTokenSettingsSettings,
        ApplicationDbContext context,
        IAspNetUser aspNetUser)
    {
        SignInManager = signInManager;
        UserManager = userManager;
        _appTokenSettingsSettings = appTokenSettingsSettings.Value;
        _aspNetUser = aspNetUser;
        _context = context;
    }

    public async Task<EduOnlineUser?> ObterUserId(Guid id)
    {
        return await UserManager.FindByIdAsync(id.ToString());
    }

    public async Task<IdentityResult> RemoverUser(EduOnlineUser eduOnlineUser)
    {
        if (eduOnlineUser.StatusId != Status.Pendente.Id)
        {
            var erros = new[] { new IdentityError { Code = "UserNotPending", Description = "Somente usuários com status Pendente podem ser removidos." } };

            return IdentityResult.Failed(erros);
        }

        return await UserManager.DeleteAsync(eduOnlineUser);
    }

    public async Task<UsuarioRepostaModel> GerarJwt(string email)
    {
        var user = await UserManager.FindByEmailAsync(email);
        
        if (user == null)
        {
            throw new InvalidOperationException($"Usuário com email '{email}' não encontrado.");
        }
        
        var claims = await UserManager.GetClaimsAsync(user);

        var identityClaims = await ObterClaimsUsuario(claims, user);
        var encodedToken = CodificarToken(identityClaims);

        var refreshToken = await GerarRefreshToken(email);

        return ObterRespostaToken(encodedToken, user, claims, refreshToken);
    }

    private async Task<ClaimsIdentity> ObterClaimsUsuario(ICollection<Claim> claims, EduOnlineUser user)
    {
        var userRoles = await UserManager.GetRolesAsync(user);

        claims.Add(new Claim(JwtRegisteredClaimNames.Sub, user.Id));
        claims.Add(new Claim(JwtRegisteredClaimNames.Email, user.Email!));
        claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id));
        claims.Add(new Claim(ClaimTypes.Email, user.Email!));
        claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
        claims.Add(new Claim(JwtRegisteredClaimNames.Nbf, ToUnixEpochDate(DateTime.UtcNow).ToString(),
            ClaimValueTypes.Integer64));
        claims.Add(new Claim(JwtRegisteredClaimNames.Iat, ToUnixEpochDate(DateTime.UtcNow).ToString(),
            ClaimValueTypes.Integer64));
        foreach (var userRole in userRoles)
        {
            claims.Add(new Claim("role", userRole));
        }

        var identityClaims = new ClaimsIdentity();
        identityClaims.AddClaims(claims);

        return identityClaims;
    }

    private string CodificarToken(ClaimsIdentity identityClaims)
    {
        if (string.IsNullOrWhiteSpace(_appTokenSettingsSettings.Segredo))
            throw new InvalidOperationException("AppTokenSettings:Segredo não configurado.");

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_appTokenSettingsSettings.Segredo));
        var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = tokenHandler.CreateToken(new SecurityTokenDescriptor
        {
            Issuer = _appTokenSettingsSettings.Issuer,
            Audience = _appTokenSettingsSettings.Audience,
            Subject = identityClaims,
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = signingCredentials
        });

        return tokenHandler.WriteToken(token);
    }

    private static UsuarioRepostaModel ObterRespostaToken(string encodedToken, IdentityUser user,
        IEnumerable<Claim> claims, RefreshToken refreshToken)
    {
        return new UsuarioRepostaModel
        {
            AccessToken = encodedToken,
            RefreshToken = refreshToken.Token,
            ExpiraEm = TimeSpan.FromHours(1).TotalSeconds,
            UsuarioToken = new UsuarioTokenModel
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                Claims = claims.Select(c => new ClaimModel { Type = c.Type, Value = c.Value })
            }
        };
    }

    private static long ToUnixEpochDate(DateTime date)
        => (long)Math.Round((date.ToUniversalTime() - new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero))
            .TotalSeconds);

    private async Task<RefreshToken> GerarRefreshToken(string email)
    {
        var refreshToken = new RefreshToken
        {
            Username = email,
            ExpirationDate = DateTime.UtcNow.AddHours(_appTokenSettingsSettings.RefreshTokenExpiration)
        };

        _context.RefreshTokens.RemoveRange(_context.RefreshTokens.Where(u => u.Username == email));
        await _context.RefreshTokens.AddAsync(refreshToken);

        await _context.SaveChangesAsync();

        return refreshToken;
    }

    public async Task<RefreshToken?> ObterRefreshToken(Guid refreshToken)
    {
        RefreshToken? token = await _context.RefreshTokens.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Token == refreshToken);

        return token != null && token.ExpirationDate.ToLocalTime() > DateTime.Now
            ? token
            : null;
    }
}
