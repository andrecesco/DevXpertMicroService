using EduOnline.Core.Mensagens;

namespace EduOnline.Alunos.Application.Commands;
public class MatriculaPagaCommand : Command
{
    public MatriculaPagaCommand(Guid id)
    {
        AggregateId = id;
    }
}
