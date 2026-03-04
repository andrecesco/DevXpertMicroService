using EduOnline.Bff.ApiRest.Services;
using EduOnline.Core.Api.Extensions;
using EduOnline.Core.ControleDeAcesso;
using EduOnline.Core.Mensagens;

namespace EduOnline.WebApps.ApiRest.Configurations;

public static class DependencyInjectionConfig
{
    public static WebApplicationBuilder RegisterServices(this WebApplicationBuilder builder)
    {
        AddDependencies(builder);
        AddServices(builder);

        //builder.Services.AddTransient<HeaderPropagationHandler>();
        //builder.Services.AddAllHttpClientsWithHeaderPropagation<Service>(assemblyRef: typeof(Service).Assembly);

        return builder;
    }

    private static void AddDependencies(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        builder.Services.AddSingleton<IAspNetUser, AspNetUser>();
        builder.Services.AddScoped<INotificador, Notificador>();
    }

    private static void AddServices(WebApplicationBuilder builder)
    {
        builder.Services.AddHttpClient();
        builder.Services.AddHttpClientService<IAuthService, AuthService>();
        builder.Services.AddHttpClientService<IAlunoService, AlunoService>();
    }
}
