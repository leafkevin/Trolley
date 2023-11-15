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

public class SqlVisitor : ISqlVisitor
{
    private static ConcurrentDictionary<int, Func<object, object>> memberGetterCache = new();
    private static string[] calcOps = new string[] { ">", ">=", "<", "<=", "+", "-", "*", "/", "%", "&", "|", "^", "<<", ">>" };
    private static Action<IDataParameterCollection, IOrmProvider, MemberMap, string, object> addDbParametersDelegate = null;

    protected string ParameterPrefix { get; set; } = "p";
    protected List<TableSegment> Tables { get; set; }
    protected Dictionary<string, TableSegment> TableAlias { get; set; }
    protected List<ReaderField> ReaderFields { get; set; }
    protected bool IsSelect { get; set; }
    protected bool IsWhere { get; set; }
    protected bool IsFromQuery { get; set; }
    protected string WhereSql { get; set; }
    protected bool IsMultiple { get; set; }
    protected int CommandIndex { get; set; }
    protected OperationType LastWhereNodeType { get; set; } = OperationType.None;
    protected char TableAsStart { get; set; }
    protected List<ReaderField> GroupFields { get; set; }

    public string DbKey { get; private set; }
    public IDataParameterCollection DbParameters { get; set; }
    public IOrmProvider OrmProvider { get; private set; }
    public IEntityMapProvider MapProvider { get; private set; }
    public bool IsNeedAlias { get; set; }
    public bool IsParameterized { get; set; }

    public SqlVisitor(string dbKey, IOrmProvider ormProvider, IEntityMapProvider mapProvider, bool isParameterized = false, char tableAsStart = 'a', string parameterPrefix = "p")
    {
        this.DbKey = dbKey;
        this.OrmProvider = ormProvider;
        this.MapProvider = mapProvider;
        this.IsParameterized = isParameterized;
        this.TableAsStart = tableAsStart;
        this.ParameterPrefix = parameterPrefix;
    }
    public virtual int BuildMultiCommand(IDbCommand command, StringBuilder sqlBuilder, MultipleCommand multiCommand, int commandIndex)
    {
        return 0;
    }
    public virtual string BuildSql(out List<IDbDataParameter> dbParameters)
    {
        dbParameters = null;
        return null;
    }
    public virtual SqlSegment VisitAndDeferred(SqlSegment sqlSegment)
    {
        sqlSegment = this.Visit(sqlSegment);
        if (!sqlSegment.HasDeferred)
            return sqlSegment;

        //处理HasValue !逻辑取反操作，这种情况下是一元操作
        return this.VisitDeferredBoolConditional(sqlSegment, true, this.OrmProvider.GetQuotedValue(true), this.OrmProvider.GetQuotedValue(false));
    }
    public virtual SqlSegment Visit(SqlSegment sqlSegment)
    {
        SqlSegment result = null;
        if (sqlSegment.Expression == null)
            throw new ArgumentNullException("sqlSegment.Expression");

        switch (sqlSegment.Expression.NodeType)
        {
            case ExpressionType.Lambda:
                var lambdaExpr = sqlSegment.Expression as LambdaExpression;
                result = this.Visit(sqlSegment.Next(lambdaExpr.Body));
                break;
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
            case ExpressionType.ListInit:
                result = this.VisitListInit(sqlSegment);
                break;
            case ExpressionType.TypeIs:
                result = this.VisitTypeIs(sqlSegment);
                break;
            default: throw new NotSupportedException($"不支持的表达式操作，{sqlSegment.Expression}");
        }
        return result;
    }
    public virtual SqlSegment VisitUnary(SqlSegment sqlSegment)
    {
        var unaryExpr = sqlSegment.Expression as UnaryExpression;
        switch (unaryExpr.NodeType)
        {
            case ExpressionType.Not:
                if (unaryExpr.Type == typeof(bool))
                {
                    //SELECT/WHERE语句，都会有Defer处理，在最外层再计算bool值
                    sqlSegment.Push(new DeferredExpr { OperationType = OperationType.Not });
                    return this.Visit(sqlSegment.Next(unaryExpr.Operand));
                }
                return sqlSegment.ChangeValue($"~{this.Visit(sqlSegment)}");
            case ExpressionType.Convert:
                //以下3种情况会走到此处
                //(int)f.TotalAmount强制转换或是枚举f.Gender = Gender.Male表达式
                //或是表达式计算，如：30 + f.TotalAmount，int amount = 30;amount + f.TotalAmount，
                //表达式把30解析为double类型常量，amount解析为double类型的强转转换
                //或是方法调用Convert.ToXxx,string.Concat,string.Format,string.Join
                //如：f.Gender.ToString(),string.Format("{0},{1},{2}", 30, DateTime.Now, Gender.Male)
                if (unaryExpr.Method != null)
                {
                    if (unaryExpr.Operand.IsParameter(out _))
                    {
                        if (unaryExpr.Type != typeof(object))
                            sqlSegment.ExpectType = unaryExpr.Type;
                        return this.Visit(sqlSegment.Next(unaryExpr.Operand));
                    }
                    return this.Evaluate(sqlSegment);
                }
                return this.Visit(sqlSegment.Next(unaryExpr.Operand));
        }
        return this.Visit(sqlSegment.Next(unaryExpr.Operand));
    }
    public virtual SqlSegment VisitBinary(SqlSegment sqlSegment)
    {
        var binaryExpr = sqlSegment.Expression as BinaryExpression;
        switch (binaryExpr.NodeType)
        {
            //And/Or，已经在Where/Having中单独处理了
            case ExpressionType.Add:
            case ExpressionType.AddChecked:
            case ExpressionType.Subtract:
            case ExpressionType.SubtractChecked:
            case ExpressionType.Multiply:
            case ExpressionType.MultiplyChecked:
            case ExpressionType.Divide:
            case ExpressionType.Modulo:
            case ExpressionType.LessThan:
            case ExpressionType.LessThanOrEqual:
            case ExpressionType.GreaterThan:
            case ExpressionType.GreaterThanOrEqual:
            case ExpressionType.Equal:
            case ExpressionType.NotEqual:
            case ExpressionType.Coalesce:
            case ExpressionType.ArrayIndex:
            case ExpressionType.And:
            case ExpressionType.Or:
            case ExpressionType.ExclusiveOr:
            case ExpressionType.RightShift:
            case ExpressionType.LeftShift:
                if (this.IsStringConcatOperator(sqlSegment, out var operatorSegment))
                    return operatorSegment;
                //TODO:DateOnly,TimeOnly两个类型要做处理
                if (this.IsDateTimeOperator(sqlSegment, out operatorSegment))
                    return operatorSegment;
                if (this.IsTimeSpanOperator(sqlSegment, out operatorSegment))
                    return operatorSegment;

                var leftSegment = this.Visit(sqlSegment.Next(binaryExpr.Left));
                var rightSegment = this.Visit(new SqlSegment { Expression = binaryExpr.Right });

                //计算数组访问，a??bb
                if (leftSegment.IsConstant && rightSegment.IsConstant)
                    return this.Evaluate(sqlSegment.Next(binaryExpr));

                if ((leftSegment.IsConstant || leftSegment.IsVariable)
                    && (rightSegment.IsConstant || rightSegment.IsVariable))
                {
                    this.Evaluate(sqlSegment.Next(binaryExpr));
                    sqlSegment.IsConstant = false;
                    sqlSegment.IsVariable = true;
                    return sqlSegment;
                }
                //下面都是带有参数的情况，带有参数表达式计算(常量、变量)、函数调用等共2种情况
                //bool类型的表达式，这里不做解析只做defer操作解析，到最外层select、where、having、joinOn子句中去解析合并
                if (binaryExpr.NodeType == ExpressionType.Equal || binaryExpr.NodeType == ExpressionType.NotEqual)
                {
                    //处理null != a.UserName和"kevin" == a.UserName情况
                    if (!leftSegment.HasField && rightSegment.HasField)
                        this.Swap(ref leftSegment, ref rightSegment);
                    if (leftSegment == SqlSegment.Null && rightSegment != SqlSegment.Null)
                        this.Swap(ref leftSegment, ref rightSegment);

                    //处理!(a.IsEnabled==true)情况，bool类型，最外层再做defer处理
                    if (binaryExpr.Left.Type == typeof(bool) && leftSegment.HasField && !rightSegment.HasField)
                    {
                        leftSegment.Push(new DeferredExpr { OperationType = OperationType.Equal, Value = SqlSegment.True });
                        if (!(bool)rightSegment.Value)
                            leftSegment.Push(new DeferredExpr { OperationType = OperationType.Not });
                        if (binaryExpr.NodeType == ExpressionType.NotEqual)
                            leftSegment.Push(new DeferredExpr { OperationType = OperationType.Not });
                        return leftSegment;
                    }
                    if (rightSegment == SqlSegment.Null)
                    {
                        leftSegment.Push(new DeferredExpr
                        {
                            OperationType = OperationType.Equal,
                            Value = SqlSegment.Null
                        });
                        if (binaryExpr.NodeType == ExpressionType.NotEqual)
                            leftSegment.Push(new DeferredExpr { OperationType = OperationType.Not });
                        return leftSegment;
                    }
                }
                //带有参数成员访问+常量/变量+带参数的函数调用的表达式
                var operators = this.OrmProvider.GetBinaryOperator(binaryExpr.NodeType);
                //如果是IsParameter,HasField,IsExpression,IsMethodCall直接返回,是SQL
                //如果是变量或是要求变成参数的常量，变成@p返回
                //如果是常量获取当前类型值，再转成QuotedValue值
                //就是枚举类型有问题，单独处理
                //... WHERE (int)(a.Price * a.Quartity)>500
                //SELECT TotalAmount = (int)(amount + (a.Price + increasedPrice) * (a.Quartity + increasedCount)) ...FROM ...
                //SELECT OrderNo = $"OrderNo-{f.CreatedAt.ToString("yyyyMMdd")}-{f.Id}"...FROM ...

                var leftType = leftSegment.ExpectType ?? binaryExpr.Left.Type;
                var rightType = rightSegment.ExpectType ?? binaryExpr.Right.Type;

                if ((leftType.IsEnum || rightType.IsEnum) && calcOps.Contains(operators))
                    throw new NotSupportedException($"枚举类成员{leftSegment.MemberMapper.MemberName}对应的数据库类型为非数字类型，不能进行{operators}操作，可以使用=、<>、IN、EXISTS等操作来代替，表达式：{binaryExpr}");

                //在调用GetQuotedValue方法前，确保左右两侧的类型一致，并都根据MemberMapper的映射类型表生成SQL语句
                this.ChangeSameType(leftSegment, rightSegment);
                string strLeft = this.GetQuotedValue(leftSegment);
                string strRight = this.GetQuotedValue(rightSegment);

                if (binaryExpr.NodeType == ExpressionType.Coalesce)
                {
                    leftSegment.IsFieldType = true;
                    return this.Merge(leftSegment, rightSegment, $"{operators}({strLeft},{strRight})", false, true);
                }

                if (leftSegment.IsExpression)
                    strLeft = $"({strLeft})";
                if (rightSegment.IsExpression)
                    strRight = $"({strRight})";
                return this.Merge(leftSegment, rightSegment, $"{strLeft}{operators}{strRight}", true, false);
        }
        return sqlSegment;
    }
    public virtual SqlSegment VisitMemberAccess(SqlSegment sqlSegment)
    {
        var memberExpr = sqlSegment.Expression as MemberExpression;
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
                    return sqlSegment.Next(memberExpr.Expression);
                }
                else if (memberExpr.Member.Name == nameof(Nullable<bool>.Value))
                    return sqlSegment.Next(memberExpr.Expression);
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
                var tableSegment = this.TableAlias[parameterName];
                tableSegment.Mapper ??= this.MapProvider.GetEntityMap(tableSegment.EntityType);
                var memberMapper = tableSegment.Mapper.GetMemberMap(memberExpr.Member.Name);

