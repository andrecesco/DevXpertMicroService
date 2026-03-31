using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace EduOnline.Alunos.Application.Automapper;

[ExcludeFromCodeCoverage]
public static class AutoMapperExtensionMethod
{
    public static void AddAutoMapperApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(cfg => { }, Assembly.GetExecutingAssembly());
    }
}
