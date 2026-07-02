using EduOnline.Bff.ApiRest.Services;
using EduOnline.Core.Api.Controllers;
using EduOnline.Core.ControleDeAcesso;
using EduOnline.WebApps.ApiRest.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace EduOnline.Bff.ApiRest.Controllers;

/// <summary>
/// Endpoints de alunos expostos pela BFF, com orquestração entre serviços.
/// </summary>
[ApiController]
[Produces("application/json")]
[Consumes("application/json")]
[ProducesResponseType(typeof(ResponseResult), StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
[ProducesResponseType(StatusCodes.Status500InternalServerError)]
[Authorize(Roles = "Aluno,Administrador")]
[Route("api/bff/alunos")]
public class AlunoController(IAlunoService alunoService, IConteudoService conteudoService, IAspNetUser user) : ControllerBase
{
    /// <summary>
    /// Lista todos os alunos (somente administrador).
    /// </summary>
    [Authorize(Roles = "Administrador")]
    [HttpGet]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObterTodos()
    {
        var response = await alunoService.ObterTodos();
        return response.Success ? Ok(response) : RespostaErro(response);
    }

    /// <summary>
    /// Obtém os dados de um aluno pelo identificador.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(Guid id)
    {
        if (id != user.GetUserId() && !user.IsInRole("Administrador"))
            return Forbid();

        var response = await alunoService.ObterPorId(id);
        return response.Success ? Ok(response) : RespostaErro(response);
    }

    /// <summary>
    /// Lista as matrículas de um aluno.
    /// </summary>
    [HttpGet("{id:guid}/matriculas")]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObterMatriculasPorAlunoId(Guid id)
    {
        if (id != user.GetUserId() && !user.IsInRole("Administrador"))
            return Forbid();

        var response = await alunoService.ObterMatriculasPorAlunoId(id);
        return response.Success ? Ok(response) : RespostaErro(response);
    }

    /// <summary>
    /// Obtém uma matrícula específica do aluno.
    /// </summary>
    [HttpGet("{id:guid}/matriculas/{matriculaId:guid}")]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterMatriculaPorId(Guid id, Guid matriculaId)
    {
        if (id != user.GetUserId() && !user.IsInRole("Administrador"))
            return Forbid();

        var response = await alunoService.ObterMatriculaPorId(id, matriculaId);
        return response.Success ? Ok(response) : RespostaErro(response);
    }

    /// <summary>
    /// Obtém o certificado de conclusão associado à matrícula.
    /// </summary>
    [HttpGet("{id:guid}/matriculas/{matriculaId:guid}/certificado")]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterCertificadoPorMatriculaId(Guid id, Guid matriculaId)
    {
        if (id != user.GetUserId() && !user.IsInRole("Administrador"))
            return Forbid();

        var response = await alunoService.ObterCertificadoPorMatriculaId(id, matriculaId);
        return response.Success ? Ok(response) : RespostaErro(response);
    }

    /// <summary>
    /// Atualiza os dados cadastrais do aluno.
    /// </summary>
    [HttpPatch("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> AtualizarAluno(Guid id, AtualizarAlunoRequest request)
    {
        if (id != user.GetUserId() && !user.IsInRole("Administrador"))
            return Forbid();

        var response = await alunoService.AtualizarAluno(id, request);
        return response.Success ? NoContent() : RespostaErro(response);
    }

    /// <summary>
    /// Realiza matrícula do aluno em um curso e prepara dados para pagamento.
    /// </summary>
    [HttpPost("{id:guid}/matriculas")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> MatricularAluno(Guid id, [FromBody] AdicionarMatriculaRequest request)
    {
        if (id != user.GetUserId() && !user.IsInRole("Administrador"))
            return Forbid();

        var cursoResponse = await conteudoService.ObterCursoPorId(request.CursoId);

        if (!cursoResponse.Success)
            return RespostaErro(cursoResponse);

        if (cursoResponse.Data is not JsonElement cursoElement)
            return BadRequest("Não foi possível interpretar os dados do curso");

        if (!cursoElement.TryGetProperty("nome", out var nomeElement) ||
            !cursoElement.TryGetProperty("valor", out var valorElement))
        {
            return BadRequest("Dados obrigatórios do curso não foram retornados pela API de Conteúdos");
        }

        request.CursoNome = nomeElement.GetString() ?? string.Empty;
        request.Valor = valorElement.GetDecimal();

        request.TotalAulas = cursoElement.TryGetProperty("aulas", out var aulasElement) && aulasElement.ValueKind == JsonValueKind.Array
            ? aulasElement.GetArrayLength()
            : 0;

        var response = await alunoService.MatricularAluno(id, request);

        return response.Success ? Created() : RespostaErro(response);
    }

    /// <summary>
    /// Atualiza o progresso do aluno em uma aula.
    /// </summary>
    [HttpPatch("{id:guid}/matriculas/{matriculaId:guid}/progresso/{aulaId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> AtualizarProgressoCurso(Guid id, Guid matriculaId, Guid aulaId)
    {
        if (id != user.GetUserId() && !user.IsInRole("Administrador"))
            return Forbid();

        var matriculaResponse = await alunoService.ObterMatriculaPorId(id, matriculaId);
        if (!matriculaResponse.Success)
            return RespostaErro(matriculaResponse);

        if (matriculaResponse.Data is not JsonElement matriculaElement ||
            !matriculaElement.TryGetProperty("cursoId", out var cursoIdElement) ||
            !cursoIdElement.TryGetGuid(out var cursoId))
        {
            return BadRequest("Não foi possível identificar o curso da matrícula para registrar o consumo da aula.");
        }

        var response = await conteudoService.RegistrarConsumoAula(cursoId, aulaId, id, matriculaId);
        return response.Success ? NoContent() : RespostaErro(response);
    }

    /// <summary>
    /// Solicita finalização do curso para a matrícula informada.
    /// </summary>
    [HttpPatch("{id:guid}/matriculas/{matriculaId:guid}/finalizar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> FinalizarCurso(Guid id, Guid matriculaId)
    {
        if (id != user.GetUserId() && !user.IsInRole("Administrador"))
            return Forbid();

        var response = await alunoService.FinalizarCurso(id, matriculaId);
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
