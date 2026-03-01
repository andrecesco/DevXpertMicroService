using EduOnline.Core.ControleDeAcesso;

namespace EduOnline.WebApps.ApiRest.Configurations;

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
        builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        builder.Services.AddSingleton<IAspNetUser, AspNetUser>();
    }

    private static void AddServices(WebApplicationBuilder builder)
    {

    }
}
