using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley;

public class QueryVisitor : SqlVisitor, IQueryVisitor
{
    protected static ConcurrentDictionary<int, (string, Action<StringBuilder, IOrmProvider, object>)> includeSqlGetterCache = new();
    protected static ConcurrentDictionary<Type, Func<object>> typedListGetters = new();
    protected static ConcurrentDictionary<Type, Action<object, ITheaDataReader, IOrmProvider, IEntityMapProvider>> typedReaderElementSetters = new();
    protected static ConcurrentDictionary<int, Action<object, object>> targetIncludeValuesSetters = new();
    private bool isDisposed;

    protected List<CommandSegment> deferredSegments = new();
    protected int? skip;
    protected int? limit;

    protected string UnionSql { get; set; }
    protected string GroupBySql { get; set; }
    protected string HavingSql { get; set; }
    protected string OrderBySql { get; set; }
    protected bool IsDistinct { get; set; }
    protected bool IsSelectMember { get; set; }
    public bool IsFromCommand { get; set; }
    public bool IsCteTable { get; set; }
    protected bool IsUnion { get; set; }
    /// <summary>
    /// 第二个Union子句
    /// </summary>
    public bool IsSecondUnion { get; set; } = false;

    protected TableSegment LastIncludeSegment { get; set; }

    public bool IsRecursive { get; set; }
    public bool IsUseCteTable { get; set; } = true;
    public ICteQuery SelfRefQueryObj { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }

