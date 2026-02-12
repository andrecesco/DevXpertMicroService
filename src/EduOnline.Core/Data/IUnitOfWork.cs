namespace EduOnline.Core.Data;

public interface IUnitOfWork
{
    Task<bool> Commit();
}
