using EduOnline.Core.Mensagens;

namespace EduOnline.Alunos.Application.Commands;

public class GerarCertificadoCommand(Guid matriculaId) : Command
{
    public Guid MatriculaId { get; private set; } = matriculaId;
}
