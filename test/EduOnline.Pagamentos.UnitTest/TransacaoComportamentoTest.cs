using Bogus;
using EduOnline.Pagamentos.Domain;
using FluentAssertions;

namespace EduOnline.Pagamentos.UnitTest;

public class TransacaoComportamentoTest
{
    #region Testes de Criação e Propriedades

    [Fact(DisplayName = "Nova Transacao deve ter Id gerado automaticamente")]
    [Trait("Categoria", "Transacao - Comportamento")]
    public void NovaTransacao_DeveTerIdGeradoAutomaticamente()
    {
        // Arrange & Act
        var transacao = new Transacao();

        // Assert
        transacao.Id.Should().NotBeEmpty();
    }

    [Fact(DisplayName = "Transacao deve permitir definir PagamentoId")]
    [Trait("Categoria", "Transacao - Comportamento")]
    public void Transacao_DevePermitirDefinirPagamentoId()
    {
        // Arrange
        var transacao = new Transacao();
        var pagamentoId = Guid.NewGuid();

        // Act
        transacao.PagamentoId = pagamentoId;

        // Assert
        transacao.PagamentoId.Should().Be(pagamentoId);
    }

    [Theory(DisplayName = "Transacao deve permitir diferentes valores totais")]
    [Trait("Categoria", "Transacao - Comportamento")]
    [InlineData(50.00)]
    [InlineData(199.90)]
    [InlineData(499.99)]
    [InlineData(1500.00)]
    public void Transacao_DevePermitirDiferentesValores(decimal valor)
    {
        // Arrange
        var transacao = new Transacao();

        // Act
        transacao.Total = valor;

        // Assert
        transacao.Total.Should().Be(valor);
    }

    #endregion

    #region Testes de Status da Transacao

    [Fact(DisplayName = "Transacao deve permitir status Aprovado")]
    [Trait("Categoria", "Transacao - Comportamento")]
    public void Transacao_DevePermitirStatusAprovado()
    {
        // Arrange
        var transacao = new Transacao();

        // Act
        transacao.StatusTransacaoId = StatusTransacao.Aprovado.Id;

        // Assert
        transacao.StatusTransacaoId.Should().Be(StatusTransacao.Aprovado.Id);
    }

    [Fact(DisplayName = "Transacao deve permitir status Recusado")]
    [Trait("Categoria", "Transacao - Comportamento")]
    public void Transacao_DevePermitirStatusRecusado()
    {
        // Arrange
        var transacao = new Transacao();

        // Act
        transacao.StatusTransacaoId = StatusTransacao.Recusado.Id;

        // Assert
        transacao.StatusTransacaoId.Should().Be(StatusTransacao.Recusado.Id);
    }

    [Fact(DisplayName = "Transacao pode iniciar sem status definido")]
    [Trait("Categoria", "Transacao - Comportamento")]
    public void Transacao_PodeIniciarSemStatus()
    {
        // Arrange & Act
        var transacao = new Transacao();

        // Assert
        transacao.StatusTransacaoId.Should().BeNull();
    }

    [Theory(DisplayName = "Transacao deve suportar mudança de status")]
    [Trait("Categoria", "Transacao - Comportamento")]
    [InlineData(null, 1)] // Null → Aprovado
    [InlineData(null, 2)] // Null → Recusado
    [InlineData(1, 2)]    // Aprovado → Recusado (reversão/estorno)
    public void Transacao_DeveSuportarMudancaDeStatus(int? statusInicial, int statusFinal)
    {
        // Arrange
        var transacao = new Transacao
        {
            StatusTransacaoId = statusInicial
        };

        // Act
        transacao.StatusTransacaoId = statusFinal;

        // Assert
        transacao.StatusTransacaoId.Should().Be(statusFinal);
    }

    #endregion

    #region Testes de Relacionamento com Pagamento

    [Fact(DisplayName = "Transacao deve referenciar o Pagamento correto")]
    [Trait("Categoria", "Transacao - Comportamento")]
    public void Transacao_DeveReferenciarPagamentoCorreto()
    {
        // Arrange
        var pagamentoId = Guid.NewGuid();
        var transacao = new Transacao
        {
            PagamentoId = pagamentoId
        };

        // Assert
        transacao.PagamentoId.Should().Be(pagamentoId);
    }

    [Fact(DisplayName = "Transacao pode ter navegação para Pagamento configurada")]
    [Trait("Categoria", "Transacao - Comportamento")]
    public void Transacao_PodeNavegacaoParaPagamentoConfigurada()
    {
        // Arrange
        var pagamento = GerarPagamento();
        var transacao = new Transacao
        {
            PagamentoId = pagamento.Id
        };

        // Act
        transacao.Pagamento = pagamento;

        // Assert
        transacao.Pagamento.Should().NotBeNull();
        transacao.Pagamento.Should().Be(pagamento);
        transacao.PagamentoId.Should().Be(pagamento.Id);
    }

    #endregion

    #region Testes de Modificação de Dados

