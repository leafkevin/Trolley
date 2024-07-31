using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
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
    public override string BuildCommandSql(out IDataParameterCollection dbParameters)
    {
        var builder = new StringBuilder("INSERT INTO");
        var entityMapper = this.Tables[0].Mapper;
        builder.Append($" {this.GetTableName(this.Tables[0])} (");
        int index = 0;
        if (this.ReaderFields == null && this.IsFromQuery)
            this.ReaderFields = this.Tables[1].ReaderFields;
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
                    //这里只设置必须加as别名的情况，常量，参数，表达式，方法调用
                    //并且需要设置当前的成员名称，方便最外层做判断，是否添加as别名
                    if (sqlSegment.IsConstant || sqlSegment.HasParameter || sqlSegment.IsExpression || sqlSegment.IsMethodCall)
                        isNeedAlias = true;
                    this.DistinctOnFields.Add(new ReaderField
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
                    bool isNeedAlias = false;
                    //这里只设置必须加as别名的情况，常量，参数，表达式，方法调用
                    //并且需要设置当前的成员名称，方便最外层做判断，是否添加as别名
                    if (sqlSegment.IsConstant || sqlSegment.HasParameter || sqlSegment.IsExpression || sqlSegment.IsMethodCall)
                        isNeedAlias = true;
                    this.DistinctOnFields.Add(new ReaderField
                    {
                        FieldType = ReaderFieldType.Field,
                        FromMember = sqlSegment.FromMember,
                        TargetMember = memberExpr.Member,
                        TargetType = memberExpr.Member.GetMemberType(),
                        NativeDbType = sqlSegment.NativeDbType,
                        TypeHandler = sqlSegment.TypeHandler,
                        Body = fieldName,
                        IsNeedAlias = isNeedAlias
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
    public override SqlSegment VisitMemberAccess(SqlSegment sqlSegment)
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
                return sqlSegment;
            }

            //此场景一定是select
            if (this.IsGroupingMember(memberExpr))
            {
                List<ReaderField> groupFields = null;
                //在子查询中，Select了Group分组对象，为了避免在Clear时，把GroupFields元素清掉，放到一个新列表中
                if (this.IsFromQuery)
                {
                    groupFields = new();
                    this.GroupFields.ForEach(f => groupFields.Add(f));
                }
                else groupFields = this.GroupFields;
                if (groupFields.Count > 1)
                {
                    return sqlSegment.Change(new ReaderField
                    {
                        FieldType = ReaderFieldType.Entity,
                        FromMember = memberInfo,
                        TargetMember = memberInfo,
                        TargetType = memberInfo.GetMemberType(),
                        ReaderFields = groupFields
                    });
                }
                //分组对象为单个字段，要返回单个字段，防止后面Reader处理实体时候报错
                var readerField = groupFields[0];
                //要返回原始FromMember，后续方便判断是否使用AS别名
                sqlSegment.FromMember = readerField.FromMember;
                if (readerField.TargetType.IsEnumType(out var underlyingType))
                    sqlSegment.ExpectType = underlyingType;
                sqlSegment.SegmentType = underlyingType;
                sqlSegment.NativeDbType = readerField.NativeDbType;
                sqlSegment.TypeHandler = readerField.TypeHandler;
                sqlSegment.Value = readerField.Body;
                return sqlSegment;
            }
            else if (this.IsDistinctOnMember(memberExpr))
            {
                List<ReaderField> distinctOnFields = null;
                //在子查询中，Select了Group分组对象，为了避免在Clear时，把GroupFields元素清掉，放到一个新列表中
                if (this.IsFromQuery)
                {
                    distinctOnFields = new();
                    this.DistinctOnFields.ForEach(f => distinctOnFields.Add(f));
                }
                else distinctOnFields = this.DistinctOnFields;
                if (distinctOnFields.Count > 1)
                {
                    return sqlSegment.Change(new ReaderField
                    {
                        FieldType = ReaderFieldType.Entity,
                        FromMember = memberInfo,
                        TargetMember = memberInfo,
                        TargetType = memberInfo.GetMemberType(),
                        ReaderFields = distinctOnFields
                    });
                }
                //分组对象为单个字段，要返回单个字段，防止后面Reader处理实体时候报错
                var readerField = distinctOnFields[0];
                //要返回原始FromMember，后续方便判断是否使用AS别名
                sqlSegment.FromMember = readerField.FromMember;
                if (readerField.TargetType.IsEnumType(out var underlyingType))
                    sqlSegment.ExpectType = underlyingType;
                sqlSegment.SegmentType = underlyingType;
                sqlSegment.NativeDbType = readerField.NativeDbType;
                sqlSegment.TypeHandler = readerField.TypeHandler;
                sqlSegment.Value = readerField.Body;
                return sqlSegment;
            }
            else if (this.IsGroupingMember(memberExpr.Expression as MemberExpression))
            {
                //此时是Grouping对象字段的引用，最外面可能会更改成员名称，要复制一份，防止更改Grouping对象中的字段
                var readerField = this.GroupFields.Find(f => f.TargetMember.Name == memberInfo.Name);
                sqlSegment.FromMember = readerField.FromMember;
                if (readerField.TargetType.IsEnumType(out var underlyingType))
                    sqlSegment.ExpectType = underlyingType;
                sqlSegment.SegmentType = underlyingType;
                sqlSegment.NativeDbType = readerField.NativeDbType;
                sqlSegment.TypeHandler = readerField.TypeHandler;
                sqlSegment.Value = readerField.Body;
                //只有成员访问，只需要返回原始FromMember，是可以的。
                //如果是常量、参数、表达式、方法调用，需要强制设置IsNeedAlias，才能在最外层正确判断是否增加AS别名
                sqlSegment.IsNeedAlias = readerField.IsNeedAlias;
                return sqlSegment;
            }
            else if (this.IsDistinctOnMember(memberExpr.Expression as MemberExpression))
            {
                //此时是Grouping对象字段的引用，最外面可能会更改成员名称，要复制一份，防止更改Grouping对象中的字段
                var readerField = this.DistinctOnFields.Find(f => f.TargetMember.Name == memberInfo.Name);
                sqlSegment.FromMember = readerField.FromMember;
                if (readerField.TargetType.IsEnumType(out var underlyingType))
                    sqlSegment.ExpectType = underlyingType;
                sqlSegment.SegmentType = underlyingType;
                sqlSegment.NativeDbType = readerField.NativeDbType;
                sqlSegment.TypeHandler = readerField.TypeHandler;
                sqlSegment.Value = readerField.Body;
                //只有成员访问，只需要返回原始FromMember，是可以的。
                //如果是常量、参数、表达式、方法调用，需要强制设置IsNeedAlias，才能在最外层正确判断是否增加AS别名
                sqlSegment.IsNeedAlias = readerField.IsNeedAlias;
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
                            sqlSegment.Value = new ReaderField
                            {
                                //需要在构建实体的时候做处理
                                FieldType = ReaderFieldType.IncludeRef,
                                FromMember = memberMapper.Member,
                                TargetMember = memberInfo
                            };
                        }
                        else
                        {
                            //引用Json实体类型字段
                            if (memberMapper.TypeHandler == null)
                                throw new NotSupportedException($"类{fromSegment.EntityType.FullName}的成员{memberExpr.Member.Name}是实体类型，未配置导航属性也没有配置TypeHandler");

                            sqlSegment.HasField = true;
                            var fieldName = this.OrmProvider.GetFieldName(memberMapper.FieldName);
                            if (this.IsNeedTableAlias) fieldName = fromSegment.AliasName + "." + fieldName;
                            if (this.IsSelect)
                            {
                                sqlSegment.Value = new ReaderField
                                {
                                    FieldType = ReaderFieldType.Field,
                                    FromMember = memberMapper.Member,
                                    TargetMember = memberInfo,
                                    TargetType = memberMapper.MemberType,
                                    NativeDbType = memberMapper.NativeDbType,
                                    TypeHandler = memberMapper.TypeHandler,
                                    Body = fieldName
                                };
                            }
                            else
                            {
                                sqlSegment.FromMember = memberMapper.Member;
                                sqlSegment.MemberMapper = memberMapper;
                                sqlSegment.SegmentType = memberMapper.UnderlyingType;
                                if (memberMapper.UnderlyingType.IsEnum)
                                    sqlSegment.ExpectType = memberMapper.UnderlyingType;
                                sqlSegment.NativeDbType = memberMapper.NativeDbType;
                                sqlSegment.TypeHandler = memberMapper.TypeHandler;
                                sqlSegment.Value = fieldName;
                            }
                        }
                    }
                    else
                    {
                        sqlSegment.HasField = true;

                        //子查询和CTE子查询场景
                        //子查询和CTE子查询中，Select了Grouping分组对象或是临时匿名对象，目前子查询，只有分组对象才是实体类型，后续会支持匿名对象
                        //OrderBy中的实体类型对象访问已经单独处理了，包括Grouping对象
                        //fromSegment.TableType: TableType.FromQuery || TableType.CteSelfRef
                        var readerField = fromSegment.ReaderFields.Find(f => f.TargetMember.Name == memberExpr.Member.Name);
                        if (this.IsSelect) sqlSegment.Value = readerField;
                        //非Select场景，直接访问字段，只有是Json实体类型字段，FieldType: Field
                        else sqlSegment.Value = readerField.Body;
                        sqlSegment.FromMember = readerField.TargetMember;
                        if (readerField.TargetType.IsEnumType(out var underlyingType))
                            sqlSegment.ExpectType = underlyingType;
                        sqlSegment.SegmentType = underlyingType;
                        sqlSegment.NativeDbType = readerField.NativeDbType;
                        sqlSegment.TypeHandler = readerField.TypeHandler;
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

                        sqlSegment.FromMember = memberMapper.Member;
                        sqlSegment.MemberMapper = memberMapper;
                        if (memberMapper.UnderlyingType.IsEnum)
                            sqlSegment.ExpectType = memberMapper.UnderlyingType;
                        sqlSegment.SegmentType = memberMapper.UnderlyingType;
                        sqlSegment.NativeDbType = memberMapper.NativeDbType;
                        sqlSegment.TypeHandler = memberMapper.TypeHandler;
                        //查询时，IsNeedAlias始终为true，新增、更新、删除时，引用联表操作时，才会为true
                        fieldName = this.OrmProvider.GetFieldName(memberMapper.FieldName);
                        //IncludeMany表时，fromSegment.AliasName为null
                        if (this.IsNeedTableAlias && !string.IsNullOrEmpty(fromSegment.AliasName))
                            fieldName = fromSegment.AliasName + "." + fieldName;
                        sqlSegment.Value = fieldName;
                    }
                    else
                    {
                        //if (fromSegment.TableType == TableType.FromQuery || fromSegment.TableType == TableType.CteSelfRef)
                        //访问子查询表的成员，子查询表没有Mapper，也不会有实体类型成员
                        //Json的实体类型字段                       
                        //子查询，Select了Grouping分组对象或是匿名对象，目前子查询中，只支持一层，匿名对象后续会做支持
                        //取AS后的字段名，与原字段名不一定一样,AS后的字段名与memberExpr.Member.Name一致
                        ReaderField readerField = null;
                        if (memberExpr.Expression.NodeType != ExpressionType.Parameter)
                        {
                            var parentMemberExpr = memberExpr.Expression as MemberExpression;
                            var parenetReaderField = fromSegment.ReaderFields.Find(f => f.TargetMember.Name == parentMemberExpr.Member.Name);
                            readerField = parenetReaderField.ReaderFields.Find(f => f.TargetMember.Name == memberExpr.Member.Name);
                        }
                        else
                        {
                            var fromReaderFields = fromSegment.ReaderFields;
                            if (fromReaderFields.Count == 1 && fromReaderFields[0].FieldType != ReaderFieldType.Field)
                                fromReaderFields = fromReaderFields[0].ReaderFields;
                            readerField = fromReaderFields.Find(f => f.TargetMember.Name == memberExpr.Member.Name);
                        }
                        sqlSegment.FromMember = readerField.TargetMember;
                        if (readerField.TargetType.IsEnumType(out var underlyingType))
                            sqlSegment.ExpectType = underlyingType;
                        sqlSegment.SegmentType = underlyingType;
                        sqlSegment.NativeDbType = readerField.NativeDbType;
                        sqlSegment.TypeHandler = readerField.TypeHandler;
                        if (fromSegment.TableType == TableType.TempReaderFields)
                            fieldName = readerField.Body;
                        else
                        {
                            fieldName = this.OrmProvider.GetFieldName(memberExpr.Member.Name);
                            if (this.IsNeedTableAlias) fieldName = fromSegment.AliasName + "." + fieldName;
                        }
                        sqlSegment.Value = fieldName;
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
                ReaderFields = this.ReaderFields,
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
