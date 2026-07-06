using EduOnline.Conteudos.ApiRest.Services;
using EduOnline.Conteudos.Data.Context;
using EduOnline.Conteudos.Data.Repository;
using EduOnline.Conteudos.Domain;
using EduOnline.Conteudos.Domain.Services;
using EduOnline.Core.Api.Extensions;
using EduOnline.Core.Communication.Mediator;
using EduOnline.Core.ControleDeAcesso;
using EduOnline.Core.Data.EventSourcing;
using EduOnline.Core.Mensagens;
using EduOnline.Core.Mensagens.Notifications;
using EventSourcing;
using MediatR;
using System.Diagnostics.CodeAnalysis;

namespace EduOnline.Conteudos.ApiRest.Configurations;

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
        builder.Services.AddScoped<ICursoService, CursoService>();
        builder.Services.AddHttpClientService<IAlunoProgressIntegrationService, AlunoProgressIntegrationService>();
        builder.Services.AddSingleton<IEventStoreService, EventStoreService>();
        builder.Services.AddSingleton<IEventSourcingRepository, EventSourcingRepository>();
    }
}
