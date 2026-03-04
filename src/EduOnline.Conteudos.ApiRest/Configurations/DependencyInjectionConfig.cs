using EduOnline.Conteudos.Data.Context;
using EduOnline.Conteudos.Data.Repository;
using EduOnline.Conteudos.Domain;
using EduOnline.Core.Communication.Mediator;
using EduOnline.Core.ControleDeAcesso;
using EduOnline.Core.Data.EventSourcing;
using EduOnline.Core.Mensagens;
using EduOnline.Core.Mensagens.Notifications;
using EventSourcing;
using MediatR;

namespace EduOnline.Conteudos.ApiRest.Configurations;

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
        builder.Services.AddScoped<ConteudosContext>();
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
        builder.Services.AddScoped<ICursoRepository, CursoRepository>();
    }

    private static void AddServices(WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IEventStoreService, EventStoreService>();
        builder.Services.AddSingleton<IEventSourcingRepository, EventSourcingRepository>();
    }
}
