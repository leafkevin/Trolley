using System.Data;
using System.Data.Common;

namespace Trolley
{
    public class RepositoryContext : IRepositoryContext
    {
        protected DbConnection Connection { get; set; }
        protected DbTransaction Transaction { get; set; }
        public string ConnString { get; set; }
        public RepositoryContext(string connString)
        {
            this.ConnString = connString;
            var provider = OrmProviderFactory.GetProvider(this.ConnString);
            this.Connection = provider.CreateConnection(this.ConnString);
        }
        public void Begin()
        {
            this.Open();
            this.Transaction = this.Connection.BeginTransaction();
        }
        public void Commit()
        {
            if (this.Transaction != null)
            {
                this.Transaction.Commit();
            }
        }
        public virtual IRepository RepositoryFor()
        {
            return new Repository(this.ConnString, this.Transaction);
        }
        public virtual IRepository<TEntity> RepositoryFor<TEntity>() where TEntity : class, new()
        {
            return new Repository<TEntity>(this.ConnString, this.Transaction);
        }
        public void Rollback()
        {
            if (this.Transaction != null)
            {
                this.Transaction.Rollback();
            }
        }
        public void Dispose()
        {
            if (this.Transaction != null)
            {
                this.Transaction.Dispose();
            }
            if (this.Connection != null)
            {
                this.Connection.Dispose();
            }
        }
        private void Open()
        {
            if (this.Connection.State == ConnectionState.Broken)
            {
                this.Connection.Close();
            }
            if (this.Connection.State == ConnectionState.Closed)
            {
                this.Connection.Open();
            }
        }
        public void Close()
        {
            this.Connection.Close();
        }
    }
}
