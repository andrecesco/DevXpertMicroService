using EduOnline.Core.Data;
using System.Linq.Expressions;

namespace EduOnline.Conteudos.Domain;

public interface ICursoRepository : IRepository<Curso>
{
    Task<Curso?> ObterPorIdAsync(Guid id);
    Task<Curso?> ObterCursoValidoPorIdAsync(Guid id);
    Task<List<Curso>> ObterTodosAsync();
    Task<List<Curso>> BuscarAsync(Expression<Func<Curso, bool>> predicate);
    Task<List<Aula>> ObterAulasPorCursoIdAsync(Guid cursoId);
    Task<Aula?> ObterAulaPorIdAsync(Guid id);
    Task<List<Aula>> BuscarAulasAsync(Expression<Func<Aula, bool>> predicate);
    void Adicionar(Curso curso);
    void Atualizar(Curso curso);
    void AdicionarAula(Aula aula);
    void AtualizarAula(Aula aula);
}
