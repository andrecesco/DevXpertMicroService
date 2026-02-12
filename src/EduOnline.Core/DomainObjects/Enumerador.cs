using System.Reflection;

namespace EduOnline.Core.DomainObjects;

public abstract class Enumerador(int id, string nome) : IComparable
{
    #region Properties
    public string Nome { get; private set; } = nome;

    public int Id { get; private set; } = id;

    #endregion

    public override string ToString() => Nome;

    public override int GetHashCode()
    {
        return HashCode.Combine(GetType(), Id);
    }

    public virtual bool Equals(Enumerador? other)
    {
        return other is not null
            && GetType().Equals(other.GetType())
            && Equals(Id, other.Id)
            && Equals(Nome, other.Nome);
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Enumerador otherValue)
        {
            return false;
        }

        var typeMatches = GetType().Equals(obj.GetType());
        var valueMatches = Id.Equals(otherValue.Id);

        return typeMatches && valueMatches;
    }

    public int CompareTo(object other) => Id.CompareTo(((Enumerador)other).Id);

    #region Static utils
    public static IEnumerable<T> GetAll<T>() where T : Enumerador
    {
        var fields = typeof(T).GetProperties(BindingFlags.Public
                                                      | BindingFlags.Static
                                                      | BindingFlags.DeclaredOnly);

        return fields.Select(s => s.GetValue(null)).Cast<T>();
    }

    public static T? GetById<T>(int id) where T : Enumerador => GetAll<T>().SingleOrDefault(s => s.Id.Equals(id));

    public static T? GetByNome<T>(string nome) where T : Enumerador => GetAll<T>().SingleOrDefault(s => s.Nome.Equals(nome, StringComparison.InvariantCultureIgnoreCase));
    #endregion
}