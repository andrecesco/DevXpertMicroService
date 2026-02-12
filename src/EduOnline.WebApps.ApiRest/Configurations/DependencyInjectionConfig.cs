using EduOnline.Alunos.Application.Commands;
using EduOnline.Alunos.Application.Events;
using EduOnline.Alunos.Application.Queries;
using EduOnline.Alunos.Data.Context;
using EduOnline.Alunos.Data.Repository;
using EduOnline.Alunos.Domain.Interfaces;
using EduOnline.Conteudos.Data.Context;
using EduOnline.Conteudos.Data.Repository;
using EduOnline.Conteudos.Domain;
using EduOnline.Conteudos.Domain.Services;
using EduOnline.Core.Communication.Mediator;
using EduOnline.Core.ControleDeAcesso;
using EduOnline.Core.Data.EventSourcing;
using EduOnline.Core.Mensagens;
using EduOnline.Core.Mensagens.IntegrationEvents;
using EduOnline.Core.Mensagens.Notifications;
using EduOnline.Pagamentos.AntiCorruption;
using EduOnline.Pagamentos.Data;
using EduOnline.Pagamentos.Domain;
using EduOnline.Pagamentos.Domain.Events;
using EduOnline.WebApps.ApiRest.Data;
using EduOnline.WebApps.ApiRest.Extensions;
using EventSourcing;
using MediatR;

namespace EduOnline.WebApps.ApiRest.Configurations;

public static class DependencyInjectionConfig
{
    public static WebApplicationBuilder RegisterServices(this WebApplicationBuilder builder)
    {
        AddNotificators(builder);
        AddContexts(builder);
        AddRepositories(builder);
        AddServices(builder);
        AddRequestHandlers(builder);

        return builder;
    }

    private static void AddContexts(WebApplicationBuilder builder)
    {
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<ApplicationDbContext>();
        builder.Services.AddScoped<PagamentosContext>();
        builder.Services.AddScoped<ConteudosContext>();
        builder.Services.AddScoped<AlunosContext>();
        builder.Services.AddScoped<IUser, AspNetUser>();
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
        builder.Services.AddScoped<IAlunoRepository, AlunoRepository>();
        builder.Services.AddScoped<IPagamentoRepository, PagamentoRepository>();
    }

    private static void AddServices(WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<ICursoService, CursoService>();
        builder.Services.AddScoped<IAlunoQuery, AlunoQuery>();

        builder.Services.AddSingleton<IEventStoreService, EventStoreService>();
        builder.Services.AddSingleton<IEventSourcingRepository, EventSourcingRepository>();
    }

    private static void AddRequestHandlers(WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IRequestHandler<AdicionarAlunoCommand, bool>, AlunoCommandHandler>();
        builder.Services.AddScoped<IRequestHandler<AlterarAlunoCommand, bool>, AlunoCommandHandler>();
        builder.Services.AddScoped<IRequestHandler<AdicionarMatriculaCommand, bool>, AlunoCommandHandler>();
        builder.Services.AddScoped<IRequestHandler<AtualizarHistoricoCommand, bool>, AlunoCommandHandler>();
        builder.Services.AddScoped<IRequestHandler<GerarCertificadoCommand, bool>, AlunoCommandHandler>();
        builder.Services.AddScoped<IRequestHandler<MatriculaPagaCommand, bool>, AlunoCommandHandler>();
        builder.Services.AddScoped<IRequestHandler<MatriculaRecusadaCommand, bool>, AlunoCommandHandler>();

        builder.Services.AddScoped<INotificationHandler<CursoFinalizadoEvent>, MatriculaEventHandler>();
        builder.Services.AddScoped<INotificationHandler<CursoCompradoIntegrationEvent>, PagamentoEventHandler>();
        builder.Services.AddScoped<INotificationHandler<PagamentoRealizadoEvent>, MatriculaEventHandler>();
        builder.Services.AddScoped<INotificationHandler<PagamentoRecusadoEvent>, MatriculaEventHandler>();

        builder.Services.AddScoped<IPagamentoService, PagamentoService>();
        builder.Services.AddScoped<IPagamentoCartaoCreditoFacade, PagamentoCartaoCreditoFacade>();
        builder.Services.AddScoped<IPayPalGateway, PayPalGateway>();
        builder.Services.AddScoped<Pagamentos.AntiCorruption.IConfigurationManager, Pagamentos.AntiCorruption.ConfigurationManager>();
    }
}
