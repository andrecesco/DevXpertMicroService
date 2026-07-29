using EduOnline.Core.ControleDeAcesso;
using EduOnline.Core.DomainObjects;
using EduOnline.Core.Mensagens;
using System.Security.Claims;

namespace EduOnline.WebApps.UnitTest.DomainObjects;

public class CoreDomainCoverageTest
{
    [Fact]
    public void GetUserId_ComPrincipalNulo_DeveLancarArgumentException()
    {
        Assert.Throws<ArgumentException>(() => ClaimsPrincipalExtensions.GetUserId(null!));
    }

    [Fact]
    public void GetUserId_SemClaim_DeveRetornarVazio()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity());

        var resultado = principal.GetUserId();

        Assert.Equal(string.Empty, resultado);
    }

    [Fact]
    public void GetUserEmail_ComPrincipalNulo_DeveLancarArgumentException()
    {
        Assert.Throws<ArgumentException>(() => ClaimsPrincipalExtensions.GetUserEmail(null!));
    }

    [Fact]
    public void GetUserEmail_ComClaim_DeveRetornarValor()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity([
            new Claim(ClaimTypes.Email, "aluno@eduonline.com")
        ]));

        var resultado = principal.GetUserEmail();

        Assert.Equal("aluno@eduonline.com", resultado);
    }

    [Fact]
    public void Entity_DeveGerenciarEventosEOperadoresCorretamente()
    {
        var entidade = new TestEntity();
        var evento = new TestEvent();
        var mesmaReferencia = entidade;
        var outraMesmaIdentidade = new TestEntity { Id = entidade.Id };
        var outraDiferente = new TestEntity();
        TestEntity? nula = null;

        entidade.AdicionarEvento(evento);
        Assert.Single(entidade.Notificacoes!);

        entidade.RemoverEvento(evento);
        Assert.Empty(entidade.Notificacoes!);

        entidade.AdicionarEvento(evento);
        entidade.LimparEventos();
        Assert.Empty(entidade.Notificacoes!);

        Assert.False(entidade.Equals(null));
        Assert.True(entidade.Equals(mesmaReferencia));
        Assert.True(entidade.Equals(outraMesmaIdentidade));
        Assert.False(entidade.Equals(outraDiferente));
        Assert.True(nula == null);
        Assert.False(entidade == null!);
        Assert.True(entidade == outraMesmaIdentidade);
        Assert.True(entidade != outraDiferente);
        Assert.Contains(entidade.Id.ToString(), entidade.ToString());
        Assert.NotEqual(0, entidade.GetHashCode());
    }

    [Fact]
    public void Enumerador_DeveResolverComparacaoBuscaEIgualdade()
    {
        var ativo = TestEnumerador.Ativo;
        var inativo = TestEnumerador.Inativo;

        var todos = Enumerador.GetAll<TestEnumerador>().ToList();

        Assert.Equal(2, todos.Count);
        Assert.Equal(ativo, Enumerador.GetById<TestEnumerador>(1));
        Assert.Equal(inativo, Enumerador.GetByNome<TestEnumerador>("inativo"));
        Assert.True(ativo.Equals(TestEnumerador.Ativo));
        Assert.False(ativo.Equals(inativo));
        Assert.True(ativo.Equals((object)TestEnumerador.Ativo));
        Assert.False(ativo.Equals(new object()));
        Assert.Equal("Ativo", ativo.ToString());
        Assert.Equal(1, ativo.CompareTo((object?)null));
        Assert.Equal(-1, ativo.CompareTo(inativo));
        Assert.Equal(1, inativo.CompareTo(ativo));
        Assert.Throws<ArgumentException>(() => ativo.CompareTo("inválido"));
        Assert.NotEqual(0, ativo.GetHashCode());
    }

    [Fact]
    public void Validacoes_DeveCobrirFluxosPrincipais()
    {
        Validacoes.ValidarSeIgual("A", "B", "erro");
        Assert.Throws<DomainException>(() => Validacoes.ValidarSeIgual("A", "A", "erro"));
        Assert.Throws<DomainException>(() => Validacoes.ValidarSeIgual(null, "A", "erro"));

        Validacoes.ValidarSeDiferente("A", "A", "erro");
        Assert.Throws<DomainException>(() => Validacoes.ValidarSeDiferente("A", "B", "erro"));

        Validacoes.ValidarSeDiferente("^abc$", "abc", "erro");
        Assert.Throws<DomainException>(() => Validacoes.ValidarSeDiferente("^abc$", "xyz", "erro"));

        Validacoes.ValidarTamanho("abc", 5, "erro");
        Assert.Throws<DomainException>(() => Validacoes.ValidarTamanho("abcdef", 5, "erro"));

        Validacoes.ValidarTamanho(" abc ", 2, 5, "erro");
        Assert.Throws<DomainException>(() => Validacoes.ValidarTamanho("a", 2, 5, "erro"));
        Assert.Throws<DomainException>(() => Validacoes.ValidarTamanho("abcdef", 2, 5, "erro"));

        Validacoes.ValidarSeVazio("abc", "erro");
        Assert.Throws<DomainException>(() => Validacoes.ValidarSeVazio(" ", "erro"));
        Assert.Throws<DomainException>(() => Validacoes.ValidarSeVazio(null, "erro"));

        Validacoes.ValidarSeNulo(new object(), "erro");
        Assert.Throws<DomainException>(() => Validacoes.ValidarSeNulo(null, "erro"));

        Validacoes.ValidarMinimoMaximo(5d, 1d, 10d, "erro");
        Assert.Throws<DomainException>(() => Validacoes.ValidarMinimoMaximo(11d, 1d, 10d, "erro"));

        Validacoes.ValidarMinimoMaximo(5f, 1f, 10f, "erro");
        Assert.Throws<DomainException>(() => Validacoes.ValidarMinimoMaximo(0.5f, 1f, 10f, "erro"));

        Validacoes.ValidarMinimoMaximo(5, 1, 10, "erro");
        Assert.Throws<DomainException>(() => Validacoes.ValidarMinimoMaximo((int?)null, 1, 10, "erro"));
        Assert.Throws<DomainException>(() => Validacoes.ValidarMinimoMaximo(11, 1, 10, "erro"));

        Validacoes.ValidarMinimoMaximo(5L, 1L, 10L, "erro");
        Assert.Throws<DomainException>(() => Validacoes.ValidarMinimoMaximo(0L, 1L, 10L, "erro"));

        Validacoes.ValidarMinimoMaximo(5m, 1m, 10m, "erro");
        Assert.Throws<DomainException>(() => Validacoes.ValidarMinimoMaximo(11m, 1m, 10m, "erro"));

        Validacoes.ValidarSeMenorQue(5L, 1L, "erro");
        Assert.Throws<DomainException>(() => Validacoes.ValidarSeMenorQue(0L, 1L, "erro"));

        Validacoes.ValidarSeMenorQue(5d, 1d, "erro");
        Assert.Throws<DomainException>(() => Validacoes.ValidarSeMenorQue(0.5d, 1d, "erro"));

        Validacoes.ValidarSeMenorQue(5m, 1m, "erro");
        Assert.Throws<DomainException>(() => Validacoes.ValidarSeMenorQue(0.5m, 1m, "erro"));

        Validacoes.ValidarSeMenorQue(new DateOnly(2026, 1, 2), new DateOnly(2026, 1, 1), "erro");
        Assert.Throws<DomainException>(() => Validacoes.ValidarSeMenorQue(new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 2), "erro"));

        Validacoes.ValidarSeMaiorQue(new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 2), "erro");
        Assert.Throws<DomainException>(() => Validacoes.ValidarSeMaiorQue(new DateOnly(2026, 1, 3), new DateOnly(2026, 1, 2), "erro"));

        Validacoes.ValidarSeMenorQue(2, 1, "erro");
        Assert.Throws<DomainException>(() => Validacoes.ValidarSeMenorQue((int?)null, 1, "erro"));
        Assert.Throws<DomainException>(() => Validacoes.ValidarSeMenorQue(0, 1, "erro"));

        Validacoes.ValidarSeMenorOuIgualQue(2, 1, "erro");
        Assert.Throws<DomainException>(() => Validacoes.ValidarSeMenorOuIgualQue((int?)null, 1, "erro"));
        Assert.Throws<DomainException>(() => Validacoes.ValidarSeMenorOuIgualQue(1, 1, "erro"));

        Validacoes.ValidarSeFalso(true, "erro");
        Assert.Throws<DomainException>(() => Validacoes.ValidarSeFalso(false, "erro"));

        Validacoes.ValidarSeVerdadeiro(false, "erro");
        Assert.Throws<DomainException>(() => Validacoes.ValidarSeVerdadeiro(true, "erro"));

        Validacoes.ValidarEnumerador<TestEnumerador>(1, "erro");
        Assert.Throws<DomainException>(() => Validacoes.ValidarEnumerador<TestEnumerador>(null, "erro"));
        Assert.Throws<DomainException>(() => Validacoes.ValidarEnumerador<TestEnumerador>(99, "erro"));
    }

    private sealed class TestEntity : Entity;

    private sealed class TestEvent : Event;

    private sealed class TestEnumerador(int id, string nome) : Enumerador(id, nome)
    {
        public static TestEnumerador Ativo => new(1, "Ativo");
        public static TestEnumerador Inativo => new(2, "Inativo");
    }
}
