using Bogus;
using EduOnline.Alunos.Domain.Models;
using EduOnline.Core.DomainObjects;

namespace EduOnline.Alunos.UnitTest;

public class CertificadoTest
{
    private static Faker<Certificado> GerarCertificado()
    {
        return new Faker<Certificado>("pt_BR")
            .CustomInstantiator(f => new Certificado(
                f.Random.Guid(),
                f.Random.Guid(),
                f.Internet.Url()
            ));
    }

    [Fact(DisplayName = "Criar objeto de Certificado deve retornar exceptions")]
    [Trait("Categoria", "Certificado Testes")]
    public void Certificado_Validar_DeveRetornarDomainException()
    {
        // Arrange
        var certificadoFaker = new Faker<Certificado>("pt_BR")
            .CustomInstantiator(f => new Certificado(Guid.Empty, Guid.Empty, f.Internet.Url()));

        // Act
        var exception = Assert.Throws<DomainException>(() => certificadoFaker.Generate());

        // Assert
        Assert.Equal("O campo Id deve ser preenchido", exception.Message);

        // Arrange
        certificadoFaker = new Faker<Certificado>("pt_BR")
            .CustomInstantiator(f => new Certificado(f.Random.Guid(), f.Random.Guid(), string.Empty));

        // Act
        exception = Assert.Throws<DomainException>(() => certificadoFaker.Generate());

        // Assert
        Assert.Equal("O campo Link deve ser preenchido", exception.Message);

        // Arrange
        certificadoFaker = new Faker<Certificado>("pt_BR")
            .CustomInstantiator(f => new Certificado(f.Random.Guid(), f.Random.Guid(), new string('a', 301)));

        // Act
        exception = Assert.Throws<DomainException>(() => certificadoFaker.Generate());

        // Assert
        Assert.Equal("O campo Link deve ter no máximo 300 caracteres", exception.Message);
    }

    [Fact(DisplayName = "Criar objeto de Certificado com sucesso")]
    [Trait("Categoria", "Certificado Testes")]
    public void Certificado_Validar_NaoDeveRetornarException()
    {
        // Arrange
        var certificadoFaker = GerarCertificado();

        // Act
        var certificado = certificadoFaker.Generate();

        // Assert
        Assert.NotEqual(certificado.Id, Guid.Empty);
        Assert.False(string.IsNullOrWhiteSpace(certificado.Link));
    }
}
