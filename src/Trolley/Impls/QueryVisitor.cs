using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Trolley;

public class QueryVisitor : SqlVisitor, IQueryVisitor
{
    private static ConcurrentDictionary<int, string> sqlCache = new();
    private static ConcurrentDictionary<int, object> getterCache = new();
    private static ConcurrentDictionary<int, object> setterCache = new();
    protected string sql = string.Empty;
    protected string groupBySql = string.Empty;
    protected string havingSql = string.Empty;
    protected string orderBySql = string.Empty;
    protected int? skip = null;
    protected int? limit = null;
    protected bool isDistinct = false;
    protected string cteTableSql = null;
    protected List<TableSegment> includeSegments = null;
    protected TableSegment lastIncludeSegment = null;
    protected List<ReaderField> groupFields = null;

    public QueryVisitor(string dbKey, IOrmProvider ormProvider, IEntityMapProvider mapProvider, bool isParameterized = false, char tableAsStart = 'a', string parameterPrefix = "p", string multiParameterPrefix = "")
        : base(dbKey, ormProvider, mapProvider, isParameterized, tableAsStart, parameterPrefix, multiParameterPrefix)
    {
        this.tables = new();
        this.tableAlias = new();
        this.dbParameters = new();
    }
    /// <summary>
    /// Union,ToSql,Join,Cte，各种子查询,各种单值查询，但都有SELECT操作
    /// </summary>
    /// <param name="dbParameters"></param>
    /// <param name="readerFields"></param>
    /// <returns></returns>
    public virtual string BuildSql(out List<IDbDataParameter> dbParameters, out List<ReaderField> readerFields, bool isUnion = false, char unionAlias = 'a')
    {
        if (!string.IsNullOrEmpty(this.sql))
        {
            dbParameters = this.dbParameters;
            readerFields = this.readerFields;
            return this.sql;
        }
        var builder = new StringBuilder();
        //各种单值查询，如：SELECT COUNT(*)/MAX(*)..等，都有SELECT操作
        //FROM临时表没有SELECT操作，直接查询表中所有字段,或许会跟Join/Union等带有子查询的SQL操作
        //如：From(f=>...).InnerJoin/UnionAll(f=>...)
        if (this.readerFields == null || this.readerFields.Count == 0)
        {
            builder.Append("*");
            this.tables[0].IsUsed = true;
        }
        else this.AddReaderFields(this.readerFields, builder);

        string selectSql = null;
        if (this.isDistinct)
            selectSql = "DISTINCT " + builder.ToString();
        else selectSql = builder.ToString();

        builder.Clear();
        string tableSql = null;
        if (this.tables.Count > 0)
        {
            foreach (var tableSegment in this.tables)
            {
                //在Update的Value子查询语句中，where子句中有更新主表的关联条件，此时IsUsed=false
                //在Include的1:1子表，通常不参与Select语句,如果有Parameter类型的主表查询时，IsUsed=true，没有则IsUsed=false
                if (!tableSegment.IsUsed) continue;
                var tableName = tableSegment.Body;
                if (string.IsNullOrEmpty(tableName))
                {
                    tableSegment.Mapper ??= this.MapProvider.GetEntityMap(tableSegment.EntityType);
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

                if (this.IsNeedAlias || tableSegment.IsNeedAlais)
                    builder.Append(" " + tableSegment.AliasName);
                if (!string.IsNullOrEmpty(tableSegment.SuffixRawSql))
                    builder.Append(" " + tableSegment.SuffixRawSql);
                if (!string.IsNullOrEmpty(tableSegment.OnExpr))
                    builder.Append($" ON {tableSegment.OnExpr}");
            }
            tableSql = builder.ToString();
        }

        builder.Clear();
        if (!string.IsNullOrEmpty(this.whereSql))
        {
            this.whereSql = $" WHERE {this.whereSql}";
            builder.Append(this.whereSql);
        }
        if (!string.IsNullOrEmpty(this.groupBySql))
            builder.Append($" GROUP BY {this.groupBySql}");
        if (!string.IsNullOrEmpty(this.havingSql))
            builder.Append($" HAVING {this.havingSql}");

        string orderBy = null;
        if (!string.IsNullOrEmpty(this.orderBySql))
        {
            orderBy = $"ORDER BY {this.orderBySql}";
            if (!this.skip.HasValue && !this.limit.HasValue)
                builder.Append(" " + orderBy);
        }
        string others = builder.ToString();

        dbParameters = this.dbParameters;
        readerFields = this.readerFields;

        builder.Clear();
        if (!string.IsNullOrEmpty(this.cteTableSql))
            builder.AppendLine(this.cteTableSql);

        if (this.skip.HasValue || this.limit.HasValue)
        {
            //SQL TEMPLATE:SELECT /**fields**/ FROM /**tables**/ /**others**/
            var pageSql = this.OrmProvider.GetPagingTemplate(this.skip, this.limit, orderBy);
            pageSql = pageSql.Replace("/**fields**/", selectSql);
            pageSql = pageSql.Replace("/**tables**/", tableSql);
            pageSql = pageSql.Replace(" /**others**/", others);

            if (this.skip.HasValue && this.limit.HasValue)
                builder.Append($"SELECT COUNT(*) FROM {tableSql}{this.whereSql};");
            builder.Append($"{pageSql}");
        }
        else builder.Append($"SELECT {selectSql} FROM {tableSql}{others}");

        //UNION的子查询中有OrderBy或是Limit，就要包一下SELECT * FROM，否则数据结果不对
        if (isUnion && (!string.IsNullOrEmpty(this.orderBySql) || this.limit.HasValue))
        {
            builder.Insert(0, "SELECT * FROM (");
            builder.Append($") {unionAlias}");
        }
        return builder.ToString();
    }
    public virtual bool HasIncludeTables() => this.includeSegments != null && this.includeSegments.Count > 0;
    public virtual bool BuildIncludeSql(object parameter, out string sql)
    {
        if (parameter == null || this.includeSegments == null || this.includeSegments.Count <= 0)
        {
            sql = null;
            return false;
        }
        bool isEffective = false;
        foreach (var includeSegment in this.includeSegments)
        {
            if (includeSegment.IsUsed)
            {
                isEffective = true;
                break;
            }
        }
        if (!isEffective)
        {
            sql = null;
            return false;
        }

        bool isMulti = false;
        Type targetType = null;
        IEnumerable entities = null;
        if (parameter is IEnumerable)
        {
            isMulti = true;
            entities = parameter as IEnumerable;
            foreach (var entity in entities)
            {
                targetType = entity.GetType();
                break;
            }
        }
        else targetType = parameter.GetType();

        var builder = new StringBuilder();
        foreach (var includeSegment in this.includeSegments)
        {
            if (!includeSegment.IsUsed)
                continue;
            var sqlFetcher = this.BuildAddIncludeFetchSqlInitializer(isMulti, targetType, includeSegment);
            if (isMulti)
            {
                var fetchSqlInitializer = sqlFetcher as Action<object, IOrmProvider, int, StringBuilder>;
                int index = 0;
                foreach (var entity in entities)
                {
                    fetchSqlInitializer.Invoke(entity, this.OrmProvider, index, builder);
                    index++;
                }
            }
            else
            {
                var fetchSqlInitializer = sqlFetcher as Action<object, IOrmProvider, StringBuilder>;
                fetchSqlInitializer.Invoke(parameter, this.OrmProvider, builder);
            }
            builder.Append(')');
            if (!string.IsNullOrEmpty(includeSegment.Filter))
                builder.Append($" AND {includeSegment.Filter}");
        }
        sql = builder.ToString();
        return true;
    }
    public virtual void SetIncludeValues(object parameter, IDataReader reader)
    {
        var valueSetter = this.BuildIncludeValueSetterInitializer(parameter, this.includeSegments);
        var valueSetterInitiliazer = valueSetter as Action<object, IDataReader, string, IOrmProvider, IEntityMapProvider, List<TableSegment>>;
        valueSetterInitiliazer.Invoke(parameter, reader, this.DbKey, this.OrmProvider, this.MapProvider, this.includeSegments);
    }
    public virtual IQueryVisitor From(params Type[] entityTypes)
    {
        int tableIndex = this.TableAsStart + this.tables.Count;
        for (int i = 0; i < entityTypes.Length; i++)
        {
            this.tables.Add(new TableSegment
            {
                EntityType = entityTypes[i],
                AliasName = $"{(char)(tableIndex + i)}",
                Path = $"{(char)(tableIndex + i)}",
                TableType = TableType.Entity,
                IsMaster = true
            });
        }
        if (this.tables.Count > 1) this.IsNeedAlias = true;
        return this;
    }
    public virtual IQueryVisitor From(char tableAsStart, params Type[] entityTypes)
    {
        this.TableAsStart = tableAsStart;
        this.From(entityTypes);
        return this;
    }
    public virtual IQueryVisitor From(char tableAsStart, Type entityType, string suffixRawSql)
    {
        this.TableAsStart = tableAsStart;
        int tableIndex = tableAsStart + this.tables.Count;
        this.tables.Add(new TableSegment
        {
            EntityType = entityType,
            Mapper = this.MapProvider.GetEntityMap(entityType),
            AliasName = $"{(char)tableIndex}",
            SuffixRawSql = suffixRawSql,
            Path = $"{(char)tableIndex}",
            TableType = TableType.Entity,
            IsMaster = true
        });
        return this;
    }
    public virtual TableSegment WithTable(Type entityType, string body, List<IDbDataParameter> dbParameters = null, List<ReaderField> readerFields = null, string joinType = "")
    {
        var tableSegment = this.AddTable(entityType, joinType, TableType.FromQuery, $"({body})", readerFields);
        //临时表需要有别名(SELECT ..) b
        tableSegment.IsNeedAlais = true;
        this.InitFromQueryReaderFields(tableSegment, readerFields);
        if (dbParameters != null)
            this.dbParameters.AddRange(dbParameters);
        return tableSegment;
    }
    public virtual IQueryVisitor WithCteTable(Type entityType, string cteTableName, bool isRecursive, string rawSql, List<IDbDataParameter> dbParameters = null, List<ReaderField> readerFields = null)
    {
        string withTable = cteTableName;
        if (isRecursive)
            withTable = "RECURSIVE " + cteTableName;

        var builder = new StringBuilder();
        if (string.IsNullOrEmpty(this.cteTableSql))
            builder.Append($"WITH {withTable}(");
        else
        {
            builder.Append(this.cteTableSql);
            builder.AppendLine(",");
            builder.Append($"{withTable}(");
        }

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
        this.cteTableSql = builder.ToString();

        var tableSegment = this.AddTable(entityType, string.Empty, TableType.FromQuery, cteTableName, readerFields);
        this.InitFromQueryReaderFields(tableSegment, readerFields);
        if (dbParameters != null)
            this.dbParameters.AddRange(dbParameters);
        //清掉构建CTE表时Union产生的sql
        this.sql = null;
        return this;
    }
    public virtual void Union(string body, List<ReaderField> readerFields, List<IDbDataParameter> dbParameters = null, char tableAlias = 'a')
    {
        var sql = this.BuildSql(out _, out _, true, tableAlias);
        sql += body;
        this.readerFields = readerFields;
        readerFields.ForEach(f => f.TableSegment = this.tables[0]);
        this.cteTableSql = null;
        if (dbParameters != null)
            this.dbParameters.AddRange(dbParameters);
        this.sql = sql;
    }
    public virtual void Include(Expression memberSelector, bool isIncludeMany = false, Expression filter = null)
    {
        if (!isIncludeMany)
        {
            this.IsNeedAlias = true;
            if (this.tables.Count > 1)
            {
                for (int i = this.tables.Count - 1; i > 0; i--)
                {
                    if (string.IsNullOrEmpty(this.tables[i].JoinType))
                        throw new NotSupportedException("表之间的关联关系需要使用LeftJoin、InnerJoin、RightJoin语句确定后，才能使用Include语句");
                }
            }
        }

        var lambdaExpr = memberSelector as LambdaExpression;
        var memberExpr = lambdaExpr.Body as MemberExpression;
        this.InitTableAlias(lambdaExpr);
        var includeSegment = this.AddIncludeTables(memberExpr);
        if (filter != null)
        {
            var filterLambdaExpr = filter as LambdaExpression;
            var parameterName = filterLambdaExpr.Parameters[0].Name;
            this.tableAlias.TryAdd(parameterName, includeSegment);
            includeSegment.Filter = this.Visit(new SqlSegment { Expression = filter }).ToString();
        }
        this.lastIncludeSegment = includeSegment;
    }
    public virtual void ThenInclude(Expression memberSelector, bool isIncludeMany = false, Expression filter = null)
    {
        if (!isIncludeMany) this.IsNeedAlias = true;
        var lambdaExpr = memberSelector as LambdaExpression;
        var memberExpr = lambdaExpr.Body as MemberExpression;
        lambdaExpr.Body.GetParameters(out var parameters);
        this.tableAlias.Clear();
        this.tableAlias.Add(parameters[0].Name, this.lastIncludeSegment);
        var includeSegment = this.AddIncludeTables(memberExpr);
        //TODO: 1:N关联条件的alias表，获取会有问题，待测试
        if (filter != null)
        {
            var filterLambdaExpr = filter as LambdaExpression;
            var parameterName = filterLambdaExpr.Parameters[0].Name;
            this.tableAlias.TryAdd(parameterName, includeSegment);
            includeSegment.Filter = this.Visit(new SqlSegment { Expression = filter }).ToString();
        }
        this.lastIncludeSegment = includeSegment;
    }
    public virtual void Join(string joinType, Expression joinOn)
    {
        this.isWhere = true;
        this.IsNeedAlias = true;
        var lambdaExpr = joinOn as LambdaExpression;
        if (!lambdaExpr.Body.GetParameters(out var parameters))
            throw new NotSupportedException("当前Join操作，没有表关联");
        if (parameters.Count != 2)
            throw new NotSupportedException("Join操作，只支持两个表进行关联，但可以多次Join操作");

        var joinTableSegment = this.InitTableAlias(lambdaExpr);
        joinTableSegment.JoinType = joinType;
        joinTableSegment.OnExpr = this.VisitConditionExpr(lambdaExpr.Body);
        this.isWhere = false;
    }
    public virtual void Join(string joinType, Type newEntityType, Expression joinOn)
    {
        this.isWhere = true;
        this.IsNeedAlias = true;
        var lambdaExpr = joinOn as LambdaExpression;
        if (!lambdaExpr.Body.GetParameters(out var parameters))
            throw new NotSupportedException("当前Join操作，没有表关联");
        if (parameters.Count != 2)
            throw new NotSupportedException("Join操作，只支持两个表进行关联，但可以多次Join操作");

        var joinTableSegment = this.AddTable(newEntityType, joinType);
        this.InitTableAlias(lambdaExpr);
        joinTableSegment.OnExpr = this.VisitConditionExpr(lambdaExpr.Body);
        this.isWhere = false;
    }
    public virtual void Join(string joinType, TableSegment joinTableSegment, Expression joinOn)
    {
        this.isWhere = true;
        this.IsNeedAlias = true;
        var lambdaExpr = joinOn as LambdaExpression;
        if (!lambdaExpr.Body.GetParameters(out var parameters))
            throw new NotSupportedException("当前Join操作，没有表关联");
        if (parameters.Count != 2)
            throw new NotSupportedException("Join操作，只支持两个表进行关联，但可以多次Join操作");

        this.InitTableAlias(lambdaExpr);
        joinTableSegment.JoinType = joinType;
        joinTableSegment.OnExpr = this.VisitConditionExpr(lambdaExpr.Body);
        this.isWhere = false;
    }
    public virtual void Join(string joinType, Type newEntityType, string cteTableName, Expression joinOn)
    {
        this.isWhere = true;
        this.IsNeedAlias = true;
        var lambdaExpr = joinOn as LambdaExpression;
        if (!lambdaExpr.Body.GetParameters(out var parameters))
            throw new NotSupportedException("当前Join操作，没有表关联");
        if (parameters.Count != 2)
            throw new NotSupportedException("Join操作，只支持两个表进行关联，但可以多次Join操作");

        var joinTableSegment = this.AddTable(newEntityType, joinType, TableType.FromQuery, cteTableName);
        var readerFields = this.FlattenTableFields(joinTableSegment);
        joinTableSegment.ReaderFields = readerFields;
        this.InitTableAlias(lambdaExpr);
        joinTableSegment.OnExpr = this.VisitConditionExpr(lambdaExpr.Body);
        this.isWhere = false;
    }
    public virtual void Select(string sqlFormat, Expression selectExpr = null, bool isFromQuery = false)
    {
        this.isSelect = true;
        this.isFromQuery = isFromQuery;
        if (selectExpr != null)
        {
            var lambdaExpr = selectExpr as LambdaExpression;
            this.InitTableAlias(lambdaExpr);
            this.readerFields = this.ConstructorFieldsTo(lambdaExpr);
        }
        if (!string.IsNullOrEmpty(sqlFormat))
        {
            //单值操作，SELECT COUNT(DISTINCT b.Id),MAX(b.Amount)等
            this.tables.FindAll(f => f.IsMaster).ForEach(f => f.IsUsed = true);
            if (this.readerFields != null && this.readerFields.Count == 1)
            {
                var readerField = this.readerFields[0];
                readerField.Body = string.Format(sqlFormat, readerField.Body);
            }
            else
            {
                //单值操作，SELECT COUNT(1)等,或是From临时表，后续Union/Join操作
                this.readerFields ??= new();
                this.readerFields.Add(new ReaderField
                {
                    Index = 0,
                    Body = sqlFormat
                });
            }
        }
        this.isFromQuery = false;
        this.isSelect = false;
    }
    public virtual void SelectGrouping(bool isFromQuery = false)
    {
        this.readerFields = this.groupFields;
        this.isFromQuery = isFromQuery;
    }
    public virtual void SelectDefault(Expression defaultExpr)
    {
        if (this.readerFields == null || this.readerFields.Count == 0)
            this.Select(null, defaultExpr);
    }
    public virtual void GroupBy(Expression expr)
    {
        var lambdaExpr = expr as LambdaExpression;
        this.InitTableAlias(lambdaExpr);
        this.groupFields = new();
        this.groupBySql = this.VisitList(lambdaExpr, true, string.Empty);
    }
    public virtual void OrderBy(string orderBy) => this.orderBySql = orderBy;
    public virtual void OrderBy(string orderType, Expression expr)
    {
        var lambdaExpr = expr as LambdaExpression;
        if (lambdaExpr.Body.NodeType != ExpressionType.New && lambdaExpr.Body.NodeType != ExpressionType.MemberAccess)
            throw new Exception("不支持的表达式访问,只支持New或MemberAccess表达式");

        var builder = new StringBuilder();
        if (!string.IsNullOrEmpty(this.orderBySql))
            builder.Append(this.orderBySql + ",");
        if (this.IsGroupingAggregateMember(lambdaExpr.Body as MemberExpression))
        {
            for (int i = 0; i < this.groupFields.Count; i++)
            {
                if (i > 0) builder.Append(',');
                builder.Append(this.groupFields[i].Body);
                if (orderType == "DESC")
                    builder.Append(" DESC");
            }
        }
        else
        {
            this.InitTableAlias(lambdaExpr);
            builder.Append(this.VisitList(lambdaExpr, false, orderType == "DESC" ? " DESC" : string.Empty));
        }
        this.orderBySql = builder.ToString();
    }
    public virtual void Having(Expression havingExpr)
    {
        this.isWhere = true;
        var lambdaExpr = havingExpr as LambdaExpression;
        this.InitTableAlias(lambdaExpr);
        this.havingSql = this.VisitConditionExpr(lambdaExpr.Body);
        this.isWhere = false;
    }
    public virtual IQueryVisitor Page(int pageIndex, int pageSize)
    {
        if (pageIndex > 0) pageIndex--;
        this.skip = pageIndex * pageSize;
        this.limit = pageSize;
        return this;
    }
    public virtual IQueryVisitor Skip(int skip)
    {
        this.skip = skip;
        return this;
    }
    public virtual IQueryVisitor Take(int limit)
    {
        this.limit = limit;
        return this;
    }
    public virtual IQueryVisitor Where(Expression whereExpr, bool isClearTableAlias = true)
    {
        this.isWhere = true;
        var lambdaExpr = whereExpr as LambdaExpression;
        if (isClearTableAlias)
            this.InitTableAlias(lambdaExpr);
        //在Update的Value子查询语句中，更新主表别名是a，引用表别名从b开始，无需别名替换
        this.lastWhereNodeType = OperationType.None;
        this.whereSql = this.VisitConditionExpr(lambdaExpr.Body);
        this.isWhere = false;
        return this;
    }
    public virtual IQueryVisitor And(Expression whereExpr)
    {
        this.isWhere = true;
        var lambdaExpr = whereExpr as LambdaExpression;
        this.InitTableAlias(lambdaExpr);
        if (this.lastWhereNodeType == OperationType.Or)
        {
            this.whereSql = $"({this.whereSql})";
            this.lastWhereNodeType = OperationType.And;
        }
        var conditionSql = this.VisitConditionExpr(lambdaExpr.Body);
        if (this.lastWhereNodeType == OperationType.Or)
        {
            conditionSql = $"({conditionSql})";
            this.lastWhereNodeType = OperationType.And;
        }
        if (!string.IsNullOrEmpty(this.whereSql))
            this.whereSql += " AND " + conditionSql;
        else this.whereSql = conditionSql;
        this.isWhere = false;
        return this;
    }
    public virtual void Distinct() => this.isDistinct = true;
    public override SqlSegment VisitParameter(SqlSegment sqlSegment)
    {
        var parameterExpr = sqlSegment.Expression as ParameterExpression;
        var fromSegment = this.tableAlias[parameterExpr.Name];
        var readerFields = this.AddTableRecursiveReaderFields(sqlSegment.ReaderIndex, fromSegment);
        readerFields.ForEach(f => f.TableSegment.IsUsed = true);

        //只有Parameter访问，IncludeMany才会生效
        if (this.includeSegments != null && this.includeSegments.Count > 0)
        {
            foreach (var includeSegment in this.includeSegments)
            {
                var parent = includeSegment.FromTable;
                while (parent != null)
                {
                    if (parent == fromSegment)
                    {
                        includeSegment.IsUsed = true;
                        break;
                    }
                    parent = parent.FromTable;
                }
            }
        }
        return sqlSegment.Change(readerFields, false);
    }
    public override SqlSegment VisitNew(SqlSegment sqlSegment)
    {
        var newExpr = sqlSegment.Expression as NewExpression;
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
        var readerFields = new List<ReaderField>();
        for (int i = 0; i < memberInitExpr.Bindings.Count; i++)
        {
            if (memberInitExpr.Bindings[i].BindingType != MemberBindingType.Assignment)
                throw new Exception("暂时不支持除MemberBindingType.Assignment类型外的成员绑定表达式");
            var memberAssignment = memberInitExpr.Bindings[i] as MemberAssignment;
            this.AddSelectElement(memberAssignment.Expression, memberAssignment.Member, readerFields);
        }
        return sqlSegment.ChangeValue(readerFields);
    }
    public override SqlSegment VisitMemberAccess(SqlSegment sqlSegment)
    {
        var memberExpr = sqlSegment.Expression as MemberExpression;
        MemberAccessSqlFormatter formatter = null;
        if (memberExpr.Expression != null)
        {
            if (this.IsGroupingAggregateMember(memberExpr))
            {
                if (this.isFromQuery)
                    throw new NotSupportedException("FROM子查询中不支持实体类型成员MemberAccess表达式访问，只支持基础字段访问访问");

                var tableSegment = new TableSegment
                {
                    EntityType = memberExpr.Type,
                    //分组临时表
                    TableType = TableType.FromQuery
                };
                foreach (var readerField in this.groupFields)
                {
                    if (this.isSelect || this.isWhere)
                        readerField.TableSegment.IsUsed = true;
                    readerField.TableSegment = tableSegment;
                }
                return new SqlSegment
                {
                    HasField = true,
                    IsConstant = false,
                    TableSegment = tableSegment,
                    MemberType = ReaderFieldType.AnonymousObject,
                    FromMember = memberExpr.Member,
                    Value = this.groupFields
                };
            }

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

            if (memberExpr.IsParameter(out var parameterName))
            {
                //Where(f=>... && f.Amount>5 && ...)
                //Include(f=>f.Buyer); 或是 IncludeMany(f=>f.Orders)
                //Select(f=>new {f.OrderId, ...})
                //Where(f=>f.Order.Id>10)
                //Include(f=>f.Order.Buyer)
                //Select(f=>new {f.Order.OrderId, ...})
                //GroupBy(f=>new {f.Order.OrderId, ...})
                //GroupBy(f=>f.Order.OrderId)
                //OrderBy(f=>new {f.Order.OrderId, ...})
                //OrderBy(f=>f.Order.OrderId)
                string path = null;
                TableSegment tableSegment = null;
                if (memberExpr.Type.IsEntityType(out _))
                {
                    if (this.isFromQuery)
                        throw new NotSupportedException("FROM子查询中不支持实体类型成员MemberAccess表达式访问，只支持基础字段访问访问");

                    var rootTableSegment = this.tableAlias[parameterName];
                    if (rootTableSegment.TableType == TableType.FromQuery)
                    {
                        //From子查询中，Select语句又做了参数访问，没有Include操作
                        //Select之后，得到一个临时表，并作为主表参与其他表进行关联操作
                        //另一个场景就是访问了Grouping对象
                        tableSegment = rootTableSegment;
                        var readerField = this.FindReaderField(memberExpr, tableSegment.ReaderFields);
                        return new SqlSegment
                        {
                            HasField = true,
                            IsConstant = false,
                            TableSegment = tableSegment,
                            MemberType = readerField.FieldType,
                            FromMember = readerField.FromMember,
                            Value = readerField
                        };
                    }
                    else
                    {
                        path = memberExpr.Expression.ToString();
                        var fromSegment = this.FindTableSegment(parameterName, path);
                        if (fromSegment == null)
                            throw new Exception($"使用导航属性前，要先使用Include方法包含进来，访问路径:{path}");
                        fromSegment.Mapper ??= this.MapProvider.GetEntityMap(fromSegment.EntityType);

                        var memberMapper = fromSegment.Mapper.GetMemberMap(memberExpr.Member.Name);
                        if (memberMapper.IsIgnore)
                            throw new Exception($"类{tableSegment.EntityType.FullName}的成员{memberExpr.Member.Name}是忽略成员无法访问");

                        if (!memberMapper.IsNavigation && memberMapper.TypeHandler == null)
                            throw new Exception($"类{tableSegment.EntityType.FullName}的成员{memberExpr.Member.Name}不是值类型，未配置为导航属性也没有配置TypeHandler");

                        if (memberMapper.IsNavigation)
                        {
                            path = memberExpr.ToString();
                            tableSegment = this.FindTableSegment(parameterName, path, memberMapper.IsToOne);
                            if (tableSegment == null)
                                throw new Exception($"使用导航属性前，要先使用Include方法包含进来，访问路径:{path}");

                            if (this.isSelect || this.isWhere)
                                throw new NotSupportedException($"导航属性{memberExpr}无需直接访问，随主表实体{memberExpr.Expression.Type.FullName}的Parameter访问自动返回，此处应该去掉");
                            //fromSegment.IsUsed = true;
                            //tableSegment.IsUsed = true;
                            //if (memberMapper.IsToOne)
                            //{
                            //    var readerFields = this.AddTableRecursiveReaderFields(sqlSegment.ReaderIndex, tableSegment);
                            //    return new SqlSegment
                            //    {
                            //        HasField = true,
                            //        IsConstantValue = false,
                            //        TableSegment = tableSegment,
                            //        MemberType = ReaderFieldType.Entity,
                            //        FromMember = memberExpr.Member,
                            //        Value = readerFields
                            //    };
                            //}
                            //else throw new NotSupportedException($"导航属性{memberExpr}无需直接访问，随主表实体{memberExpr.Expression.Type.FullName}的Parameter访问自动返回，此处应该去掉");
                        }
                        else
                        {
                            if (memberMapper.TypeHandler != null)
                            {
                                //.NET枚举类型总是解析成对应的UnderlyingType数值类型，如：a.Gender ?? Gender.Male == Gender.Male
                                //如果枚举类型对应的数据库类型是字符串就会有问题，需要把数字变为枚举，再把枚举的名字字符串完成后续操作。
                                if (this.isWhere && memberMapper.MemberType.IsEnumType(out var expectType, out _))
                                {
                                    var targetType = this.OrmProvider.MapDefaultType(memberMapper.NativeDbType);
                                    sqlSegment.ExpectType = expectType;
                                    sqlSegment.TargetType = targetType;
                                }
                                //类似Json的实体类型字段
                                var fieldName = this.GetFieldName(fromSegment, memberMapper.FieldName);
                                if (this.isSelect || this.isWhere)
                                    fromSegment.IsUsed = true;

                                sqlSegment.HasField = true;
                                sqlSegment.IsConstant = false;
                                sqlSegment.TableSegment = fromSegment;
                                sqlSegment.MemberType = ReaderFieldType.Field;
                                sqlSegment.FromMember = memberMapper.Member;
                                sqlSegment.MemberMapper = memberMapper;
                                sqlSegment.Value = fieldName;
                                return sqlSegment;
                            }
                            else throw new NotSupportedException($"类{fromSegment.EntityType.FullName}的成员{memberMapper.MemberName}不是值类型，未配置为导航属性也没有配置TypeHandler，也不是忽略成员");
                        }
                    }
                }
                else
                {
                    if (this.IsGroupingAggregateMember(memberExpr.Expression as MemberExpression))
                    {
                        var readerField = this.groupFields.Find(f => (f.TargetMember ?? f.FromMember).Name == memberExpr.Member.Name);
                        if (this.isSelect || this.isWhere)
                            readerField.TableSegment.IsUsed = true;

                        sqlSegment.HasField = true;
                        sqlSegment.IsConstant = false;
                        sqlSegment.TableSegment = readerField.TableSegment;
                        sqlSegment.MemberType = ReaderFieldType.Field;
                        sqlSegment.FromMember = readerField.FromMember;
                        sqlSegment.Value = readerField.Body;
                        return sqlSegment;
                    }

                    string fieldName = null;
                    MemberInfo memberInfo = null;
                    MemberMap memberMapper = null;
                    var rootTableSegment = this.tableAlias[parameterName];

                    if (rootTableSegment.TableType == TableType.FromQuery)
                    {
                        tableSegment = rootTableSegment;
                        var readerField = this.FindReaderField(memberExpr, tableSegment.ReaderFields);
                        memberInfo = readerField.FromMember;
                        fieldName = readerField.Body;
                        //TODO:访问了临时表的Enum类型，对应的数据库类型是VARCHAR类型，此栏位只做SELECT子句操作，不需要处理
                        //如果还做Where条件，就需要处理
                    }
                    else
                    {
                        path = memberExpr.Expression.ToString();
                        tableSegment = this.FindTableSegment(parameterName, path);
                        if (tableSegment == null)
                            throw new Exception($"使用导航属性前，要先使用Include方法包含进来，访问路径:{path}");

                        tableSegment.Mapper ??= this.MapProvider.GetEntityMap(tableSegment.EntityType);
                        memberMapper = tableSegment.Mapper.GetMemberMap(memberExpr.Member.Name);

                        if (memberMapper.IsIgnore)
                            throw new Exception($"类{tableSegment.EntityType.FullName}的成员{memberMapper.MemberName}是忽略成员无法访问");

                        //.NET枚举类型总是解析成对应的UnderlyingType数值类型，如：a.Gender ?? Gender.Male == Gender.Male
                        //如果枚举类型对应的数据库类型是字符串就会有问题，需要把数字变为枚举，再把枚举的名字字符串完成后续操作。
                        if (memberMapper.MemberType.IsEnumType(out var expectType, out _))
                        {
                            var targetType = this.OrmProvider.MapDefaultType(memberMapper.NativeDbType);
                            if (targetType == typeof(string))
                            {
                                sqlSegment.ExpectType = expectType;
                                sqlSegment.TargetType = targetType;
                            }
                        }

                        //有Join时采用别名，如果当前类是IncludeMany的导航类时，没有别名
                        fieldName = this.GetFieldName(tableSegment, memberMapper.FieldName);
                        memberInfo = memberMapper.Member;
                    }
                    if (this.isSelect || this.isWhere)
                        tableSegment.IsUsed = true;

                    sqlSegment.HasField = true;
                    sqlSegment.IsConstant = false;
                    sqlSegment.TableSegment = tableSegment;
                    sqlSegment.MemberType = ReaderFieldType.Field;
                    sqlSegment.FromMember = memberInfo;
                    sqlSegment.MemberMapper = memberMapper;
                    sqlSegment.Value = fieldName;
                    return sqlSegment;
                }
            }
        }

        //各种静态成员访问，如：DateTime.Now,int.MaxValue,string.Empty, DBNull.Value     
        if (this.OrmProvider.TryGetMemberAccessSqlFormatter(memberExpr, out formatter))
            //静态成员访问，理论上没有target对象，为了不再创建sqlSegment对象，此处直接把sqlSegment对象传了进去
            return formatter.Invoke(this, sqlSegment);

        //访问局部变量或是成员变量，当作常量处理,直接计算，如果是字符串变成参数@p
        //var orderIds=new List<int>{1,2,3}; Where(f=>orderIds.Contains(f.OrderId)); orderIds
        //private Order order; Where(f=>f.OrderId==this.Order.Id); this.Order.Id
        //var orderId=10; Select(f=>new {OrderId=orderId,...}
        //Select(f=>new {OrderId=this.Order.Id, ...}
        this.Evaluate(sqlSegment);

        //.NET枚举类型总是解析成对应的UnderlyingType数值类型，如：a.Gender ?? Gender.Male == Gender.Male
        //如果枚举类型对应的数据库类型是字符串就会有问题，需要把数字变为枚举，再把枚举的名字字符串完成后续操作。
        //if (sqlSegment.Expression.Type.IsEnumType(out var underlyingType, out _))
        //    sqlSegment.Type = underlyingType;

        //当变量为数组或是IEnumerable时，此处变为参数，方法Sql.In，Contains无法继续解析
        //这里不做参数化，后面统一走参数化处理，在二元操作表达式解析时做参数化处理
        sqlSegment.IsConstant = false;
        sqlSegment.IsVariable = true;
        sqlSegment.IsExpression = false;
        sqlSegment.IsMethodCall = false;
        return sqlSegment;
    }
    public virtual TableSegment InitTableAlias(LambdaExpression lambdaExpr)
    {
        TableSegment tableSegment = null;
        this.tableAlias.Clear();
        lambdaExpr.Body.GetParameterNames(out var parameterNames);
        if ((parameterNames == null || parameterNames.Count <= 0))
            return tableSegment;
        var masterTables = this.tables.FindAll(f => f.IsMaster);
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
            this.tableAlias.Add(parameterExpr.Name, masterTables[index]);
            tableSegment = masterTables[index];
            index++;
        }
        return tableSegment;
    }
    public virtual TableSegment AddTable(TableSegment tableSegment)
    {
        this.tables.Add(tableSegment);
        if (this.tables.Count > 1)
        {
            this.IsNeedAlias = true;
            if (this.tables.Count == 2 && this.tables[0].TableType == TableType.FromQuery && this.tables[0].ReaderFields != null)
                this.InitFromQueryReaderFields(this.tables[0], this.tables[0].ReaderFields);
        }
        return tableSegment;
    }
    public virtual TableSegment AddTable(Type entityType, string joinType = "", TableType tableType = TableType.Entity, string body = null, List<ReaderField> readerFields = null)
    {
        int tableIndex = this.TableAsStart + this.tables.Count;
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
    public virtual void AddAliasTable(string aliasName, TableSegment tableSegment)
        => this.tableAlias.TryAdd(aliasName, tableSegment);
    public virtual IQueryVisitor Clone(char tableAsStart = 'a', string parameterPrefix = "p")
    {
        var visitor = this.OrmProvider.NewQueryVisitor(this.DbKey, this.MapProvider, this.IsParameterized, tableAsStart, parameterPrefix);
        visitor.IsNeedAlias = this.IsNeedAlias;
        return visitor;
    }

    protected void InitFromQueryReaderFields(TableSegment tableSegment, List<ReaderField> readerFields)
    {
        if (readerFields == null || readerFields.Count == 0)
            return;

        foreach (var readerField in readerFields)
        {
            if (readerField.FieldType == ReaderFieldType.Entity
                || readerField.FieldType == ReaderFieldType.AnonymousObject)
            {
                readerField.TableSegment = tableSegment;
                this.InitFromQueryReaderFields(tableSegment, readerField.ReaderFields);
            }
            else
            {
                readerField.TableSegment = tableSegment;
                //中间临时表的字段，可能会有更改，优先取TargetMember，没有则取FromMember
                readerField.FromMember = readerField.TargetMember ?? readerField.FromMember;
                readerField.TargetMember = null;
                //body后面再设置，现在不确定是否需要别名
                readerField.Body = this.GetFieldName(tableSegment, readerField.FromMember.Name);
            }
        }
    }
    private ReaderField FindReaderField(MemberExpression memberExpr, List<ReaderField> readerFields)
    {
        var currentExpr = memberExpr;
        var visitedStack = new Stack<string>();
        while (true)
        {
            if (currentExpr == null || currentExpr.NodeType == ExpressionType.Parameter)
                break;
            visitedStack.Push(currentExpr.Member.Name);
            currentExpr = currentExpr.Expression as MemberExpression;
        }
        List<ReaderField> currentFields = readerFields;
        ReaderField readerField = null;
        //From语句生成的临时表，没有Include操作，实体类的成员，只能是参数访问
        while (visitedStack.TryPop(out var memberName))
        {
            readerField = currentFields.Find(f => f.FromMember.Name == memberName);
            if (readerField.ReaderFields != null && readerField.ReaderFields.Count > 0)
                currentFields = readerField.ReaderFields;
        }
        return readerField;
    }
    private TableSegment FindTableSegment(string parameterName, string path, bool isToOne = true)
    {
        var rootTableSegment = this.tableAlias[parameterName];
        var index = path.IndexOf(".");
        if (rootTableSegment.TableType == TableType.FromQuery)
        {
            var readerField = rootTableSegment.ReaderFields.Find(f => path.Contains(f.FromMember.Name));
        }
        if (isToOne) return this.FindTableSegment(parameterName, path);

        if (index > 0)
        {

            path = path.Replace(parameterName + ".", rootTableSegment.AliasName + ".");
            return this.includeSegments.Find(f => f.Path == path);
        }
        return null;
    }
    private TableSegment FindTableSegment(string parameterName, string path)
    {
        var index = path.IndexOf(".");
        if (index > 0)
        {
            var rootTableSegment = this.tableAlias[parameterName];
            path = path.Replace(parameterName + ".", rootTableSegment.AliasName + ".");
            return this.tables.Find(f => f.Path == path);
        }
        else return this.tableAlias[parameterName];
    }
    private void AddSelectElement(Expression elementExpr, MemberInfo memberInfo, List<ReaderField> readerFields)
    {
        string fieldName = null;
        var sqlSegment = new SqlSegment { Expression = elementExpr, ReaderIndex = readerFields.Count };
        switch (elementExpr.NodeType)
        {
            case ExpressionType.Parameter:
                if (this.isFromQuery)
                    throw new NotSupportedException("FROM子查询中不支持参数Parameter表达式访问，只支持基础字段访问访问");
                //Select语句中的参数访问，From子查询中的Select没有Include,最外层的Select语句有Include
                //如下两种情况：
                //repository.From<Order, OrderDetail>()
                //    .Include((x, y) => x.Buyer)
                //    .Where((a, b) => a.Id == b.OrderId)
                //    .Select((a, b) => new { Order = a, a.BuyerId, DetailId = b.Id, b.Price, b.Quantity, b.Amount }))
                //    .ToList();
                //repository.From(f => f.From<Order, OrderDetail, User>()
                //        .Where((a, b, c) => a.Id == b.OrderId && a.BuyerId == c.Id)
                //        .GroupBy((a, b, c) => new { OrderId = a.Id, a.BuyerId, c.Age })
                //        .Having((x, a, b, c) => c.Age > 20 && x.Sum(b.Amount) > 500)
                //        .Select((x, a, b, c) => new { x.Grouping.OrderId, a.BuyerId, TotalAmount = x.Sum(b.Amount) }))
                //    .InnerJoin<User>((a, b) => a.BuyerId == b.Id)
                //    .InnerJoin<Order>((a, b, c) => a.OrderId == c.Id)
                //    .Select((a, b, c) => new { xx = a, a.BuyerId, Buyer = b, Order = c, a.TotalAmount })
                //    .ToSql(out _);
                sqlSegment = this.VisitParameter(sqlSegment);
                var tableReaderFields = sqlSegment.Value as List<ReaderField>;
                tableReaderFields[0].FromMember = memberInfo;
                tableReaderFields[0].TargetMember = memberInfo;
                readerFields.AddRange(tableReaderFields);
                break;
            case ExpressionType.New:
            case ExpressionType.MemberInit:
                //为了简化SELECT操作，只支持一次New/MemberInit表达式操作
                throw new NotSupportedException("不支持的表达式访问，SELECT语句只支持一次New/MemberInit表达式操作");
            case ExpressionType.MemberAccess:
                if (elementExpr.Type.IsEntityType(out _))
                {
                    //TODO:访问了1:N关联关系的成员访问，在第二次查询中处理，此处什么也不做
                    //成员访问，一种情况是直接访问参数的成员，另一种情况是临时的匿名对象，
                    //如：Grouping对象或是FromQuery返回的匿名对象中直接访问了参数User，
                    //后续的查询中引用了这个匿名对象中这个参数User成员
                    if (this.IsGroupingAggregateMember(elementExpr as MemberExpression))
                    {
                        foreach (var readerField in this.groupFields)
                        {
                            readerField.TableSegment.IsUsed = true;
                            //子查询中的字段别名要带有本地化包装
                            if (readerField.TargetMember != null && readerField.FromMember.Name != readerField.TargetMember.Name)
                                readerField.Body += " AS " + this.OrmProvider.GetFieldName(readerField.TargetMember.Name);
                        }
                        readerFields.Add(new ReaderField
                        {
                            Index = readerFields.Count,
                            FieldType = ReaderFieldType.AnonymousObject,
                            FromMember = memberInfo,
                            TargetMember = memberInfo,
                            ReaderFields = this.groupFields
                        });
                    }
                    else
                    {
                        if (this.isFromQuery)
                            throw new NotSupportedException("FROM子查询中不支持实体类型成员MemberAccess表达式访问，只支持基础字段访问");

                        sqlSegment = this.VisitMemberAccess(sqlSegment);
                        //Include成员访问，可能有多层，如：order.Buyer.Compony
                        if (sqlSegment.Value is List<ReaderField> includedReaderFields)
                        {
                            includedReaderFields[0].FromMember = memberInfo;
                            //成员访问，还是实体类型属性，一定不是目标，应该是include
                            //主表Parameter访问，该includedReaderFields才能被返回
                            int index = readerFields.Count;
                            includedReaderFields.ForEach(f =>
                            {
                                f.Index = index;
                                index++;
                            });
                            readerFields.AddRange(includedReaderFields);
                        }
                        else if (sqlSegment.Value is ReaderField readerField)
                        {
                            //分组子查询后作为Select的临时表，访问x.Grouping对象或是Sql.FlattenTo返回的临时对象
                            readerField.Index = readerFields.Count;
                            readerField.FromMember = memberInfo;
                            readerField.TargetMember = memberInfo;
                            //成员访问，还是实体类型属性，一定不是目标，应该是include
                            readerFields.Add(readerField);
                        }
                        else
                        {
                            //类似Json类型的实体类字段
                            fieldName = this.GetQuotedValue(sqlSegment);
                            readerFields.Add(new ReaderField
                            {
                                Index = readerFields.Count,
                                FieldType = ReaderFieldType.Field,
                                TableSegment = sqlSegment.TableSegment,
                                FromMember = sqlSegment.FromMember,
                                TargetMember = memberInfo,
                                IsOnlyField = true,
                                Body = fieldName
                            });
                        }
                    }
                }
                else
                {
                    sqlSegment = this.VisitAndDeferred(sqlSegment);
                    //使用GetQuotedValue方法把常量都变成对应的字符串格式
                    //String和DateTime类型变成'...'数据,数字类型变成数字字符串
                    fieldName = this.GetQuotedValue(sqlSegment);
                    if (sqlSegment.IsExpression)
                        fieldName = $"({fieldName})";
                    if (sqlSegment.IsParameter || sqlSegment.IsExpression || sqlSegment.IsMethodCall || sqlSegment.FromMember?.Name != memberInfo.Name)
                        fieldName += " AS " + this.OrmProvider.GetFieldName(memberInfo.Name);

                    readerFields.Add(new ReaderField
                    {
                        Index = readerFields.Count,
                        FieldType = ReaderFieldType.Field,
                        TableSegment = sqlSegment.TableSegment,
                        FromMember = sqlSegment.FromMember,
                        TargetMember = memberInfo,
                        IsOnlyField = !(sqlSegment.IsParameter || sqlSegment.IsExpression || sqlSegment.IsMethodCall),
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
                    //repository.From<Activity>()
                    //.Where(f => f.Id == activityId)
                    //.Select(f => Sql.FlattenTo<ActivityQueryResponse>(() => new
                    //{
                    //    ActivityTypeEnum = this.GetEmnuName(f.ActivityType),
                    //}))
                    //.First()               
                    methodCallField.Index = readerFields.Count;
                    methodCallField.FromMember = memberInfo;
                    methodCallField.TargetMember = memberInfo;
                    readerFields.Add(methodCallField);
                    break;
                }
                else fieldName = this.GetQuotedValue(sqlSegment);
                if (sqlSegment.IsExpression)
                    fieldName = $"({fieldName})";
                if (sqlSegment.IsParameter || sqlSegment.IsExpression || sqlSegment.IsMethodCall || sqlSegment.FromMember?.Name != memberInfo.Name)
                    fieldName += " AS " + this.OrmProvider.GetFieldName(memberInfo.Name);

                readerFields.Add(new ReaderField
                {
                    Index = readerFields.Count,
                    FieldType = ReaderFieldType.Field,
                    TableSegment = sqlSegment.TableSegment,
                    FromMember = memberInfo,
                    TargetMember = memberInfo,
                    Body = fieldName
                });
                break;
        }
    }
    private void AddReaderFields(List<ReaderField> readerFields, StringBuilder builder)
    {
        foreach (var readerField in readerFields)
        {
            if (readerField.FieldType == ReaderFieldType.Entity
                || readerField.FieldType == ReaderFieldType.AnonymousObject)
            {
                if (readerField.ReaderFields == null || readerField.ReaderFields.Count == 0)
                {
                    readerField.TableSegment.Mapper ??= this.MapProvider.GetEntityMap(readerField.TableSegment.EntityType);
                    foreach (var memberMapper in readerField.TableSegment.Mapper.MemberMaps)
                    {
                        if (memberMapper.IsIgnore || memberMapper.IsNavigation
                            || (memberMapper.MemberType.IsEntityType(out _) && memberMapper.TypeHandler == null))
                            continue;

                        if (builder.Length > 0)
                            builder.Append(',');
                        if (this.IsNeedAlias)
                            builder.Append(readerField.TableSegment.AliasName + ".");
                        builder.Append(this.OrmProvider.GetFieldName(memberMapper.FieldName));
                    }
                }
                else this.AddReaderFields(readerField.ReaderFields, builder);
            }
            else
            {
                if (builder.Length > 0)
                    builder.Append(',');
                builder.Append(readerField.Body);
            }
        }
    }
    private string VisitList(LambdaExpression lambdaExpr, bool isGrouping, string suffix)
    {
        if (lambdaExpr.Body is NewExpression newExpr)
        {
            var builder = new StringBuilder();
            int index = 0;
            foreach (var argumentExpr in newExpr.Arguments)
            {
                //OrderBy访问分组
                if (this.IsGroupingAggregateMember(argumentExpr as MemberExpression))
                {
                    for (int i = 0; i < this.groupFields.Count; i++)
                    {
                        if (i > 0) builder.Append(',');
                        builder.Append(this.groupFields[i].Body);
                        builder.Append(suffix);
                    }
                    continue;
                }
                var memberInfo = newExpr.Members[index];
                var sqlSegment = this.VisitAndDeferred(new SqlSegment { Expression = argumentExpr });
                if (builder.Length > 0)
                    builder.Append(',');

                var fieldName = sqlSegment.Value.ToString();
                builder.Append(fieldName);
                if (isGrouping)
                {
                    this.groupFields.Add(new ReaderField
                    {
                        FieldType = ReaderFieldType.Field,
                        //分组字段的原表
                        TableSegment = sqlSegment.TableSegment,
                        FromMember = sqlSegment.FromMember,
                        TargetMember = memberInfo,
                        Body = fieldName
                    });
                }
                builder.Append(suffix);
                index++;
            }
            return builder.ToString();
        }
        else
        {
            var sqlSegment = this.VisitAndDeferred(new SqlSegment { Expression = lambdaExpr.Body });
            var fieldName = sqlSegment.ToString();
            if (isGrouping)
            {
                this.groupFields.Add(new ReaderField
                {
                    FieldType = ReaderFieldType.Field,
                    TableSegment = sqlSegment.TableSegment,
                    FromMember = sqlSegment.FromMember,
                    Body = fieldName
                });
            }
            if (!string.IsNullOrEmpty(suffix))
                return fieldName + suffix;
            return fieldName;
        }
    }
    private TableSegment AddIncludeTables(MemberExpression memberExpr)
    {
        ParameterExpression parameterExpr = null;
        TableSegment tableSegment = null;
        var memberExprs = new Stack<MemberExpression>();

        var memberType = memberExpr.Member.GetMemberType();
        if (!memberType.IsEntityType(out _))
            throw new Exception($"Include方法只支持实体属性，{memberExpr.Member.DeclaringType.FullName}.{memberExpr.Member.Name}不是实体，Path:{memberExpr}");

        var currentExpr = memberExpr;
        while (currentExpr != null)
        {
            memberExprs.Push(currentExpr);
            if (currentExpr.Expression.NodeType == ExpressionType.Parameter)
            {
                parameterExpr = currentExpr.Expression as ParameterExpression;
                break;
            }
            currentExpr = currentExpr.Expression as MemberExpression;
        }

        var fromSegment = this.tableAlias[parameterExpr.Name];
        var fromType = fromSegment.EntityType;
        var builder = new StringBuilder(fromSegment.AliasName);

        while (memberExprs.TryPop(out currentExpr))
        {
            fromSegment.Mapper ??= this.MapProvider.GetEntityMap(fromType);
            var fromMapper = fromSegment.Mapper;
            var memberMapper = fromMapper.GetMemberMap(currentExpr.Member.Name);

            if (!memberMapper.IsNavigation)
                throw new Exception($"实体{fromType.FullName}的属性{currentExpr.Member.Name}未配置为导航属性");

            //TODO:目前只支持1:1关系导航属性做过滤条件，如：
            //f=>f.Order.Buyer.Name.Contains("leafkevin") 或 (a,b)=>a.ProvinceId=b.Order.Buyer.ProvinceId
            //不支持：f=>f.Order.Details.Count > 5
            //场景：Include的filter, Where中的条件,Join关联右侧的条件
            //场景: (a,b)=>a.BuyerId=b.Order.BuyerId

            //最后一个成员访问之前，都必须是1:1关系才可以
            //if (!memberMapper.IsToOne && memberExprs.Count > 0)
            //    throw new Exception($"暂时不支持的Include表达式:{memberExpr}");

            //实体类型是成员的声明类型，映射类型不一定是成员的声明类型，一定是成员的Map类型
            //如：成员是UserInfo类型，对应的模型是User类型，UserInfo类型只是User类型的一个子集，成员名称和映射关系完全一致
            var entityType = memberMapper.NavigationType;
            var entityMapper = this.MapProvider.GetEntityMap(entityType, memberMapper.MapType);
            if (entityMapper.KeyMembers.Count > 1)
                throw new Exception($"导航属性表，暂时不支持多个主键字段，实体：{memberMapper.MapType.FullName}");

            var rightAlias = $"{(char)(this.TableAsStart + this.tables.Count)}";
            builder.Append("." + currentExpr.Member.Name);
            var path = builder.ToString();

            if (memberMapper.IsToOne)
            {
                //TODO:之前有IncludeMany时，也放到includeSegments中,暂时没有处理
                this.tables.Add(tableSegment = new TableSegment
                {
                    //默认外联
                    JoinType = "LEFT JOIN",
                    EntityType = entityType,
                    Mapper = entityMapper,
                    AliasName = rightAlias,
                    FromTable = fromSegment,
                    FromMember = memberMapper,
                    OnExpr = $"{fromSegment.AliasName}.{this.OrmProvider.GetFieldName(memberMapper.ForeignKey)}={rightAlias}.{this.OrmProvider.GetFieldName(entityMapper.KeyMembers[0].FieldName)}",
                    Path = path
                });
            }
            else
            {
                if (fromMapper.KeyMembers.Count > 1)
                    throw new Exception($"导航属性表，暂时不支持多个主键字段，实体：{fromMapper.EntityType.FullName}");
                this.includeSegments ??= new();
                this.includeSegments.Add(tableSegment = new TableSegment
                {
                    FromTable = fromSegment,
                    Mapper = entityMapper,
                    FromMember = memberMapper,
                    Path = path
                });
            }
            fromSegment = tableSegment;
            fromType = memberMapper.NavigationType;
        }
        return tableSegment;
    }
    private string BuildIncludeFetchHeadSql(TableSegment includeSegment)
    {
        var targetType = includeSegment.Mapper.EntityType;
        var foreignKey = includeSegment.FromMember.ForeignKey;
        var cacheKey = HashCode.Combine(this.DbKey, this.OrmProvider, targetType, foreignKey);
        if (!sqlCache.TryGetValue(cacheKey, out var sql))
        {
            int index = 0;
            var builder = new StringBuilder("SELECT ");
            foreach (var memberMapper in includeSegment.Mapper.MemberMaps)
            {
                if (memberMapper.IsIgnore || memberMapper.IsNavigation
                    || (memberMapper.MemberType.IsEntityType(out _) && memberMapper.TypeHandler == null))
                    continue;

                if (index > 0) builder.Append(',');
                builder.Append(this.OrmProvider.GetFieldName(memberMapper.FieldName));
                if (memberMapper.MemberName != memberMapper.FieldName)
                    builder.Append(" AS " + memberMapper.MemberName);
                index++;
            }
            var tableName = this.OrmProvider.GetTableName(includeSegment.Mapper.TableName);
            builder.Append($" FROM {tableName} WHERE {foreignKey} IN (");
            sqlCache.TryAdd(cacheKey, sql = builder.ToString());
        }
        return sql;
    }
    private object BuildAddIncludeFetchSqlInitializer(bool isMulti, Type targetType, TableSegment includeSegment)
    {
        var fromType = includeSegment.FromTable.EntityType;
        includeSegment.FromTable.Mapper ??= this.MapProvider.GetEntityMap(fromType);
        var keyMember = includeSegment.FromTable.Mapper.KeyMembers[0];
        var cacheKey = HashCode.Combine(this.DbKey, this.OrmProvider, targetType, fromType, keyMember.MemberName, isMulti);
        if (!getterCache.TryGetValue(cacheKey, out var fetchSqlInitializer))
        {
            var readerField = this.readerFields.Find(f => f.TableSegment == includeSegment.FromTable);
            var memberInfoStack = new Stack<MemberInfo>();
            var current = readerField;
            while (true)
            {
                if (!current.ParentIndex.HasValue)
                {
                    //最外层，有值就说明是Parameter表达式访问
                    //如：Select((x, y) => new { Order = x, Buyer = y })
                    //没有值通常是单表访问，走默认实体返回 Expression<Func<T, T>> defaultExpr = f => f;
                    if (current.FromMember != null)
                        memberInfoStack.Push(current.FromMember);
                    break;
                }
                memberInfoStack.Push(current.FromMember);
                current = readerFields.Find(f => f.Index == current.ParentIndex.Value);
            }

            var objExpr = Expression.Parameter(typeof(object), "obj");
            var builderExpr = Expression.Parameter(typeof(StringBuilder), "builder");
            var ormProviderExpr = Expression.Parameter(typeof(IOrmProvider), "ormProvider");
            ParameterExpression indexExpr = null;
            if (isMulti) indexExpr = Expression.Parameter(typeof(int), "index");
            var targetExpr = Expression.Variable(targetType, "target");
            var blockParameters = new List<ParameterExpression>();
            var blockBodies = new List<Expression>();

            blockParameters.Add(targetExpr);
            blockBodies.Add(Expression.Assign(targetExpr, Expression.Convert(objExpr, targetType)));

            Expression currentValueExpr = targetExpr;
            MemberInfo memberInfo = null;
            while (memberInfoStack.TryPop(out memberInfo))
            {
                currentValueExpr = Expression.PropertyOrField(currentValueExpr, memberInfo.Name);
            }
            currentValueExpr = Expression.Convert(Expression.PropertyOrField(currentValueExpr, keyMember.MemberName), typeof(object));

            var methedInfo = typeof(IOrmProvider).GetMethod(nameof(IOrmProvider.GetQuotedValue));
            var fieldTypeExpr = Expression.Constant(keyMember.MemberType);
            var keyValueExpr = Expression.Call(ormProviderExpr, methedInfo, fieldTypeExpr, currentValueExpr);

            methedInfo = typeof(StringBuilder).GetMethod(nameof(StringBuilder.Append), new Type[] { typeof(string) });
            var headSql = this.BuildIncludeFetchHeadSql(includeSegment);
            var addSqlExpr = Expression.Call(builderExpr, methedInfo, Expression.Constant(headSql));
            if (isMulti)
            {
                var greaterThanExpr = Expression.GreaterThan(indexExpr, Expression.Constant(0, typeof(int)));
                var addCommaExpr = Expression.Call(builderExpr, methedInfo, Expression.Constant(","));
                //SQL: SELECT * FROM sys_order_detail WHERE OrderId IN (
                blockBodies.Add(Expression.IfThenElse(greaterThanExpr, addCommaExpr, addSqlExpr));
            }
            else blockBodies.Add(addSqlExpr);
            //SQL: SELECT * FROM sys_order_detail WHERE OrderId IN (1,2,3
            blockBodies.Add(Expression.Call(builderExpr, methedInfo, keyValueExpr));

            if (isMulti)
                fetchSqlInitializer = Expression.Lambda<Action<object, IOrmProvider, int, StringBuilder>>(Expression.Block(blockParameters, blockBodies), objExpr, ormProviderExpr, indexExpr, builderExpr).Compile();
            else fetchSqlInitializer = Expression.Lambda<Action<object, IOrmProvider, StringBuilder>>(Expression.Block(blockParameters, blockBodies), objExpr, ormProviderExpr, builderExpr).Compile();
            getterCache.TryAdd(cacheKey, fetchSqlInitializer);
        }
        return fetchSqlInitializer;
    }
    private object BuildIncludeValueSetterInitializer(object parameter, List<TableSegment> includeSegments)
    {
        bool isMulti = false;
        Type targetType = null;
        if (parameter is IEnumerable entities)
        {
            isMulti = true;
            foreach (var entity in entities)
            {
                targetType = entity.GetType();
                break;
            }
        }
        else targetType = parameter.GetType();
        var cacheKey = this.GetIncludeSetterKey(targetType, includeSegments, isMulti);
        if (!setterCache.TryGetValue(cacheKey, out var includeSetterInitializer))
        {
            var objExpr = Expression.Parameter(typeof(object), "obj");
            var readerExpr = Expression.Parameter(typeof(IDataReader), "reader");
            var dbKeyExpr = Expression.Parameter(typeof(string), "dbKey");
            var ormProviderExpr = Expression.Parameter(typeof(IOrmProvider), "ormProvider");
            var mapProviderExpr = Expression.Parameter(typeof(IEntityMapProvider), "mapProvider");
            var includeSegmentsExpr = Expression.Parameter(typeof(List<TableSegment>), "includeSegments");
            var blockParameters = new List<ParameterExpression>();
            var blockBodies = new List<Expression>();

            Type parameterType = null;
            if (isMulti) parameterType = typeof(List<>).MakeGenericType(targetType);
            else parameterType = targetType;
            //可能是单个实体，也可能是多个实体
            var targetExpr = Expression.Variable(parameterType, "target");
            blockParameters.Add(targetExpr);
            blockBodies.Add(Expression.Assign(targetExpr, Expression.Convert(objExpr, parameterType)));

            //Action<object, IDataReader, dbKey, IOrmProvider, IEntityMapProvider, List<IncludeSegment>>
            int index = 1;
            //var includeResult1=new List<OrderDetail>();
            //var includeResult2=new List<OrderDetail>();
            foreach (var includeSegment in includeSegments)
            {
                var includeType = typeof(List<>).MakeGenericType(includeSegment.FromMember.NavigationType);
                var includeResultExpr = Expression.Variable(includeType, $"includeResult{index}");
                blockParameters.Add(includeResultExpr);
                blockBodies.Add(Expression.Assign(includeResultExpr, Expression.New(includeType.GetConstructor(Type.EmptyTypes))));
                index++;
            }

            //while(true)
            //{
            //  if(!reader.Read())break;
            //  includeResult1.Add(reader.To<T>(reader, dbKey, ormProvider, false, readerFields));
            //}
            //reader.NextResult()
            //while(true)
            //{
            //  if(!reader.Read())break;
            //  includeResult2.Add(reader.To<T>(reader, dbKey, ormProvider, false, readerFields));
            //}
            //reader.NextResult()
            var breakLabel = Expression.Label();
            var toMethodInfo = typeof(Extensions).GetMethod(nameof(Extensions.To),
                   BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static,
                   new Type[] { typeof(IDataReader), typeof(string), typeof(IOrmProvider), typeof(IEntityMapProvider) });

            var readMethodInfo = typeof(IDataReader).GetMethod(nameof(IDataReader.Read), Type.EmptyTypes);

            index = 1;
            foreach (var includeSegment in includeSegments)
            {
                var includeResultExpr = blockParameters[index];
                var readBreakLabel = Expression.Label();
                //if(!reader.Read())break;
                var ifFalseExpr = Expression.IsFalse(Expression.Call(readerExpr, readMethodInfo));
                var ifThenBreakExpr = Expression.IfThen(ifFalseExpr, Expression.Break(readBreakLabel));
                var methodInfo = toMethodInfo.MakeGenericMethod(includeSegment.FromMember.NavigationType);
                //reader.To<T>(reader, dbKey, ormProvider, false, readerFields);
                var includeValueExpr = Expression.Call(methodInfo, readerExpr, dbKeyExpr, ormProviderExpr, mapProviderExpr);
                var includeType = typeof(List<>).MakeGenericType(includeSegment.FromMember.NavigationType);
                methodInfo = includeType.GetMethod("Add", new Type[] { includeSegment.FromMember.NavigationType });
                var addValueExpr = Expression.Call(includeResultExpr, methodInfo, includeValueExpr);
                //includeResult1.Add(entity);
                blockBodies.Add(Expression.Loop(Expression.Block(ifThenBreakExpr, addValueExpr), readBreakLabel));
                //reader.NextResult()
                if (index < includeSegments.Count - 1)
                {
                    methodInfo = typeof(IDataReader).GetMethod(nameof(IDataReader.NextResult), Type.EmptyTypes);
                    blockBodies.Add(Expression.Call(readerExpr, methodInfo));
                }
                index++;
            }
            //reader.Close();
            //reader.Dispose();
            var closeMethodInfo = typeof(IDataReader).GetMethod(nameof(IDataReader.Close), Type.EmptyTypes);
            blockBodies.Add(Expression.Call(readerExpr, closeMethodInfo));
            var disposeMethodInfo = typeof(IDisposable).GetMethod(nameof(IDisposable.Dispose), Type.EmptyTypes);
            blockBodies.Add(Expression.Call(readerExpr, disposeMethodInfo));

            var indexExpr = Expression.Variable(typeof(int), "index");
            var countExpr = Expression.Variable(typeof(int), "count");
            //ToList场景
            if (isMulti)
            {
                blockParameters.Add(indexExpr);
                blockParameters.Add(countExpr);
            }

            index = 1;
            foreach (var includeSegment in includeSegments)
            {
                var includeResultExpr = blockParameters[index];
                if (isMulti)
                {
                    //int index=0;int count=result.Count;
                    //if(index==count)break;
                    var readBreakLabel = Expression.Label();
                    blockBodies.Add(Expression.Assign(indexExpr, Expression.Constant(0, typeof(int))));
                    var listTargetType = typeof(List<>).MakeGenericType(targetType);
                    blockBodies.Add(Expression.Assign(countExpr, Expression.Property(targetExpr, "Count")));
                    var ifThenBreakExpr = Expression.IfThen(Expression.Equal(indexExpr, countExpr), Expression.Break(readBreakLabel));

                    var itemPropertyInfo = listTargetType.GetProperty("Item", targetType, new Type[] { typeof(int) });
                    var indexTargetExpr = Expression.MakeIndex(targetExpr, itemPropertyInfo, new[] { indexExpr });
                    var keyValueExpr = this.GetKeyValue(indexTargetExpr, includeSegment);

                    //var orderDetails=includeResult.FindAll(f=>f.OrderId==result[index].Order.Id);                    
                    var predicateType = typeof(Predicate<>).MakeGenericType(includeSegment.FromMember.NavigationType);
                    var parameterExpr = Expression.Parameter(includeSegment.FromMember.NavigationType, "f");
                    var foreignKey = includeSegment.FromMember.ForeignKey;
                    var equalExpr = Expression.Equal(Expression.PropertyOrField(parameterExpr, foreignKey), keyValueExpr);

                    var predicateExpr = Expression.Lambda(predicateType, equalExpr, parameterExpr);
                    var includeType = typeof(List<>).MakeGenericType(includeSegment.FromMember.NavigationType);
                    var methodInfo = includeType.GetMethod("FindAll", new Type[] { predicateType });
                    var filterValuesExpr = Expression.Call(includeResultExpr, methodInfo, predicateExpr);

                    //if(orderDetails!=null && orderDetails.Count>0)
                    //  result[index].Order.Details=orderDetails;
                    var notNullExpr = Expression.NotEqual(filterValuesExpr, Expression.Constant(null));
                    var listCountExpr = Expression.Property(filterValuesExpr, "Count");
                    var greaterThanThenExpr = Expression.GreaterThan(listCountExpr, Expression.Constant(0, typeof(int)));
                    var setValueExpr = this.SetValue(indexTargetExpr, includeSegment, filterValuesExpr);
                    var ifThenExpr = Expression.IfThen(Expression.AndAlso(notNullExpr, greaterThanThenExpr), setValueExpr);
                    var indexIncrementExpr = Expression.Assign(indexExpr, Expression.Increment(indexExpr));

                    //while(true)
                    //{
                    //  if(index==count)break;
                    //  var orderDetails=includeResult.FindAll(f=>f.OrderId==result[index].Order.Id);
                    //  if(orderDetails!=null && orderDetails.Count>0)
                    //      result[index].Order.Details=orderDetails;
                    //  index++;
                    //}
                    blockBodies.Add(Expression.Loop(Expression.Block(ifThenBreakExpr, ifThenExpr, indexIncrementExpr), readBreakLabel));
                }
                else
                {
                    //if(includeResult!=null&&includeResult.Count>0)
                    //  result.Order.Details=includeResult;
                    var notNullExpr = Expression.NotEqual(includeResultExpr, Expression.Constant(null));
                    var listCountExpr = Expression.Property(includeResultExpr, "Count");
                    var greaterThanThenExpr = Expression.GreaterThan(listCountExpr, Expression.Constant(0, typeof(int)));
                    var setValueExpr = this.SetValue(targetExpr, includeSegment, includeResultExpr);
                    blockBodies.Add(Expression.IfThen(Expression.AndAlso(notNullExpr, greaterThanThenExpr), setValueExpr));
                }
                index++;
            }

            includeSetterInitializer = Expression.Lambda<Action<object, IDataReader, string, IOrmProvider, IEntityMapProvider, List<TableSegment>>>(
                Expression.Block(blockParameters, blockBodies), objExpr, readerExpr, dbKeyExpr, ormProviderExpr, mapProviderExpr, includeSegmentsExpr).Compile();
            setterCache.TryAdd(cacheKey, includeSetterInitializer);
        }
        return includeSetterInitializer;
    }
    private Expression SetValue(Expression targetExpr, TableSegment includeSegment, Expression valueExpr)
    {
        var readerField = this.readerFields.Find(f => f.TableSegment == includeSegment.FromTable);
        var memberInfoStack = new Stack<MemberInfo>();
        var current = readerField;
        while (true)
        {
            if (!current.ParentIndex.HasValue)
            {
                //最外层，有值就说明是Parameter表达式访问
                //如：Select((x, y) => new { Order = x, Buyer = y })
                //没有值通常是单表访问，走默认实体返回 Expression<Func<T, T>> defaultExpr = f => f;
                if (current.FromMember != null)
                    memberInfoStack.Push(current.FromMember);
                break;
            }
            memberInfoStack.Push(current.FromMember);
            current = readerFields.Find(f => f.Index == current.ParentIndex.Value);
        }
        Expression currentExpr = targetExpr;
        MemberInfo memberInfo = null;
        while (memberInfoStack.TryPop(out memberInfo))
        {
            currentExpr = Expression.PropertyOrField(currentExpr, memberInfo.Name);
        }
        var memberName = includeSegment.FromMember.MemberName;

        Expression setValueExpr = null;
        switch (includeSegment.FromMember.Member.MemberType)
        {
            case MemberTypes.Field:
                setValueExpr = Expression.Assign(Expression.Field(currentExpr, memberName), valueExpr);
                break;
            case MemberTypes.Property:
                var methodInfo = (includeSegment.FromMember.Member as PropertyInfo).GetSetMethod();
                setValueExpr = Expression.Call(currentExpr, methodInfo, valueExpr);
                break;
            default: throw new NotSupportedException("目前只支持Field或是Property两种成员访问");
        }
        return setValueExpr;
    }
    private Expression GetKeyValue(Expression targetExpr, TableSegment includeSegment)
    {
        var readerField = this.readerFields.Find(f => f.TableSegment == includeSegment.FromTable);
        var memberInfoStack = new Stack<MemberInfo>();
        var current = readerField;
        while (true)
        {
            if (!current.ParentIndex.HasValue)
            {
                //最外层，有值就说明是Parameter表达式访问
                //如：Select((x, y) => new { Order = x, Buyer = y })
                //没有值通常是单表访问，走默认实体返回 Expression<Func<T, T>> defaultExpr = f => f;
                if (current.FromMember != null)
                    memberInfoStack.Push(current.FromMember);
                break;
            }
            memberInfoStack.Push(current.FromMember);
            current = readerFields.Find(f => f.Index == current.ParentIndex.Value);
        }
        Expression currentExpr = targetExpr;
        MemberInfo memberInfo = null;
        while (memberInfoStack.TryPop(out memberInfo))
        {
            currentExpr = Expression.PropertyOrField(currentExpr, memberInfo.Name);
        }
        var memberName = includeSegment.FromTable.Mapper.KeyMembers[0].MemberName;
        return Expression.PropertyOrField(currentExpr, memberName);
    }
    private int GetIncludeSetterKey(Type targetType, List<TableSegment> includeSegments, bool isMulti)
    {
        var hashCode = new HashCode();
        hashCode.Add(this.DbKey);
        hashCode.Add(this.OrmProvider);
        hashCode.Add(targetType);
        hashCode.Add(includeSegments.Count);
        foreach (var includeSegment in includeSegments)
        {
            hashCode.Add(includeSegment.FromTable.EntityType);
            hashCode.Add(includeSegment.FromMember.MemberName);
        }
        hashCode.Add(isMulti);
        return hashCode.ToHashCode();
    }
    private bool IsGroupingAggregateMember(MemberExpression memberExpr)
    {
        if (memberExpr == null) return false;
        return typeof(IAggregateSelect).IsAssignableFrom(memberExpr.Member.DeclaringType) && memberExpr.Member.Name == "Grouping";
    }
}
