using EduOnline.Pagamentos.Domain;
using FluentAssertions;

namespace EduOnline.Pagamentos.UnitTest;

public class StatusTransacaoTest
{
    #region Testes de Propriedades Estáticas

    [Fact(DisplayName = "StatusTransacao.Aprovado deve ter Id correto")]
    [Trait("Categoria", "StatusTransacao - Enumerador")]
    public void StatusTransacao_Aprovado_DeveTerIdCorreto()
    {
        // Arrange & Act
        var aprovado = StatusTransacao.Aprovado;

        // Assert
        aprovado.Id.Should().Be(1);
    }

    [Fact(DisplayName = "StatusTransacao.Recusado deve ter Id correto")]
    [Trait("Categoria", "StatusTransacao - Enumerador")]
    public void StatusTransacao_Recusado_DeveTerIdCorreto()
    {
        // Arrange & Act
        var recusado = StatusTransacao.Recusado;

        // Assert
        recusado.Id.Should().Be(2);
    }

    [Fact(DisplayName = "StatusTransacao.Aprovado deve ter Nome correto")]
    [Trait("Categoria", "StatusTransacao - Enumerador")]
    public void StatusTransacao_Aprovado_DeveTerNomeCorreto()
    {
        // Arrange & Act
        var aprovado = StatusTransacao.Aprovado;

        // Assert
        aprovado.Nome.Should().Be("Aprovado");
    }

    [Fact(DisplayName = "StatusTransacao.Recusado deve ter Nome correto")]
    [Trait("Categoria", "StatusTransacao - Enumerador")]
    public void StatusTransacao_Recusado_DeveTerNomeCorreto()
    {
        // Arrange & Act
        var recusado = StatusTransacao.Recusado;

        // Assert
        recusado.Nome.Should().Be("Recusado"); // Typo corrigido!
    }

    #endregion

    #region Testes de Comparação

    [Fact(DisplayName = "StatusTransacao Aprovado e Recusado devem ser diferentes")]
    [Trait("Categoria", "StatusTransacao - Comparação")]
    public void StatusTransacao_AprovadoERecusado_DevemSerDiferentes()
    {
        // Arrange & Act
        var aprovado = StatusTransacao.Aprovado;
        var recusado = StatusTransacao.Recusado;

        // Assert
        aprovado.Id.Should().NotBe(recusado.Id);
        aprovado.Nome.Should().NotBe(recusado.Nome);
    }

    [Fact(DisplayName = "StatusTransacao Aprovado deve ser igual a outro Aprovado")]
    [Trait("Categoria", "StatusTransacao - Comparação")]
    public void StatusTransacao_Aprovado_DeveSerIgualAOutroAprovado()
    {
        // Arrange & Act
        var aprovado1 = StatusTransacao.Aprovado;
        var aprovado2 = StatusTransacao.Aprovado;

        // Assert
        aprovado1.Id.Should().Be(aprovado2.Id);
        aprovado1.Nome.Should().Be(aprovado2.Nome);
    }

    #endregion

    #region Testes de Uso com Transacao

    [Fact(DisplayName = "Transacao deve aceitar StatusTransacao.Aprovado")]
    [Trait("Categoria", "StatusTransacao - Integração")]
    public void Transacao_DeveAceitarStatusAprovado()
    {
        // Arrange
        var transacao = new Transacao();

        // Act
        transacao.StatusTransacaoId = StatusTransacao.Aprovado.Id;

        // Assert
        transacao.StatusTransacaoId.Should().Be(1);
        transacao.StatusTransacaoId.Should().Be(StatusTransacao.Aprovado.Id);
    }

    [Fact(DisplayName = "Transacao deve aceitar StatusTransacao.Recusado")]
    [Trait("Categoria", "StatusTransacao - Integração")]
    public void Transacao_DeveAceitarStatusRecusado()
    {
        // Arrange
        var transacao = new Transacao();

        // Act
        transacao.StatusTransacaoId = StatusTransacao.Recusado.Id;

        // Assert
        transacao.StatusTransacaoId.Should().Be(2);
        transacao.StatusTransacaoId.Should().Be(StatusTransacao.Recusado.Id);
    }

    #endregion

    #region Testes de Valores

    [Theory(DisplayName = "StatusTransacao deve ter apenas 2 valores válidos")]
    [Trait("Categoria", "StatusTransacao - Validação")]
    [InlineData(1, "Aprovado")]
    [InlineData(2, "Recusado")]
    public void StatusTransacao_DeveTerApenasValoresValidos(int id, string nome)
    {
        // Arrange & Act
        var status = id == 1 ? StatusTransacao.Aprovado : StatusTransacao.Recusado;

        // Assert
        status.Id.Should().Be(id);
        status.Nome.Should().Be(nome);
    }

    #endregion

    #region Testes de Cenários

    [Fact(DisplayName = "Cenário: Verificar se transação foi aprovada")]
    [Trait("Categoria", "StatusTransacao - Cenário")]
    public void Cenario_VerificarSeTransacaoFoiAprovada()
    {
        // Arrange
        var transacao = new Transacao
        {
            StatusTransacaoId = StatusTransacao.Aprovado.Id
        };

        // Act
        var foiAprovada = transacao.StatusTransacaoId == StatusTransacao.Aprovado.Id;

        // Assert
        foiAprovada.Should().BeTrue();
    }

    [Fact(DisplayName = "Cenário: Verificar se transação foi recusada")]
    [Trait("Categoria", "StatusTransacao - Cenário")]
    public void Cenario_VerificarSeTransacaoFoiRecusada()
    {
        // Arrange
        var transacao = new Transacao
        {
            StatusTransacaoId = StatusTransacao.Recusado.Id
        };

        // Act
        var foiRecusada = transacao.StatusTransacaoId == StatusTransacao.Recusado.Id;

        // Assert
        foiRecusada.Should().BeTrue();
    }

    [Fact(DisplayName = "Cenário: Alternar status de Aprovado para Recusado")]
    [Trait("Categoria", "StatusTransacao - Cenário")]
    public void Cenario_AlternarStatusAprovadoParaRecusado()
    {
        // Arrange
        var transacao = new Transacao
        {
            StatusTransacaoId = StatusTransacao.Aprovado.Id
        };

        // Act - Simular estorno
        transacao.StatusTransacaoId = StatusTransacao.Recusado.Id;

        // Assert
        transacao.StatusTransacaoId.Should().Be(StatusTransacao.Recusado.Id);
        transacao.StatusTransacaoId.Should().NotBe(StatusTransacao.Aprovado.Id);
    }

    #endregion

    #region Testes de ToString

    [Fact(DisplayName = "StatusTransacao.Aprovado ToString deve retornar nome")]
    [Trait("Categoria", "StatusTransacao - ToString")]
    public void StatusTransacao_Aprovado_ToStringDeveRetornarNome()
    {
        // Arrange & Act
        var aprovado = StatusTransacao.Aprovado;
        var resultado = aprovado.ToString();

        // Assert
        resultado.Should().Contain("Aprovado");
    }

    [Fact(DisplayName = "StatusTransacao.Recusado ToString deve retornar nome")]
    [Trait("Categoria", "StatusTransacao - ToString")]
    public void StatusTransacao_Recusado_ToStringDeveRetornarNome()
    {
        // Arrange & Act
        var recusado = StatusTransacao.Recusado;
        var resultado = recusado.ToString();

        // Assert
        resultado.Should().Contain("Recusado"); // Typo corrigido
    }

    #endregion
}
