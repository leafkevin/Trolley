using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Trolley.PostgreSql;

public class PostgreSqlQueryVisitor : QueryVisitor
{
    public PostgreSqlQueryVisitor(string dbKey, IOrmProvider ormProvider, IEntityMapProvider mapProvider, ITableShardingProvider shardingProvider, bool isParameterized = false, char tableAsStart = 'a', string parameterPrefix = "p", IDataParameterCollection dbParameters = null)
        : base(dbKey, ormProvider, mapProvider, shardingProvider, isParameterized, tableAsStart, parameterPrefix, dbParameters) { }

    public bool IsDistinctOn { get; set; }
    public List<SqlFieldSegment> DistinctOnFields { get; set; }
    public string DistinctOnSql { get; set; }
    public override string BuildSql(out List<SqlFieldSegment> readerFields)
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

        //各种单值查询，如：SELECT COUNT(*)/MAX(*)..等，都有SELECT操作     
        //如：From(f=>...).InnerJoin/UnionAll(f=>...)

        //生成sql时，include表的字段，一定要紧跟着主表字段后面，方便赋值主表实体的属性中，所以在插入时候就排好序
        //方案：在buildSql时确定，ReaderFields要重新排好序，include字段放到对应主表字段后面，表别名顺序不变
        if (this.ReaderFields == null)
            throw new Exception("缺少Select语句");

        if (this.IsDistinctOn)
            builder.Append($"DISTINCT ON ({this.DistinctOnSql}) ");
        this.AddSelectFieldsSql(builder, this.ReaderFields);

        string selectSql = null;
        if (this.IsDistinct)
            selectSql = "DISTINCT " + builder.ToString();
        else selectSql = builder.ToString();

        builder.Clear();
        string whereSql = null;
        if (!string.IsNullOrEmpty(this.WhereSql))
        {
            whereSql = $" WHERE {this.WhereSql}";
            builder.Append(whereSql);
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
                builder.Append($"SELECT COUNT(*) FROM {tableSql}{whereSql};");
            builder.Append($"{pageSql}");
        }
        else builder.Append($"SELECT {selectSql} FROM {tableSql}{others}");

