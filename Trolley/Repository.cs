using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
#if ASYNC
using System.Threading.Tasks;
#endif

namespace Trolley
{
    public class Repository : IRepository
    {
        #region 属性
        protected DbConnection Connection { get; private set; }       
        protected IRepositoryContext DbContext { get; private set; }
        protected DbTransaction Transaction { get { return this.DbContext.Transaction; } }
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
        public Repository(string connString, IRepositoryContext dbContext)
        {
            this.ConnString = connString;
            this.DbContext = dbContext;
            this.Connection = dbContext.Connection;
            this.Provider = OrmProviderFactory.GetProvider(connString);
        }
        #endregion

        #region 同步方法
        public TEntity QueryFirst<TEntity>(string sql, object objParameter = null, CommandType cmdType = CommandType.Text)
        {
            Type paramType = objParameter != null ? objParameter.GetType() : null;
            int cacheKey = RepositoryHelper.GetHashKey(this.ConnString, sql, paramType);
            return this.QueryFirstImpl<TEntity>(cacheKey, typeof(TEntity), sql, cmdType, objParameter, paramType);
        }
        public List<TEntity> Query<TEntity>(string sql, object objParameter = null, CommandType cmdType = CommandType.Text)
        {
            Type paramType = objParameter != null ? objParameter.GetType() : null;
            int cacheKey = RepositoryHelper.GetHashKey(this.ConnString, sql, paramType);
            return this.QueryImpl<TEntity>(cacheKey, typeof(TEntity), sql, cmdType, objParameter, paramType);
        }
        public PagedList<TEntity> QueryPage<TEntity>(string sql, int pageIndex, int pageSize, string orderBy = null, object objParameter = null, CommandType cmdType = CommandType.Text)
        {
            var builder = new SqlBuilder(this.Provider);
            string countSql = this.Provider.GetPagingCountExpression(sql);
            builder.RawSql(countSql + ";" + sql).Paging(pageIndex, pageSize, orderBy);
            var reader = this.QueryMultiple(builder.BuildSql(), objParameter, cmdType);
            var count = reader.Read<int>();
            return reader.ReadPageList<TEntity>(pageIndex, pageSize, count);
        }
        public QueryReader QueryMultiple(string sql, object objParameter = null, CommandType cmdType = CommandType.Text)
        {
            Type paramType = objParameter != null ? objParameter.GetType() : null;
            int cacheKey = RepositoryHelper.GetHashKey(this.ConnString, sql);
            if (this.Connection == null)
            {
                return this.QueryMultipleImpl(cacheKey, sql, this.Provider.CreateConnection(this.ConnString), null, cmdType, CommandBehavior.SequentialAccess, objParameter, paramType, true);
            }
            else return this.QueryMultipleImpl(cacheKey, sql, this.Connection, this.Transaction, cmdType, CommandBehavior.SequentialAccess, objParameter, paramType, false);
        }
        public TResult QueryMap<TResult>(Func<QueryReader, TResult> mapping, string sql, object objParameter = null, CommandType cmdType = CommandType.Text)
        {
            Type paramType = objParameter != null ? objParameter.GetType() : null;
            int cacheKey = RepositoryHelper.GetHashKey(this.ConnString, sql);
            QueryReader reader = null;
            if (this.Connection == null)
            {
                reader = this.QueryMultipleImpl(cacheKey, sql, this.Provider.CreateConnection(this.ConnString), null, cmdType, CommandBehavior.SequentialAccess, objParameter, paramType, true);
            }
            else reader = this.QueryMultipleImpl(cacheKey, sql, this.Connection, this.Transaction, cmdType, CommandBehavior.SequentialAccess, objParameter, paramType, false);
            return mapping(reader);
        }
        public Dictionary<TKey, TValue> QueryDictionary<TKey, TValue>(string sql, object objParameter = null, CommandType cmdType = CommandType.Text)
        {
            Type paramType = objParameter != null ? objParameter.GetType() : null;
            int cacheKey = RepositoryHelper.GetHashKey(this.ConnString, sql);
            if (this.Connection == null)
            {
                return this.QueryDictionaryImplInternal<TKey, TValue>(cacheKey, sql, this.Provider.CreateConnection(this.ConnString), null, cmdType, CommandBehavior.SequentialAccess, objParameter, paramType);
            }
            else return this.QueryDictionaryImplInternal<TKey, TValue>(cacheKey, sql, this.Connection, this.Transaction, cmdType, CommandBehavior.SequentialAccess, objParameter, paramType);
        }
        public int ExecSql(string sql, object objParameter = null, CommandType cmdType = CommandType.Text)
        {
            Type paramType = objParameter != null ? objParameter.GetType() : null;
            int cacheKey = RepositoryHelper.GetHashKey(this.ConnString, sql, paramType);
            return this.ExecSqlImpl(cacheKey, sql, cmdType, objParameter, paramType);
        }
        #endregion

