using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley;

public class CreateDialectProvider : DialectProvider
{
    #region Create
    public int Create<TEntity>(object insertObj)
    {
        (var isNeedClose, var connection, var command) = this.CreateInsertCommand(typeof(TEntity), insertObj, false);
        connection.Open();
        var result = command.ExecuteNonQuery(CommandSqlType.Insert);

        command.Dispose();
        if (isNeedClose) connection.Close();
        return result;
    }
    public async Task<int> CreateAsync<TEntity>(object insertObj, CancellationToken cancellationToken = default)
    {
        (var isNeedClose, var connection, var command) = this.CreateInsertCommand(typeof(TEntity), insertObj, false);
        await connection.OpenAsync(cancellationToken);
        var result = await command.ExecuteNonQueryAsync(CommandSqlType.Insert, cancellationToken);

        await command.DisposeAsync();
        if (isNeedClose) await connection.CloseAsync();
        return result;
    }

    public int Create<TEntity>(IEnumerable insertObjs, int bulkCount)
    {
        if (insertObjs is IDictionary<string, object> dict)
            return this.Create<TEntity>(dict);

        (var isNeedClose, var connection, var command, var headSql, var commandInitializer)
            = this.CreateInsertBulkCommand(typeof(TEntity), insertObjs, bulkCount);

        connection.Open();
        int index = 0, result = 0;
        var builder = new StringBuilder(headSql);
        foreach (var insertObj in insertObjs)
        {
            commandInitializer.Invoke(command.Parameters, builder, this.DbContext, insertObj, index.ToString());
            index++;

            if (index >= bulkCount)
            {
                builder.Remove(builder.Length - 1, 1);
                command.CommandText = builder.ToString();
                result += command.ExecuteNonQuery(CommandSqlType.BulkInsert);
                builder.Clear();
                command.Parameters.Clear();
                builder.Append(headSql);
                index = 0;
            }
        }
        if (index > 0)
        {
            builder.Remove(builder.Length - 1, 1);
            command.CommandText = builder.ToString();
            result += command.ExecuteNonQuery(CommandSqlType.BulkInsert);
            builder.Clear();
            command.Parameters.Clear();
        }

        command.Dispose();
        if (isNeedClose) connection.Close();
        return result;
    }
    public async Task<int> CreateAsync<TEntity>(IEnumerable insertObjs, int bulkCount, CancellationToken cancellationToken = default)
    {
        if (insertObjs is IDictionary<string, object> dict)
            return await this.CreateAsync<TEntity>(dict, cancellationToken);

        (var isNeedClose, var connection, var command, var headSql, var commandInitializer)
            = this.CreateInsertBulkCommand(typeof(TEntity), insertObjs, bulkCount);

        await connection.OpenAsync(cancellationToken);
        int index = 0, result = 0;
        var builder = new StringBuilder(headSql);
        foreach (var insertObj in insertObjs)
        {
            commandInitializer.Invoke(command.Parameters, builder, this.DbContext, insertObj, index.ToString());
            index++;

            if (index >= bulkCount)
            {
                builder.Remove(builder.Length - 1, 1);
                command.CommandText = builder.ToString();
                result += await command.ExecuteNonQueryAsync(CommandSqlType.BulkInsert, cancellationToken);
                builder.Clear();
                command.Parameters.Clear();
                builder.Append(headSql);
                index = 0;
            }
        }
        if (index > 0)
        {
            builder.Remove(builder.Length - 1, 1);
            command.CommandText = builder.ToString();
            result += await command.ExecuteNonQueryAsync(CommandSqlType.BulkInsert, cancellationToken);
            builder.Clear();
            command.Parameters.Clear();
        }

        await command.DisposeAsync();
        if (isNeedClose) await connection.CloseAsync();
        return result;
    }

