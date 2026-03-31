using EduOnline.Core.Mensagens.IntegrationEvents;
using EduOnline.Core.Mensagens.RabbitMq;
using MediatR;
using System.Diagnostics.CodeAnalysis;

namespace EduOnline.Alunos.ApiRest.BackgroundServices;

[ExcludeFromCodeCoverage]
public class PagamentoIntegrationEventsConsumerHostedService(IRabbitMqEventBus eventBus, IMediator mediator) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await eventBus.SubscribeAsync<PagamentoRealizadoEvent>(
            async integrationEvent => await mediator.Publish(integrationEvent, stoppingToken),
            stoppingToken);

        await eventBus.SubscribeAsync<PagamentoRecusadoEvent>(
            async integrationEvent => await mediator.Publish(integrationEvent, stoppingToken),
            stoppingToken);

        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (TaskCanceledException)
        {
        }
    }
}
