using MediatR;

namespace EduOnline.Core.Mensagens.Notifications;

public class DomainNotification(string key, string value) : Message, INotification
{
    public DateTime Timestamp { get; private set; } = DateTime.Now;
    public Guid DomainNotificationId { get; private set; } = Guid.NewGuid();
    public string Key { get; private set; } = key;
    public string Value { get; private set; } = value;
    public int Version { get; private set; } = 1;
}
