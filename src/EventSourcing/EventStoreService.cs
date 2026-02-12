
using EventStore.Client;
using Microsoft.Extensions.Configuration;

namespace EventSourcing;

public class EventStoreService(IConfiguration configuration) : IEventStoreService
{
    private readonly EventStoreClient _client = new(EventStoreClientSettings.Create(configuration?.GetConnectionString("EventStoreConnection") ?? throw new Exception("String de conexão não do event store não informada")));

    public EventStoreClient GetClient()
    {
        return _client;
    }
}
