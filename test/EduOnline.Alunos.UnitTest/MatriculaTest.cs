using Bogus;
using EduOnline.Alunos.Domain.Models;
using EduOnline.Core.DomainObjects;

namespace EduOnline.Alunos.UnitTest;

public class MatriculaTest
{
    private static Faker<Matricula> GerarMatricula(Guid? alunoId = null)
    {
        return new Faker<Matricula>("pt_BR")
            .CustomInstantiator(f => new Matricula(
                f.Random.Guid(),
                alunoId ?? f.Random.Guid(),
                f.Random.Guid(),
                f.Lorem.Sentence(3),
                f.Date.FutureDateOnly()
            ));
    }

    [Fact(DisplayName = "Criar objeto de Matricula deve retornar exceptions")]
    [Trait("Categoria", "Matricula Testes")]
    public void Matricula_Validar_DeveRetornarDomainException()
    {
        // Arrange
        var alunoId = Guid.NewGuid();

        var matriculaFaker = new Faker<Matricula>("pt_BR")
            .CustomInstantiator(f => new Matricula(Guid.Empty, alunoId, f.Random.Guid(), f.Lorem.Sentence(3), f.Date.FutureDateOnly()));

        // Act
        var exception = Assert.Throws<DomainException>(() => matriculaFaker.Generate());

        // Assert
        Assert.Equal("O campo Id deve ser preenchido", exception.Message);

        // Arrange
        matriculaFaker = new Faker<Matricula>("pt_BR")
            .CustomInstantiator(f => new Matricula(f.Random.Guid(), Guid.Empty, f.Random.Guid(), f.Lorem.Sentence(3), f.Date.FutureDateOnly()));

        // Act
        exception = Assert.Throws<DomainException>(() => matriculaFaker.Generate());

        // Assert
        Assert.Equal("O campo AlunoId deve ser preenchido", exception.Message);

        // Arrange
        matriculaFaker = new Faker<Matricula>("pt_BR")
            .CustomInstantiator(f => new Matricula(f.Random.Guid(), alunoId, Guid.Empty, f.Lorem.Sentence(3), f.Date.FutureDateOnly()));

        // Act
        exception = Assert.Throws<DomainException>(() => matriculaFaker.Generate());

        // Assert
        Assert.Equal("O campo CursoId deve ser preenchido", exception.Message);

        // Arrange
        matriculaFaker = new Faker<Matricula>("pt_BR")
            .CustomInstantiator(f => new Matricula(f.Random.Guid(), alunoId, f.Random.Guid(), string.Empty, f.Date.FutureDateOnly()));

        // Act
        exception = Assert.Throws<DomainException>(() => matriculaFaker.Generate());

        // Assert
        Assert.Equal("O campo CursoNome deve ser preenchido", exception.Message);

        // Arrange
        matriculaFaker = new Faker<Matricula>("pt_BR")
            .CustomInstantiator(f => new Matricula(f.Random.Guid(), alunoId, f.Random.Guid(), f.Random.String(101), f.Date.FutureDateOnly()));

        // Act
        exception = Assert.Throws<DomainException>(() => matriculaFaker.Generate());

        // Assert
        Assert.Equal("O campo CursoNome deve ter no máximo 100 caracteres", exception.Message);

        // Arrange
        matriculaFaker = new Faker<Matricula>("pt_BR")
            .CustomInstantiator(f => new Matricula(f.Random.Guid(), alunoId, f.Random.Guid(), f.Lorem.Sentence(3), f.Date.PastDateOnly()));

        // Act
        exception = Assert.Throws<DomainException>(() => matriculaFaker.Generate());

        // Assert
        Assert.Equal("O campo Validade deve ser maior ou igual a data atual", exception.Message);
    }

    [Fact(DisplayName = "Criar objeto de Matricula com sucesso")]
    [Trait("Categoria", "Matricula Testes")]
    public void Matricula_Validar_NaoDeveRetornarException()
    {
        // Arrange
        var matriculaFaker = GerarMatricula();

        // Act
        var matricula = matriculaFaker.Generate();

        // Assert
        Assert.NotEqual(matricula.Id, Guid.Empty);
    }

    [Fact(DisplayName = "Alterar CursoNome da Matricula deve retornar exception")]
    [Trait("Categoria", "Matricula Testes")]
    public void Matricula_AlterarCursoNome_DeveRetornarDomainException()
    {
        // Arrange
        var matriculaFaker = GerarMatricula();
        var matricula = matriculaFaker.Generate();

        // Act
        var exception = Assert.Throws<DomainException>(() => matricula.AlterarCursoNome(string.Empty));
        // Assert
        Assert.Equal("O campo CursoNome deve ser preenchido", exception.Message);

        // Act
        exception = Assert.Throws<DomainException>(() => matricula.AlterarCursoNome(new string('a', 101)));
        // Assert
        Assert.Equal("O campo CursoNome deve ter no máximo 100 caracteres", exception.Message);
    }

    [Fact(DisplayName = "Alterar Validade da Matricula deve retornar exception")]
    [Trait("Categoria", "Matricula Testes")]
    public void Matricula_AlterarValidade_DeveRetornarDomainException()
    {
        // Arrange
        var matriculaFaker = GerarMatricula();
        var matricula = matriculaFaker.Generate();

        // Act
        var exception = Assert.Throws<DomainException>(() => matricula.AlterarValidade(new Faker().Date.PastDateOnly()));
        // Assert
        Assert.Equal("O campo Validade deve ser maior ou igual a data atual", exception.Message);
    }

    [Fact(DisplayName = "Alterar CursoNome da Matricula deve alterar com sucesso")]
    [Trait("Categoria", "Matricula Testes")]
    public void Matricula_AlterarCursoNome_DeveAlterarComSucesso()
    {
        // Arrange
        var matriculaFaker = GerarMatricula();
        var matricula = matriculaFaker.Generate();
        var novoCursoNome = new Faker().Lorem.Sentence(3);

        // Act
        matricula.AlterarCursoNome(novoCursoNome);

        // Assert
        Assert.Equal(matricula.CursoNome, novoCursoNome);
    }

    [Fact(DisplayName = "Alterar Validade da Matricula deve alterar com sucesso")]
    [Trait("Categoria", "Matricula Testes")]
    public void Matricula_AlterarValidade_DeveAlterarComSucesso()
    {
        // Arrange
        var matriculaFaker = GerarMatricula();
        var matricula = matriculaFaker.Generate();
        var novaValidade = new Faker().Date.FutureDateOnly();

        // Act
        matricula.AlterarValidade(novaValidade);

        // Assert
        Assert.Equal(matricula.Validade, novaValidade);
    }

    [Fact(DisplayName = "Ativar e Desativar Matricula deve alterar o estado")]
    [Trait("Categoria", "Matricula Testes")]
    public void Matricula_AtivarDesativar_DeveAlterarEstado()
    {
        // Arrange
        var matriculaFaker = GerarMatricula();
        var matricula = matriculaFaker.Generate();

        // Act
        matricula.Ativar();

        // Assert
        Assert.True(matricula.Ativo);

        // Act
        matricula.Desativar();

        // Assert
        Assert.False(matricula.Ativo);
    }
}
