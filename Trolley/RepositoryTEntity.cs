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
using System.Threading.Tasks;

namespace Trolley
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class, new()
    {
        #region 私有变量
        private static ConcurrentDictionary<int, Action<IDbCommand, TEntity>> ActionCache = new ConcurrentDictionary<int, Action<IDbCommand, TEntity>>();
        private static ConcurrentDictionary<int, string> SqlCache = new ConcurrentDictionary<int, string>();
        #endregion

        #region 属性
        protected DbConnection Connection { get; private set; }
        protected DbTransaction Transaction { get; private set; }
        protected static EntityMapper Mapper { get; private set; } = new EntityMapper(typeof(TEntity));
        public string ConnString { get; private set; }
        public IOrmProvider Provider { get; private set; }
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
        public Repository(string connString, DbTransaction transaction)
        {
            this.ConnString = connString;
            this.Provider = OrmProviderFactory.GetProvider(connString);
            if (transaction != null)
            {
                this.Transaction = transaction;
                this.Connection = transaction.Connection;
            }
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
            return await this.QueryPageImplAsync<TTarget>(cacheKey, Mapper.EntityType, sql, cmdType, objParameter);
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
        private void Open()
        {
            if (this.Connection.State == ConnectionState.Broken) this.Connection.Close();
            if (this.Connection.State == ConnectionState.Closed) this.Connection.Open();
        }
        private static string GetSqlCache(int hashKey, string connString, string sqlKey, IOrmProvider provider)
        {
            string result = sqlKey;
            switch (sqlKey)
            {
                case "GET":
                    if (!SqlCache.TryGetValue(hashKey, out result))
                    {
                        result = BuildGetSql(Mapper, provider);
                        SqlCache.TryAdd(hashKey, result);
                    }
                    break;
                case "CREATE":
                    if (!SqlCache.TryGetValue(hashKey, out result))
                    {
                        result = BuildCreateSql(Mapper, provider);
                        SqlCache.TryAdd(hashKey, result);
                    }
                    break;
                case "DELETE":
                    if (!SqlCache.TryGetValue(hashKey, out result))
                    {
                        result = BuildDeleteSql(Mapper, provider);
                        SqlCache.TryAdd(hashKey, result);
                    }
                    break;
                case "UPDATE":
                    if (!SqlCache.TryGetValue(hashKey, out result))
                    {
                        var list = Mapper.MemberMappers.Keys.Where(f => (!Mapper.PrimaryKeys.Select(m => m.MemberName).Contains(f))).ToArray();
                        result = BuildUpdateSql(Mapper, provider, list);
                        SqlCache.TryAdd(hashKey, result);
                    }
                    break;
            }
            return result;
        }
        private static Action<IDbCommand, TEntity> GetActionCache(int hashKey, string sql, IOrmProvider provider, bool isPk)
        {
            Action<IDbCommand, TEntity> result;
            if (!ActionCache.TryGetValue(hashKey, out result))
            {
                if (isPk) result = RepositoryHelper.CreateParametersHandler<TEntity>(provider.ParamPrefix, typeof(TEntity), Mapper.PrimaryKeys);
                else
                {
                    var colMappers = Mapper.MemberMappers.Values.Where(p => Regex.IsMatch(sql, @"[?@:]" + p.MemberName + "([^a-z0-9_]+|$)", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.CultureInvariant));
                    result = RepositoryHelper.CreateParametersHandler<TEntity>(provider.ParamPrefix, typeof(TEntity), colMappers);
                }
                ActionCache.TryAdd(hashKey, result);
            }
            return result;
        }
        private TTarget QueryFirstImpl<TTarget>(int hashKey, Type targetType, string sql, CommandType cmdType, TEntity objParameter = null, bool isPk = false)
        {
            DbCommand command = null;
            string propName = String.Empty;
            DbDataReader reader = null;
            TTarget result = default(TTarget);
            if (this.Connection == null)
            {
                using (var conn = this.Provider.CreateConnection(this.ConnString))
                {
                    command = conn.CreateCommand();
                    command.CommandText = sql;
                    command.CommandType = cmdType;
                    if (objParameter != null)
                    {
                        var paramAction = GetActionCache(hashKey, sql, this.Provider, isPk);
                        paramAction(command, objParameter);
                    }
                    conn.Open();
                    reader = command.ExecuteReader(CommandBehavior.CloseConnection | CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow);
                    var func = RepositoryHelper.GetReader(hashKey, targetType, reader);
                    while (reader.Read())
                    {
                        object funcResult = func?.Invoke(reader);
                        if (funcResult == null || funcResult is TTarget) result = (TTarget)funcResult;
                        else result = (TTarget)Convert.ChangeType(funcResult, targetType, CultureInfo.InvariantCulture);
                    }
                    while (reader.NextResult()) { }
                    conn.Close();
                    reader.Dispose();
                    reader = null;
                }
            }
            else
            {
                command = this.Connection.CreateCommand();
                command.CommandText = sql;
                command.CommandType = CommandType.Text;
                if (this.Transaction != null) command.Transaction = this.Transaction;
                if (objParameter != null)
                {
                    var paramAction = GetActionCache(hashKey, sql, this.Provider, isPk);
                    paramAction(command, objParameter);
                }
                this.Open();
                reader = command.ExecuteReader(CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow);
                var func = RepositoryHelper.GetReader(hashKey, targetType, reader);
                while (reader.Read())
                {
                    object funcResult = func?.Invoke(reader);
                    if (funcResult == null || funcResult is TTarget) result = (TTarget)funcResult;
                    else result = (TTarget)Convert.ChangeType(funcResult, targetType, CultureInfo.InvariantCulture);
                }
                while (reader.NextResult()) { }
                reader.Dispose();
                reader = null;
            }
            return result;
        }
        private List<TTarget> QueryImpl<TTarget>(int hashKey, Type targetType, string sql, CommandType cmdType, TEntity objParameter = null, bool isPk = false)
        {
            DbCommand command = null;
            string propName = String.Empty;
            DbDataReader reader = null;
            List<TTarget> result = new List<TTarget>();
            if (this.Connection == null)
            {
                using (var conn = this.Provider.CreateConnection(this.ConnString))
                {
                    command = conn.CreateCommand();
                    command.CommandText = sql;
                    command.CommandType = cmdType;
                    if (objParameter != null)
                    {
                        var paramAction = GetActionCache(hashKey, sql, this.Provider, isPk);
                        paramAction(command, objParameter);
                    }
                    conn.Open();
                    reader = command.ExecuteReader(CommandBehavior.CloseConnection | CommandBehavior.SequentialAccess);
                    var func = RepositoryHelper.GetReader(hashKey, targetType, reader);
                    while (reader.Read())
                    {
                        object funcResult = func?.Invoke(reader);
                        if (funcResult == null) continue;
                        if (funcResult is TTarget) result.Add((TTarget)funcResult);
                        else result.Add((TTarget)Convert.ChangeType(funcResult, targetType, CultureInfo.InvariantCulture));
                    }
                    while (reader.NextResult()) { }
                    conn.Close();
                    reader.Dispose();
                    reader = null;
                }
            }
            else
            {
                command = this.Connection.CreateCommand();
                command.CommandText = sql;
                command.CommandType = CommandType.Text;
                if (this.Transaction != null) command.Transaction = this.Transaction;
                if (objParameter != null)
                {
                    var paramAction = GetActionCache(hashKey, sql, this.Provider, isPk);
                    paramAction(command, objParameter);
                }
                this.Open();
                reader = command.ExecuteReader(CommandBehavior.SequentialAccess);
                var func = RepositoryHelper.GetReader(hashKey, targetType, reader);
                while (reader.Read())
                {
                    object funcResult = func?.Invoke(reader);
                    if (funcResult == null) continue;
                    if (funcResult is TTarget) result.Add((TTarget)funcResult);
                    else result.Add((TTarget)Convert.ChangeType(funcResult, targetType, CultureInfo.InvariantCulture));
                }
                while (reader.NextResult()) { }
                reader.Dispose();
                reader = null;
            }
            return result;
        }
        private PagedList<TTarget> QueryPageImpl<TTarget>(int hashKey, Type targetType, string sql, CommandType cmdType, TEntity objParameter = null, bool isPk = false)
        {
            DbCommand command = null;
            string propName = String.Empty;
            DbDataReader reader = null;
            PagedList<TTarget> result = new PagedList<TTarget>();
            if (this.Connection == null)
            {
                using (var conn = this.Provider.CreateConnection(this.ConnString))
                {
                    command = conn.CreateCommand();
                    command.CommandText = sql;
                    command.CommandType = cmdType;
                    if (objParameter != null)
                    {
                        var paramAction = GetActionCache(hashKey, sql, this.Provider, isPk);
                        paramAction(command, objParameter);
                    }
                    conn.Open();
                    reader = command.ExecuteReader(CommandBehavior.CloseConnection | CommandBehavior.SequentialAccess);
                    var func = RepositoryHelper.GetReader(hashKey, targetType, reader);
                    while (reader.Read())
                    {
                        object funcResult = func?.Invoke(reader);
                        if (funcResult == null) continue;
                        if (funcResult is TTarget) result.Add((TTarget)funcResult);
                        else result.Add((TTarget)Convert.ChangeType(funcResult, targetType, CultureInfo.InvariantCulture));
                    }
                    while (reader.NextResult()) { }
                    conn.Close();
                    reader.Dispose();
                    reader = null;
                }
            }
            else
            {
                command = this.Connection.CreateCommand();
                command.CommandText = sql;
                command.CommandType = CommandType.Text;
                if (this.Transaction != null) command.Transaction = this.Transaction;
                if (objParameter != null)
                {
                    var paramAction = GetActionCache(hashKey, sql, this.Provider, isPk);
                    paramAction(command, objParameter);
                }
                this.Open();
                reader = command.ExecuteReader(CommandBehavior.SequentialAccess);
                var func = RepositoryHelper.GetReader(hashKey, targetType, reader);
                while (reader.Read())
                {
                    object funcResult = func?.Invoke(reader);
                    if (funcResult == null) continue;
                    if (funcResult is TTarget) result.Add((TTarget)funcResult);
                    else result.Add((TTarget)Convert.ChangeType(funcResult, targetType, CultureInfo.InvariantCulture));
                }
                while (reader.NextResult()) { }
                reader.Dispose();
                reader = null;
            }
            return result;
        }
        private int ExecSqlImpl(int hashKey, string sql, CommandType cmdType = CommandType.Text, TEntity objParameter = null, bool isPk = false)
        {
            DbCommand command = null;
            int result = 0;
            if (this.Connection == null)
            {
                using (var conn = this.Provider.CreateConnection(this.ConnString))
                {
                    command = conn.CreateCommand();
                    command.CommandText = sql;
                    command.CommandType = cmdType;
                    if (objParameter != null)
                    {
                        var paramAction = GetActionCache(hashKey, sql, this.Provider, isPk);
                        paramAction(command, objParameter);
                    }
                    conn.Open();
                    result = command.ExecuteNonQuery();
                    conn.Close();
                }
            }
            else
            {
                command = this.Connection.CreateCommand();
                command.CommandText = sql;
                command.CommandType = cmdType;
                if (this.Transaction != null) command.Transaction = this.Transaction;
                if (objParameter != null)
                {
                    var paramAction = GetActionCache(hashKey, sql, this.Provider, isPk);
                    paramAction(command, objParameter);
                }
                this.Open();
                result = command.ExecuteNonQuery();
            }
            return result;
        }
#if ASYNC
        private async Task<TTarget> QueryFirstImplAsync<TTarget>(int hashKey, Type targetType, string sql, CommandType cmdType, TEntity objParameter = null, bool isPk = false)
        {
            DbCommand command = null;
            string propName = String.Empty;
            DbDataReader reader = null;
            TTarget result = default(TTarget);
            if (this.Connection == null)
            {
                using (var conn = this.Provider.CreateConnection(this.ConnString))
                {
                    command = conn.CreateCommand();
                    command.CommandText = sql;
                    command.CommandType = cmdType;
                    if (objParameter != null)
                    {
                        var paramAction = GetActionCache(hashKey, sql, this.Provider, isPk);
                        paramAction(command, objParameter);
                    }
                    conn.Open();
                    reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection | CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow);
                    var func = RepositoryHelper.GetReader(hashKey, targetType, reader);
                    while (reader.Read())
                    {
                        object funcResult = func?.Invoke(reader);
                        if (funcResult == null || funcResult is TTarget) result = (TTarget)funcResult;
                        else result = (TTarget)Convert.ChangeType(funcResult, targetType, CultureInfo.InvariantCulture);
                    }
                    while (reader.NextResult()) { }
                    conn.Close();
                    reader.Dispose();
                    reader = null;
                }
            }
            else
            {
                command = this.Connection.CreateCommand();
                command.CommandText = sql;
                command.CommandType = CommandType.Text;
                if (this.Transaction != null) command.Transaction = this.Transaction;
                if (objParameter != null)
                {
                    var paramAction = GetActionCache(hashKey, sql, this.Provider, isPk);
                    paramAction(command, objParameter);
                }
                this.Open();
                reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow);
                var func = RepositoryHelper.GetReader(hashKey, targetType, reader);
                while (reader.Read())
                {
                    object funcResult = func?.Invoke(reader);
                    if (funcResult == null || funcResult is TTarget) result = (TTarget)funcResult;
                    else result = (TTarget)Convert.ChangeType(funcResult, targetType, CultureInfo.InvariantCulture);
                }
                while (reader.NextResult()) { }
                reader.Dispose();
                reader = null;
            }
            return result;
        }
        private async Task<List<TTarget>> QueryImplAsync<TTarget>(int hashKey, Type targetType, string sql, CommandType cmdType, TEntity objParameter = null, bool isPk = false)
        {
            DbCommand command = null;
            string propName = String.Empty;
            DbDataReader reader = null;
            List<TTarget> result = new List<TTarget>();
            if (this.Connection == null)
            {
                using (var conn = this.Provider.CreateConnection(this.ConnString))
                {
                    command = conn.CreateCommand();
                    command.CommandText = sql;
                    command.CommandType = cmdType;
                    if (objParameter != null)
                    {
                        var paramAction = GetActionCache(hashKey, sql, this.Provider, isPk);
                        paramAction(command, objParameter);
                    }
                    conn.Open();
                    reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection | CommandBehavior.SequentialAccess);
                    var func = RepositoryHelper.GetReader(hashKey, targetType, reader);
                    while (reader.Read())
                    {
                        object funcResult = func?.Invoke(reader);
                        if (funcResult == null) continue;
                        if (funcResult is TTarget) result.Add((TTarget)funcResult);
                        else result.Add((TTarget)Convert.ChangeType(funcResult, targetType, CultureInfo.InvariantCulture));
                    }
                    while (reader.NextResult()) { }
                    conn.Close();
                    reader.Dispose();
                    reader = null;
                }
            }
            else
            {
                command = this.Connection.CreateCommand();
                command.CommandText = sql;
                command.CommandType = CommandType.Text;
                if (this.Transaction != null) command.Transaction = this.Transaction;
                if (objParameter != null)
                {
                    var paramAction = GetActionCache(hashKey, sql, this.Provider, isPk);
                    paramAction(command, objParameter);
                }
                this.Open();
                reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess);
                var func = RepositoryHelper.GetReader(hashKey, targetType, reader);
                while (reader.Read())
                {
                    object funcResult = func?.Invoke(reader);
                    if (funcResult == null) continue;
                    if (funcResult is TTarget) result.Add((TTarget)funcResult);
                    else result.Add((TTarget)Convert.ChangeType(funcResult, targetType, CultureInfo.InvariantCulture));
                }
                while (reader.NextResult()) { }
                reader.Dispose();
                reader = null;
            }
            return result;
        }
        private async Task<PagedList<TTarget>> QueryPageImplAsync<TTarget>(int hashKey, Type targetType, string sql, CommandType cmdType, TEntity objParameter = null, bool isPk = false)
        {
            DbCommand command = null;
            string propName = String.Empty;
            DbDataReader reader = null;
            PagedList<TTarget> result = new PagedList<TTarget>();
            if (this.Connection == null)
            {
                using (var conn = this.Provider.CreateConnection(this.ConnString))
                {
                    command = conn.CreateCommand();
                    command.CommandText = sql;
                    command.CommandType = cmdType;
                    if (objParameter != null)
                    {
                        var paramAction = GetActionCache(hashKey, sql, this.Provider, isPk);
                        paramAction(command, objParameter);
                    }
                    conn.Open();
                    reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection | CommandBehavior.SequentialAccess);
                    var func = RepositoryHelper.GetReader(hashKey, targetType, reader);
                    while (reader.Read())
                    {
                        object funcResult = func?.Invoke(reader);
                        if (funcResult == null) continue;
                        if (funcResult is TTarget) result.Add((TTarget)funcResult);
                        else result.Add((TTarget)Convert.ChangeType(funcResult, targetType, CultureInfo.InvariantCulture));
                    }
                    while (reader.NextResult()) { }
                    conn.Close();
                    reader.Dispose();
                    reader = null;
                }
            }
            else
            {
                command = this.Connection.CreateCommand();
                command.CommandText = sql;
                command.CommandType = CommandType.Text;
                if (this.Transaction != null) command.Transaction = this.Transaction;
                if (objParameter != null)
                {
                    var paramAction = GetActionCache(hashKey, sql, this.Provider, isPk);
                    paramAction(command, objParameter);
                }
                this.Open();
                reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess);
                var func = RepositoryHelper.GetReader(hashKey, targetType, reader);
                while (reader.Read())
                {
                    object funcResult = func?.Invoke(reader);
                    if (funcResult == null) continue;
                    if (funcResult is TTarget) result.Add((TTarget)funcResult);
                    else result.Add((TTarget)Convert.ChangeType(funcResult, targetType, CultureInfo.InvariantCulture));
                }
                while (reader.NextResult()) { }
                reader.Dispose();
                reader = null;
            }
            return result;
        }
        private async Task<int> ExecSqlImplAsync(int hashKey, string sql, CommandType cmdType = CommandType.Text, TEntity objParameter = null, bool isPk = false)
        {
            DbCommand command = null;
            int result = 0;
            if (this.Connection == null)
            {
                using (var conn = this.Provider.CreateConnection(this.ConnString))
                {
                    command = conn.CreateCommand();
                    command.CommandText = sql;
                    command.CommandType = cmdType;
                    if (objParameter != null)
                    {
                        var paramAction = GetActionCache(hashKey, sql, this.Provider, isPk);
                        paramAction(command, objParameter);
                    }
                    conn.Open();
                    result = await command.ExecuteNonQueryAsync();
                    conn.Close();
                }
            }
            else
            {
                command = this.Connection.CreateCommand();
                command.CommandText = sql;
                command.CommandType = cmdType;
                if (this.Transaction != null) command.Transaction = this.Transaction;
                if (objParameter != null)
                {
                    var paramAction = GetActionCache(hashKey, sql, this.Provider, isPk);
                    paramAction(command, objParameter);
                }
                this.Open();
                result = await command.ExecuteNonQueryAsync();
            }
            return result;
        }