        #region 异步方法
#if ASYNC
        public async Task<TEntity> QueryFirstAsync<TEntity>(string sql, object objParameter = null, CommandType cmdType = CommandType.Text)
        {
            Type paramType = objParameter != null ? objParameter.GetType() : null;
            int cacheKey = RepositoryHelper.GetHashKey(this.ConnString, sql, paramType);
            return await this.QueryFirstImplAsync<TEntity>(cacheKey, typeof(TEntity), sql, cmdType, objParameter, paramType);
        }
        public async Task<List<TEntity>> QueryAsync<TEntity>(string sql, object objParameter = null, CommandType cmdType = CommandType.Text)
        {
            Type paramType = objParameter != null ? objParameter.GetType() : null;
            int cacheKey = RepositoryHelper.GetHashKey(this.ConnString, sql, paramType);
            return await this.QueryImplAsync<TEntity>(cacheKey, typeof(TEntity), sql, cmdType, objParameter, paramType);
        }
        public async Task<PagedList<TEntity>> QueryPageAsync<TEntity>(string sql, int pageIndex, int pageSize, string orderBy = null, object objParameter = null, CommandType cmdType = CommandType.Text)
        {
            var builder = new SqlBuilder(this.Provider);
            string countSql = this.Provider.GetPagingCountExpression(sql);
            builder.RawSql(countSql + ";" + sql).Paging(pageIndex, pageSize, orderBy);
            var reader = await this.QueryMultipleAsync(builder.BuildSql(), objParameter, cmdType);
            var count = reader.Read<int>();
            return reader.ReadPageList<TEntity>(pageIndex, pageSize, count);
        }
        public async Task<QueryReader> QueryMultipleAsync(string sql, object objParameter = null, CommandType cmdType = CommandType.Text)
        {
            Type paramType = objParameter != null ? objParameter.GetType() : null;
            int cacheKey = RepositoryHelper.GetHashKey(this.ConnString, sql);
            if (this.Connection == null)
            {
                return await this.QueryMultipleImplAsync(cacheKey, sql, this.Provider.CreateConnection(this.ConnString), null, cmdType, CommandBehavior.SequentialAccess, objParameter, paramType, true);
            }
            else return await this.QueryMultipleImplAsync(cacheKey, sql, this.Connection, this.Transaction, cmdType, CommandBehavior.SequentialAccess, objParameter, paramType, false);
        }
        public async Task<TResult> QueryMapAsync<TResult>(Func<QueryReader, TResult> mapping, string sql, object objParameter = null, CommandType cmdType = CommandType.Text)
        {
            Type paramType = objParameter != null ? objParameter.GetType() : null;
            int cacheKey = RepositoryHelper.GetHashKey(this.ConnString, sql);
            QueryReader reader = null;
            if (this.Connection == null)
            {
                reader = await this.QueryMultipleImplAsync(cacheKey, sql, this.Provider.CreateConnection(this.ConnString), null, cmdType, CommandBehavior.SequentialAccess, objParameter, paramType, true);
            }
            else reader = await this.QueryMultipleImplAsync(cacheKey, sql, this.Connection, this.Transaction, cmdType, CommandBehavior.SequentialAccess, objParameter, paramType, false);
            return mapping(reader);
        }
        public async Task<Dictionary<TKey, TValue>> QueryDictionaryAsync<TKey, TValue>(string sql, object objParameter = null, CommandType cmdType = CommandType.Text)
        {
            Type paramType = objParameter != null ? objParameter.GetType() : null;
            int cacheKey = RepositoryHelper.GetHashKey(this.ConnString, sql);
            if (this.Connection == null)
            {
                return await this.QueryDictionaryImplInternalAsync<TKey, TValue>(cacheKey, sql, this.Provider.CreateConnection(this.ConnString), null, cmdType, CommandBehavior.SequentialAccess, objParameter, paramType);
            }
            else return await this.QueryDictionaryImplInternalAsync<TKey, TValue>(cacheKey, sql, this.Connection, this.Transaction, cmdType, CommandBehavior.SequentialAccess, objParameter, paramType);
        }
        public async Task<int> ExecSqlAsync(string sql, object objParameter = null, CommandType cmdType = CommandType.Text)
        {
            Type paramType = objParameter != null ? objParameter.GetType() : null;
            int cacheKey = RepositoryHelper.GetHashKey(this.ConnString, sql, paramType);
            return await this.ExecSqlImplAsync(cacheKey, sql, cmdType, objParameter, paramType);
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
        private TEntity QueryFirstImpl<TEntity>(int hashKey, Type entityType, string sql, CommandType cmdType, object objParameter = null, Type paramType = null)
        {
            TEntity result = default(TEntity);
            var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
            if (this.Connection == null)
            {
                using (var conn = this.Provider.CreateConnection(this.ConnString))
                {
                    behavior = CommandBehavior.CloseConnection | behavior;
                    this.QueryImplInternal<TEntity>(hashKey, entityType, sql, conn, null, cmdType, behavior, objResult =>
                    {
                        if (objResult == null || objResult is TEntity) result = (TEntity)objResult;
                        else result = (TEntity)Convert.ChangeType(objResult, entityType, CultureInfo.InvariantCulture);
                    }, objParameter, paramType);
                }
            }
            else this.QueryImplInternal<TEntity>(hashKey, entityType, sql, this.Connection, this.Transaction, cmdType, behavior, objResult =>
            {
                if (objResult == null || objResult is TEntity) result = (TEntity)objResult;
                else result = (TEntity)Convert.ChangeType(objResult, entityType, CultureInfo.InvariantCulture);
            }, objParameter, paramType);
            return result;
        }
        private List<TEntity> QueryImpl<TEntity>(int hashKey, Type entityType, string sql, CommandType cmdType, object objParameter = null, Type paramType = null)
        {
            List<TEntity> result = new List<TEntity>();
            var behavior = CommandBehavior.SequentialAccess;
            if (this.Connection == null)
            {
                using (var conn = this.Provider.CreateConnection(this.ConnString))
                {
                    behavior = CommandBehavior.CloseConnection | behavior;
                    this.QueryImplInternal<TEntity>(hashKey, entityType, sql, conn, null, cmdType, behavior, objResult =>
                    {
                        if (objResult == null) return;
                        if (objResult is TEntity) result.Add((TEntity)objResult);
                        else result.Add((TEntity)Convert.ChangeType(objResult, entityType, CultureInfo.InvariantCulture));
                    }, objParameter, paramType);
                }
            }
            else this.QueryImplInternal<TEntity>(hashKey, entityType, sql, this.Connection, this.Transaction, cmdType, behavior, objResult =>
            {
                if (objResult == null) return;
                if (objResult is TEntity) result.Add((TEntity)objResult);
                else result.Add((TEntity)Convert.ChangeType(objResult, entityType, CultureInfo.InvariantCulture));
            }, objParameter, paramType);
            return result;
        }
        private QueryReader QueryMultipleImpl(int hashKey, string sql, DbConnection conn, DbTransaction trans, CommandType cmdType, CommandBehavior behavior, object objParameter, Type paramType, bool isCloseConnection)
        {
            DbCommand command = conn.CreateCommand();
            command.CommandText = sql;
            command.CommandType = cmdType;
            command.Transaction = trans;
            if (objParameter != null)
            {
                var paramAction = RepositoryHelper.GetActionCache(hashKey, sql, paramType, this.Provider);
                paramAction(command, objParameter);
            }
            this.Open(conn);
            DbDataReader reader = command.ExecuteReader(behavior);
            return new QueryReader(hashKey, command, reader, this.Provider.IsMappingIgnoreCase, isCloseConnection);
        }
        private int ExecSqlImpl(int hashKey, string sql, CommandType cmdType = CommandType.Text, object objParameter = null, Type paramType = null)
        {
            int result = 0;
            if (this.Connection == null)
            {
                using (var conn = this.Provider.CreateConnection(this.ConnString))
                {
                    result = this.ExecSqlImplInternal(hashKey, sql, conn, null, cmdType, objParameter, paramType);
                    conn.Close();
                }
            }
            else result = this.ExecSqlImplInternal(hashKey, sql, this.Connection, this.Transaction, cmdType, objParameter, paramType);
            return result;
        }
        private void QueryImplInternal<TTarget>(int hashKey, Type targetType, string sql, DbConnection conn, DbTransaction trans, CommandType cmdType, CommandBehavior behavior, Action<object> resultHandler, object objParameter, Type paramType)
        {
            DbCommand command = conn.CreateCommand();
            command.CommandText = sql;
            command.CommandType = cmdType;
            command.Transaction = trans;
            if (objParameter != null)
            {
                var paramAction = RepositoryHelper.GetActionCache(hashKey, sql, paramType, this.Provider);
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
        private Dictionary<TKey, TValue> QueryDictionaryImplInternal<TKey, TValue>(int hashKey, string sql, DbConnection conn, DbTransaction trans, CommandType cmdType, CommandBehavior behavior, object objParameter, Type paramType)
        {
            DbCommand command = conn.CreateCommand();
            command.CommandText = sql;
            command.CommandType = cmdType;
            command.Transaction = trans;
            if (objParameter != null)
            {
                var paramAction = RepositoryHelper.GetActionCache(hashKey, sql, paramType, this.Provider);
                paramAction(command, objParameter);
            }
            this.Open(conn);
            DbDataReader reader = command.ExecuteReader(behavior);
            int keyIndex = reader.GetOrdinal(nameof(KeyValuePair<TKey, TValue>.Key));
            int valueIndex = reader.GetOrdinal(nameof(KeyValuePair<TKey, TValue>.Value));
            Dictionary<TKey, TValue> result = new Dictionary<TKey, TValue>();
            while (reader.Read())
            {
                result.Add(reader.GetFieldValue<TKey>(keyIndex), reader.GetFieldValue<TValue>(valueIndex));
            }
            while (reader.NextResult()) { }
#if COREFX
            try { command.Cancel(); } catch { }
#else
            reader.Close();
#endif
            reader.Dispose();
            reader = null;
            return result;
        }
        private int ExecSqlImplInternal(int hashKey, string sql, DbConnection conn, DbTransaction trans, CommandType cmdType, object objParameter, Type paramType)
        {
            DbCommand command = conn.CreateCommand();
            command.CommandText = sql;
            command.CommandType = cmdType;
            command.Transaction = trans;
            if (objParameter != null)
            {
                var paramAction = RepositoryHelper.GetActionCache(hashKey, sql, paramType, this.Provider);
                paramAction(command, objParameter);
            }
            this.Open(conn);
            return command.ExecuteNonQuery();
        }
#if ASYNC
        private async Task<TEntity> QueryFirstImplAsync<TEntity>(int hashKey, Type entityType, string sql, CommandType cmdType, object objParameter = null, Type paramType = null)
        {
            TEntity result = default(TEntity);
            var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
            if (this.Connection == null)
            {
                using (var conn = this.Provider.CreateConnection(this.ConnString))
                {
                    behavior = CommandBehavior.CloseConnection | behavior;
                    await this.QueryImplInternalAsync<TEntity>(hashKey, entityType, sql, conn, null, cmdType, behavior, objResult =>
                    {
                        if (objResult == null || objResult is TEntity) result = (TEntity)objResult;
                        else result = (TEntity)Convert.ChangeType(objResult, entityType, CultureInfo.InvariantCulture);
                    }, objParameter, paramType);
                }
            }
            else await this.QueryImplInternalAsync<TEntity>(hashKey, entityType, sql, this.Connection, this.Transaction, cmdType, behavior, objResult =>
            {
                if (objResult == null || objResult is TEntity) result = (TEntity)objResult;
                else result = (TEntity)Convert.ChangeType(objResult, entityType, CultureInfo.InvariantCulture);
            }, objParameter, paramType);
            return result;
        }
        private async Task<List<TEntity>> QueryImplAsync<TEntity>(int hashKey, Type entityType, string sql, CommandType cmdType, object objParameter = null, Type paramType = null)
        {
            List<TEntity> result = new List<TEntity>();
            var behavior = CommandBehavior.SequentialAccess;
            if (this.Connection == null)
            {
                using (var conn = this.Provider.CreateConnection(this.ConnString))
                {
                    behavior = CommandBehavior.CloseConnection | behavior;
                    await this.QueryImplInternalAsync<TEntity>(hashKey, entityType, sql, conn, null, cmdType, behavior, objResult =>
                    {
                        if (objResult == null) return;
                        if (objResult is TEntity) result.Add((TEntity)objResult);
                        else result.Add((TEntity)Convert.ChangeType(objResult, entityType, CultureInfo.InvariantCulture));
                    }, objParameter, paramType);
                }
            }
            else await this.QueryImplInternalAsync<TEntity>(hashKey, entityType, sql, this.Connection, this.Transaction, cmdType, behavior, objResult =>
            {
                if (objResult == null) return;
                if (objResult is TEntity) result.Add((TEntity)objResult);
                else result.Add((TEntity)Convert.ChangeType(objResult, entityType, CultureInfo.InvariantCulture));
            }, objParameter, paramType);
            return result;
        }
        private async Task<QueryReader> QueryMultipleImplAsync(int hashKey, string sql, DbConnection conn, DbTransaction trans, CommandType cmdType, CommandBehavior behavior, object objParameter, Type paramType, bool isCloseConnection)
        {
            DbCommand command = conn.CreateCommand();
            command.CommandText = sql;
            command.CommandType = cmdType;
            command.Transaction = trans;
            if (objParameter != null)
            {
                var paramAction = RepositoryHelper.GetActionCache(hashKey, sql, paramType, this.Provider);
                paramAction(command, objParameter);
            }
            this.Open(conn);
            DbDataReader reader = await command.ExecuteReaderAsync(behavior);
            return new QueryReader(hashKey, command, reader, this.Provider.IsMappingIgnoreCase, isCloseConnection);
        }
        private async Task<int> ExecSqlImplAsync(int hashKey, string sql, CommandType cmdType = CommandType.Text, object objParameter = null, Type paramType = null)
        {
            int result = 0;
            if (this.Connection == null)
            {
                using (var conn = this.Provider.CreateConnection(this.ConnString))
                {
                    result = await this.ExecSqlImplInternalAsync(hashKey, sql, conn, null, cmdType, objParameter, paramType);
                    conn.Close();
                }
            }
            else result = await this.ExecSqlImplInternalAsync(hashKey, sql, this.Connection, this.Transaction, cmdType, objParameter, paramType);
            return result;
        }
        private async Task QueryImplInternalAsync<TTarget>(int hashKey, Type targetType, string sql, DbConnection conn, DbTransaction trans, CommandType cmdType, CommandBehavior behavior, Action<object> resultHandler, object objParameter, Type paramType)
        {
            DbCommand command = conn.CreateCommand();
            command.CommandText = sql;
            command.CommandType = cmdType;
            command.Transaction = trans;
            if (objParameter != null)
            {
                var paramAction = RepositoryHelper.GetActionCache(hashKey, sql, paramType, this.Provider);
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
        private async Task<Dictionary<TKey, TValue>> QueryDictionaryImplInternalAsync<TKey, TValue>(int hashKey, string sql, DbConnection conn, DbTransaction trans, CommandType cmdType, CommandBehavior behavior, object objParameter, Type paramType)
        {
            DbCommand command = conn.CreateCommand();
            command.CommandText = sql;
            command.CommandType = cmdType;
            command.Transaction = trans;
            if (objParameter != null)
            {
                var paramAction = RepositoryHelper.GetActionCache(hashKey, sql, paramType, this.Provider);
                paramAction(command, objParameter);
            }
            this.Open(conn);
            DbDataReader reader = await command.ExecuteReaderAsync(behavior);
            int keyIndex = reader.GetOrdinal(nameof(KeyValuePair<TKey, TValue>.Key));
            int valueIndex = reader.GetOrdinal(nameof(KeyValuePair<TKey, TValue>.Value));
            Dictionary<TKey, TValue> result = new Dictionary<TKey, TValue>();
            while (reader.Read())
            {
                result.Add(reader.GetFieldValue<TKey>(keyIndex), reader.GetFieldValue<TValue>(valueIndex));
            }
            while (reader.NextResult()) { }
#if COREFX
            try { command.Cancel(); } catch { }
#else
            reader.Close();
#endif
            reader.Dispose();
            reader = null;
            return result;
        }
        private async Task<int> ExecSqlImplInternalAsync(int hashKey, string sql, DbConnection conn, DbTransaction trans, CommandType cmdType, object objParameter, Type paramType)
        {
            DbCommand command = conn.CreateCommand();
            command.CommandText = sql;
            command.CommandType = cmdType;
            command.Transaction = trans;
            if (objParameter != null)
            {
                var paramAction = RepositoryHelper.GetActionCache(hashKey, sql, paramType, this.Provider);
                paramAction(command, objParameter);
            }
            this.Open(conn);
            return await command.ExecuteNonQueryAsync();
        }
#endif
        #endregion
    }
}
