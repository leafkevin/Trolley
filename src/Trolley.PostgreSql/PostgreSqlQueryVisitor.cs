using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Text;

namespace Trolley.PostgreSql;

public class PostgreSqlQueryVisitor : QueryVisitor
{
    private bool isDisposed;
    private bool isDistinctOn;

    public List<SqlSegment> DistinctOnFields { get; set; }
    public string DistinctOnSql { get; set; }

    public PostgreSqlQueryVisitor(DbContext dbContext, char tableAsStart = 'a', IDataParameterCollection dbParameters = null)
        : base(dbContext, tableAsStart, dbParameters) { }

    public override string BuildSql(bool isBuildCteSql, out List<SqlSegment> readerFields)
    {
        var builder = new StringBuilder();
        if (isBuildCteSql && this.RefQueries != null && this.RefQueries.Count > 0)
        {
            bool isRecursive = false;
            var cteQueries = this.FlattenRefCteTables(this.RefQueries);
            if (cteQueries.Count > 0)
            {
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
        }
        readerFields = this.ReaderFields;

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

        //先判断表是否有多分表IsManyShardingTables
        string tableSql = null;
        var hasShardingTables = this.ShardingTables != null && this.ShardingTables.Count > 0;
        if (this.Tables.Count > 0)
        {
            for (int i = 0; i < this.Tables.Count; i++)
            {
                var tableSegment = this.Tables[i];
                string tableName = this.GetFormatTableName(tableSegment);
                if (i > 0)
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

        if (this.isDistinctOn)
            builder.Append($"DISTINCT ON ({this.DistinctOnSql}) ");

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
        if (this.IsManyShardingTables && this.IsNeedFormatShardingTables && this.AggFieldAlias != null)
            builder.Append($" AS {this.AggFieldAlias},COUNT(*) AS AVG_COUNT");

        string selectSql = null;
        if (this.IsDistinct)
            selectSql = "DISTINCT " + builder.ToString();
        else selectSql = builder.ToString();

        builder.Clear();
        if (this.WhereBuilder != null && this.WhereBuilder.Length > 0)
            builder.Append($" WHERE {this.WhereBuilder.ToString()}");
        //有多分表还有Group By操作，每个分表语句中做Group By操作，Union All语句后，还要再做Group By操作
        if (!string.IsNullOrEmpty(this.GroupBySql))
            builder.Append($" GROUP BY {this.GroupBySql}");
        //有多分表还有Group By+Having操作，每个分表语句中只做Group By操作，不做Having操作，在Union All语句后，再做Group By+Having操作
        if (!this.IsManyShardingTables && !string.IsNullOrEmpty(this.HavingSql))
            builder.Append($" HAVING {this.HavingSql}");

        string orderBy = null;
        if (!string.IsNullOrEmpty(this.OrderBySql) && (!this.IsManyShardingTables
            || (this.IsManyShardingTables && !this.offset.HasValue && this.limit.HasValue)))
        {
            orderBy = $"ORDER BY {this.OrderBySql}";
            if (!this.offset.HasValue && !this.limit.HasValue)
                builder.Append(" " + orderBy);
        }
        string others = builder.ToString();

        builder.Clear();
        if (!string.IsNullOrEmpty(headSql))
            builder.Append(headSql);

        if (!this.IsManyShardingTables && (this.offset.HasValue || this.limit.HasValue)
            || (this.IsManyShardingTables && !this.offset.HasValue && this.limit.HasValue))
        {
            //SQL TEMPLATE:SELECT /**fields**/ FROM /**tables**/ /**others**/
            var pageSql = this.OrmProvider.GetPagingTemplate(this.offset, this.limit, orderBy);
            pageSql = pageSql.Replace("/**fields**/", selectSql);
            pageSql = pageSql.Replace("/**tables**/", tableSql);
            pageSql = pageSql.Replace(" /**others**/", others);

            if (this.IsNeedPaging && this.offset.HasValue && this.limit.HasValue)
            {
                var myTableSql = $"{tableSql}{others}";
                if (this.HasAggFields || !string.IsNullOrEmpty(this.GroupBySql))
                    myTableSql = $"(SELECT {selectSql} FROM {tableSql}{others}) a";
                builder.Append($"SELECT COUNT(*) FROM {myTableSql};");
            }
            builder.Append($"{pageSql}");
        }
        else builder.Append($"SELECT {selectSql} FROM {tableSql}{others}");

        if (this.IsManyShardingTables && (!string.IsNullOrEmpty(this.GroupBySql)
            || !string.IsNullOrEmpty(this.OrderBySql) || this.offset.HasValue || this.limit.HasValue || this.HasAggFields))
            this.IsNeedUnionShardingTables = true;

        //判断是否需要SELECT * FROM包装，UNION的子查询中有OrderBy或是Limit，就要包一下SELECT * FROM，否则数据结果不正确
        bool isNeedWrap = ((this.IsUnion || this.IsSecondUnion) && (!string.IsNullOrEmpty(this.OrderBySql) || this.limit.HasValue))
            || (this.IsManyShardingTables && !this.offset.HasValue && this.limit.HasValue);
        if (isNeedWrap)
        {
            builder.Insert(0, "SELECT * FROM (");
            builder.Append($") a");
        }
        sql = builder.ToString();
        builder.Clear();
        return sql;
    }
    public override string BuildCommandSql(bool isBuildCteSql, out IDataParameterCollection dbParameters)
    {
        var entityMapper = this.Tables[0].Mapper;
        var builder = new StringBuilder($"INSERT INTO {this.GetFormatTableName(this.Tables[0])} (");
        int index = 0;
        //如果ReaderFields没有设置，通常是从Query中来的，ReaderFields是从Query中获取的
        //if (this.ReaderFields == null && this.IsFromQuery)
        //    this.ReaderFields = this.Tables[1].Fields;
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
            builder.Append(fieldsSql);
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
                string tableName = this.GetFormatTableName(tableSegment);
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

        if (this.isDistinctOn)
            builder.Append($"DISTINCT ON ({this.DistinctOnSql}) ");

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
        if (this.IsManyShardingTables && this.IsNeedFormatShardingTables && this.AggFieldAlias != null)
            builder.Append($" AS {this.AggFieldAlias},COUNT(*) AS AVG_COUNT");

        string selectSql = null;
        if (this.IsDistinct)
            selectSql = "DISTINCT " + builder.ToString();
        else selectSql = builder.ToString();

        builder.Clear();
        if (this.WhereBuilder != null && this.WhereBuilder.Length > 0)
            builder.Append($" WHERE {this.WhereBuilder.ToString()}");
        //有多分表还有Group By操作，每个分表语句中做Group By操作，Union All语句后，还要再做Group By操作
        if (!string.IsNullOrEmpty(this.GroupBySql))
            builder.Append($" GROUP BY {this.GroupBySql}");
        //有多分表还有Group By+Having操作，每个分表语句中只做Group By操作，不做Having操作，在Union All语句后，再做Group By+Having操作
        if (!this.IsManyShardingTables && !string.IsNullOrEmpty(this.HavingSql))
            builder.Append($" HAVING {this.HavingSql}");

        string orderBy = null;
        if (!string.IsNullOrEmpty(this.OrderBySql) && (!this.IsManyShardingTables
            || (this.IsManyShardingTables && !this.offset.HasValue && this.limit.HasValue)))
        {
            orderBy = $"ORDER BY {this.OrderBySql}";
            if (!this.offset.HasValue && !this.limit.HasValue)
                builder.Append(" " + orderBy);
        }
        string others = builder.ToString();

        builder.Clear();
        if (!string.IsNullOrEmpty(headSql))
            builder.Append(headSql);

        if (!this.IsManyShardingTables && (this.offset.HasValue || this.limit.HasValue)
            || (this.IsManyShardingTables && !this.offset.HasValue && this.limit.HasValue))
        {
            //SQL TEMPLATE:SELECT /**fields**/ FROM /**tables**/ /**others**/
            var pageSql = this.OrmProvider.GetPagingTemplate(this.offset, this.limit, orderBy);
            pageSql = pageSql.Replace("/**fields**/", selectSql);
            pageSql = pageSql.Replace("/**tables**/", tableSql);
            pageSql = pageSql.Replace(" /**others**/", others);

            if (this.IsNeedPaging && this.offset.HasValue && this.limit.HasValue)
            {
                var myTableSql = $"{tableSql}{others}";
                if (this.HasAggFields || !string.IsNullOrEmpty(this.GroupBySql))
                    myTableSql = $"(SELECT {selectSql} FROM {tableSql}{others}) a";
                builder.Append($"SELECT COUNT(*) FROM {myTableSql};");
            }
            builder.Append($"{pageSql}");
        }
        else builder.Append($"SELECT {selectSql} FROM {tableSql}{others}");

        if (this.IsManyShardingTables && (!string.IsNullOrEmpty(this.GroupBySql)
            || !string.IsNullOrEmpty(this.OrderBySql) || this.offset.HasValue || this.limit.HasValue || this.HasAggFields))
            this.IsNeedUnionShardingTables = true;

        //判断是否需要SELECT * FROM包装，UNION的子查询中有OrderBy或是Limit，就要包一下SELECT * FROM，否则数据结果不正确
        bool isNeedWrap = ((this.IsUnion || this.IsSecondUnion) && (!string.IsNullOrEmpty(this.OrderBySql) || this.limit.HasValue))
            || (this.IsManyShardingTables && !this.offset.HasValue && this.limit.HasValue);
        if (isNeedWrap)
        {
            builder.Insert(0, "SELECT * FROM (");
            builder.Append($") a");
        }
        sql = builder.ToString();
        builder.Clear();
        return sql;
    }
    public override void Distinct()
    {
        if (this.isDistinctOn)
            throw new NotSupportedException("使用了DistinctOn方法，无需再使用Distinct方法来去重了");
        this.IsDistinct = true;
    }
    public virtual void DistinctOn(Expression fieldsSelector)
    {
        this.isDistinctOn = true;
        var lambdaExpr = fieldsSelector as LambdaExpression;
        if (lambdaExpr.Body.NodeType != ExpressionType.New && lambdaExpr.Body.NodeType != ExpressionType.MemberAccess)
            throw new Exception("不支持的表达式访问，DistinctOn只支持New或MemberAccess表达式");

        this.ClearUnionSql();
        this.InitTableAlias(lambdaExpr);
        this.DistinctOnFields = new();
        switch (lambdaExpr.Body.NodeType)
        {
            case ExpressionType.New:
                var builder = new StringBuilder();
                int index = 0;
                var newExpr = lambdaExpr.Body as NewExpression;
                foreach (var argumentExpr in newExpr.Arguments)
                {
                    var memberInfo = newExpr.Members[index];
                    var sqlSegment = this.VisitAndDeferred(new SqlSegment { Expression = argumentExpr });
                    if (builder.Length > 0)
                        builder.Append(',');

                    var fieldName = sqlSegment.Body ?? sqlSegment.Value.ToString();
                    builder.Append(fieldName);
                    sqlSegment.TargetMember = memberInfo;
                    sqlSegment.SegmentType = memberInfo.GetMemberType();
                    this.DistinctOnFields.Add(sqlSegment);
                    index++;
                }
                //DistinctOn都不需要别名，没有别名
                this.DistinctOnSql = builder.ToString();
                break;
            case ExpressionType.MemberAccess:
                {
                    var memberExpr = lambdaExpr.Body as MemberExpression;
                    var sqlSegment = this.VisitAndDeferred(new SqlSegment { Expression = memberExpr });
                    var fieldName = sqlSegment.Body ?? sqlSegment.Value.ToString();
                    var memberInfo = memberExpr.Member;
                    sqlSegment.TargetMember = memberInfo;
                    sqlSegment.SegmentType = memberInfo.GetMemberType();
                    this.DistinctOnSql = fieldName;
                }
                break;
        }
    }
    public override void OrderBy(string orderType, Expression expr)
    {
        var lambdaExpr = expr as LambdaExpression;
        if (lambdaExpr.Body.NodeType != ExpressionType.New && lambdaExpr.Body.NodeType != ExpressionType.MemberAccess)
            throw new Exception("不支持的表达式访问，OrderBy只支持New或MemberAccess表达式");

        this.ClearUnionSql();
        var builder = new StringBuilder();
        if (!string.IsNullOrEmpty(this.OrderBySql))
            builder.Append(this.OrderBySql + ",");

        this.OrderByFields ??= new();
        //能够访问Grouping属性的场景，通常是在最外层的Select子句或是OrderBy子句
        //访问Grouping字段，并且Grouping对象是一个字段
        if (this.IsGroupingMember(lambdaExpr.Body as MemberExpression))
        {
            for (int i = 0; i < this.GroupByFields.Count; i++)
            {
                if (i > 0) builder.Append(',');
                //order by 尽力取源字段值，不管是字段还是表达式，还是函数调用
                var fieldName = this.GroupByFields[i].Body ?? this.GroupByFields[i].Value.ToString();
                builder.Append(fieldName);
                var orderField = new OrderByField { Field = this.GroupByFields[i] };
                this.OrderByFields.Add(orderField);
                if (orderType == "DESC")
                {
                    builder.Append(" DESC");
                    orderField.OrderSuffix = " DESC";
                }
            }
        }
        else if (this.IsDistinctOnMember(lambdaExpr.Body as MemberExpression))
        {
            for (int i = 0; i < this.DistinctOnFields.Count; i++)
            {
                if (i > 0) builder.Append(',');
                //order by 尽力取源字段值，不管是字段还是表达式，还是函数调用
                var fieldName = this.DistinctOnFields[i].Body ?? this.DistinctOnFields[i].Value.ToString();
                builder.Append(fieldName);
                var orderField = new OrderByField { Field = this.DistinctOnFields[i] };
                this.OrderByFields.Add(orderField);
                if (orderType == "DESC")
                {
                    builder.Append(" DESC");
                    orderField.OrderSuffix = " DESC";
                }
            }
        }
        else
        {
            this.InitTableAlias(lambdaExpr);
            switch (lambdaExpr.Body.NodeType)
            {
                case ExpressionType.New:
                    int index = 0;
                    var newExpr = lambdaExpr.Body as NewExpression;
                    foreach (var argumentExpr in newExpr.Arguments)
                    {
                        //OrderBy访问分组
                        if (this.IsGroupingMember(argumentExpr as MemberExpression))
                        {
                            for (int i = 0; i < this.GroupByFields.Count; i++)
                            {
                                if (i > 0) builder.Append(',');
                                //order by 尽力取源字段值，不管是字段还是表达式，还是函数调用
                                var fieldName = this.GroupByFields[i].Body ?? this.GroupByFields[i].Value.ToString();
                                builder.Append(fieldName);
                                var orderField = new OrderByField { Field = this.GroupByFields[i] };
                                this.OrderByFields.Add(orderField);
                                if (orderType == "DESC")
                                {
                                    builder.Append(" DESC");
                                    orderField.OrderSuffix = " DESC";
                                }
                            }
                        }
                        else if (this.IsDistinctOnMember(argumentExpr as MemberExpression))
                        {
                            for (int i = 0; i < this.DistinctOnFields.Count; i++)
                            {
                                if (i > 0) builder.Append(',');
                                //order by 尽力取源字段值，不管是字段还是表达式，还是函数调用
                                var fieldName = this.DistinctOnFields[i].Body ?? this.DistinctOnFields[i].Value.ToString();
                                builder.Append(fieldName);
                                var orderField = new OrderByField { Field = this.DistinctOnFields[i] };
                                this.OrderByFields.Add(orderField);
                                if (orderType == "DESC")
                                {
                                    builder.Append(" DESC");
                                    orderField.OrderSuffix = " DESC";
                                }
                            }
                        }
                        else
                        {
                            var memberInfo = newExpr.Members[index];
                            var sqlSegment = this.VisitAndDeferred(new SqlSegment { Expression = argumentExpr });
                            if (index > 0) builder.Append(',');
                            //order by 尽力取源字段值，不管是字段还是表达式，还是函数调用
                            builder.Append(sqlSegment.Body ?? sqlSegment.Value.ToString());
                            var orderField = new OrderByField { Field = sqlSegment };
                            this.OrderByFields.Add(orderField);
                            if (orderType == "DESC")
                            {
                                builder.Append(" DESC");
                                orderField.OrderSuffix = " DESC";
                            }
                        }
                        index++;
                    }
                    break;
                case ExpressionType.MemberAccess:
                    var memberExpr = lambdaExpr.Body as MemberExpression;
                    if (this.IsGroupingMember(memberExpr))
                    {
                        for (int i = 0; i < this.GroupByFields.Count; i++)
                        {
                            if (i > 0) builder.Append(',');
                            //order by 尽力取源字段值，不管是字段还是表达式，还是函数调用
                            var fieldName = this.GroupByFields[i].Body ?? this.GroupByFields[i].Value.ToString();
                            builder.Append(fieldName);
                            var orderField = new OrderByField { Field = this.GroupByFields[i] };
                            this.OrderByFields.Add(orderField);
                            if (orderType == "DESC")
                            {
                                builder.Append(" DESC");
                                orderField.OrderSuffix = " DESC";
                            }
                        }
                    }
                    else if (this.IsGroupingMember(memberExpr.Expression as MemberExpression))
                    {
                        var readerField = this.GroupByFields.Find(f => f.TargetMember.Name == memberExpr.Member.Name);
                        //order by 尽力取源字段值，不管是字段还是表达式，还是函数调用
                        builder.Append(readerField.Body ?? readerField.Value.ToString());
                        var orderField = new OrderByField { Field = readerField };
                        this.OrderByFields.Add(orderField);
                        if (orderType == "DESC")
                        {
                            builder.Append(" DESC");
                            orderField.OrderSuffix = " DESC";
                        }
                    }
                    else if (this.IsDistinctOnMember(memberExpr))
                    {
                        for (int i = 0; i < this.DistinctOnFields.Count; i++)
                        {
                            if (i > 0) builder.Append(',');
                            //order by 尽力取源字段值，不管是字段还是表达式，还是函数调用
                            var fieldName = this.DistinctOnFields[i].Body ?? this.DistinctOnFields[i].Value.ToString();
                            builder.Append(fieldName);
                            var orderField = new OrderByField { Field = this.DistinctOnFields[i] };
                            this.OrderByFields.Add(orderField);
                            if (orderType == "DESC")
                            {
                                builder.Append(" DESC");
                                orderField.OrderSuffix = " DESC";
                            }
                        }
                    }
                    else if (this.IsDistinctOnMember(memberExpr.Expression as MemberExpression))
                    {
                        var readerField = this.DistinctOnFields.Find(f => f.TargetMember.Name == memberExpr.Member.Name);
                        //order by 尽力取源字段值，不管是字段还是表达式，还是函数调用
                        var fieldName = readerField.Body ?? readerField.Value.ToString();
                        builder.Append(fieldName);
                        var orderField = new OrderByField { Field = readerField };
                        this.OrderByFields.Add(orderField);
                        if (orderType == "DESC")
                        {
                            builder.Append(" DESC");
                            orderField.OrderSuffix = " DESC";
                        }
                    }
                    else
                    {
                        var sqlSegment = this.VisitAndDeferred(new SqlSegment { Expression = memberExpr });
                        //order by 尽力取源字段值，不管是字段还是表达式，还是函数调用
                        builder.Append(sqlSegment.Body ?? sqlSegment.Value.ToString());
                        var orderField = new OrderByField { Field = sqlSegment };
                        this.OrderByFields.Add(orderField);
                        if (orderType == "DESC")
                        {
                            builder.Append(" DESC");
                            orderField.OrderSuffix = " DESC";
                        }
                    }
                    break;
            }
        }
        this.OrderBySql = builder.ToString();
    }
    public virtual void SelectDistinctOn() => this.ReaderFields = this.DistinctOnFields;
    public override SqlSegment VisitMemberAccess(SqlSegment sqlSegment)
    {
        //Select场景，实体成员访问，返回ReaderField实体类型，ReaderFields并且有值，子ReaderFields的Body可无值
        //Select场景和Where场景，单个字段成员访(包括Json实体类型字段)，返回FromMember，TargetMember，字段类型，Body有值为带有别名的FieldName
        var memberExpr = sqlSegment.Expression as MemberExpression;
        var memberInfo = memberExpr.Member;

        if (sqlSegment.IsDeferredFields && this.IsSelect)
        {
            //延迟属性访问，两种场景：
            //主动延迟方法调用：如，把返回的枚举列转成描述，参数就是枚举列，返回值是对应的描述
            string fields = null;
            List<SqlSegment> readerFields = null;
            var visitor = new DeferredExpressionVisitor();
            visitor.Visit(memberExpr);
            //$"{f.OrderNo} : {f.TotalAmount.ToString("C")}"
            //f.TotalAmount.ToString("C")
            //"TotalAmount: " + (f.Price * f.Quantity).ToString("C")
            //this.DeferredInvoke(f.Price, f.Quantity)
            if (visitor.Members.Count > 0)
            {
                readerFields = new List<SqlSegment>();
                var builder = new StringBuilder();
                foreach (var argsExpr in visitor.Members)
                {
                    var argumentSegment = this.VisitAndDeferred(new SqlSegment { Expression = argsExpr });
                    if (argumentSegment.HasField)
                    {
                        sqlSegment.HasField = true;
                        var fieldName = argumentSegment.Body;
                        readerFields.Add(new SqlSegment
                        {
                            SegmentType = argsExpr.Type,
                            TargetMember = argsExpr.Member,
                            NativeDbType = argumentSegment.NativeDbType,
                            TypeHandler = argumentSegment.TypeHandler
                        });
                        if (builder.Length > 0)
                            builder.Append(',');
                        builder.Append(fieldName);
                    }
                }
                if (readerFields.Count > 0)
                    fields = builder.ToString();
            }

            if (readerFields == null)
                fields = "NULL";
            sqlSegment.IsDeferredFields = true;
            sqlSegment.FieldType = ReaderFieldType.DeferredFields;
            sqlSegment.Body = fields;
            sqlSegment.OriginalExpression = memberExpr;
            sqlSegment.Fields = readerFields;
            sqlSegment.IsMethodCall = true;
            return sqlSegment;
        }

        MemberAccessSqlFormatter formatter = null;
        if (memberExpr.Expression != null)
        {
            //Where(f=>... && !f.OrderId.HasValue && ...)
            //Where(f=>... f.OrderId.Value==10 && ...)
            //Select(f=>... ,f.OrderId.HasValue  ...)
            //Select(f=>... ,f.OrderId.Value==10  ...)
            if (Nullable.GetUnderlyingType(memberExpr.Member.DeclaringType) != null)
            {
                if (memberExpr.Member.Name == nameof(Nullable<bool>.HasValue))
                {
                    sqlSegment.Push(new DeferredExpr { OperationType = OperationType.Equal, Value = SqlSegment.Null });
                    sqlSegment.Push(new DeferredExpr { OperationType = OperationType.Not });
                    return this.Visit(sqlSegment.Next(memberExpr.Expression));
                }
                else if (memberExpr.Member.Name == nameof(Nullable<bool>.Value))
                    return this.Visit(sqlSegment.Next(memberExpr.Expression));
                else throw new ArgumentException($"不支持的MemberAccess操作，表达式'{memberExpr}'返回值不是boolean类型");
            }

            //各种类型实例成员访问，如：DateTime,TimeSpan,String.Length,List.Count
            if (this.OrmProvider.TryGetMemberAccessSqlFormatter(memberExpr, out formatter))
            {
                //Where(f=>... && f.CreatedAt.Month<5 && ...)
                //Where(f=>... && f.Order.OrderNo.Length==10 && ...)
                var targetSegment = sqlSegment.Next(memberExpr.Expression);
                sqlSegment = formatter.Invoke(this, targetSegment);
                sqlSegment.TargetMember = memberExpr.Member;
                return sqlSegment;
            }

            //此场景一定是select
            //Select((x, a, b, ... ) => new { x.Grouping, ... });
            if (this.IsGroupingMember(memberExpr))
            {
                List<SqlSegment> groupFields = new();
                //在子查询中，Select了Group分组对象，为了避免在Clear时，把GroupFields元素清掉，放到一个新列表中
                if (this.GroupByFields.Count > 1)
                {
                    this.GroupByFields.ForEach(f => groupFields.Add(f.Clone()));
                    sqlSegment.FieldType = ReaderFieldType.Entity;
                    sqlSegment.HasField = true;
                    sqlSegment.FromMember = memberInfo;
                    sqlSegment.TargetMember = memberInfo;
                    sqlSegment.SegmentType = memberInfo.GetMemberType();
                    sqlSegment.Fields = groupFields;
                }
                else sqlSegment = this.GroupByFields[0].Clone();
                sqlSegment.IsGroupingField = true;
                return sqlSegment;
            }
            else if (this.IsDistinctOnMember(memberExpr))
            {
                List<SqlSegment> distinctOnFields = new();
                //在子查询中，Select了Group分组对象，为了避免在Clear时，把GroupFields元素清掉，放到一个新列表中

                if (this.DistinctOnFields.Count > 1)
                {
                    this.DistinctOnFields.ForEach(f => distinctOnFields.Add(f));
                    sqlSegment.FieldType = ReaderFieldType.Entity;
                    sqlSegment.HasField = true;
                    sqlSegment.FromMember = memberInfo;
                    sqlSegment.TargetMember = memberInfo;
                    sqlSegment.SegmentType = memberInfo.GetMemberType();
                    sqlSegment.Fields = distinctOnFields;
                }
                //分组对象为单个字段，要返回单个字段，防止后面Reader处理实体时候报错
                //要返回原始FromMember，后续方便判断是否使用AS别名
                else sqlSegment = this.DistinctOnFields[0].Clone();
                return sqlSegment;
            }
            else if (this.IsGroupingMember(memberExpr.Expression as MemberExpression))
            {
                //此时是Grouping对象字段的引用，最外面可能会更改成员名称，要复制一份，防止更改Grouping对象中的字段
                var readerField = this.GroupByFields.Find(f => f.TargetMember.Name == memberInfo.Name);
                sqlSegment = readerField.Clone();
                sqlSegment.IsGroupingField = true;
                return sqlSegment;
            }
            else if (this.IsDistinctOnMember(memberExpr.Expression as MemberExpression))
            {
                //此时是Grouping对象字段的引用，最外面可能会更改成员名称，要复制一份，防止更改Grouping对象中的字段
                var readerField = this.DistinctOnFields.Find(f => f.TargetMember.Name == memberInfo.Name);
                sqlSegment = readerField.Clone();
                return sqlSegment;
            }
            if (memberExpr.HasParameter())
            {
                string path = null;
                TableSegment fromSegment = null;

                var rootTableSegment = this.TableAliases[parameterName];
                if (rootTableSegment.TableType == TableType.Entity)
                {
                    var builder = new StringBuilder(rootTableSegment.AliasName);
                    var memberExprs = this.GetMemberExprs(memberExpr);
                    if (memberExprs.Count > 1)
                    {
                        while (memberExprs.Count > 1)
                        {
                            var currentExpr = memberExprs.Pop();
                            builder.Append("." + currentExpr.Member.Name);
                        }
                        path = builder.ToString();
                        fromSegment = this.Tables.Find(f => f.TableType == TableType.Include && f.Path == path);
                    }
                    else fromSegment = rootTableSegment;
                }
                else fromSegment = rootTableSegment;

                if (memberExpr.Type.IsEntityType(out _))
                {
                    //TODO:匿名实体类型类似于Grouping对象，在子查询后续会支持
                    //if (this.IsFromQuery && this.IsSelectMember)
                    if (this.IsSelectMember)
                        throw new NotSupportedException("FROM子查询中不支持实体类型成员MemberAccess表达式访问，只支持基础字段访问");

                    //实体类型字段，三个场景：Json类型实体字段成员访问(包含实体表和子查询表)，Include导航实体类型成员访问(包括1:1,1:N关系)，
                    //Grouping分组对象的访问(包含当前查询中的和子查询表中的)                  
                    //子查询时，Mapper为null
                    if (fromSegment.Mapper != null)
                    {
                        //非子查询场景
                        var memberMapper = fromSegment.Mapper.GetMemberMap(memberExpr.Member.Name);
                        if (memberMapper.IsIgnore)
                            throw new NotSupportedException($"类{fromSegment.EntityType.FullName}的成员{memberExpr.Member.Name}是忽略成员无法访问");

                        if (memberMapper.IsNavigation)
                        {
                            //引用导航属性
                            if (this.IsWhere)
                                throw new NotSupportedException("不支持使用Include成员作为where条件，可以使用Join关联后再做where条件筛选");

                            path ??= fromSegment.Path.Replace(fromSegment.AliasName, parameterName);
                            path += "." + memberExpr.Member.Name;
                            var refReaderField = this.ReaderFields.Find(f => f.Path == path);
                            if (refReaderField == null)
                                throw new NotSupportedException("Select访问Include成员，要先Select访问Include成员的主表实体，如：.Select((x, y) =&gt; new { Order = x, x.Seller, x.Buyer, ... })");

                            //引用实体类型导航属性，当前导航属性可能还会有Include导航属性，所以构造时只给默认值
                            //在初始化完最外层实体后，再做赋值
                            if (!memberMapper.IsToOne) throw new NotSupportedException("暂时不支持引用1:N关系Include导航属性成员访问");
                            var readerField = this.ReaderFields.Find(f => f.Path == path);
                            //只有select场景才会Include对象
                            sqlSegment.HasField = true;
                            sqlSegment.FieldType = ReaderFieldType.IncludeRef;
                            sqlSegment.FromMember = memberMapper.Member;
                            sqlSegment.Value = refReaderField;
                            return sqlSegment;
                        }
                        else
                        {
                            //引用Json实体类型字段
                            if (memberMapper.TypeHandler == null)
                                throw new NotSupportedException($"类{fromSegment.EntityType.FullName}的成员{memberExpr.Member.Name}是实体类型，未配置导航属性也没有配置TypeHandler");

                            var fieldName = this.OrmProvider.GetFieldName(memberMapper.FieldName);
                            if (this.IsNeedTableAlias) fieldName = fromSegment.AliasName + "." + fieldName;
                            sqlSegment.FieldType = ReaderFieldType.Field;
                            sqlSegment.HasField = true;
                            sqlSegment.FromMember = memberMapper.Member;
                            sqlSegment.TargetMember = memberInfo;
                            sqlSegment.SegmentType = memberMapper.MemberType;
                            if (memberMapper.UnderlyingType.IsEnum)
                                sqlSegment.ExpectType = memberMapper.UnderlyingType;
                            sqlSegment.NativeDbType = memberMapper.NativeDbType;
                            sqlSegment.MappedTargetType = memberMapper.MappedTargetType;
                            sqlSegment.TypeHandler = memberMapper.TypeHandler;
                            sqlSegment.Body = fieldName;
                        }
                    }
                    else
                    {
                        //子查询和CTE子查询场景
                        //子查询和CTE子查询中，Select了Grouping分组对象或是临时匿名对象，目前子查询，只有分组对象才是实体类型，后续会支持匿名对象
                        //OrderBy中的实体类型对象访问已经单独处理了，包括Grouping对象
                        //fromSegment.TableType: TableType.FromQuery || TableType.CteSelfRef
                        var readerField = fromSegment.Fields.Find(f => f.TargetMember.Name == memberExpr.Member.Name);
                        sqlSegment.HasField = true;
                        sqlSegment.FieldType = readerField.FieldType;
                        sqlSegment.FromMember = readerField.TargetMember;
                        sqlSegment.TargetMember = readerField.TargetMember;
                        sqlSegment.SegmentType = readerField.SegmentType;
                        if (readerField.SegmentType.IsEnumType(out var underlyingType))
                            sqlSegment.ExpectType = underlyingType;
                        sqlSegment.NativeDbType = readerField.NativeDbType;
                        sqlSegment.MappedTargetType = readerField.MappedTargetType;
                        sqlSegment.TypeHandler = readerField.TypeHandler;
                        sqlSegment.Body = readerField.Body;
                        sqlSegment.Fields = readerField.Fields;
                    }
                }
                else
                {
                    //Where(f => f.Amount > 5)
                    //Select(f => new { f.OrderId, f.Disputes ...})                    
                    string fieldName = null;
                    sqlSegment.HasField = true;

                    if (fromSegment.Mapper != null)
                    {
                        var memberMapper = fromSegment.Mapper.GetMemberMap(memberExpr.Member.Name);
                        if (memberMapper.IsIgnore)
                            throw new Exception($"类{fromSegment.EntityType.FullName}的成员{memberMapper.MemberName}是忽略成员无法访问");
                        if (memberMapper.MemberType.IsEntityType(out _) && !memberMapper.IsNavigation && memberMapper.TypeHandler == null)
                            throw new Exception($"类{fromSegment.EntityType.FullName}的成员{memberExpr.Member.Name}不是值类型，未配置为导航属性也没有配置TypeHandler");

                        sqlSegment.FieldType = ReaderFieldType.Field;
                        sqlSegment.FromMember = memberMapper.Member;
                        sqlSegment.TargetMember = memberMapper.Member;
                        if (memberMapper.UnderlyingType.IsEnum)
                            sqlSegment.ExpectType = memberMapper.UnderlyingType;
                        sqlSegment.SegmentType = memberMapper.MemberType;
                        sqlSegment.NativeDbType = memberMapper.NativeDbType;
                        sqlSegment.MappedTargetType = memberMapper.MappedTargetType;
                        sqlSegment.TypeHandler = memberMapper.TypeHandler;

                        //查询时，IsNeedAlias始终为true，新增、更新、删除时，引用联表操作时，才会为true
                        fieldName = this.OrmProvider.GetFieldName(memberMapper.FieldName);
                        //IncludeMany表时，fromSegment.AliasName为null
                        if (this.IsNeedTableAlias && !string.IsNullOrEmpty(fromSegment.AliasName))
                            fieldName = fromSegment.AliasName + "." + fieldName;

                        //设置是否是分组字段，以便后面分表添加分组字段处理
                        if (this.IsSelect && this.GroupByFields != null && this.GroupByFields.Count > 0)
                            sqlSegment.IsGroupByField = this.GroupByFields.Exists(f => f.FromMember == memberMapper.Member);
                        sqlSegment.Body = fieldName;
                    }
                    else
                    {
                        //if (fromSegment.TableType == TableType.FromQuery || fromSegment.TableType == TableType.CteSelfRef)
                        //访问子查询表的成员，子查询表没有Mapper，也不会有实体类型成员
                        //Json的实体类型字段                       
                        //子查询，Select了Grouping分组对象或是匿名对象，目前子查询中，只支持一层，匿名对象后续会做支持
                        //取AS后的字段名，与原字段名不一定一样，AS后的字段名与memberExpr.Member.Name一致
                        SqlSegment readerField = null;
                        if (memberExpr.Expression.NodeType != ExpressionType.Parameter)
                        {
                            var parentMemberExpr = memberExpr.Expression as MemberExpression;
                            var parenetReaderField = fromSegment.Fields.Find(f => f.TargetMember.Name == parentMemberExpr.Member.Name);
                            readerField = parenetReaderField.Fields.Find(f => f.TargetMember.Name == memberExpr.Member.Name);
                        }
                        else
                        {
                            var fromReaderFields = fromSegment.Fields;
                            if (fromReaderFields.Count == 1 && fromReaderFields[0].FieldType != ReaderFieldType.Field)
                                fromReaderFields = fromReaderFields[0].Fields;
                            readerField = fromReaderFields.Find(f => f.TargetMember.Name == memberExpr.Member.Name);
                        }
                        sqlSegment.FieldType = readerField.FieldType;
                        sqlSegment.FromMember = readerField.FromMember;
                        sqlSegment.TargetMember = readerField.TargetMember;
                        sqlSegment.SegmentType = readerField.SegmentType;
                        if (readerField.SegmentType.IsEnumType(out var underlyingType))
                            sqlSegment.ExpectType = underlyingType;

                        sqlSegment.NativeDbType = readerField.NativeDbType;
                        sqlSegment.MappedTargetType = readerField.MappedTargetType;
                        sqlSegment.TypeHandler = readerField.TypeHandler;
                        if (fromSegment.TableType == TableType.SelectReaderFields)
                            fieldName = readerField.Body;
                        else
                        {
                            fieldName = this.OrmProvider.GetFieldName(memberExpr.Member.Name);
                            if (this.IsNeedTableAlias) fieldName = fromSegment.AliasName + "." + fieldName;
                        }
                        sqlSegment.Body = fieldName;
                        sqlSegment.Fields = readerField.Fields;
                    }
                }
                return sqlSegment;
            }
        }

        if (memberExpr.Member.DeclaringType == typeof(DBNull))
            return SqlSegment.Null;

        //各种静态成员访问，如：DateTime.Now,int.MaxValue,string.Empty
        if (this.OrmProvider.TryGetMemberAccessSqlFormatter(memberExpr, out formatter))
        {
            sqlSegment = formatter.Invoke(this, sqlSegment);
            sqlSegment.SegmentType = memberExpr.Type;
            sqlSegment.TargetMember = memberExpr.Member;
            sqlSegment.IsNeedAlias = true;
            return sqlSegment;
        }

        //访问局部变量或是成员变量，当作常量处理，直接计算，后面统一做参数化处理
        //var orderIds=new List<int>{1,2,3}; Where(f=>orderIds.Contains(f.OrderId)); orderIds
        //private Order order; Where(f=>f.OrderId==this.Order.Id); this.Order.Id
        //var orderId=10; Select(f=>new {OrderId=orderId,...}
        //Select(f=>new {OrderId=this.Order.Id, ...}
        this.Evaluate(sqlSegment);

        sqlSegment.IsConstant = false;
        sqlSegment.IsVariable = true;
        return sqlSegment;
    }
    public override TableSegment InitTableAlias(LambdaExpression lambdaExpr)
    {
        TableSegment tableSegment = null;
        this.TableAliases.Clear();
        lambdaExpr.Body.TryGetParameterNames(out var parameterNames);
        if (parameterNames == null || parameterNames.Count <= 0)
            return tableSegment;

        //为了实现Select之后，有的表达式计算、函数调用或是普通字段，都有可能改变了名字，为了之后select之后还可以OrderBy操作，
        //在解析字段的时候，如果ReaderFields有值说明已经select过了(Union除外)，就取ReaderFields中的字段，否则就取原表中的字段
        //有加新表操作或是Join操作就要清空ReaderFields，以免后续的解析字段时找不到字段
        if (this.ReaderFields != null && this.ReaderFields.Count > 0)
        {
            this.TableAliases.Add(parameterNames[0], tableSegment = new TableSegment
            {
                TableType = TableType.SelectReaderFields,
                Fields = this.ReaderFields,
            });
            return tableSegment;
        }
        var masterTables = this.Tables.FindAll(f => f.IsMaster);
        if (masterTables.Count > 0)
        {
            int index = 0;
            foreach (var parameterExpr in lambdaExpr.Parameters)
            {
                if (typeof(IAggregateSelect).IsAssignableFrom(parameterExpr.Type))
                    continue;
                if (parameterExpr.Type.FullName.StartsWith("Trolley.PostgreSql.IDistinctOnObject"))
                    continue;
                if (typeof(IFromQuery).IsAssignableFrom(parameterExpr.Type))
                    continue;
                if (!parameterNames.Contains(parameterExpr.Name))
                {
                    index++;
                    continue;
                }
                if (this.TableAliases.ContainsKey(parameterExpr.Name))
                    continue;
                this.TableAliases.Add(parameterExpr.Name, masterTables[index]);
                tableSegment = masterTables[index];
                index++;
            }
        }
        if (this.RefTableAliases != null && parameterNames.Count > this.TableAliases.Count)
        {
            foreach (var parameterName in parameterNames)
            {
                if (this.TableAliases.ContainsKey(parameterName))
                    continue;
                if (!this.RefTableAliases.ContainsKey(parameterName))
                    continue;
                this.TableAliases.Add(parameterName, this.RefTableAliases[parameterName]);
            }
        }
        return tableSegment;
    }
    public override SqlSegment VisitGroupConcatMethodCall(SqlSegment sqlSegment)
        => throw new NotSupportedException("不支持的方法调用，请考虑使用Sql.StringAgg方法");
    public override SqlSegment VisitStringAggMethodCall(SqlSegment sqlSegment)
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
        var builder = new StringBuilder("STRING_AGG(");
        bool hasOrder = false;
        SqlSegment fieldsSegment = null;
        while (callStack.TryPop(out methodCallExpr))
        {
            switch (methodCallExpr.Method.Name)
            {
                case "StringAgg":
                    var fieldsExpr = methodCallExpr.Arguments[0];
                    if (fieldsExpr.NodeType == ExpressionType.New || fieldsExpr.NodeType == ExpressionType.MemberInit)
                        throw new NotSupportedException("不支持的字段类型，Sql.StringAgg方法，只支持单个字段，不支持多个字段");
                    fieldsSegment = this.Visit(new SqlSegment { Expression = fieldsExpr });
                    this.AddVisitedFieldsSqlWithoutAlias(builder, fieldsSegment);
                    var separator = this.Evaluate<string>(methodCallExpr.Arguments[1]);
                    builder.Append($",{this.OrmProvider.GetQuotedValue(typeof(string), separator)}");
                    break;
                case "OrderBy":
                    fieldsSegment = this.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                    if (hasOrder) builder.Append(',');
                    else builder.Append(" ORDER BY ");
                    this.AddVisitedFieldsSqlWithoutAlias(builder, fieldsSegment);
                    hasOrder = true;
                    break;
                case "OrderByDescending":
                    fieldsSegment = this.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                    if (hasOrder) builder.Append(',');
                    else builder.Append(" ORDER BY ");
                    this.AddVisitedFieldsSqlWithoutAlias(builder, fieldsSegment, " DESC");
                    hasOrder = true;
                    break;
            }
        }
        builder.Append(')');
        var sql = builder.ToString();
        builder.Clear();
        return sqlSegment.Change(sql, false, true);
    }
    public override void Dispose()
    {
        if (this.isDisposed)
            return;
        this.isDisposed = true;

        this.DistinctOnFields = null;
        base.Dispose();
    }
    private bool IsDistinctOnMember(MemberExpression memberExpr)
    {
        if (memberExpr == null) return false;
        return memberExpr.Member.Name == "DistinctOn" && memberExpr.Member.DeclaringType.FullName.StartsWith("Trolley.PostgreSql.IDistinctOnObject");
    }
}