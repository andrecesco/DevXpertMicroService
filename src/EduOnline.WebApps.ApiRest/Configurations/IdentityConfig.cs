using EduOnline.WebApps.ApiRest.Data;
using EduOnline.WebApps.ApiRest.Extensions;
using Microsoft.AspNetCore.Identity;

namespace EduOnline.WebApps.ApiRest.Configurations;

public static class IdentityConfig
{
    public static WebApplicationBuilder AddIdentityConfig(this WebApplicationBuilder builder)
    {
        builder.Services.AddDefaultIdentity<IdentityUser>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddErrorDescriber<IdentityMensagensPortugues>()
            .AddDefaultTokenProviders();

        return builder;
    }
}
