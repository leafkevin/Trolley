using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
#if ASYNC
using System.Threading.Tasks;
#endif

namespace Trolley
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class, new()
    {
        #region 私有变量
        private static ConcurrentDictionary<int, Action<IDbCommand, TEntity>> actionCache = new ConcurrentDictionary<int, Action<IDbCommand, TEntity>>();
        private static ConcurrentDictionary<int, string> sqlCache = new ConcurrentDictionary<int, string>();
        #endregion

        #region 属性
        protected DbConnection Connection { get; private set; }
        protected static EntityMapper Mapper { get; private set; } = new EntityMapper(typeof(TEntity));
        protected IOrmProvider Provider { get; private set; }
        protected IRepositoryContext DbContext { get; private set; }
        protected DbTransaction Transaction { get { return this.DbContext.Transaction; } }
        public string ConnString { get; private set; }
        #endregion

        #region 构造方法
        public Repository()
        {
            this.ConnString = OrmProviderFactory.DefaultConnString;
            this.Provider = OrmProviderFactory.DefaultProvider;
        }
        public Repository(string connString)
        {
            this.ConnString = connString;
            this.Provider = OrmProviderFactory.GetProvider(connString);
        }
        public Repository(string connString, IRepositoryContext dbContext)
        {
            this.ConnString = connString;
            this.DbContext = dbContext;
            this.Connection = dbContext.Connection;
            this.Provider = OrmProviderFactory.GetProvider(connString);
        }
        #endregion

        #region 同步方法
        public TEntity Get(TEntity key)
        {
            int cacheKey = RepositoryHelper.GetHashKey(this.ConnString, "GET");
            var sql = GetSqlCache(cacheKey, this.ConnString, "GET", this.Provider);
            return this.QueryFirstImpl<TEntity>(cacheKey, Mapper.EntityType, sql, CommandType.Text, key, true);
        }
        public int Create(TEntity entity)
        {
            int cacheKey = RepositoryHelper.GetHashKey(this.ConnString, "CREATE");
            var sql = GetSqlCache(cacheKey, this.ConnString, "CREATE", this.Provider);
            return this.ExecSqlImpl(cacheKey, sql, CommandType.Text, entity);
        }
        public int Delete(TEntity key)
        {
            int cacheKey = RepositoryHelper.GetHashKey(this.ConnString, "DELETE");
            var sql = GetSqlCache(cacheKey, this.ConnString, "DELETE", this.Provider);
            return this.ExecSqlImpl(cacheKey, sql, CommandType.Text, key, true);
        }
        public int Update(string sql, TEntity entity = null)
        {
            int cacheKey = RepositoryHelper.GetHashKey(this.ConnString, sql);
            return this.ExecSqlImpl(cacheKey, sql, CommandType.Text, entity);
        }
        public int Update<TFields>(Expression<Func<TEntity, TFields>> fieldsExpression, TEntity objParameter = null)
        {
            var sql = GetUpdateFieldsSql(fieldsExpression, this.Provider);
            int cacheKey = RepositoryHelper.GetHashKey(this.ConnString, sql);
            return this.ExecSqlImpl(cacheKey, sql, CommandType.Text, objParameter);
        }
        public TEntity QueryFirst(string sql, TEntity objParameter = null, CommandType cmdType = CommandType.Text)
        {
            int cacheKey = RepositoryHelper.GetHashKey(this.ConnString, sql);
            return this.QueryFirstImpl<TEntity>(cacheKey, Mapper.EntityType, sql, cmdType, objParameter);
        }
        public TTarget QueryFirst<TTarget>(string sql, TEntity objParameter = null, CommandType cmdType = CommandType.Text)
        {
            int cacheKey = RepositoryHelper.GetHashKey(this.ConnString, sql);
            return this.QueryFirstImpl<TTarget>(cacheKey, typeof(TTarget), sql, cmdType, objParameter);
        }
        public List<TEntity> Query(string sql, TEntity objParameter = null, CommandType cmdType = CommandType.Text)
        {
            int cacheKey = RepositoryHelper.GetHashKey(this.ConnString, sql);
            return this.QueryImpl<TEntity>(cacheKey, Mapper.EntityType, sql, cmdType, objParameter);
        }
        public List<TTarget> Query<TTarget>(string sql, TEntity objParameter = null, CommandType cmdType = CommandType.Text)
        {
            int cacheKey = RepositoryHelper.GetHashKey(this.ConnString, sql);
            return this.QueryImpl<TTarget>(cacheKey, typeof(TTarget), sql, cmdType, objParameter);
        }
        public PagedList<TEntity> QueryPage(string sql, int pageIndex, int pageSize, string orderBy = null, TEntity objParameter = null, CommandType cmdType = CommandType.Text)
        {
            int cacheKey = RepositoryHelper.GetHashKey(this.ConnString, sql + orderBy ?? "");
            sql = RepositoryHelper.GetPagingCache(cacheKey, this.ConnString, sql, pageIndex, pageSize, orderBy, this.Provider);
            return this.QueryPageImpl<TEntity>(cacheKey, Mapper.EntityType, sql, cmdType, objParameter);
        }
        public PagedList<TTarget> QueryPage<TTarget>(string sql, int pageIndex, int pageSize, string orderBy = null, TEntity objParameter = null, CommandType cmdType = CommandType.Text)
        {
            int cacheKey = RepositoryHelper.GetHashKey(this.ConnString, sql);
            sql = RepositoryHelper.GetPagingCache(cacheKey, this.ConnString, sql, pageIndex, pageSize, orderBy, this.Provider);
            return this.QueryPageImpl<TTarget>(cacheKey, typeof(TTarget), sql, cmdType, objParameter);
        }
        public QueryReader QueryMultiple(string sql, TEntity objParameter = null, CommandType cmdType = CommandType.Text)
        {
            int cacheKey = RepositoryHelper.GetHashKey(this.ConnString, sql);
            if (this.Connection == null)
            {
                return this.QueryMultipleImpl(cacheKey, sql, this.Provider.CreateConnection(this.ConnString), null, cmdType, CommandBehavior.SequentialAccess, objParameter, true);
            }
            else return this.QueryMultipleImpl(cacheKey, sql, this.Connection, this.Transaction, cmdType, CommandBehavior.SequentialAccess, objParameter, false);
        }
        public int ExecSql(string sql, TEntity objParameter = null, CommandType cmdType = CommandType.Text)
        {
            int cacheKey = RepositoryHelper.GetHashKey(this.ConnString, sql);
            return this.ExecSqlImpl(cacheKey, sql, cmdType, objParameter);
        }
        #endregion

        #region 异步方法
#if ASYNC
        public async Task<TEntity> GetAsync(TEntity key)
        {
            int cacheKey = RepositoryHelper.GetHashKey(this.ConnString, "GET");
            var sql = GetSqlCache(cacheKey, this.ConnString, "GET", this.Provider);
            return await this.QueryFirstImplAsync<TEntity>(cacheKey, Mapper.EntityType, sql, CommandType.Text, key);
        }
        public async Task<int> CreateAsync(TEntity entity)
        {
            int cacheKey = RepositoryHelper.GetHashKey(this.ConnString, "CREATE");
            var sql = GetSqlCache(cacheKey, this.ConnString, "CREATE", this.Provider);
            return await this.ExecSqlImplAsync(cacheKey, sql, CommandType.Text, entity);
        }
        public async Task<int> DeleteAsync(TEntity key)
        {
            int cacheKey = RepositoryHelper.GetHashKey(this.ConnString, "DELETE");
            var sql = GetSqlCache(cacheKey, this.ConnString, "DELETE", this.Provider);
            return await this.ExecSqlImplAsync(cacheKey, sql, CommandType.Text, key);
        }
        public async Task<int> UpdateAsync(string sql, TEntity entity = null)
        {
            int cacheKey = RepositoryHelper.GetHashKey(this.ConnString, sql);
            return await this.ExecSqlImplAsync(cacheKey, sql, CommandType.Text, entity);
        }
        public async Task<int> UpdateAsync<TFields>(Expression<Func<TEntity, TFields>> fieldsExpression, TEntity objParameter = null)
        {
            var sql = GetUpdateFieldsSql(fieldsExpression, this.Provider);
            int cacheKey = RepositoryHelper.GetHashKey(this.ConnString, sql);
            return await this.ExecSqlImplAsync(cacheKey, sql, CommandType.Text, objParameter);
        }
        public async Task<TEntity> QueryFirstAsync(string sql, TEntity objParameter = null, CommandType cmdType = CommandType.Text)
        {
            int cacheKey = RepositoryHelper.GetHashKey(this.ConnString, sql);
            return await this.QueryFirstImplAsync<TEntity>(cacheKey, Mapper.EntityType, sql, cmdType, objParameter);
        }
        public async Task<TTarget> QueryFirstAsync<TTarget>(string sql, TEntity objParameter = null, CommandType cmdType = CommandType.Text)
        {
            int cacheKey = RepositoryHelper.GetHashKey(this.ConnString, sql);
            return await this.QueryFirstImplAsync<TTarget>(cacheKey, typeof(TTarget), sql, cmdType, objParameter);
        }
        public async Task<List<TEntity>> QueryAsync(string sql, TEntity objParameter = null, CommandType cmdType = CommandType.Text)
        {
            int cacheKey = RepositoryHelper.GetHashKey(this.ConnString, sql);
            return await this.QueryImplAsync<TEntity>(cacheKey, Mapper.EntityType, sql, cmdType, objParameter);
        }
        public async Task<List<TTarget>> QueryAsync<TTarget>(string sql, TEntity objParameter = null, CommandType cmdType = CommandType.Text)
        {
            int cacheKey = RepositoryHelper.GetHashKey(this.ConnString, sql);
            return await this.QueryImplAsync<TTarget>(cacheKey, typeof(TTarget), sql, cmdType, objParameter);
        }
        public async Task<PagedList<TEntity>> QueryPageAsync(string sql, int pageIndex, int pageSize, string orderBy = null, TEntity objParameter = null, CommandType cmdType = CommandType.Text)
        {
            int cacheKey = RepositoryHelper.GetHashKey(this.ConnString, sql + orderBy ?? "");
            sql = RepositoryHelper.GetPagingCache(cacheKey, this.ConnString, sql, pageIndex, pageSize, orderBy, this.Provider);
            return await this.QueryPageImplAsync<TEntity>(cacheKey, Mapper.EntityType, sql, cmdType, objParameter);
        }
        public async Task<PagedList<TTarget>> QueryPageAsync<TTarget>(string sql, int pageIndex, int pageSize, string orderBy = null, TEntity objParameter = null, CommandType cmdType = CommandType.Text)
        {
            int cacheKey = RepositoryHelper.GetHashKey(this.ConnString, sql + orderBy ?? "");
            sql = RepositoryHelper.GetPagingCache(cacheKey, this.ConnString, sql, pageIndex, pageSize, orderBy, this.Provider);
            return await this.QueryPageImplAsync<TTarget>(cacheKey, typeof(TTarget), sql, cmdType, objParameter);
        }
        public async Task<QueryReader> QueryMultipleAsync(string sql, TEntity objParameter = null, CommandType cmdType = CommandType.Text)
        {
            int cacheKey = RepositoryHelper.GetHashKey(this.ConnString, sql);
            if (this.Connection == null)
            {
                return await this.QueryMultipleImplAsync(cacheKey, sql, this.Provider.CreateConnection(this.ConnString), null, cmdType, CommandBehavior.SequentialAccess, objParameter, true);
            }
            else return await this.QueryMultipleImplAsync(cacheKey, sql, this.Connection, this.Transaction, cmdType, CommandBehavior.SequentialAccess, objParameter, false);
        }
        public async Task<int> ExecSqlAsync(string sql, TEntity objParameter = null, CommandType cmdType = CommandType.Text)
        {
            int cacheKey = RepositoryHelper.GetHashKey(this.ConnString, sql);
            return await this.ExecSqlImplAsync(cacheKey, sql, cmdType, objParameter);
        }
#endif
        #endregion

        #region 实现IDisposable
        public void Dispose()
        {
            if (this.Transaction != null) this.Transaction.Dispose();
            if (this.Connection != null) this.Connection.Dispose();
        }
        #endregion

        #region 私有方法
        private void Open(DbConnection conn)
        {
            if (conn.State == ConnectionState.Broken) conn.Close();
            if (conn.State == ConnectionState.Closed) conn.Open();
        }
        private static string GetSqlCache(int hashKey, string connString, string sqlKey, IOrmProvider provider)
        {
            string result = sqlKey;
            switch (sqlKey)
            {
                case "GET":
                    if (!sqlCache.TryGetValue(hashKey, out result))
                    {
                        result = BuildGetSql(Mapper, provider);
                        sqlCache.TryAdd(hashKey, result);
                    }
                    break;
                case "CREATE":
                    if (!sqlCache.TryGetValue(hashKey, out result))
                    {
                        result = BuildCreateSql(Mapper, provider);
                        sqlCache.TryAdd(hashKey, result);
                    }
                    break;
                case "DELETE":
                    if (!sqlCache.TryGetValue(hashKey, out result))
                    {
                        result = BuildDeleteSql(Mapper, provider);
                        sqlCache.TryAdd(hashKey, result);
                    }
                    break;
                case "UPDATE":
                    if (!sqlCache.TryGetValue(hashKey, out result))
                    {
                        var list = Mapper.MemberMappers.Keys.Where(f => (!Mapper.PrimaryKeys.Select(m => m.MemberName).Contains(f))).ToArray();
                        result = BuildUpdateSql(Mapper, provider, list);
                        sqlCache.TryAdd(hashKey, result);
                    }
                    break;
            }
            return result;
        }
        private static Action<IDbCommand, TEntity> GetActionCache(int hashKey, string sql, IOrmProvider provider, bool isPk)
        {
            Action<IDbCommand, TEntity> result;
            if (!actionCache.TryGetValue(hashKey, out result))
            {
                if (isPk) result = RepositoryHelper.CreateParametersHandler<TEntity>(provider.ParamPrefix, typeof(TEntity), Mapper.PrimaryKeys);
                else
                {
                    var colMappers = Mapper.MemberMappers.Values.Where(p => Regex.IsMatch(sql, @"[?@:]" + p.MemberName + "([^a-z0-9_]+|$)", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.CultureInvariant));
                    result = RepositoryHelper.CreateParametersHandler<TEntity>(provider.ParamPrefix, typeof(TEntity), colMappers);
                }
                actionCache.TryAdd(hashKey, result);
            }
            return result;
        }
        private TTarget QueryFirstImpl<TTarget>(int hashKey, Type targetType, string sql, CommandType cmdType, TEntity objParameter = null, bool isPk = false)
        {
            TTarget result = default(TTarget);
            var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
            if (this.Connection == null)
            {
                using (var conn = this.Provider.CreateConnection(this.ConnString))
                {
                    behavior = CommandBehavior.CloseConnection | behavior;
                    this.QueryImplInternal<TTarget>(hashKey, targetType, sql, conn, null, cmdType, behavior, objResult =>
                    {
                        if (objResult == null || objResult is TTarget) result = (TTarget)objResult;
                        else result = (TTarget)Convert.ChangeType(objResult, targetType, CultureInfo.InvariantCulture);
                    }, objParameter, isPk);
                    conn.Close();
                }
            }
            else this.QueryImplInternal<TTarget>(hashKey, targetType, sql, this.Connection, this.Transaction, cmdType, behavior, objResult =>
            {
                if (objResult == null || objResult is TTarget) result = (TTarget)objResult;
                else result = (TTarget)Convert.ChangeType(objResult, targetType, CultureInfo.InvariantCulture);
            }, objParameter, isPk);
            return result;
        }
        private List<TTarget> QueryImpl<TTarget>(int hashKey, Type targetType, string sql, CommandType cmdType, TEntity objParameter = null, bool isPk = false)
        {
            List<TTarget> result = new List<TTarget>();
            var behavior = CommandBehavior.SequentialAccess;
            if (this.Connection == null)
            {
                using (var conn = this.Provider.CreateConnection(this.ConnString))
                {
                    behavior = CommandBehavior.CloseConnection | behavior;
                    this.QueryImplInternal<TTarget>(hashKey, targetType, sql, conn, null, cmdType, behavior, objResult =>
                    {
                        if (objResult == null) return;
                        if (objResult is TTarget) result.Add((TTarget)objResult);
                        else result.Add((TTarget)Convert.ChangeType(objResult, targetType, CultureInfo.InvariantCulture));
                    }, objParameter, isPk);
                    conn.Close();
                }
            }
            else this.QueryImplInternal<TTarget>(hashKey, targetType, sql, this.Connection, this.Transaction, cmdType, behavior, objResult =>
            {
                if (objResult == null) return;
                if (objResult is TTarget) result.Add((TTarget)objResult);
                else result.Add((TTarget)Convert.ChangeType(objResult, targetType, CultureInfo.InvariantCulture));
            }, objParameter, isPk);
            return result;
        }
        private PagedList<TTarget> QueryPageImpl<TTarget>(int hashKey, Type targetType, string sql, CommandType cmdType, TEntity objParameter = null, bool isPk = false)
        {
            PagedList<TTarget> result = new PagedList<TTarget>();
            var behavior = CommandBehavior.SequentialAccess;
            if (this.Connection == null)
            {
                using (var conn = this.Provider.CreateConnection(this.ConnString))
                {
                    behavior = CommandBehavior.CloseConnection | behavior;
                    this.QueryImplInternal<TTarget>(hashKey, targetType, sql, conn, null, cmdType, behavior, objResult =>
                    {
                        if (objResult == null) return;
                        if (objResult is TTarget) result.Add((TTarget)objResult);
                        else result.Add((TTarget)Convert.ChangeType(objResult, targetType, CultureInfo.InvariantCulture));
                    }, objParameter, isPk);
                    conn.Close();
                }
            }
            else this.QueryImplInternal<TTarget>(hashKey, targetType, sql, this.Connection, this.Transaction, cmdType, behavior, objResult =>
            {
                if (objResult == null) return;
                if (objResult is TTarget) result.Add((TTarget)objResult);
                else result.Add((TTarget)Convert.ChangeType(objResult, targetType, CultureInfo.InvariantCulture));
            }, objParameter, isPk);
            return result;
        }
        private QueryReader QueryMultipleImpl(int hashKey, string sql, DbConnection conn, DbTransaction trans, CommandType cmdType, CommandBehavior behavior, TEntity objParameter, bool isCloseConnection)
        {
            DbCommand command = conn.CreateCommand();
            command.CommandText = sql;
            command.CommandType = cmdType;
            command.Transaction = trans;
            if (objParameter != null)
            {
                var paramAction = GetActionCache(hashKey, sql, this.Provider, false);
                paramAction(command, objParameter);
            }
            this.Open(conn);
            DbDataReader reader = command.ExecuteReader(behavior);
            return new QueryReader(hashKey, command, reader, this.Provider.IsMappingIgnoreCase, isCloseConnection);
        }
        private int ExecSqlImpl(int hashKey, string sql, CommandType cmdType = CommandType.Text, TEntity objParameter = null, bool isPk = false)
        {
            int result = 0;
            if (this.Connection == null)
            {
                using (var conn = this.Provider.CreateConnection(this.ConnString))
                {
                    result = this.ExecSqlImplInternal(hashKey, sql, conn, null, cmdType, objParameter, isPk);
                    conn.Close();
                }
            }
            else result = this.ExecSqlImplInternal(hashKey, sql, this.Connection, this.Transaction, cmdType, objParameter, isPk);
            return result;
        }
        private void QueryImplInternal<TTarget>(int hashKey, Type targetType, string sql, DbConnection conn, DbTransaction trans, CommandType cmdType, CommandBehavior behavior, Action<object> resultHandler, TEntity objParameter, bool isPk)
        {
            DbCommand command = conn.CreateCommand();
            command.CommandText = sql;
            command.CommandType = cmdType;
            command.Transaction = trans;
            if (objParameter != null)
            {
                var paramAction = GetActionCache(hashKey, sql, this.Provider, isPk);
                paramAction(command, objParameter);
            }
            this.Open(conn);
            DbDataReader reader = command.ExecuteReader(behavior);
            var func = RepositoryHelper.GetReader(hashKey, targetType, reader, this.Provider.IsMappingIgnoreCase);
            while (reader.Read())
            {
                resultHandler(func?.Invoke(reader));
            }
            while (reader.NextResult()) { }
#if COREFX
            try { command.Cancel(); } catch { }
#else
            reader.Close();
#endif
            reader.Dispose();
            reader = null;
        }
        private int ExecSqlImplInternal(int hashKey, string sql, DbConnection conn, DbTransaction trans, CommandType cmdType, TEntity objParameter, bool isPk)
        {
            DbCommand command = conn.CreateCommand();
            command.CommandText = sql;
            command.CommandType = cmdType;
            command.Transaction = trans;
            if (objParameter != null)
            {
                var paramAction = GetActionCache(hashKey, sql, this.Provider, isPk);
                paramAction(command, objParameter);
            }
            this.Open(conn);
            return command.ExecuteNonQuery();
        }

#if ASYNC
        private async Task<TTarget> QueryFirstImplAsync<TTarget>(int hashKey, Type targetType, string sql, CommandType cmdType, TEntity objParameter = null, bool isPk = false)
        {
            TTarget result = default(TTarget);
            var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
            if (this.Connection == null)
            {
                using (var conn = this.Provider.CreateConnection(this.ConnString))
                {
                    behavior = CommandBehavior.CloseConnection | behavior;
                    await this.QueryImplInternalAsync<TTarget>(hashKey, targetType, sql, conn, null, cmdType, behavior, objResult =>
                    {
                        if (objResult == null || objResult is TTarget) result = (TTarget)objResult;
                        else result = (TTarget)Convert.ChangeType(objResult, targetType, CultureInfo.InvariantCulture);
                    }, objParameter, isPk);
                }
            }
            else await this.QueryImplInternalAsync<TTarget>(hashKey, targetType, sql, this.Connection, this.Transaction, cmdType, behavior, objResult =>
            {
                if (objResult == null || objResult is TTarget) result = (TTarget)objResult;
                else result = (TTarget)Convert.ChangeType(objResult, targetType, CultureInfo.InvariantCulture);
            }, objParameter, isPk);
            return result;
        }
        private async Task<List<TTarget>> QueryImplAsync<TTarget>(int hashKey, Type targetType, string sql, CommandType cmdType, TEntity objParameter = null, bool isPk = false)
        {
            List<TTarget> result = new List<TTarget>();
            var behavior = CommandBehavior.SequentialAccess;
            if (this.Connection == null)
            {
                using (var conn = this.Provider.CreateConnection(this.ConnString))
                {
                    behavior = CommandBehavior.CloseConnection | behavior;
                    await this.QueryImplInternalAsync<TTarget>(hashKey, targetType, sql, conn, null, cmdType, behavior, objResult =>
                    {
                        if (objResult == null) return;
                        if (objResult is TTarget) result.Add((TTarget)objResult);
                        else result.Add((TTarget)Convert.ChangeType(objResult, targetType, CultureInfo.InvariantCulture));
                    }, objParameter, isPk);
                }
            }
            else await this.QueryImplInternalAsync<TTarget>(hashKey, targetType, sql, this.Connection, this.Transaction, cmdType, behavior, objResult =>
            {
                if (objResult == null) return;
                if (objResult is TTarget) result.Add((TTarget)objResult);
                else result.Add((TTarget)Convert.ChangeType(objResult, targetType, CultureInfo.InvariantCulture));
            }, objParameter, isPk);
            return result;
        }
        private async Task<PagedList<TTarget>> QueryPageImplAsync<TTarget>(int hashKey, Type targetType, string sql, CommandType cmdType, TEntity objParameter = null, bool isPk = false)
        {
            PagedList<TTarget> result = new PagedList<TTarget>();
            var behavior = CommandBehavior.SequentialAccess;
            if (this.Connection == null)
            {
                using (var conn = this.Provider.CreateConnection(this.ConnString))
                {
                    behavior = CommandBehavior.CloseConnection | behavior;
                    await this.QueryImplInternalAsync<TTarget>(hashKey, targetType, sql, conn, null, cmdType, behavior, objResult =>
                    {
                        if (objResult == null) return;
                        if (objResult is TTarget) result.Add((TTarget)objResult);
                        else result.Add((TTarget)Convert.ChangeType(objResult, targetType, CultureInfo.InvariantCulture));
                    }, objParameter, isPk);
                }
            }
            else await this.QueryImplInternalAsync<TTarget>(hashKey, targetType, sql, this.Connection, this.Transaction, cmdType, behavior, objResult =>
            {
                if (objResult == null) return;
                if (objResult is TTarget) result.Add((TTarget)objResult);
                else result.Add((TTarget)Convert.ChangeType(objResult, targetType, CultureInfo.InvariantCulture));
            }, objParameter, isPk);
            return result;
        }
        private async Task<QueryReader> QueryMultipleImplAsync(int hashKey, string sql, DbConnection conn, DbTransaction trans, CommandType cmdType, CommandBehavior behavior, TEntity objParameter, bool isCloseConnection)
        {
            DbCommand command = conn.CreateCommand();
            command.CommandText = sql;
            command.CommandType = cmdType;
            command.Transaction = trans;
            if (objParameter != null)
            {
                var paramAction = GetActionCache(hashKey, sql, this.Provider, false);
                paramAction(command, objParameter);
            }
            this.Open(conn);
            DbDataReader reader = await command.ExecuteReaderAsync(behavior);
            return new QueryReader(hashKey, command, reader, this.Provider.IsMappingIgnoreCase, isCloseConnection);
        }
        private async Task<int> ExecSqlImplAsync(int hashKey, string sql, CommandType cmdType = CommandType.Text, TEntity objParameter = null, bool isPk = false)
        {
            int result = 0;
            if (this.Connection == null)
            {
                using (var conn = this.Provider.CreateConnection(this.ConnString))
                {
                    result = await this.ExecSqlImplInternalAsync(hashKey, sql, conn, null, cmdType, objParameter, isPk);
                    conn.Close();
                }
            }
            else result = await this.ExecSqlImplInternalAsync(hashKey, sql, this.Connection, this.Transaction, cmdType, objParameter, isPk);
            return result;
        }
        private async Task QueryImplInternalAsync<TTarget>(int hashKey, Type targetType, string sql, DbConnection conn, DbTransaction trans, CommandType cmdType, CommandBehavior behavior, Action<object> resultHandler, TEntity objParameter, bool isPk)
        {
            DbCommand command = conn.CreateCommand();
            command.CommandText = sql;
            command.CommandType = cmdType;
            command.Transaction = trans;
            if (objParameter != null)
            {
                var paramAction = GetActionCache(hashKey, sql, this.Provider, isPk);
                paramAction(command, objParameter);
            }
            this.Open(conn);
            DbDataReader reader = await command.ExecuteReaderAsync(behavior);
            var func = RepositoryHelper.GetReader(hashKey, targetType, reader, this.Provider.IsMappingIgnoreCase);
            while (reader.Read())
            {
                resultHandler(func?.Invoke(reader));
            }
            while (reader.NextResult()) { }
#if COREFX
            try { command.Cancel(); } catch { }
#else
            reader.Close();
#endif
            reader.Dispose();
            reader = null;
        }
        private async Task<int> ExecSqlImplInternalAsync(int hashKey, string sql, DbConnection conn, DbTransaction trans, CommandType cmdType, TEntity objParameter, bool isPk)
        {
            DbCommand command = conn.CreateCommand();
            command.CommandText = sql;
            command.CommandType = cmdType;
            command.Transaction = trans;
            if (objParameter != null)
            {
                var paramAction = GetActionCache(hashKey, sql, this.Provider, isPk);
                paramAction(command, objParameter);
            }
            this.Open(conn);
            return await command.ExecuteNonQueryAsync();
        }
#endif
        private static string BuildCreateSql(EntityMapper mapper, IOrmProvider provider)
        {
            StringBuilder insertBuilder = new StringBuilder();
            insertBuilder.Append("INSERT INTO " + provider.GetTableName(mapper.TableName) + " (");
            StringBuilder valueBuilder = new StringBuilder();
            valueBuilder.Append(") VALUES(");
            int i = 0;
            foreach (var colMapper in mapper.MemberMappers.Values)
            {
                if (colMapper.IsAutoIncrement) continue;
                if (i > 0) insertBuilder.Append(",");
                if (i > 0) valueBuilder.Append(",");
                insertBuilder.Append(provider.GetColumnName(colMapper.FieldName));
                valueBuilder.Append(provider.ParamPrefix + colMapper.MemberName);
                i++;
            }
            valueBuilder.Append(")");
            return insertBuilder.ToString() + valueBuilder.ToString();
        }
        private static string BuildGetSql(EntityMapper mapper, IOrmProvider provider)
        {
            StringBuilder sqlBuilder = new StringBuilder();
            sqlBuilder.Append("SELECT ");
            StringBuilder whereBuilder = new StringBuilder();
            whereBuilder.Append(" WHERE ");
            int i = 0;
            foreach (var colMapper in mapper.MemberMappers.Values)
            {
                if (i > 0) sqlBuilder.Append(",");
                if (colMapper.IsPrimaryKey)
                {
                    if (i > 0) whereBuilder.Append(" AND ");
                    whereBuilder.Append(GetSetParameterSql(provider, colMapper));
                }
                sqlBuilder.Append(GetAliasParameterSql(provider, colMapper));
                i++;
            }
            sqlBuilder.Append(" FROM " + provider.GetTableName(mapper.TableName));
            return sqlBuilder.ToString() + whereBuilder.ToString();
        }
        private static string BuildDeleteSql(EntityMapper mapper, IOrmProvider provider)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("DELETE FROM " + provider.GetTableName(mapper.TableName) + " WHERE ");
            int i = 0;
            foreach (var colMapper in mapper.PrimaryKeys)
            {
                if (i > 0) builder.Append(" AND ");
                builder.Append(GetSetParameterSql(provider, colMapper));
                i++;
            }
            return builder.ToString();
        }
        private static string BuildUpdateSql(EntityMapper mapper, IOrmProvider provider, string[] updateParameters)
        {
            var builder = new StringBuilder("UPDATE " + provider.GetTableName(mapper.TableName) + " SET ");
            int index = 0;
            foreach (var propName in updateParameters)
            {
                if (index > 0) builder.Append(",");
                builder.Append(GetSetParameterSql(provider, mapper.MemberMappers[propName]));
                index++;
            }
            builder.Append(" WHERE ");
            index = 0;
            foreach (var colMapper in mapper.PrimaryKeys)
            {
                if (index > 0) builder.Append(" AND ");
                builder.Append(GetSetParameterSql(provider, colMapper));
                index++;
            }
            return builder.ToString();
        }
        private static string GetUpdateFieldsSql<TFields>(Expression<Func<TEntity, TFields>> fieldsExpression, IOrmProvider provider)
        {
            var builder = new StringBuilder("UPDATE " + provider.GetTableName(Mapper.TableName) + " SET ");
            var expression = ((LambdaExpression)fieldsExpression).Body;
            if (expression.NodeType == ExpressionType.New)
            {
                var newExpression = expression as NewExpression;
                int index = 0;
                foreach (var item in newExpression.Arguments)
                {
                    if (item.NodeType == ExpressionType.MemberAccess)
                    {
                        var exp = item as MemberExpression;
                        if (index > 0) builder.Append(",");
                        builder.Append(GetSetParameterSql(provider, Mapper.MemberMappers[exp.Member.Name]));
                        index++;
                    }
                    else throw new Exception("不支持的Linq表达式");
                }
            }
            else if (expression.NodeType == ExpressionType.MemberAccess)
            {
                var exp = expression as MemberExpression;
                builder.Append(GetSetParameterSql(provider, Mapper.MemberMappers[exp.Member.Name]));
            }
            else throw new Exception("不支持的Linq表达式");
            builder.Append(" WHERE ");
            foreach (var colMapper in Mapper.PrimaryKeys)
            {
                builder.Append(GetSetParameterSql(provider, colMapper));
            }
            return builder.ToString();
        }
        private static string GetSetParameterSql(IOrmProvider provider, MemberMapper colMapper)
        {
            return provider.GetColumnName(colMapper.FieldName) + "=" + provider.ParamPrefix + colMapper.MemberName;
        }
        private static string GetAliasParameterSql(IOrmProvider provider, MemberMapper colMapper)
        {
            return provider.GetColumnName(colMapper.FieldName) + " AS " + provider.GetPropertyName(colMapper.MemberName);
        }
        #endregion
    }
}
