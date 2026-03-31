using System.Diagnostics.CodeAnalysis;

namespace EduOnline.Auth.ApiRest.Configurations;

[ExcludeFromCodeCoverage]
public static class ApiConfig
{
    public static WebApplicationBuilder AddApiConfig(this WebApplicationBuilder builder)
    {
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        builder.Services.AddControllers();

        //builder.Services.AddCors(options =>
        //{
        //    options.AddPolicy("Total",
        //        builder =>
        //            builder
        //                .AllowAnyOrigin()
        //                .AllowAnyMethod()
        //                .AllowAnyHeader());
        //});

        return builder;
    }
}
