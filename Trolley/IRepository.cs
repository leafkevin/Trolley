using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Trolley
{
    public interface IRepository : IDisposable
    {
        #region 属性
        IOrmProvider Provider { get; }
        #endregion

        #region 同步方法
        TEntity QueryFirst<TEntity>(string sql, object objParameter = null, CommandType cmdType = CommandType.Text);
        TEntity QueryFirst<TEntity>(Action<SqlBuilder> builder);
        List<TEntity> Query<TEntity>(string sql, object objParameter = null, CommandType cmdType = CommandType.Text);
        List<TEntity> Query<TEntity>(Action<SqlBuilder> builder);
        PagedList<TEntity> QueryPage<TEntity>(string sql, int pageIndex, int pageSize, string orderBy = null, object objParameter = null, CommandType cmdType = CommandType.Text);
        PagedList<TEntity> QueryPage<TEntity>(Action<SqlBuilder> builder, int pageIndex, int pageSize, string orderBy = null);
        QueryReader QueryMultiple(string sql, object objParameter = null, CommandType cmdType = CommandType.Text);
        QueryReader QueryMultiple(Action<SqlBuilder> builder);
        TResult QueryMap<TResult>(string sql, Func<QueryReader, TResult> mapping, object objParameter = null, CommandType cmdType = CommandType.Text);
        TResult QueryMap<TResult>(Action<SqlBuilder> builder, Func<QueryReader, TResult> mapping);
        Dictionary<TKey, TValue> QueryDictionary<TKey, TValue>(string sql, object objParameter = null, CommandType cmdType = CommandType.Text);
        Dictionary<TKey, TValue> QueryDictionary<TKey, TValue>(Action<SqlBuilder> builder);
        int ExecSql(string sql, object objParameter = null, CommandType cmdType = CommandType.Text);
        int ExecSql(Action<SqlBuilder> builder);
        #endregion

        #region 异步方法
        Task<TEntity> QueryFirstAsync<TEntity>(string sql, object objParameter = null, CommandType cmdType = CommandType.Text);
        Task<TEntity> QueryFirstAsync<TEntity>(Action<SqlBuilder> builder);
        Task<List<TEntity>> QueryAsync<TEntity>(string sql, object objParameter = null, CommandType cmdType = CommandType.Text);
        Task<List<TEntity>> QueryAsync<TEntity>(Action<SqlBuilder> builder);
        Task<PagedList<TEntity>> QueryPageAsync<TEntity>(string sql, int pageIndex, int pageSize, string orderBy = null, object objParameter = null, CommandType cmdType = CommandType.Text);
        Task<PagedList<TEntity>> QueryPageAsync<TEntity>(Action<SqlBuilder> builder, int pageIndex, int pageSize, string orderBy = null);
        Task<QueryReader> QueryMultipleAsync(string sql, object objParameter = null, CommandType cmdType = CommandType.Text);
        Task<QueryReader> QueryMultipleAsync(Action<SqlBuilder> builder);
        Task<TResult> QueryMapAsync<TResult>(Func<QueryReader, TResult> mapping, string sql, object objParameter = null, CommandType cmdType = CommandType.Text);
        Task<TResult> QueryMapAsync<TResult>(Action<SqlBuilder> builder, Func<QueryReader, TResult> mapping);
        Task<Dictionary<TKey, TValue>> QueryDictionaryAsync<TKey, TValue>(string sql, object objParameter = null, CommandType cmdType = CommandType.Text);
        Task<Dictionary<TKey, TValue>> QueryDictionaryAsync<TKey, TValue>(Action<SqlBuilder> builder);
        Task<int> ExecSqlAsync(string sql, object objParameter = null, CommandType cmdType = CommandType.Text);
        Task<int> ExecSqlAsync(Action<SqlBuilder> builder);
        #endregion
    }
}
