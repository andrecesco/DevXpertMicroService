using EduOnline.Core.DomainObjects;

namespace EduOnline.Core.Data;

public interface IRepository<TEntity> : IDisposable where TEntity : Entity, IAggregateRoot
{
    IUnitOfWork UnitOfWork { get; }
}
