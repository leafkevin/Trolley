using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Trolley;

public class SqlExpressionVisitor
{
    private readonly IOrmDbFactory dbFactory;
    private readonly IOrmProvider ormProvider;
    private Dictionary<Type, FromTableInfo> fromTables = new();
    private List<ReaderFieldInfo> readerFields;
    private bool hasScope = false;
    private SqlSegmentType nodeType = SqlSegmentType.None;
    private SqlExpressionScope currentScope;
    private List<IDbDataParameter> dbParameters;

    private string selectSql = null;
    private string whereSql = null;
    private string groupBySql = null;
    private string orderBySql = null;
    private Stack<DeferredExpr> deferredExprs = new();

    public List<IDbDataParameter> Parameters => this.dbParameters;
    public List<ReaderFieldInfo> ReaderFields => this.readerFields;

    public SqlExpressionVisitor(IOrmDbFactory dbFactory, IOrmProvider ormProvider)
    {
        this.dbFactory = dbFactory;
        this.ormProvider = ormProvider;
    }

    public string BuildSql(Expression defaultSelectExpr = null)
    {
        var builder = new StringBuilder();
        if (string.IsNullOrEmpty(this.selectSql))
        {
            var sqlSegment = this.Visit(defaultSelectExpr) as SqlSegment;
            this.selectSql = sqlSegment.ToString();
        }
        builder.Append($"SELECT {this.selectSql}");
        if (this.fromTables.Count > 0)
        {
            foreach (var tableInfo in this.fromTables.Values)
            {
                if (tableInfo.Mapper == null)
                {
                    if (this.dbFactory.TryGetEntityMap(tableInfo.EntityType, out var mapper))
                        tableInfo.Mapper = mapper;
                    else tableInfo.Mapper = new EntityMap(tableInfo.EntityType);
                }
                if (builder.Length > 0) builder.Append(' ');
                builder.Append($"{tableInfo.JoinType} {tableInfo.Mapper.TableName} {tableInfo.AlaisName}");
                if (!string.IsNullOrEmpty(tableInfo.JoinOn))
                    builder.Append($" ON {tableInfo.JoinOn}");
            }
        }
        if (!string.IsNullOrEmpty(whereSql))
        {
            if (builder.Length > 0) builder.Append(' ');
            builder.Append($"WHERE {this.whereSql}");
        }
        return builder.ToString();
    }
    public SqlExpressionVisitor From(params Type[] tableTypes)
    {
        char tableIndex = 'a';
        tableIndex += (char)this.fromTables.Count;
        foreach (var tableType in tableTypes)
        {
            this.fromTables.TryAdd(tableType, new FromTableInfo
            {
                EntityType = tableType,
                AlaisName = $"{tableIndex}",
                JoinType = this.fromTables.Count > 0 ? "INNER JOIN" : "FROM"
            });
            tableIndex++;
        }
        return this;
    }
    //public void From(Type entityType, string tableSql) => this.AddTable(entityType, "FROM");
    private FromTableInfo TryAddTable(Type entityType, EntityMap mapper)
    {
        if (this.fromTables.TryGetValue(entityType, out var fromTableInfo))
        {
            if (fromTableInfo.Mapper == null)
                fromTableInfo.Mapper = mapper;
            return fromTableInfo;
        }
        return this.TryAddTable(entityType);
    }
    private FromTableInfo TryAddTable(Type entityType, string joinType = null)
    {
        if (this.fromTables.TryGetValue(entityType, out var fromTable))
            return fromTable;

        char tableIndex = 'a';
        tableIndex += (char)this.fromTables.Count;
        if (string.IsNullOrEmpty(joinType))
            joinType = this.fromTables.Count > 0 ? "INNER JOIN" : "FROM";
        var tableInfo = new FromTableInfo
        {
            EntityType = entityType,
            JoinType = joinType,
            AlaisName = $"{tableIndex}"
        };
        this.fromTables.TryAdd(entityType, tableInfo);
        return tableInfo;
    }
    public void Include(Expression includeExpr)
    {
        this.hasScope = false;
        this.nodeType = SqlSegmentType.Include;
        if (this.readerFields == null)
            this.readerFields = new List<ReaderFieldInfo>();
        var sqlSegment = this.Visit(includeExpr) as SqlSegment;
        if (sqlSegment.Value is List<SqlSegment> tableSegments)
        {
            if (!this.fromTables.TryGetValue(sqlSegment.Member.GetMemberType(), out var fromTableInfo))
                throw new Exception($"MemberAccess表达式解析有误，当前引用的实体类型{sqlSegment.Member.DeclaringType.FullName}未添加到fromTables字典中");

            var fieldsBuilder = new StringBuilder();
            foreach (var fieldSegment in tableSegments)
            {
                this.readerFields.Add(new ReaderFieldInfo
                {
                    Index = this.readerFields.Count,
                    IsTarget = false,
                    FromType = sqlSegment.Member?.DeclaringType,
                    Member = sqlSegment.Member,
                    RefMapper = fromTableInfo.Mapper,
                    MemberName = fieldSegment.Member.Name,
                    Expression = sqlSegment.Expression,
                    IsIncluded = true
                });
                if (fieldsBuilder.Length > 0)
                    fieldsBuilder.Append(", ");
                fieldsBuilder.Append(fieldSegment.ToString());
            }
            sqlSegment.Value = fieldsBuilder.ToString();
        }
        if (string.IsNullOrEmpty(this.selectSql))
            this.selectSql = sqlSegment.ToString();
        else this.selectSql += ", " + sqlSegment.ToString();
    }
    public void Join(Type entityType, Expression joinExpr = null, string joinType = null)
    {
        this.hasScope = false;
        this.nodeType = SqlSegmentType.InnerJoin;
        FromTableInfo joinTableInfo = null;
        if (string.IsNullOrEmpty(joinType))
            joinTableInfo = this.TryAddTable(entityType);
        else joinTableInfo = this.TryAddTable(entityType, joinType);
        if (joinExpr != null)
        {
            var sqlSegment = this.Visit(joinExpr) as SqlSegment;
            sqlSegment = this.VisitBooleanDeferredMember(sqlSegment);
            if (string.IsNullOrEmpty(joinTableInfo.JoinOn))
                joinTableInfo.JoinOn = sqlSegment.ToString();
        }
    }
    public void Select(Expression expr)
    {
        this.hasScope = false;
        this.nodeType = SqlSegmentType.Select;
        this.readerFields = new List<ReaderFieldInfo>();
        var sqlSegment = this.Visit(expr);
        if (string.IsNullOrEmpty(this.selectSql))
            this.selectSql = sqlSegment.ToString();
        else this.selectSql += ", " + sqlSegment.ToString();
    }
    public SqlExpressionVisitor Where(Expression expr)
    {
        this.hasScope = false;
        this.nodeType = SqlSegmentType.Where;
        var sqlSegment = this.Visit(expr) as SqlSegment;
        if (this.hasScope)
        {
            var builder = new StringBuilder();
            if (this.currentScope.Deep > 0)
            {
                for (int i = 0; i < this.currentScope.Deep; i++)
                {
                    builder.Append('(');
                }
            }
            sqlSegment = this.VisitBooleanDeferredMember(sqlSegment);
            builder.Append(sqlSegment.ToString());

            while (true)
            {
                if (this.currentScope.TryPop(out var latestScopeExpr))
                {
                    if (latestScopeExpr is Expression nextExpr)
                    {
                        builder.Append(" " + this.currentScope.Separator + " ");
                        var latestScope = this.currentScope;
                        sqlSegment = this.Visit(nextExpr) as SqlSegment;
                        sqlSegment = this.VisitBooleanDeferredMember(sqlSegment);
                        //Scope发生变化,变深就要加(左括号
                        if (this.currentScope.Deep > latestScope.Deep)
                        {
                            var loopTimes = this.currentScope.Deep - latestScope.Deep;
                            for (int i = 0; i < loopTimes; i++)
                            {
                                builder.Append('(');
                            }
                        }
                        builder.Append(sqlSegment.ToString());
                    }
                }
                else
                {
                    if (this.currentScope.Parent == null) break;
                    this.currentScope = this.currentScope.Parent;
                    //Scope发生变化,变浅结束上一个Scope,加)右括号
                    builder.Append(')');
                }
            }
            this.whereSql = builder.ToString();
            return this;
        }
        sqlSegment = this.VisitBooleanDeferredMember(sqlSegment);
        this.whereSql = sqlSegment.ToString();
        return this;
    }
    public SqlExpressionVisitor GroupBy(Expression expr)
    {
        this.hasScope = false;
        this.nodeType = SqlSegmentType.GroupBy;
        var sqlSegment = this.Visit(expr);
        this.groupBySql = sqlSegment.ToString();
        return this;
    }
    public SqlExpressionVisitor OrderBy(Expression expr)
    {
        this.hasScope = false;
        this.nodeType = SqlSegmentType.OrderBy;
        var sqlSegment = this.Visit(expr);
        this.orderBySql += sqlSegment.ToString();
        return this;
    }
    public SqlExpressionVisitor OrderByDescending(Expression expr)
    {
        this.hasScope = false;
        this.nodeType = SqlSegmentType.OrderByDescending;
        var sqlSegment = this.Visit(expr);
        this.orderBySql += sqlSegment.ToString();
        return this;
    }
    /// <summary>
    /// 解析一个SQL片段
    /// </summary>
    /// <param name="exprObj"></param>
    /// <returns></returns>
    private object Visit(Expression expr)
    {
        if (expr == null)
            return SqlSegment.None;

