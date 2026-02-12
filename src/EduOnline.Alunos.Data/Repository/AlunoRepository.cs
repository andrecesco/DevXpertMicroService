using EduOnline.Alunos.Data.Context;
using EduOnline.Alunos.Domain.Interfaces;
using EduOnline.Alunos.Domain.Models;
using EduOnline.Alunos.Domain.ValueObjects;
using EduOnline.Core.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace EduOnline.Alunos.Data.Repository;

public class AlunoRepository(AlunosContext context) : IAlunoRepository
{
    public IUnitOfWork UnitOfWork => context;

    public Task<List<Aluno>> ObterTodos()
    {
        return context.Alunos
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Aluno?> ObterPorId(Guid id)
    {
        return await context.Alunos
            .Include(a => a.Matriculas)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<List<Aluno>> BuscarAsync(Expression<Func<Aluno, bool>> predicate)
    {
        return await context.Alunos
            .Where(predicate)
            .AsNoTracking()
            .ToListAsync();
    }

    public void Adicionar(Aluno aluno)
    {
        context.Alunos.Add(aluno);
    }

    public void Atualizar(Aluno aluno)
    {
        context.Alunos.Update(aluno);
    }

    public async Task<Matricula?> ObterMatriculaPorId(Guid id)
    {
        return await context.Matriculas
            .Include(a => a.Certificado)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<List<Matricula>> ObterMatriculasPorAlunoId(Guid alunoId)
    {
        return await context.Matriculas
            .Where(m => m.AlunoId == alunoId)
            .Include(m => m.Certificado)
            .AsNoTracking()
            .OrderBy(m => m.DataCriacao)
            .ToListAsync();
    }

    public async Task<Matricula?> ObterMatriculaPorCursoPorAlunoId(Guid cursoId, Guid alunoId)
    {
        return await context.Matriculas
            .FirstOrDefaultAsync(m => m.CursoId == cursoId && m.AlunoId == alunoId);
    }

    public Task<Certificado?> ObterCertificadoPorMatriculaId(Guid matriculaId)
    {
        return context.Certificados
            .FirstOrDefaultAsync(c => c.MatriculaId == matriculaId);
    }

    public void AdicionarMatricula(Aluno aluno, Matricula matricula)
    {
        aluno.AdicionarMatricula(matricula);
        context.Matriculas.Add(matricula);
        context.Alunos.Update(aluno);
    }

    public void AtualizarMatricula(Matricula matricula)
    {
        context.Matriculas.Update(matricula);
    }

    public void AdicionarCertificado(Matricula matricula, Certificado certificado)
    {
        context.Certificados.Add(certificado);
        context.Matriculas.Update(matricula);
    }

    public void Dispose()
    {
        context.Dispose();
        GC.SuppressFinalize(this);
    }
}
