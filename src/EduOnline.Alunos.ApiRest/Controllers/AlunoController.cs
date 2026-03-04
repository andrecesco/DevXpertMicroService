using EduOnline.Alunos.Application.Commands;
using EduOnline.Alunos.Application.Queries;
using EduOnline.Alunos.ApiRest.Models;
using EduOnline.Core.Api.Controllers;
using EduOnline.Core.Communication.Mediator;
using EduOnline.Core.ControleDeAcesso;
using EduOnline.Core.Mensagens.Notifications;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduOnline.Alunos.ApiRest.Controllers;

[Authorize]
[Route("api/alunos")]
public class AlunoController(IMediatorHandler mediatorHandler,
INotificationHandler<DomainNotification> notifications,
IAlunoQuery alunoQuery,
IAspNetUser user) : MainController(notifications, user)
{
    private readonly IMediatorHandler _mediatorHandler = mediatorHandler;
    private readonly INotificationHandler<DomainNotification> _notifications = notifications;
    private readonly IAlunoQuery _alunoQuery = alunoQuery;
    private readonly IAspNetUser _user = user;

    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseResult))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpGet()]
    public async Task<IActionResult> ObterTodos()
    {
        var alunos = await _alunoQuery.ObterTodos();
        return CustomResponse(alunos);
    }

    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseResult))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpGet("{id}")]
    public async Task<IActionResult> ObterPorId(Guid id)
    {
        var aluno = await _alunoQuery.ObterPorId(id);
        if (aluno is null)
            return NotFound();
        return CustomResponse(aluno);
    }

    [HttpGet("{id}/matriculas")]
    public async Task<IActionResult> ObterMatriculasPorAlunoId(Guid id)
    {
        if (id != _user.GetUserId() && !_user.IsInRole("Administrador"))
            return Unauthorized();

        var matriculas = await _alunoQuery.ObterMatriculasPorAlunoId(id);

        return CustomResponse(matriculas);
    }

    [HttpGet("{id}/matriculas/{matriculaId}")]
    public async Task<IActionResult> ObterMatriculaPorId(Guid id, Guid matriculaId)
    {
        if (id != _user.GetUserId() && !_user.IsInRole("Administrador"))
            return Unauthorized();

        var matricula = await _alunoQuery.ObterMatriculaPorId(matriculaId);

        return CustomResponse(matricula);
    }

    [HttpGet("{id}/matriculas/{matriculaId}/certificado")]
    public async Task<IActionResult> ObterCertificadoPorMatriculaId(Guid id, Guid matriculaId)
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

    [HttpPost("{id}")]
    public async Task<IActionResult> Cadastrar(Guid id, AdicionarAlunoRequest request)
    {
        var command = new AdicionarAlunoCommand(id, request.Nome, request.Email);

        var resultado = await _mediatorHandler.EnviarComando(command);

        if (!resultado)
            return CustomResponse();

        return CreatedAtAction(actionName: nameof(ObterPorId), routeValues: id, value: null);
    }

    [HttpPatch]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResponseResult))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [Route("{id}")]
    public async Task<IActionResult> AtualizarAluno(Guid id, AtualizarAlunoRequest request)
    {
        if (id != _user.GetUserId() && !_user.IsInRole("Administrador"))
            return Unauthorized();

        var command = new AlterarAlunoCommand(id, request.Nome, request.DataNascimento);

        var resultado = await _mediatorHandler.EnviarComando(command);

        if (!resultado)
            return CustomResponse();

        return NoContent();
    }

    [HttpPost("{id}/matriculas")]
    public async Task<IActionResult> MatricularAluno(Guid id, [FromBody] AdicionarMatriculaRequest request)
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

    [HttpPatch]
    [Route("{id}/matriculas/{matriculaId}/progresso/{aulaId}")]
    public async Task<IActionResult> AtualizarProgressoCurso(Guid id, Guid matriculaId, Guid aulaId)
    {
        if (id != _user.GetUserId() && !_user.IsInRole("Administrador"))
            return Unauthorized();

        var command = new AtualizarHistoricoCommand(matriculaId, aulaId);

        var resultado = await _mediatorHandler.EnviarComando(command);

        if (!resultado)
            return CustomResponse();

        return NoContent();
    }
}