        object result = null;
        var currentExpr = expr;
        while (currentExpr != null)
        {
            switch (currentExpr.NodeType)
            {
                case ExpressionType.Lambda:
                    var lambdaExpr = currentExpr as LambdaExpression;
                    currentExpr = lambdaExpr.Body;
                    continue;
                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                case ExpressionType.Not:
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                case ExpressionType.ArrayLength:
                case ExpressionType.Quote:
                case ExpressionType.TypeAs:
                    result = this.VisitUnary(currentExpr as UnaryExpression);
                    break;
                case ExpressionType.MemberAccess:
                    result = this.VisitMemberAccess(currentExpr as MemberExpression);
                    break;
                case ExpressionType.Constant:
                    result = this.VisitConstant(currentExpr as ConstantExpression);
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
                    result = this.VisitBinary(currentExpr as BinaryExpression);
                    break;
                case ExpressionType.Parameter:
                    result = this.VisitParameter(currentExpr as ParameterExpression);
                    break;
                case ExpressionType.Call:
                    result = this.VisitMethodCall(currentExpr as MethodCallExpression);
                    break;
                case ExpressionType.New:
                    result = this.VisitNew(currentExpr as NewExpression);
                    break;
                case ExpressionType.NewArrayInit:
                case ExpressionType.NewArrayBounds:
                    result = this.VisitNewArray(currentExpr as NewArrayExpression);
                    break;
                case ExpressionType.MemberInit:
                    result = this.VisitMemberInit(currentExpr as MemberInitExpression);
                    break;
                case ExpressionType.Index:
                    result = this.VisitIndexExpression(currentExpr as IndexExpression);
                    break;
                case ExpressionType.Conditional:
                    result = this.VisitConditional(currentExpr as ConditionalExpression);
                    break;
                    //default: return nextExpr.ToString();
            }
            if (result is Expression nextExpr)
            {
                currentExpr = nextExpr;
                continue;
            }
            else
            {
                if (result is SqlSegment sqlSegment && sqlSegment.Expression == null)
                    sqlSegment.Expression = currentExpr;
                break;
            }
        }
        return result;
    }
    private object VisitUnary(UnaryExpression unaryExpr)
    {
        switch (unaryExpr.NodeType)
        {
            case ExpressionType.Not:
                if (this.nodeType == SqlSegmentType.Where)
                {
                    if (this.IsParameterExpr(unaryExpr.Operand))
                    {
                        //目前只有Not，一元/二元 bool类型才有延时处理,到参数表达式再统一处理
                        this.deferredExprs.Push(new DeferredExpr { ExpressionType = ExpressionType.Not });
                        return unaryExpr.Operand;
                    }
                }
                var sqlSegment = this.Visit(unaryExpr.Operand) as SqlSegment;
                if (unaryExpr.Type == typeof(bool))
                    return sqlSegment.Change($"NOT ({sqlSegment})");

                return sqlSegment.Change($"~{sqlSegment}");
            case ExpressionType.Convert:
                if (unaryExpr.Method != null)
                {
                    if (unaryExpr.Operand.NodeType == ExpressionType.Parameter)
                        return this.Visit(unaryExpr);

                    return this.Evaluate(unaryExpr);
                }
                break;
        }
        return this.Visit(unaryExpr.Operand);
    }
    private object VisitBinary(BinaryExpression binaryExpr)
    {
        object result = null;
        switch (binaryExpr.NodeType)
        {
            case ExpressionType.AndAlso:
            case ExpressionType.OrElse:
                var newSeparator = this.GetOperator(binaryExpr.NodeType);
                if (this.currentScope == null)
                {
                    this.hasScope = true;
                    this.currentScope = new SqlExpressionScope(newSeparator);
                }
                if (newSeparator != this.currentScope.Separator)
                {
                    var newScope = new SqlExpressionScope(newSeparator);
                    this.currentScope.Push(newScope);
                    this.currentScope = newScope;
                }
                this.currentScope.Push(binaryExpr.Right);
                result = binaryExpr.Left;
                break;
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
                var leftSegment = this.Visit(binaryExpr.Left) as SqlSegment;
                var rightSegment = this.Visit(binaryExpr.Right) as SqlSegment;

                var operators = this.GetOperator(binaryExpr.NodeType);
                if (binaryExpr.NodeType == ExpressionType.Equal || binaryExpr.NodeType == ExpressionType.NotEqual)
                {
                    if (!leftSegment.HasField && rightSegment.HasField)
                        this.Swap(ref leftSegment, ref rightSegment);

                    //至少有一边有字段
                    if (binaryExpr.Left.Type == typeof(bool) && leftSegment.HasField && !rightSegment.HasField)
                    {
                        this.deferredExprs.Push(new DeferredExpr
                        {
                            ExpressionType = ExpressionType.Equal,
                            Value = rightSegment.IsParameter ? rightSegment : SqlSegment.True
                        });
                        if (!rightSegment.IsParameter && !(bool)rightSegment.Value)
                            this.deferredExprs.Push(new DeferredExpr { ExpressionType = ExpressionType.Not });
                        if (binaryExpr.NodeType == ExpressionType.NotEqual)
                            this.deferredExprs.Push(new DeferredExpr { ExpressionType = ExpressionType.Not });
                        return this.VisitBooleanDeferredMember(leftSegment);
                    }

                    if (leftSegment == SqlSegment.Null)
                        this.Swap(ref leftSegment, ref rightSegment);
                    if (rightSegment == SqlSegment.Null)
                    {
                        if (binaryExpr.NodeType == ExpressionType.Equal)
                            operators = "IS";
                        else if (binaryExpr.NodeType == ExpressionType.NotEqual)
                            operators = "IS NOT";
                    }
                }
                if (binaryExpr.NodeType == ExpressionType.Add && binaryExpr.Left.Type == typeof(string) && binaryExpr.Right.Type == typeof(string))
                {
                    if (this.deferredExprs.TryPeek(out var deferredExpr) && deferredExpr.ExpressionType == ExpressionType.Add)
                    {
                        var exprs = deferredExpr.Value as List<Expression>;
                        exprs.Add(binaryExpr.Right);
                    }
                    else this.deferredExprs.Push(new DeferredExpr
                    {
                        ExpressionType = ExpressionType.Add,
                        Value = new List<Expression> { binaryExpr.Left, binaryExpr.Right }
                    });
                    return SqlSegment.None;
                }
                result = leftSegment.Merge(rightSegment, $"{this.GetSqlValue(leftSegment)}{operators}{this.GetSqlValue(rightSegment)}");
                break;
            case ExpressionType.ArrayIndex:
                break;
            case ExpressionType.Coalesce:
                break;
        }
        return result;
    }
    private object VisitMemberAccess(MemberExpression memberExpr)
    {
        if (memberExpr.Expression != null)
        {
            if (Nullable.GetUnderlyingType(memberExpr.Member.DeclaringType) != null)
            {
                if (memberExpr.Member.Name == nameof(Nullable<bool>.HasValue))
                {
                    this.deferredExprs.Push(new DeferredExpr { ExpressionType = ExpressionType.Equal, Value = SqlSegment.Null });
                    this.deferredExprs.Push(new DeferredExpr { ExpressionType = ExpressionType.Not });
                    return memberExpr.Expression;
                }
                else if (memberExpr.Member.Name == nameof(Nullable<bool>.Value))
                    return memberExpr.Expression;
                else throw new ArgumentException($"不支持的MemberAccess操作，表达式'{memberExpr}'返回值不是boolean类型");
            }
            if (memberExpr.Member.DeclaringType == typeof(string)
                && memberExpr.Member.Name == nameof(string.Length))
            {
                //返回SQL有参数，参数就不用放到deferrdExprs中了
                var sqlSegment = this.Visit(memberExpr.Expression) as SqlSegment;
                //TODO:不同的数据库会有本地化的处理              
                sqlSegment.Value = $"CHAR_LENGTH({this.GetSqlValue(sqlSegment)})";
                return sqlSegment;
            }
            if (this.IsParameterExpr(memberExpr))
            {
                //TODO:要确定最后是常量、成员变量、实体字段访问、参数
                if (memberExpr.Expression.NodeType == ExpressionType.Parameter)
                {
                    //参数类型，只能是from子句
                    Type entityType = null;
                    FromTableInfo leftTableInfo = null, fromTableInfo = null;
                    EntityMap entityMapper = null, leftMapper = null;
                    MemberMap memberMapper = null, leftMemberMapper = null;
                    bool isEntity = false;

                    while (true)
                    {
                        //处理FROM来源表之间JOIN ON语句，这里的实体都是模型
                        entityType = memberExpr.Member.DeclaringType;
                        if (entityType.Name.StartsWith("<>"))
                            entityType = this.fromTables.Values.First(f => f.JoinType == "FROM").EntityType;
                        entityMapper = this.dbFactory.GetEntityMap(entityType);
                        fromTableInfo = this.TryAddTable(entityType, entityMapper);
                        //TODO:enum类型，数据库中的值可能是字符串要处理为enum类型
                        memberMapper = entityMapper.GetMemberMap(memberExpr.Member.Name);

                        //没有joinOn子句，只能是导航属性，如果未设置导航属性，会有InnerJoin,LeftJoin,RightJoin子句来指定
                        this.SetRightTableJoinOn(entityMapper, fromTableInfo, leftMapper, leftTableInfo, leftMemberMapper);

                        isEntity = memberMapper.UnderlyingType.IsEntityType();
                        if (isEntity)
                        {
                            if (this.deferredExprs.TryPeek(out var deferredExpr)
                                && deferredExpr.ExpressionType == ExpressionType.MemberAccess)
                            {
                                leftMapper = entityMapper;
                                leftMemberMapper = memberMapper;
                                leftTableInfo = fromTableInfo;
                                memberExpr = deferredExpr.Value as MemberExpression;
                                this.deferredExprs.TryPop(out _);
                                continue;
                            }

                            //直接取实体属性
                            leftMapper = entityMapper;
                            leftMemberMapper = memberMapper;
                            leftTableInfo = fromTableInfo;

                            entityType = memberMapper.UnderlyingType;
                            entityMapper = this.dbFactory.GetEntityMap(entityType);
                            fromTableInfo = this.TryAddTable(entityType, entityMapper);
                            fromTableInfo.Mapper = entityMapper;

                            //没有joinOn子句，只能是导航属性，如果未设置导航属性，会有InnerJoin,LeftJoin,RightJoin子句来指定
                            this.SetRightTableJoinOn(entityMapper, fromTableInfo, leftMapper, leftTableInfo, leftMemberMapper);
                            break;
                        }
                        else break;
                    }
                    if (isEntity)
                    {
                        //直接取实体属性
                        var entitySegements = new List<SqlSegment>();
                        var memberInfos = entityMapper.GetMembers();
                        foreach (var memberInfo in memberInfos)
                        {
                            var mapper = entityMapper.GetMemberMap(memberInfo.Name);
                            if (mapper.IsIgnore || mapper.UnderlyingType.IsEntityType())
                                continue;
                            var fieldName = this.ormProvider.GetFieldName(mapper.FieldName);
                            fieldName = fromTableInfo.AlaisName + "." + fieldName;
                            entitySegements.Add(new SqlSegment
                            {
                                HasField = true,
                                //
                                Member = memberInfo,
                                Value = fieldName
                            });
                        }

                        return new SqlSegment
                        {
                            HasField = true,
                            //取当前scope中原来类的MemberInfo
                            Member = memberExpr.Member,
                            Value = entitySegements,
                            Expression = memberExpr
                        };
                    }
                    else
                    {
                        var fieldName = this.ormProvider.GetFieldName(memberMapper.FieldName);
                        fieldName = fromTableInfo.AlaisName + "." + fieldName;
                        return new SqlSegment
                        {
                            HasField = true,
                            Member = memberExpr.Member,
                            Value = fieldName
                        };
                    }
                }
                else
                {
                    //此时不知道是计算值，还是变成字段访问
                    //如果最后是常量、非参数成员访问，就开始处理deffered表达式，计算值
                    //如果最后是参数成员访问，就解析字段变成SQL
                    this.deferredExprs.Push(new DeferredExpr
                    {
                        ExpressionType = ExpressionType.MemberAccess,
                        Value = memberExpr
                    });
                    return memberExpr.Expression;
                }
            }
        }
        //访问局部变量或是成员变量，当作常量处理
        return this.Evaluate(memberExpr);
    }
    private object VisitConstant(ConstantExpression constantExpr)
    {
        if (constantExpr.Value == null)
            return SqlSegment.Null;
        return new SqlSegment { Value = constantExpr.Value };
    }
    private object VisitParameter(ParameterExpression parameterExpr)
    {

