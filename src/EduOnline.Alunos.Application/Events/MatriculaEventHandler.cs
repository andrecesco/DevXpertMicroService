using EduOnline.Alunos.Application.Commands;
using EduOnline.Core.Communication.Mediator;
using EduOnline.Core.Mensagens.IntegrationEvents;
using MediatR;

namespace EduOnline.Alunos.Application.Events;

public class MatriculaEventHandler(IMediatorHandler mediatorHandler) :
    INotificationHandler<CursoFinalizadoEvent>,
    INotificationHandler<PagamentoRealizadoIntegrationEvent>,
    INotificationHandler<PagamentoRecusadoIntegrationEvent>
{
    private readonly IMediatorHandler _mediatorHandler = mediatorHandler;

    public async Task Handle(CursoFinalizadoEvent notification, CancellationToken cancellationToken)
    {
        await _mediatorHandler.EnviarComando(new GerarCertificadoCommand(notification.AggregateId));
    }

    public async Task Handle(PagamentoRealizadoIntegrationEvent notification, CancellationToken cancellationToken)
    {
        await _mediatorHandler.EnviarComando(new MatriculaPagaCommand(notification.AggregateId));
    }

    public async Task Handle(PagamentoRecusadoIntegrationEvent notification, CancellationToken cancellationToken)
    {
        await _mediatorHandler.EnviarComando(new MatriculaRecusadaCommand(notification.AggregateId));
    }
}
