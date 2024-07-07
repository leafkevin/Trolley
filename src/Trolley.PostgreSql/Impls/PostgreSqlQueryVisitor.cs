using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Text;

namespace Trolley.PostgreSql;

public class PostgreSqlQueryVisitor : QueryVisitor
{
    public PostgreSqlQueryVisitor(string dbKey, IOrmProvider ormProvider, IEntityMapProvider mapProvider, IShardingProvider shardingProvider, bool isParameterized = false, char tableAsStart = 'a', string parameterPrefix = "p", IDataParameterCollection dbParameters = null)
        : base(dbKey, ormProvider, mapProvider, shardingProvider, isParameterized, tableAsStart, parameterPrefix, dbParameters) { }

    public bool IsDistinctOn { get; set; }
    public List<ReaderField> DistinctOnFields { get; set; }
    public string DistinctOnSql { get; set; }
    public override string BuildSql(out List<ReaderField> readerFields)
    {
        var builder = new StringBuilder();
        if (this.IsUseCteTable && this.RefQueries != null && this.RefQueries.Count > 0)
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
            builder = null;
            return sql;
        }
        var headSql = builder.ToString();
        builder.Clear();

        //先判断表是否有多分表isManySharding
        string tableSql = null;
        bool isManySharding = false;
        if (this.Tables.Count > 0)
        {
            //每个表都要有单独的GUID值，否则有类似的表前缀名，也会被替换导致表名替换错误
            int startIndex = 0;
            if (this.IsFromCommand) startIndex = 1;
            for (int i = startIndex; i < this.Tables.Count; i++)
            {
                var tableSegment = this.Tables[i];
                string tableName = this.GetTableName(tableSegment);
                if (i > startIndex)
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
                if (tableSegment.TableNames != null && tableSegment.TableNames.Count > 1)
                    isManySharding = true;
            }
            tableSql = builder.ToString();
        }
        builder.Clear();

        //再判断是否需要SELECT * FROM包装，UNION的子查询中有OrderBy或是Limit，就要包一下SELECT * FROM，否则数据结果不对
        bool isNeedWrap = (this.IsUnion || this.IsSecondUnion || isManySharding) && (!string.IsNullOrEmpty(this.OrderBySql) || this.limit.HasValue);

        //各种单值查询，如：SELECT COUNT(*)/MAX(*)..等，都有SELECT操作     
        //如：From(f=>...).InnerJoin/UnionAll(f=>...)

        //生成sql时，include表的字段，一定要紧跟着主表字段后面，方便赋值主表实体的属性中，所以在插入时候就排好序
        //方案：在buildSql时确定，ReaderFields要重新排好序，include字段放到对应主表字段后面，表别名顺序不变
        if (this.ReaderFields == null)
            throw new Exception("缺少Select语句");

        if (this.IsDistinctOn)
            builder.Append($"DISTINCT ON ({this.DistinctOnSql}) ");
        this.AddSelectFieldsSql(builder, this.ReaderFields, isNeedWrap);

        string selectSql = null;
        if (this.IsDistinct)
            selectSql = "DISTINCT " + builder.ToString();
        else selectSql = builder.ToString();

        builder.Clear();
        if (!string.IsNullOrEmpty(this.WhereSql))
        {
            this.WhereSql = $" WHERE {this.WhereSql}";
            builder.Append(this.WhereSql);
        }
        if (!string.IsNullOrEmpty(this.GroupBySql))
            builder.Append($" GROUP BY {this.GroupBySql}");
        if (!string.IsNullOrEmpty(this.HavingSql))
            builder.Append($" HAVING {this.HavingSql}");

        string orderBy = null;
        if (!string.IsNullOrEmpty(this.OrderBySql))
        {
            orderBy = $"ORDER BY {this.OrderBySql}";
            if (!this.skip.HasValue && !this.limit.HasValue)
                builder.Append(" " + orderBy);
        }
        string others = builder.ToString();

        builder.Clear();
        if (!string.IsNullOrEmpty(headSql))
            builder.Append(headSql);

        if (this.skip.HasValue || this.limit.HasValue)
        {
            //SQL TEMPLATE:SELECT /**fields**/ FROM /**tables**/ /**others**/
            var pageSql = this.OrmProvider.GetPagingTemplate(this.skip, this.limit, orderBy);
            pageSql = pageSql.Replace("/**fields**/", selectSql);
            pageSql = pageSql.Replace("/**tables**/", tableSql);
            pageSql = pageSql.Replace(" /**others**/", others);

            if (this.skip.HasValue && this.limit.HasValue)
                builder.Append($"SELECT COUNT(*) FROM {tableSql}{this.WhereSql};");
            builder.Append($"{pageSql}");
        }
        else builder.Append($"SELECT {selectSql} FROM {tableSql}{others}");

