using EduOnline.Core.Mensagens.IntegrationEvents;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;

namespace EduOnline.Core.Mensagens.RabbitMq;

public class RabbitMqEventBus : IRabbitMqEventBus, IDisposable
{
    private readonly RabbitMqSettings _settings;
    private readonly ILogger<RabbitMqEventBus> _logger;
    private readonly ConnectionFactory _factory;
    private readonly ConcurrentBag<IChannel> _consumerChannels = [];
    private readonly SemaphoreSlim _connectionLock = new(1, 1);
    private IConnection? _connection;
    private IChannel? _publisherChannel;

    public RabbitMqEventBus(IOptions<RabbitMqSettings> options, ILogger<RabbitMqEventBus> logger)
    {
        _settings = options.Value;
        _logger = logger;

        _factory = new ConnectionFactory
        {
            HostName = _settings.HostName,
            Port = _settings.Port,
            UserName = _settings.UserName,
            Password = _settings.Password
        };
    }

    public async Task PublishAsync<T>(T integrationEvent) where T : IntegrationEvent
    {
        if (!_settings.Enabled) return;

        try
        {
            await EnsureConnectionAsync();

            if (_publisherChannel is null)
                return;

            var routingKey = typeof(T).Name;
            var payload = JsonSerializer.Serialize(integrationEvent);
            var body = Encoding.UTF8.GetBytes(payload);

            await _publisherChannel.BasicPublishAsync<BasicProperties>(
                exchange: _settings.ExchangeName,
                routingKey: routingKey,
                mandatory: false,
                basicProperties: new BasicProperties(),
                body: body);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Falha ao publicar evento de integração {EventType} no RabbitMQ", typeof(T).Name);
        }
    }

    public async Task SubscribeAsync<T>(Func<T, Task> onMessage, CancellationToken cancellationToken) where T : IntegrationEvent
    {
        if (!_settings.Enabled) return;

        try
        {
            await EnsureConnectionAsync(cancellationToken);

            if (_connection is null || !_connection.IsOpen)
                return;

            var channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);
            _consumerChannels.Add(channel);

            var queueName = typeof(T).Name;

            await channel.ExchangeDeclareAsync(_settings.ExchangeName, ExchangeType.Direct, durable: true, autoDelete: false, cancellationToken: cancellationToken);
            await channel.QueueDeclareAsync(queueName, durable: true, exclusive: false, autoDelete: false, cancellationToken: cancellationToken);
            await channel.QueueBindAsync(queueName, _settings.ExchangeName, queueName, cancellationToken: cancellationToken);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (_, ea) =>
            {
                try
                {
                    var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                    var message = JsonSerializer.Deserialize<T>(json);

                    if (message is null)
                    {
                        await channel.BasicAckAsync(ea.DeliveryTag, false, cancellationToken);
                        return;
                    }

                    await onMessage(message);
                    await channel.BasicAckAsync(ea.DeliveryTag, false, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao processar evento de integração {EventType} vindo do RabbitMQ", typeof(T).Name);
                    await channel.BasicNackAsync(ea.DeliveryTag, false, requeue: true, cancellationToken);
                }
            };

            await channel.BasicConsumeAsync(queue: queueName, autoAck: false, consumer: consumer, cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Falha ao assinar evento de integração {EventType} no RabbitMQ", typeof(T).Name);
        }
    }

    private async Task EnsureConnectionAsync(CancellationToken cancellationToken = default)
    {
        if (_connection?.IsOpen == true && _publisherChannel?.IsOpen == true)
            return;

        await _connectionLock.WaitAsync(cancellationToken);
        try
        {
            if (_connection?.IsOpen == true && _publisherChannel?.IsOpen == true)
                return;

            _connection?.Dispose();
            _publisherChannel?.Dispose();

            _connection = await _factory.CreateConnectionAsync(cancellationToken);
            _publisherChannel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);
            await _publisherChannel.ExchangeDeclareAsync(_settings.ExchangeName, ExchangeType.Direct, durable: true, autoDelete: false, cancellationToken: cancellationToken);
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    public void Dispose()
    {
        while (_consumerChannels.TryTake(out var channel))
        {
            channel.Dispose();
        }

        _publisherChannel?.Dispose();
        _connection?.Dispose();
        _connectionLock.Dispose();
    }
}