        //判断是否需要SELECT * FROM包装，UNION的子查询中有OrderBy或是Limit，就要包一下SELECT * FROM，否则数据结果不正确
        if (isManySharding)
            isManySharding = this.ShardingTables != null && this.ShardingTables[0].TableNames != null && this.ShardingTables[0].TableNames.Count > 1;
        bool isNeedWrap = (this.IsUnion || this.IsSecondUnion || isManySharding) && (!string.IsNullOrEmpty(this.OrderBySql) || this.limit.HasValue);
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
    public override string BuildCommandSql(out IDataParameterCollection dbParameters)
    {
        var builder = new StringBuilder("INSERT INTO");
        var entityMapper = this.Tables[0].Mapper;
        builder.Append($" {this.GetTableName(this.Tables[0])} (");
        int index = 0;
        if (this.ReaderFields == null && this.IsFromQuery)
            this.ReaderFields = this.Tables[1].Fields;
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

        //各种单值查询，如：SELECT COUNT(*)/MAX(*)..等，都有SELECT操作     
        //如：From(f=>...).InnerJoin/UnionAll(f=>...)
        //生成sql时，include表的字段，一定要紧跟着主表字段后面，方便赋值主表实体的属性中，所以在插入时候就排好序
        //方案：在buildSql时确定，ReaderFields要重新排好序，include字段放到对应主表字段后面，表别名顺序不变
        if (this.ReaderFields == null)
            throw new Exception("缺少Select语句");

        if (this.IsDistinctOn)
            builder.Append($"DISTINCT ON ({this.DistinctOnSql}) ");
        this.AddSelectFieldsSql(builder, this.ReaderFields);

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

        //判断是否需要SELECT * FROM包装，UNION的子查询中有OrderBy或是Limit，就要包一下SELECT * FROM，否则数据结果不正确
        if (isManySharding)
            isManySharding = this.ShardingTables != null && this.ShardingTables[0].TableNames != null && this.ShardingTables[0].TableNames.Count > 1;
        bool isNeedWrap = (this.IsUnion || this.IsSecondUnion || isManySharding) && (!string.IsNullOrEmpty(this.OrderBySql) || this.limit.HasValue);
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
                    var sqlSegment = this.VisitAndDeferred(new SqlFieldSegment { Expression = argumentExpr });
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
                    var sqlSegment = this.VisitAndDeferred(new SqlFieldSegment { Expression = memberExpr });
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
                            var sqlSegment = this.VisitAndDeferred(new SqlFieldSegment { Expression = argumentExpr });
                            if (builder.Length > 0)
                                builder.Append(',');
                            builder.Append(sqlSegment.Body ?? sqlSegment.Value.ToString());
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
                            var sqlSegment = this.VisitAndDeferred(new SqlFieldSegment { Expression = memberExpr });
                            builder.Append(sqlSegment.Body ?? sqlSegment.Value.ToString());
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
    public override SqlFieldSegment VisitMemberAccess(SqlFieldSegment sqlSegment)
    {
        //Select场景，实体成员访问，返回ReaderField实体类型，ReaderFields并且有值，子ReaderFields的Body可无值
        //Select场景和Where场景，单个字段成员访(包括Json实体类型字段)，返回FromMember，TargetMember，字段类型，Body有值为带有别名的FieldName
        var memberExpr = sqlSegment.Expression as MemberExpression;
        var memberInfo = memberExpr.Member;

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
                    sqlSegment.Push(new DeferredExpr { OperationType = OperationType.Equal, Value = SqlFieldSegment.Null });
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
                return sqlSegment;
            }

            //此场景一定是select
            if (this.IsGroupingMember(memberExpr))
            {
                List<SqlFieldSegment> groupFields = new();
                //在子查询中，Select了Group分组对象，为了避免在Clear时，把GroupFields元素清掉，放到一个新列表中
                if (this.GroupFields.Count > 1)
                {
                    this.GroupFields.ForEach(f => groupFields.Add(f.Clone()));
                    sqlSegment.FieldType = SqlFieldType.Entity;
                    sqlSegment.HasField = true;
                    sqlSegment.FromMember = memberInfo;
                    sqlSegment.TargetMember = memberInfo;
                    sqlSegment.SegmentType = memberInfo.GetMemberType();
                    sqlSegment.Fields = groupFields;
                }
                else sqlSegment = this.GroupFields[0].Clone();
                return sqlSegment;
            }
            else if (this.IsDistinctOnMember(memberExpr))
            {
                List<SqlFieldSegment> distinctOnFields = new();
                //在子查询中，Select了Group分组对象，为了避免在Clear时，把GroupFields元素清掉，放到一个新列表中

                if (this.DistinctOnFields.Count > 1)
                {
                    this.DistinctOnFields.ForEach(f => distinctOnFields.Add(f));
                    sqlSegment.FieldType = SqlFieldType.Entity;
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
                var readerField = this.GroupFields.Find(f => f.TargetMember.Name == memberInfo.Name);
                sqlSegment = readerField.Clone();
                return sqlSegment;
            }
            else if (this.IsDistinctOnMember(memberExpr.Expression as MemberExpression))
            {
                //此时是Grouping对象字段的引用，最外面可能会更改成员名称，要复制一份，防止更改Grouping对象中的字段
                var readerField = this.DistinctOnFields.Find(f => f.TargetMember.Name == memberInfo.Name);
                sqlSegment = readerField.Clone();
                return sqlSegment;
            }
            if (memberExpr.IsParameter(out var parameterName))
            {
                string path = null;
                TableSegment fromSegment = null;

                var rootTableSegment = this.TableAliases[parameterName];
                if (rootTableSegment.TableType == TableType.Entity)
                {
                    var builder = new StringBuilder(rootTableSegment.AliasName);
                    var memberExprs = this.GetMemberExprs(memberExpr, out var parameterExpr);
                    if (memberExprs.Count > 1)
                    {
                        while (memberExprs.TryPop(out var currentExpr))
                        {
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
                    if (this.IsFromQuery && this.IsSelectMember)
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
                            if (!this.IsSelect)
                                throw new NotSupportedException("暂时不支持Select场景以外的访问Include成员场景");

                            path += "." + memberExpr.Member.Name;
                            var refReaderField = this.ReaderFields.Find(f => f.Path == path);
                            if (refReaderField == null)
                                throw new NotSupportedException("Select访问Include成员，要先Select访问Include成员的主表实体，如：.Select((x, y) =&gt; new { Order = x, x.Seller, x.Buyer, ... })");

                            //引用实体类型导航属性，当前导航属性可能还会有Include导航属性，所以构造时只给默认值
                            //在初始化完最外层实体后，再做赋值，但要先确定返回目标的当前成员是否支持Set，不支持Set无法完成
                            if (memberInfo is PropertyInfo propertyInfo && propertyInfo.GetSetMethod() == null)
                                throw new NotSupportedException($"类型{propertyInfo.DeclaringType.FullName}的成员{propertyInfo.Name}不支持Set操作");

                            var includeSegment = this.IncludeTables.Find(f => f.Path == path);
                            var rootReaderField = this.ReaderFields.Find(f => f.Path == parameterName);
                            var refRootPath = rootReaderField.TargetMember.Name;
                            var refPath = refRootPath + path.Substring(path.IndexOf("."));
                            var cacheKey = GetRefIncludeKey(memberInfo.DeclaringType, refPath);
                            var deferredSetter = targetRefIncludeValuesSetters.GetOrAdd(cacheKey, f =>
                            {
                                var targetExpr = Expression.Parameter(typeof(object), "target");
                                var typedTargetExpr = Expression.Convert(targetExpr, memberInfo.DeclaringType);
                                var targetMemberExpr = Expression.PropertyOrField(typedTargetExpr, memberInfo.Name);

                                Expression refValueExpr = typedTargetExpr;
                                refValueExpr = Expression.PropertyOrField(refValueExpr, refRootPath);
                                foreach (var memberInfo in includeSegment.ParentMemberVisits)
                                {
                                    refValueExpr = Expression.PropertyOrField(refValueExpr, memberInfo.Name);
                                }
                                refValueExpr = Expression.PropertyOrField(refValueExpr, includeSegment.FromMember.MemberName);
                                Expression bodyExpr = null;
                                if (memberInfo.MemberType == MemberTypes.Property)
                                    bodyExpr = Expression.Call(targetMemberExpr, (memberInfo as PropertyInfo).GetSetMethod(), refValueExpr);
                                else if (memberInfo.MemberType == MemberTypes.Field)
                                    bodyExpr = Expression.Assign(targetMemberExpr, refValueExpr);
                                return Expression.Lambda<Action<object>>(bodyExpr, targetExpr).Compile();
                            });
                            this.deferredRefIncludeValuesSetters ??= new();
                            this.deferredRefIncludeValuesSetters.Add(deferredSetter);

                            //只有select场景才会Include对象
                            sqlSegment.HasField = true;
                            sqlSegment.FieldType = SqlFieldType.IncludeRef;
                            sqlSegment.FromMember = memberMapper.Member;
                            sqlSegment.TargetMember = memberInfo;
                            return sqlSegment;
                        }
                        else
                        {
                            //引用Json实体类型字段
                            if (memberMapper.TypeHandler == null)
                                throw new NotSupportedException($"类{fromSegment.EntityType.FullName}的成员{memberExpr.Member.Name}是实体类型，未配置导航属性也没有配置TypeHandler");

                            sqlSegment.HasField = true;
                            var fieldName = this.OrmProvider.GetFieldName(memberMapper.FieldName);
                            if (this.IsNeedTableAlias) fieldName = fromSegment.AliasName + "." + fieldName;
                            sqlSegment.FieldType = SqlFieldType.Field;
                            sqlSegment.HasField = true;
                            sqlSegment.FromMember = memberMapper.Member;
                            sqlSegment.TargetMember = memberInfo;
                            sqlSegment.SegmentType = memberMapper.MemberType;
                            if (memberMapper.UnderlyingType.IsEnum)
                                sqlSegment.ExpectType = memberMapper.UnderlyingType;
                            sqlSegment.NativeDbType = memberMapper.NativeDbType;
                            sqlSegment.TypeHandler = memberMapper.TypeHandler;
                            sqlSegment.Body = fieldName;
                        }
                    }
                    else
                    {
                        sqlSegment.HasField = true;
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

                        sqlSegment.FieldType = SqlFieldType.Field;
                        sqlSegment.FromMember = memberMapper.Member;
                        sqlSegment.TargetMember = memberMapper.Member;
                        if (memberMapper.UnderlyingType.IsEnum)
                            sqlSegment.ExpectType = memberMapper.UnderlyingType;
                        sqlSegment.SegmentType = memberMapper.MemberType;
                        sqlSegment.NativeDbType = memberMapper.NativeDbType;
                        sqlSegment.TypeHandler = memberMapper.TypeHandler;

                        //查询时，IsNeedAlias始终为true，新增、更新、删除时，引用联表操作时，才会为true
                        fieldName = this.OrmProvider.GetFieldName(memberMapper.FieldName);
                        //IncludeMany表时，fromSegment.AliasName为null
                        if (this.IsNeedTableAlias && !string.IsNullOrEmpty(fromSegment.AliasName))
                            fieldName = fromSegment.AliasName + "." + fieldName;
                        sqlSegment.Body = fieldName;
                    }
                    else
                    {
                        //if (fromSegment.TableType == TableType.FromQuery || fromSegment.TableType == TableType.CteSelfRef)
                        //访问子查询表的成员，子查询表没有Mapper，也不会有实体类型成员
                        //Json的实体类型字段                       
                        //子查询，Select了Grouping分组对象或是匿名对象，目前子查询中，只支持一层，匿名对象后续会做支持
                        //取AS后的字段名，与原字段名不一定一样,AS后的字段名与memberExpr.Member.Name一致
                        SqlFieldSegment readerField = null;
                        if (memberExpr.Expression.NodeType != ExpressionType.Parameter)
                        {
                            var parentMemberExpr = memberExpr.Expression as MemberExpression;
                            var parenetReaderField = fromSegment.Fields.Find(f => f.TargetMember.Name == parentMemberExpr.Member.Name);
                            readerField = parenetReaderField.Fields.Find(f => f.TargetMember.Name == memberExpr.Member.Name);
                        }
                        else
                        {
                            var fromReaderFields = fromSegment.Fields;
                            if (fromReaderFields.Count == 1 && fromReaderFields[0].FieldType != SqlFieldType.Field)
                                fromReaderFields = fromReaderFields[0].Fields;
                            readerField = fromReaderFields.Find(f => f.TargetMember.Name == memberExpr.Member.Name);
                        }
                        sqlSegment.FieldType = readerField.FieldType;
                        sqlSegment.FromMember = readerField.FromMember;
                        sqlSegment.TargetMember = readerField.TargetMember;
                        if (readerField.SegmentType.IsEnumType(out var underlyingType))
                            sqlSegment.ExpectType = underlyingType;
                        sqlSegment.SegmentType = readerField.SegmentType;
                        sqlSegment.NativeDbType = readerField.NativeDbType;
                        sqlSegment.TypeHandler = readerField.TypeHandler;
                        if (fromSegment.TableType == TableType.TempReaderFields)
                            fieldName = readerField.Body;
                        else
                        {
                            fieldName = this.OrmProvider.GetFieldName(memberExpr.Member.Name);
                            if (this.IsNeedTableAlias) fieldName = fromSegment.AliasName + "." + fieldName;
                        }
                        sqlSegment.Body = fieldName;
                    }
                }
                return sqlSegment;
            }
        }

        if (memberExpr.Member.DeclaringType == typeof(DBNull))
            return SqlFieldSegment.Null;

        //各种静态成员访问，如：DateTime.Now,int.MaxValue,string.Empty
        if (this.OrmProvider.TryGetMemberAccessSqlFormatter(memberExpr, out formatter))
        {
            sqlSegment = formatter.Invoke(this, sqlSegment);
            sqlSegment.SegmentType = memberExpr.Type;
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
        lambdaExpr.Body.GetParameterNames(out var parameterNames);
        if (parameterNames == null || parameterNames.Count <= 0)
            return tableSegment;

        //为了实现Select之后，有的表达式计算、函数调用或是普通字段，都有可能改变了名字，为了之后select之后还可以OrderBy操作，
        //在解析字段的时候，如果ReaderFields有值说明已经select过了(Union除外)，就取ReaderFields中的字段，否则就取原表中的字段
        //有加新表操作或是Join操作就要清空ReaderFields，以免后续的解析字段时找不到字段
        if (this.ReaderFields != null && this.ReaderFields.Count > 0)
        {
            this.TableAliases.Add(parameterNames[0], tableSegment = new TableSegment
            {
                TableType = TableType.TempReaderFields,
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
                this.TableAliases.Add(parameterName, this.RefTableAliases[parameterName]);
            }
        }
        return tableSegment;
    }
    private bool IsDistinctOnMember(MemberExpression memberExpr)
    {
        if (memberExpr == null) return false;
        return memberExpr.Member.Name == "DistinctOn" && memberExpr.Member.DeclaringType.FullName.StartsWith("Trolley.PostgreSql.IDistinctOnObject");
    }
}
