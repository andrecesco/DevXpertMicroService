using EduOnline.Alunos.Application.Events;
using EduOnline.Alunos.Data.Context;
using EduOnline.Alunos.Domain.Enumeradores;
using EduOnline.Alunos.Domain.Interfaces;
using EduOnline.Alunos.Domain.Models;
using EduOnline.Alunos.Domain.ValueObjects;
using EduOnline.Core.Communication.Mediator;
using EduOnline.Core.Mensagens;
using EduOnline.Core.Mensagens.IntegrationEvents;
using EduOnline.Core.Mensagens.Notifications;
using MediatR;

namespace EduOnline.Alunos.Application.Commands;

public class AlunoCommandHandler(IAlunoRepository repository, IMediatorHandler mediatorHandler) : IRequestHandler<AdicionarAlunoCommand, bool>,
    IRequestHandler<AlterarAlunoCommand, bool>,
    IRequestHandler<AdicionarMatriculaCommand, bool>,
    IRequestHandler<AtualizarHistoricoCommand, bool>,
    IRequestHandler<GerarCertificadoCommand, bool>,
    IRequestHandler<MatriculaPagaCommand, bool>,
    IRequestHandler<MatriculaRecusadaCommand, bool>
{
    public static DateOnly ValidadeMatricula => DateOnly.FromDateTime(DateTime.Now.AddYears(1));

    public async Task<bool> Handle(AdicionarAlunoCommand request, CancellationToken cancellationToken)
    {
        if (!ValidarComando(request)) return false;

        if (await AlunoExistente(request.Email)) return false;

        var aluno = new Aluno(request.AggregateId, request.Nome, request.Email, null);

        repository.Adicionar(aluno);

        return await repository.UnitOfWork.Commit();
    }

    public async Task<bool> Handle(AlterarAlunoCommand request, CancellationToken cancellationToken)
    {
        if (!ValidarComando(request)) return false;

        var aluno = await repository.ObterPorId(request.AggregateId);

        if (aluno is null)
        {
            await mediatorHandler.PublicarNotificacao(new DomainNotification("AlunoNaoEncontrado", "Não foi encontrado o Aluno"));
            return false;
        }

        aluno.AlterarNome(request.Nome ?? string.Empty);
        aluno.AlterarDataNascimento(request.DataNascimento);

        repository.Atualizar(aluno);

        await repository.UnitOfWork.Commit();

        return true;
    }

    public async Task<bool> Handle(AdicionarMatriculaCommand request, CancellationToken cancellationToken)
    {
        if (!ValidarComando(request)) return false;

        var aluno = await repository.ObterPorId(request.AlunoId);
        if (aluno is null)
        {
            await mediatorHandler.PublicarNotificacao(new DomainNotification("AlunoNaoEncontrado", "Não foi encontrado o Aluno para matricular"));
            return false;
        }

        var matricula = new Matricula(request.AggregateId, request.AlunoId, request.CursoId, request.CursoNome ?? string.Empty, ValidadeMatricula);
        var historico = new HistoricoAprendizagem(request.TotalAulas, []);
        matricula.AdicionarHistoricoAprendizagem(historico);

        repository.AdicionarMatricula(aluno, matricula);

        matricula.AdicionarEvento(new CursoCompradoIntegrationEvent(matricula.Id, request.CursoId, request.AlunoId, request.Valor, request.NomeCartao ?? string.Empty, request.NumeroCartao ?? string.Empty, request.ExpiracaoCartao ?? string.Empty, request.CvvCartao ?? string.Empty));

        await repository.UnitOfWork.Commit();

        return true;
    }

    public async Task<bool> Handle(AtualizarHistoricoCommand request, CancellationToken cancellationToken)
    {
        if (!ValidarComando(request)) return false;
        var matricula = await repository.ObterMatriculaPorId(request.MatriculaId);
        if (matricula is null)
        {
            await mediatorHandler.PublicarNotificacao(new DomainNotification("MatriculaNaoEncontrada", "Não foi encontrada a Matrícula para atualizar o histórico"));
            return false;
        }

        if(matricula.PagamentoStatusId != PagamentoStatus.Pago.Id)
        {
            await mediatorHandler.PublicarNotificacao(new DomainNotification("CursoNaoPago", "O pagamento do curso ainda não foi confirmado"));
            return false;
        }

        if (matricula.HistoricoAprendizagem?.AulaFoiConcluida(request.AulaId) ?? false)
        {
            await mediatorHandler.PublicarNotificacao(new DomainNotification("AulaJaConcluida", "A aula já foi concluída anteriormente"));
            return false;
        }

        matricula.HistoricoAprendizagem?.AdicionarAulaConcluida(request.AulaId);
        matricula.AtualizarStatusParaIniciado();

        repository.AtualizarMatricula(matricula);

        if (matricula?.HistoricoAprendizagem?.TodasAulasConcluidas() ?? false)
        {
            matricula.AdicionarEvento(new CursoFinalizadoEvent(matricula.Id));
        }

        return await repository.UnitOfWork.Commit();
    }

    public async Task<bool> Handle(GerarCertificadoCommand request, CancellationToken cancellationToken)
    {
        var matricula = await repository.ObterMatriculaPorId(request.MatriculaId);
        if (matricula is null)
        {
            await mediatorHandler.PublicarNotificacao(new DomainNotification("MatriculaNaoEncontrada", "Não foi encontrada a Matrícula para gerar o certificado"));
            return false;
        }

        if (matricula.HistoricoAprendizagem == null || !matricula.HistoricoAprendizagem.TodasAulasConcluidas())
        {
            await mediatorHandler.PublicarNotificacao(new DomainNotification("HistoricoIncompleto", "O histórico de aprendizagem não está completo para gerar o certificado"));
            return false;
        }

        var certificado = new Certificado(Guid.NewGuid(), matricula.Id, $"http://eduonline.com.br/{request.MatriculaId}/certificados");

        matricula.AlterarMatriculaParaFinalizado();

        repository.AdicionarCertificado(matricula, certificado);

        return await repository.UnitOfWork.Commit();
    }

    public async Task<bool> Handle(MatriculaPagaCommand request, CancellationToken cancellationToken)
    {
        var matricula = await repository.ObterMatriculaPorId(request.AggregateId);

        if(matricula is null)
        {
            await mediatorHandler.PublicarNotificacao(new DomainNotification("MatriculaNaoEncontrada", "Não foi encontrada a Matrícula para atualizar o pagamento"));
            return false;
        }

        matricula.AltararStatusPagamento(PagamentoStatus.Pago);

        repository.AtualizarMatricula(matricula);

        return await repository.UnitOfWork.Commit();
    }

    public async Task<bool> Handle(MatriculaRecusadaCommand request, CancellationToken cancellationToken)
    {
        var matricula = await repository.ObterMatriculaPorId(request.AggregateId);

        if (matricula is null)
        {
            await mediatorHandler.PublicarNotificacao(new DomainNotification("MatriculaNaoEncontrada", "Não foi encontrada a Matrícula para atualizar o pagamento"));
            return false;
        }

        matricula.AltararStatusPagamento(PagamentoStatus.Recusado);

        repository.AtualizarMatricula(matricula);

        return await repository.UnitOfWork.Commit();
    }

    private async Task<bool> AlunoExistente(string? email)
    {
        var aluno = await repository.BuscarAsync(a => a.Email == email);
        if (aluno != null && aluno.Count > 0)
        {
            await mediatorHandler.PublicarNotificacao(new DomainNotification("AlunoExistente", "Já existe um aluno cadastrado com esse Id"));
            return true;
        }
        return false;
    }

    private bool ValidarComando(Command message)
    {
        if (message.EhValido()) return true;

        foreach (var error in message.ValidationResult?.Errors ?? [])
        {
            mediatorHandler.PublicarNotificacao(new DomainNotification(message.MessageType, error.ErrorMessage));
        }

        return false;
    }
}