    public TResult CreateIdentity<TEntity, TResult>(object insertObj)
    {
        TResult result = default;
        (var isNeedClose, var connection, var command) = this.CreateInsertCommand(typeof(TEntity), insertObj, true);

        connection.Open();
        var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        using var reader = command.ExecuteReader(CommandSqlType.Insert, behavior);
        if (reader.Read()) result = reader.ToValue<TResult>(this.DbContext);

        reader.Dispose();
        command.Dispose();
        if (isNeedClose) connection.Close();
        return result;
    }
    public async Task<TResult> CreateIdentityAsync<TEntity, TResult>(object insertObj, CancellationToken cancellationToken = default)
    {
        TResult result = default;
        (var isNeedClose, var connection, var command) = this.CreateInsertCommand(typeof(TEntity), insertObj, true);

        await connection.OpenAsync(cancellationToken);
        var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        using var reader = await command.ExecuteReaderAsync(CommandSqlType.Insert, behavior, cancellationToken);
        if (await reader.ReadAsync(cancellationToken)) result = reader.ToValue<TResult>(this.DbContext);

        await reader.DisposeAsync();
        await command.DisposeAsync();
        if (isNeedClose) await connection.CloseAsync();
        return result;
    }
    private (bool, ITheaConnection, ITheaCommand) CreateInsertCommand(Type entityType, object insertObj, bool hasIdentity)
    {
        if (insertObj == null)
            throw new ArgumentNullException(nameof(insertObj));

        (var isNeedClose, var connection, var command) = this.UseMasterCommand();
        if (insertObj is IDictionary<string, object> dict)
        {
            var entityMapper = this.entityMapProvider.GetEntityMap(entityType);
            int index = 0;
            var fieldsBuilder = new StringBuilder();
            var valuesBuilder = new StringBuilder();
            foreach (var key in dict.Keys)
            {
                if (!entityMapper.TryGetMemberMap(key, out var memberMapper) || memberMapper.IsIgnore
                    || memberMapper.IsAutoIncrement || memberMapper.IsNavigation
                    || memberMapper.IsIgnoreInsert || memberMapper.IsRowVersion)
                    continue;

                var fieldValue = dict[key];
                var parameterName = $"{this.ormProvider.ParameterPrefix}{memberMapper.MemberName}";
                if (index > 0)
                {
                    fieldsBuilder.Append(',');
                    valuesBuilder.Append(',');
                }
                fieldsBuilder.Append(this.ormProvider.GetFieldName(memberMapper.FieldName));
                valuesBuilder.Append(parameterName);
                if (fieldValue == null)
                    fieldValue = DBNull.Value;
                else if (memberMapper.TypeHandler != null)
                    fieldValue = memberMapper.TypeHandler.ToFieldValue(fieldValue);
                else
                {
                    var targetType = memberMapper.MappedTargetType;
                    var fieldValueType = fieldValue.GetType();
                    if (fieldValueType != targetType)
                    {
                        var myValueGetter = this.ormProvider.GetParameterValueGetter(fieldValueType, targetType, !memberMapper.IsRequired, this.options);
                        fieldValue = myValueGetter.Invoke(fieldValue);
                    }
                }
                command.Parameters.Add(this.ormProvider.CreateParameter(parameterName, memberMapper.NativeDbType, fieldValue));
                index++;
            }
            command.CommandText = $"INSERT INTO {this.ormProvider.GetTableName(entityMapper.TableName)} ({fieldsBuilder.ToString()}) VALUES ({valuesBuilder.ToString()})";
            if (hasIdentity)
            {
                var keyFieldName = this.ormProvider.GetFieldName(entityMapper.KeyMembers[0].FieldName);
                command.CommandText += this.ormProvider.GetIdentitySql(keyFieldName);
            }
        }
        else
        {
            if (insertObj is IEnumerable && insertObj is not string)
                throw new NotSupportedException("此方法只支持单条数据插入");

            var parameterType = insertObj.GetType();
            var commandInitializer = RepositoryHelper.BuildTypedCommandInitializer(this.DbContext, entityType, parameterType, 1, true, hasIdentity, null, null)
                as Func<IDataParameterCollection, DbContext, object, string>;
            command.CommandText = commandInitializer.Invoke(command.Parameters, this.DbContext, insertObj);
        }
        return (isNeedClose, connection, command);
    }
    private (bool, ITheaConnection, ITheaCommand, string, Action<IDataParameterCollection, StringBuilder, DbContext, object, string>)
        CreateInsertBulkCommand(Type entityType, IEnumerable insertObjs, int bulkCount)
    {
        if (insertObjs == null)
            throw new ArgumentNullException(nameof(insertObjs));
        if (bulkCount <= 0)
            throw new ArgumentOutOfRangeException("bulkCount必须大于0");

        (var isNeedClose, var connection, var command) = this.UseMasterCommand();
        object firstInsertObj = null;
        foreach (var insertObj in insertObjs)
        {
            firstInsertObj = insertObj;
            break;
        }
        var insertObjType = firstInsertObj.GetType();

        string headSql = null;
        Action<IDataParameterCollection, StringBuilder, DbContext, object, string> commandInitializer = null;
        var entityMapper = this.entityMapProvider.GetEntityMap(entityType);
        if (firstInsertObj is IDictionary<string, object> dict)
        {
            int index = 0;
            var builder = new StringBuilder();
            var valueSetters = new List<Action<IDataParameterCollection, StringBuilder, IDictionary<string, object>, string>>();
            builder.Append($"INSERT INTO {this.ormProvider.GetTableName(entityMapper.TableName)} (");
            foreach (var key in dict.Keys)
            {
                if (!entityMapper.TryGetMemberMap(key, out var memberMapper) || memberMapper.IsIgnore
                    || memberMapper.IsAutoIncrement || memberMapper.IsNavigation
                    || memberMapper.IsIgnoreInsert || memberMapper.IsRowVersion)
                    continue;

                if (index > 0) builder.Append(',');
                builder.Append(this.ormProvider.GetFieldName(memberMapper.FieldName));
                Func<IDictionary<string, object>, object> valueGetter = null;

                if (memberMapper.TypeHandler != null)
                    valueGetter = insertObj => memberMapper.TypeHandler.ToFieldValue(insertObj[key]);
                else
                {
                    var targetType = memberMapper.MappedTargetType;
                    var fieldValue = dict[key];
                    if (memberMapper.IsRequired)
                    {
                        if (fieldValue == null)
                            throw new Exception($"实体{entityMapper.EntityType.FullName}表，字段{memberMapper.FieldName}为必填，值不能为空");

                        var fieldValueType = fieldValue.GetType();
                        if (fieldValueType.ToUnderlyingType() != targetType)
                        {
                            var myValueGetter = this.ormProvider.GetParameterValueGetter(fieldValueType, targetType, !memberMapper.IsRequired, this.options);
                            valueGetter = insertObj => myValueGetter.Invoke(insertObj[key]);
                        }
                        else valueGetter = insertObj => insertObj[key];
                    }
                    else
                    {
                        if (fieldValue != null)
                        {
                            var fieldValueType = dict[key].GetType();
                            if (fieldValueType.ToUnderlyingType() != targetType)
                            {
                                var myValueGetter = this.ormProvider.GetParameterValueGetter(fieldValueType, targetType, !memberMapper.IsRequired, this.options);
                                valueGetter = insertObj =>
                                {
                                    var fieldValue = insertObj[key];
                                    return fieldValue == null ? memberMapper.DefaultValue : myValueGetter.Invoke(fieldValue);
                                };
                            }
                            else valueGetter = insertObj => insertObj[key] ?? memberMapper.DefaultValue;
                        }
                        else valueGetter = insertObj => insertObj[key] ?? memberMapper.DefaultValue;
                    }
                }

                Action<IDataParameterCollection, StringBuilder, IDictionary<string, object>, string> valueSetter = null;
                if (index > 0)
                {
                    valueSetter = (dbParameters, builder, insertObj, suffix) =>
                    {
                        var fieldValue = valueGetter.Invoke(insertObj);
                        var parameterName = $"{this.ormProvider.ParameterPrefix}{memberMapper.MemberName}{suffix}";
                        builder.Append(',');
                        builder.Append(parameterName);
                        dbParameters.Add(this.ormProvider.CreateParameter(parameterName, memberMapper.NativeDbType, fieldValue));
                    };
                }
                else
                {
                    valueSetter = (dbParameters, builder, insertObj, suffix) =>
                    {
                        var fieldValue = valueGetter.Invoke(insertObj);
                        var parameterName = $"{this.ormProvider.ParameterPrefix}{memberMapper.MemberName}{suffix}";
                        builder.Append(parameterName);
                        dbParameters.Add(this.ormProvider.CreateParameter(parameterName, memberMapper.NativeDbType, fieldValue));
                    };
                }
                valueSetters.Add(valueSetter);
                index++;
            }
            builder.Append(") VALUES ");
            headSql = builder.ToString();
            builder.Clear();
            commandInitializer = (dbParameters, builder, dbContext, insertObj, suffix) =>
            {
                var dictObj = insertObj as IDictionary<string, object>;
                builder.Append('(');
                foreach (var valueSetter in valueSetters)
                    valueSetter.Invoke(dbParameters, builder, dictObj, suffix);
                builder.Append("),");
            };
        }
        else
        {
            (var fieldsSql, var typedCommandInitializer) = ((string, Action<IDataParameterCollection, StringBuilder, DbContext, string, string, object, string>))
                RepositoryHelper.BuildTypedBulkCommandInitializer(this.DbContext, entityType, insertObjType, 1, null, null);
            headSql = $"INSERT INTO {this.ormProvider.GetTableName(entityMapper.TableName)} ({fieldsSql}) VALUES ";
            commandInitializer = (dbParameters, builder, dbContext, insertObj, suffix) =>
                typedCommandInitializer.Invoke(dbParameters, builder, dbContext, "(", "),", insertObj, suffix);
        }
        return (isNeedClose, connection, command, headSql, commandInitializer);
    }

