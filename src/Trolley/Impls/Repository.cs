using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley;

public class Repository : IRepository
{
    #region Fields 
    protected DbContext dbContext;
    protected bool isParameterized => this.dbContext.IsParameterized;
    #endregion

    #region Properties
    public string DbKey => this.dbContext.DbKey;
    public IDbConnection Connection => this.dbContext.Connection;
    public IOrmProvider OrmProvider => this.dbContext.OrmProvider;
    public IEntityMapProvider MapProvider => this.dbContext.MapProvider;
    public IDbTransaction Transaction => this.dbContext.Transaction;
    #endregion

    #region Constructor
    public Repository(string dbKey, IDbConnection connection, IOrmProvider ormProvider, IEntityMapProvider mapProvider)
    {
        this.dbContext = new DbContext
        {
            DbKey = dbKey,
            Connection = new TheaConnection { DbKey = dbKey, BaseConnection = connection },
            OrmProvider = ormProvider,
            MapProvider = mapProvider
        };
    }
    public Repository(TheaConnection connection, IOrmProvider ormProvider, IEntityMapProvider mapProvider)
    {
        this.dbContext = new DbContext
        {
            DbKey = connection.DbKey,
            Connection = connection,
            OrmProvider = ormProvider,
            MapProvider = mapProvider
        };
    }
    #endregion

