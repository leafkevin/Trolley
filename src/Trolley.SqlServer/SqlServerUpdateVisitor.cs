using System;
using System.Linq.Expressions;
using System.Text;

namespace Trolley.SqlServer;

public class SqlServerUpdateVisitor : UpdateVisitor, IUpdateVisitor
{
    public SqlServerUpdateVisitor(string dbKey, IOrmProvider ormProvider, IEntityMapProvider mapProvider, bool isParameterized = false, char tableAsStart = 'a', string parameterPrefix = "p")
        : base(dbKey, ormProvider, mapProvider, isParameterized, tableAsStart, parameterPrefix) { }

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
                builder.Append($"{setField.Fields}={setField.Values}");
                index++;
            }
        }
        if (!string.IsNullOrEmpty(this.WhereSql) || (this.WhereFields != null && this.WhereFields.Count > 0))
            builder.Append(" WHERE ");
        bool hasWhere = false;
        if (this.WhereFields != null && this.WhereFields.Count > 0)
        {
            index = 0;
            foreach (var whereField in this.WhereFields)
            {
                if (index > 0) builder.Append(" AND ");
                if (this.IsNeedTableAlias) builder.Append($"{aliasName}");
                builder.Append($"{whereField.Fields}={whereField.Values}");
                index++;
            }
            hasWhere = true;
        }
        if (!string.IsNullOrEmpty(this.WhereSql))
        {
            if (hasWhere)
                builder.Append(" AND ");
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
