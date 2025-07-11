using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Trolley.MySqlConnector;

public class MySqlDeleteVisitor : DeleteVisitor
{
    private MySqlProvider dialectProvider => this.OrmProvider as MySqlProvider;
    public string OutputSql { get; set; }

    public MySqlDeleteVisitor(DbContext dbContext, char tableAsStart = 'a')
        : base(dbContext, tableAsStart) { }

    public override string BuildCommand(ITheaCommand command, out List<SqlFieldSegment> readerFields)
    {
        string sql = null;
        this.DbParameters ??= command.Parameters;
        foreach (var deferredSegment in this.deferredSegments)
        {
            switch (deferredSegment.Type)
            {
                case "Where":
                    this.VisitWhere(deferredSegment.Value as Expression);
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

        if (this.IsWhereKeys)
        {
            var entityType = this.Tables[0].EntityType;
            var whereKeys = this.deferredSegments[0].Value;
            Type whereObjType = null;
            var isBulk = whereKeys is IEnumerable && whereKeys is not string && whereKeys is not IDictionary<string, object>;
            IEnumerable entities = null;
            if (isBulk)
            {
                entities = whereKeys as IEnumerable;
                foreach (var entity in entities)
                {
                    whereObjType = entity.GetType();
                    break;
                }
            }
            else whereObjType = whereKeys.GetType();
            (var isMultiKeys, var origName, var headSqlSetter, var whereSqlSetter) = RepositoryHelper.BuildDeleteCommandInitializer(this.DbContext, entityType, whereObjType, this.IsMultiple, isBulk);

            int index = 0;
            var builder = new StringBuilder();
            var whereSqlBuilder = new StringBuilder();
            Action sqlExecuter = null;
            if (isBulk)
            {
                var typedWhereSqlSetter = whereSqlSetter as Action<IDataParameterCollection, StringBuilder, DbContext, object, string>;
                Func<int, string> suffixGetter = index => this.IsMultiple ? $"_m{this.CommandIndex}{index}" : $"{index}";
                Action<object, int> loopExecute = (entity, index) => typedWhereSqlSetter.Invoke(command.Parameters, whereSqlBuilder, this.DbContext, entity, suffixGetter.Invoke(index));
                if (isMultiKeys && !string.IsNullOrEmpty(this.OutputSql))
                {
                    loopExecute = (entity, index) =>
                    {
                        typedWhereSqlSetter.Invoke(command.Parameters, whereSqlBuilder, this.DbContext, entity, suffixGetter.Invoke(index));
                        whereSqlBuilder.Append(this.OutputSql);
                    };
                }
                sqlExecuter = () =>
                {
                    var jointMark = isMultiKeys ? " OR " : ",";
                    foreach (var entity in entities)
                    {
                        if (index > 0) whereSqlBuilder.Append(jointMark);
                        loopExecute.Invoke(entity, index);
                        index++;
                    }
                    if (!isMultiKeys)
                    {
                        whereSqlBuilder.Append(')');
                        if (!string.IsNullOrEmpty(this.OutputSql))
                            whereSqlBuilder.Append(this.OutputSql);
                    }
                };
            }
            else
            {
                if (this.IsMultiple)
                {
                    var typedWhereSqlSetter = whereSqlSetter as Action<IDataParameterCollection, StringBuilder, DbContext, object, string>;
                    if (!string.IsNullOrEmpty(this.OutputSql))
                    {
                        sqlExecuter = () =>
                        {
                            typedWhereSqlSetter.Invoke(command.Parameters, whereSqlBuilder, this.DbContext, whereKeys, $"_m{this.CommandIndex}");
                            whereSqlBuilder.Append(this.OutputSql);
                        };
                    }
                    else sqlExecuter = () => typedWhereSqlSetter.Invoke(command.Parameters, whereSqlBuilder, this.DbContext, whereKeys, $"_m{this.CommandIndex}");
                }
                else
                {
                    var typedWhereSqlSetter = whereSqlSetter as Action<IDataParameterCollection, StringBuilder, DbContext, object>;
                    if (!string.IsNullOrEmpty(this.OutputSql))
                    {
                        sqlExecuter = () =>
                        {
                            typedWhereSqlSetter.Invoke(command.Parameters, whereSqlBuilder, this.DbContext, whereKeys);
                            whereSqlBuilder.Append(this.OutputSql);
                        };
                    }
                    else sqlExecuter = () => typedWhereSqlSetter.Invoke(command.Parameters, whereSqlBuilder, this.DbContext, whereKeys);
                }
            }
            if (!string.IsNullOrEmpty(this.Tables[0].TableSchema))
                headSqlSetter = (builder, tableName) => headSqlSetter.Invoke(builder, this.Tables[0].TableSchema + "." + tableName);
            if (this.ShardingTables != null && this.ShardingTables.Count > 0)
            {
                var tableNames = this.ShardingTables[0].TableNames;
                sqlExecuter.Invoke();
                for (int i = 0; i < tableNames.Count; i++)
                {
                    if (i > 0) builder.Append(';');
                    headSqlSetter.Invoke(builder, tableNames[i]);
                    builder.Append(whereSqlBuilder);
                }
            }
            else
            {
                sqlExecuter.Invoke();
                headSqlSetter.Invoke(builder, this.Tables[0].Body ?? origName);
                builder.Append(whereSqlBuilder);
            }
            sql = builder.ToString();
            builder.Clear();
            whereSqlBuilder.Clear();
        }
        else
        {
            var builder = new StringBuilder();
            if (this.ShardingTables != null && this.ShardingTables.Count > 0)
            {
                var tableSegment = this.ShardingTables[0];
                var tableNames = tableSegment.TableNames;
                for (int i = 0; i < tableNames.Count; i++)
                {
                    if (i > 0) builder.Append(';');
                    builder.Append("DELETE FROM ");
                    builder.Append(this.OrmProvider.GetTableName(tableNames[i]));
                    builder.Append(" WHERE ");
                    builder.Append(this.WhereSql);
                    if (!string.IsNullOrEmpty(this.OutputSql))
                        builder.Append(this.OutputSql);
                }
            }
            else
            {
                var tableName = this.Tables[0].Body ?? this.Tables[0].Mapper.TableName;
                builder.Append($"DELETE FROM {this.OrmProvider.GetTableName(tableName)} WHERE {this.WhereSql}");
                if (!string.IsNullOrEmpty(this.OutputSql))
                    builder.Append(this.OutputSql);
            }
            sql = builder.ToString();
        }
        return sql;
    }
    public override void UseTableSchema(bool isIncludeMany, string tableSchema)
    {
        var defaultSchemaName = this.dialectProvider.GetDefaultSchemaName(this.DbContext);
        if (tableSchema == defaultSchemaName) return;

        var tableSegment = isIncludeMany ? this.IncludeTables.Last() : this.Tables.Last();
        tableSegment.TableSchema = tableSchema;
    }
    public override string BuildTableShardingsSql()
    {
        var builder = new StringBuilder($"SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE' AND ");
        var schemaBuilders = new Dictionary<string, StringBuilder>();
        var defaultSchemaName = this.dialectProvider.GetDefaultSchemaName(this.DbContext);
        foreach (var tableSegment in this.ShardingTables)
        {
            if (tableSegment.ShardingType > ShardingTableType.MultiTable)
            {
                var tableSchema = tableSegment.TableSchema ?? defaultSchemaName;
                if (!schemaBuilders.TryGetValue(tableSchema, out var tableBuilder))
                    schemaBuilders.Add(tableSchema, tableBuilder = new StringBuilder());

                if (tableBuilder.Length > 0) tableBuilder.Append(" OR ");
                tableBuilder.Append($"TABLE_NAME LIKE '{tableSegment.Mapper.TableName}%'");
            }
        }
        if (schemaBuilders.Count > 1)
            builder.Append('(');
        int index = 0;
        foreach (var schemaBuilder in schemaBuilders)
        {
            if (index > 0) builder.Append(" OR ");
            builder.Append($"TABLE_SCHEMA='{schemaBuilder.Key}' AND ({schemaBuilder.Value.ToString()})");
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
    public virtual void Returning(Expression fieldsSelector)
    {
        this.deferredSegments.Add(new CommandSegment
        {
            Type = "OutputExpression",
            Value = fieldsSelector
        });
    }
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
    public virtual void InitTableAlias(LambdaExpression lambdaExpr)
    {
        this.TableAliases.Clear();
        lambdaExpr.Body.GetParameterNames(out var parameters);
        if (parameters == null || parameters.Count == 0)
            return;
        int index = 0;
        foreach (var parameterExpr in lambdaExpr.Parameters)
        {
            if (!parameters.Contains(parameterExpr.Name))
            {
                index++;
                continue;
            }
            if (this.TableAliases.ContainsKey(parameterExpr.Name))
                continue;
            this.TableAliases.Add(parameterExpr.Name, this.Tables[index]);
            index++;
        }
    }
}