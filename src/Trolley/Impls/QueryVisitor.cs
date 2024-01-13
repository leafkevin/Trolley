using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley;

public class QueryVisitor : SqlVisitor, IQueryVisitor
{
    private static ConcurrentDictionary<int, (string, Action<IOrmProvider, StringBuilder, object>)> includeSqlGetterCache = new();
    private static ConcurrentDictionary<Type, Func<object>> typedListGetters = new();
    private static ConcurrentDictionary<Type, Action<object, string, IDataReader, IOrmProvider, IEntityMapProvider>> typedReaderElementSetters = new();
    private static ConcurrentDictionary<int, Action<object, object>> targetIncludeValuesSetters = new();
    private static ConcurrentDictionary<int, Action<object>> targetRefIncludeValuesSetters = new();

    protected List<CommandSegment> deferredSegments = new();
    protected List<Action<object>> deferredRefIncludeValuesSetters = null;
    protected int? skip;
    protected int? limit;

    protected string UnionSql { get; set; }
    protected string GroupBySql { get; set; }
    protected string HavingSql { get; set; }
    protected string OrderBySql { get; set; }
    protected bool IsDistinct { get; set; }
    protected bool IsSelectMember { get; set; }
    /// <summary>
    /// Union的第二个子句解析时，此值为true，第一次是为false
    /// </summary>
    protected bool IsUnion { get; set; }
    protected bool IsFromCommand { get; set; }

    protected List<TableSegment> IncludeSegments { get; set; }
    protected TableSegment LastIncludeSegment { get; set; }

    public bool IsRecursive { get; set; }
    public List<TableSegment> CteTables { get; set; }
    public List<object> CteQueries { get; set; }
    public Dictionary<object, TableSegment> CteTableSegments { get; set; }
    /// <summary>
    /// 只有使用CTE时候有值，当前CTE子查询自身引用，因为不确定后面是否会引用自身，先把自身引用保存起来，方便后面引用
    /// </summary>
    public TableSegment SelfTableSegment { get; set; }

    public QueryVisitor(string dbKey, IOrmProvider ormProvider, IEntityMapProvider mapProvider, bool isParameterized = false, char tableAsStart = 'a', string parameterPrefix = "p", IDataParameterCollection dbParameters = null)
    {
        this.DbKey = dbKey;
        this.OrmProvider = ormProvider;
        this.MapProvider = mapProvider;
        this.IsParameterized = isParameterized;
        this.TableAsStart = tableAsStart;
        this.ParameterPrefix = parameterPrefix;
        this.DbParameters = dbParameters ?? new TheaDbParameterCollection();
        this.IsNeedAlias = true;
    }
    public virtual string BuildSql(out List<ReaderField> readerFields, bool hasCteSql = true, bool isUnion = false)
    {
        var builder = new StringBuilder();
        if (this.CteQueries != null)
        {
            builder.Append("WITH ");
            if (this.IsRecursive)
                builder.Append("RECURSIVE ");
            for (int i = 0; i < this.CteTables.Count; i++)
            {
                if (i > 0) builder.AppendLine(",");
                builder.Append(this.CteTables[i].Body);
                builder.AppendLine();
            }
        }
        readerFields = this.ReaderFields;

        if (!string.IsNullOrEmpty(this.UnionSql))
        {
            builder.Append(this.UnionSql);
            var sql = builder.ToString();
            builder.Clear();
            return sql;
        }
        var headSql = builder.ToString();
        builder.Clear();

        //各种单值查询，如：SELECT COUNT(*)/MAX(*)..等，都有SELECT操作     
        //如：From(f=>...).InnerJoin/UnionAll(f=>...)

        //生成sql时，include表的字段，一定要紧跟着主表字段后面，方便赋值主表实体的属性中，所以在插入时候就排好序
        //方案：在buildSql时确定，ReaderFields要重新排好序，include字段放到对应主表字段后面，表别名顺序不变
        builder.Append(this.BuildSelectSql(this.ReaderFields, this.IsFromQuery || isUnion));

        string selectSql = null;
        if (this.IsDistinct)
            selectSql = "DISTINCT " + builder.ToString();
        else selectSql = builder.ToString();

        builder.Clear();
        string tableSql = null;
        if (this.Tables.Count > 0)
        {
            foreach (var tableSegment in this.Tables)
            {
                string tableName = string.Empty;
                if (tableSegment.TableType == TableType.CteSelfRef)
                    tableName = this.OrmProvider.GetTableName(tableSegment.RefTableName);
                else
                {
                    tableName = tableSegment.Body;
                    if (string.IsNullOrEmpty(tableName))
                        tableName = this.OrmProvider.GetTableName(tableSegment.Mapper.TableName);
                }

                if (builder.Length > 0)
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
            }
            tableSql = builder.ToString();
        }

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
        if (isUnion && (!string.IsNullOrEmpty(this.OrderBySql) || this.limit.HasValue))
        {
            builder.Insert(0, "SELECT * FROM (");
            builder.Append($") a");
        }
        return builder.ToString();
    }
    public virtual string BuildCommandSql(Type targetType, out IDataParameterCollection dbParameters)
    {
        var builder = new StringBuilder();
        var entityMapper = this.MapProvider.GetEntityMap(targetType);
        builder.Append($"INSERT INTO {this.OrmProvider.GetTableName(entityMapper.TableName)} (");
        int index = 0;
        foreach (var readerField in this.ReaderFields)
        {
            //Union后，如果没有select语句时，通常实体类型或是select分组对象
            if (!entityMapper.TryGetMemberMap(readerField.TargetMember.Name, out var propMapper)
                || propMapper.IsIgnore || propMapper.IsNavigation || propMapper.IsAutoIncrement
                || (propMapper.MemberType.IsEntityType(out _) && propMapper.TypeHandler == null))
                continue;
            if (index > 0) builder.Append(',');
            builder.Append($"{this.OrmProvider.GetFieldName(propMapper.FieldName)}");
            index++;
        }
        builder.Append(") ");
        //有CTE表
        if (this.CteQueries != null)
        {
            builder.Append("WITH ");
            if (this.IsRecursive)
                builder.Append("RECURSIVE ");
            for (int i = 0; i < this.CteTables.Count; i++)
            {
                if (i > 0) builder.AppendLine(",");
                builder.Append(this.CteTables[i].Body);
                builder.AppendLine();
            }
        }
        dbParameters = this.DbParameters;
        if (!string.IsNullOrEmpty(this.UnionSql))
        {
            builder.Append($"SELECT * FROM ({this.UnionSql}) t");
            var sql = builder.ToString();
            builder.Clear();
            return sql;
        }
        var headSql = builder.ToString();
        builder.Clear();
        //生成sql时，include表的字段，一定要紧跟着主表字段后面，方便赋值主表实体的属性中，所以，要排序或是插入时候就排好序
        //方案：在buildSql时确定，ReaderFields要重新排好序，include字段放到对应主表字段后面，表别名顺序不变
        this.AddSelectSqlTo(builder, this.ReaderFields);

        string selectSql = null;
        if (this.IsDistinct)
            selectSql = "DISTINCT " + builder.ToString();
        else selectSql = builder.ToString();

        builder.Clear();
        string tableSql = null;
        if (this.Tables.Count > 0)
        {
            index = 0;
            foreach (var tableSegment in this.Tables)
            {
                string tableName = string.Empty;
                if (tableSegment.TableType == TableType.CteSelfRef)
                    tableName = this.OrmProvider.GetTableName(tableSegment.RefTableName);
                else
                {
                    tableName = tableSegment.Body;
                    if (string.IsNullOrEmpty(tableName))
                        tableName = this.OrmProvider.GetTableName(tableSegment.Mapper.TableName);
                }

                if (index > 0)
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

                index++;
            }
            tableSql = builder.ToString();
        }

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
        return builder.ToString();
    }
    public void From(char tableAsStart = 'a', string suffixRawSql = null, params Type[] entityTypes)
    {
        //this.UnionSql = null;
        this.TableAsStart = tableAsStart;
        foreach (var entityType in entityTypes)
        {
            int tableIndex = tableAsStart + this.Tables.Count;
            this.AddTable(new TableSegment
            {
                EntityType = entityType,
                Mapper = this.MapProvider.GetEntityMap(entityType),
                AliasName = $"{(char)tableIndex}",
                SuffixRawSql = suffixRawSql,
                Path = $"{(char)tableIndex}",
                TableType = TableType.Entity,
                IsMaster = true
            });
        }
    }
    public IVisitableQuery From(Type targetType, bool isFirst, IVisitableQuery subQueryObj) => this.From(targetType, isFirst, () => subQueryObj);
    public IVisitableQuery From(Type targetType, bool isFirst, DbContext dbContext, Delegate subQueryGetter)
    {
        return this.From(targetType, isFirst, () =>
        {
            var fromQuery = new FromQuery(dbContext, this);
            return subQueryGetter.DynamicInvoke(fromQuery) as IVisitableQuery;
        });
    }
    protected IVisitableQuery From(Type targetType, bool isFirst, Func<IVisitableQuery> queryObjGetter)
    {
        //子查询使用，原WithTable方法
        this.IsFromQuery = true;
        var subQueryObj = queryObjGetter.Invoke();
        var rawSql = subQueryObj.Visitor.BuildSql(out var readerFields);
        if (!this.Equals(subQueryObj.Visitor))
        {
            subQueryObj.Visitor.CopyTo(this);
            subQueryObj.Visitor.Dispose();
        }
        else if (isFirst) this.Clear();
        this.AddTable(targetType, null, TableType.FromQuery, $"({rawSql})", readerFields);
        this.IsFromQuery = false;
        return subQueryObj;
    }
    public IVisitableQuery FromWith(Type targetType, bool isFirst, IVisitableQuery cteQueryObj)
        => this.FromWith(targetType, true, () => cteQueryObj);

