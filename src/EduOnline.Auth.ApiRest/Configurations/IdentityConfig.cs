using EduOnline.Auth.ApiRest.Data;
using EduOnline.Auth.ApiRest.Extensions;
using Microsoft.AspNetCore.Identity;
using System.Diagnostics.CodeAnalysis;

namespace EduOnline.Auth.ApiRest.Configurations;

[ExcludeFromCodeCoverage]
public static class IdentityConfig
{
    public static WebApplicationBuilder AddIdentityConfig(this WebApplicationBuilder builder)
    {
        // 1. Configura e Valida as Opções de Token logo no Start
        builder.Services.AddOptions<AppTokenSettings>()
            .Bind(builder.Configuration.GetSection("AppTokenSettings"))
            .ValidateDataAnnotations() // Valida os atributos [Required], [MinLength], etc.
            .ValidateOnStart();       // Faz a API travar na subida se os dados estiverem errados

        // 2. Registra o Identity com as customizações brasileiras e stores
        builder.Services.AddDefaultIdentity<EduOnlineUser>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddErrorDescriber<IdentityMensagensPortugues>()
            .AddDefaultTokenProviders();

        return builder;
    }
}
