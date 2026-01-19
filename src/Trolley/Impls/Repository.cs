using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley;

public class Repository : IRepository
{
    #region Properties
    public DbContext DbContext { get; set; }
    public IOrmProvider OrmProvider => this.DbContext.OrmProvider;
    public IEntityMapProvider MapProvider => this.DbContext.EntityMapProvider;
    public ITableShardingProvider ShardingProvider => this.DbContext.TableShardingProvider;
    public bool IsParameterized => this.DbContext.IsConstantParameterized;
    #endregion

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
    public virtual Task CreateShardingTableAsync<TEntity>(string tableName, string fromTableSchema = null, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public virtual string GetShardingTableName<TEntity>(params object[] fieldValues)
        => this.DbContext.GetShardingTable(typeof(TEntity), fieldValues);
    public virtual void CreateShardingTable<TEntity>(object[] fieldValues, string fromTableSchema = null)
    {
        var tableName = this.DbContext.GetShardingTable(typeof(TEntity), fieldValues);
        this.CreateShardingTable<TEntity>(tableName, fromTableSchema);
    }
    public virtual async Task CreateShardingTableAsync<TEntity>(object[] fieldValues, string fromTableSchema = null, CancellationToken cancellationToken = default)
    {
        var tableName = this.DbContext.GetShardingTable(typeof(TEntity), fieldValues);
        await this.CreateShardingTableAsync<TEntity>(tableName, fromTableSchema, cancellationToken);
    }
    #endregion     

    #region From
    public virtual IQuery<T> From<T>(char tableAsStart = 'a')
    {
        var visitor = this.CreateQueryVisitor(tableAsStart);
        visitor.From(tableAsStart, typeof(T));
        return this.OrmProvider.NewQuery<T>(this.DbContext, visitor);
    }
    public virtual IQuery<T1, T2> From<T1, T2>(char tableAsStart = 'a')
    {
        var visitor = this.CreateQueryVisitor(tableAsStart);
        visitor.From(tableAsStart, typeof(T1), typeof(T2));
        return this.OrmProvider.NewQuery<T1, T2>(this.DbContext, visitor);
    }
    public virtual IQuery<T1, T2, T3> From<T1, T2, T3>(char tableAsStart = 'a')
    {
        var visitor = this.CreateQueryVisitor(tableAsStart);
        visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3));
        return this.OrmProvider.NewQuery<T1, T2, T3>(this.DbContext, visitor);
    }
    public virtual IQuery<T1, T2, T3, T4> From<T1, T2, T3, T4>(char tableAsStart = 'a')
    {
        var visitor = this.CreateQueryVisitor(tableAsStart);
        visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4));
        return this.OrmProvider.NewQuery<T1, T2, T3, T4>(this.DbContext, visitor);
    }
    public virtual IQuery<T1, T2, T3, T4, T5> From<T1, T2, T3, T4, T5>(char tableAsStart = 'a')
    {
        var visitor = this.CreateQueryVisitor(tableAsStart);
        visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5));
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5>(this.DbContext, visitor);
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6> From<T1, T2, T3, T4, T5, T6>(char tableAsStart = 'a')
    {
        var visitor = this.CreateQueryVisitor(tableAsStart);
        visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6));
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6>(this.DbContext, visitor);
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7> From<T1, T2, T3, T4, T5, T6, T7>(char tableAsStart = 'a')
    {
        var visitor = this.CreateQueryVisitor(tableAsStart);
        visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7));
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7>(this.DbContext, visitor);
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8> From<T1, T2, T3, T4, T5, T6, T7, T8>(char tableAsStart = 'a')
    {
        var visitor = this.CreateQueryVisitor(tableAsStart);
        visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8));
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8>(this.DbContext, visitor);
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> From<T1, T2, T3, T4, T5, T6, T7, T8, T9>(char tableAsStart = 'a')
    {
        var visitor = this.CreateQueryVisitor(tableAsStart);
        visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9));
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this.DbContext, visitor);
    }
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> From<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(char tableAsStart = 'a')
    {
        var visitor = this.CreateQueryVisitor(tableAsStart);
        visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10));
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this.DbContext, visitor);
    }
    #endregion

    #region FromQuery
    public virtual IQuery<T> FromQuery<T>(IQuery<T> subQuery)
    {
        var visitor = this.CreateQueryVisitor();
        visitor.UseQuery(typeof(T), subQuery, true);
        return this.OrmProvider.NewQuery<T>(this.DbContext, visitor);
    }
    public virtual IQuery<T> FromQuery<T>(Expression<Func<IFromQuery, IQuery<T>>> subQueryExpr)
    {
        var visitor = this.CreateQueryVisitor();
        visitor.UseNewQuery(typeof(T), subQueryExpr, true);
        return this.OrmProvider.NewQuery<T>(this.DbContext, visitor);
    }
    #endregion

    #region QueryScalar
    public virtual TValue QueryScalar<TValue>(string rawSql, CommandType commandType = CommandType.Text)
        => this.DbContext.QueryScalar<TValue>(rawSql, commandType);
    public virtual async Task<TValue> QueryScalarAsync<TValue>(string rawSql, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default)
        => await this.DbContext.QueryScalarAsync<TValue>(rawSql, commandType, cancellationToken);
    public virtual TValue QueryScalar<TValue>(string rawSql, object parameters, CommandType commandType = CommandType.Text)
        => this.DbContext.QueryScalar<TValue>(rawSql, parameters, commandType);
    public virtual async Task<TValue> QueryScalarAsync<TValue>(string rawSql, object parameters = null, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default)
        => await this.DbContext.QueryScalarAsync<TValue>(rawSql, parameters, commandType, cancellationToken);
    public virtual TValue QueryScalar<TValue>(string rawSql, List<IDbDataParameter> parameters, CommandType commandType = CommandType.Text)
        => this.DbContext.QueryScalar<TValue>(rawSql, parameters, commandType);
    public virtual async Task<TValue> QueryScalarAsync<TValue>(string rawSql, List<IDbDataParameter> parameters, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default)
        => await this.DbContext.QueryScalarAsync<TValue>(rawSql, parameters, commandType, cancellationToken);
    #endregion

    #region QueryById
    public virtual TEntity QueryById<TEntity>(object whereKey)
        => this.DbContext.Query<TEntity, TEntity>(whereKey, true, false, (reader, deserializer) => reader.Read() ? (TEntity)deserializer.Invoke(reader) : default);
    public virtual async Task<TEntity> QueryByIdAsync<TEntity>(object whereKey, CancellationToken cancellationToken = default)
        => await this.DbContext.QueryAsync<TEntity, TEntity>(whereKey, true, false, async (reader, deserializer, cancellationToken) => (await reader.ReadAsync(cancellationToken)) ? (TEntity)deserializer.Invoke(reader) : default, cancellationToken);
    #endregion

    #region QueryFirst
    public virtual TEntity QueryFirst<TEntity>(string rawSql, CommandType commandType = CommandType.Text)
        => this.DbContext.Query<TEntity, TEntity>(rawSql, false, (reader, deserializer) => reader.Read() ? (TEntity)deserializer.Invoke(reader) : default, commandType);
    public virtual async Task<TEntity> QueryFirstAsync<TEntity>(string rawSql, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default)
        => await this.DbContext.QueryAsync<TEntity, TEntity>(rawSql, false, async (reader, deserializer, cancellationToken) => (await reader.ReadAsync(cancellationToken)) ? (TEntity)deserializer.Invoke(reader) : default, commandType, cancellationToken);
    public virtual TEntity QueryFirst<TEntity>(string rawSql, object parameters, CommandType commandType = CommandType.Text)
        => this.DbContext.Query<TEntity, TEntity>(rawSql, false, parameters, (reader, deserializer) => reader.Read() ? (TEntity)deserializer.Invoke(reader) : default, commandType);
    public virtual async Task<TEntity> QueryFirstAsync<TEntity>(string rawSql, object parameters, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default)
        => await this.DbContext.QueryAsync<TEntity, TEntity>(rawSql, false, parameters, async (reader, deserializer, cancellationToken) => (await reader.ReadAsync(cancellationToken)) ? (TEntity)deserializer.Invoke(reader) : default, commandType, cancellationToken);
    public virtual TEntity QueryFirst<TEntity>(string rawSql, List<IDbDataParameter> parameters, CommandType commandType = CommandType.Text)
        => this.DbContext.QueryRaw<TEntity, TEntity>(rawSql, false, parameters, (reader, deserializer) => reader.Read() ? (TEntity)deserializer.Invoke(reader) : default, commandType);
    public virtual async Task<TEntity> QueryFirstAsync<TEntity>(string rawSql, List<IDbDataParameter> parameters, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default)
        => await this.DbContext.QueryRawAsync<TEntity, TEntity>(rawSql, false, parameters, async (reader, deserializer, cancellationToken) => (await reader.ReadAsync(cancellationToken)) ? (TEntity)deserializer.Invoke(reader) : default, commandType, cancellationToken);
    public virtual TEntity QueryFirst<TEntity>(object whereObj = null)
        => this.DbContext.Query<TEntity, TEntity>(whereObj, false, false, (reader, deserializer) => reader.Read() ? (TEntity)deserializer.Invoke(reader) : default);
    public virtual async Task<TEntity> QueryFirstAsync<TEntity>(object whereObj = null, CancellationToken cancellationToken = default)
        => await this.DbContext.QueryAsync<TEntity, TEntity>(whereObj, false, false, async (reader, deserializer, cancellationToken) => (await reader.ReadAsync(cancellationToken)) ? (TEntity)deserializer.Invoke(reader) : default, cancellationToken);
    #endregion

    #region QueryByIds
    public virtual List<TEntity> QueryByIds<TEntity>(IEnumerable whereKeys)
    {
        return this.DbContext.Query<TEntity, List<TEntity>>(whereKeys, true, true, (reader, deserializer) =>
        {
            var result = new List<TEntity>();
            while (reader.Read())
                result.Add((TEntity)deserializer.Invoke(reader));
            return result;
        });
    }
    public virtual async Task<List<TEntity>> QueryByIdsAsync<TEntity>(IEnumerable whereKeys, CancellationToken cancellationToken = default)
    {
        return await this.DbContext.QueryAsync<TEntity, List<TEntity>>(whereKeys, true, true, async (reader, deserializer, cancellationToken) =>
        {
            var result = new List<TEntity>();
            while (await reader.ReadAsync(cancellationToken))
                result.Add((TEntity)deserializer.Invoke(reader));
            return result;
        }, cancellationToken);
    }
    #endregion

    #region Query
    public virtual List<TEntity> Query<TEntity>(string rawSql, CommandType commandType = CommandType.Text)
        => this.DbContext.Query<TEntity, List<TEntity>>(rawSql, true, (reader, deserializer) =>
        {
            var result = new List<TEntity>();
            while (reader.Read())
                result.Add((TEntity)deserializer.Invoke(reader));
            return result;
        }, commandType);
    public virtual async Task<List<TEntity>> QueryAsync<TEntity>(string rawSql, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default)
        => await this.DbContext.QueryAsync<TEntity, List<TEntity>>(rawSql, true, async (reader, deserializer, cancellationToken) =>
        {
            var result = new List<TEntity>();
            while (await reader.ReadAsync(cancellationToken))
                result.Add((TEntity)deserializer.Invoke(reader));
            return result;
        }, commandType, cancellationToken);
    public virtual List<TEntity> Query<TEntity>(string rawSql, object parameters, CommandType commandType = CommandType.Text)
        => this.DbContext.Query<TEntity, List<TEntity>>(rawSql, true, parameters, (reader, deserializer) =>
        {
            var result = new List<TEntity>();
            while (reader.Read())
                result.Add((TEntity)deserializer.Invoke(reader));
            return result;
        }, commandType);
    public virtual async Task<List<TEntity>> QueryAsync<TEntity>(string rawSql, object parameters = null, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default)
        => await this.DbContext.QueryAsync<TEntity, List<TEntity>>(rawSql, true, parameters, async (reader, deserializer, cancellationToken) =>
        {
            var result = new List<TEntity>();
            while (await reader.ReadAsync(cancellationToken))
                result.Add((TEntity)deserializer.Invoke(reader));
            return result;
        }, commandType, cancellationToken);
    public virtual List<TEntity> Query<TEntity>(string rawSql, List<IDbDataParameter> parameters, CommandType commandType = CommandType.Text)
        => this.DbContext.QueryRaw<TEntity, List<TEntity>>(rawSql, true, parameters, (reader, deserializer) =>
        {
            var result = new List<TEntity>();
            while (reader.Read())
                result.Add((TEntity)deserializer.Invoke(reader));
            return result;
        }, commandType);
    public virtual async Task<List<TEntity>> QueryAsync<TEntity>(string rawSql, List<IDbDataParameter> parameters, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default)
        => await this.DbContext.QueryRawAsync<TEntity, List<TEntity>>(rawSql, true, parameters, async (reader, deserializer, cancellationToken) =>
        {
            var result = new List<TEntity>();
            while (await reader.ReadAsync(cancellationToken))
                result.Add((TEntity)deserializer.Invoke(reader));
            return result;
        }, commandType, cancellationToken);

    public virtual List<TEntity> Query<TEntity>(object whereObj = null)
        => this.DbContext.Query<TEntity, List<TEntity>>(whereObj, false, true, (reader, deserializer) =>
        {
            var result = new List<TEntity>();
            while (reader.Read())
                result.Add((TEntity)deserializer.Invoke(reader));
            return result;
        });
    public virtual async Task<List<TEntity>> QueryAsync<TEntity>(object whereObj = null, CancellationToken cancellationToken = default)
        => await this.DbContext.QueryAsync<TEntity, List<TEntity>>(whereObj, false, true, async (reader, deserializer, cancellationToken) =>
        {
            var result = new List<TEntity>();
            while (await reader.ReadAsync(cancellationToken))
                result.Add((TEntity)deserializer.Invoke(reader));
            return result;
        }, cancellationToken);
    #endregion

    #region Exists
    public virtual bool ExistsBy<TEntity>(object whereObj) => this.DbContext.Exists<TEntity>(whereObj, false, false);
    public virtual async Task<bool> ExistsByAsync<TEntity>(object whereObj, CancellationToken cancellationToken = default)
        => await this.DbContext.ExistsAsync<TEntity>(whereObj, false, false, cancellationToken);
    public virtual bool ExistsById<TEntity>(object whereKey) => this.DbContext.Exists<TEntity>(whereKey, true, false);
    public virtual async Task<bool> ExistsByIdAsync<TEntity>(object whereKey, CancellationToken cancellationToken = default)
        => await this.DbContext.ExistsAsync<TEntity>(whereKey, true, false, cancellationToken);
    public virtual bool ExistsByIds<TEntity>(IEnumerable whereKeys) => this.DbContext.Exists<TEntity>(whereKeys, true, true);
    public virtual async Task<bool> ExistsByIdsAsync<TEntity>(IEnumerable whereKeys, CancellationToken cancellationToken = default)
        => await this.DbContext.ExistsAsync<TEntity>(whereKeys, true, true, cancellationToken);
    #endregion

    #region Create
    public virtual ICreate<TEntity> Create<TEntity>() => this.OrmProvider.NewCreate<TEntity>(this.DbContext);
    public virtual int Create<TEntity>(object insertObj) => this.DbContext.Create<TEntity>(insertObj);
    public virtual int Create<TEntity>(IEnumerable insertObjs, int bulkCount)
        => this.DbContext.Create<TEntity>(insertObjs, bulkCount);
    public virtual async Task<int> CreateAsync<TEntity>(object insertObj, CancellationToken cancellationToken = default)
        => await this.DbContext.CreateAsync<TEntity>(insertObj, cancellationToken);
    public virtual async Task<int> CreateAsync<TEntity>(IEnumerable insertObjs, int bulkCount, CancellationToken cancellationToken = default)
        => await this.DbContext.CreateAsync<TEntity>(insertObjs, bulkCount, cancellationToken);
    public virtual int CreateIdentity<TEntity>(object insertObj) => this.DbContext.CreateIdentity<TEntity, int>(insertObj);
    public virtual async Task<int> CreateIdentityAsync<TEntity>(object insertObj, CancellationToken cancellationToken = default)
        => await this.DbContext.CreateIdentityAsync<TEntity, int>(insertObj, cancellationToken);
    public virtual long CreateIdentityLong<TEntity>(object insertObj) => this.DbContext.CreateIdentity<TEntity, long>(insertObj);
    public virtual async Task<long> CreateIdentityLongAsync<TEntity>(object insertObj, CancellationToken cancellationToken = default)
        => await this.DbContext.CreateIdentityAsync<TEntity, long>(insertObj, cancellationToken);
    #endregion

    #region Update
    public virtual IUpdate<TEntity> Update<TEntity>() => this.OrmProvider.NewUpdate<TEntity>(this.DbContext);
    public virtual int Update<TEntity>(object updateObj) => this.DbContext.Update<TEntity>(updateObj);
    public virtual async Task<int> UpdateAsync<TEntity>(object updateObj, CancellationToken cancellationToken = default)
        => await this.DbContext.UpdateAsync<TEntity>(updateObj, cancellationToken);
    public virtual int Update<TEntity>(IEnumerable updateObjs, int bulkCount)
        => this.DbContext.Update<TEntity>(updateObjs, bulkCount);
    public virtual async Task<int> UpdateAsync<TEntity>(IEnumerable updateObjs, int bulkCount, CancellationToken cancellationToken = default)
        => await this.DbContext.UpdateAsync<TEntity>(updateObjs, bulkCount, cancellationToken);
    #endregion

    #region Delete
    public virtual IDelete<TEntity> Delete<TEntity>() => this.OrmProvider.NewDelete<TEntity>(this.DbContext);
    public virtual int DeleteBy<TEntity>(object whereObj) => this.DbContext.Delete<TEntity>(whereObj, false, false);
    public virtual async Task<int> DeleteByAsync<TEntity>(object whereObj, CancellationToken cancellationToken = default)
        => await this.DbContext.DeleteAsync<TEntity>(whereObj, false, false, cancellationToken);
    public virtual int DeleteById<TEntity>(object whereKey) => this.DbContext.Delete<TEntity>(whereKey, true, false);
    public virtual async Task<int> DeleteByIdAsync<TEntity>(object whereKey, CancellationToken cancellationToken = default)
        => await this.DbContext.DeleteAsync<TEntity>(whereKey, true, false, cancellationToken);
    public virtual int DeleteByIds<TEntity>(IEnumerable whereKeys) => this.DbContext.Delete<TEntity>(whereKeys, true, true);
    public virtual async Task<int> DeleteByIdsAsync<TEntity>(IEnumerable whereKeys, CancellationToken cancellationToken = default)
        => await this.DbContext.DeleteAsync<TEntity>(whereKeys, true, true, cancellationToken);
    #endregion

    #region Execute
    public virtual int Execute(string rawSql, object parameters = null, CommandType commandType = CommandType.Text)
        => this.DbContext.Execute(rawSql, parameters, commandType);
    public virtual async Task<int> ExecuteAsync(string rawSql, object parameters = null, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default)
        => await this.DbContext.ExecuteAsync(rawSql, parameters, commandType, cancellationToken);
    public virtual int Execute(string rawSql, List<IDbDataParameter> parameters, CommandType commandType = CommandType.Text)
        => this.DbContext.Execute(rawSql, parameters, commandType);
    public virtual async Task<int> ExecuteAsync(string rawSql, List<IDbDataParameter> parameters, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default)
        => await this.DbContext.ExecuteAsync(rawSql, parameters, commandType, cancellationToken);
    #endregion

    #region QueryMultiple
    public virtual IMultiQueryReader QueryMultiple(Action<IMultipleQuery> subQueries)
    {
        if (subQueries == null)
            throw new ArgumentNullException(nameof(subQueries));

        using var multiQuery = this.OrmProvider.NewMultipleQuery(this.DbContext);
        subQueries.Invoke(multiQuery);
        (var isNeedClose, var connection, var command) = this.DbContext.UseSlaveCommand(multiQuery.Command);
        multiQuery.Command.Connection = connection.BaseConnection;
        command.CommandText = multiQuery.BuildSql(out var readerAfters);
        connection.Open();
        var reader = command.ExecuteReader(CommandSqlType.MultiQuery, CommandBehavior.SequentialAccess);
        //多语句查询，在最后reader读取后，自动关闭
        return new MultiQueryReader(this.DbContext, connection, command, reader, readerAfters, isNeedClose);
    }
    public virtual async Task<IMultiQueryReader> QueryMultipleAsync(Action<IMultipleQuery> subQueries, CancellationToken cancellationToken = default)
    {
        if (subQueries == null)
            throw new ArgumentNullException(nameof(subQueries));

        using var multiQuery = this.OrmProvider.NewMultipleQuery(this.DbContext);
        subQueries.Invoke(multiQuery);
        (var isNeedClose, var connection, var command) = this.DbContext.UseSlaveCommand(multiQuery.Command);
        multiQuery.Command.Connection = connection.BaseConnection;
        command.CommandText = multiQuery.BuildSql(out var readerAfters);
        await connection.OpenAsync(cancellationToken);
        var reader = await command.ExecuteReaderAsync(CommandSqlType.MultiQuery, CommandBehavior.SequentialAccess, cancellationToken);
        //多语句查询，在最后reader读取后，自动关闭
        return new MultiQueryReader(this.DbContext, connection, command, reader, readerAfters, isNeedClose);
    }
    #endregion

    #region Others 
    public virtual void Close() => this.DbContext.Connection.Close();
    public virtual async Task CloseAsync() => await this.DbContext.Connection.CloseAsync();
    public virtual void BeginTransaction() => this.DbContext.BeginTransaction();
    public virtual async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        => await this.DbContext.BeginTransactionAsync(cancellationToken);
    public virtual void Commit() => this.DbContext.Commit();
    public virtual async Task CommitAsync(CancellationToken cancellationToken = default)
        => await this.DbContext.CommitAsync(cancellationToken);
    public virtual void Rollback() => this.DbContext.Rollback();
    public virtual async Task RollbackAsync(CancellationToken cancellationToken = default)
        => await this.DbContext.RollbackAsync(cancellationToken);
    public virtual IRepository WithTimeout(int seconds)
    {
        this.DbContext.CommandTimeout = seconds;
        return this;
    }
    //抛异常的时候，会走到析构函数，但是Transaction，没有提交也没有回滚
    private IQueryVisitor CreateQueryVisitor(char tableAsStart = 'a')
        => this.OrmProvider.NewQueryVisitor(this.DbContext, tableAsStart);
    #endregion
}