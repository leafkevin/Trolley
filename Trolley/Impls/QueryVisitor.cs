using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Trolley;

class QueryVisitor : SqlVisitor
{
    private static ConcurrentDictionary<int, string> sqlCache = new();
    private static ConcurrentDictionary<int, object> getterCache = new();
    private static ConcurrentDictionary<int, object> setterCache = new();
    private string whereSql = string.Empty;
    private string groupBySql = string.Empty;
    private string havingSql = string.Empty;
    private string orderBySql = string.Empty;
    private int? skip = null;
    private int? limit = null;
    private bool isDistinct = false;
    private bool isUnion = false;
    private List<TableSegment> includeSegments = null;
    private TableSegment lastIncludeSegment = null;
    private List<ReaderField> groupFields = null;
    protected internal bool isFromQuery = false;

    public QueryVisitor(string dbKey, IOrmProvider ormProvider, IEntityMapProvider mapProvider, char tableAsStart = 'a', string parameterPrefix = "p")
        : base(dbKey, ormProvider, mapProvider, tableAsStart, parameterPrefix)
    {
        this.tables = new();
        this.tableAlias = new();
    }
    public string BuildSql(out List<IDbDataParameter> dbParameters, out List<ReaderField> readerFields)
    {
        if (this.readerFields == null || this.readerFields.Count == 0)
            this.Select("*");

        var builder = new StringBuilder();
        this.AddReaderFields(this.readerFields, builder);
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
                    tableSegment.Mapper ??= this.mapProvider.GetEntityMap(tableSegment.EntityType);
                    tableName = this.ormProvider.GetTableName(tableSegment.Mapper.TableName);
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

                if (this.isNeedAlias)
                    builder.Append(" " + tableSegment.AliasName);
                if (!string.IsNullOrEmpty(tableSegment.SuffixRawSql))
                    builder.Append(" " + tableSegment.SuffixRawSql);
                if (!string.IsNullOrEmpty(tableSegment.OnExpr))
                    builder.Append($" ON {tableSegment.OnExpr}");

                if (!string.IsNullOrEmpty(tableSegment.Filter))
                {
                    if (!string.IsNullOrEmpty(tableSegment.OnExpr))
                        builder.Append(" AND ");
                    builder.Append(tableSegment.Filter);
                }
            }
            tableSql = builder.ToString();
        }

        builder.Clear();
        if (!string.IsNullOrEmpty(this.whereSql))
            builder.Append(this.whereSql);
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

        if (this.skip.HasValue || this.limit.HasValue)
        {
            //SQL TEMPLATE:SELECT /**fields**/ FROM /**tables**/ /**others**/
            var pageSql = this.ormProvider.GetPagingTemplate(this.skip ?? 0, this.limit, orderBy);
            pageSql = pageSql.Replace("/**fields**/", selectSql);
            pageSql = pageSql.Replace("/**tables**/", tableSql);
            pageSql = pageSql.Replace(" /**others**/", others);
            return $"SELECT COUNT(*) FROM {tableSql}{this.whereSql};{pageSql}";
        }
        else return $"SELECT {selectSql} FROM {tableSql}{others}";
    }
    public string BuildSql(Expression defaultExpr, Type entityType, Expression toTargetExpr, out List<IDbDataParameter> dbParameters, out List<ReaderField> readerFields)
    {
        if (this.readerFields == null || this.readerFields.Count == 0)
            this.Select(null, defaultExpr);

        List<ReaderField> targetFields = null;
        if (toTargetExpr != null)
            targetFields = this.FlattenReaderFields(entityType, toTargetExpr);
        else targetFields = this.readerFields;
        var builder = new StringBuilder();
        this.AddReaderFields(targetFields, builder);

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
                    tableSegment.Mapper ??= this.mapProvider.GetEntityMap(tableSegment.EntityType);
                    tableName = this.ormProvider.GetTableName(tableSegment.Mapper.TableName);
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

                if (this.isNeedAlias)
                    builder.Append(" " + tableSegment.AliasName);
                if (!string.IsNullOrEmpty(tableSegment.SuffixRawSql))
                    builder.Append(" " + tableSegment.SuffixRawSql);

                if (!string.IsNullOrEmpty(tableSegment.OnExpr))
                    builder.Append($" ON {tableSegment.OnExpr}");

                if (!string.IsNullOrEmpty(tableSegment.Filter))
                {
                    if (!string.IsNullOrEmpty(tableSegment.OnExpr))
                        builder.Append(" AND ");
                    builder.Append(tableSegment.Filter);
                }
            }
            tableSql = builder.ToString();
        }

        builder.Clear();
        if (!string.IsNullOrEmpty(this.whereSql))
            builder.Append(this.whereSql);
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
        readerFields = targetFields;

        if (this.skip.HasValue || this.limit.HasValue)
        {
            //SQL TEMPLATE:SELECT /**fields**/ FROM /**tables**/ /**others**/
            var pageSql = this.ormProvider.GetPagingTemplate(this.skip ?? 0, this.limit, orderBy);
            pageSql = pageSql.Replace("/**fields**/", selectSql);
            pageSql = pageSql.Replace("/**tables**/", tableSql);
            pageSql = pageSql.Replace(" /**others**/", others);
            return $"SELECT COUNT(*) FROM {tableSql}{this.whereSql};{pageSql}";
        }
        else return $"SELECT {selectSql} FROM {tableSql}{others}";
    }
    public QueryVisitor Clone(char tableAsStart = 'a', string parameterPrefix = "p")
    {
        var visitor = new QueryVisitor(this.dbKey, this.ormProvider, this.mapProvider, tableAsStart, parameterPrefix);
        visitor.isNeedAlias = this.isNeedAlias;
        return visitor;
    }
    public bool BuildIncludeSql(object parameter, out string sql)
    {
        if (this.includeSegments == null || this.includeSegments.Count <= 0)
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
                    fetchSqlInitializer.Invoke(entity, this.ormProvider, index, builder);
                    index++;
                }
            }
            else
            {
                var fetchSqlInitializer = sqlFetcher as Action<object, IOrmProvider, StringBuilder>;
                fetchSqlInitializer.Invoke(parameter, this.ormProvider, builder);
            }
            builder.Append(')');
            if (!string.IsNullOrEmpty(includeSegment.Filter))
                builder.Append($" AND {includeSegment.Filter}");
        }
        sql = builder.ToString();
        return true;
    }
    public void SetIncludeValues(object parameter, IDataReader reader)
    {
        var valueSetter = this.BuildIncludeValueSetterInitializer(parameter, this.includeSegments);
        var valueSetterInitiliazer = valueSetter as Action<object, IDataReader, string, IOrmProvider, IEntityMapProvider, List<TableSegment>>;
        valueSetterInitiliazer.Invoke(parameter, reader, this.dbKey, this.ormProvider, this.mapProvider, this.includeSegments);
    }
    public QueryVisitor From(params Type[] entityTypes)
    {
        int tableIndex = this.tableAsStart + this.tables.Count;
        for (int i = 0; i < entityTypes.Length; i++)
        {
            this.tables.Add(new TableSegment
            {
                EntityType = entityTypes[i],
                AliasName = $"{(char)(tableIndex + i)}",
                Path = $"{(char)(tableIndex + i)}",
                TableType = TableType.Master,
                IsMaster = true
            });
        }
        if (this.tables.Count > 1) this.isNeedAlias = true;
        return this;
    }
    public QueryVisitor From(char tableAsStart, params Type[] entityTypes)
    {
        this.tableAsStart = tableAsStart;
        this.From(entityTypes);
        return this;
    }
    public QueryVisitor From(char tableAsStart, Type entityType, string suffixRawSql)
    {
        this.tableAsStart = tableAsStart;
        int tableIndex = tableAsStart + this.tables.Count;
        this.tables.Add(new TableSegment
        {
            EntityType = entityType,
            Mapper = this.mapProvider.GetEntityMap(entityType),
            AliasName = $"{(char)tableIndex}",
            SuffixRawSql = suffixRawSql,
            Path = $"{(char)tableIndex}",
            TableType = TableType.Master,
            IsMaster = true
        });
        return this;
    }
    public QueryVisitor AddTable(TableSegment tableSegment)
    {
        this.tables.Add(tableSegment);
        if (this.tables.Count > 1)
            this.isNeedAlias = true;
        return this;
    }
    public QueryVisitor WithTable(Type entityType, string body, List<IDbDataParameter> dbParameters = null, List<ReaderField> readerFields = null, string joinType = "")
    {
        int tableIndex = this.tableAsStart + this.tables.Count;
        if (string.IsNullOrEmpty(joinType) && this.tables.Count > 0)
            joinType = "INNER JOIN";
        var tableSegment = new TableSegment
        {
            JoinType = joinType,
            EntityType = entityType,
            AliasName = $"{(char)tableIndex}",
            Body = $"({body})",
            Path = $"{(char)tableIndex}",
            ReaderFields = readerFields,
            TableType = TableType.MapTable,
            IsMaster = true
        };
        this.tables.Add(tableSegment);
        this.InitMapTableReaderFields(tableSegment, (char)tableIndex, readerFields);

        if (dbParameters != null)
        {
            if (this.dbParameters == null)
                this.dbParameters = dbParameters;
            else this.dbParameters.AddRange(dbParameters);
        }
        return this;
    }
    public void Union(Type entityType, string body, List<IDbDataParameter> dbParameters = null)
    {
        //TODO:清理所有变量值
        this.isUnion = true;
        this.WithTable(entityType, body, dbParameters);
    }
    public void Include(Expression memberSelector, bool isIncludeMany = false, Expression filter = null)
    {
        if (!isIncludeMany) this.isNeedAlias = true;
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
    public void ThenInclude(Expression memberSelector, bool isIncludeMany = false, Expression filter = null)
    {
        if (!isIncludeMany) this.isNeedAlias = true;
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
    public void Join(string joinType, Type newEntityType, Expression joinOn, bool isClearTableAlias = true)
    {
        this.isWhere = true;
        this.isNeedAlias = true;
        var lambdaExpr = joinOn as LambdaExpression;
        TableSegment joinTableSegment = null;
        if (newEntityType != null)
            joinTableSegment = this.AddTable(joinType, newEntityType);
        else joinTableSegment = this.tables.Last();
        if (isClearTableAlias)
            this.InitTableAlias(lambdaExpr);
        var joinOnExpr = this.VisitConditionExpr(lambdaExpr.Body);
        joinTableSegment.OnExpr = joinOnExpr;
        this.isWhere = false;
    }
    public void Select(string sqlFormat, Expression selectExpr = null)
    {
        this.isSelect = true;
        if (selectExpr != null)
        {
            var lambdaExpr = selectExpr as LambdaExpression;
            this.InitTableAlias(lambdaExpr);
            this.readerFields = this.ToTargetReaderFields(lambdaExpr);
        }
        if (!string.IsNullOrEmpty(sqlFormat))
        {
            this.tables.FindAll(f => f.IsMaster).ForEach(f => f.IsUsed = true);
            if (this.readerFields != null && this.readerFields.Count == 1)
            {
                var readerField = this.readerFields[0];
                readerField.Body = string.Format(sqlFormat, readerField.Body);
            }
            else
            {
                this.readerFields ??= new();
                this.readerFields.Add(new ReaderField
                {
                    Index = 0,
                    Body = sqlFormat
                });
            }
        }
        this.isSelect = false;
    }
    public void SelectGrouping() => this.readerFields = this.groupFields;
    public void GroupBy(Expression expr)
    {
        var lambdaExpr = expr as LambdaExpression;
        this.InitTableAlias(lambdaExpr);
        this.groupFields = new();
        this.groupBySql = this.VisitList(lambdaExpr, true, string.Empty);
    }
    public void OrderBy(string orderType, Expression expr)
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
    public void Having(Expression havingExpr)
    {
        this.isWhere = true;
        var lambdaExpr = havingExpr as LambdaExpression;
        this.InitTableAlias(lambdaExpr);
        this.havingSql = this.VisitConditionExpr(lambdaExpr.Body);
        this.isWhere = false;
    }
    public QueryVisitor Page(int pageIndex, int pageSize)
    {
        if (pageIndex > 0) pageIndex--;
        this.skip = pageIndex * pageSize;
        this.limit = pageSize;
        return this;
    }
    public QueryVisitor Skip(int skip)
    {
        this.skip = skip;
        return this;
    }
    public QueryVisitor Take(int limit)
    {
        this.limit = limit;
        return this;
    }
    public QueryVisitor Where(Expression whereExpr, bool isClearTableAlias = true)
    {
        this.isWhere = true;
        var lambdaExpr = whereExpr as LambdaExpression;
        if (isClearTableAlias)
            this.InitTableAlias(lambdaExpr);
        //在Update的Value子查询语句中，更新主表别名是a，引用表别名从b开始，无需别名替换
        this.whereSql = " WHERE " + this.VisitConditionExpr(lambdaExpr.Body);
        this.isWhere = false;
        return this;
    }
    public QueryVisitor And(Expression whereExpr)
    {
        this.isWhere = true;
        var lambdaExpr = whereExpr as LambdaExpression;
        this.InitTableAlias(lambdaExpr);
        this.whereSql += " AND " + this.VisitConditionExpr(lambdaExpr.Body);
        this.isWhere = false;
        return this;
    }
    public void Distinct() => this.isDistinct = true;
    public override SqlSegment VisitParameter(SqlSegment sqlSegment)
    {
        var parameterExpr = sqlSegment.Expression as ParameterExpression;
        var fromSegment = this.tableAlias[parameterExpr.Name];
        var readerFields = this.AddTableReaderFields(sqlSegment.ReaderIndex, fromSegment);
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
            return sqlSegment.Change(readerFields);
        }
        return this.EvaluateAndParameter(sqlSegment);
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
        return sqlSegment.Change(readerFields);
    }
    public override SqlSegment VisitMemberAccess(SqlSegment sqlSegment)
    {
        var memberExpr = sqlSegment.Expression as MemberExpression;
        MemberAccessSqlFormatter formatter = null;
        if (memberExpr.Expression != null)
        {
            if (this.IsGroupingAggregateMember(memberExpr))
            {
                var tableSegment = new TableSegment { EntityType = memberExpr.Type };
                foreach (var readerField in this.groupFields)
                {
                    if (this.isSelect || this.isWhere)
                        readerField.TableSegment.IsUsed = true;
                    readerField.TableSegment = tableSegment;
                }
                return new SqlSegment
                {
                    HasField = true,
                    IsConstantValue = false,
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

            //各种类型值的属性访问，如：DateTime,TimeSpan,String.Length,List.Count
            if (this.ormProvider.TryGetMemberAccessSqlFormatter(sqlSegment, memberExpr.Member, out formatter))
            {
                //Where(f=>... && f.CreatedAt.Month<5 && ...)
                //Where(f=>... && f.Order.OrderNo.Length==10 && ...)
                var targetSegment = this.Visit(sqlSegment.Next(memberExpr.Expression));
                return sqlSegment.Change(formatter.Invoke(targetSegment), false);
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
                if (memberExpr.Type.IsEntityType())
                {
                    var rootTableSegment = this.tableAlias[parameterName];
                    if (rootTableSegment.TableType == TableType.MapTable)
                    {
                        tableSegment = rootTableSegment;
                        var readerField = this.FindReaderField(memberExpr, tableSegment.ReaderFields);
                        return new SqlSegment
                        {
                            HasField = true,
                            IsConstantValue = false,
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
                        fromSegment.Mapper ??= this.mapProvider.GetEntityMap(fromSegment.EntityType);

                        var vavigationMapper = fromSegment.Mapper.GetMemberMap(memberExpr.Member.Name);
                        if (vavigationMapper.IsIgnore)
                            throw new Exception($"类{tableSegment.EntityType.FullName}的成员{memberExpr.Member.Name}是忽略成员无法访问");

                        if (!vavigationMapper.IsNavigation && vavigationMapper.TypeHandler == null)
                            throw new Exception($"类{tableSegment.EntityType.FullName}的成员{memberExpr.Member.Name}不是值类型，未配置为导航属性也没有配置TypeHandler");

                        if (vavigationMapper.IsNavigation)
                        {
                            path = memberExpr.ToString();
                            tableSegment = this.FindTableSegment(parameterName, path, vavigationMapper.IsToOne);
                            if (tableSegment == null)
                                throw new Exception($"使用导航属性前，要先使用Include方法包含进来，访问路径:{path}");

                            if (this.isSelect || this.isWhere)
                            {
                                fromSegment.IsUsed = true;
                                tableSegment.IsUsed = true;
                            }

                            if (vavigationMapper.IsToOne)
                            {
                                var readerFields = this.AddTableReaderFields(sqlSegment.ReaderIndex, tableSegment);
                                return new SqlSegment
                                {
                                    HasField = true,
                                    IsConstantValue = false,
                                    TableSegment = tableSegment,
                                    MemberType = ReaderFieldType.Entity,
                                    FromMember = memberExpr.Member,
                                    Value = new ReaderField
                                    {
                                        FieldType = ReaderFieldType.Entity,
                                        TableSegment = tableSegment,
                                        FromMember = memberExpr.Member,
                                        ReaderFields = readerFields
                                    }
                                };
                            }
                            //else
                            //{
                            //    var fieldName = this.ormProvider.GetFieldName(vavigationMapper.ForeignKey);
                            //    if (this.isNeedAlias && !string.IsNullOrEmpty(fromSegment.AliasName))
                            //        fieldName = fromSegment.AliasName + "." + fieldName;

                            //    //此处先把主表的主键字段查询出来，在第二次查询时，再把此表数据查询出来
                            //    return new SqlSegment
                            //    {
                            //        HasField = true,
                            //        IsConstantValue = false,
                            //        TableSegment = tableSegment,
                            //        Value = new ReaderField
                            //        {
                            //            Type = ReaderFieldType.MasterField,
                            //            TableSegment = fromSegment,
                            //            Body = fieldName
                            //        }
                            //    };
                            //}
                        }
                        else throw new NotSupportedException($"不支持直接访问实体类型成员，需要通过参数访问对应的实体类:{memberExpr.Expression.Type.FullName}");
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
                        sqlSegment.IsConstantValue = false;
                        sqlSegment.TableSegment = readerField.TableSegment;
                        sqlSegment.MemberType = ReaderFieldType.Field;
                        sqlSegment.FromMember = readerField.FromMember;
                        sqlSegment.Value = readerField.Body;
                        return sqlSegment;
                    }

                    string fieldName = null;
                    MemberInfo memberInfo = null;
                    var rootTableSegment = this.tableAlias[parameterName];
                    if (rootTableSegment.TableType == TableType.MapTable)
                    {
                        tableSegment = rootTableSegment;
                        var readerField = this.FindReaderField(memberExpr, tableSegment.ReaderFields);
                        memberInfo = readerField.FromMember;
                        fieldName = readerField.Body;
                    }
                    else
                    {
                        path = memberExpr.Expression.ToString();
                        tableSegment = this.FindTableSegment(parameterName, path);
                        if (tableSegment == null)
                            throw new Exception($"使用导航属性前，要先使用Include方法包含进来，访问路径:{path}");

                        tableSegment.Mapper ??= this.mapProvider.GetEntityMap(tableSegment.EntityType);
                        var memberMapper = tableSegment.Mapper.GetMemberMap(memberExpr.Member.Name);

                        if (memberMapper.IsIgnore)
                            throw new Exception($"类{tableSegment.EntityType.FullName}的成员{memberMapper.MemberName}是忽略成员无法访问");
                        fieldName = this.ormProvider.GetFieldName(memberMapper.FieldName);

                        //有联表时采用别名，如果当前类是IncludeMany的导航类时，没有别名
                        if (this.isNeedAlias && !string.IsNullOrEmpty(tableSegment.AliasName))
                            fieldName = tableSegment.AliasName + "." + fieldName;
                        memberInfo = memberMapper.Member;
                    }
                    if (this.isSelect || this.isWhere)
                        tableSegment.IsUsed = true;

                    sqlSegment.HasField = true;
                    sqlSegment.IsConstantValue = false;
                    sqlSegment.TableSegment = tableSegment;
                    sqlSegment.MemberType = ReaderFieldType.Field;
                    sqlSegment.FromMember = memberInfo;
                    sqlSegment.Value = fieldName;
                    return sqlSegment;
                }
            }
        }

        //各种类型的常量或是静态成员访问，如：DateTime.Now,int.MaxValue,string.Empty
        if (this.ormProvider.TryGetMemberAccessSqlFormatter(sqlSegment, memberExpr.Member, out formatter))
            return sqlSegment.Change(formatter(null), false);

        //访问局部变量或是成员变量，当作常量处理,直接计算，如果是字符串变成参数@p
        //var orderIds=new List<int>{1,2,3}; Where(f=>orderIds.Contains(f.OrderId)); orderIds
        //private Order order; Where(f=>f.OrderId==this.Order.Id); this.Order.Id
        //var orderId=10; Select(f=>new {OrderId=orderId,...}
        //Select(f=>new {OrderId=this.Order.Id, ...}
        return this.EvaluateAndParameter(sqlSegment);
    }
    public TableSegment FindTableSegment(string parameterName, string path, bool isToOne = true)
    {
        var rootTableSegment = this.tableAlias[parameterName];
        var index = path.IndexOf(".");
        if (rootTableSegment.TableType == TableType.MapTable)
        {
            var readerField = rootTableSegment.ReaderFields.Find(f => path.Contains(f.FromMember.Name));
        }
        if (isToOne) return base.FindTableSegment(parameterName, path);

        if (index > 0)
        {

            path = path.Replace(parameterName + ".", rootTableSegment.AliasName + ".");
            return this.includeSegments.Find(f => f.Path == path);
        }
        return null;
    }
    internal TableSegment InitTableAlias(LambdaExpression lambdaExpr)
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

    private void AddSelectElement(Expression elementExpr, MemberInfo memberInfo, List<ReaderField> readerFields)
    {
        string fieldName = null;
        var sqlSegment = new SqlSegment { Expression = elementExpr, ReaderIndex = readerFields.Count };
        switch (elementExpr.NodeType)
        {
            case ExpressionType.Parameter:
                //匿名类临时对象
                if (this.IsGroupingAggregateMember(elementExpr as MemberExpression))
                {
                    foreach (var readerField in this.groupFields)
                    {
                        readerField.TableSegment.IsUsed = true;
                    }
                    readerFields.Add(new ReaderField
                    {
                        Index = readerFields.Count,
                        FieldType = ReaderFieldType.AnonymousObject,
                        FromMember = memberInfo,
                        ReaderFields = this.groupFields
                    });
                }
                else
                {
                    sqlSegment = this.VisitParameter(sqlSegment);
                    var tableReaderFields = sqlSegment.Value as List<ReaderField>;
                    tableReaderFields[0].FromMember = memberInfo;
                    readerFields.AddRange(tableReaderFields);
                }
                break;
            case ExpressionType.New:
                {
                    sqlSegment = this.VisitNew(sqlSegment);
                    var tableReaderFields = sqlSegment.Value as List<ReaderField>;
                    tableReaderFields[0].FromMember = memberInfo;
                    readerFields.AddRange(tableReaderFields);
                }
                break;
            case ExpressionType.MemberInit:
                if (this.IsGroupingAggregateMember(elementExpr as MemberExpression))
                {
                    foreach (var readerField in this.groupFields)
                    {
                        readerField.TableSegment.IsUsed = true;
                    }
                    readerFields.Add(new ReaderField
                    {
                        Index = readerFields.Count,
                        FieldType = ReaderFieldType.AnonymousObject,
                        FromMember = memberInfo,
                        ReaderFields = this.groupFields
                    });
                }
                else
                {
                    sqlSegment = this.VisitMemberInit(sqlSegment);
                    var tableReaderFields = sqlSegment.Value as List<ReaderField>;
                    tableReaderFields[0].FromMember = memberInfo;
                    readerFields.AddRange(tableReaderFields);
                }
                break;
            case ExpressionType.MemberAccess:
                if (elementExpr.Type.IsEntityType())
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
                            if (readerField.FromMember.Name != readerField.TargetMember.Name)
                                readerField.Body += " AS " + readerField.TargetMember.Name;
                        }
                        readerFields.Add(new ReaderField
                        {
                            Index = readerFields.Count,
                            FieldType = ReaderFieldType.AnonymousObject,
                            FromMember = memberInfo,
                            ReaderFields = this.groupFields
                        });
                    }
                    else
                    {
                        sqlSegment = this.VisitMemberAccess(sqlSegment);
                        readerFields.Add(sqlSegment.Value as ReaderField);
                    }
                }
                else
                {
                    sqlSegment = this.VisitAndDeferred(sqlSegment);
                    fieldName = sqlSegment.ToString();
                    if (sqlSegment.IsExpression && sqlSegment.IsNeedParentheses)
                        fieldName = $"({fieldName})";
                    if (sqlSegment.IsParameter || sqlSegment.IsExpression || sqlSegment.FromMember?.Name != memberInfo.Name)
                    {
                        fieldName += " AS ";
                        if (this.isFromQuery && this.ormProvider.DatabaseType == DatabaseType.Postgresql)
                            fieldName += this.ormProvider.GetFieldName(memberInfo.Name);
                        else fieldName += memberInfo.Name;
                    }
                    readerFields.Add(new ReaderField
                    {
                        Index = readerFields.Count,
                        FieldType = ReaderFieldType.Field,
                        TableSegment = sqlSegment.TableSegment,
                        FromMember = memberInfo,
                        Body = fieldName
                    });
                }
                break;
            default:
                //常量或方法或表达式访问
                sqlSegment = this.VisitAndDeferred(sqlSegment);
                fieldName = sqlSegment.ToString();
                if (sqlSegment.IsExpression && sqlSegment.IsNeedParentheses)
                    fieldName = $"({fieldName})";
                if (sqlSegment.IsParameter || sqlSegment.IsExpression || sqlSegment.FromMember?.Name != memberInfo.Name)
                {
                    fieldName += " AS ";
                    if (this.isFromQuery && this.ormProvider.DatabaseType == DatabaseType.Postgresql)
                        fieldName += this.ormProvider.GetFieldName(memberInfo.Name);
                    else fieldName += memberInfo.Name;
                }
                readerFields.Add(new ReaderField
                {
                    Index = readerFields.Count,
                    FieldType = ReaderFieldType.Field,
                    TableSegment = sqlSegment.TableSegment,
                    FromMember = memberInfo,
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
                    readerField.TableSegment.Mapper ??= this.mapProvider.GetEntityMap(readerField.TableSegment.EntityType);
                    foreach (var memberMapper in readerField.TableSegment.Mapper.MemberMaps)
                    {
                        if (memberMapper.IsIgnore || memberMapper.IsNavigation
                            || (memberMapper.MemberType.IsEntityType() && memberMapper.TypeHandler == null))
                            continue;

                        if (builder.Length > 0)
                            builder.Append(',');
                        if (this.isNeedAlias)
                            builder.Append(readerField.TableSegment.AliasName + ".");
                        builder.Append(this.ormProvider.GetFieldName(memberMapper.FieldName));
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
                var sqlSegment = this.Visit(new SqlSegment { Expression = argumentExpr });
                if (builder.Length > 0)
                    builder.Append(',');

                var fieldName = sqlSegment.Value.ToString();
                builder.Append(fieldName);
                if (isGrouping)
                {
                    this.groupFields.Add(new ReaderField
                    {
                        FieldType = ReaderFieldType.Field,
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
            var sqlSegment = this.Visit(new SqlSegment { Expression = lambdaExpr.Body });
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
    private TableSegment AddTable(string joinType, Type entityType)
    {
        int tableIndex = this.tableAsStart + this.tables.Count;
        var tableSegment = new TableSegment
        {
            EntityType = entityType,
            AliasName = $"{(char)tableIndex}",
            Path = $"{(char)tableIndex}",
            JoinType = joinType,
            TableType = TableType.Master,
            IsMaster = true
        };
        this.tables.Add(tableSegment);
        return tableSegment;
    }
    private void InitMapTableReaderFields(TableSegment tableSegment, char tableAlias, List<ReaderField> readerFields)
    {
        foreach (var readerField in readerFields)
        {
            if (readerField.FieldType == ReaderFieldType.Entity
                || readerField.FieldType == ReaderFieldType.AnonymousObject)
            {
                readerField.TableSegment = tableSegment;
                this.InitMapTableReaderFields(tableSegment, tableAlias, readerField.ReaderFields);
            }
            else
            {
                readerField.TableSegment = tableSegment;
                var memberInfo = readerField.TargetMember ?? readerField.FromMember;
                var fieldName = this.ormProvider.GetFieldName(memberInfo.Name);
                readerField.Body = $"{tableAlias}.{fieldName}";
            }
        }
    }
    private TableSegment AddIncludeTables(MemberExpression memberExpr)
    {
        ParameterExpression parameterExpr = null;
        TableSegment tableSegment = null;
        var memberExprs = new Stack<MemberExpression>();

        var memberType = memberExpr.Member.GetMemberType();
        if (!memberType.IsEntityType())
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
            fromSegment.Mapper ??= this.mapProvider.GetEntityMap(fromType);
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
            var entityMapper = this.mapProvider.GetEntityMap(entityType, memberMapper.MapType);
            if (entityMapper.KeyMembers.Count > 1)
                throw new Exception($"导航属性表，暂时不支持多个主键字段，实体：{memberMapper.MapType.FullName}");

            var rightAlias = $"{(char)(this.tableAsStart + this.tables.Count)}";
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
                    OnExpr = $"{fromSegment.AliasName}.{this.ormProvider.GetFieldName(memberMapper.ForeignKey)}={rightAlias}.{this.ormProvider.GetFieldName(entityMapper.KeyMembers[0].FieldName)}",
                    //IsInclude = true,
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
        var cacheKey = HashCode.Combine(this.dbKey, this.ormProviderType, targetType, foreignKey);
        if (!sqlCache.TryGetValue(cacheKey, out var sql))
        {
            int index = 0;
            var builder = new StringBuilder("SELECT ");
            foreach (var memberMapper in includeSegment.Mapper.MemberMaps)
            {
                if (memberMapper.IsIgnore || memberMapper.IsNavigation
                    || (memberMapper.MemberType.IsEntityType() && memberMapper.TypeHandler == null))
                    continue;

                if (index > 0) builder.Append(',');
                builder.Append(this.ormProvider.GetFieldName(memberMapper.FieldName));
                if (memberMapper.MemberName != memberMapper.FieldName)
                    builder.Append(" AS " + memberMapper.MemberName);
                index++;
            }
            var tableName = this.ormProvider.GetTableName(includeSegment.Mapper.TableName);
            builder.Append($" FROM {tableName} WHERE {foreignKey} IN (");
            sqlCache.TryAdd(cacheKey, sql = builder.ToString());
        }
        return sql;
    }
    private object BuildAddIncludeFetchSqlInitializer(bool isMulti, Type targetType, TableSegment includeSegment)
    {
        var fromType = includeSegment.FromTable.EntityType;
        includeSegment.FromTable.Mapper ??= this.mapProvider.GetEntityMap(fromType);
        var keyMember = includeSegment.FromTable.Mapper.KeyMembers[0];
        var cacheKey = HashCode.Combine(this.dbKey, this.ormProviderType, targetType, fromType, keyMember.MemberName, isMulti);
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
                var ifFalseExpr = Expression.IsFalse(Expression.Call(readerExpr, readMethodInfo));
                var ifThenBreakExpr = Expression.IfThen(ifFalseExpr, Expression.Break(readBreakLabel));
                var methodInfo = toMethodInfo.MakeGenericMethod(includeSegment.FromMember.NavigationType);
                var includeValueExpr = Expression.Call(methodInfo, readerExpr, dbKeyExpr, ormProviderExpr, mapProviderExpr);
                var includeType = typeof(List<>).MakeGenericType(includeSegment.FromMember.NavigationType);
                methodInfo = includeType.GetMethod("Add", new Type[] { includeSegment.FromMember.NavigationType });
                var addValueExpr = Expression.Call(includeResultExpr, methodInfo, includeValueExpr);
                blockBodies.Add(Expression.Loop(Expression.Block(ifThenBreakExpr, addValueExpr), readBreakLabel));
                if (index < includeSegments.Count - 1)
                {
                    methodInfo = typeof(IDataReader).GetMethod(nameof(IDataReader.NextResult), Type.EmptyTypes);
                    blockBodies.Add(Expression.Call(readerExpr, methodInfo));
                }
                index++;
            }
            var closeMethodInfo = typeof(IDataReader).GetMethod(nameof(IDataReader.Close), Type.EmptyTypes);
            blockBodies.Add(Expression.Call(readerExpr, closeMethodInfo));
            var disposeMethodInfo = typeof(IDisposable).GetMethod(nameof(IDisposable.Dispose), Type.EmptyTypes);
            blockBodies.Add(Expression.Call(readerExpr, disposeMethodInfo));

            var indexExpr = Expression.Variable(typeof(int), "index");
            var countExpr = Expression.Variable(typeof(int), "count");
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
        hashCode.Add(this.dbKey);
        hashCode.Add(this.ormProviderType);
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
