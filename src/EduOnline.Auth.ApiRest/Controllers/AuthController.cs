using EduOnline.Auth.ApiRest.Data;
using EduOnline.Auth.ApiRest.Models;
using EduOnline.Auth.ApiRest.Services;
using EduOnline.Core.Api.Controllers;
using EduOnline.Core.ControleDeAcesso;
using EduOnline.Core.Mensagens;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EduOnline.Auth.ApiRest.Controllers;

/// <summary>
/// Endpoints de autenticação e gestão de contas de usuários.
/// </summary>
[Produces("application/json")]
[Consumes("application/json")]
[ProducesResponseType(typeof(ResponseResult), StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
[ProducesResponseType(StatusCodes.Status500InternalServerError)]
[Route("api/auth")]
public class AuthController(INotificador notificador,
                      AuthenticationService authenticationService,
                      IAspNetUser user,
                      RoleManager<IdentityRole> roleManager,
                      ILogger<AuthController> logger) : MainController(notificador, user)
{
    private readonly ILogger _logger = logger;

    /// <summary>
    /// Obtém dados de um usuário pelo identificador.
    /// </summary>
    [Authorize]
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(Guid id)
    {
        if (user != null && id != user.GetUserId() && !user.IsInRole("Administrador"))
            return Unauthorized();

        var result = await authenticationService.ObterUserId(id);

        if (result is null) return NotFound();

        return CustomResponse(result);
    }

    /// <summary>
    /// Registra uma nova conta de usuário.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("nova-conta")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult> Registrar(UsuarioRegistroModel usarioRegistro)
    {
        if (!ModelState.IsValid) return CustomResponse(ModelState);

        var perfil = usarioRegistro.Perfil.Trim();
        perfil = perfil.Equals("Administrador", StringComparison.OrdinalIgnoreCase)
            ? "Administrador"
            : "Aluno";

        if (perfil == "Administrador" && (!user.IsAuthenticated() || !user.IsInRole("Administrador")))
        {
            NotificarErro("Somente administradores podem cadastrar novos administradores.");
            return CustomResponse();
        }

        var usuario = new EduOnlineUser
        {
            UserName = usarioRegistro.Email,
            Email = usarioRegistro.Email,
            EmailConfirmed = true
        };

        var result = await authenticationService.UserManager.CreateAsync(usuario, usarioRegistro.Senha);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                NotificarErro(error.Description);
            }

            return CustomResponse();
        }

        if (!await roleManager.RoleExistsAsync(perfil))
        {
            await roleManager.CreateAsync(new IdentityRole(perfil));
        }

        var roleResult = await authenticationService.UserManager.AddToRoleAsync(usuario, perfil);
        if (!roleResult.Succeeded)
        {
            await authenticationService.UserManager.DeleteAsync(usuario);

            foreach (var error in roleResult.Errors)
            {
                NotificarErro(error.Description);
            }

            return CustomResponse();
        }

        return CreatedAtAction(actionName: nameof(ObterPorId), routeValues: new { id = usuario.Id }, null);
    }

    /// <summary>
    /// Realiza autenticação com e-mail e senha e retorna token JWT.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("entrar")]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status200OK)]
    public async Task<ActionResult> Login(UsuarioLoginModel loginUser)
    {
        if (!ModelState.IsValid) return CustomResponse(ModelState);

        var result = await authenticationService.SignInManager.PasswordSignInAsync(loginUser.Email, loginUser.Senha,
            false, true);

        var userAuth = await authenticationService.UserManager.FindByEmailAsync(loginUser.Email);

        if (userAuth is null)
        {
            NotificarErro("Usuário ou Senha incorretos");
            return CustomResponse(loginUser);
        }

        if (userAuth.StatusId == Status.Pendente.Id)
        {
            userAuth.StatusId = Status.Cadastrado.Id;
            userAuth.StatusNome = Status.Cadastrado.Nome;
            var resultadoUpdate = await authenticationService.UserManager.UpdateAsync(userAuth);

            if (!resultadoUpdate.Succeeded)
            {
                NotificarErro($"Erro ao atualizar o usuário para Cadastrado: {resultadoUpdate.Errors.First().Description}");
                return CustomResponse();
            }
        }

        if (result.Succeeded)
        {
            _logger.LogInformation("Usuario " + loginUser.Email + " logado com sucesso");
            return CustomResponse(await authenticationService.GerarJwt(loginUser.Email));
        }

        if (result.IsLockedOut)
        {
            NotificarErro("Usuário temporariamente bloqueado por tentativas inválidas");
            return CustomResponse(loginUser);
        }

        NotificarErro("Usuário ou Senha incorretos");
        return CustomResponse(loginUser);
    }

    /// <summary>
    /// Exclui um usuário existente.
    /// </summary>
    [Authorize(Roles = "Administrador")]
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Excluir(Guid id)
    {
        var result = await authenticationService.ObterUserId(id);

        if (result is null) return NotFound();

        var deleteResult = await authenticationService.RemoverUser(result);
        if (!deleteResult.Succeeded)
        {
            foreach (var error in deleteResult.Errors)
            {
                NotificarErro(error.Description);
            }
            return CustomResponse();
        }
        return NoContent();
    }

    /// <summary>
    /// Renova o token de acesso com base em um refresh token válido.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status200OK)]
    public async Task<ActionResult> RefreshToken([FromBody] string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken) || !Guid.TryParse(refreshToken, out var refreshTokenGuid))
        {
            NotificarErro("Refresh Token inválido");
            return CustomResponse();
        }

        var token = await authenticationService.ObterRefreshToken(refreshTokenGuid);

        if (token is null)
        {
            NotificarErro("Refresh Token expirado");
            return CustomResponse();
        }

        return CustomResponse(await authenticationService.GerarJwt(token.Username));
    }

    /// <summary>
    /// Encerra a sessão do usuário autenticado.
    /// </summary>
    [Authorize]
    [HttpPost("sair")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout()
    {
        await authenticationService.SignInManager.SignOutAsync();
        return NoContent();
    }
}
