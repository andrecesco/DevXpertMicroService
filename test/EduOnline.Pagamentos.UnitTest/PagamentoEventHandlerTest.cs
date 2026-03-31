using Bogus;
using EduOnline.Core.DomainObjects.Dtos;
using EduOnline.Core.Mensagens.IntegrationEvents;
using EduOnline.Pagamentos.Domain;
using EduOnline.Pagamentos.Domain.Events;
using FluentAssertions;
using Moq;

namespace EduOnline.Pagamentos.UnitTest;

public class PagamentoEventHandlerTest
{
    private readonly Mock<IPagamentoService> _pagamentoServiceMock;
    private readonly PagamentoEventHandler _handler;

    public PagamentoEventHandlerTest()
    {
        _pagamentoServiceMock = new Mock<IPagamentoService>();
        _handler = new PagamentoEventHandler(_pagamentoServiceMock.Object);
    }

    #region Testes de Handle - CursoCompradoIntegrationEvent

    [Fact(DisplayName = "Handle deve chamar RealizarPagamentoCurso com dados corretos")]
    [Trait("Categoria", "PagamentoEventHandler - Comportamento")]
    public async Task Handle_DeveChamarRealizarPagamentoCurso_ComDadosCorretos()
    {
        // Arrange
        var evento = GerarCursoCompradoEvent();
        PagamentoCurso? pagamentoCursoCapturado = null;

        _pagamentoServiceMock
            .Setup(s => s.RealizarPagamentoCurso(It.IsAny<PagamentoCurso>()))
            .Callback<PagamentoCurso>(pc => pagamentoCursoCapturado = pc)
            .ReturnsAsync(new Transacao { StatusTransacaoId = StatusTransacao.Aprovado.Id });

        // Act
        await _handler.Handle(evento, CancellationToken.None);

        // Assert
        _pagamentoServiceMock.Verify(s => s.RealizarPagamentoCurso(It.IsAny<PagamentoCurso>()), Times.Once);
        
        pagamentoCursoCapturado.Should().NotBeNull();
        pagamentoCursoCapturado!.MatriculaId.Should().Be(evento.AggregateId);
        pagamentoCursoCapturado.AlunoId.Should().Be(evento.AlunoId);
        pagamentoCursoCapturado.CursoId.Should().Be(evento.CursoId);
        pagamentoCursoCapturado.Total.Should().Be(evento.Total);
        pagamentoCursoCapturado.NomeCartao.Should().Be(evento.NomeCartao);
        pagamentoCursoCapturado.NumeroCartao.Should().Be(evento.NumeroCartao);
        pagamentoCursoCapturado.ExpiracaoCartao.Should().Be(evento.ExpiracaoCartao);
        pagamentoCursoCapturado.CvvCartao.Should().Be(evento.CvvCartao);
    }

    [Fact(DisplayName = "Handle deve mapear corretamente MatriculaId do evento para PagamentoCurso")]
    [Trait("Categoria", "PagamentoEventHandler - Mapeamento")]
    public async Task Handle_DeveMapearCorretamenteMatriculaId()
    {
        // Arrange
        var matriculaId = Guid.NewGuid();
        var evento = new CursoCompradoIntegrationEvent(
            matriculaId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            199.90m,
            "João Silva",
            "4111111111111111",
            "12/29",
            "123"
        );

        PagamentoCurso? pagamentoCursoCapturado = null;
        _pagamentoServiceMock
            .Setup(s => s.RealizarPagamentoCurso(It.IsAny<PagamentoCurso>()))
            .Callback<PagamentoCurso>(pc => pagamentoCursoCapturado = pc)
            .ReturnsAsync(new Transacao());

        // Act
        await _handler.Handle(evento, CancellationToken.None);

        // Assert
        pagamentoCursoCapturado.Should().NotBeNull();
        pagamentoCursoCapturado!.MatriculaId.Should().Be(matriculaId);
        pagamentoCursoCapturado.MatriculaId.Should().Be(evento.AggregateId);
    }

    [Theory(DisplayName = "Handle deve processar diferentes valores de pagamento")]
    [Trait("Categoria", "PagamentoEventHandler - Comportamento")]
    [InlineData(50.00)]
    [InlineData(199.90)]
    [InlineData(499.99)]
    [InlineData(1500.00)]
    public async Task Handle_DeveProcessarDiferentesValores(decimal valor)
    {
        // Arrange
        var evento = GerarCursoCompradoEvent();
        evento.GetType().GetProperty("Total")!.SetValue(evento, valor);

        PagamentoCurso? pagamentoCursoCapturado = null;
        _pagamentoServiceMock
            .Setup(s => s.RealizarPagamentoCurso(It.IsAny<PagamentoCurso>()))
            .Callback<PagamentoCurso>(pc => pagamentoCursoCapturado = pc)
            .ReturnsAsync(new Transacao());

        // Act
        await _handler.Handle(evento, CancellationToken.None);

        // Assert
        pagamentoCursoCapturado.Should().NotBeNull();
        pagamentoCursoCapturado!.Total.Should().Be(valor);
    }

