using System;
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

    public MySqlDeleteVisitor(Type entityType, DbContext dbContext, char tableAsStart = 'a')
        : base(entityType, dbContext, tableAsStart) { }

    public override string BuildSql(ITheaCommand command, out List<SqlFieldSegment> readerFields)
    {
        readerFields = null;
        this.DbParameters = command.Parameters;

        var tableSegment = this.Tables[0];
        var entityType = tableSegment.EntityType;
        if (tableSegment.TableShardingInfo != null && !tableSegment.IsSharding)
            throw new NotSupportedException($"实体表{entityType.FullName}已设置分表，但未指定分表，请使用UseTable/UseTableBy/UseTableByRange方法手动指定分表，原始表：{tableSegment.Mapper.TableName}");

        if (this.HasWhere) this.WhereBuilder = new();
        Func<IDataParameterCollection, DbContext, object, string> whereSqlInitializer = null;
        foreach (var deferredSegment in this.deferredSegments)
        {
            switch (deferredSegment.Type)
            {
                case "AndBy":
                    whereSqlInitializer = RepositoryHelper.BuildWhereCommandInitializer(this.DbContext, entityType, deferredSegment.Value, 4, false, false, false);
                    this.VisitAndSql(whereSqlInitializer.Invoke(this.DbParameters, this.DbContext, deferredSegment.Value));
                    break;
                case "AndById":
                    whereSqlInitializer = RepositoryHelper.BuildWhereCommandInitializer(this.DbContext, entityType, deferredSegment.Value, 4, true, false, false);
                    this.VisitAndSql(whereSqlInitializer.Invoke(this.DbParameters, this.DbContext, deferredSegment.Value));
                    break;
                case "AndByIds":
                    whereSqlInitializer = RepositoryHelper.BuildWhereCommandInitializer(this.DbContext, entityType, deferredSegment.Value, 4, true, false, true);
                    this.VisitAndSql(whereSqlInitializer.Invoke(this.DbParameters, this.DbContext, deferredSegment.Value));
                    break;
                case "And":
                    this.VisitAnd(deferredSegment.Value as Expression);
                    break;
                case "OrBy":
                    whereSqlInitializer = RepositoryHelper.BuildWhereCommandInitializer(this.DbContext, entityType, deferredSegment.Value, 4, false, false, false);
                    this.VisitOrSql(whereSqlInitializer.Invoke(this.DbParameters, this.DbContext, deferredSegment.Value));
                    break;
                case "OrById":
                    whereSqlInitializer = RepositoryHelper.BuildWhereCommandInitializer(this.DbContext, entityType, deferredSegment.Value, 4, true, false, false);
                    this.VisitOrSql(whereSqlInitializer.Invoke(this.DbParameters, this.DbContext, deferredSegment.Value));
                    break;
                case "OrByIds":
                    whereSqlInitializer = RepositoryHelper.BuildWhereCommandInitializer(this.DbContext, entityType, deferredSegment.Value, 4, true, false, true);
                    this.VisitOrSql(whereSqlInitializer.Invoke(this.DbParameters, this.DbContext, deferredSegment.Value));
                    break;
                case "Or":
                    this.VisitOr(deferredSegment.Value as Expression);
                    break;
            }
        }
        var whereSql = this.WhereBuilder.ToString();
        this.WhereBuilder.Clear();

        var builder = this.WhereBuilder;
        builder.Append($"DELETE FROM {this.GetFormatTableName(tableSegment)}");
        if (this.HasWhere) builder.Append($" WHERE {whereSql}");
        if (!string.IsNullOrEmpty(this.OutputSql))
            builder.Append(this.OutputSql);

        var sql = builder.ToString();
        if (tableSegment.ShardingType > ShardingTableType.SingleTable)
        {
            builder.Clear();
            var tableNames = tableSegment.TableNames;
            for (int i = 0; i < tableNames.Count; i++)
            {
                if (i > 0) builder.Append(';');
                builder.Append(sql);
            }
            sql = builder.ToString();
        }
        builder.Clear();
        return sql;
    }
    public override void UseTableSchema(bool isIncludeMany, string tableSchema)
    {
        var defaultSchemaName = this.dialectProvider.GetDefaultSchemaName(this.DbContext);
        if (tableSchema == defaultSchemaName) return;
        var tableSegment = isIncludeMany ? this.IncludeTables.Last() : this.Tables.Last();
        tableSegment.TableSchema = tableSchema;
    }
    public void Returning(string fieldNames)
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
                    MappedTargetType = memberMapper.MappedTargetType,
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
    public virtual void Returning(LambdaExpression fieldsSelector)
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