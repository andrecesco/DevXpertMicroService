using EduOnline.Alunos.Application.Commands;
using EduOnline.Alunos.Application.Queries;
using EduOnline.Alunos.Application.Queries.Dtos;
using EduOnline.Alunos.Domain.Models;
using EduOnline.Conteudos.Domain;
using EduOnline.Core.Communication.Mediator;
using EduOnline.Core.ControleDeAcesso;
using EduOnline.Core.Mensagens.Notifications;
using EduOnline.WebApps.ApiRest.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduOnline.WebApps.ApiRest.Controllers;

[Authorize]
[Route("api/alunos")]
public class AlunoController : MainController
{
    private readonly IMediatorHandler _mediatorHandler;
    private readonly INotificationHandler<DomainNotification> _notifications;
    private readonly IAlunoQuery _alunoQuery;
    private readonly ICursoRepository _cursoRepository;
    private readonly IUser _user;

    public AlunoController(IMediatorHandler mediatorHandler,
    INotificationHandler<DomainNotification> notifications,
    IAlunoQuery alunoQuery,
    ICursoRepository cursoRepository,
    IUser user) : base(notifications, mediatorHandler, user)
    {
        _mediatorHandler = mediatorHandler;
        _notifications = notifications;
        _alunoQuery = alunoQuery;
        _cursoRepository = cursoRepository;
        _user = user;
    }

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

        if(certificado is null)
        {
            NotificarErro("Certificado não encontrado");
            return CustomResponse();
        }

        return CustomResponse(certificado);
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
    public async Task<IActionResult> MatricularAluno(Guid id, [FromBody]AdicionarMatriculaRequest request)
    {
        if (id != _user.GetUserId() && !_user.IsInRole("Administrador"))
            return Forbid();

        var curso = await _cursoRepository.ObterCursoValidoPorIdAsync(request.CursoId);

        if (curso is null)
        {
            NotificarErro("Curso não encontrado ou sua validade já expirou");
            return CustomResponse();
        }

        var command = new AdicionarMatriculaCommand(_user.GetUserId(), curso.Id, curso.Nome, curso.Valor, request.NomeCartao,
            request.NumeroCartao, request.ExpiracaoCartao, request.CvvCartao, curso.Aulas?.Count ?? 0);

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

        var aula = await _cursoRepository.ObterAulaPorIdAsync(aulaId);

        if(aula is null)
        {
            NotificarErro("Aula não encontrada");
            return CustomResponse();
        }

        var command = new AtualizarHistoricoCommand(matriculaId, aula.Id);

        var resultado = await _mediatorHandler.EnviarComando(command);

        if (!resultado)
            return CustomResponse();

        return NoContent();
    }
}
