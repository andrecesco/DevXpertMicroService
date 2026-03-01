using EduOnline.Auth.ApiRest.Data;
using EduOnline.Auth.ApiRest.Extensions;
using EduOnline.Core.ControleDeAcesso;
using EduOnline.Core.Mensagens;
using EduOnline.Core.Mensagens.Notifications;
using MediatR;

namespace EduOnline.Auth.ApiRest.Configurations;

public static class DependencyInjectionConfig
{
    public static WebApplicationBuilder RegisterServices(this WebApplicationBuilder builder)
    {
        AddNotificators(builder);
        AddContexts(builder);

        return builder;
    }

    private static void AddContexts(WebApplicationBuilder builder)
    {
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<ApplicationDbContext>();
        builder.Services.AddScoped<IAspNetUser, AspNetUser>();
    }

    private static void AddNotificators(WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<INotificador, Notificador>();
    }
}
