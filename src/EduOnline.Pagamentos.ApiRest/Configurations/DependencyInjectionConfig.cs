using System.Diagnostics.CodeAnalysis;
using EduOnline.Core.Communication.Mediator;
using EduOnline.Core.ControleDeAcesso;
using EduOnline.Core.Data.EventSourcing;
using EduOnline.Core.Mensagens;
using EduOnline.Core.Mensagens.Notifications;
using EduOnline.Core.Mensagens.RabbitMq;
using EduOnline.Pagamentos.AntiCorruption;
using EduOnline.Pagamentos.ApiRest.BackgroundServices;
using EduOnline.Pagamentos.Data;
using EduOnline.Pagamentos.Domain;
using EventSourcing;
using MediatR;

namespace EduOnline.Pagamentos.ApiRest.Configurations;

[ExcludeFromCodeCoverage]
public static class DependencyInjectionConfig
{
    public static WebApplicationBuilder RegisterServices(this WebApplicationBuilder builder)
    {
        AddNotificators(builder);
        AddContexts(builder);
        AddRepositories(builder);
        AddServices(builder);

        return builder;
    }

    private static void AddContexts(WebApplicationBuilder builder)
    {
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<PagamentosContext>();
        builder.Services.AddScoped<IAspNetUser, AspNetUser>();
    }

    private static void AddNotificators(WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IMediatorHandler, MediatorHandler>();
        builder.Services.AddScoped<INotificationHandler<DomainNotification>, DomainNotificationHandler>();
        builder.Services.AddScoped<INotificador, Notificador>();
    }

    private static void AddRepositories(WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IPagamentoRepository, PagamentoRepository>();
    }

    private static void AddServices(WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IPayPalGateway, PayPalGateway>();
        builder.Services.AddScoped<EduOnline.Pagamentos.AntiCorruption.IConfigurationManager, EduOnline.Pagamentos.AntiCorruption.ConfigurationManager>();
        builder.Services.AddScoped<IPagamentoCartaoCreditoFacade, PagamentoCartaoCreditoFacade>();
        builder.Services.AddScoped<IPagamentoService, PagamentoService>();

        builder.Services.AddSingleton<IEventStoreService, EventStoreService>();
        builder.Services.AddSingleton<IEventSourcingRepository, EventSourcingRepository>();

        builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection("RabbitMq"));
        builder.Services.AddSingleton<IRabbitMqEventBus, RabbitMqEventBus>();
        builder.Services.AddHostedService<CursoCompradoConsumerHostedService>();
    }
}
