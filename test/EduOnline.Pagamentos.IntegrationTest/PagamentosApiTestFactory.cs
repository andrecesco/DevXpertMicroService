using EduOnline.Core.Data.EventSourcing;
using EduOnline.Core.Mensagens;
using EduOnline.Pagamentos.Data;
using EduOnline.Pagamentos.Domain;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EduOnline.Pagamentos.IntegrationTest;

public class PagamentosApiTestFactory : WebApplicationFactory<Program>
{
    private const string Issuer = "https://localhost:7020";
    private const string Audience = "EduOnline-Dev";
    private const string Secret = "ChaveSuperSecretaParaJWT_2024_EduOnline_MinhaChaveDeve_TerMaisde32Caracteres!@#$%^&*";

    public Guid Aluno1Id { get; } = Guid.NewGuid();
    public Guid Aluno2Id { get; } = Guid.NewGuid();
    public Guid PagamentoAluno1Id { get; } = Guid.NewGuid();
    public Guid PagamentoAluno2Id { get; } = Guid.NewGuid();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var dbFile = Path.Combine(Path.GetTempPath(), $"pagamentos-it-{Guid.NewGuid():N}.db");

        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnectionLite"] = $"Data Source={dbFile}",
                ["AppTokenSettings:Issuer"] = Issuer,
                ["AppTokenSettings:Audience"] = Audience,
                ["AppTokenSettings:Segredo"] = Secret,
                ["RabbitMq:Enabled"] = "false"
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IPagamentoCartaoCreditoFacade>();
            services.AddScoped<IPagamentoCartaoCreditoFacade, PagamentoCartaoCreditoFacadeFake>();
            services.RemoveAll<IEventSourcingRepository>();
            services.AddSingleton<IEventSourcingRepository, NoOpEventSourcingRepository>();

            using var scope = services.BuildServiceProvider().CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<PagamentosContext>();

            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            Seed(context);
        });
    }

    public string GerarTokenAluno(Guid alunoId) => GerarToken(alunoId, "Aluno");

    public string GerarTokenAdmin(Guid adminId) => GerarToken(adminId, "Administrador");

    private static string GerarToken(Guid userId, string role)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Email, $"{userId:N}@eduonline.com"),
            new(ClaimTypes.Role, role),
            new("role", role)
        };

        var token = new JwtSecurityToken(
            issuer: Issuer,
            audience: Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private void Seed(PagamentosContext context)
    {
        var pagamento1 = new Pagamento
        {
            Id = PagamentoAluno1Id,
            AlunoId = Aluno1Id,
            CursoId = Guid.NewGuid(),
            Total = 120.50m,
            NomeCartao = "Aluno Um",
            NumeroCartao = "4111111111111111",
            ExpiracaoCartao = "12/30",
            CvvCartao = "123"
        };

        var pagamento2 = new Pagamento
        {
            Id = PagamentoAluno2Id,
            AlunoId = Aluno2Id,
            CursoId = Guid.NewGuid(),
            Total = 250.00m,
            NomeCartao = "Aluno Dois",
            NumeroCartao = "5555555555554444",
            ExpiracaoCartao = "11/31",
            CvvCartao = "456"
        };

        var transacao1 = new Transacao
        {
            Id = Guid.NewGuid(),
            PagamentoId = PagamentoAluno1Id,
            Total = pagamento1.Total,
            StatusTransacaoId = StatusTransacao.Aprovado.Id
        };

        var transacao2 = new Transacao
        {
            Id = Guid.NewGuid(),
            PagamentoId = PagamentoAluno2Id,
            Total = pagamento2.Total,
            StatusTransacaoId = StatusTransacao.Recusado.Id
        };

        context.Pagamentos.AddRange(pagamento1, pagamento2);
        context.Transacoes.AddRange(transacao1, transacao2);
        context.SaveChanges();
    }

    private sealed class PagamentoCartaoCreditoFacadeFake : IPagamentoCartaoCreditoFacade
    {
        public Transacao RealizarPagamento(EduOnline.Pagamentos.Domain.Curso curso, Pagamento pagamento)
        {
            return new Transacao
            {
                Id = Guid.NewGuid(),
                PagamentoId = pagamento.Id,
                Total = curso.Valor,
                StatusTransacaoId = StatusTransacao.Aprovado.Id
            };
        }
    }

    private sealed class NoOpEventSourcingRepository : IEventSourcingRepository
    {
        public Task SalvarEvento<TEvent>(TEvent evento) where TEvent : Event
            => Task.CompletedTask;

        public Task<IEnumerable<StoredEvent>> ObterEventos(Guid aggregateId)
            => Task.FromResult<IEnumerable<StoredEvent>>([]);
    }

    public async Task<int> ContarPagamentosAsync()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PagamentosContext>();
        return await context.Pagamentos.CountAsync();
    }

    public async Task<Pagamento?> ObterPagamentoPorAlunoETotalAsync(Guid alunoId, decimal total)
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PagamentosContext>();

        return await context.Pagamentos
            .Include(p => p.Transacao)
            .OrderByDescending(p => p.Id)
            .FirstOrDefaultAsync(p => p.AlunoId == alunoId && p.Total == total);
    }
}
