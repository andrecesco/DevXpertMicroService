using EduOnline.Core.Mensagens.IntegrationEvents;
using EduOnline.Core.Mensagens.RabbitMq;
using MediatR;
using System.Diagnostics.CodeAnalysis;

namespace EduOnline.Alunos.ApiRest.BackgroundServices;

[ExcludeFromCodeCoverage]
public class PagamentoIntegrationEventsConsumerHostedService(IServiceProvider serviceProvider, IRabbitMqEventBus eventBus) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        SetSubscriber(stoppingToken);
        return Task.CompletedTask;
    }

    public void SetSubscriber(CancellationToken stoppingToken)
    {
        eventBus.SubscribeAsync<PagamentoRealizadoIntegrationEvent>(PagamentoRealizado, stoppingToken);
        eventBus.SubscribeAsync<PagamentoRecusadoIntegrationEvent>(PagamentoRecusado, stoppingToken);
    }

    public async Task PagamentoRealizado(PagamentoRealizadoIntegrationEvent message)
    {
        await using var scope = serviceProvider.CreateAsyncScope();

        var handler = scope.ServiceProvider.GetRequiredService<INotificationHandler<PagamentoRealizadoIntegrationEvent>>();

        await handler.Handle(message, CancellationToken.None);
    }

    public async Task PagamentoRecusado(PagamentoRecusadoIntegrationEvent message)
    {
        await using var scope = serviceProvider.CreateAsyncScope();

        var handler = scope.ServiceProvider.GetRequiredService<INotificationHandler<PagamentoRecusadoIntegrationEvent>>();

        await handler.Handle(message, CancellationToken.None);
    }
}
