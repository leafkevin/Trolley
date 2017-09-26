using System;
using System.Data.Common;
using System.Threading.Tasks;

namespace Trolley
{
    public interface IRepositoryContext: IDisposable
    {
        DbConnection Connection { get; }
        DbTransaction Transaction { get; }
        IRepository RepositoryFor();
        IRepository<TEntity> RepositoryFor<TEntity>() where TEntity : class, new();
        void Begin();
        Task BeginAsync();
        void Commit();
        void Rollback();
        void Close();
    }
}