        //UNION的子查询中有OrderBy或是Limit，就要包一下SELECT * FROM，否则数据结果不对
        if (isNeedWrap)
        {
            builder.Insert(0, "SELECT * FROM (");
            builder.Append($") a");
        }
        sql = builder.ToString();
        builder.Clear();
        builder = null;
        return sql;
    }
    public override string BuildCommandSql(Type targetType, out IDataParameterCollection dbParameters)
    {
        var builder = new StringBuilder("INSERT INTO");
        var entityMapper = this.Tables[0].Mapper;
        builder.Append($" {this.GetTableName(this.Tables[0])} (");
        int index = 0;
        foreach (var readerField in this.ReaderFields)
        {
            //Union后，如果没有select语句时，通常实体类型或是select分组对象
            if (!entityMapper.TryGetMemberMap(readerField.TargetMember.Name, out var memberMapper)
                || memberMapper.IsIgnore || memberMapper.IsIgnoreInsert
                || memberMapper.IsNavigation || memberMapper.IsAutoIncrement || memberMapper.IsRowVersion
                || (memberMapper.MemberType.IsEntityType(out _) && memberMapper.TypeHandler == null))
                continue;
            if (index > 0) builder.Append(',');
            builder.Append($"{this.OrmProvider.GetFieldName(memberMapper.FieldName)}");
            index++;
        }
        builder.Append(") ");
        //有CTE表
        if (this.IsUseCteTable && this.RefQueries != null && this.RefQueries.Count > 0)
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
        dbParameters = this.DbParameters;
        string sql = null;
        if (!string.IsNullOrEmpty(this.UnionSql))
        {
            builder.Append(this.UnionSql);
            sql = builder.ToString();
            builder.Clear();
            builder = null;
            return sql;
        }
        var headSql = builder.ToString();
        builder.Clear();

        //先判断表是否有多分表isManySharding
        string tableSql = null;
        bool isManySharding = false;
        if (this.Tables.Count > 0)
        {
            int startIndex = 0;
            if (this.IsFromCommand) startIndex = 1;
            for (int i = startIndex; i < this.Tables.Count; i++)
            {
                var tableSegment = this.Tables[i];
                string tableName = this.GetTableName(tableSegment);
                if (i > startIndex)
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
                if (tableSegment.TableNames != null && tableSegment.TableNames.Count > 1)
                    isManySharding = true;
            }
            tableSql = builder.ToString();
        }
        builder.Clear();

        //再判断是否需要SELECT * FROM包装，UNION的子查询中有OrderBy或是Limit，就要包一下SELECT * FROM，否则数据结果不对
        bool isNeedWrap = (this.IsUnion || this.IsSecondUnion || isManySharding) && (!string.IsNullOrEmpty(this.OrderBySql) || this.limit.HasValue);

        //各种单值查询，如：SELECT COUNT(*)/MAX(*)..等，都有SELECT操作     
        //如：From(f=>...).InnerJoin/UnionAll(f=>...)
        //生成sql时，include表的字段，一定要紧跟着主表字段后面，方便赋值主表实体的属性中，所以在插入时候就排好序
        //方案：在buildSql时确定，ReaderFields要重新排好序，include字段放到对应主表字段后面，表别名顺序不变
        if (this.ReaderFields == null)
            throw new Exception("缺少Select语句");

        if (this.IsDistinctOn)
            builder.Append($"DISTINCT ON ({this.DistinctOnSql}) ");
        this.AddSelectFieldsSql(builder, this.ReaderFields, isNeedWrap);

        string selectSql = null;
        if (this.IsDistinct)
            selectSql = "DISTINCT " + builder.ToString();
        else selectSql = builder.ToString();

        builder.Clear();
        if (!string.IsNullOrEmpty(this.WhereSql))
        {
            this.WhereSql = $" WHERE {this.WhereSql}";
            builder.Append(this.WhereSql);
        }
        if (!string.IsNullOrEmpty(this.GroupBySql))
            builder.Append($" GROUP BY {this.GroupBySql}");
        if (!string.IsNullOrEmpty(this.HavingSql))
            builder.Append($" HAVING {this.HavingSql}");

        string orderBy = null;
        if (!string.IsNullOrEmpty(this.OrderBySql))
        {
            orderBy = $"ORDER BY {this.OrderBySql}";
            if (!this.skip.HasValue && !this.limit.HasValue)
                builder.Append(" " + orderBy);
        }
        string others = builder.ToString();

        builder.Clear();
        if (!string.IsNullOrEmpty(headSql))
            builder.Append(headSql);

        if (this.skip.HasValue || this.limit.HasValue)
        {
            //SQL TEMPLATE:SELECT /**fields**/ FROM /**tables**/ /**others**/
            var pageSql = this.OrmProvider.GetPagingTemplate(this.skip, this.limit, orderBy);
            pageSql = pageSql.Replace("/**fields**/", selectSql);
            pageSql = pageSql.Replace("/**tables**/", tableSql);
            pageSql = pageSql.Replace(" /**others**/", others);
            builder.Append($"{pageSql}");
        }
        else builder.Append($"SELECT {selectSql} FROM {tableSql}{others}");

        //UNION的子查询中有OrderBy或是Limit，就要包一下SELECT * FROM，否则数据结果不对
        if (isNeedWrap)
        {
            builder.Insert(0, "SELECT * FROM (");
            builder.Append($") a");
        }
        sql = builder.ToString();
        builder.Clear();
        builder = null;
        return sql;
    }
    public override string BuildShardingTablesSql(string tableSchema)
    {
        var count = this.ShardingTables.FindAll(f => f.ShardingType > ShardingTableType.MultiTable).Count;
        var builder = new StringBuilder($"SELECT a.relname FROM pg_class a,pg_namespace b WHERE a.relnamespace=b.oid AND b.nspname='{tableSchema}' AND a.relkind='r' AND ");
        if (count > 1)
        {
            builder.Append('(');
            int index = 0;
            foreach (var tableSegment in this.ShardingTables)
            {
                if (tableSegment.ShardingType > ShardingTableType.MultiTable)
                {
                    if (index > 0) builder.Append(" OR ");
                    builder.Append($"a.relname LIKE '{tableSegment.Mapper.TableName}%'");
                    index++;
                }
            }
            builder.Append(')');
        }
        else
        {
            if (this.ShardingTables.Count > 1)
            {
                var tableSegment = this.ShardingTables.Find(f => f.ShardingType > ShardingTableType.MultiTable);
                builder.Append($"a.relname LIKE '{tableSegment.Mapper.TableName}%'");
            }
            else builder.Append($"a.relname LIKE '{this.ShardingTables[0].Mapper.TableName}%'");
        }
        return builder.ToString();
    }
    public override void Distinct()
    {
        if (this.IsDistinctOn)
            throw new NotSupportedException("使用了DistinctOn方法，无需再使用Distinct方法来去重了");
        this.IsDistinct = true;
    }
    public virtual void DistinctOn(Expression fieldsSelector)
    {
        this.IsDistinctOn = true;
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

                    var fieldName = sqlSegment.Value.ToString();
                    builder.Append(fieldName);
                    bool isNeedAlias = false;
                    if (sqlSegment.IsConstant || sqlSegment.HasParameter || sqlSegment.IsExpression || sqlSegment.IsMethodCall
                         || sqlSegment.FromMember == null || sqlSegment.FromMember.Name != memberInfo.Name)
                        isNeedAlias = true;
                    this.GroupFields.Add(new ReaderField
                    {
                        FieldType = ReaderFieldType.Field,
                        FromMember = sqlSegment.FromMember,
                        TargetMember = memberInfo,
                        TargetType = memberInfo.GetMemberType(),
                        NativeDbType = sqlSegment.NativeDbType,
                        TypeHandler = sqlSegment.TypeHandler,
                        Body = fieldName,
                        IsNeedAlias = isNeedAlias
                    });
                    index++;
                }
                //DistinctOn都不需要别名，没有别名
                this.DistinctOnSql = builder.ToString();
                break;
            case ExpressionType.MemberAccess:
                {
                    var memberExpr = lambdaExpr.Body as MemberExpression;
                    var sqlSegment = this.VisitAndDeferred(new SqlSegment { Expression = memberExpr });
                    var fieldName = sqlSegment.Value.ToString();
                    //DistinctOn都不需要别名
                    this.GroupFields.Add(new ReaderField
                    {
                        FieldType = ReaderFieldType.Field,
                        FromMember = sqlSegment.FromMember,
                        TargetMember = memberExpr.Member,
                        TargetType = memberExpr.Member.GetMemberType(),
                        NativeDbType = sqlSegment.NativeDbType,
                        TypeHandler = sqlSegment.TypeHandler,
                        Body = fieldName
                    });
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

        //能够访问Grouping属性的场景，通常是在最外层的Select子句或是OrderBy子句
        //访问Grouping字段，并且Grouping对象是一个字段
        if (this.IsGroupingMember(lambdaExpr.Body as MemberExpression))
        {
            for (int i = 0; i < this.GroupFields.Count; i++)
            {
                if (i > 0) builder.Append(',');
                builder.Append(this.GroupFields[i].Body);
                if (orderType == "DESC")
                    builder.Append(" DESC");
            }
        }
        else if (this.IsDistinctOnMember(lambdaExpr.Body as MemberExpression))
        {
            for (int i = 0; i < this.DistinctOnFields.Count; i++)
            {
                if (i > 0) builder.Append(',');
                builder.Append(this.DistinctOnFields[i].Body);
                if (orderType == "DESC")
                    builder.Append(" DESC");
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
                            for (int i = 0; i < this.GroupFields.Count; i++)
                            {
                                if (i > 0) builder.Append(',');
                                builder.Append(this.GroupFields[i].Body);
                                if (orderType == "DESC")
                                    builder.Append(" DESC");
                            }
                        }
                        else if (this.IsDistinctOnMember(argumentExpr as MemberExpression))
                        {
                            for (int i = 0; i < this.DistinctOnFields.Count; i++)
                            {
                                if (i > 0) builder.Append(',');
                                builder.Append(this.DistinctOnFields[i].Body);
                                if (orderType == "DESC")
                                    builder.Append(" DESC");
                            }
                        }
                        else
                        {
                            var memberInfo = newExpr.Members[index];
                            var sqlSegment = this.VisitAndDeferred(new SqlSegment { Expression = argumentExpr });
                            if (builder.Length > 0)
                                builder.Append(',');

                            builder.Append(sqlSegment.Value.ToString());
                            if (orderType == "DESC")
                                builder.Append(" DESC");
                        }
                        index++;
                    }
                    break;
                case ExpressionType.MemberAccess:
                    {
                        var memberExpr = lambdaExpr.Body as MemberExpression;
                        if (this.IsGroupingMember(memberExpr))
                        {
                            for (int i = 0; i < this.GroupFields.Count; i++)
                            {
                                if (i > 0) builder.Append(',');
                                builder.Append(this.GroupFields[i].Body);
                                if (orderType == "DESC")
                                    builder.Append(" DESC");
                            }
                        }
                        else if (this.IsDistinctOnMember(memberExpr.Expression as MemberExpression))
                        {
                            var readerField = this.DistinctOnFields.Find(f => f.TargetMember.Name == memberExpr.Member.Name);
                            builder.Append(readerField.Body);
                            if (orderType == "DESC")
                                builder.Append(" DESC");
                        }
                        else
                        {
                            var sqlSegment = this.VisitAndDeferred(new SqlSegment { Expression = memberExpr });
                            builder.Append(sqlSegment.Value.ToString());
                            if (orderType == "DESC")
                                builder.Append(" DESC");
                        }
                    }
                    break;
            }
        }
        this.OrderBySql = builder.ToString();
    }
    public virtual void SelectDistinctOn() => this.ReaderFields = this.DistinctOnFields;
    public bool IsDistinctOnMember(MemberExpression memberExpr)
    {
        if (memberExpr == null) return false;
        //IPostgreSqlDistinctOn
        return memberExpr.Member.Name == "Grouping" && memberExpr.Member.DeclaringType.FullName.StartsWith("Trolley.PostgreSql.IPostgreSqlDistinctOn");
    }
    //public override SqlSegment VisitNew(SqlSegment sqlSegment)
    //{
    //    if (this.IsDistinct)
    //    {
    //        var builder = new StringBuilder();
    //        var newExpr = sqlSegment.Expression as NewExpression;
    //        for (int i = 0; i < newExpr.Arguments.Count; i++)
    //        {
    //            //成员访问(Json实体类型字段、普通字段场景)、常量、方法、表达式访问
    //            sqlSegment = this.VisitAndDeferred(new SqlSegment { Expression = newExpr.Arguments[i] });
    //            if (i > 0) builder.Append(',');
    //            builder.Append(this.GetQuotedValue(sqlSegment));
    //        }
    //        var fields = builder.ToString();
    //        builder.Clear();
    //        builder = null;
    //        return sqlSegment.Change(fields);
    //    }
    //    else return base.VisitNew(sqlSegment);
    //}
    //public override SqlSegment VisitMemberInit(SqlSegment sqlSegment)
    //{
    //    if (this.IsDistinct)
    //    {
    //        var builder = new StringBuilder();
    //        var memberInitExpr = sqlSegment.Expression as MemberInitExpression;
    //        for (int i = 0; i < memberInitExpr.Bindings.Count; i++)
    //        {
    //            if (memberInitExpr.Bindings[i].BindingType != MemberBindingType.Assignment)
    //                throw new NotSupportedException("暂时不支持除MemberBindingType.Assignment类型外的成员绑定表达式");
    //            var memberAssignment = memberInitExpr.Bindings[i] as MemberAssignment;
    //            //成员访问(Json实体类型字段、普通字段场景)、常量、方法、表达式访问
    //            sqlSegment = this.VisitAndDeferred(new SqlSegment { Expression = memberAssignment.Expression });
    //            if (i > 0) builder.Append(',');
    //            builder.Append(this.GetQuotedValue(sqlSegment));
    //        }
    //        var fields = builder.ToString();
    //        builder.Clear();
    //        builder = null;
    //        return sqlSegment.Change(fields);
    //    }
    //    else return base.VisitMemberInit(sqlSegment);
    //}
}
