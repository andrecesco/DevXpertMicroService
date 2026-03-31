using Microsoft.OpenApi;
using System.Diagnostics.CodeAnalysis;

namespace EduOnline.WebApps.ApiRest.Configurations;

[ExcludeFromCodeCoverage]
public static class SwaggerConfig
{
    /// <summary>   
    /// Configuração do Swagger para autenticação JWT
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static WebApplicationBuilder AddSwaggerConfig(this WebApplicationBuilder builder)
    {
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer((document, context, cancellationToken) =>
            {
                // Informações da API
                document.Info = new OpenApiInfo
                {
                    Title = "EduOnline - API de Integração",
                    Version = "v1",
                    Description = "API responsável pela integração (BFF) de serviços"
                };

                // Configuração do esquema de segurança Bearer
                var securityScheme = new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Description = "Cole apenas o token JWT (sem prefixo Bearer)",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT"
                };

                document.Components ??= new OpenApiComponents();
                document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
                document.Components.SecuritySchemes["Bearer"] = securityScheme;

                var securityRequirement = new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecuritySchemeReference("Bearer",document),
                        new List<string>()
                    }
                };

                document.Security ??= new List<OpenApiSecurityRequirement>();
                document.Security.Add(securityRequirement);

                return Task.CompletedTask;
            });
        });

        return builder;
    }
}
