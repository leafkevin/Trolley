using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Trolley
{
    public class Repository : IRepository
    {
        #region 属性
        protected DbConnection Connection { get; private set; }
        protected IRepositoryContext DbContext { get; private set; }
        protected DbTransaction Transaction => this.DbContext?.Transaction;
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
            TEntity result = default(TEntity);
            var paramType = objParameter != null ? objParameter.GetType() : null;
            int hashKey = RepositoryHelper.GetHashKey(this.ConnString, sql, paramType);
            var entityType = typeof(TEntity);
            var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
            if (this.Connection == null)
            {
                using (var conn = this.Provider.CreateConnection(this.ConnString))
                {
                    behavior = CommandBehavior.CloseConnection | behavior;
                    this.QueryImpl<TEntity>(hashKey, entityType, sql, conn, null, cmdType, behavior, objResult =>
                    {
                        if (objResult == null || objResult is TEntity) result = (TEntity)objResult;
                        else result = (TEntity)Convert.ChangeType(objResult, entityType, CultureInfo.InvariantCulture);
                    }, objParameter, paramType);
                    conn.Close();
                }
            }
            else this.QueryImpl<TEntity>(hashKey, entityType, sql, this.Connection, this.Transaction, cmdType, behavior, objResult =>
            {
                if (objResult == null || objResult is TEntity) result = (TEntity)objResult;
                else result = (TEntity)Convert.ChangeType(objResult, entityType, CultureInfo.InvariantCulture);
            }, objParameter, paramType);
            return result;
        }
        public TEntity QueryFirst<TEntity>(Action<SqlBuilder> builder)
        {
            TEntity result = default(TEntity);
            var entityType = typeof(TEntity);
            var sqlBuilder = new SqlBuilder(this.Provider);
            builder.Invoke(sqlBuilder);
            var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
            if (this.Connection == null)
            {
                using (var conn = this.Provider.CreateConnection(this.ConnString))
                {
                    behavior = CommandBehavior.CloseConnection | behavior;
                    this.QueryImpl(entityType, sqlBuilder, conn, null, CommandType.Text, behavior, objResult =>
                    {
                        if (objResult == null || objResult is TEntity) result = (TEntity)objResult;
                        else result = (TEntity)Convert.ChangeType(objResult, entityType, CultureInfo.InvariantCulture);
                    });
                    conn.Close();
                }
            }
            else this.QueryImpl(entityType, sqlBuilder, this.Connection, this.Transaction, CommandType.Text, behavior, objResult =>
            {
                if (objResult == null || objResult is TEntity) result = (TEntity)objResult;
                else result = (TEntity)Convert.ChangeType(objResult, entityType, CultureInfo.InvariantCulture);
            });
            return result;
        }
        public List<TEntity> Query<TEntity>(string sql, object objParameter = null, CommandType cmdType = CommandType.Text)
        {
            List<TEntity> result = new List<TEntity>();
            var paramType = objParameter != null ? objParameter.GetType() : null;
            int hashKey = RepositoryHelper.GetHashKey(this.ConnString, sql, paramType);
            var entityType = typeof(TEntity);
            var behavior = CommandBehavior.SequentialAccess;
            if (this.Connection == null)
            {
                using (var conn = this.Provider.CreateConnection(this.ConnString))
                {
                    behavior = CommandBehavior.CloseConnection | behavior;
                    this.QueryImpl<TEntity>(hashKey, entityType, sql, conn, null, cmdType, behavior, objResult =>
                    {
                        if (objResult == null) return;
                        if (objResult is TEntity) result.Add((TEntity)objResult);
                        else result.Add((TEntity)Convert.ChangeType(objResult, entityType, CultureInfo.InvariantCulture));
                    }, objParameter, paramType);
                    conn.Close();
                }
            }
            else this.QueryImpl<TEntity>(hashKey, entityType, sql, this.Connection, this.Transaction, cmdType, behavior, objResult =>
            {
                if (objResult == null) return;
                if (objResult is TEntity) result.Add((TEntity)objResult);
                else result.Add((TEntity)Convert.ChangeType(objResult, entityType, CultureInfo.InvariantCulture));
            }, objParameter, paramType);
            return result;
        }
        public List<TEntity> Query<TEntity>(Action<SqlBuilder> builder)
        {
            List<TEntity> result = new List<TEntity>();
            var entityType = typeof(TEntity);
            var sqlBuilder = new SqlBuilder(this.Provider);
            builder.Invoke(sqlBuilder);
            var behavior = CommandBehavior.SequentialAccess;

            if (this.Connection == null)
            {
                using (var conn = this.Provider.CreateConnection(this.ConnString))
                {
                    behavior = CommandBehavior.CloseConnection | behavior;
                    this.QueryImpl(entityType, sqlBuilder, conn, null, CommandType.Text, behavior, objResult =>
                    {
                        if (objResult == null) return;
                        if (objResult is TEntity) result.Add((TEntity)objResult);
                        else result.Add((TEntity)Convert.ChangeType(objResult, entityType, CultureInfo.InvariantCulture));
                    });
                    conn.Close();
                }
            }
            else this.QueryImpl(entityType, sqlBuilder, this.Connection, this.Transaction, CommandType.Text, behavior, objResult =>
            {
                if (objResult == null) return;
                if (objResult is TEntity) result.Add((TEntity)objResult);
                else result.Add((TEntity)Convert.ChangeType(objResult, entityType, CultureInfo.InvariantCulture));
            });
            return result;
        }
        public PagedList<TEntity> QueryPage<TEntity>(string sql, int pageIndex, int pageSize, string orderBy = null, object objParameter = null, CommandType cmdType = CommandType.Text)
        {
            var pagingSql = this.Provider.GetPagingExpression(sql, pageIndex * pageSize, pageSize, orderBy);
            string countSql = this.Provider.GetPagingCountExpression(sql);
            var reader = this.QueryMultiple(countSql + ";" + pagingSql, objParameter, cmdType);
            var count = reader.Read<int>();
            return reader.ReadPageList<TEntity>(pageIndex, pageSize, count);
        }
        public PagedList<TEntity> QueryPage<TEntity>(Action<SqlBuilder> builder, int pageIndex, int pageSize, string orderBy = null)
        {
            var sqlBuilder = new SqlBuilder(this.Provider);
            builder.Invoke(sqlBuilder);
            var sql = sqlBuilder.BuildSql();
            var pagingSql = this.Provider.GetPagingExpression(sql, pageIndex * pageSize, pageSize, orderBy);
            string countSql = this.Provider.GetPagingCountExpression(sql);
            sql = countSql + ";" + pagingSql;
            QueryReader reader = null;
            var conn = this.Connection;
            if (this.Connection == null)
            {
                conn = this.Provider.CreateConnection(this.ConnString);
                reader = this.QueryMultipleImpl(sql, sqlBuilder, conn, null, CommandType.Text, CommandBehavior.SequentialAccess, true);
            }
            else reader = this.QueryMultipleImpl(sql, sqlBuilder, this.Connection, this.Transaction, CommandType.Text, CommandBehavior.SequentialAccess, false);
            var count = reader.Read<int>();
            return reader.ReadPageList<TEntity>(pageIndex, pageSize, count);
        }
        public QueryReader QueryMultiple(string sql, object objParameter = null, CommandType cmdType = CommandType.Text)
        {
            var paramType = objParameter != null ? objParameter.GetType() : null;
            int hashKey = RepositoryHelper.GetHashKey(this.ConnString, sql, paramType);
            if (this.Connection == null)
            {
                return this.QueryMultipleImpl(hashKey, sql, this.Provider.CreateConnection(this.ConnString), null, cmdType, CommandBehavior.SequentialAccess, objParameter, paramType, true);
            }
            else return this.QueryMultipleImpl(hashKey, sql, this.Connection, this.Transaction, cmdType, CommandBehavior.SequentialAccess, objParameter, paramType, false);
        }
        public QueryReader QueryMultiple(Action<SqlBuilder> builder)
        {
            var sqlBuilder = new SqlBuilder(this.Provider);
            builder.Invoke(sqlBuilder);
            var sql = sqlBuilder.BuildSql();
            if (this.Connection == null)
            {
                return this.QueryMultipleImpl(sql, sqlBuilder, this.Provider.CreateConnection(this.ConnString), null, CommandType.Text, CommandBehavior.SequentialAccess, true);
            }
            else return this.QueryMultipleImpl(sql, sqlBuilder, this.Connection, this.Transaction, CommandType.Text, CommandBehavior.SequentialAccess, false);
        }
        public TResult QueryMap<TResult>(string sql, Func<QueryReader, TResult> mapping, object objParameter = null, CommandType cmdType = CommandType.Text)
        {
            var paramType = objParameter != null ? objParameter.GetType() : null;
            int hashKey = RepositoryHelper.GetHashKey(this.ConnString, sql, paramType);
            QueryReader reader = null;
            if (this.Connection == null)
            {
                reader = this.QueryMultipleImpl(hashKey, sql, this.Provider.CreateConnection(this.ConnString), null, cmdType, CommandBehavior.SequentialAccess, objParameter, paramType, true);
            }
            else reader = this.QueryMultipleImpl(hashKey, sql, this.Connection, this.Transaction, cmdType, CommandBehavior.SequentialAccess, objParameter, paramType, false);
            return mapping(reader);
        }
        public TResult QueryMap<TResult>(Action<SqlBuilder> builder, Func<QueryReader, TResult> mapping)
        {
            var sqlBuilder = new SqlBuilder(this.Provider);
            builder.Invoke(sqlBuilder);
            var sql = sqlBuilder.BuildSql();
            QueryReader reader = null;
            if (this.Connection == null)
            {
                reader = this.QueryMultipleImpl(sql, sqlBuilder, this.Provider.CreateConnection(this.ConnString), null, CommandType.Text, CommandBehavior.SequentialAccess, true);
            }
            else reader = this.QueryMultipleImpl(sql, sqlBuilder, this.Connection, this.Transaction, CommandType.Text, CommandBehavior.SequentialAccess, false);
            return mapping(reader);
        }
        public Dictionary<TKey, TValue> QueryDictionary<TKey, TValue>(string sql, object objParameter = null, CommandType cmdType = CommandType.Text)
        {
            Dictionary<TKey, TValue> result = null;
            var paramType = objParameter != null ? objParameter.GetType() : null;
            int hashKey = RepositoryHelper.GetHashKey(this.ConnString, sql, paramType);
            if (this.Connection == null)
            {
                using (var conn = this.Provider.CreateConnection(this.ConnString))
                {
                    result = this.QueryDictionaryImpl<TKey, TValue>(hashKey, sql, conn, null, cmdType, CommandBehavior.SequentialAccess, objParameter, paramType);
                    conn.Close();
                }
            }
            else result = this.QueryDictionaryImpl<TKey, TValue>(hashKey, sql, this.Connection, this.Transaction, cmdType, CommandBehavior.SequentialAccess, objParameter, paramType);
            return result;
        }
        public Dictionary<TKey, TValue> QueryDictionary<TKey, TValue>(Action<SqlBuilder> builder)
        {
            Dictionary<TKey, TValue> result = null;
            var sqlBuilder = new SqlBuilder(this.Provider);
            builder.Invoke(sqlBuilder);
            var sql = sqlBuilder.BuildSql();
            if (this.Connection == null)
            {
                using (var conn = this.Provider.CreateConnection(this.ConnString))
                {
                    result = this.QueryDictionaryImpl<TKey, TValue>(sqlBuilder, this.Provider.CreateConnection(this.ConnString), null, CommandType.Text, CommandBehavior.SequentialAccess, true);
                    conn.Close();
                }
            }
            else result = this.QueryDictionaryImpl<TKey, TValue>(sqlBuilder, this.Connection, this.Transaction, CommandType.Text, CommandBehavior.SequentialAccess, false);
            return result;
        }
        public int ExecSql(string sql, object objParameter = null, CommandType cmdType = CommandType.Text)
        {
            int result = 0;
            var paramType = objParameter != null ? objParameter.GetType() : null;
            int hashKey = RepositoryHelper.GetHashKey(this.ConnString, sql, paramType);
            if (this.Connection == null)
            {
                using (var conn = this.Provider.CreateConnection(this.ConnString))
                {
                    result = this.ExecSqlImpl(hashKey, sql, conn, null, cmdType, objParameter, paramType);
                    conn.Close();
                }
            }
            else result = this.ExecSqlImpl(hashKey, sql, this.Connection, this.Transaction, cmdType, objParameter, paramType);
            return result;
        }
        public int ExecSql(Action<SqlBuilder> builder)
        {
            int result = 0;
            var sqlBuilder = new SqlBuilder(this.Provider);
            builder.Invoke(sqlBuilder);
            if (this.Connection == null)
            {
                using (var conn = this.Provider.CreateConnection(this.ConnString))
                {
                    result = this.ExecSqlImpl(sqlBuilder, conn, null, CommandType.Text);
                    conn.Close();
                }
            }
            else result = this.ExecSqlImpl(sqlBuilder, this.Connection, this.Transaction, CommandType.Text);
            return result;
        }
        #endregion

        #region 异步方法
        public async Task<TEntity> QueryFirstAsync<TEntity>(string sql, object objParameter = null, CommandType cmdType = CommandType.Text)
        {
            TEntity result = default(TEntity);
            var paramType = objParameter != null ? objParameter.GetType() : null;
            int hashKey = RepositoryHelper.GetHashKey(this.ConnString, sql, paramType);
            var entityType = typeof(TEntity);
            var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
            if (this.Connection == null)
            {
                using (var conn = this.Provider.CreateConnection(this.ConnString))
                {
                    behavior = CommandBehavior.CloseConnection | behavior;
                    await this.QueryImplAsync<TEntity>(hashKey, entityType, sql, conn, null, cmdType, behavior, objResult =>
                    {
                        if (objResult == null || objResult is TEntity) result = (TEntity)objResult;
                        else result = (TEntity)Convert.ChangeType(objResult, entityType, CultureInfo.InvariantCulture);
                    }, objParameter, paramType);
                    conn.Close();
                }
            }
            else await this.QueryImplAsync<TEntity>(hashKey, entityType, sql, this.Connection, this.Transaction, cmdType, behavior, objResult =>
            {
                if (objResult == null || objResult is TEntity) result = (TEntity)objResult;
                else result = (TEntity)Convert.ChangeType(objResult, entityType, CultureInfo.InvariantCulture);
            }, objParameter, paramType);
            return result;
        }
        public async Task<TEntity> QueryFirstAsync<TEntity>(Action<SqlBuilder> builder)
        {
            TEntity result = default(TEntity);
            var entityType = typeof(TEntity);
            var sqlBuilder = new SqlBuilder(this.Provider);
            builder.Invoke(sqlBuilder);
            var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
            if (this.Connection == null)
            {
                using (var conn = this.Provider.CreateConnection(this.ConnString))
                {
                    behavior = CommandBehavior.CloseConnection | behavior;
                    await this.QueryImplAsync(entityType, sqlBuilder, conn, null, CommandType.Text, behavior, objResult =>
                    {
                        if (objResult == null || objResult is TEntity) result = (TEntity)objResult;
                        else result = (TEntity)Convert.ChangeType(objResult, entityType, CultureInfo.InvariantCulture);
                    });
                    conn.Close();
                }
            }
            else await this.QueryImplAsync(entityType, sqlBuilder, this.Connection, this.Transaction, CommandType.Text, behavior, objResult =>
            {
                if (objResult == null || objResult is TEntity) result = (TEntity)objResult;
                else result = (TEntity)Convert.ChangeType(objResult, entityType, CultureInfo.InvariantCulture);
            });
            return result;
        }
        public async Task<List<TEntity>> QueryAsync<TEntity>(string sql, object objParameter = null, CommandType cmdType = CommandType.Text)
        {
            List<TEntity> result = new List<TEntity>();
            var paramType = objParameter != null ? objParameter.GetType() : null;
            int hashKey = RepositoryHelper.GetHashKey(this.ConnString, sql, paramType);
            var entityType = typeof(TEntity);
            var behavior = CommandBehavior.SequentialAccess;
            if (this.Connection == null)
            {
                using (var conn = this.Provider.CreateConnection(this.ConnString))
                {
                    behavior = CommandBehavior.CloseConnection | behavior;
                    await this.QueryImplAsync<TEntity>(hashKey, entityType, sql, conn, null, cmdType, behavior, objResult =>
                    {
                        if (objResult == null) return;
                        if (objResult is TEntity) result.Add((TEntity)objResult);
                        else result.Add((TEntity)Convert.ChangeType(objResult, entityType, CultureInfo.InvariantCulture));
                    }, objParameter, paramType);
                    conn.Close();
                }
            }
            else await this.QueryImplAsync<TEntity>(hashKey, entityType, sql, this.Connection, this.Transaction, cmdType, behavior, objResult =>
            {
                if (objResult == null) return;
                if (objResult is TEntity) result.Add((TEntity)objResult);
                else result.Add((TEntity)Convert.ChangeType(objResult, entityType, CultureInfo.InvariantCulture));
            }, objParameter, paramType);
            return result;
        }
        public async Task<List<TEntity>> QueryAsync<TEntity>(Action<SqlBuilder> builder)
        {
            List<TEntity> result = new List<TEntity>();
            var entityType = typeof(TEntity);
            var sqlBuilder = new SqlBuilder(this.Provider);
            builder.Invoke(sqlBuilder);
            var behavior = CommandBehavior.SequentialAccess;
            if (this.Connection == null)
            {
                using (var conn = this.Provider.CreateConnection(this.ConnString))
                {
                    behavior = CommandBehavior.CloseConnection | behavior;
                    await this.QueryImplAsync(entityType, sqlBuilder, conn, null, CommandType.Text, behavior, objResult =>
                    {
                        if (objResult == null) return;
                        if (objResult is TEntity) result.Add((TEntity)objResult);
                        else result.Add((TEntity)Convert.ChangeType(objResult, entityType, CultureInfo.InvariantCulture));
                    });
                    conn.Close();
                }
            }
            else await this.QueryImplAsync(entityType, sqlBuilder, this.Connection, this.Transaction, CommandType.Text, behavior, objResult =>
            {
                if (objResult == null) return;
                if (objResult is TEntity) result.Add((TEntity)objResult);
                else result.Add((TEntity)Convert.ChangeType(objResult, entityType, CultureInfo.InvariantCulture));
            });
            return result;
        }
        public async Task<PagedList<TEntity>> QueryPageAsync<TEntity>(string sql, int pageIndex, int pageSize, string orderBy = null, object objParameter = null, CommandType cmdType = CommandType.Text)
        {
            var pagingSql = this.Provider.GetPagingExpression(sql, pageIndex * pageSize, pageSize, orderBy);
            string countSql = this.Provider.GetPagingCountExpression(sql);
            var reader = await this.QueryMultipleAsync(countSql + ";" + pagingSql, objParameter, cmdType);
            var count = reader.Read<int>();
            return reader.ReadPageList<TEntity>(pageIndex, pageSize, count);
        }
        public async Task<PagedList<TEntity>> QueryPageAsync<TEntity>(Action<SqlBuilder> builder, int pageIndex, int pageSize, string orderBy = null)
        {
            var sqlBuilder = new SqlBuilder(this.Provider);
            builder.Invoke(sqlBuilder);
            var sql = sqlBuilder.BuildSql();
            var pagingSql = this.Provider.GetPagingExpression(sql, pageIndex * pageSize, pageSize, orderBy);
            string countSql = this.Provider.GetPagingCountExpression(sql);
            sql = countSql + ";" + pagingSql;
            QueryReader reader = null;
            if (this.Connection == null)
            {
                reader = await this.QueryMultipleImplAsync(sql, sqlBuilder, this.Provider.CreateConnection(this.ConnString), null, CommandType.Text, CommandBehavior.SequentialAccess, true);
            }
            else reader = await this.QueryMultipleImplAsync(sql, sqlBuilder, this.Connection, this.Transaction, CommandType.Text, CommandBehavior.SequentialAccess, false);
            var count = reader.Read<int>();
            return reader.ReadPageList<TEntity>(pageIndex, pageSize, count);
        }
        public async Task<QueryReader> QueryMultipleAsync(string sql, object objParameter = null, CommandType cmdType = CommandType.Text)
        {
            var paramType = objParameter != null ? objParameter.GetType() : null;
            int hashKey = RepositoryHelper.GetHashKey(this.ConnString, sql, paramType);
            if (this.Connection == null)
            {
                return await this.QueryMultipleImplAsync(hashKey, sql, this.Provider.CreateConnection(this.ConnString), null, cmdType, CommandBehavior.SequentialAccess, objParameter, paramType, true);
            }
            else return await this.QueryMultipleImplAsync(hashKey, sql, this.Connection, this.Transaction, cmdType, CommandBehavior.SequentialAccess, objParameter, paramType, false);
        }
        public async Task<QueryReader> QueryMultipleAsync(Action<SqlBuilder> builder)
        {
            var sqlBuilder = new SqlBuilder(this.Provider);
            builder.Invoke(sqlBuilder);
            var sql = sqlBuilder.BuildSql();
            if (this.Connection == null)
            {
                return await this.QueryMultipleImplAsync(sql, sqlBuilder, this.Provider.CreateConnection(this.ConnString), null, CommandType.Text, CommandBehavior.SequentialAccess, true);
            }
            else return await this.QueryMultipleImplAsync(sql, sqlBuilder, this.Connection, this.Transaction, CommandType.Text, CommandBehavior.SequentialAccess, false);
        }
        public async Task<TResult> QueryMapAsync<TResult>(Func<QueryReader, TResult> mapping, string sql, object objParameter = null, CommandType cmdType = CommandType.Text)
        {
            var paramType = objParameter != null ? objParameter.GetType() : null;
            int hashKey = RepositoryHelper.GetHashKey(this.ConnString, sql, paramType);
            QueryReader reader = null;
            if (this.Connection == null)
            {
                reader = await this.QueryMultipleImplAsync(hashKey, sql, this.Provider.CreateConnection(this.ConnString), null, cmdType, CommandBehavior.SequentialAccess, objParameter, paramType, true);
            }
            else reader = await this.QueryMultipleImplAsync(hashKey, sql, this.Connection, this.Transaction, cmdType, CommandBehavior.SequentialAccess, objParameter, paramType, false);
            return mapping(reader);
        }
        public async Task<TResult> QueryMapAsync<TResult>(Action<SqlBuilder> builder, Func<QueryReader, TResult> mapping)
        {
            var sqlBuilder = new SqlBuilder(this.Provider);
            builder.Invoke(sqlBuilder);
            var sql = sqlBuilder.BuildSql();
            QueryReader reader = null;
            if (this.Connection == null)
            {
                reader = await this.QueryMultipleImplAsync(sql, sqlBuilder, this.Provider.CreateConnection(this.ConnString), null, CommandType.Text, CommandBehavior.SequentialAccess, true);
            }
            else reader = await this.QueryMultipleImplAsync(sql, sqlBuilder, this.Connection, this.Transaction, CommandType.Text, CommandBehavior.SequentialAccess, false);
            return mapping(reader);
        }
        public async Task<Dictionary<TKey, TValue>> QueryDictionaryAsync<TKey, TValue>(string sql, object objParameter = null, CommandType cmdType = CommandType.Text)
        {
            Dictionary<TKey, TValue> result = null;
            var paramType = objParameter != null ? objParameter.GetType() : null;
            int hashKey = RepositoryHelper.GetHashKey(this.ConnString, sql, paramType);
            if (this.Connection == null)
            {
                using (var conn = this.Provider.CreateConnection(this.ConnString))
                {
                    result = await this.QueryDictionaryImplAsync<TKey, TValue>(hashKey, sql, this.Provider.CreateConnection(this.ConnString), null, cmdType, CommandBehavior.SequentialAccess, objParameter, paramType);
                    conn.Close();
                }
            }
            else result = await this.QueryDictionaryImplAsync<TKey, TValue>(hashKey, sql, this.Connection, this.Transaction, cmdType, CommandBehavior.SequentialAccess, objParameter, paramType);
            return result;
        }
        public async Task<Dictionary<TKey, TValue>> QueryDictionaryAsync<TKey, TValue>(Action<SqlBuilder> builder)
        {
            Dictionary<TKey, TValue> result = null;
            var sqlBuilder = new SqlBuilder(this.Provider);
            builder.Invoke(sqlBuilder);
            var sql = sqlBuilder.BuildSql();
            if (this.Connection == null)
            {
                using (var conn = this.Provider.CreateConnection(this.ConnString))
                {
                    result = await this.QueryDictionaryImplAsync<TKey, TValue>(sqlBuilder, this.Provider.CreateConnection(this.ConnString), null, CommandType.Text, CommandBehavior.SequentialAccess);
                    conn.Close();
                }
            }
            else result = await this.QueryDictionaryImplAsync<TKey, TValue>(sqlBuilder, this.Connection, this.Transaction, CommandType.Text, CommandBehavior.SequentialAccess);
            return result;
        }
        public async Task<int> ExecSqlAsync(string sql, object objParameter = null, CommandType cmdType = CommandType.Text)
        {
            int result = 0;
            var paramType = objParameter != null ? objParameter.GetType() : null;
            int hashKey = RepositoryHelper.GetHashKey(this.ConnString, sql, paramType);
            if (this.Connection == null)
            {
                using (var conn = this.Provider.CreateConnection(this.ConnString))
                {
                    result = await this.ExecSqlImplAsync(hashKey, sql, conn, null, cmdType, objParameter, paramType);
                    conn.Close();
                }
            }
            else result = await this.ExecSqlImplAsync(hashKey, sql, this.Connection, this.Transaction, cmdType, objParameter, paramType);
            return result;
        }
        public async Task<int> ExecSqlAsync(Action<SqlBuilder> builder)
        {
            int result = 0;
            var sqlBuilder = new SqlBuilder(this.Provider);
            builder.Invoke(sqlBuilder);
            var sql = sqlBuilder.BuildSql();
            if (this.Connection == null)
            {
                using (var conn = this.Provider.CreateConnection(this.ConnString))
                {
                    result = await this.ExecSqlImplAsync(sqlBuilder, conn, null, CommandType.Text);
                    conn.Close();
                }
            }
            else result = await this.ExecSqlImplAsync(sqlBuilder, this.Connection, this.Transaction, CommandType.Text);
            return result;
        }
        #endregion

        #region 实现IDisposable
        public void Dispose()
        {
            //如果DbContext不为空，则由DbContext去Dispose
            if (this.DbContext == null)
            {
                if (this.Connection != null)
                {
                    this.Connection.Close();
                    this.Connection.Dispose();
                }
                GC.SuppressFinalize(this);
            }
        }
        #endregion

        #region 私有方法
        private void Open(DbConnection conn)
        {
            if (conn.State == ConnectionState.Broken) conn.Close();
            if (conn.State == ConnectionState.Closed) conn.Open();
        }
        private async Task OpenAsync(DbConnection conn)
        {
            if (conn.State == ConnectionState.Broken) conn.Close();
            if (conn.State == ConnectionState.Closed) await conn.OpenAsync().ConfigureAwait(false);
        }
        private void QueryImpl<TTarget>(int hashKey, Type targetType, string sql, DbConnection conn, DbTransaction trans, CommandType cmdType, CommandBehavior behavior, Action<object> resultHandler, object objParameter, Type paramType)
        {
            DbCommand command = conn.CreateCommand();
            command.CommandText = sql;
            command.CommandType = cmdType;
            command.Transaction = trans;
            if (objParameter != null)
            {
                var paramAction = RepositoryHelper.GetActionCache(hashKey, sql, this.Provider, paramType);
                paramAction(command, objParameter);
            }
            this.Open(conn);
            DbDataReader reader = command.ExecuteReader(behavior);
            hashKey = RepositoryHelper.GetReaderKey(targetType, hashKey);
            var func = RepositoryHelper.GetReader(hashKey, targetType, reader, this.Provider.IsMappingIgnoreCase);
            while (reader.Read())
            {
                resultHandler(func?.Invoke(reader));
            }
            while (reader.NextResult()) { }
            reader.Close();
            reader.Dispose();
            reader = null;
        }
        private void QueryImpl(Type targetType, SqlBuilder sqlBuilder, DbConnection conn, DbTransaction trans, CommandType cmdType, CommandBehavior behavior, Action<object> resultHandler)
        {
            var sql = sqlBuilder.BuildSql();
            DbCommand command = conn.CreateCommand();
            command.CommandText = sql;
            command.CommandType = cmdType;
            command.Transaction = trans;
            this.InitBuilderParameter(sql, command, sqlBuilder);
            this.Open(conn);
            DbDataReader reader = command.ExecuteReader(behavior);
            int hashKey = RepositoryHelper.GetHashKey(this.ConnString, sql, targetType);
            var func = RepositoryHelper.GetReader(hashKey, targetType, reader, this.Provider.IsMappingIgnoreCase);
            while (reader.Read())
            {
                resultHandler(func?.Invoke(reader));
            }
            while (reader.NextResult()) { }
            reader.Close();
            reader.Dispose();
            reader = null;
        }
        private QueryReader QueryMultipleImpl(int hashKey, string sql, DbConnection conn, DbTransaction trans, CommandType cmdType, CommandBehavior behavior, object objParameter, Type paramType, bool isCloseConnection)
        {
            DbCommand command = conn.CreateCommand();
            command.CommandText = sql;
            command.CommandType = cmdType;
            command.Transaction = trans;
            if (objParameter != null)
            {
                var paramAction = RepositoryHelper.GetActionCache(hashKey, sql, this.Provider, paramType);
                paramAction(command, objParameter);
            }
            this.Open(conn);
            DbDataReader reader = command.ExecuteReader(behavior);
            return new QueryReader(hashKey, command, reader, this.Provider.IsMappingIgnoreCase, isCloseConnection);
        }
        private QueryReader QueryMultipleImpl(string sql, SqlBuilder sqlBuilder, DbConnection conn, DbTransaction trans, CommandType cmdType, CommandBehavior behavior, bool isCloseConnection)
        {
            DbCommand command = conn.CreateCommand();
            command.CommandText = sql;
            command.CommandType = cmdType;
            command.Transaction = trans;
            this.InitBuilderParameter(sql, command, sqlBuilder);
            this.Open(conn);
            DbDataReader reader = command.ExecuteReader(behavior);
            int hashKey = RepositoryHelper.GetHashKey(this.ConnString, sql, cmdType);
            return new QueryReader(hashKey, command, reader, this.Provider.IsMappingIgnoreCase, isCloseConnection);
        }
        private Dictionary<TKey, TValue> QueryDictionaryImpl<TKey, TValue>(int hashKey, string sql, DbConnection conn, DbTransaction trans, CommandType cmdType, CommandBehavior behavior, object objParameter, Type paramType)
        {
            DbCommand command = conn.CreateCommand();
            command.CommandText = sql;
            command.CommandType = cmdType;
            command.Transaction = trans;
            if (objParameter != null)
            {
                var paramAction = RepositoryHelper.GetActionCache(hashKey, sql, this.Provider, paramType);
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
            reader.Close();
            reader.Dispose();
            reader = null;
            return result;
        }
        private Dictionary<TKey, TValue> QueryDictionaryImpl<TKey, TValue>(SqlBuilder sqlBuilder, DbConnection conn, DbTransaction trans, CommandType cmdType, CommandBehavior behavior, bool isCloseConnection)
        {
            var sql = sqlBuilder.BuildSql();
            DbCommand command = conn.CreateCommand();
            command.CommandText = sql;
            command.CommandType = cmdType;
            command.Transaction = trans;
            this.InitBuilderParameter(sql, command, sqlBuilder);
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
            reader.Close();
            reader.Dispose();
            reader = null;
            return result;
        }
        private int ExecSqlImpl(int hashKey, string sql, DbConnection conn, DbTransaction trans, CommandType cmdType, object objParameter, Type paramType)
        {
            DbCommand command = conn.CreateCommand();
            command.CommandText = sql;
            command.CommandType = cmdType;
            command.Transaction = trans;
            if (objParameter != null)
            {
                var paramAction = RepositoryHelper.GetActionCache(hashKey, sql, this.Provider, paramType);
                paramAction(command, objParameter);
            }
            this.Open(conn);
            return command.ExecuteNonQuery();
        }
        private int ExecSqlImpl(SqlBuilder sqlBuilder, DbConnection conn, DbTransaction trans, CommandType cmdType)
        {
            var sql = sqlBuilder.BuildSql();
            DbCommand command = conn.CreateCommand();
            command.CommandText = sql;
            command.CommandType = cmdType;
            command.Transaction = trans;
            this.InitBuilderParameter(sql, command, sqlBuilder);
            this.Open(conn);
            return command.ExecuteNonQuery();
        }
        private async Task QueryImplAsync<TTarget>(int hashKey, Type targetType, string sql, DbConnection conn, DbTransaction trans, CommandType cmdType, CommandBehavior behavior, Action<object> resultHandler, object objParameter, Type paramType)
        {
            DbCommand command = conn.CreateCommand();
            command.CommandText = sql;
            command.CommandType = cmdType;
            command.Transaction = trans;
            if (objParameter != null)
            {
                var paramAction = RepositoryHelper.GetActionCache(hashKey, sql, this.Provider, paramType);
                paramAction(command, objParameter);
            }
            await this.OpenAsync(conn);
            DbDataReader reader = await command.ExecuteReaderAsync(behavior).ConfigureAwait(false);
            hashKey = RepositoryHelper.GetReaderKey(targetType, hashKey);
            var func = RepositoryHelper.GetReader(hashKey, targetType, reader, this.Provider.IsMappingIgnoreCase);
            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                resultHandler(func?.Invoke(reader));
            }
            while (await reader.NextResultAsync().ConfigureAwait(false)) { }
            reader.Close();
            reader.Dispose();
            reader = null;
        }
        private async Task QueryImplAsync(Type targetType, SqlBuilder sqlBuilder, DbConnection conn, DbTransaction trans, CommandType cmdType, CommandBehavior behavior, Action<object> resultHandler)
        {
            var sql = sqlBuilder.BuildSql();
            DbCommand command = conn.CreateCommand();
            command.CommandText = sql;
            command.CommandType = cmdType;
            command.Transaction = trans;
            this.InitBuilderParameter(sql, command, sqlBuilder);
            await this.OpenAsync(conn);
            DbDataReader reader = await command.ExecuteReaderAsync(behavior).ConfigureAwait(false);
            int hashKey = RepositoryHelper.GetHashKey(this.ConnString, sql, targetType);
            var func = RepositoryHelper.GetReader(hashKey, targetType, reader, this.Provider.IsMappingIgnoreCase);
            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                resultHandler(func?.Invoke(reader));
            }
            while (await reader.NextResultAsync().ConfigureAwait(false)) { }
            reader.Close();
            reader.Dispose();
            reader = null;
        }
        private async Task<QueryReader> QueryMultipleImplAsync(int hashKey, string sql, DbConnection conn, DbTransaction trans, CommandType cmdType, CommandBehavior behavior, object objParameter, Type paramType, bool isCloseConnection)
        {
            DbCommand command = conn.CreateCommand();
            command.CommandText = sql;
            command.CommandType = cmdType;
            command.Transaction = trans;
            if (objParameter != null)
            {
                var paramAction = RepositoryHelper.GetActionCache(hashKey, sql, this.Provider, paramType);
                paramAction(command, objParameter);
            }
            await this.OpenAsync(conn);
            DbDataReader reader = await command.ExecuteReaderAsync(behavior).ConfigureAwait(false);
            return new QueryReader(hashKey, command, reader, this.Provider.IsMappingIgnoreCase, isCloseConnection);
        }
        private async Task<QueryReader> QueryMultipleImplAsync(string sql, SqlBuilder sqlBuilder, DbConnection conn, DbTransaction trans, CommandType cmdType, CommandBehavior behavior, bool isCloseConnection)
        {
            DbCommand command = conn.CreateCommand();
            command.CommandText = sql;
            command.CommandType = cmdType;
            command.Transaction = trans;
            this.InitBuilderParameter(sql, command, sqlBuilder);
            await this.OpenAsync(conn);
            DbDataReader reader = await command.ExecuteReaderAsync(behavior).ConfigureAwait(false);
            int hashKey = RepositoryHelper.GetHashKey(this.ConnString, sql, cmdType);
            return new QueryReader(hashKey, command, reader, this.Provider.IsMappingIgnoreCase, isCloseConnection);
        }
        private async Task<Dictionary<TKey, TValue>> QueryDictionaryImplAsync<TKey, TValue>(int hashKey, string sql, DbConnection conn, DbTransaction trans, CommandType cmdType, CommandBehavior behavior, object objParameter, Type paramType)
        {
            DbCommand command = conn.CreateCommand();
            command.CommandText = sql;
            command.CommandType = cmdType;
            command.Transaction = trans;
            if (objParameter != null)
            {
                var paramAction = RepositoryHelper.GetActionCache(hashKey, sql, this.Provider, paramType);
                paramAction(command, objParameter);
            }
            this.Open(conn);
            DbDataReader reader = await command.ExecuteReaderAsync(behavior);
            int keyIndex = reader.GetOrdinal(nameof(KeyValuePair<TKey, TValue>.Key));
            int valueIndex = reader.GetOrdinal(nameof(KeyValuePair<TKey, TValue>.Value));
            Dictionary<TKey, TValue> result = new Dictionary<TKey, TValue>();
            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                result.Add(reader.GetFieldValue<TKey>(keyIndex), reader.GetFieldValue<TValue>(valueIndex));
            }
            while (await reader.NextResultAsync().ConfigureAwait(false)) { }
            reader.Close();
            reader.Dispose();
            reader = null;
            return result;
        }
        private async Task<Dictionary<TKey, TValue>> QueryDictionaryImplAsync<TKey, TValue>(SqlBuilder sqlBuilder, DbConnection conn, DbTransaction trans, CommandType cmdType, CommandBehavior behavior)
        {
            var sql = sqlBuilder.BuildSql();
            DbCommand command = conn.CreateCommand();
            command.CommandText = sql;
            command.CommandType = cmdType;
            command.Transaction = trans;
            this.InitBuilderParameter(sql, command, sqlBuilder);
            await this.OpenAsync(conn);
            DbDataReader reader = await command.ExecuteReaderAsync(behavior).ConfigureAwait(false);
            int keyIndex = reader.GetOrdinal(nameof(KeyValuePair<TKey, TValue>.Key));
            int valueIndex = reader.GetOrdinal(nameof(KeyValuePair<TKey, TValue>.Value));
            Dictionary<TKey, TValue> result = new Dictionary<TKey, TValue>();
            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                result.Add(reader.GetFieldValue<TKey>(keyIndex), reader.GetFieldValue<TValue>(valueIndex));
            }
            while (await reader.NextResultAsync().ConfigureAwait(false)) { }
            reader.Close();
            reader.Dispose();
            reader = null;
            return result;
        }
        private async Task<int> ExecSqlImplAsync(int hashKey, string sql, DbConnection conn, DbTransaction trans, CommandType cmdType, object objParameter, Type paramType)
        {
            DbCommand command = conn.CreateCommand();
            command.CommandText = sql;
            command.CommandType = cmdType;
            command.Transaction = trans;
            if (objParameter != null)
            {
                var paramAction = RepositoryHelper.GetActionCache(hashKey, sql, this.Provider, paramType);
                paramAction(command, objParameter);
            }
            await this.OpenAsync(conn);
            return await command.ExecuteNonQueryAsync().ConfigureAwait(false);
        }
        private async Task<int> ExecSqlImplAsync(SqlBuilder sqlBuilder, DbConnection conn, DbTransaction trans, CommandType cmdType)
        {
            var sql = sqlBuilder.BuildSql();
            DbCommand command = conn.CreateCommand();
            command.CommandText = sql;
            command.CommandType = cmdType;
            command.Transaction = trans;
            this.InitBuilderParameter(sql, command, sqlBuilder);
            await this.OpenAsync(conn);
            return await command.ExecuteNonQueryAsync().ConfigureAwait(false);
        }
        private void InitBuilderParameter(string sql, DbCommand command, SqlBuilder sqlBuilder)
        {
            if (sqlBuilder.Parameters.Count > 0)
            {
                DbParameter parameter = null;
                Type valueType = null;
                foreach (var item in sqlBuilder.Parameters)
                {
                    if (!Regex.IsMatch(sql, @"[^a-z0-9_]+" + item.Key + "([^a-z0-9_]+|$)", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.CultureInvariant))
                    {
                        continue;
                    }
                    parameter = command.CreateParameter();
                    parameter.ParameterName = item.Key;
                    valueType = item.Value.GetType();
                    parameter.DbType = DbTypeMap.LookupDbType(valueType);
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = item.Value;
                    command.Parameters.Add(parameter);
                }
            }
        }
        #endregion
    }
}
