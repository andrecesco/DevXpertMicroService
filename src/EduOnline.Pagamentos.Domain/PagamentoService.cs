using EduOnline.Core.Communication.Mediator;
using EduOnline.Core.DomainObjects.Dtos;
using EduOnline.Core.Mensagens.IntegrationEvents;
using EduOnline.Core.Mensagens.Notifications;

namespace EduOnline.Pagamentos.Domain;

public class PagamentoService(IPagamentoCartaoCreditoFacade pagamentoCartaoCreditoFacade, IPagamentoRepository pagamentoRepository, IMediatorHandler mediatorHandler) : IPagamentoService
{
    public async Task<Transacao> RealizarPagamentoCurso(PagamentoCurso pagamentoCurso)
    {
        var curso = new Curso
        {
            Id = pagamentoCurso.CursoId,
            Valor = pagamentoCurso.Total
        };

        var pagamento = new Pagamento
        {
            AlunoId = pagamentoCurso.AlunoId,
            Total = pagamentoCurso.Total,
            NomeCartao = pagamentoCurso.NomeCartao,
            NumeroCartao = pagamentoCurso.NumeroCartao,
            ExpiracaoCartao = pagamentoCurso.ExpiracaoCartao,
            CvvCartao = pagamentoCurso.CvvCartao
        };

        var transacao = pagamentoCartaoCreditoFacade.RealizarPagamento(curso, pagamento);

        if(transacao.StatusTransacaoId == StatusTransacao.Aprovado.Id)
        {
            pagamentoRepository.Adicionar(pagamento);
            pagamentoRepository.AdicionarTransacao(transacao);

            pagamento.AdicionarEvento(new PagamentoRealizadoEvent(pagamentoCurso.MatriculaId, curso.Id, pagamento.AlunoId, pagamento.Id, transacao.Id, transacao.Total));

            await pagamentoRepository.UnitOfWork.Commit();

            return transacao;
        }

        await mediatorHandler.PublicarNotificacao(new DomainNotification("pagamento", "O pagamento foi recusado pela operadora"));
        await mediatorHandler.PublicarEvento(new PagamentoRecusadoEvent(pagamentoCurso.MatriculaId, curso.Id, pagamento.AlunoId, pagamento.Id, transacao.Id, pagamento.Total));

        return transacao;
    }
}
