﻿using System;
using System.Linq.Expressions;
using System.Text;

namespace Trolley.SqlServer;

public class SqlServerUpdateVisitor : UpdateVisitor, IUpdateVisitor
{
    public SqlServerUpdateVisitor(IOrmProvider ormProvider, IEntityMapProvider mapProvider, bool isParameterized = false, char tableAsStart = 'a', string parameterPrefix = "p")
        : base(ormProvider, mapProvider, isParameterized, tableAsStart, parameterPrefix) { }

    public override string BuildSql()
    {
        var entityTableName = this.OrmProvider.GetTableName(this.Tables[0].Mapper.TableName);
        var builder = new StringBuilder($"UPDATE ");
        if (!this.IsJoin) builder.Append($"{entityTableName} ");
        var aliasName = this.Tables[0].AliasName;
        if (this.IsNeedTableAlias || this.IsJoin)
            builder.Append($"{aliasName} ");

        if (this.IsJoin && this.Tables.Count > 1)
        {
            builder.Append($" FROM {entityTableName} {aliasName} ");
            for (var i = 1; i < this.Tables.Count; i++)
            {
                var tableSegment = this.Tables[i];
                var tableName = tableSegment.Body;
                if (string.IsNullOrEmpty(tableName))
                    tableName = this.OrmProvider.GetTableName(tableSegment.Mapper.TableName);
                builder.Append($"{tableSegment.JoinType} {tableName} {tableSegment.AliasName}");
                builder.Append($" ON {tableSegment.OnExpr} ");
            }
        }
        int index = 0;
        builder.Append("SET ");
        if (this.UpdateFields != null && this.UpdateFields.Count > 0)
        {
            //两种场景都不加别名
            foreach (var setField in this.UpdateFields)
            {
                if (index > 0) builder.Append(',');
                builder.Append($"{setField}");
                index++;
            }
        }
        if (!string.IsNullOrEmpty(this.WhereSql))
        {
            builder.Append(" WHERE ");
            builder.Append(this.WhereSql);
        }
        return builder.ToString();
    }
    public override IUpdateVisitor Join(string joinType, Type entityType, Expression joinOn)
    {
        this.IsJoin = true;
        var lambdaExpr = joinOn as LambdaExpression;
        var joinTable = new TableSegment
        {
            EntityType = entityType,
            AliasName = $"{(char)('a' + this.Tables.Count)}",
            JoinType = joinType
        };
        this.Tables.Add(joinTable);
        this.InitTableAlias(lambdaExpr);
        joinTable.OnExpr = this.VisitConditionExpr(lambdaExpr.Body);
        return this;
    }
    public override IUpdateVisitor From(params Type[] entityTypes)
        => throw new NotSupportedException("SqlServer不支持Update From语法，请使用Update InnerJoin/LeftJoin语法");
}
