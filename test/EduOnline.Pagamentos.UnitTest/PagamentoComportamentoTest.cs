using Bogus;
using EduOnline.Pagamentos.Domain;
using FluentAssertions;

namespace EduOnline.Pagamentos.UnitTest;

public class PagamentoComportamentoTest
{
    #region Testes de Criação e Propriedades

    [Fact(DisplayName = "Novo Pagamento deve ter Id gerado automaticamente")]
    [Trait("Categoria", "Pagamento - Comportamento")]
    public void NovoPagamento_DeveTerIdGeradoAutomaticamente()
    {
        // Arrange & Act
        var pagamento = new Pagamento();

        // Assert
        pagamento.Id.Should().NotBeEmpty();
    }

    [Fact(DisplayName = "Pagamento deve permitir definir AlunoId")]
    [Trait("Categoria", "Pagamento - Comportamento")]
    public void Pagamento_DevePermitirDefinirAlunoId()
    {
        // Arrange
        var pagamento = new Pagamento();
        var alunoId = Guid.NewGuid();

        // Act
        pagamento.AlunoId = alunoId;

        // Assert
        pagamento.AlunoId.Should().Be(alunoId);
    }

    [Fact(DisplayName = "Pagamento deve permitir definir CursoId")]
    [Trait("Categoria", "Pagamento - Comportamento")]
    public void Pagamento_DevePermitirDefinirCursoId()
    {
        // Arrange
        var pagamento = new Pagamento();
        var cursoId = Guid.NewGuid();

        // Act
        pagamento.CursoId = cursoId;

        // Assert
        pagamento.CursoId.Should().Be(cursoId);
    }

    [Theory(DisplayName = "Pagamento deve permitir diferentes valores totais")]
    [Trait("Categoria", "Pagamento - Comportamento")]
    [InlineData(50.00)]
    [InlineData(199.90)]
    [InlineData(499.99)]
    [InlineData(1500.00)]
    public void Pagamento_DevePermitirDiferentesValores(decimal valor)
    {
        // Arrange
        var pagamento = new Pagamento();

        // Act
        pagamento.Total = valor;

        // Assert
        pagamento.Total.Should().Be(valor);
    }

    #endregion

    #region Testes de Dados do Cartão

    [Fact(DisplayName = "Pagamento deve armazenar NomeCartao")]
    [Trait("Categoria", "Pagamento - Comportamento")]
    public void Pagamento_DeveArmazenarNomeCartao()
    {
        // Arrange
        var pagamento = new Pagamento();
        var nomeCartao = "João Silva Santos";

        // Act
        pagamento.NomeCartao = nomeCartao;

        // Assert
        pagamento.NomeCartao.Should().Be(nomeCartao);
    }

    [Theory(DisplayName = "Pagamento deve armazenar diferentes números de cartão")]
    [Trait("Categoria", "Pagamento - Comportamento")]
    [InlineData("4111111111111111")] // Visa
    [InlineData("5555555555554444")] // Mastercard
    [InlineData("378282246310005")]  // Amex
    public void Pagamento_DeveArmazenarNumeroCartao(string numeroCartao)
    {
        // Arrange
        var pagamento = new Pagamento();

        // Act
        pagamento.NumeroCartao = numeroCartao;

        // Assert
        pagamento.NumeroCartao.Should().Be(numeroCartao);
    }

    [Theory(DisplayName = "Pagamento deve armazenar datas de expiração válidas")]
    [Trait("Categoria", "Pagamento - Comportamento")]
    [InlineData("12/25")]
    [InlineData("06/28")]
    [InlineData("11/30")]
    public void Pagamento_DeveArmazenarExpiracaoCartao(string expiracao)
    {
        // Arrange
        var pagamento = new Pagamento();

        // Act
        pagamento.ExpiracaoCartao = expiracao;

        // Assert
        pagamento.ExpiracaoCartao.Should().Be(expiracao);
    }

    [Theory(DisplayName = "Pagamento deve armazenar CVV válido")]
    [Trait("Categoria", "Pagamento - Comportamento")]
    [InlineData("123")]
    [InlineData("456")]
    [InlineData("999")]
    public void Pagamento_DeveArmazenarCvvCartao(string cvv)
    {
        // Arrange
        var pagamento = new Pagamento();

        // Act
        pagamento.CvvCartao = cvv;

        // Assert
        pagamento.CvvCartao.Should().Be(cvv);
    }

    #endregion

    #region Testes de Relacionamento com Transacao

    [Fact(DisplayName = "Pagamento pode ter Transacao associada")]
    [Trait("Categoria", "Pagamento - Comportamento")]
    public void Pagamento_PodeTerTransacaoAssociada()
    {
        // Arrange
        var pagamento = GerarPagamento();
        var transacao = GerarTransacao(pagamento.Id);

        // Act
        pagamento.Transacao = transacao;

        // Assert
        pagamento.Transacao.Should().NotBeNull();
        pagamento.Transacao.Should().Be(transacao);
        pagamento.Transacao!.PagamentoId.Should().Be(pagamento.Id);
    }

