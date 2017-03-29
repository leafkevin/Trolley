namespace Trolley
{
    public interface IRepositoryContext
    {
        IRepository RepositoryFor();
        IRepository<TEntity> RepositoryFor<TEntity>() where TEntity : class, new();
    }
}
