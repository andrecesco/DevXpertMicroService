using EduOnline.Alunos.Application.Automapper;
using EduOnline.Alunos.Application.Commands;
using EduOnline.Alunos.Application.Events;
using EduOnline.Alunos.Application.Queries;
using EduOnline.Alunos.Data.Context;
using EduOnline.Alunos.Data.Repository;
using EduOnline.Alunos.Domain.Interfaces;
using EduOnline.Alunos.ApiRest.Extensions;
using EduOnline.Core.Communication.Mediator;
using EduOnline.Core.ControleDeAcesso;
using EduOnline.Core.Mensagens.IntegrationEvents;
using EduOnline.Core.Mensagens.Notifications;
using MediatR;
using EduOnline.Core.Mensagens;

namespace EduOnline.Alunos.ApiRest.Configurations;

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
        builder.Services.AddScoped<AlunosContext>();
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
        builder.Services.AddScoped<IAlunoRepository, AlunoRepository>();
    }

    private static void AddServices(WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IAlunoQuery, AlunoQuery>();
        builder.Services.AddAutoMapperApplication();
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
        builder.Services.AddScoped<INotificationHandler<PagamentoRealizadoEvent>, MatriculaEventHandler>();
        builder.Services.AddScoped<INotificationHandler<PagamentoRecusadoEvent>, MatriculaEventHandler>();
    }
}
