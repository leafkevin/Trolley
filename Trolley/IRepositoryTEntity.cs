using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
#if ASYNC
using System.Threading.Tasks;
#endif
namespace Trolley
{
    public interface IRepository<TEntity> : IDisposable where TEntity : class, new()
    {
        IOrmProvider Provider { get; }
        TEntity Get(TEntity key);
        int Create(TEntity entity);
        int Delete(TEntity key);
        int Update(TEntity entity);
        int Update(string sql, TEntity entity = null);
        int Update<TFields>(Expression<Func<TEntity, TFields>> fields, TEntity objParameter = null);
        TEntity QueryFirst(string sql, TEntity objParameter = null, CommandType cmdType = CommandType.Text);
        TTarget QueryFirst<TTarget>(string sql, TEntity objParameter = null, CommandType cmdType = CommandType.Text);
        List<TEntity> Query(string sql, TEntity objParameter = null, CommandType cmdType = CommandType.Text);
        List<TTarget> Query<TTarget>(string sql, TEntity objParameter = null, CommandType cmdType = CommandType.Text);
        PagedList<TEntity> QueryPage(string sql, int pageIndex, int pageSize, string orderBy = null, TEntity objParameter = null, CommandType cmdType = CommandType.Text);
        PagedList<TTarget> QueryPage<TTarget>(string sql, int pageIndex, int pageSize, string orderBy = null, TEntity objParameter = null, CommandType cmdType = CommandType.Text);
        int ExecSql(string sql, TEntity objParameter = null, CommandType cmdType = CommandType.Text);
#if ASYNC
        Task<TEntity> GetAsync(TEntity key);
        Task<int> CreateAsync(TEntity entity);
        Task<int> DeleteAsync(TEntity key);
        Task<int> UpdateAsync(TEntity entity);
        Task<int> UpdateAsync(string sql, TEntity entity = null);
        Task<int> UpdateAsync<TFields>(Expression<Func<TEntity, TFields>> fields, TEntity objParameter = null);
        Task<TEntity> QueryFirstAsync(string sql, TEntity objParameter = null, CommandType cmdType = CommandType.Text);
        Task<TTarget> QueryFirstAsync<TTarget>(string sql, TEntity objParameter = null, CommandType cmdType = CommandType.Text);
        Task<List<TEntity>> QueryAsync(string sql, TEntity objParameter = null, CommandType cmdType = CommandType.Text);
        Task<List<TTarget>> QueryAsync<TTarget>(string sql, TEntity objParameter = null, CommandType cmdType = CommandType.Text);
        Task<PagedList<TEntity>> QueryPageAsync(string sql, int pageIndex, int pageSize, string orderBy = null, TEntity objParameter = null, CommandType cmdType = CommandType.Text);
        Task<PagedList<TTarget>> QueryPageAsync<TTarget>(string sql, int pageIndex, int pageSize, string orderBy = null, TEntity objParameter = null, CommandType cmdType = CommandType.Text);
        Task<int> ExecSqlAsync(string sql, TEntity objParameter = null, CommandType cmdType = CommandType.Text);
#endif
    }
}
