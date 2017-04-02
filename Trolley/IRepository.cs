using System;
using System.Collections.Generic;
using System.Data;
#if ASYNC
using System.Threading.Tasks;
#endif

namespace Trolley
{
    public interface IRepository : IDisposable
    {
        string ConnString { get; }
        IOrmProvider Provider { get; }
        TEntity QueryFirst<TEntity>(string sql, object objParameter = null, CommandType cmdType = CommandType.Text);
        List<TEntity> Query<TEntity>(string sql, object objParameter = null, CommandType cmdType = CommandType.Text);
        PagedList<TEntity> QueryPage<TEntity>(string sql, int pageIndex, int pageSize, string orderBy = null, object objParameter = null, CommandType cmdType = CommandType.Text);
        int ExecSql(string sql, object objParameter = null, CommandType cmdType = CommandType.Text);
#if ASYNC
        Task<TEntity> QueryFirstAsync<TEntity>(string sql, object objParameter = null, CommandType cmdType = CommandType.Text);
        Task<List<TEntity>> QueryAsync<TEntity>(string sql, object objParameter = null, CommandType cmdType = CommandType.Text);
        Task<PagedList<TEntity>> QueryPageAsync<TEntity>(string sql, int pageIndex, int pageSize, string orderBy = null, object objParameter = null, CommandType cmdType = CommandType.Text);
        Task<int> ExecSqlAsync(string sql, object objParameter = null, CommandType cmdType = CommandType.Text);
#endif
    }
}
