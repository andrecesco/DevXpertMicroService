using EduOnline.Core.Api.Extensions;
using System.Diagnostics.CodeAnalysis;

namespace EduOnline.Pagamentos.ApiRest.Configurations;

[ExcludeFromCodeCoverage]
public static class ApiConfig
{
    public static WebApplicationBuilder AddApiConfig(this WebApplicationBuilder builder)
    {
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
            })
            .ConfigureApiBehaviorOptions(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });

        builder.Services.AddDefaultCorsByEnvironment(builder.Environment, builder.Configuration);

        return builder;
    }
}
