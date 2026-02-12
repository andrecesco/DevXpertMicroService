using EduOnline.Alunos.Application.Commands;
using EduOnline.Core.Communication.Mediator;
using EduOnline.Core.ControleDeAcesso;
using EduOnline.Core.Mensagens;
using EduOnline.WebApps.ApiRest.Extensions;
using EduOnline.WebApps.ApiRest.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EduOnline.WebApps.ApiRest.Controllers;

[Route("api/auth")]
public class AuthController(IMediatorHandler mediatorHandler, INotificador notificador,
                      SignInManager<IdentityUser> signInManager,
                      UserManager<IdentityUser> userManager,
                      IOptions<JwtSettings> jwtSettings,
                      IUser user, ILogger<AuthController> logger) : MainController(notificador, user)
{
    private readonly SignInManager<IdentityUser> _signInManager = signInManager;
    private readonly UserManager<IdentityUser> _userManager = userManager;
    private readonly JwtSettings _jwtSettings = jwtSettings.Value;
    private readonly ILogger _logger = logger;

    [HttpPost("nova/conta")]
    public async Task<ActionResult> Registrar(UsuarioRegistroModel usarioRegistro)
    {
        if (!ModelState.IsValid) return CustomResponse(ModelState);

        var user = new IdentityUser
        {
            UserName = usarioRegistro.Email,
            Email = usarioRegistro.Email,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, usarioRegistro.Senha);
        if (result.Succeeded)
        {
            var command = new AdicionarAlunoCommand(Guid.Parse(user.Id), usarioRegistro.Nome, usarioRegistro.Email);

            var resultado = await mediatorHandler.EnviarComando(command);

            if (!resultado)
            {
                await _userManager.DeleteAsync(user);
                return CustomResponse();
            }

            await _signInManager.SignInAsync(user, false);
            return CustomResponse(await GerarJwt(user.Email));
        }

        foreach (var error in result.Errors)
        {
            NotificarErro(error.Description);
        }

        return CustomResponse();
    }

    [HttpPost("entrar")]
    public async Task<ActionResult> Login(UsuarioLoginModel loginUser)
    {
        if (!ModelState.IsValid) return CustomResponse(ModelState);

        var result = await _signInManager.PasswordSignInAsync(loginUser.Email, loginUser.Senha, false, true);

        if (result.Succeeded)
        {
            _logger.LogInformation("Usuario " + loginUser.Email + " logado com sucesso");
            return CustomResponse(await GerarJwt(loginUser.Email));
        }
        if (result.IsLockedOut)
        {
            NotificarErro("Usuário temporariamente bloqueado por tentativas inválidas");
            return CustomResponse(loginUser);
        }

        NotificarErro("Usuário ou Senha incorretos");
        return CustomResponse(loginUser);
    }

    private async Task<UsuarioRepostaModel> GerarJwt(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        var claims = await _userManager.GetClaimsAsync(user);
        var userRoles = await _userManager.GetRolesAsync(user);

        claims.Add(new Claim(JwtRegisteredClaimNames.Sub, user.Id));
        claims.Add(new Claim(JwtRegisteredClaimNames.Email, user.Email));
        claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
        claims.Add(new Claim(JwtRegisteredClaimNames.Nbf, ToUnixEpochDate(DateTime.UtcNow).ToString()));
        claims.Add(new Claim(JwtRegisteredClaimNames.Iat, ToUnixEpochDate(DateTime.UtcNow).ToString(), ClaimValueTypes.Integer64));
        foreach (var userRole in userRoles)
        {
            claims.Add(new Claim("role", userRole));
        }

        var identityClaims = new ClaimsIdentity();
        identityClaims.AddClaims(claims);

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtSettings.Segredo);
        var token = tokenHandler.CreateToken(new SecurityTokenDescriptor
        {
            Issuer = _jwtSettings.Emissor,
            Audience = _jwtSettings.Audiencia,
            Subject = identityClaims,
            Expires = DateTime.UtcNow.AddHours(_jwtSettings.ExpiracaoHoras),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        });

        var encodedToken = tokenHandler.WriteToken(token);

        var response = new UsuarioRepostaModel
        {
            AccessToken = encodedToken,
            ExpiraEm = TimeSpan.FromHours(_jwtSettings.ExpiracaoHoras).TotalSeconds,
            UsuarioToken = new UsuarioTokenModel
            {
                Id = user.Id,
                Email = user.Email,
                Claims = claims.Select(c => new ClaimModel { Type = c.Type, Value = c.Value })
            }
        };

        return response;
    }

    private static long ToUnixEpochDate(DateTime date)
        => (long)Math.Round((date.ToUniversalTime() - new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero)).TotalSeconds);
}