    public QueryVisitor(DbContext dbContext, char tableAsStart = 'a', IDataParameterCollection dbParameters = null)
    {
        this.DbContext = dbContext;
        this.TableAsStart = tableAsStart;
        this.DbParameters = dbParameters ?? new TheaDbParameterCollection();
        this.IsNeedTableAlias = true;
    }
    public virtual string BuildSql(out List<SqlFieldSegment> readerFields)
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
            for (int i = 0; i < this.Tables.Count; i++)
            {
                var tableSegment = this.Tables[i];
                string tableName = this.GetTableName(tableSegment);
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
        return sql;
    }
    public virtual string BuildCommandSql(out IDataParameterCollection dbParameters)
    {
        var builder = new StringBuilder();
        var entityMapper = this.Tables[0].Mapper;
        builder.Append($"INSERT INTO {this.GetTableName(this.Tables[0])} (");
        int index = 0;
        if (this.ReaderFields == null && this.IsFromQuery)
            this.ReaderFields = this.Tables[1].Fields;
        foreach (var readerField in this.ReaderFields)
        {
            //Union后，如果没有select语句时，通常实体类型或是select分组对象
            var memberName = readerField.TargetMember.Name;
            if (!entityMapper.TryGetMemberMap(memberName, out var memberMapper)
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
    public virtual string BuildCteTableSql(string tableName, out List<SqlFieldSegment> readerFields, out bool isRecursive)
    {
        this.IsUseCteTable = false;
        isRecursive = this.IsRecursive;
        if (this.SelfRefQueryObj != null)
        {
            var tempTableName = this.SelfRefQueryObj.TableName;
            this.UnionSql = this.UnionSql.Replace(tempTableName, tableName);
            this.SelfRefQueryObj.TableName = tableName;
        }
        tableName = this.OrmProvider.GetTableName(tableName);
        this.IsCteTable = true;
        var rawSql = this.BuildSql(out readerFields);
        this.IsCteTable = false;
        var builder = new StringBuilder($"{tableName}(");
        int index = 0;
        foreach (var readerField in readerFields)
        {
            if (readerField.FieldType == SqlFieldType.Field)
            {
                if (index > 0) builder.Append(',');
                builder.Append(this.OrmProvider.GetFieldName(readerField.TargetMember.Name));
                index++;
            }
            else
            {
                foreach (var childReaderField in readerField.Fields)
                {
                    if (index > 0) builder.Append(',');
                    builder.Append(this.OrmProvider.GetFieldName(childReaderField.TargetMember.Name));
                    index++;
                }
            }
            //在引用CTE表时，会更新FromMember，此处无需更新
            //readerField.FromMember = readerField.TargetMember; 
        }
        builder.AppendLine(") AS ");
        builder.AppendLine("(");
        builder.AppendLine(rawSql);
        builder.Append(')');
        this.IsUseCteTable = true;
        this.SelfRefQueryObj = null;
        var sql = builder.ToString();
        builder.Clear();
        builder = null;
        return sql;
    }
    public virtual void From(char tableAsStart = 'a', params Type[] entityTypes)
    {
        this.TableAsStart = tableAsStart;
        foreach (var entityType in entityTypes)
        {
            int tableIndex = tableAsStart + this.Tables.Count;
            this.AddTable(new TableSegment
            {
                EntityType = entityType,
                Mapper = this.MapProvider.GetEntityMap(entityType),
                AliasName = $"{(char)tableIndex}",
                Path = $"{(char)tableIndex}",
                TableType = TableType.Entity,
                IsMaster = true
            });
        }
    }
    public virtual void From(Type targetType, IQuery subQueryObj)
    {
        this.AddSubQueryTable(targetType, subQueryObj);
        if (this.ReaderFields != null)
            this.ReaderFields = null;
    }
    public virtual void From(Type targetType, DbContext dbContext, Delegate subQueryGetter)
    {
        //可能是CTE表，也可能是子查询
        var queryVisitor = this.CreateQueryVisitor();
        this.IsFromQuery = true;
        var fromQuery = new FromQuery(dbContext, queryVisitor);
        var subQueryObj = subQueryGetter.DynamicInvoke(fromQuery) as IQuery;
        if (queryVisitor.IsRecursive && queryVisitor.SelfRefQueryObj != null)
            throw new Exception("调用UnionRecursive/UnionAllRecursive方法后，必须调用AsCteTable方法，生成CTE表");
        this.From(targetType, subQueryObj);
        this.IsFromQuery = false;
    }
    public virtual void Union(string union, Type targetType, IQuery subQuery)
    {
        //解析第一个UNION子句，需要AS别名
        this.IsUnion = true;
        var rawSql = this.BuildSql(out var readerFields);
        //解析第二个UNION子句，不需要AS别名，如果有CTE表，也不生成CTE表SQL，只是引用CTE表名
        subQuery.Visitor.IsSecondUnion = true;
        subQuery.Visitor.IsUseCteTable = false;

        if (subQuery is ICteQuery cteQuery)
        {
            if (!this.RefQueries.Contains(cteQuery))
                this.RefQueries.Add(cteQuery);
            rawSql += union + Environment.NewLine + $"SELECT * FROM {this.OrmProvider.GetTableName(cteQuery.TableName)}";
        }
        else
        {
            this.Clear();
            rawSql += union + Environment.NewLine + subQuery.Visitor.BuildSql(out _);
        }

        var tableSegment = this.AddTable(targetType, null, TableType.FromQuery, $"({rawSql})", readerFields);
        this.InitFromQueryReaderFields(tableSegment, readerFields);
        subQuery.CopyTo(this);
        this.UnionSql = rawSql;
        this.IsUnion = false;
    }
    public virtual void Union(string union, Type targetType, DbContext dbContext, Delegate subQueryGetter)
    {
        //解析第一个UNION子句，需要AS别名
        this.IsUnion = true;
        this.IsUseCteTable = false;
        var rawSql = this.BuildSql(out var readerFields);
        this.Clear();
        //解析第二个UNION子句，不需要AS别名，如果有CTE表，也不生成CTE表SQL，只是引用CTE表名
        this.IsSecondUnion = true;
        this.IsUseCteTable = false;
        this.IsFromQuery = true;

        //把第二个Union子句当作独立的SQL解析，先设置IsUnion = false，以免影响UNION的正常解析
        this.IsUnion = false;
        var fromQuery = new FromQuery(dbContext, this);
        var subQuery = subQueryGetter.DynamicInvoke(fromQuery);
        this.IsUnion = true;

        if (subQuery is ICteQuery cteQuery)
        {
            if (!this.RefQueries.Contains(cteQuery))
                this.RefQueries.Add(cteQuery);
            rawSql += union + Environment.NewLine + $"SELECT * FROM {this.OrmProvider.GetTableName(cteQuery.TableName)}";
        }
        else
        {
            this.IsUseCteTable = false;
            rawSql += union + Environment.NewLine + this.BuildSql(out _);
        }
        this.IsFromQuery = false;
        this.Clear();
        var tableSegment = this.AddTable(targetType, null, TableType.FromQuery, $"({rawSql})", readerFields);
        this.InitFromQueryReaderFields(tableSegment, readerFields);
        //使用第一个Union的ReaderFields，第二个Union有可能是*
        this.ReaderFields = readerFields;
        this.UnionSql = rawSql;
        this.IsUnion = false;
    }
    public virtual void UnionRecursive(string union, DbContext dbContext, ICteQuery selfQueryObj, Delegate subQueryGetter)
    {
        this.IsUnion = true;
        this.IsRecursive = true;
        var rawSql = this.BuildSql(out var readerFields);
        this.Clear();
        this.IsUseCteTable = false;

        //此时产生的queryObj是一个新的对象，只能用于解析sql，与传进来的queryObj不是同一个对象，舍弃
        //临时产生一个随机表名，在后面的AsCteTable时，再做替换
        var tempTableName = $"__CTE_TABLE_{Guid.NewGuid():N}__";
        selfQueryObj.TableName = tempTableName;
        selfQueryObj.ReaderFields = readerFields;
        selfQueryObj.IsRecursive = true;
        this.SelfRefQueryObj = selfQueryObj;

        //把第二个Union子句当作独立的SQL解析，先设置IsUnion = false，以免影响UNION的正常解析
        this.IsUnion = false;
        var fromQuery = new FromQuery(dbContext, this);
        var subQuery = subQueryGetter.DynamicInvoke(fromQuery, selfQueryObj) as IQuery;
        this.IsUnion = true;

        rawSql += union + Environment.NewLine + subQuery.Visitor.BuildSql(out _);
        //sql解析完毕，不再需要selfQueryObj对象了，在CteQueries中删除selfQueryObj对象，防止最后扁平化CteQueries时无限循环引用
        this.RefQueries.Remove(selfQueryObj);
        //先放到UnionSql中，在AsCteTable方法中，BuildCteTableSql时能得到这个SQL
        this.UnionSql = rawSql;
        this.IsUnion = false;
    }
    public virtual void Join(string joinType, Expression joinOn)
    {
        this.Join(joinOn, f =>
        {
            var tableSegment = this.InitTableAlias(f);
            tableSegment.JoinType = joinType;
            return tableSegment;
        });
    }
    public virtual void Join(string joinType, Type newEntityType, Expression joinOn)
    {
        this.Join(joinOn, f =>
        {
            this.From(this.TableAsStart, newEntityType);
            var tableSegment = this.InitTableAlias(f);
            tableSegment.JoinType = joinType;
            return tableSegment;
        });
    }
    public virtual void Join(string joinType, Type newEntityType, IQuery subQuery, Expression joinOn)
    {
        this.Join(joinOn, f =>
        {
            var tableSegment = this.AddSubQueryTable(newEntityType, subQuery, joinType);
            this.InitTableAlias(f);
            return tableSegment;
        });
    }
    public virtual void Join(string joinType, Type newEntityType, DbContext dbContext, Delegate subQueryGetter, Expression joinOn)
    {
        this.Join(joinOn, f =>
        {
            var visitor = this.CreateQueryVisitor();
            var fromQuery = new FromQuery(dbContext, visitor);
            var subQuery = subQueryGetter.DynamicInvoke(fromQuery) as IQuery;
            var tableSegment = this.AddSubQueryTable(newEntityType, subQuery, joinType);
            this.InitTableAlias(f);
            return tableSegment;
        });
    }
    public virtual void Join(Expression joinOn, Func<LambdaExpression, TableSegment> joinTableSegmentGetter)
    {
        this.IsWhere = true;
        var lambdaExpr = joinOn as LambdaExpression;
        if (!lambdaExpr.Body.GetParameters(out var parameters))
            throw new NotSupportedException("当前Join操作，没有表关联");
        if (parameters.Count != 2)
            throw new NotSupportedException("Join操作，只支持两个表进行关联，但可以多次Join操作");

        var joinTableSegment = joinTableSegmentGetter(lambdaExpr);
        joinTableSegment.OnExpr = this.VisitConditionExpr(lambdaExpr.Body, out _);
        this.IsWhere = false;
    }
    public virtual bool Include(Expression memberSelector, Expression filter = null)
        => this.Include(memberSelector, (a, b) => this.InitTableAlias(a), filter);
    public virtual bool ThenInclude(Expression memberSelector, Expression filter = null)
        => this.Include(memberSelector, (a, b) =>
    {
        this.TableAliases.Clear();
        this.TableAliases.Add(b[0].Name, this.LastIncludeSegment);
    }, filter);
    public virtual bool HasIncludeTables() => this.IncludeTables != null && this.IncludeTables.Count > 0;
    public virtual bool BuildIncludeSql(Type targetType, object target, bool isSingle, out string sql)
    {
        sql = null;
        if (target == null) return false;
        ICollection targets = null;
        if (!isSingle)
        {
            targets = target as ICollection;
            if (targets.Count == 0)
                return false;
        }
        if (this.IncludeTables == null) return false;

        Action<StringBuilder, Action<StringBuilder, IOrmProvider, object>> sqlBuilderInitializer = null;
        if (isSingle)
        {
            sqlBuilderInitializer = (builder, foreignKeysSetter) =>
            {
                foreignKeysSetter.Invoke(builder, this.OrmProvider, target);
                builder.Append(')');
            };
        }
        else
        {
            sqlBuilderInitializer = (builder, foreignKeysSetter) =>
            {
                int index = 0;
                foreach (var target in targets)
                {
                    if (index > 0) builder.Append(',');
                    foreignKeysSetter.Invoke(builder, this.OrmProvider, target);
                    index++;
                }
                builder.Append(')');
            };
        }
        var builder = new StringBuilder();
        for (int i = 0; i < this.IncludeTables.Count; i++)
        {
            if (i > 0 && builder.Length > 0) builder.Append(';');
            var includeTableSegment = this.IncludeTables[i];
            var rootPath = includeTableSegment.Path.Substring(0, 1);
            var rootReaderField = this.ReaderFields.Find(f => f.Path == rootPath);
            if (rootReaderField == null)
                continue;
            //throw new NotSupportedException("Include导航属性成员，一定要Select对应的实体表，如：\r\nrepository.From<Order>()\r\n    .InnerJoin<User>((x, y) => x.SellerId == y.Id)\r\n    .Include((x, y) => x.Buyer)\r\n    .Include((x, y) => y.Company)\r\n    .Select((x, y) => new { Order = x, Seller = y, ... })");
            var firstMember = rootReaderField.TargetMember;

            (var headSql, var sqlInitializer) = this.BuildIncludeSqlGetter(targetType, firstMember, includeTableSegment);
            headSql = string.Format(headSql, this.GetTableName(includeTableSegment));

            if (includeTableSegment.IsSharding && includeTableSegment.TableNames.Count > 0)
            {
                var sqlBuilder = new StringBuilder();
                sqlBuilderInitializer.Invoke(sqlBuilder, sqlInitializer);
                if (!string.IsNullOrEmpty(includeTableSegment.Filter))
                    sqlBuilder.Append($" AND {includeTableSegment.Filter}");
                var afterSql = sqlBuilder.ToString();
                var origName = includeTableSegment.Mapper.TableName;
                var formatName = $"__SHARDING_{includeTableSegment.ShardingId}_{origName}";
                int index = 0;
                foreach (var tableName in includeTableSegment.TableNames)
                {
                    if (index > 0) builder.Append(" UNION ALL ");
                    builder.Append(headSql.Replace(formatName, tableName));
                    builder.Append(afterSql);
                    index++;
                }
            }
            else
            {
                builder.Append(headSql);
                sqlBuilderInitializer.Invoke(builder, sqlInitializer);
                if (!string.IsNullOrEmpty(includeTableSegment.Filter))
                    builder.Append($" AND {includeTableSegment.Filter}");
            }
        }
        if (builder.Length > 0)
        {
            sql = builder.ToString();
            return true;
        }
        return false;
    }
    public virtual void SetIncludeValues(Type targetType, object target, ITheaDataReader reader, bool isSingle)
    {
        var deferredInitializers = new List<(object, Action<object>)>();
        foreach (var includeSegment in this.IncludeTables)
        {
            var navigationType = includeSegment.FromMember.NavigationType;
            var includeValues = this.CreateIncludeValues(navigationType);
            var rootPath = includeSegment.Path.Substring(0, 1);
            var rootReaderField = this.ReaderFields.Find(f => f.Path == rootPath);
            //当最外层实体是参数访问时，此值为null
            var firstMember = rootReaderField.TargetMember;

            while (reader.Read())
                this.AddIncludeValue(navigationType, includeValues, reader);
            Action<object> includeValuesSetter = f => this.SetIncludeValueToTarget(targetType, firstMember, includeSegment, f, includeValues);
            deferredInitializers.Add((includeValues, includeValuesSetter));
        }
        if (isSingle)
        {
            foreach ((var includeValues, var valueSetter) in deferredInitializers)
            {
                if (includeValues is ICollection collection && collection.Count > 0)
                    valueSetter(target);
            }
        }
        else
        {
            var targets = target as ICollection;
            foreach ((var includeValues, var includeValuesSetter) in deferredInitializers)
            {
                if (includeValues is ICollection collection && collection.Count > 0)
                {
                    foreach (var targetItem in targets)
                        includeValuesSetter(targetItem);
                }
            }
        }
        reader.NextResult();
    }
    public virtual async Task SetIncludeValuesAsync(Type targetType, object target, ITheaDataReader reader, bool isSingle, CancellationToken cancellationToken = default)
    {
        var deferredInitializers = new List<(object, Action<object>)>();
        foreach (var includeSegment in this.IncludeTables)
        {
            var navigationType = includeSegment.FromMember.NavigationType;
            var includeValues = this.CreateIncludeValues(navigationType);
            var rootPath = includeSegment.Path.Substring(0, 1);
            var rootReaderField = this.ReaderFields.Find(f => f.Path == rootPath);
            var firstMember = rootReaderField.TargetMember;

            while (await reader.ReadAsync(cancellationToken))
                this.AddIncludeValue(navigationType, includeValues, reader);
            Action<object> includeValuesSetter = f => this.SetIncludeValueToTarget(targetType, firstMember, includeSegment, f, includeValues);
            deferredInitializers.Add((includeValues, includeValuesSetter));
        }
        if (isSingle)
        {
            foreach ((var includeValues, var includeValuesSetter) in deferredInitializers)
            {
                if (includeValues is ICollection collection && collection.Count > 0)
                    includeValuesSetter(target);
            }
        }
        else
        {
            var targets = target as ICollection;
            foreach ((var includeValues, var includeValuesSetter) in deferredInitializers)
            {
                if (includeValues is ICollection collection && collection.Count > 0)
                {
                    foreach (var targetItem in targets)
                        includeValuesSetter(targetItem);
                }
            }
        }
        await reader.NextResultAsync(cancellationToken);
    }
    private object CreateIncludeValues(Type elementType)
    {
        var typedListGetter = typedListGetters.GetOrAdd(elementType, f =>
        {
            var listType = typeof(List<>).MakeGenericType(elementType);
            var bodyExpr = Expression.New(listType.GetConstructor(Type.EmptyTypes));
            return Expression.Lambda<Func<object>>(bodyExpr).Compile();
        });
        return typedListGetter.Invoke();
    }
    private void AddIncludeValue(Type elementType, object includeValues, ITheaDataReader reader)
    {
        var typedReaderElementSetter = typedReaderElementSetters.GetOrAdd(elementType, f =>
        {
            var anonObjsExpr = Expression.Parameter(typeof(object), "anonObjs");
            var readerExpr = Expression.Parameter(typeof(ITheaDataReader), "reader");
            var ormProviderExpr = Expression.Parameter(typeof(IOrmProvider), "ormProvider");
            var mapProviderExpr = Expression.Parameter(typeof(IEntityMapProvider), "mapProvider");

            var blockParameters = new List<ParameterExpression>();
            var blockBodies = new List<Expression>();
            var listType = typeof(List<>).MakeGenericType(elementType);
            var typedListExpr = Expression.Variable(listType, "typedList");
            blockParameters.Add(typedListExpr);
            blockBodies.Add(Expression.Assign(typedListExpr, Expression.Convert(anonObjsExpr, listType)));

            var methodInfo = typeof(Extensions).GetMethod(nameof(Extensions.To), new Type[] { typeof(ITheaDataReader), typeof(IOrmProvider), typeof(IEntityMapProvider) });
            methodInfo = methodInfo.MakeGenericMethod(elementType);
            var elementExpr = Expression.Call(methodInfo, readerExpr, ormProviderExpr, mapProviderExpr);
            methodInfo = listType.GetMethod("Add", new Type[] { elementType });
            blockBodies.Add(Expression.Call(typedListExpr, methodInfo, elementExpr));
            return Expression.Lambda<Action<object, ITheaDataReader, IOrmProvider, IEntityMapProvider>>(
                Expression.Block(blockParameters, blockBodies), anonObjsExpr, readerExpr, ormProviderExpr, mapProviderExpr).Compile();
        });
        typedReaderElementSetter.Invoke(includeValues, reader, this.OrmProvider, this.MapProvider);
    }
    private void SetIncludeValueToTarget(Type targetType, MemberInfo firstMember, TableSegment includeSegment, object target, object includeValues)
    {
        var cacheKey = this.GetIncludeKey(targetType, firstMember, includeSegment);
        var includeValuesSetter = targetIncludeValuesSetters.GetOrAdd(cacheKey, f =>
        {
            var targetExpr = Expression.Parameter(typeof(object), "target");
            var anonListExpr = Expression.Parameter(typeof(object), "anonObjs");
            var blockParameters = new List<ParameterExpression>();
            var blockBodies = new List<Expression>();

            var elementType = includeSegment.FromMember.NavigationType;
            var listType = typeof(List<>).MakeGenericType(elementType);
            var typedListExpr = Expression.Variable(listType, "typedList");
            var typedTargetExpr = Expression.Variable(targetType, "typedTarget");
            blockParameters.AddRange(new[] { typedListExpr, typedTargetExpr });
            blockBodies.Add(Expression.Assign(typedListExpr, Expression.Convert(anonListExpr, listType)));
            blockBodies.Add(Expression.Assign(typedTargetExpr, Expression.Convert(targetExpr, targetType)));

            //order.Seller.Company.Products
            //var foreignKeyValue = target.Seller.Company.Id;
            //target.Order.Seller.Company.Products或是target.Details
            Expression parentExpr = typedTargetExpr;
            if (firstMember != null)
            {
                //target.Order.Seller.Company.Products
                parentExpr = Expression.PropertyOrField(typedTargetExpr, firstMember.Name);
                for (int i = 0; i < includeSegment.ParentMemberVisits.Count - 1; i++)
                {
                    //取父亲对象的完整访问路径：target.Order.Seller.Company
                    var memberInfo = includeSegment.ParentMemberVisits[i];
                    parentExpr = Expression.PropertyOrField(parentExpr, memberInfo.Name);
                }
            }
            var foreignKeyMember = includeSegment.FromTable.Mapper.KeyMembers[0];
            Expression foreignKeyValueExpr = Expression.PropertyOrField(parentExpr, foreignKeyMember.MemberName);
            var memberName = includeSegment.FromMember.MemberName;
            for (int i = 0; i < includeSegment.ParentMemberVisits.Count - 1; i++)
            {
                var memberInfo = includeSegment.ParentMemberVisits[i];
                parentExpr = Expression.PropertyOrField(parentExpr, memberInfo.Name);
            }
            var keyMember = includeSegment.FromTable.Mapper.KeyMembers[0];
            var includeMemberExpr = Expression.PropertyOrField(parentExpr, memberName);

            //var myIncludeValues = includeValues.FindAll(f => f.CompanyId == target.Seller.Company.Id);
            var predicateType = typeof(Predicate<>).MakeGenericType(elementType);
            var parameterExpr = Expression.Parameter(elementType, "f");
            var foreignKey = includeSegment.FromMember.ForeignKey;
            var equalExpr = Expression.Equal(Expression.PropertyOrField(parameterExpr, foreignKey), foreignKeyValueExpr);

            var predicateExpr = Expression.Lambda(predicateType, equalExpr, parameterExpr);
            var methodInfo = listType.GetMethod("FindAll", new Type[] { predicateType });
            var filterValuesExpr = Expression.Call(typedListExpr, methodInfo, predicateExpr);

            var myIncludeValuesExpr = Expression.Variable(listType, "myIncludeValues");
            blockParameters.Add(myIncludeValuesExpr);
            blockBodies.Add(Expression.Assign(myIncludeValuesExpr, filterValuesExpr));

            //target.Seller.Company.Products = myIncludeValues;
            Expression setValueExpr = null;
            switch (includeSegment.FromMember.Member.MemberType)
            {
                case MemberTypes.Field:
                    setValueExpr = Expression.Assign(Expression.Field(parentExpr, memberName), myIncludeValuesExpr);
                    break;
                case MemberTypes.Property:
                    methodInfo = (includeSegment.FromMember.Member as PropertyInfo).GetSetMethod();
                    setValueExpr = Expression.Call(parentExpr, methodInfo, myIncludeValuesExpr);
                    break;
                default: throw new NotSupportedException("目前只支持Field或是Property两种成员访问");
            }

            //if(myIncludeValues.Count>0)
            //  target.Seller.Company.Products = myIncludeValues;
            var greaterThanExpr = Expression.GreaterThan(Expression.Property(myIncludeValuesExpr, "Count"), Expression.Constant(0));
            blockBodies.Add(Expression.IfThen(greaterThanExpr, setValueExpr));
            return Expression.Lambda<Action<object, object>>(Expression.Block(blockParameters, blockBodies), targetExpr, anonListExpr).Compile();
        });
        includeValuesSetter.Invoke(target, includeValues);
    }
    protected bool Include(Expression memberSelector, Action<LambdaExpression, List<ParameterExpression>> tableAliasInitializer, Expression filter = null)
    {
        //if (!string.IsNullOrEmpty(this.WhereSql) || !string.IsNullOrEmpty(this.GroupBySql) || !string.IsNullOrEmpty(this.OrderBySql)
        //    || string.IsNullOrEmpty(this.UnionSql) && this.ReaderFields != null && this.ReaderFields.Count > 0)
        //    throw new NotSupportedException("Include/ThenInclude操作必须要在Where/And/GroupBy/OrderBy/Select等操作之前完成，紧跟From/Join等操作之后");
        var lambdaExpr = memberSelector as LambdaExpression;
        var memberExpr = lambdaExpr.Body as MemberExpression;
        lambdaExpr.Body.GetParameters(out var parameters);
        tableAliasInitializer.Invoke(lambdaExpr, parameters);
        (var includeSegment, var isIncludeMany) = this.AddIncludeTables(memberExpr);

        if (filter != null)
        {
            this.IsIncludeMany = true;
            var filterLambdaExpr = filter as LambdaExpression;
            var parameterName = filterLambdaExpr.Parameters[0].Name;
            this.TableAliases.Clear();
            this.TableAliases.Add(parameterName, includeSegment);
            var sqlSegment = this.Visit(new SqlFieldSegment { Expression = filter });
            includeSegment.Filter = sqlSegment.Body;
            this.IsIncludeMany = false;
        }
        this.LastIncludeSegment = includeSegment;
        return isIncludeMany;
    }
    protected (TableSegment, bool) AddIncludeTables(MemberExpression memberExpr)
    {
        TableSegment tableSegment = null;
        bool isIncludeMany = false;
        var memberType = memberExpr.Member.GetMemberType();
        if (!memberType.IsEntityType(out _))
            throw new NotSupportedException($"Include方法只支持实体属性，{memberExpr.Member.DeclaringType.FullName}.{memberExpr.Member.Name}不是实体，Path:{memberExpr}");

        //支持N级成员访问，如：.Include((x, y) => x.Seller.Company.Products)
        //TODO:IncludeMany后的ThenInclude未实现
        //.IncludeMany(x => x.Orders).ThenInclude(x => x.Buyer)
        var memberExprs = this.GetMemberExprs(memberExpr, out var parameterExpr);
        var fromSegment = this.TableAliases[parameterExpr.Name];
        var fromType = fromSegment.EntityType;
        var builder = new StringBuilder(fromSegment.AliasName);
        //1:N关系，需要记录访问路径，为后面结果赋值做准备
        var memberVisits = new List<MemberInfo>();
        while (memberExprs.TryPop(out var currentExpr))
        {
            //多级成员访问，fromSegment.Mapper可能为null，如：f.Order.Seller.Company
            fromSegment.Mapper ??= this.MapProvider.GetEntityMap(fromType);
            var memberMapper = fromSegment.Mapper.GetMemberMap(currentExpr.Member.Name);

            if (!memberMapper.IsNavigation)
                throw new Exception($"实体{fromType.FullName}的属性{currentExpr.Member.Name}未配置为导航属性");

            //实体类型是成员的声明类型，映射类型不一定是成员的声明类型，一定是成员的Map类型
            //如：成员是UserInfo类型，对应的模型是User类型，UserInfo类型只是User类型的一个子集，成员名称和映射关系完全一致
            var entityType = memberMapper.NavigationType;
            var entityMapper = this.MapProvider.GetEntityMap(entityType, memberMapper.MapType);
            if (entityMapper.KeyMembers.Count > 1)
                throw new Exception($"导航属性表，暂时不支持多个主键字段，实体：{memberMapper.MapType.FullName}");

            memberVisits.Add(currentExpr.Member);
            var rightAlias = $"{(char)(this.TableAsStart + this.Tables.Count)}";
            //path是从顶级级到子级的完整链路，用户查找TableSegment，如：a.Order.Seller.Company
            builder.Append("." + currentExpr.Member.Name);
            //在映射实体时，根据ParentIndex+FromMember值，设置到主表实体的属性中
            if (memberMapper.IsToOne)
            {
                this.Tables.Add(tableSegment = new TableSegment
                {
                    TableType = TableType.Include,
                    JoinType = "LEFT JOIN",
                    EntityType = entityType,
                    Mapper = entityMapper,
                    AliasName = rightAlias,
                    FromTable = fromSegment,
                    FromMember = memberMapper,
                    OnExpr = $"{fromSegment.AliasName}.{this.OrmProvider.GetFieldName(memberMapper.ForeignKey)}={rightAlias}.{this.OrmProvider.GetFieldName(entityMapper.KeyMembers[0].FieldName)}",
                    Path = builder.ToString()
                });
            }
            else
            {
                if (fromSegment.Mapper.KeyMembers.Count > 1)
                    throw new NotSupportedException($"导航属性表，暂时不支持多个主键字段，实体：{fromSegment.EntityType.FullName}");
                this.IncludeTables ??= new();
                this.IncludeTables.Add(tableSegment = new TableSegment
                {
                    TableType = TableType.Include,
                    JoinType = "LEFT JOIN",
                    EntityType = entityType,
                    Mapper = entityMapper,
                    FromTable = fromSegment,
                    FromMember = memberMapper,
                    Path = builder.ToString(),
                    ParentMemberVisits = memberVisits
                });
                isIncludeMany = true;
            }
            fromSegment = tableSegment;
            fromType = memberMapper.NavigationType;
        }
        return (tableSegment, isIncludeMany);
    }
    private (string, Action<StringBuilder, IOrmProvider, object>) BuildIncludeSqlGetter(Type targetType, MemberInfo firstMember, TableSegment includeSegment)
    {
        var cacheKey = this.GetIncludeKey(targetType, firstMember, includeSegment);
        return includeSqlGetterCache.GetOrAdd(cacheKey, f =>
        {
            var targetExpr = Expression.Parameter(typeof(object), "target");
            var builderExpr = Expression.Parameter(typeof(StringBuilder), "builder");
            var ormProviderExpr = Expression.Parameter(typeof(IOrmProvider), "ormProvider");
            var blockParameters = new List<ParameterExpression>();
            var blockBodies = new List<Expression>();

            var typedTargetExpr = Expression.Variable(targetType, "typedTarget");
            blockParameters.Add(typedTargetExpr);
            blockBodies.Add(Expression.Assign(typedTargetExpr, Expression.Convert(targetExpr, targetType)));

            //target.Order.Seller.Company.Products或是target.Details           
            Expression parentExpr = typedTargetExpr;
            if (firstMember != null)
            {
                //target.Order.Seller.Company.Products
                parentExpr = Expression.PropertyOrField(typedTargetExpr, firstMember.Name);
                for (int i = 0; i < includeSegment.ParentMemberVisits.Count - 1; i++)
                {
                    //取父亲对象的完整访问路径：target.Order.Seller.Company
                    var memberInfo = includeSegment.ParentMemberVisits[i];
                    parentExpr = Expression.PropertyOrField(parentExpr, memberInfo.Name);
                }
            }
            var foreignKeyMember = includeSegment.FromTable.Mapper.KeyMembers[0];
            Expression foreignKeyValueExpr = Expression.PropertyOrField(parentExpr, foreignKeyMember.MemberName);
            //includeMany支持分表，UNION SQL处理
            foreignKeyValueExpr = Expression.Convert(foreignKeyValueExpr, typeof(object));
            var methedInfo = typeof(IOrmProvider).GetMethod(nameof(IOrmProvider.GetQuotedValue));
            var fieldTypeExpr = Expression.Constant(foreignKeyMember.MemberType);
            foreignKeyValueExpr = Expression.Call(ormProviderExpr, methedInfo, fieldTypeExpr, foreignKeyValueExpr);
            methedInfo = typeof(StringBuilder).GetMethod(nameof(StringBuilder.Append), new Type[] { typeof(string) });
            blockBodies.Add(Expression.Call(builderExpr, methedInfo, foreignKeyValueExpr));

            var foreignKey = this.OrmProvider.GetFieldName(includeSegment.FromMember.ForeignKey);
            var fields = RepositoryHelper.BuildFieldsSqlPart(this.OrmProvider, includeSegment.Mapper, includeSegment.EntityType, true);
            var headSql = $"SELECT {fields} FROM {{0}} WHERE {foreignKey} IN (";
            var sqlInitializer = Expression.Lambda<Action<StringBuilder, IOrmProvider, object>>(Expression.Block(blockParameters, blockBodies), builderExpr, ormProviderExpr, targetExpr).Compile();
            return (headSql, sqlInitializer);
        });
    }

    public virtual void Where(Expression whereExpr)
    {
        //为了兼容，Where条件中的Exists，多个表联合查询时，有Where+Exists的场景
        if (!string.IsNullOrEmpty(this.WhereSql))
        {
            this.And(whereExpr);
            return;
        }
        this.IsWhere = true;
        var lambdaExpr = whereExpr as LambdaExpression;
        this.ClearUnionSql();
        this.InitTableAlias(lambdaExpr);
        this.LastWhereOperationType = OperationType.None;
        this.WhereSql = this.VisitConditionExpr(lambdaExpr.Body, out var operationType);
        this.LastWhereOperationType = operationType;
        this.IsWhere = false;
    }
    public virtual void And(Expression whereExpr)
    {
        this.IsWhere = true;
        var lambdaExpr = whereExpr as LambdaExpression;
        this.ClearUnionSql();
        this.InitTableAlias(lambdaExpr);
        if (this.LastWhereOperationType == OperationType.Or)
            this.WhereSql = $"({this.WhereSql})";
        var conditionSql = this.VisitConditionExpr(lambdaExpr.Body, out var operationType);
        if (operationType == OperationType.Or)
            conditionSql = $"({conditionSql})";
        this.LastWhereOperationType = OperationType.And;
        if (!string.IsNullOrEmpty(this.WhereSql))
            this.WhereSql += " AND " + conditionSql;
        else this.WhereSql = conditionSql;
        this.IsWhere = false;
    }
    public virtual void GroupBy(Expression expr)
    {
        var lambdaExpr = expr as LambdaExpression;
        if (lambdaExpr.Body.NodeType != ExpressionType.New && lambdaExpr.Body.NodeType != ExpressionType.MemberAccess)
            throw new Exception("不支持的表达式访问，GroupBy只支持New或MemberAccess表达式");

        this.ClearUnionSql();
        this.InitTableAlias(lambdaExpr);
        this.GroupFields = new();
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
                    this.GroupFields.Add(sqlSegment);
                    index++;
                }
                //GroupBy SQL中不含别名
                this.GroupBySql = builder.ToString();
                break;
            case ExpressionType.MemberAccess:
                {
                    var memberExpr = lambdaExpr.Body as MemberExpression;
                    var sqlSegment = this.VisitAndDeferred(new SqlFieldSegment { Expression = memberExpr });
                    var fieldName = sqlSegment.Body ?? sqlSegment.Value.ToString();
                    var memberInfo = memberExpr.Member;
                    sqlSegment.TargetMember = memberInfo;
                    sqlSegment.SegmentType = memberInfo.GetMemberType();
                    this.GroupFields.Add(sqlSegment);
                    this.GroupBySql = fieldName;
                }
                break;
        }
    }
    public virtual void OrderBy(string orderType, Expression expr)
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
                            index++;
                            continue;
                        }
                        var memberInfo = newExpr.Members[index];
                        var sqlSegment = this.VisitAndDeferred(new SqlFieldSegment { Expression = argumentExpr });
                        if (builder.Length > 0)
                            builder.Append(',');