    public IVisitableQuery FromWith(Type targetType, bool isFirst, DbContext dbContext, Delegate cteSubQueryGetter)
    {
        return this.FromWith(targetType, true, () =>
        {
            var fromQuery = new FromQuery(dbContext, this);
            if (isFirst) return cteSubQueryGetter.DynamicInvoke(fromQuery) as IVisitableQuery;

            var parameters = new List<object> { fromQuery };
            parameters.AddRange(this.CteQueries);
            return cteSubQueryGetter.DynamicInvoke(parameters.ToArray()) as IVisitableQuery;
        });
    }
    protected IVisitableQuery FromWith(Type targetType, bool isFirst, Func<IVisitableQuery> cteQueryObjGetter)
    {
        if (isFirst)
        {
            this.CteTables = new();
            this.CteQueries = new();
            this.CteTableSegments = new();
        }
        var cteQueryObj = cteQueryObjGetter();
        var rawSql = cteQueryObj.Visitor.BuildSql(out var readerFields, false, false);
        if (!this.Equals(cteQueryObj.Visitor))
        {
            cteQueryObj.Visitor.CopyTo(this);
            cteQueryObj.Visitor.Dispose();
        }
        else if (isFirst) this.Clear();

        TableSegment tableSegment = null;
        if (this.SelfTableSegment == null)
            tableSegment = this.AddCteTable(targetType, cteQueryObj, rawSql, readerFields);
        tableSegment.Body = this.BuildCteTableSql(tableSegment.RefTableName, rawSql, readerFields);
        //var builder = new StringBuilder();
        //builder.Append("WITH ");
        //if (this.IsRecursive)
        //    builder.Append("RECURSIVE ");   
        this.Tables.Add(tableSegment);
        //this.UnionSql = null;
        return cteQueryObj;
    }
    public void Union(string union, Type targetType, IVisitableQuery subQuery)
    {
        //Union操作时，不使用当前Visitor中的对象生成TableSegment对象，使用子查询中的对象来生成TableSegment对象
        //当前Visitor中的对象只生成SQL
        var rawSql = this.BuildSql(out var readerFields, false, true);
        this.Clear();
        //使用子查询返回的IVisitableQuery对象和当前Visitor中的对象生成SQL+readerFields一起生成TableSegment对象
        //解析第二个UNION子句，不需要AS别名
        this.IsUnion = true;


        this.UseTable(targetType, rawSql, readerFields, subQuery, true);
        rawSql += union + Environment.NewLine + subQuery.Visitor.BuildSql(out _, false, true);

        tableSegment = this.AddTable(targetType, null, TableType.FromQuery, $"({rawSql})", readerFields);
        //如果是union操作，需要重新初始化readerFields的TableSegment属性
        if (isUnion) this.InitFromQueryReaderFields(tableSegment, readerFields);


        if (!this.Equals(subQuery.Visitor))
        {
            subQuery.Visitor.CopyTo(this);
            subQuery.Visitor.Dispose();
        }
        this.UnionSql = rawSql;
        this.IsUnion = false;
    }
    public void Union(string union, Type targetType, DbContext dbContext, Delegate subQueryGetter)
    {
        var visitor = this.CreateQueryVisitor();
        var fromQuery = new FromQuery(dbContext, visitor);
        var subQuery = subQueryGetter.DynamicInvoke(fromQuery) as IVisitableQuery;
        this.Union(union, targetType, subQuery);
    }
    public void UnionRecursive(string union, Type targetType, DbContext dbContext, IVisitableQuery subQueryObj, Delegate selfSubQueryGetter)
    {
        var rawSql = this.BuildSql(out var readerFields, false, true);
        this.Clear();
        //可能会有多个UnionRecursive场景，所以，SelfTableSegment要使用同一个引用对象，用一次就要更新一次body sql，最后FromWith/NextWith的时候，再更新body sql
        if (this.SelfTableSegment == null)
            this.SelfTableSegment = this.AddCteTable(targetType, subQueryObj, rawSql, readerFields);
        var fromQuery = new FromQuery(dbContext, this);
        var parameters = new List<object> { fromQuery };
        parameters.AddRange(this.CteQueries);
        //此时产生的queryObj是一个新的对象，只能用于解析sql，与传进来的queryObj不是同一个对象，舍弃
        var subQuery = selfSubQueryGetter.DynamicInvoke(parameters.ToArray()) as IVisitableQuery;
        rawSql += union + Environment.NewLine + subQuery.Visitor.BuildSql(out _, false, true);
        this.SelfTableSegment.Body = rawSql;
        this.UnionSql = rawSql;
    }
    /// <summary>
    /// join/union操作时，新加一个子查询表关联，这个新子查询表可能是CTE表，也可能不是，为了便于判断，必须传入queryObj对象
    /// </summary>
    /// <param name="targetType"></param>
    /// <param name="rawSql"></param>
    /// <param name="readerFields"></param>
    /// <param name="queryObj"></param>
    /// <param name="isUnion"></param>
    /// <returns></returns>
    public TableSegment UseTable(Type targetType, string rawSql, List<ReaderField> readerFields, object queryObj, bool isUnion)
    {
        TableSegment tableSegment = null;
        //在join/union的时候，右侧的表可能是以前的CTE表，也可能不是，所以每次操作都要判断一下，以前的cte表都保存在CteTableSegments字典中
        //递归UnionRecursive场景不走此方法，单独处理的
        if (this.CteTableSegments != null && this.CteTableSegments.TryGetValue(queryObj, out tableSegment))
        {
            var aliasName = $"{(char)(this.TableAsStart + this.Tables.Count)}";
            this.AddTable(tableSegment = tableSegment.Clone(aliasName));
        }
        else tableSegment = this.AddTable(targetType, null, TableType.FromQuery, $"({rawSql})", readerFields);
        //如果是union操作，需要重新初始化readerFields的TableSegment属性
        if (isUnion) this.InitFromQueryReaderFields(tableSegment, readerFields);
        return tableSegment;
    }
    public void Join(string joinType, Expression joinOn)
        => this.Join(joinType, joinOn, f => this.InitTableAlias(f));
    public void Join(string joinType, Type newEntityType, Expression joinOn)
    {
        this.Join(joinType, joinOn, f =>
        {
            this.From(this.TableAsStart, null, newEntityType);
            return this.InitTableAlias(f);
        });
    }
    public void Join(string joinType, Type newEntityType, IVisitableQuery subQuery, Expression joinOn)
    {
        this.Join(joinType, joinOn, f =>
        {
            var rawSql = subQuery.Visitor.BuildSql(out var readerFields, false);
            if (!this.Equals(subQuery.Visitor))
            {
                subQuery.Visitor.CopyTo(this);
                subQuery.Visitor.Dispose();
            }
            return this.UseTable(newEntityType, rawSql, readerFields, subQuery, false);
        });
    }
    public void Join(string joinType, Type newEntityType, DbContext dbContext, Delegate subQueryGetter, Expression joinOn)
    {
        this.Join(joinType, joinOn, f =>
        {
            var fromQuery = new FromQuery(dbContext, this);
            var subQueryObj = subQueryGetter.DynamicInvoke(fromQuery) as IVisitableQuery;
            var rawSql = subQueryObj.Visitor.BuildSql(out var readerFields, false);
            if (!this.Equals(subQueryObj.Visitor))
            {
                subQueryObj.Visitor.CopyTo(this);
                subQueryObj.Visitor.Dispose();
            }
            return this.UseTable(newEntityType, rawSql, readerFields, subQueryObj, false);
        });
    }
    protected void Join(string joinType, Expression joinOn, Func<LambdaExpression, TableSegment> joinTableSegmentGetter)
    {
        this.IsWhere = true;
        var lambdaExpr = joinOn as LambdaExpression;
        if (!lambdaExpr.Body.GetParameters(out var parameters))
            throw new NotSupportedException("当前Join操作，没有表关联");
        if (parameters.Count != 2)
            throw new NotSupportedException("Join操作，只支持两个表进行关联，但可以多次Join操作");

        var joinTableSegment = joinTableSegmentGetter(lambdaExpr);
        joinTableSegment.JoinType = joinType;
        joinTableSegment.OnExpr = this.VisitConditionExpr(lambdaExpr.Body);
        this.IsWhere = false;
    }
    public virtual void Include(Expression memberSelector, bool isIncludeMany = false, Expression filter = null)
        => this.Include(memberSelector, (a, b) => this.InitTableAlias(a), isIncludeMany, filter);
    public virtual void ThenInclude(Expression memberSelector, bool isIncludeMany = false, Expression filter = null)
    {
        this.Include(memberSelector, (a, b) =>
        {
            this.TableAlias.Clear();
            this.TableAlias.Add(b[0].Name, this.LastIncludeSegment);
        }, isIncludeMany, filter);
    }
    public virtual bool HasIncludeTables() => this.IncludeSegments != null && this.IncludeSegments.Count > 0;
    public virtual bool BuildIncludeSql<TTarget>(Type targetType, TTarget target, out string sql)
    {
        sql = null;
        if (target == null) return false;
        if (this.IncludeSegments == null)
        {
            if (this.deferredRefIncludeValuesSetters == null) return false;
            this.deferredRefIncludeValuesSetters.ForEach(f => f.Invoke(target));
            return false;
        }
        sql = this.BuildIncludeSql(targetType, (builder, sqlInitializer) => sqlInitializer.Invoke(this.OrmProvider, builder, target));
        return true;
    }
    public virtual bool BuildIncludeSql<TTarget>(Type targetType, List<TTarget> targets, out string sql)
    {
        sql = null;
        if (targets == null) return false;
        if (this.IncludeSegments == null)
        {
            if (this.deferredRefIncludeValuesSetters == null) return false;
            targets.ForEach(t => this.deferredRefIncludeValuesSetters.ForEach(f => f.Invoke(t)));
            return false;
        }
        sql = this.BuildIncludeSql(targetType, (builder, sqlInitializer) =>
        {
            int index = 0;
            foreach (var target in targets)
            {
                if (index > 0) builder.Append(',');
                sqlInitializer.Invoke(this.OrmProvider, builder, target);
                index++;
            }
            builder.Append(')');
        });
        return true;
    }
    private string BuildIncludeSql(Type targetType, Action<StringBuilder, Action<IOrmProvider, StringBuilder, object>> sqlBuilderInitializer)
    {
        var builder = new StringBuilder();
        foreach (var includeSegment in this.IncludeSegments)
        {
            (var headSql, Action<IOrmProvider, StringBuilder, object> sqlInitializer) = this.BuildIncludeSqlGetter(targetType, includeSegment);
            builder.Append(headSql);
            sqlBuilderInitializer.Invoke(builder, sqlInitializer);
            if (!string.IsNullOrEmpty(includeSegment.Filter))
                builder.Append($" AND {includeSegment.Filter}");
            builder.Append(';');
        }
        return builder.ToString();
    }
    public void SetIncludeValues<TTarget>(Type targetType, TTarget target, IDataReader reader)
    {
        var deferredInitializers = new List<Action<object>>();
        foreach (var includeSegment in this.IncludeSegments)
        {
            var navigationType = includeSegment.FromMember.NavigationType;
            var includeValues = this.CreateIncludeValues(navigationType);
            while (reader.Read())
                this.AddIncludeValue(navigationType, includeValues, reader);
            deferredInitializers.Add(f => this.SetIncludeValueToTarget(targetType, includeSegment, f, includeValues));
        }
        if (this.deferredRefIncludeValuesSetters != null)
            deferredInitializers.AddRange(this.deferredRefIncludeValuesSetters);
        deferredInitializers.ForEach(t => t.Invoke(target));
        reader.NextResult();
    }
    public async Task SetIncludeValuesAsync<TTarget>(Type targetType, TTarget target, DbDataReader reader, CancellationToken cancellationToken)
    {
        var deferredInitializers = new List<Action<object>>();
        foreach (var includeSegment in this.IncludeSegments)
        {
            var navigationType = includeSegment.FromMember.NavigationType;
            var includeValues = this.CreateIncludeValues(navigationType);
            while (await reader.ReadAsync(cancellationToken))
                this.AddIncludeValue(navigationType, includeValues, reader);
            deferredInitializers.Add(f => this.SetIncludeValueToTarget(targetType, includeSegment, f, includeValues));
        }
        if (this.deferredRefIncludeValuesSetters != null)
            deferredInitializers.AddRange(this.deferredRefIncludeValuesSetters);
        deferredInitializers.ForEach(t => t.Invoke(target));
        await reader.NextResultAsync(cancellationToken);
    }
    public void SetIncludeValues<TTarget>(Type targetType, List<TTarget> targets, IDataReader reader)
    {
        var deferredInitializers = new List<Action<object>>();
        foreach (var includeSegment in this.IncludeSegments)
        {
            var navigationType = includeSegment.FromMember.NavigationType;
            var includeValues = this.CreateIncludeValues(navigationType);
            while (reader.Read())
                this.AddIncludeValue(navigationType, includeValues, reader);
            deferredInitializers.Add(f => this.SetIncludeValueToTarget(targetType, includeSegment, f, includeValues));
        }
        if (this.deferredRefIncludeValuesSetters != null)
            deferredInitializers.AddRange(this.deferredRefIncludeValuesSetters);
        targets.ForEach(f => deferredInitializers.ForEach(t => t.Invoke(f)));
        reader.NextResult();
    }
    public async Task SetIncludeValueAsync<TTarget>(Type targetType, List<TTarget> targets, DbDataReader reader, CancellationToken cancellationToken)
    {
        var deferredInitializers = new List<Action<object>>();
        foreach (var includeSegment in this.IncludeSegments)
        {
            var navigationType = includeSegment.FromMember.NavigationType;
            var includeValues = this.CreateIncludeValues(navigationType);
            while (await reader.ReadAsync(cancellationToken))
                this.AddIncludeValue(navigationType, includeValues, reader);
            deferredInitializers.Add(f => this.SetIncludeValueToTarget(targetType, includeSegment, f, includeValues));
        }
        if (this.deferredRefIncludeValuesSetters != null)
            deferredInitializers.AddRange(this.deferredRefIncludeValuesSetters);
        targets.ForEach(f => deferredInitializers.ForEach(t => t.Invoke(f)));
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
    private void AddIncludeValue(Type elementType, object includeValues, IDataReader reader)
    {
        var typedReaderElementSetter = typedReaderElementSetters.GetOrAdd(elementType, f =>
        {
            var anonObjsExpr = Expression.Parameter(typeof(object), "anonObjs");
            var dbKeyExpr = Expression.Parameter(typeof(string), "dbKey");
            var readerExpr = Expression.Parameter(typeof(IDataReader), "reader");
            var ormProviderExpr = Expression.Parameter(typeof(IOrmProvider), "ormProvider");
            var mapProviderExpr = Expression.Parameter(typeof(IEntityMapProvider), "mapProvider");

            var blockParameters = new List<ParameterExpression>();
            var blockBodies = new List<Expression>();
            var listType = typeof(List<>).MakeGenericType(elementType);
            var typedListExpr = Expression.Variable(listType, "typedList");
            blockParameters.Add(typedListExpr);
            blockBodies.Add(Expression.Assign(typedListExpr, Expression.Convert(anonObjsExpr, listType)));

            var methodInfo = typeof(Extensions).GetMethod(nameof(Extensions.To), new Type[] { typeof(IDataReader), typeof(string), typeof(IOrmProvider), typeof(IEntityMapProvider) });
            var elementExpr = Expression.Call(methodInfo, readerExpr, dbKeyExpr, ormProviderExpr, mapProviderExpr);
            methodInfo = listType.GetMethod("Add", new Type[] { elementType });
            var bodyExpr = Expression.Call(typedListExpr, methodInfo, elementExpr);
            return Expression.Lambda<Action<object, string, IDataReader, IOrmProvider, IEntityMapProvider>>(
                bodyExpr, anonObjsExpr, dbKeyExpr, readerExpr, ormProviderExpr, mapProviderExpr).Compile();
        });
        typedReaderElementSetter.Invoke(includeValues, this.DbKey, reader, this.OrmProvider, this.MapProvider);
    }
    private void SetIncludeValueToTarget(Type targetType, TableSegment includeSegment, object target, object includeValues)
    {
        var cacheKey = this.GetIncludeKey(targetType, includeSegment);
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
            Expression parentExpr = targetExpr;
            var memberName = includeSegment.FromMember.MemberName;
            foreach (var memberInfo in includeSegment.ParentMemberVisits)
            {
                parentExpr = Expression.PropertyOrField(parentExpr, memberInfo.Name);
            }
            var keyMember = includeSegment.FromTable.Mapper.KeyMembers[0];
            var foreignKeyValueExpr = Expression.PropertyOrField(parentExpr, keyMember.MemberName);
            var includeMemberExpr = Expression.PropertyOrField(parentExpr, memberName);

            //var myIncludeValues = includeValues.FindAll(f => f.CompanyId == target.Seller.Company.Id);
            var predicateType = typeof(Predicate<>).MakeGenericType(elementType);
            var parameterExpr = Expression.Parameter(elementType, "f");
            var foreignKey = includeSegment.FromMember.ForeignKey;
            var equalExpr = Expression.Equal(Expression.PropertyOrField(parameterExpr, foreignKey), foreignKeyValueExpr);

            var predicateExpr = Expression.Lambda(predicateType, equalExpr, parameterExpr);
            var methodInfo = listType.GetMethod("FindAll", new Type[] { predicateType });
            var filterValuesExpr = Expression.Call(typedListExpr, methodInfo, predicateExpr);

            //target.Seller.Company.Products = myIncludeValues;
            Expression setValueExpr = null;
            switch (includeSegment.FromMember.Member.MemberType)
            {
                case MemberTypes.Field:
                    setValueExpr = Expression.Assign(Expression.Field(parentExpr, memberName), filterValuesExpr);
                    break;
                case MemberTypes.Property:
                    methodInfo = (includeSegment.FromMember.Member as PropertyInfo).GetSetMethod();
                    setValueExpr = Expression.Call(parentExpr, methodInfo, filterValuesExpr);
                    break;
                default: throw new NotSupportedException("目前只支持Field或是Property两种成员访问");
            }

            //if(includeValues.Count>0)
            //  target.Seller.Company.Products = myIncludeValues;
            var greaterThanExpr = Expression.GreaterThan(Expression.Property(typedListExpr, "Count"), Expression.Constant(0));
            blockBodies.Add(Expression.IfThen(greaterThanExpr, setValueExpr));
            return Expression.Lambda<Action<object, object>>(Expression.Block(blockParameters, blockBodies), targetExpr, anonListExpr).Compile();
        });
        includeValuesSetter.Invoke(target, includeValues);
    }
    protected void Include(Expression memberSelector, Action<LambdaExpression, List<ParameterExpression>> tableAliasInitializer, bool isIncludeMany = false, Expression filter = null)
    {
        if (!string.IsNullOrEmpty(this.WhereSql) || !string.IsNullOrEmpty(this.GroupBySql) || !string.IsNullOrEmpty(this.OrderBySql)
            || string.IsNullOrEmpty(this.UnionSql) && this.ReaderFields != null && this.ReaderFields.Count > 0)
            throw new NotSupportedException("Include/ThenInclude操作必须要在Where/And/GroupBy/OrderBy/Select等操作之前完成，紧跟From/Join等操作之后");

        var lambdaExpr = memberSelector as LambdaExpression;
        var memberExpr = lambdaExpr.Body as MemberExpression;
        lambdaExpr.Body.GetParameters(out var parameters);
        tableAliasInitializer.Invoke(lambdaExpr, parameters);
        var includeSegment = this.AddIncludeTables(memberExpr);
        //TODO: 1:N关联条件的alias表，获取会有问题，待测试
        if (filter != null)
        {
            var filterLambdaExpr = filter as LambdaExpression;
            var parameterName = filterLambdaExpr.Parameters[0].Name;
            this.TableAlias.Clear();
            this.TableAlias.Add(parameterName, includeSegment);
            includeSegment.Filter = this.Visit(new SqlSegment { Expression = filter }).ToString();
        }
        this.LastIncludeSegment = includeSegment;
    }
    protected TableSegment AddIncludeTables(MemberExpression memberExpr)
    {
        TableSegment tableSegment = null;
        var memberType = memberExpr.Member.GetMemberType();
        if (!memberType.IsEntityType(out _))
            throw new NotSupportedException($"Include方法只支持实体属性，{memberExpr.Member.DeclaringType.FullName}.{memberExpr.Member.Name}不是实体，Path:{memberExpr}");

        //支持N级成员访问，如：.Include((x, y) => x.Seller.Company.Products)
        //TODO:IncludeMany后的ThenInclude未实现
        //.IncludeMany(x => x.Orders).ThenInclude(x => x.Buyer)
        var memberExprs = this.GetMemberExprs(memberExpr, out var parameterExpr);
        var fromSegment = this.TableAlias[parameterExpr.Name];
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
                builder.Append("." + memberExpr.Member.Name);
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
                builder.Append("." + memberExpr.Member.Name);
                this.IncludeSegments ??= new();
                this.IncludeSegments.Add(tableSegment = new TableSegment
                {
                    TableType = TableType.Include,
                    FromTable = fromSegment,
                    Mapper = entityMapper,
                    FromMember = memberMapper,
                    Path = builder.ToString(),
                    ParentMemberVisits = memberVisits
                });
            }
            fromSegment = tableSegment;
            fromType = memberMapper.NavigationType;
        }
        return tableSegment;
    }
    private (string, Action<IOrmProvider, StringBuilder, object>) BuildIncludeSqlGetter(Type targetType, TableSegment includeSegment)
    {
        var cacheKey = this.GetIncludeKey(targetType, includeSegment);
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

            Expression parentExpr = typedTargetExpr;
            //target.Order.Seller.Company.Products
            foreach (var memberInfo in includeSegment.ParentMemberVisits)
            {
                parentExpr = Expression.PropertyOrField(parentExpr, memberInfo.Name);
            }
            var foreignKeyMember = includeSegment.FromTable.Mapper.KeyMembers[0];
            Expression foreignKeyValueExpr = Expression.PropertyOrField(parentExpr, foreignKeyMember.MemberName);

            foreignKeyValueExpr = Expression.Convert(foreignKeyValueExpr, typeof(object));
            var methedInfo = typeof(IOrmProvider).GetMethod(nameof(IOrmProvider.GetQuotedValue));
            var fieldTypeExpr = Expression.Constant(foreignKeyMember.MemberType);
            foreignKeyValueExpr = Expression.Call(ormProviderExpr, methedInfo, fieldTypeExpr, parentExpr);
            methedInfo = typeof(StringBuilder).GetMethod(nameof(StringBuilder.Append), new Type[] { typeof(string) });
            blockBodies.Add(Expression.Call(builderExpr, methedInfo, foreignKeyValueExpr));

            var foreignKey = includeSegment.FromMember.ForeignKey;
            var fields = RepositoryHelper.BuildFieldsSqlPart(this.OrmProvider, this.MapProvider, includeSegment.EntityType, includeSegment.EntityType, true);
            var tableName = this.OrmProvider.GetTableName(includeSegment.Mapper.TableName);
            var headSql = $" SELECT {fields} FROM {tableName} WHERE {foreignKey} IN (";

            var sqlInitializer = Expression.Lambda<Action<IOrmProvider, StringBuilder, object>>(Expression.Block(blockParameters, blockBodies), ormProviderExpr, builderExpr, targetExpr).Compile();
            return (headSql, sqlInitializer);
        });
    }

    public virtual void Where(Expression whereExpr, bool isClearTableAlias = true)
    {
        this.IsWhere = true;
        var lambdaExpr = whereExpr as LambdaExpression;
        if (isClearTableAlias)
            this.InitTableAlias(lambdaExpr);
        this.LastWhereNodeType = OperationType.None;
        this.WhereSql = this.VisitConditionExpr(lambdaExpr.Body);
        this.IsWhere = false;
    }
    public virtual void And(Expression whereExpr)
    {
        this.IsWhere = true;
        var lambdaExpr = whereExpr as LambdaExpression;
        this.InitTableAlias(lambdaExpr);
        if (this.LastWhereNodeType == OperationType.Or)
        {
            this.WhereSql = $"({this.WhereSql})";
            this.LastWhereNodeType = OperationType.And;
        }
        var conditionSql = this.VisitConditionExpr(lambdaExpr.Body);
        if (this.LastWhereNodeType == OperationType.Or)
        {
            conditionSql = $"({conditionSql})";
            this.LastWhereNodeType = OperationType.And;
        }
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
                    var sqlSegment = this.VisitAndDeferred(new SqlSegment { Expression = argumentExpr });
                    if (builder.Length > 0)
                        builder.Append(',');

                    var fieldName = sqlSegment.Value.ToString();
                    builder.Append(fieldName);
                    //此时，字段不能加别名，后面有可能还会有OrderBy操作，到最外层Select的时候用到Grouping时，再加别名
                    this.GroupFields.Add(new ReaderField
                    {
                        FieldType = ReaderFieldType.Field,
                        TableSegment = sqlSegment.TableSegment,
                        FromMember = sqlSegment.FromMember,
                        MemberMapper = sqlSegment.MemberMapper,
                        TargetMember = memberInfo,
                        Body = fieldName
                    });
                    index++;
                }
                //GroupBy SQL中不含别名
                this.GroupBySql = builder.ToString();
                break;
            case ExpressionType.MemberAccess:
                {
                    var memberExpr = lambdaExpr.Body as MemberExpression;
                    var sqlSegment = this.VisitAndDeferred(new SqlSegment { Expression = memberExpr });
                    var fieldName = sqlSegment.Value.ToString();
                    //此时，字段不能加别名，后面有可能还会有OrderBy操作，到最外层Select的时候用到Grouping时，再加别名
                    this.GroupFields.Add(new ReaderField
                    {
                        FieldType = ReaderFieldType.Field,
                        TableSegment = sqlSegment.TableSegment,
                        FromMember = sqlSegment.FromMember,
                        MemberMapper = sqlSegment.MemberMapper,
                        TargetMember = memberExpr.Member,
                        Body = fieldName
                    });
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

        var builder = new StringBuilder();
        if (!string.IsNullOrEmpty(this.OrderBySql))
            builder.Append(this.OrderBySql + ",");

        //能够访问Grouping属性的场景，通常是在最外层的Select子句或是OrderBy子句
        //此处特殊处理
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
                        var sqlSegment = this.VisitAndDeferred(new SqlSegment { Expression = argumentExpr });
                        if (builder.Length > 0)
                            builder.Append(',');

                        builder.Append(sqlSegment.Value.ToString());
                        if (orderType == "DESC")
                            builder.Append(" DESC");
                        index++;
                    }
                    this.GroupBySql = builder.ToString();
                    break;
                case ExpressionType.MemberAccess:
                    {
                        if (this.IsGroupingMember(lambdaExpr.Body as MemberExpression))
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
                        var sqlSegment = this.VisitAndDeferred(new SqlSegment { Expression = lambdaExpr.Body });
                        builder.Append(sqlSegment.Value.ToString());
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
        this.HavingSql = this.VisitConditionExpr(lambdaExpr.Body);
        this.IsWhere = false;
    }

    public virtual void SelectGrouping() => this.ReaderFields = this.GroupFields;
    public virtual void SelectDefault(Expression defaultExpr)
    {
        if (this.ReaderFields == null || this.ReaderFields.Count == 0)
            this.Select(null, defaultExpr);
    }
    public virtual void Select(string sqlFormat, Expression selectExpr = null, bool isFromCommand = false)
    {
        this.IsSelect = true;
        this.IsFromCommand = isFromCommand;
        if (selectExpr != null)
        {
            var lambdaExpr = selectExpr as LambdaExpression;
            this.InitTableAlias(lambdaExpr);
            this.SelectToReaderFields(lambdaExpr);
        }
        if (!string.IsNullOrEmpty(sqlFormat))
        {
            //单值操作，SELECT COUNT(DISTINCT b.Id),MAX(b.Amount)等
            if (this.ReaderFields != null && this.ReaderFields.Count == 1)
            {
                var readerField = this.ReaderFields[0];
                readerField.Body = string.Format(sqlFormat, readerField.Body);
            }
            else
            {
                //单值操作，SELECT COUNT(1)等,或是From临时表，后续Union/Join操作
                this.ReaderFields ??= new();
                this.ReaderFields.Add(new ReaderField { Body = sqlFormat });
            }
        }
        this.IsFromCommand = false;
        this.IsSelect = false;
    }
    public virtual void SelectFlattenTo(Type targetType, Expression specialMemberSelector = null)
    {
        this.IsSelect = true;
        if (specialMemberSelector != null)
        {
            var lambdaExpr = specialMemberSelector as LambdaExpression;
            var sqlSegment = this.VisitMemberInit(new SqlSegment { Expression = lambdaExpr.Body });
            this.ReaderFields = sqlSegment.Value as List<ReaderField>;
            var existsMembers = this.ReaderFields.Select(f => f.TargetMember.Name).ToList();

            var targetMembers = targetType.GetMembers(BindingFlags.Public | BindingFlags.Instance)
                .Where(f => f.MemberType == MemberTypes.Property | f.MemberType == MemberTypes.Field).ToList();

            foreach (var memberInfo in targetMembers)
            {
                if (existsMembers.Contains(memberInfo.Name)) continue;
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
    public void SelectToReaderFields(LambdaExpression toTargetExpr)
    {
        var sqlSegment = new SqlSegment { Expression = toTargetExpr.Body };
        switch (toTargetExpr.Body.NodeType)
        {
            case ExpressionType.MemberAccess:
                var memberExpr = toTargetExpr.Body as MemberExpression;
                sqlSegment = this.VisitMemberAccess(sqlSegment);
                //实体类型成员访问，只有两种场景：主表的实体成员(Include子表访问)或是Grouping分组对象访问
                if (sqlSegment.MemberType == ReaderFieldType.Entity)
                    this.ReaderFields = sqlSegment.Value as List<ReaderField>;
                else
                {
                    //一定有成员成名
                    this.ReaderFields = new List<ReaderField>
                        {
                            new ReaderField
                            {
                                FieldType = ReaderFieldType.Field ,
                                TableSegment = sqlSegment.TableSegment,
                                FromMember = sqlSegment.FromMember,
                                MemberMapper = sqlSegment.MemberMapper ,
                                TargetMember =sqlSegment.FromMember,
                                Body = sqlSegment.Value.ToString()
                            }
                        };
                }
                break;
            case ExpressionType.New:
                sqlSegment = this.VisitNew(sqlSegment);
                this.ReaderFields = sqlSegment.Value as List<ReaderField>;
                break;
            case ExpressionType.MemberInit:
                sqlSegment = this.VisitMemberInit(sqlSegment);
                this.ReaderFields = sqlSegment.Value as List<ReaderField>;
                break;
            case ExpressionType.Parameter:
                sqlSegment = this.VisitParameter(sqlSegment);
                this.ReaderFields = sqlSegment.Value as List<ReaderField>;
                this.ReaderFields[0].IsTargetType = true;
                break;
            default:
                //单个字段，有表达式计算二元操作或是有方法调用的场景
                if (toTargetExpr.Body.NodeType == ExpressionType.Call)
                    sqlSegment.OriginalExpression = toTargetExpr;
                sqlSegment = this.VisitAndDeferred(sqlSegment);
                if (sqlSegment.Value is List<ReaderField> readerFields)
                    this.ReaderFields = readerFields;
                else
                {
                    //单个值，单个字段访问或是有表达式访问或是函数调用等，不一定有成员名称，如：.Select(f => f.Age / 10 * 10)
                    this.ReaderFields = new List<ReaderField>
                    {
                        new ReaderField
                        {
                            //可能字段组成来源多个表，不同字段运算或是函数调用，无需设置TableSegment/FromMember/TargetMember
                            FieldType = sqlSegment.MemberType,
                            Body = sqlSegment.Value.ToString()
                        }
                    };
                }
                break;
        }
    }
    public bool TryFindReaderField(MemberInfo memberInfo, out ReaderField readerField)
    {
        foreach (var tableSegment in this.Tables)
        {
            if (this.TryFindReaderField(tableSegment, memberInfo, out readerField))
                return true;
        }
        readerField = null;
        return false;
    }
    public bool TryFindReaderField(TableSegment tableSegment, MemberInfo memberInfo, out ReaderField readerField)
    {
        readerField = null;
        if (tableSegment.ReaderFields != null)
        {
            readerField = tableSegment.ReaderFields.Find(f => f.FromMember.Name == memberInfo.Name);
            if (readerField == null) return false;
            readerField.TargetMember = memberInfo;
        }
        else
        {
            if (!tableSegment.Mapper.TryGetMemberMap(memberInfo.Name, out var memberMapper))
                return false;
            readerField = new ReaderField
            {
                FieldType = ReaderFieldType.Field,
                MemberMapper = memberMapper,
                FromMember = memberMapper.Member,
                TargetMember = memberInfo,
                TableSegment = tableSegment,
                Body = tableSegment.AliasName + "." + this.OrmProvider.GetFieldName(memberMapper.FieldName)
            };
        }
        return true;
    }
    public virtual void Distinct() => this.IsDistinct = true;
    public virtual void Page(int pageIndex, int pageSize)
    {
        if (pageIndex > 0) pageIndex--;
        this.skip = pageIndex * pageSize;
        this.limit = pageSize;
    }
    public virtual void Skip(int skip) => this.skip = skip;
    public virtual void Take(int limit) => this.limit = limit;

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
                return formatter.Invoke(this, targetSegment);
            }

            if (this.IsGroupingMember(memberExpr))
            {
                List<ReaderField> groupFields = null;
                if (this.IsFromQuery) groupFields = new();
                foreach (var readerField in this.GroupFields)
                {
                    //子查询中的字段别名要带有本地化包装
                    //TODO:尝试在最后BuildSql时，增加别名，可以判断是否需要增加别名
                    //if (readerField.TargetMember != null && readerField.FromMember.Name != readerField.TargetMember.Name)
                    //    readerField.Body += " AS " + this.OrmProvider.GetFieldName(readerField.TargetMember.Name);
                    if (this.IsFromQuery) groupFields.Add(readerField);
                }
                //在子查询中，Select了Group分组对象，为了避免在Clear时，把GroupFields元素清掉，放到一个新列表中
                if (!this.IsFromQuery) groupFields = this.GroupFields;
                //TODO:此处暂时先不设置body,最后BuildSql时再设置

                return sqlSegment.Change(new ReaderField
                {
                    FieldType = ReaderFieldType.Entity,
                    FromMember = memberInfo,
                    TargetMember = memberInfo,
                    ReaderFields = groupFields
                    //TODO:Body暂时先不设置，最后BuildSql时再设置
                    //Body = body
                });
            }

            if (memberExpr.IsParameter(out var parameterName))
            {
                string path = null;
                TableSegment fromSegment = null;

                var rootTableSegment = this.TableAlias[parameterName];
                if (rootTableSegment.TableType == TableType.Entity)
                {
                    var builder = new StringBuilder(rootTableSegment.AliasName);
                    var memberExprs = this.GetMemberExprs(memberExpr, out var parameterExpr);
                    if (memberExprs.Count > 0)
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

                    MemberMap memberMapper = null;
                    if (fromSegment.Mapper != null)
                    {
                        //非子查询场景
                        memberMapper = fromSegment.Mapper.GetMemberMap(memberExpr.Member.Name);
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

                            var includeSegment = this.IncludeSegments.Find(f => f.Path == path);
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

                            sqlSegment.Value = new ReaderField
                            {
                                IsRef = true,//需要在构建实体的时候做处理
                                FieldType = ReaderFieldType.Entity,
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
                            if (this.IsNeedAlias) fieldName = fromSegment.AliasName + "." + fieldName;
                            if (this.IsSelect)
                            {
                                sqlSegment.Value = new ReaderField
                                {
                                    FieldType = ReaderFieldType.Field,
                                    TableSegment = fromSegment,
                                    FromMember = memberMapper.Member,
                                    MemberMapper = memberMapper,
                                    TargetMember = memberInfo,
                                    Body = fieldName
                                };
                            }
                            else
                            {
                                sqlSegment.TableSegment = fromSegment;
                                sqlSegment.FromMember = memberMapper.Member;
                                sqlSegment.MemberMapper = memberMapper;
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
                        if (this.IsSelect)
                            sqlSegment.Value = readerField;
                        else
                        {
                            //子查询中的Json实体类型字段,FieldType: Field                           
                            sqlSegment.TableSegment = fromSegment;
                            sqlSegment.FromMember = readerField.TargetMember;
                            sqlSegment.MemberMapper = readerField.MemberMapper;
                            sqlSegment.Value = readerField.Body;
                        }
                    }
                }
                else
                {
                    //Where(f => f.Amount > 5)
                    //Select(f => new { f.OrderId, f.Disputes ...})                    
                    MemberMap memberMapper = null;
                    string fieldName = null;
                    sqlSegment.HasField = true;

                    if (fromSegment.Mapper != null)
                    {
                        memberMapper = fromSegment.Mapper.GetMemberMap(memberExpr.Member.Name);
                        if (memberMapper.IsIgnore)
                            throw new Exception($"类{fromSegment.EntityType.FullName}的成员{memberMapper.MemberName}是忽略成员无法访问");
                        if (memberMapper.MemberType.IsEntityType(out _) && !memberMapper.IsNavigation && memberMapper.TypeHandler == null)
                            throw new Exception($"类{fromSegment.EntityType.FullName}的成员{memberExpr.Member.Name}不是值类型，未配置为导航属性也没有配置TypeHandler");

                        sqlSegment.FromMember = memberMapper.Member;
                        sqlSegment.MemberMapper = memberMapper;
                        //查询时，IsNeedAlias始终为true，新增、更新、删除时，引用联表操作时，才会为true
                        fieldName = this.OrmProvider.GetFieldName(memberMapper.FieldName);
                        if (this.IsNeedAlias) fieldName = fromSegment.AliasName + "." + fieldName;
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
                        else readerField = fromSegment.ReaderFields.Find(f => f.TargetMember.Name == memberExpr.Member.Name);

                        memberMapper = readerField.MemberMapper;
                        sqlSegment.FromMember = readerField.TargetMember;
                        sqlSegment.MemberMapper = memberMapper;
                        fieldName = this.OrmProvider.GetFieldName(memberExpr.Member.Name);
                        if (this.IsNeedAlias) fieldName = fromSegment.AliasName + "." + fieldName;
                        sqlSegment.Value = fieldName;
                    }

                    //.NET枚举类型总是解析成对应的UnderlyingType数值类型，如：a.Gender ?? Gender.Male == Gender.Male
                    //如果枚举类型对应的数据库类型是字符串就会有问题，需要把数字变为枚举，再把枚举的名字字符串完成后续操作。
                    if (memberMapper != null && memberMapper.MemberType.IsEnumType(out var expectType, out _) && memberMapper.DbDefaultType == typeof(string))
                    {
                        sqlSegment.ExpectType = expectType;
                        sqlSegment.TargetType = memberMapper.DbDefaultType;
                    }
                }
                return sqlSegment;
            }
        }

        if (memberExpr.Member.DeclaringType == typeof(DBNull))
            return SqlSegment.Null;

        //各种静态成员访问，如：DateTime.Now,int.MaxValue,string.Empty
        if (this.OrmProvider.TryGetMemberAccessSqlFormatter(memberExpr, out formatter))
            return formatter.Invoke(this, sqlSegment);

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
    public override SqlSegment VisitNew(SqlSegment sqlSegment)
    {
        var newExpr = sqlSegment.Expression as NewExpression;
        //Select场景
        if (newExpr.Type.Name.StartsWith("<>"))
        {
            var readerFields = new List<ReaderField>();
            for (int i = 0; i < newExpr.Arguments.Count; i++)
            {
                this.AddSelectElement(newExpr.Arguments[i], newExpr.Members[i], readerFields);
            }
            return sqlSegment.Change(readerFields);
        }
        return this.Evaluate(sqlSegment);
    }
    public override SqlSegment VisitMemberInit(SqlSegment sqlSegment)
    {
        var memberInitExpr = sqlSegment.Expression as MemberInitExpression;
        //Select场景
        var readerFields = new List<ReaderField>();
        for (int i = 0; i < memberInitExpr.Bindings.Count; i++)
        {
            if (memberInitExpr.Bindings[i].BindingType != MemberBindingType.Assignment)
                throw new NotSupportedException("暂时不支持除MemberBindingType.Assignment类型外的成员绑定表达式");
            var memberAssignment = memberInitExpr.Bindings[i] as MemberAssignment;
            this.AddSelectElement(memberAssignment.Expression, memberAssignment.Member, readerFields);
        }
        return sqlSegment.Change(readerFields);
    }
    public virtual void AddSelectElement(Expression elementExpr, MemberInfo memberInfo, List<ReaderField> readerFields)
    {
        this.IsSelectMember = true;
        string fieldName = null;
        bool isNeedAlias = false;
        var sqlSegment = new SqlSegment { Expression = elementExpr };
        switch (elementExpr.NodeType)
        {
            case ExpressionType.Parameter:
                if (this.IsFromQuery)
                    throw new NotSupportedException("FROM子查询中不支持参数Parameter表达式访问，只支持基础字段访问访问");
                //两种场景：.Select((x, y) => new { Order = x, x.Seller, x.Buyer, ... }) 和 .Select((x, y) => x)，可能有include操作
                sqlSegment = this.VisitParameter(sqlSegment);
                var tableReaderFields = sqlSegment.Value as List<ReaderField>;
                tableReaderFields[0].FromMember = memberInfo;
                tableReaderFields[0].TargetMember = memberInfo;
                readerFields.AddRange(tableReaderFields);
                break;
            case ExpressionType.New:
            case ExpressionType.MemberInit:
                //为了简化SELECT操作，只支持一次New/MemberInit表达式操作
                throw new NotSupportedException("不支持的表达式访问，SELECT语句只支持一次New/MemberInit表达式访问操作");
            case ExpressionType.MemberAccess:
                sqlSegment = this.VisitMemberAccess(sqlSegment);
                if (sqlSegment.Value is ReaderField entityReaderField)
                    readerFields.Add(entityReaderField);
                else
                {
                    fieldName = this.GetQuotedValue(sqlSegment);
                    if (sqlSegment.IsExpression)
                        fieldName = $"({fieldName})";
                    isNeedAlias = sqlSegment.IsConstant || sqlSegment.HasParameter || sqlSegment.IsExpression
                        || sqlSegment.IsMethodCall || sqlSegment.FromMember == null || sqlSegment.FromMember.Name != memberInfo.Name;
                    if (isNeedAlias && (!this.IsUnion || !this.IsFromCommand))
                        fieldName += " AS " + this.OrmProvider.GetFieldName(memberInfo.Name);

                    readerFields.Add(new ReaderField
                    {
                        FieldType = ReaderFieldType.Field,
                        TableSegment = sqlSegment.TableSegment,
                        FromMember = sqlSegment.FromMember,
                        MemberMapper = sqlSegment.MemberMapper,
                        TargetMember = memberInfo,
                        Body = fieldName
                    });
                }
                break;
            default:
                //常量或方法或表达式访问
                sqlSegment = this.VisitAndDeferred(sqlSegment);
                //使用GetQuotedValue方法把常量都变成对应的字符串格式
                //String和DateTime类型变成'...'数据,数字类型变成数字字符串
                //DeferredFields场景
                if (sqlSegment.Value is ReaderField methodCallField)
                {
                    //函数调用，参数引用多个字段
                    //.SelectFlattenTo<DTO>((a, b ...) => new DTO
                    //{
                    //    ActivityTypeEnum = this.GetEmnuName(f.ActivityType)
                    //})
                    methodCallField.FromMember = memberInfo;
                    methodCallField.TargetMember = memberInfo;
                    methodCallField.IsNeedAlias = true;
                    //if (methodCallField.FieldType == ReaderFieldType.DeferredFields && methodCallField.ReaderFields == null)
                    //    methodCallField.Body += " AS " + this.OrmProvider.GetFieldName(memberInfo.Name);
                    readerFields.Add(methodCallField);
                }
                else
                {
                    fieldName = this.GetQuotedValue(sqlSegment);
                    if (sqlSegment.IsExpression)
                        fieldName = $"({fieldName})";
                    isNeedAlias = sqlSegment.IsConstant || sqlSegment.HasParameter || sqlSegment.IsExpression
                        || sqlSegment.IsMethodCall || sqlSegment.FromMember == null || sqlSegment.FromMember.Name != memberInfo.Name;
                    if (isNeedAlias && (!this.IsUnion || !this.IsFromCommand))
                        fieldName += " AS " + this.OrmProvider.GetFieldName(memberInfo.Name);
                    readerFields.Add(new ReaderField
                    {
                        FieldType = ReaderFieldType.Field,
                        TableSegment = sqlSegment.TableSegment,
                        FromMember = memberInfo,
                        TargetMember = memberInfo,
                        Body = fieldName
                    });
                }
                break;
        }
        this.IsSelectMember = false;
    }
    public virtual TableSegment AddTable(TableSegment tableSegment)
    {
        //Union后，有加新表，要把前一个UnionSql设置完整
        if (this.UnionSql != null)
        {
            //有union操作的visitor，都是新New的，前面只有一个表
            this.Tables[0].Body = this.UnionSql;
            this.UnionSql = null;
        }
        this.Tables.Add(tableSegment);
        if (this.Tables.Count == 2 && this.Tables[0].ReaderFields != null)
            this.InitFromQueryReaderFields(this.Tables[0], this.Tables[0].ReaderFields);
        if (this.Tables.Count > 2 && tableSegment.ReaderFields != null)
            this.InitFromQueryReaderFields(tableSegment, tableSegment.ReaderFields);
        return tableSegment;
    }
    public virtual TableSegment AddTable(Type entityType, string joinType = "", TableType tableType = TableType.Entity, string body = null, List<ReaderField> readerFields = null)
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
            ReaderFields = readerFields,
            IsMaster = true
        });
    }
    public virtual TableSegment InitTableAlias(LambdaExpression lambdaExpr)
    {
        TableSegment tableSegment = null;
        this.TableAlias.Clear();
        lambdaExpr.Body.GetParameterNames(out var parameterNames);
        if ((parameterNames == null || parameterNames.Count <= 0))
            return tableSegment;
        var masterTables = this.Tables.FindAll(f => f.IsMaster);
        int index = 0;
        foreach (var parameterExpr in lambdaExpr.Parameters)
        {
            if (typeof(IAggregateSelect).IsAssignableFrom(parameterExpr.Type))
                continue;
            if (typeof(IFromQuery).IsAssignableFrom(parameterExpr.Type))
                continue;
            if (!parameterNames.Contains(parameterExpr.Name))
            {
                index++;
                continue;
            }
            this.TableAlias.Add(parameterExpr.Name, masterTables[index]);
            tableSegment = masterTables[index];
            index++;
        }
        return tableSegment;
    }
    public virtual IQueryVisitor CreateQueryVisitor(bool isNewCteQuery = false)
    {
        var queryVisiter = this.OrmProvider.NewQueryVisitor(this.DbKey, this.MapProvider, this.IsParameterized, this.TableAsStart, this.ParameterPrefix, this.DbParameters);
        queryVisiter.IsMultiple = this.IsMultiple;
        queryVisiter.CommandIndex = this.CommandIndex;
        if (isNewCteQuery)
        {
            queryVisiter.CteTables = new();
            queryVisiter.CteQueries = new();
            queryVisiter.CteTableSegments = new();
        }
        else
        {
            queryVisiter.CteTables = this.CteTables;
            queryVisiter.CteQueries = this.CteQueries;
            queryVisiter.CteTableSegments = this.CteTableSegments;
        }
        return queryVisiter;
    }
    public void Clear(bool isClearReaderFields = false)
    {
        this.Tables.Clear();
        if (isClearReaderFields)
            this.ReaderFields?.Clear();
        this.WhereSql = null;
        this.TableAsStart = 'a';

        this.skip = null;
        this.limit = null;
        this.UnionSql = null;
        this.GroupBySql = null;
        this.HavingSql = null;
        this.OrderBySql = null;
        this.IsDistinct = false;
        this.IncludeSegments?.Clear();
        this.LastIncludeSegment = null;
        this.GroupFields?.Clear();
    }
    public void CopyTo(IQueryVisitor visitor)
    {
        if (this.DbParameters != null && this.DbParameters.Count > 0)
        {
            if (visitor.DbParameters == null || visitor.DbParameters.Count == 0)
                visitor.DbParameters = this.DbParameters;
            else
            {
                foreach (var dbParameter in this.DbParameters)
                    visitor.DbParameters.Add(dbParameter);
            }
        }
        if (this.CteTables == null || this.CteTables.Count == 0)
            return;

        visitor.CteTables.AddRange(this.CteTables);
        visitor.CteQueries.AddRange(this.CteQueries);
        foreach (var queryTableSegment in this.CteTableSegments)
            visitor.CteTableSegments.TryAdd(queryTableSegment.Key, queryTableSegment.Value);
    }
    public override void Dispose()
    {
        base.Dispose();

        this.UnionSql = null;
        this.GroupBySql = null;
        this.HavingSql = null;
        this.OrderBySql = null;

        this.IncludeSegments = null;
        this.LastIncludeSegment = null;

        this.CteTables = null;
        this.CteQueries = null;
        this.CteTableSegments = null;
        this.SelfTableSegment = null;
    }
    protected void InitFromQueryReaderFields(TableSegment tableSegment, List<ReaderField> readerFields, bool isNeedChange = true)
    {
        if (readerFields == null || readerFields.Count == 0)
            return;

        foreach (var readerField in readerFields)
        {
            //子查询中，访问了实体类对象，比如：Grouping分组对象
            if (readerField.FieldType == ReaderFieldType.Entity)
            {
                readerField.TableSegment = tableSegment;
                //实体类型字段的ReaderFields中FromMember、Body不需要变更，一变更就错了
                this.InitFromQueryReaderFields(tableSegment, readerField.ReaderFields, false);
                if (string.IsNullOrEmpty(readerField.Body))
                {
                    readerField.Body = string.Empty;
                    for (int i = 0; i < readerField.ReaderFields.Count; i++)
                    {
                        if (i > 0) readerField.Body += ",";
                        readerField.Body += tableSegment.AliasName + "." + this.OrmProvider.GetFieldName(readerField.ReaderFields[i].TargetMember.Name);
                    }
                }
            }
            else
            {
                readerField.TableSegment = tableSegment;
                //已经变成子查询了，原表字段名已经没意义了，直接变成新的字段名
                if (isNeedChange)
                {
                    if (readerField.TargetMember != null)
                        readerField.FromMember = readerField.TargetMember;
                    //重新设置body内容，表别名变更
                    readerField.Body = tableSegment.AliasName + "." + this.OrmProvider.GetFieldName(readerField.FromMember.Name);
                    //TODO:
                    //if (readerField.FromMember.Name != readerField.TargetMember.Name)
                    //    readerField.Body += " AS " + readerField.TargetMember.Name;
                }
            }
        }
    }
    public Stack<MemberExpression> GetMemberExprs(MemberExpression memberExpr, out ParameterExpression parameterExpr)
    {
        var currentExpr = memberExpr.Expression;
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
    private TableSegment AddCteTable(Type targetType, IVisitableQuery cteQueryObj, string rawSql, List<ReaderField> readerFields)
    {
        var tableSegment = this.AddTable(targetType, null, TableType.CteSelfRef, rawSql, readerFields);
        this.InitFromQueryReaderFields(tableSegment, readerFields);
        //添加CTE表对象并设置表名，后面Union操作需要使用
        this.CteQueries.Add(cteQueryObj);
        this.CteTableSegments.Add(cteQueryObj, tableSegment);
        this.CteTables.Add(tableSegment);
        tableSegment.RefTableName = $"MyCte{this.CteTables.Count + 1}";
        return tableSegment;
    }
    private string BuildCteTableSql(string cteTableName, string rawSql, List<ReaderField> readerFields)
    {
        var builder = new StringBuilder($"{cteTableName}(");
        int index = 0;
        foreach (var readerField in readerFields)
        {
            if (index > 0) builder.Append(',');
            builder.Append(readerField.TargetMember.Name);
            index++;
        }
        builder.AppendLine(") AS ");
        builder.AppendLine("(");
        builder.AppendLine(rawSql);
        builder.Append(')');
        return builder.ToString();
    }
    private int GetIncludeKey(Type targetType, TableSegment includeSegment)
    {
        var hashCode = new HashCode();
        hashCode.Add(this.DbKey);
        hashCode.Add(this.OrmProvider);
        hashCode.Add(targetType);
        hashCode.Add(includeSegment.ParentMemberVisits.Count);
        foreach (var memberInfo in includeSegment.ParentMemberVisits)
        {
            hashCode.Add(memberInfo.DeclaringType);
            hashCode.Add(memberInfo.Name);
        }
        hashCode.Add(includeSegment.FromMember.MemberName);
        return hashCode.ToHashCode();
    }
    private int GetRefIncludeKey(Type targetType, string refPath)
    {
        var hashCode = new HashCode();
        hashCode.Add(this.DbKey);
        hashCode.Add(this.OrmProvider);
        hashCode.Add(targetType);
        hashCode.Add(refPath);
        return hashCode.ToHashCode();
    }
}
