using MediatR;

namespace EduOnline.Core.Mensagens.Notifications;

public class DomainNotificationHandler : INotificationHandler<DomainNotification>
{
    private List<DomainNotification> _notificacoes;

    public DomainNotificationHandler()
    {
        _notificacoes = [];
    }

    public Task Handle(DomainNotification message, CancellationToken cancellationToken)
    {
        _notificacoes.Add(message);
        return Task.CompletedTask;
    }

    public virtual List<DomainNotification> ObterNotificacoes()
    {
        return _notificacoes;
    }

    public virtual bool TemNotificacao()
    {
        return ObterNotificacoes().Count != 0;
    }

    public void Dispose()
    {
        _notificacoes = [];
    }
}
