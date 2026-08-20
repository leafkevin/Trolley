using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley;

public class Repository : DialectProvider, IRepository
{
    #region Constructor
    public Repository(DbContext dbContext) => this.DbContext = dbContext;
    #endregion

    #region ShardingDatabase
    public IRepository UseMaster(params object[] selectorValues)
    {
        this.DbContext.ConnectionString = this.DbContext.Database.Select(selectorValues);
        return this;
    }
    public IRepository UseSlave(params object[] selectorValues)
    {
        this.DbContext.ConnectionString = this.DbContext.Database.SelectSlave(selectorValues);
        return this;
    }
    #endregion

    #region ShardingTable
    public virtual void CreateShardingTable<TEntity>(string tableName, string fromTableSchema = null) { }
    public virtual Task CreateShardingTableAsync<TEntity>(string tableName, string fromTableSchema = null, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
    public virtual string GetShardingTableName<TEntity>(params object[] fieldValues)
        => this.GetShardingTable(typeof(TEntity), fieldValues);
    public virtual void CreateShardingTable<TEntity>(object[] fieldValues, string fromTableSchema = null)
    {
        var tableName = this.GetShardingTable(typeof(TEntity), fieldValues);
        this.CreateShardingTable<TEntity>(tableName, fromTableSchema);
    }
    public virtual async Task CreateShardingTableAsync<TEntity>(object[] fieldValues, string fromTableSchema = null, CancellationToken cancellationToken = default)
    {
        var tableName = this.GetShardingTable(typeof(TEntity), fieldValues);
        await this.CreateShardingTableAsync<TEntity>(tableName, fromTableSchema, cancellationToken);
    }
    #endregion     

    #region From
    public virtual IQuery<T> From<T>(char tableAsStart = 'a')
    {
        var visitor = this.CreateQueryVisitor(tableAsStart);
        visitor.From(tableAsStart, typeof(T));
        return this.ormProvider.NewQuery<T>(this.DbContext, visitor);
    }
    public virtual IQuery<T1, T2> From<T1, T2>(char tableAsStart = 'a')
    {
        var visitor = this.CreateQueryVisitor(tableAsStart);
        visitor.From(tableAsStart, typeof(T1), typeof(T2));
        return this.ormProvider.NewQuery<T1, T2>(this.DbContext, visitor);
    }
    public virtual IQuery<T1, T2, T3> From<T1, T2, T3>(char tableAsStart = 'a')
    {
        var visitor = this.CreateQueryVisitor(tableAsStart);
        visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3));
        return this.ormProvider.NewQuery<T1, T2, T3>(this.DbContext, visitor);
    }
    public virtual IQuery<T1, T2, T3, T4> From<T1, T2, T3, T4>(char tableAsStart = 'a')
    {
        var visitor = this.CreateQueryVisitor(tableAsStart);
        visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4));
        return this.ormProvider.NewQuery<T1, T2, T3, T4>(this.DbContext, visitor);
    }
    public virtual IQuery<T1, T2, T3, T4, T5> From<T1, T2, T3, T4, T5>(char tableAsStart = 'a')
    {
        var visitor = this.CreateQueryVisitor(tableAsStart);
        visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5));
        return this.ormProvider.NewQuery<T1, T2, T3, T4, T5>(this.DbContext, visitor);
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6> From<T1, T2, T3, T4, T5, T6>(char tableAsStart = 'a')
    {
        var visitor = this.CreateQueryVisitor(tableAsStart);
        visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6));
        return this.ormProvider.NewQuery<T1, T2, T3, T4, T5, T6>(this.DbContext, visitor);
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7> From<T1, T2, T3, T4, T5, T6, T7>(char tableAsStart = 'a')
    {
        var visitor = this.CreateQueryVisitor(tableAsStart);
        visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7));
        return this.ormProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7>(this.DbContext, visitor);
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8> From<T1, T2, T3, T4, T5, T6, T7, T8>(char tableAsStart = 'a')
    {
        var visitor = this.CreateQueryVisitor(tableAsStart);
        visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8));
        return this.ormProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8>(this.DbContext, visitor);
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> From<T1, T2, T3, T4, T5, T6, T7, T8, T9>(char tableAsStart = 'a')
    {
        var visitor = this.CreateQueryVisitor(tableAsStart);
        visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9));
        return this.ormProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this.DbContext, visitor);
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> From<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(char tableAsStart = 'a')
    {
        var visitor = this.CreateQueryVisitor(tableAsStart);
        visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10));
        return this.ormProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this.DbContext, visitor);
    }
    #endregion

    #region FromQuery
    public virtual IQuery<T> FromQuery<T>(IQuery<T> subQuery)
    {
        var visitor = this.CreateQueryVisitor('a', subQuery.Visitor.Command);
        visitor.UseQuery(typeof(T), subQuery, true);
        return this.ormProvider.NewQuery<T>(this.DbContext, visitor);
    }
    public virtual IQuery<T> FromQuery<T>(Expression<Func<IFromQuery, IQuery<T>>> subQueryExpr)
    {
        var visitor = this.CreateQueryVisitor();
        visitor.UseNewQuery(typeof(T), subQueryExpr, true);
        return this.ormProvider.NewQuery<T>(this.DbContext, visitor);
    }
    #endregion

    #region QueryScalar
    public virtual TValue QueryScalar<TValue>(string rawSql, CommandType commandType = CommandType.Text)
    {
        (var isNeedClose, var connection, var command) = this.CreateQueryCommand(rawSql, commandType);
        return this.QueryScalar<TValue>(isNeedClose, connection, command);
    }
    public virtual async Task<TValue> QueryScalarAsync<TValue>(string rawSql, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default)
    {
        (var isNeedClose, var connection, var command) = this.CreateQueryCommand(rawSql, commandType);
        return await this.QueryScalarAsync<TValue>(isNeedClose, connection, command, cancellationToken);
    }
    public virtual TValue QueryScalar<TValue>(string rawSql, object parameters, CommandType commandType = CommandType.Text)
    {
        (var isNeedClose, var connection, var command) = this.CreateQueryCommand(rawSql, parameters, commandType);
        return this.QueryScalar<TValue>(isNeedClose, connection, command);
    }
    public virtual async Task<TValue> QueryScalarAsync<TValue>(string rawSql, object parameters = null, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default)
    {
        (var isNeedClose, var connection, var command) = this.CreateQueryCommand(rawSql, parameters, commandType);
        return await this.QueryScalarAsync<TValue>(isNeedClose, connection, command, cancellationToken);
    }
    public virtual TValue QueryScalar<TValue>(string rawSql, List<IDbDataParameter> parameters, CommandType commandType = CommandType.Text)
    {
        (var isNeedClose, var connection, var command) = this.CreateQueryCommand(rawSql, parameters, commandType);
        return this.QueryScalar<TValue>(isNeedClose, connection, command);
    }
    public virtual async Task<TValue> QueryScalarAsync<TValue>(string rawSql, List<IDbDataParameter> parameters, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default)
    {
        (var isNeedClose, var connection, var command) = this.CreateQueryCommand(rawSql, parameters, commandType);
        return await this.QueryScalarAsync<TValue>(isNeedClose, connection, command, cancellationToken);
    }
    #endregion

    #region QueryById
    public virtual TEntity QueryById<TEntity>(object whereKey)
    {
        (var isNeedClose, var connection, var command) = this.CreateQueryByCommand(typeof(TEntity), whereKey, true, false);
        return this.QuerySingle<TEntity>(isNeedClose, connection, command);
    }
    public virtual async Task<TEntity> QueryByIdAsync<TEntity>(object whereKey, CancellationToken cancellationToken = default)
    {
        (var isNeedClose, var connection, var command) = this.CreateQueryByCommand(typeof(TEntity), whereKey, true, false);
        return await this.QuerySingleAsync<TEntity>(isNeedClose, connection, command, cancellationToken);
    }
    #endregion

    #region QueryFirst
    public virtual TEntity QueryFirst<TEntity>(string rawSql, CommandType commandType = CommandType.Text)
    {
        (var isNeedClose, var connection, var command) = this.CreateQueryCommand(rawSql, commandType);
        return this.QuerySingle<TEntity>(isNeedClose, connection, command);
    }
    public virtual async Task<TEntity> QueryFirstAsync<TEntity>(string rawSql, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default)
    {
        (var isNeedClose, var connection, var command) = this.CreateQueryCommand(rawSql, commandType);
        return await this.QuerySingleAsync<TEntity>(isNeedClose, connection, command, cancellationToken);
    }
    public virtual TEntity QueryFirst<TEntity>(string rawSql, object parameters, CommandType commandType = CommandType.Text)
    {
        (var isNeedClose, var connection, var command) = this.CreateQueryCommand(rawSql, parameters, commandType);
        return this.QuerySingle<TEntity>(isNeedClose, connection, command);
    }
    public virtual async Task<TEntity> QueryFirstAsync<TEntity>(string rawSql, object parameters, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default)
    {
        (var isNeedClose, var connection, var command) = this.CreateQueryCommand(rawSql, parameters, commandType);
        return await this.QuerySingleAsync<TEntity>(isNeedClose, connection, command, cancellationToken);
    }
    public virtual TEntity QueryFirst<TEntity>(string rawSql, List<IDbDataParameter> parameters, CommandType commandType = CommandType.Text)
    {
        (var isNeedClose, var connection, var command) = this.CreateQueryCommand(rawSql, parameters, commandType);
        return this.QuerySingle<TEntity>(isNeedClose, connection, command);
    }
    public virtual async Task<TEntity> QueryFirstAsync<TEntity>(string rawSql, List<IDbDataParameter> parameters, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default)
    {
        (var isNeedClose, var connection, var command) = this.CreateQueryCommand(rawSql, parameters, commandType);
        return await this.QuerySingleAsync<TEntity>(isNeedClose, connection, command, cancellationToken);
    }
    public virtual TEntity QueryFirst<TEntity>(object whereObj = null)
    {
        var isBulk = whereObj is IEnumerable && whereObj is not string && whereObj is not IDictionary<string, object>;
        (var isNeedClose, var connection, var command) = this.CreateQueryByCommand(typeof(TEntity), whereObj, false, isBulk);
        return this.QuerySingle<TEntity>(isNeedClose, connection, command);
    }
    public virtual async Task<TEntity> QueryFirstAsync<TEntity>(object whereObj = null, CancellationToken cancellationToken = default)
    {
        var isBulk = whereObj is IEnumerable && whereObj is not string && whereObj is not IDictionary<string, object>;
        (var isNeedClose, var connection, var command) = this.CreateQueryByCommand(typeof(TEntity), whereObj, false, isBulk);
        return await this.QuerySingleAsync<TEntity>(isNeedClose, connection, command, cancellationToken);
    }
    #endregion

    #region QueryByIds
    public virtual List<TEntity> QueryByIds<TEntity>(IEnumerable whereKeys)
    {
        (var isNeedClose, var connection, var command) = this.CreateQueryByCommand(typeof(TEntity), whereKeys, true, true);
        return this.Query<TEntity>(isNeedClose, connection, command);
    }
    public virtual async Task<List<TEntity>> QueryByIdsAsync<TEntity>(IEnumerable whereKeys, CancellationToken cancellationToken = default)
    {
        (var isNeedClose, var connection, var command) = this.CreateQueryByCommand(typeof(TEntity), whereKeys, true, true);
        return await this.QueryAsync<TEntity>(isNeedClose, connection, command, cancellationToken);
    }
    #endregion

    #region Query
    public virtual List<TEntity> Query<TEntity>(string rawSql, CommandType commandType = CommandType.Text)
    {
        (var isNeedClose, var connection, var command) = this.CreateQueryCommand(rawSql, commandType);
        return this.Query<TEntity>(isNeedClose, connection, command);
    }
    public virtual async Task<List<TEntity>> QueryAsync<TEntity>(string rawSql, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default)
    {
        (var isNeedClose, var connection, var command) = this.CreateQueryCommand(rawSql, commandType);
        return await this.QueryAsync<TEntity>(isNeedClose, connection, command, cancellationToken);
    }
    public virtual List<TEntity> Query<TEntity>(string rawSql, object parameters, CommandType commandType = CommandType.Text)
    {
        (var isNeedClose, var connection, var command) = this.CreateQueryCommand(rawSql, parameters, commandType);
        return this.Query<TEntity>(isNeedClose, connection, command);
    }
    public virtual async Task<List<TEntity>> QueryAsync<TEntity>(string rawSql, object parameters = null, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default)
    {
        (var isNeedClose, var connection, var command) = this.CreateQueryCommand(rawSql, parameters, commandType);
        return await this.QueryAsync<TEntity>(isNeedClose, connection, command, cancellationToken);
    }
    public virtual List<TEntity> Query<TEntity>(string rawSql, List<IDbDataParameter> parameters, CommandType commandType = CommandType.Text)
    {
        (var isNeedClose, var connection, var command) = this.CreateQueryCommand(rawSql, parameters, commandType);
        return this.Query<TEntity>(isNeedClose, connection, command);
    }
    public virtual async Task<List<TEntity>> QueryAsync<TEntity>(string rawSql, List<IDbDataParameter> parameters, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default)
    {
        (var isNeedClose, var connection, var command) = this.CreateQueryCommand(rawSql, parameters, commandType);
        return await this.QueryAsync<TEntity>(isNeedClose, connection, command, cancellationToken);
    }
    public virtual List<TEntity> Query<TEntity>(object whereObj = null)
    {
        var isBulk = whereObj is IEnumerable && whereObj is not string && whereObj is not IDictionary<string, object>;
        (var isNeedClose, var connection, var command) = this.CreateQueryByCommand(typeof(TEntity), whereObj, false, isBulk);
        return this.Query<TEntity>(isNeedClose, connection, command);
    }
    public virtual async Task<List<TEntity>> QueryAsync<TEntity>(object whereObj = null, CancellationToken cancellationToken = default)
    {
        var isBulk = whereObj is IEnumerable && whereObj is not string && whereObj is not IDictionary<string, object>;
        (var isNeedClose, var connection, var command) = this.CreateQueryByCommand(typeof(TEntity), whereObj, false, isBulk);
        return await this.QueryAsync<TEntity>(isNeedClose, connection, command, cancellationToken);
    }
    #endregion

    #region Exists
    public virtual bool ExistsBy<TEntity>(object whereObj)
    {
        (var isNeedClose, var connection, var command) = this.CreateExistsCommand(typeof(TEntity), whereObj, false, false);
        return this.Exists(isNeedClose, connection, command);
    }
    public virtual async Task<bool> ExistsByAsync<TEntity>(object whereObj, CancellationToken cancellationToken = default)
    {
        (var isNeedClose, var connection, var command) = this.CreateExistsCommand(typeof(TEntity), whereObj, false, false);
        return await this.ExistsAsync(isNeedClose, connection, command, cancellationToken);
    }
    public virtual bool ExistsById<TEntity>(object whereKey)
    {
        (var isNeedClose, var connection, var command) = this.CreateExistsCommand(typeof(TEntity), whereKey, true, false);
        return this.Exists(isNeedClose, connection, command);
    }
    public virtual async Task<bool> ExistsByIdAsync<TEntity>(object whereKey, CancellationToken cancellationToken = default)
    {
        (var isNeedClose, var connection, var command) = this.CreateExistsCommand(typeof(TEntity), whereKey, true, false);
        return await this.ExistsAsync(isNeedClose, connection, command, cancellationToken);
    }
    public virtual bool ExistsByIds<TEntity>(IEnumerable whereKeys)
    {
        (var isNeedClose, var connection, var command) = this.CreateExistsCommand(typeof(TEntity), whereKeys, true, true);
        return this.Exists(isNeedClose, connection, command);
    }
    public virtual async Task<bool> ExistsByIdsAsync<TEntity>(IEnumerable whereKeys, CancellationToken cancellationToken = default)
    {
        (var isNeedClose, var connection, var command) = this.CreateExistsCommand(typeof(TEntity), whereKeys, true, true);
        return await this.ExistsAsync(isNeedClose, connection, command, cancellationToken);
    }
    /// <summary>
    /// 判断TEntity表是否存在满足wherePredicate条件的记录，存在返回true，否则返回false，不支持分表
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <param name="wherePredicate">where条件表达式，可以为null</param>
    /// <returns>返回是否存在</returns>
    public bool Exists<TEntity>(Expression<Func<TEntity, bool>> wherePredicate = null)
    {
        var entityType = typeof(TEntity);
        (var isNeedClose, var connection, var command) = this.UseSlaveCommand();
        if (wherePredicate != null)
        {
            using var queryVisitor = this.CreateQueryVisitor('a', command);
            queryVisitor.From('a', entityType);
            queryVisitor.And(wherePredicate);
            queryVisitor.SelectRaw(typeof(int), "1");
            queryVisitor.Take(1);
            command.CommandText = this.BuildScalarSql(queryVisitor);
        }
        else
        {
            var entityMapper = this.entityMapProvider.GetEntityMap(entityType);
            var tableName = this.ormProvider.GetTableName(entityMapper.TableName);
            command.CommandText = $"SELECT 1 FROM {tableName} LIMIT 1";
        }
        if (this.interceptor != null)
            command = this.interceptor.CommandInitialized(command);
        return this.Exists(isNeedClose, connection, command);
    }
    /// <summary>
    /// 判断TEntity表是否存在满足wherePredicate条件的记录，存在返回true，否则返回false，不支持分表
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <param name="wherePredicate">where条件表达式，可以为null</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回是否存在，布尔值</returns>
    public async Task<bool> ExistsAsync<TEntity>(Expression<Func<TEntity, bool>> wherePredicate = null, CancellationToken cancellationToken = default)
    {
        var entityType = typeof(TEntity);
        (var isNeedClose, var connection, var command) = this.UseSlaveCommand();
        if (wherePredicate != null)
        {
            using var queryVisitor = this.CreateQueryVisitor('a', command);
            queryVisitor.From('a', entityType);
            queryVisitor.And(wherePredicate);
            queryVisitor.SelectRaw(typeof(int), "1");
            queryVisitor.Take(1);
            command.CommandText = this.BuildScalarSql(queryVisitor);
        }
        else
        {
            var entityMapper = this.entityMapProvider.GetEntityMap(entityType);
            var tableName = this.ormProvider.GetTableName(entityMapper.TableName);
            command.CommandText = $"SELECT 1 FROM {tableName} LIMIT 1";
        }
        if (this.interceptor != null)
            command = this.interceptor.CommandInitialized(command);
        return await this.ExistsAsync(isNeedClose, connection, command, cancellationToken);
    }
    #endregion

    #region QueryMultiple
    public virtual IMultiQueryReader QueryMultiple(Action<IMultipleQuery> subQueries)
    {
        if (subQueries == null)
            throw new ArgumentNullException(nameof(subQueries));

        (var isNeedClose, var connection, var command) = this.UseSlaveCommand();
        using var multiQuery = this.ormProvider.NewMultipleQuery(this.DbContext, command);
        subQueries.Invoke(multiQuery);
        command.CommandText = multiQuery.BuildSql(out var readerAfters);
        if (this.interceptor != null)
            command = this.interceptor.CommandInitialized(command);

        connection.Open();
        var reader = command.ExecuteReader(CommandBehavior.SequentialAccess);
        //多语句查询，在最后reader读取后，自动关闭
        return new MultiQueryReader(this.DbContext, connection, command, reader, readerAfters, isNeedClose);
    }
    public virtual async Task<IMultiQueryReader> QueryMultipleAsync(Action<IMultipleQuery> subQueries, CancellationToken cancellationToken = default)
    {
        if (subQueries == null)
            throw new ArgumentNullException(nameof(subQueries));

        (var isNeedClose, var connection, var command) = this.UseSlaveCommand();
        using var multiQuery = this.ormProvider.NewMultipleQuery(this.DbContext, command);
        subQueries.Invoke(multiQuery);
        command.CommandText = multiQuery.BuildSql(out var readerAfters);
        if (this.interceptor != null)
            command = this.interceptor.CommandInitialized(command);

        await connection.OpenAsync(cancellationToken);
        var reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess, cancellationToken);
        //多语句查询，在最后reader读取后，自动关闭
        return new MultiQueryReader(this.DbContext, connection, command, reader, readerAfters, isNeedClose);
    }
    #endregion

    #region Create
    public virtual ICreate Create(Type entityType) => this.ormProvider.NewCreate(entityType, this.DbContext);
    public virtual ICreate<TEntity> Create<TEntity>() => this.ormProvider.NewCreate<TEntity>(this.DbContext);
    public virtual int Create<TEntity>(object insertObj)
    {
        (var isNeedClose, var connection, var command) = this.CreateInsertCommand(typeof(TEntity), insertObj, false);
        return this.Execute(isNeedClose, connection, command);
    }
    public virtual async Task<int> CreateAsync<TEntity>(object insertObj, CancellationToken cancellationToken = default)
    {
        (var isNeedClose, var connection, var command) = this.CreateInsertCommand(typeof(TEntity), insertObj, false);
        return await this.ExecuteAsync(isNeedClose, connection, command, cancellationToken);
    }
    public virtual int Create<TEntity>(IEnumerable insertObjs, int bulkCount = 500)
    {
        (var isNeedClose, var connection, var command, var headSql, var commandInitializer)
            = this.CreateInsertBulkCommand(typeof(TEntity), insertObjs, bulkCount);
        return this.CreateBulk<TEntity>(isNeedClose, connection, command, insertObjs, bulkCount, headSql, commandInitializer);
    }
    public virtual async Task<int> CreateAsync<TEntity>(IEnumerable insertObjs, int bulkCount = 500, CancellationToken cancellationToken = default)
    {
        (var isNeedClose, var connection, var command, var headSql, var commandInitializer)
            = this.CreateInsertBulkCommand(typeof(TEntity), insertObjs, bulkCount);
        return await this.CreateBulkAsync<TEntity>(isNeedClose, connection, command, insertObjs, bulkCount, headSql, commandInitializer, cancellationToken);
    }
    public virtual int CreateIdentity<TEntity>(object insertObj)
    {
        (var isNeedClose, var connection, var command) = this.CreateInsertCommand(typeof(TEntity), insertObj, true);
        return this.QueryScalar<int>(isNeedClose, connection, command);
    }
    public virtual async Task<int> CreateIdentityAsync<TEntity>(object insertObj, CancellationToken cancellationToken = default)
    {
        (var isNeedClose, var connection, var command) = this.CreateInsertCommand(typeof(TEntity), insertObj, true);
        return await this.QueryScalarAsync<int>(isNeedClose, connection, command, cancellationToken);
    }
    public virtual long CreateIdentityLong<TEntity>(object insertObj)
    {
        (var isNeedClose, var connection, var command) = this.CreateInsertCommand(typeof(TEntity), insertObj, true);
        return this.QueryScalar<long>(isNeedClose, connection, command);
    }
    public virtual async Task<long> CreateIdentityLongAsync<TEntity>(object insertObj, CancellationToken cancellationToken = default)
    {
        (var isNeedClose, var connection, var command) = this.CreateInsertCommand(typeof(TEntity), insertObj, true);
        return await this.QueryScalarAsync<long>(isNeedClose, connection, command, cancellationToken);
    }
    #endregion

    #region Update
    public virtual IUpdate<TEntity> Update<TEntity>() => this.ormProvider.NewUpdate<TEntity>(this.DbContext);
    public virtual int Update<TEntity>(object updateObj)
    {
        (var isNeedClose, var connection, var command) = this.CreateUpdateCommand(typeof(TEntity), updateObj);
        return this.Execute(isNeedClose, connection, command);
    }
    public virtual async Task<int> UpdateAsync<TEntity>(object updateObj, CancellationToken cancellationToken = default)
    {
        (var isNeedClose, var connection, var command) = this.CreateUpdateCommand(typeof(TEntity), updateObj);
        return await this.ExecuteAsync(isNeedClose, connection, command);
    }
    public virtual int Update<TEntity>(IEnumerable updateObjs, int bulkCount)
    {
        (var isNeedClose, var connection, var command, var commandInitializer) = this.CreateUpdateBulkCommand(typeof(TEntity), updateObjs, bulkCount);
        return this.UpdateBulk<TEntity>(isNeedClose, connection, command, updateObjs, bulkCount, commandInitializer);
    }
    public virtual async Task<int> UpdateAsync<TEntity>(IEnumerable updateObjs, int bulkCount, CancellationToken cancellationToken = default)
    {
        (var isNeedClose, var connection, var command, var commandInitializer) = this.CreateUpdateBulkCommand(typeof(TEntity), updateObjs, bulkCount);
        return await this.UpdateBulkAsync<TEntity>(isNeedClose, connection, command, updateObjs, bulkCount, commandInitializer, cancellationToken);
    }
    #endregion

    #region Delete
    public virtual IDelete Delete(Type entityType) => this.ormProvider.NewDelete(entityType, this.DbContext);
    public virtual IDelete<TEntity> Delete<TEntity>() => this.ormProvider.NewDelete<TEntity>(this.DbContext);
    public virtual int DeleteBy<TEntity>(object whereObj)
    {
        (var isNeedClose, var connection, var command) = this.CreateDeleteCommand(typeof(TEntity), whereObj, false, false);
        return this.Execute(isNeedClose, connection, command);
    }
    public virtual async Task<int> DeleteByAsync<TEntity>(object whereObj, CancellationToken cancellationToken = default)
    {
        (var isNeedClose, var connection, var command) = this.CreateDeleteCommand(typeof(TEntity), whereObj, false, false);
        return await this.ExecuteAsync(isNeedClose, connection, command);
    }
    public virtual int DeleteById<TEntity>(object whereKey)
    {
        (var isNeedClose, var connection, var command) = this.CreateDeleteCommand(typeof(TEntity), whereKey, true, false);
        return this.Execute(isNeedClose, connection, command);
    }
    public virtual async Task<int> DeleteByIdAsync<TEntity>(object whereKey, CancellationToken cancellationToken = default)
    {
        (var isNeedClose, var connection, var command) = this.CreateDeleteCommand(typeof(TEntity), whereKey, true, false);
        return await this.ExecuteAsync(isNeedClose, connection, command);
    }
    public virtual int DeleteByIds<TEntity>(IEnumerable whereKeys)
    {
        (var isNeedClose, var connection, var command) = this.CreateDeleteCommand(typeof(TEntity), whereKeys, true, true);
        return this.Execute(isNeedClose, connection, command);
    }
    public virtual async Task<int> DeleteByIdsAsync<TEntity>(IEnumerable whereKeys, CancellationToken cancellationToken = default)
    {
        (var isNeedClose, var connection, var command) = this.CreateDeleteCommand(typeof(TEntity), whereKeys, true, true);
        return await this.ExecuteAsync(isNeedClose, connection, command);
    }
    #endregion

    #region Execute
    public virtual int Execute(string rawSql, object parameters = null, CommandType commandType = CommandType.Text)
    {
        (var isNeedClose, var connection, var command) = this.CreateExecuteCommand(rawSql, parameters, commandType);
        return this.Execute(isNeedClose, connection, command);
    }
    public virtual async Task<int> ExecuteAsync(string rawSql, object parameters = null, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default)
    {
        (var isNeedClose, var connection, var command) = this.CreateExecuteCommand(rawSql, parameters, commandType);
        return await this.ExecuteAsync(isNeedClose, connection, command);
    }
    public virtual int Execute(string rawSql, List<IDbDataParameter> parameters, CommandType commandType = CommandType.Text)
    {
        (var isNeedClose, var connection, var command) = this.CreateExecuteCommand(rawSql, parameters, commandType);
        return this.Execute(isNeedClose, connection, command);
    }
    public virtual async Task<int> ExecuteAsync(string rawSql, List<IDbDataParameter> parameters, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default)
    {
        (var isNeedClose, var connection, var command) = this.CreateExecuteCommand(rawSql, parameters, commandType);
        return await this.ExecuteAsync(isNeedClose, connection, command);
    }
    #endregion

    #region Others
    public virtual IRepository WithTimeout(int seconds)
    {
        this.DbContext.Options.CommandTimeout = seconds;
        return this;
    }
    public virtual IRepository WithOptions(Action<OrmDbFactoryOptions> optionsInitializer)
    {
        if (optionsInitializer == null) throw new ArgumentNullException(nameof(optionsInitializer));
        optionsInitializer.Invoke(this.DbContext.Options);
        return this;
    }
    //抛异常的时候，会走到析构函数，但是Transaction，没有提交也没有回滚
    #endregion
}