    public TResult CreateIdentity<TResult>(ICreateVisitor visitor)
    {
        TResult result = default;
        (var isNeedClose, var connection, var command) = this.UseMasterCommand(visitor);
        visitor.IsReturnIdentity = true;
        command.CommandText = visitor.BuildSql(command, out _);

        connection.Open();
        var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        using var reader = command.ExecuteReader(CommandSqlType.Insert, behavior);
        if (reader.Read()) result = reader.ToValue<TResult>(this.DbContext);

        reader.Dispose();
        command.Dispose();
        if (isNeedClose) connection.Close();
        return result;
    }
    public async Task<TResult> CreateIdentityAsync<TResult>(ICreateVisitor visitor, CancellationToken cancellationToken = default)
    {
        TResult result = default;
        (var isNeedClose, var connection, var command) = this.UseMasterCommand(visitor);
        visitor.IsReturnIdentity = true;
        command.CommandText = visitor.BuildSql(command, out _);

        await connection.OpenAsync(cancellationToken);
        var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
        using var reader = await command.ExecuteReaderAsync(CommandSqlType.Insert, behavior, cancellationToken);
        if (await reader.ReadAsync(cancellationToken)) result = reader.ToValue<TResult>(this.DbContext);

        await reader.DisposeAsync();
        await command.DisposeAsync();
        if (isNeedClose) await connection.CloseAsync();
        return result;
    }

