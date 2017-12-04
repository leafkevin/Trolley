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
        private static EntityMapper mapper = new EntityMapper(typeof(TEntity));
        private static ConcurrentDictionary<int, Action<IDbCommand, TEntity>> paramActionCache = new ConcurrentDictionary<int, Action<IDbCommand, TEntity>>();
        private static ConcurrentDictionary<int, string> sqlCache = new ConcurrentDictionary<int, string>();
        #endregion

        #region 属性
        protected static EntityMapper Mapper => mapper;
        protected DbConnection Connection { get; private set; }
        protected IRepositoryContext DbContext { get; private set; }
        protected DbTransaction Transaction => this.DbContext.Transaction;
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
        public TEntity Get(TEntity key)
        {
            int hashKey = RepositoryHelper.GetHashKey(this.ConnString, "GET", CommandType.Text);
            var sql = GetSqlCache(hashKey, this.ConnString, "GET", this.Provider);
            return this.QueryFirstImpl<TEntity>(hashKey, Mapper.EntityType, sql, CommandType.Text, key, true);
        }
        public int Create(TEntity entity)
        {
            int hashKey = RepositoryHelper.GetHashKey(this.ConnString, "CREATE", CommandType.Text);
            var sql = GetSqlCache(hashKey, this.ConnString, "CREATE", this.Provider);
            return this.ExecSqlImpl(sql, entity, CommandType.Text, false);
        }
        public int Delete(TEntity key)
        {
            int hashKey = RepositoryHelper.GetHashKey(this.ConnString, "DELETE", CommandType.Text);
            var sql = GetSqlCache(hashKey, this.ConnString, "DELETE", this.Provider);
            return this.ExecSqlImpl(sql, key, CommandType.Text, true);
        }
        public int Update(TEntity entity)
        {
            int hashKey = RepositoryHelper.GetHashKey(this.ConnString, "UPDATE", CommandType.Text);
            var sql = GetSqlCache(hashKey, this.ConnString, "UPDATE", this.Provider);
            return this.ExecSqlImpl(sql, entity, CommandType.Text, false);
        }
        public int Update(string sql, TEntity entity = null) => this.ExecSqlImpl(sql, entity, CommandType.Text, false);
        public int Update<TFields>(Expression<Func<TEntity, TFields>> fieldsExpression, TEntity objParameter = null)
        {
            var sql = GetUpdateFieldsSql(fieldsExpression, this.Provider);
            int hashKey = RepositoryHelper.GetHashKey(this.ConnString, sql, CommandType.Text);
            return this.ExecSqlImpl(sql, objParameter, CommandType.Text, false);
        }
        public int Update(Action<SqlBuilder> builder) => this.ExecSql(builder);
        public TEntity QueryFirst(string sql, TEntity objParameter = null, CommandType cmdType = CommandType.Text)
        {
            int hashKey = RepositoryHelper.GetHashKey(this.ConnString, sql, cmdType);
            return this.QueryFirstImpl<TEntity>(hashKey, Mapper.EntityType, sql, cmdType, objParameter, false);
        }
        public TEntity QueryFirst(Action<SqlBuilder> builder) => this.QueryFirstImpl<TEntity>(Mapper.EntityType, builder);
        public TTarget QueryFirst<TTarget>(string sql, TEntity objParameter = null, CommandType cmdType = CommandType.Text)
        {
            int hashKey = RepositoryHelper.GetHashKey(this.ConnString, sql, cmdType);
            return this.QueryFirstImpl<TTarget>(hashKey, typeof(TTarget), sql, cmdType, objParameter, false);
        }
        public TTarget QueryFirst<TTarget>(Action<SqlBuilder> builder) => this.QueryFirstImpl<TTarget>(typeof(TTarget), builder);
        public List<TEntity> Query(string sql, TEntity objParameter = null, CommandType cmdType = CommandType.Text) => this.QueryImpl<TEntity>(sql, Mapper.EntityType, objParameter, cmdType);
        public List<TEntity> Query(Action<SqlBuilder> builder) => this.QueryImpl<TEntity>(Mapper.EntityType, builder);
        public List<TTarget> Query<TTarget>(string sql, TEntity objParameter = null, CommandType cmdType = CommandType.Text) => this.QueryImpl<TTarget>(sql, typeof(TTarget), objParameter, cmdType);
        public List<TTarget> Query<TTarget>(Action<SqlBuilder> builder) => this.QueryImpl<TTarget>(Mapper.EntityType, builder);
        public PagedList<TEntity> QueryPage(string sql, int pageIndex, int pageSize, string orderBy = null, TEntity objParameter = null, CommandType cmdType = CommandType.Text) => this.QueryPageImpl<TEntity>(sql, pageIndex, pageSize, orderBy, objParameter, cmdType);
        public PagedList<TEntity> QueryPage(Action<SqlBuilder> builder, int pageIndex, int pageSize, string orderBy = null) => this.QueryPageImpl<TEntity>(builder, pageIndex, pageSize, orderBy);
        public PagedList<TTarget> QueryPage<TTarget>(string sql, int pageIndex, int pageSize, string orderBy = null, TEntity objParameter = null, CommandType cmdType = CommandType.Text) => this.QueryPageImpl<TTarget>(sql, pageIndex, pageSize, orderBy, objParameter, cmdType);
        public PagedList<TTarget> QueryPage<TTarget>(Action<SqlBuilder> builder, int pageIndex, int pageSize, string orderBy = null) => this.QueryPageImpl<TTarget>(builder, pageIndex, pageSize, orderBy);
        public TEntity QueryMap(Func<QueryReader, TEntity> mapping, string sql, TEntity objParameter = null, CommandType cmdType = CommandType.Text) => this.QueryMapImpl<TEntity>(mapping, sql, objParameter, cmdType);
        public TEntity QueryMap(Action<SqlBuilder> builder, Func<QueryReader, TEntity> mapping) => this.QueryMapImpl<TEntity>(builder, mapping);
        public TTarget QueryMap<TTarget>(Func<QueryReader, TTarget> mapping, string sql, TEntity objParameter = null, CommandType cmdType = CommandType.Text) => this.QueryMapImpl<TTarget>(mapping, sql, objParameter, cmdType);
        public TTarget QueryMap<TTarget>(Action<SqlBuilder> builder, Func<QueryReader, TTarget> mapping) => this.QueryMapImpl<TTarget>(builder, mapping);
        public QueryReader QueryMultiple(string sql, TEntity objParameter = null, CommandType cmdType = CommandType.Text)
        {
            int hashKey = RepositoryHelper.GetHashKey(this.ConnString, sql, cmdType);
            if (this.Connection == null)
            {
                return this.QueryMultipleImpl(hashKey, sql, this.Provider.CreateConnection(this.ConnString), null, cmdType, CommandBehavior.SequentialAccess, objParameter, true);
            }
            else return this.QueryMultipleImpl(hashKey, sql, this.Connection, this.Transaction, cmdType, CommandBehavior.SequentialAccess, objParameter, false);
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
        public int ExecSql(string sql, TEntity objParameter = null, CommandType cmdType = CommandType.Text) => this.ExecSqlImpl(sql, objParameter, cmdType, false);
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
        public async Task<TEntity> GetAsync(TEntity key)
        {
            int hashKey = RepositoryHelper.GetHashKey(this.ConnString, "GET", CommandType.Text);
            var sql = GetSqlCache(hashKey, this.ConnString, "GET", this.Provider);
            return await this.QueryFirstImplAsync<TEntity>(hashKey, Mapper.EntityType, sql, CommandType.Text, key, true);
        }
        public async Task<int> CreateAsync(TEntity entity)
        {
            int hashKey = RepositoryHelper.GetHashKey(this.ConnString, "CREATE", CommandType.Text);
            var sql = GetSqlCache(hashKey, this.ConnString, "CREATE", this.Provider);
            return await this.ExecSqlImplAsync(sql, CommandType.Text, entity, false);
        }
        public async Task<int> DeleteAsync(TEntity key)
        {
            int hashKey = RepositoryHelper.GetHashKey(this.ConnString, "DELETE", CommandType.Text);
            var sql = GetSqlCache(hashKey, this.ConnString, "DELETE", this.Provider);
            return await this.ExecSqlImplAsync(sql, CommandType.Text, key, true);
        }
        public async Task<int> UpdateAsync(TEntity entity)
        {
            int hashKey = RepositoryHelper.GetHashKey(this.ConnString, "UPDATE", CommandType.Text);
            var sql = GetSqlCache(hashKey, this.ConnString, "UPDATE", this.Provider);
            return await this.ExecSqlImplAsync(sql, CommandType.Text, entity, false);
        }
        public async Task<int> UpdateAsync(string sql, TEntity entity = null) => await this.ExecSqlImplAsync(sql, CommandType.Text, entity, false);
        public async Task<int> UpdateAsync<TFields>(Expression<Func<TEntity, TFields>> fieldsExpression, TEntity objParameter = null)
        {
            var sql = GetUpdateFieldsSql(fieldsExpression, this.Provider);
            int hashKey = RepositoryHelper.GetHashKey(this.ConnString, sql, CommandType.Text);
            return await this.ExecSqlImplAsync(sql, CommandType.Text, objParameter, false);
        }
        public async Task<int> UpdateAsync(Action<SqlBuilder> builder) => await this.ExecSqlAsync(builder);
        public async Task<TEntity> QueryFirstAsync(string sql, TEntity objParameter = null, CommandType cmdType = CommandType.Text)
        {
            int hashKey = RepositoryHelper.GetHashKey(this.ConnString, sql, cmdType);
            return await this.QueryFirstImplAsync<TEntity>(hashKey, Mapper.EntityType, sql, cmdType, objParameter, false);
        }
        public async Task<TEntity> QueryFirstAsync(Action<SqlBuilder> builder) => await this.QueryFirstImplAsync<TEntity>(Mapper.EntityType, builder);
        public async Task<TTarget> QueryFirstAsync<TTarget>(string sql, TEntity objParameter = null, CommandType cmdType = CommandType.Text)
        {
            int hashKey = RepositoryHelper.GetHashKey(this.ConnString, sql, cmdType);
            return await this.QueryFirstImplAsync<TTarget>(hashKey, typeof(TTarget), sql, cmdType, objParameter, false);
        }
        public async Task<TTarget> QueryFirstAsync<TTarget>(Action<SqlBuilder> builder) => await this.QueryFirstImplAsync<TTarget>(Mapper.EntityType, builder);
        public async Task<List<TEntity>> QueryAsync(string sql, TEntity objParameter = null, CommandType cmdType = CommandType.Text) => await this.QueryImplAsync<TEntity>(sql, Mapper.EntityType, objParameter, cmdType);
        public async Task<List<TEntity>> QueryAsync(Action<SqlBuilder> builder) => await this.QueryImplAsync<TEntity>(Mapper.EntityType, builder);
        public async Task<List<TTarget>> QueryAsync<TTarget>(string sql, TEntity objParameter = null, CommandType cmdType = CommandType.Text) => await this.QueryImplAsync<TTarget>(sql, Mapper.EntityType, objParameter, cmdType);
        public async Task<List<TTarget>> QueryAsync<TTarget>(Action<SqlBuilder> builder) => await this.QueryImplAsync<TTarget>(typeof(TTarget), builder);
        public async Task<PagedList<TEntity>> QueryPageAsync(string sql, int pageIndex, int pageSize, string orderBy = null, TEntity objParameter = null, CommandType cmdType = CommandType.Text) => await this.QueryPageImplAsync<TEntity>(sql, pageIndex, pageSize, orderBy, objParameter, cmdType);
        public async Task<PagedList<TEntity>> QueryPageAsync(Action<SqlBuilder> builder, int pageIndex, int pageSize, string orderBy = null) => await this.QueryPageImplAsync<TEntity>(builder, pageIndex, pageSize, orderBy);
        public async Task<PagedList<TTarget>> QueryPageAsync<TTarget>(string sql, int pageIndex, int pageSize, string orderBy = null, TEntity objParameter = null, CommandType cmdType = CommandType.Text) => await this.QueryPageImplAsync<TTarget>(sql, pageIndex, pageSize, orderBy, objParameter, cmdType);
        public async Task<PagedList<TTarget>> QueryPageAsync<TTarget>(Action<SqlBuilder> builder, int pageIndex, int pageSize, string orderBy = null) => await this.QueryPageImplAsync<TTarget>(builder, pageIndex, pageSize, orderBy);
        public async Task<TEntity> QueryMapAsync(Func<QueryReader, TEntity> mapping, string sql, TEntity objParameter = null, CommandType cmdType = CommandType.Text) => await this.QueryMapImplAsync<TEntity>(mapping, sql, objParameter, cmdType);
        public async Task<TEntity> QueryMapAsync(Action<SqlBuilder> builder, Func<QueryReader, TEntity> mapping) => await this.QueryMapImplAsync<TEntity>(builder, mapping);
        public async Task<TTarget> QueryMapAsync<TTarget>(Func<QueryReader, TTarget> mapping, string sql, TEntity objParameter = null, CommandType cmdType = CommandType.Text) => await this.QueryMapImplAsync<TTarget>(mapping, sql, objParameter, cmdType);
        public async Task<TTarget> QueryMapAsync<TTarget>(Action<SqlBuilder> builder, Func<QueryReader, TTarget> mapping) => await this.QueryMapImplAsync<TTarget>(builder, mapping);
        public async Task<QueryReader> QueryMultipleAsync(string sql, TEntity objParameter = null, CommandType cmdType = CommandType.Text)
        {
            int hashKey = RepositoryHelper.GetHashKey(this.ConnString, sql, cmdType);
            if (this.Connection == null)
            {
                return await this.QueryMultipleImplAsync(hashKey, sql, this.Provider.CreateConnection(this.ConnString), null, cmdType, CommandBehavior.SequentialAccess, objParameter, true);
            }
            else return await this.QueryMultipleImplAsync(hashKey, sql, this.Connection, this.Transaction, cmdType, CommandBehavior.SequentialAccess, objParameter, false);
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
        public async Task<int> ExecSqlAsync(string sql, TEntity objParameter = null, CommandType cmdType = CommandType.Text) => await this.ExecSqlImplAsync(sql, cmdType, objParameter, false);
        public async Task<int> ExecSqlAsync(Action<SqlBuilder> builder)
        {
            int result = 0;
            var sqlBuilder = new SqlBuilder(this.Provider);
            builder.Invoke(sqlBuilder);
            if (this.Connection == null)
            {
                using (var conn = this.Provider.CreateConnection(this.ConnString))
                {
                    result = await this.ExecSqlImplAsync(sqlBuilder, conn, null, CommandType.Text);
                    conn.Close();
                }
            }
            else result = this.ExecSqlImpl(sqlBuilder, this.Connection, this.Transaction, CommandType.Text);
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
        private static Action<IDbCommand, TEntity> GetActionCache(int hashKey, string sql, IOrmProvider provider, bool isPkParameter)
        {
            Action<IDbCommand, TEntity> result;
            if (!paramActionCache.TryGetValue(hashKey, out result))
            {
                if (isPkParameter) result = RepositoryHelper.CreateParametersHandler<TEntity>(provider.ParamPrefix, Mapper.EntityType, Mapper.PrimaryKeys);
                else
                {
                    var colMappers = Mapper.MemberMappers.Values.Where(p => Regex.IsMatch(sql, @"[?@:]" + p.MemberName + "([^a-z0-9_]+|$)", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.CultureInvariant));
                    result = RepositoryHelper.CreateParametersHandler<TEntity>(provider.ParamPrefix, Mapper.EntityType, colMappers);
                }
                paramActionCache.TryAdd(hashKey, result);
            }
            return result;
        }
        private TTarget QueryFirstImpl<TTarget>(int hashKey, Type targetType, string sql, CommandType cmdType, TEntity objParameter, bool isPkParameter)
        {
            TTarget result = default(TTarget);
            var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
            if (this.Connection == null)
            {
                using (var conn = this.Provider.CreateConnection(this.ConnString))
                {
                    behavior = CommandBehavior.CloseConnection | behavior;
                    this.QueryImpl<TTarget>(hashKey, targetType, sql, conn, null, cmdType, behavior, objResult =>
                    {
                        if (objResult == null || objResult is TTarget) result = (TTarget)objResult;
                        else result = (TTarget)Convert.ChangeType(objResult, targetType, CultureInfo.InvariantCulture);
                    }, objParameter, isPkParameter);
                    conn.Close();
                }
            }
            else this.QueryImpl<TTarget>(hashKey, targetType, sql, this.Connection, this.Transaction, cmdType, behavior, objResult =>
            {
                if (objResult == null || objResult is TTarget) result = (TTarget)objResult;
                else result = (TTarget)Convert.ChangeType(objResult, targetType, CultureInfo.InvariantCulture);
            }, objParameter, isPkParameter);
            return result;
        }
        private TTarget QueryFirstImpl<TTarget>(Type targetType, Action<SqlBuilder> builder)
        {
            TTarget result = default(TTarget);
            var sqlBuilder = new SqlBuilder(this.Provider);
            builder.Invoke(sqlBuilder);

            var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
            if (this.Connection == null)
            {
                using (var conn = this.Provider.CreateConnection(this.ConnString))
                {
                    behavior = CommandBehavior.CloseConnection | behavior;
                    this.QueryImpl<TTarget>(targetType, sqlBuilder, conn, null, CommandType.Text, behavior, objResult =>
                    {
                        if (objResult == null || objResult is TTarget) result = (TTarget)objResult;
                        else result = (TTarget)Convert.ChangeType(objResult, targetType, CultureInfo.InvariantCulture);
                    });
                    conn.Close();
                }
            }
            else this.QueryImpl<TTarget>(targetType, sqlBuilder, this.Connection, this.Transaction, CommandType.Text, behavior, objResult =>
            {
                if (objResult == null || objResult is TTarget) result = (TTarget)objResult;
                else result = (TTarget)Convert.ChangeType(objResult, targetType, CultureInfo.InvariantCulture);
            });
            return result;
        }
        private List<TTarget> QueryImpl<TTarget>(string sql, Type targetType, TEntity objParameter, CommandType cmdType)
        {
            List<TTarget> result = new List<TTarget>();
            int hashKey = RepositoryHelper.GetHashKey(this.ConnString, sql, cmdType);
            var behavior = CommandBehavior.SequentialAccess;
            if (this.Connection == null)
            {
                using (var conn = this.Provider.CreateConnection(this.ConnString))
                {
                    behavior = CommandBehavior.CloseConnection | behavior;
                    this.QueryImpl<TTarget>(hashKey, targetType, sql, conn, null, cmdType, behavior, objResult =>
                    {
                        if (objResult == null) return;
                        if (objResult is TTarget) result.Add((TTarget)objResult);
                        else result.Add((TTarget)Convert.ChangeType(objResult, targetType, CultureInfo.InvariantCulture));
                    }, objParameter, false);
                    conn.Close();
                }
            }
            else this.QueryImpl<TTarget>(hashKey, targetType, sql, this.Connection, this.Transaction, cmdType, behavior, objResult =>
            {
                if (objResult == null) return;
                if (objResult is TTarget) result.Add((TTarget)objResult);
                else result.Add((TTarget)Convert.ChangeType(objResult, targetType, CultureInfo.InvariantCulture));
            }, objParameter, false);
            return result;
        }
        private void QueryImpl<TTarget>(int hashKey, Type targetType, string sql, DbConnection conn, DbTransaction trans, CommandType cmdType, CommandBehavior behavior, Action<object> resultHandler, TEntity objParameter, bool isPkParameter)
        {
            DbCommand command = conn.CreateCommand();
            command.CommandText = sql;
            command.CommandType = cmdType;
            command.Transaction = trans;
            if (objParameter != null)
            {
                var paramAction = GetActionCache(hashKey, sql, this.Provider, isPkParameter);
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
        private List<TTarget> QueryImpl<TTarget>(Type targetType, Action<SqlBuilder> builder)
        {
            List<TTarget> result = new List<TTarget>();
            var sqlBuilder = new SqlBuilder(this.Provider);
            builder.Invoke(sqlBuilder);
            var behavior = CommandBehavior.SequentialAccess;
            if (this.Connection == null)
            {
                using (var conn = this.Provider.CreateConnection(this.ConnString))
                {
                    behavior = CommandBehavior.CloseConnection | behavior;
                    this.QueryImpl<TEntity>(targetType, sqlBuilder, conn, null, CommandType.Text, behavior, objResult =>
                    {
                        if (objResult == null) return;
                        if (objResult is TTarget) result.Add((TTarget)objResult);
                        else result.Add((TTarget)Convert.ChangeType(objResult, targetType, CultureInfo.InvariantCulture));
                    });
                    conn.Close();
                }
            }
            else this.QueryImpl<TTarget>(targetType, sqlBuilder, this.Connection, this.Transaction, CommandType.Text, behavior, objResult =>
            {
                if (objResult == null) return;
                if (objResult is TTarget) result.Add((TTarget)objResult);
                else result.Add((TTarget)Convert.ChangeType(objResult, targetType, CultureInfo.InvariantCulture));
            });
            return result;
        }
        private void QueryImpl<TTarget>(Type targetType, SqlBuilder sqlBuilder, DbConnection conn, DbTransaction trans, CommandType cmdType, CommandBehavior behavior, Action<object> resultHandler)
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
        private PagedList<TTarget> QueryPageImpl<TTarget>(string sql, int pageIndex, int pageSize, string orderBy = null, TEntity objParameter = null, CommandType cmdType = CommandType.Text)
        {
            var pagingSql = this.Provider.GetPagingExpression(sql, pageIndex * pageSize, pageSize, orderBy);
            string countSql = this.Provider.GetPagingCountExpression(sql);
            var reader = this.QueryMultiple(countSql + ";" + pagingSql, objParameter, cmdType);
            var count = reader.Read<int>();
            return reader.ReadPageList<TTarget>(pageIndex, pageSize, count);
        }
        private PagedList<TTarget> QueryPageImpl<TTarget>(Action<SqlBuilder> builder, int pageIndex, int pageSize, string orderBy = null)
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
            return reader.ReadPageList<TTarget>(pageIndex, pageSize, count);
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
        private TTarget QueryMapImpl<TTarget>(Func<QueryReader, TTarget> mapping, string sql, TEntity objParameter = null, CommandType cmdType = CommandType.Text)
        {
            int cacheKey = RepositoryHelper.GetHashKey(this.ConnString, sql, cmdType);
            QueryReader reader = null;
            if (this.Connection == null)
            {
                reader = this.QueryMultipleImpl(cacheKey, sql, this.Provider.CreateConnection(this.ConnString), null, cmdType, CommandBehavior.SequentialAccess, objParameter, true);
            }
            else reader = this.QueryMultipleImpl(cacheKey, sql, this.Connection, this.Transaction, cmdType, CommandBehavior.SequentialAccess, objParameter, false);
            return mapping(reader);
        }
        private TTarget QueryMapImpl<TTarget>(Action<SqlBuilder> builder, Func<QueryReader, TTarget> mapping)
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

        private int ExecSqlImpl(string sql, TEntity objParameter, CommandType cmdType, bool isPkParameter)
        {
            int result = 0;
            int hashKey = RepositoryHelper.GetHashKey(this.ConnString, sql, cmdType);
            if (this.Connection == null)
            {
                using (var conn = this.Provider.CreateConnection(this.ConnString))
                {
                    result = this.ExecSqlImpl(hashKey, sql, conn, null, cmdType, objParameter, isPkParameter);
                    conn.Close();
                }
            }
            else result = this.ExecSqlImpl(hashKey, sql, this.Connection, this.Transaction, cmdType, objParameter, isPkParameter);
            return result;
        }
        private int ExecSqlImpl(int hashKey, string sql, DbConnection conn, DbTransaction trans, CommandType cmdType, TEntity objParameter, bool isPkParameter)
        {
            DbCommand command = conn.CreateCommand();
            command.CommandText = sql;
            command.CommandType = cmdType;
            command.Transaction = trans;
            if (objParameter != null)
            {
                var paramAction = GetActionCache(hashKey, sql, this.Provider, isPkParameter);
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
        private async Task<TTarget> QueryFirstImplAsync<TTarget>(Type targetType, Action<SqlBuilder> builder)
        {
            TTarget result = default(TTarget);
            var sqlBuilder = new SqlBuilder(this.Provider);
            builder.Invoke(sqlBuilder);
            var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
            if (this.Connection == null)
            {
                using (var conn = this.Provider.CreateConnection(this.ConnString))
                {
                    behavior = CommandBehavior.CloseConnection | behavior;
                    await this.QueryImplAsync<TTarget>(targetType, sqlBuilder, conn, null, CommandType.Text, behavior, objResult =>
                    {
                        if (objResult == null || objResult is TTarget) result = (TTarget)objResult;
                        else result = (TTarget)Convert.ChangeType(objResult, targetType, CultureInfo.InvariantCulture);
                    });
                    conn.Close();
                }
            }
            else await this.QueryImplAsync<TTarget>(targetType, sqlBuilder, this.Connection, this.Transaction, CommandType.Text, behavior, objResult =>
            {
                if (objResult == null || objResult is TTarget) result = (TTarget)objResult;
                else result = (TTarget)Convert.ChangeType(objResult, targetType, CultureInfo.InvariantCulture);
            });
            return result;
        }
        private async Task<TTarget> QueryFirstImplAsync<TTarget>(int hashKey, Type targetType, string sql, CommandType cmdType, TEntity objParameter, bool isPkParameter)
        {
            TTarget result = default(TTarget);
            var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
            if (this.Connection == null)
            {
                using (var conn = this.Provider.CreateConnection(this.ConnString))
                {
                    behavior = CommandBehavior.CloseConnection | behavior;
                    await this.QueryImplAsync<TTarget>(hashKey, targetType, sql, conn, null, cmdType, behavior, objResult =>
                    {
                        if (objResult == null || objResult is TTarget) result = (TTarget)objResult;
                        else result = (TTarget)Convert.ChangeType(objResult, targetType, CultureInfo.InvariantCulture);
                    }, objParameter, isPkParameter);
                    conn.Close();
                }
            }
            else await this.QueryImplAsync<TTarget>(hashKey, targetType, sql, this.Connection, this.Transaction, cmdType, behavior, objResult =>
            {
                if (objResult == null || objResult is TTarget) result = (TTarget)objResult;
                else result = (TTarget)Convert.ChangeType(objResult, targetType, CultureInfo.InvariantCulture);
            }, objParameter, isPkParameter);
            return result;
        }
        private async Task<List<TTarget>> QueryImplAsync<TTarget>(string sql, Type targetType, TEntity objParameter, CommandType cmdType)
        {
            List<TTarget> result = new List<TTarget>();
            int hashKey = RepositoryHelper.GetHashKey(this.ConnString, sql, cmdType);
            var behavior = CommandBehavior.SequentialAccess;
            if (this.Connection == null)
            {
                using (var conn = this.Provider.CreateConnection(this.ConnString))
                {
                    behavior = CommandBehavior.CloseConnection | behavior;
                    await this.QueryImplAsync<TTarget>(hashKey, targetType, sql, conn, null, cmdType, behavior, objResult =>
                    {
                        if (objResult == null) return;
                        if (objResult is TTarget) result.Add((TTarget)objResult);
                        else result.Add((TTarget)Convert.ChangeType(objResult, targetType, CultureInfo.InvariantCulture));
                    }, objParameter, false);
                }
            }
            else await this.QueryImplAsync<TTarget>(hashKey, targetType, sql, this.Connection, this.Transaction, cmdType, behavior, objResult =>
            {
                if (objResult == null) return;
                if (objResult is TTarget) result.Add((TTarget)objResult);
                else result.Add((TTarget)Convert.ChangeType(objResult, targetType, CultureInfo.InvariantCulture));
            }, objParameter, false);
            return result;
        }
        private async Task<List<TTarget>> QueryImplAsync<TTarget>(Type targetType, Action<SqlBuilder> builder)
        {
            List<TTarget> result = new List<TTarget>();
            var sqlBuilder = new SqlBuilder(this.Provider);
            builder.Invoke(sqlBuilder);
            var behavior = CommandBehavior.SequentialAccess;
            if (this.Connection == null)
            {
                using (var conn = this.Provider.CreateConnection(this.ConnString))
                {
                    behavior = CommandBehavior.CloseConnection | behavior;
                    await this.QueryImplAsync<TTarget>(targetType, sqlBuilder, conn, null, CommandType.Text, behavior, objResult =>
                    {
                        if (objResult == null) return;
                        if (objResult is TTarget) result.Add((TTarget)objResult);
                        else result.Add((TTarget)Convert.ChangeType(objResult, targetType, CultureInfo.InvariantCulture));
                    });
                    conn.Close();
                }
            }
            else await this.QueryImplAsync<TTarget>(targetType, sqlBuilder, this.Connection, this.Transaction, CommandType.Text, behavior, objResult =>
            {
                if (objResult == null) return;
                if (objResult is TTarget) result.Add((TTarget)objResult);
                else result.Add((TTarget)Convert.ChangeType(objResult, targetType, CultureInfo.InvariantCulture));
            });
            return result;
        }
        private async Task QueryImplAsync<TTarget>(int hashKey, Type targetType, string sql, DbConnection conn, DbTransaction trans, CommandType cmdType, CommandBehavior behavior, Action<object> resultHandler, TEntity objParameter, bool isPkParameter)
        {
            DbCommand command = conn.CreateCommand();
            command.CommandText = sql;
            command.CommandType = cmdType;
            command.Transaction = trans;
            if (objParameter != null)
            {
                var paramAction = GetActionCache(hashKey, sql, this.Provider, isPkParameter);
                paramAction(command, objParameter);
            }
            await this.OpenAsync(conn);
            DbDataReader reader = await command.ExecuteReaderAsync(behavior).ConfigureAwait(false);
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
        private async Task QueryImplAsync<TTarget>(Type targetType, SqlBuilder sqlBuilder, DbConnection conn, DbTransaction trans, CommandType cmdType, CommandBehavior behavior, Action<object> resultHandler)
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
        private async Task<PagedList<TTarget>> QueryPageImplAsync<TTarget>(string sql, int pageIndex, int pageSize, string orderBy = null, TEntity objParameter = null, CommandType cmdType = CommandType.Text)
        {
            var pagingSql = this.Provider.GetPagingExpression(sql, pageIndex * pageSize, pageSize, orderBy);
            string countSql = this.Provider.GetPagingCountExpression(sql);
            var reader = await this.QueryMultipleAsync(countSql + ";" + pagingSql, objParameter, cmdType);
            var count = reader.Read<int>();
            return reader.ReadPageList<TTarget>(pageIndex, pageSize, count);
        }
        private async Task<PagedList<TTarget>> QueryPageImplAsync<TTarget>(Action<SqlBuilder> builder, int pageIndex, int pageSize, string orderBy = null)
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
            return reader.ReadPageList<TTarget>(pageIndex, pageSize, count);
        }
        private async Task<TTarget> QueryMapImplAsync<TTarget>(Func<QueryReader, TTarget> mapping, string sql, TEntity objParameter = null, CommandType cmdType = CommandType.Text)
        {
            int hashKey = RepositoryHelper.GetHashKey(this.ConnString, sql, cmdType);
            QueryReader reader = null;
            if (this.Connection == null)
            {
                reader = await this.QueryMultipleImplAsync(hashKey, sql, this.Provider.CreateConnection(this.ConnString), null, cmdType, CommandBehavior.SequentialAccess, objParameter, true);
            }
            else reader = await this.QueryMultipleImplAsync(hashKey, sql, this.Connection, this.Transaction, cmdType, CommandBehavior.SequentialAccess, objParameter, false);
            return mapping(reader);
        }
        private async Task<TTarget> QueryMapImplAsync<TTarget>(Action<SqlBuilder> builder, Func<QueryReader, TTarget> mapping)
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
        private async Task<int> ExecSqlImplAsync(string sql, CommandType cmdType, TEntity objParameter, bool isPkParameter)
        {
            int result = 0;
            int hashKey = RepositoryHelper.GetHashKey(this.ConnString, sql, cmdType);
            if (this.Connection == null)
            {
                using (var conn = this.Provider.CreateConnection(this.ConnString))
                {
                    result = await this.ExecSqlImplAsync(hashKey, sql, conn, null, cmdType, objParameter, isPkParameter);
                    conn.Close();
                }
            }
            else result = await this.ExecSqlImplAsync(hashKey, sql, this.Connection, this.Transaction, cmdType, objParameter, isPkParameter);
            return result;
        }
        private async Task<int> ExecSqlImplAsync(Action<SqlBuilder> builder)
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
        private async Task<int> ExecSqlImplAsync(int hashKey, string sql, DbConnection conn, DbTransaction trans, CommandType cmdType, TEntity objParameter, bool isPkParameter)
        {
            DbCommand command = conn.CreateCommand();
            command.CommandText = sql;
            command.CommandType = cmdType;
            command.Transaction = trans;
            if (objParameter != null)
            {
                var paramAction = GetActionCache(hashKey, sql, this.Provider, isPkParameter);
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
