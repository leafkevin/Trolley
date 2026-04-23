using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Trolley.MySqlConnector;

public class MySqlDeleteVisitor : DeleteVisitor
{
    private MySqlProvider dialectProvider => this.OrmProvider as MySqlProvider;

    public MySqlDeleteVisitor(Type entityType, DbContext dbContext, char tableAsStart = 'a', ITheaCommand command = null)
        : base(entityType, dbContext, tableAsStart, command) { }

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
        lambdaExpr.Body.TryGetParameterNames(out var parameters);
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