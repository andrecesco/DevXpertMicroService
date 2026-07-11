using EduOnline.Api.Status;
using HealthChecks.UI.Client;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLogging();

builder.Services.AddHealthChecks();

var healthCheckBuilder = builder.Services.AddHealthChecksUI(setup =>
{
    setup.SetHeaderText("EduOnline - Status Page");

    var endpoints = builder.Configuration.GetSection("ENDPOINTS").Get<string>() ?? throw (new Exception("Nenhum endpoint encontrado!"));

    foreach (var endpoint in endpoints.Split(";"))
    {
        var name = endpoint.Split('|')[0];
        var uri = endpoint.Split('|')[1];

        setup.AddHealthCheckEndpoint(name, uri);
    }

    setup.UseApiEndpointHttpMessageHandler(sp => HttpExtensions.ConfigureClientHandler());
});

healthCheckBuilder.AddInMemoryStorage();
//var (database, connString) = DetectDatabase(builder.Configuration);

//switch (database)
//{
//    case DatabaseType.None:
//        break;
//    case DatabaseType.SqlServer:
//        healthCheckBuilder.AddSqlServerStorage(connString);
//        break;
//    case DatabaseType.MySql:
//        healthCheckBuilder.AddMySqlStorage(connString);
//        break;
//    case DatabaseType.Postgre:
//        healthCheckBuilder.AddPostgreSqlStorage(connString);
//        break;
//    case DatabaseType.Sqlite:
//        healthCheckBuilder.AddSqliteStorage(connString);
//        break;
//    default:
//        healthCheckBuilder.AddInMemoryStorage();
//        break;
//}

var app = builder.Build();

// Under certain scenarios, e.g minikube / linux environment / behind load balancer
// https redirection could lead dev's to over complicated configuration for testing purpouses
// In production is a good practice to keep it true
if (app.Configuration["USE_HTTPS_REDIRECTION"] == "true")
{
    app.UseHttpsRedirection();
    app.UseHsts();
}

app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

// Under certain scenarios, e.g minikube / linux environment / behind load balancer
// https redirection could lead dev's to over complicated configuration for testing purpouses
// In production is a good practice to keep it true
if (app.Configuration["USE_HTTPS_REDIRECTION"] == "true")
{
    app.UseHttpsRedirection();
    app.UseHsts();
}

app.MapHealthChecksUI(setup =>
{
    setup.AddCustomStylesheet("devstore.css");
    setup.UIPath = "/";
    setup.PageTitle = "EduOnline - Status";
});
app.Run();


//var builder = WebApplication.CreateBuilder(args);

// 1. ADICIONE ESTA LINHA para registrar os serviços necessários
//builder.Services.AddHealthChecks();

// Se estiver usando a interface gráfica do Health Checks, adicione também:
//builder.Services.AddHealthChecksUI(setupSettings =>
//{
//     Monitorar a própria API local
//    setupSettings.AddHealthCheckEndpoint("BFF", "https://localhost:7098/health");

//     Você pode adicionar outras APIs ou Microserviços aqui:
//     setupSettings.AddHealthCheckEndpoint("API de Vendas", "https://api-vendas/health");
//})
//    .AddInMemoryStorage();

//var app = builder.Build();

// 2. Mapeie o endpoint utilizando o formatador JSON do UI
//app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
//{
//    Predicate = _ => true,
//    ResponseWriter = HealthChecks.UI.Client.UIResponseWriter.WriteHealthCheckUIResponse
//});

// Opcional: Mapeia a interface visual (/healthchecks-ui)
//app.MapHealthChecksUI();

//app.Run();