                        builder.Append(sqlSegment.Body ?? sqlSegment.Value.ToString());
                        if (orderType == "DESC")
                            builder.Append(" DESC");
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
                            break;
                        }
                        if (this.IsGroupingMember(memberExpr.Expression as MemberExpression))
                        {
                            var readerField = this.GroupFields.Find(f => f.TargetMember.Name == memberExpr.Member.Name);
                            builder.Append(readerField.Body);
                            if (orderType == "DESC")
                                builder.Append(" DESC");
                            break;
                        }
                        var sqlSegment = this.VisitAndDeferred(new SqlFieldSegment { Expression = memberExpr });
                        builder.Append(sqlSegment.Body ?? sqlSegment.Value.ToString());
                        if (orderType == "DESC")
                            builder.Append(" DESC");
                    }
                    break;
            }
        }
        this.OrderBySql = builder.ToString();
    }
    public virtual void Having(Expression havingExpr)
    {
        this.IsWhere = true;
        var lambdaExpr = havingExpr as LambdaExpression;
        this.InitTableAlias(lambdaExpr);
        this.HavingSql = this.VisitConditionExpr(lambdaExpr.Body, out _);
        this.IsWhere = false;
    }

    public virtual void SelectGrouping() => this.ReaderFields = this.GroupFields;
    public virtual void SelectDefault(Expression defaultExpr)
    {
        if (this.ReaderFields != null && this.ReaderFields.Count > 0)
            return;
        this.Select(null, defaultExpr);
    }
    public virtual void Select(string sqlFormat, Expression selectExpr = null)
    {
        this.IsSelect = true;
        if (selectExpr != null)
        {
            var toTargetExpr = selectExpr as LambdaExpression;
            this.ClearUnionSql();
            this.InitTableAlias(toTargetExpr);
            var sqlSegment = new SqlFieldSegment { Expression = toTargetExpr.Body };
            switch (toTargetExpr.Body.NodeType)
            {
                case ExpressionType.Parameter:
                    sqlSegment = this.VisitParameter(sqlSegment);
                    this.ReaderFields = sqlSegment.Value as List<SqlFieldSegment>;
                    this.ReaderFields[0].IsTargetType = true;
                    break;
                case ExpressionType.New:
                case ExpressionType.MemberInit:
                    sqlSegment = this.VisitAndDeferred(sqlSegment);
                    this.ReaderFields = sqlSegment.Value as List<SqlFieldSegment>;
                    break;
                case ExpressionType.MemberAccess:
                    MemberInfo memberInfo = null;
                    if (toTargetExpr.Body is MemberExpression memberExpr)
                        memberInfo = memberExpr.Member;
                    sqlSegment = this.VisitAndDeferred(sqlSegment);
                    sqlSegment.TargetMember = memberInfo;
                    this.ReaderFields = new List<SqlFieldSegment> { sqlSegment };
                    break;
                default:
                    //单个字段或单个值，常量、方法调用、表达式计算场景
                    if (toTargetExpr.Body.NodeType == ExpressionType.Call)
                        sqlSegment.OriginalExpression = toTargetExpr;
                    sqlSegment = this.VisitAndDeferred(sqlSegment);
                    //延迟方法调用，参数可能有多个，返回的ReaderField只有一个
                    //不一定有成员名称，无需设置TableSegment/FromMember/TargetMember，如：.Select(f => f.Age / 10 * 10)
                    sqlSegment.FieldType = SqlFieldType.Field;
                    sqlSegment.SegmentType ??= selectExpr.Type;
                    //常量和变量body没有值，最后BuildSql时，再进行设置
                    this.ReaderFields = new List<SqlFieldSegment> { sqlSegment };
                    break;
            }
        }
        if (!string.IsNullOrEmpty(sqlFormat))
        {
            //单值操作，SELECT COUNT(DISTINCT b.Id),MAX(b.Amount),COUNT(1)等
            if (this.ReaderFields != null && this.ReaderFields.Count > 0)
            {
                var readerField = this.ReaderFields[0];
                readerField.Body = string.Format(sqlFormat, readerField.Body);
            }
            //单值操作，SELECT COUNT(1)/*等
            else this.ReaderFields = new List<SqlFieldSegment> { new SqlFieldSegment { Body = sqlFormat } };
        }
        this.IsSelect = false;
    }
    public virtual void SelectFlattenTo(Type targetType, Expression specialMemberSelector = null)
    {
        this.IsSelect = true;
        if (specialMemberSelector != null)
        {
            var lambdaExpr = specialMemberSelector as LambdaExpression;
            this.ClearUnionSql();
            this.InitTableAlias(lambdaExpr);
            if (lambdaExpr.Body.NodeType == ExpressionType.MemberInit)
            {
                var sqlSegment = this.VisitAndDeferred(new SqlFieldSegment { Expression = lambdaExpr.Body });
                this.ReaderFields = sqlSegment.Value as List<SqlFieldSegment>;
            }
            else this.ReaderFields = new();
            bool isExistsFields = false;
            List<string> existsMembers = null;
            if (this.ReaderFields.Count > 0)
            {
                existsMembers = this.ReaderFields.Select(f => f.TargetMember.Name).ToList();
                isExistsFields = true;
            }
            var targetMembers = targetType.GetMembers(BindingFlags.Public | BindingFlags.Instance)
                .Where(f => f.MemberType == MemberTypes.Property | f.MemberType == MemberTypes.Field).ToList();

            foreach (var memberInfo in targetMembers)
            {
                if (isExistsFields && existsMembers.Contains(memberInfo.Name)) continue;
                if (this.TryFindReaderField(memberInfo, out var readerField))
                    this.ReaderFields.Add(readerField);
            }
        }
        else
        {
            this.ReaderFields = new();
            var targetMembers = targetType.GetMembers(BindingFlags.Public | BindingFlags.Instance)
                .Where(f => f.MemberType == MemberTypes.Property | f.MemberType == MemberTypes.Field).ToList();

            foreach (var memberInfo in targetMembers)
            {
                if (this.TryFindReaderField(memberInfo, out var readerField))
                    this.ReaderFields.Add(readerField);
            }
        }
        this.IsFromQuery = false;
        this.IsSelect = false;
    }
    public virtual bool TryFindReaderField(MemberInfo memberInfo, out SqlFieldSegment readerField)
    {
        foreach (var tableSegment in this.Tables)
        {
            if (this.TryFindReaderField(tableSegment, memberInfo, out readerField))
                return true;
        }
        readerField = null;
        return false;
    }
    public virtual bool TryFindReaderField(TableSegment tableSegment, MemberInfo memberInfo, out SqlFieldSegment readerField)
    {
        readerField = null;
        if (tableSegment.Fields != null)
        {
            readerField = tableSegment.Fields.Find(f => f.FromMember.Name == memberInfo.Name);
            if (readerField == null) return false;
            readerField.TargetMember = memberInfo;
        }
        else
        {
            if (!tableSegment.Mapper.TryGetMemberMap(memberInfo.Name, out var memberMapper))
                return false;
            var segmentType = memberInfo.GetMemberType();
            Type expectType = null;
            if (segmentType.IsEnumType(out var underlyingType, out _))
                expectType = underlyingType;
            readerField = new SqlFieldSegment
            {
                FieldType = SqlFieldType.Field,
                HasField = true,
                FromMember = memberMapper.Member,
                TargetMember = memberInfo,
                SegmentType = segmentType,
                ExpectType = expectType,
                NativeDbType = memberMapper.NativeDbType,
                TypeHandler = memberMapper.TypeHandler,
                Body = tableSegment.AliasName + "." + this.OrmProvider.GetFieldName(memberMapper.FieldName)
            };
        }
        return true;
    }
    public virtual void Distinct() => this.IsDistinct = true;
    public virtual void Page(int pageNumber, int pageSize)
    {
        this.PageNumber = pageNumber;
        this.PageSize = pageSize;
        if (pageNumber > 0) pageNumber--;
        this.skip = pageNumber * pageSize;
        this.limit = pageSize;
        this.ClearUnionSql();
    }
    public virtual void Skip(int skip)
    {
        this.skip = skip;
        this.ClearUnionSql();
    }
    public virtual void Take(int limit)
    {
        this.limit = limit;
        this.ClearUnionSql();
    }

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
            //Select((x, a, b, ... ) => new { x.Grouping, ... });
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
            //Select((x, a, b, ... ) => new { x.Grouping.Id, x.Grouping.Name, ... });
            else if (this.IsGroupingMember(memberExpr.Expression as MemberExpression))
            {
                //此时是Grouping对象字段的引用，最外面可能会更改成员名称，要复制一份，防止更改Grouping对象中的字段
                var readerField = this.GroupFields.Find(f => f.TargetMember.Name == memberInfo.Name);
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
                            sqlSegment.FieldType = SqlFieldType.IncludeRef;
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
                        //取AS后的字段名，与原字段名不一定一样，AS后的字段名与memberExpr.Member.Name一致
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
                        sqlSegment.SegmentType = readerField.SegmentType;
                        if (readerField.SegmentType.IsEnumType(out var underlyingType))
                            sqlSegment.ExpectType = underlyingType;

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
                        sqlSegment.Fields = readerField.Fields;
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
    public override SqlFieldSegment VisitNew(SqlFieldSegment sqlSegment)
    {
        var newExpr = sqlSegment.Expression as NewExpression;
        //Select场景
        if (this.IsSelect && newExpr.Type.Name.StartsWith("<>"))
        {
            this.IsSelectMember = true;
            var readerFields = new List<SqlFieldSegment>();
            //为给里面的成员访问提供数据，有参数访问、引用Include成员访问的场景提供数据参数访问的ReaderField查询
            this.ReaderFields = readerFields;
            for (int i = 0; i < newExpr.Arguments.Count; i++)
            {
                this.AddSelectElement(newExpr.Arguments[i], newExpr.Members[i], readerFields);
            }
            this.IsSelectMember = false;
            return sqlSegment.ChangeValue(readerFields);
        }
        return this.Evaluate(sqlSegment);
    }
    public override SqlFieldSegment VisitMemberInit(SqlFieldSegment sqlSegment)
    {
        var memberInitExpr = sqlSegment.Expression as MemberInitExpression;
        //Select场景
        if (this.IsSelect)
        {
            this.IsSelectMember = true;
            var readerFields = new List<SqlFieldSegment>();
            //为给里面的成员访问提供数据，有参数访问、引用Include成员访问的场景提供数据参数访问的ReaderField查询
            this.ReaderFields = readerFields;
            for (int i = 0; i < memberInitExpr.Bindings.Count; i++)
            {
                if (memberInitExpr.Bindings[i].BindingType != MemberBindingType.Assignment)
                    throw new NotSupportedException("暂时不支持除MemberBindingType.Assignment类型外的成员绑定表达式");
                var memberAssignment = memberInitExpr.Bindings[i] as MemberAssignment;
                this.AddSelectElement(memberAssignment.Expression, memberAssignment.Member, readerFields);
            }
            this.IsSelectMember = false;
            return sqlSegment.ChangeValue(readerFields);
        }
        return this.Evaluate(sqlSegment);
    }
    public virtual void AddSelectElement(Expression elementExpr, MemberInfo memberInfo, List<SqlFieldSegment> readerFields)
    {
        var sqlSegment = new SqlFieldSegment { Expression = elementExpr };
        switch (elementExpr.NodeType)
        {
            case ExpressionType.Parameter:
                if (this.IsFromQuery)
                    throw new NotSupportedException("FROM子查询中不支持参数Parameter表达式访问，只支持基础字段访问访问");
                //两种场景：.Select((x, y) => new { Order = x, x.Seller, x.Buyer, ... }) 和 .Select((x, y) => x)，可能有include操作
                sqlSegment = this.VisitParameter(sqlSegment);
                var tableReaderFields = sqlSegment.Value as List<SqlFieldSegment>;
                tableReaderFields[0].FromMember = memberInfo;
                tableReaderFields[0].TargetMember = memberInfo;
                readerFields.AddRange(tableReaderFields);
                break;
            case ExpressionType.New:
            case ExpressionType.MemberInit:
                //为了简化SELECT操作，只支持一次New/MemberInit表达式操作
                throw new NotSupportedException("不支持的表达式访问，SELECT语句只支持一次New/MemberInit表达式访问操作");
            case ExpressionType.MemberAccess:
                sqlSegment = this.VisitAndDeferred(sqlSegment);
                if (sqlSegment.FieldType != SqlFieldType.IncludeRef)
                {
                    this.GetQuotedValue(sqlSegment, true);
                    if (sqlSegment.IsConstant || sqlSegment.IsVariable || sqlSegment.HasParameter || sqlSegment.IsExpression || sqlSegment.IsMethodCall)
                        sqlSegment.IsNeedAlias = true;
                }
                sqlSegment.TargetMember = memberInfo;
                sqlSegment.SegmentType = memberInfo.GetMemberType();
                readerFields.Add(sqlSegment);
                break;
            default:
                //常量或方法或表达式访问
                sqlSegment = this.VisitAndDeferred(sqlSegment);
                this.GetQuotedValue(sqlSegment, true);
                //DeferredFields场景
                //函数调用，参数引用多个字段
                //.SelectFlattenTo<DTO>((a, b ...) => new DTO
                //{
                //    ActivityTypeEnum = this.GetEmnuName(f.ActivityType)
                //})
                if (!sqlSegment.IsDeferredFields)
                {
                    sqlSegment.FieldType = SqlFieldType.Field;
                    //只有常量、方法调用、表达式计算，没有设置NativeDbType和TypeHandler，需要根据memberInfo类型获取
                    //常量和变量，暂时不做GetQuotedValue处理，在BuildSql时候，再进行处理，Value值保留
                    if (sqlSegment.IsConstant || sqlSegment.IsVariable || sqlSegment.HasParameter || sqlSegment.IsExpression || sqlSegment.IsMethodCall)
                        sqlSegment.IsNeedAlias = true;
                }
                //sqlSegment.FromMember = memberInfo;
                sqlSegment.TargetMember = memberInfo;
                //常量或变量场景，此值为null
                sqlSegment.SegmentType = memberInfo.GetMemberType();
                readerFields.Add(sqlSegment);
                break;
        }
    }
    public virtual TableSegment AddTable(TableSegment tableSegment)
    {
        //Union后，有加新表，要把前一个UnionSql设置完整
        this.ClearUnionSql();
        this.Tables.Add(tableSegment);
        if (this.ReaderFields != null && !this.IsUnion)
            this.ReaderFields = null;
        return tableSegment;
    }
    public virtual TableSegment AddTable(Type entityType, string joinType = "", TableType tableType = TableType.Entity, string body = null, List<SqlFieldSegment> readerFields = null)
    {
        int tableIndex = this.TableAsStart + this.Tables.Count;
        return this.AddTable(new TableSegment
        {
            JoinType = joinType,
            EntityType = entityType,
            AliasName = $"{(char)tableIndex}",
            Path = $"{(char)tableIndex}",
            TableType = tableType,
            Body = body,
            Fields = readerFields,
            IsMaster = true
        });
    }
    public TableSegment AddSubQueryTable(Type targetType, IQuery subQuery, string joinType = null)
    {
        TableSegment tableSegment = null;
        List<SqlFieldSegment> readerFields = null;
        var tableType = TableType.FromQuery;
        string body = null;
        subQuery.Visitor.IsUseCteTable = false;
        if (subQuery is ICteQuery cteQuery)
        {
            readerFields = new();
            //不能更改原CTE表中的ReaderFields，更改了后续的SQL使用时，就不正确了，这里只能更改一个副本用于SQL解析
            cteQuery.ReaderFields.ForEach(f => readerFields.Add(f.Clone()));
            tableType = TableType.CteSelfRef;
            //当作正常表名处理
            body = cteQuery.TableName;
        }
        else body = $"({subQuery.Visitor.BuildSql(out readerFields)})";

        //TODO:CTE表和子查询都要添加到当前的CteQueries中，用于判断参数是否添加，避免重复添加参数
        if (!this.RefQueries.Contains(subQuery))
            subQuery.CopyTo(this);
        tableSegment = this.AddTable(targetType, joinType, tableType, body, readerFields);
        //因为表别名发生变化，需要更新ReaderField的body栏位
        this.InitFromQueryReaderFields(tableSegment, readerFields);

        //子查询中引用了分表，最外层也需要设置分表信息IsSharding、ShardingTables
        if (subQuery.Visitor.ShardingTables != null && subQuery.Visitor.ShardingTables.Count > 0)
        {
            tableSegment.IsSharding = true;
            this.IsNeedFetchShardingTables = subQuery.Visitor.IsNeedFetchShardingTables;
            if (this.ShardingTables == null)
                this.ShardingTables = subQuery.Visitor.ShardingTables;
            else
            {
                foreach (var shardingTable in subQuery.Visitor.ShardingTables)
                {
                    if (!this.ShardingTables.Contains(shardingTable))
                        this.ShardingTables.Add(shardingTable);
                }
            }
        }
        return tableSegment;
    }
    public virtual TableSegment InitTableAlias(LambdaExpression lambdaExpr)
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
                if (typeof(IAggregateSelect).IsAssignableFrom(parameterExpr.Type))
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
                this.TableAliases.Add(parameterName, this.RefTableAliases[parameterName]);
            }
        }
        return tableSegment;
    }
    public virtual void AddSelectFieldsSql(StringBuilder builder, List<SqlFieldSegment> readerFields)
    {
        int index = 0;
        string body = null;
        bool isOnlyField = readerFields.Count == 1 && readerFields[0].FieldType == SqlFieldType.Field;
        foreach (var readerField in readerFields)
        {
            if (readerField.FieldType == SqlFieldType.IncludeRef)
                continue;
            if (index > 0) builder.Append(',');
            switch (readerField.FieldType)
            {
                case SqlFieldType.Entity:
                    this.AddSelectFieldsSql(builder, readerField.Fields);
                    break;
                case SqlFieldType.DeferredFields:
                    if (readerField.Fields == null)
                        continue;
                    body = this.GetQuotedValue(readerField);
                    builder.Append(body);
                    //生成SQL的时候，才加上AS别名
                    if (this.IsNeedAlias(readerField, isOnlyField))
                        builder.Append($" AS {this.OrmProvider.GetFieldName(readerField.TargetMember.Name)}");
                    break;
                default:
                    body = this.GetQuotedValue(readerField);
                    //CTE表字段是常量/变量/字段名称，都有可能和声明的字段不一致，所以需要获取CTE表的声明字段
                    //body里面的值，是原始的值或是字段名
                    if (readerField.TableSegment != null && readerField.TableSegment.TableType == TableType.CteSelfRef)
                        body = $"{readerField.TableSegment.AliasName}.{this.OrmProvider.GetFieldName(readerField.TargetMember.Name)}";
                    builder.Append(body);
                    //生成SQL的时候，才加上AS别名
                    if (this.IsNeedAlias(readerField, isOnlyField))
                        builder.Append($" AS {this.OrmProvider.GetFieldName(readerField.TargetMember.Name)}");
                    break;
            }
            index++;
        }
    }
    public virtual bool IsNeedAlias(SqlFieldSegment readerField, bool isOnlyField)
    {
        if (this.IsSecondUnion || this.IsFromCommand || this.IsSecondUnion || this.IsCteTable) return false;
        if (readerField.IsNeedAlias) return true;
        if (isOnlyField) return false;
        if (readerField.Fields != null && readerField.Fields.Count > 1)
            return false;
        //GroupFields中的ReaderField只设置了必须加as别名的情况，没有设置TargetMember.Name !=FromMember.Name的情况，这里把这种情况补上
        //PostgreSql时，DistinctOnFields中的ReaderField也是这个场景
        if (readerField.IsConstant || readerField.IsVariable || readerField.HasParameter
            || readerField.IsExpression || readerField.IsMethodCall) return true;
        if (readerField.TargetMember != null && readerField.FromMember != null)
            return readerField.TargetMember.Name != readerField.FromMember.Name;
        return false;
    }
    public virtual void ClearUnionSql()
    {
        if (this.UnionSql == null) return;

        //有union操作的visitor，都是新New的，前面只有一个表
        this.Tables[0].Body = $"({this.UnionSql})";
        this.Tables[0].TableType = TableType.FromQuery;
        this.UnionSql = null;
    }
    public virtual void Clear(bool isClearReaderFields = false)
    {
        this.Tables.Clear();
        if (isClearReaderFields)
            this.ReaderFields = null;
        this.WhereSql = null;
        this.TableAsStart = 'a';

        this.skip = null;
        this.limit = null;
        this.UnionSql = null;
        this.GroupBySql = null;
        this.HavingSql = null;
        this.OrderBySql = null;
        this.IsDistinct = false;
        this.IncludeTables?.Clear();
        this.LastIncludeSegment = null;
        this.GroupFields?.Clear();
        this.IsSecondUnion = false;
        this.IsUseCteTable = true;
        this.IsNeedTableAlias = true;
    }
    public override void Dispose()
    {
        if (this.isDisposed)
            return;
        this.isDisposed = true;

        this.UnionSql = null;
        this.GroupBySql = null;
        this.HavingSql = null;
        this.OrderBySql = null;

        this.deferredSegments = null;
        this.LastIncludeSegment = null;
        this.SelfRefQueryObj = null;

        base.Dispose();
    }
    protected void InitFromQueryReaderFields(TableSegment tableSegment, List<SqlFieldSegment> readerFields)
    {
        foreach (var readerField in readerFields)
        {
            //子查询中，访问了实体类对象，比如：Grouping分组对象或是匿名对象
            if (readerField.FieldType == SqlFieldType.Entity)
                this.InitFromQueryReaderFields(tableSegment, readerField.Fields);
            else
            {
                //已经变成子查询了，原表字段名已经没意义了，直接变成新的字段名
                readerField.TableSegment = tableSegment;
                readerField.FromMember = readerField.TargetMember;
                //重新设置body内容，表别名变更，字段名也可能变更
                if (readerField.TargetMember != null)
                    readerField.Body = tableSegment.AliasName + "." + this.OrmProvider.GetFieldName(readerField.TargetMember.Name);
            }
        }
    }
    public Stack<MemberExpression> GetMemberExprs(MemberExpression memberExpr, out ParameterExpression parameterExpr)
    {
        Expression currentExpr = memberExpr;
        parameterExpr = null;
        var memberExprs = new Stack<MemberExpression>();
        while (currentExpr != null)
        {
            if (currentExpr.NodeType == ExpressionType.Parameter)
            {
                parameterExpr = currentExpr as ParameterExpression;
                break;
            }
            if (currentExpr.NodeType == ExpressionType.Convert)
            {
                var unaryExpr = currentExpr as UnaryExpression;
                if (unaryExpr.Operand.NodeType == ExpressionType.Parameter)
                {
                    parameterExpr = unaryExpr.Operand as ParameterExpression;
                    break;
                }
                else throw new NotSupportedException($"不支持的成员访问表达式，访问路径：{currentExpr.ToString()}");
            }
            if (currentExpr.NodeType == ExpressionType.MemberAccess)
            {
                var parentExpr = currentExpr as MemberExpression;
                memberExprs.Push(parentExpr);
                currentExpr = parentExpr.Expression;
            }
        }
        return memberExprs;
    }
    public int GetIncludeKey(Type targetType, MemberInfo firstMember, TableSegment includeSegment)
    {
        int pathLength = includeSegment.ParentMemberVisits.Count;
        var builder = new StringBuilder();
        if (firstMember != null)
        {
            pathLength++;
            builder.Append($".{firstMember.Name}");
        }
        foreach (var memberInfo in includeSegment.ParentMemberVisits)
        {
            builder.Append($".{memberInfo.Name}");
        }
        var path = builder.ToString();
#if NETCOREAPP2_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        var hashCode = new HashCode();
        hashCode.Add(targetType);
        hashCode.Add(pathLength);
        hashCode.Add(path);
        return hashCode.ToHashCode();
#else
        int hashCode = 17;
        unchecked
        {
            hashCode = hashCode * 23 + this.OrmProvider.OrmProviderType.GetHashCode();
            hashCode = hashCode * 23 + targetType.GetHashCode();
            hashCode = hashCode * 23 + pathLength.GetHashCode();
            hashCode = hashCode * 23 + path.GetHashCode();
        }
        return hashCode;
#endif
    }
}
