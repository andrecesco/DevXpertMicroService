using EduOnline.Alunos.Application.Commands;
using EduOnline.Core.Communication.Mediator;
using EduOnline.Core.Mensagens.IntegrationEvents;
using MediatR;

namespace EduOnline.Alunos.Application.Events;

public class MatriculaEventHandler(IMediatorHandler mediatorHandler) :
    INotificationHandler<CursoFinalizadoEvent>,
    INotificationHandler<PagamentoRealizadoEvent>,
    INotificationHandler<PagamentoRecusadoEvent>
{
    private readonly IMediatorHandler _mediatorHandler = mediatorHandler;

    public async Task Handle(CursoFinalizadoEvent notification, CancellationToken cancellationToken)
    {
        await _mediatorHandler.EnviarComando(new GerarCertificadoCommand(notification.AggregateId));
    }

    public async Task Handle(PagamentoRealizadoEvent notification, CancellationToken cancellationToken)
    {
        await _mediatorHandler.EnviarComando(new MatriculaPagaCommand(notification.AggregateId));
    }

    public async Task Handle(PagamentoRecusadoEvent notification, CancellationToken cancellationToken)
    {
        await _mediatorHandler.EnviarComando(new MatriculaRecusadaCommand(notification.AggregateId));
    }
}