    public TResult CreateResult<TTarget, TResult>(ICreateVisitor visitor, Func<ITheaDataReader, List<ReaderField>, Func<ITheaDataReader, List<ReaderField>, object>, TResult> readerInitializer)
    {
        (var isNeedClose, var connection, var command) = this.UseMasterCommand(visitor);
        command.CommandText = visitor.BuildSql(command, out var readerFields);

        connection.Open();
        using var reader = command.ExecuteReader(CommandSqlType.Insert, CommandBehavior.SequentialAccess);
        var deserializer = reader.GetReaderDeserializer(typeof(TTarget), this.DbContext, readerFields);
        var result = readerInitializer.Invoke(reader, readerFields, deserializer);

        reader.Dispose();
        command.Dispose();
        if (isNeedClose) connection.Close();
        return result;
    }
    public async Task<TResult> CreateResultAsync<TTarget, TResult>(ICreateVisitor visitor, Func<ITheaDataReader, List<ReaderField>, Func<ITheaDataReader, List<ReaderField>, object>, TResult> readerInitializer, CancellationToken cancellationToken = default)
    {
        (var isNeedClose, var connection, var command) = this.UseMasterCommand(visitor);
        command.CommandText = visitor.BuildSql(command, out var readerFields);

        await connection.OpenAsync(cancellationToken);
        using var reader = await command.ExecuteReaderAsync(CommandSqlType.Insert, CommandBehavior.SequentialAccess, cancellationToken);
        var deserializer = reader.GetReaderDeserializer(typeof(TTarget), this.DbContext, readerFields);
        var result = readerInitializer.Invoke(reader, readerFields, deserializer);

        await reader.DisposeAsync();
        await command.DisposeAsync();
        if (isNeedClose) await connection.CloseAsync();
        return result;
    }
    #endregion
}