    #region From
    public IQuery<T> From<T>(char tableAsStart = 'a', string suffixRawSql = null)
    {
        var visitor = this.CreateQueryVisitor(tableAsStart);
        visitor.From(tableAsStart, typeof(T), suffixRawSql);
        return this.OrmProvider.NewQuery<T>(this.dbContext, visitor);
    }
    public IQuery<T1, T2> From<T1, T2>(char tableAsStart = 'a')
    {
        var visitor = this.CreateQueryVisitor(tableAsStart);
        visitor.From(tableAsStart, typeof(T1), typeof(T2));
        return this.OrmProvider.NewQuery<T1, T2>(this.dbContext, visitor);
    }
    public IQuery<T1, T2, T3> From<T1, T2, T3>(char tableAsStart = 'a')
    {
        var visitor = this.CreateQueryVisitor(tableAsStart);
        visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3));
        return this.OrmProvider.NewQuery<T1, T2, T3>(this.dbContext, visitor);
    }
    public IQuery<T1, T2, T3, T4> From<T1, T2, T3, T4>(char tableAsStart = 'a')
    {
        var visitor = this.CreateQueryVisitor(tableAsStart);
        visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4));
        return this.OrmProvider.NewQuery<T1, T2, T3, T4>(this.dbContext, visitor);
    }
    public IQuery<T1, T2, T3, T4, T5> From<T1, T2, T3, T4, T5>(char tableAsStart = 'a')
    {
        var visitor = this.CreateQueryVisitor(tableAsStart);
        visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5));
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5>(this.dbContext, visitor);
    }
    public IQuery<T1, T2, T3, T4, T5, T6> From<T1, T2, T3, T4, T5, T6>(char tableAsStart = 'a')
    {
        var visitor = this.CreateQueryVisitor(tableAsStart);
        visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6));
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6>(this.dbContext, visitor);
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7> From<T1, T2, T3, T4, T5, T6, T7>(char tableAsStart = 'a')
    {
        var visitor = this.CreateQueryVisitor(tableAsStart);
        visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7));
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7>(this.dbContext, visitor);
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8> From<T1, T2, T3, T4, T5, T6, T7, T8>(char tableAsStart = 'a')
    {
        var visitor = this.CreateQueryVisitor(tableAsStart);
        visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8));
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8>(this.dbContext, visitor);
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> From<T1, T2, T3, T4, T5, T6, T7, T8, T9>(char tableAsStart = 'a')
    {
        var visitor = this.CreateQueryVisitor(tableAsStart);
        visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9));
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this.dbContext, visitor);
    }
    public IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> From<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(char tableAsStart = 'a')
    {
        var visitor = this.CreateQueryVisitor(tableAsStart);
        visitor.From(tableAsStart, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10));
        return this.OrmProvider.NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this.dbContext, visitor);
    }
    #endregion

    #region From SubQuery
    public IQuery<T> From<T>(IQuery<T> subQuery, char tableAsStart = 'a')
    {
        var visitor = subQuery.Visitor;
        var sql = visitor.BuildSql(out var readerFields);
        visitor.WithTable(typeof(T), sql, readerFields);
        return subQuery;
    }
    #endregion

    #region FromWith
    public IQuery<T> FromWith<T>(IQuery<T> cteSubQuery, string cteTableName = null, char tableAsStart = 'a')
    {
        var visitor = cteSubQuery.Visitor;
        var rawSql = cteSubQuery.Visitor.BuildSql(out var readerFields);
        visitor.BuildCteTable(cteTableName, rawSql, readerFields, cteSubQuery, true);
        return cteSubQuery;
    }
    public IQuery<T> FromWith<T>(Func<IFromQuery, IQuery<T>> cteSubQuery, string cteTableName = null, char tableAsStart = 'a')
    {
        var visitor = this.CreateQueryVisitor(tableAsStart, true);
        var fromQuery = new FromQuery(this.dbContext, visitor);
        var query = cteSubQuery.Invoke(fromQuery);
        if (!visitor.Equals(query.Visitor))
        {
            visitor.Dispose();
            visitor = query.Visitor;
        }
        var rawSql = visitor.BuildSql(out var readerFields, false);
        visitor.BuildCteTable(cteTableName, rawSql, readerFields, query, true);
        return query;
    }
    #endregion

    #region QueryFirst/Query
    public TEntity QueryFirst<TEntity>(string rawSql, object parameters = null)
    {
        if (string.IsNullOrEmpty(rawSql))
            throw new ArgumentNullException(nameof(rawSql));

        return this.dbContext.QueryFirst<TEntity>(f =>
        {
            f.CommandText = rawSql;
            if (parameters != null)
            {
                var commandInitializer = RepositoryHelper.BuildQueryRawSqlParameters(this.DbKey, this.OrmProvider, rawSql, parameters);
                commandInitializer.Invoke(f.Parameters, this.OrmProvider, parameters);
            }
        });
    }
    public async Task<TEntity> QueryFirstAsync<TEntity>(string rawSql, object parameters = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(rawSql))
            throw new ArgumentNullException(nameof(rawSql));

        return await this.dbContext.QueryFirstAsync<TEntity>(f =>
        {
            f.CommandText = rawSql;
            if (parameters != null)
            {
                var commandInitializer = RepositoryHelper.BuildQueryRawSqlParameters(this.DbKey, this.OrmProvider, rawSql, parameters);
                commandInitializer.Invoke(f.Parameters, this.OrmProvider, parameters);
            }
        }, cancellationToken);
    }
    public TEntity QueryFirst<TEntity>(object whereObj)
    {
        if (whereObj == null)
            throw new ArgumentNullException(nameof(whereObj));

        return this.dbContext.QueryFirst<TEntity>(f =>
        {
            var entityType = typeof(TEntity);
            var commandInitializer = RepositoryHelper.BuildQueryWhereObjSqlParameters(this.DbKey, this.OrmProvider, this.MapProvider, entityType, whereObj, false);
            var typedCommandInitializer = commandInitializer as Func<IDataParameterCollection, IOrmProvider, object, string>;
            f.CommandText = typedCommandInitializer.Invoke(f.Parameters, this.OrmProvider, whereObj);
        });
    }
    public async Task<TEntity> QueryFirstAsync<TEntity>(object whereObj, CancellationToken cancellationToken = default)
    {
        if (whereObj == null)
            throw new ArgumentNullException(nameof(whereObj));

        return await this.dbContext.QueryFirstAsync<TEntity>(f =>
        {
            var entityType = typeof(TEntity);
            var commandInitializer = RepositoryHelper.BuildQueryWhereObjSqlParameters(this.DbKey, this.OrmProvider, this.MapProvider, entityType, whereObj, false);
            var typedCommandInitializer = commandInitializer as Func<IDataParameterCollection, IOrmProvider, object, string>;
            f.CommandText = typedCommandInitializer.Invoke(f.Parameters, this.OrmProvider, whereObj);
        }, cancellationToken);
    }
    public List<TEntity> Query<TEntity>(string rawSql, object parameters = null)
    {
        if (string.IsNullOrEmpty(rawSql))
            throw new ArgumentNullException(nameof(rawSql));

        return this.dbContext.Query<TEntity>(f =>
        {
            f.CommandText = rawSql;
            if (parameters != null)
            {
                var commandInitializer = RepositoryHelper.BuildQueryRawSqlParameters(this.DbKey, this.OrmProvider, rawSql, parameters);
                commandInitializer.Invoke(f.Parameters, this.OrmProvider, parameters);
            }
        });
    }
    public async Task<List<TEntity>> QueryAsync<TEntity>(string rawSql, object parameters = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(rawSql))
            throw new ArgumentNullException(nameof(rawSql));

        return await this.dbContext.QueryAsync<TEntity>(f =>
        {
            f.CommandText = rawSql;
            if (parameters != null)
            {
                var commandInitializer = RepositoryHelper.BuildQueryRawSqlParameters(this.DbKey, this.OrmProvider, rawSql, parameters);
                commandInitializer.Invoke(f.Parameters, this.OrmProvider, parameters);
            }
        }, cancellationToken);
    }
    public List<TEntity> Query<TEntity>(object whereObj)
    {
        if (whereObj == null)
            throw new ArgumentNullException(nameof(whereObj));

        return this.dbContext.Query<TEntity>(f =>
        {
            var entityType = typeof(TEntity);
            var commandInitializer = RepositoryHelper.BuildQueryWhereObjSqlParameters(this.DbKey, this.OrmProvider, this.MapProvider, entityType, whereObj, false);
            var typedCommandInitializer = commandInitializer as Func<IDataParameterCollection, IOrmProvider, object, string>;
            f.CommandText = typedCommandInitializer.Invoke(f.Parameters, this.OrmProvider, whereObj);
        });
    }
    public async Task<List<TEntity>> QueryAsync<TEntity>(object whereObj, CancellationToken cancellationToken = default)
    {
        if (whereObj == null)
            throw new ArgumentNullException(nameof(whereObj));

        return await this.dbContext.QueryAsync<TEntity>(f =>
        {
            var entityType = typeof(TEntity);
            var commandInitializer = RepositoryHelper.BuildQueryWhereObjSqlParameters(this.DbKey, this.OrmProvider, this.MapProvider, entityType, whereObj, false);
            var typedCommandInitializer = commandInitializer as Func<IDataParameterCollection, IOrmProvider, object, string>;
            f.CommandText = typedCommandInitializer.Invoke(f.Parameters, this.OrmProvider, whereObj);
        }, cancellationToken);
    }
    #endregion

    #region Get
    public TEntity Get<TEntity>(object whereObj) => this.dbContext.Get<TEntity>(whereObj);
    public async Task<TEntity> GetAsync<TEntity>(object whereObj, CancellationToken cancellationToken = default)
        => await this.dbContext.GetAsync<TEntity>(whereObj, cancellationToken);
    #endregion

    #region Create
    public virtual ICreate<TEntity> Create<TEntity>() => this.OrmProvider.NewCreate<TEntity>(this.dbContext);
    public int Create<TEntity>(object insertObjs, int bulkCount = 500)
    {
        if (insertObjs == null)
            throw new ArgumentNullException(nameof(insertObjs));

        using var command = this.dbContext.CreateCommand();
        int result = 0;
        bool isNeedClose = this.dbContext.IsNeedClose;
        try
        {
            var entityType = typeof(TEntity);
            bool isBulk = insertObjs is IEnumerable && insertObjs is not string && insertObjs is not IDictionary<string, object>;
            if (isBulk)
            {
                int index = 0;
                bool isFirst = true;
                var sqlBuilder = new StringBuilder();
                var entities = insertObjs as IEnumerable;
                (var headSqlSetter, var commandInitializer) = RepositoryHelper.BuildCreateMultiSqlParameters(this.DbKey, this.OrmProvider, this.MapProvider, entityType, insertObjs, null, null, true);
                headSqlSetter.Invoke(sqlBuilder);

                foreach (var insertObj in entities)
                {
                    if (index > 0) sqlBuilder.Append(',');
                    commandInitializer.Invoke(command.Parameters, this.OrmProvider, sqlBuilder, insertObj, index.ToString());
                    if (index >= bulkCount)
                    {
                        command.CommandText = sqlBuilder.ToString();
                        if (isFirst)
                        {
                            this.dbContext.Connection.Open();
                            isFirst = false;
                        }
                        result += command.ExecuteNonQuery();
                        command.Parameters.Clear();
                        sqlBuilder.Clear();
                        headSqlSetter.Invoke(sqlBuilder);
                        index = 0;
                        continue;
                    }
                    index++;
                }
                if (index > 0)
                {
                    command.CommandText = sqlBuilder.ToString();
                    if (isFirst) this.dbContext.Connection.Open();
                    result = command.ExecuteNonQuery();
                }
                sqlBuilder.Clear();
            }
            else
            {
                var commandInitializer = RepositoryHelper.BuildCreateSqlParameters(this.DbKey, this.OrmProvider, this.MapProvider, entityType, insertObjs, null, null, false);
                var typedCommandInitializer = commandInitializer as Func<IDataParameterCollection, IOrmProvider, object, string>;
                typedCommandInitializer.Invoke(command.Parameters, OrmProvider, insertObjs);
                this.Connection.Open();
                result = command.ExecuteNonQuery();
            }
        }
        catch
        {
            isNeedClose = true;
        }
        finally
        {
            command.Dispose();
            if (isNeedClose) this.Dispose();
        }
        return result;
    }
    public async Task<int> CreateAsync<TEntity>(object insertObjs, int bulkCount = 500, CancellationToken cancellationToken = default)
    {
        if (insertObjs == null)
            throw new ArgumentNullException(nameof(insertObjs));

        using var command = this.dbContext.CreateDbCommand();
        int result = 0;
        bool isNeedClose = this.dbContext.IsNeedClose;
        try
        {
            var entityType = typeof(TEntity);
            bool isBulk = insertObjs is IEnumerable && insertObjs is not string && insertObjs is not IDictionary<string, object>;
            if (isBulk)
            {
                int index = 0;
                bool isFirst = true;
                var sqlBuilder = new StringBuilder();
                var entities = insertObjs as IEnumerable;
                (var headSqlSetter, var commandInitializer) = RepositoryHelper.BuildCreateMultiSqlParameters(this.DbKey, this.OrmProvider, this.MapProvider, entityType, insertObjs, null, null, true);
                headSqlSetter.Invoke(sqlBuilder);

                foreach (var insertObj in entities)
                {
                    if (index > 0) sqlBuilder.Append(';');
                    commandInitializer.Invoke(command.Parameters, this.OrmProvider, sqlBuilder, insertObj, index.ToString());
                    if (index >= bulkCount)
                    {
                        command.CommandText = sqlBuilder.ToString();
                        if (isFirst)
                        {
                            await this.dbContext.Connection.OpenAsync(cancellationToken);
                            isFirst = false;
                        }
                        result += await command.ExecuteNonQueryAsync(cancellationToken);
                        command.Parameters.Clear();
                        sqlBuilder.Clear();
                        headSqlSetter.Invoke(sqlBuilder);
                        index = 0;
                        continue;
                    }
                    index++;
                }
                if (index > 0)
                {
                    command.CommandText = sqlBuilder.ToString();
                    if (isFirst) await this.dbContext.Connection.OpenAsync(cancellationToken);
                    result = await command.ExecuteNonQueryAsync(cancellationToken);
                }
                sqlBuilder.Clear();
            }
            else
            {
                var commandInitializer = RepositoryHelper.BuildCreateSqlParameters(this.DbKey, this.OrmProvider, this.MapProvider, entityType, insertObjs, null, null, false);
                var typedCommandInitializer = commandInitializer as Func<IDataParameterCollection, IOrmProvider, object, string>;
                typedCommandInitializer.Invoke(command.Parameters, OrmProvider, insertObjs);
                await this.dbContext.Connection.OpenAsync(cancellationToken);
                result = await command.ExecuteNonQueryAsync(cancellationToken);
            }
        }
        catch
        {
            isNeedClose = true;
        }
        finally
        {
            await command.DisposeAsync();
            if (isNeedClose) await this.DisposeAsync();
        }
        return result;
    }
    public int CreateIdentity<TEntity>(object insertObj)
    {
        if (insertObj == null)
            throw new ArgumentNullException(nameof(insertObj));
        bool isBulk = insertObj is IEnumerable && insertObj is not string && insertObj is not IDictionary<string, object>;
        if (isBulk) throw new NotSupportedException("CreateIdentity方法只支持单条数据插入，不支持批量插入返回Identity");

        return this.dbContext.CreateIdentity<int>(f =>
        {
            var entityType = typeof(TEntity);
            var commandInitializer = RepositoryHelper.BuildCreateSqlParameters(this.DbKey, this.OrmProvider, this.MapProvider, entityType, insertObj, null, null, true);
            var typedCommandInitializer = commandInitializer as Func<IDataParameterCollection, IOrmProvider, object, string>;
            f.CommandText = typedCommandInitializer.Invoke(f.Parameters, this.OrmProvider, insertObj);
        });
    }
    public async Task<int> CreateIdentityAsync<TEntity>(object insertObj, CancellationToken cancellationToken = default)
    {
        if (insertObj == null)
            throw new ArgumentNullException(nameof(insertObj));
        bool isBulk = insertObj is IEnumerable && insertObj is not string && insertObj is not IDictionary<string, object>;
        if (isBulk) throw new NotSupportedException("CreateIdentity方法只支持单条数据插入，不支持批量插入返回Identity");

        return await this.dbContext.CreateIdentityAsync<int>(f =>
        {
            var entityType = typeof(TEntity);
            var commandInitializer = RepositoryHelper.BuildCreateSqlParameters(this.DbKey, this.OrmProvider, this.MapProvider, entityType, insertObj, null, null, true);
            var typedCommandInitializer = commandInitializer as Func<IDataParameterCollection, IOrmProvider, object, string>;
            f.CommandText = typedCommandInitializer.Invoke(f.Parameters, this.OrmProvider, insertObj);
        }, cancellationToken);
    }
    public long CreateIdentityLong<TEntity>(object insertObj)
    {
        if (insertObj == null)
            throw new ArgumentNullException(nameof(insertObj));
        bool isBulk = insertObj is IEnumerable && insertObj is not string && insertObj is not IDictionary<string, object>;
        if (isBulk) throw new NotSupportedException("CreateIdentity方法只支持单条数据插入，不支持批量插入返回Identity");

        return this.dbContext.CreateIdentity<int>(f =>
        {
            var entityType = typeof(TEntity);
            var commandInitializer = RepositoryHelper.BuildCreateSqlParameters(this.DbKey, this.OrmProvider, this.MapProvider, entityType, insertObj, null, null, true);
            var typedCommandInitializer = commandInitializer as Func<IDataParameterCollection, IOrmProvider, object, string>;
            f.CommandText = typedCommandInitializer.Invoke(f.Parameters, this.OrmProvider, insertObj);
        });
    }
    public async Task<long> CreateIdentityLongAsync<TEntity>(object insertObj, CancellationToken cancellationToken = default)
    {
        if (insertObj == null)
            throw new ArgumentNullException(nameof(insertObj));
        bool isBulk = insertObj is IEnumerable && insertObj is not string && insertObj is not IDictionary<string, object>;
        if (isBulk) throw new NotSupportedException("CreateIdentity方法只支持单条数据插入，不支持批量插入返回Identity");

        return await this.dbContext.CreateIdentityAsync<long>(f =>
        {
            var entityType = typeof(TEntity);
            var commandInitializer = RepositoryHelper.BuildCreateSqlParameters(this.DbKey, this.OrmProvider, this.MapProvider, entityType, insertObj, null, null, true);
            var typedCommandInitializer = commandInitializer as Func<IDataParameterCollection, IOrmProvider, object, string>;
            f.CommandText = typedCommandInitializer.Invoke(f.Parameters, this.OrmProvider, insertObj);
        }, cancellationToken);
    }
    #endregion

    #region Update
    public virtual IUpdate<TEntity> Update<TEntity>() => this.OrmProvider.NewUpdate<TEntity>(this.dbContext);
    public int Update<TEntity>(object updateObjs, int bulkCount = 500)
    {
        if (updateObjs == null)
            throw new ArgumentNullException(nameof(updateObjs));

        using var command = this.dbContext.CreateCommand();
        int result = 0;
        bool isNeedClose = this.Transaction == null;
        try
        {
            var entityType = typeof(TEntity);
            bool isBulk = updateObjs is IEnumerable && updateObjs is not string && updateObjs is not IDictionary<string, object>;
            if (isBulk)
            {
                int index = 0;
                bool isFirst = true;
                var sqlBuilder = new StringBuilder();
                var entities = updateObjs as IEnumerable;
                var commandInitializer = RepositoryHelper.BuildUpdateMultiSqlParameters(this.DbKey, this.OrmProvider, this.MapProvider, entityType, updateObjs, null, null);
                foreach (var updateObj in entities)
                {
                    if (index > 0) sqlBuilder.Append(';');
                    commandInitializer.Invoke(command.Parameters, this.OrmProvider, sqlBuilder, updateObj, index.ToString());
                    if (index >= bulkCount)
                    {
                        command.CommandText = sqlBuilder.ToString();
                        if (isFirst)
                        {
                            this.dbContext.Connection.Open();
                            isFirst = false;
                        }
                        result += command.ExecuteNonQuery();
                        command.Parameters.Clear();
                        sqlBuilder.Clear();
                        index = 0;
                        continue;
                    }
                    index++;
                }
                if (index > 0)
                {
                    command.CommandText = sqlBuilder.ToString();
                    if (isFirst) this.dbContext.Connection.Open();
                    result += command.ExecuteNonQuery();
                }
                sqlBuilder.Clear();
            }
            else
            {
                var commandInitializer = RepositoryHelper.BuildUpdateSqlParameters(this.DbKey, this.OrmProvider, this.MapProvider, entityType, updateObjs, null, null);
                command.CommandText = commandInitializer.Invoke(command.Parameters, this.OrmProvider, updateObjs);
                this.dbContext.Connection.Open();
                result = command.ExecuteNonQuery();
            }
        }
        catch
        {
            isNeedClose = true;
        }
        finally
        {
            command.Dispose();
            if (isNeedClose) this.Dispose();
        }
        return result;
    }
    public async Task<int> UpdateAsync<TEntity>(object updateObjs, int bulkCount = 500, CancellationToken cancellationToken = default)
    {
        if (updateObjs == null)
            throw new ArgumentNullException(nameof(updateObjs));

        using var command = this.dbContext.CreateDbCommand();
        int result = 0;
        bool isNeedClose = this.dbContext.IsNeedClose;
        try
        {
            var entityType = typeof(TEntity);
            bool isBulk = updateObjs is IEnumerable && updateObjs is not string && updateObjs is not IDictionary<string, object>;
            if (isBulk)
            {
                int index = 0;
                bool isFirst = true;
                var sqlBuilder = new StringBuilder();
                var entities = updateObjs as IEnumerable;
                var commandInitializer = RepositoryHelper.BuildUpdateMultiSqlParameters(this.DbKey, this.OrmProvider, this.MapProvider, entityType, updateObjs, null, null);
                foreach (var updateObj in entities)
                {
                    if (index > 0) sqlBuilder.Append(';');
                    commandInitializer.Invoke(command.Parameters, this.OrmProvider, sqlBuilder, updateObj, index.ToString());
                    if (index >= bulkCount)
                    {
                        command.CommandText = sqlBuilder.ToString();
                        if (isFirst)
                        {
                            await this.dbContext.Connection.OpenAsync(cancellationToken);
                            isFirst = false;
                        }
                        result += await command.ExecuteNonQueryAsync(cancellationToken);
                        command.Parameters.Clear();
                        sqlBuilder.Clear();
                        index = 0;
                        continue;
                    }
                    index++;
                }
                if (index > 0)
                {
                    command.CommandText = sqlBuilder.ToString();
                    if (isFirst) await this.dbContext.Connection.OpenAsync(cancellationToken);
                    result += await command.ExecuteNonQueryAsync(cancellationToken);
                }
                sqlBuilder.Clear();
            }
            else
            {
                var commandInitializer = RepositoryHelper.BuildUpdateSqlParameters(this.DbKey, this.OrmProvider, this.MapProvider, entityType, updateObjs, null, null);
                command.CommandText = commandInitializer.Invoke(command.Parameters, this.OrmProvider, updateObjs);
                await this.dbContext.Connection.OpenAsync(cancellationToken);
                result = await command.ExecuteNonQueryAsync(cancellationToken);
            }
        }
        catch
        {
            isNeedClose = true;
        }
        finally
        {
            await command.DisposeAsync();
            if (isNeedClose) await this.DisposeAsync();
        }
        return result;
    }
    #endregion

    #region Delete
    public virtual IDelete<TEntity> Delete<TEntity>() => this.OrmProvider.NewDelete<TEntity>(this.dbContext);
    public int Delete<TEntity>(object whereKeys)
    {
        if (whereKeys == null)
            throw new ArgumentNullException(nameof(whereKeys));

        using var command = this.dbContext.CreateCommand();
        int result = 0;
        bool isNeedClose = this.dbContext.IsNeedClose;
        try
        {
            var entityType = typeof(TEntity);
            bool isBulk = whereKeys is IEnumerable && whereKeys is not string && whereKeys is not IDictionary<string, object>;
            if (isBulk)
            {
                int index = 0;
                var sqlBuilder = new StringBuilder();
                var entities = whereKeys as IEnumerable;
                (var isMultiKeys, var headSql, var commandInitializer) = RepositoryHelper.BuildDeleteBulkCommandInitializer(this.DbKey, this.OrmProvider, this.MapProvider, entityType, whereKeys, false);
                var typedCommandInitializer = commandInitializer as Action<IDataParameterCollection, IOrmProvider, StringBuilder, object, int>;
                string separator = null;
                if (isMultiKeys) separator = ";";
                else
                {
                    separator = ",";
                    sqlBuilder.Append(headSql);
                }
                foreach (var entity in entities)
                {
                    if (index > 0) sqlBuilder.Append(separator);
                    typedCommandInitializer.Invoke(command.Parameters, this.OrmProvider, sqlBuilder, entity, index);
                    index++;
                }
                command.CommandText = sqlBuilder.ToString();
                sqlBuilder.Clear();
            }
            else
            {
                var commandInitializer = RepositoryHelper.BuildDeleteCommandInitializer(this.DbKey, this.OrmProvider, this.MapProvider, entityType, whereKeys, false);
                var typedCommandInitializer = commandInitializer as Func<IDataParameterCollection, IOrmProvider, object, string>;
                command.CommandText = typedCommandInitializer.Invoke(command.Parameters, this.OrmProvider, whereKeys);
            }
            this.dbContext.Connection.Open();
            result = command.ExecuteNonQuery();
        }
        finally
        {
            command.Dispose();
            if (isNeedClose) this.Dispose();
        }
        return result;
    }
    public async Task<int> DeleteAsync<TEntity>(object whereKeys, CancellationToken cancellationToken = default)
    {
        if (whereKeys == null)
            throw new ArgumentNullException(nameof(whereKeys));

        using var command = this.dbContext.CreateDbCommand();
        int result = 0;
        bool isNeedClose = this.dbContext.IsNeedClose;
        try
        {
            var entityType = typeof(TEntity);
            bool isBulk = whereKeys is IEnumerable && whereKeys is not string && whereKeys is not IDictionary<string, object>;
            if (isBulk)
            {
                int index = 0;
                var sqlBuilder = new StringBuilder();
                var entities = whereKeys as IEnumerable;
                (var isMultiKeys, var headSql, var commandInitializer) = RepositoryHelper.BuildDeleteBulkCommandInitializer(this.DbKey, this.OrmProvider, this.MapProvider, entityType, whereKeys, false);
                var typedCommandInitializer = commandInitializer as Action<IDataParameterCollection, IOrmProvider, StringBuilder, object, int>;
                string separator = null;
                if (isMultiKeys) separator = ";";
                else
                {
                    separator = ",";
                    sqlBuilder.Append(headSql);
                }
                foreach (var entity in entities)
                {
                    if (index > 0) sqlBuilder.Append(separator);
                    typedCommandInitializer.Invoke(command.Parameters, this.OrmProvider, sqlBuilder, entity, index);
                    index++;
                }
                command.CommandText = sqlBuilder.ToString();
                sqlBuilder.Clear();
            }
            else
            {
                var commandInitializer = RepositoryHelper.BuildDeleteCommandInitializer(this.DbKey, this.OrmProvider, this.MapProvider, entityType, whereKeys, false);
                var typedCommandInitializer = commandInitializer as Func<IDataParameterCollection, IOrmProvider, object, string>;
                command.CommandText = typedCommandInitializer.Invoke(command.Parameters, this.OrmProvider, whereKeys);
            }
            await this.dbContext.Connection.OpenAsync(cancellationToken);
            result = await command.ExecuteNonQueryAsync(cancellationToken);
        }
        finally
        {
            await command.DisposeAsync();
            if (isNeedClose) await this.DisposeAsync();
        }
        return result;
    }
    #endregion

    #region Exists
    public bool Exists<TEntity>(object whereObj)
    {
        if (whereObj == null)
            throw new ArgumentNullException(nameof(whereObj));

        var result = this.dbContext.QueryFirst<int>(f =>
        {
            var entityType = typeof(TEntity);
            var commandInitializer = RepositoryHelper.BuildQueryWhereObjSqlParameters(this.DbKey, this.OrmProvider, this.MapProvider, entityType, whereObj, false);
            var typedCommandInitializer = commandInitializer as Func<IDataParameterCollection, IOrmProvider, object, string>;
            f.CommandText = typedCommandInitializer.Invoke(f.Parameters, this.OrmProvider, whereObj);
        });
        return result > 0;
    }
    public async Task<bool> ExistsAsync<TEntity>(object whereObj, CancellationToken cancellationToken = default)
    {
        if (whereObj == null)
            throw new ArgumentNullException(nameof(whereObj));

        var result = await this.dbContext.QueryFirstAsync<int>(f =>
        {
            var entityType = typeof(TEntity);
            var commandInitializer = RepositoryHelper.BuildQueryWhereObjSqlParameters(this.DbKey, this.OrmProvider, this.MapProvider, entityType, whereObj, false);
            var typedCommandInitializer = commandInitializer as Func<IDataParameterCollection, IOrmProvider, object, string>;
            f.CommandText = typedCommandInitializer.Invoke(f.Parameters, this.OrmProvider, whereObj);
        }, cancellationToken);
        return result > 0;
    }
    public bool Exists<TEntity>(Expression<Func<TEntity, bool>> wherePredicate)
        => this.From<TEntity>().Where(wherePredicate).Count() > 0;
    public async Task<bool> ExistsAsync<TEntity>(Expression<Func<TEntity, bool>> wherePredicate, CancellationToken cancellationToken = default)
        => await this.From<TEntity>().Where(wherePredicate).CountAsync() > 0;
    #endregion

    #region Execute
    public int Execute(string rawSql, object parameters = null)
    {
        if (string.IsNullOrEmpty(rawSql))
            throw new ArgumentNullException(nameof(rawSql));

        return this.dbContext.Execute(f =>
        {
            f.CommandText = rawSql;
            if (parameters != null)
            {
                var commandInitializer = RepositoryHelper.BuildQueryRawSqlParameters(this.DbKey, this.OrmProvider, rawSql, parameters);
                commandInitializer.Invoke(f.Parameters, this.OrmProvider, parameters);
            }
        });
    }
    public async Task<int> ExecuteAsync(string rawSql, object parameters = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(rawSql))
            throw new ArgumentNullException(nameof(rawSql));

        return await this.dbContext.ExecuteAsync(f =>
        {
            f.CommandText = rawSql;
            if (parameters != null)
            {
                var commandInitializer = RepositoryHelper.BuildQueryRawSqlParameters(this.DbKey, this.OrmProvider, rawSql, parameters);
                commandInitializer.Invoke(f.Parameters, this.OrmProvider, parameters);
            }
        }, cancellationToken);
    }
    #endregion

    #region QueryMultiple
    public IMultiQueryReader QueryMultiple(Action<IMultipleQuery> subQueries)
    {
        if (subQueries == null)
            throw new ArgumentNullException(nameof(subQueries));

        using var command = this.dbContext.CreateCommand();
        IMultiQueryReader result = null;
        IDataReader reader = null;
        bool isNeedClose = this.dbContext.IsNeedClose;
        try
        {
            using var multiQuery = new MultipleQuery(this.dbContext, command);
            subQueries.Invoke(multiQuery);
            command.CommandText = multiQuery.BuildSql(out var readerAfters);
            this.dbContext.Connection.Open();
            reader = command.ExecuteReader(CommandBehavior.SequentialAccess);
            result = new MultiQueryReader(command, reader, readerAfters, isNeedClose);
        }
        catch
        {
            isNeedClose = true;
        }
        finally
        {
            reader?.Dispose();
            command.Dispose();
            if (isNeedClose) this.Dispose();
        }
        return result;
    }
    public async Task<IMultiQueryReader> QueryMultipleAsync(Action<IMultipleQuery> subQueries, CancellationToken cancellationToken = default)
    {
        if (subQueries == null)
            throw new ArgumentNullException(nameof(subQueries));

        using var command = this.dbContext.CreateDbCommand();
        IMultiQueryReader result = null;
        DbDataReader reader = null;
        bool isNeedClose = this.dbContext.IsNeedClose;
        try
        {
            using var multiQuery = new MultipleQuery(this.dbContext, command);
            subQueries.Invoke(multiQuery);
            command.CommandText = multiQuery.BuildSql(out var readerAfters);
            await this.dbContext.Connection.OpenAsync(cancellationToken);
            reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess, cancellationToken);
            result = new MultiQueryReader(command, reader, readerAfters, isNeedClose);
        }
        catch
        {
            isNeedClose = true;
        }
        finally
        {
            if (reader != null)
                await reader.DisposeAsync();
            await command.DisposeAsync();
            if (isNeedClose) await this.DisposeAsync();
        }
        return null;
    }
    #endregion

    #region MultipleExecute
    public void MultipleExecute(List<MultipleCommand> commands)
    {
        if (commands == null || commands.Count == 0)
            throw new ArgumentNullException(nameof(commands));

        using var command = this.dbContext.CreateCommand();
        bool isNeedClose = this.dbContext.IsNeedClose;
        try
        {
            int commandIndex = 0;
            var sqlBuilder = new StringBuilder();
            var visitors = new Dictionary<MultipleCommandType, object>();
            foreach (var multiCcommand in commands)
            {
                bool isFirst = false;
                if (!visitors.TryGetValue(multiCcommand.CommandType, out var visitor))
                {
                    visitor = multiCcommand.CommandType switch
                    {
                        MultipleCommandType.Insert => this.OrmProvider.NewCreateVisitor(this.DbKey, this.MapProvider, this.isParameterized),
                        MultipleCommandType.Update => this.OrmProvider.NewUpdateVisitor(this.DbKey, this.MapProvider, this.isParameterized),
                        MultipleCommandType.Delete => this.OrmProvider.NewDeleteVisitor(this.DbKey, this.MapProvider, this.isParameterized),
                        _ => this.OrmProvider.NewUpdateVisitor(this.DbKey, this.MapProvider, this.isParameterized)
                    };
                    visitors.Add(multiCcommand.CommandType, visitor);
                    isFirst = true;
                }
                switch (multiCcommand.CommandType)
                {
                    case MultipleCommandType.Insert:
                        var insertVisitor = visitor as ICreateVisitor;
                        insertVisitor.Initialize(multiCcommand.EntityType, isFirst);
                        insertVisitor.BuildMultiCommand(command, sqlBuilder, multiCcommand, commandIndex);
                        break;
                    case MultipleCommandType.Update:
                        var updateVisitor = visitor as IUpdateVisitor;
                        updateVisitor.Initialize(multiCcommand.EntityType, isFirst);
                        updateVisitor.BuildMultiCommand(command, sqlBuilder, multiCcommand, commandIndex);
                        break;
                    case MultipleCommandType.Delete:
                        var deleteVisitor = visitor as IDeleteVisitor;
                        deleteVisitor.Initialize(multiCcommand.EntityType, isFirst);
                        deleteVisitor.BuildMultiCommand(command, sqlBuilder, multiCcommand, commandIndex);
                        break;
                }
                commandIndex++;
            }
            command.CommandText = sqlBuilder.ToString();
            this.dbContext.Connection.Open();
            var result = command.ExecuteNonQuery();
        }
        catch
        {
            isNeedClose = true;
        }
        finally
        {
            command.Dispose();
            if (isNeedClose) this.Dispose();
        }
    }
    public async Task MultipleExecuteAsync(List<MultipleCommand> commands, CancellationToken cancellationToken = default)
    {
        if (commands == null || commands.Count == 0)
            throw new ArgumentNullException(nameof(commands));

        using var command = this.dbContext.CreateDbCommand();
        bool isNeedClose = this.dbContext.IsNeedClose;
        try
        {
            int commandIndex = 0;
            var sqlBuilder = new StringBuilder();
            var visitors = new Dictionary<MultipleCommandType, object>();
            foreach (var multiCcommand in commands)
            {
                bool isFirst = false;
                if (!visitors.TryGetValue(multiCcommand.CommandType, out var visitor))
                {
                    visitor = multiCcommand.CommandType switch
                    {
                        MultipleCommandType.Insert => this.OrmProvider.NewCreateVisitor(this.DbKey, this.MapProvider, this.isParameterized),
                        MultipleCommandType.Update => this.OrmProvider.NewUpdateVisitor(this.DbKey, this.MapProvider, this.isParameterized),
                        MultipleCommandType.Delete => this.OrmProvider.NewDeleteVisitor(this.DbKey, this.MapProvider, this.isParameterized),
                        _ => this.OrmProvider.NewUpdateVisitor(this.DbKey, this.MapProvider, this.isParameterized)
                    };
                    visitors.Add(multiCcommand.CommandType, visitor);
                    isFirst = true;
                }
                switch (multiCcommand.CommandType)
                {
                    case MultipleCommandType.Insert:
                        var insertVisitor = visitor as ICreateVisitor;
                        insertVisitor.Initialize(multiCcommand.EntityType, isFirst);
                        insertVisitor.BuildMultiCommand(command, sqlBuilder, multiCcommand, commandIndex);
                        break;
                    case MultipleCommandType.Update:
                        var updateVisitor = visitor as IUpdateVisitor;
                        updateVisitor.Initialize(multiCcommand.EntityType, isFirst);
                        updateVisitor.BuildMultiCommand(command, sqlBuilder, multiCcommand, commandIndex);
                        break;
                    case MultipleCommandType.Delete:
                        var deleteVisitor = visitor as IDeleteVisitor;
                        deleteVisitor.Initialize(multiCcommand.EntityType, isFirst);
                        deleteVisitor.BuildMultiCommand(command, sqlBuilder, multiCcommand, commandIndex);
                        break;
                }
                commandIndex++;
            }
            command.CommandText = sqlBuilder.ToString();
            await this.dbContext.Connection.OpenAsync(cancellationToken);
            var result = await command.ExecuteNonQueryAsync(cancellationToken);
        }
        catch
        {
            isNeedClose = true;
        }
        finally
        {
            await command.DisposeAsync();
            if (isNeedClose) await this.DisposeAsync();
        }
    }
    #endregion

    #region Others
    public void Close() => this.Dispose();
    public async Task CloseAsync() => await this.DisposeAsync();
    public IRepository Timeout(int timeout)
    {
        this.dbContext.Connection.CommandTimeout = timeout;
        return this;
    }
    public IRepository WithParameterized(bool isParameterized = true)
    {
        this.dbContext.IsParameterized = isParameterized;
        return this;
    }
    public IRepository With(OrmDbFactoryOptions options)
    {
        if (options == null) return this;
        this.dbContext.IsParameterized = options.IsParameterized;
        this.dbContext.Connection.CommandTimeout = options.Timeout;
        return this;
    }
    public void BeginTransaction()
    {
        this.dbContext.Connection.Open();
        this.dbContext.Transaction = this.dbContext.Connection.BeginTransaction();
    }
    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        await this.dbContext.Connection.OpenAsync(cancellationToken);
        this.dbContext.Transaction = await this.dbContext.Connection.BeginTransactionAsync(cancellationToken);
    }
    public void Commit()
    {
        this.Transaction?.Commit();
        this.Dispose();
    }
    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        if (this.Transaction != null)
        {
            if (this.Transaction is not DbTransaction dbTransaction)
                throw new NotSupportedException("当前数据库驱动不支持异步操作");
            await dbTransaction.CommitAsync(cancellationToken);
        }
        await this.DisposeAsync();
    }
    public void Rollback()
    {
        this.Transaction?.Rollback();
        this.Dispose();
    }
    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        if (this.Transaction != null)
        {
            if (this.Transaction is not DbTransaction dbTransaction)
                throw new NotSupportedException("当前数据库驱动不支持异步操作");
            await dbTransaction.RollbackAsync(cancellationToken);
        }
        await this.DisposeAsync();
    }
    public void Dispose()
    {
        this.dbContext.Dispose();
        GC.SuppressFinalize(this);
    }
    public async ValueTask DisposeAsync()
    {
        await this.dbContext.DisposeAsync();
        GC.SuppressFinalize(this);
    }
    ~Repository() => this.Dispose();
    private IQueryVisitor CreateQueryVisitor(char tableAsStart, bool isCteQuery = false)
    {
        var visitor = this.OrmProvider.NewQueryVisitor(this.DbKey, this.MapProvider, this.isParameterized, tableAsStart);
        if (isCteQuery)
        {
            visitor.CteTables = new();
            visitor.CteQueries = new();
            visitor.CteTableSegments = new();
        }
        return visitor;
    }
    #endregion
}