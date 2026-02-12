using EduOnline.Alunos.Application.Commands;
using EduOnline.Alunos.Domain.Enumeradores;
using EduOnline.Alunos.Domain.Interfaces;
using EduOnline.Alunos.Domain.Models;
using EduOnline.Core.Communication.Mediator;
using EduOnline.Core.Mensagens.Notifications;
using Moq;
using System.Linq.Expressions;

namespace EduOnline.Alunos.UnitTest;

public class AlunoCommandHandlerTest
{
    private readonly Mock<IAlunoRepository> _repositoryMock;
    private readonly Mock<IMediatorHandler> _mediatorMock;

    public AlunoCommandHandlerTest()
    {
        _repositoryMock = new();
        _mediatorMock = new();
    }

    [Fact]
    public async Task AlunoCommandHandler_AdicionarAlunoCommand_DeveRetornarFalseQuandoComandoInvalido()
    {
        //Arrange
        var command = new AdicionarAlunoCommand(Guid.Empty, "", "");
        var handler = new AlunoCommandHandler(_repositoryMock.Object, _mediatorMock.Object);

        //Act
        var result = await handler.Handle(command, CancellationToken.None);

        //Assert
        Assert.False(result);
        _mediatorMock.Verify(m => m.PublicarNotificacao(It.IsAny<DomainNotification>()), Times.Exactly(2));
    }

    [Fact]
    public async Task AlunoCommandHandler_AdicionarAlunoCommand_DeveRetornarFalseQuandoAlunoExistente()
    {
        //Arrange
        var command = new AdicionarAlunoCommand(Guid.Empty, "Nome Completo", "email@email.com");
        var aluno = new Aluno(Guid.NewGuid(), "Nome", "email@email.com", DateOnly.FromDateTime(DateTime.Now.AddYears(-18)));

        _repositoryMock.Setup(r => r.BuscarAsync(It.IsAny<Expression<Func<Aluno, bool>>>()))
            .ReturnsAsync([aluno]);

        var handler = new AlunoCommandHandler(_repositoryMock.Object, _mediatorMock.Object);

        //Act
        var result = await handler.Handle(command, CancellationToken.None);

        //Assert
        Assert.False(result);
        _mediatorMock.Verify(m => m.PublicarNotificacao(It.IsAny<DomainNotification>()), Times.Once);
    }

    [Fact]
    public async Task AlunoCommandHandler_AdicionarAlunoCommand_DeveRetornarTrueQuandoSucesso()
    {
        //Arrange
        var command = new AdicionarAlunoCommand(Guid.NewGuid(), "Nome", "email@email.com");

        _repositoryMock.Setup(r => r.BuscarAsync(It.IsAny<Expression<Func<Aluno, bool>>>()))
            .ReturnsAsync([]);

        _repositoryMock.Setup(r => r.Adicionar(It.IsAny<Aluno>()));
        _repositoryMock.Setup(r => r.UnitOfWork.Commit()).Returns(Task.FromResult(true));

        var handler = new AlunoCommandHandler(_repositoryMock.Object, _mediatorMock.Object);

        //Act
        var result = await handler.Handle(command, CancellationToken.None);

        //Assert
        Assert.True(result);
    }

    [Fact]
    public async Task AlunoCommandHandler_AlterarAlunoCommand_DeveRetornarFalseQuandoComandoInvalido()
    {
        //Arrange
        var command = new AlterarAlunoCommand(Guid.NewGuid(), null, DateOnly.MinValue);
        var handler = new AlunoCommandHandler(_repositoryMock.Object, _mediatorMock.Object);

        //Act
        var result = await handler.Handle(command, CancellationToken.None);

        //Assert
        Assert.False(result);
        _mediatorMock.Verify(m => m.PublicarNotificacao(It.IsAny<DomainNotification>()), Times.Exactly(2));
    }

    [Fact]
    public async Task AlunoCommandHandler_AlterarAlunoCommand_DeveRetornarFalseQuandoAlunoNaoEncontrado()
    {
        //Arrange
        var command = new AlterarAlunoCommand(Guid.NewGuid(), "NovoNome", DateOnly.FromDateTime(DateTime.Now.AddYears(-18)));
        _repositoryMock.Setup(r => r.ObterPorId(It.IsAny<Guid>()))
            .ReturnsAsync((Aluno)null);

        var handler = new AlunoCommandHandler(_repositoryMock.Object, _mediatorMock.Object);

        //Act
        var result = await handler.Handle(command, CancellationToken.None);

        //Assert
        Assert.False(result);
        _mediatorMock.Verify(m => m.PublicarNotificacao(It.IsAny<DomainNotification>()), Times.Once);
    }

    [Fact]
    public async Task AlunoCommandHandler_AlterarAlunoCommand_DeveRetornarTrueQuandoSucesso()
    {
        //Arrange
        var aluno = new Aluno(Guid.NewGuid(), "Nome", "email@email.com", DateOnly.FromDateTime(DateTime.Now.AddYears(-18)));
        var command = new AlterarAlunoCommand(aluno.Id, "NovoNome", DateOnly.FromDateTime(DateTime.Now.AddYears(-18)));

        _repositoryMock.Setup(r => r.ObterPorId(It.IsAny<Guid>()))
            .ReturnsAsync(aluno);

        _repositoryMock.Setup(r => r.Atualizar(It.IsAny<Aluno>()));
        _repositoryMock.Setup(r => r.UnitOfWork.Commit()).Returns(Task.FromResult(true));

        var handler = new AlunoCommandHandler(_repositoryMock.Object, _mediatorMock.Object);

        //Act
        var result = await handler.Handle(command, CancellationToken.None);

        //Assert
        Assert.True(result);
    }