                if (memberMapper.IsIgnore)
                    throw new Exception($"类{tableSegment.EntityType.FullName}的成员{memberMapper.MemberName}是忽略成员无法访问");
                if (memberMapper.MemberType.IsEntityType(out _) && !memberMapper.IsNavigation && memberMapper.TypeHandler == null)
                    throw new Exception($"类{tableSegment.EntityType.FullName}的成员{memberExpr.Member.Name}不是值类型，未配置为导航属性也没有配置TypeHandler");

                //.NET枚举类型总是解析成对应的UnderlyingType数值类型，如：a.Gender ?? Gender.Male == Gender.Male
                //如果枚举类型对应的数据库类型是字符串就会有问题，需要把数字变为枚举，再把枚举的名字字符串完成后续操作。
                if (memberMapper.MemberType.IsEnumType(out var expectType, out _) && memberMapper.DbDefaultType == typeof(string))
                {
                    sqlSegment.ExpectType = expectType;
                    sqlSegment.TargetType = memberMapper.DbDefaultType;
                }

                var fieldName = this.OrmProvider.GetFieldName(memberMapper.FieldName);
                if (this.IsNeedAlias)
                    fieldName = tableSegment.AliasName + "." + fieldName;

                sqlSegment.HasField = true;
                sqlSegment.IsConstant = false;
                sqlSegment.TableSegment = tableSegment;
                sqlSegment.FromMember = memberMapper.Member;
                sqlSegment.MemberMapper = memberMapper;
                sqlSegment.Value = fieldName;
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
    public virtual SqlSegment VisitConstant(SqlSegment sqlSegment)
    {
        var constantExpr = sqlSegment.Expression as ConstantExpression;
        if (constantExpr.Value == null)
            return SqlSegment.Null;

        sqlSegment.Value = constantExpr.Value;
        sqlSegment.IsConstant = true;
        return sqlSegment;
    }
    public virtual SqlSegment VisitMethodCall(SqlSegment sqlSegment)
    {
        var methodCallExpr = sqlSegment.Expression as MethodCallExpression;
        if (methodCallExpr.Method.DeclaringType == typeof(Sql)
            || typeof(IAggregateSelect).IsAssignableFrom(methodCallExpr.Method.DeclaringType))
            return this.VisitSqlMethodCall(sqlSegment);

        if (!sqlSegment.IsDeferredFields && this.OrmProvider.TryGetMethodCallSqlFormatter(methodCallExpr, out var formatter))
            return formatter.Invoke(this, methodCallExpr, methodCallExpr.Object, sqlSegment.DeferredExprs, methodCallExpr.Arguments.ToArray());

        string fields = null;
        List<ReaderField> readerFields = null;
        List<Expression> constArgs = null;
        if (methodCallExpr.Arguments != null && methodCallExpr.Arguments.Count > 0)
        {
            readerFields = new List<ReaderField>();
            constArgs = new List<Expression>();
            var builder = new StringBuilder();
            for (int i = 0; i < methodCallExpr.Arguments.Count; i++)
            {
                var argumentSegment = this.VisitAndDeferred(new SqlSegment { Expression = methodCallExpr.Arguments[i] });
                if (argumentSegment.HasField)
                {
                    sqlSegment.Merge(argumentSegment);
                    var fieldName = argumentSegment.Value.ToString();
                    readerFields.Add(new ReaderField
                    {
                        Index = i,
                        FieldType = ReaderFieldType.Field,
                        TableSegment = argumentSegment.TableSegment,
                        FromMember = argumentSegment.FromMember,
                        Body = fieldName
                    });
                    if (builder.Length > 0)
                        builder.Append(',');
                    builder.Append(fieldName);
                }
                else constArgs.Add(Expression.Constant(argumentSegment.Value));
            }
            if (readerFields.Count > 0)
                fields = builder.ToString();
        }

        if (sqlSegment.IsDeferredFields || !string.IsNullOrEmpty(fields))
        {
            if (readerFields == null)
                fields = "NULL";
            return sqlSegment.Change(new ReaderField
            {
                FieldType = ReaderFieldType.DeferredFields,
                Body = fields,
                DeferCallTarget = methodCallExpr.Object,
                DeferCallMethod = methodCallExpr.Method,
                DeferCallArgs = constArgs,
                ReaderFields = readerFields
            }, false, false, true);
        }
        return this.Evaluate(sqlSegment);
    }
    public virtual SqlSegment VisitParameter(SqlSegment sqlSegment)
    {
        var parameterExpr = sqlSegment.Expression as ParameterExpression;

        if (this.IsFromQuery)
            throw new NotSupportedException($"FROM子查询中不支持实体类型成员MemberAccess表达式访问，只支持基础字段访问访问,{parameterExpr}");

        var fromSegment = this.TableAlias[parameterExpr.Name];
        //参数访问的是模型，通常是成员访问或是SELECT语句的实体访问，成员访问已经处理了参数访问，不会走到此方法
        //参数访问通常都是SELECT语句的实体访问
        if (!this.IsSelect) throw new NotSupportedException($"不支持的参数表达式访问，{parameterExpr}");

        var readerFields = this.AddTableRecursiveReaderFields(sqlSegment.ReaderIndex, fromSegment);
        return sqlSegment.Change(readerFields, false);
    }
    public virtual SqlSegment VisitNew(SqlSegment sqlSegment)
    {
        throw new NotImplementedException();
    }
    public virtual SqlSegment VisitMemberInit(SqlSegment sqlSegment)
    {
        throw new NotImplementedException();
    }
    public virtual SqlSegment VisitNewArray(SqlSegment sqlSegment)
    {
        sqlSegment.IsArray = true;
        var newArrayExpr = sqlSegment.Expression as NewArrayExpression;
        var result = new List<SqlSegment>();
        bool isConstantValue = true;
        foreach (var elementExpr in newArrayExpr.Expressions)
        {
            var elementSegment = new SqlSegment { Expression = elementExpr };
            elementSegment = this.VisitAndDeferred(elementSegment);
            if (!elementSegment.IsConstant)
                isConstantValue = false;
            sqlSegment.Merge(elementSegment);
            result.Add(elementSegment);
        }
        return sqlSegment.Change(result, isConstantValue);
    }
    public virtual SqlSegment VisitIndexExpression(SqlSegment sqlSegment)
    {
        if (sqlSegment.Expression.IsParameter(out _))
            throw new NotSupportedException("索引表达式不支持Parameter访问操作");
        return this.Evaluate(sqlSegment);
    }
    public virtual SqlSegment VisitConditional(SqlSegment sqlSegment)
    {
        var conditionalExpr = sqlSegment.Expression as ConditionalExpression;
        sqlSegment = this.Visit(sqlSegment.Next(conditionalExpr.Test));
        var ifTrueSegment = this.Visit(new SqlSegment { Expression = conditionalExpr.IfTrue });
        var ifFalseSegment = this.Visit(new SqlSegment { Expression = conditionalExpr.IfFalse });
        if (!this.ChangeSameType(ifTrueSegment, ifFalseSegment))
            this.ChangeSameType(ifFalseSegment, ifTrueSegment);

        var leftArgument = this.GetQuotedValue(ifTrueSegment);
        var rightArgument = this.GetQuotedValue(ifFalseSegment);
        sqlSegment.Merge(ifTrueSegment);
        sqlSegment.Merge(ifFalseSegment);
        this.ChangeSameType(ifTrueSegment, sqlSegment);
        sqlSegment.IsFieldType = true;
        return this.VisitDeferredBoolConditional(sqlSegment, conditionalExpr.IfTrue.Type == typeof(bool), leftArgument, rightArgument);
    }
    public virtual SqlSegment VisitListInit(SqlSegment sqlSegment)
    {
        sqlSegment.IsArray = true;
        var listExpr = sqlSegment.Expression as ListInitExpression;
        var result = new List<SqlSegment>();
        bool isConstantValue = true;
        foreach (var elementInit in listExpr.Initializers)
        {
            if (elementInit.Arguments.Count == 0)
                continue;
            var elementSegment = new SqlSegment { Expression = elementInit.Arguments[0] };
            elementSegment = this.VisitAndDeferred(elementSegment);
            if (!elementSegment.IsConstant)
                isConstantValue = false;
            result.Add(elementSegment);
        }
        return sqlSegment.Change(result, isConstantValue);
    }
    public virtual SqlSegment VisitTypeIs(SqlSegment sqlSegment)
    {
        var binaryExpr = sqlSegment.Expression as TypeBinaryExpression;
        if (!binaryExpr.Expression.IsParameter(out _))
            return this.Evaluate(sqlSegment);
        if (binaryExpr.TypeOperand == typeof(DBNull))
        {
            sqlSegment.Push(new DeferredExpr
            {
                OperationType = OperationType.Equal,
                Value = SqlSegment.Null
            });
            return this.Visit(sqlSegment.Next(binaryExpr.Expression));
        }
        throw new NotSupportedException($"不支持的表达式操作，{sqlSegment.Expression}");
    }
    public virtual SqlSegment Evaluate(SqlSegment sqlSegment)
    {
        var objValue = this.Evaluate(sqlSegment.Expression);
        if (objValue == null)
            return SqlSegment.Null;

        return sqlSegment.Change(objValue);
    }
    public virtual T Evaluate<T>(Expression expr)
    {
        var objValue = this.Evaluate(expr);
        if (objValue == null)
            return default;
        return (T)objValue;
    }
    public virtual object Evaluate(Expression expr)
    {
        var lambdaExpr = Expression.Lambda(expr);
        return lambdaExpr.Compile().DynamicInvoke();
    }
    public virtual object EvaluateAndCache(object valueOrEntity, string memberName)
    {
        if (valueOrEntity is IDictionary<string, object> dict)
            return dict[memberName];

        var type = valueOrEntity.GetType();
        if (type.IsEntityType(out var underlyingType))
        {
            var cacheKey = HashCode.Combine(underlyingType, memberName);
            if (!memberGetterCache.TryGetValue(cacheKey, out var getter))
            {
                var objExpr = Expression.Parameter(typeof(object), "obj");
                var typedObjExpr = Expression.Convert(objExpr, type);
                Expression valueExpr = Expression.PropertyOrField(typedObjExpr, memberName);
                if (valueExpr.Type != typeof(object))
                    valueExpr = Expression.Convert(valueExpr, typeof(object));
                getter = Expression.Lambda<Func<object, object>>(valueExpr, objExpr).Compile();
                memberGetterCache.TryAdd(cacheKey, getter);
            }
            return getter.Invoke(valueOrEntity);
        }
        return valueOrEntity;
    }
    public virtual SqlSegment VisitSqlMethodCall(SqlSegment sqlSegment)
    {
        var methodCallExpr = sqlSegment.Expression as MethodCallExpression;
        LambdaExpression lambdaExpr = null;
        switch (methodCallExpr.Method.Name)
        {
            case "FlattenTo"://通常在最外层的SELECT中转为其他类型
                var targetType = methodCallExpr.Method.ReturnType;
                if (methodCallExpr.Arguments != null && methodCallExpr.Arguments.Count > 0)
                {
                    lambdaExpr = sqlSegment.OriginalExpression as LambdaExpression;
                    var visitedParameters = lambdaExpr.Parameters;
                    lambdaExpr = this.EnsureLambda(methodCallExpr.Arguments[0]);
                    lambdaExpr = Expression.Lambda(lambdaExpr.Body, visitedParameters);
                }
                var readerFields = this.FlattenFieldsTo(targetType, lambdaExpr);
                sqlSegment.ChangeValue(readerFields);
                break;
            case "Deferred":
                sqlSegment.IsDeferredFields = true;
                sqlSegment = this.VisitMethodCall(sqlSegment.Next(methodCallExpr.Arguments[0]));
                break;
            case "IsNull":
                if (methodCallExpr.Arguments != null && methodCallExpr.Arguments.Count > 0)
                {
                    sqlSegment.Push(new DeferredExpr { OperationType = OperationType.Equal, Value = SqlSegment.Null });
                    sqlSegment = this.VisitAndDeferred(sqlSegment.Next(methodCallExpr.Arguments[0]));
                }
                break;
            case "ToParameter":
                sqlSegment.IsParameterized = true;
                sqlSegment = this.Change(this.Visit(sqlSegment.Next(methodCallExpr.Arguments[0])));
                sqlSegment.IsParameterized = false;
                break;
            case "In":
                var elementType = methodCallExpr.Method.GetGenericArguments()[0];
                var type = methodCallExpr.Arguments[1].Type;
                var fieldSegment = this.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                if (type.IsArray || typeof(IEnumerable<>).MakeGenericType(elementType).IsAssignableFrom(type))
                {
                    var rightSegment = this.VisitAndDeferred(new SqlSegment { Expression = methodCallExpr.Arguments[1] });
                    if (rightSegment == SqlSegment.Null)
                        return sqlSegment.Change("1=0", false, true, false);
                    var enumerable = rightSegment.Value as IEnumerable;

                    var builder = new StringBuilder();
                    foreach (var item in enumerable)
                    {
                        if (builder.Length > 0) builder.Append(',');
                        builder.Append(this.OrmProvider.GetQuotedValue(item));
                    }
                    sqlSegment.ChangeValue(builder.ToString());
                }
                else
                {
                    lambdaExpr = methodCallExpr.Arguments[1] as LambdaExpression;
                    var sql = this.VisitFromQuery(lambdaExpr, out var isNeedAlias);
                    sqlSegment.ChangeValue(sql);
                    if (isNeedAlias)
                    {
                        //重新解析一下，增加别名
                        if (!this.IsNeedAlias)
                        {
                            this.IsNeedAlias = true;
                            fieldSegment = this.Visit(fieldSegment.Next(methodCallExpr.Arguments[0]));
                        }
                    }
                }
                if (sqlSegment.HasDeferrdNot())
                    sqlSegment.Change($"{fieldSegment} NOT IN ({sqlSegment})", false, true, false);
                else sqlSegment.Change($"{fieldSegment} IN ({sqlSegment})", false, true, false);
                break;
            case "Exists":
                lambdaExpr = this.EnsureLambda(methodCallExpr.Arguments[0]);
                this.IsNeedAlias = true;
                string existsSql = null;
                var subTableTypes = methodCallExpr.Method.GetGenericArguments();
                if (subTableTypes != null && subTableTypes.Length > 0)
                {
                    var removeIndices = new List<int>();
                    var builder = new StringBuilder("SELECT * FROM ");
                    int index = 0;
                    foreach (var subTableType in subTableTypes)
                    {
                        var subTableMapper = this.MapProvider.GetEntityMap(subTableType);
                        var aliasName = lambdaExpr.Parameters[index].Name;
                        var tableSegment = new TableSegment
                        {
                            EntityType = subTableType,
                            AliasName = aliasName
                        };
                        removeIndices.Add(this.Tables.Count);
                        this.Tables.Add(tableSegment);
                        this.TableAlias[aliasName] = tableSegment;
                        if (index > 0) builder.Append(',');
                        builder.Append(this.OrmProvider.GetTableName(subTableMapper.TableName));
                        builder.Append($" {tableSegment.AliasName}");
                        index++;
                    }
                    builder.Append(" WHERE ");
                    builder.Append(this.VisitConditionExpr(lambdaExpr.Body));
                    removeIndices.Reverse();
                    removeIndices.ForEach(f => this.Tables.RemoveAt(f));
                    existsSql = builder.ToString();
                }
                else existsSql = this.VisitFromQuery(lambdaExpr, out _);

                if (sqlSegment.HasDeferrdNot())
                    sqlSegment.Change($"NOT EXISTS({existsSql})", false, false, true);
                else sqlSegment.Change($"EXISTS({existsSql})", false, false, true);
                break;
            case "Count":
            case "LongCount":
                if (methodCallExpr.Arguments != null && methodCallExpr.Arguments.Count == 1)
                {
                    if (methodCallExpr.Arguments[0].NodeType != ExpressionType.MemberAccess)
                        throw new NotSupportedException("不支持的表达式，只支持MemberAccess类型表达式");

                    sqlSegment = this.VisitMemberAccess(sqlSegment.Next(methodCallExpr.Arguments[0]));
                    sqlSegment.Change($"COUNT({sqlSegment})", false, false, true);
                }
                else sqlSegment.Change("COUNT(1)", false, false, true);
                if (this.IsSelect || this.IsWhere)
                    this.Tables.FindAll(f => f.IsMaster).ForEach(f => f.IsUsed = true);
                break;
            case "CountDistinct":
            case "LongCountDistinct":
                if (methodCallExpr.Arguments != null && methodCallExpr.Arguments.Count == 1)
                {
                    if (methodCallExpr.Arguments[0].NodeType != ExpressionType.MemberAccess)
                        throw new NotSupportedException("不支持的表达式，只支持MemberAccess类型表达式");

                    sqlSegment = this.VisitMemberAccess(sqlSegment.Next(methodCallExpr.Arguments[0]));
                    sqlSegment.Change($"COUNT(DISTINCT {sqlSegment})", false, false, true);
                }
                if (this.IsSelect || this.IsWhere)
                    this.Tables.FindAll(f => f.IsMaster).ForEach(f => f.IsUsed = true);
                break;
            case "Sum":
                if (methodCallExpr.Arguments != null && methodCallExpr.Arguments.Count == 1)
                {
                    if (methodCallExpr.Arguments[0].NodeType != ExpressionType.MemberAccess)
                        throw new NotSupportedException("不支持的表达式，只支持MemberAccess类型表达式");
                    sqlSegment = this.VisitMemberAccess(sqlSegment.Next(methodCallExpr.Arguments[0]));
                    sqlSegment.Change($"SUM({sqlSegment})", false, false, true);
                }
                if (this.IsSelect || this.IsWhere)
                    this.Tables.FindAll(f => f.IsMaster).ForEach(f => f.IsUsed = true);
                break;
            case "Avg":
                if (methodCallExpr.Arguments != null && methodCallExpr.Arguments.Count == 1)
                {
                    if (methodCallExpr.Arguments[0].NodeType != ExpressionType.MemberAccess)
                        throw new NotSupportedException("不支持的表达式，只支持MemberAccess类型表达式");
                    sqlSegment = this.VisitMemberAccess(sqlSegment.Next(methodCallExpr.Arguments[0]));
                    sqlSegment.Change($"AVG({sqlSegment})", false, false, true);
                }
                if (this.IsSelect || this.IsWhere)
                    this.Tables.FindAll(f => f.IsMaster).ForEach(f => f.IsUsed = true);
                break;
            case "Max":
                if (methodCallExpr.Arguments != null && methodCallExpr.Arguments.Count == 1)
                {
                    if (methodCallExpr.Arguments[0].NodeType != ExpressionType.MemberAccess)
                        throw new NotSupportedException("不支持的表达式，只支持MemberAccess类型表达式");
                    sqlSegment = this.VisitMemberAccess(sqlSegment.Next(methodCallExpr.Arguments[0]));
                    sqlSegment.Change($"MAX({sqlSegment})", false, false, true);
                }
                if (this.IsSelect || this.IsWhere)
                    this.Tables.FindAll(f => f.IsMaster).ForEach(f => f.IsUsed = true);
                break;
            case "Min":
                if (methodCallExpr.Arguments != null && methodCallExpr.Arguments.Count == 1)
                {
                    if (methodCallExpr.Arguments[0].NodeType != ExpressionType.MemberAccess)
                        throw new NotSupportedException("不支持的表达式，只支持MemberAccess类型表达式");
                    sqlSegment = this.VisitMemberAccess(sqlSegment.Next(methodCallExpr.Arguments[0]));
                    sqlSegment.Change($"MIN({sqlSegment})", false, false, true);
                }
                if (this.IsSelect || this.IsWhere)
                    this.Tables.FindAll(f => f.IsMaster).ForEach(f => f.IsUsed = true);
                break;
        }
        return sqlSegment;
    }
    public virtual bool IsStringConcatOperator(SqlSegment sqlSegment, out SqlSegment result)
    {
        var binaryExpr = sqlSegment.Expression as BinaryExpression;
        if (binaryExpr.NodeType == ExpressionType.Add && (binaryExpr.Left.Type == typeof(string) || binaryExpr.Right.Type == typeof(string)))
        {
            //先打开所有要拼接的部分，最后再拼接
            var concatExprs = this.SplitConcatList(sqlSegment.Expression);
            //调用拼接方法Concat,每个数据库Provider都实现了这个方法
            var methodInfo = typeof(string).GetMethod(nameof(string.Concat), new Type[] { typeof(object[]) });
            var parameters = Expression.NewArrayInit(typeof(object), concatExprs);
            var methodCallExpr = Expression.Call(methodInfo, parameters);
            sqlSegment.Expression = methodCallExpr;
            this.OrmProvider.TryGetMethodCallSqlFormatter(methodCallExpr, out var formater);
            //返回的SQL表达式中直接拼接好          
            result = formater.Invoke(this, binaryExpr, null, null, concatExprs);
            return true;
        }
        result = null;
        return false;
    }
    public virtual string VisitConditionExpr(Expression conditionExpr)
    {
        if (conditionExpr.NodeType == ExpressionType.AndAlso || conditionExpr.NodeType == ExpressionType.OrElse)
        {
            var completedExprs = this.VisitLogicBinaryExpr(conditionExpr);
            if (conditionExpr.NodeType == ExpressionType.OrElse)
                this.LastWhereNodeType = OperationType.Or;
            else this.LastWhereNodeType = OperationType.And;

            var builder = new StringBuilder();
            foreach (var completedExpr in completedExprs)
            {
                if (completedExpr.ExpressionType == ConditionType.OperatorType)
                {
                    builder.Append(completedExpr.Body);
                    continue;
                }
                var sqlSegment = this.VisitAndDeferred(this.CreateConditionSegment(completedExpr.Body as Expression));
                builder.Append(sqlSegment);
            }
            return builder.ToString();
        }
        return this.VisitAndDeferred(this.CreateConditionSegment(conditionExpr)).ToString();
    }
    public virtual List<Expression> ConvertFormatToConcatList(Expression[] argsExprs)
    {
        var format = this.Evaluate<string>(argsExprs[0]);
        int index = 1, formatIndex = 0;
        var parameters = new List<Expression>();
        for (int i = 1; i < argsExprs.Length; i++)
        {
            switch (argsExprs[i].NodeType)
            {
                case ExpressionType.ListInit:
                    var listExpr = argsExprs[i] as ListInitExpression;
                    foreach (var elementInit in listExpr.Initializers)
                    {
                        if (elementInit.Arguments.Count == 0)
                            continue;
                        parameters.Add(elementInit.Arguments[0]);
                    }
                    break;
                case ExpressionType.NewArrayBounds:
                case ExpressionType.NewArrayInit:
                    var newArrayExpr = argsExprs[i] as NewArrayExpression;
                    foreach (var elementExpr in newArrayExpr.Expressions)
                    {
                        parameters.Add(elementExpr);
                    }
                    break;
                default: parameters.Add(argsExprs[i]); break;
            }
        }
        index = 0;
        var result = new List<Expression>();
        while (formatIndex < format.Length)
        {
            var nextIndex = format.IndexOf('{', formatIndex);
            if (nextIndex > formatIndex)
            {
                var constValue = format.Substring(formatIndex, nextIndex - formatIndex);
                result.Add(Expression.Constant(constValue));
            }
            result.AddRange(this.SplitConcatList(parameters[index]));
            index++;
            formatIndex = format.IndexOf('}', nextIndex + 2) + 1;
        }
        return result;
    }
    public virtual List<Expression> SplitConcatList(Expression[] argsExprs)
    {
        var completedExprs = new List<Expression>();
        var deferredExprs = new Stack<Expression>();
        Func<Expression, bool> isConcatBinary = f =>
        {
            if (f is BinaryExpression binaryExpr && binaryExpr.NodeType == ExpressionType.Add && binaryExpr.Type == typeof(string)
                && (binaryExpr.Left.Type == typeof(string) || binaryExpr.Right.Type == typeof(string)))
                return true;
            if (f is MethodCallExpression callExpr && callExpr.Method.Name == "Concat")
                return true;
            return false;
        };
        Expression nextExpr = null;
        for (int i = argsExprs.Length - 1; i > 0; i--)
        {
            deferredExprs.Push(argsExprs[i]);
        }
        nextExpr = argsExprs[0];
        while (true)
        {
            if (isConcatBinary(nextExpr))
            {
                //字符串连接+
                if (nextExpr is BinaryExpression binaryExpr)
                {
                    if (isConcatBinary(binaryExpr.Left))
                    {
                        deferredExprs.Push(binaryExpr.Right);
                        nextExpr = binaryExpr.Left;
                        continue;
                    }
                    completedExprs.Add(nextExpr);
                    if (isConcatBinary(binaryExpr.Right))
                    {
                        nextExpr = binaryExpr.Right;
                        continue;
                    }
                    completedExprs.Add(binaryExpr.Right);
                    if (!deferredExprs.TryPop(out nextExpr))
                        break;
                    continue;
                }
                else
                {
                    var callExpr = nextExpr as MethodCallExpression;
                    for (int i = callExpr.Arguments.Count - 1; i > 0; i--)
                    {
                        deferredExprs.Push(callExpr.Arguments[i]);
                    }
                    nextExpr = callExpr.Arguments[0];
                    continue;
                }
            }
            completedExprs.Add(nextExpr);
            if (!deferredExprs.TryPop(out nextExpr))
                break;
        }
        return completedExprs;
    }
    public virtual Expression[] SplitConcatList(Expression concatExpr)
    {
        var completedExprs = new List<Expression>();
        var deferredExprs = new Stack<Expression>();
        Func<Expression, bool> isConcatBinary = f =>
        {
            if (f is BinaryExpression binaryExpr && binaryExpr.NodeType == ExpressionType.Add && binaryExpr.Type == typeof(string)
                && (binaryExpr.Left.Type == typeof(string) || binaryExpr.Right.Type == typeof(string)))
                return true;
            if (f is MethodCallExpression callExpr && callExpr.Method.Name == "Concat")
                return true;
            return false;
        };
        var nextExpr = concatExpr;
        while (true)
        {
            if (isConcatBinary(nextExpr))
            {
                //字符串连接+
                if (nextExpr is BinaryExpression binaryExpr)
                {
                    if (isConcatBinary(binaryExpr.Left))
                    {
                        deferredExprs.Push(binaryExpr.Right);
                        nextExpr = binaryExpr.Left;
                        continue;
                    }
                    completedExprs.Add(binaryExpr.Left);
                    if (isConcatBinary(binaryExpr.Right))
                    {
                        nextExpr = binaryExpr.Right;
                        continue;
                    }
                    completedExprs.Add(binaryExpr.Right);
                    if (!deferredExprs.TryPop(out nextExpr))
                        break;
                    continue;
                }
                else
                {
                    //Concat方法
                    var callExpr = nextExpr as MethodCallExpression;
                    for (int i = callExpr.Arguments.Count - 1; i > 0; i--)
                    {
                        deferredExprs.Push(callExpr.Arguments[i]);
                    }
                    nextExpr = callExpr.Arguments[0];
                    continue;
                }
            }
            completedExprs.Add(nextExpr);
            if (!deferredExprs.TryPop(out nextExpr))
                break;
        }
        return completedExprs.ToArray();
    }
    public virtual List<ReaderField> AddTableRecursiveReaderFields(int readerIndex, TableSegment fromSegment)
    {
        var readerFields = new List<ReaderField>();
        fromSegment.Mapper ??= this.MapProvider.GetEntityMap(fromSegment.EntityType);
        var lastReaderField = new ReaderField
        {
            Index = readerIndex,
            FieldType = ReaderFieldType.Entity,
            TableSegment = fromSegment,
            ReaderFields = this.FlattenTableFields(fromSegment)
            //最外层Select对象的成员，位于顶层, FromMember暂时不设置值，到Select时候去设置 
        };
        readerFields.Add(lastReaderField);
        this.AddIncludeTables(lastReaderField, readerFields);
        return readerFields;
    }
    public virtual string VisitFromQuery(LambdaExpression lambdaExpr, out bool isNeedAlias)
    {
        var currentExpr = lambdaExpr.Body;
        var callStack = new Stack<MethodCallExpression>();
        while (true)
        {
            if (currentExpr.NodeType == ExpressionType.Parameter)
                break;

            if (currentExpr is MethodCallExpression callExpr)
            {
                callStack.Push(callExpr);
                currentExpr = callExpr.Object;
            }
        }
        var queryVisitor = this.OrmProvider.NewQueryVisitor(this.DbKey, this.MapProvider, this.IsParameterized, this.TableAsStart, this.ParameterPrefix, this.DbParameters);
        //TODO:参数与command 
        queryVisitor.IsNeedAlias = this.IsNeedAlias;

        while (callStack.TryPop(out var callExpr))
        {
            var genericArguments = callExpr.Method.GetGenericArguments();
            LambdaExpression lambdaArgsExpr = null;
            switch (callExpr.Method.Name)
            {
                case "From":
                case "Union":
                case "UnionAll":
                    queryVisitor.From(this.Evaluate<char>(callExpr.Arguments[0]), genericArguments);
                    break;
                case "InnerJoin":
                case "LeftJoin":
                case "RightJoin":
                    var joinType = callExpr.Method.Name switch
                    {
                        "LeftJoin" => "LEFT JOIN",
                        "RightJoin" => "RIGHT JOIN",
                        _ => "INNER JOIN"
                    };
                    queryVisitor.IsNeedAlias = true;
                    lambdaArgsExpr = this.EnsureLambda(callExpr.Arguments[0]);
                    if (lambdaArgsExpr.Body.GetParameters(out var visitedParameters))
                    {
                        foreach (var tableAlias in this.TableAlias.Keys)
                        {
                            if (visitedParameters.Exists(f => f.Name == tableAlias))
                                queryVisitor.AddTable(this.TableAlias[tableAlias]);
                        }
                        lambdaArgsExpr = Expression.Lambda(lambdaArgsExpr.Body, visitedParameters);
                    }
                    queryVisitor.Join(joinType, genericArguments[0], lambdaArgsExpr);
                    break;
                case "Where":
                    lambdaArgsExpr = this.EnsureLambda(callExpr.Arguments[0]);
                    queryVisitor.InitTableAlias(lambdaArgsExpr);
                    if (lambdaArgsExpr.Body.GetParameters(out visitedParameters))
                    {
                        queryVisitor.IsNeedAlias = true;
                        foreach (var tableAlias in this.TableAlias.Keys)
                        {
                            if (visitedParameters.Exists(f => f.Name == tableAlias))
                            {
                                var tableSegment = this.TableAlias[tableAlias];
                                queryVisitor.AddAliasTable(tableAlias, tableSegment);
                            }
                        }
                        lambdaArgsExpr = Expression.Lambda(lambdaArgsExpr.Body, visitedParameters);
                    }
                    queryVisitor.Where(lambdaArgsExpr, false);
                    break;
                case "And":
                    lambdaArgsExpr = this.EnsureLambda(callExpr.Arguments[1]);
                    if (this.Evaluate<bool>(callExpr.Arguments[0]))
                        queryVisitor.And(lambdaArgsExpr);
                    break;
                case "GroupBy":
                    lambdaArgsExpr = this.EnsureLambda(callExpr.Arguments[0]);
                    queryVisitor.GroupBy(lambdaArgsExpr);
                    break;
                case "Having":
                    if (callExpr.Arguments.Count > 1 && this.Evaluate<bool>(callExpr.Arguments[0]))
                    {
                        lambdaArgsExpr = this.EnsureLambda(callExpr.Arguments[1]);
                        queryVisitor.Having(lambdaArgsExpr);
                    }
                    else
                    {
                        lambdaArgsExpr = this.EnsureLambda(callExpr.Arguments[0]);
                        queryVisitor.Having(lambdaArgsExpr);
                    }
                    break;
                case "OrderBy":
                    lambdaArgsExpr = this.EnsureLambda(callExpr.Arguments[0]);
                    queryVisitor.OrderBy("ASC", lambdaArgsExpr);
                    break;
                case "OrderByDescending":
                    lambdaArgsExpr = this.EnsureLambda(callExpr.Arguments[0]);
                    queryVisitor.OrderBy("DESC", lambdaArgsExpr);
                    break;
                case "Select":
                case "SelectAggregate":
                    if (callExpr.Arguments[0].NodeType == ExpressionType.Constant)
                        queryVisitor.Select(this.Evaluate<string>(callExpr.Arguments[0]));
                    else
                    {
                        lambdaArgsExpr = this.EnsureLambda(callExpr.Arguments[0]);
                        queryVisitor.Select(null, lambdaArgsExpr);
                    }
                    break;
                case "Distinct":
                    queryVisitor.Distinct();
                    break;
            }
        }
        var result = queryVisitor.BuildSql(out _);
        isNeedAlias = queryVisitor.IsNeedAlias;
        return result;
    }
    public virtual SqlSegment Merge(SqlSegment sqlSegment, SqlSegment rightSegment, object segmentValue)
    {
        sqlSegment.IsConstant = sqlSegment.IsConstant && rightSegment.IsConstant;
        sqlSegment.IsVariable = sqlSegment.IsVariable || rightSegment.IsVariable;
        sqlSegment.HasField = sqlSegment.HasField || rightSegment.HasField;
        sqlSegment.IsParameter = sqlSegment.IsParameter || rightSegment.IsParameter;
        return this.Change(sqlSegment, segmentValue);
    }
    public virtual SqlSegment Merge(SqlSegment sqlSegment, SqlSegment args0Segment, SqlSegment args1Segment, object segmentValue)
    {
        sqlSegment.IsConstant = sqlSegment.IsConstant && args0Segment.IsConstant && args1Segment.IsConstant;
        sqlSegment.IsVariable = sqlSegment.IsVariable || args0Segment.IsVariable || args1Segment.IsVariable;
        sqlSegment.HasField = sqlSegment.HasField || args0Segment.HasField || args1Segment.HasField;
        sqlSegment.IsParameter = sqlSegment.IsParameter || args0Segment.IsParameter || args1Segment.IsParameter;
        return this.Change(sqlSegment, segmentValue);
    }
    public virtual SqlSegment Merge(SqlSegment sqlSegment, SqlSegment rightSegment, object segmentValue, bool isExpression, bool isMethodCall)
    {
        sqlSegment.IsConstant = sqlSegment.IsConstant && rightSegment.IsConstant;
        sqlSegment.IsVariable = sqlSegment.IsVariable || rightSegment.IsVariable;
        sqlSegment.HasField = sqlSegment.HasField || rightSegment.HasField;
        sqlSegment.IsParameter = sqlSegment.IsParameter || rightSegment.IsParameter;
        return this.Change(sqlSegment, segmentValue, isExpression, isMethodCall);
    }
    public virtual SqlSegment Merge(SqlSegment sqlSegment, SqlSegment args0Segment, SqlSegment args1Segment, object segmentValue, bool isExpression, bool isMethodCall)
    {
        sqlSegment.IsConstant = sqlSegment.IsConstant && args0Segment.IsConstant && args1Segment.IsConstant;
        sqlSegment.IsVariable = sqlSegment.IsVariable || args0Segment.IsVariable || args1Segment.IsVariable;
        sqlSegment.HasField = sqlSegment.HasField || args0Segment.HasField || args1Segment.HasField;
        sqlSegment.IsParameter = sqlSegment.IsParameter || args0Segment.IsParameter || args1Segment.IsParameter;
        return this.Change(sqlSegment, segmentValue, isExpression, isMethodCall);
    }
    public virtual SqlSegment Change(SqlSegment sqlSegment)
    {
        if (sqlSegment.IsVariable || (sqlSegment.IsParameterized || this.IsParameterized) && sqlSegment.IsConstant)
        {
            var parameterName = this.OrmProvider.ParameterPrefix + this.ParameterPrefix + this.DbParameters.Count.ToString();
            if (this.IsMultiple) parameterName += $"m{this.CommandIndex}";

            if (sqlSegment.MemberMapper != null)
                this.AddDbParameter(sqlSegment.MemberMapper, parameterName, sqlSegment.Value);
            else this.DbParameters.Add(this.OrmProvider.CreateParameter(parameterName, sqlSegment.Value));

            sqlSegment.Value = parameterName;
            sqlSegment.IsParameter = true;
            sqlSegment.IsVariable = false;
            sqlSegment.IsConstant = false;
            return sqlSegment;
        }
        if (sqlSegment.IsConstant && sqlSegment.MemberMapper != null && sqlSegment.TargetType != sqlSegment.ExpectType)
        {
            sqlSegment.Value = this.OrmProvider.ToFieldValue(sqlSegment.MemberMapper, sqlSegment.Value);
            sqlSegment.ExpectType = sqlSegment.TargetType;
        }
        return sqlSegment;
    }
    public virtual SqlSegment Change(SqlSegment sqlSegment, object segmentValue)
    {
        sqlSegment.Value = segmentValue;
        return this.Change(sqlSegment);
    }
    public virtual SqlSegment Change(SqlSegment sqlSegment, object segmentValue, bool isExpression, bool isMethodCall)
    {
        sqlSegment.IsExpression = isExpression;
        sqlSegment.IsMethodCall = isMethodCall;
        if (sqlSegment.IsConstant && (isExpression || isMethodCall))
            sqlSegment.IsConstant = false;
        return this.Change(sqlSegment, segmentValue);
    }
    public virtual string GetQuotedValue(SqlSegment sqlSegment)
    {
        //默认只要是变量就设置为参数
        if (sqlSegment.IsVariable || (this.IsParameterized || sqlSegment.IsParameterized) && sqlSegment.IsConstant)
        {
            //只有常量和变量才有可能是数组
            if (sqlSegment.IsArray && sqlSegment.Value is List<SqlSegment> sqlSegments)
                sqlSegment.Value = sqlSegments.Select(f => f.Value).ToArray();

            var parameterName = this.OrmProvider.ParameterPrefix + this.ParameterPrefix + this.DbParameters.Count.ToString();
            if (this.IsMultiple) parameterName += $"_m{this.CommandIndex}";
            if (sqlSegment.MemberMapper != null)
                this.AddDbParameter(sqlSegment.MemberMapper, parameterName, sqlSegment.Value);
            else this.DbParameters.Add(this.OrmProvider.CreateParameter(parameterName, sqlSegment.Value));

            sqlSegment.Value = parameterName;
            sqlSegment.IsParameter = true;
            sqlSegment.IsVariable = false;
            sqlSegment.IsConstant = false;
            return parameterName;
        }
        else if (sqlSegment.IsConstant)
        {
            //对枚举常量，且数据库类型是字符串类型做了特殊处理，目前只有这一种情况
            if (sqlSegment.MemberMapper != null)
            {
                //只有常量和变量才有可能是数组
                if (sqlSegment.IsArray && sqlSegment.Value is List<SqlSegment> sqlSegments)
                    sqlSegment.Value = sqlSegments.Select(f => f.Value).ToArray();
                sqlSegment.Value = this.OrmProvider.ToFieldValue(sqlSegment.MemberMapper, sqlSegment.Value);
                return this.OrmProvider.GetQuotedValue(sqlSegment.Value);
            }
            return this.OrmProvider.GetQuotedValue(sqlSegment);
        }
        //带有参数或字段的表达式或函数调用、或是只有参数或字段
        return sqlSegment.ToString();
    }
    public virtual string GetQuotedValue(object elementValue, SqlSegment arraySegment)
    {
        if (elementValue is DBNull || elementValue == null)
            return "NULL";
        if (arraySegment.IsVariable || (this.IsParameterized || arraySegment.IsParameterized) && arraySegment.IsConstant)
        {
            var parameterName = this.OrmProvider.ParameterPrefix + this.ParameterPrefix + this.DbParameters.Count.ToString();
            if (this.IsMultiple) parameterName += $"m{this.CommandIndex}";

            if (arraySegment.MemberMapper != null)
                this.AddDbParameter(arraySegment.MemberMapper, parameterName, elementValue);
            else this.DbParameters.Add(this.OrmProvider.CreateParameter(parameterName, elementValue));
            return parameterName;
        }
        if (arraySegment.IsConstant && arraySegment.MemberMapper != null && arraySegment.TargetType != arraySegment.ExpectType)
            elementValue = this.OrmProvider.ToFieldValue(arraySegment.MemberMapper, elementValue);
        return this.OrmProvider.GetQuotedValue(elementValue);
    }
    public void AddDbParameter(MemberMap memberMapper, string parameterName, object fieldValue)
    {
        var AddDbParametersDelegate = RepositoryHelper.BuildAddDbParameters(this.DbKey, this.OrmProvider, memberMapper, fieldValue);
        AddDbParametersDelegate.Invoke(this.DbParameters, this.OrmProvider, parameterName, fieldValue);
    }
    /// <summary>
    /// 用于Where条件中，IS NOT NULL,!= 两种情况判断
    /// </summary>
    /// <param name="sqlSegment"></param>
    /// <param name="isExpectBooleanType"></param>
    /// <param name="ifTrueValue"></param>
    /// <param name="ifFalseValue"></param>
    /// <returns></returns>
    public SqlSegment VisitDeferredBoolConditional(SqlSegment sqlSegment, bool isExpectBooleanType, string ifTrueValue, string ifFalseValue)
    {
        //处理HasValue !逻辑取反操作，这种情况下是一元操作
        int notIndex = 0;
        SqlSegment deferredSegment = null;
        //复杂bool条件判断，有IS NOT NULL, <> != 两种情况，只能在
        while (sqlSegment.TryPop(out var deferredExpr))
        {
            switch (deferredExpr.OperationType)
            {
                case OperationType.Equal:
                    deferredSegment = deferredExpr.Value as SqlSegment;
                    break;
                case OperationType.Not:
                    notIndex++;
                    break;
            }
        }
        if (deferredSegment == null)
            deferredSegment = SqlSegment.True;

        string strOperator = null;
        if (notIndex % 2 > 0)
            strOperator = deferredSegment == SqlSegment.Null ? "IS NOT" : "<>";
        else strOperator = deferredSegment == SqlSegment.Null ? "IS" : "=";

        string strExpression = null;
        if (!sqlSegment.IsExpression && (this.IsWhere || this.IsSelect))
        {
            if (deferredSegment == SqlSegment.Null)
                strExpression = $"{sqlSegment} {strOperator} {deferredSegment.Value}";
            else strExpression = $"{sqlSegment}{strOperator}{this.OrmProvider.GetQuotedValue(typeof(bool), deferredSegment.Value)}";
        }
        else strExpression = $"{sqlSegment}";
        if (this.IsSelect || (this.IsWhere && !isExpectBooleanType))
            sqlSegment.Change($"CASE WHEN {strExpression} THEN {ifTrueValue} ELSE {ifFalseValue} END", false, true, false);
        else sqlSegment.Change($"{strExpression}", false, true, false);
        return sqlSegment;
    }
    public List<ReaderField> FlattenFieldsTo(Type targetType, Expression toTargetExpr = null)
    {
        List<ReaderField> targetFields = null;
        if (targetType == null)
            throw new ArgumentNullException(nameof(targetType));

        //通过表达式设置的字段
        if (toTargetExpr != null)
            targetFields = this.ConstructorFieldsTo(toTargetExpr as LambdaExpression);

        targetFields ??= new List<ReaderField>();
        var targetMembers = targetType.GetMembers(BindingFlags.Public | BindingFlags.Instance)
            .Where(f => f.MemberType == MemberTypes.Property | f.MemberType == MemberTypes.Field).ToList();

        for (int i = targetFields.Count - 1; i >= 0; i--)
        {
            var memberInfo = targetFields[i].TargetMember;
            if (!targetMembers.Exists(t => t.Name == memberInfo.Name))
                targetFields.RemoveAt(i);
        }
        foreach (var memberInfo in targetMembers)
        {
            var targetField = targetFields.Find(f => f.TargetMember.Name == memberInfo.Name);
            if (targetField != null)
            {
                targetField.TargetMember = memberInfo;
                continue;
            }
            if (this.FindReaderField(memberInfo, targetFields.Count, out var readerField))
                targetFields.Add(readerField);
        }
        return targetFields;
    }
    public List<ReaderField> ConstructorFieldsTo(LambdaExpression toTargetExpr)
    {
        List<ReaderField> result = null;
        if (toTargetExpr != null)
        {
            var sqlSegment = new SqlSegment { Expression = toTargetExpr.Body };
            switch (toTargetExpr.Body.NodeType)
            {
                case ExpressionType.MemberAccess:
                    sqlSegment = this.VisitMemberAccess(sqlSegment);
                    //成员访问，最多是Include子表访问或是临时表访问，如：x.Grouping分组，
                    //可能会有多个子表或是临时表
                    if (sqlSegment.MemberType == ReaderFieldType.Entity
                        || sqlSegment.MemberType == ReaderFieldType.AnonymousObject)
                        result = sqlSegment.Value as List<ReaderField>;
                    else
                    {
                        result = new List<ReaderField>
                        {
                            new ReaderField
                            {
                                TableSegment = sqlSegment.TableSegment,
                                FieldType = sqlSegment.MemberType,
                                FromMember = sqlSegment.FromMember,
                                TargetMember = sqlSegment.FromMember,
                                Body = sqlSegment.Value.ToString()
                            }
                        };
                    }
                    break;
                case ExpressionType.New:
                    sqlSegment = this.VisitNew(sqlSegment);
                    result = sqlSegment.Value as List<ReaderField>;
                    break;
                case ExpressionType.MemberInit:
                    sqlSegment = this.VisitMemberInit(sqlSegment);
                    result = sqlSegment.Value as List<ReaderField>;
                    break;
                case ExpressionType.Parameter:
                    sqlSegment = this.VisitParameter(sqlSegment);
                    result = sqlSegment.Value as List<ReaderField>;
                    result[0].IsTargetType = true;
                    break;
                default:
                    //单个字段或是方法调用
                    if (toTargetExpr.Body.NodeType == ExpressionType.Call)
                        sqlSegment.OriginalExpression = toTargetExpr;
                    sqlSegment = this.VisitAndDeferred(sqlSegment);
                    if (sqlSegment.Value is List<ReaderField> readerFields)
                        result = readerFields;
                    else
                    {
                        result = new List<ReaderField>
                        {
                            new ReaderField
                            {
                                TableSegment = sqlSegment.TableSegment,
                                FromMember = sqlSegment.FromMember,
                                Body = sqlSegment.Value.ToString()
                            }
                        };
                    }
                    break;
            }
        }
        return result;
    }
    /// <summary>
    /// 展开当前表tableSegment的所有字段，tableSegment是实体表(主表或Include表)
    /// repository.From(f => f.From<Order, OrderDetail, User>()
    ///         .Where((a, b, c) => a.Id == b.OrderId && a.BuyerId == c.Id)
    ///         .GroupBy((a, b, c) => new { OrderId = a.Id, a.BuyerId })
    ///         .Having((x, a, b, c) => c.Age > 20 && x.Sum(b.Amount) > 500)
    ///         .Select((x, a, b, c) => new { x.Grouping.OrderId, x.Grouping.BuyerId, TotalAmount = x.Sum(b.Amount) }))
    ///     .InnerJoin<User>((a, b) => a.BuyerId == b.Id)
    ///     .InnerJoin<Order>((a, b, c) => a.OrderId == c.Id)
    ///     .Select((a, b, c) => new { a.OrderId, a.BuyerId, Buyer = b, Order = c, a.TotalAmount })
    ///     .ToSql(out _);
    /// </summary>
    /// <param name="tableSegment"></param>
    /// <returns></returns>
    public List<ReaderField> FlattenTableFields(TableSegment tableSegment)
    {
        var targetFields = new List<ReaderField>();
        tableSegment.Mapper ??= this.MapProvider.GetEntityMap(tableSegment.EntityType);
        foreach (var memberMapper in tableSegment.Mapper.MemberMaps)
        {
            if (memberMapper.IsIgnore || memberMapper.IsNavigation
                || (memberMapper.MemberType.IsEntityType(out _) && memberMapper.TypeHandler == null))
                continue;
            targetFields.Add(new ReaderField
            {
                TableSegment = tableSegment,
                FromMember = memberMapper.Member,
                TargetMember = memberMapper.Member,
                FieldType = ReaderFieldType.Field,
                Body = this.GetFieldName(tableSegment, memberMapper.FieldName)
            });
        }
        return targetFields;
    }

