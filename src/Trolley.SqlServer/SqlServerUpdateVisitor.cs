using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Text;

namespace Trolley.SqlServer;

public class SqlServerUpdateVisitor : UpdateVisitor, IUpdateVisitor
{
    public SqlServerUpdateVisitor(DbContext dbContext, char tableAsStart = 'a')
        : base(dbContext, tableAsStart) { }

    public override void Initialize(Type entityType, bool isMultiple = false, bool isFirst = true)
    {
        if (!isMultiple)
        {
            this.Tables = new();
            this.TableAliases = new();
            var mapper = this.MapProvider.GetEntityMap(entityType);
            this.Tables.Add(new TableSegment
            {
                TableType = TableType.Entity,
                EntityType = entityType,
                //默认别名就是表名，在SetFrom时使用的别名就是表名
                AliasName = this.OrmProvider.GetTableName(mapper.TableName),
                Mapper = mapper
            });
        }
        if (!isFirst) this.Clear();
    }
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
                    var entityType = this.Tables[0].EntityType;
                    //非Bulk场景
                    this.DbParameters ??= command.Parameters;
                    foreach (var deferredSegment in this.deferredSegments)
                    {
                        switch (deferredSegment.Type)
                        {
                            case "Set":
                                this.VisitSet(deferredSegment.Value as Expression);
                                break;
                            case "SetFrom":
                                this.VisitSet(deferredSegment.Value as Expression);
                                break;
                            case "SetField":
                                this.VisitSetField(deferredSegment.Value);
                                break;
                            case "SetWith":
                                this.VisitSetWith(deferredSegment.Value);
                                break;
                            case "SetFromField":
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
                    if (this.IsJoin)
                    {
                        builder.Append($"UPDATE {aliasName} SET ");
                        int index = 0;
                        if (this.UpdateFields != null && this.UpdateFields.Count > 0)
                        {
                            foreach (var setField in this.UpdateFields)
                            {
                                if (index > 0) builder.Append(',');
                                builder.Append($"{aliasName}.");
                                builder.Append(setField);
                                index++;
                            }
                        }
                        builder.Append($" FROM {this.GetTableName(this.Tables[0])} {aliasName}");
                        for (var i = 1; i < this.Tables.Count; i++)
                        {
                            var tableSegment = this.Tables[i];
                            var tableName = this.GetTableName(tableSegment);
                            builder.Append($" {tableSegment.JoinType} {tableName} {tableSegment.AliasName}");
                            builder.Append($" ON {tableSegment.OnExpr}");
                        }
                    }
                    else
                    {
                        if (this.IsNeedTableAlias)
                            builder.Append($" {aliasName}");

                        int index = 0;
                        builder.Append(" SET ");
                        if (this.UpdateFields != null && this.UpdateFields.Count > 0)
                        {
                            foreach (var setField in this.UpdateFields)
                            {
                                if (index > 0) builder.Append(',');
                                if (this.IsNeedTableAlias) builder.Append($"{aliasName}.");
                                builder.Append(setField);
                                index++;
                            }
                        }
                    }
                    if (!string.IsNullOrEmpty(this.WhereSql))
                    {
                        builder.Append(" WHERE ");
                        builder.Append(this.WhereSql);
                    }
                    sql = builder.ToString();
                    builder.Clear();

                    if (this.IsJoin)
                    {
                        if (this.ShardingTables != null && this.ShardingTables.Count > 0)
                            sql = dbContext.BuildShardingTablesSqlByFormat(this, sql, ";");
                    }
                    else
                    {
                        Action<string> headSqlSetter = null;
                        var tableSegment = this.Tables[0];
                        if (!string.IsNullOrEmpty(tableSegment.TableSchema))
                            headSqlSetter = tableName => builder.Append($"UPDATE {this.OrmProvider.GetTableName(tableSegment.TableSchema + "." + tableName)}");
                        else headSqlSetter = tableName => builder.Append($"UPDATE {this.OrmProvider.GetTableName(tableName)}");
                        if (this.ShardingTables != null && this.ShardingTables.Count > 0)
                        {
                            var tableNames = this.ShardingTables[0].TableNames;
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
        var builder = new StringBuilder($"SELECT a.name FROM sys.objects a,sys.schemas b WHERE a.schema_id=b.schema_id AND A.type='U' AND ");
        var schemaBuilders = new Dictionary<string, StringBuilder>();
        foreach (var tableSegment in this.ShardingTables)
        {
            if (tableSegment.ShardingType > ShardingTableType.MultiTable)
            {
                var tableSchema = tableSegment.TableSchema ?? this.DefaultTableSchema;
                if (!schemaBuilders.TryGetValue(tableSchema, out var tableBuilder))
                    schemaBuilders.Add(tableSchema, tableBuilder = new StringBuilder());

                if (tableBuilder.Length > 0) tableBuilder.Append(" OR ");
                tableBuilder.Append($"a.name LIKE '{tableSegment.Mapper.TableName}%'");
            }
        }
        if (schemaBuilders.Count > 1)
            builder.Append('(');
        int index = 0;
        foreach (var schemaBuilder in schemaBuilders)
        {
            if (index > 0) builder.Append(" OR ");
            builder.Append($"b.name='{schemaBuilder.Key}' AND ({schemaBuilder.Value.ToString()})");
            index++;
        }
        if (schemaBuilders.Count > 1)
            builder.Append(')');
        return builder.ToString();
    }
    public override void SetFrom(Expression fieldsAssignment)
    {
        this.deferredSegments.Add(new CommandSegment
        {
            Type = "SetFrom",
            Value = fieldsAssignment
        });
    }
    public override void SetFrom(Expression fieldSelector, Expression valueSelector)
    {
        this.deferredSegments.Add(new CommandSegment
        {
            Type = "SetFromField",
            Value = (fieldSelector, valueSelector)
        });
    }
    public override void Join(string joinType, Type entityType, Expression joinOn)
    {
        this.Tables[0].AliasName = "a";
        base.Join(joinType, entityType, joinOn);
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
