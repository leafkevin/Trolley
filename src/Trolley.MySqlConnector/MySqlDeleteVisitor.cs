using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Trolley.MySqlConnector;

public class MySqlDeleteVisitor : DeleteVisitor
{
    public MySqlDeleteVisitor(Type entityType, DbContext dbContext, char tableAsStart = 'a', ITheaCommand command = null)
        : base(entityType, dbContext, tableAsStart, command) { }

    public override void UseTableSchema(bool isIncludeMany, string tableSchema)
    {
        if (tableSchema == this.DbContext.DefaultTableSchema) return;
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
                this.ReaderFields.Add(new ReaderField
                {
                    FieldType = ReaderFieldType.Field,
                    TargetMember = memberMapper.Member,
                    ReaderType = memberMapper.MemberType,
                    MappedTargetType = memberMapper.MappedTargetType,
                    TypeHandler = memberMapper.TypeHandler,
                    MemberName = memberMapper.MemberName,
                    Value = memberMapper.FieldName
                });
            }
        }
        else
        {
            this.ReaderFields.Add(new ReaderField
            {
                FieldType = ReaderFieldType.RawSql,
                Value = fieldNames
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
                    var sqlSegment = this.Visit(new SqlSegment { Expression = memberExpr });
                    var fieldName = this.WrapSql(sqlSegment);
                    builder.Append(fieldName);
                    var isNeedAlias = false;
                    if (sqlSegment.MemberName != memberExpr.Member.Name)
                    {
                        isNeedAlias = true;
                        builder.Append($" AS {this.OrmProvider.GetFieldName(memberExpr.Member.Name)}");
                    }
                    this.ReaderFields.Add(new ReaderField
                    {
                        FieldType = ReaderFieldType.Field,
                        TargetMember = memberExpr.Member,
                        ReaderType = memberExpr.Type,
                        MappedTargetType = sqlSegment.MappedTargetType,
                        TypeHandler = sqlSegment.TypeHandler,
                        MemberName = memberExpr.Member.Name,
                        Value = fieldName,
                        IsNeedAlias = isNeedAlias
                    });
                }
                break;
            case ExpressionType.New:
                var newExpr = fieldsSelector.Body as NewExpression;
                for (int i = 0; i < newExpr.Arguments.Count; i++)
                {
                    var memberInfo = newExpr.Members[i];
                    var sqlSegment = this.Visit(new SqlSegment { Expression = newExpr.Arguments[i] });
                    var fieldName = this.WrapSql(sqlSegment);
                    if (i > 0) builder.Append(',');
                    builder.Append(fieldName);

                    var isNeedAlias = false;
                    if (sqlSegment.SqlType > SqlType.OnlyField || sqlSegment.IsVariable || sqlSegment.MemberName != memberInfo.Name)
                    {
                        isNeedAlias = true;
                        builder.Append($" AS {this.OrmProvider.GetFieldName(memberInfo.Name)}");
                    }
                    this.ReaderFields.Add(new ReaderField
                    {
                        FieldType = ReaderFieldType.Field,
                        TargetMember = memberInfo,
                        ReaderType = memberInfo.GetMemberType(),
                        MappedTargetType = sqlSegment.MappedTargetType,
                        TypeHandler = sqlSegment.TypeHandler,
                        MemberName = memberInfo.Name,
                        Value = fieldName,
                        IsNeedAlias = isNeedAlias
                    });
                }
                break;
            case ExpressionType.MemberInit:
                var memberInitExpr = fieldsSelector.Body as MemberInitExpression;
                for (int i = 0; i < memberInitExpr.Bindings.Count; i++)
                {
                    if (memberInitExpr.Bindings[i].BindingType != MemberBindingType.Assignment)
                        throw new NotSupportedException("暂时不支持除MemberBindingType.Assignment类型外的成员绑定表达式");

                    var memberAssignment = memberInitExpr.Bindings[i] as MemberAssignment;
                    var sqlSegment = this.Visit(new SqlSegment { Expression = memberAssignment.Expression });
                    var fieldName = this.WrapSql(sqlSegment);
                    if (i > 0) builder.Append(',');
                    builder.Append(fieldName);

                    var isNeedAlias = false;
                    if (sqlSegment.SqlType > SqlType.OnlyField || sqlSegment.IsVariable || sqlSegment.MemberName != memberAssignment.Member.Name)
                    {
                        isNeedAlias = true;
                        builder.Append($" AS {this.OrmProvider.GetFieldName(memberAssignment.Member.Name)}");
                    }
                    this.ReaderFields.Add(new ReaderField
                    {
                        FieldType = ReaderFieldType.Field,
                        TargetMember = memberAssignment.Member,
                        ReaderType = memberAssignment.Member.GetMemberType(),
                        MappedTargetType = sqlSegment.MappedTargetType,
                        TypeHandler = sqlSegment.TypeHandler,
                        MemberName = memberAssignment.Member.Name,
                        Value = fieldName,
                        IsNeedAlias = isNeedAlias
                    });
                }
                break;
            case ExpressionType.Parameter:
                foreach (var memberMapper in entityMapper.MemberMaps)
                {
                    if (memberMapper.IsIgnore || memberMapper.IsNavigation)
                        continue;

                    this.ReaderFields.Add(new ReaderField
                    {
                        FieldType = ReaderFieldType.Field,
                        TargetMember = memberMapper.Member,
                        ReaderType = memberMapper.MemberType,
                        MappedTargetType = memberMapper.MappedTargetType,
                        TypeHandler = memberMapper.TypeHandler,
                        MemberName = memberMapper.Member.Name,
                        Value = memberMapper.FieldName
                    });
                }
                builder.Append('*');
                break;
            default:
                {
                    var sqlSegment = this.Visit(new SqlSegment { Expression = fieldsSelector });
                    for (int i = 0; i < this.ReaderFields.Count; i++)
                    {
                        var readerField = this.ReaderFields[i];
                        if (i > 0) builder.Append(',');
                        builder.Append(readerField.Value);
                    }
                    this.ReaderFields.Add(new ReaderField
                    {
                        FieldType = ReaderFieldType.Field,
                        ReaderType = fieldsSelector.Type,
                        Value = sqlSegment.Value
                    });
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