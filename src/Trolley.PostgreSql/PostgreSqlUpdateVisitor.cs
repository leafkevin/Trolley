using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Text;

namespace Trolley.PostgreSql;

public class PostgreSqlUpdateVisitor : UpdateVisitor, IUpdateVisitor
{
    public PostgreSqlUpdateVisitor(DbContext dbContext, char tableAsStart = 'a')
        : base(dbContext, tableAsStart) { }
    public override string BuildCommand(DbContext dbContext, IDbCommand command)
    {
        string sql = null;
        var builder = new StringBuilder();
        switch (this.ActionMode)
        {
            case ActionMode.Bulk:
                {
                    //此SQL只能用在多命令查询时和返回ToSql两个场景
                    (var updateObjs, var bulkCount, var tableName, var firstParametersSetter,
                        var firstSqlParametersSetter, var headSqlSetter, var sqlSetter) = this.BuildWithBulk(command);
                    Func<int, string> suffixGetter = index => this.IsMultiple ? $"_m{this.CommandIndex}{index}" : $"{index}";

                    Action<object, int> sqlExecuter = null;
                    if (this.ShardingTables != null && this.ShardingTables.Count > 0)
                    {
                        sqlExecuter = (updateObj, index) =>
                        {
                            if (index > 0) builder.Append(';');
                            var tableNames = this.ShardingTables[0].TableNames;
                            headSqlSetter.Invoke(builder, tableNames[0]);
                            firstSqlParametersSetter.Invoke(command.Parameters, builder, this.OrmProvider, updateObj, suffixGetter.Invoke(index));

                            for (int i = 1; i < tableNames.Count; i++)
                            {
                                builder.Append(';');
                                headSqlSetter.Invoke(builder, tableNames[i]);
                                sqlSetter.Invoke(builder, this.OrmProvider, updateObj, suffixGetter.Invoke(index));
                            }
                        };
                    }
                    else
                    {
                        sqlExecuter = (updateObj, index) =>
                        {
                            if (index > 0) builder.Append(';');
                            headSqlSetter.Invoke(builder, tableName);
                            firstSqlParametersSetter.Invoke(command.Parameters, builder, this.OrmProvider, updateObj, suffixGetter.Invoke(index));
                        };
                    }

                    int index = 0;
                    firstParametersSetter?.Invoke(command.Parameters);
                    foreach (var updateObj in updateObjs)
                    {
                        sqlExecuter.Invoke(updateObj, index);
                        index++;
                    }
                    sql = builder.ToString();
                }
                break;
            case ActionMode.Single:
                {
                    this.UpdateFields = new();
                    this.DbParameters ??= command.Parameters;
                    foreach (var deferredSegment in this.deferredSegments)
                    {
                        switch (deferredSegment.Type)
                        {
                            case "Set":
                                this.VisitSet(deferredSegment.Value as Expression);
                                break;
                            case "SetFrom":
                                this.IsNeedTableAlias = true;
                                this.VisitSet(deferredSegment.Value as Expression);
                                break;
                            case "SetField":
                                this.VisitSetField(deferredSegment.Value);
                                break;
                            case "SetWith":
                                this.VisitSetWith(deferredSegment.Value);
                                break;
                            case "SetFromField":
                                this.IsNeedTableAlias = true;
                                this.VisitSetFromField(deferredSegment.Value);
                                break;
                            case "Where":
                                this.VisitWhere(deferredSegment.Value as Expression);
                                break;
                            case "WhereWith":
                                this.VisitWhereWith(deferredSegment.Value);
                                break;
                            case "And":
                                this.VisitAnd(deferredSegment.Value as Expression);
                                break;
                        }
                    }

                    var aliasName = this.Tables[0].AliasName;
                    if (this.IsNeedTableAlias)
                        builder.Append($"AS {aliasName} ");

                    int index = 0;
                    builder.Append("SET ");
                    if (this.UpdateFields != null && this.UpdateFields.Count > 0)
                    {
                        foreach (var setField in this.UpdateFields)
                        {
                            if (index > 0) builder.Append(',');
                            builder.Append(setField);
                            index++;
                        }
                    }
                    string whereSql = this.WhereSql;
                    if (this.IsJoin)
                    {
                        builder.Append(" FROM ");
                        var whereBuildr = new StringBuilder();
                        for (var i = 1; i < this.Tables.Count; i++)
                        {
                            var tableSegment = this.Tables[i];
                            var tableName = this.GetTableName(this.Tables[i]);
                            if (i > 1)
                            {
                                builder.Append(',');
                                whereBuildr.Append(" AND ");
                            }
                            builder.Append($"{tableName} {tableSegment.AliasName}");
                            whereBuildr.Append(tableSegment.OnExpr);
                        }
                        if (!string.IsNullOrEmpty(this.WhereSql))
                        {
                            whereBuildr.Append(" AND ");
                            whereBuildr.Append(this.WhereSql);
                        }
                        whereSql = whereBuildr.ToString();
                    }
                    if (!string.IsNullOrEmpty(whereSql))
                    {
                        builder.Append(" WHERE ");
                        builder.Append(whereSql);
                    }
                    sql = builder.ToString();
                    builder.Clear();

                    if (this.IsJoin)
                    {
                        builder.Append($"UPDATE {this.GetTableName(this.Tables[0])} {sql}");
                        sql = builder.ToString();
                        if (this.ShardingTables != null && this.ShardingTables.Count > 0)
                            sql = dbContext.BuildShardingTablesSqlByFormat(this, sql, ";");
                    }
                    else
                    {
                        Action<string> headSqlSetter = null;
                        var tableSegment = this.Tables[0];
                        if (!string.IsNullOrEmpty(tableSegment.TableSchema))
                            headSqlSetter = tableName => builder.Append($"UPDATE {this.OrmProvider.GetTableName(tableSegment.TableSchema + "." + tableName)} ");
                        else headSqlSetter = tableName => builder.Append($"UPDATE {this.OrmProvider.GetTableName(tableName)} ");
                        if (this.ShardingTables != null && this.ShardingTables.Count > 0)
                        {
                            var tableNames = this.ShardingTables[0].TableNames;
                            if (tableNames.Count > 1) tableNames.Sort((x, y) => x.CompareTo(y));
                            for (int i = 0; i < tableNames.Count; i++)
                            {
                                if (i > 0) builder.Append(';');
                                headSqlSetter.Invoke(tableNames[i]);
                                builder.Append(sql);
                            }
                        }
                        else
                        {
                            var tableName = this.Tables[0].Mapper.TableName;
                            headSqlSetter.Invoke(this.Tables[0].Body ?? tableName);
                            builder.Append(sql);
                        }
                        sql = builder.ToString();
                    }
                }
                break;
        }
        builder.Clear();
        builder = null;
        return sql;
    }
    public override string BuildTableShardingsSql()
    {
        var builder = new StringBuilder($"SELECT a.relname FROM pg_class a,pg_namespace b WHERE a.relnamespace=b.oid AND a.relkind='r' AND ");
        var schemaBuilders = new Dictionary<string, StringBuilder>();
        foreach (var tableSegment in this.ShardingTables)
        {
            if (tableSegment.ShardingType > ShardingTableType.MultiTable)
            {
                var tableSchema = tableSegment.TableSchema ?? this.DefaultTableSchema;
                if (!schemaBuilders.TryGetValue(tableSchema, out var tableBuilder))
                    schemaBuilders.TryAdd(tableSchema, tableBuilder = new StringBuilder());

                if (tableBuilder.Length > 0) tableBuilder.Append(" OR ");
                tableBuilder.Append($"a.relname LIKE '{tableSegment.Mapper.TableName}%'");
            }
        }
        if (schemaBuilders.Count > 1)
            builder.Append('(');
        int index = 0;
        foreach (var schemaBuilder in schemaBuilders)
        {
            if (index > 0) builder.Append(" OR ");
            builder.Append($"b.nspname='{schemaBuilder.Key}' AND ({schemaBuilder.Value.ToString()})");
            index++;
        }
        if (schemaBuilders.Count > 1)
            builder.Append(')');
        return builder.ToString();
    }
    public void WithBulkCopy(IEnumerable updateObjs, int? timeoutSeconds)
    {
        this.ActionMode = ActionMode.BulkCopy;
        this.deferredSegments.Add(new CommandSegment
        {
            Type = "WithBulkCopy",
            Value = (updateObjs, timeoutSeconds)
        });
    }
    public (IEnumerable, int?) BuildWithBulkCopy() => ((IEnumerable, int?))this.deferredSegments[0].Value;
}
