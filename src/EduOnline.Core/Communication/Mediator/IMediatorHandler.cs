using EduOnline.Core.Mensagens;
using EduOnline.Core.Mensagens.DomainEvents;
using EduOnline.Core.Mensagens.Notifications;

namespace EduOnline.Core.Communication.Mediator;

public interface IMediatorHandler
{
    Task PublicarEvento<T>(T evento) where T : Event;
    Task<bool> EnviarComando<T>(T comando) where T : Command;
    Task PublicarNotificacao<T>(T notificacao) where T : DomainNotification;
    Task PublicarDomainEvent<T>(T notificacao) where T : DomainEvent;
}
