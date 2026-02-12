using EduOnline.Core.Mensagens;

namespace EduOnline.Alunos.Application.Events;

public class CursoFinalizadoEvent : Event
{
    public CursoFinalizadoEvent(Guid matriculaId)
    {
        AggregateId = matriculaId;
    }
}
