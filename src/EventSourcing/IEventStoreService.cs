
using EventStore.Client;

namespace EventSourcing;

public interface IEventStoreService
{
    EventStoreClient GetClient();
}
