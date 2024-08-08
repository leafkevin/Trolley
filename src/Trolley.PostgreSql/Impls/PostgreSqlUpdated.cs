using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley.PostgreSql;

public class PostgreSqlUpdated<TEntity> : Updated<TEntity>, IPostgreSqlUpdated<TEntity>
{
    #region Properties
    public PostgreSqlUpdateVisitor DialectVisitor { get; protected set; }
    public IOrmProvider OrmProvider => this.Visitor.OrmProvider;
    #endregion

    #region Constructor
    public PostgreSqlUpdated(DbContext dbContext, IUpdateVisitor visitor)
        : base(dbContext, visitor)
    {
        this.DialectVisitor = this.Visitor as PostgreSqlUpdateVisitor;
    }
    #endregion

    #region Execute
    public override int Execute()
    {
        int result = 0;
        Exception exception = null;
        bool isNeedClose = this.DbContext.IsNeedClose;
        try
        {
            bool isOpened = false;
            switch (this.Visitor.ActionMode)
            {
                case ActionMode.BulkCopy:
                    {
                        (var updateObjs, var timeoutSeconds) = this.DialectVisitor.BuildWithBulkCopy();
                        Type updateObjType = null;
                        foreach (var updateObj in updateObjs)
                        {
                            updateObjType = updateObj.GetType();
                            break;
                        }
                        if (updateObjType == null) throw new Exception("批量更新，updateObjs参数至少要有一条数据");
                        var fromMapper = this.Visitor.Tables[0].Mapper;
                        var memberMappers = this.Visitor.GetRefMemberMappers(updateObjType, fromMapper, true);
                        var tableName = this.OrmProvider.GetTableName($"{fromMapper.TableName}_{Guid.NewGuid():N}");

                        //添加临时表
                        var builder = new StringBuilder();
                        builder.AppendLine($"CREATE TEMPORARY TABLE {tableName}(");
                        var pkColumns = new List<string>();
                        foreach (var memberMapper in memberMappers)
                        {
                            var refMemberMapper = memberMapper.RefMemberMapper;
                            var fieldName = this.OrmProvider.GetFieldName(refMemberMapper.FieldName);
                            builder.Append($"{fieldName} {refMemberMapper.DbColumnType}");
                            if (refMemberMapper.IsKey)
                            {
                                builder.Append(" NOT NULL");
                                pkColumns.Add(fieldName);
                            }
                            builder.AppendLine(",");
                        }
                        builder.AppendLine($"PRIMARY KEY({string.Join(',', pkColumns)})");
                        builder.AppendLine(");");
                        if (this.Visitor.IsNeedFetchShardingTables)
                            builder.Append(this.Visitor.BuildShardingTablesSql(this.DbContext.TableSchema));

                        var command = this.DbContext.CreateCommand();
                        command.CommandText = builder.ToString();
                        this.DbContext.Open();
                        command.ExecuteNonQuery();

                        builder.Clear();
                        builder.Append($"COPY {tableName}(");
                        for (int i = 0; i < memberMappers.Count; i++)
                        {
                            if (i > 0) builder.Append(',');
                            var refMemberMapper = memberMappers[i].RefMemberMapper;
                            builder.Append(this.OrmProvider.GetFieldName(refMemberMapper.FieldName));
                        }
                        builder.Append(") FROM STDIN BINARY");
                        var bulkCopySql = builder.ToString();

                        builder.Clear();
                        Action<string, string> sqlExecutor = (target, source) =>
                        {
                            builder.Append($"UPDATE {this.OrmProvider.GetTableName(target)} a SET ");
                            int setIndex = 0;
                            for (int i = 0; i < memberMappers.Count; i++)
                            {
                                var fieldName = this.Visitor.OrmProvider.GetFieldName(memberMappers[i].RefMemberMapper.FieldName);
                                if (pkColumns.Contains(fieldName)) continue;
                                if (setIndex > 0) builder.Append(',');
                                builder.Append($"{fieldName}=b.{fieldName}");
                                setIndex++;
                            }
                            builder.Append($" FROM {source} b WHERE ");
                            for (int i = 0; i < pkColumns.Count; i++)
                            {
                                if (i > 0) builder.Append(" AND ");
                                builder.Append($"a.{pkColumns[i]}=b.{pkColumns[i]}");
                            }
                        };
                        if (this.Visitor.ShardingTables != null && this.Visitor.ShardingTables.Count > 0)
                        {
                            var tableNames = this.Visitor.ShardingTables[0].TableNames;
                            for (int i = 0; i < tableNames.Count; i++)
                            {
                                if (i > 0) builder.Append(';');
                                sqlExecutor.Invoke(this.Visitor.Tables[0].Body ?? tableNames[i], tableName);
                            }
                        }
                        else sqlExecutor.Invoke(fromMapper.TableName, tableName);
                        builder.Append($";DROP TABLE {tableName}");
                        var updateSql = builder.ToString();

                        var connection = this.DbContext.Connection as NpgsqlConnection;
                        using (var writer = connection.BeginBinaryImport(bulkCopySql))
                        {
                            foreach (var updateObj in updateObjs)
                            {
                                writer.StartRow();
                                foreach (var memberMapper in memberMappers)
                                {
                                    var refMemberMapper = memberMapper.RefMemberMapper;
                                    var fieldValue = memberMapper.ValueGetter.Invoke(updateObj);
                                    writer.Write(fieldValue, (NpgsqlDbType)refMemberMapper.NativeDbType);
                                }
                                result++;
                            }
                            writer.Complete();
                        }
                        command.CommandText = updateSql;
                        result = command.ExecuteNonQuery();
                        command.Parameters.Clear();
                        command.Dispose();
                        command = null;
                    }
                    break;
                case ActionMode.Bulk:
                    {
                        using var command = this.DbContext.CreateCommand();
                        var builder = new StringBuilder();
                        (var updateObjs, var bulkCount, var tableName, var firstParametersSetter,
                            var firstSqlParametersSetter, var headSqlSetter, var sqlSetter) = this.Visitor.BuildWithBulk(command);
                        Func<int, string> suffixGetter = index => this.Visitor.IsMultiple ? $"_m{this.Visitor.CommandIndex}{index}" : $"{index}";

                        Action<object, int> sqlExecuter = null;
                        if (this.Visitor.ShardingTables != null && this.Visitor.ShardingTables.Count > 0)
                        {
                            sqlExecuter = (updateObj, index) =>
                            {
                                if (index > 0) builder.Append(';');
                                var tableNames = this.Visitor.ShardingTables[0].TableNames;
                                headSqlSetter.Invoke(builder, tableNames[0]);
                                firstSqlParametersSetter.Invoke(command.Parameters, builder, this.DbContext.OrmProvider, updateObj, suffixGetter.Invoke(index));

                                for (int i = 1; i < tableNames.Count; i++)
                                {
                                    builder.Append(';');
                                    headSqlSetter.Invoke(builder, tableNames[i]);
                                    sqlSetter.Invoke(builder, this.DbContext.OrmProvider, updateObj, suffixGetter.Invoke(index));
                                }
                            };
                        }
                        else
                        {
                            sqlExecuter = (updateObj, index) =>
                            {
                                if (index > 0) builder.Append(';');
                                headSqlSetter.Invoke(builder, tableName);
                                firstSqlParametersSetter.Invoke(command.Parameters, builder, this.DbContext.OrmProvider, updateObj, suffixGetter.Invoke(index));
                            };
                        }
                        if (this.Visitor.IsNeedFetchShardingTables)
                        {
                            this.DbContext.FetchShardingTables(this.Visitor as SqlVisitor);
                            isOpened = true;
                        }
                        int index = 0;
                        firstParametersSetter?.Invoke(command.Parameters);
                        foreach (var updateObj in updateObjs)
                        {
                            sqlExecuter.Invoke(updateObj, index);
                            if (index >= bulkCount)
                            {
                                command.CommandText = builder.ToString();
                                if (!isOpened)
                                {
                                    this.DbContext.Open();
                                    isOpened = true;
                                }
                                result += command.ExecuteNonQuery();
                                command.Parameters.Clear();
                                firstParametersSetter?.Invoke(command.Parameters);
                                builder.Clear();
                                index = 0;
                                continue;
                            }
                            index++;
                        }
                        if (index > 0)
                        {
                            command.CommandText = builder.ToString();
                            if (!isOpened) this.DbContext.Open();
                            result += command.ExecuteNonQuery();
                        }
                        builder.Clear();
                        builder = null;
                    }
                    break;
                default:
                    {
                        if (!this.Visitor.HasWhere)
                            throw new InvalidOperationException("缺少where条件，请使用Where/And方法完成where条件");

                        if (this.Visitor.IsNeedFetchShardingTables)
                        {
                            this.DbContext.FetchShardingTables(this.Visitor as SqlVisitor);
                            isOpened = true;
                        }
                        using var command = this.DbContext.CreateCommand();
                        command.CommandText = this.Visitor.BuildCommand(this.DbContext, command);
                        if (!isOpened) this.DbContext.Open();
                        result = command.ExecuteNonQuery();
                        command.Parameters.Clear();
                        command.Dispose();
                    }
                    break;
            }
        }
        catch (Exception ex)
        {
            isNeedClose = true;
            exception = ex;
        }
        finally
        {
            if (isNeedClose) this.Close();
        }
        if (exception != null) throw exception;
        return result;
    }
    public override async Task<int> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        int result = 0;
        Exception exception = null;
        bool isNeedClose = this.DbContext.IsNeedClose;
        try
        {
            bool isOpened = false;
            switch (this.Visitor.ActionMode)
            {
                case ActionMode.BulkCopy:
                    {
                        (var updateObjs, var timeoutSeconds) = this.DialectVisitor.BuildWithBulkCopy();
                        Type updateObjType = null;
                        foreach (var updateObj in updateObjs)
                        {
                            updateObjType = updateObj.GetType();
                            break;
                        }
                        if (updateObjType == null) throw new Exception("批量更新，updateObjs参数至少要有一条数据");
                        var fromMapper = this.Visitor.Tables[0].Mapper;
                        var memberMappers = this.Visitor.GetRefMemberMappers(updateObjType, fromMapper, true);
                        var tableName = this.OrmProvider.GetTableName($"{fromMapper.TableName}_{Guid.NewGuid():N}");

                        //添加临时表
                        var builder = new StringBuilder();
                        builder.AppendLine($"CREATE TEMPORARY TABLE {tableName}(");
                        var pkColumns = new List<string>();
                        foreach (var memberMapper in memberMappers)
                        {
                            var refMemberMapper = memberMapper.RefMemberMapper;
                            var fieldName = this.OrmProvider.GetFieldName(refMemberMapper.FieldName);
                            builder.Append($"{fieldName} {refMemberMapper.DbColumnType}");
                            if (refMemberMapper.IsKey)
                            {
                                builder.Append(" NOT NULL");
                                pkColumns.Add(fieldName);
                            }
                            builder.AppendLine(",");
                        }
                        builder.AppendLine($"PRIMARY KEY({string.Join(',', pkColumns)})");
                        builder.AppendLine(");");
                        if (this.Visitor.IsNeedFetchShardingTables)
                            builder.Append(this.Visitor.BuildShardingTablesSql(this.DbContext.TableSchema));

                        var command = this.DbContext.CreateDbCommand();
                        command.CommandText = builder.ToString();
                        await this.DbContext.OpenAsync(cancellationToken);
                        await command.ExecuteNonQueryAsync(cancellationToken);

                        builder.Clear();
                        builder.Append($"COPY {tableName}(");
                        for (int i = 0; i < memberMappers.Count; i++)
                        {
                            if (i > 0) builder.Append(',');
                            var refMemberMapper = memberMappers[i].RefMemberMapper;
                            builder.Append(this.OrmProvider.GetFieldName(refMemberMapper.FieldName));
                        }
                        builder.Append(") FROM STDIN BINARY");
                        var bulkCopySql = builder.ToString();

                        builder.Clear();
                        Action<string, string> sqlExecutor = (target, source) =>
                        {
                            builder.Append($"UPDATE {this.DbContext.OrmProvider.GetTableName(target)} a SET ");
                            int setIndex = 0;
                            for (int i = 0; i < memberMappers.Count; i++)
                            {
                                var fieldName = this.Visitor.OrmProvider.GetFieldName(memberMappers[i].RefMemberMapper.FieldName);
                                if (pkColumns.Contains(fieldName)) continue;
                                if (setIndex > 0) builder.Append(',');
                                builder.Append($"{fieldName}=b.{fieldName}");
                                setIndex++;
                            }
                            builder.Append($" FROM {source} b WHERE ");
                            for (int i = 0; i < pkColumns.Count; i++)
                            {
                                if (i > 0) builder.Append(" AND ");
                                builder.Append($"a.{pkColumns[i]}=b.{pkColumns[i]}");
                            }
                        };
                        if (this.Visitor.ShardingTables != null && this.Visitor.ShardingTables.Count > 0)
                        {
                            var tableNames = this.Visitor.ShardingTables[0].TableNames;
                            for (int i = 0; i < tableNames.Count; i++)
                            {
                                if (i > 0) builder.Append(';');
                                sqlExecutor.Invoke(this.Visitor.Tables[0].Body ?? tableNames[i], tableName);
                            }
                        }
                        else sqlExecutor.Invoke(fromMapper.TableName, tableName);
                        builder.Append($";DROP TABLE {tableName}");
                        var updateSql = builder.ToString();

                        var connection = this.DbContext.Connection as NpgsqlConnection;
                        using (var writer = await connection.BeginBinaryImportAsync(bulkCopySql, cancellationToken))
                        {
                            foreach (var updateObj in updateObjs)
                            {
                                await writer.StartRowAsync(cancellationToken);
                                foreach (var memberMapper in memberMappers)
                                {
                                    var refMemberMapper = memberMapper.RefMemberMapper;
                                    var fieldValue = memberMapper.ValueGetter.Invoke(updateObj);
                                    await writer.WriteAsync(fieldValue, (NpgsqlDbType)refMemberMapper.NativeDbType, cancellationToken);
                                }
                                result++;
                            }
                            await writer.CompleteAsync(cancellationToken);
                        }
                        command.CommandText = updateSql;
                        result = await command.ExecuteNonQueryAsync(cancellationToken);
                        command.Parameters.Clear();
                        command.Dispose();
                        command = null;
                    }
                    break;
                case ActionMode.Bulk:
                    {
                        using var command = this.DbContext.CreateDbCommand();
                        var builder = new StringBuilder();
                        (var updateObjs, var bulkCount, var tableName, var firstParametersSetter,
                            var firstSqlParametersSetter, var headSqlSetter, var sqlSetter) = this.Visitor.BuildWithBulk(command);
                        Func<int, string> suffixGetter = index => this.Visitor.IsMultiple ? $"_m{this.Visitor.CommandIndex}{index}" : $"{index}";

                        Action<object, int> sqlExecuter = null;
                        if (this.Visitor.ShardingTables != null && this.Visitor.ShardingTables.Count > 0)
                        {
                            sqlExecuter = (updateObj, index) =>
                            {
                                if (index > 0) builder.Append(';');
                                var tableNames = this.Visitor.ShardingTables[0].TableNames;
                                headSqlSetter.Invoke(builder, tableNames[0]);
                                firstSqlParametersSetter.Invoke(command.Parameters, builder, this.DbContext.OrmProvider, updateObj, suffixGetter.Invoke(index));

                                for (int i = 1; i < tableNames.Count; i++)
                                {
                                    builder.Append(';');
                                    headSqlSetter.Invoke(builder, tableNames[i]);
                                    sqlSetter.Invoke(builder, this.DbContext.OrmProvider, updateObj, suffixGetter.Invoke(index));
                                }
                            };
                        }
                        else
                        {
                            sqlExecuter = (updateObj, index) =>
                            {
                                if (index > 0) builder.Append(';');
                                headSqlSetter.Invoke(builder, tableName);
                                firstSqlParametersSetter.Invoke(command.Parameters, builder, this.DbContext.OrmProvider, updateObj, suffixGetter.Invoke(index));
                            };
                        }
                        if (this.Visitor.IsNeedFetchShardingTables)
                        {
                            await this.DbContext.FetchShardingTablesAsync(this.Visitor as SqlVisitor, cancellationToken);
                            isOpened = true;
                        }
                        int index = 0;
                        firstParametersSetter?.Invoke(command.Parameters);
                        foreach (var updateObj in updateObjs)
                        {
                            sqlExecuter.Invoke(updateObj, index);
                            if (index >= bulkCount)
                            {
                                command.CommandText = builder.ToString();
                                if (!isOpened)
                                {
                                    await this.DbContext.OpenAsync(cancellationToken);
                                    isOpened = true;
                                }
                                result += await command.ExecuteNonQueryAsync(cancellationToken);
                                command.Parameters.Clear();
                                firstParametersSetter?.Invoke(command.Parameters);
                                builder.Clear();
                                index = 0;
                                continue;
                            }
                            index++;
                        }
                        if (index > 0)
                        {
                            command.CommandText = builder.ToString();
                            if (!isOpened) await this.DbContext.OpenAsync(cancellationToken);
                            result += await command.ExecuteNonQueryAsync(cancellationToken);
                        }
                        builder.Clear();
                        builder = null;
                    }
                    break;
                default:
                    {
                        if (!this.Visitor.HasWhere)
                            throw new InvalidOperationException("缺少where条件，请使用Where/And方法完成where条件");

                        if (this.Visitor.IsNeedFetchShardingTables)
                        {
                            this.DbContext.FetchShardingTables(this.Visitor as SqlVisitor);
                            isOpened = true;
                        }
                        using var command = this.DbContext.CreateCommand();
                        command.CommandText = this.Visitor.BuildCommand(this.DbContext, command);
                        if (!isOpened) this.DbContext.Open();
                        result = command.ExecuteNonQuery();
                        command.Parameters.Clear();
                        command.Dispose();
                    }
                    break;
            }
        }
        catch (Exception ex)
        {
            isNeedClose = true;
            exception = ex;
        }
        finally
        {
            if (isNeedClose) this.Close();
        }
        if (exception != null) throw exception;
        return result;
    }
    #endregion

    #region ToSql
    public override string ToSql(out List<IDbDataParameter> dbParameters)
    {
        string sql = null;
        dbParameters = null;
        var builder = new StringBuilder();
        if (this.Visitor.ActionMode == ActionMode.BulkCopy)
        {
            (var updateObjs, var timeoutSeconds) = this.DialectVisitor.BuildWithBulkCopy();
            Type updateObjType = null;
            foreach (var updateObj in updateObjs)
            {
                updateObjType = updateObj.GetType();
                break;
            }
            if (updateObjType == null) throw new Exception("批量更新，updateObjs参数至少要有一条数据");
            var fromMapper = this.Visitor.Tables[0].Mapper;
            var memberMappers = this.Visitor.GetRefMemberMappers(updateObjType, fromMapper, true);
            var ormProvider = this.Visitor.OrmProvider;
            var tableName = ormProvider.GetTableName($"{fromMapper.TableName}_{Guid.NewGuid():N}");

            //添加临时表
            builder.AppendLine($"CREATE TEMPORARY TABLE {tableName}(");
            var pkColumns = new List<string>();
            foreach (var memberMapper in memberMappers)
            {
                var refMemberMapper = memberMapper.RefMemberMapper;
                var fieldName = ormProvider.GetFieldName(refMemberMapper.FieldName);
                builder.Append($"{fieldName} {refMemberMapper.DbColumnType}");
                if (refMemberMapper.IsKey)
                {
                    builder.Append(" NOT NULL");
                    pkColumns.Add(fieldName);
                }
                builder.AppendLine(",");
            }
            builder.AppendLine($"PRIMARY KEY({string.Join(',', pkColumns)})");
            builder.AppendLine(");");
            if (this.Visitor.IsNeedFetchShardingTables)
            {
                builder.Append(this.Visitor.BuildShardingTablesSql(this.DbContext.TableSchema));
                builder.Append(';');
            }

            Action<string, string> sqlExecutor = (target, source) =>
            {
                builder.Append($"UPDATE {this.DbContext.OrmProvider.GetTableName(target)} a INNER JOIN {source} b ON ");
                for (int i = 0; i < pkColumns.Count; i++)
                {
                    if (i > 0) builder.Append(" AND ");
                    builder.Append($"a.{pkColumns[i]}=b.{pkColumns[i]}");
                }
                builder.Append(" SET ");
                int setIndex = 0;
                for (int i = 0; i < memberMappers.Count; i++)
                {
                    var fieldName = this.Visitor.OrmProvider.GetFieldName(memberMappers[i].RefMemberMapper.FieldName);
                    if (pkColumns.Contains(fieldName)) continue;
                    if (setIndex > 0) builder.Append(',');
                    builder.Append($"a.{fieldName}=b.{fieldName}");
                    setIndex++;
                }
            };
            if (this.Visitor.ShardingTables != null && this.Visitor.ShardingTables.Count > 0)
            {
                var tableNames = this.Visitor.ShardingTables[0].TableNames;
                for (int i = 0; i < tableNames.Count; i++)
                {
                    if (i > 0) builder.Append(';');
                    sqlExecutor.Invoke(this.Visitor.Tables[0].Body ?? tableNames[i], tableName);
                }
            }
            else sqlExecutor.Invoke(fromMapper.TableName, tableName);
            builder.Append($";DROP TABLE {tableName}");
            sql = builder.ToString();
        }
        else
        {
            if (this.Visitor.IsNeedFetchShardingTables)
            {
                this.DbContext.FetchShardingTables(this.Visitor as SqlVisitor);
                builder.Append(this.Visitor.BuildShardingTablesSql(this.DbContext.TableSchema));
            }
            using var command = this.DbContext.CreateCommand();
            sql = this.Visitor.BuildCommand(this.DbContext, command);
            if (this.Visitor.IsNeedFetchShardingTables)
            {
                builder.Append(sql);
                sql = builder.ToString();
            }
            dbParameters = this.Visitor.DbParameters.Cast<IDbDataParameter>().ToList();
        }
        builder.Clear();
        builder = null;
        return sql;
    }
    #endregion
}
