using Bogus;
using EduOnline.Alunos.Domain.Models;
using EduOnline.Core.DomainObjects;

namespace EduOnline.Alunos.UnitTest;

public class AlunoTest
{
    private static Faker<Aluno> GerarAluno()
    {
        return new Faker<Aluno>("pt_BR")
            .CustomInstantiator(f => new Aluno(f.Random.Guid(), f.Person.FullName, f.Person.Email, f.Date.PastDateOnly()));
    }

    [Fact(DisplayName = "Criar Objeto de Aluno deve retornar exceptions ")]
    [Trait("Categoria", "Aluno Testes")]
    public void Aluno_Validar_DeveRetornarDomainException()
    {
        // Arrange
        var alunoFaker = new Faker<Aluno>("pt_BR")
            .CustomInstantiator(f => new Aluno(Guid.Empty, f.Person.FullName, f.Person.Email, f.Date.PastDateOnly()));

        // Act
        var exception = Assert.Throws<DomainException>(() => alunoFaker.Generate());

        // Assert
        Assert.Equal("O campo Id deve ser preenchido", exception.Message);

        // Arrange
        alunoFaker = new Faker<Aluno>("pt_BR")
            .CustomInstantiator(f => new Aluno(f.Random.Guid(), string.Empty, f.Person.Email, f.Date.PastDateOnly()));

        // Act
        exception = Assert.Throws<DomainException>(() => alunoFaker.Generate());

        // Assert
        Assert.Equal("O campo Nome deve ser preenchido", exception.Message);

        // Arrange
        alunoFaker = new Faker<Aluno>("pt_BR")
            .CustomInstantiator(f => new Aluno(f.Random.Guid(), f.Random.String(101), f.Person.Email, f.Date.PastDateOnly()));

        // Act
        exception = Assert.Throws<DomainException>(() => alunoFaker.Generate());

        // Assert
        Assert.Equal("O campo Nome deve ter no máximo 100 caracteres", exception.Message);

        // Arrange
        alunoFaker = new Faker<Aluno>("pt_BR")
            .CustomInstantiator(f => new Aluno(f.Random.Guid(), f.Person.FullName, string.Empty, f.Date.PastDateOnly()));

        // Act
        exception = Assert.Throws<DomainException>(() => alunoFaker.Generate());

        // Assert
        Assert.Equal("O campo Email deve ser preenchido", exception.Message);

        // Arrange
        alunoFaker = new Faker<Aluno>("pt_BR")
            .CustomInstantiator(f => new Aluno(f.Random.Guid(), f.Person.FullName, f.Random.String(101), f.Date.PastDateOnly()));

        // Act
        exception = Assert.Throws<DomainException>(() => alunoFaker.Generate());

        // Assert
        Assert.Equal("O campo Email deve ter no máximo 100 caracteres", exception.Message);
    }

    [Fact(DisplayName = "Criar objeto de Aluno com sucesso")]
    [Trait("Categoria", "Aluno Testes")]
    public void Aluno_Validar_NaoDeveRetornarException()
    {
        // Arrange
        var alunoFaker = GerarAluno();

        // Act
        var aluno = alunoFaker.Generate();

        // Assert
        Assert.NotEqual(aluno.Id, Guid.Empty);
    }

    [Fact(DisplayName = "Alterar Nome do Aluno deve retornar exception")]
    [Trait("Categoria", "Aluno Testes")]
    public void Aluno_AlterarNome_DeveRetornarDomainException()
    {
        // Arrange
        var alunoFaker = GerarAluno();
        var aluno = alunoFaker.Generate();
        // Act
        var exception = Assert.Throws<DomainException>(() => aluno.AlterarNome(string.Empty));
        // Assert
        Assert.Equal("O campo Nome deve ser preenchido", exception.Message);
        // Act
        exception = Assert.Throws<DomainException>(() => aluno.AlterarNome(new string('a', 101)));
        // Assert
        Assert.Equal("O campo Nome deve ter no máximo 100 caracteres", exception.Message);
    }

    [Fact(DisplayName = "Alterar Data de Nascimento do Aluno deve retornar exception")]
    [Trait("Categoria", "Aluno Testes")]
    public void Aluno_AlterarDataNascimento_DeveRetornarDomainException()
    {
        // Arrange
        var alunoFaker = GerarAluno();
        var aluno = alunoFaker.Generate();
        // Act
        var exception = Assert.Throws<DomainException>(() => aluno.AlterarDataNascimento(new Faker().Date.FutureDateOnly()));
        // Assert
        Assert.Equal("O campo Data de Nascimento deve ser menor do que a data atual", exception.Message);
    }

    [Fact(DisplayName = "Alterar Nome do Aluno deve alterar com sucesso")]
    [Trait("Categoria", "Aluno Testes")]
    public void Aluno_AlterarNome_DeveAlterarComSucesso()
    {
        // Arrange
        var alunoFaker = GerarAluno();
        var aluno = alunoFaker.Generate();
        var novoNome = new Faker().Person.FullName;

        // Act
        aluno.AlterarNome(novoNome);

        // Assert
        Assert.Equal(aluno.Nome, novoNome);
    }

    [Fact(DisplayName = "Alterar Data de Nascimento do Aluno deve alterar com sucesso")]
    [Trait("Categoria", "Aluno Testes")]
    public void Aluno_AlterarDataNascimento_DeveAlterarComSucesso()
    {
        // Arrange
        var alunoFaker = GerarAluno();
        var aluno = alunoFaker.Generate();
        var novaDataNascimento = new Faker().Date.PastDateOnly();

        // Act
        aluno.AlterarDataNascimento(novaDataNascimento);

        // Assert
        Assert.Equal(aluno.DataNascimento, novaDataNascimento);
    }

    [Fact(DisplayName = "Adicionar Matrícula do Aluno deve incrimentar a lista")]
    [Trait("Categoria", "Aluno Testes")]
    public void Aluno_AdicionarMatricula_DeveIncrimentarLista()
    {
        // Arrange
        var alunoFaker = GerarAluno();
        var aluno = alunoFaker.Generate();
        var matricula = new Faker<Matricula>("pt_BR")
            .CustomInstantiator(f => new Matricula(f.Random.Guid(), aluno.Id, f.Random.Guid(), f.Lorem.Sentence(), f.Date.FutureDateOnly()))
            .Generate();

        // Act
        aluno.AdicionarMatricula(matricula);

        // Assert
        Assert.Single(aluno.Matriculas);
        Assert.Contains(matricula, aluno.Matriculas);
    }
}

