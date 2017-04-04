using System.Data.Common;

namespace Trolley
{
    public interface IRepositoryContext
    {
        DbConnection Connection { get; }
        DbTransaction Transaction { get; }
        IRepository RepositoryFor();
        IRepository<TEntity> RepositoryFor<TEntity>() where TEntity : class, new();
    }
}
