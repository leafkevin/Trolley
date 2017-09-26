using System;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace Trolley
{
    public class RepositoryContext : IRepositoryContext
    {
        protected string ConnString { get; private set; }
        public DbConnection Connection { get; private set; }
        public DbTransaction Transaction { get; private set; }
        public RepositoryContext()
        {
            this.ConnString = OrmProviderFactory.DefaultConnString;
            var provider = OrmProviderFactory.DefaultProvider;
            this.Connection = provider.CreateConnection(this.ConnString);
        }
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
        public async Task BeginAsync()
        {
            await this.OpenAsync();
            this.Transaction = this.Connection.BeginTransaction();
        }
        public void Commit()
        {
            if (this.Transaction != null) this.Transaction.Commit();
        }
        /// <summary>
        /// 获取无类型Repository对象，支持IOC重载
        /// </summary>
        /// <returns></returns>
        public virtual IRepository RepositoryFor() => new Repository(this.ConnString, this);
        /// <summary>
        /// 获取强类型Repository对象，支持IOC重载
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        public virtual IRepository<TEntity> RepositoryFor<TEntity>() where TEntity : class, new() => new Repository<TEntity>(this.ConnString, this);
        public void Rollback()
        {
            if (this.Transaction != null) this.Transaction.Rollback();
        }
        public void Dispose()
        {
            if (this.Transaction != null) this.Transaction.Dispose();
            if (this.Connection != null)
            {
                this.Connection.Close();
                this.Connection.Dispose();
            }
            GC.SuppressFinalize(this);
        }
        private void Open()
        {
            if (this.Connection.State == ConnectionState.Broken) this.Connection.Close();
            if (this.Connection.State == ConnectionState.Closed) this.Connection.Open();
        }
        private async Task OpenAsync()
        {
            if (this.Connection.State == ConnectionState.Broken) this.Connection.Close();
            if (Connection.State == ConnectionState.Closed) await this.Connection.OpenAsync().ConfigureAwait(false);
        }
        public void Close() => this.Connection.Close();
    }
}
