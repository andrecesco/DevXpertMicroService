using EduOnline.Core.Communication.Mediator;
using EduOnline.Core.Data.EventSourcing;
using EduOnline.Core.Mensagens;
using EduOnline.Core.Mensagens.DomainEvents;
using EduOnline.Core.Mensagens.IntegrationEvents;
using EduOnline.Core.Mensagens.Notifications;
using EduOnline.Core.Mensagens.RabbitMq;
using MediatR;
using Moq;

namespace EduOnline.WebApps.UnitTest.Communication;

public class MediatorHandlerCoverageTest
{
    [Fact]
    public async Task EnviarComando_DeveRetornarResultadoDoMediator()
    {
        var mediator = new Mock<IMediator>();
        mediator.Setup(m => m.Send(It.IsAny<TestCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var eventSourcing = new Mock<IEventSourcingRepository>();
        var sut = new MediatorHandler(mediator.Object, eventSourcing.Object);

        var resultado = await sut.EnviarComando(new TestCommand());

        Assert.True(resultado);
    }

    [Fact]
    public async Task PublicarEvento_DeDominio_DevePublicarESalvarSemRabbitMq()
    {
        var mediator = new Mock<IMediator>();
        var eventSourcing = new Mock<IEventSourcingRepository>();
        var rabbitMq = new Mock<IRabbitMqEventBus>();
        var sut = new MediatorHandler(mediator.Object, eventSourcing.Object, rabbitMq.Object);
        var evento = new TestEvent();

        await sut.PublicarEvento(evento);

        mediator.Verify(m => m.Publish(evento, It.IsAny<CancellationToken>()), Times.Once);
        eventSourcing.Verify(r => r.SalvarEvento(evento), Times.Once);
        rabbitMq.Verify(r => r.PublishAsync(It.IsAny<IntegrationEvent>()), Times.Never);
    }

    [Fact]
    public async Task PublicarEvento_DeIntegracao_ComRabbitMq_DevePublicarEmTodosOsCanais()
    {
        var mediator = new Mock<IMediator>();
        var eventSourcing = new Mock<IEventSourcingRepository>();
        var rabbitMq = new Mock<IRabbitMqEventBus>();
        var sut = new MediatorHandler(mediator.Object, eventSourcing.Object, rabbitMq.Object);
        var evento = new TestIntegrationEvent(Guid.NewGuid());

        await sut.PublicarEvento(evento);

        mediator.Verify(m => m.Publish(evento, It.IsAny<CancellationToken>()), Times.Once);
        eventSourcing.Verify(r => r.SalvarEvento(evento), Times.Once);
        rabbitMq.Verify(r => r.PublishAsync(It.Is<IntegrationEvent>(e => ReferenceEquals(e, evento))), Times.Once);
    }

    [Fact]
    public async Task PublicarNotificacao_DeveEncaminharAoMediator()
    {
        var mediator = new Mock<IMediator>();
        var eventSourcing = new Mock<IEventSourcingRepository>();
        var sut = new MediatorHandler(mediator.Object, eventSourcing.Object);
        var notificacao = new DomainNotification("campo", "erro");

        await sut.PublicarNotificacao(notificacao);

        mediator.Verify(m => m.Publish(notificacao, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PublicarDomainEvent_DeveEncaminharAoMediator()
    {
        var mediator = new Mock<IMediator>();
        var eventSourcing = new Mock<IEventSourcingRepository>();
        var sut = new MediatorHandler(mediator.Object, eventSourcing.Object);
        var evento = new TestDomainEvent(Guid.NewGuid());

        await sut.PublicarDomainEvent(evento);

        mediator.Verify(m => m.Publish(evento, It.IsAny<CancellationToken>()), Times.Once);
    }

    private sealed class TestCommand : Command
    {
        public override bool EhValido() => true;
    }

    private sealed class TestEvent : Event
    {
        public TestEvent()
        {
            AggregateId = Guid.NewGuid();
        }
    }

    private sealed class TestIntegrationEvent : IntegrationEvent
    {
        public TestIntegrationEvent(Guid aggregateId)
        {
            AggregateId = aggregateId;
        }
    }

    private sealed class TestDomainEvent(Guid aggregateId) : DomainEvent(aggregateId);
}
