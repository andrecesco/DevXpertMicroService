using EduOnline.Core.Mensagens.IntegrationEvents;

namespace EduOnline.Core.Mensagens.RabbitMq;

public interface IRabbitMqEventBus
{
    Task PublishAsync<T>(T integrationEvent) where T : IntegrationEvent;
    Task SubscribeAsync<T>(Func<T, Task> onMessage, CancellationToken cancellationToken) where T : IntegrationEvent;
}