    [Fact(DisplayName = "Transacao deve permitir atualizar todos os campos")]
    [Trait("Categoria", "Transacao - Comportamento")]
    public void Transacao_DevePermitirAtualizarTodosCampos()
    {
        // Arrange
        var transacao = GerarTransacao();
        var novoPagamentoId = Guid.NewGuid();

        // Act
        transacao.PagamentoId = novoPagamentoId;
        transacao.Total = 999.99m;
        transacao.StatusTransacaoId = StatusTransacao.Recusado.Id;

        // Assert
        transacao.PagamentoId.Should().Be(novoPagamentoId);
        transacao.Total.Should().Be(999.99m);
        transacao.StatusTransacaoId.Should().Be(StatusTransacao.Recusado.Id);
    }

    #endregion

    #region Testes de Cenários Reais

    [Fact(DisplayName = "Cenário: Transacao aprovada para pagamento válido")]
    [Trait("Categoria", "Transacao - Comportamento")]
    public void Cenario_TransacaoAprovadaParaPagamentoValido()
    {
        // Arrange
        var pagamento = GerarPagamento();
        
        // Act - Criar transação aprovada
        var transacao = new Transacao
        {
            PagamentoId = pagamento.Id,
            Total = pagamento.Total,
            StatusTransacaoId = StatusTransacao.Aprovado.Id,
            Pagamento = pagamento
        };

        // Assert
        transacao.StatusTransacaoId.Should().Be(StatusTransacao.Aprovado.Id);
        transacao.Total.Should().Be(pagamento.Total);
        transacao.Pagamento.Should().Be(pagamento);
    }

    [Fact(DisplayName = "Cenário: Transacao recusada por saldo insuficiente")]
    [Trait("Categoria", "Transacao - Comportamento")]
    public void Cenario_TransacaoRecusadaPorSaldoInsuficiente()
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

        // Assert
        transacao.StatusTransacaoId.Should().Be(StatusTransacao.Recusado.Id);
        transacao.StatusTransacaoId.Should().NotBe(StatusTransacao.Aprovado.Id);
    }

    [Fact(DisplayName = "Cenário: Múltiplas transações para o mesmo Pagamento (tentativas)")]
    [Trait("Categoria", "Transacao - Comportamento")]
    public void Cenario_MultiplasTentativasTransacao()
    {
        // Arrange
        var pagamento = GerarPagamento();

        // Act - Primeira tentativa recusada
        var transacao1 = new Transacao
        {
            PagamentoId = pagamento.Id,
            Total = pagamento.Total,
            StatusTransacaoId = StatusTransacao.Recusado.Id
        };

        // Act - Segunda tentativa aprovada
        var transacao2 = new Transacao
        {
            PagamentoId = pagamento.Id,
            Total = pagamento.Total,
            StatusTransacaoId = StatusTransacao.Aprovado.Id
        };

        // Assert
        transacao1.PagamentoId.Should().Be(pagamento.Id);
        transacao2.PagamentoId.Should().Be(pagamento.Id);
        transacao1.Id.Should().NotBe(transacao2.Id);
        transacao1.StatusTransacaoId.Should().Be(StatusTransacao.Recusado.Id);
        transacao2.StatusTransacaoId.Should().Be(StatusTransacao.Aprovado.Id);
    }

    [Fact(DisplayName = "Cenário: Estorno - Transacao aprovada alterada para recusada")]
    [Trait("Categoria", "Transacao - Comportamento")]
    public void Cenario_EstornoTransacaoAprovada()
    {
        // Arrange - Transação inicialmente aprovada
        var transacao = new Transacao
        {
            PagamentoId = Guid.NewGuid(),
            Total = 299.90m,
            StatusTransacaoId = StatusTransacao.Aprovado.Id
        };

        // Act - Realizar estorno (mudança de status)
        transacao.StatusTransacaoId = StatusTransacao.Recusado.Id;

        // Assert
        transacao.StatusTransacaoId.Should().Be(StatusTransacao.Recusado.Id);
    }

    #endregion

    #region Testes de IDs Únicos

    [Fact(DisplayName = "Múltiplas Transacoes devem ter IDs únicos")]
    [Trait("Categoria", "Transacao - Comportamento")]
    public void MultiplasTransacoes_DevemTerIdsUnicos()
    {
        // Arrange & Act
        var transacao1 = new Transacao();
        var transacao2 = new Transacao();
        var transacao3 = new Transacao();

        // Assert
        transacao1.Id.Should().NotBe(transacao2.Id);
        transacao2.Id.Should().NotBe(transacao3.Id);
        transacao1.Id.Should().NotBe(transacao3.Id);
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

    private static Transacao GerarTransacao()
    {
        var faker = new Faker();
        return new Transacao
        {
            PagamentoId = Guid.NewGuid(),
            Total = faker.Random.Decimal(50, 1500),
            StatusTransacaoId = faker.PickRandom(StatusTransacao.Aprovado.Id, StatusTransacao.Recusado.Id)
        };
    }

    #endregion
}
