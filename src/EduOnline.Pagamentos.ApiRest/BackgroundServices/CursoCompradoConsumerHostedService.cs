using EduOnline.Core.Mensagens.IntegrationEvents;
using EduOnline.Core.Mensagens.RabbitMq;
using MediatR;
using System.Diagnostics.CodeAnalysis;

namespace EduOnline.Pagamentos.ApiRest.BackgroundServices;

[ExcludeFromCodeCoverage]
public class CursoCompradoConsumerHostedService(IServiceProvider serviceProvider, IRabbitMqEventBus eventBus) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        SetSubscriber(stoppingToken);
        return Task.CompletedTask;
    }

    public void SetSubscriber(CancellationToken stoppingToken)
    {
        eventBus.SubscribeAsync<CursoCompradoIntegrationEvent>(RealizarPagamento, stoppingToken);
    }

    public async Task RealizarPagamento(CursoCompradoIntegrationEvent message)
    {
        await using var scope = serviceProvider.CreateAsyncScope();

        var handler = scope.ServiceProvider.GetRequiredService<INotificationHandler<CursoCompradoIntegrationEvent>>();

        await handler.Handle(message, CancellationToken.None);
    }
}