    public void AddSelectElement(Expression elementExpr, MemberInfo memberInfo, List<ReaderField> readerFields)
    {
        string fieldName = null;
        var sqlSegment = new SqlSegment { Expression = elementExpr, ReaderIndex = readerFields.Count };
        switch (elementExpr.NodeType)
        {
            case ExpressionType.Parameter:
                if (this.IsFromQuery)
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
                        foreach (var readerField in this.GroupFields)
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
                            ReaderFields = this.GroupFields
                        });
                    }
                    else
                    {
                        if (this.IsFromQuery)
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
                    //参数或是本地成员变量访问
                    fieldName = this.GetQuotedValue(sqlSegment);
                    if (sqlSegment.IsExpression)//TODO:if (sqlSegment.IsExpression && !this.IsInsertTo)
                        fieldName = $"({fieldName})";
                    //if ((sqlSegment.IsParameter || sqlSegment.IsExpression || sqlSegment.IsMethodCall || sqlSegment.FromMember?.Name != memberInfo.Name) && !this.IsInsertTo)
                    if ((sqlSegment.IsParameter || sqlSegment.IsExpression || sqlSegment.IsMethodCall || sqlSegment.FromMember?.Name != memberInfo.Name))
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
                if (sqlSegment.IsExpression)//TODO:if (sqlSegment.IsExpression && !this.IsInsertTo)
                    fieldName = $"({fieldName})";
                //if ((sqlSegment.IsParameter || sqlSegment.IsExpression || sqlSegment.IsMethodCall || sqlSegment.FromMember?.Name != memberInfo.Name) && !this.IsInsertTo)
                if ((sqlSegment.IsParameter || sqlSegment.IsExpression || sqlSegment.IsMethodCall || sqlSegment.FromMember?.Name != memberInfo.Name))
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

    /// <summary>
    /// 根据表、字段获取Field表达式,SELECT和WHERE语句都可以使用
    /// 有表且有别名，将生成a.Field，常量访问没有表则就是字段访问Field
    /// </summary>
    /// <param name="tableSegment">表</param> 
    /// <param name="fieldName">字段名称或是表达式</param>
    /// <returns></returns>
    public string GetFieldName(TableSegment tableSegment, string fieldName)
    {
        fieldName = this.OrmProvider.GetFieldName(fieldName);
        if (tableSegment != null && !string.IsNullOrEmpty(tableSegment.AliasName) && (this.IsNeedAlias || tableSegment.IsNeedAlais))
            fieldName = tableSegment.AliasName + "." + fieldName;
        return fieldName;
    }
    public bool IsDateTimeOperator(SqlSegment sqlSegment, out SqlSegment result)
    {
        var binaryExpr = sqlSegment.Expression as BinaryExpression;
        if (binaryExpr.Left.Type == typeof(DateTime) && binaryExpr.Right.Type == typeof(TimeSpan) && binaryExpr.NodeType == ExpressionType.Add)
        {
            var methodInfo = typeof(DateTime).GetMethod(nameof(DateTime.Add), new Type[] { binaryExpr.Right.Type });
            var operatorExpr = Expression.Call(binaryExpr.Left, methodInfo, binaryExpr.Right);
            result = this.VisitMethodCall(sqlSegment.Next(operatorExpr));
            return true;
        }
        if (binaryExpr.Left.Type == typeof(DateTime) && (binaryExpr.Right.Type == typeof(DateTime) || binaryExpr.Right.Type == typeof(TimeSpan)) && binaryExpr.NodeType == ExpressionType.Subtract)
        {
            var methodInfo = typeof(DateTime).GetMethod(nameof(DateTime.Subtract), new Type[] { binaryExpr.Right.Type });
            var operatorExpr = Expression.Call(binaryExpr.Left, methodInfo, binaryExpr.Right);
            result = this.VisitMethodCall(sqlSegment.Next(operatorExpr));
            return true;
        }
        result = null;
        return false;
    }
    public bool IsTimeSpanOperator(SqlSegment sqlSegment, out SqlSegment result)
    {
        var binaryExpr = sqlSegment.Expression as BinaryExpression;
        if (binaryExpr.Left.Type == typeof(TimeSpan) && binaryExpr.Right.Type == typeof(TimeSpan) && binaryExpr.NodeType == ExpressionType.Add)
        {
            var methodInfo = typeof(TimeSpan).GetMethod(nameof(TimeSpan.Add), new Type[] { binaryExpr.Right.Type });
            var operatorExpr = Expression.Call(binaryExpr.Left, methodInfo, binaryExpr.Right);
            result = this.VisitMethodCall(sqlSegment.Next(operatorExpr));
            return true;
        }
        if (binaryExpr.Left.Type == typeof(TimeSpan) && binaryExpr.Right.Type == typeof(TimeSpan) && binaryExpr.NodeType == ExpressionType.Subtract)
        {
            var methodInfo = typeof(TimeSpan).GetMethod(nameof(TimeSpan.Subtract), new Type[] { binaryExpr.Right.Type });
            var operatorExpr = Expression.Call(binaryExpr.Left, methodInfo, binaryExpr.Right);
            result = this.VisitMethodCall(sqlSegment.Next(operatorExpr));
            return true;
        }
        if (binaryExpr.Left.Type == typeof(TimeSpan) && binaryExpr.NodeType == ExpressionType.Multiply)
        {
            var rightExpr = binaryExpr.Right;
            if (binaryExpr.Right.Type != typeof(double))
                rightExpr = Expression.Convert(rightExpr, typeof(double));
            var methodInfo = typeof(TimeSpan).GetMethod(nameof(TimeSpan.Multiply), new Type[] { typeof(double) });
            var operatorExpr = Expression.Call(binaryExpr.Left, methodInfo, rightExpr);
            result = this.VisitMethodCall(sqlSegment.Next(operatorExpr));
            return true;
        }
        if (binaryExpr.Left.Type == typeof(TimeSpan) && binaryExpr.NodeType == ExpressionType.Divide)
        {
            Type divideType = null;
            if (binaryExpr.Right.Type == typeof(TimeSpan))
                divideType = typeof(TimeSpan);
            else divideType = typeof(double);
            var methodInfo = typeof(TimeSpan).GetMethod(nameof(TimeSpan.Divide), new Type[] { divideType });
            var rightExpr = binaryExpr.Right;
            if (divideType == typeof(double) && binaryExpr.Right.Type != typeof(double))
                rightExpr = Expression.Convert(rightExpr, typeof(double));
            var operatorExpr = Expression.Call(binaryExpr.Left, methodInfo, rightExpr);
            result = this.VisitMethodCall(sqlSegment.Next(operatorExpr));
            return true;
        }
        result = null;
        return false;
    }
    public void Swap<T>(ref T left, ref T right)
    {
        var temp = right;
        right = left;
        left = temp;
    }
    public bool ChangeSameType(SqlSegment leftSegment, SqlSegment rightSegment)
    {
        //表达式左侧有枚举类字段访问，直接字段访问或是表达式计算(加、减、乘、除、取模、按位与、按位或...)
        //如：f.SourceType = UserSourceType.WebSite 或是f.SourceType & UserSourceType.WebSite = UserSourceType.WebSite
        //在表达式解析过程中，计算时使用UnderlyingType类型，条件等于判断使用枚举类型
        if (leftSegment.HasField && (!leftSegment.IsExpression && !leftSegment.IsMethodCall || leftSegment.IsFieldType))
        {
            rightSegment.MemberMapper = leftSegment.MemberMapper;
            return true;
        }

        //var leftType = leftSegment.ExpectType ?? leftSegment.Expression.Type;
        //var rightType = rightSegment.ExpectType ?? rightSegment.Expression.Type;
        //if (leftType != rightType && (leftType.IsEnum || rightType.IsEnum)
        //    && (rightSegment.IsConstant || rightSegment.IsVariable))
        //{
        //    rightSegment.ExpectType = leftSegment.ExpectType;
        //    rightSegment.Value = Enum.ToObject(leftSegment.ExpectType, rightSegment.Value);
        //    if (leftSegment.HasField)
        //    {
        //        rightSegment.MemberMapper = leftSegment.MemberMapper;
        //        if (leftSegment.MemberMapper.DbDefaultType == typeof(string))
        //        {
        //            leftSegment.TargetType = typeof(string);
        //            rightSegment.TargetType = typeof(string);
        //        }
        //    }
        //    return true;
        //}
        return false;
    }
    public LambdaExpression EnsureLambda(Expression expr)
    {
        if (expr.NodeType == ExpressionType.Lambda)
            return expr as LambdaExpression;
        var currentExpr = expr;
        while (true)
        {
            if (currentExpr.NodeType == ExpressionType.Lambda)
                break;

            if (currentExpr is UnaryExpression unaryExpr)
                currentExpr = unaryExpr.Operand;
        }
        return currentExpr as LambdaExpression;
    }
    public bool IsGroupingAggregateMember(MemberExpression memberExpr)
    {
        if (memberExpr == null) return false;
        return typeof(IAggregateSelect).IsAssignableFrom(memberExpr.Member.DeclaringType) && memberExpr.Member.Name == "Grouping";
    }
    private List<ConditionExpression> VisitLogicBinaryExpr(Expression conditionExpr)
    {
        Func<Expression, bool> isConditionExpr = f => f.NodeType == ExpressionType.AndAlso || f.NodeType == ExpressionType.OrElse;

        int deep = 0;
        string lastOperationType = string.Empty;
        var operators = new Stack<ConditionOperator>();
        var leftExprs = new Stack<Expression>();
        var completedStackExprs = new Stack<ConditionExpression>();

        var nextExpr = conditionExpr as BinaryExpression;
        while (nextExpr != null)
        {
            var operationType = nextExpr.NodeType == ExpressionType.AndAlso ? " AND " : " OR ";
            if (!string.IsNullOrEmpty(lastOperationType) && lastOperationType != operationType)
                deep++;

            if (isConditionExpr(nextExpr.Right))
            {
                leftExprs.Push(nextExpr.Left);
                nextExpr = nextExpr.Right as BinaryExpression;
                lastOperationType = operationType;
                if (deep > 0)
                {
                    operators.Push(new ConditionOperator
                    {
                        OperatorType = operationType,
                        Deep = deep
                    });
                }
                continue;
            }
            //先压进右括号
            var lastDeep = 0;
            if (operators.TryPop(out var conditionOperator))
                lastDeep = conditionOperator.Deep;
            for (int i = deep; i > lastDeep; i--)
            {
                completedStackExprs.Push(new ConditionExpression
                {
                    ExpressionType = ConditionType.OperatorType,
                    Body = ")"
                });
            }
            //再压进右侧表达式
            completedStackExprs.Push(new ConditionExpression
            {
                ExpressionType = ConditionType.Expression,
                Body = nextExpr.Right
            });
            //再压进当前操作符
            completedStackExprs.Push(new ConditionExpression
            {
                ExpressionType = ConditionType.OperatorType,
                Body = operationType
            });
            if (isConditionExpr(nextExpr.Left))
            {
                nextExpr = nextExpr.Left as BinaryExpression;
                lastOperationType = operationType;
                if (deep > 0)
                {
                    operators.Push(new ConditionOperator
                    {
                        OperatorType = operationType,
                        Deep = deep
                    });
                }
                continue;
            }
            //再压进左侧表达式
            completedStackExprs.Push(new ConditionExpression
            {
                ExpressionType = ConditionType.Expression,
                Body = nextExpr.Left
            });
            if (operators.TryPop(out conditionOperator))
            {
                lastDeep = conditionOperator.Deep;
                lastOperationType = conditionOperator.OperatorType;
            }
            else lastDeep = 0;
            //再压进左括号
            for (int i = deep; i > lastDeep; i--)
            {
                completedStackExprs.Push(new ConditionExpression
                {
                    ExpressionType = ConditionType.OperatorType,
                    Body = "("
                });
            }
            //再压进操作符
            if (leftExprs.Count > 0)
            {
                for (int i = deep; i > lastDeep; i--)
                {
                    completedStackExprs.Push(new ConditionExpression
                    {
                        ExpressionType = ConditionType.OperatorType,
                        Body = lastOperationType
                    });
                }
            }
            if (leftExprs.TryPop(out var deferredExpr))
            {
                if (operators.TryPop(out conditionOperator))
                    deep = conditionOperator.Deep;
                else deep = 0;

                if (isConditionExpr(deferredExpr))
                {
                    nextExpr = deferredExpr as BinaryExpression;
                    continue;
                }
                completedStackExprs.Push(new ConditionExpression
                {
                    ExpressionType = ConditionType.Expression,
                    Body = deferredExpr
                });
                break;
            }
            else break;
        }
        var completedExprs = new List<ConditionExpression>();
        while (completedStackExprs.TryPop(out var completedExpr))
        {
            completedExprs.Add(completedExpr);
        }
        return completedExprs;
    }
    private bool FindReaderField(MemberInfo memberInfo, int index, out ReaderField readerField)
    {
        foreach (var tableSegment in this.Tables)
        {
            if (this.FindReaderField(tableSegment, memberInfo, index, out readerField))
                return true;
        }
        readerField = null;
        return false;
    }
    private bool FindReaderField(TableSegment tableSegment, MemberInfo memberInfo, int index, out ReaderField readerField)
    {
        switch (tableSegment.TableType)
        {
            case TableType.FromQuery:
                if (tableSegment.ReaderFields == null || tableSegment.ReaderFields.Count == 0)
                {
                    readerField = null;
                    return false;
                }
                readerField = tableSegment.ReaderFields.Find(f => f.FromMember.Name == memberInfo.Name);
                if (readerField != null)
                {
                    readerField.Index = index;
                    readerField.TargetMember = memberInfo;
                    tableSegment.IsUsed = true;
                }
                return readerField != null;
            default:
                tableSegment.Mapper ??= this.MapProvider.GetEntityMap(tableSegment.EntityType);
                if (!tableSegment.Mapper.TryGetMemberMap(memberInfo.Name, out var memberMapper))
                {
                    readerField = null;
                    return false;
                }
                readerField = new ReaderField
                {
                    Index = index,
                    FieldType = ReaderFieldType.Field,
                    FromMember = memberMapper.Member,
                    TargetMember = memberInfo,
                    TableSegment = tableSegment,
                    Body = this.GetFieldName(tableSegment, memberMapper.FieldName)
                };
                tableSegment.IsUsed = true;
                return true;
        }
    }
    private SqlSegment CreateConditionSegment(Expression conditionExpr)
    {
        var sqlSegment = new SqlSegment { Expression = conditionExpr };
        if (conditionExpr.NodeType == ExpressionType.MemberAccess && conditionExpr.Type == typeof(bool))
        {
            sqlSegment.DeferredExprs ??= new();
            sqlSegment.DeferredExprs.Push(new DeferredExpr { OperationType = OperationType.Equal, Value = SqlSegment.True });
        }
        return sqlSegment;
    }
    private void AddIncludeTables(ReaderField lastReaderField, List<ReaderField> readerFields)
    {
        var includedSegments = this.Tables.FindAll(f => !f.IsMaster && f.FromTable == lastReaderField.TableSegment);
        if (includedSegments != null && includedSegments.Count > 0)
        {
            lastReaderField.HasNextInclude = true;
            int index = lastReaderField.Index;
            foreach (var includedSegment in includedSegments)
            {
                index++;
                var readerField = new ReaderField
                {
                    Index = index,
                    FieldType = ReaderFieldType.Entity,
                    TableSegment = includedSegment,
                    FromMember = includedSegment.FromMember.Member,
                    TargetMember = includedSegment.FromMember.Member,
                    ParentIndex = lastReaderField.Index,
                    ReaderFields = this.FlattenTableFields(includedSegment)
                };
                //主表使用，include子表也将使用
                includedSegment.IsUsed = lastReaderField.TableSegment.IsUsed;
                readerFields.Add(readerField);
                if (this.Tables.Exists(f => !f.IsMaster && f.FromTable == includedSegment))
                    this.AddIncludeTables(readerField, readerFields);
            }
        }
    }

    class ConditionOperator
    {
        public string OperatorType { get; set; }
        public int Deep { get; set; }
    }
    class ConditionExpression
    {
        public object Body { get; set; }
        public ConditionType ExpressionType { get; set; }
    }
    enum ConditionType
    {
        OperatorType,
        Expression
    }
}
