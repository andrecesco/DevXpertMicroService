using EduOnline.Core.Mensagens;

namespace EduOnline.Alunos.Application.Commands;

public class MatriculaRecusadaCommand : Command
{
    public MatriculaRecusadaCommand(Guid id)
    {
        AggregateId = id;
    }
}
