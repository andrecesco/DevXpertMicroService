using EduOnline.Core.Mensagens;

namespace EduOnline.Core.DomainObjects;

public abstract class Entity
{
    public Guid Id { get; set; }

    public DateTime DataCriacao { get; set; }

    private List<Event>? _notificacoes;
    public IReadOnlyCollection<Event>? Notificacoes => _notificacoes?.AsReadOnly();

    protected Entity()
    {
        Id = Guid.NewGuid();
    }

    public void AdicionarEvento(Event evento)
    {
        _notificacoes = _notificacoes ?? [];
        _notificacoes.Add(evento);
    }

    public void RemoverEvento(Event eventItem)
    {
        _notificacoes?.Remove(eventItem);
    }

    public void LimparEventos()
    {
        _notificacoes?.Clear();
    }

    public override bool Equals(object? obj)
    {
        if (obj == null) return false;

        var compareTo = obj as Entity;

        if (ReferenceEquals(this, compareTo)) return true;
        if (compareTo is null) return false;

        return Id.Equals(compareTo.Id);
    }

    public static bool operator ==(Entity a, Entity b)
    {
        if (a is null && b is null)
            return true;

        if (a is null || b is null)
            return false;

        return a.Equals(b);
    }

    public static bool operator !=(Entity a, Entity b)
    {
        return !(a == b);
    }

    public override int GetHashCode()
    {
        return (GetType().GetHashCode() * 907) + Id.GetHashCode();
    }

    public override string ToString()
    {
        return $"{GetType().Name} [Id={Id}]";
    }
}
