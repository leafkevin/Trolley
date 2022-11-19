using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Trolley;

class QueryVisitor
{
    private static readonly MethodInfo concatMethodInfo = typeof(string).GetMethod(nameof(string.Concat), new Type[] { typeof(object[]) });
    private readonly IOrmDbFactory dbFactory;
    private readonly IOrmProvider ormProvider;
    private List<MemberSegment> readerFields;
    private SqlSegmentType nodeType = SqlSegmentType.None;
    private List<IDbDataParameter> dbParameters;

    private string selectSql = string.Empty;
    private string whereSql = string.Empty;
    private string groupBySql = string.Empty;
    private string havingSql = string.Empty;
    private string orderBySql = string.Empty;
    private bool isDistinct = false;
    private int? skip = null;
    private int? limit = null;
    private bool isUnion = false;
    private string parameterPrefix;

    private bool isMultiTable = false;
    private List<TableSegment> tables = new();
    private Dictionary<string, TableSegment> tableAlias = new();
    private List<TableSegment> includeTables = new();
    public QueryVisitor(IOrmDbFactory dbFactory, IOrmProvider ormProvider, string parameterPrefix = "p")
    {
        this.dbFactory = dbFactory;
        this.ormProvider = ormProvider;
        this.parameterPrefix = parameterPrefix;
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
                    tableSegment.Mapper = this.dbFactory.GetEntityMap(tableSegment.EntityType);
                    tableName = this.ormProvider.GetTableName(tableSegment.Mapper.TableName);
                }
                if (builder.Length > 0) builder.Append(' ');
                builder.Append($"{tableSegment.NodeType} {tableName} {tableSegment.AliasName}");

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
            foreach (var tableInfo in this.tables)
            {
                var tableName = tableInfo.Body;
                if (string.IsNullOrEmpty(tableName))
                {
                    if (tableInfo.Mapper == null)
                        tableInfo.Mapper = this.dbFactory.GetEntityMap(tableInfo.EntityType);
                    tableName = this.ormProvider.GetTableName(tableInfo.Mapper.TableName);
                }
                if (builder.Length > 0) builder.Append(' ');
                builder.Append($"{tableInfo.NodeType} {tableName} {tableInfo.AliasName}");

                if (!string.IsNullOrEmpty(tableInfo.OnExpr))
                    builder.Append($" ON {tableInfo.OnExpr}");

                if (!string.IsNullOrEmpty(tableInfo.Filter))
                {
                    if (!string.IsNullOrEmpty(tableInfo.OnExpr))
                        builder.Append(" AND ");
                    builder.Append(tableInfo.Filter);
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
    public void From(params Type[] entityTypes)
    {
        char tableIndex = 'a';
        this.isMultiTable = entityTypes.Length > 0;
        for (int i = 0; i < entityTypes.Length; i++)
        {
            this.tables.Add(new TableSegment
            {
                EntityType = entityTypes[i],
                AliasName = $"{(char)(tableIndex + i)}",
                Path = $"{(char)(tableIndex + i)}"
            });
        }
    }
    public void WithTable(Type entityType, string body, List<IDbDataParameter> dbParameters = null, string joinType = "")
    {
        char tableIndex = 'a';
        tableIndex += (char)this.tables.Count;
        if (this.tables.Count > 0)
            joinType = "INNER JOIN";

        this.tables.Add(new TableSegment
        {
            NodeType = joinType,
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
        this.InitTableAlias(memberSelector);
        var tableSegment = this.AddIncludeTables(memberExpr);
        //TODO: 1:N关联条件的alias表，获取会有问题，待测试
        if (filter != null)
            tableSegment.Filter = this.Visit(new SqlSegment { Expression = filter }).ToString();
    }
    public void Join(string joinType, Expression joinOn)
    {
        this.tableAlias.Clear();
        var lambdaExpr = joinOn as LambdaExpression;
        //参数顺序和刨除include表外的表顺序一致
        this.tableAlias.Clear();
        var joinTables = this.tables.FindAll(f => !f.IsInclude);
        for (int i = 0; i < lambdaExpr.Parameters.Count; i++)
        {
            var parameterName = lambdaExpr.Parameters[i].Name;
            this.tableAlias.Add(parameterName, joinTables[i]);
        }
        var sqlSegment = this.VisitAndDeferred(new SqlSegment { Expression = joinOn });
        var joinSegment = this.tables.Last(f => !f.IsInclude);
        joinSegment.NodeType = joinType;
        joinSegment.OnExpr = sqlSegment.ToString();
    }
    public void Select(string sqlFormat, Expression selectExpr = null)
    {
        string body = null;
        if (selectExpr != null)
        {
            this.InitTableAlias(selectExpr);
            var lambdaExpr = selectExpr as LambdaExpression;
            var sqlSegment = new SqlSegment
            {
                NodeType = SqlSegmentType.Select,
                Expression = lambdaExpr.Body
            };
            var builder = new StringBuilder();
            switch (lambdaExpr.Body.NodeType)
            {
                case ExpressionType.New:
                case ExpressionType.MemberInit:
                case ExpressionType.Parameter:
                    sqlSegment = this.Visit(sqlSegment);
                    if (sqlSegment.Value is List<MemberSegment> memberSegments)
                    {
                        this.readerFields = memberSegments;
                        int readerIndex = 0;
                        int tableIndex = 0;
                        TableSegment lastTableSegment = null;
                        foreach (var memberSegment in this.readerFields)
                        {
                            if (memberSegment.TableSegment != lastTableSegment)
                            {
                                lastTableSegment = memberSegment.TableSegment;
                                lastTableSegment.IsUsed = true;
                                if (lastTableSegment.IncludedFrom != null)
                                {
                                    lastTableSegment.ReaderIndex = lastTableSegment.IncludedFrom.ReaderIndex;
                                    readerIndex = lastTableSegment.ReaderIndex;
                                    tableIndex++;
                                    lastTableSegment.TableIndex = tableIndex;
                                }
                                else
                                {
                                    readerIndex++;
                                    tableIndex++;
                                    lastTableSegment.ReaderIndex = readerIndex;
                                    lastTableSegment.TableIndex = tableIndex;
                                }
                            }
                            memberSegment.ReaderIndex = readerIndex;
                            memberSegment.TableIndex = tableIndex;
                            if (builder.Length > 0)
                                builder.Append(", ");
                            builder.Append(memberSegment.Body);
                            if (memberSegment.IsNeedAlias)
                                builder.Append($" AS {memberSegment.FromMember.Name}");
                        }
                    }
                    body = sqlSegment.Change(builder.ToString()).ToString();
                    break;
                default:
                    //单值
                    body = this.Visit(sqlSegment).ToString();
                    break;
            }
        }
        if (!string.IsNullOrEmpty(sqlFormat))
            this.selectSql = string.Format(sqlFormat, body);
        else this.selectSql = body;
    }
    public void GroupBy(Expression expr)
    {
        this.InitTableAlias(expr);
        this.groupBySql = this.VisitList(expr, string.Empty);
    }
    public void OrderBy(string orderType, Expression expr)
    {
        this.InitTableAlias(expr);
        var orderBy = this.VisitList(expr, orderType == "DESC" ? " DESC" : string.Empty);
        if (string.IsNullOrEmpty(this.orderBySql))
            this.orderBySql = orderBy;
        else this.orderBySql += "," + orderBy;
    }
    public void Having(Expression havingExpr)
    {
        this.nodeType = SqlSegmentType.Where;
        this.InitTableAlias(havingExpr);
        var lambdaExpr = havingExpr as LambdaExpression;
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
    public void Where(Expression whereExpr)
    {
        this.nodeType = SqlSegmentType.Where;
        this.InitTableAlias(whereExpr);
        var lambdaExpr = whereExpr as LambdaExpression;
        this.whereSql = " WHERE " + this.VisitConditionExpr(lambdaExpr.Body);
    }
    public void Distinct() => this.isDistinct = true;
    private SqlSegment VisitAndDeferred(SqlSegment sqlSegment)
       => this.VisitBooleanDeferred(this.Visit(sqlSegment));
    private SqlSegment Visit(SqlSegment sqlSegment)
    {
        if (sqlSegment == SqlSegment.None)
            return SqlSegment.None;

        SqlSegment result = null;
        var currentExpr = sqlSegment.Expression;
        while (currentExpr != null)
        {
            switch (currentExpr.NodeType)
            {
                case ExpressionType.Lambda:
                    var lambdaExpr = currentExpr as LambdaExpression;
                    sqlSegment.Expression = lambdaExpr.Body;
                    continue;
                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                case ExpressionType.Not:
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                case ExpressionType.ArrayLength:
                case ExpressionType.Quote:
                case ExpressionType.TypeAs:
                    result = this.VisitUnary(sqlSegment);
                    break;
                case ExpressionType.MemberAccess:
                    result = this.VisitMemberAccess(sqlSegment);
                    break;
                case ExpressionType.Constant:
                    result = this.VisitConstant(sqlSegment);
                    break;
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                case ExpressionType.Divide:
                case ExpressionType.Modulo:
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.Coalesce:
                case ExpressionType.ArrayIndex:
                case ExpressionType.RightShift:
                case ExpressionType.LeftShift:
                case ExpressionType.ExclusiveOr:
                    result = this.VisitBinary(sqlSegment);
                    break;
                case ExpressionType.Parameter:
                    result = this.VisitParameter(sqlSegment);
                    break;
                case ExpressionType.Call:
                    result = this.VisitMethodCall(sqlSegment);
                    break;
                case ExpressionType.New:
                    result = this.VisitNew(sqlSegment);
                    break;
                case ExpressionType.NewArrayInit:
                case ExpressionType.NewArrayBounds:
                    result = this.VisitNewArray(sqlSegment);
                    break;
                case ExpressionType.MemberInit:
                    result = this.VisitMemberInit(sqlSegment);
                    break;
                case ExpressionType.Index:
                    result = this.VisitIndexExpression(sqlSegment);
                    break;
                case ExpressionType.Conditional:
                    result = this.VisitConditional(sqlSegment);
                    break;
                    //default: return nextExpr.ToString();
            }
            if (!result.IsCompleted)
                continue;
            return result;
        }
        return result;
    }
    private SqlSegment VisitUnary(SqlSegment sqlSegment)
    {
        var unaryExpr = sqlSegment.Expression as UnaryExpression;
        switch (unaryExpr.NodeType)
        {
            case ExpressionType.Not:
                //TODO:待测试
                //if (sqlSegment.NodeType == SqlSegmentType.Where)
                if (unaryExpr.Type == typeof(bool))
                {
                    if (unaryExpr.Operand.IsParameter(out _))
                    {
                        //目前只有Not，一元/二元 bool类型才有延时处理,到参数表达式再统一处理
                        sqlSegment.Push(new DeferredExpr { OperationType = OperationType.Not });
                        return sqlSegment.Next(unaryExpr.Operand);
                    }
                    //TODO:测试Value赋值
                    return sqlSegment.Next(unaryExpr.Operand, $"NOT ({this.Visit(sqlSegment)})");
                }
                return sqlSegment.Change($"~{this.Visit(sqlSegment)}");
            case ExpressionType.Convert:
                if (unaryExpr.Method != null)
                {
                    //TODO:测试类型转换
                    if (unaryExpr.Operand.IsParameter(out _))
                        return sqlSegment.Next(unaryExpr.Operand);
                    return this.Evaluate(unaryExpr);
                }
                break;
        }
        return sqlSegment.Next(unaryExpr.Operand);
    }
    private SqlSegment VisitBinary(SqlSegment sqlSegment)
    {
        var binaryExpr = sqlSegment.Expression as BinaryExpression;
        switch (binaryExpr.NodeType)
        {
            //And/Or，已经在Where/Having中单独处理了
            //case ExpressionType.AndAlso:
            //case ExpressionType.OrElse:
            //var operationType = binaryExpr.NodeType == ExpressionType.AndAlso ?
            //         OperationType.And : OperationType.Or;
            //if (sqlSegment.OperationType == OperationType.None)
            //    sqlSegment.OperationType = operationType;
            //if (sqlSegment.OperationType != operationType)
            //{
            //    sqlSegment.Push(new DeferredExpr
            //    {
            //        OperationType = sqlSegment.OperationType,
            //        Value = sqlSegment.Value
            //    });
            //}
            //sqlSegment.Push(new DeferredExpr
            //{
            //    OperationType = operationType,
            //    Value = binaryExpr.Right
            //});
            //return sqlSegment.Next(binaryExpr.Left);
            case ExpressionType.Equal:
            case ExpressionType.NotEqual:
            case ExpressionType.GreaterThan:
            case ExpressionType.GreaterThanOrEqual:
            case ExpressionType.LessThan:
            case ExpressionType.LessThanOrEqual:
            case ExpressionType.Add:
            case ExpressionType.AddChecked:
            case ExpressionType.Subtract:
            case ExpressionType.SubtractChecked:
            case ExpressionType.Multiply:
            case ExpressionType.MultiplyChecked:
            case ExpressionType.Divide:
            case ExpressionType.Modulo:
            case ExpressionType.And:
            case ExpressionType.Or:
            case ExpressionType.ExclusiveOr:
            case ExpressionType.RightShift:
            case ExpressionType.LeftShift:
                //字符串连接单独处理
                if (binaryExpr.NodeType == ExpressionType.Add && binaryExpr.Left.Type == typeof(string) && binaryExpr.Right.Type == typeof(string))
                    return sqlSegment.Change(this.VisitConcatAndDeferred(binaryExpr));

                var leftSegment = this.Visit(new SqlSegment { Expression = binaryExpr.Left });
                var rightSegment = this.Visit(new SqlSegment { Expression = binaryExpr.Right });
                var operators = this.GetOperator(binaryExpr.NodeType);

                if (binaryExpr.NodeType == ExpressionType.Equal || binaryExpr.NodeType == ExpressionType.NotEqual)
                {
                    if (!leftSegment.HasField && rightSegment.HasField)
                        this.Swap(ref leftSegment, ref rightSegment);

                    //处理!(a.IsEnabled==true)情况,bool类型，最外层再做defer处理
                    if (binaryExpr.Left.Type == typeof(bool) && leftSegment.HasField && !rightSegment.HasField)
                    {
                        if (!(bool)rightSegment.Value)
                            leftSegment.Push(new DeferredExpr { OperationType = OperationType.Not });
                        if (binaryExpr.NodeType == ExpressionType.NotEqual)
                            leftSegment.Push(new DeferredExpr { OperationType = OperationType.Not });
                        return sqlSegment.Merge(leftSegment);
                    }
                    //处理a.UserName!=null情况
                    if (leftSegment == SqlSegment.Null)
                        this.Swap(ref leftSegment, ref rightSegment);
                    if (rightSegment == SqlSegment.Null)
                    {
                        sqlSegment.Push(new DeferredExpr
                        {
                            OperationType = OperationType.Equal,
                            Value = SqlSegment.Null
                        });
                        if (binaryExpr.NodeType == ExpressionType.NotEqual)
                            sqlSegment.Push(new DeferredExpr { OperationType = OperationType.Not });
                        return sqlSegment.Merge(leftSegment);
                    }
                }
                return sqlSegment.Merge(leftSegment.Merge(rightSegment), $"{this.GetSqlValue(leftSegment)}{operators}{this.GetSqlValue(rightSegment)}");
            case ExpressionType.ArrayIndex:
                break;
            case ExpressionType.Coalesce:
                break;
        }
        return sqlSegment;
    }
    private SqlSegment VisitMemberAccess(SqlSegment sqlSegment)
    {
        var memberExpr = sqlSegment.Expression as MemberExpression;
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
                    return sqlSegment.Next(memberExpr.Expression);
                }
                else if (memberExpr.Member.Name == nameof(Nullable<bool>.Value))
                    return sqlSegment.Next(memberExpr.Expression);
                else throw new ArgumentException($"不支持的MemberAccess操作，表达式'{memberExpr}'返回值不是boolean类型");
            }

            //各种类型值的属性访问，如：DateTime,TimeSpan,String.Length,List.Count,
            if (this.ormProvider.TryGetMemberAccessSqlFormatter(memberExpr.Member, out var formatter))
            {
                //Where(f=>... && f.OrderNo.Length==10 && ...)
                //Where(f=>... && f.Order.OrderNo.Length==10 && ...)
                var targetSegment = this.Visit(sqlSegment.Next(memberExpr.Expression));
                return sqlSegment.Complete(formatter.Invoke(targetSegment));
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
                    path = memberExpr.Expression.ToString();
                    var fromSegment = this.FindTableSegment(parameterName, path);
                    if (fromSegment == null)
                        throw new Exception($"使用导航属性前，要先使用Include方法包含进来，访问路径:{path}");

                    var vavigationMapper = tableSegment.Mapper.GetMemberMap(memberExpr.Member.Name);
                    if (!vavigationMapper.IsNavigation)
                        throw new Exception($"类{tableSegment.EntityType.FullName}属性{memberExpr.Member.Name}未配置为导航属性");

                    path = memberExpr.ToString();
                    tableSegment = this.FindTableSegment(parameterName, path);
                    if (tableSegment == null)
                        throw new Exception($"使用导航属性前，要先使用Include方法包含进来，访问路径:{path}");

                    var memberSegments = this.GetTableFields(tableSegment, tableSegment.FromMember);
                    return new SqlSegment
                    {
                        IsCompleted = true,
                        Expression = null,
                        HasField = true,
                        TableSegment = tableSegment,
                        Value = memberSegments
                    };
                }
                else
                {
                    path = memberExpr.Expression.ToString();
                    tableSegment = this.FindTableSegment(parameterName, path);
                    if (tableSegment == null)
                        throw new Exception($"使用导航属性前，要先使用Include方法包含进来，访问路径:{path}");

                    var memberMapper = tableSegment.Mapper.GetMemberMap(memberExpr.Member.Name);
                    var fieldName = this.ormProvider.GetFieldName(memberMapper.FieldName);
                    return new SqlSegment
                    {
                        IsCompleted = true,
                        Expression = null,
                        HasField = true,
                        TableSegment = tableSegment,
                        MemberMapper = memberMapper,
                        Value = $"{tableSegment.AliasName}.{fieldName}"
                    };
                }
            }
        }
        //访问局部变量或是成员变量，当作常量处理,直接计算，如果是字符串变成参数@p
        //var orderIds=new List<int>{1,2,3}; Where(f=>orderIds.Contains(f.OrderId)); orderIds
        //private Order order; Where(f=>f.OrderId==this.Order.Id); this.Order.Id
        //var orderId=10; Select(f=>new {OrderId=orderId,...}
        //Select(f=>new {OrderId=this.Order.Id, ...}
        return this.Evaluate(memberExpr);
    }
    private SqlSegment VisitConstant(SqlSegment sqlSegment)
    {
        var constantExpr = sqlSegment.Expression as ConstantExpression;
        if (constantExpr.Value == null)
            return SqlSegment.Null;
        return sqlSegment.Complete(constantExpr.Value);
    }
    private SqlSegment VisitParameter(SqlSegment sqlSegment)
    {
        //业务场景：方法调用Target对象/参数，Select实体
        var parameterExpr = sqlSegment.Expression as ParameterExpression;
        if (typeof(IAggregateSelect).IsAssignableFrom(parameterExpr.Type))
            return new SqlSegment { Value = parameterExpr.Name };
        if (typeof(IWhereSql).IsAssignableFrom(parameterExpr.Type))
            return new SqlSegment { Value = parameterExpr.Name };

        //处理Include表字段
        var fromSegment = this.tableAlias[parameterExpr.Name];
        var tableSegment = fromSegment;
        //参数的FromMeber没有设置，在Select的时候设置
        var memberSegments = this.GetTableFields(fromSegment);
        while (true)
        {
            var childSegment = this.tables.Find(f => f.IncludedFrom == tableSegment);
            if (childSegment == null) break;
            memberSegments.AddRange(this.GetTableFields(childSegment, childSegment.FromMember));
            tableSegment = childSegment;
        }
        return new SqlSegment
        {
            HasField = true,
            IsCompleted = true,
            TableSegment = fromSegment,
            Expression = parameterExpr,
            Value = memberSegments
        };
    }
    private SqlSegment VisitMethodCall(SqlSegment sqlSegment)
    {
        var methodCallExpr = sqlSegment.Expression as MethodCallExpression;
        if (!this.ormProvider.TryGetMethodCallSqlFormatter(methodCallExpr.Method, out var formatter))
            throw new Exception($"不支持的方法访问，或是IOrmProvider未实现此方法{methodCallExpr.Method.Name}");

        object target = null;
        object[] args = null;
        //如果方法对象是聚合查询，不做任何处理
        if (methodCallExpr.Object?.Type != typeof(IAggregateSelect))
            target = this.Visit(sqlSegment.Next(methodCallExpr.Object));

        if (methodCallExpr.Object?.Type == typeof(IAggregateSelect))
        {
            if (methodCallExpr.Arguments.Count > 1)
                throw new Exception("聚合查询Count，暂时不支持多个参数");

            var argumentExpr = methodCallExpr.Arguments[0];
            //访问1:N关系的导航属性，做Count操作，当作取子表数据COUNT,如：a.Count(b.Details)
            if (methodCallExpr.Method.IsGenericMethod
               && methodCallExpr.Method.Name.Contains("Count")
               && argumentExpr.NodeType == ExpressionType.MemberAccess
               && argumentExpr.Type.GenericTypeArguments[0].IsEntityType())
                throw new Exception($"聚合查询方法{methodCallExpr.Method.Name}，参数必须是字段，不能是导航属性实体，或者使用无参数聚合函数");
        }
        //TODO:字符串连接单独特殊处理
        if (methodCallExpr.Arguments != null && methodCallExpr.Arguments.Count > 0)
        {
            var arguments = new List<object>();
            foreach (var argumentExpr in methodCallExpr.Arguments)
            {
                arguments.Add(this.VisitAndDeferred(new SqlSegment { Expression = argumentExpr }));
            }
            args = arguments.ToArray();
        }
        var result = formatter.Invoke(target, sqlSegment.DeferredExprs, args);
        sqlSegment.IsMethodCall = true;
        return sqlSegment.Change(result);
    }
    private SqlSegment VisitNew(SqlSegment sqlSegment)
    {
        var newExpr = sqlSegment.Expression as NewExpression;
        if (newExpr.Type.Name.StartsWith("<>"))
        {
            var arguments = new List<SqlSegment>();
            var memberSegments = new List<MemberSegment>();
            foreach (var argumentExpr in newExpr.Arguments)
            {
                var argumentSegment = this.VisitAndDeferred(new SqlSegment { Expression = argumentExpr });
                arguments.Add(argumentSegment);
            }
            for (int i = 0; i < arguments.Count; i++)
            {
                var argumentExpr = newExpr.Arguments[i];
                var argumentSegment = arguments[i];
                if (argumentSegment.Value is List<MemberSegment> tableMemberSegments)
                {
                    int tableIndex = i;
                    var path = tableMemberSegments[0].Path;
                    var memberInfo = newExpr.Members[i];
                    foreach (var memberSegment in tableMemberSegments)
                    {
                        memberSegment.ReaderIndex = i;
                        if (memberSegment.Path != path)
                        {
                            tableIndex++;
                            path = memberSegment.Path;
                        }
                        memberSegment.TableIndex = tableIndex;
                        if (tableIndex == i)
                            memberSegment.FromMember = memberInfo;
                        memberSegment.Body = $"{memberSegment.TableSegment.AliasName}.{this.ormProvider.GetFieldName(memberSegment.MemberMapper.FieldName)}";
                        memberSegment.Path = memberSegment.TableSegment.Path + "." + memberSegment.MemberMapper.MemberName;
                    }
                }
                else
                {
                    var memberInfo = newExpr.Members[i];
                    var isNeedAlias = argumentSegment.IsParameter || argumentSegment.IsMethodCall || argumentSegment.MemberMapper.MemberName != memberInfo.Name;
                    memberSegments.Add(new MemberSegment
                    {
                        ReaderIndex = i,
                        TableIndex = i,
                        Expression = argumentExpr,
                        FromMember = memberInfo,
                        MemberMapper = argumentSegment.MemberMapper,
                        TableSegment = argumentSegment.TableSegment,
                        Body = argumentSegment.ToString(),
                        IsNeedAlias = isNeedAlias,
                        IsTarget = true,
                        Path = argumentSegment.TableSegment.AliasName + argumentSegment.ToString()[1..]
                    });
                }
            }
            return sqlSegment.Change(memberSegments);
        }
        return this.Evaluate(newExpr);
    }
    private SqlSegment VisitNewArray(SqlSegment sqlSegment)
    {
        var newArrayExpr = sqlSegment.Expression as NewArrayExpression;
        var result = new List<object>();
        foreach (var elementExpr in newArrayExpr.Expressions)
        {
            result.Add(this.Visit(new SqlSegment { Expression = elementExpr }));
        }
        return sqlSegment.Change(result);
    }
    private SqlSegment VisitMemberInit(SqlSegment sqlSegment)
    {
        int index = 0;
        var memberInitExpr = sqlSegment.Expression as MemberInitExpression;
        var memberSegments = new List<MemberSegment>();
        foreach (var memberBinding in memberInitExpr.Bindings)
        {
            if (memberBinding.BindingType != MemberBindingType.Assignment)
                throw new Exception("暂时不支持除MemberBindingType.Assignment类型外的成员绑定表达式");

            var bindingExpr = ((MemberAssignment)memberBinding).Expression;
            var bindingSegment = this.VisitAndDeferred(new SqlSegment { Expression = bindingExpr });
            if (bindingSegment.Value is List<TableSegment> tableSegments)
            {
                int fromIndex = 0;
                foreach (var tableSegment in tableSegments)
                {
                    foreach (var memberMapper in tableSegment.Mapper.MemberMaps)
                    {
                        MemberInfo memberInfo = null;
                        if (fromIndex == tableSegments.Count - 1)
                            memberInfo = memberBinding.Member;
                        else memberInfo = tableSegment.FromMember;
                        memberSegments.Add(new MemberSegment
                        {
                            TableIndex = index,
                            Expression = bindingExpr,
                            FromMember = memberInfo,
                            MemberMapper = memberMapper,
                            TableSegment = tableSegment,
                            Body = bindingSegment.ToString(),
                            IsNeedAlias = false,
                            Path = memberBinding.Member.Name + tableSegment.Path[1..]
                        });
                        index++;
                    }
                }
            }
            else
            {
                var memberInfo = memberBinding.Member;
                var isNeedAlias = bindingSegment.IsParameter || bindingSegment.IsMethodCall || bindingSegment.MemberMapper.MemberName != memberBinding.Member.Name;
                memberSegments.Add(new MemberSegment
                {
                    TableIndex = index,
                    Expression = bindingExpr,
                    FromMember = memberInfo,
                    MemberMapper = bindingSegment.MemberMapper,
                    TableSegment = bindingSegment.TableSegment,
                    Body = bindingSegment.ToString(),
                    IsNeedAlias = isNeedAlias,
                    Path = memberBinding.Member.Name + bindingSegment.ToString()[1..]
                });
                index++;
            }
        }
        return sqlSegment.Change(memberSegments);
    }
    private SqlSegment VisitIndexExpression(SqlSegment sqlSegment)
    {
        var indexExpr = sqlSegment.Expression as IndexExpression;
        var argExpr = indexExpr.Arguments[0];
        var objIndex = argExpr is ConstantExpression constant
            ? constant.Value : this.Evaluate(argExpr).Value;
        var index = (int)Convert.ChangeType(objIndex, typeof(int));
        var objList = this.Evaluate(indexExpr.Object).Value;
        if (objList is List<object> list)
            return sqlSegment.Complete(list[index]);
        return SqlSegment.Null;
    }
    private SqlSegment VisitConditional(SqlSegment sqlSegment)
    {
        //var conditionalExpr = sqlSegment.Expression as ConditionalExpression;
        //this.Visit(conditionalExpr.Test);
        //return conditionalExpr.Test;
        return sqlSegment;
    }
    private SqlSegment VisitBooleanDeferred(SqlSegment fieldSegment)
    {
        if (!fieldSegment.HasDeferred)
            return fieldSegment;

        //处理HasValue !逻辑取反操作，这种情况下是一元操作
        int notIndex = 0;
        var sqlSegment = SqlSegment.None;
        var operationTypes = new OperationType[] { OperationType.Equal, OperationType.Not };

        while (fieldSegment.TryPop(operationTypes, out var deferredExpr))
        {
            if (deferredExpr.OperationType == OperationType.Equal)
            {
                sqlSegment = deferredExpr.Value as SqlSegment;
                break;
            }
            if (deferredExpr.OperationType == OperationType.Not)
                notIndex++;
        }
        if (sqlSegment == SqlSegment.None)
            sqlSegment = SqlSegment.True;

        string strOperator = null;
        if (notIndex % 2 > 0)
            strOperator = sqlSegment == SqlSegment.Null ? "IS NOT" : "<>";
        else strOperator = sqlSegment == SqlSegment.Null ? "IS" : "=";

        return fieldSegment.Merge(sqlSegment, $"{fieldSegment} {strOperator} {this.GetSqlValue(sqlSegment)}");
    }
    private string VisitConcatAndDeferred(Expression concatExpr)
    {
        var concatSegments = this.VisitConcatExpr(concatExpr);
        this.ormProvider.TryGetMethodCallSqlFormatter(concatMethodInfo, out var formater);
        for (int i = 0; i < concatSegments.Count; i++)
        {
            concatSegments[i] = this.Visit(concatSegments[i]);
        }
        return formater.Invoke(null, null, concatSegments);
    }
    private List<SqlSegment> VisitConcatExpr(Expression concatExpr)
    {
        var deferredExprs = new Stack<SqlSegment>();
        var completedSegements = new List<SqlSegment>();
        var binaryExpr = concatExpr as BinaryExpression;
        Func<Expression, bool> isConcatBinary = f =>
        {
            if (f is BinaryExpression binaryExpr && binaryExpr.NodeType == ExpressionType.Add
                && binaryExpr.Left.Type == typeof(string) && binaryExpr.Right.Type == typeof(string))
                return true;
            return false;
        };

        while (binaryExpr != null)
        {
            var isLeftLeaf = isConcatBinary(binaryExpr.Left);
            var isRightLeaf = isConcatBinary(binaryExpr.Right);
            if (isLeftLeaf)
            {
                completedSegements.Add(new SqlSegment
                {
                    OperationType = OperationType.Concat,
                    Expression = binaryExpr.Left
                });
            }
            if (isRightLeaf)
            {
                deferredExprs.Push(new SqlSegment
                {
                    OperationType = OperationType.Concat,
                    Expression = binaryExpr.Right
                });
            }
            Expression nextExpr = null;
            if (isLeftLeaf && isRightLeaf) break;
            if (isLeftLeaf && !isRightLeaf)
                nextExpr = binaryExpr.Right;
            if (!isLeftLeaf && isRightLeaf)
                nextExpr = binaryExpr.Left;

            binaryExpr = nextExpr as BinaryExpression;
        }
        while (deferredExprs.TryPop(out var sqlSegment))
        {
            completedSegements.Add(sqlSegment);
        }
        return completedSegements;
    }
    private string VisitConditionExpr(Expression conditionExpr)
    {
        if (conditionExpr.NodeType == ExpressionType.AndAlso || conditionExpr.NodeType == ExpressionType.OrElse)
        {
            int lastDeep = 0;
            var builder = new StringBuilder();
            var sqlSegments = this.VisitLogicBinaryExpr(conditionExpr);
            for (int i = 0; i < sqlSegments.Count; i++)
            {
                var sqlSegment = this.Visit(sqlSegments[i]);
                if (sqlSegment.HasDeferred)
                    sqlSegment = this.VisitBooleanDeferred(sqlSegment);

                var separator = sqlSegment.OperationType == OperationType.And ? " AND " : " OR ";
                if (i > 0)
                {
                    if (sqlSegment.Deep < lastDeep)
                    {
                        var loopTimes = lastDeep - sqlSegment.Deep;
                        for (int j = 0; j < loopTimes; j++)
                        {
                            builder.Append(')');
                        }
                    }
                    builder.Append(separator);
                }
                if (i == 0)
                {
                    if (sqlSegment.Deep > 0)
                    {
                        for (int j = 0; j < sqlSegment.Deep; j++)
                        {
                            builder.Append('(');
                        }
                    }
                }
                else
                {
                    if (sqlSegment.Deep > lastDeep)
                    {
                        var loopTimes = sqlSegment.Deep - lastDeep;
                        for (int j = 0; j < loopTimes; j++)
                        {
                            builder.Append('(');
                        }
                    }
                }
                builder.Append(sqlSegment.ToString());
                lastDeep = sqlSegment.Deep;
            }
            return builder.ToString();
        }
        return this.Visit(new SqlSegment { Expression = conditionExpr }).ToString();
    }
    private List<SqlSegment> VisitLogicBinaryExpr(Expression expr)
    {
        Func<Expression, bool> isLogicBinary = f => f.NodeType != ExpressionType.AndAlso && f.NodeType != ExpressionType.OrElse;

        int deep = 0;
        var lastOperationType = OperationType.None;
        var deferredExprs = new Stack<SqlSegment>();
        var completedSegements = new List<SqlSegment>();
        var binaryExpr = expr as BinaryExpression;

        while (binaryExpr != null)
        {
            var operationType = binaryExpr.NodeType == ExpressionType.AndAlso ? OperationType.And : OperationType.Or;
            if (lastOperationType == OperationType.None)
                lastOperationType = operationType;
            if (operationType != lastOperationType)
                deep++;

            var isLeftLeaf = isLogicBinary(binaryExpr.Left);
            var isRightLeaf = isLogicBinary(binaryExpr.Right);
            if (isLeftLeaf)
            {
                completedSegements.Add(new SqlSegment
                {
                    OperationType = operationType,
                    Expression = binaryExpr.Left,
                    Deep = deep
                });
            }
            if (isRightLeaf)
            {
                deferredExprs.Push(new SqlSegment
                {
                    OperationType = operationType,
                    Expression = binaryExpr.Right,
                    Deep = deep
                });
            }
            Expression nextExpr = null;
            if (isLeftLeaf && isRightLeaf) break;
            if (isLeftLeaf && !isRightLeaf)
                nextExpr = binaryExpr.Right;
            if (!isLeftLeaf && isRightLeaf)
                nextExpr = binaryExpr.Left;

            binaryExpr = nextExpr as BinaryExpression;
        }
        while (deferredExprs.TryPop(out var sqlSegment))
        {
            completedSegements.Add(sqlSegment);
        }
        return completedSegements;
    }
    private string VisitList(Expression expr, string suffix)
    {
        var lambdaExpr = expr as LambdaExpression;
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
        else return this.Visit(new SqlSegment { Expression = expr }).ToString();
    }
    private void InitTableAlias(Expression expr)
    {
        this.tableAlias.Clear();
        var lambdaExpr = expr as LambdaExpression;
        //参数顺序和刨除include表外的表顺序一致 
        var joinTables = this.tables.FindAll(f => !f.IsInclude);
        for (int i = 0; i < lambdaExpr.Parameters.Count; i++)
        {
            var parameterName = lambdaExpr.Parameters[i].Name;
            this.tableAlias.Add(parameterName, joinTables[i]);
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
            if (entityMapper.KeyFields.Count > 1)
                throw new Exception($"导航属性表，暂时不支持多个主键字段，实体：{memberMapper.MapType.FullName}");

            var rightAlias = $"{(char)('a' + this.tables.Count)}";
            builder.Append("." + currentExpr.Member.Name);
            var path = builder.ToString();
            if (memberMapper.IsToOne)
            {
                this.tables.Add(tableSegment = new TableSegment
                {
                    //默认外联
                    NodeType = "LEFT JOIN",
                    EntityType = entityType,
                    Mapper = entityMapper,
                    AliasName = rightAlias,
                    IncludedFrom = fromSegment,
                    FromMember = memberMapper.Member,
                    OnExpr = $"{fromSegment.AliasName}.{this.ormProvider.GetFieldName(memberMapper.ForeignKey)}={rightAlias}.{this.ormProvider.GetFieldName(entityMapper.KeyFields[0])}",
                    IsInclude = true,
                    IsSelectAll = true,
                    Path = path
                });
            }
            else
            {
                if (fromMapper.KeyFields.Count > 1)
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
                    IsSelectAll = true,
                    Path = path
                });
            }
            fromSegment = tableSegment;
            fromType = memberMapper.NavigationType;
        }
        return tableSegment;
    }
    private TableSegment FindTableSegment(string parameterName, string path)
    {
        var index = path.IndexOf(".");
        path = parameterName + path.Substring(index);
        return this.tables.Find(f => f.Path == path);
    }
    private List<MemberSegment> GetTableFields(TableSegment fromSegment, MemberInfo fromMember = null)
    {
        var memberMappers = fromSegment.Mapper.GetMembers();
        var memberSegments = new List<MemberSegment>();
        foreach (var memberInfo in memberMappers)
        {
            if (memberInfo.GetMemberType().IsEntityType())
                continue;
            var memberMapper = fromSegment.Mapper.GetMemberMap(memberInfo.Name);
            memberSegments.Add(new MemberSegment
            {
                //此处FromIndex,FromMember,Body不做处理，不知道是否作为Select内容
                FromMember = fromMember ?? memberInfo,
                MemberMapper = memberMapper,
                TableSegment = fromSegment,
                Body = $"{fromSegment.AliasName}.{this.ormProvider.GetFieldName(memberMapper.FieldName)}",
                IsNeedAlias = false,
                Path = fromSegment.Path + "." + memberInfo.Name
            });
        }
        return memberSegments;
    }
    //private bool IsExpressionFor(Expression expr, ExpressionType exprType)
    //{
    //    while (expr != null)
    //    {
    //        if (expr.NodeType == exprType)
    //        {
    //            var subUnaryExpr = expr as UnaryExpression;
    //            var isSubExprAccess = subUnaryExpr?.Operand is IndexExpression;
    //            if (!isSubExprAccess)
    //                return true;
    //        }
    //        if (expr is BinaryExpression binaryExpr)
    //        {
    //            if (this.IsExpressionFor(binaryExpr.Left, exprType))
    //                return true;
    //            if (this.IsExpressionFor(binaryExpr.Right, exprType))
    //                return true;
    //        }
    //        if (expr is MethodCallExpression methodCallExpr)
    //        {
    //            for (var i = 0; i < methodCallExpr.Arguments.Count; i++)
    //            {
    //                if (this.IsExpressionFor(methodCallExpr.Arguments[i], exprType))
    //                    return true;
    //            }
    //            if (this.IsExpressionFor(methodCallExpr.Object, exprType))
    //                return true;
    //        }
    //        if (expr is UnaryExpression unaryExpr)
    //        {
    //            if (this.IsExpressionFor(unaryExpr.Operand, exprType))
    //                return true;
    //        }
    //        if (expr is ConditionalExpression condExpr)
    //        {
    //            if (this.IsExpressionFor(condExpr.Test, exprType))
    //                return true;
    //            if (this.IsExpressionFor(condExpr.IfTrue, exprType))
    //                return true;
    //            if (this.IsExpressionFor(condExpr.IfFalse, exprType))
    //                return true;
    //        }
    //        var memberExpr = expr as MemberExpression;
    //        expr = memberExpr?.Expression;
    //    }
    //    return false;
    //}
    //private bool IsBooleanBinary(Expression expr)
    //{
    //    switch (expr.NodeType)
    //    {
    //        case ExpressionType.AndAlso:
    //        case ExpressionType.OrElse:
    //        case ExpressionType.LessThan:
    //        case ExpressionType.LessThanOrEqual:
    //        case ExpressionType.GreaterThan:
    //        case ExpressionType.GreaterThanOrEqual:
    //        case ExpressionType.Equal:
    //        case ExpressionType.NotEqual:
    //            return true;
    //        case ExpressionType.Call:
    //            return expr.Type == typeof(bool);
    //    }
    //    return false;
    //}
    private string GetSqlValue(SqlSegment sqlSegment)
    {
        if (sqlSegment == SqlSegment.Null || sqlSegment.HasField || sqlSegment.IsParameter)
            return sqlSegment.ToString();
        return this.ormProvider.GetQuotedValue(sqlSegment.Value);
    }
    private SqlSegment Evaluate(Expression expr)
    {
        var member = Expression.Convert(expr, typeof(object));
        var lambda = Expression.Lambda<Func<object>>(member);
        var getter = lambda.Compile();
        var objValue = getter();

        //只有字符串会变成参数，有可能sql注入
        if (expr.Type == typeof(string))
        {
            if (this.dbParameters == null)
                this.dbParameters = new List<IDbDataParameter>();
            var parameterName = $"{this.ormProvider.ParameterPrefix}{this.parameterPrefix}{this.dbParameters.Count + 1}";
            this.dbParameters.Add(this.ormProvider.CreateParameter(parameterName, objValue));
            return new SqlSegment { IsCompleted = true, IsParameter = true, Value = parameterName };
        }
        return new SqlSegment { IsCompleted = true, Value = objValue };
    }
    private void Swap<T>(ref T left, ref T right)
    {
        var temp = right;
        right = left;
        left = temp;
    }
    private string GetOperator(ExpressionType exprType)
    {
        switch (exprType)
        {
            case ExpressionType.Equal: return "=";
            case ExpressionType.NotEqual: return "<>";
            case ExpressionType.GreaterThan: return ">";
            case ExpressionType.GreaterThanOrEqual: return ">=";
            case ExpressionType.LessThan: return "<";
            case ExpressionType.LessThanOrEqual: return "<=";
            case ExpressionType.AndAlso: return "AND";
            case ExpressionType.OrElse: return "OR";
            case ExpressionType.Add: return "+";
            case ExpressionType.Subtract: return "-";
            case ExpressionType.Multiply: return "*";
            case ExpressionType.Divide: return "/";
            case ExpressionType.Modulo: return "MOD";
            case ExpressionType.Coalesce: return "COALESCE";
            case ExpressionType.And: return "&";
            case ExpressionType.Or: return "|";
            case ExpressionType.ExclusiveOr: return "^";
            case ExpressionType.LeftShift: return "<<";
            case ExpressionType.RightShift: return ">>";
            default: return exprType.ToString();
        }
    }
}
