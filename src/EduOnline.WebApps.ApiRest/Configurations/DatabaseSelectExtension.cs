using EduOnline.Alunos.Data.Context;
using EduOnline.Conteudos.Data.Context;
using EduOnline.Pagamentos.Data;
using EduOnline.WebApps.ApiRest.Data;
using Microsoft.EntityFrameworkCore;

namespace EduOnline.WebApps.ApiRest.Configurations;

public static class DatabaseSelectExtension
{
    public static WebApplicationBuilder AddDatabaseSelector(this WebApplicationBuilder builder)
    {
        //var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        //builder.Services.AddDbContext<ApplicationDbContext>(options =>
        //    options.UseSqlServer(connectionString));

        //builder.Services.AddDbContext<ConteudosContext>(options =>
        //options.UseSqlServer(connectionString));

        //builder.Services.AddDbContext<AlunosContext>(options =>
        //options.UseSqlServer(connectionString));

        //builder.Services.AddDbContext<PagamentosContext>(options =>
        //options.UseSqlServer(connectionString));

        if (builder.Environment.IsDevelopment())
        {
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnectionLite") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite(connectionString));
            builder.Services.AddDbContext<ConteudosContext>(options =>
            options.UseSqlite(connectionString));
            builder.Services.AddDbContext<AlunosContext>(options =>
            options.UseSqlite(connectionString));
            builder.Services.AddDbContext<PagamentosContext>(options =>
            options.UseSqlite(connectionString));
        }
        else
        {
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));
            builder.Services.AddDbContext<ConteudosContext>(options =>
            options.UseSqlServer(connectionString));
            builder.Services.AddDbContext<AlunosContext>(options =>
            options.UseSqlServer(connectionString));
            builder.Services.AddDbContext<PagamentosContext>(options =>
            options.UseSqlServer(connectionString));
        }

        return builder;
    }
}
