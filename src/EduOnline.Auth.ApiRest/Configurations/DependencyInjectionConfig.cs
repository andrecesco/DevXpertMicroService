using EduOnline.Auth.ApiRest.Data;
using EduOnline.Auth.ApiRest.Services;
using EduOnline.Core.ControleDeAcesso;
using EduOnline.Core.Mensagens;
using System.Diagnostics.CodeAnalysis;

namespace EduOnline.Auth.ApiRest.Configurations;

[ExcludeFromCodeCoverage]
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
        builder.Services.AddScoped<ApplicationDbContext>();
        builder.Services.AddScoped<AuthenticationService>();
        builder.Services.AddScoped<IAspNetUser, AspNetUser>();
    }

    private static void AddNotificators(WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<INotificador, Notificador>();
    }
}
