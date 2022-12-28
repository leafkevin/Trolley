using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Trolley;

class QueryVisitor : SqlVisitor
{
    private static readonly MethodInfo concatMethodInfo = typeof(string).GetMethod(nameof(string.Concat), new Type[] { typeof(object[]) });
    private List<MemberSegment> readerFields;

    private string selectSql = string.Empty;
    private string whereSql = string.Empty;
    private string groupBySql = string.Empty;
    private string havingSql = string.Empty;
    private string orderBySql = string.Empty;
    private int? skip = null;
    private int? limit = null;
    private bool isDistinct = false;
    private bool isUnion = false;
    private readonly List<TableSegment> includeTables = new();

    public QueryVisitor(IOrmDbFactory dbFactory, IOrmProvider ormProvider, string parameterPrefix = "p")
        : base(dbFactory, ormProvider, parameterPrefix)
    {
        this.tables = new();
        this.tableAlias = new();
    }
    public string BuildSql(out List<IDbDataParameter> dbParameters, out List<MemberSegment> readerFields)
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
            orderBy = $"ORDER BY {this.orderBySql}";

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
        else return $"SELECT {this.selectSql} FROM {tableSql} WHERE {this.whereSql}{others} {orderBy}";
    }
    public string BuildSql(Expression defaultExpr, out List<IDbDataParameter> dbParameters, out List<MemberSegment> readerFields)
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

        dbParameters = this.dbParameters;
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
    public QueryVisitor From(params Type[] entityTypes)
    {
        char tableIndex = 'a';
        for (int i = 0; i < entityTypes.Length; i++)
        {
            this.tables.Add(new TableSegment
            {
                EntityType = entityTypes[i],
                AliasName = $"{(char)(tableIndex + i)}",
                Path = $"{(char)(tableIndex + i)}"
            });
        }
        return this;
    }
    public QueryVisitor WithTable(Type entityType, string body, List<IDbDataParameter> dbParameters = null, string joinType = "")
    {
        char tableIndex = 'a';
        tableIndex += (char)this.tables.Count;
        if (this.tables.Count > 0)
            joinType = "INNER JOIN";

        this.tables.Add(new TableSegment
        {
            JoinType = joinType,
            EntityType = entityType,
            AliasName = $"{tableIndex}",
            Body = $"({body})",
            Path = $"{tableIndex}"
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
    public void Join(string joinType, Expression joinOn)
    {
        var lambdaExpr = joinOn as LambdaExpression;
        var joinTableSegment = this.InitTableAlias(lambdaExpr);
        var joinOnExpr = this.VisitConditionExpr(lambdaExpr.Body);
        if (this.tableAlias.Count == 2)
        {
            joinTableSegment.JoinType = joinType;
            joinTableSegment.OnExpr = joinOnExpr;
        }
    }
    public void Select(string sqlFormat, Expression selectExpr = null)
    {
        string body = null;
        if (selectExpr != null)
        {
            this.readerFields = new List<MemberSegment>();
            var lambdaExpr = selectExpr as LambdaExpression;
            this.InitTableAlias(lambdaExpr);
            var builder = new StringBuilder();
            switch (lambdaExpr.Body.NodeType)
            {
                case ExpressionType.New:
                    var newExpr = lambdaExpr.Body as NewExpression;
                    for (int i = 0; i < newExpr.Arguments.Count; i++)
                    {
                        this.AddSelectElement(newExpr.Arguments[i], newExpr.Members[i], i + 1, this.readerFields);
                    }
                    foreach (var memberSegment in this.readerFields)
                    {
                        if (builder.Length > 0)
                            builder.Append(", ");
                        builder.Append(memberSegment.Body);
                        if (memberSegment.IsNeedAlias)
                            builder.Append($" AS {memberSegment.FromMember.Name}");
                    }
                    body = builder.ToString();
                    break;
                case ExpressionType.MemberInit:
                    var memberInitExpr = lambdaExpr.Body as MemberInitExpression;
                    for (int i = 0; i < memberInitExpr.Bindings.Count; i++)
                    {
                        var memberAssignment = memberInitExpr.Bindings[i] as MemberAssignment;
                        this.AddSelectElement(memberAssignment.Expression, memberAssignment.Member, i + 1, this.readerFields);
                    }
                    foreach (var memberSegment in this.readerFields)
                    {
                        if (builder.Length > 0)
                            builder.Append(", ");
                        builder.Append(memberSegment.Body);
                        if (memberSegment.IsNeedAlias)
                            builder.Append($" AS {memberSegment.FromMember.Name}");
                    }
                    body = builder.ToString();
                    break;
                default:
                    //单值
                    body = this.Visit(new SqlSegment { Expression = lambdaExpr.Body }).ToString();
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
    public void Distinct() => this.isDistinct = true;
    public override SqlSegment VisitNew(SqlSegment sqlSegment)
    {
        var newExpr = sqlSegment.Expression as NewExpression;
        if (newExpr.Type.Name.StartsWith("<>"))
        {
            var memberSegments = new List<MemberSegment>();
            for (int i = 0; i < newExpr.Arguments.Count; i++)
            {
                this.AddSelectElement(newExpr.Arguments[i], newExpr.Members[i], i + 1, memberSegments);
            }
            return sqlSegment.Change(memberSegments);
        }
        return this.EvaluateAndParameter(sqlSegment);
    }
    public override SqlSegment VisitMemberInit(SqlSegment sqlSegment)
    {
        var memberInitExpr = sqlSegment.Expression as MemberInitExpression;
        var memberSegments = new List<MemberSegment>();
        for (int i = 0; i < memberInitExpr.Bindings.Count; i++)
        {
            if (memberInitExpr.Bindings[i].BindingType != MemberBindingType.Assignment)
                throw new Exception("暂时不支持除MemberBindingType.Assignment类型外的成员绑定表达式");
            var memberAssignment = memberInitExpr.Bindings[i] as MemberAssignment;
            this.AddSelectElement(memberAssignment.Expression, memberAssignment.Member, i + 1, memberSegments);
        }
        return sqlSegment.Change(memberSegments);
    }
    private void AddSelectElement(Expression elementExpr, MemberInfo memberInfo, int readerIndex, List<MemberSegment> result)
    {
        bool isNeedAlias = false;
        switch (elementExpr.NodeType)
        {
            case ExpressionType.Parameter:
            case ExpressionType.MemberAccess:
                var sqlSegment = this.Visit(new SqlSegment { Expression = elementExpr });
                if (elementExpr.Type.IsEntityType())
                {
                    var argumentSegments = sqlSegment.Value as List<MemberSegment>;
                    argumentSegments.ForEach(f =>
                    {
                        f.ReaderIndex = readerIndex;
                        f.TableSegment.IsUsed = true;
                    });
                    if (elementExpr.NodeType == ExpressionType.Parameter)
                    {
                        argumentSegments.ForEach(f => f.FromMember = memberInfo);
                    }
                    result.AddRange(argumentSegments);
                }
                else
                {
                    isNeedAlias = sqlSegment.IsParameter || sqlSegment.IsMethodCall || sqlSegment.MemberMapper.MemberName != memberInfo.Name;
                    sqlSegment.TableSegment.IsUsed = true;
                    result.Add(new MemberSegment
                    {
                        ReaderIndex = readerIndex,
                        TableIndex = 1,
                        FromMember = memberInfo,
                        MemberMapper = sqlSegment.MemberMapper,
                        TableSegment = sqlSegment.TableSegment,
                        Body = sqlSegment.ToString(),
                        IsNeedAlias = isNeedAlias
                    });
                }
                break;
            default:
                sqlSegment = this.Visit(new SqlSegment { Expression = elementExpr });
                sqlSegment.TableSegment.IsUsed = true;
                isNeedAlias = sqlSegment.IsParameter || sqlSegment.IsMethodCall || sqlSegment.MemberMapper.MemberName != memberInfo.Name;
                result.Add(new MemberSegment
                {
                    ReaderIndex = readerIndex,
                    TableIndex = 1,
                    FromMember = memberInfo,
                    MemberMapper = sqlSegment.MemberMapper,
                    TableSegment = sqlSegment.TableSegment,
                    Body = sqlSegment.ToString(),
                    IsNeedAlias = isNeedAlias
                });
                break;
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
        if (lambdaExpr.Parameters.Count == 1)
        //ThenInclude场景
        {
            tableSegment = this.tables.Last();
            this.tableAlias.Add(parameters[0], tableSegment);
        }
        else
        {
            var joinTables = this.tables.FindAll(f => !f.IsInclude);
            for (int i = 0; i < lambdaExpr.Parameters.Count; i++)
            {
                var parameterName = lambdaExpr.Parameters[i].Name;
                if (!parameters.Contains(parameterName))
                    continue;
                this.tableAlias.Add(parameterName, joinTables[i]);
                tableSegment = joinTables[i];
            }
        }
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
            if (fromSegment.Mapper == null)
                fromSegment.Mapper = this.dbFactory.GetEntityMap(fromType);
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
                var targetMemberMapper = targetMapper.GetMemberMap(memberMapper.ForeignKey);
                this.includeTables.Add(tableSegment = new TableSegment
                {
                    //默认FROM
                    EntityType = entityType,
                    Mapper = entityMapper,
                    IncludedFrom = fromSegment,
                    FromMember = memberMapper.Member,
                    OnExpr = $"{this.ormProvider.GetFieldName(targetMemberMapper.FieldName)} IN ({{0}})",
                    IsInclude = true,
                    Path = path
                });
            }
            fromSegment = tableSegment;
            fromType = memberMapper.NavigationType;
        }
        return tableSegment;
    }
}
