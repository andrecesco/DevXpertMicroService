using Bogus;
using EduOnline.Alunos.Domain.ValueObjects;
using EduOnline.Core.DomainObjects;

namespace EduOnline.Alunos.UnitTest;

public class HistoricoAprendizagemTest
{
    private static Faker<HistoricoAprendizagem> GerarHistorico(int totalAulas = 10, int concluidas = 5)
    {
        var aulasConcluidas = Enumerable.Range(0, concluidas)
            .Select(_ => Guid.NewGuid())
            .ToArray();

        return new Faker<HistoricoAprendizagem>("pt_BR")
            .CustomInstantiator(f => new HistoricoAprendizagem(totalAulas, aulasConcluidas));
    }

    [Fact(DisplayName = "Criar objeto de HistoricoAprendizagem deve retornar exceptions")]
    [Trait("Categoria", "HistoricoAprendizagem Testes")]
    public void HistoricoAprendizagem_Validar_DeveRetornarDomainException()
    {
        // TotalAulas menor ou igual a zero
        var exception = Assert.Throws<DomainException>(() =>
            new HistoricoAprendizagem(-10, []).Validar());
        Assert.Equal("O campo Total de Aulas deve ter no mínimo 0 e no máximo 1000", exception.Message);

        // AulasConcluidas maior que TotalAulas
        var aulasConcluidas = Enumerable.Range(0, 6).Select(_ => Guid.NewGuid()).ToArray();
        exception = Assert.Throws<DomainException>(() =>
            new HistoricoAprendizagem(5, aulasConcluidas).Validar());
        Assert.Equal("O campo Aulas Concluídas não pode ser maior que o Total de Aulas", exception.Message);
    }

    [Fact(DisplayName = "Criar objeto de HistoricoAprendizagem com sucesso")]
    [Trait("Categoria", "HistoricoAprendizagem Testes")]
    public void HistoricoAprendizagem_Validar_NaoDeveRetornarException()
    {
        // Arrange
        var historicoFaker = GerarHistorico(10, 5);

        // Act
        var historico = historicoFaker.Generate();

        // Assert
        historico.Validar();
        Assert.Equal(10, historico.TotalAulas);
        Assert.Equal(5, historico.AulasConcluidas.Length);
    }

    [Fact(DisplayName = "Adicionar aula concluída deve incrementar a lista")]
    [Trait("Categoria", "HistoricoAprendizagem Testes")]
    public void HistoricoAprendizagem_AdicionarAulaConcluida_DeveIncrementarLista()
    {
        // Arrange
        var historico = new HistoricoAprendizagem(3, []);
        var aulaId = Guid.NewGuid();

        // Act
        historico.AdicionarAulaConcluida(aulaId);

        // Assert
        Assert.Single(historico.AulasConcluidas);
        Assert.Contains(aulaId, historico.AulasConcluidas);
    }

    [Fact(DisplayName = "Adicionar aula já concluída não deve duplicar")]
    [Trait("Categoria", "HistoricoAprendizagem Testes")]
    public void HistoricoAprendizagem_AdicionarAulaConcluida_DeveRetornarException()
    {
        // Arrange
        var aulaId = Guid.NewGuid();
        var historico = new HistoricoAprendizagem(3, [aulaId]);

        // Act
        var exception = Assert.Throws<DomainException>(() => historico.AdicionarAulaConcluida(aulaId));

        // Assert
        Assert.Equal("Aula já concluída", exception.Message);
    }
}