    [Fact(DisplayName = "Handle deve suportar CancellationToken")]
    [Trait("Categoria", "PagamentoEventHandler - Comportamento")]
    public async Task Handle_DeveSuportarCancellationToken()
    {
        // Arrange
        var evento = GerarCursoCompradoEvent();
        var cts = new CancellationTokenSource();

        _pagamentoServiceMock
            .Setup(s => s.RealizarPagamentoCurso(It.IsAny<PagamentoCurso>()))
            .ReturnsAsync(new Transacao());

        // Act
        await _handler.Handle(evento, cts.Token);

        // Assert
        _pagamentoServiceMock.Verify(s => s.RealizarPagamentoCurso(It.IsAny<PagamentoCurso>()), Times.Once);
    }

    #endregion

    #region Testes de Mapeamento Completo

    [Fact(DisplayName = "Handle deve preservar todos os dados do cartão")]
    [Trait("Categoria", "PagamentoEventHandler - Mapeamento")]
    public async Task Handle_DevePreservarTodosDadosCartao()
    {
        // Arrange
        var evento = new CursoCompradoIntegrationEvent(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            299.90m,
            "Maria Santos Silva",
            "5555555555554444",
            "06/28",
            "789"
        );

        PagamentoCurso? pagamentoCursoCapturado = null;
        _pagamentoServiceMock
            .Setup(s => s.RealizarPagamentoCurso(It.IsAny<PagamentoCurso>()))
            .Callback<PagamentoCurso>(pc => pagamentoCursoCapturado = pc)
            .ReturnsAsync(new Transacao());

        // Act
        await _handler.Handle(evento, CancellationToken.None);

        // Assert
        pagamentoCursoCapturado.Should().NotBeNull();
        pagamentoCursoCapturado!.NomeCartao.Should().Be("Maria Santos Silva");
        pagamentoCursoCapturado.NumeroCartao.Should().Be("5555555555554444");
        pagamentoCursoCapturado.ExpiracaoCartao.Should().Be("06/28");
        pagamentoCursoCapturado.CvvCartao.Should().Be("789");
    }

    [Fact(DisplayName = "Handle deve mapear corretamente todos os GUIDs")]
    [Trait("Categoria", "PagamentoEventHandler - Mapeamento")]
    public async Task Handle_DeveMapearCorretamenteTodosGuids()
    {
        // Arrange
        var matriculaId = Guid.NewGuid();
        var alunoId = Guid.NewGuid();
        var cursoId = Guid.NewGuid();

        var evento = new CursoCompradoIntegrationEvent(
            matriculaId,
            cursoId,
            alunoId,
            199.90m,
            "João",
            "4111111111111111",
            "12/29",
            "123"
        );

        PagamentoCurso? pagamentoCursoCapturado = null;
        _pagamentoServiceMock
            .Setup(s => s.RealizarPagamentoCurso(It.IsAny<PagamentoCurso>()))
            .Callback<PagamentoCurso>(pc => pagamentoCursoCapturado = pc)
            .ReturnsAsync(new Transacao());

        // Act
        await _handler.Handle(evento, CancellationToken.None);

        // Assert
        pagamentoCursoCapturado.Should().NotBeNull();
        pagamentoCursoCapturado!.MatriculaId.Should().Be(matriculaId);
        pagamentoCursoCapturado.AlunoId.Should().Be(alunoId);
        pagamentoCursoCapturado.CursoId.Should().Be(cursoId);
    }

    #endregion

    #region Testes de Cenários Reais

    [Fact(DisplayName = "Cenário: Processar compra de curso básico")]
    [Trait("Categoria", "PagamentoEventHandler - Cenário")]
    public async Task Cenario_ProcessarCompraCursoBasico()
    {
        // Arrange - Aluno compra curso básico
        var evento = new CursoCompradoIntegrationEvent(
            Guid.NewGuid(),  // MatriculaId
            Guid.NewGuid(),  // CursoId
            Guid.NewGuid(),  // AlunoId
            99.90m,          // Curso barato
            "Pedro Costa",
            "4111111111111111",
            "12/26",
            "123"
        );

        _pagamentoServiceMock
            .Setup(s => s.RealizarPagamentoCurso(It.IsAny<PagamentoCurso>()))
            .ReturnsAsync(new Transacao { StatusTransacaoId = StatusTransacao.Aprovado.Id });

        // Act
        await _handler.Handle(evento, CancellationToken.None);

        // Assert
        _pagamentoServiceMock.Verify(
            s => s.RealizarPagamentoCurso(It.Is<PagamentoCurso>(pc => pc.Total == 99.90m)),
            Times.Once
        );
    }

