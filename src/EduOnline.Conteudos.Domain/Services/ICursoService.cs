namespace EduOnline.Conteudos.Domain.Services;

public interface ICursoService : IDisposable
{
    Task Adicionar(Curso curso);
    Task Atualizar(Curso curso);
    Task Inativar(Guid id);
    Task AdicionarAula(Guid cursoId, Aula aula);
    Task AtualizarAula(Guid cursoId, Aula aula);

}
