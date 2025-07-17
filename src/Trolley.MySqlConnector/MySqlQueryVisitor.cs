using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Trolley.MySqlConnector;

public class MySqlQueryVisitor : QueryVisitor
{
    private MySqlProvider dialectProvider => this.OrmProvider as MySqlProvider;
    public bool IsUseIgnoreInto { get; set; }
    public MySqlQueryVisitor(DbContext dbContext)
        : base(dbContext) { }
    public MySqlQueryVisitor(DbContext dbContext, char tableAsStart, IDataParameterCollection dbParameters = null)
        : base(dbContext, tableAsStart, dbParameters) { }

    public override string BuildCommandSql(bool isBuildCteSql, out IDataParameterCollection dbParameters)
    {
        var builder = new StringBuilder();
        var entityMapper = this.Tables[0].Mapper;
        if (this.IsUseIgnoreInto)
            builder.Append("INSERT IGNORE INTO");
        else builder.Append("INSERT INTO");
        builder.Append($" {this.GetTableName(this.Tables[0])} (");
        int index = 0;
        if (this.ReaderFields == null && this.IsFromQuery)
            this.ReaderFields = this.Tables[1].Fields;
        foreach (var readerField in this.ReaderFields)
        {
            //Union后，如果没有select语句时，通常实体类型或是select分组对象
            var memberName = readerField.TargetMember.Name;
            if (!entityMapper.TryGetMemberMap(memberName, out var memberMapper)
                || memberMapper.IsIgnore || memberMapper.IsIgnoreInsert
                || memberMapper.IsNavigation || memberMapper.IsAutoIncrement || memberMapper.IsRowVersion)
                continue;
            if (index > 0) builder.Append(',');
            builder.Append($"{this.OrmProvider.GetFieldName(memberMapper.FieldName)}");
            index++;
        }
        builder.Append(") ");
        //有CTE表
        if (isBuildCteSql && this.RefQueries != null && this.RefQueries.Count > 0)
        {
            var fieldsSql = builder.ToString();
            builder.Clear();
            bool isRecursive = false;
            var cteQueries = this.FlattenRefCteTables(this.RefQueries);
            if (cteQueries.Count > 0)
            {
                builder.AppendLine();
                for (int i = 0; i < cteQueries.Count; i++)
                {
                    if (i > 0) builder.AppendLine(",");
                    builder.Append(cteQueries[i].Body);
                    if (cteQueries[i].IsRecursive)
                        isRecursive = true;
                }
                if (isRecursive)
                    builder.Insert(0, "WITH RECURSIVE ");
                else builder.Insert(0, "WITH ");
                builder.AppendLine();
            }
            builder.Insert(0, fieldsSql);
        }
        dbParameters = this.DbParameters;
        string sql = null;
        if (!string.IsNullOrEmpty(this.UnionSql))
        {
            builder.Append(this.UnionSql);
            sql = builder.ToString();
            builder.Clear();
            return sql;
        }
        var headSql = builder.ToString();
        builder.Clear();

        //先判断表是否有多分表isManySharding
        string tableSql = null;
        var hasShardingTables = this.ShardingTables != null && this.ShardingTables.Count > 0;
        if (this.Tables.Count > 0)
        {
            for (int i = 1; i < this.Tables.Count; i++)
            {
                var tableSegment = this.Tables[i];
                string tableName = this.GetTableName(tableSegment);
                if (i > 1)
                {
                    if (!string.IsNullOrEmpty(tableSegment.JoinType))
                    {
                        builder.Append(' ');
                        builder.Append($"{tableSegment.JoinType} ");
                    }
                    else builder.Append(',');
                }
                builder.Append(tableName);
                //子查询要设置表别名               
                builder.Append(" " + tableSegment.AliasName);
                if (!string.IsNullOrEmpty(tableSegment.SuffixRawSql))
                    builder.Append(" " + tableSegment.SuffixRawSql);
                if (!string.IsNullOrEmpty(tableSegment.OnExpr))
                    builder.Append($" ON {tableSegment.OnExpr}");
                if (hasShardingTables && this.ShardingTables[0] == tableSegment
                    && tableSegment.TableNames != null && tableSegment.TableNames.Count > 1)
                    this.IsManyShardingTables = true;
            }
            tableSql = builder.ToString();
        }
        builder.Clear();

        //各种单值查询，如：SELECT COUNT(*)/MAX(*)..等，都有SELECT操作     
        //如：From(f=>...).InnerJoin/UnionAll(f=>...)
        //生成sql时，include表的字段，一定要紧跟着主表字段后面，方便赋值主表实体的属性中，所以在插入时候就排好序
        //方案：在buildSql时确定，ReaderFields要重新排好序，include字段放到对应主表字段后面，表别名顺序不变
        if (this.ReaderFields == null)
            throw new Exception("缺少Select语句");

        if (this.IsManyShardingTables)
        {
            if (!string.IsNullOrEmpty(this.GroupBySql))
            {
                //当有多分表时，有分组，Select字段中，没有完全的分组字段，则需要补全所有分组字段
                foreach (var groupByField in this.GroupByFields)
                {
                    var memberInfo = groupByField.TargetMember ?? groupByField.FromMember;
                    if (this.ReaderFields.Exists(f => f.IsGroupByField && f.TargetMember.Name == memberInfo.Name || f.IsGroupingField))
                        continue;
                    this.ReaderFields.Add(groupByField);
                }
            }
            if (!string.IsNullOrEmpty(this.OrderBySql))
            {
                //当有多分表时，有排序，Select字段中，没有完全的排序字段，则需要补全所有排序字段
                var hasGrouping = this.ReaderFields.Exists(f => f.IsGroupingField);
                foreach (var orderByField in this.OrderByFields)
                {
                    var memberInfo = orderByField.Field.TargetMember ?? orderByField.Field.FromMember;
                    if (this.ReaderFields.Exists(f => f.TargetMember.Name == memberInfo.Name || f.FromMember == memberInfo))
                        continue;
                    if (hasGrouping && this.GroupByFields.Exists(f => f.TargetMember.Name == memberInfo.Name || f.FromMember == memberInfo))
                        continue;
                    this.ReaderFields.Add(orderByField.Field);
                }
            }
        }

        this.AddSelectFieldsSql(builder, this.ReaderFields);
        if (this.IsManyShardingTables && this.AggFieldAlias != null)
            builder.Append($" AS {this.AggFieldAlias}");

        string selectSql = null;
        if (this.IsDistinct)
            selectSql = "DISTINCT " + builder.ToString();
        else selectSql = builder.ToString();

        builder.Clear();
        if (!string.IsNullOrEmpty(this.WhereSql))
            builder.Append($" WHERE {this.WhereSql}");

        if (!string.IsNullOrEmpty(this.GroupBySql))
            builder.Append($" GROUP BY {this.GroupBySql}");
        if (!string.IsNullOrEmpty(this.HavingSql))
            builder.Append($" HAVING {this.HavingSql}");

        string orderBy = null;
        if (!string.IsNullOrEmpty(this.OrderBySql) && (!this.IsManyShardingTables
            || (this.IsManyShardingTables && !this.skip.HasValue && this.limit.HasValue)))
        {
            orderBy = $"ORDER BY {this.OrderBySql}";
            if (!this.skip.HasValue && !this.limit.HasValue)
                builder.Append(" " + orderBy);
        }
        string others = builder.ToString();

        builder.Clear();
        if (!string.IsNullOrEmpty(headSql))
            builder.Append(headSql);

        if (!this.IsManyShardingTables && (this.skip.HasValue || this.limit.HasValue)
            || (this.IsManyShardingTables && !this.skip.HasValue && this.limit.HasValue))
        {
            //SQL TEMPLATE:SELECT /**fields**/ FROM /**tables**/ /**others**/
            var pageSql = this.OrmProvider.GetPagingTemplate(this.skip, this.limit, orderBy);
            pageSql = pageSql.Replace("/**fields**/", selectSql);
            pageSql = pageSql.Replace("/**tables**/", tableSql);
            pageSql = pageSql.Replace(" /**others**/", others);
            builder.Append($"{pageSql}");
        }
        else builder.Append($"SELECT {selectSql} FROM {tableSql}{others}");

        if (this.IsManyShardingTables && (!string.IsNullOrEmpty(this.GroupBySql) || !string.IsNullOrEmpty(this.OrderBySql) || this.skip.HasValue || this.limit.HasValue))
            this.IsNeedUnionShardingTables = true;

        //判断是否需要SELECT * FROM包装，UNION的子查询中有OrderBy或是Limit，就要包一下SELECT * FROM，否则数据结果不正确
        bool isNeedWrap = ((this.IsUnion || this.IsSecondUnion) && (!string.IsNullOrEmpty(this.OrderBySql) || this.limit.HasValue))
            || (this.IsManyShardingTables && !this.skip.HasValue && this.limit.HasValue);
        if (isNeedWrap)
        {
            builder.Insert(0, "SELECT * FROM (");
            builder.Append($") a");
        }
        sql = builder.ToString();
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
    public override SqlFieldSegment VisitGroupConcatMethodCall(SqlFieldSegment sqlSegment)
    {
        var methodCallExpr = sqlSegment.Expression as MethodCallExpression;
        var currentExpr = methodCallExpr.Object;
        var callStack = new Stack<MethodCallExpression>();
        while (currentExpr is MethodCallExpression callExpr)
        {
            if (callExpr.Type == typeof(Sql))
                break;
            callStack.Push(callExpr);
            currentExpr = callExpr.Object;
        }
        var builder = new StringBuilder();
        bool hasOrder = false, hasDistinct = false;
        string fieldsSql = null, separator = null, orderBySql = null;
        SqlFieldSegment fieldsSegment = null;
        while (callStack.TryPop(out methodCallExpr))
        {
            switch (methodCallExpr.Method.Name)
            {
                case "GroupConcat":
                    fieldsSegment = this.Visit(new SqlFieldSegment { Expression = methodCallExpr.Arguments[0] });
                    this.AddVisitedFieldsSqlWithoutAlias(builder, fieldsSegment);
                    fieldsSql = builder.ToString();
                    builder.Clear();
                    if (methodCallExpr.Arguments.Count > 1)
                        separator = this.Evaluate<string>(methodCallExpr.Arguments[1]);
                    break;
                case "OrderBy":
                    fieldsSegment = this.Visit(new SqlFieldSegment { Expression = methodCallExpr.Arguments[0] });
                    if (hasOrder) builder.Append(',');
                    else builder.Append("ORDER BY ");
                    this.AddVisitedFieldsSqlWithoutAlias(builder, fieldsSegment);
                    hasOrder = true;
                    break;
                case "OrderByDescending":
                    fieldsSegment = this.Visit(new SqlFieldSegment { Expression = methodCallExpr.Arguments[0] });
                    if (hasOrder) builder.Append(',');
                    else builder.Append("ORDER BY ");
                    this.AddVisitedFieldsSqlWithoutAlias(builder, fieldsSegment, " DESC");
                    hasOrder = true;
                    break;
                case "Distinct":
                    hasDistinct = true;
                    break;
            }
        }
        if (hasOrder) orderBySql = builder.ToString();
        builder.Clear();
        builder.Append($"GROUP_CONCAT(");
        if (hasDistinct) builder.Append("DISTINCT ");
        builder.Append(fieldsSql);
        if (hasOrder) builder.Append($" {orderBySql}");
        if (!string.IsNullOrEmpty(separator))
            builder.Append($" SEPARATOR '{this.OrmProvider.GetQuotedValue(typeof(string), separator)}'");
        builder.Append(')');
        fieldsSql = builder.ToString();
        builder.Clear();
        return sqlSegment.Change(fieldsSql, false, true);
    }
    public override SqlFieldSegment VisitStringAggMethodCall(SqlFieldSegment sqlSegment)
        => throw new NotSupportedException("不支持的方法调用，请考虑使用Sql.GroupConcat方法");
}
