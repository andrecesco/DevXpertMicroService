using EduOnline.Core.Api.Controllers;
using EduOnline.Core.ControleDeAcesso;
using EduOnline.Core.DomainObjects.Dtos;
using EduOnline.Core.Mensagens.Notifications;
using EduOnline.Pagamentos.ApiRest.Models;
using EduOnline.Pagamentos.Domain;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduOnline.Pagamentos.ApiRest.Controllers;

/// <summary>
/// Endpoints para processamento e consulta de pagamentos.
/// </summary>
[Produces("application/json")]
[Consumes("application/json")]
[ProducesResponseType(typeof(ResponseResult), StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
[ProducesResponseType(StatusCodes.Status500InternalServerError)]
[Authorize(Roles = "Aluno,Administrador")]
[Route("api/pagamentos")]
public class PagamentosController(
    IPagamentoService pagamentoService,
    IPagamentoRepository pagamentoRepository,
    INotificationHandler<DomainNotification> notifications,
    IAspNetUser user) : MainController(notifications, user)
{
    /// <summary>
    /// Processa o pagamento de uma matrícula.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> RealizarPagamento(RealizarPagamentoRequest request)
    {
        if (!ModelState.IsValid)
            return CustomResponse(ModelState);

        var pagamentoCurso = new PagamentoCurso
        {
            MatriculaId = request.MatriculaId,
            CursoId = request.CursoId,
            AlunoId = AppUser.GetUserId(),
            Total = request.Total,
            NomeCartao = request.NomeCartao,
            NumeroCartao = request.NumeroCartao,
            ExpiracaoCartao = request.ExpiracaoCartao,
            CvvCartao = request.CvvCartao
        };

        var transacao = await pagamentoService.RealizarPagamentoCurso(pagamentoCurso);

        if (!OperacaoValida())
            return CustomResponse(transacao);

        return CustomResponse(transacao);
    }

    /// <summary>
    /// Lista todos os pagamentos cadastrados.
    /// </summary>
    [Authorize(Roles = "Administrador")]
    [HttpGet]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObterTodos()
    {
        var pagamentos = await pagamentoRepository.ObterTodosAsync();
        return CustomResponse(pagamentos);
    }

    /// <summary>
    /// Obtém um pagamento pelo identificador.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ResponseResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(Guid id)
    {
        var pagamento = await pagamentoRepository.ObterPorIdAsync(id);

        if (pagamento is null)
            return NotFound();

        if (pagamento.AlunoId != AppUser.GetUserId() && !AppUser.IsInRole("Administrador"))
            return Forbid();

        return CustomResponse(pagamento);
    }
}
