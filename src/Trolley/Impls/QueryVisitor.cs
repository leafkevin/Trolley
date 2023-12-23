using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
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
    private int? skip;
    private int? limit;

    protected string UnionSql = string.Empty;
    protected string GroupBySql { get; set; } = string.Empty;
    protected string HavingSql { get; set; } = string.Empty;
    protected string OrderBySql { get; set; } = string.Empty;
    protected bool IsDistinct { get; set; }

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
    }
    public virtual string BuildSql(out List<ReaderField> readerFields, bool hasCteSql = true, bool isUnion = false)
    {
        var builder = new StringBuilder();
        if (this.CteQueries != null)
        {
            //TODO:组织CTE SQL
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
        this.AddSelectSqlTo(builder, this.ReaderFields);

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
    protected void AddSelectSqlTo(StringBuilder builder, List<ReaderField> readerFields)
    {
        foreach (var readerField in readerFields)
        {
            if (readerField.FieldType == ReaderFieldType.Entity)
                this.AddSelectSqlTo(builder, readerField.ReaderFields);
            else
            {
                if (builder.Length > 0)
                    builder.Append(',');
                builder.Append(readerField.Body);
            }
        }
    }
    public void From(char tableAsStart = 'a', string suffixRawSql = null, params Type[] entityTypes)
    {
        this.UnionSql = null;
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
        this.UnionSql = null;
        return cteQueryObj;
    }
    public void Union(string union, Type targetType, IVisitableQuery subQuery)
    {
        //Union操作时，不使用当前Visitor中的对象生成TableSegment对象，使用子查询中的对象来生成TableSegment对象
        //当前Visitor中的对象只生成SQL
        var rawSql = this.BuildSql(out var readerFields, false, true);
        this.Clear();
        //使用子查询返回的IVisitableQuery对象和当前Visitor中的对象生成SQL+readerFields一起生成TableSegment对象
        this.UseTable(targetType, rawSql, readerFields, subQuery, true);
        rawSql += union + Environment.NewLine + subQuery.Visitor.BuildSql(out _, false, true);
        if (!this.Equals(subQuery.Visitor))
        {
            subQuery.Visitor.CopyTo(this);
            subQuery.Visitor.Dispose();
        }
        this.UnionSql = rawSql;
    }
    public void Union(string union, Type targetType, DbContext dbContext, Delegate subQueryGetter)
    {
        var fromQuery = new FromQuery(dbContext, this);
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
        if (target == null || this.IncludeSegments == null || this.IncludeSegments.Count == 0)
        {
            sql = null;
            return false;
        }
        sql = this.BuildIncludeSql(targetType, (builder, sqlInitializer) => sqlInitializer.Invoke(this.OrmProvider, builder, target));
        return true;
    }
    public virtual bool BuildIncludeSql<TTarget>(Type targetType, List<TTarget> targets, out string sql)
    {
        if (targets == null || this.IncludeSegments == null || this.IncludeSegments.Count == 0)
        {
            sql = null;
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
    public virtual void Select(string sqlFormat, Expression selectExpr = null)
    {
        this.IsSelect = true;
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
        this.IsFromQuery = false;
        this.IsSelect = false;
    }
    protected void SelectToReaderFields(LambdaExpression toTargetExpr)
    {
        var sqlSegment = new SqlSegment { Expression = toTargetExpr.Body };
        switch (toTargetExpr.Body.NodeType)
        {
            case ExpressionType.MemberAccess:
                var memberExpr = toTargetExpr.Body as MemberExpression;
                if (this.IsGroupingMember(memberExpr))
                {
                    //能够访问Grouping属性的场景，通常是在最外层的Select子句或是OrderBy子句
                    //此处特殊处理
                    if (this.IsFromQuery && memberExpr.Type.IsEntityType(out _))
                        throw new NotSupportedException("FROM子查询中不支持实体类型成员MemberAccess表达式访问，只支持基础字段访问访问");

                    foreach (var readerField in this.GroupFields)
                    {
                        if (readerField.TargetMember.Name != readerField.FromMember.Name)
                            readerField.Body += $" AS {readerField.TargetMember.Name}";
                    }
                    this.ReaderFields = this.GroupFields;
                }
                else
                {
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
                                MemberMapper = sqlSegment.MemberMapper,
                                TargetMember =sqlSegment.FromMember,
                                Body = sqlSegment.Value.ToString()
                            }
                        };
                    }
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

    public virtual void Distinct() => this.IsDistinct = true;
    public virtual void Page(int pageIndex, int pageSize)
    {
        if (pageIndex > 0) pageIndex--;
        this.skip = pageIndex * pageSize;
        this.limit = pageSize;
    }
    public virtual void Skip(int skip) => this.skip = skip;
    public virtual void Take(int limit) => this.limit = limit;

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
            return sqlSegment.ChangeValue(readerFields);
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
        return sqlSegment.ChangeValue(readerFields);
    }
    public virtual void AddSelectElement(Expression elementExpr, MemberInfo memberInfo, List<ReaderField> readerFields)
    {
        string fieldName = null;
        var sqlSegment = new SqlSegment { Expression = elementExpr };
        switch (elementExpr.NodeType)
        {
            case ExpressionType.Parameter:
                if (this.IsFromQuery)
                    throw new NotSupportedException("FROM子查询中不支持参数Parameter表达式访问，只支持基础字段访问访问");
                //场景: .Select((a, b) => new { Order = a, ... }) 或是 .Select((a, b) => a) 可能有include操作
                sqlSegment = this.VisitParameter(sqlSegment);
                var tableReaderFields = sqlSegment.Value as List<ReaderField>;
                tableReaderFields[0].FromMember = memberInfo;
                tableReaderFields[0].TargetMember = memberInfo;
                readerFields.AddRange(tableReaderFields);
                //1:N关系的Include成员设置完整成员访问路径，方便后面成员赋值
                if (this.IncludeSegments != null)
                {
                    var rootPath = tableReaderFields[0].TableSegment.AliasName;
                    foreach (var includeSegment in this.IncludeSegments)
                    {
                        if (!includeSegment.Path.Contains(rootPath))
                            continue;
                        includeSegment.ParentMemberVisits.Insert(0, memberInfo);
                    }
                }
                break;
            case ExpressionType.New:
            case ExpressionType.MemberInit:
                //为了简化SELECT操作，只支持一次New/MemberInit表达式操作
                throw new NotSupportedException("不支持的表达式访问，SELECT语句只支持一次New/MemberInit表达式访问操作");
            case ExpressionType.MemberAccess:
                if (elementExpr.Type.IsEntityType(out _))
                {
                    //三种场景：单个Json类型的实体字段访问，主表的Include导航属性访问，分组对象Grouping访问
                    //如：Grouping对象或是FromQuery返回的匿名对象中直接访问了参数User，
                    //后续的查询中引用了这个匿名对象中这个参数User成员
                    var memberExpr = elementExpr as MemberExpression;
                    if (this.IsGroupingMember(memberExpr))
                    {
                        foreach (var readerField in this.GroupFields)
                        {
                            //子查询中的字段别名要带有本地化包装
                            if (readerField.TargetMember != null && readerField.FromMember.Name != readerField.TargetMember.Name)
                                readerField.Body += " AS " + this.OrmProvider.GetFieldName(readerField.TargetMember.Name);
                        }
                        readerFields.Add(new ReaderField
                        {
                            FieldType = ReaderFieldType.Entity,
                            FromMember = memberInfo,
                            TargetMember = memberInfo,
                            ReaderFields = this.GroupFields
                        });
                        break;
                    }

                    if (this.IsFromQuery)
                        throw new NotSupportedException("FROM子查询中不支持实体类型成员MemberAccess表达式访问，只支持基础字段访问");

                    if (memberExpr.IsParameter(out var parameterName))
                    {
                        MemberMap memberMapper = null;
                        TableSegment fromSegment = null;
                        string path = null;

                        var rootTableSegment = this.TableAlias[parameterName];
                        var memberExprs = this.GetMemberExprs(memberExpr, out var parameterExpr);
                        var builder = new StringBuilder(rootTableSegment.AliasName);
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

                        //实体类型字段，两个场景：Json类型实体字段成员访问(包含实体表和子查询表)，Include导航实体类型成员访问(包括1:1,1:N关系)
                        //前一种情况，VisitMemberVisitor已经处理，此处只处理后者
                        if (fromSegment.Mapper != null)
                        {
                            memberMapper = fromSegment.Mapper.GetMemberMap(memberExpr.Member.Name);
                            if (memberMapper.IsIgnore)
                                throw new NotSupportedException($"类{fromSegment.EntityType.FullName}的成员{memberExpr.Member.Name}是忽略成员无法访问");

                            if (memberMapper.IsNavigation)
                            {
                                if (memberMapper.TypeHandler == null)
                                    throw new NotSupportedException($"类{fromSegment.EntityType.FullName}的成员{memberExpr.Member.Name}是实体类型，未配置导航属性也没有配置TypeHandler");

                                if (memberExprs.Count > 0)
                                    //此类型字段通常是JSON类型字段，不支持查询其内部字段
                                    throw new NotSupportedException("非导航属性的实体类型字段，不支持内部成员访问，只支持整个实体访问");

                                builder.Append("." + memberExpr.Member.Name);
                                path = builder.ToString();

                                if (memberMapper.IsToOne)
                                {
                                    var refReaderField = readerFields.Find(f => f.Path == path);
                                    readerFields.Add(new ReaderField
                                    {
                                        IsRef = true,//需要在构建实体的时候做处理
                                        FieldType = ReaderFieldType.Entity,
                                        FromMember = memberInfo,
                                        TargetMember = memberInfo,
                                        ReaderFields = new List<ReaderField> { refReaderField }
                                    });
                                }
                                else
                                {
                                    //1:N场景，要在第二次查询的时候再做处理，此处只构建延迟处理Action
                                    var includeSegment = this.IncludeSegments.Find(f => f.Path == path);
                                    var cacheKey = GetRefIncludeKey(memberInfo.DeclaringType, memberInfo.Name, includeSegment);
                                    var deferredSetter = targetRefIncludeValuesSetters.GetOrAdd(cacheKey, f =>
                                     {
                                         var rootReaderField = readerFields.Find(f => f.Path == parameterName);
                                         var targetExpr = Expression.Parameter(typeof(object), "target");
                                         var typedTargetExpr = Expression.Convert(targetExpr, memberInfo.DeclaringType);
                                         var refObjExpr = Expression.PropertyOrField(typedTargetExpr, memberInfo.Name);

                                         Expression refValueExpr = typedTargetExpr;
                                         foreach (var memberInfo in includeSegment.ParentMemberVisits)
                                         {
                                             refValueExpr = Expression.PropertyOrField(refValueExpr, memberInfo.Name);
                                         }
                                         refValueExpr = Expression.PropertyOrField(refValueExpr, includeSegment.FromMember.MemberName);

                                         Expression bodyExpr = null;
                                         if (memberInfo.MemberType == MemberTypes.Property)
                                             bodyExpr = Expression.Call(refObjExpr, (memberInfo as PropertyInfo).GetSetMethod(), refValueExpr);
                                         else if (memberInfo.MemberType == MemberTypes.Field)
                                             bodyExpr = Expression.Assign(refObjExpr, refValueExpr);
                                         return Expression.Lambda<Action<object>>(bodyExpr, targetExpr).Compile();
                                     });
                                    this.deferredRefIncludeValuesSetters ??= new();
                                    this.deferredRefIncludeValuesSetters.Add(deferredSetter);
                                }
                                break;
                            }
                        }
                    }

                    //类似Json类型的实体类字段
                    sqlSegment = this.VisitMemberAccess(sqlSegment);
                    fieldName = this.GetQuotedValue(sqlSegment);
                    readerFields.Add(new ReaderField
                    {
                        FieldType = ReaderFieldType.Field,
                        TableSegment = sqlSegment.TableSegment,
                        FromMember = sqlSegment.FromMember,
                        TargetMember = memberInfo,
                        IsOnlyField = true,
                        Body = fieldName
                    });
                    break;
                }

                //参数或是本地成员变量访问
                sqlSegment = this.VisitAndDeferred(sqlSegment);
                fieldName = this.GetQuotedValue(sqlSegment);
                if (sqlSegment.IsExpression)
                    fieldName = $"({fieldName})";
                if ((sqlSegment.IsParameter || sqlSegment.FromMember?.Name != memberInfo.Name))
                    fieldName += " AS " + this.OrmProvider.GetFieldName(memberInfo.Name);

                readerFields.Add(new ReaderField
                {
                    FieldType = ReaderFieldType.Field,
                    TableSegment = sqlSegment.TableSegment,
                    FromMember = sqlSegment.FromMember,
                    TargetMember = memberInfo,
                    IsOnlyField = !(sqlSegment.IsParameter || sqlSegment.IsExpression || sqlSegment.IsMethodCall),
                    Body = fieldName
                });
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
                    //.SelectTo<DTO>((a, b ...) => new DTO
                    //{
                    //    ActivityTypeEnum = this.GetEmnuName(f.ActivityType)
                    //})
                    methodCallField.FromMember = memberInfo;
                    methodCallField.TargetMember = memberInfo;
                    readerFields.Add(methodCallField);
                    break;
                }
                else fieldName = this.GetQuotedValue(sqlSegment);
                if (sqlSegment.IsExpression)//TODO:if (sqlSegment.IsExpression && !this.IsInsertTo)
                    fieldName = $"({fieldName})";
                //if ((sqlSegment.IsParameter || sqlSegment.IsExpression || sqlSegment.IsMethodCall || sqlSegment.FromMember?.Name != memberInfo.Name) && !this.IsInsertTo)
                if ((sqlSegment.IsParameter || sqlSegment.IsExpression || sqlSegment.IsMethodCall))
                    fieldName += " AS " + this.OrmProvider.GetFieldName(memberInfo.Name);

                readerFields.Add(new ReaderField
                {
                    FieldType = ReaderFieldType.Field,
                    TableSegment = sqlSegment.TableSegment,
                    FromMember = memberInfo,
                    TargetMember = memberInfo,
                    Body = fieldName
                });
                break;
        }
    }
    public virtual TableSegment AddTable(TableSegment tableSegment)
    {
        this.UnionSql = null;
        this.Tables.Add(tableSegment);
        if (this.Tables.Count == 2)
        {
            //没有select过，就不用设置body值
            if (this.Tables[0].ReaderFields != null && this.Tables[0].ReaderFields.Count > 0)
                this.InitFromQueryReaderFields(this.Tables[0], this.Tables[0].ReaderFields);
        }
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
    public void Clear(bool isClearReaderFields = false)
    {
        this.Tables.Clear();
        if (isClearReaderFields)
            this.ReaderFields?.Clear();
        this.WhereSql = null;
        this.TableAsStart = 'a';

        this.skip = null;
        this.limit = null;
        this.UnionSql = string.Empty;
        this.GroupBySql = string.Empty;
        this.HavingSql = string.Empty;
        this.OrderBySql = string.Empty;
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
        this.CteTableSql = null;

        this.IncludeSegments = null;
        this.LastIncludeSegment = null;

        this.CteTables = null;
        this.CteQueries = null;
        this.CteTableSegments = null;
        this.SelfTableSegment = null;
    }
    protected void InitFromQueryReaderFields(TableSegment tableSegment, List<ReaderField> readerFields)
    {
        if (readerFields == null || readerFields.Count == 0)
            return;

        foreach (var readerField in readerFields)
        {
            if (readerField.FieldType == ReaderFieldType.Entity)
            {
                readerField.TableSegment = tableSegment;
                this.InitFromQueryReaderFields(tableSegment, readerField.ReaderFields);
            }
            else
            {
                readerField.TableSegment = tableSegment;
                //已经变成子查询了，原表字段名已经没意义了，直接变成新的字段名
                if (readerField.TargetMember != null)
                    readerField.FromMember = readerField.TargetMember;
                //重新设置body内容，表别名变更
                readerField.Body = tableSegment.AliasName + "." + this.OrmProvider.GetFieldName(readerField.FromMember.Name);
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
            var memberInfo = readerField.FromMember;
            if (index > 0) builder.Append(',');
            builder.Append(memberInfo.Name);
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
    private int GetRefIncludeKey(Type targetType, string refMemberName, TableSegment includeSegment)
    {
        var hashCode = new HashCode();
        hashCode.Add(this.DbKey);
        hashCode.Add(this.OrmProvider);
        hashCode.Add(targetType);
        hashCode.Add(refMemberName);
        hashCode.Add(includeSegment.ParentMemberVisits.Count);
        foreach (var memberInfo in includeSegment.ParentMemberVisits)
        {
            hashCode.Add(memberInfo.DeclaringType);
            hashCode.Add(memberInfo.Name);
        }
        hashCode.Add(includeSegment.FromMember.MemberName);
        return hashCode.ToHashCode();
    }
}