    [Fact]
    public async Task AlunoCommandHandler_AdicionarMatriculaCommand_DeveRetornarFalseQuandoComandoInvalido()
    {
        //Arrange
        var command = new AdicionarMatriculaCommand(Guid.Empty, Guid.Empty, null, 0, null, null, null, null, 0);

        //Act
        var handler = new AlunoCommandHandler(_repositoryMock.Object, _mediatorMock.Object);
        var result = await handler.Handle(command, CancellationToken.None);

        //Assert
        Assert.False(result);
        _mediatorMock.Verify(m => m.PublicarNotificacao(It.IsAny<DomainNotification>()), Times.Exactly(9));
    }

    [Fact]
    public async Task AlunoCommandHandler_AdicionarMatriculaCommand_DeveRetornarFalseQuandoAlunoNaoEncontrado()
    {
        //Arrange
        var command = new AdicionarMatriculaCommand(Guid.NewGuid(), Guid.NewGuid(), "Curso", 1, "null", "null", "null", "null", 1);
        _repositoryMock.Setup(r => r.ObterPorId(It.IsAny<Guid>()))
            .ReturnsAsync((Aluno)null);

        //Act
        var handler = new AlunoCommandHandler(_repositoryMock.Object, _mediatorMock.Object);
        var result = await handler.Handle(command, CancellationToken.None);

        //Assert
        Assert.False(result);
        _mediatorMock.Verify(m => m.PublicarNotificacao(It.IsAny<DomainNotification>()), Times.Once);
    }

    [Fact]
    public async Task AlunoCommandHandler_AdicionarMatriculaCommand_DeveRetornarTrueQuandoSucesso()
    {
        // Arrange
        var aluno = new Aluno(Guid.NewGuid(), "Nome", "email@email.com", DateOnly.FromDateTime(DateTime.Now.AddYears(-18)));
        var command = new AdicionarMatriculaCommand(aluno.Id, Guid.NewGuid(), "Curso", 1, "null", "null", "null", "null", 1);

        _repositoryMock.Setup(r => r.ObterPorId(It.IsAny<Guid>()))
            .ReturnsAsync(aluno);

        _repositoryMock.Setup(r => r.Atualizar(It.IsAny<Aluno>()));
        _repositoryMock.Setup(r => r.UnitOfWork.Commit()).Returns(Task.FromResult(true));

        // Act
        var handler = new AlunoCommandHandler(_repositoryMock.Object, _mediatorMock.Object);
        var result = await handler.Handle(command, CancellationToken.None);

        //Assert
        Assert.True(result);
    }

    [Fact]
    public async Task AlunoCommandHandler_AtualizarHistorico_DeveRetornarFalseQuandoComandoInvalido()
    {
        //Arrange
        var command = new AtualizarHistoricoCommand(Guid.Empty, Guid.Empty);

        //Act
        var handler = new AlunoCommandHandler(_repositoryMock.Object, _mediatorMock.Object);
        var result = await handler.Handle(command, CancellationToken.None);

        //Assert
        Assert.False(result);
        _mediatorMock.Verify(m => m.PublicarNotificacao(It.IsAny<DomainNotification>()), Times.Exactly(2));
    }

    [Fact]
    public async Task AlunoCommandHandler_AtualizarHistorico_DeveRetornarFalseQuandoMatriculaNaoEncontrada()
    {
        //Arrange
        var command = new AtualizarHistoricoCommand(Guid.NewGuid(), Guid.NewGuid());
        _repositoryMock.Setup(r => r.ObterMatriculaPorId(It.IsAny<Guid>()))
            .ReturnsAsync((Matricula)null);

        //Act
        var handler = new AlunoCommandHandler(_repositoryMock.Object, _mediatorMock.Object);
        var result = await handler.Handle(command, CancellationToken.None);

        //Assert
        Assert.False(result);
        _mediatorMock.Verify(m => m.PublicarNotificacao(It.IsAny<DomainNotification>()), Times.Once);
    }

    [Fact]
    public async Task AlunoCommandHandler_AtualizarHistorico_DeveRetornarTrueQuandoSucesso()
    {
        //Arrange
        var matricula = new Matricula(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Curso", DateOnly.FromDateTime(DateTime.Now.AddYears(1)));
        var command = new AtualizarHistoricoCommand(matricula.Id, Guid.NewGuid());

        matricula.AltararStatusPagamento(PagamentoStatus.Pago);

        _repositoryMock.Setup(r => r.ObterMatriculaPorId(It.IsAny<Guid>()))
            .ReturnsAsync(matricula);

        _repositoryMock.Setup(r => r.AtualizarMatricula(It.IsAny<Matricula>()));
        _repositoryMock.Setup(r => r.UnitOfWork.Commit()).Returns(Task.FromResult(true));

        //Act
        var handler = new AlunoCommandHandler(_repositoryMock.Object, _mediatorMock.Object);
        var result = await handler.Handle(command, CancellationToken.None);

        //Assert
        Assert.True(result);
    }
}
