using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Text;

namespace Trolley;

public class CreateVisitor : SqlVisitor, ICreateVisitor
{
    private string bulkHeadSql;
    private Action<IDbCommand, ISqlVisitor, StringBuilder, int, object> bulkCommandInitializer;

    protected List<InsertField> InsertFields { get; set; } = new();
    protected bool IsFrom { get; set; } = false;
    protected bool IsUseIgnore { get; set; }
    protected bool IsUseUpdate { get; set; }
    protected virtual bool IsBulk { get; set; } = false;

    public CreateVisitor(string dbKey, IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType, bool isParameterized = false, char tableAsStart = 'a', string parameterPrefix = "p", string multiParameterPrefix = "")
        : base(dbKey, ormProvider, mapProvider, isParameterized, tableAsStart, parameterPrefix, multiParameterPrefix)
    {
        this.Tables = new()
        {
            new TableSegment
            {
                EntityType = entityType,
                Mapper = this.MapProvider.GetEntityMap(entityType)
            }
        };
        this.TableAlias = new();
        this.DbParameters = new();
    }
    public virtual string BuildSql(out List<IDbDataParameter> dbParameters)
    {
        var tableName = this.OrmProvider.GetTableName(this.Tables[0].Mapper.TableName);
        var fieldsBuilder = new StringBuilder($"{this.BuildHeadSql()} {tableName} (");
        var valuesBuilder = new StringBuilder();
        string fromTables = null;
        if (this.IsFrom) valuesBuilder.Append(" SELECT ");
        else valuesBuilder.Append(" VALUES(");
        for (int i = 0; i < this.InsertFields.Count; i++)
        {
            var insertField = this.InsertFields[i];
            if (i > 0)
            {
                fieldsBuilder.Append(',');
                valuesBuilder.Append(',');
            }
            fieldsBuilder.Append(insertField.Fields);
            valuesBuilder.Append(insertField.Values);
            if (insertField.Type == InsertFieldType.FromTables)
                fromTables = insertField.FromTables;
        }
        fieldsBuilder.Append(')');
        if (this.IsFrom)
        {
            valuesBuilder.Append($" FROM {fromTables}");
            if (!string.IsNullOrEmpty(this.WhereSql))
                valuesBuilder.Append(" WHERE " + this.WhereSql);
        }
        else valuesBuilder.Append(')');

        var tailSql = this.BuildTailSql();
        if (!string.IsNullOrEmpty(tailSql))
            valuesBuilder.Append(tailSql);
        fieldsBuilder.Append(valuesBuilder);
        dbParameters = this.DbParameters;
        return fieldsBuilder.ToString();
    }
    public virtual string BuildHeadSql() => $"INSERT INTO";
    public virtual string BuildTailSql() => string.Empty;
    public virtual ICreateVisitor UseIgnore()
    {
        this.IsUseIgnore = true;
        return this;
    }
    public virtual ICreateVisitor UseUpdate()
    {
        this.IsUseUpdate = true;
        return this;
    }
    public virtual ICreateVisitor WithBy(object insertObj)
    {
        var entityType = this.Tables[0].EntityType;
        var commandInitializer = RepositoryHelper.BuildCreateWithBiesCommandInitializer(this, entityType, insertObj);
        var fieldsBuilder = new StringBuilder();
        var valuesBuilder = new StringBuilder();
        commandInitializer.Invoke(this, this.DbParameters, insertObj, fieldsBuilder, valuesBuilder);
        this.InsertFields.Add(new InsertField
        {
            Type = InsertFieldType.Fields,
            Fields = fieldsBuilder.ToString(),
            Values = valuesBuilder.ToString()
        });
        return this;
    }
    public virtual ICreateVisitor WithBy(Expression fieldSelector, object fieldValue)
    {
        var lambdaExpr = fieldSelector as LambdaExpression;
        var memberExpr = lambdaExpr.Body as MemberExpression;
        var entityMapper = this.Tables[0].Mapper;
        var memberMapper = entityMapper.GetMemberMap(memberExpr.Member.Name);
        var parameterName = this.OrmProvider.ParameterPrefix + memberMapper.MemberName;
        this.DbParameters.Add(this.OrmProvider.CreateParameter(memberMapper, parameterName, fieldValue));
        this.InsertFields.Add(new InsertField
        {
            Type = InsertFieldType.Fields,
            Fields = this.OrmProvider.GetFieldName(memberMapper.FieldName),
            Values = parameterName
        });
        return this;
    }
    public virtual ICreateVisitor WithBulkFirst(object insertObjs)
    {
        var entityTppe = this.Tables[0].EntityType;
        this.bulkCommandInitializer = RepositoryHelper.BuildCreateWithBulkCommandInitializer(this, entityTppe, insertObjs, out this.bulkHeadSql);
        this.IsBulk = true;
        return this;
    }
    public virtual ICreateVisitor WithBulk(IDbCommand command, StringBuilder builder, int index, object insertObj)
    {
        if (index == 0)
        {
            var tableName = this.OrmProvider.GetTableName(this.Tables[0].Mapper.TableName);
            builder.Append($"{this.BuildHeadSql()} {tableName} ({this.bulkHeadSql}) VALUES ");
        }
        else builder.Append(',');
        builder.Append('(');
        this.bulkCommandInitializer.Invoke(command, this, builder, index, insertObj);
        builder.Append(')');
        var tailSql = this.BuildTailSql();
        if (!string.IsNullOrEmpty(tailSql))
            builder.Append(tailSql);
        return this;
    }
    public virtual ICreateVisitor From(Expression fieldSelector)
    {
        if (this.IsFrom) throw new NotSupportedException("INSERT INTO数据，只允许有一次From操作");
        this.IsNeedAlias = true;
        var lambdaExpr = fieldSelector as LambdaExpression;
        for (int i = 0; i < lambdaExpr.Parameters.Count; i++)
        {
            var parameterExpr = lambdaExpr.Parameters[i];
            var tableSegment = new TableSegment
            {
                EntityType = parameterExpr.Type,
                Mapper = this.MapProvider.GetEntityMap(parameterExpr.Type),
                AliasName = $"{(char)(this.TableAsStart + i)}"
            };
            this.Tables.Add(tableSegment);
        }
        this.InitTableAlias(lambdaExpr);
        var sqlSegment = lambdaExpr.Body.NodeType switch
        {
            ExpressionType.New => this.VisitNew(new SqlSegment { Expression = lambdaExpr.Body }),
            ExpressionType.MemberInit => this.VisitMemberInit(new SqlSegment { Expression = lambdaExpr.Body }),
            _ => throw new NotImplementedException("不支持的表达式，只支持New或MemberInit表达式，如: new { a.Id, b.Name + &quot;xxx&quot; } 或是new User { Id = a.Id, Name = b.Name + &quot;xxx&quot; }")
        };
        var insertFields = (InsertField)sqlSegment.Value;
        var fromTablesBuilder = new StringBuilder();
        for (var i = 1; i < this.Tables.Count; i++)
        {
            var tableSegment = this.Tables[i];
            var tableName = tableSegment.Body;
            if (string.IsNullOrEmpty(tableName))
            {
                tableSegment.Mapper ??= this.MapProvider.GetEntityMap(tableSegment.EntityType);
                tableName = this.OrmProvider.GetTableName(tableSegment.Mapper.TableName);
            }
            if (i > 1) fromTablesBuilder.Append(',');
            fromTablesBuilder.Append(tableName + " " + tableSegment.AliasName);
        }
        insertFields.FromTables = fromTablesBuilder.ToString();
        this.InsertFields.Add(insertFields);
        this.IsFrom = true;
        return this;
    }
    //public virtual ICreateVisitor WhereWith(object whereObj, bool isOnlyKeys = false)
    //{
    //    var entityMapper = this.Tables[0].Mapper;
    //    var whereInitializer = RepositoryHelper.BuildUpdateWhereWithParameters(this, entityMapper.EntityType, whereObj, isOnlyKeys);
    //    whereInitializer.Invoke(this, this.IsUseUpdate, this.DbParameters, whereObj);
    //    return this;
    //}
    public virtual ICreateVisitor Where(Expression whereExpr)
    {
        this.IsWhere = true;
        var lambdaExpr = whereExpr as LambdaExpression;
        this.InitTableAlias(lambdaExpr);
        this.LastWhereNodeType = OperationType.None;
        this.WhereSql = this.VisitConditionExpr(lambdaExpr.Body);
        this.IsWhere = false;
        return this;
    }
    public virtual ICreateVisitor And(Expression whereExpr)
    {
        this.IsWhere = true;
        var lambdaExpr = whereExpr as LambdaExpression;
        if (this.LastWhereNodeType == OperationType.Or)
        {
            this.WhereSql = $"({this.WhereSql})";
            this.LastWhereNodeType = OperationType.And;
        }
        var conditionSql = this.VisitConditionExpr(lambdaExpr.Body);
        if (this.LastWhereNodeType == OperationType.Or)
        {
            conditionSql = $"({conditionSql})";
            this.LastWhereNodeType = OperationType.And;
        }
        if (!string.IsNullOrEmpty(this.WhereSql))
            this.WhereSql += " AND " + conditionSql;
        else this.WhereSql = conditionSql;
        this.IsWhere = false;
        return this;
    }
    public override SqlSegment VisitNew(SqlSegment sqlSegment)
    {
        var newExpr = sqlSegment.Expression as NewExpression;
        if (newExpr.Type.Name.StartsWith("<>"))
        {
            var fieldsBuilder = new StringBuilder();
            var valuesBuilder = new StringBuilder();
            var entityMapper = this.Tables[0].Mapper;
            for (int i = 0; i < newExpr.Arguments.Count; i++)
            {
                var memberInfo = newExpr.Members[i];
                if (!entityMapper.TryGetMemberMap(memberInfo.Name, out var memberMapper))
                    continue;

                this.AddMemberElement(i, new SqlSegment { Expression = newExpr.Arguments[i] }, memberMapper, fieldsBuilder, valuesBuilder);
            }
            return sqlSegment.ChangeValue(new InsertField
            {
                Type = InsertFieldType.FromTables,
                Fields = fieldsBuilder.ToString(),
                Values = valuesBuilder.ToString()
            });
        }
        return this.Evaluate(sqlSegment);
    }
    public override SqlSegment VisitMemberInit(SqlSegment sqlSegment)
    {
        var memberInitExpr = sqlSegment.Expression as MemberInitExpression;
        var fieldsBuilder = new StringBuilder();
        var valuesBuilder = new StringBuilder();
        var entityMapper = this.Tables[0].Mapper;
        for (int i = 0; i < memberInitExpr.Bindings.Count; i++)
        {
            if (memberInitExpr.Bindings[i].BindingType != MemberBindingType.Assignment)
                throw new NotImplementedException($"不支持除MemberBindingType.Assignment类型外的成员绑定表达式, {memberInitExpr.Bindings[i]}");
            var memberAssignment = memberInitExpr.Bindings[i] as MemberAssignment;
            if (!entityMapper.TryGetMemberMap(memberAssignment.Member.Name, out var memberMapper))
                continue;
            this.AddMemberElement(i, new SqlSegment { Expression = memberAssignment.Expression }, memberMapper, fieldsBuilder, valuesBuilder);
        }
        return sqlSegment.ChangeValue(new InsertField
        {
            Type = InsertFieldType.FromTables,
            Fields = fieldsBuilder.ToString(),
            Values = valuesBuilder.ToString()
        });
    }
    private void InitTableAlias(LambdaExpression lambdaExpr)
    {
        this.TableAlias.Clear();
        lambdaExpr.Body.GetParameters(out var parameters);
        if (parameters == null || parameters.Count == 0)
            return;
        for (int i = 0; i < lambdaExpr.Parameters.Count; i++)
        {
            var parameterExpr = lambdaExpr.Parameters[i];
            this.TableAlias.Add(parameterExpr.Name, this.Tables[i + 1]);
        }
    }
    private void AddMemberElement(int index, SqlSegment sqlSegment, MemberMap memberMapper, StringBuilder fieldsBuilder, StringBuilder valuesBuilder)
    {
        sqlSegment = this.VisitAndDeferred(sqlSegment);
        if (index > 0)
        {
            fieldsBuilder.Append(',');
            valuesBuilder.Append(',');
        }
        fieldsBuilder.Append(this.OrmProvider.GetFieldName(memberMapper.FieldName));
        if (sqlSegment == SqlSegment.Null)
            valuesBuilder.Append("NULL");
        else
        {
            sqlSegment.IsParameterized = true;
            sqlSegment.MemberMapper = memberMapper;
            sqlSegment.ParameterName = memberMapper.MemberName;
            valuesBuilder.Append(this.GetQuotedValue(sqlSegment));
        }
    }
}
public enum InsertFieldType
{
    Fields,
    FromTables
}
public struct InsertField
{
    public InsertFieldType Type { get; set; }
    public string Fields { get; set; }
    public string Values { get; set; }
    public string FromTables { get; set; }
}