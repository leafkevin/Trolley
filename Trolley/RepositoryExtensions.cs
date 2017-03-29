using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Trolley
{
    public static class RepositoryExtensions
    {
        #region IRepository扩展
        public static TEntity QueryFirst<TEntity>(this IRepository repository, Action<SqlBuilder> builder, object objParameter = null)
        {
            SqlBuilder sqlBuilder = new SqlBuilder(repository.Provider);
            builder.Invoke(sqlBuilder);
            return repository.QueryFirst<TEntity>(sqlBuilder.BuildSql(), objParameter);
        }
        public static List<TEntity> Query<TEntity>(this IRepository repository, Action<SqlBuilder> builder, object objParameter = null)
        {
            SqlBuilder sqlBuilder = new SqlBuilder(repository.Provider);
            builder.Invoke(sqlBuilder);
            return repository.Query<TEntity>(sqlBuilder.BuildSql(), objParameter);
        }
        public static List<TEntity> QueryPage<TEntity>(this IRepository repository, Action<SqlBuilder> builder, int pageIndex, int pageSize, string orderBy = null, object objParameter = null)
        {
            SqlBuilder sqlBuilder = new SqlBuilder(repository.Provider);
            builder.Invoke(sqlBuilder);
            return repository.QueryPage<TEntity>(sqlBuilder.BuildSql(), pageIndex, pageSize, orderBy, objParameter, CommandType.Text);
        }
        public static int ExecSql(this IRepository repository, Action<SqlBuilder> builder, object objParameter = null)
        {
            SqlBuilder sqlBuilder = new SqlBuilder(repository.Provider);
            builder.Invoke(sqlBuilder);
            return repository.ExecSql(sqlBuilder.BuildSql(), objParameter);
        }
        public static async Task<TEntity> QueryFirstAsync<TEntity>(this IRepository repository, Action<SqlBuilder> builder, object objParameter = null)
        {
            SqlBuilder sqlBuilder = new SqlBuilder(repository.Provider);
            builder.Invoke(sqlBuilder);
            return await repository.QueryFirstAsync<TEntity>(sqlBuilder.BuildSql(), objParameter);
        }
        public static async Task<List<TEntity>> QueryAsync<TEntity>(this IRepository repository, Action<SqlBuilder> builder, object objParameter = null)
        {
            SqlBuilder sqlBuilder = new SqlBuilder(repository.Provider);
            builder.Invoke(sqlBuilder);
            return await repository.QueryAsync<TEntity>(sqlBuilder.BuildSql(), objParameter);
        }
        public static Task<PagedList<TEntity>> QueryPageAsync<TEntity>(this IRepository repository, Action<SqlBuilder> builder, int pageIndex, int pageSize, string orderBy = null, object objParameter = null)
        {
            SqlBuilder sqlBuilder = new SqlBuilder(repository.Provider);
            builder.Invoke(sqlBuilder);
            return repository.QueryPageAsync<TEntity>(sqlBuilder.BuildSql(), pageIndex, pageSize, orderBy, objParameter);
        }
        public static async Task<int> ExecSqlAsync(this IRepository repository, Action<SqlBuilder> builder, object objParameter = null)
        {
            SqlBuilder sqlBuilder = new SqlBuilder(repository.Provider);
            builder.Invoke(sqlBuilder);
            return await repository.ExecSqlAsync(sqlBuilder.BuildSql(), objParameter);
        }
        #endregion

        #region IRepository<TEntity>扩展
        public static int Update<TEntity>(this IRepository<TEntity> repository, Action<SqlBuilder> builder, TEntity entity = null) where TEntity : class, new()
        {
            SqlBuilder sqlBuilder = new SqlBuilder(repository.Provider);
            builder.Invoke(sqlBuilder);
            return repository.ExecSql(sqlBuilder.BuildSql(), entity);
        }
        public static TEntity QueryFirst<TEntity>(this IRepository<TEntity> repository, Action<SqlBuilder> builder, TEntity objParameter = null) where TEntity : class, new()
        {
            SqlBuilder sqlBuilder = new SqlBuilder(repository.Provider);
            builder.Invoke(sqlBuilder);
            return repository.QueryFirst(sqlBuilder.BuildSql(), objParameter);
        }
        public static TTarget QueryFirst<TEntity, TTarget>(this IRepository<TEntity> repository, Action<SqlBuilder> builder, TEntity objParameter = null) where TEntity : class, new()
        {
            SqlBuilder sqlBuilder = new SqlBuilder(repository.Provider);
            builder.Invoke(sqlBuilder);
            return repository.QueryFirst<TTarget>(sqlBuilder.BuildSql(), objParameter);
        }
        public static List<TEntity> Query<TEntity>(this IRepository<TEntity> repository, Action<SqlBuilder> builder, TEntity objParameter = null) where TEntity : class, new()
        {
            SqlBuilder sqlBuilder = new SqlBuilder(repository.Provider);
            builder.Invoke(sqlBuilder);
            return repository.Query(sqlBuilder.BuildSql(), objParameter);
        }
        public static List<TTarget> Query<TEntity, TTarget>(this IRepository<TEntity> repository, Action<SqlBuilder> builder, TEntity objParameter = null) where TEntity : class, new()
        {
            SqlBuilder sqlBuilder = new SqlBuilder(repository.Provider);
            builder.Invoke(sqlBuilder);
            return repository.Query<TTarget>(sqlBuilder.BuildSql(), objParameter);
        }
        public static PagedList<TEntity> QueryPage<TEntity>(this IRepository<TEntity> repository, Action<SqlBuilder> builder, int pageIndex, int pageSize, string orderBy = null, TEntity objParameter = null) where TEntity : class, new()
        {
            SqlBuilder sqlBuilder = new SqlBuilder(repository.Provider);
            builder.Invoke(sqlBuilder);
            return repository.QueryPage(sqlBuilder.BuildSql(), pageIndex, pageSize, orderBy, objParameter);
        }
        public static PagedList<TTarget> QueryPage<TEntity, TTarget>(this IRepository<TEntity> repository, Action<SqlBuilder> builder, int pageIndex, int pageSize, string orderBy = null, TEntity objParameter = null) where TEntity : class, new()
        {
            SqlBuilder sqlBuilder = new SqlBuilder(repository.Provider);
            builder.Invoke(sqlBuilder);
            return repository.QueryPage<TTarget>(sqlBuilder.BuildSql(), pageIndex, pageSize, orderBy, objParameter);
        }
        public static int ExecSql<TEntity>(this IRepository<TEntity> repository, Action<SqlBuilder> builder, TEntity objParameter = null) where TEntity : class, new()
        {
            SqlBuilder sqlBuilder = new SqlBuilder(repository.Provider);
            builder.Invoke(sqlBuilder);
            return repository.ExecSql(sqlBuilder.BuildSql(), objParameter);
        }
        public static async Task<int> UpdateAsync<TEntity>(this IRepository<TEntity> repository, Action<SqlBuilder> builder, TEntity entity = null) where TEntity : class, new()
        {
            SqlBuilder sqlBuilder = new SqlBuilder(repository.Provider);
            builder.Invoke(sqlBuilder);
            return await repository.ExecSqlAsync(sqlBuilder.BuildSql(), entity);
        }
        public static async Task<TEntity> QueryFirstAsync<TEntity>(this IRepository<TEntity> repository, Action<SqlBuilder> builder, TEntity objParameter = null) where TEntity : class, new()
        {
            SqlBuilder sqlBuilder = new SqlBuilder(repository.Provider);
            builder.Invoke(sqlBuilder);
            return await repository.QueryFirstAsync(sqlBuilder.BuildSql(), objParameter);
        }
        public static async Task<TTarget> QueryFirstAsync<TEntity, TTarget>(this IRepository<TEntity> repository, Action<SqlBuilder> builder, TEntity objParameter = null) where TEntity : class, new()
        {
            SqlBuilder sqlBuilder = new SqlBuilder(repository.Provider);
            builder.Invoke(sqlBuilder);
            return await repository.QueryFirstAsync<TTarget>(sqlBuilder.BuildSql(), objParameter);
        }
        public static async Task<List<TEntity>> QueryAsync<TEntity>(this IRepository<TEntity> repository, Action<SqlBuilder> builder, TEntity objParameter = null) where TEntity : class, new()
        {
            SqlBuilder sqlBuilder = new SqlBuilder(repository.Provider);
            builder.Invoke(sqlBuilder);
            return await repository.QueryAsync(sqlBuilder.BuildSql(), objParameter);
        }
        public static async Task<List<TTarget>> QueryAsync<TEntity, TTarget>(this IRepository<TEntity> repository, Action<SqlBuilder> builder, TEntity objParameter = null) where TEntity : class, new()
        {
            SqlBuilder sqlBuilder = new SqlBuilder(repository.Provider);
            builder.Invoke(sqlBuilder);
            return await repository.QueryAsync<TTarget>(sqlBuilder.BuildSql(), objParameter);
        }
        public static Task<PagedList<TEntity>> QueryPageAsync<TEntity>(this IRepository<TEntity> repository, Action<SqlBuilder> builder, int pageIndex, int pageSize, string orderBy = null, TEntity objParameter = null) where TEntity : class, new()
        {
            SqlBuilder sqlBuilder = new SqlBuilder(repository.Provider);
            builder.Invoke(sqlBuilder);
            return repository.QueryPageAsync(sqlBuilder.BuildSql(), pageIndex, pageSize, orderBy, objParameter);
        }
        public static Task<PagedList<TTarget>> QueryPageAsync<TEntity, TTarget>(this IRepository<TEntity> repository, Action<SqlBuilder> builder, int pageIndex, int pageSize, string orderBy = null, TEntity objParameter = null) where TEntity : class, new()
        {
            SqlBuilder sqlBuilder = new SqlBuilder(repository.Provider);
            builder.Invoke(sqlBuilder);
            return repository.QueryPageAsync<TTarget>(sqlBuilder.BuildSql(), pageIndex, pageSize, orderBy, objParameter);
        }
        public static async Task<int> ExecSqlAsync<TEntity>(this IRepository<TEntity> repository, Action<SqlBuilder> builder, TEntity objParameter = null) where TEntity : class, new()
        {
            SqlBuilder sqlBuilder = new SqlBuilder(repository.Provider);
            builder.Invoke(sqlBuilder);
            return await repository.ExecSqlAsync(sqlBuilder.BuildSql(), objParameter);
        }
        #endregion
    }
}
