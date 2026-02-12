using EduOnline.Core.Data.EventSourcing;
using EduOnline.Core.Mensagens;
using EventStore.Client;
using System.Text;
using System.Text.Json;

namespace EventSourcing;

public class EventSourcingRepository(IEventStoreService eventStoreService) : IEventSourcingRepository
{
    private readonly IEventStoreService _eventStoreService = eventStoreService;

    public async Task SalvarEvento<TEvent>(TEvent evento) where TEvent : Event
    {
        var client = _eventStoreService.GetClient();

        var eventoFormatado = FormatarEvento(evento);

        await client.AppendToStreamAsync(evento.AggregateId.ToString(), StreamState.Any, [FormatarEvento(evento)]);
    }

    public async Task<IEnumerable<StoredEvent>> ObterEventos(Guid aggregateId)
    {
        var client = _eventStoreService.GetClient();
        var eventos = client.ReadStreamAsync(Direction.Forwards, aggregateId.ToString(), StreamPosition.Start);

        var listaEventos = new List<StoredEvent>();

        foreach (var resolvedEvent in await eventos.ToListAsync())
        {
            var dataEncoded = Encoding.UTF8.GetString(resolvedEvent.OriginalEvent.Data.ToArray());
            var jsonData = JsonSerializer.Deserialize<BaseEvent>(dataEncoded);

            if(jsonData == null)
                continue;

            var evento = new StoredEvent(
                Guid.Parse(resolvedEvent.Event.EventId.ToString()),
                resolvedEvent.Event.EventType,
                jsonData.Timestamp,
                dataEncoded);

            listaEventos.Add(evento);
        }

        return listaEventos.OrderBy(e => e.DataOcorrencia);
    }

    private static EventData FormatarEvento<TEvent>(TEvent evento) where TEvent : Event
    {
        return new EventData(
            Uuid.NewUuid(),
            evento.MessageType,
            JsonSerializer.SerializeToUtf8Bytes(evento));
    }
}

internal class BaseEvent
{
    public DateTime Timestamp { get; set; }
}
