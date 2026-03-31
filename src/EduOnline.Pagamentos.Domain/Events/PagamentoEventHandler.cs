using EduOnline.Core.DomainObjects.Dtos;
using EduOnline.Core.Mensagens.IntegrationEvents;
using MediatR;

namespace EduOnline.Pagamentos.Domain.Events;

public class PagamentoEventHandler(IPagamentoService pagamentoService) : INotificationHandler<CursoCompradoIntegrationEvent>
{
    public async Task Handle(CursoCompradoIntegrationEvent notification, CancellationToken cancellationToken)
    {
        var pagamentoCurso = new PagamentoCurso
        {
            MatriculaId = notification.AggregateId,
            AlunoId = notification.AlunoId,
            CursoId = notification.CursoId,
            Total = notification.Total,
            NomeCartao = notification.NomeCartao,
            NumeroCartao = notification.NumeroCartao,
            ExpiracaoCartao = notification.ExpiracaoCartao,
            CvvCartao = notification.CvvCartao
        };

        await pagamentoService.RealizarPagamentoCurso(pagamentoCurso);
    }
}
