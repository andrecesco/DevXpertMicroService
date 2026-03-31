using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EduOnline.Core.Api.Extensions;

public static class CorsExtensions
{
    public static IServiceCollection AddDefaultCorsByEnvironment(this IServiceCollection services, IHostEnvironment environment, IConfiguration configuration)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("Total", policy =>
            {
                if (environment.IsDevelopment())
                {
                    policy
                        .AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();

                    return;
                }

                var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

                if (allowedOrigins.Length > 0)
                {
                    policy
                        .WithOrigins(allowedOrigins)
                        .AllowAnyMethod()
                        .AllowAnyHeader();

                    return;
                }

                policy
                    .SetIsOriginAllowed(_ => false)
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });

        return services;
    }
}