    [Fact(DisplayName = "Pagamento pode iniciar sem Transacao")]
    [Trait("Categoria", "Pagamento - Comportamento")]
    public void Pagamento_PodeIniciarSemTransacao()
    {
        // Arrange & Act
        var pagamento = new Pagamento();

        // Assert
        pagamento.Transacao.Should().BeNull();
    }

    #endregion

    #region Testes de Modificação de Dados

    [Fact(DisplayName = "Pagamento deve permitir atualizar todos os campos")]
    [Trait("Categoria", "Pagamento - Comportamento")]
    public void Pagamento_DevePermitirAtualizarTodosCampos()
    {
        // Arrange
        var pagamento = GerarPagamento();
        var novoAlunoId = Guid.NewGuid();
        var novoCursoId = Guid.NewGuid();

        // Act
        pagamento.AlunoId = novoAlunoId;
        pagamento.CursoId = novoCursoId;
        pagamento.Total = 599.99m;
        pagamento.NomeCartao = "Maria Souza";
        pagamento.NumeroCartao = "5555555555554444";
        pagamento.ExpiracaoCartao = "12/30";
        pagamento.CvvCartao = "999";

        // Assert
        pagamento.AlunoId.Should().Be(novoAlunoId);
        pagamento.CursoId.Should().Be(novoCursoId);
        pagamento.Total.Should().Be(599.99m);
        pagamento.NomeCartao.Should().Be("Maria Souza");
        pagamento.NumeroCartao.Should().Be("5555555555554444");
        pagamento.ExpiracaoCartao.Should().Be("12/30");
        pagamento.CvvCartao.Should().Be("999");
    }

    #endregion

    #region Testes de Cenários Reais

    [Fact(DisplayName = "Cenário: Pagamento completo aprovado com Transacao")]
    [Trait("Categoria", "Pagamento - Comportamento")]
    public void Cenario_PagamentoCompletoAprovadoComTransacao()
    {
        // Arrange - Criar pagamento
        var pagamento = new Pagamento
        {
            AlunoId = Guid.NewGuid(),
            CursoId = Guid.NewGuid(),
            Total = 299.90m,
            NomeCartao = "Carlos Alberto",
            NumeroCartao = "4111111111111111",
            ExpiracaoCartao = "06/29",
            CvvCartao = "456"
        };

        // Act - Associar transação aprovada
        var transacao = new Transacao
        {
            PagamentoId = pagamento.Id,
            Total = pagamento.Total,
            StatusTransacaoId = StatusTransacao.Aprovado.Id
        };
        pagamento.Transacao = transacao;

        // Assert
        pagamento.Transacao.Should().NotBeNull();
        pagamento.Transacao!.StatusTransacaoId.Should().Be(StatusTransacao.Aprovado.Id);
        pagamento.Transacao.Total.Should().Be(pagamento.Total);
    }

    [Fact(DisplayName = "Cenário: Pagamento recusado com Transacao")]
    [Trait("Categoria", "Pagamento - Comportamento")]
    public void Cenario_PagamentoRecusadoComTransacao()
    {
        // Arrange
        var pagamento = GerarPagamento();

        // Act - Transação recusada
        var transacao = new Transacao
        {
            PagamentoId = pagamento.Id,
            Total = pagamento.Total,
            StatusTransacaoId = StatusTransacao.Recusado.Id
        };
        pagamento.Transacao = transacao;

        // Assert
        pagamento.Transacao.Should().NotBeNull();
        pagamento.Transacao!.StatusTransacaoId.Should().Be(StatusTransacao.Recusado.Id);
    }

    #endregion

    #region Helpers

    private static Pagamento GerarPagamento()
    {
        var faker = new Faker("pt_BR");
        return new Pagamento
        {
            AlunoId = Guid.NewGuid(),
            CursoId = Guid.NewGuid(),
            Total = faker.Random.Decimal(50, 1500),
            NomeCartao = faker.Name.FullName(),
            NumeroCartao = faker.Finance.CreditCardNumber(),
            ExpiracaoCartao = $"{faker.Random.Int(1, 12):D2}/{faker.Random.Int(25, 35)}",
            CvvCartao = faker.Random.Int(100, 999).ToString()
        };
    }

    private static Transacao GerarTransacao(Guid pagamentoId)
    {
        var faker = new Faker();
        return new Transacao
        {
            PagamentoId = pagamentoId,
            Total = faker.Random.Decimal(50, 1500),
            StatusTransacaoId = faker.PickRandom(StatusTransacao.Aprovado.Id, StatusTransacao.Recusado.Id)
        };
    }

    #endregion
}
