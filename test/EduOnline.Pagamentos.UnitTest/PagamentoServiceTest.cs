using EduOnline.Core.Communication.Mediator;
using EduOnline.Core.Data;
using EduOnline.Core.DomainObjects.Dtos;
using EduOnline.Core.Mensagens.IntegrationEvents;
using EduOnline.Core.Mensagens.Notifications;
using EduOnline.Pagamentos.Domain;
using Moq;
using Moq.AutoMock;

namespace EduOnline.Pagamentos.UnitTest;

public class PagamentoServiceTest
{
    [Fact]
    public async Task RealizarPagamentoCurso_Aprovado_PersistePagamentoTransacaoECommit()
    {
        // Arrange
        var mocker = new AutoMocker();

        var pagamentoCurso = new PagamentoCurso
        {
            MatriculaId = Guid.NewGuid(),
            CursoId = Guid.NewGuid(),
            AlunoId = Guid.NewGuid(),
            Total = 199.90m,
            NomeCartao = "Fulano de Tal",
            NumeroCartao = "4111111111111111",
            ExpiracaoCartao = "12/29",
            CvvCartao = "123"
        };

        var transacaoAprovada = new Transacao
        {
            Id = Guid.NewGuid(),
            Total = pagamentoCurso.Total,
            StatusTransacaoId = StatusTransacao.Aprovado.Id
        };

        var facadeMock = mocker.GetMock<IPagamentoCartaoCreditoFacade>();
        facadeMock.Setup(f => f.RealizarPagamento(It.IsAny<Curso>(), It.IsAny<Pagamento>()))
                  .Returns(transacaoAprovada);

        var repoMock = mocker.GetMock<IPagamentoRepository>();
        var uowMock = mocker.GetMock<IUnitOfWork>();
        repoMock.Setup(r => r.UnitOfWork).Returns(uowMock.Object);
        uowMock.Setup(u => u.Commit()).ReturnsAsync(true);

        Pagamento? pagamentoAdicionado = null;
        repoMock.Setup(r => r.Adicionar(It.IsAny<Pagamento>()))
                .Callback<Pagamento>(p => pagamentoAdicionado = p);

        var service = mocker.CreateInstance<PagamentoService>();

        // Act
        var resultado = await service.RealizarPagamentoCurso(pagamentoCurso);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(StatusTransacao.Aprovado.Id, resultado.StatusTransacaoId);
        Assert.Equal(pagamentoCurso.Total, resultado.Total);

        repoMock.Verify(r => r.Adicionar(It.IsAny<Pagamento>()), Times.Once);
        repoMock.Verify(r => r.AdicionarTransacao(It.Is<Transacao>(t => t.StatusTransacaoId == StatusTransacao.Aprovado.Id)), Times.Once);
        uowMock.Verify(u => u.Commit(), Times.Once);

        // Não deve publicar notificações em caso aprovado
        mocker.GetMock<IMediatorHandler>().Verify(m => m.PublicarNotificacao(It.IsAny<DomainNotification>()), Times.Never);

        // O evento de PagamentoRealizado deve ser adicionado ao pagamento antes do commit
        Assert.NotNull(pagamentoAdicionado);
        Assert.NotNull(pagamentoAdicionado!.Notificacoes);
        Assert.Contains(pagamentoAdicionado.Notificacoes!, e => e is PagamentoRealizadoEvent);
    }

    [Fact]
    public async Task RealizarPagamentoCurso_Recusado_PublicaNotificacaoEEventoRecusado_SemPersistencia()
    {
        // Arrange
        var mocker = new AutoMocker();

        var pagamentoCurso = new PagamentoCurso
        {
            MatriculaId = Guid.NewGuid(),
            CursoId = Guid.NewGuid(),
            AlunoId = Guid.NewGuid(),
            Total = 299.90m,
            NomeCartao = "Beltrano",
            NumeroCartao = "5555555555554444",
            ExpiracaoCartao = "11/28",
            CvvCartao = "456"
        };

        var transacaoRecusada = new Transacao
        {
            Id = Guid.NewGuid(),
            Total = pagamentoCurso.Total,
            StatusTransacaoId = StatusTransacao.Recusado.Id
        };

        var facadeMock = mocker.GetMock<IPagamentoCartaoCreditoFacade>();
        facadeMock.Setup(f => f.RealizarPagamento(It.IsAny<Curso>(), It.IsAny<Pagamento>()))
                  .Returns(transacaoRecusada);

        var repoMock = mocker.GetMock<IPagamentoRepository>();
        var uowMock = mocker.GetMock<IUnitOfWork>();
        repoMock.Setup(r => r.UnitOfWork).Returns(uowMock.Object);

        var mediatorMock = mocker.GetMock<IMediatorHandler>();

        var service = mocker.CreateInstance<PagamentoService>();

        // Act
        var resultado = await service.RealizarPagamentoCurso(pagamentoCurso);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(StatusTransacao.Recusado.Id, resultado.StatusTransacaoId);
        Assert.Equal(pagamentoCurso.Total, resultado.Total);

        // Não deve persistir pagamento/transação nem realizar commit
        repoMock.Verify(r => r.Adicionar(It.IsAny<Pagamento>()), Times.Never);
        repoMock.Verify(r => r.AdicionarTransacao(It.IsAny<Transacao>()), Times.Never);
        uowMock.Verify(u => u.Commit(), Times.Never);

        // Deve publicar a DomainNotification e o evento PagamentoRecusado
        mediatorMock.Verify(m => m.PublicarNotificacao(It.Is<DomainNotification>(n => n.Key == "pagamento")), Times.Once);
        mediatorMock.Verify(m => m.PublicarEvento(It.Is<PagamentoRecusadoEvent>(e => e.AggregateId == pagamentoCurso.MatriculaId && e.Total == pagamentoCurso.Total)), Times.Once);
    }
}
