using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace EduOnline.Alunos.Application.Automapper;

public static class AutoMapperExtensionMethod
{
    public static void AddAutoMapperApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(cfg => { }, Assembly.GetExecutingAssembly());
    }
}
