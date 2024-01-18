using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Trolley;

public class SqlVisitor : ISqlVisitor
{
    private static ConcurrentDictionary<int, Func<object, object>> memberGetterCache = new();
    private static string[] calcOps = new string[] { ">", ">=", "<", "<=", "+", "-", "*", "/", "%", "&", "|", "^", "<<", ">>" };

    public string DbKey { get; set; }
    public IDataParameterCollection DbParameters { get; set; }
    public IOrmProvider OrmProvider { get; set; }
    public IEntityMapProvider MapProvider { get; set; }
    public bool IsParameterized { get; set; }
    public bool IsMultiple { get; set; }
    public int CommandIndex { get; set; }
    public string ParameterPrefix { get; set; } = "p";

    /// <summary>
    /// 所有表都是扁平化的，主表、1:1关系Include子表，也在这里
    /// </summary>
    public List<TableSegment> Tables { get; set; } = new();
    public Dictionary<string, TableSegment> TableAlias { get; set; } = new();
    public bool IsSelect { get; set; }
    public bool IsWhere { get; set; }
    public bool IsNeedTableAlias { get; set; }

    public List<ReaderField> ReaderFields { get; set; }
    public bool IsFromQuery { get; set; }
    public string WhereSql { get; set; }
    public OperationType LastWhereNodeType { get; set; } = OperationType.None;
    public char TableAsStart { get; set; }
    public List<ReaderField> GroupFields { get; set; }
    public List<TableSegment> IncludeSegments { get; set; }
    public List<ICteQuery> CteQueries { get; set; }

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
                return sqlSegment.Change($"~{this.Visit(sqlSegment)}", false, false, true);
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
                    return sqlSegment.Change(Expression.Lambda(binaryExpr).Compile().DynamicInvoke(), true);

                if ((leftSegment.IsConstant || leftSegment.IsVariable)
                    && (rightSegment.IsConstant || rightSegment.IsVariable))
                    return sqlSegment.Change(Expression.Lambda(binaryExpr).Compile().DynamicInvoke(), false, true);

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

                //??操作类型没有变更，可以当作Field使用
                if (binaryExpr.NodeType == ExpressionType.Coalesce)
                    leftSegment.IsFieldType = true;

                //如果是IsParameter,HasField,IsExpression,IsMethodCall直接返回,是SQL
                //如果是变量或是要求变成参数的常量，变成@p返回
                //如果是常量获取当前类型值，再转成QuotedValue值
                //就是枚举类型有问题，单独处理
                //... WHERE (int)(a.Price * a.Quartity)>500
                //SELECT TotalAmount = (int)(amount + (a.Price + increasedPrice) * (a.Quartity + increasedCount)) ...FROM ...
                //SELECT OrderNo = $"OrderNo-{f.CreatedAt.ToString("yyyyMMdd")}-{f.Id}"...FROM ...
                var leftType = leftSegment.GetExpectType(binaryExpr.Left);
                var rightType = rightSegment.GetExpectType(binaryExpr.Right);
                if (leftType.IsEnum || rightType.IsEnum || leftType != rightType)
                {
                    if (calcOps.Contains(operators))
                        throw new NotSupportedException($"枚举类成员{leftSegment.MemberMapper.MemberName}对应的数据库类型为非数字类型，不能进行{operators}操作，可以使用=、<>、IN、EXISTS等操作来代替，表达式：{binaryExpr}");

                    //在调用GetQuotedValue方法前，确保左右两侧的类型一致，并都根据MemberMapper的映射类型表生成SQL语句                   
                    this.ChangeSameType(leftSegment, rightSegment);
                }
                string strLeft = this.GetQuotedValue(leftSegment);
                string strRight = this.GetQuotedValue(rightSegment);

                if (binaryExpr.NodeType == ExpressionType.Coalesce)
                    return sqlSegment.Merge(leftSegment, rightSegment, $"{operators}({strLeft},{strRight})", false, false, true);

