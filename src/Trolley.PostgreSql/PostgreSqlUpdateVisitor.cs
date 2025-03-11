using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Trolley.PostgreSql;

public class PostgreSqlUpdateVisitor : UpdateVisitor, IUpdateVisitor
{
    public string OutputSql { get; set; }

    public PostgreSqlUpdateVisitor(DbContext dbContext, char tableAsStart = 'a')
        : base(dbContext, tableAsStart) { }
    public override string BuildCommand(DbContext dbContext, ITheaCommand command, out List<SqlFieldSegment> readerFields)
    {
        string sql = null;
        readerFields = null;
        var builder = new StringBuilder();
        switch (this.ActionMode)
        {
            case ActionMode.Bulk:
                {
                    //此SQL只能用在多命令查询时和返回ToSql两个场景
                    (var updateObjs, var bulkCount, var tableName, var fixedParameterSetter, var firstSqlSetter, var sqlSetter, readerFields) = this.BuildWithBulk(command);
                    Func<int, string> suffixGetter = index => this.IsMultiple ? $"_m{this.CommandIndex}{index}" : $"{index}";
                    Action<object, int> sqlExecute = null;
                    if (this.ShardingTables != null && this.ShardingTables.Count > 0)
                    {
                        sqlExecute = (updateObj, index) =>
                        {
                            if (index > 0) builder.Append(';');
                            var tableNames = this.ShardingTables[0].TableNames;
                            firstSqlSetter.Invoke(command.Parameters, builder, this.DbContext, tableNames[0], updateObj, suffixGetter.Invoke(index));

                            for (int i = 1; i < tableNames.Count; i++)
                            {
                                builder.Append(';');
                                sqlSetter.Invoke(builder, this.DbContext, tableNames[i], updateObj, suffixGetter.Invoke(index));
                            }
                        };
                    }
                    else
                    {
                        sqlExecute = (updateObj, index) =>
                        {
                            if (index > 0) builder.Append(';');
                            firstSqlSetter.Invoke(command.Parameters, builder, this.DbContext, tableName, updateObj, suffixGetter.Invoke(index));
                        };
                    }

                    int index = 0;
                    fixedParameterSetter?.Invoke(command.Parameters);
                    foreach (var updateObj in updateObjs)
                    {
                        sqlExecute.Invoke(updateObj, index);
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
                            case "OutputFields":
                                this.VisitOutputFields(deferredSegment.Value as string);
                                break;
                            case "OutputExpression":
                                this.VisitOutputExpression(deferredSegment.Value as LambdaExpression);
                                break;
                        }
                    }
                    readerFields = this.ReaderFields;
                    var aliasName = this.Tables[0].AliasName;
                    if (this.IsNeedTableAlias)
                        builder.Append($"{aliasName} ");

                    int index = 0;
                    builder.Append("SET ");
                    if (this.UpdateFields.Count > 0)
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
                        builder.Append($" WHERE {whereSql}");
                    if (!string.IsNullOrEmpty(this.OutputSql))
                        builder.Append(this.OutputSql);
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
                        var tableSchema = this.Tables[0].TableSchema;
                        if (!string.IsNullOrEmpty(tableSchema))
                            headSqlSetter = tableName => builder.Append($"UPDATE {this.OrmProvider.GetTableName(tableSchema + "." + tableName)} ");
                        else headSqlSetter = tableName => builder.Append($"UPDATE {this.OrmProvider.GetTableName(tableName)} ");
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
        return sql;
    }
    public override (IEnumerable, int, string, Action<IDataParameterCollection>, Action<IDataParameterCollection, StringBuilder, DbContext, string, object, string>,
        Action<StringBuilder, DbContext, string, object, string>, List<SqlFieldSegment>) BuildWithBulk(ITheaCommand command)
    {
        Type updateObjType = null;
        (var updateObjs, var bulkCount) = ((IEnumerable, int))this.deferredSegments[0].Value;
        foreach (var updateObj in updateObjs)
        {
            updateObjType = updateObj.GetType();
            break;
        }
        var builder = new StringBuilder();
        List<IDbDataParameter> fixedDbParameters = null;
        string fixedSql = null;
        int index = 0;
        if (this.deferredSegments.Count > 1)
        {
            this.DbParameters = new TheaDbParameterCollection();
            //先解析其他sql，生成固定sql
            this.UpdateFields = new();
            for (int i = 1; i < this.deferredSegments.Count; i++)
            {
                var deferredSegment = this.deferredSegments[i];
                switch (deferredSegment.Type)
                {
                    case "Set":
                        this.VisitSet(deferredSegment.Value as Expression);
                        break;
                    case "SetField":
                        this.VisitSetField(deferredSegment.Value);
                        break;
                    case "SetWith":
                        this.VisitSetWith(deferredSegment.Value);
                        break;
                    case "OutputFields":
                        this.VisitOutputFields(deferredSegment.Value as string);
                        break;
                    case "OutputExpression":
                        this.VisitOutputExpression(deferredSegment.Value as LambdaExpression);
                        break;
                    default: throw new NotSupportedException("SetBulk操作后，只支持Set/IgnoreFields/OnlyFields/Returning操作");
                }
            }
            if (this.UpdateFields.Count > 0)
            {
                foreach (var setField in this.UpdateFields)
                {
                    if (index > 0) builder.Append(',');
                    builder.Append(setField);
                    index++;
                }
                builder.Append(',');
                fixedSql = builder.ToString();
            }
            if (this.DbParameters.Count > 0)
                fixedDbParameters = this.DbParameters.Cast<IDbDataParameter>().ToList();
            this.DbParameters = command.Parameters;
            this.UpdateFields.Clear();
            builder.Clear();
        }
        //多命令查询时，第二次以后，DbParameters有值，不能再赋值
        else this.DbParameters ??= command.Parameters;

        builder.Append("UPDATE ");
        var tableSegment = this.Tables[0];
        if (!string.IsNullOrEmpty(tableSegment.TableSchema))
            builder.Append($" {this.OrmProvider.GetTableName(tableSegment.TableSchema)}.");
        var headSql = builder.ToString();

        var entityType = tableSegment.EntityType;
        (var bulkSqlSetter, var shardingSqlSetter) = RepositoryHelper.BuildUpdateBulkSetWithSqlParametersPart(this.DbContext, entityType, updateObjType, this.IsMultiple, false, this.OnlyFieldNames, this.IgnoreFieldNames);
        //处理有tableSchema的场景
        Action<IDataParameterCollection> fixedParametersSetter = null;
        if (fixedDbParameters != null)
            fixedParametersSetter = dbParameters => fixedDbParameters.ForEach(f => dbParameters.Add(f));
        Action<IDataParameterCollection, StringBuilder, DbContext, string, object, string> firstSqlSetter = null;
        Action<StringBuilder, DbContext, string, object, string> sqlSetter = null;
        if (!string.IsNullOrEmpty(this.OutputSql))
        {
            firstSqlSetter = (dbParameters, builder, dbContext, tableName, updateObj, suffix) =>
            {
                builder.Append($"{headSql}{this.OrmProvider.GetTableName(tableName)} SET {fixedSql}");
                bulkSqlSetter.Invoke(dbParameters, builder, dbContext, updateObj, suffix);
                builder.Append(this.OutputSql);
            };
            sqlSetter = (builder, dbContext, tableName, updateObj, suffix) =>
            {
                builder.Append($"{headSql}{this.OrmProvider.GetTableName(tableName)} SET {fixedSql}");
                shardingSqlSetter.Invoke(builder, dbContext, updateObj, suffix);
                builder.Append(this.OutputSql);
            };
        }
        else
        {
            firstSqlSetter = (dbParameters, builder, dbContext, tableName, updateObj, suffix) =>
            {
                builder.Append($"{headSql}{this.OrmProvider.GetTableName(tableName)} SET {fixedSql}");
                bulkSqlSetter.Invoke(dbParameters, builder, dbContext, updateObj, suffix);
            };
            sqlSetter = (builder, dbContext, tableName, updateObj, suffix) =>
            {
                builder.Append($"{headSql}{this.OrmProvider.GetTableName(tableName)} SET {fixedSql}");
                shardingSqlSetter.Invoke(builder, dbContext, updateObj, suffix);
            };
        }
        var tableName = tableSegment.Mapper.TableName;
        return (updateObjs, bulkCount, tableName, fixedParametersSetter, firstSqlSetter, sqlSetter, this.ReaderFields);
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
                    schemaBuilders.Add(tableSchema, tableBuilder = new StringBuilder());

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
    public void Returning(string fieldNames)
    {
        this.deferredSegments.Add(new CommandSegment
        {
            Type = "OutputFields",
            Value = fieldNames
        });
    }
    public void Returning(Expression fieldsSelector)
    {
        this.deferredSegments.Add(new CommandSegment
        {
            Type = "OutputExpression",
            Value = fieldsSelector
        });
    }
    public void WithBulkCopy(IEnumerable updateObjs)
    {
        this.ActionMode = ActionMode.BulkCopy;
        this.deferredSegments.Add(new CommandSegment
        {
            Type = "WithBulkCopy",
            Value = updateObjs
        });
    }
    public IEnumerable BuildWithBulkCopy() => (IEnumerable)this.deferredSegments[0].Value;
    public void VisitOutputFields(string fieldNames)
    {
        this.ReaderFields = new();
        this.OutputSql = $" RETURNING {fieldNames}";
        var entityType = this.Tables[0].EntityType;
        if (fieldNames == "*")
        {
            var entityMapper = this.Tables[0].Mapper;
            foreach (var memberMapper in entityMapper.MemberMaps)
            {
                if (memberMapper.IsIgnore || memberMapper.IsNavigation)
                    continue;
                this.ReaderFields.Add(new SqlFieldSegment
                {
                    FieldType = SqlFieldType.Field,
                    FromMember = memberMapper.Member,
                    TargetMember = memberMapper.Member,
                    SegmentType = memberMapper.MemberType,
                    NativeDbType = memberMapper.NativeDbType,
                    TypeHandler = memberMapper.TypeHandler,
                    Body = memberMapper.FieldName
                });
            }
        }
        else
        {
            this.ReaderFields.Add(new SqlFieldSegment
            {
                FieldType = SqlFieldType.RawSql,
                Body = fieldNames
            });
        }
    }
    public void VisitOutputExpression(LambdaExpression fieldsSelector)
    {
        this.ReaderFields = new();
        var entityMapper = this.Tables[0].Mapper;
        var builder = new StringBuilder(" RETURNING ");
        this.InitTableAlias(fieldsSelector);
        switch (fieldsSelector.Body.NodeType)
        {
            case ExpressionType.MemberAccess:
                {
                    var memberExpr = fieldsSelector.Body as MemberExpression;
                    var sqlSegment = this.VisitAndDeferred(new SqlFieldSegment { Expression = memberExpr });
                    this.GetQuotedValue(sqlSegment, true);
                    sqlSegment.TargetMember = memberExpr.Member;
                    sqlSegment.SegmentType = memberExpr.Type;
                    builder.Append(sqlSegment.Body);
                    if (sqlSegment.IsNeedAlias || sqlSegment.IsConstant || sqlSegment.IsVariable || sqlSegment.HasParameter || sqlSegment.IsExpression || sqlSegment.IsMethodCall
                        || sqlSegment.FromMember != null && sqlSegment.FromMember.Name != sqlSegment.TargetMember.Name)
                        builder.Append($" AS {this.OrmProvider.GetFieldName(memberExpr.Member.Name)}");
                    this.ReaderFields.Add(sqlSegment);
                }
                break;
            case ExpressionType.New:
                var newExpr = fieldsSelector.Body as NewExpression;
                for (int i = 0; i < newExpr.Arguments.Count; i++)
                {
                    var memberInfo = newExpr.Members[i];
                    var sqlSegment = this.VisitAndDeferred(new SqlFieldSegment { Expression = newExpr.Arguments[i] });
                    this.GetQuotedValue(sqlSegment, true);
                    sqlSegment.TargetMember = memberInfo;
                    sqlSegment.SegmentType = memberInfo.GetMemberType();
                    if (i > 0) builder.Append(',');
                    builder.Append(sqlSegment.Body);
                    if (sqlSegment.IsNeedAlias || sqlSegment.IsConstant || sqlSegment.IsVariable || sqlSegment.HasParameter || sqlSegment.IsExpression || sqlSegment.IsMethodCall)
                        builder.Append($" AS {this.OrmProvider.GetFieldName(memberInfo.Name)}");
                    this.ReaderFields.Add(sqlSegment);
                }
                break;
            case ExpressionType.MemberInit:
                var memberInitExpr = fieldsSelector.Body as MemberInitExpression;
                for (int i = 0; i < memberInitExpr.Bindings.Count; i++)
                {
                    if (memberInitExpr.Bindings[i].BindingType != MemberBindingType.Assignment)
                        throw new NotSupportedException("暂时不支持除MemberBindingType.Assignment类型外的成员绑定表达式");

                    var memberAssignment = memberInitExpr.Bindings[i] as MemberAssignment;
                    var sqlSegment = this.VisitAndDeferred(new SqlFieldSegment { Expression = memberAssignment.Expression });
                    this.GetQuotedValue(sqlSegment, true);
                    sqlSegment.TargetMember = memberAssignment.Member;
                    sqlSegment.SegmentType = memberAssignment.Member.GetMemberType();
                    if (i > 0) builder.Append(',');
                    builder.Append(sqlSegment.Body);
                    if (sqlSegment.IsNeedAlias || sqlSegment.IsConstant || sqlSegment.IsVariable || sqlSegment.HasParameter || sqlSegment.IsExpression || sqlSegment.IsMethodCall)
                        builder.Append($" AS {this.OrmProvider.GetFieldName(memberAssignment.Member.Name)}");
                    this.ReaderFields.Add(sqlSegment);
                }
                break;
        }
        this.OutputSql = builder.ToString();
        builder.Clear();
    }
}