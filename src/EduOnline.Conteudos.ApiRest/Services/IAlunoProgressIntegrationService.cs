namespace EduOnline.Conteudos.ApiRest.Services;

public interface IAlunoProgressIntegrationService
{
    Task<bool> AtualizarProgressoAsync(Guid alunoId, Guid matriculaId, Guid aulaId, CancellationToken cancellationToken = default);
}