                if (leftSegment.IsExpression)
                    strLeft = $"({strLeft})";
                if (rightSegment.IsExpression)
                    strRight = $"({strRight})";

                return sqlSegment.Merge(leftSegment, rightSegment, $"{strLeft}{operators}{strRight}", false, false, true);
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
                //Where(f => f.Amount > 5)
                //Select(f => new { f.OrderId, f.Disputes ...})
                var tableSegment = this.TableAlias[parameterName];
                sqlSegment.HasField = true;
                sqlSegment.TableSegment = tableSegment;
                MemberMap memberMapper = null;
                string fieldName = null;

                if (tableSegment.TableType == TableType.FromQuery || tableSegment.TableType == TableType.CteSelfRef)
                {
                    //访问子查询表的成员，子查询表没有Mapper，也不会有实体类型成员
                    //Json的实体类型字段
                    ReaderField readerField = null;
                    //子查询中，Select了Grouping分组对象，子查询中，只有一个分组对象才是实体类型，目前子查询，只支持一层
                    //取AS后的字段名，与原字段名不一定一样,AS后的字段名与memberExpr.Member.Name一致
                    if (memberExpr.Expression.NodeType != ExpressionType.Parameter)
                    {
                        fieldName = this.OrmProvider.GetFieldName(memberExpr.Member.Name);
                        if (this.IsNeedTableAlias) fieldName = tableSegment.AliasName + "." + fieldName;
                        sqlSegment.Value = fieldName;

                        var parentMemberExpr = memberExpr.Expression as MemberExpression;
                        var parenetReaderField = tableSegment.ReaderFields.Count == 1 ? tableSegment.ReaderFields.First()
                            : tableSegment.ReaderFields.Find(f => f.TargetMember.Name == parentMemberExpr.Member.Name);
                        var fromReaderFields = parenetReaderField.ReaderFields;
                        readerField = fromReaderFields.Count == 1 ? fromReaderFields.First()
                            : fromReaderFields.Find(f => f.TargetMember.Name == memberExpr.Member.Name);
                        sqlSegment.FromMember = readerField.TargetMember;
                    }
                    else
                    {
                        readerField = tableSegment.ReaderFields.Count == 1 ? tableSegment.ReaderFields.First()
                          : tableSegment.ReaderFields.Find(f => f.TargetMember.Name == memberExpr.Member.Name);
                        fieldName = readerField.Body;
                        sqlSegment.FromMember = readerField.TargetMember;
                        sqlSegment.Value = readerField.Body;
                    }
                    memberMapper = readerField.MemberMapper;
                    sqlSegment.MemberMapper = memberMapper;
                }
                else
                {
                    memberMapper = tableSegment.Mapper.GetMemberMap(memberExpr.Member.Name);
                    if (memberMapper.IsIgnore)
                        throw new Exception($"类{tableSegment.EntityType.FullName}的成员{memberMapper.MemberName}是忽略成员无法访问");
                    if (memberMapper.MemberType.IsEntityType(out _) && !memberMapper.IsNavigation && memberMapper.TypeHandler == null)
                        throw new Exception($"类{tableSegment.EntityType.FullName}的成员{memberExpr.Member.Name}不是值类型，未配置为导航属性也没有配置TypeHandler");
                    sqlSegment.FromMember = memberMapper.Member;
                    sqlSegment.MemberMapper = memberMapper;
                    //查询时，IsNeedAlias始终为true，新增、更新、删除时，引用联表操作时，才会为true
                    fieldName = this.OrmProvider.GetFieldName(memberMapper.FieldName);
                    if (this.IsNeedTableAlias) fieldName = tableSegment.AliasName + "." + fieldName;
                    sqlSegment.Value = fieldName;
                }
                //.NET枚举类型总是解析成对应的UnderlyingType数值类型，如：a.Gender ?? Gender.Male == Gender.Male
                //如果枚举类型对应的数据库类型是字符串就会有问题，需要把数字变为枚举，再把枚举的名字字符串完成后续操作。
                if (memberMapper != null && memberMapper.MemberType.IsEnumType(out var expectType, out _) && memberMapper.DbDefaultType == typeof(string))
                {
                    sqlSegment.ExpectType = expectType;
                    sqlSegment.TargetType = memberMapper.DbDefaultType;
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

        if (this.IsSelect)
        {
            //延迟方法调用，两种场景：
            //1.主动延迟方法调用：如，把返回的枚举列转成描述，参数就是枚举列，返回值是对应的描述
            //2.Select子句中Include导航成员访问，主表数据已经查询了，此处成员访问只是多一个引用赋值动作，做成了延迟委托调用
            string fields = null;
            List<ReaderField> readerFields = null;
            Expression deferredDelegate = null;
            if (methodCallExpr.Arguments != null && methodCallExpr.Arguments.Count > 0)
            {
                readerFields = new List<ReaderField>();
                var builder = new StringBuilder();
                var visitor = new ReplaceParameterVisitor();
                deferredDelegate = visitor.Visit(methodCallExpr);

                foreach (var argsExpr in visitor.OrgMembers)
                {
                    var argumentSegment = this.VisitAndDeferred(new SqlSegment { Expression = argsExpr });
                    if (argumentSegment.HasField)
                    {
                        sqlSegment.HasField = true;
                        var fieldName = argumentSegment.Value.ToString();
                        readerFields.Add(new ReaderField
                        {
                            FieldType = ReaderFieldType.Field,
                            FromMember = argumentSegment.FromMember,
                            MemberMapper = argumentSegment.MemberMapper,
                            TargetMember = argumentSegment.FromMember,
                            Body = fieldName
                        });
                        if (builder.Length > 0)
                            builder.Append(',');
                        builder.Append(fieldName);
                    }
                }
                if (readerFields.Count > 0)
                    fields = builder.ToString();
            }
            else deferredDelegate = Expression.Lambda(methodCallExpr);

            if (sqlSegment.IsDeferredFields || !string.IsNullOrEmpty(fields))
            {
                if (readerFields == null)
                    fields = "NULL";
                return sqlSegment.Change(new ReaderField
                {
                    FieldType = ReaderFieldType.DeferredFields,
                    Body = fields,
                    DeferredDelegate = deferredDelegate,
                    ReaderFields = readerFields
                });
            }
        }
        return this.Evaluate(sqlSegment);
    }
    public virtual SqlSegment VisitParameter(SqlSegment sqlSegment)
    {
        var parameterExpr = sqlSegment.Expression as ParameterExpression;
        //两种场景：.Select((x, y) => new { Order = x, x.Seller, x.Buyer, ... }) 和 .Select((x, y) => x)
        //参数访问通常都是SELECT语句的实体访问
        if (!this.IsSelect) throw new NotSupportedException($"不支持的参数表达式访问，只支持Select语句中，{parameterExpr}");
        if (this.IsFromQuery)
            throw new NotSupportedException($"FROM子查询中不支持参数表达式访问，只支持基础字段访问访问,{parameterExpr}");

        var fromSegment = this.TableAlias[parameterExpr.Name];
        var readerField = new ReaderField
        {
            FieldType = ReaderFieldType.Entity,
            TableSegment = fromSegment,
            ReaderFields = this.FlattenTableFields(fromSegment),
            Path = parameterExpr.Name
        };
        //include表的ReaderField字段，紧跟在主表ReaderField后面
        var readerFields = new List<ReaderField>() { readerField };
        this.AddIncludeTableReaderFields(readerField, readerFields);
        return sqlSegment.Change(readerFields);
    }
    protected void AddIncludeTableReaderFields(ReaderField parent, List<ReaderField> readerFields)
    {
        var includedSegments = this.Tables.FindAll(f => f.TableType == TableType.Include && f.FromTable == parent.TableSegment);
        if (includedSegments.Count > 0)
        {
            parent.HasNextInclude = true;
            foreach (var includedSegment in includedSegments)
            {
                var childReaderFields = this.FlattenTableFields(includedSegment);
                var readerField = new ReaderField
                {
                    FieldType = ReaderFieldType.Entity,
                    TableSegment = includedSegment,
                    MemberMapper = includedSegment.FromMember,
                    FromMember = includedSegment.FromMember.Member,
                    TargetMember = includedSegment.FromMember.Member,
                    Parent = parent,
                    ReaderFields = this.FlattenTableFields(includedSegment),
                    //更换path，方便后续Include成员赋值时，能够找到parent对象
                    Path = includedSegment.Path.Replace(parent.TableSegment.Path, parent.Path)
                };
                readerFields.Add(readerField);
                if (this.Tables.Exists(f => f.TableType == TableType.Include && f.FromTable == includedSegment))
                    this.AddIncludeTableReaderFields(readerField, readerFields);
            }
        }
        if (this.IncludeSegments != null)
        {
            var manyIncludedSegments = this.IncludeSegments.FindAll(f => f.FromTable == parent.TableSegment);
            if (manyIncludedSegments.Count > 0)
            {
                //目前，1:N关系只支持1级
                foreach (var includedSegment in manyIncludedSegments)
                {
                    //更换path，方便后续Include成员赋值时，能够找到parent对象
                    includedSegment.Path = includedSegment.Path.Replace(parent.TableSegment.Path, parent.Path);
                }
            }
        }
    }
    protected string BuildSelectSql(List<ReaderField> readerFields)
    {
        var builder = new StringBuilder();
        foreach (var readerField in readerFields)
        {
            if (builder.Length > 0)
                builder.Append(',');
            if (readerField.FieldType == ReaderFieldType.Entity)
                builder.Append(this.BuildSelectSql(readerField.ReaderFields));
            else builder.Append(readerField.Body);
        }
        return builder.ToString();
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
        var result = new List<object>();
        foreach (var elementExpr in newArrayExpr.Expressions)
        {
            var elementSegment = new SqlSegment { Expression = elementExpr };
            elementSegment = this.VisitAndDeferred(elementSegment);
            if (elementSegment.HasField)
                throw new NotSupportedException("不支持的表达式访问，NewArrayExpression表达式只支持常量和变量，不支持参数访问");
            sqlSegment.Merge(elementSegment);
            result.Add(elementSegment.Value);
        }
        return sqlSegment.Change(result);
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
        //确保返回的值类型与数据库类型一致
        this.ChangeSameType(ifTrueSegment, sqlSegment);
        sqlSegment.IsFieldType = true;
        return this.VisitDeferredBoolConditional(sqlSegment, conditionalExpr.IfTrue.Type == typeof(bool), leftArgument, rightArgument);
    }
    public virtual SqlSegment VisitListInit(SqlSegment sqlSegment)
    {
        sqlSegment.IsArray = true;
        var listExpr = sqlSegment.Expression as ListInitExpression;
        var result = new List<object>();
        foreach (var elementInit in listExpr.Initializers)
        {
            if (elementInit.Arguments.Count == 0)
                continue;
            var elementSegment = new SqlSegment { Expression = elementInit.Arguments[0] };
            elementSegment = this.VisitAndDeferred(elementSegment);
            if (elementSegment.HasField)
                throw new NotSupportedException("不支持的表达式访问，ListInitExpression表达式只支持常量和变量，不支持参数访问");
            sqlSegment.Merge(elementSegment);
            result.Add(elementSegment.Value);
        }
        return sqlSegment.Change(result);
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
        var objValue = sqlSegment.Expression.Evaluate();
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
    public virtual object Evaluate(Expression expr) => expr.Evaluate();
    public virtual SqlSegment VisitSqlMethodCall(SqlSegment sqlSegment)
    {
        var methodCallExpr = sqlSegment.Expression as MethodCallExpression;
        LambdaExpression lambdaExpr = null;
        switch (methodCallExpr.Method.Name)
        {
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
            //case "ToParameter":
            //    sqlSegment.IsParameterized = true;
            //    sqlSegment.Value = this.Visit(sqlSegment.Next(methodCallExpr.Arguments[0]));
            //    sqlSegment.IsParameterized = false;
            //    break;
            case "In":
                var elementType = methodCallExpr.Method.GetGenericArguments()[0];
                var type = methodCallExpr.Arguments[1].Type;
                var fieldSegment = this.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                if (type.IsArray || typeof(IEnumerable<>).MakeGenericType(elementType).IsAssignableFrom(type))
                {
                    var rightSegment = this.VisitAndDeferred(new SqlSegment { Expression = methodCallExpr.Arguments[1] });
                    if (rightSegment == SqlSegment.Null)
                        return sqlSegment.Change("1=0", false, false, true);
                    var enumerable = rightSegment.Value as IEnumerable;

                    var builder = new StringBuilder();
                    foreach (var item in enumerable)
                    {
                        if (builder.Length > 0) builder.Append(',');
                        builder.Append(this.OrmProvider.GetQuotedValue(item));
                    }
                    sqlSegment.Change(builder.ToString());
                }
                else
                {
                    var visitor = this.CreateQueryVisitor();
                    var fromQuery = new FromQuery(this.DbKey, this.OrmProvider, this.MapProvider, visitor, this.IsParameterized);
                    var queryObj = methodCallExpr.Method.Invoke(null, new object[] { fromQuery });
                    if (queryObj is ICteQuery cteQuery)
                    {

                    }

                    lambdaExpr = methodCallExpr.Arguments[1] as LambdaExpression;
                    var sql = this.VisitFromQuery(lambdaExpr);
                    sqlSegment.Change(sql);
                }
                if (sqlSegment.HasDeferrdNot())
                    sqlSegment.Change($"{fieldSegment} NOT IN ({sqlSegment})", false, false, true);
                else sqlSegment.Change($"{fieldSegment} IN ({sqlSegment})", false, false, true);
                break;
            case "Exists":
                lambdaExpr = this.EnsureLambda(methodCallExpr.Arguments[0]);
                string existsSql = null;
                var subTableTypes = methodCallExpr.Method.GetGenericArguments();
                if (subTableTypes != null && subTableTypes.Length > 0)
                {
                    //保存现场，临时添加这几个新表及别名，解析之后再删除
                    var removeIndex = this.Tables.Count;
                    Dictionary<string, TableSegment> tableAlias = null;
                    if (this.TableAlias.Count > 0)
                    {
                        tableAlias = new Dictionary<string, TableSegment>();
                        foreach (var item in this.TableAlias)
                            tableAlias.Add(item.Key, item.Value);
                    }
                    this.TableAlias.Clear();

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
                        this.Tables.Add(tableSegment);
                        this.TableAlias.Add(aliasName, tableSegment);
                        if (index > 0) builder.Append(',');
                        builder.Append(this.OrmProvider.GetTableName(subTableMapper.TableName));
                        builder.Append($" {tableSegment.AliasName}");
                        index++;
                    }
                    builder.Append(" WHERE ");
                    builder.Append(this.VisitConditionExpr(lambdaExpr.Body));

                    //恢复现场
                    while (this.Tables.Count > removeIndex)
                        this.Tables.RemoveAt(removeIndex);
                    if (tableAlias != null)
                        this.TableAlias = tableAlias;
                    existsSql = builder.ToString();
                }
                else existsSql = this.VisitFromQuery(lambdaExpr);

                if (sqlSegment.HasDeferrdNot())
                    sqlSegment.Change($"NOT EXISTS({existsSql})", false, false, false, true);
                else sqlSegment.Change($"EXISTS({existsSql})", false, false, false, true);
                break;
            case "Count":
            case "LongCount":
                if (methodCallExpr.Arguments != null && methodCallExpr.Arguments.Count == 1)
                {
                    if (methodCallExpr.Arguments[0].NodeType != ExpressionType.MemberAccess)
                        throw new NotSupportedException("不支持的表达式，只支持MemberAccess类型表达式");

                    sqlSegment = this.VisitMemberAccess(sqlSegment.Next(methodCallExpr.Arguments[0]));
                    sqlSegment.Change($"COUNT({sqlSegment})", false, false, false, true);
                }
                else sqlSegment.Change("COUNT(1)", false, false, false, true);
                break;
            case "CountDistinct":
            case "LongCountDistinct":
                if (methodCallExpr.Arguments != null && methodCallExpr.Arguments.Count == 1)
                {
                    if (methodCallExpr.Arguments[0].NodeType != ExpressionType.MemberAccess)
                        throw new NotSupportedException("不支持的表达式，只支持MemberAccess类型表达式");

                    sqlSegment = this.VisitMemberAccess(sqlSegment.Next(methodCallExpr.Arguments[0]));
                    sqlSegment.Change($"COUNT(DISTINCT {sqlSegment})", false, false, false, true);
                }
                break;
            case "Sum":
                if (methodCallExpr.Arguments != null && methodCallExpr.Arguments.Count == 1)
                {
                    if (methodCallExpr.Arguments[0].NodeType != ExpressionType.MemberAccess)
                        throw new NotSupportedException("不支持的表达式，只支持MemberAccess类型表达式");
                    sqlSegment = this.VisitMemberAccess(sqlSegment.Next(methodCallExpr.Arguments[0]));
                    sqlSegment.Change($"SUM({sqlSegment})", false, false, false, true);
                }
                break;
            case "Avg":
                if (methodCallExpr.Arguments != null && methodCallExpr.Arguments.Count == 1)
                {
                    if (methodCallExpr.Arguments[0].NodeType != ExpressionType.MemberAccess)
                        throw new NotSupportedException("不支持的表达式，只支持MemberAccess类型表达式");
                    sqlSegment = this.VisitMemberAccess(sqlSegment.Next(methodCallExpr.Arguments[0]));
                    sqlSegment.Change($"AVG({sqlSegment})", false, false, false, true);
                }
                break;
            case "Max":
                if (methodCallExpr.Arguments != null && methodCallExpr.Arguments.Count == 1)
                {
                    if (methodCallExpr.Arguments[0].NodeType != ExpressionType.MemberAccess)
                        throw new NotSupportedException("不支持的表达式，只支持MemberAccess类型表达式");
                    sqlSegment = this.VisitMemberAccess(sqlSegment.Next(methodCallExpr.Arguments[0]));
                    sqlSegment.Change($"MAX({sqlSegment})", false, false, false, true);
                }
                break;
            case "Min":
                if (methodCallExpr.Arguments != null && methodCallExpr.Arguments.Count == 1)
                {
                    if (methodCallExpr.Arguments[0].NodeType != ExpressionType.MemberAccess)
                        throw new NotSupportedException("不支持的表达式，只支持MemberAccess类型表达式");
                    sqlSegment = this.VisitMemberAccess(sqlSegment.Next(methodCallExpr.Arguments[0]));
                    sqlSegment.Change($"MIN({sqlSegment})", false, false, false, true);
                }
                break;
        }
        return sqlSegment;
    }
    public virtual bool IsStringConcatOperator(SqlSegment sqlSegment, out SqlSegment result)
    {
        var binaryExpr = sqlSegment.Expression as BinaryExpression;
        if (binaryExpr.NodeType == ExpressionType.Add && (binaryExpr.Left.Type == typeof(string) || binaryExpr.Right.Type == typeof(string)))
        {
            //调用拼接方法Concat,每个数据库Provider都实现了这个方法
            var methodInfo = typeof(string).GetMethod(nameof(string.Concat), new Type[] { typeof(object[]) });
            var parameters = Expression.NewArrayInit(typeof(object), binaryExpr);
            var methodCallExpr = Expression.Call(methodInfo, parameters);
            sqlSegment.Expression = methodCallExpr;
            this.OrmProvider.TryGetMethodCallSqlFormatter(methodCallExpr, out var formater);
            //返回的SQL表达式中直接拼接好          
            result = formater.Invoke(this, binaryExpr, null, null, binaryExpr);
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
    public virtual string VisitFromQuery(LambdaExpression lambdaExpr)
    {
        var currentExpr = lambdaExpr.Body;
        Expression firstExpr = null;
        var callStack = new Stack<MethodCallExpression>();
        while (true)
        {
            if (currentExpr is not MethodCallExpression callExpr)
                break;

            if (callExpr.Object.NodeType == ExpressionType.Parameter)
            {
                firstExpr = callExpr;
                break;
            }
            callStack.Push(callExpr);
            currentExpr = callExpr.Object;
        }
        var queryVisitor = this.OrmProvider.NewQueryVisitor(this.DbKey, this.MapProvider, this.IsParameterized, this.TableAsStart, this.ParameterPrefix, this.DbParameters);
        var fromQuery = new FromQuery(this.DbKey, this.OrmProvider, this.MapProvider, queryVisitor, this.IsParameterized);
        if (callStack.Count > 0)
        {
            while (callStack.TryPop(out var callExpr))
            {
                var methodInfo = callExpr.Method;
                var genericArguments = methodInfo.GetGenericArguments();
                LambdaExpression lambdaArgsExpr = null;
                switch (callExpr.Method.Name)
                {
                    case "From":
                        queryVisitor.From(this.Evaluate<char>(callExpr.Arguments[0]), this.Evaluate<string>(callExpr.Arguments[1]), genericArguments);
                        break;
                    //case "Union":
                    //case "UnionAll":
                    //    queryVisitor.Union(this.Evaluate<string>(callExpr.Arguments[0]));
                    //    queryVisitor.From(this.Evaluate<char>(callExpr.Arguments[0]), genericArguments);
                    //    break;
                    case "InnerJoin":
                    case "LeftJoin":
                    case "RightJoin":
                        var joinType = callExpr.Method.Name switch
                        {
                            "LeftJoin" => "LEFT JOIN",
                            "RightJoin" => "RIGHT JOIN",
                            _ => "INNER JOIN"
                        };
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
        }
        else
        {
            int sdfsd = 0;
        }
        var result = queryVisitor.BuildSql(out _);
        return result;
    }
    public virtual string GetQuotedValue(SqlSegment sqlSegment)
    {
        //默认只要是变量就设置为参数
        if (sqlSegment.IsVariable || (this.IsParameterized || sqlSegment.IsParameterized) && sqlSegment.IsConstant)
        {
            var parameterName = this.OrmProvider.ParameterPrefix + this.ParameterPrefix + this.DbParameters.Count.ToString();
            if (this.IsMultiple) parameterName += $"_m{this.CommandIndex}";
            if (sqlSegment.MemberMapper != null)
                this.OrmProvider.AddDbParameter(this.DbKey, this.DbParameters, sqlSegment.MemberMapper, parameterName, sqlSegment.Value);
            else this.DbParameters.Add(this.OrmProvider.CreateParameter(parameterName, sqlSegment.Value));

            sqlSegment.Value = parameterName;
            sqlSegment.HasParameter = true;
            sqlSegment.IsVariable = false;
            sqlSegment.IsConstant = false;
            return parameterName;
        }
        else if (sqlSegment.IsConstant)
        {
            var typedValue = sqlSegment.Value;
            if (sqlSegment.ExpectType != null)
                typedValue = this.ChangeTypedValue(sqlSegment.ExpectType, typedValue, sqlSegment.TargetType);
            return this.OrmProvider.GetQuotedValue(typedValue);
        }
        //带有参数或字段的表达式或函数调用、或是只有参数或字段
        //TODO:本地函数调用返回值，非常量、变量、字段、SQL函数调用
        return sqlSegment.ToString();
    }
    public virtual string GetQuotedValue(object elementValue, SqlSegment arraySegment)
    {
        if (elementValue is DBNull || elementValue == null)
            return "NULL";
        if (arraySegment.IsVariable || (this.IsParameterized || arraySegment.IsParameterized) && arraySegment.IsConstant)
        {
            var parameterName = this.OrmProvider.ParameterPrefix + this.ParameterPrefix + this.DbParameters.Count.ToString();
            if (this.IsMultiple) parameterName += $"_m{this.CommandIndex}";

            if (arraySegment.MemberMapper != null)
                this.OrmProvider.AddDbParameter(this.DbKey, this.DbParameters, arraySegment.MemberMapper, parameterName, elementValue);
            else this.DbParameters.Add(this.OrmProvider.CreateParameter(parameterName, elementValue));
            return parameterName;
        }
        if (arraySegment.IsConstant)
        {
            var typedValue = elementValue;
            if (arraySegment.ExpectType != null)
                typedValue = this.ChangeTypedValue(arraySegment.ExpectType, elementValue, arraySegment.TargetType);
            return this.OrmProvider.GetQuotedValue(typedValue);
        }
        return this.OrmProvider.GetQuotedValue(elementValue);
    }
    public virtual IQueryVisitor CreateQueryVisitor()
    {
        var queryVisiter = this.OrmProvider.NewQueryVisitor(this.DbKey, this.MapProvider, this.IsParameterized, this.TableAsStart, this.ParameterPrefix, this.DbParameters);
        queryVisiter.IsMultiple = this.IsMultiple;
        queryVisiter.CommandIndex = this.CommandIndex;
        queryVisiter.CteQueries = this.CteQueries;
        return queryVisiter;
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
        else strExpression = sqlSegment.ToString();
        if (this.IsSelect || (this.IsWhere && !isExpectBooleanType))
            strExpression = $"CASE WHEN {strExpression} THEN {ifTrueValue} ELSE {ifFalseValue} END";
        return sqlSegment.Change(strExpression, false, false, true);
    }
    public List<ReaderField> FlattenTableFields(TableSegment tableSegment)
    {
        var targetFields = new List<ReaderField>();
        foreach (var memberMapper in tableSegment.Mapper.MemberMaps)
        {
            if (memberMapper.IsIgnore || memberMapper.IsNavigation
                || (memberMapper.MemberType.IsEntityType(out _) && memberMapper.TypeHandler == null))
                continue;
            targetFields.Add(new ReaderField
            {
                FieldType = ReaderFieldType.Field,
                TableSegment = tableSegment,
                FromMember = memberMapper.Member,
                MemberMapper = memberMapper,
                TargetMember = memberMapper.Member,
                Body = tableSegment.AliasName + "." + this.OrmProvider.GetFieldName(memberMapper.FieldName)
            });
        }
        return targetFields;
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
            //变量时，根据MemberMapper的配置，把参数转变成真正的数据库字段类型
            rightSegment.MemberMapper = leftSegment.MemberMapper;
            //常量时，根据ExpectType、TargetType值，把常量转变成真正的数据库字段类型
            if (rightSegment.IsConstant)
            {
                rightSegment.ExpectType = leftSegment.ExpectType;
                rightSegment.TargetType = leftSegment.TargetType;
            }
            return true;
        }
        return false;
    }
    public object ChangeTypedValue(Type expectType, object value, Type targetType = null)
    {
        var result = value;
        if (expectType.IsEnum)
        {
            if (value.GetType() != expectType)
                result = Enum.ToObject(expectType, value);
            if (targetType != null && targetType == typeof(string))
                result = result.ToString();
        }
        return result;
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
    public bool IsGroupingMember(MemberExpression memberExpr)
    {
        if (memberExpr == null) return false;
        return memberExpr.Member.Name == "Grouping" && typeof(IAggregateSelect).IsAssignableFrom(memberExpr.Member.DeclaringType);
    }
    public virtual void Dispose()
    {
        this.ParameterPrefix = null;
        this.Tables = null;
        this.TableAlias = null;
        this.ReaderFields = null;
        this.WhereSql = null;
        this.GroupFields = null;

        this.DbKey = null;
        this.DbParameters = null;
        this.OrmProvider = null;
        this.MapProvider = null;
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
