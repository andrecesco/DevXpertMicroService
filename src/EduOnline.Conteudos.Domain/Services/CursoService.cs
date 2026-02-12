using EduOnline.Conteudos.Domain.ValidacoesServices;
using EduOnline.Core.Mensagens;

namespace EduOnline.Conteudos.Domain.Services;

public class CursoService(INotificador notificador, ICursoRepository repository) : BaseService(notificador), ICursoService
{
    public async Task Adicionar(Curso curso)
    {
        if (!ExecutarValidacao(new CursoValidation(), curso)) return;

        var cursoExistente = await repository.BuscarAsync(c => c.Nome == curso.Nome);
        if (cursoExistente is not null && cursoExistente.Count != 0)
        {
            Notificar("Já existe um curso cadastrado com este nome.");
            return;
        }

        repository.Adicionar(curso);

        await PersistirDados(repository.UnitOfWork);
    }

    public async Task Atualizar(Curso curso)
    {
        if (!ExecutarValidacao(new CursoValidation(), curso)) return;

        var cursoExistente = await repository.BuscarAsync(c => c.Nome == curso.Nome);
        if (cursoExistente is not null && cursoExistente.Count != 0 && !cursoExistente.Any(c => c.Id == curso.Id))
        {
            Notificar("Já existe um curso cadastrado com este nome.");
            return;
        }

        repository.Atualizar(curso);

        await PersistirDados(repository.UnitOfWork);
    }

    public async Task Inativar(Guid id)
    {
        var curso = await repository.ObterPorIdAsync(id);

        if(curso is null)
        {
            Notificar("Curso não encontrado.");
            return;
        }

        curso.Ativo = false;

        repository.Atualizar(curso);

        await PersistirDados(repository.UnitOfWork);
    }

    public async Task Ativar(Guid id)
    {
        var curso = await repository.ObterPorIdAsync(id);

        if (curso is null)
        {
            Notificar("Curso não encontrado.");
            return;
        }

        curso.Ativo = true;

        repository.Atualizar(curso);

        await PersistirDados(repository.UnitOfWork);
    }

    public async Task AdicionarAula(Guid cursoId, Aula aula)
    {
        if (!ExecutarValidacao(new AulaValidation(), aula)) return;

        var curso = await repository.ObterPorIdAsync(cursoId);
        if (curso is null)
        {
            Notificar("Curso não encontrado.");
            return;
        }

        if (curso.Aulas != null && curso.Aulas.Any(a => a.Titulo == aula.Titulo))
        {
            Notificar("Já existe uma aula com este nome neste curso.");
            return;
        }

        repository.AdicionarAula(aula);

        await PersistirDados(repository.UnitOfWork);
    }

    public async Task AtualizarAula(Guid cursoId, Aula aula)
    {
        if (!ExecutarValidacao(new AulaValidation(), aula)) return;

        var curso = await repository.ObterPorIdAsync(cursoId);
        if (curso is null)
        {
            Notificar("Curso não encontrado.");
            return;
        }

        if (curso.Aulas?.FirstOrDefault(a => a.Id == aula.Id) is null)
        {
            Notificar("Aula não encontrada neste curso.");
            return;
        }

        if (curso.Aulas != null && curso.Aulas.Any(a => a.Titulo == aula.Titulo && a.Id != aula.Id))
        {
            Notificar("Já existe uma aula com este Titulo neste curso.");
            return;
        }

        repository.AtualizarAula(aula);

        await PersistirDados(repository.UnitOfWork);
    }

    //public async Task RemoverAula(Guid cursoId, Guid aulaId)
    //{
    //    var curso = await repository.ObterPorIdAsync(cursoId);
    //    if (curso is null)
    //    {
    //        Notificar("Curso não encontrado.");
    //        return;
    //    }

    //    var aula = curso.Aulas?.FirstOrDefault(a => a.Id == aulaId);
    //    if (aula is null)
    //    {
    //        Notificar("Aula não encontrada neste curso.");
    //        return;
    //    }

    //    curso.Aulas ??= [];
    //    curso.Aulas = [.. curso.Aulas.Where(a => a.Id != aulaId)];
    //    repository.Atualizar(curso);
    //    await PersistirDados(repository.UnitOfWork);
    //}

    private async Task<bool> CursoExiste(Guid id)
    {
        var curso = await repository.ObterPorIdAsync(id);
        return curso is not null;
    }

    public void Dispose()
    {
        repository.Dispose();
        GC.SuppressFinalize(this);
    }
}
