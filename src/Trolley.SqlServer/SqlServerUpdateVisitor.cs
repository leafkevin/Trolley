using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Trolley.SqlServer;

public class SqlServerUpdateVisitor : UpdateVisitor, IUpdateVisitor
{
    public SqlServerUpdateVisitor(string dbKey, IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType, bool isParameterized = false, char tableAsStart = 'a', string parameterPrefix = "p")
      : base(dbKey, ormProvider, mapProvider, entityType, isParameterized, tableAsStart, parameterPrefix)
    {
    }
    public override string BuildSql(out List<IDbDataParameter> dbParameters)
    {
        var entityTableName = this.OrmProvider.GetTableName(this.tables[0].Mapper.TableName);
        var builder = new StringBuilder($"UPDATE {entityTableName} ");

        builder.Append("SET ");
        builder.Append(this.setSql);
        if (this.isFrom && this.tables.Count > 1)
        {
            builder.Append(" FROM ");
            for (var i = 1; i < this.tables.Count; i++)
            {
                var tableSegment = this.tables[i];
                var tableName = tableSegment.Body;
                if (string.IsNullOrEmpty(tableName))
                {
                    tableSegment.Mapper ??= this.mapProvider.GetEntityMap(tableSegment.EntityType);
                    tableName = this.OrmProvider.GetTableName(tableSegment.Mapper.TableName);
                }
                builder.Append($"{tableName} {tableSegment.AliasName}");
            }
        }

        if (!string.IsNullOrEmpty(this.whereSql))
            builder.Append(this.whereSql);
        dbParameters = this.dbParameters;
        return builder.ToString();
    }
    public override IUpdateVisitor From(params Type[] entityTypes)
    {
        this.IsNeedAlias = true;
        this.isFrom = true;
        int tableIndex = this.TableAsStart + this.tables.Count;
        for (int i = 0; i < entityTypes.Length; i++)
        {
            this.tables.Add(new TableSegment
            {
                EntityType = entityTypes[i],
                AliasName = $"{(char)(tableIndex + i)}"
            });
        }
        return this;
    }
    public override IUpdateVisitor Join(string joinType, Type entityType, Expression joinOn)
    {
        throw new NotSupportedException("SqlServer不支持Update Join语法，支持Update From语法");
    }
    public override IUpdateVisitor Set(Expression fieldsExpr, object fieldValue = null)
    {
        var lambdaExpr = fieldsExpr as LambdaExpression;
        var entityMapper = this.tables[0].Mapper;
        MemberMap memberMapper = null;
        var setFields = new List<SetField>();

        var builder = new StringBuilder();
        if (!string.IsNullOrEmpty(this.setSql))
        {
            builder.Append(this.setSql);
            builder.Append(',');
        }
        switch (lambdaExpr.Body.NodeType)
        {
            case ExpressionType.MemberAccess:
                var memberExpr = lambdaExpr.Body as MemberExpression;
                memberMapper = entityMapper.GetMemberMap(memberExpr.Member.Name);
                setFields.Add(this.AddMemberElement(memberMapper, fieldValue));
                break;
            case ExpressionType.New:
                this.InitTableAlias(lambdaExpr);
                var newExpr = lambdaExpr.Body as NewExpression;
                for (int i = 0; i < newExpr.Arguments.Count; i++)
                {
                    var memberInfo = newExpr.Members[i];
                    if (!entityMapper.TryGetMemberMap(memberInfo.Name, out memberMapper))
                        continue;

                    var argumentExpr = newExpr.Arguments[i];
                    var sqlSegment = this.VisitAndDeferred(new SqlSegment { Expression = argumentExpr });
                    //只一个成员访问，没有设置语句，什么也不做，忽略
                    if (sqlSegment.HasField && !sqlSegment.IsExpression && !sqlSegment.IsMethodCall && !sqlSegment.IsMethodCall && sqlSegment.FromMember.Name == memberInfo.Name)
                        continue;
                    setFields.Add(this.AddMemberElement(sqlSegment, memberMapper));
                }
                break;
            case ExpressionType.MemberInit:
                this.InitTableAlias(lambdaExpr);
                var memberInitExpr = lambdaExpr.Body as MemberInitExpression;
                for (int i = 0; i < memberInitExpr.Bindings.Count; i++)
                {
                    var memberAssignment = memberInitExpr.Bindings[i] as MemberAssignment;
                    if (!entityMapper.TryGetMemberMap(memberAssignment.Member.Name, out memberMapper))
                        continue;

                    var argumentExpr = memberAssignment.Expression;
                    var sqlSegment = this.VisitAndDeferred(new SqlSegment { Expression = argumentExpr });
                    //只一个成员访问，没有设置语句，什么也不做，忽略
                    if (sqlSegment.HasField && !sqlSegment.IsExpression && !sqlSegment.IsMethodCall && sqlSegment.FromMember.Name == memberAssignment.Member.Name)
                        continue;
                    setFields.Add(this.AddMemberElement(sqlSegment, memberMapper));
                }
                break;
        }
        if (setFields != null && setFields.Count > 0)
        {
            for (int i = 0; i < setFields.Count; i++)
            {
                if (i > 0) builder.Append(',');
                builder.Append($"{OrmProvider.GetFieldName(setFields[i].MemberMapper.FieldName)}={setFields[i].Value}");
            }
        }
        this.setSql = builder.ToString();
        return this;
    }
    public override IUpdateVisitor SetFromQuery(Expression fieldsExpr, Expression valueExpr = null)
    {
        var lambdaExpr = fieldsExpr as LambdaExpression;
        var entityMapper = this.tables[0].Mapper;
        MemberMap memberMapper = null;
        List<ParameterExpression> argumentParameters = null;
        var setFields = new List<SetField>();
        var builder = new StringBuilder();
        if (!string.IsNullOrEmpty(this.setSql))
        {
            builder.Append(this.setSql);
            builder.Append(',');
        }
        switch (lambdaExpr.Body.NodeType)
        {
            case ExpressionType.MemberAccess:
                {
                    var memberExpr = lambdaExpr.Body as MemberExpression;
                    memberMapper = entityMapper.GetMemberMap(memberExpr.Member.Name);
                    lambdaExpr = valueExpr as LambdaExpression;
                    this.InitTableAlias(lambdaExpr);
                    var sql = this.VisitFromQuery(lambdaExpr, out var isNeedAlias);
                    if (isNeedAlias) this.IsNeedAlias = true;
                    setFields.Add(new SetField { MemberMapper = memberMapper, Value = $"({sql})" });
                }
                break;
            case ExpressionType.New:
                this.InitTableAlias(lambdaExpr);
                var newExpr = lambdaExpr.Body as NewExpression;
                for (int i = 0; i < newExpr.Arguments.Count; i++)
                {
                    var memberInfo = newExpr.Members[i];
                    if (!entityMapper.TryGetMemberMap(memberInfo.Name, out memberMapper))
                        continue;

                    var argumentExpr = newExpr.Arguments[i];
                    if (argumentExpr.GetParameters(out argumentParameters)
                        && argumentParameters.Exists(f => f.Type == typeof(IFromQuery)))
                    {
                        var newLambdaExpr = Expression.Lambda(argumentExpr, lambdaExpr.Parameters.ToList());
                        var sql = this.VisitFromQuery(newLambdaExpr, out var isNeedAlias);
                        if (isNeedAlias) this.IsNeedAlias = true;
                        setFields.Add(new SetField { MemberMapper = memberMapper, Value = $"({sql})" });
                    }
                    else
                    {
                        var sqlSegment = this.VisitAndDeferred(new SqlSegment { Expression = argumentExpr });
                        //只一个成员访问，没有设置语句，什么也不做，忽略
                        if (sqlSegment.HasField && !sqlSegment.IsExpression && !sqlSegment.IsMethodCall && sqlSegment.FromMember.Name == memberInfo.Name)
                            continue;
                        setFields.Add(this.AddMemberElement(sqlSegment, memberMapper));
                    }
                }
                break;
            case ExpressionType.MemberInit:
                this.InitTableAlias(lambdaExpr);
                var memberInitExpr = lambdaExpr.Body as MemberInitExpression;
                for (int i = 0; i < memberInitExpr.Bindings.Count; i++)
                {
                    var memberAssignment = memberInitExpr.Bindings[i] as MemberAssignment;
                    if (!entityMapper.TryGetMemberMap(memberAssignment.Member.Name, out memberMapper))
                        continue;

                    var argumentExpr = memberAssignment.Expression;
                    if (argumentExpr.GetParameters(out argumentParameters)
                        && argumentParameters.Exists(f => f.Type == typeof(IFromQuery)))
                    {
                        var newLambdaExpr = Expression.Lambda(argumentExpr, lambdaExpr.Parameters.ToList());
                        var sql = this.VisitFromQuery(newLambdaExpr, out var isNeedAlias);
                        if (isNeedAlias) this.IsNeedAlias = true;
                        setFields.Add(new SetField { MemberMapper = memberMapper, Value = $"({sql})" });
                    }
                    else
                    {
                        var sqlSegment = this.VisitAndDeferred(new SqlSegment { Expression = argumentExpr });
                        //只一个成员访问，没有设置语句，什么也不做，忽略
                        if (sqlSegment.HasField && !sqlSegment.IsExpression && !sqlSegment.IsMethodCall && sqlSegment.FromMember.Name == memberAssignment.Member.Name)
                            continue;
                        setFields.Add(this.AddMemberElement(sqlSegment, memberMapper));
                    }
                }
                break;
        }
        if (setFields != null && setFields.Count > 0)
        {
            for (int i = 0; i < setFields.Count; i++)
            {
                if (i > 0) builder.Append(',');
                builder.Append($"{OrmProvider.GetFieldName(setFields[i].MemberMapper.FieldName)}={setFields[i].Value}");
            }
        }
        this.setSql = builder.ToString();
        return this;
    }
}
