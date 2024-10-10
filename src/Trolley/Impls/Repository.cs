using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley;

public class Repository : IRepository
{
    #region Properties
    public DbContext DbContext { get; set; }
    public IOrmProvider OrmProvider => this.DbContext.OrmProvider;
    public IEntityMapProvider MapProvider => this.DbContext.MapProvider;
    public ITableShardingProvider ShardingProvider => this.DbContext.ShardingProvider;
    public bool IsParameterized => this.DbContext.IsConstantParameterized;
    #endregion

    #region Constructor
    public Repository(DbContext dbContext) => this.DbContext = dbContext;
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

    #region From SubQuery
    public virtual IQuery<T> From<T>(IQuery<T> subQuery)
    {
        var visitor = this.CreateQueryVisitor();
        visitor.From(typeof(T), subQuery);
        return this.OrmProvider.NewQuery<T>(this.DbContext, visitor);
    }
    public virtual IQuery<T> From<T>(Func<IFromQuery, IQuery<T>> subQuery)
    {
        var visitor = this.CreateQueryVisitor();
        visitor.From(typeof(T), this.DbContext, subQuery);
        return this.OrmProvider.NewQuery<T>(this.DbContext, visitor);
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
                var commandInitializer = RepositoryHelper.BuildQueryRawSqlParameters(this.OrmProvider, rawSql, parameters);
                commandInitializer.Invoke(f.Parameters, this.OrmProvider, parameters);
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
                var commandInitializer = RepositoryHelper.BuildQueryRawSqlParameters(this.OrmProvider, rawSql, parameters);
                commandInitializer.Invoke(f.Parameters, this.OrmProvider, parameters);
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
            var commandInitializer = RepositoryHelper.BuildQueryWhereObjSqlParameters(this.DbContext, entityType, whereObjType, false);
            var typedCommandInitializer = commandInitializer as Func<IDataParameterCollection, DbContext, object, string>;
            f.CommandText = typedCommandInitializer.Invoke(f.Parameters, this.DbContext, whereObj);
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
            var commandInitializer = RepositoryHelper.BuildQueryWhereObjSqlParameters(this.DbContext, entityType, whereObjType, false);
            var typedCommandInitializer = commandInitializer as Func<IDataParameterCollection, DbContext, object, string>;
            f.CommandText = typedCommandInitializer.Invoke(f.Parameters, this.DbContext, whereObj);
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
                var commandInitializer = RepositoryHelper.BuildQueryRawSqlParameters(this.OrmProvider, rawSql, parameters);
                commandInitializer.Invoke(f.Parameters, this.OrmProvider, parameters);
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
                var commandInitializer = RepositoryHelper.BuildQueryRawSqlParameters(this.OrmProvider, rawSql, parameters);
                commandInitializer.Invoke(f.Parameters, this.OrmProvider, parameters);
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
            var commandInitializer = RepositoryHelper.BuildQueryWhereObjSqlParameters(this.DbContext, entityType, whereObjType, false);
            var typedCommandInitializer = commandInitializer as Func<IDataParameterCollection, DbContext, object, string>;
            f.CommandText = typedCommandInitializer.Invoke(f.Parameters, this.DbContext, whereObj);
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
            var commandInitializer = RepositoryHelper.BuildQueryWhereObjSqlParameters(this.DbContext, entityType, whereObjType, false);
            var typedCommandInitializer = commandInitializer as Func<IDataParameterCollection, DbContext, object, string>;
            f.CommandText = typedCommandInitializer.Invoke(f.Parameters, this.DbContext, whereObj);
        }, cancellationToken);
    }
    #endregion

    #region Get
    public virtual TEntity Get<TEntity>(object whereObj) => this.DbContext.Get<TEntity>(whereObj);
    public virtual async Task<TEntity> GetAsync<TEntity>(object whereObj, CancellationToken cancellationToken = default)
        => await this.DbContext.GetAsync<TEntity>(whereObj, cancellationToken);
    #endregion

    #region Create
    public virtual ICreate<TEntity> Create<TEntity>() => this.OrmProvider.NewCreate<TEntity>(this.DbContext);
    public virtual int Create<TEntity>(object insertObjs, int bulkCount = 500)
    {
        if (insertObjs == null)
            throw new ArgumentNullException(nameof(insertObjs));

        int result = 0;
        (var isNeedClose, var connection, var command) = this.DbContext.UseMasterCommand();
        bool isBulk = insertObjs is IEnumerable && insertObjs is not string && insertObjs is not IDictionary<string, object>;
        var sqlType = isBulk ? CommandSqlType.BulkInsert : CommandSqlType.Insert;
        var entityType = typeof(TEntity);
        if (isBulk)
        {
            var builder = new StringBuilder();
            object firstInsertObj = null;
            Type insertObjType = null;
            var entities = insertObjs as IEnumerable;
            foreach (var insertObj in entities)
            {
                firstInsertObj = insertObj;
                break;
            }
            insertObjType = firstInsertObj.GetType();
            var ormProvider = this.DbContext.OrmProvider;
            var mapProvider = this.DbContext.MapProvider;

            var fieldsSqlPartSetter = RepositoryHelper.BuildCreateFieldsSqlPart(ormProvider, mapProvider, entityType, insertObjType, null, null);
            var valuesSqlPartSetter = RepositoryHelper.BuildCreateValuesSqlParametes(this.DbContext, entityType, insertObjType, null, null, true);
            bool isDictionary = typeof(IDictionary<string, object>).IsAssignableFrom(insertObjType);

            Action<IDataParameterCollection, StringBuilder, string> firstSqlSetter = null;
            Action<IDataParameterCollection, StringBuilder, object, string> loopSqlSetter = null;

            if (isDictionary)
            {
                var typedFieldsSqlPartSetter = fieldsSqlPartSetter as Func<StringBuilder, object, List<MemberMap>>;
                var typedValuesSqlPartSetter = valuesSqlPartSetter as Action<IDataParameterCollection, StringBuilder, DbContext, List<MemberMap>, object, string>;

                var memberMappers = typedFieldsSqlPartSetter.Invoke(builder, firstInsertObj);
                builder.Append(") VALUES ");
                var firstHeadSql = builder.ToString();
                builder.Clear();
                builder = null;

                firstSqlSetter = (dbParameters, builder, tableName) =>
                {
                    builder.Append($"INSERT INTO {ormProvider.GetTableName(tableName)} (");
                    builder.Append(firstHeadSql);
                };
                loopSqlSetter = (dbParameters, builder, insertObj, suffix) =>
                {
                    builder.Append('(');
                    typedValuesSqlPartSetter.Invoke(dbParameters, builder, this.DbContext, memberMappers, insertObj, suffix);
                    builder.Append(')');
                };
            }
            else
            {
                var typedFieldsSqlPartSetter = fieldsSqlPartSetter as Action<StringBuilder>;
                var typedValuesSqlPartSetter = valuesSqlPartSetter as Action<IDataParameterCollection, StringBuilder, DbContext, object, string>;

                firstSqlSetter = (dbParameters, builder, tableName) =>
                {
                    builder.Append($"INSERT INTO {ormProvider.GetTableName(tableName)} (");
                    typedFieldsSqlPartSetter.Invoke(builder);
                    builder.Append(") VALUES ");
                };
                loopSqlSetter = (dbParameters, builder, insertObj, suffix) =>
                {
                    builder.Append('(');
                    typedValuesSqlPartSetter.Invoke(dbParameters, builder, this.DbContext, insertObj, suffix);
                    builder.Append(')');
                };
            }
            int executor(string tableName, IEnumerable insertObjs)
            {
                int count = 0, index = 0;
                foreach (var insertObj in insertObjs)
                {
                    if (index > 0) builder.Append(',');
                    loopSqlSetter.Invoke(command.Parameters, builder, insertObj, index.ToString());
                    if (index >= bulkCount)
                    {
                        command.CommandText = builder.ToString();
                        count += command.ExecuteNonQuery(sqlType);
                        builder.Clear();
                        command.Parameters.Clear();
                        firstSqlSetter.Invoke(command.Parameters, builder, tableName);
                        index = 0;
                        continue;
                    }
                    index++;
                }
                if (index > 0)
                {
                    command.CommandText = builder.ToString();
                    count += command.ExecuteNonQuery(sqlType);
                }
                return count;
            }

            connection.Open();
            if (this.DbContext.ShardingProvider != null && this.DbContext.ShardingProvider.TryGetTableSharding(entityType, out _))
            {
                var tabledInsertObjs = this.DbContext.SplitShardingParameters(entityType, entities);
                foreach (var tabledInsertObj in tabledInsertObjs)
                {
                    firstSqlSetter.Invoke(command.Parameters, builder, tabledInsertObj.Key);
                    result += executor(tabledInsertObj.Key, tabledInsertObj.Value);
                    builder.Clear();
                    command.Parameters.Clear();
                }
            }
            else
            {
                var entityMapper = mapProvider.GetEntityMap(entityType);
                var tableName = entityMapper.TableName;
                firstSqlSetter.Invoke(command.Parameters, builder, tableName);
                result = executor(tableName, entities);
            }
            builder.Clear();
            builder = null;
        }
        else
        {
            this.DbContext.BuildCreateCommand(command.BaseCommand, entityType, insertObjs, false);
            connection.Open();
            result = command.ExecuteNonQuery(sqlType);
        }
        command.Parameters.Clear();
        command.Dispose();
        if (isNeedClose) connection.Close();
        return result;
    }
    public virtual async Task<int> CreateAsync<TEntity>(object insertObjs, int bulkCount = 500, CancellationToken cancellationToken = default)
    {
        if (insertObjs == null)
            throw new ArgumentNullException(nameof(insertObjs));

        int result = 0;
        (var isNeedClose, var connection, var command) = this.DbContext.UseMasterCommand();
        bool isBulk = insertObjs is IEnumerable && insertObjs is not string && insertObjs is not IDictionary<string, object>;
        var sqlType = isBulk ? CommandSqlType.BulkInsert : CommandSqlType.Insert;
        var entityType = typeof(TEntity);
        if (isBulk)
        {
            var builder = new StringBuilder();
            object firstInsertObj = null;
            Type insertObjType = null;
            var entities = insertObjs as IEnumerable;
            foreach (var insertObj in entities)
            {
                firstInsertObj = insertObj;
                break;
            }
            insertObjType = firstInsertObj.GetType();
            var ormProvider = this.DbContext.OrmProvider;
            var mapProvider = this.DbContext.MapProvider;

            var fieldsSqlPartSetter = RepositoryHelper.BuildCreateFieldsSqlPart(ormProvider, mapProvider, entityType, insertObjType, null, null);
            var valuesSqlPartSetter = RepositoryHelper.BuildCreateValuesSqlParametes(this.DbContext, entityType, insertObjType, null, null, true);
            bool isDictionary = typeof(IDictionary<string, object>).IsAssignableFrom(insertObjType);

            Action<IDataParameterCollection, StringBuilder, string> firstSqlSetter = null;
            Action<IDataParameterCollection, StringBuilder, object, string> loopSqlSetter = null;

            if (isDictionary)
            {
                var typedFieldsSqlPartSetter = fieldsSqlPartSetter as Func<StringBuilder, object, List<MemberMap>>;
                var typedValuesSqlPartSetter = valuesSqlPartSetter as Action<IDataParameterCollection, StringBuilder, DbContext, List<MemberMap>, object, string>;

                var memberMappers = typedFieldsSqlPartSetter.Invoke(builder, firstInsertObj);
                builder.Append(") VALUES ");
                var firstHeadSql = builder.ToString();
                builder.Clear();
                builder = null;

                firstSqlSetter = (dbParameters, builder, tableName) =>
                {
                    builder.Append($"INSERT INTO {ormProvider.GetTableName(tableName)} (");
                    builder.Append(firstHeadSql);
                };
                loopSqlSetter = (dbParameters, builder, insertObj, suffix) =>
                {
                    builder.Append('(');
                    typedValuesSqlPartSetter.Invoke(dbParameters, builder, this.DbContext, memberMappers, insertObj, suffix);
                    builder.Append(')');
                };
            }
            else
            {
                var typedFieldsSqlPartSetter = fieldsSqlPartSetter as Action<StringBuilder>;
                var typedValuesSqlPartSetter = valuesSqlPartSetter as Action<IDataParameterCollection, StringBuilder, DbContext, object, string>;

                firstSqlSetter = (dbParameters, builder, tableName) =>
                {
                    builder.Append($"INSERT INTO {ormProvider.GetTableName(tableName)} (");
                    typedFieldsSqlPartSetter.Invoke(builder);
                    builder.Append(") VALUES ");
                };
                loopSqlSetter = (dbParameters, builder, insertObj, suffix) =>
                {
                    builder.Append('(');
                    typedValuesSqlPartSetter.Invoke(dbParameters, builder, this.DbContext, insertObj, suffix);
                    builder.Append(')');
                };
            }
            async Task<int> executor(string tableName, IEnumerable insertObjs)
            {
                int count = 0, index = 0;
                foreach (var insertObj in insertObjs)
                {
                    if (index > 0) builder.Append(',');
                    loopSqlSetter.Invoke(command.Parameters, builder, insertObj, index.ToString());
                    if (index >= bulkCount)
                    {
                        command.CommandText = builder.ToString();
                        count += await command.ExecuteNonQueryAsync(sqlType, cancellationToken);
                        builder.Clear();
                        command.Parameters.Clear();
                        firstSqlSetter.Invoke(command.Parameters, builder, tableName);
                        index = 0;
                        continue;
                    }
                    index++;
                }
                if (index > 0)
                {
                    command.CommandText = builder.ToString();
                    count += await command.ExecuteNonQueryAsync(sqlType, cancellationToken);
                }
                return count;
            }
            await connection.OpenAsync(cancellationToken);
            if (this.DbContext.ShardingProvider != null && this.DbContext.ShardingProvider.TryGetTableSharding(entityType, out _))
            {
                var tabledInsertObjs = this.DbContext.SplitShardingParameters(entityType, entities);
                foreach (var tabledInsertObj in tabledInsertObjs)
                {
                    firstSqlSetter.Invoke(command.Parameters, builder, tabledInsertObj.Key);
                    result += await executor(tabledInsertObj.Key, tabledInsertObj.Value);
                    builder.Clear();
                    command.Parameters.Clear();
                }
            }
            else
            {
                var entityMapper = mapProvider.GetEntityMap(entityType);
                var tableName = entityMapper.TableName;
                firstSqlSetter.Invoke(command.Parameters, builder, tableName);
                result = await executor(tableName, entities);
            }
            builder.Clear();
            builder = null;
        }
        else
        {
            this.DbContext.BuildCreateCommand(command.BaseCommand, entityType, insertObjs, false);
            await connection.OpenAsync(cancellationToken);
            result = await command.ExecuteNonQueryAsync(sqlType, cancellationToken);
        }
        command.Parameters.Clear();
        await command.DisposeAsync();
        if (isNeedClose) await connection.CloseAsync();
        return result;
    }
    public virtual int CreateIdentity<TEntity>(object insertObj)
    {
        if (insertObj == null)
            throw new ArgumentNullException(nameof(insertObj));
        bool isBulk = insertObj is IEnumerable && insertObj is not string && insertObj is not IDictionary<string, object>;
        if (isBulk) throw new NotSupportedException("CreateIdentity方法只支持单条数据插入，不支持批量插入返回Identity");

        return this.DbContext.CreateResult<int>((command, dbContext) =>
        {
            dbContext.BuildCreateCommand(command, typeof(TEntity), insertObj, true);
            return null;
        });
    }
    public virtual async Task<int> CreateIdentityAsync<TEntity>(object insertObj, CancellationToken cancellationToken = default)
    {
        if (insertObj == null)
            throw new ArgumentNullException(nameof(insertObj));
        bool isBulk = insertObj is IEnumerable && insertObj is not string && insertObj is not IDictionary<string, object>;
        if (isBulk) throw new NotSupportedException("CreateIdentity方法只支持单条数据插入，不支持批量插入返回Identity");
        return await this.DbContext.CreateResultAsync<int>((command, dbContext) =>
        {
            dbContext.BuildCreateCommand(command, typeof(TEntity), insertObj, true);
            return null;
        }, cancellationToken);
    }
    public virtual long CreateIdentityLong<TEntity>(object insertObj)
    {
        if (insertObj == null)
            throw new ArgumentNullException(nameof(insertObj));
        bool isBulk = insertObj is IEnumerable && insertObj is not string && insertObj is not IDictionary<string, object>;
        if (isBulk) throw new NotSupportedException("CreateIdentity方法只支持单条数据插入，不支持批量插入返回Identity");
        return this.DbContext.CreateResult<long>((command, dbContext) =>
        {
            dbContext.BuildCreateCommand(command, typeof(TEntity), insertObj, true);
            return null;
        });
    }
    public virtual async Task<long> CreateIdentityLongAsync<TEntity>(object insertObj, CancellationToken cancellationToken = default)
    {
        if (insertObj == null)
            throw new ArgumentNullException(nameof(insertObj));
        bool isBulk = insertObj is IEnumerable && insertObj is not string && insertObj is not IDictionary<string, object>;
        if (isBulk) throw new NotSupportedException("CreateIdentity方法只支持单条数据插入，不支持批量插入返回Identity");

        return await this.DbContext.CreateResultAsync<long>((command, dbContext) =>
        {
            dbContext.BuildCreateCommand(command, typeof(TEntity), insertObj, true);
            return null;
        }, cancellationToken);
    }
    #endregion

    #region Update
    public virtual IUpdate<TEntity> Update<TEntity>() => this.OrmProvider.NewUpdate<TEntity>(this.DbContext);
    public virtual int Update<TEntity>(object updateObjs, int bulkCount = 500)
    {
        if (updateObjs == null)
            throw new ArgumentNullException(nameof(updateObjs));

        int result = 0;
        (var isNeedClose, var connection, var command) = this.DbContext.UseMasterCommand();
        bool isBulk = updateObjs is IEnumerable && updateObjs is not string && updateObjs is not IDictionary<string, object>;
        var entityType = typeof(TEntity);
        var builder = new StringBuilder();
        if (isBulk)
        {
            int index = 0;
            var entities = updateObjs as IEnumerable;
            Type updateObjType = null;
            foreach (var updateObj in entities)
            {
                updateObjType = updateObj.GetType();
                break;
            }
            (var tableName, var headSqlSetter, var sqlSetter, _) = RepositoryHelper.BuildUpdateSqlParameters(this.DbContext, entityType, updateObjType, true, null, null);
            var typedSqlSetter = sqlSetter as Action<IDataParameterCollection, StringBuilder, DbContext, object, string>;

            connection.Open();
            foreach (var updateObj in entities)
            {
                if (index > 0) builder.Append(';');
                headSqlSetter.Invoke(builder, tableName);
                typedSqlSetter.Invoke(command.Parameters, builder, this.DbContext, updateObj, index.ToString());
                if (index >= bulkCount)
                {
                    command.CommandText = builder.ToString();
                    result += command.ExecuteNonQuery(CommandSqlType.BulkUpdate);
                    command.Parameters.Clear();
                    builder.Clear();
                    index = 0;
                    continue;
                }
                index++;
            }
            if (index > 0)
            {
                command.CommandText = builder.ToString();
                result += command.ExecuteNonQuery(CommandSqlType.BulkUpdate);
            }
        }
        else
        {
            var updateObjType = updateObjs.GetType();
            (var tableName, var headSqlSetter, var sqlSetter, _) = RepositoryHelper.BuildUpdateSqlParameters(this.DbContext, entityType, updateObjType, false, null, null);
            headSqlSetter.Invoke(builder, tableName);
            var typedSqlSetter = sqlSetter as Action<IDataParameterCollection, StringBuilder, DbContext, object>;
            typedSqlSetter.Invoke(command.Parameters, builder, this.DbContext, updateObjs);
            command.CommandText = builder.ToString();
            connection.Open();
            result = command.ExecuteNonQuery(CommandSqlType.Update);
        }
        builder.Clear();
        command.Parameters.Clear();
        command.Dispose();
        if (isNeedClose) connection.Close();
        return result;
    }
    public virtual async Task<int> UpdateAsync<TEntity>(object updateObjs, int bulkCount = 500, CancellationToken cancellationToken = default)
    {
        if (updateObjs == null)
            throw new ArgumentNullException(nameof(updateObjs));

        int result = 0;
        (var isNeedClose, var connection, var command) = this.DbContext.UseMasterCommand();
        bool isBulk = updateObjs is IEnumerable && updateObjs is not string && updateObjs is not IDictionary<string, object>;
        var entityType = typeof(TEntity);
        var builder = new StringBuilder();
        if (isBulk)
        {
            int index = 0;
            var entities = updateObjs as IEnumerable;
            Type updateObjType = null;
            foreach (var updateObj in entities)
            {
                updateObjType = updateObj.GetType();
                break;
            }
            (var tableName, var headSqlSetter, var sqlSetter, _) = RepositoryHelper.BuildUpdateSqlParameters(this.DbContext, entityType, updateObjType, true, null, null);
            var typedSqlSetter = sqlSetter as Action<IDataParameterCollection, StringBuilder, DbContext, object, string>;
            await connection.OpenAsync(cancellationToken);
            foreach (var updateObj in entities)
            {
                if (index > 0) builder.Append(';');
                headSqlSetter.Invoke(builder, tableName);
                typedSqlSetter.Invoke(command.Parameters, builder, this.DbContext, updateObj, index.ToString());
                if (index >= bulkCount)
                {
                    command.CommandText = builder.ToString();
                    result += await command.ExecuteNonQueryAsync(CommandSqlType.BulkUpdate, cancellationToken);
                    command.Parameters.Clear();
                    builder.Clear();
                    index = 0;
                    continue;
                }
                index++;
            }
            if (index > 0)
            {
                command.CommandText = builder.ToString();
                result += await command.ExecuteNonQueryAsync(CommandSqlType.BulkUpdate, cancellationToken);
            }
        }
        else
        {
            var updateObjType = updateObjs.GetType();
            (var tableName, var headSqlSetter, var sqlSetter, _) = RepositoryHelper.BuildUpdateSqlParameters(this.DbContext, entityType, updateObjType, false, null, null);
            headSqlSetter.Invoke(builder, tableName);
            var typedSqlSetter = sqlSetter as Action<IDataParameterCollection, StringBuilder, DbContext, object>;
            typedSqlSetter.Invoke(command.Parameters, builder, this.DbContext, updateObjs);
            command.CommandText = builder.ToString();
            await connection.OpenAsync(cancellationToken);
            result = await command.ExecuteNonQueryAsync(CommandSqlType.Update, cancellationToken);
        }
        builder.Clear();
        command.Parameters.Clear();
        await command.DisposeAsync();
        if (isNeedClose) await connection.CloseAsync();
        return result;
    }
    #endregion

    #region Delete
    public virtual IDelete<TEntity> Delete<TEntity>() => this.OrmProvider.NewDelete<TEntity>(this.DbContext);
    public virtual int Delete<TEntity>(object whereKeys)
    {
        if (whereKeys == null)
            throw new ArgumentNullException(nameof(whereKeys));

        (var isNeedClose, var connection, var command) = this.DbContext.UseMasterCommand();
        var entityType = typeof(TEntity);
        this.BuildDeleteCommand(command.BaseCommand, entityType, whereKeys);
        connection.Open();
        var result = command.ExecuteNonQuery(CommandSqlType.Delete);
        command.Parameters.Clear();
        command.Dispose();
        if (isNeedClose) connection.Close();
        return result;
    }
    public virtual async Task<int> DeleteAsync<TEntity>(object whereKeys, CancellationToken cancellationToken = default)
    {
        if (whereKeys == null)
            throw new ArgumentNullException(nameof(whereKeys));

        (var isNeedClose, var connection, var command) = this.DbContext.UseMasterCommand();
        var entityType = typeof(TEntity);
        this.BuildDeleteCommand(command.BaseCommand, entityType, whereKeys);
        await connection.OpenAsync(cancellationToken);
        var result = await command.ExecuteNonQueryAsync(CommandSqlType.Delete, cancellationToken);

        command.Parameters.Clear();
        await command.DisposeAsync();
        if (isNeedClose) await connection.CloseAsync();
        return result;
    }
    private void BuildDeleteCommand(IDbCommand command, Type entityType, object whereKeys)
    {
        Type whereObjType = null;
        var isBulk = whereKeys is IEnumerable && whereKeys is not string && whereKeys is not IDictionary<string, object>;
        var entities = whereKeys as IEnumerable;
        if (isBulk)
        {
            foreach (var entity in entities)
            {
                whereObjType = entity.GetType();
                break;
            }
        }
        else whereObjType = whereKeys.GetType();
        (var isMultiKeys, var tableName, var whereSqlParametersSetter, var sqlSetter) = RepositoryHelper.BuildDeleteCommandInitializer(this.DbContext, entityType, whereObjType, isBulk, isBulk);

        int index = 0;
        var builder = new StringBuilder();
        var whereSqlBuilder = new StringBuilder();
        if (isBulk)
        {
            var jointMark = isMultiKeys ? " OR " : ",";
            var typedWhereSqlParametersSetter = whereSqlParametersSetter as Action<IDataParameterCollection, StringBuilder, DbContext, object, string>;

            foreach (var entity in entities)
            {
                if (index > 0) whereSqlBuilder.Append(jointMark);
                typedWhereSqlParametersSetter.Invoke(command.Parameters, whereSqlBuilder, this.DbContext, entity, $"{index}");
                index++;
            }
            if (!isMultiKeys) whereSqlBuilder.Append(')');
        }
        else
        {
            var typedWhereSqlParametersSetter = whereSqlParametersSetter as Action<IDataParameterCollection, StringBuilder, DbContext, object>;
            typedWhereSqlParametersSetter.Invoke(command.Parameters, whereSqlBuilder, this.DbContext, whereKeys);
        }
        sqlSetter.Invoke(builder, tableName);
        builder.Append(whereSqlBuilder);
        command.CommandText = builder.ToString();
        builder.Clear();
        whereSqlBuilder.Clear();
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
            var whereObjType = whereObj.GetType();
            var commandInitializer = RepositoryHelper.BuildExistsSqlParameters(this.DbContext, entityType, whereObjType, false);
            var typedCommandInitializer = commandInitializer as Func<IDataParameterCollection, DbContext, object, string>;
            f.CommandText = typedCommandInitializer.Invoke(f.Parameters, this.DbContext, whereObj);
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
            var whereObjType = whereObj.GetType();
            var commandInitializer = RepositoryHelper.BuildExistsSqlParameters(this.DbContext, entityType, whereObjType, false);
            var typedCommandInitializer = commandInitializer as Func<IDataParameterCollection, DbContext, object, string>;
            f.CommandText = typedCommandInitializer.Invoke(f.Parameters, this.DbContext, whereObj);
        }, cancellationToken);
        return result > 0;
    }
    public virtual bool Exists<TEntity>(Expression<Func<TEntity, bool>> wherePredicate = null)
    {
        if (wherePredicate != null)
            return this.From<TEntity>().Where(wherePredicate).Count() > 0;
        return this.From<TEntity>().Count() > 0;
    }
    public virtual async Task<bool> ExistsAsync<TEntity>(Expression<Func<TEntity, bool>> wherePredicate, CancellationToken cancellationToken = default)
    {
        if (wherePredicate != null)
            return await this.From<TEntity>().Where(wherePredicate).CountAsync(cancellationToken) > 0;
        return await this.From<TEntity>().CountAsync(cancellationToken) > 0;
    }
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
                var commandInitializer = RepositoryHelper.BuildQueryRawSqlParameters(this.OrmProvider, rawSql, parameters);
                commandInitializer.Invoke(f.Parameters, this.OrmProvider, parameters);
            }
            return false;
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
                var commandInitializer = RepositoryHelper.BuildQueryRawSqlParameters(this.OrmProvider, rawSql, parameters);
                commandInitializer.Invoke(f.Parameters, this.OrmProvider, parameters);
            }
            return false;
        }, cancellationToken);
    }
    #endregion

    #region QueryMultiple
    public virtual IMultiQueryReader QueryMultiple(Action<IMultipleQuery> subQueries)
    {
        if (subQueries == null)
            throw new ArgumentNullException(nameof(subQueries));

        using var multiQuery = new MultipleQuery(this.DbContext);
        subQueries.Invoke(multiQuery);
        (var isNeedClose, var connection, var command) = this.DbContext.UseSlaveCommand(multiQuery.isUseMaster, multiQuery.Command);
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

        using var multiQuery = new MultipleQuery(this.DbContext);
        subQueries.Invoke(multiQuery);
        (var isNeedClose, var connection, var command) = this.DbContext.UseSlaveCommand(multiQuery.isUseMaster, multiQuery.Command);
        multiQuery.Command.Connection = connection.BaseConnection;
        command.CommandText = multiQuery.BuildSql(out var readerAfters);
        await connection.OpenAsync(cancellationToken);
        var reader = await command.ExecuteReaderAsync(CommandSqlType.MultiQuery, CommandBehavior.SequentialAccess, cancellationToken);
        //多语句查询，在最后reader读取后，自动关闭
        return new MultiQueryReader(this.DbContext, connection, command, reader, readerAfters, isNeedClose);
    }
    #endregion

    #region MultipleExecute
    public virtual int MultipleExecute(List<MultipleCommand> commands)
    {
        if (commands == null || commands.Count == 0)
            throw new ArgumentNullException(nameof(commands));

        (var isNeedClose, var connection, var command) = this.DbContext.UseMasterCommand();
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
                    MultipleCommandType.Insert => this.OrmProvider.NewCreateVisitor(this.DbContext),
                    MultipleCommandType.Update => this.OrmProvider.NewUpdateVisitor(this.DbContext),
                    MultipleCommandType.Delete => this.OrmProvider.NewDeleteVisitor(this.DbContext),
                    _ => this.OrmProvider.NewUpdateVisitor(this.DbContext)
                };
                visitors.Add(multiCcommand.CommandType, visitor);
                isFirst = true;
            }
            switch (multiCcommand.CommandType)
            {
                case MultipleCommandType.Insert:
                    var insertVisitor = visitor as ICreateVisitor;
                    insertVisitor.Initialize(multiCcommand.EntityType, true, isFirst);
                    insertVisitor.BuildMultiCommand(command.BaseCommand, sqlBuilder, multiCcommand, commandIndex);
                    break;
                case MultipleCommandType.Update:
                    var updateVisitor = visitor as IUpdateVisitor;
                    updateVisitor.Initialize(multiCcommand.EntityType, true, isFirst);
                    updateVisitor.BuildMultiCommand(this.DbContext, command.BaseCommand, sqlBuilder, multiCcommand, commandIndex);
                    break;
                case MultipleCommandType.Delete:
                    var deleteVisitor = visitor as IDeleteVisitor;
                    deleteVisitor.Initialize(multiCcommand.EntityType, true, isFirst);
                    deleteVisitor.BuildMultiCommand(this.DbContext, command.BaseCommand, sqlBuilder, multiCcommand, commandIndex);
                    break;
            }
            commandIndex++;
        }
        command.CommandText = sqlBuilder.ToString();
        connection.Open();
        var result = command.ExecuteNonQuery(CommandSqlType.MultiCommand);
        command.Parameters.Clear();
        command.Dispose();
        if (isNeedClose) connection.Close();
        return result;
    }
    public virtual async Task<int> MultipleExecuteAsync(List<MultipleCommand> commands, CancellationToken cancellationToken = default)
    {
        if (commands == null || commands.Count == 0)
            throw new ArgumentNullException(nameof(commands));

        (var isNeedClose, var connection, var command) = this.DbContext.UseMasterCommand();
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
                    MultipleCommandType.Insert => this.OrmProvider.NewCreateVisitor(this.DbContext),
                    MultipleCommandType.Update => this.OrmProvider.NewUpdateVisitor(this.DbContext),
                    MultipleCommandType.Delete => this.OrmProvider.NewDeleteVisitor(this.DbContext),
                    _ => this.OrmProvider.NewUpdateVisitor(this.DbContext)
                };
                visitors.Add(multiCcommand.CommandType, visitor);
                isFirst = true;
            }
            switch (multiCcommand.CommandType)
            {
                case MultipleCommandType.Insert:
                    var insertVisitor = visitor as ICreateVisitor;
                    insertVisitor.Initialize(multiCcommand.EntityType, true, isFirst);
                    insertVisitor.BuildMultiCommand(command.BaseCommand, sqlBuilder, multiCcommand, commandIndex);
                    break;
                case MultipleCommandType.Update:
                    var updateVisitor = visitor as IUpdateVisitor;
                    updateVisitor.Initialize(multiCcommand.EntityType, true, isFirst);
                    updateVisitor.BuildMultiCommand(this.DbContext, command.BaseCommand, sqlBuilder, multiCcommand, commandIndex);
                    break;
                case MultipleCommandType.Delete:
                    var deleteVisitor = visitor as IDeleteVisitor;
                    deleteVisitor.Initialize(multiCcommand.EntityType, true, isFirst);
                    deleteVisitor.BuildMultiCommand(this.DbContext, command.BaseCommand, sqlBuilder, multiCcommand, commandIndex);
                    break;
            }
            commandIndex++;
        }
        command.CommandText = sqlBuilder.ToString();
        await connection.OpenAsync(cancellationToken);
        var result = await command.ExecuteNonQueryAsync(CommandSqlType.MultiCommand, cancellationToken);
        command.Parameters.Clear();
        await command.DisposeAsync();
        if (isNeedClose) await connection.CloseAsync();
        return result;
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
    //抛异常的时候，会走到析构函数，但是Transaction，没有提交也没有回滚
    private IQueryVisitor CreateQueryVisitor(char tableAsStart = 'a')
        => this.OrmProvider.NewQueryVisitor(this.DbContext, tableAsStart);
    #endregion
}
