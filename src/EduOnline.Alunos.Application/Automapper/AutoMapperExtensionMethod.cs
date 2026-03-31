using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Diagnostics.CodeAnalysis;

namespace EduOnline.Alunos.Application.Automapper;

[ExcludeFromCodeCoverage]
public static class AutoMapperExtensionMethod
{
    public static void AddAutoMapperApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(cfg => { }, Assembly.GetExecutingAssembly());
    }
}
