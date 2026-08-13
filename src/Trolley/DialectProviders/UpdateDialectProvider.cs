using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley;

public class UpdateDialectProvider : DialectProvider
{
    #region Update
    public int Update<TEntity>(object updateObj)
    {
        (var isNeedClose, var connection, var command) = this.CreateUpdateCommand(typeof(TEntity), updateObj);
        connection.Open();
        var result = command.ExecuteNonQuery(CommandSqlType.Update);

        command.Dispose();
        if (isNeedClose) connection.Close();
        return result;
    }
    public async Task<int> UpdateAsync<TEntity>(object updateObj, CancellationToken cancellationToken = default)
    {
        (var isNeedClose, var connection, var command) = this.CreateUpdateCommand(typeof(TEntity), updateObj);
        await connection.OpenAsync(cancellationToken);
        var result = await command.ExecuteNonQueryAsync(CommandSqlType.Update, cancellationToken);

        await command.DisposeAsync();
        if (isNeedClose) await connection.CloseAsync();
        return result;
    }
    private (bool, ITheaConnection, ITheaCommand) CreateUpdateCommand(Type entityType, object updateObj)
    {
        if (updateObj == null)
            throw new ArgumentNullException(nameof(updateObj));

        (var isNeedClose, var connection, var command) = this.UseMasterCommand();
        if (updateObj is IDictionary<string, object> dict)
        {
            var entityMapper = this.entityMapProvider.GetEntityMap(entityType);
            int index = 0;
            var fieldsBuilder = new StringBuilder();
            var whereBuilder = new StringBuilder();
            foreach (var key in dict.Keys)
            {
                if (!entityMapper.TryGetMemberMap(key, out var memberMapper)
                    || memberMapper.IsIgnore || memberMapper.IsNavigation
                    || memberMapper.IsIgnoreUpdate || memberMapper.IsRowVersion)
                    continue;

                var fieldValue = dict[key];
                var parameterName = $"{this.ormProvider.ParameterPrefix}{memberMapper.MemberName}";
                if (fieldsBuilder.Length > 0) fieldsBuilder.Append(',');
                if (whereBuilder.Length > 0) whereBuilder.Append(" AND ");
                var sql = $"{this.ormProvider.GetFieldName(memberMapper.FieldName)}={parameterName}";
                if (memberMapper.IsKey) whereBuilder.Append(sql);
                else fieldsBuilder.Append(sql);

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
            command.CommandText = $"UPDATE {this.ormProvider.GetTableName(entityMapper.TableName)} SET {fieldsBuilder.ToString()} WHERE ({whereBuilder.ToString()})";
        }
        else
        {
            if (updateObj is IEnumerable && updateObj is not string)
                throw new NotSupportedException("此方法只支持单条数据更新");

            var parameterType = updateObj.GetType();
            var commandInitializer = RepositoryHelper.BuildTypedCommandInitializer(this.DbContext, entityType, parameterType, 2, true, false, null, null)
                as Func<IDataParameterCollection, DbContext, object, string>;
            command.CommandText = commandInitializer.Invoke(command.Parameters, this.DbContext, updateObj);
        }
        return (isNeedClose, connection, command);
    }

    public int Update<TEntity>(IEnumerable updateObjs, int bulkCount)
    {
        (var isNeedClose, var connection, var command, var commandInitializer) =
            this.CreateUpdateBulkCommand(typeof(TEntity), updateObjs, bulkCount);
        int index = 0, result = 0;
        var builder = new StringBuilder();

        connection.Open();
        foreach (var updateObj in updateObjs)
        {
            if (index > 0) builder.Append(';');
            commandInitializer.Invoke(command.Parameters, builder, this.DbContext, updateObj, index.ToString());
            index++;

            if (index >= bulkCount)
            {
                command.CommandText = builder.ToString();
                result += command.ExecuteNonQuery(CommandSqlType.BulkInsert);
                builder.Clear();
                command.Parameters.Clear();
                index = 0;
            }
        }
        if (index > 0)
        {
            command.CommandText = builder.ToString();
            result += command.ExecuteNonQuery(CommandSqlType.BulkInsert);
            builder.Clear();
            command.Parameters.Clear();
        }

        command.Dispose();
        if (isNeedClose) connection.Close();
        return result;
    }
    public async Task<int> UpdateAsync<TEntity>(IEnumerable updateObjs, int bulkCount, CancellationToken cancellationToken = default)
    {
        (var isNeedClose, var connection, var command, var commandInitializer) =
             this.CreateUpdateBulkCommand(typeof(TEntity), updateObjs, bulkCount);
        int index = 0, result = 0;
        var builder = new StringBuilder();

        await connection.OpenAsync(cancellationToken);
        foreach (var updateObj in updateObjs)
        {
            if (index > 0) builder.Append(';');
            commandInitializer.Invoke(command.Parameters, builder, this.DbContext, updateObj, index.ToString());
            index++;

            if (index >= bulkCount)
            {
                command.CommandText = builder.ToString();
                result += await command.ExecuteNonQueryAsync(CommandSqlType.BulkUpdate, cancellationToken);
                builder.Clear();
                command.Parameters.Clear();
                index = 0;
            }
        }
        if (index > 0)
        {
            command.CommandText = builder.ToString();
            result += await command.ExecuteNonQueryAsync(CommandSqlType.BulkUpdate, cancellationToken);
            builder.Clear();
            command.Parameters.Clear();
        }

        await command.DisposeAsync();
        if (isNeedClose) await connection.CloseAsync();
        return result;
    }
    private (bool, ITheaConnection, ITheaCommand, Action<IDataParameterCollection, StringBuilder, DbContext, object, string>)
        CreateUpdateBulkCommand(Type entityType, IEnumerable updateObjs, int bulkCount)
    {
        if (updateObjs == null)
            throw new ArgumentNullException(nameof(updateObjs));
        if (bulkCount <= 0)
            throw new ArgumentOutOfRangeException("bulkCount必须大于0");

        (var isNeedClose, var connection, var command) = this.UseMasterCommand();
        object firstUpdateObj = null;
        foreach (var updateObj in updateObjs)
        {
            if (updateObj == null) throw new ArgumentNullException(nameof(updateObj));
            firstUpdateObj = updateObj;
            break;
        }
        var updateObjType = firstUpdateObj.GetType();

        Action<IDataParameterCollection, StringBuilder, DbContext, object, string> commandInitializer = null;
        if (firstUpdateObj is IDictionary<string, object> dict)
        {
            var entityMapper = this.entityMapProvider.GetEntityMap(entityType);
            var valueSetters = new List<Action<IDataParameterCollection, StringBuilder, IDictionary<string, object>, string>>();
            var whereSetters = new List<Action<IDataParameterCollection, StringBuilder, IDictionary<string, object>, string>>();
            foreach (var key in dict.Keys)
            {
                if (!entityMapper.TryGetMemberMap(key, out var memberMapper)
                    || memberMapper.IsIgnore || memberMapper.IsNavigation
                    || memberMapper.IsIgnoreUpdate || memberMapper.IsRowVersion)
                    continue;

                Func<IDictionary<string, object>, object> valueGetter = null;
                if (memberMapper.TypeHandler != null)
                    valueGetter = updateObj => memberMapper.TypeHandler.ToFieldValue(updateObj[key]);
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
                            valueGetter = updateObj => myValueGetter.Invoke(updateObj[key]);
                        }
                        else valueGetter = updateObj => updateObj[key];
                    }
                    else
                    {
                        if (fieldValue != null)
                        {
                            var fieldValueType = dict[key].GetType();
                            if (fieldValueType.ToUnderlyingType() != targetType)
                            {
                                var myValueGetter = this.ormProvider.GetParameterValueGetter(fieldValueType, targetType, !memberMapper.IsRequired, this.options);
                                valueGetter = updateObj =>
                                {
                                    var fieldValue = updateObj[key];
                                    return fieldValue == null ? memberMapper.DefaultValue : myValueGetter.Invoke(fieldValue);
                                };
                            }
                            else valueGetter = updateObj => updateObj[key] ?? memberMapper.DefaultValue;
                        }
                        else valueGetter = updateObj => updateObj[key] ?? memberMapper.DefaultValue;
                    }
                }

                Action<IDataParameterCollection, StringBuilder, IDictionary<string, object>, string> valueSetter = null;
                if (memberMapper.IsKey)
                {
                    if (whereSetters.Count > 0)
                    {
                        valueSetter = (dbParameters, builder, insertObj, suffix) =>
                        {
                            var fieldValue = valueGetter.Invoke(insertObj);
                            builder.Append(" AND ");
                            var parameterName = $"{this.ormProvider.ParameterPrefix}{memberMapper.MemberName}{suffix}";
                            builder.Append($"{this.ormProvider.GetFieldName(memberMapper.FieldName)}={parameterName}");
                            dbParameters.Add(this.ormProvider.CreateParameter(parameterName, memberMapper.NativeDbType, fieldValue));
                        };
                    }
                    else
                    {
                        valueSetter = (dbParameters, builder, insertObj, suffix) =>
                        {
                            var fieldValue = valueGetter.Invoke(insertObj);
                            var parameterName = $"{this.ormProvider.ParameterPrefix}{memberMapper.MemberName}{suffix}";
                            builder.Append($"{this.ormProvider.GetFieldName(memberMapper.FieldName)}={parameterName}");
                            dbParameters.Add(this.ormProvider.CreateParameter(parameterName, memberMapper.NativeDbType, fieldValue));
                        };
                    }
                    whereSetters.Add(valueSetter);
                }
                else
                {
                    if (valueSetters.Count > 0)
                    {
                        valueSetter = (dbParameters, builder, insertObj, suffix) =>
                        {
                            var fieldValue = valueGetter.Invoke(insertObj);
                            builder.Append(',');
                            var parameterName = $"{this.ormProvider.ParameterPrefix}{memberMapper.MemberName}{suffix}";
                            builder.Append($"{this.ormProvider.GetFieldName(memberMapper.FieldName)}={parameterName}");
                            dbParameters.Add(this.ormProvider.CreateParameter(parameterName, memberMapper.NativeDbType, fieldValue));
                        };
                    }
                    else
                    {
                        valueSetter = (dbParameters, builder, insertObj, suffix) =>
                        {
                            var fieldValue = valueGetter.Invoke(insertObj);
                            var parameterName = $"{this.ormProvider.ParameterPrefix}{memberMapper.MemberName}{suffix}";
                            builder.Append($"{this.ormProvider.GetFieldName(memberMapper.FieldName)}={parameterName}");
                            dbParameters.Add(this.ormProvider.CreateParameter(parameterName, memberMapper.NativeDbType, fieldValue));
                        };
                    }
                    valueSetters.Add(valueSetter);
                }
            }
            commandInitializer = (dbParameters, builder, dbContext, insertObj, suffix) =>
            {
                var dictObj = insertObj as IDictionary<string, object>;
                builder.Append($"UPDATE {this.ormProvider.GetTableName(entityMapper.TableName)} SET ");
                foreach (var valueSetter in valueSetters)
                    valueSetter.Invoke(dbParameters, builder, dictObj, suffix);
                builder.Append(" WHERE ");
                foreach (var valueSetter in whereSetters)
                    valueSetter.Invoke(dbParameters, builder, dictObj, suffix);
            };
        }
        else commandInitializer = RepositoryHelper.BuildTypedBulkCommandInitializer(this.DbContext, entityType, updateObjType, 2, null, null)
            as Action<IDataParameterCollection, StringBuilder, DbContext, object, string>;
        return (isNeedClose, connection, command, commandInitializer);
    }
    #endregion
}