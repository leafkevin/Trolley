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
    public virtual List<string> GetShardingTableNames<TEntity>(Func<string, bool> tableNameSelector, string tableSchema = null) => null;
    public virtual Task<List<string>> GetShardingTableNamesAsync<TEntity>(Func<string, bool> tableNameSelector, string tableSchema = null, CancellationToken cancellationToken = default) => null;
    public virtual void CreateShardingTable<TEntity>(string tableName, string fromTableSchema = null) { }
    public virtual Task CreateShardingTableAsync<TEntity>(string tableName, string fromTableSchema = null, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public virtual string GetShardingTableNameBy<TEntity>(params object[] fieldValues)
        => this.DbContext.GetShardingTableBy(typeof(TEntity), fieldValues);
    public virtual void CreateShardingTableBy<TEntity>(object[] fieldValues, string fromTableSchema = null)
    {
        var tableName = this.DbContext.GetShardingTableBy(typeof(TEntity), fieldValues);
        this.CreateShardingTable<TEntity>(tableName, fromTableSchema);
    }
    public virtual async Task CreateShardingTableByAsync<TEntity>(object[] fieldValues, string fromTableSchema = null, CancellationToken cancellationToken = default)
    {
        var tableName = this.DbContext.GetShardingTableBy(typeof(TEntity), fieldValues);
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

    #region GetById
    public virtual TEntity GetById<TEntity>(object whereKey)
        => this.DbContext.QueryById<TEntity, TEntity>(whereKey, false, (reader, deserializer) => reader.Read() ? (TEntity)deserializer.Invoke(reader) : default);
    public virtual async Task<TEntity> GetByIdAsync<TEntity>(object whereKey, CancellationToken cancellationToken = default)
        => await this.DbContext.QueryByIdAsync<TEntity, TEntity>(whereKey, false, async (reader, deserializer, cancellationToken) => (await reader.ReadAsync(cancellationToken)) ? (TEntity)deserializer.Invoke(reader) : default, cancellationToken);
    #endregion

    #region GetByIds
    public virtual List<TEntity> GetByIds<TEntity>(IEnumerable whereKeys)
    {
        return this.DbContext.QueryById<TEntity, List<TEntity>>(whereKeys, true, (reader, deserializer) =>
        {
            var result = new List<TEntity>();
            while (reader.Read())
                result.Add((TEntity)deserializer.Invoke(reader));
            return result;
        });
    }
    public virtual async Task<List<TEntity>> GetByIdsAsync<TEntity>(IEnumerable whereKeys, CancellationToken cancellationToken = default)
    {
        return await this.DbContext.QueryByIdAsync<TEntity, List<TEntity>>(whereKeys, true, async (reader, deserializer, cancellationToken) =>
        {
            var result = new List<TEntity>();
            while (await reader.ReadAsync(cancellationToken))
                result.Add((TEntity)deserializer.Invoke(reader));
            return result;
        }, cancellationToken);
    }
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
        => this.DbContext.Query<TEntity, TEntity>(rawSql, false, parameters, (reader, deserializer) => reader.Read() ? (TEntity)deserializer.Invoke(reader) : default, commandType);
    public virtual async Task<TEntity> QueryFirstAsync<TEntity>(string rawSql, List<IDbDataParameter> parameters, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default)
        => await this.DbContext.QueryAsync<TEntity, TEntity>(rawSql, false, parameters, async (reader, deserializer, cancellationToken) => (await reader.ReadAsync(cancellationToken)) ? (TEntity)deserializer.Invoke(reader) : default, commandType, cancellationToken);
    public virtual TEntity QueryFirst<TEntity>(object whereObj)
        => this.DbContext.Query<TEntity, TEntity>(whereObj, false, (reader, deserializer) => reader.Read() ? (TEntity)deserializer.Invoke(reader) : default);
    public virtual async Task<TEntity> QueryFirstAsync<TEntity>(object whereObj, CancellationToken cancellationToken = default)
        => await this.DbContext.QueryAsync<TEntity, TEntity>(whereObj, false, async (reader, deserializer, cancellationToken) => (await reader.ReadAsync(cancellationToken)) ? (TEntity)deserializer.Invoke(reader) : default, cancellationToken);
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
        => this.DbContext.Query<TEntity, List<TEntity>>(rawSql, true, parameters, (reader, deserializer) =>
        {
            var result = new List<TEntity>();
            while (reader.Read())
                result.Add((TEntity)deserializer.Invoke(reader));
            return result;
        }, commandType);
    public virtual async Task<List<TEntity>> QueryAsync<TEntity>(string rawSql, List<IDbDataParameter> parameters, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default)
        => await this.DbContext.QueryAsync<TEntity, List<TEntity>>(rawSql, true, parameters, async (reader, deserializer, cancellationToken) =>
        {
            var result = new List<TEntity>();
            while (await reader.ReadAsync(cancellationToken))
                result.Add((TEntity)deserializer.Invoke(reader));
            return result;
        }, commandType, cancellationToken);
    public virtual List<TEntity> Query<TEntity>(object whereObj)
        => this.DbContext.Query<TEntity, List<TEntity>>(whereObj, true, (reader, deserializer) =>
        {
            var result = new List<TEntity>();
            while (reader.Read())
                result.Add((TEntity)deserializer.Invoke(reader));
            return result;
        });
    public virtual async Task<List<TEntity>> QueryAsync<TEntity>(object whereObj, CancellationToken cancellationToken = default)
        => await this.DbContext.QueryAsync<TEntity, List<TEntity>>(whereObj, true, async (reader, deserializer, cancellationToken) =>
        {
            var result = new List<TEntity>();
            while (await reader.ReadAsync(cancellationToken))
                result.Add((TEntity)deserializer.Invoke(reader));
            return result;
        }, cancellationToken);
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
    public virtual int Delete<TEntity>(object whereKeys) => this.DbContext.Delete<TEntity>(whereKeys);
    public virtual async Task<int> DeleteAsync<TEntity>(object whereKeys, CancellationToken cancellationToken = default)
        => await this.DbContext.DeleteAsync<TEntity>(whereKeys, cancellationToken);
    #endregion

    #region Exists
    public virtual bool Exists<TEntity>(object whereObjs) => this.DbContext.Exists<TEntity>(whereObjs);
    public virtual async Task<bool> ExistsAsync<TEntity>(object whereObjs, CancellationToken cancellationToken = default)
        => await this.DbContext.ExistsAsync<TEntity>(whereObjs, cancellationToken);
    public virtual bool Exists<TEntity>(Expression<Func<TEntity, bool>> wherePredicate = null)
        => this.From<TEntity>().Where(wherePredicate).Count() > 0;
    public virtual async Task<bool> ExistsAsync<TEntity>(Expression<Func<TEntity, bool>> wherePredicate, CancellationToken cancellationToken = default)
        => await this.From<TEntity>().Where(wherePredicate).CountAsync(cancellationToken) > 0;
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