using Microsoft.OpenApi;
using System.Diagnostics.CodeAnalysis;

namespace EduOnline.Conteudos.ApiRest.Configurations;

[ExcludeFromCodeCoverage]
public static class SwaggerConfig
{
    /// <summary>   
    /// Configuração do Swagger para autenticação JWT
    /// </summary>
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
                    Title = "EduOnline - API de Gestão de Cursos (Conteúdo)",
                    Version = "v1",
                    Description = "API responsável pela gestão de cursos"
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
