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
    protected List<UpdateField> UpdateFields { get; set; } = new();
    protected bool IsUseIgnore { get; set; }
    protected bool IsUseIfNotExists { get; set; }
    protected bool IsUseOrUpdate { get; set; }
    protected virtual bool IsBulk { get; set; } = false;

    public CreateVisitor(string dbKey, IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType, bool isParameterized = false, char tableAsStart = 'a', string parameterPrefix = "p", List<IDbDataParameter> dbParameters = null)
        : base(dbKey, ormProvider, mapProvider, isParameterized, tableAsStart, parameterPrefix, "", dbParameters)
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
        this.DbParameters ??= new();
    }
    public virtual string BuildSql(out List<IDbDataParameter> dbParameters)
    {
        var tableName = this.OrmProvider.GetTableName(this.Tables[0].Mapper.TableName);
        var fieldsBuilder = new StringBuilder($"{this.BuildHeadSql()} {tableName} (");
        var valuesBuilder = new StringBuilder();
        if (this.IsUseIfNotExists) valuesBuilder.Append(" SELECT ");
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
        }
        fieldsBuilder.Append(')');
        if (this.IsUseIfNotExists)
        {
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
    public virtual ICreateVisitor IfNotExists(object whereObj)
    {
        this.IsUseIfNotExists = true;
        var commandInitializer = RepositoryHelper.BuildWhereWithKeysSqlParameters(this, this.Tables[0].EntityType, whereObj);
        this.WhereSql = commandInitializer.Invoke(this, this.DbParameters, whereObj);
        return this;
    }
    public virtual ICreateVisitor IfNotExists(Expression keysPredicate)
    {
        this.IsUseIfNotExists = true;
        this.IsWhere = true;
        var lambdaExpr = keysPredicate as LambdaExpression;
        this.InitTableAlias(lambdaExpr);
        this.LastWhereNodeType = OperationType.None;
        this.WhereSql = this.VisitConditionExpr(lambdaExpr.Body);
        this.IsWhere = false;
        return this;
    }
    public virtual ICreateVisitor Set(Expression fieldsAssignment)
    {
        this.IsUseOrUpdate = true;
        var lambdaExpr = fieldsAssignment as LambdaExpression;
        var entityMapper = this.Tables[0].Mapper;
        switch (lambdaExpr.Body.NodeType)
        {
            case ExpressionType.New:
                this.InitTableAlias(lambdaExpr);
                var newExpr = lambdaExpr.Body as NewExpression;
                for (int i = 0; i < newExpr.Arguments.Count; i++)
                {
                    var memberInfo = newExpr.Members[i];
                    if (!entityMapper.TryGetMemberMap(memberInfo.Name, out var memberMapper))
                        continue;

                    var sqlSegment = this.VisitAndDeferred(new SqlSegment { Expression = newExpr.Arguments[i], MemberMapper = memberMapper });
                    //只一个成员访问，没有设置语句，什么也不做，忽略
                    if (sqlSegment.HasField && !sqlSegment.IsExpression && !sqlSegment.IsMethodCall && sqlSegment.FromMember.Name == memberInfo.Name)
                        continue;
                    this.AddMemberElement(sqlSegment, memberMapper);
                }
                break;
            case ExpressionType.MemberInit:
                this.InitTableAlias(lambdaExpr);
                var memberInitExpr = lambdaExpr.Body as MemberInitExpression;
                for (int i = 0; i < memberInitExpr.Bindings.Count; i++)
                {
                    var memberAssignment = memberInitExpr.Bindings[i] as MemberAssignment;
                    if (!entityMapper.TryGetMemberMap(memberAssignment.Member.Name, out var memberMapper))
                        continue;

                    var sqlSegment = this.VisitAndDeferred(new SqlSegment { Expression = memberAssignment.Expression });
                    //只一个成员访问，没有设置语句，什么也不做，忽略
                    if (sqlSegment.HasField && !sqlSegment.IsExpression && !sqlSegment.IsMethodCall && sqlSegment.FromMember.Name == memberAssignment.Member.Name)
                        continue;
                    this.AddMemberElement(sqlSegment, memberMapper);
                }
                break;
        }
        return this;
    }
    public virtual ICreateVisitor Set(object updateObj)
    {
        this.IsUseOrUpdate = true;
        var entityMapper = this.Tables[0].Mapper;
        var parametersInitializer = RepositoryHelper.BuildUpdateSetWithParameters(this, entityMapper.EntityType, updateObj, false);
        parametersInitializer.Invoke(this, this.UpdateFields, this.DbParameters, updateObj);
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
    public virtual IQueryVisitor CreateQuery(params Type[] sourceTypes)
    {
        var queryVisiter = this.OrmProvider.NewQueryVisitor(this.DbKey, this.MapProvider, this.IsParameterized, this.TableAsStart, this.ParameterPrefix, "", this.DbParameters);
        queryVisiter.From(this.TableAsStart, sourceTypes);
        return queryVisiter;
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
    protected void AddMemberElement(int index, SqlSegment sqlSegment, MemberMap memberMapper, StringBuilder fieldsBuilder, StringBuilder valuesBuilder)
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
    protected void AddMemberElement(SqlSegment sqlSegment, MemberMap memberMapper)
    {
        if (sqlSegment == SqlSegment.Null)
        {
            this.UpdateFields.Add(new UpdateField { Type = UpdateFieldType.SetValue, MemberMapper = memberMapper, Value = "NULL" });
            return;
        }
        sqlSegment.IsParameterized = true;
        sqlSegment.MemberMapper = memberMapper;
        sqlSegment.ParameterName = "u" + memberMapper.MemberName;
        this.UpdateFields.Add(new UpdateField { MemberMapper = memberMapper, Value = this.GetQuotedValue(sqlSegment) });
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
}