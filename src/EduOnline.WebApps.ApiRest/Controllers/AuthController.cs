using EduOnline.Bff.ApiRest.Requests;
using EduOnline.Bff.ApiRest.Services;
using EduOnline.Core.Api.Controllers;
using EduOnline.Core.ControleDeAcesso;
using EduOnline.Core.Mensagens;
using EduOnline.WebApps.ApiRest.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduOnline.Bff.ApiRest.Controllers;

/// <summary>
/// Endpoints de autenticação expostos pela BFF para o front-end.
/// </summary>
[ApiController]
[Produces("application/json")]
[Consumes("application/json")]
[ProducesResponseType(typeof(ResponseResult), StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
[ProducesResponseType(StatusCodes.Status500InternalServerError)]
[Route("api/bff/auth")]
public class AuthController(IAuthService authService, IAlunoService alunoService, INotificador notificador, IAspNetUser user) : MainController(notificador, user)
{
    /// <summary>
    /// Cria um novo usuário e seu respectivo aluno no ecossistema.
    /// </summary>
    [AllowAnonymous]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> CriarUsuario(CriarUsuarioRequest request)
    {
        var responseAuth = await authService.CriarUsuarioIdentity(request) ?? throw new Exception("Ocorreu um erro ao se comunicar com a api de autenticação");

        if (!responseAuth.Success) return BadRequest(responseAuth);

        var responseAluno = await alunoService.CriarAluno(authService.AggregateId, new CriarAlunoRequest { Email = request.Email, Nome = request.Nome }) ?? throw new Exception("Ocorreu um erro ao se comunicar com a api de Aluno");

        if (!responseAluno.Success)
        {
            var responseAuthDelete = await authService.RemoverUsuarioIdentity(authService.AggregateId) ?? throw new Exception("Ocorreu um erro ao se comunicar com a api de autenticação");

            if (!responseAuthDelete.Success)
            {
                return BadRequest(responseAuthDelete);
            }

            return BadRequest(responseAluno);
        }

        return Created();
    }

    /// <summary>
    /// Realiza login e retorna token JWT.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("entrar")]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> Entrar(UsuarioLoginModel request)
    {
        var response = await authService.Login(request);
        return response.Success ? Ok(response) : RespostaErro(response);
    }

    /// <summary>
    /// Renova o token de acesso.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> RefreshToken([FromBody] string refreshToken)
    {
        var response = await authService.RefreshToken(refreshToken);
        return response.Success ? Ok(response) : RespostaErro(response);
    }

    /// <summary>
    /// Finaliza a sessão do usuário autenticado.
    /// </summary>
    [Authorize]
    [HttpPost("sair")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Sair()
    {
        var response = await authService.Logout();
        return response.Success ? NoContent() : RespostaErro(response);
    }

    /// <summary>
    /// Obtém dados do usuário autenticado (ou administrador).
    /// </summary>
    [Authorize]
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(Guid id)
    {
        if (id != AppUser.GetUserId() && !AppUser.IsInRole("Administrador"))
            return Forbid();

        var response = await authService.ObterUsuarioPorId(id);
        return response.Success ? Ok(response) : RespostaErro(response);
    }

    /// <summary>
    /// Exclui um usuário do Identity (somente administrador).
    /// </summary>
    [Authorize(Roles = "Administrador")]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Excluir(Guid id)
    {
        var response = await authService.RemoverUsuarioIdentity(id);
        return response.Success ? NoContent() : RespostaErro(response);
    }

    private IActionResult RespostaErro(ResponseResult response)
    {
        var mensagens = response.Errors?.Select(e => e.Value).Where(m => !string.IsNullOrWhiteSpace(m)).ToList() ?? [];

        if (mensagens.Any(m => m.Contains("não encontrado", StringComparison.OrdinalIgnoreCase)))
            return NotFound(response);

        if (mensagens.Any(m => m.Contains("não autenticado", StringComparison.OrdinalIgnoreCase)))
            return Unauthorized(response);

        if (mensagens.Any(m => m.Contains("acesso negado", StringComparison.OrdinalIgnoreCase)))
            return StatusCode(StatusCodes.Status403Forbidden, response);

        return BadRequest(response);
    }
}