    [Fact(DisplayName = "Cenário: Processar compra de curso premium")]
    [Trait("Categoria", "PagamentoEventHandler - Cenário")]
    public async Task Cenario_ProcessarCompraCursoPremium()
    {
        // Arrange - Aluno compra curso caro
        var evento = new CursoCompradoIntegrationEvent(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            1499.90m,  // Curso premium
            "Ana Oliveira",
            "5555555555554444",
            "11/30",
            "999"
        );

        _pagamentoServiceMock
            .Setup(s => s.RealizarPagamentoCurso(It.IsAny<PagamentoCurso>()))
            .ReturnsAsync(new Transacao { StatusTransacaoId = StatusTransacao.Aprovado.Id });

        // Act
        await _handler.Handle(evento, CancellationToken.None);

        // Assert
        _pagamentoServiceMock.Verify(
            s => s.RealizarPagamentoCurso(It.Is<PagamentoCurso>(pc => pc.Total == 1499.90m)),
            Times.Once
        );
    }

    [Fact(DisplayName = "Cenário: Processar múltiplas compras sequenciais")]
    [Trait("Categoria", "PagamentoEventHandler - Cenário")]
    public async Task Cenario_ProcessarMultiplasComprasSequenciais()
    {
        // Arrange
        var evento1 = GerarCursoCompradoEvent();
        var evento2 = GerarCursoCompradoEvent();
        var evento3 = GerarCursoCompradoEvent();

        _pagamentoServiceMock
            .Setup(s => s.RealizarPagamentoCurso(It.IsAny<PagamentoCurso>()))
            .ReturnsAsync(new Transacao { StatusTransacaoId = StatusTransacao.Aprovado.Id });

        // Act
        await _handler.Handle(evento1, CancellationToken.None);
        await _handler.Handle(evento2, CancellationToken.None);
        await _handler.Handle(evento3, CancellationToken.None);

        // Assert
        _pagamentoServiceMock.Verify(
            s => s.RealizarPagamentoCurso(It.IsAny<PagamentoCurso>()),
            Times.Exactly(3)
        );
    }

    #endregion

    #region Testes de Integração com Service

    [Fact(DisplayName = "Handle deve aguardar conclusão do RealizarPagamentoCurso")]
    [Trait("Categoria", "PagamentoEventHandler - Integração")]
    public async Task Handle_DeveAguardarConclusaoRealizarPagamento()
    {
        // Arrange
        var evento = GerarCursoCompradoEvent();
        var taskCompleted = false;

        _pagamentoServiceMock
            .Setup(s => s.RealizarPagamentoCurso(It.IsAny<PagamentoCurso>()))
            .ReturnsAsync(() =>
            {
                taskCompleted = true;
                return new Transacao();
            });

        // Act
        await _handler.Handle(evento, CancellationToken.None);

        // Assert
        taskCompleted.Should().BeTrue();
    }

    [Fact(DisplayName = "Handle deve propagar exceções do PagamentoService")]
    [Trait("Categoria", "PagamentoEventHandler - Error Handling")]
    public async Task Handle_DevePropagandoExcecoesDoService()
    {
        // Arrange
        var evento = GerarCursoCompradoEvent();

        _pagamentoServiceMock
            .Setup(s => s.RealizarPagamentoCurso(It.IsAny<PagamentoCurso>()))
            .ThrowsAsync(new InvalidOperationException("Erro no pagamento"));

        // Act
        Func<Task> act = async () => await _handler.Handle(evento, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Erro no pagamento");
    }

    #endregion

    #region Helpers

    private static CursoCompradoIntegrationEvent GerarCursoCompradoEvent()
    {
        var faker = new Faker("pt_BR");
        return new CursoCompradoIntegrationEvent(
            Guid.NewGuid(),  // MatriculaId
            Guid.NewGuid(),  // CursoId
            Guid.NewGuid(),  // AlunoId
            faker.Random.Decimal(50, 1500),
            faker.Name.FullName(),
            faker.Finance.CreditCardNumber(),
            $"{faker.Random.Int(1, 12):D2}/{faker.Random.Int(25, 35)}",
            faker.Random.Int(100, 999).ToString()
        );
    }

    #endregion
}
