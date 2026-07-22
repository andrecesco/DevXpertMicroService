using EduOnline.Alunos.ApiRest.Models;
using EduOnline.Alunos.Application.Commands;
using EduOnline.Alunos.Application.Queries;
using EduOnline.Core.Api.Controllers;
using EduOnline.Core.Communication.Mediator;
using EduOnline.Core.ControleDeAcesso;
using EduOnline.Core.DomainObjects;
using EduOnline.Core.Mensagens.Notifications;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduOnline.Alunos.ApiRest.Controllers;

/// <summary>
/// Endpoints para gestão de alunos, matrículas, progresso e certificados.
/// </summary>
[Produces("application/json")]
[Consumes("application/json")]
[ProducesResponseType(typeof(ResponseResult), StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
[ProducesResponseType(StatusCodes.Status500InternalServerError)]
[Authorize(Roles = "Aluno,Administrador")]
[Route("api/alunos")]
public class AlunoController(IMediatorHandler mediatorHandler,
INotificationHandler<DomainNotification> notifications,
IAlunoQuery alunoQuery,
IAspNetUser user) : MainController(notifications, user)
{
    private readonly IMediatorHandler _mediatorHandler = mediatorHandler;
    private readonly IAlunoQuery _alunoQuery = alunoQuery;
    private readonly IAspNetUser _user = user;

    /// <summary>
    /// Lista todos os alunos cadastrados.
    /// </summary>
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseResult))]
    [HttpGet()]
    public async Task<IActionResult> ObterTodos()
    {
        try
        {
            var alunos = await _alunoQuery.ObterTodos();
            return CustomResponse(alunos);
        }
        catch (DomainException ex)
        {
            NotificarErro(ex.Message);
            return CustomResponse();
        }
        catch (Exception)
        {
            NotificarErro("Ocorreu um erro inesperado ao atualizar o progresso do curso.");
            return CustomResponse();
        }
    }

    /// <summary>
    /// Obtém um aluno pelo identificador.
    /// </summary>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseResult))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpGet("{id}")]
    public async Task<IActionResult> ObterPorId(Guid id)
    {
        try
        {
            if (id != _user.GetUserId() && !_user.IsInRole("Administrador"))
                return Unauthorized();

            var aluno = await _alunoQuery.ObterPorId(id);
            if (aluno is null)
                return NotFound();
            return CustomResponse(aluno);

        }
        catch (DomainException ex)
        {
            NotificarErro(ex.Message);
            return CustomResponse();
        }
        catch (Exception)
        {
            NotificarErro("Ocorreu um erro inesperado ao atualizar o progresso do curso.");
            return CustomResponse();
        }
    }

    /// <summary>
    /// Lista matrículas de um aluno.
    /// </summary>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseResult))]
    [HttpGet("{id}/matriculas")]
    public async Task<IActionResult> ObterMatriculasPorAlunoId(Guid id)
    {
        try
        {
            if (id != _user.GetUserId() && !_user.IsInRole("Administrador"))
                return Unauthorized();

            var matriculas = await _alunoQuery.ObterMatriculasPorAlunoId(id);

            return CustomResponse(matriculas);
        }
        catch (DomainException ex)
        {
            NotificarErro(ex.Message);
            return CustomResponse();
        }
        catch (Exception)
        {
            NotificarErro("Ocorreu um erro inesperado ao atualizar o progresso do curso.");
            return CustomResponse();
        }
    }

    /// <summary>
    /// Obtém uma matrícula específica de um aluno.
    /// </summary>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseResult))]
    [HttpGet("{id}/matriculas/{matriculaId}")]
    public async Task<IActionResult> ObterMatriculaPorId(Guid id, Guid matriculaId)
    {
        try
        {
            if (id != _user.GetUserId() && !_user.IsInRole("Administrador"))
                return Unauthorized();

            var matricula = await _alunoQuery.ObterMatriculaPorId(matriculaId);

            return CustomResponse(matricula);
        }
        catch (DomainException ex)
        {
            NotificarErro(ex.Message);
            return CustomResponse();
        }
        catch (Exception)
        {
            NotificarErro("Ocorreu um erro inesperado ao atualizar o progresso do curso.");
            return CustomResponse();
        }
    }

    /// <summary>
    /// Obtém o certificado gerado para uma matrícula.
    /// </summary>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseResult))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpGet("{id}/matriculas/{matriculaId}/certificado")]
    public async Task<IActionResult> ObterCertificadoPorMatriculaId(Guid id, Guid matriculaId)
    {
        try
        {
            if (id != _user.GetUserId() && !_user.IsInRole("Administrador"))
                return Unauthorized();

            var certificado = await _alunoQuery.ObterCertificadoPorMatriculaId(matriculaId);

            if (certificado is null)
            {
                NotificarErro("Certificado não encontrado");
                return CustomResponse();
            }

            return CustomResponse(certificado);
        }
        catch (DomainException ex)
        {
            NotificarErro(ex.Message);
            return CustomResponse();
        }
        catch (Exception)
        {
            NotificarErro("Ocorreu um erro inesperado ao atualizar o progresso do curso.");
            return CustomResponse();
        }
    }

    /// <summary>
    /// Cadastra um aluno vinculado a um usuário já criado na autenticação.
    /// </summary>
    [Authorize(Roles = "Administrador")]
    [HttpPost("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Cadastrar(Guid id, AdicionarAlunoRequest request)
    {
        try
        {
            var command = new AdicionarAlunoCommand(id, request.Nome, request.Email);

            var resultado = await _mediatorHandler.EnviarComando(command);

            if (!resultado)
                return CustomResponse();

            return NoContent();
        }
        catch (DomainException ex)
        {
            NotificarErro(ex.Message);
            return CustomResponse();
        }
        catch (Exception)
        {
            NotificarErro("Ocorreu um erro inesperado ao atualizar o progresso do curso.");
            return CustomResponse();
        }
    }

    /// <summary>
    /// Atualiza dados cadastrais de um aluno.
    /// </summary>
    [HttpPatch]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResponseResult))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [Route("{id}")]
    public async Task<IActionResult> AtualizarAluno(Guid id, AtualizarAlunoRequest request)
    {
        try
        {
            if (id != _user.GetUserId() && !_user.IsInRole("Administrador"))
                return Unauthorized();

            var command = new AlterarAlunoCommand(id, request.Nome, request.DataNascimento);

            var resultado = await _mediatorHandler.EnviarComando(command);

            if (!resultado)
                return CustomResponse();

            return NoContent();
        }
        catch (DomainException ex)
        {
            NotificarErro(ex.Message);
            return CustomResponse();
        }
        catch (Exception)
        {
            NotificarErro("Ocorreu um erro inesperado ao atualizar o progresso do curso.");
            return CustomResponse();
        }
    }

    /// <summary>
    /// Realiza a matrícula do aluno em um curso.
    /// </summary>
    [HttpPost("{id}/matriculas")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> MatricularAluno(Guid id, [FromBody] AdicionarMatriculaRequest request)
    {
        try
        {
            if (id != _user.GetUserId() && !_user.IsInRole("Administrador"))
                return Forbid();

            var command = new AdicionarMatriculaCommand(
                _user.GetUserId(),
                request.CursoId,
                request.CursoNome,
                request.Valor,
                request.NomeCartao,
                request.NumeroCartao,
                request.ExpiracaoCartao,
                request.CvvCartao,
                request.TotalAulas);

            var resultado = await _mediatorHandler.EnviarComando(command);

            if (!resultado)
                return CustomResponse();

            return CreatedAtAction(nameof(ObterMatriculaPorId), new { id, matriculaId = command.AggregateId }, null);
        }
        catch (DomainException ex)
        {
            NotificarErro(ex.Message);
            return CustomResponse();
        }
        catch (Exception)
        {
            NotificarErro("Ocorreu um erro inesperado ao criar a matrícula.");
            return CustomResponse();
        }
    }

    /// <summary>
    /// Atualiza o progresso do aluno em uma aula.
    /// </summary>
    [HttpPatch]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [Route("{id}/matriculas/{matriculaId}/progresso/{aulaId}")]
    public async Task<IActionResult> AtualizarProgressoCurso(Guid id, Guid matriculaId, Guid aulaId)
    {
        try
        {
            if (id != _user.GetUserId() && !_user.IsInRole("Administrador"))
                return Unauthorized();

            var command = new AtualizarHistoricoCommand(matriculaId, aulaId);

            var resultado = await _mediatorHandler.EnviarComando(command);

            if (!resultado)
                return CustomResponse();

            return NoContent();
        }
        catch (DomainException ex)
        {
            NotificarErro(ex.Message);
            return CustomResponse();
        }
        catch (Exception)
        {
            NotificarErro("Ocorreu um erro inesperado ao atualizar o progresso do curso.");
            return CustomResponse();
        }
    }

    /// <summary>
    /// Solicita a finalização de um curso e geração de certificado.
    /// </summary>
    [HttpPatch]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [Route("{id}/matriculas/{matriculaId}/finalizar")]
    public async Task<IActionResult> FinalizarCurso(Guid id, Guid matriculaId)
    {
        try
        {
            if (id != _user.GetUserId() && !_user.IsInRole("Administrador"))
                return Unauthorized();

            var command = new GerarCertificadoCommand(matriculaId);

            var resultado = await _mediatorHandler.EnviarComando(command);

            if (!resultado)
                return CustomResponse();

            return NoContent();
        }
        catch (DomainException ex)
        {
            NotificarErro(ex.Message);
            return CustomResponse();
        }
        catch (Exception)
        {
            NotificarErro("Ocorreu um erro inesperado ao atualizar o progresso do curso.");
            return CustomResponse();
        }
    }
}
