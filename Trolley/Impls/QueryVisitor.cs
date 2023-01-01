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
    private static readonly MethodInfo concatMethodInfo = typeof(string).GetMethod(nameof(string.Concat), new Type[] { typeof(object[]) });
    private List<ReaderField> readerFields;
    private string selectSql = string.Empty;
    private string whereSql = string.Empty;
    private string groupBySql = string.Empty;
    private string havingSql = string.Empty;
    private string orderBySql = string.Empty;
    private int? skip = null;
    private int? limit = null;
    private bool isDistinct = false;
    private bool isUnion = false;
    private List<IncludeSegment> includeSegments = null;

    public QueryVisitor(IOrmDbFactory dbFactory, TheaConnection connection, IDbTransaction transaction, char tableStartAs = 'a', string parameterPrefix = "p")
        : base(dbFactory, connection, transaction, tableStartAs, parameterPrefix)
    {
        this.tables = new();
        this.tableAlias = new();
    }
    public string BuildSql(out List<IDbDataParameter> dbParameters, out List<ReaderField> readerFields)
    {
        var builder = new StringBuilder();
        string tableSql = null;

        if (this.tables.Count > 0)
        {
            foreach (var tableSegment in this.tables)
            {
                var tableName = tableSegment.Body;
                if (string.IsNullOrEmpty(tableName))
                {
                    tableSegment.Mapper ??= this.dbFactory.GetEntityMap(tableSegment.EntityType);
                    tableName = this.ormProvider.GetTableName(tableSegment.Mapper.TableName);
                }
                if (builder.Length > 0) builder.Append(' ');
                builder.Append($"{tableSegment.JoinType} {tableName} {tableSegment.AliasName}");

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

        if (string.IsNullOrEmpty(this.selectSql))
            throw new Exception("丢失Select操作");

        builder.Clear();
        if (!string.IsNullOrEmpty(this.groupBySql))
        {
            if (builder.Length > 0) builder.Append(' ');
            builder.Append($"GROUP BY {this.groupBySql}");
        }
        if (!string.IsNullOrEmpty(this.havingSql))
        {
            if (builder.Length > 0) builder.Append(' ');
            builder.Append($"HAVING {this.havingSql}");
        }
        if (builder.Length > 0) builder.Append(' ');
        string others = builder.ToString();
        string orderBy = null;
        if (!string.IsNullOrEmpty(this.orderBySql))
            orderBy = $" ORDER BY {this.orderBySql}";

        dbParameters = this.dbParameters;
        readerFields = this.readerFields;

        if (this.skip.HasValue || this.limit.HasValue)
        {
            //SQL TEMPLATE:SELECT /**fields**/ FROM /**tables**/ WHERE /**conditions**/");
            var pageSql = this.ormProvider.GetPagingTemplate(this.skip ?? 0, this.limit, orderBy);
            pageSql = pageSql.Replace("/**fields**/", this.selectSql);
            pageSql = pageSql.Replace("/**tables**/", tableSql);
            pageSql = pageSql.Replace("/**conditions**/", this.whereSql);
            return $"SELECT COUNT(*) FROM {tableSql};{pageSql}{others}";
        }
        else return $"SELECT {this.selectSql} FROM {tableSql}{this.whereSql}{others}{orderBy}";
    }
    public string BuildSql(Expression defaultExpr, out List<IDbDataParameter> dbParameters, out List<ReaderField> readerFields)
    {
        var builder = new StringBuilder();
        string tableSql = null;

        if (string.IsNullOrEmpty(this.selectSql))
            this.Select(null, defaultExpr);

        if (this.tables.Count > 0)
        {
            foreach (var tableSegment in this.tables)
            {
                if (!tableSegment.IsUsed) continue;
                var tableName = tableSegment.Body;
                if (string.IsNullOrEmpty(tableName))
                {
                    tableSegment.Mapper ??= this.dbFactory.GetEntityMap(tableSegment.EntityType);
                    tableName = this.ormProvider.GetTableName(tableSegment.Mapper.TableName);
                }
                if (builder.Length > 0) builder.Append(' ');
                if (!string.IsNullOrEmpty(tableSegment.JoinType))
                    builder.Append($"{tableSegment.JoinType} ");
                builder.Append($"{tableName} {tableSegment.AliasName}");

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
        if (!string.IsNullOrEmpty(this.groupBySql))
        {
            if (builder.Length > 0) builder.Append(' ');
            builder.Append($"GROUP BY {this.groupBySql}");
        }
        if (!string.IsNullOrEmpty(this.havingSql))
        {
            if (builder.Length > 0) builder.Append(' ');
            builder.Append($"HAVING {this.havingSql}");
        }
        if (builder.Length > 0) builder.Append(' ');
        string others = builder.ToString();
        string orderBy = null;
        if (!string.IsNullOrEmpty(this.orderBySql))
            orderBy = $"ORDER BY {this.orderBySql}";

        dbParameters = this.dbParameters = new List<IDbDataParameter> { this.ormProvider.CreateParameter("@p", "111") };
        readerFields = this.readerFields;

        if (this.skip.HasValue || this.limit.HasValue)
        {
            //SQL TEMPLATE:SELECT /**fields**/ FROM /**tables**/ WHERE /**conditions**/");
            var pageSql = this.ormProvider.GetPagingTemplate(this.skip ?? 0, this.limit, orderBy);
            pageSql = pageSql.Replace("/**fields**/", this.selectSql);
            pageSql = pageSql.Replace("/**tables**/", tableSql);
            pageSql = pageSql.Replace("/**others**/", this.whereSql);
            return $"SELECT COUNT(*) FROM {tableSql};{pageSql}{others}";
        }
        else return $"SELECT {this.selectSql} FROM {tableSql}{this.whereSql}{others} {orderBy}";
    }
    public bool BuildIncludeSql(object parameter, out string sql)
    {
        if (this.includeSegments == null || this.includeSegments.Count <= 0)
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
            var sqlFetcher = BuildAddIncludeFetchSqlInitializer(isMulti, targetType, includeSegment);
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
        }
        sql = builder.ToString();
        return true;
    }
    public void SetIncludeValues(object parameter, IDataReader reader, TheaConnection connection)
    {
        var valueSetter = this.BuildIncludeValueSetterInitializer(parameter, this.includeSegments);
        var valueSetterInitiliazer = valueSetter as Action<object, IDataReader, IOrmDbFactory, TheaConnection, List<IncludeSegment>>;
        valueSetterInitiliazer.Invoke(parameter, reader, this.dbFactory, connection, this.includeSegments);
    }
    public QueryVisitor From(params Type[] entityTypes)
    {
        for (int i = 0; i < entityTypes.Length; i++)
        {
            this.tables.Add(new TableSegment
            {
                EntityType = entityTypes[i],
                AliasName = $"{(char)(this.tableStartAs + i)}",
                Path = $"{(char)(this.tableStartAs + i)}"
            });
        }
        return this;
    }
    public QueryVisitor WithTable(Type entityType, string body, List<IDbDataParameter> dbParameters = null, string joinType = "")
    {
        int tableIndex = this.tableStartAs + this.tables.Count;
        if (this.tables.Count > 0)
            joinType = "INNER JOIN";

        this.tables.Add(new TableSegment
        {
            JoinType = joinType,
            EntityType = entityType,
            AliasName = $"{(char)tableIndex}",
            Body = $"({body})",
            Path = $"{(char)tableIndex}"
        });
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
        this.isUnion = true;
        this.WithTable(entityType, body, dbParameters);
    }
    public void Include(Expression memberSelector, Expression filter = null)
    {
        var lambdaExpr = memberSelector as LambdaExpression;
        var memberExpr = lambdaExpr.Body as MemberExpression;
        this.InitTableAlias(lambdaExpr);
        var tableSegment = this.AddIncludeTables(memberExpr);
        //TODO: 1:N关联条件的alias表，获取会有问题，待测试
        if (filter != null)
            tableSegment.Filter = this.Visit(new SqlSegment { Expression = filter }).ToString();
    }
    public void ThenInclude(Expression memberSelector, Expression filter = null)
    {
        var lambdaExpr = memberSelector as LambdaExpression;
        var memberExpr = lambdaExpr.Body as MemberExpression;
        lambdaExpr.Body.GetParameters(out var parameters);
        this.tableAlias.Clear();
        this.tableAlias.Add(parameters[0], this.tables.Last(f => f.IsInclude));
        var tableSegment = this.AddIncludeTables(memberExpr);
        //TODO: 1:N关联条件的alias表，获取会有问题，待测试
        if (filter != null)
            tableSegment.Filter = this.Visit(new SqlSegment { Expression = filter }).ToString();
    }
    public void Join(string joinType, Type newEntityType, Expression joinOn)
    {
        var lambdaExpr = joinOn as LambdaExpression;
        if (newEntityType != null)
            this.AddTable(joinType, newEntityType);
        var joinTableSegment = this.InitTableAlias(lambdaExpr);
        var joinOnExpr = this.VisitConditionExpr(lambdaExpr.Body);
        joinTableSegment.JoinType = joinType;
        joinTableSegment.OnExpr = joinOnExpr;
    }
    public void Select(string sqlFormat, Expression selectExpr = null)
    {
        string body = null;
        if (selectExpr != null)
        {
            var lambdaExpr = selectExpr as LambdaExpression;
            this.InitTableAlias(lambdaExpr);
            StringBuilder builder = null;
            SqlSegment sqlSegment = null;
            switch (lambdaExpr.Body.NodeType)
            {
                case ExpressionType.MemberAccess:
                    sqlSegment = this.Visit(new SqlSegment { Expression = lambdaExpr.Body });
                    sqlSegment.TableSegment.IsUsed = true;
                    body = sqlSegment.ToString();
                    break;
                case ExpressionType.New:
                    var newExpr = lambdaExpr.Body as NewExpression;
                    builder = new StringBuilder();
                    readerFields = new List<ReaderField>();
                    for (int i = 0; i < newExpr.Arguments.Count; i++)
                    {
                        this.AddSelectElement(newExpr.Arguments[i], newExpr.Members[i], this.readerFields);
                    }
                    this.AddReaderFields(this.readerFields, builder);
                    body = builder.ToString();
                    break;
                case ExpressionType.MemberInit:
                    var memberInitExpr = lambdaExpr.Body as MemberInitExpression;
                    builder = new StringBuilder();
                    readerFields = new List<ReaderField>();
                    for (int i = 0; i < memberInitExpr.Bindings.Count; i++)
                    {
                        var memberAssignment = memberInitExpr.Bindings[i] as MemberAssignment;
                        this.AddSelectElement(memberAssignment.Expression, memberAssignment.Member, this.readerFields);
                    }
                    this.AddReaderFields(this.readerFields, builder);
                    body = builder.ToString();
                    break;
                case ExpressionType.Parameter:
                    sqlSegment = this.VisitParameter(new SqlSegment { Expression = lambdaExpr.Body, ReaderIndex = 0 });
                    builder = new StringBuilder();
                    this.readerFields = sqlSegment.Value as List<ReaderField>;
                    this.AddReaderFields(this.readerFields, builder);
                    body = builder.ToString();
                    break;
            }
        }
        if (!string.IsNullOrEmpty(sqlFormat))
            this.selectSql = string.Format(sqlFormat, body);
        else this.selectSql = body;
    }
    public void GroupBy(Expression expr)
    {
        var lambdaExpr = expr as LambdaExpression;
        this.InitTableAlias(lambdaExpr);
        this.groupBySql = this.VisitList(lambdaExpr, string.Empty);
    }
    public void OrderBy(string orderType, Expression expr)
    {
        var lambdaExpr = expr as LambdaExpression;
        this.InitTableAlias(lambdaExpr);
        var orderBy = this.VisitList(lambdaExpr, orderType == "DESC" ? " DESC" : string.Empty);
        if (string.IsNullOrEmpty(this.orderBySql))
            this.orderBySql = orderBy;
        else this.orderBySql += "," + orderBy;
    }
    public void Having(Expression havingExpr)
    {
        var lambdaExpr = havingExpr as LambdaExpression;
        this.InitTableAlias(lambdaExpr);
        this.havingSql = this.VisitConditionExpr(lambdaExpr.Body);
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
    public QueryVisitor Where(Expression whereExpr)
    {
        var lambdaExpr = whereExpr as LambdaExpression;
        this.InitTableAlias(lambdaExpr);
        this.whereSql = " WHERE " + this.VisitConditionExpr(lambdaExpr.Body);
        return this;
    }
    public QueryVisitor And(Expression whereExpr)
    {
        var lambdaExpr = whereExpr as LambdaExpression;
        this.InitTableAlias(lambdaExpr);
        this.whereSql += " AND " + this.VisitConditionExpr(lambdaExpr.Body);
        return this;
    }
    public void Distinct() => this.isDistinct = true;
    public override SqlSegment VisitNew(SqlSegment sqlSegment)
    {
        var newExpr = sqlSegment.Expression as NewExpression;
        if (newExpr.Type.Name.StartsWith("<>"))
        {
            var memberSegments = new List<ReaderField>();
            for (int i = 0; i < newExpr.Arguments.Count; i++)
            {
                this.AddSelectElement(newExpr.Arguments[i], newExpr.Members[i], memberSegments);
            }
            return sqlSegment.Change(memberSegments);
        }
        return this.EvaluateAndParameter(sqlSegment);
    }
    public override SqlSegment VisitMemberInit(SqlSegment sqlSegment)
    {
        var memberInitExpr = sqlSegment.Expression as MemberInitExpression;
        var memberSegments = new List<ReaderField>();
        for (int i = 0; i < memberInitExpr.Bindings.Count; i++)
        {
            if (memberInitExpr.Bindings[i].BindingType != MemberBindingType.Assignment)
                throw new Exception("暂时不支持除MemberBindingType.Assignment类型外的成员绑定表达式");
            var memberAssignment = memberInitExpr.Bindings[i] as MemberAssignment;
            this.AddSelectElement(memberAssignment.Expression, memberAssignment.Member, memberSegments);
        }
        return sqlSegment.Change(memberSegments);
    }
    private void AddSelectElement(Expression elementExpr, MemberInfo memberInfo, List<ReaderField> readerFields)
    {
        string fieldName = null;
        var sqlSegment = new SqlSegment { Expression = elementExpr, ReaderIndex = readerFields.Count };
        switch (elementExpr.NodeType)
        {
            case ExpressionType.Parameter:
            case ExpressionType.MemberAccess:
                sqlSegment = this.Visit(sqlSegment);
                if (elementExpr.Type.IsEntityType())
                {
                    var tableReaderFields = sqlSegment.Value as List<ReaderField>;
                    tableReaderFields[0].FromMember = memberInfo;
                    readerFields.AddRange(tableReaderFields);
                }
                else
                {
                    fieldName = sqlSegment.ToString();
                    if (sqlSegment.IsParameter || sqlSegment.IsMethodCall || sqlSegment.MemberMapper.MemberName != memberInfo.Name)
                        fieldName += " AS " + memberInfo.Name;
                    readerFields.Add(new ReaderField
                    {
                        Index = readerFields.Count,
                        Type = ReaderFieldType.Field,
                        TableSegment = sqlSegment.TableSegment,
                        FromMember = memberInfo,
                        Body = fieldName
                    });
                }
                break;
            default:
                //常量访问
                sqlSegment = this.Visit(sqlSegment);
                fieldName = sqlSegment.ToString();
                if (sqlSegment.IsParameter || sqlSegment.IsMethodCall || sqlSegment.MemberMapper.MemberName != memberInfo.Name)
                    fieldName += " AS " + memberInfo.Name;
                readerFields.Add(new ReaderField
                {
                    Index = readerFields.Count,
                    Type = ReaderFieldType.Field,
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
            readerField.TableSegment.IsUsed = true;
            if (readerField.Type == ReaderFieldType.Entity)
            {
                readerField.TableSegment.Mapper ??= this.dbFactory.GetEntityMap(readerField.TableSegment.EntityType);
                foreach (var memberMapper in readerField.TableSegment.Mapper.MemberMaps)
                {
                    if (memberMapper.IsNavigation || memberMapper.MemberType.IsEntityType())
                        continue;
                    if (builder.Length > 0)
                        builder.Append(',');
                    builder.Append(readerField.TableSegment.AliasName + ".");
                    builder.Append(this.ormProvider.GetFieldName(memberMapper.FieldName));
                }
            }
            else
            {
                if (builder.Length > 0)
                    builder.Append(',');
                builder.Append(readerField.Body);
            }
        }
    }
    private string VisitList(LambdaExpression lambdaExpr, string suffix)
    {
        if (lambdaExpr.Body is NewExpression newExpr)
        {
            var builder = new StringBuilder();
            foreach (var argumentExpr in newExpr.Arguments)
            {
                var fieldName = this.Visit(new SqlSegment { Expression = argumentExpr }).ToString();
                if (builder.Length > 0)
                    builder.Append(", ");
                builder.Append(fieldName);
                builder.Append(suffix);
            }
            return builder.ToString();
        }
        else return this.Visit(new SqlSegment { Expression = lambdaExpr.Body }).ToString();
    }
    private TableSegment InitTableAlias(LambdaExpression lambdaExpr)
    {
        TableSegment tableSegment = null;
        this.tableAlias.Clear();
        lambdaExpr.Body.GetParameters(out var parameters);
        var joinTables = this.tables.FindAll(f => !f.IsInclude);
        for (int i = 0; i < lambdaExpr.Parameters.Count; i++)
        {
            var parameterName = lambdaExpr.Parameters[i].Name;
            if (!parameters.Contains(parameterName))
                continue;
            this.tableAlias.Add(parameterName, joinTables[i]);
            tableSegment = joinTables[i];
        }
        return tableSegment;
    }
    private TableSegment AddTable(string joinType, Type entityType)
    {
        int tableIndex = this.tableStartAs + this.tables.Count;
        var tableSegment = new TableSegment
        {
            EntityType = entityType,
            AliasName = $"{(char)tableIndex}",
            Path = $"{(char)tableIndex}",
            JoinType = joinType
        };
        this.tables.Add(tableSegment);
        return tableSegment;
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
            fromSegment.Mapper ??= this.dbFactory.GetEntityMap(fromType);
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
            var entityMapper = this.dbFactory.GetEntityMap(entityType, memberMapper.MapType);
            if (entityMapper.KeyMembers.Count > 1)
                throw new Exception($"导航属性表，暂时不支持多个主键字段，实体：{memberMapper.MapType.FullName}");

            var rightAlias = $"{(char)('a' + this.tables.Count)}";
            builder.Append("." + currentExpr.Member.Name);
            var path = builder.ToString();
            if (memberMapper.IsToOne)
            {
                this.tables.Add(tableSegment = new TableSegment
                {
                    //默认外联
                    JoinType = "LEFT JOIN",
                    EntityType = entityType,
                    Mapper = entityMapper,
                    AliasName = rightAlias,
                    IncludedFrom = fromSegment,
                    FromMember = memberMapper.Member,
                    OnExpr = $"{fromSegment.AliasName}.{this.ormProvider.GetFieldName(memberMapper.ForeignKey)}={rightAlias}.{this.ormProvider.GetFieldName(entityMapper.KeyMembers[0].FieldName)}",
                    IsInclude = true,
                    Path = path
                });
            }
            else
            {
                if (fromMapper.KeyMembers.Count > 1)
                    throw new Exception($"导航属性表，暂时不支持多个主键字段，实体：{fromMapper.EntityType.FullName}");
                var targetMapper = this.dbFactory.GetEntityMap(memberMapper.NavigationType);
                this.includeSegments ??= new();
                this.includeSegments.Add(new IncludeSegment
                {
                    FromTable = fromSegment,
                    TargetMapper = targetMapper,
                    IncludeMember = memberMapper
                });
            }
            fromSegment = tableSegment;
            fromType = memberMapper.NavigationType;
        }
        return tableSegment;
    }
    private string BuildIncludeFetchHeadSql(IncludeSegment includeSegment)
    {
        var targetType = includeSegment.TargetMapper.EntityType;
        var foreignKey = includeSegment.IncludeMember.ForeignKey;
        var cacheKey = HashCode.Combine(targetType, foreignKey);
        if (!sqlCache.TryGetValue(cacheKey, out var sql))
        {
            int index = 0;
            var builder = new StringBuilder("SELECT ");
            foreach (var memberMapper in includeSegment.TargetMapper.MemberMaps)
            {
                if (memberMapper.IsIgnore || memberMapper.IsNavigation || memberMapper.MemberType.IsEntityType())
                    continue;
                if (index > 0) builder.Append(',');
                builder.Append(this.ormProvider.GetFieldName(memberMapper.FieldName));
                if (memberMapper.MemberName != memberMapper.FieldName)
                    builder.Append(" AS " + memberMapper.MemberName);
                index++;
            }
            var tableName = this.ormProvider.GetTableName(includeSegment.TargetMapper.TableName);
            builder.Append($" FROM {tableName} WHERE {foreignKey} IN (");
            sqlCache.TryAdd(cacheKey, sql = builder.ToString());
        }
        return sql;
    }
    private object BuildAddIncludeFetchSqlInitializer(bool isMulti, Type targetType, IncludeSegment includeSegment)
    {
        var fromType = includeSegment.FromTable.EntityType;
        includeSegment.FromTable.Mapper ??= this.dbFactory.GetEntityMap(fromType);
        var keyMember = includeSegment.FromTable.Mapper.KeyMembers[0];
        var cacheKey = HashCode.Combine(targetType, fromType, keyMember.MemberName, isMulti);
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
    private object BuildIncludeValueSetterInitializer(object parameter, List<IncludeSegment> includeSegments)
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
            var dbFactoryExpr = Expression.Parameter(typeof(IOrmDbFactory), "dbFactory");
            var connectionExpr = Expression.Parameter(typeof(TheaConnection), "connection");
            var includeSegmentsExpr = Expression.Parameter(typeof(List<IncludeSegment>), "includeSegments");
            var blockParameters = new List<ParameterExpression>();
            var blockBodies = new List<Expression>();

            Type parameterType = null;
            if (isMulti) parameterType = typeof(List<>).MakeGenericType(targetType);
            else parameterType = targetType;
            var targetExpr = Expression.Variable(parameterType, "target");
            blockParameters.Add(targetExpr);
            blockBodies.Add(Expression.Assign(targetExpr, Expression.Convert(objExpr, parameterType)));

            //Action<object, IDataReader, IOrmDbFactory, TheaConnection, List<IncludeSegment>>
            int index = 1;
            //var includeResult1=new List<OrderDetail>();
            //var includeResult2=new List<OrderDetail>();
            foreach (var includeSegment in includeSegments)
            {
                var includeType = typeof(List<>).MakeGenericType(includeSegment.IncludeMember.NavigationType);
                var includeResultExpr = Expression.Variable(includeType, $"includeResult{index}");
                blockParameters.Add(includeResultExpr);
                blockBodies.Add(Expression.Assign(includeResultExpr, Expression.New(includeType.GetConstructor(Type.EmptyTypes))));
                index++;
            }
            var breakLabel = Expression.Label();
            var toMethodInfo = typeof(TrolleyExtensions).GetMethod(nameof(TrolleyExtensions.To),
                   BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static,
                   new Type[] { typeof(IDataReader), typeof(IOrmDbFactory), typeof(TheaConnection) });
            var readMethodInfo = typeof(IDataReader).GetMethod(nameof(IDataReader.Read), Type.EmptyTypes);

            index = 1;
            foreach (var includeSegment in includeSegments)
            {
                var includeResultExpr = blockParameters[index];
                var readBreakLabel = Expression.Label();
                var ifFalseExpr = Expression.IsFalse(Expression.Call(readerExpr, readMethodInfo));
                var ifThenBreakExpr = Expression.IfThen(ifFalseExpr, Expression.Break(readBreakLabel));
                var methodInfo = toMethodInfo.MakeGenericMethod(includeSegment.IncludeMember.NavigationType);
                var includeValueExpr = Expression.Call(methodInfo, readerExpr, dbFactoryExpr, connectionExpr);
                var includeType = typeof(List<>).MakeGenericType(includeSegment.IncludeMember.NavigationType);
                methodInfo = includeType.GetMethod("Add", new Type[] { includeSegment.IncludeMember.NavigationType });
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
                    var predicateType = typeof(Predicate<>).MakeGenericType(includeSegment.IncludeMember.NavigationType);
                    var parameterExpr = Expression.Parameter(includeSegment.IncludeMember.NavigationType, "f");
                    var foreignKey = includeSegment.IncludeMember.ForeignKey;
                    var equalExpr = Expression.Equal(Expression.PropertyOrField(parameterExpr, foreignKey), keyValueExpr);

                    var predicateExpr = Expression.Lambda(predicateType, equalExpr, parameterExpr);
                    var includeType = typeof(List<>).MakeGenericType(includeSegment.IncludeMember.NavigationType);
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

            includeSetterInitializer = Expression.Lambda<Action<object, IDataReader, IOrmDbFactory, TheaConnection, List<IncludeSegment>>>(
                Expression.Block(blockParameters, blockBodies), objExpr, readerExpr, dbFactoryExpr, connectionExpr, includeSegmentsExpr).Compile();
            setterCache.TryAdd(cacheKey, includeSetterInitializer);
        }
        return includeSetterInitializer;
    }
    private Expression SetValue(Expression targetExpr, IncludeSegment includeSegment, Expression valueExpr)
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
        var memberName = includeSegment.IncludeMember.MemberName;

        Expression setValueExpr = null;
        switch (includeSegment.IncludeMember.Member.MemberType)
        {
            case MemberTypes.Field:
                setValueExpr = Expression.Assign(Expression.Field(currentExpr, memberName), valueExpr);
                break;
            case MemberTypes.Property:
                var methodInfo = (includeSegment.IncludeMember.Member as PropertyInfo).GetSetMethod();
                setValueExpr = Expression.Call(currentExpr, methodInfo, valueExpr);
                break;
            default: throw new NotSupportedException("目前只支持Field或是Property两种成员访问");
        }
        return setValueExpr;
    }
    private Expression GetKeyValue(Expression targetExpr, IncludeSegment includeSegment)
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
    private int GetIncludeSetterKey(Type targetType, List<IncludeSegment> includeSegments, bool isMulti)
    {
        var hashCode = new HashCode();
        hashCode.Add(targetType);
        hashCode.Add(includeSegments.Count);
        foreach (var includeSegment in includeSegments)
        {
            hashCode.Add(includeSegment.FromTable.EntityType);
            hashCode.Add(includeSegment.IncludeMember.MemberName);
        }
        hashCode.Add(isMulti);
        return hashCode.ToHashCode();
    }
}