#endif
        private static string BuildCreateSql(EntityMapper mapper, IOrmProvider provider)
        {
            StringBuilder insertBuilder = new StringBuilder();
            insertBuilder.Append("INSERT INTO " + provider.GetQuotedTableName(mapper.TableName) + " (");
            StringBuilder valueBuilder = new StringBuilder();
            valueBuilder.Append(") VALUES(");
            int i = 0;
            foreach (var colMapper in mapper.MemberMappers.Values)
            {
                if (colMapper.IsAutoIncrement) continue;
                if (i > 0) insertBuilder.Append(",");
                if (i > 0) valueBuilder.Append(",");
                insertBuilder.Append(provider.GetQuotedColumnName(colMapper.FieldName));
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
            sqlBuilder.Append(" FROM " + provider.GetQuotedTableName(mapper.TableName));
            return sqlBuilder.ToString() + whereBuilder.ToString();
        }
        private static string BuildDeleteSql(EntityMapper mapper, IOrmProvider provider)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("DELETE FROM " + provider.GetQuotedTableName(mapper.TableName) + " WHERE ");
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
            var builder = new StringBuilder("UPDATE " + provider.GetQuotedTableName(mapper.TableName) + " SET ");
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
            var builder = new StringBuilder("UPDATE " + Mapper.TableName + " SET ");
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
            return provider.GetQuotedColumnName(colMapper.FieldName) + "=" + provider.ParamPrefix + colMapper.MemberName;
        }
        private static string GetAliasParameterSql(IOrmProvider provider, MemberMapper colMapper)
        {
            return provider.GetQuotedColumnName(colMapper.FieldName) + (colMapper.MemberName == colMapper.FieldName ? String.Empty : " AS " + colMapper.MemberName);
        }
        #endregion
    }
}
