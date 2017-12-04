using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Trolley
{
    public interface IRepository<TEntity> : IDisposable where TEntity : class, new()
    {
        #region 属性
        IOrmProvider Provider { get; }
        #endregion

        #region 同步方法
        TEntity Get(TEntity key);
        int Create(TEntity entity);
        int Delete(TEntity key);
        int Update(TEntity entity);
        int Update(string sql, TEntity entity = null);
        int Update<TFields>(Expression<Func<TEntity, TFields>> fieldsExpression, TEntity objParameter = null);
        int Update(Action<SqlBuilder> builder);
        TEntity QueryFirst(string sql, TEntity objParameter = null, CommandType cmdType = CommandType.Text);
        TEntity QueryFirst(Action<SqlBuilder> builder);
        TTarget QueryFirst<TTarget>(string sql, TEntity objParameter = null, CommandType cmdType = CommandType.Text);
        TTarget QueryFirst<TTarget>(Action<SqlBuilder> builder);
        List<TEntity> Query(string sql, TEntity objParameter = null, CommandType cmdType = CommandType.Text);
        List<TEntity> Query(Action<SqlBuilder> builder);
        List<TTarget> Query<TTarget>(string sql, TEntity objParameter = null, CommandType cmdType = CommandType.Text);
        List<TTarget> Query<TTarget>(Action<SqlBuilder> builder);
        PagedList<TEntity> QueryPage(string sql, int pageIndex, int pageSize, string orderBy = null, TEntity objParameter = null, CommandType cmdType = CommandType.Text);
        PagedList<TEntity> QueryPage(Action<SqlBuilder> builder, int pageIndex, int pageSize, string orderBy = null);
        PagedList<TTarget> QueryPage<TTarget>(string sql, int pageIndex, int pageSize, string orderBy = null, TEntity objParameter = null, CommandType cmdType = CommandType.Text);
        PagedList<TTarget> QueryPage<TTarget>(Action<SqlBuilder> builder, int pageIndex, int pageSize, string orderBy = null);
        TEntity QueryMap(Func<QueryReader, TEntity> mapping, string sql, TEntity objParameter = null, CommandType cmdType = CommandType.Text);
        TEntity QueryMap(Action<SqlBuilder> builder, Func<QueryReader, TEntity> mapping);
        TTarget QueryMap<TTarget>(Func<QueryReader, TTarget> mapping, string sql, TEntity objParameter = null, CommandType cmdType = CommandType.Text);
        TTarget QueryMap<TTarget>(Action<SqlBuilder> builder, Func<QueryReader, TTarget> mapping);
        QueryReader QueryMultiple(string sql, TEntity objParameter = null, CommandType cmdType = CommandType.Text);
        Dictionary<TKey, TValue> QueryDictionary<TKey, TValue>(string sql, object objParameter = null, CommandType cmdType = CommandType.Text);
        Dictionary<TKey, TValue> QueryDictionary<TKey, TValue>(Action<SqlBuilder> builder);
        int ExecSql(string sql, TEntity objParameter = null, CommandType cmdType = CommandType.Text);
        int ExecSql(Action<SqlBuilder> builder);
        #endregion

        #region 异步方法
        Task<TEntity> GetAsync(TEntity key);
        Task<int> CreateAsync(TEntity entity);
        Task<int> DeleteAsync(TEntity key);
        Task<int> UpdateAsync(TEntity entity);
        Task<int> UpdateAsync(string sql, TEntity entity = null);
        Task<int> UpdateAsync<TFields>(Expression<Func<TEntity, TFields>> fieldsExpression, TEntity objParameter = null);
        Task<int> UpdateAsync(Action<SqlBuilder> builder);
        Task<TEntity> QueryFirstAsync(string sql, TEntity objParameter = null, CommandType cmdType = CommandType.Text);
        Task<TEntity> QueryFirstAsync(Action<SqlBuilder> builder);
        Task<TTarget> QueryFirstAsync<TTarget>(string sql, TEntity objParameter = null, CommandType cmdType = CommandType.Text);
        Task<TTarget> QueryFirstAsync<TTarget>(Action<SqlBuilder> builder);
        Task<List<TEntity>> QueryAsync(string sql, TEntity objParameter = null, CommandType cmdType = CommandType.Text);
        Task<List<TEntity>> QueryAsync(Action<SqlBuilder> builder);
        Task<List<TTarget>> QueryAsync<TTarget>(string sql, TEntity objParameter = null, CommandType cmdType = CommandType.Text);
        Task<List<TTarget>> QueryAsync<TTarget>(Action<SqlBuilder> builder);
        Task<PagedList<TEntity>> QueryPageAsync(string sql, int pageIndex, int pageSize, string orderBy = null, TEntity objParameter = null, CommandType cmdType = CommandType.Text);
        Task<PagedList<TEntity>> QueryPageAsync(Action<SqlBuilder> builder, int pageIndex, int pageSize, string orderBy = null);
        Task<PagedList<TTarget>> QueryPageAsync<TTarget>(string sql, int pageIndex, int pageSize, string orderBy = null, TEntity objParameter = null, CommandType cmdType = CommandType.Text);
        Task<PagedList<TTarget>> QueryPageAsync<TTarget>(Action<SqlBuilder> builder, int pageIndex, int pageSize, string orderBy = null);
        Task<TEntity> QueryMapAsync(Func<QueryReader, TEntity> mapping, string sql, TEntity objParameter = null, CommandType cmdType = CommandType.Text);
        Task<TEntity> QueryMapAsync(Action<SqlBuilder> builder, Func<QueryReader, TEntity> mapping);
        Task<TTarget> QueryMapAsync<TTarget>(Func<QueryReader, TTarget> mapping, string sql, TEntity objParameter = null, CommandType cmdType = CommandType.Text);
        Task<TTarget> QueryMapAsync<TTarget>(Action<SqlBuilder> builder, Func<QueryReader, TTarget> mapping);
        Task<QueryReader> QueryMultipleAsync(string sql, TEntity objParameter = null, CommandType cmdType = CommandType.Text);
        Task<Dictionary<TKey, TValue>> QueryDictionaryAsync<TKey, TValue>(string sql, object objParameter = null, CommandType cmdType = CommandType.Text);
        Task<Dictionary<TKey, TValue>> QueryDictionaryAsync<TKey, TValue>(Action<SqlBuilder> builder);
        Task<int> ExecSqlAsync(string sql, TEntity objParameter = null, CommandType cmdType = CommandType.Text);
        Task<int> ExecSqlAsync(Action<SqlBuilder> builder);
        #endregion
    }
}