        return SqlSegment.Null;
    }
    private SqlSegment VisitMethodCall(MethodCallExpression methodCallExpr)
    {
        if (!this.ormProvider.TryGetMethodCallSqlFormatter(methodCallExpr.Method, out var formatter))
            throw new Exception($"不支持的方法访问，或是IOrmProvider未实现此方法{methodCallExpr.Method.Name}");

        object target = null;
        if (methodCallExpr.Object != null)
            target = this.Visit(methodCallExpr.Object);

        object[] args = null;
        if (methodCallExpr.Arguments != null && methodCallExpr.Arguments.Count > 0)
        {
            var arguments = new List<object>();
            foreach (var argumentExpr in methodCallExpr.Arguments)
            {
                arguments.Add(this.VisitAndDeferred(argumentExpr));
            }
            args = arguments.ToArray();
        }
        var result = formatter.Invoke(target, this.deferredExprs, args);
        return new SqlSegment { IsMethodCall = true, Value = result };
    }
    private SqlSegment VisitNew(NewExpression newExpr)
    {
        var arguments = new List<SqlSegment>();
        foreach (var argumentExpr in newExpr.Arguments)
        {
            var sqlSegment = this.VisitAndDeferred(argumentExpr) as SqlSegment;
            arguments.Add(sqlSegment);
        }
        var isAnonymousType = newExpr.Type.Name.StartsWith("<>");
        if (this.nodeType == SqlSegmentType.Select && isAnonymousType)
        {
            var builder = new StringBuilder();
            for (int i = 0; i < arguments.Count; i++)
            {
                var argumentExpr = newExpr.Arguments[i];
                var sqlSegment = arguments[i];
                var memberInfo = newExpr.Members[i];
                if (sqlSegment.Value is List<SqlSegment> tableSegments)
                {
                    if (!this.fromTables.TryGetValue(memberInfo.GetMemberType(), out var fromTableInfo))
                        throw new Exception($"MemberAccess表达式解析有误，当前引用的实体类型{memberInfo.DeclaringType.FullName}未添加到fromTables字典中");
                    var fieldsBuilder = new StringBuilder();

                    foreach (var fieldSegment in tableSegments)
                    {
                        this.readerFields.Add(new ReaderFieldInfo
                        {
                            Index = this.readerFields.Count,
                            IsTarget = false,
                            FromType = fieldSegment.Member.DeclaringType,
                            Member = memberInfo,
                            RefMapper = fromTableInfo.Mapper,
                            MemberName = fieldSegment.Member.Name,
                            Expression = argumentExpr
                        });
                        if (fieldsBuilder.Length > 0)
                            fieldsBuilder.Append(", ");
                        fieldsBuilder.Append(fieldSegment.ToString());
                    }
                    sqlSegment.Value = fieldsBuilder.ToString();
                }
                else
                {
                    this.readerFields.Add(new ReaderFieldInfo
                    {
                        Index = this.readerFields.Count,
                        IsTarget = true,
                        FromType = sqlSegment.Member?.DeclaringType,
                        Member = memberInfo,
                        MemberName = memberInfo.Name,
                        Expression = argumentExpr
                    });
                    if (sqlSegment.IsParameter || sqlSegment.IsMethodCall)
                        sqlSegment.Value = $"{sqlSegment} AS {memberInfo.Name}";
                    else if (argumentExpr is MemberExpression memberExpr)
                    {
                        if (memberExpr.Member.Name != memberInfo.Name)
                            sqlSegment.Value = $"{sqlSegment} AS {memberInfo.Name}";
                    }
                    else sqlSegment.Value = $"{this.GetSqlValue(sqlSegment)} AS {memberInfo.Name}";
                }
                if (builder.Length > 0)
                    builder.Append(", ");
                builder.Append(sqlSegment.Value);
            }
            return new SqlSegment { Value = builder.ToString() };
        }
        return this.Evaluate(newExpr);
    }
    private SqlSegment VisitNewArray(NewArrayExpression newArrayExpr)
    {
        var result = new List<object>();
        foreach (var elementExpr in newArrayExpr.Expressions)
        {
            var sqlSegment = this.Visit(elementExpr) as SqlSegment;
            result.Add(sqlSegment.Value);
        }
        return new SqlSegment { Value = result };
    }
    private SqlSegment VisitMemberInit(MemberInitExpression memberInitExpr)
    {
        if (this.nodeType == SqlSegmentType.Select)
        {
            var targetMapper = this.dbFactory.GetEntityMap(memberInitExpr.Type);
            //memberInitExpr.
            // memberInitExpr.Bindings
        }
        return this.Evaluate(memberInitExpr);
    }
    private SqlSegment VisitIndexExpression(IndexExpression indexExpr)
    {
        return SqlSegment.Null;
    }
    private object VisitConditional(ConditionalExpression conditionalExpr)
    {
        this.Visit(conditionalExpr.Test);
        return conditionalExpr.Test;
    }
    private SqlSegment VisitBooleanDeferredMember(SqlSegment fieldSegment)
    {
        if (!this.IsBooleanBinary(fieldSegment.Expression))
        {
            this.deferredExprs.Push(new DeferredExpr
            {
                ExpressionType = ExpressionType.Equal,
                Value = SqlSegment.True
            });
        }
        if (this.deferredExprs.Count <= 0)
            return fieldSegment;

        //处理HasValue !逻辑取反操作，这种情况下是一元操作
        int notIndex = 0;
        SqlSegment sqlSegment = null;

        while (this.deferredExprs.TryPop(out var deferredExpr))
        {
            switch (deferredExpr.ExpressionType)
            {
                case ExpressionType.Not: notIndex++; break;
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                    if (deferredExpr.ExpressionType == ExpressionType.NotEqual)
                        notIndex++;
                    if (sqlSegment != null)
                    {
                        var deferredSegment = deferredExpr.Value as SqlSegment;
                        if (deferredSegment == SqlSegment.True)
                            continue;
                        if (sqlSegment == SqlSegment.True)
                        {
                            sqlSegment = deferredSegment;
                            continue;
                        }
                        throw new Exception($"不支持的deferredExpr，当前表达式={sqlSegment},deferredExpr:={deferredExpr.Value}");
                    }
                    else sqlSegment = deferredExpr.Value as SqlSegment;
                    break;
                default: throw new Exception("不应该走到这个分支，没有这个场景");
            }
        }
        string strOperator = null;
        if (notIndex % 2 > 0)
            strOperator = sqlSegment == SqlSegment.Null ? "IS NOT" : "<>";
        else strOperator = sqlSegment == SqlSegment.Null ? "IS" : "=";

        return fieldSegment.Merge(sqlSegment, $"{fieldSegment} {strOperator} {this.GetSqlValue(sqlSegment)}");
    }
    private SqlSegment VisitConcatDeferredMember(Expression originalExpr)
    {
        if (!this.deferredExprs.TryPeek(out var deferredExpr) || deferredExpr.ExpressionType != ExpressionType.Add)
            throw new Exception("不支持的DeferredExpr操作");

        var exprs = deferredExpr.Value as List<Expression>;
        var values = new List<object>();
        foreach (var expr in exprs)
        {
            values.Add(this.Visit(expr));
        }
        var methodInfo = typeof(string).GetMethod("Concat", new Type[] { typeof(object[]) });
        if (!this.ormProvider.TryGetMethodCallSqlFormatter(methodInfo, out var formatter))
            throw new Exception($"{this.ormProvider.GetType().FullName}类未实现TryGetMethodCallSqlFormatter中的Concat方法调用");
        var result = formatter.Invoke(null, this.deferredExprs, values.ToArray());
        this.deferredExprs.TryPop(out _);
        return new SqlSegment { IsMethodCall = true, Expression = originalExpr, Value = result };
    }
    private SqlSegment VisitAndDeferred(Expression expr)
    {
        var sqlSegment = this.Visit(expr) as SqlSegment;
        if (sqlSegment == SqlSegment.None && expr.Type == typeof(string))
            sqlSegment = this.VisitConcatDeferredMember(expr);
        return sqlSegment;
    }
    private void SetRightTableJoinOn(EntityMap entityMapper, FromTableInfo fromTableInfo, EntityMap leftMapper, FromTableInfo leftTableInfo, MemberMap leftMemberMapper)
    {
        //没有joinOn子句，只能是导航属性，如果未设置导航属性，会有InnerJoin,LeftJoin,RightJoin子句来指定
        if (leftTableInfo != null && string.IsNullOrEmpty(fromTableInfo.JoinOn))
        {
            if (!leftMemberMapper.IsNavigation)
                throw new Exception($"未提供{entityMapper.EntityType.FullName}的JoinOn关联语句,缺少InnerJoin/LeftJoin/RightJoin子句或是未设置导航属性");

            string leftField = null;
            string rightField = null;
            if (leftMemberMapper.IsToOne)
            {
                //1:1或是n:1
                var navigationMapper = leftMapper.GetMemberMap(leftMemberMapper.NavigationMemberName);
                leftField = this.ormProvider.GetFieldName(navigationMapper.MemberName);

                if (entityMapper.KeyFields == null && entityMapper.KeyFields.Count == 0)
                    throw new Exception($"实体类{entityMapper.EntityType.FullName}是实体类{leftMapper.EntityType.FullName}的导航类型，但未提供主键定义KeyFields");
                if (entityMapper.KeyFields.Count > 1)
                    throw new Exception($"实体类{entityMapper.EntityType.FullName}是实体类{leftMapper.EntityType.FullName}的导航类型，主键定义KeyFields只支持一个字段，不支持多个字段联合主键");
                rightField = this.ormProvider.GetFieldName(entityMapper.KeyFields[0]);
            }
            else
            {
                //1:n                          
                if (leftMapper.KeyFields == null && leftMapper.KeyFields.Count == 0)
                    throw new Exception($"实体类{leftMapper.EntityType.FullName}包内导航属性，但未提供主键定义KeyFields");
                if (leftMapper.KeyFields.Count > 1)
                    throw new Exception($"实体类{leftMapper.EntityType.FullName}包内导航属性，主键定义KeyFields只支持一个字段，不支持多个字段联合主键");
                leftField = this.ormProvider.GetFieldName(leftMapper.KeyFields[0]);

                var navigationMapper = entityMapper.GetMemberMap(leftMemberMapper.NavigationMemberName);
                rightField = this.ormProvider.GetFieldName(navigationMapper.FieldName);
            }
            fromTableInfo.JoinOn = $"{leftTableInfo.AlaisName}.{leftField}={fromTableInfo.AlaisName}.{rightField}";
        }
    }
    private bool IsParameterExpr(Expression expr)
    {
        var nextExpr = expr;
        MemberExpression memberExpr;
        while (nextExpr != null)
        {
            switch (nextExpr.NodeType)
            {
                case ExpressionType.Parameter:
                    return true;
                case ExpressionType.Constant:
                    return false;
                case ExpressionType.MemberAccess:
                    memberExpr = nextExpr as MemberExpression;
                    nextExpr = memberExpr.Expression;
                    continue;
                case ExpressionType.Call:
                    var methodCallExpr = nextExpr as MethodCallExpression;
                    if (this.IsParameterExpr(methodCallExpr.Object))
                        return true;
                    for (var i = 0; i < methodCallExpr.Arguments.Count; i++)
                    {
                        if (this.IsParameterExpr(methodCallExpr.Arguments[i]))
                            return true;
                    }
                    return false;
                case ExpressionType.Conditional:
                    var conditionalExpr = nextExpr as ConditionalExpression;
                    if (this.IsParameterExpr(conditionalExpr.Test))
                        return true;
                    if (this.IsParameterExpr(conditionalExpr.IfTrue))
                        return true;
                    if (this.IsParameterExpr(conditionalExpr.IfFalse))
                        return true;
                    else return false;
            }
            if (nextExpr is UnaryExpression unaryExpr)
            {
                nextExpr = unaryExpr.Operand;
                continue;
            }
            if (nextExpr is BinaryExpression binaryExpr)
            {
                if (this.IsParameterExpr(binaryExpr.Left))
                    return true;
                if (this.IsParameterExpr(binaryExpr.Right))
                    return true;
                return false;
            }
            memberExpr = nextExpr as MemberExpression;
            nextExpr = memberExpr?.Expression;
        }
        return false;
    }
    private bool IsBooleanBinary(Expression expr)
    {
        switch (expr.NodeType)
        {
            case ExpressionType.AndAlso:
            case ExpressionType.OrElse:
            case ExpressionType.LessThan:
            case ExpressionType.LessThanOrEqual:
            case ExpressionType.GreaterThan:
            case ExpressionType.GreaterThanOrEqual:
            case ExpressionType.Equal:
            case ExpressionType.NotEqual:
                return true;
            case ExpressionType.Call:
                return expr.Type == typeof(bool);
        }
        return false;
    }
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
            var parameterName = $"{this.ormProvider.ParameterPrefix}p{this.dbParameters.Count + 1}";
            this.dbParameters.Add(this.ormProvider.CreateParameter(parameterName, objValue));
            return new SqlSegment { IsParameter = true, Value = parameterName };
        }
        return new SqlSegment { Value = objValue };
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
