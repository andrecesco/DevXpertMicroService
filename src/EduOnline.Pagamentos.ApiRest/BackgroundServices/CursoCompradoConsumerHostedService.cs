using EduOnline.Core.Mensagens.IntegrationEvents;
using EduOnline.Core.Mensagens.RabbitMq;
using MediatR;
using System.Diagnostics.CodeAnalysis;

namespace EduOnline.Pagamentos.ApiRest.BackgroundServices;

[ExcludeFromCodeCoverage]
public class CursoCompradoConsumerHostedService(IRabbitMqEventBus eventBus, IMediator mediator) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await eventBus.SubscribeAsync<CursoCompradoIntegrationEvent>(
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
