﻿using System;
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
    protected string dbKey => this.DbContext.DbKey;
    protected IOrmProvider ormProvider => this.DbContext.OrmProvider;
    protected IEntityMapProvider mapProvider => this.DbContext.MapProvider;
    protected IShardingProvider shardingProvider => this.DbContext.ShardingProvider;
    protected bool isParameterized => this.DbContext.IsParameterized;
    #endregion

    #region Properties
    public virtual DbContext DbContext { get; set; }
    #endregion

    #region Constructor
    public Repository() { }
    public Repository(DbContext dbContext) => this.DbContext = dbContext;
    #endregion

    #region GetShardingTableNames
    public virtual List<string> GetShardingTableNames(params Type[] entityTypes)
    {
        if (entityTypes == null)
            throw new ArgumentNullException(nameof(entityTypes));
        var tableSchema = this.DbContext.Connection.Database;
        var builder = new StringBuilder($"SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE' AND TABLE_SCHEMA='{tableSchema}' AND ");

        if (entityTypes.Length > 1)
        {
            builder.Append('(');
            int index = 0;
            Array.ForEach(entityTypes, f =>
            {
                var entityMapper = this.mapProvider.GetEntityMap(f);
                if (index > 0) builder.Append(" OR ");
                builder.Append($"TABLE_NAME LIKE '{entityMapper.TableName}%'");
                index++;
            });
            builder.Append(')');
        }
        else
        {
            var entityMapper = this.mapProvider.GetEntityMap(entityTypes[0]);
            builder.Append($"TABLE_NAME LIKE '{entityMapper.TableName}%'");
        }
        var sql = builder.ToString();
        return this.DbContext.Query<string>(f => f.CommandText = sql);
    }
    public virtual async Task<List<string>> GetShardingTableNamesAsync(params Type[] entityTypes)
    {
        if (entityTypes == null)
            throw new ArgumentNullException(nameof(entityTypes));
        var tableSchema = this.DbContext.Connection.Database;
        var builder = new StringBuilder($"SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE' AND TABLE_SCHEMA='{tableSchema}' AND ");

        if (entityTypes.Length > 1)
        {
            builder.Append('(');
            int index = 0;
            Array.ForEach(entityTypes, f =>
            {
                var entityMapper = this.mapProvider.GetEntityMap(f);
                if (index > 0) builder.Append(" OR ");
                builder.Append($"TABLE_NAME LIKE '{entityMapper.TableName}%'");
                index++;
            });
            builder.Append(')');
        }
        else
        {
            var entityMapper = this.mapProvider.GetEntityMap(entityTypes[0]);
            builder.Append($"TABLE_NAME LIKE '{entityMapper.TableName}%'");
        }
        var sql = builder.ToString();
        return await this.DbContext.QueryAsync<string>(f => f.CommandText = sql);
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

    #region From SubQuery
    public virtual IQuery<T> From<T>(IQuery<T> subQuery)
    {
        var visitor = this.CreateQueryVisitor();
        visitor.From(typeof(T), subQuery);
        return this.ormProvider.NewQuery<T>(this.DbContext, visitor);
    }
    public virtual IQuery<T> From<T>(Func<IFromQuery, IQuery<T>> subQuery)
    {
        var visitor = this.CreateQueryVisitor();
        visitor.From(typeof(T), this.DbContext, subQuery);
        return this.ormProvider.NewQuery<T>(this.DbContext, visitor);
    }
    #endregion

    #region QueryFirst/Query
    public virtual TEntity QueryFirst<TEntity>(string rawSql, object parameters = null)
    {
        if (string.IsNullOrEmpty(rawSql))
            throw new ArgumentNullException(nameof(rawSql));
        if (parameters != null)
        {
            var whereObjType = parameters.GetType();
            if (!whereObjType.IsEntityType(out _))
                throw new NotSupportedException("不支持的参数类型，QueryFirst方法的parameters参数，支持实体类型参数，命名、匿名对象或是字典对象");
        }

        return this.DbContext.QueryFirst<TEntity>(f =>
        {
            f.CommandText = rawSql;
            if (parameters != null)
            {
                var commandInitializer = RepositoryHelper.BuildQueryRawSqlParameters(this.ormProvider, rawSql, parameters);
                commandInitializer.Invoke(f.Parameters, this.ormProvider, parameters);
            }
        });
    }
    public virtual async Task<TEntity> QueryFirstAsync<TEntity>(string rawSql, object parameters = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(rawSql))
            throw new ArgumentNullException(nameof(rawSql));
        if (parameters != null)
        {
            var whereObjType = parameters.GetType();
            if (!whereObjType.IsEntityType(out _))
                throw new NotSupportedException("不支持的参数类型，QueryFirstAsync方法的parameters参数，支持实体类型参数，命名、匿名对象或是字典对象");
        }

        return await this.DbContext.QueryFirstAsync<TEntity>(f =>
        {
            f.CommandText = rawSql;
            if (parameters != null)
            {
                var commandInitializer = RepositoryHelper.BuildQueryRawSqlParameters(this.ormProvider, rawSql, parameters);
                commandInitializer.Invoke(f.Parameters, this.ormProvider, parameters);
            }
        }, cancellationToken);
    }
    public virtual TEntity QueryFirst<TEntity>(object whereObj)
    {
        if (whereObj == null)
            throw new ArgumentNullException(nameof(whereObj));
        var whereObjType = whereObj.GetType();
        if (!whereObjType.IsEntityType(out _))
            throw new NotSupportedException("不支持的参数类型，QueryFirst方法的whereObj参数，支持实体类型参数，命名、匿名对象或是字典对象");

        return this.DbContext.QueryFirst<TEntity>(f =>
        {
            var entityType = typeof(TEntity);
            var commandInitializer = RepositoryHelper.BuildQueryWhereObjSqlParameters(this.ormProvider, this.mapProvider, entityType, whereObj, false);
            var typedCommandInitializer = commandInitializer as Func<IDataParameterCollection, IOrmProvider, object, string>;
            f.CommandText = typedCommandInitializer.Invoke(f.Parameters, this.ormProvider, whereObj);
        });
    }
    public virtual async Task<TEntity> QueryFirstAsync<TEntity>(object whereObj, CancellationToken cancellationToken = default)
    {
        if (whereObj == null)
            throw new ArgumentNullException(nameof(whereObj));
        var whereObjType = whereObj.GetType();
        if (!whereObjType.IsEntityType(out _))
            throw new NotSupportedException("不支持的参数类型，QueryFirstAsync方法的whereObj参数，支持实体类型参数，命名、匿名对象或是字典对象");

        return await this.DbContext.QueryFirstAsync<TEntity>(f =>
        {
            var entityType = typeof(TEntity);
            var commandInitializer = RepositoryHelper.BuildQueryWhereObjSqlParameters(this.ormProvider, this.mapProvider, entityType, whereObj, false);
            var typedCommandInitializer = commandInitializer as Func<IDataParameterCollection, IOrmProvider, object, string>;
            f.CommandText = typedCommandInitializer.Invoke(f.Parameters, this.ormProvider, whereObj);
        }, cancellationToken);
    }
    public virtual List<TEntity> Query<TEntity>(string rawSql, object parameters = null)
    {
        if (string.IsNullOrEmpty(rawSql))
            throw new ArgumentNullException(nameof(rawSql));
        if (parameters != null)
        {
            var whereObjType = parameters.GetType();
            if (!whereObjType.IsEntityType(out _))
                throw new NotSupportedException("不支持的参数类型，Query方法的parameters参数，支持实体类型参数，命名、匿名对象或是字典对象");
        }

        return this.DbContext.Query<TEntity>(f =>
        {
            f.CommandText = rawSql;
            if (parameters != null)
            {
                var commandInitializer = RepositoryHelper.BuildQueryRawSqlParameters(this.ormProvider, rawSql, parameters);
                commandInitializer.Invoke(f.Parameters, this.ormProvider, parameters);
            }
        });
    }
    public virtual async Task<List<TEntity>> QueryAsync<TEntity>(string rawSql, object parameters = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(rawSql))
            throw new ArgumentNullException(nameof(rawSql));
        if (parameters != null)
        {
            var whereObjType = parameters.GetType();
            if (!whereObjType.IsEntityType(out _))
                throw new NotSupportedException("不支持的参数类型，QueryAsync方法的parameters参数，支持实体类型参数，命名、匿名对象或是字典对象");
        }

        return await this.DbContext.QueryAsync<TEntity>(f =>
        {
            f.CommandText = rawSql;
            if (parameters != null)
            {
                var commandInitializer = RepositoryHelper.BuildQueryRawSqlParameters(this.ormProvider, rawSql, parameters);
                commandInitializer.Invoke(f.Parameters, this.ormProvider, parameters);
            }
        }, cancellationToken);
    }
    public virtual List<TEntity> Query<TEntity>(object whereObj)
    {
        if (whereObj == null)
            throw new ArgumentNullException(nameof(whereObj));
        var whereObjType = whereObj.GetType();
        if (!whereObjType.IsEntityType(out _))
            throw new NotSupportedException("不支持的参数类型，Query方法的whereObj参数，支持实体类型参数，命名、匿名对象或是字典对象");

        return this.DbContext.Query<TEntity>(f =>
        {
            var entityType = typeof(TEntity);
            var commandInitializer = RepositoryHelper.BuildQueryWhereObjSqlParameters(this.ormProvider, this.mapProvider, entityType, whereObj, false);
            var typedCommandInitializer = commandInitializer as Func<IDataParameterCollection, IOrmProvider, object, string>;
            f.CommandText = typedCommandInitializer.Invoke(f.Parameters, this.ormProvider, whereObj);
        });
    }
    public virtual async Task<List<TEntity>> QueryAsync<TEntity>(object whereObj, CancellationToken cancellationToken = default)
    {
        if (whereObj == null)
            throw new ArgumentNullException(nameof(whereObj));
        var whereObjType = whereObj.GetType();
        if (!whereObjType.IsEntityType(out _))
            throw new NotSupportedException("不支持的参数类型，QueryAsync方法的whereObj参数，支持实体类型参数，命名、匿名对象或是字典对象");

        return await this.DbContext.QueryAsync<TEntity>(f =>
        {
            var entityType = typeof(TEntity);
            var commandInitializer = RepositoryHelper.BuildQueryWhereObjSqlParameters(this.ormProvider, this.mapProvider, entityType, whereObj, false);
            var typedCommandInitializer = commandInitializer as Func<IDataParameterCollection, IOrmProvider, object, string>;
            f.CommandText = typedCommandInitializer.Invoke(f.Parameters, this.ormProvider, whereObj);
        }, cancellationToken);
    }
    #endregion

    #region Get
    public virtual TEntity Get<TEntity>(object whereObj) => this.DbContext.Get<TEntity>(whereObj);
    public virtual async Task<TEntity> GetAsync<TEntity>(object whereObj, CancellationToken cancellationToken = default)
        => await this.DbContext.GetAsync<TEntity>(whereObj, cancellationToken);
    #endregion

    #region Create
    public virtual ICreate<TEntity> Create<TEntity>() => this.ormProvider.NewCreate<TEntity>(this.DbContext);
    public virtual int Create<TEntity>(object insertObjs, int bulkCount = 500)
    {
        if (insertObjs == null)
            throw new ArgumentNullException(nameof(insertObjs));

        using var command = this.DbContext.CreateCommand();
        int result = 0;
        bool isNeedClose = this.DbContext.IsNeedClose;
        Exception exception = null;
        try
        {
            var entityType = typeof(TEntity);
            bool isBulk = insertObjs is IEnumerable && insertObjs is not string && insertObjs is not IDictionary<string, object>;
            if (isBulk)
            {
                var sqlBuilder = new StringBuilder();
                var entities = insertObjs as IEnumerable;
                object firstInsertObj = null;
                Type insertObjType = null;
                foreach (var entity in entities)
                {
                    firstInsertObj = entity;
                    break;
                }
                insertObjType = firstInsertObj.GetType();

                (var tableName, var headSqlSetter, var valuesSqlSetter) = RepositoryHelper.BuildCreateBulkSqlParameters(
                    this.ormProvider, this.mapProvider, entityType, insertObjType, null, null);

                Func<string, IEnumerable, int> executor = (tableName, insertObjs) =>
                {
                    var isFirst = true;
                    int count = 0, index = 0;
                    foreach (var insertObj in insertObjs)
                    {
                        if (index > 0) sqlBuilder.Append(',');
                        valuesSqlSetter.Invoke(command.Parameters, sqlBuilder, insertObj, index.ToString());
                        if (index >= bulkCount)
                        {
                            command.CommandText = sqlBuilder.ToString();
                            if (isFirst)
                            {
                                this.DbContext.Open();
                                isFirst = false;
                            }
                            count += command.ExecuteNonQuery();
                            sqlBuilder.Clear();
                            command.Parameters.Clear();
                            headSqlSetter.Invoke(command.Parameters, sqlBuilder, tableName, insertObj);
                            index = 0;
                            continue;
                        }
                        index++;
                    }
                    if (index > 0)
                    {
                        command.CommandText = sqlBuilder.ToString();
                        if (isFirst) this.DbContext.Open();
                        count += command.ExecuteNonQuery();
                    }
                    return count;
                };

                if (this.shardingProvider.TryGetShardingTable(entityType, out _))
                {
                    var tabledInsertObjs = this.DbContext.SplitShardingParameters(entityType, entities);
                    foreach (var tabledInsertObj in tabledInsertObjs)
                    {
                        headSqlSetter.Invoke(command.Parameters, sqlBuilder, tabledInsertObj.Key, tabledInsertObj);
                        result += executor.Invoke(tabledInsertObj.Key, tabledInsertObj.Value);
                        sqlBuilder.Clear();
                        command.Parameters.Clear();
                    }
                }
                else
                {
                    headSqlSetter.Invoke(command.Parameters, sqlBuilder, tableName, firstInsertObj);
                    result = executor.Invoke(tableName, entities);
                }
                sqlBuilder.Clear();
                sqlBuilder = null;
            }
            else result = this.DbContext.Create(command, entityType, insertObjs);
        }
        catch (Exception ex)
        {
            isNeedClose = true;
            exception = ex;
        }
        finally
        {
            command.Dispose();
            if (isNeedClose) this.Dispose();
        }
        if (exception != null) throw exception;
        return result;
    }
    public virtual async Task<int> CreateAsync<TEntity>(object insertObjs, int bulkCount = 500, CancellationToken cancellationToken = default)
    {
        if (insertObjs == null)
            throw new ArgumentNullException(nameof(insertObjs));

        using var command = this.DbContext.CreateDbCommand();
        int result = 0;
        bool isNeedClose = this.DbContext.IsNeedClose;
        Exception exception = null;
        try
        {
            var entityType = typeof(TEntity);
            bool isBulk = insertObjs is IEnumerable && insertObjs is not string && insertObjs is not IDictionary<string, object>;
            if (isBulk)
            {
                var sqlBuilder = new StringBuilder();
                var entities = insertObjs as IEnumerable;
                object firstInsertObj = null;
                Type insertObjType = null;
                foreach (var entity in entities)
                {
                    firstInsertObj = entity;
                    break;
                }
                insertObjType = firstInsertObj.GetType();

                (var tableName, var headSqlSetter, var valuesSqlSetter) = RepositoryHelper.BuildCreateBulkSqlParameters(
                    this.ormProvider, this.mapProvider, entityType, insertObjType, null, null);

                Func<string, IEnumerable, Task<int>> executor = async (tableName, insertObjs) =>
                {
                    var isFirst = true;
                    int count = 0, index = 0;
                    foreach (var insertObj in insertObjs)
                    {
                        if (index > 0) sqlBuilder.Append(',');
                        valuesSqlSetter.Invoke(command.Parameters, sqlBuilder, insertObj, index.ToString());
                        if (index >= bulkCount)
                        {
                            command.CommandText = sqlBuilder.ToString();
                            if (isFirst)
                            {
                                await this.DbContext.OpenAsync(cancellationToken);
                                isFirst = false;
                            }
                            count += await command.ExecuteNonQueryAsync(cancellationToken);
                            sqlBuilder.Clear();
                            command.Parameters.Clear();
                            headSqlSetter.Invoke(command.Parameters, sqlBuilder, tableName, insertObj);
                            index = 0;
                            continue;
                        }
                        index++;
                    }
                    if (index > 0)
                    {
                        command.CommandText = sqlBuilder.ToString();
                        if (isFirst) await this.DbContext.OpenAsync(cancellationToken);
                        count += await command.ExecuteNonQueryAsync(cancellationToken);
                    }
                    return count;
                };
                if (this.shardingProvider.TryGetShardingTable(entityType, out _))
                {
                    var tabledInsertObjs = this.DbContext.SplitShardingParameters(entityType, entities);
                    foreach (var tabledInsertObj in tabledInsertObjs)
                    {
                        headSqlSetter.Invoke(command.Parameters, sqlBuilder, tabledInsertObj.Key, tabledInsertObj);
                        result += await executor.Invoke(tabledInsertObj.Key, tabledInsertObj.Value);
                        sqlBuilder.Clear();
                        command.Parameters.Clear();
                    }
                }
                else
                {
                    headSqlSetter.Invoke(command.Parameters, sqlBuilder, tableName, firstInsertObj);
                    result = await executor.Invoke(tableName, entities);
                }
                sqlBuilder.Clear();
                sqlBuilder = null;
            }
            else result = await this.DbContext.CreateAsync(command, entityType, insertObjs, cancellationToken);
        }
        catch (Exception ex)
        {
            isNeedClose = true;
            exception = ex;
        }
        finally
        {
            await command.DisposeAsync();
            if (isNeedClose) await this.DisposeAsync();
        }
        if (exception != null) throw exception;
        return result;
    }
    public virtual int CreateIdentity<TEntity>(object insertObj)
    {
        if (insertObj == null)
            throw new ArgumentNullException(nameof(insertObj));
        bool isBulk = insertObj is IEnumerable && insertObj is not string && insertObj is not IDictionary<string, object>;
        if (isBulk) throw new NotSupportedException("CreateIdentity方法只支持单条数据插入，不支持批量插入返回Identity");
        return this.DbContext.CreateIdentity<int>(typeof(TEntity), insertObj);
    }
    public virtual async Task<int> CreateIdentityAsync<TEntity>(object insertObj, CancellationToken cancellationToken = default)
    {
        if (insertObj == null)
            throw new ArgumentNullException(nameof(insertObj));
        bool isBulk = insertObj is IEnumerable && insertObj is not string && insertObj is not IDictionary<string, object>;
        if (isBulk) throw new NotSupportedException("CreateIdentity方法只支持单条数据插入，不支持批量插入返回Identity");
        return await this.DbContext.CreateIdentityAsync<int>(typeof(TEntity), insertObj, cancellationToken);
    }
    public virtual long CreateIdentityLong<TEntity>(object insertObj)
    {
        if (insertObj == null)
            throw new ArgumentNullException(nameof(insertObj));
        bool isBulk = insertObj is IEnumerable && insertObj is not string && insertObj is not IDictionary<string, object>;
        if (isBulk) throw new NotSupportedException("CreateIdentity方法只支持单条数据插入，不支持批量插入返回Identity");
        return this.DbContext.CreateIdentity<long>(typeof(TEntity), insertObj);
    }
    public virtual async Task<long> CreateIdentityLongAsync<TEntity>(object insertObj, CancellationToken cancellationToken = default)
    {
        if (insertObj == null)
            throw new ArgumentNullException(nameof(insertObj));
        bool isBulk = insertObj is IEnumerable && insertObj is not string && insertObj is not IDictionary<string, object>;
        if (isBulk) throw new NotSupportedException("CreateIdentity方法只支持单条数据插入，不支持批量插入返回Identity");

        return await this.DbContext.CreateIdentityAsync<long>(typeof(TEntity), insertObj, cancellationToken);
    }
    #endregion

    #region Update
    public virtual IUpdate<TEntity> Update<TEntity>() => this.ormProvider.NewUpdate<TEntity>(this.DbContext);
    public virtual int Update<TEntity>(object updateObjs, int bulkCount = 500)
    {
        if (updateObjs == null)
            throw new ArgumentNullException(nameof(updateObjs));

        using var command = this.DbContext.CreateCommand();
        int result = 0;
        bool isNeedClose = this.DbContext.IsNeedClose;
        Exception exception = null;
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
                var commandInitializer = RepositoryHelper.BuildUpdateMultiSqlParameters(this.dbKey, this.ormProvider, this.mapProvider, this.shardingProvider, entityType, updateObjs, null, null);
                foreach (var updateObj in entities)
                {
                    if (index > 0) sqlBuilder.Append(';');
                    commandInitializer.Invoke(command.Parameters, this.ormProvider, sqlBuilder, updateObj, index.ToString());
                    if (index >= bulkCount)
                    {
                        command.CommandText = sqlBuilder.ToString();
                        if (isFirst)
                        {
                            this.DbContext.Open();
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
                    if (isFirst) this.DbContext.Open();
                    result += command.ExecuteNonQuery();
                }
                sqlBuilder.Clear();
            }
            else
            {
                var commandInitializer = RepositoryHelper.BuildUpdateSqlParameters(this.dbKey, this.ormProvider, this.mapProvider, this.shardingProvider, entityType, updateObjs, null, null);
                command.CommandText = commandInitializer.Invoke(command.Parameters, this.ormProvider, updateObjs);
                this.DbContext.Open();
                result = command.ExecuteNonQuery();
            }
        }
        catch (Exception ex)
        {
            isNeedClose = true;
            exception = ex;
        }
        finally
        {
            command.Dispose();
            if (isNeedClose) this.Dispose();
        }
        if (exception != null) throw exception;
        return result;
    }
    public virtual async Task<int> UpdateAsync<TEntity>(object updateObjs, int bulkCount = 500, CancellationToken cancellationToken = default)
    {
        if (updateObjs == null)
            throw new ArgumentNullException(nameof(updateObjs));

        using var command = this.DbContext.CreateDbCommand();
        int result = 0;
        bool isNeedClose = this.DbContext.IsNeedClose;
        Exception exception = null;
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
                var commandInitializer = RepositoryHelper.BuildUpdateMultiSqlParameters(this.dbKey, this.ormProvider, this.mapProvider, this.shardingProvider, entityType, updateObjs, null, null);
                foreach (var updateObj in entities)
                {
                    if (index > 0) sqlBuilder.Append(';');
                    commandInitializer.Invoke(command.Parameters, this.ormProvider, sqlBuilder, updateObj, index.ToString());
                    if (index >= bulkCount)
                    {
                        command.CommandText = sqlBuilder.ToString();
                        if (isFirst)
                        {
                            await this.DbContext.OpenAsync(cancellationToken);
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
                    if (isFirst) await this.DbContext.OpenAsync(cancellationToken);
                    result += await command.ExecuteNonQueryAsync(cancellationToken);
                }
                sqlBuilder.Clear();
            }
            else
            {
                var commandInitializer = RepositoryHelper.BuildUpdateSqlParameters(this.dbKey, this.ormProvider, this.mapProvider, this.shardingProvider, entityType, updateObjs, null, null);
                command.CommandText = commandInitializer.Invoke(command.Parameters, this.ormProvider, updateObjs);
                await this.DbContext.OpenAsync(cancellationToken);
                result = await command.ExecuteNonQueryAsync(cancellationToken);
            }
        }
        catch (Exception ex)
        {
            isNeedClose = true;
            exception = ex;
        }
        finally
        {
            await command.DisposeAsync();
            if (isNeedClose) await this.DisposeAsync();
        }
        if (exception != null) throw exception;
        return result;
    }
    #endregion

    #region Delete
    public virtual IDelete<TEntity> Delete<TEntity>() => this.ormProvider.NewDelete<TEntity>(this.DbContext);
    public virtual int Delete<TEntity>(object whereKeys)
    {
        if (whereKeys == null)
            throw new ArgumentNullException(nameof(whereKeys));

        using var command = this.DbContext.CreateCommand();
        int result = 0;
        bool isNeedClose = this.DbContext.IsNeedClose;
        Exception exception = null;
        try
        {
            var entityType = typeof(TEntity);
            if (this.shardingProvider.TryGetShardingTable(entityType, out _))
                throw new NotSupportedException($"实体表{entityType.FullName}有配置分表，当前方法不支持分表，请使用repository.Delete<T>().UseTable或UseTableBy方法可指定分表");

            bool isBulk = whereKeys is IEnumerable && whereKeys is not string && whereKeys is not IDictionary<string, object>;
            if (isBulk)
            {
                int index = 0;
                string separator = null;
                var entities = whereKeys as IEnumerable;
                Type whereObjType = null;
                foreach (var entity in entities)
                {
                    whereObjType = entity.GetType();
                    break;
                }
                (var isMultiKeys, var origName, var headSqlSetter, var commandInitializer) = RepositoryHelper.BuildDeleteBulkCommandInitializer(this.ormProvider, this.mapProvider, entityType, whereObjType);

                var sqlBuilder = new StringBuilder();
                if (isMultiKeys) separator = ";";
                else
                {
                    separator = ",";
                    headSqlSetter(sqlBuilder, origName);
                }
                foreach (var entity in entities)
                {
                    if (index > 0) sqlBuilder.Append(separator);
                    commandInitializer.Invoke(command.Parameters, sqlBuilder, this.ormProvider, origName, entity, $"{index}");
                    index++;
                }
                if (!isMultiKeys) sqlBuilder.Append(')');
                command.CommandText = sqlBuilder.ToString();
                sqlBuilder.Clear();
                sqlBuilder = null;
            }
            else
            {
                (var origName, var commandInitializer) = RepositoryHelper.BuildDeleteCommandInitializer(this.ormProvider, this.mapProvider, entityType, whereKeys, false);
                var typedCommandInitializer = commandInitializer as Func<IDataParameterCollection, IOrmProvider, string, object, string>;
                command.CommandText = typedCommandInitializer.Invoke(command.Parameters, this.ormProvider, origName, whereKeys);
            }
            this.DbContext.Open();
            result = command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            isNeedClose = true;
            exception = ex;
        }
        finally
        {
            command.Dispose();
            if (isNeedClose) this.Dispose();
        }
        if (exception != null) throw exception;
        return result;
    }
    public virtual async Task<int> DeleteAsync<TEntity>(object whereKeys, CancellationToken cancellationToken = default)
    {
        if (whereKeys == null)
            throw new ArgumentNullException(nameof(whereKeys));

        using var command = this.DbContext.CreateDbCommand();
        int result = 0;
        bool isNeedClose = this.DbContext.IsNeedClose;
        Exception exception = null;
        try
        {
            var entityType = typeof(TEntity);
            if (this.shardingProvider.TryGetShardingTable(entityType, out _))
                throw new NotSupportedException($"实体表{entityType.FullName}有配置分表，当前方法不支持分表，请使用repository.Delete<T>().UseTable或UseTableBy方法可指定分表");

            bool isBulk = whereKeys is IEnumerable && whereKeys is not string && whereKeys is not IDictionary<string, object>;
            if (isBulk)
            {
                int index = 0;
                string separator = null;
                var entities = whereKeys as IEnumerable;
                Type whereObjType = null;
                foreach (var entity in entities)
                {
                    whereObjType = entity.GetType();
                    break;
                }
                (var isMultiKeys, var origName, var headSqlSetter, var commandInitializer) = RepositoryHelper.BuildDeleteBulkCommandInitializer(this.ormProvider, this.mapProvider, entityType, whereObjType);

                var sqlBuilder = new StringBuilder();
                if (isMultiKeys) separator = ";";
                else
                {
                    separator = ",";
                    headSqlSetter(sqlBuilder, origName);
                }
                foreach (var entity in entities)
                {
                    if (index > 0) sqlBuilder.Append(separator);
                    commandInitializer.Invoke(command.Parameters, sqlBuilder, this.ormProvider, origName, entity, $"{index}");
                    index++;
                }
                if (!isMultiKeys) sqlBuilder.Append(')');
                command.CommandText = sqlBuilder.ToString();
                sqlBuilder.Clear();
                sqlBuilder = null;
            }
            else
            {
                (var origName, var commandInitializer) = RepositoryHelper.BuildDeleteCommandInitializer(this.ormProvider, this.mapProvider, entityType, whereKeys, false);
                var typedCommandInitializer = commandInitializer as Func<IDataParameterCollection, IOrmProvider, string, object, string>;
                command.CommandText = typedCommandInitializer.Invoke(command.Parameters, this.ormProvider, origName, whereKeys);
            }
            await this.DbContext.OpenAsync(cancellationToken);
            result = await command.ExecuteNonQueryAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            isNeedClose = true;
            exception = ex;
        }
        finally
        {
            await command.DisposeAsync();
            if (isNeedClose) await this.DisposeAsync();
        }
        if (exception != null) throw exception;
        return result;
    }
    #endregion

    #region Exists
    public virtual bool Exists<TEntity>(object whereObj)
    {
        if (whereObj == null)
            throw new ArgumentNullException(nameof(whereObj));

        var result = this.DbContext.QueryFirst<int>(f =>
        {
            var entityType = typeof(TEntity);
            var commandInitializer = RepositoryHelper.BuildExistsSqlParameters(this.ormProvider, this.mapProvider, entityType, whereObj, false);
            var typedCommandInitializer = commandInitializer as Func<IDataParameterCollection, IOrmProvider, object, string>;
            f.CommandText = typedCommandInitializer.Invoke(f.Parameters, this.ormProvider, whereObj);
        });
        return result > 0;
    }
    public virtual async Task<bool> ExistsAsync<TEntity>(object whereObj, CancellationToken cancellationToken = default)
    {
        if (whereObj == null)
            throw new ArgumentNullException(nameof(whereObj));

        var result = await this.DbContext.QueryFirstAsync<int>(f =>
        {
            var entityType = typeof(TEntity);
            var commandInitializer = RepositoryHelper.BuildExistsSqlParameters(this.ormProvider, this.mapProvider, entityType, whereObj, false);
            var typedCommandInitializer = commandInitializer as Func<IDataParameterCollection, IOrmProvider, object, string>;
            f.CommandText = typedCommandInitializer.Invoke(f.Parameters, this.ormProvider, whereObj);
        }, cancellationToken);
        return result > 0;
    }
    public virtual bool Exists<TEntity>(Expression<Func<TEntity, bool>> wherePredicate)
        => this.From<TEntity>().Where(wherePredicate).Count() > 0;
    public virtual async Task<bool> ExistsAsync<TEntity>(Expression<Func<TEntity, bool>> wherePredicate, CancellationToken cancellationToken = default)
        => await this.From<TEntity>().Where(wherePredicate).CountAsync() > 0;
    #endregion

    #region Execute
    public virtual int Execute(string rawSql, object parameters = null)
    {
        if (string.IsNullOrEmpty(rawSql))
            throw new ArgumentNullException(nameof(rawSql));

        return this.DbContext.Execute(f =>
        {
            f.CommandText = rawSql;
            if (parameters != null)
            {
                var commandInitializer = RepositoryHelper.BuildQueryRawSqlParameters(this.ormProvider, rawSql, parameters);
                commandInitializer.Invoke(f.Parameters, this.ormProvider, parameters);
            }
        });
    }
    public virtual async Task<int> ExecuteAsync(string rawSql, object parameters = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(rawSql))
            throw new ArgumentNullException(nameof(rawSql));

        return await this.DbContext.ExecuteAsync(f =>
        {
            f.CommandText = rawSql;
            if (parameters != null)
            {
                var commandInitializer = RepositoryHelper.BuildQueryRawSqlParameters(this.ormProvider, rawSql, parameters);
                commandInitializer.Invoke(f.Parameters, this.ormProvider, parameters);
            }
        }, cancellationToken);
    }
    #endregion

    #region QueryMultiple
    public virtual IMultiQueryReader QueryMultiple(Action<IMultipleQuery> subQueries)
    {
        if (subQueries == null)
            throw new ArgumentNullException(nameof(subQueries));

        using var command = this.DbContext.CreateCommand();
        IMultiQueryReader result = null;
        IDataReader reader = null;
        bool isNeedClose = false;
        Exception exception = null;
        try
        {
            using var multiQuery = new MultipleQuery(this.DbContext, command);
            subQueries.Invoke(multiQuery);
            command.CommandText = multiQuery.BuildSql(out var readerAfters);
            this.DbContext.Open();
            reader = command.ExecuteReader(CommandBehavior.SequentialAccess);
            result = new MultiQueryReader(this.ormProvider, command, reader, readerAfters, isNeedClose);
        }
        catch (Exception ex)
        {
            isNeedClose = true;
            exception = ex;
        }
        finally
        {
            if (isNeedClose)
            {
                reader?.Dispose();
                command.Dispose();
                this.Dispose();
            }
        }
        if (exception != null) throw exception;
        return result;
    }
    public virtual async Task<IMultiQueryReader> QueryMultipleAsync(Action<IMultipleQuery> subQueries, CancellationToken cancellationToken = default)
    {
        if (subQueries == null)
            throw new ArgumentNullException(nameof(subQueries));

        using var command = this.DbContext.CreateDbCommand();
        IMultiQueryReader result = null;
        DbDataReader reader = null;
        bool isNeedClose = false;
        Exception exception = null;
        try
        {
            using var multiQuery = new MultipleQuery(this.DbContext, command);
            subQueries.Invoke(multiQuery);
            command.CommandText = multiQuery.BuildSql(out var readerAfters);
            await this.DbContext.OpenAsync(cancellationToken);
            reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess, cancellationToken);
            result = new MultiQueryReader(this.ormProvider, command, reader, readerAfters, isNeedClose);
        }
        catch (Exception ex)
        {
            isNeedClose = true;
            exception = ex;
        }
        finally
        {
            if (isNeedClose)
            {
                if (reader != null)
                    await reader.DisposeAsync();
                await command.DisposeAsync();
                await this.DisposeAsync();
            }
        }
        if (exception != null) throw exception;
        return result;
    }
    #endregion

    #region MultipleExecute
    public virtual int MultipleExecute(List<MultipleCommand> commands)
    {
        if (commands == null || commands.Count == 0)
            throw new ArgumentNullException(nameof(commands));

        using var command = this.DbContext.CreateCommand();
        bool isNeedClose = this.DbContext.IsNeedClose;
        Exception exception = null;
        int result = 0;
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
                        MultipleCommandType.Insert => this.ormProvider.NewCreateVisitor(this.dbKey, this.mapProvider, this.shardingProvider, this.isParameterized),
                        MultipleCommandType.Update => this.ormProvider.NewUpdateVisitor(this.dbKey, this.mapProvider, this.shardingProvider, this.isParameterized),
                        MultipleCommandType.Delete => this.ormProvider.NewDeleteVisitor(this.dbKey, this.mapProvider, this.shardingProvider, this.isParameterized),
                        _ => this.ormProvider.NewUpdateVisitor(this.dbKey, this.mapProvider, this.shardingProvider, this.isParameterized)
                    };
                    visitors.Add(multiCcommand.CommandType, visitor);
                    isFirst = true;
                }
                switch (multiCcommand.CommandType)
                {
                    case MultipleCommandType.Insert:
                        var insertVisitor = visitor as ICreateVisitor;
                        insertVisitor.Initialize(multiCcommand.EntityType, true, isFirst);
                        insertVisitor.BuildMultiCommand(command, sqlBuilder, multiCcommand, commandIndex);
                        break;
                    case MultipleCommandType.Update:
                        var updateVisitor = visitor as IUpdateVisitor;
                        updateVisitor.Initialize(multiCcommand.EntityType, true, isFirst);
                        updateVisitor.BuildMultiCommand(command, sqlBuilder, multiCcommand, commandIndex);
                        break;
                    case MultipleCommandType.Delete:
                        var deleteVisitor = visitor as IDeleteVisitor;
                        deleteVisitor.Initialize(multiCcommand.EntityType, true, isFirst);
                        deleteVisitor.BuildMultiCommand(command, sqlBuilder, multiCcommand, commandIndex);
                        break;
                }
                commandIndex++;
            }
            command.CommandText = sqlBuilder.ToString();
            this.DbContext.Open();
            result = command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            isNeedClose = true;
            exception = ex;
        }
        finally
        {
            command.Dispose();
            if (isNeedClose) this.Dispose();
        }
        if (exception != null) throw exception;
        return result;
    }
    public virtual async Task<int> MultipleExecuteAsync(List<MultipleCommand> commands, CancellationToken cancellationToken = default)
    {
        if (commands == null || commands.Count == 0)
            throw new ArgumentNullException(nameof(commands));

        using var command = this.DbContext.CreateDbCommand();
        bool isNeedClose = this.DbContext.IsNeedClose;
        Exception exception = null;
        int result = 0;
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
                        MultipleCommandType.Insert => this.ormProvider.NewCreateVisitor(this.dbKey, this.mapProvider, this.shardingProvider, this.isParameterized),
                        MultipleCommandType.Update => this.ormProvider.NewUpdateVisitor(this.dbKey, this.mapProvider, this.shardingProvider, this.isParameterized),
                        MultipleCommandType.Delete => this.ormProvider.NewDeleteVisitor(this.dbKey, this.mapProvider, this.shardingProvider, this.isParameterized),
                        _ => this.ormProvider.NewUpdateVisitor(this.dbKey, this.mapProvider, this.shardingProvider, this.isParameterized)
                    };
                    visitors.Add(multiCcommand.CommandType, visitor);
                    isFirst = true;
                }
                switch (multiCcommand.CommandType)
                {
                    case MultipleCommandType.Insert:
                        var insertVisitor = visitor as ICreateVisitor;
                        insertVisitor.Initialize(multiCcommand.EntityType, true, isFirst);
                        insertVisitor.BuildMultiCommand(command, sqlBuilder, multiCcommand, commandIndex);
                        break;
                    case MultipleCommandType.Update:
                        var updateVisitor = visitor as IUpdateVisitor;
                        updateVisitor.Initialize(multiCcommand.EntityType, true, isFirst);
                        updateVisitor.BuildMultiCommand(command, sqlBuilder, multiCcommand, commandIndex);
                        break;
                    case MultipleCommandType.Delete:
                        var deleteVisitor = visitor as IDeleteVisitor;
                        deleteVisitor.Initialize(multiCcommand.EntityType, true, isFirst);
                        deleteVisitor.BuildMultiCommand(command, sqlBuilder, multiCcommand, commandIndex);
                        break;
                }
                commandIndex++;
            }
            command.CommandText = sqlBuilder.ToString();
            await this.DbContext.OpenAsync(cancellationToken);
            result = await command.ExecuteNonQueryAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            isNeedClose = true;
            exception = ex;
        }
        finally
        {
            await command.DisposeAsync();
            if (isNeedClose) await this.DisposeAsync();
        }
        if (exception != null) throw exception;
        return result;
    }
    #endregion

    #region Others
    public virtual void Close() => this.Dispose();
    public virtual async Task CloseAsync() => await this.DisposeAsync();
    public virtual IRepository Timeout(int timeout)
    {
        this.DbContext.CommandTimeout = timeout;
        return this;
    }
    public virtual IRepository WithParameterized(bool isParameterized = true)
    {
        this.DbContext.IsParameterized = isParameterized;
        return this;
    }
    public virtual IRepository With(OrmDbFactoryOptions options)
    {
        if (options == null) return this;
        this.DbContext.IsParameterized = options.IsParameterized;
        this.DbContext.CommandTimeout = options.Timeout;
        return this;
    }
    public virtual void BeginTransaction() => this.DbContext.BeginTransaction();
    public virtual async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        => await this.DbContext.BeginTransactionAsync(cancellationToken);
    public virtual void Commit()
    {
        this.DbContext.Commit();
        this.DbContext.Close();
        this.DbContext.Transaction = null;
    }
    public virtual async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        await this.DbContext.CommitAsync(cancellationToken);
        await this.DbContext.CloseAsync();
        this.DbContext.Transaction = null;
    }
    public virtual void Rollback()
    {
        this.DbContext.Rollback();
        this.DbContext.Close();
        this.DbContext.Transaction = null;
    }
    public virtual async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        await this.DbContext.RollbackAsync(cancellationToken);
        await this.DbContext.CloseAsync();
        this.DbContext.Transaction = null;
    }
    public virtual void Dispose()
    {
        this.DbContext.Close();
        GC.SuppressFinalize(this);
    }
    public virtual async ValueTask DisposeAsync()
    {
        await this.DbContext.CloseAsync();
        GC.SuppressFinalize(this);
    }
    ~Repository() => this.Dispose();
    private IQueryVisitor CreateQueryVisitor(char tableAsStart = 'a')
        => this.ormProvider.NewQueryVisitor(this.dbKey, this.mapProvider, this.DbContext.ShardingProvider, this.isParameterized, tableAsStart);
    #endregion
}
