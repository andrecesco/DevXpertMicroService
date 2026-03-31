using EduOnline.Bff.ApiRest.Services;
using EduOnline.Core.Api.Extensions;
using EduOnline.Core.ControleDeAcesso;
using EduOnline.Core.Mensagens;
using System.Diagnostics.CodeAnalysis;

namespace EduOnline.WebApps.ApiRest.Configurations;

[ExcludeFromCodeCoverage]
public static class DependencyInjectionConfig
{
    public static WebApplicationBuilder RegisterServices(this WebApplicationBuilder builder)
    {
        AddDependencies(builder);
        AddServices(builder);

        return builder;
    }

    private static void AddDependencies(this WebApplicationBuilder builder)
    {
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<IAspNetUser, AspNetUser>();
        builder.Services.AddScoped<INotificador, Notificador>();
    }

    private static void AddServices(WebApplicationBuilder builder)
    {
        builder.Services.AddHttpClient();
        builder.Services.AddHttpClientService<IAuthService, AuthService>();
        builder.Services.AddHttpClientService<IAlunoService, AlunoService>();
        builder.Services.AddHttpClientService<IConteudoService, ConteudoService>();
        builder.Services.AddHttpClientService<IPagamentoBffService, PagamentoBffService>();
    }
}
