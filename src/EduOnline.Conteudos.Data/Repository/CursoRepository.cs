using EduOnline.Conteudos.Data.Context;
using EduOnline.Conteudos.Domain;
using EduOnline.Core.Data;
using EduOnline.Core.DomainObjects;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace EduOnline.Conteudos.Data.Repository;

public class CursoRepository(ConteudosContext context) : ICursoRepository
{
    private readonly ConteudosContext _context = context;

    public IUnitOfWork UnitOfWork => _context;

    public async Task<List<Curso>> ObterTodosAsync()
    {
        return await _context.Cursos
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Curso?> ObterPorIdAsync(Guid id)
    {
        return await _context.Cursos
            .Include(a => a.Aulas)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<Curso?> ObterCursoValidoPorIdAsync(Guid id)
    {
        return await _context.Cursos
            .Include(a => a.Aulas)
            .FirstOrDefaultAsync(a => a.Id == id && a.Validade >= DateOnly.FromDateTime(DateTime.Now));
    }

    public async Task<List<Curso>> BuscarAsync(Expression<Func<Curso, bool>> predicate)
    {
        return await _context.Cursos
            .AsNoTracking()
            .Where(predicate)
            .ToListAsync();
    }

    public async Task<List<Aula>> ObterAulasPorCursoIdAsync(Guid cursoId)
    {
        return await _context.Aulas
            .Where(a => a.CursoId == cursoId)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Aula?> ObterAulaPorIdAsync(Guid id)
    {
        return await _context.Aulas
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<List<Aula>> BuscarAulasAsync(Expression<Func<Aula, bool>> predicate)
    {
        return await _context.Aulas
            .AsNoTracking()
            .Where(predicate)
            .ToListAsync();
    }

    public void Adicionar(Curso curso)
    {
        _context.Cursos.Add(curso);
    }

    public void Atualizar(Curso curso)
    {
        _context.Cursos.Update(curso);
    }

    public void AdicionarAula(Aula aula)
    {
        _context.Aulas.Add(aula);
    }

    public void AtualizarAula(Aula aula)
    {
        _context.Aulas.Update(aula);
    }

    public void Dispose()
    {
        _context?.Dispose();
        GC.SuppressFinalize(this);
    }
}
