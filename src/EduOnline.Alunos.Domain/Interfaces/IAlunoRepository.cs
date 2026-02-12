using EduOnline.Alunos.Domain.Models;
using EduOnline.Alunos.Domain.ValueObjects;
using EduOnline.Core.Data;
using System.Linq.Expressions;

namespace EduOnline.Alunos.Domain.Interfaces;

public interface IAlunoRepository : IRepository<Aluno>
{
    Task<List<Aluno>> ObterTodos();
    Task<Aluno?> ObterPorId(Guid id);
    Task<List<Aluno>> BuscarAsync(Expression<Func<Aluno, bool>> predicate);
    void Adicionar(Aluno aluno);
    void Atualizar(Aluno aluno);
    Task<Matricula?> ObterMatriculaPorId(Guid id);
    Task<List<Matricula>> ObterMatriculasPorAlunoId(Guid alunoId);
    Task<Matricula?> ObterMatriculaPorCursoPorAlunoId(Guid cursoId, Guid alunoId);
    Task<Certificado?> ObterCertificadoPorMatriculaId(Guid matriculaId);
    void AdicionarMatricula(Aluno aluno, Matricula matricula);
    void AtualizarMatricula(Matricula matricula);
    void AdicionarCertificado(Matricula matricula, Certificado certificado);
}
