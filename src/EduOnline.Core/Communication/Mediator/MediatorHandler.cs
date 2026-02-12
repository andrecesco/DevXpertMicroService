using EduOnline.Core.Data.EventSourcing;
using EduOnline.Core.DomainObjects;
using EduOnline.Core.Mensagens;
using EduOnline.Core.Mensagens.DomainEvents;
using EduOnline.Core.Mensagens.Notifications;
using MediatR;

namespace EduOnline.Core.Communication.Mediator;

public class MediatorHandler(IMediator mediator, IEventSourcingRepository eventSourcingRepository) : IMediatorHandler
{
    private readonly IMediator _mediator = mediator;
    private readonly IEventSourcingRepository _eventSourcingRepository = eventSourcingRepository;

    public async Task<bool> EnviarComando<T>(T comando) where T : Command
    {
        return await _mediator.Send(comando);
    }

    public async Task PublicarEvento<T>(T evento) where T : Event
    {
        await _mediator.Publish(evento);
        await _eventSourcingRepository.SalvarEvento(evento);
    }

    public async Task PublicarNotificacao<T>(T notificacao) where T : DomainNotification
    {
        await _mediator.Publish(notificacao);
    }

    public async Task PublicarDomainEvent<T>(T notificacao) where T : DomainEvent
    {
        await _mediator.Publish(notificacao);
    }
}
