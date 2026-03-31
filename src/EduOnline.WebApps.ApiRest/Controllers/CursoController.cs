using EduOnline.Bff.ApiRest.Services;
using EduOnline.Core.Api.Controllers;
using EduOnline.WebApps.ApiRest.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduOnline.Bff.ApiRest.Controllers;

/// <summary>
/// Endpoints de cursos expostos pela BFF.
/// </summary>
[ApiController]
[Produces("application/json")]
[Consumes("application/json")]
[ProducesResponseType(typeof(ResponseResult), StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
[ProducesResponseType(StatusCodes.Status500InternalServerError)]
[Authorize(Roles = "Aluno,Administrador")]
[Route("api/bff/conteudos/cursos")]
public class CursoController(IConteudoService conteudoService) : ControllerBase
{
    /// <summary>
    /// Lista todos os cursos.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObterTodos()
    {
        var response = await conteudoService.ObterTodosCursos();
        return response.Success ? Ok(response) : RespostaErro(response);
    }

    /// <summary>
    /// Obtém um curso pelo identificador.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(Guid id)
    {
        var response = await conteudoService.ObterCursoPorId(id);
        return response.Success ? Ok(response) : RespostaErro(response);
    }

    /// <summary>
    /// Lista as aulas de um curso.
    /// </summary>
    [HttpGet("{id:guid}/aulas")]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObterAulasPorCursoId(Guid id)
    {
        var response = await conteudoService.ObterAulasPorCursoId(id);
        return response.Success ? Ok(response) : RespostaErro(response);
    }

    /// <summary>
    /// Cria um novo curso (somente administrador).
    /// </summary>
    [Authorize(Roles = "Administrador")]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> Criar(CursoRequest request)
    {
        var response = await conteudoService.CriarCurso(request);
        return response.Success ? Created() : RespostaErro(response);
    }

    /// <summary>
    /// Atualiza um curso existente (somente administrador).
    /// </summary>
    [Authorize(Roles = "Administrador")]
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Atualizar(Guid id, CursoRequest request)
    {
        var response = await conteudoService.AtualizarCurso(id, request);
        return response.Success ? NoContent() : RespostaErro(response);
    }

    /// <summary>
    /// Inativa um curso (somente administrador).
    /// </summary>
    [Authorize(Roles = "Administrador")]
    [HttpPatch("{id:guid}/inativar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> InativarCurso(Guid id)
    {
        var response = await conteudoService.InativarCurso(id);
        return response.Success ? NoContent() : RespostaErro(response);
    }

    /// <summary>
    /// Adiciona aula a um curso (somente administrador).
    /// </summary>
    [Authorize(Roles = "Administrador")]
    [HttpPost("{id:guid}/aulas")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> AdicionarAula(Guid id, AulaRequest request)
    {
        var response = await conteudoService.AdicionarAula(id, request);
        return response.Success ? NoContent() : RespostaErro(response);
    }

    /// <summary>
    /// Atualiza uma aula de um curso (somente administrador).
    /// </summary>
    [Authorize(Roles = "Administrador")]
    [HttpPut("{id:guid}/aulas/{aulaId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> AtualizarAula(Guid id, Guid aulaId, AulaRequest request)
    {
        var response = await conteudoService.AtualizarAula(id, aulaId, request);
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
