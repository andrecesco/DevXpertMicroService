using EduOnline.Alunos.Application.Queries.Dtos;
using System.Linq.Expressions;

namespace EduOnline.Alunos.Application.Queries;

public interface IAlunoQuery
{
    Task<List<AlunoDto>> ObterTodos();
    Task<AlunoDto> ObterPorId(Guid id);
    Task<List<AlunoDto>> BuscarAsync(Expression<Func<AlunoDto, bool>> predicate);
    Task<MatriculaDto> ObterMatriculaPorId(Guid id);
    Task<List<MatriculaDto>> ObterMatriculasPorAlunoId(Guid alunoId);
    Task<CertificadoDto> ObterCertificadoPorMatriculaId(Guid matriculaId);
}
