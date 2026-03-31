using System.Diagnostics.CodeAnalysis;

namespace EduOnline.WebApps.ApiRest.Models;

[ExcludeFromCodeCoverage]
public class AtualizarHistoricoRequest
{
    public Guid MatriculaId { get; set; }
    public Guid CursoId { get; set; }
}
