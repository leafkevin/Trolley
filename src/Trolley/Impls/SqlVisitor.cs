using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlTypes;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Trolley;

public class SqlVisitor : ISqlVisitor, ICommandContext
{
    private bool isDisposed;
    private static MethodInfo IsNullMethodInfo = typeof(Sql).GetMethods().Where(f => f.Name == nameof(Sql.IsNull) && f.GetParameters().Length == 2).First();

    public DbContext DbContext { get; set; }
    public string DbKey => this.DbContext.DbKey;
    public IOrmProvider OrmProvider => this.DbContext.OrmProvider;
    public IEntityMapProvider EntityMapProvider => this.DbContext.EntityMapProvider;
    public ITableShardingProvider ShardingProvider => this.DbContext.TableShardingProvider;
    public string DefaultTableSchema => this.DbContext.DefaultTableSchema;
    public bool IsConstantParameterized => this.DbContext.Options.IsConstantParameterized;
    public string UserParameterPrefix => this.DbContext.Options.UserParameterPrefix;

    public ITheaCommand Command { get; set; }
    public IDataParameterCollection DbParameters { get; set; }
    public IDataParameterCollection NextDbParameters { get; set; }
    public char TableAsStart { get; set; }

    /// <summary>
    /// 所有表都是扁平化的，主表、1:1关系Include子表，也在这里
    /// </summary>
    public List<TableSegment> Tables { get; set; } = new();
    public Dictionary<string, TableSegment> TableAliases { get; set; } = new();
    /// <summary>
    /// 在解析子查询中，会用到父查询中的所有表，父查询中所有表别名引用
    /// </summary> 
    public Dictionary<string, TableSegment> RefTableAliases { get; set; }

    #region Build Sql时使用，临时状态变量
    public bool IsSelect { get; set; }
    public bool IsSelectMember { get; set; }
    public bool IsWhere { get; set; }
    public bool IsIncludeMany { get; set; }
    #endregion

    public bool IsNeedTableAlias { get; set; }
    public List<ReaderField> ReaderFields { get; set; }

    public StringBuilder WhereBuilder { get; set; }
    public OperationType LastWhereOperationType { get; set; } = OperationType.None;

    public List<TableSegment> IncludeTables { get; set; }
    /// <summary>
    /// 引用的CTE子查询或是子查询对象引用列表
    /// </summary>
    public List<IQuery> RefQueries { get; set; } = new();
    /// <summary>
    /// 当前子查询最后AsCteTable后生成的对象，或是CTE表UnionRecursive语句解析中使用的自引用对象，此时IsRecursive=true
    /// </summary>
    public ICteQuery CteQueryObj { get; set; }
    /// <summary>
    /// 是否是递归查询，在UnionRecursive语句解析中会用到
    /// </summary>
    public bool IsRecursive { get; set; }
    public bool IsUnion { get; set; }
    /// <summary>
    /// 第二个Union子句
    /// </summary>
    public bool IsSecondUnion { get; set; }

    public string ShardingTableJointMark { get; set; } = " UNION ALL ";
    public bool IsNeedUnionShardingTables { get; set; }
    public bool IsNeedFormatShardingTables { get; set; }
    public bool IsManyShardingTables { get; set; }

    public string AggFieldAlias { get; set; }
    /// <summary>
    /// 分页查询的Count操作，是否需要全部字段Count
    /// </summary>
    public bool HasAggFields { get; set; }
    /// <summary>
    /// 当前查询语句中，所有有多个分表的实体表列表，第一个多个分表的实体表为主表，其他多个分表的实体表，必须调用UseTableMap方法，指定与主表的映射关系
    /// </summary>
    public List<TableSegment> ShardingTables { get; set; }
    /// <summary>
    /// 如果当前queryVisitor对象是在一个queryVisitor对象中创建的，这个值就是父queryVisitor对象，
    /// 用于判断一些引用参数是否需要拷贝
    /// </summary>
    public object RefFrom { get; set; }
    public string UnionSql { get; set; }
    public string HeadRawSql { get; set; }
    public string TailRawSql { get; set; }

    public void UseTable(TableShardingUsageMode usageMode, bool isIncludeMany, params string[] tableNames)
    {
        if (tableNames == null || tableNames.Length == 0)
            throw new ArgumentNullException(nameof(tableNames), "tableNames参数不能为空");

        var tableSegment = isIncludeMany ? this.IncludeTables.Last() : this.Tables.Last();
        if (tableNames.Length > 1 && !this.TrySetTableShardingInfo(tableSegment, usageMode, out var tableShardingInfo))
            return;

        //多个分表，才当作分表处理
        tableSegment.IsSharding = tableSegment.TableShardingInfo != null;
        if (tableSegment.IsSharding) tableSegment.IsIncludeManySharding = isIncludeMany;
        if (tableNames.Length > 1)
        {
            tableSegment.ShardingType = ShardingTableType.MultiTable;
            tableSegment.TableNames = [.. tableNames];
            this.ShardingTables ??= new();
            if (this.ShardingTables.Exists(f => f.ShardingType == ShardingTableType.MultiTable))
                throw new NotSupportedException("不存在多分表的实体表，不能使用此方法，可直接使用首个多分表为MultiTable类型，其余表只能为调用方法UseTableMap与首个多分表表名映射实现多分表");
            if (!this.ShardingTables.Contains(tableSegment))
            {
                tableSegment.ShardingId = Guid.NewGuid().ToString("N");
                this.ShardingTables.Add(tableSegment);
            }
            this.IsNeedFormatShardingTables = true;
        }
        //一个分表的，当作不分表处理
        else
        {
            if (tableSegment.IsSharding) tableSegment.ShardingType = ShardingTableType.SingleTable;
            tableSegment.Body = tableNames[0];
        }
        this.IsManyShardingTables = this.ShardingTables[0].ShardingType == ShardingTableType.MultiTable;
    }
    public void UseTableByRange(TableShardingUsageMode usageMode, bool isIncludeMany, object[] fieldValues)
    {
        var tableSegment = isIncludeMany ? this.IncludeTables.Last() : this.Tables.Last();
        if (!this.TrySetTableShardingInfo(tableSegment, usageMode, out var tableShardingInfo))
            return;

        var origTableName = tableSegment.Mapper.TableName;
        var tableNames = tableShardingInfo.RangleRule.Invoke(origTableName, fieldValues);
        if (tableNames == null || tableNames.Count == 0)
            throw new Exception($"没有搜索到满足条件的{tableSegment.Mapper.TableName}分表");
        this.ShardingTables ??= new();
        if (this.ShardingTables.Exists(f => f.ShardingType == ShardingTableType.MultiTable))
            throw new NotSupportedException("仅支持首个多分表为MultiTable类型，其余表只能为调用方法UseTableMap与首个多分表表名映射实现多分表");

        tableSegment.IsSharding = true;
        tableSegment.IsIncludeManySharding = isIncludeMany;
        tableSegment.ShardingType = ShardingTableType.MultiTable;
        if (!this.ShardingTables.Contains(tableSegment))
        {
            tableSegment.ShardingId = Guid.NewGuid().ToString("N");
            this.ShardingTables.Add(tableSegment);
        }
        tableSegment.TableNames ??= new();
        if (!string.IsNullOrEmpty(tableSegment.Body)
            && !tableSegment.TableNames.Contains(tableSegment.Body))
        {
            tableSegment.TableNames.Add(tableSegment.Body);
            tableSegment.Body = null;
        }
        if (tableSegment.TableNames != null)
            tableSegment.TableNames.AddRange(tableNames);

        //范围分表，都当作多分表处理，方便后续表映射
        this.IsNeedFormatShardingTables = true;
        this.IsManyShardingTables = this.ShardingTables[0].ShardingType == ShardingTableType.MultiTable;
    }
    public void UseTableMap(TableShardingUsageMode usageMode, bool isIncludeMany, Func<string, string, string, string> tableNameGetter)
    {
        if (tableNameGetter == null)
            throw new ArgumentNullException(nameof(tableNameGetter), "tableNameGetter参数不能为空");
        var tableSegment = isIncludeMany ? this.IncludeTables.Last() : this.Tables.Last();

        if (!this.TrySetTableShardingInfo(tableSegment, usageMode, out var tableShardingInfo))
            return;
        if (this.ShardingTables == null || !this.ShardingTables.Exists(f => f.ShardingType == ShardingTableType.MultiTable))
            throw new NotSupportedException("不存在多分表的实体表，无法配置多分表映射，使用UseTable、UseTableBy方法后存在多分表后，才能使用本方法配置多分表映射");

        tableSegment.IsSharding = true;
        tableSegment.IsIncludeManySharding = isIncludeMany;
        tableSegment.ShardingType = ShardingTableType.ShardingTableMap;
        tableSegment.ShardingMapGetter = tableNameGetter;
        if (!this.ShardingTables.Contains(tableSegment))
        {
            tableSegment.ShardingId = Guid.NewGuid().ToString("N");
            this.ShardingTables.Add(tableSegment);
        }
        this.IsNeedFormatShardingTables = true;
    }
    public void UseTableBy(TableShardingUsageMode usageMode, bool isIncludeMany, params object[] fieldValues)
    {
        var tableSegment = isIncludeMany ? this.IncludeTables.Last() : this.Tables.Last();
        if (!this.TrySetTableShardingInfo(tableSegment, usageMode, out var tableShardingInfo))
            return;
        if (fieldValues == null)
            throw new ArgumentNullException($"字段值fieldValues不可为null");
        if (tableShardingInfo.Rule.Method.GetParameters().Length != fieldValues.Length)
            throw new Exception($"实体{tableSegment.EntityType.FullName}表有配置分表规则依赖字段个数与提供的字段值fieldValues个数不一致");

        tableSegment.IsSharding = true;
        tableSegment.IsIncludeManySharding = isIncludeMany;
        var origTableName = tableSegment.Mapper.TableName;
        var tableName = tableShardingInfo.Rule.Invoke(origTableName, fieldValues) as string;

        //单个分表，直接设置body表名，当作不分表处理
        if (!string.IsNullOrEmpty(tableSegment.Body))
        {
            if (tableName == tableSegment.Body)
                return;
            if (tableSegment.TableNames == null)
            {
                tableSegment.TableNames = new();
                this.ShardingTables ??= new();
                tableSegment.ShardingType = ShardingTableType.MultiTable;
                if (!this.ShardingTables.Contains(tableSegment))
                    this.ShardingTables.Add(tableSegment);
            }
        }
        if (tableSegment.TableNames != null)
        {
            if (!string.IsNullOrEmpty(tableSegment.Body))
            {
                tableSegment.TableNames.Add(tableSegment.Body);
                tableSegment.Body = null;
            }
            if (!tableSegment.TableNames.Contains(tableName))
                tableSegment.TableNames.Add(tableName);
            this.IsNeedFormatShardingTables = true;
        }
        else
        {
            tableSegment.ShardingType = ShardingTableType.SingleTable;
            tableSegment.Body = tableName;
        }
        this.IsManyShardingTables = this.ShardingTables[0].ShardingType == ShardingTableType.MultiTable;
    }
    /// <summary>
    /// 设置批量插入、更新操作时的分表名获取委托
    /// </summary>
    /// <param name="tableNameGetter"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public void UseTable(TableShardingUsageMode usageMode, Func<object, string> tableNameGetter)
    {
        if (tableNameGetter == null)
            throw new ArgumentNullException(nameof(tableNameGetter), "tableNameGetter参数不能为空");
        this.Tables[0].ShardingTableGetter = tableNameGetter;
    }
    public bool TrySetTableShardingInfo(TableSegment tableSegment, TableShardingUsageMode usageMode, out TableShardingInfo tableShardingInfo)
    {
        if (tableSegment.TableShardingInfo != null)
        {
            tableShardingInfo = tableSegment.TableShardingInfo;
            return true;
        }
        if (this.ShardingProvider == null)
            throw new Exception("当前系统没有配置任何分表信息");
        var entityType = tableSegment.EntityType;
        if (!this.ShardingProvider.TryGetTableSharding(entityType, out tableShardingInfo))
        {
            var entityMapper = this.DbContext.EntityMapProvider.GetEntityMap(entityType);
            throw new Exception($"实体表{entityType.FullName}没有配置分表，原表名：{entityMapper?.TableName}");
        }
        if (tableShardingInfo.UsageMode != TableShardingUsageMode.Default || tableShardingInfo.UsageMode != usageMode)
            throw new Exception($"实体表{entityType.FullName}的分表规则无法应用于当前操作，当前配置的应用范围为：TableShardingUsageMode.{tableShardingInfo.UsageMode}");
        tableSegment.TableShardingInfo = tableShardingInfo;
        return true;
    }
    public bool TryGetTableShardingInfo(Type entityType, TableShardingUsageMode usageMode, out TableShardingInfo tableShardingInfo)
    {
        tableShardingInfo = null;
        if (this.ShardingProvider == null)
            return false;
        if (this.ShardingProvider.TryGetTableSharding(entityType, out tableShardingInfo)
           && (tableShardingInfo.UsageMode == TableShardingUsageMode.Default || tableShardingInfo.UsageMode == usageMode))
            return true;
        return false;
    }
    public void UseUnionShardingTable() => this.ShardingTableJointMark = "UNION";
    public virtual void UseTableSchema(bool isIncludeMany, string tableSchema)
    {
        if (tableSchema == this.DefaultTableSchema) return;

        var tableSegment = isIncludeMany ? this.IncludeTables.Last() : this.Tables.Last();
        tableSegment.TableSchema = tableSchema;
    }

    public virtual void VisitAndSql(string whereSql, OperationType operationType = OperationType.None)
    {
        this.WhereBuilder ??= new();
        var lastOperationType = this.WhereBuilder.Length > 0 ? OperationType.And : operationType;
        if (this.LastWhereOperationType == OperationType.Or)
        {
            this.WhereBuilder.Insert(0, '(');
            this.WhereBuilder.Append(')');
        }
        if (this.WhereBuilder.Length > 0)
        {
            this.WhereBuilder.Append(" AND ");
            if (operationType == OperationType.Or)
                whereSql = $"({whereSql})";
        }
        this.WhereBuilder.Append(whereSql);
        this.LastWhereOperationType = lastOperationType;
    }
    public virtual void VisitOrSql(string whereSql, OperationType operationType = OperationType.None)
    {
        this.WhereBuilder ??= new();
        var lastOperationType = this.WhereBuilder.Length > 0 ? OperationType.Or : operationType;
        if (this.WhereBuilder.Length > 0)
            this.WhereBuilder.Append(" OR ");
        this.WhereBuilder.Append(whereSql);
        this.LastWhereOperationType = lastOperationType;
    }
    public virtual void WithHeadSql(string rawSql)
    {
        if (string.IsNullOrEmpty(rawSql))
            throw new ArgumentNullException(nameof(rawSql));
        this.HeadRawSql = rawSql;
    }
    public virtual void WithTailSql(string rawSql)
    {
        if (string.IsNullOrEmpty(rawSql))
            throw new ArgumentNullException(nameof(rawSql));
        this.TailRawSql = rawSql;
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
        var result = sqlSegment;
        //初始值为表达式的类型
        //sqlSegment.SegmentType = sqlSegment.Expression.Type;
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
            case ExpressionType.AndAlso:
            case ExpressionType.OrElse:
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
        SqlSegment resultSegment = default;
        var unaryExpr = sqlSegment.Expression as UnaryExpression;
        switch (unaryExpr.NodeType)
        {
            case ExpressionType.Not:
                if (unaryExpr.Type == typeof(bool))
                {
                    //SELECT/WHERE语句，都会有Defer处理，在最外层再计算bool值
                    sqlSegment.Push(DeferredOperation.Not);
                    return this.Visit(sqlSegment.Next(unaryExpr.Operand));
                }
                resultSegment = this.Visit(sqlSegment.Next(unaryExpr.Operand));
                if (resultSegment.SqlType < SqlType.OnlyField)
                    return resultSegment.Change(ValueEvalutor.Not(resultSegment.Value));
                return resultSegment.Change($"~{resultSegment.Value}", SqlType.Expression);
            case ExpressionType.Convert:
                //以下3种情况会走到此处
                //(int)f.TotalAmount强制转换或是枚举f.Gender = Gender.Male表达式
                //或是表达式计算，如：30 + f.TotalAmount，int amount = 30;amount + f.TotalAmount，
                //表达式把30解析为double类型常量，amount解析为double类型的强转转换
                //或是方法调用Convert.ToXxx,string.Concat,string.Format,string.Join
                //如：f.Gender.ToString(),string.Format("{0},{1},{2}", 30, DateTime.Now, Gender.Male)

                resultSegment = this.Visit(sqlSegment.Next(unaryExpr.Operand));
                if (unaryExpr.Method != null)
                    resultSegment.IsEnum = unaryExpr.Operand.Type.IsEnum;
                //if (unaryExpr.Operand.IsParameter(out _))
                //{
                //    if (unaryExpr.Type != typeof(object))
                //        sqlSegment.ExpectType = unaryExpr.Type;
                //    return this.Visit(sqlSegment.Next(unaryExpr.Operand));
                //}
                //return this.Evaluate(sqlSegment);
                return resultSegment;
        }
        return this.Visit(sqlSegment.Next(unaryExpr.Operand));
    }
    public virtual SqlSegment VisitBinary(SqlSegment sqlSegment)
    {
        var binaryExpr = sqlSegment.Expression as BinaryExpression;
        if (binaryExpr.NodeType == ExpressionType.Add || binaryExpr.NodeType == ExpressionType.AddChecked
            || binaryExpr.NodeType == ExpressionType.Subtract || binaryExpr.NodeType == ExpressionType.SubtractChecked)
        {
            //处理字符串连接的场景，EFCore中字符串连接会翻译成string.Concat方法调用，但也有可能是+操作，所以两种情况都要处理
            if (this.IsStringConcatOperator(sqlSegment, binaryExpr, out var operatorSegment))
                return operatorSegment;
            //TODO:DateOnly,TimeOnly两个类型要做处理
            if (this.IsDateTimeOperator(sqlSegment, binaryExpr, out operatorSegment))
                return operatorSegment;
            if (this.IsTimeSpanOperator(sqlSegment, binaryExpr, out operatorSegment))
                return operatorSegment;
        }
        var leftSegment = this.Visit(new SqlSegment { Expression = binaryExpr.Left });
        if (leftSegment.IsDeferredFields) return sqlSegment;
        var rightSegment = this.Visit(new SqlSegment { Expression = binaryExpr.Right });
        if (rightSegment.IsDeferredFields) return sqlSegment;

        //常量或变量的场景
        if (leftSegment.IsValue && rightSegment.IsValue)
        {
            var sqlType = rightSegment.SqlType > leftSegment.SqlType ? rightSegment.SqlType : leftSegment.SqlType;
            var value = ValueEvalutor.Evaluate(binaryExpr, leftSegment.Value, rightSegment.Value);
            return sqlSegment.Change(value, sqlType);
        }

        //下面都是带有参数的情况，带有参数表达式计算(常量、变量)、函数调用等共2种情况
        //bool类型的表达式，这里不做解析只做defer操作解析，到最外层select、where、having、joinOn子句中去解析合并
        if (binaryExpr.NodeType == ExpressionType.Equal || binaryExpr.NodeType == ExpressionType.NotEqual)
        {
            //处理null != a.UserName和"kevin" == a.UserName情况
            if (leftSegment.IsValue && rightSegment.HasField)
                this.Swap(ref leftSegment, ref rightSegment);
            if (leftSegment.IsNull && !rightSegment.IsNull)
                this.Swap(ref leftSegment, ref rightSegment);

            //处理!(a.IsEnabled==true)情况，bool类型，最外层再做defer处理
            if (leftSegment.HasField && rightSegment.IsFixedValue)
            {
                if (binaryExpr.Left.Type == typeof(bool))
                {
                    leftSegment.Push(DeferredOperation.IsTrue);
                    if (!(bool)rightSegment.Value)
                        leftSegment.Push(DeferredOperation.Not);
                }
                else leftSegment.Push(DeferredOperation.IsNull);

                if (binaryExpr.NodeType == ExpressionType.NotEqual)
                    leftSegment.Push(DeferredOperation.Not);
                return leftSegment;
            }
        }
        //带有参数成员访问+常量/变量+带参数的函数调用的表达式
        var operators = this.OrmProvider.GetBinaryOperator(binaryExpr.NodeType);

        //??操作类型没有变更，可以当作Field使用
        //if (binaryExpr.NodeType == ExpressionType.Coalesce)
        //    leftSegment.IsFieldType = true;

        //如果是IsParameter,HasField,IsExpression,IsMethodCall直接返回,是SQL
        //如果是变量或是要求变成参数的常量，变成@p返回
        //如果是常量获取当前类型值，再转成QuotedValue值
        //就是枚举类型有问题，单独处理
        //... WHERE (int)(a.Price * a.Quartity)>500
        //SELECT TotalAmount = (int)(amount + (a.Price + increasedPrice) * (a.Quartity + increasedCount)) ...FROM ...
        //SELECT OrderNo = $"OrderNo-{f.CreatedAt.ToString("yyyyMMdd")}-{f.Id}"...FROM ...

        //单个字段访问，才会设置nativeDbType和typeHandler
        //如果是枚举类型，右边是值，左边是字段访问，且字段访问的表达式类型是枚举类型，则把右边的值当作枚举值处理
        if (leftSegment.SqlType == SqlType.OnlyField && rightSegment.IsValue)
        {
            rightSegment.MemberMapper = leftSegment.MemberMapper;
            rightSegment.MappedTargetType = leftSegment.MappedTargetType;
            rightSegment.TypeHandler = leftSegment.TypeHandler;
        }

        string leftValue = this.GetQuotedValue(leftSegment);
        string rightValue = this.GetQuotedValue(rightSegment);
        if (binaryExpr.NodeType == ExpressionType.Coalesce)
            return sqlSegment.Change($"{operators}({leftValue},{rightValue})", SqlType.Expression);
        return sqlSegment.Change($"{leftValue}{operators}{rightValue}", SqlType.Expression);
    }
    public virtual SqlSegment VisitMemberAccess(SqlSegment sqlSegment)
    {
        var memberExpr = sqlSegment.Expression as MemberExpression;
        MemberAccessSqlFormatter formatter = null;
        var memberInfo = memberExpr.Member;

        if (sqlSegment.IsDeferredFields)
            return this.VisitDeferredSqlSegment(sqlSegment);

        if (memberExpr.Expression != null)
        {
            //Where(f=>... && !f.OrderId.HasValue && ...)
            //Where(f=>... f.OrderId.Value==10 && ...)
            //Select(f=>... ,f.OrderId.HasValue  ...)
            //Select(f=>... ,f.OrderId.Value==10  ...)
            if (memberExpr.Type.IsValueType && Nullable.GetUnderlyingType(memberExpr.Type) != null)
            {
                if (memberInfo.Name == "HasValue")
                {
                    sqlSegment.Push(DeferredOperation.IsNull);
                    sqlSegment.Push(DeferredOperation.Not);
                }
                return this.Visit(sqlSegment.Next(memberExpr.Expression));
            }
            //各种类型实例成员访问，如：DateTime,TimeSpan,String.Length,List.Count
            if (this.OrmProvider.TryGetMemberAccessSqlFormatter(memberExpr, out formatter))
            {
                //Where(f=>... && f.CreatedAt.Month<5 && ...)
                //Where(f=>... && f.Order.OrderNo.Length==10 && ...)
                var targetSegment = sqlSegment.Next(memberExpr.Expression);
                sqlSegment = formatter.Invoke(this, targetSegment);
                return sqlSegment;
            }

            if (memberExpr.HasParameter(out var parameterName))
            {
                //Where(f => f.Amount > 5)
                //Select(f => new { f.OrderId, f.Disputes ...})
                var tableSegment = this.TableAliases[parameterName];
                sqlSegment.TableSegment = tableSegment;
                string fieldName = null;

                if (tableSegment.TableType == TableType.FromQuery || tableSegment.TableType == TableType.CteSelfRef)
                {
                    //访问子查询表的成员，子查询表没有Mapper，也不会有实体类型成员
                    //Json的实体类型字段
                    ReaderField readerField = default;
                    //子查询中，Select了Grouping分组对象，子查询中，只有一个分组对象才是实体类型，目前子查询，只支持一层
                    //取AS后的字段名，与原字段名不一定一样,AS后的字段名与memberExpr.Member.Name一致
                    if (memberExpr.Expression.NodeType != ExpressionType.Parameter)
                    {
                        var parentMemberExpr = memberExpr.Expression as MemberExpression;
                        var parenetReaderField = tableSegment.Fields.Count == 1 ? tableSegment.Fields.First()
                            : tableSegment.Fields.Find(f => f.TargetMember.Name == parentMemberExpr.Member.Name);
                        var fromReaderFields = parenetReaderField.Fields;
                        readerField = fromReaderFields.Count == 1 ? fromReaderFields.First()
                            : fromReaderFields.Find(f => f.TargetMember.Name == memberInfo.Name);
                        fieldName = this.OrmProvider.GetFieldName(memberInfo.Name);
                        if (this.IsNeedTableAlias) fieldName = tableSegment.AliasName + "." + fieldName;
                    }
                    else
                    {
                        readerField = tableSegment.Fields.Count == 1 ? tableSegment.Fields.First()
                          : tableSegment.Fields.Find(f => f.TargetMember.Name == memberInfo.Name);
                        fieldName = readerField.Value;
                    }
                    sqlSegment.Value = fieldName;
                }
                else
                {
                    var memberMapper = tableSegment.Mapper.GetMemberMap(memberInfo.Name);
                    if (memberMapper.IsIgnore)
                        throw new Exception($"类{tableSegment.EntityType.FullName}的成员{memberMapper.MemberName}是忽略成员无法访问");
                    if (memberMapper.MemberType.IsEntityType(out _) && !memberMapper.IsNavigation && memberMapper.TypeHandler == null)
                        throw new Exception($"类{tableSegment.EntityType.FullName}的成员{memberInfo.Name}不是值类型，未配置为导航属性也没有配置TypeHandler");
                    sqlSegment.MemberMapper = memberMapper;
                    sqlSegment.IsEnum = memberMapper.UnderlyingType.IsEnum;
                    //查询时，IsNeedAlias始终为true，新增、更新、删除时，引用联表操作时，才会为true
                    fieldName = this.OrmProvider.GetFieldName(memberMapper.FieldName);
                    if (this.IsNeedTableAlias) fieldName = tableSegment.AliasName + "." + fieldName;
                    sqlSegment.Value = fieldName;
                }
                //.NET枚举类型总是解析成对应的UnderlyingType数值类型，如：a.Gender ?? Gender.Male == Gender.Male
                return sqlSegment;
            }
        }

        if (memberInfo.DeclaringType == typeof(DBNull))
            return SqlSegment.Null;

        //各种静态成员访问，如：DateTime.Now,int.MaxValue,string.Empty
        if (this.OrmProvider.TryGetMemberAccessSqlFormatter(memberExpr, out formatter))
        {
            sqlSegment = formatter.Invoke(this, sqlSegment);
            //sqlSegment.TargetType = memberExpr.Type;
            return sqlSegment;
        }

        //优化本地成员访问
        if (memberExpr.Expression != null)
        {
            var objSegment = this.Visit(new SqlSegment { Expression = memberExpr.Expression });
            if (objSegment.IsValue)
            {
                object memberValue = null;
                if (memberInfo is PropertyInfo propertyInfo)
                    memberValue = propertyInfo.GetValue(objSegment.Value);
                else if (memberInfo is FieldInfo fieldInfo)
                    memberValue = fieldInfo.GetValue(objSegment.Value);
                //sqlSegment.SegmentType = memberExpr.Type;
                return sqlSegment.Change(memberValue);
            }
        }

        //访问局部变量或是成员变量，当作常量处理，直接计算，后面统一做参数化处理
        //var orderIds=new List<int>{1,2,3}; Where(f=>orderIds.Contains(f.OrderId)); orderIds
        //private Order order; Where(f=>f.OrderId==this.Order.Id); this.Order.Id
        //var orderId=10; Select(f=>new {OrderId=orderId,...}
        //Select(f=>new {OrderId=this.Order.Id, ...}

        //sqlSegment.IsConstant = false;
        //sqlSegment.IsVariable = true;
        //sqlSegment.SegmentType = memberExpr.Type;
        return this.Evaluate(sqlSegment, SqlType.Variable);
    }
    public virtual SqlSegment VisitConstant(SqlSegment sqlSegment)
    {
        var constantExpr = sqlSegment.Expression as ConstantExpression;
        if (constantExpr.Value == null)
            return SqlSegment.Null;

        sqlSegment.SqlType = SqlType.Constant;
        sqlSegment.Value = constantExpr.Value;
        return sqlSegment;
    }
    public virtual SqlSegment VisitMethodCall(SqlSegment sqlSegment)
    {
        var methodCallExpr = sqlSegment.Expression as MethodCallExpression;
        var declaringType = methodCallExpr.Method.DeclaringType;
        if (declaringType == typeof(Sql) || declaringType == typeof(IRepository)
           || typeof(IAggregateSelect).IsAssignableFrom(declaringType)
           || declaringType == typeof(IQueryBase))
        {
            sqlSegment = this.VisitSqlMethodCall(sqlSegment);
            //sqlSegment.TargetType = methodCallExpr.Type;
            return sqlSegment;
        }
        if (methodCallExpr.Method.Name == "ToValue")
        {
            if (declaringType.FullName.StartsWith("Trolley.ISqlOver") || declaringType.FullName.StartsWith("Trolley.IPartitionByOver"))
                sqlSegment = this.VisitOverMethodCall(sqlSegment);
            else if (methodCallExpr.Method.Name == "ToValue" && declaringType == typeof(IGroupConcat))
                sqlSegment = this.VisitGroupConcatMethodCall(sqlSegment);
            else if (methodCallExpr.Method.Name == "ToValue" && declaringType == typeof(IStringAgg))
                sqlSegment = this.VisitStringAggMethodCall(sqlSegment);
            return sqlSegment;
        }
        if (sqlSegment.IsDeferredFields)
            return this.VisitDeferredSqlSegment(sqlSegment);

        if (!sqlSegment.IsDeferredFields && this.OrmProvider.TryGetMethodCallSqlFormatter(methodCallExpr, out var formatter))
        {
            sqlSegment = formatter.Invoke(this, methodCallExpr, methodCallExpr.Object, sqlSegment.DeferredOperations, methodCallExpr.Arguments.ToArray());
            //sqlSegment.TargetType = methodCallExpr.Type;
            return sqlSegment;
        }
        //如果是Select，并且有参数访问，当作延迟方法调用
        if (this.IsSelect && sqlSegment.Expression.HasParameter())
        {
            //延迟方法调用，两种场景：
            //1.主动延迟方法调用：如，把返回的枚举列转成描述，参数就是枚举列，返回值是对应的描述
            //2.Select子句中Include导航成员引用访问，主表数据已经查询了，此处成员访问只是多一个引用赋值动作，做成了延迟委托调用

            //$"{f.OrderNo} : {f.TotalAmount.ToString("C")}"
            //f.TotalAmount.ToString("C")
            //"TotalAmount: " + (f.Price * f.Quantity).ToString("C")
            //this.DeferredInvoke(f.Price, f.Quantity)
            return this.VisitDeferredSqlSegment(sqlSegment);
        }
        return this.Evaluate(sqlSegment);
        //sqlSegment.SegmentType = methodCallExpr.Type;
        //如果未指定常量和变量，当作变量处理，通常是常量或是变量经过一系列Linq操作得到的结果值
        //if (!sqlSegment.IsConstant && !sqlSegment.IsVariable)
        //    sqlSegment.IsVariable = true;
    }
    public virtual SqlSegment VisitParameter(SqlSegment sqlSegment)
    {
        var parameterExpr = sqlSegment.Expression as ParameterExpression;
        //两种场景：.Select((x, y) => new { Order = x, x.Seller, x.Buyer, ... }) 和 .Select((x, y) => x)
        //参数访问通常都是SELECT语句的实体访问
        if (!this.IsSelect) throw new NotSupportedException($"不支持的参数表达式访问，只支持Select语句中，{parameterExpr}");

        var fromSegment = this.TableAliases[parameterExpr.Name];
        var readerField = new ReaderField
        {
            FieldType = SqlFieldType.Entity,
            TableSegment = fromSegment,
            ReaderType = fromSegment.EntityType,
            Fields = this.FlattenTableFields(fromSegment),
            Path = parameterExpr.Name,
            IsTargetType = true
        };
        //include表的ReaderField字段，紧跟在主表ReaderField后面
        List<ReaderField> readerFields = [readerField];
        this.AddIncludeTableReaderFields(readerField, readerFields);
        return sqlSegment.Change(readerField, SqlType.ReaderFields);
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
                    FieldType = SqlFieldType.Entity,
                    TableSegment = includedSegment,
                    TargetMember = includedSegment.FromMember.Member,
                    ReaderType = includedSegment.EntityType,
                    Parent = parent,
                    Fields = this.FlattenTableFields(includedSegment),
                    //更换path，方便后续Include成员赋值时，能够找到parent对象
                    Path = includedSegment.Path.Replace(parent.TableSegment.Path, parent.Path)
                };
                readerFields.Add(readerField);
                if (this.Tables.Exists(f => f.TableType == TableType.Include && f.FromTable == includedSegment))
                    this.AddIncludeTableReaderFields(readerField, readerFields);
            }
        }
        if (this.IncludeTables != null)
        {
            var manyIncludedSegments = this.IncludeTables.FindAll(f => f.FromTable == parent.TableSegment);
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
        var newArrayExpr = sqlSegment.Expression as NewArrayExpression;
        var result = new List<object>();
        foreach (var elementExpr in newArrayExpr.Expressions)
        {
            var elementSegment = new SqlSegment { Expression = elementExpr };
            elementSegment = this.VisitAndDeferred(elementSegment);
            result.Add(elementSegment.Value);
        }
        //走到这里肯定是常量，变量会走到成员访问
        return sqlSegment.Change(result);
    }
    public virtual SqlSegment VisitIndexExpression(SqlSegment sqlSegment)
    {
        if (sqlSegment.Expression.HasParameter())
            throw new NotSupportedException("索引表达式不支持Parameter访问操作");
        return this.Evaluate(sqlSegment);
    }
    public virtual SqlSegment VisitConditional(SqlSegment sqlSegment)
    {
        var conditionalExpr = sqlSegment.Expression as ConditionalExpression;
        sqlSegment = this.Visit(sqlSegment.Next(conditionalExpr.Test));
        if (sqlSegment.IsValue)
        {
            var isTest = (bool)sqlSegment.Value;
            return isTest ? this.Visit(new SqlSegment { Expression = conditionalExpr.IfTrue }) :
                this.Visit(new SqlSegment { Expression = conditionalExpr.IfFalse });
        }

        var ifTrueSegment = this.Visit(new SqlSegment { Expression = conditionalExpr.IfTrue });
        var ifFalseSegment = this.Visit(new SqlSegment { Expression = conditionalExpr.IfFalse });
        string testValue = this.GetQuotedValue(sqlSegment);
        string leftValue = this.GetQuotedValue(ifTrueSegment);
        string rightValue = this.GetQuotedValue(ifFalseSegment);
        var strExpression = $"CASE WHEN {testValue} THEN {leftValue} ELSE {rightValue} END";
        return sqlSegment.Change(strExpression, SqlType.Expression);
    }
    public virtual SqlSegment VisitListInit(SqlSegment sqlSegment)
        => this.Evaluate(sqlSegment);
    public virtual SqlSegment VisitTypeIs(SqlSegment sqlSegment)
    {
        var binaryExpr = sqlSegment.Expression as TypeBinaryExpression;
        if (binaryExpr.TypeOperand == typeof(DBNull))
        {
            sqlSegment.Push(DeferredOperation.IsNull);
            return this.Visit(sqlSegment.Next(binaryExpr.Expression));
        }
        var visitor = new HasParameterVisitor();
        visitor.Visit(binaryExpr.Expression);
        if (!visitor.HasParameter)
        {
            var sqlType = visitor.HasVariable ? SqlType.Variable : SqlType.Constant;
            return sqlSegment.Change(binaryExpr.Type == binaryExpr.TypeOperand, sqlType);
        }
        throw new NotSupportedException($"不支持的表达式操作，{sqlSegment.Expression}");
    }
    public virtual T Evaluate<T>(Expression expr)
        => ValueEvalutor.Evaluate<T>(expr);
    public virtual object Evaluate(Expression expr)
        => ValueEvalutor.Evaluate(expr);
    public virtual SqlSegment VisitSqlMethodCall(SqlSegment sqlSegment)
    {
        var methodCallExpr = sqlSegment.Expression as MethodCallExpression;
        LambdaExpression lambdaExpr = null;
        switch (methodCallExpr.Method.Name)
        {
            case "Raw":
                sqlSegment.IsRawSqlFields = true;
                var rawSql = this.Evaluate<string>(methodCallExpr.Arguments[0]);
                var targetType = methodCallExpr.Method.GetGenericArguments()[0];
                if (targetType.IsEntityType(out _))
                {
                    if (this.IsSelectMember && methodCallExpr.Arguments.Count == 1)
                        throw new NotSupportedException("当返回类型为多字段时，Sql.Raw方法必须指定fieldsCount");
                    sqlSegment.Change(new ReaderField
                    {
                        FieldType = SqlFieldType.RawSql,
                        ReaderType = targetType,
                        Value = rawSql,
                        FieldsCount = this.Evaluate<int>(methodCallExpr.Arguments[1])
                    }, SqlType.ReaderField);
                }
                //单个字段的原始SQL当作函数处理
                else sqlSegment.Change(rawSql, SqlType.MethodCall);
                break;
            case "Deferred":
                sqlSegment.IsDeferredFields = true;
                sqlSegment = this.Visit(sqlSegment.Next(methodCallExpr.Arguments[0]));
                break;
            case "IsNull":
                if (methodCallExpr.Arguments.Count > 1)
                {
                    if (!this.OrmProvider.TryGetMethodCallSqlFormatter(methodCallExpr, out var sqlFormatter))
                        throw new NotImplementedException($"当前Provider:{this.OrmProvider.GetType().FullName}未实现方法IsNull");
                    sqlSegment = sqlFormatter.Invoke(this, methodCallExpr, null, null, methodCallExpr.Arguments.ToArray());
                }
                else
                {
                    sqlSegment = this.Visit(sqlSegment.Next(methodCallExpr.Arguments[0]));
                    sqlSegment.Change($"{this.GetQuotedValue(sqlSegment)} IS NULL", SqlType.Expression);
                }
                break;
            case "ToParameter":
                sqlSegment.IsParameterized = true;
                sqlSegment.ParameterName = this.Evaluate<string>(methodCallExpr.Arguments[1]);
                sqlSegment = this.Visit(sqlSegment.Next(methodCallExpr.Arguments[0]));
                sqlSegment.IsParameterized = false;
                break;
            case "In":
                var elementType = methodCallExpr.Method.GetGenericArguments()[0];
                var type = methodCallExpr.Arguments[1].Type;
                var fieldSegment = this.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                string inSql = null;
                if (type.IsArray || typeof(IEnumerable<>).MakeGenericType(elementType).IsAssignableFrom(type))
                {
                    var rightSegment = this.VisitAndDeferred(new SqlSegment { Expression = methodCallExpr.Arguments[1] });
                    if (rightSegment.IsNull)
                        return sqlSegment.Change("1=0");

                    var enumerable = rightSegment.Value as IEnumerable;
                    var builder = new StringBuilder();
                    foreach (var item in enumerable)
                    {
                        if (builder.Length > 0) builder.Append(',');
                        builder.Append(this.OrmProvider.GetQuotedValue(item));
                    }
                    inSql = builder.ToString();
                }
                else
                {
                    var predicateExpr = methodCallExpr.Arguments[1];
                    if (typeof(IQuery).IsAssignableFrom(type))
                    {
                        var funcType = typeof(Func<,>).MakeGenericType(typeof(IFromQuery), type);
                        var parameterExpr = Expression.Parameter(typeof(IFromQuery), "f");
                        predicateExpr = Expression.Lambda(funcType, methodCallExpr.Arguments[1], parameterExpr);
                    }
                    (inSql, _, _) = this.VisitFromQuery(predicateExpr);
                }
                var fieldArgument = this.GetQuotedValue(fieldSegment);
                if (sqlSegment.HasDeferredNot(out _))
                    sqlSegment.Change($"{fieldArgument} NOT IN ({inSql})", SqlType.Expression);
                else sqlSegment.Change($"{fieldArgument} IN ({inSql})", SqlType.Expression);
                break;
            case "Exists":
            case "ExistsAsync":
                string existsSql = null;
                if (methodCallExpr.Method.DeclaringType == typeof(Sql))
                {
                    var argsType = methodCallExpr.Arguments[0].Type;
                    var lastReturnType = argsType.GenericTypeArguments.Last();
                    if (lastReturnType == typeof(bool))
                    {
                        //Sql.Exists<T1, T2>(Expression<Func<T1, T2, bool>> predicate)                      
                        //保存现场，临时添加这几个新表及别名，解析之后再删除
                        var removeTables = new List<TableSegment>();
                        var builder = new StringBuilder("SELECT * FROM ");
                        int index = 0;
                        lambdaExpr = this.EnsureLambda(methodCallExpr.Arguments[0]);
                        var genericArguments = methodCallExpr.Method.GetGenericArguments();
                        foreach (var tableType in genericArguments)
                        {
                            var aliasName = lambdaExpr.Parameters[index].Name;
                            if (this.TableAliases.ContainsKey(aliasName))
                                continue;

                            var tableMapper = this.EntityMapProvider.GetEntityMap(tableType);
                            var tableSegment = new TableSegment
                            {
                                EntityType = tableType,
                                AliasName = aliasName,
                                Mapper = tableMapper
                            };
                            this.Tables.Add(tableSegment);
                            this.TableAliases.Add(aliasName, tableSegment);
                            removeTables.Add(tableSegment);
                            if (index > 0) builder.Append(',');
                            builder.Append(this.OrmProvider.GetTableName(tableMapper.TableName));
                            builder.Append($" {tableSegment.AliasName}");
                            index++;
                        }
                        builder.Append(" WHERE ");
                        builder.Append(this.VisitConditionExpr(lambdaExpr.Body, out _));
                        //恢复现场
                        removeTables.ForEach(f =>
                        {
                            this.Tables.Remove(f);
                            this.TableAliases.Remove(f.AliasName);
                        });
                        existsSql = builder.ToString();
                    }
                    else
                    {
                        var predicateExpr = methodCallExpr.Arguments[0];
                        if (typeof(IQuery).IsAssignableFrom(argsType))
                        {
                            //Exists<TTarget>(IQuery<TTarget> subQuery)
                            var funcType = typeof(Func<,>).MakeGenericType(typeof(IFromQuery), argsType);
                            var parameterExpr = Expression.Parameter(typeof(IFromQuery), "f");
                            predicateExpr = Expression.Lambda(funcType, methodCallExpr.Arguments[0], parameterExpr);
                        }
                        //Exists<TTarget>(Func<IFromQuery, IQuery<TTarget>> subQuery)
                        (existsSql, _, _) = this.VisitFromQuery(predicateExpr);
                    }
                }
                else if (methodCallExpr.TryGetParameters(out var parameters))
                {
                    //repository.Exists<TEntity>
                    //repository.ExistsAsync<TEntity>
                    lambdaExpr = Expression.Lambda(methodCallExpr, parameters);
                    (existsSql, _, _) = this.VisitFromQuery(lambdaExpr);
                }
                else throw new NotSupportedException("不支持的repository.Exists/ExistsAsync表达式访问");

                if (sqlSegment.HasDeferredNot(out _))
                    sqlSegment.Change($"NOT EXISTS({existsSql})", SqlType.Expression);
                else sqlSegment.Change($"EXISTS({existsSql})", SqlType.MethodCall);
                break;
            case "Count":
            case "LongCount":
                if (methodCallExpr.Arguments != null && methodCallExpr.Arguments.Count == 1)
                {
                    sqlSegment = this.Visit(sqlSegment.Next(methodCallExpr.Arguments[0]));
                    //COUNT有时候是常量*,1等
                    sqlSegment.Change($"COUNT({sqlSegment.Value})", SqlType.MethodCall);
                }
                else sqlSegment.Change("COUNT(1)", SqlType.MethodCall);
                sqlSegment.Change(new ReaderField
                {
                    FieldType = SqlFieldType.Field,
                    ReaderType = methodCallExpr.Type,
                    Value = sqlSegment.Value,
                    IsAggField = true,
                    AggFunc = "SUM"
                }, SqlType.ReaderField);
                this.HasAggFields = true;
                break;
            case "CountDistinct":
            case "LongCountDistinct":
                if (methodCallExpr.Arguments != null && methodCallExpr.Arguments.Count == 1)
                {
                    sqlSegment = this.Visit(sqlSegment.Next(methodCallExpr.Arguments[0]));
                    sqlSegment.Change($"COUNT(DISTINCT {sqlSegment.Value})", SqlType.MethodCall);
                }
                //TODO:已知bug，分表后，count(distinct)，这个聚合结果是不准确的
                sqlSegment.Change(new ReaderField
                {
                    FieldType = SqlFieldType.Field,
                    ReaderType = methodCallExpr.Type,
                    Value = sqlSegment.Value,
                    IsAggField = true,
                    AggFunc = "COUNT"
                }, SqlType.ReaderField);
                this.HasAggFields = true;
                break;
            case "Sum":
                sqlSegment = this.Visit(sqlSegment.Next(methodCallExpr.Arguments[0]));
                sqlSegment.Change(new ReaderField
                {
                    FieldType = SqlFieldType.Field,
                    ReaderType = methodCallExpr.Type,
                    Value = $"SUM({sqlSegment.Value})",
                    IsAggField = true,
                    AggFunc = "SUM"
                }, SqlType.ReaderField);
                this.HasAggFields = true;
                break;
            case "Avg":
                sqlSegment = this.Visit(sqlSegment.Next(methodCallExpr.Arguments[0]));
                //优先确定是否是多分表情况，多分表时Value值是两个字段，SUM(...),COUNT(...)，
                //最外层SELECT时，捞取IsAvgField=true的字段，再进行SUM(...)/COUNT(...)得到平均值，不是多分表时，直接Value值是AVG(...)
                //最外层SELECT时，
                if (this.IsManyShardingTables)
                {
                    List<ReaderField> readerFields = [new ReaderField
                    {
                        FieldType = SqlFieldType.Field,
                        ReaderType = methodCallExpr.Type,
                        Value = $"SUM({sqlSegment.Value})",
                        IsAggField = true,
                        IsAvgField = true,
                        AggFunc = "SUM"
                    },new ReaderField
                    {
                        FieldType = SqlFieldType.Field,
                        ReaderType = methodCallExpr.Type,
                        Value = $"COUNT({sqlSegment.Value})",
                        IsAggField = true,
                        IsAvgField = true,
                        AggFunc = "COUNT"
                    }];
                    sqlSegment.Change(readerFields, SqlType.ReaderFields);
                }
                else sqlSegment.Change(new ReaderField
                {
                    FieldType = SqlFieldType.Field,
                    ReaderType = methodCallExpr.Type,
                    Value = $"AVG({sqlSegment.Value})",
                    IsAggField = true,
                    AggFunc = "AVG"
                }, SqlType.ReaderField);
                this.HasAggFields = true;
                break;
            case "Max":
                sqlSegment = this.Visit(sqlSegment.Next(methodCallExpr.Arguments[0]));
                sqlSegment.Change(new ReaderField
                {
                    FieldType = SqlFieldType.Field,
                    ReaderType = methodCallExpr.Type,
                    Value = $"MAX({sqlSegment.Value})",
                    IsAggField = true,
                    AggFunc = "MAX"
                }, SqlType.ReaderField);
                this.HasAggFields = true;
                break;
            case "Min":
                sqlSegment = this.Visit(sqlSegment.Next(methodCallExpr.Arguments[0]));
                sqlSegment.Change(new ReaderField
                {
                    FieldType = SqlFieldType.Field,
                    ReaderType = methodCallExpr.Type,
                    Value = $"MIN({sqlSegment.Value})",
                    IsAggField = true,
                    AggFunc = "MIN"
                }, SqlType.ReaderField);
                this.HasAggFields = true;
                break;
        }
        return sqlSegment;
    }
    public virtual SqlSegment VisitOverMethodCall(SqlSegment sqlSegment)
    {
        var methodCallExpr = sqlSegment.Expression as MethodCallExpression;
        var currentExpr = methodCallExpr.Object;
        var callStack = new Stack<MethodCallExpression>();
        while (currentExpr is MethodCallExpression callExpr)
        {
            if (callExpr.Type == typeof(Sql))
                break;
            callStack.Push(callExpr);
            currentExpr = callExpr.Object;
        }
        bool hasPartitionBy = false;
        bool hasOrder = false;
        bool hasOver = false;
        var builder = new StringBuilder();
        while (callStack.TryPop(out methodCallExpr))
        {
            switch (methodCallExpr.Method.Name)
            {
                case "PartitionBy":
                    sqlSegment = this.Visit(sqlSegment.Next(methodCallExpr.Arguments[0]));
                    builder.Append($"PARTITION BY {sqlSegment.Value}");
                    hasPartitionBy = true;
                    break;
                case "OrderBy":
                    sqlSegment = this.Visit(sqlSegment.Next(methodCallExpr.Arguments[0]));
                    if (hasOrder) builder.Append(',');
                    else
                    {
                        if (hasPartitionBy) builder.Append(' ');
                        builder.Append("ORDER BY ");
                    }
                    if (this.ReaderFields != null && this.ReaderFields.Count > 0)
                    {
                        for (int i = 0; i < this.ReaderFields.Count; i++)
                        {
                            var readerField = this.ReaderFields[i];
                            if (i > 0) builder.Append(',');
                            var fieldName = readerField.Value;
                            //CTE表字段是常量/变量/字段名称，都有可能和声明的字段不一致，所以需要获取CTE表的声明字段
                            //body里面的值，是原始的值或是字段名
                            if (readerField.TableSegment != null && readerField.TableSegment.TableType == TableType.CteSelfRef)
                                fieldName = $"{readerField.TableSegment.AliasName}.{this.OrmProvider.GetFieldName(readerField.TargetMember.Name)}";
                            builder.Append(fieldName);
                        }
                        this.ReaderFields.Clear();
                    }
                    else builder.Append(sqlSegment.Value);
                    hasOrder = true;
                    break;
                case "OrderByDescending":
                    sqlSegment = this.Visit(sqlSegment.Next(methodCallExpr.Arguments[0]));
                    if (hasOrder) builder.Append(',');
                    else
                    {
                        if (hasPartitionBy) builder.Append(' ');
                        builder.Append("ORDER BY ");
                    }
                    if (this.ReaderFields != null && this.ReaderFields.Count > 0)
                    {
                        for (int i = 0; i < this.ReaderFields.Count; i++)
                        {
                            var readerField = this.ReaderFields[i];
                            if (i > 0) builder.Append(',');
                            var fieldName = readerField.Value;
                            //CTE表字段是常量/变量/字段名称，都有可能和声明的字段不一致，所以需要获取CTE表的声明字段
                            //body里面的值，是原始的值或是字段名
                            if (readerField.TableSegment != null && readerField.TableSegment.TableType == TableType.CteSelfRef)
                                fieldName = $"{readerField.TableSegment.AliasName}.{this.OrmProvider.GetFieldName(readerField.TargetMember.Name)}";
                            builder.Append($"{fieldName} DESC");
                        }
                        this.ReaderFields.Clear();
                    }
                    else builder.Append($"{sqlSegment.Value} DESC");
                    hasOrder = true;
                    break;
                case "Over":
                    builder.Append($" OVER(");
                    hasOver = true;
                    break;
                case "Rank":
                case "DenseRank":
                case "RowNumber":
                    builder.Append($"{methodCallExpr.Method.Name.ToUpper()}()");
                    break;
                case "LongRank":
                case "LongDenseRank":
                case "LongRowNumber":
                    builder.Append($"{methodCallExpr.Method.Name.Replace("Long", "").ToUpper()}()");
                    break;
                case "Count":
                case "LongCount":
                    if (methodCallExpr.Arguments != null && methodCallExpr.Arguments.Count == 1)
                    {
                        sqlSegment = this.Visit(sqlSegment.Next(methodCallExpr.Arguments[0]));
                        builder.Append($"COUNT({sqlSegment.Value})");
                    }
                    else builder.Append("COUNT(*)");
                    sqlSegment.IsAggField = true;
                    sqlSegment.AggFunc = "COUNT";
                    this.HasAggFields = true;
                    break;
                case "CountDistinct":
                case "LongCountDistinct":
                    sqlSegment = this.Visit(sqlSegment.Next(methodCallExpr.Arguments[0]));
                    builder.Append($"COUNT(DISTINCT {sqlSegment.Value})");
                    sqlSegment.IsAggField = true;
                    //TODO:已知bug，分表后，count(distinct)，这个聚合结果是不准确的
                    sqlSegment.AggFunc = "COUNT";
                    this.HasAggFields = true;
                    break;
                case "Sum":
                    if (this.IsWhere || sqlSegment.IsNullFields)
                    {
                        sqlSegment = this.Visit(sqlSegment.Next(methodCallExpr.Arguments[0]));
                        builder.Append($"SUM({sqlSegment.Value})");
                        sqlSegment.IsAggField = true;
                        sqlSegment.AggFunc = "SUM";
                        this.HasAggFields = true;
                    }
                    else
                    {
                        var myMethodInfo = IsNullMethodInfo.MakeGenericMethod(methodCallExpr.Type);
                        var nullValueExpr = Expression.Constant(Convert.ChangeType(0, methodCallExpr.Type), methodCallExpr.Type);
                        var isNullCallExpr = Expression.Call(myMethodInfo, methodCallExpr, nullValueExpr);
                        sqlSegment = this.Visit(sqlSegment.Next(isNullCallExpr));
                    }
                    break;
                case "Avg":
                    if (this.IsWhere || sqlSegment.IsNullFields)
                    {
                        sqlSegment = this.Visit(sqlSegment.Next(methodCallExpr.Arguments[0]));
                        builder.Append($"AVG({sqlSegment.Value})");
                        sqlSegment.IsAggField = true;
                        sqlSegment.AggFunc = "AVG";
                        this.HasAggFields = true;
                    }
                    else
                    {
                        var myMethodInfo = IsNullMethodInfo.MakeGenericMethod(methodCallExpr.Type);
                        var nullValueExpr = Expression.Constant(Convert.ChangeType(0, methodCallExpr.Type), methodCallExpr.Type);
                        var isNullCallExpr = Expression.Call(myMethodInfo, methodCallExpr, nullValueExpr);
                        sqlSegment = this.Visit(sqlSegment.Next(isNullCallExpr));
                    }
                    break;
                case "Max":
                    if (this.IsWhere || sqlSegment.IsNullFields)
                    {
                        sqlSegment = this.Visit(sqlSegment.Next(methodCallExpr.Arguments[0]));
                        builder.Append($"MAX({sqlSegment.Value})");
                        sqlSegment.IsAggField = true;
                        sqlSegment.AggFunc = "MAX";
                        this.HasAggFields = true;
                    }
                    else
                    {
                        var myMethodInfo = IsNullMethodInfo.MakeGenericMethod(methodCallExpr.Type);
                        var nullValueExpr = Expression.Constant(Convert.ChangeType(0, methodCallExpr.Type), methodCallExpr.Type);
                        var isNullCallExpr = Expression.Call(myMethodInfo, methodCallExpr, nullValueExpr);
                        sqlSegment = this.Visit(sqlSegment.Next(isNullCallExpr));
                    }
                    break;
                case "Min":
                    if (this.IsWhere || sqlSegment.IsNullFields)
                    {
                        sqlSegment = this.Visit(sqlSegment.Next(methodCallExpr.Arguments[0]));
                        builder.Append($"MIN({sqlSegment.Value})");
                        sqlSegment.IsAggField = true;
                        sqlSegment.AggFunc = "MIN";
                        this.HasAggFields = true;
                    }
                    else
                    {
                        var myMethodInfo = IsNullMethodInfo.MakeGenericMethod(methodCallExpr.Type);
                        var nullValueExpr = Expression.Constant(Convert.ChangeType(0, methodCallExpr.Type), methodCallExpr.Type);
                        var isNullCallExpr = Expression.Call(myMethodInfo, methodCallExpr, nullValueExpr);
                        sqlSegment = this.Visit(sqlSegment.Next(isNullCallExpr));
                    }
                    break;
            }
        }
        if (hasOver) builder.Append(')');
        var sql = builder.ToString();
        builder.Clear();
        return sqlSegment.Change(sql, SqlType.Expression);
    }
    public virtual SqlSegment VisitGroupConcatMethodCall(SqlSegment sqlSegment) => sqlSegment;
    public virtual SqlSegment VisitStringAggMethodCall(SqlSegment sqlSegment) => sqlSegment;
    public virtual string VisitConditionExpr(Expression conditionExpr, out OperationType operationType)
    {
        if (conditionExpr.NodeType == ExpressionType.AndAlso || conditionExpr.NodeType == ExpressionType.OrElse)
        {
            operationType = conditionExpr.NodeType == ExpressionType.AndAlso ? OperationType.And : OperationType.Or;
            var builder = new StringBuilder();
            this.VisitLogicBinaryExpr(builder, conditionExpr);
            return builder.ToString();
        }
        operationType = OperationType.None;
        var sqlSegment = this.VisitAndDeferred(this.CreateConditionSegment(conditionExpr));
        return sqlSegment.Value.ToString();
    }
    public virtual (string, TableSegment, List<SqlSegment>) VisitFromQuery(Expression lambdaExpr, ICteQuery selfQueryObj = null)
    {
        var myLambdaExpr = this.EnsureLambda(lambdaExpr);
        var currentExpr = myLambdaExpr.Body;
        var callStack = new Stack<MethodCallExpression>();

        IQuery subQueryObj = null;
        var queryVisitor = this.OrmProvider.NewQueryVisitor(this.DbContext, 'a', this.Command);
        if (this.IsWhere)
            queryVisitor.TableAsStart = (char)(this.TableAsStart + this.Tables.Count);
        while (true)
        {
            if (currentExpr is not MethodCallExpression callExpr
                || currentExpr.NodeType == ExpressionType.Parameter)
                break;
            callStack.Push(callExpr);
            currentExpr = callExpr.Object;
        }
        string sql = null;
        Type entityType = null;
        TableSegment tableSegment = null;
        List<SqlSegment> readyReaderFields = null;

        //引用现有子查询对象
        if (currentExpr.NodeType == ExpressionType.MemberAccess
            && typeof(IQuery).IsAssignableFrom(currentExpr.Type))
        {
            subQueryObj = this.Evaluate(currentExpr) as IQuery;
            if (callStack.Count == 0)
            {
                if (this.IsWhere)
                {
                    sql = subQueryObj.Visitor.BuildSql(false, out _);
                    return (sql, tableSegment, readyReaderFields);
                }

                //直接引用，无任何操作
                var targetType = currentExpr.Type.GenericTypeArguments[0];
                tableSegment = queryVisitor.UseQuery(targetType, subQueryObj, true);
                return (sql, tableSegment, readyReaderFields);
            }

            if (subQueryObj is ICteQuery cteQueryObj)
            {
                entityType = currentExpr.Type.GenericTypeArguments[0];
                var isCopyRefParameters = !(this.IsSecondUnion && this.IsRecursive);
                tableSegment = queryVisitor.UseQuery(entityType, subQueryObj, isCopyRefParameters);
                readyReaderFields = tableSegment.Fields;
            }
            else
            {
                //TODO:一些引用类型的拷贝是不对的，比如：排序，分组等不能引用，也需要拷贝，否则会更改之前的内容
                //如果子查询有排序、分组、分页，此处再次更改，则需要包装一下子查询，再做排序、分组、分页
                //如果子查询没有排序、分组、分页，则不需要包装子查询，直接继续做排序、分组、分页
                //如果是where条件，也不需要包装子查询，直接继续做where条件
                subQueryObj.Visitor.CloneTo(queryVisitor);
                if (subQueryObj.Visitor.Tables != null && subQueryObj.Visitor.Tables.Count > 0)
                {
                    queryVisitor.Tables ??= new();
                    subQueryObj.Visitor.Tables.ForEach(f => queryVisitor.Tables.Add(f));
                }
                if (subQueryObj.Visitor.ReaderFields != null && subQueryObj.Visitor.ReaderFields.Count > 0)
                {
                    readyReaderFields = new();
                    subQueryObj.Visitor.ReaderFields.ForEach(f => readyReaderFields.Add(f.Clone()));
                }
            }
        }

        string unionType = null;
        object[] fieldValues = null;
        while (callStack.TryPop(out var callExpr))
        {
            var methodInfo = callExpr.Method;
            var genericArguments = methodInfo.GetGenericArguments();
            LambdaExpression lambdaArgsExpr = null;
            switch (methodInfo.Name)
            {
                case "Use":
                    subQueryObj = this.Evaluate(callExpr.Arguments[0]) as IQuery;
                    if (subQueryObj is ICteQuery cteQueryObj)
                    {
                        entityType = callExpr.Type.GenericTypeArguments[0];
                        var isCopyRefParameters = !(this.IsSecondUnion && this.IsRecursive);
                        tableSegment = queryVisitor.UseQuery(entityType, subQueryObj, isCopyRefParameters);
                        if (callStack.Count == 0) return (sql, tableSegment, readyReaderFields);
                        readyReaderFields = tableSegment.Fields;
                    }
                    else
                    {
                        //如果子查询有排序、分组、分页，此处再次更改，则需要包装一下子查询，再做排序、分组、分页
                        //如果子查询没有排序、分组、分页，则不需要包装子查询，直接继续做排序、分组、分页
                        //如果是where条件，也不需要包装子查询，直接继续做where条件
                        if (callStack.Count > 0)
                        {
                            subQueryObj.Visitor.CloneTo(queryVisitor);
                            if (subQueryObj.Visitor.Tables != null && subQueryObj.Visitor.Tables.Count > 0)
                            {
                                queryVisitor.Tables ??= new();
                                subQueryObj.Visitor.Tables.ForEach(f => queryVisitor.Tables.Add(f));
                            }
                            if (subQueryObj.Visitor.ReaderFields != null && subQueryObj.Visitor.ReaderFields.Count > 0)
                            {
                                readyReaderFields = new();
                                subQueryObj.Visitor.ReaderFields.ForEach(f => readyReaderFields.Add(f.Clone()));
                            }
                        }
                        else
                        {
                            tableSegment = queryVisitor.UseQuery(entityType, subQueryObj, true);
                            return (sql, tableSegment, readyReaderFields);
                        }
                    }
                    break;
                case "UseTable":
                    entityType = methodInfo.DeclaringType.GetGenericArguments().Last();
                    var parameterInfos = methodInfo.GetParameters();
                    var tableNames = this.Evaluate<string[]>(callExpr.Arguments[0]);
                    queryVisitor.UseTable(TableShardingUsageMode.ReadOnly, false, tableNames);
                    break;
                case "UseTableMap":
                    var tableNameMapGetter = this.Evaluate<Func<string, string, string, string>>(callExpr.Arguments[0]);
                    queryVisitor.UseTableMap(TableShardingUsageMode.ReadOnly, false, tableNameMapGetter);
                    break;
                case "UseTableBy":
                    fieldValues = (object[])this.Evaluate(callExpr.Arguments[0]);
                    entityType = methodInfo.DeclaringType.GetGenericArguments().Last();
                    queryVisitor.UseTableBy(TableShardingUsageMode.ReadOnly, false, fieldValues);
                    break;
                case "UseTableByRange":
                    fieldValues = (object[])this.Evaluate(callExpr.Arguments[0]);
                    entityType = methodInfo.DeclaringType.GetGenericArguments().Last();
                    queryVisitor.UseTableByRange(TableShardingUsageMode.ReadOnly, false, fieldValues);
                    break;
                case "UseTableSchema":
                    queryVisitor.UseTableSchema(false, this.Evaluate<string>(callExpr.Arguments[0]));
                    break;
                case "From":
                    if (callExpr.Arguments.Count > 0)
                    {
                        var tableAsStart = this.Evaluate<char>(callExpr.Arguments[0]);
                        queryVisitor.From(tableAsStart, genericArguments);
                    }
                    else queryVisitor.AddTable(genericArguments);
                    break;
                case "WithTable":
                    if (callExpr.Arguments.Count > 0)
                    {
                        entityType = genericArguments[0];
                        if (typeof(IQuery).IsAssignableFrom(callExpr.Arguments[0].Type))
                            queryVisitor.UseQuery(entityType, this.Evaluate(callExpr.Arguments[0]) as IQuery, true);
                        else queryVisitor.UseNewQuery(entityType, callExpr.Arguments[0], false);
                    }
                    else queryVisitor.AddTable(genericArguments);
                    break;
                case "Union":
                case "UnionAll":
                    entityType = callExpr.Object.Type.GenericTypeArguments[0];
                    unionType = methodInfo.Name == "Union" ? " UNION" : " UNION ALL";
                    if (typeof(IQuery).IsAssignableFrom(callExpr.Arguments[0].Type))
                    {
                        var queryObj = this.Evaluate(callExpr.Arguments[0]) as IQuery;
                        queryVisitor.Union(unionType, entityType, queryObj);
                    }
                    else queryVisitor.Union(unionType, entityType, callExpr.Arguments[0]);
                    break;
                case "UnionRecursive":
                case "UnionAllRecursive":
                    entityType = callExpr.Object.Type.GenericTypeArguments[0];
                    unionType = methodInfo.Name == "UnionRecursive" ? " UNION" : " UNION ALL";
                    entityType = typeof(CteQuery<>).MakeGenericType(entityType);
                    cteQueryObj = RepositoryHelper.CreateInstance(entityType,
                        [typeof(DbContext), typeof(IQueryVisitor)], this.DbContext, queryVisitor) as ICteQuery;
                    queryVisitor.UnionRecursive(unionType, cteQueryObj, callExpr.Arguments[0]);
                    break;
                case "InnerJoin":
                case "LeftJoin":
                case "RightJoin":
                    var joinType = methodInfo.Name switch
                    {
                        "LeftJoin" => "LEFT JOIN",
                        "RightJoin" => "RIGHT JOIN",
                        _ => "INNER JOIN"
                    };
                    lambdaArgsExpr = this.EnsureLambda(callExpr.Arguments.Last());
                    queryVisitor.RefTableAliases = this.TableAliases;
                    if (genericArguments.Length > 0)
                    {
                        if (callExpr.Arguments.Count > 1)
                        {
                            if (typeof(IQuery).IsAssignableFrom(callExpr.Arguments[0].Type))
                            {
                                IQuery queryObj = null;
                                //如果是递归查询，且是第二个UNION，则使用CteQueryObj对象
                                if (callExpr.Arguments[0].NodeType == ExpressionType.Parameter
                                    && queryVisitor.IsRecursive && queryVisitor.IsSecondUnion)
                                    queryObj = selfQueryObj;
                                else queryObj = this.Evaluate(callExpr.Arguments[0]) as IQuery;
                                queryVisitor.Join(joinType, genericArguments[0], queryObj, lambdaArgsExpr);
                            }
                            else queryVisitor.Join(joinType, genericArguments[0], callExpr.Arguments[0], lambdaArgsExpr);
                        }
                        else queryVisitor.Join(joinType, genericArguments[0], lambdaArgsExpr);
                    }
                    else queryVisitor.Join(joinType, lambdaArgsExpr);
                    queryVisitor.RefTableAliases = null;
                    break;
                case "WhereBy":
                case "AndBy":
                    queryVisitor.AndBy(this.Evaluate(callExpr.Arguments[0]));
                    break;
                case "WhereById":
                case "AndById":
                    queryVisitor.AndById(this.Evaluate(callExpr.Arguments[0]));
                    break;
                case "WhereByIds":
                case "AndByIds":
                    queryVisitor.AndByIds(this.Evaluate(callExpr.Arguments[0]));
                    break;
                case "OrBy":
                    queryVisitor.OrBy(this.Evaluate(callExpr.Arguments[0]));
                    break;
                case "OrById":
                    queryVisitor.OrById(this.Evaluate(callExpr.Arguments[0]));
                    break;
                case "OrByIds":
                    queryVisitor.OrByIds(this.Evaluate(callExpr.Arguments[0]));
                    break;
                case "Where":
                case "And":
                case "Or":
                    if (callExpr.Arguments.Count > 1)
                    {
                        if (this.Evaluate<bool>(callExpr.Arguments[0]))
                            lambdaArgsExpr = this.EnsureLambda(callExpr.Arguments[1]);
                        else if (callExpr.Arguments.Count > 2) lambdaArgsExpr = this.EnsureLambda(callExpr.Arguments[2]);
                    }
                    else lambdaArgsExpr = this.EnsureLambda(callExpr.Arguments[0]);
                    if (lambdaArgsExpr != null)
                    {
                        queryVisitor.RefTableAliases = this.TableAliases;
                        switch (methodInfo.Name)
                        {
                            case "Where":
                            case "And": queryVisitor.And(lambdaArgsExpr); break;
                            case "Or": queryVisitor.Or(lambdaArgsExpr); break;
                        }
                        queryVisitor.RefTableAliases = null;
                    }
                    break;
                case "WherePredicate":
                case "AndPredicate":
                case "OrPredicate":
                    var builderType = callExpr.Arguments[0].Type.GenericTypeArguments[0];
                    var initializer = this.Evaluate(callExpr.Arguments[0]) as Delegate;
                    var builder = RepositoryHelper.CreateInstance(builderType);
                    var predicateExpr = initializer.DynamicInvoke(builder) as Expression;
                    lambdaArgsExpr = this.EnsureLambda(predicateExpr);
                    queryVisitor.RefTableAliases = this.TableAliases;
                    switch (methodInfo.Name)
                    {
                        case "WherePredicate":
                        case "AndPredicate": queryVisitor.And(lambdaArgsExpr); break;
                        case "OrPredicate": queryVisitor.Or(lambdaArgsExpr); break;
                    }
                    queryVisitor.RefTableAliases = null;
                    break;
                case "GroupBy":
                    lambdaArgsExpr = this.EnsureLambda(callExpr.Arguments[0]);
                    queryVisitor.GroupBy(lambdaArgsExpr);
                    break;
                case "Having":
                    if (callExpr.Arguments.Count > 1 && this.Evaluate<bool>(callExpr.Arguments[0]))
                        lambdaArgsExpr = this.EnsureLambda(callExpr.Arguments[1]);
                    else lambdaArgsExpr = this.EnsureLambda(callExpr.Arguments[0]);
                    queryVisitor.RefTableAliases = this.TableAliases;
                    queryVisitor.Having(lambdaArgsExpr);
                    queryVisitor.RefTableAliases = null;
                    break;
                case "OrderBy":
                    lambdaArgsExpr = this.EnsureLambda(callExpr.Arguments[0]);
                    queryVisitor.OrderBy("ASC", lambdaArgsExpr);
                    break;
                case "OrderByDescending":
                    lambdaArgsExpr = this.EnsureLambda(callExpr.Arguments[0]);
                    queryVisitor.OrderBy("DESC", lambdaArgsExpr);
                    break;
                case "Skip":
                    queryVisitor.Skip(this.Evaluate<int>(callExpr.Arguments[0]));
                    break;
                case "Take":
                    queryVisitor.Take(this.Evaluate<int>(callExpr.Arguments[0]));
                    break;
                case "Page":
                    queryVisitor.Page(this.Evaluate<int>(callExpr.Arguments[0]), this.Evaluate<int>(callExpr.Arguments[1]));
                    break;
                case "Select":
                    if (callExpr.Arguments.Count > 0)
                    {
                        if (callExpr.Arguments[0].Type == typeof(string))
                            queryVisitor.Select(this.Evaluate<string>(callExpr.Arguments[0]));
                        else
                        {
                            lambdaArgsExpr = this.EnsureLambda(callExpr.Arguments[0]);
                            queryVisitor.Select(null, lambdaArgsExpr);
                        }
                    }
                    else
                    {
                        if (methodInfo.DeclaringType.FullName.StartsWith("Trolley.IGroupingQueryBase"))
                            queryVisitor.SelectGrouping();
                        else
                        {
                            //Expression<Func<T, T>> defaultExpr = f => f;
                            //this.Visitor.Select(null, defaultExpr);
                            var declaringTypeGenericArguments = methodInfo.DeclaringType.GetGenericArguments();
                            var genericType = declaringTypeGenericArguments[0];
                            var funcType = typeof(Func<,>).MakeGenericType(genericType, genericType);
                            var parameterExpr = Expression.Parameter(genericType, "f");
                            var defaultExpr = Expression.Lambda(funcType, parameterExpr, parameterExpr);
                            lambdaArgsExpr = this.EnsureLambda(defaultExpr);
                            queryVisitor.Select(null, lambdaArgsExpr);
                        }
                    }
                    break;
                case "SelectAggregate":
                    lambdaArgsExpr = this.EnsureLambda(callExpr.Arguments[0]);
                    queryVisitor.Select(null, lambdaArgsExpr);
                    break;
                case "SelectTo":
                    if (callExpr.Arguments.Count > 0)
                        lambdaArgsExpr = this.EnsureLambda(callExpr.Arguments[0]);
                    queryVisitor.SelectTo(genericArguments[0], lambdaArgsExpr);
                    break;
                case "Distinct":
                    queryVisitor.Distinct();
                    break;
                case "ExistsBy":
                    queryVisitor.OrBy(this.Evaluate(callExpr.Arguments[0]));
                    break;
                case "ExistsById":
                    queryVisitor.OrById(this.Evaluate(callExpr.Arguments[0]));
                    break;
                case "ExistsByIds":
                    queryVisitor.OrByIds(this.Evaluate(callExpr.Arguments[0]));
                    break;
                case "Exists":
                case "ExistsAsync":
                    //repository.Exists<TEntity>(object whereObj)此场景，在前面已经报错了，不会进来
                    //repository.ExistsAsync<TEntity>(Expression<Func<TEntity, bool>> wherePredicate, CancellationToken cancellationToken = default)
                    if (callExpr.Arguments.Count > 0)
                    {
                        lambdaArgsExpr = this.EnsureLambda(callExpr.Arguments[0]);
                        queryVisitor.From(queryVisitor.TableAsStart, genericArguments);
                        queryVisitor.RefTableAliases = this.TableAliases;
                        queryVisitor.And(lambdaArgsExpr);
                        queryVisitor.RefTableAliases = null;
                        queryVisitor.Select("*");
                    }
                    //repository.From<Company>('b').Where(t => ...).Exists()
                    //此场景什么也不做
                    break;
                case "AsCteTable":
                    //TODO: 当前visitor添加该CTE子查询表引用，并生成CTE子查询表的引用的SQL
                    var cteTableName = this.Evaluate<string>(callExpr.Arguments[0]);
                    entityType = callExpr.Type.GenericTypeArguments[0];
                    queryVisitor.AsCteTable(entityType, cteTableName);
                    queryVisitor.Clear();
                    queryVisitor.Tables.Clear();
                    tableSegment = this.UseQuery(entityType, queryVisitor.CteQueryObj, true);
                    return (sql, tableSegment, readyReaderFields);
                default: throw new NotSupportedException("不支持的表达式解析");
            }
        }
        if (queryVisitor.ReaderFields == null || queryVisitor.ReaderFields.Count == 0)
        {
            if (this.IsWhere) queryVisitor.Select("*");
            else queryVisitor.ReaderFields = readyReaderFields;
        }
        sql = queryVisitor.BuildSql(false, out var readerFields);
        return (sql, tableSegment, readerFields);
    }
    public virtual string GetQuotedValue(SqlSegment sqlSegment, bool isNeedExprWrap = false)
    {
        if (isNeedExprWrap && sqlSegment.SqlType == SqlType.Expression)
            return $"({sqlSegment.Value})";
        if (sqlSegment.SqlType > SqlType.Variable)
            return sqlSegment.Value.ToString();

        if (sqlSegment.SqlType == SqlType.Constant && !sqlSegment.IsParameterized)
            return sqlSegment.Value.ToString();

        if (sqlSegment.IsFixedValue)
        {
            if (sqlSegment.IsTrue)
                return this.OrmProvider.GetQuotedValue(typeof(bool), sqlSegment.Value);
            return sqlSegment.Value.ToString();
        }
        //下面是处理参数
        var dbParameters = this.DbParameters;
        if (this.IsIncludeMany)
        {
            this.NextDbParameters ??= new TheaDbParameterCollection();
            dbParameters = this.NextDbParameters;
        }
        var parameterName = sqlSegment.ParameterName ?? this.OrmProvider.ParameterPrefix + this.UserParameterPrefix + dbParameters.Count.ToString();
        var dbFieldValue = sqlSegment.Value;
        if (sqlSegment.MemberMapper != null)
        {
            var memberMapper = sqlSegment.MemberMapper;
            if (memberMapper.TypeHandler != null)
            {
                //枚举类型或是有强制转换时，要取sqlSegment.ExpectType值
                //常量、方法调用、计算表达式时，sqlSegment.FromMember没有值，只能取Expression.Type值
                dbFieldValue = memberMapper.TypeHandler.ToFieldValue(dbFieldValue);
                dbParameters.Add(this.OrmProvider.CreateParameter(parameterName, memberMapper.NativeDbType, dbFieldValue));
            }
            else
            {
                if (memberMapper.UnderlyingType.IsEnum)
                    dbFieldValue = Enum.ToObject(memberMapper.UnderlyingType, dbFieldValue);
                var targetType = memberMapper.MappedTargetType;
                var segmentType = dbFieldValue.GetType();
                if (segmentType != targetType)
                {
                    var valueGetter = this.OrmProvider.GetParameterValueGetter(segmentType, targetType, false, this.DbContext.Options);
                    dbFieldValue = valueGetter.Invoke(dbFieldValue);
                }
                dbParameters.Add(this.OrmProvider.CreateParameter(parameterName, memberMapper.NativeDbType, dbFieldValue));
            }
        }
        else dbParameters.Add(this.OrmProvider.CreateParameter(parameterName, dbFieldValue));
        return parameterName;
    }
    //public virtual string GetQuotedValue(SqlSegment sqlSegment, bool isNeedExprWrap = false)
    //{
    //    //默认只要是变量就设置为参数
    //    if (sqlSegment.IsVariable || (this.IsConstantParameterized || sqlSegment.IsParameterized) && sqlSegment.IsConstant)
    //    {
    //        var dbParameters = this.DbParameters;
    //        if (this.IsIncludeMany)
    //        {
    //            this.NextDbParameters ??= new TheaDbParameterCollection();
    //            dbParameters = this.NextDbParameters;
    //        }
    //        var parameterName = sqlSegment.ParameterName ?? this.OrmProvider.ParameterPrefix + this.UserParameterPrefix + dbParameters.Count.ToString();

    //        if (sqlSegment.Value == null || sqlSegment.Value == DBNull.Value)
    //            dbParameters.Add(this.OrmProvider.CreateParameter(parameterName, DBNull.Value));
    //        else
    //        {
    //            var dbFieldValue = sqlSegment.Value;
    //            if (sqlSegment.TypeHandler != null)
    //            {
    //                //枚举类型或是有强制转换时，要取sqlSegment.ExpectType值
    //                //常量、方法调用、计算表达式时，sqlSegment.FromMember没有值，只能取Expression.Type值
    //                dbFieldValue = sqlSegment.TypeHandler.ToFieldValue(dbFieldValue);
    //                if (sqlSegment.NativeDbType != null)
    //                    dbParameters.Add(this.OrmProvider.CreateParameter(parameterName, sqlSegment.NativeDbType, dbFieldValue));
    //                else dbParameters.Add(this.OrmProvider.CreateParameter(parameterName, dbFieldValue));
    //            }
    //            else
    //            {
    //                if (sqlSegment.ExpectType != null && sqlSegment.SegmentType != sqlSegment.ExpectType)
    //                {
    //                    if (sqlSegment.ExpectType.IsEnum)
    //                        dbFieldValue = Enum.ToObject(sqlSegment.ExpectType, dbFieldValue);
    //                    else dbFieldValue = Convert.ChangeType(dbFieldValue, sqlSegment.ExpectType);
    //                    sqlSegment.SegmentType = sqlSegment.ExpectType;
    //                }
    //                if (sqlSegment.NativeDbType != null)
    //                {
    //                    var targetType = sqlSegment.MappedTargetType;
    //                    if (sqlSegment.SegmentType != targetType)
    //                    {
    //                        var valueGetter = this.OrmProvider.GetParameterValueGetter(sqlSegment.SegmentType, targetType, false, this.DbContext.Options);
    //                        dbFieldValue = valueGetter.Invoke(dbFieldValue);
    //                        sqlSegment.SegmentType = targetType;
    //                    }
    //                    dbParameters.Add(this.OrmProvider.CreateParameter(parameterName, sqlSegment.NativeDbType, dbFieldValue));
    //                }
    //                else dbParameters.Add(this.OrmProvider.CreateParameter(parameterName, dbFieldValue));
    //            }
    //        }
    //        //清空指定的参数化名称
    //        if (sqlSegment.IsParameterized)
    //        {
    //            sqlSegment.ParameterName = null;
    //            sqlSegment.IsParameterized = false;
    //        }

    //        sqlSegment.Body = parameterName;
    //        sqlSegment.HasParameter = true;
    //        sqlSegment.IsVariable = false;
    //        sqlSegment.IsConstant = false;
    //        return parameterName;
    //    }
    //    else if (sqlSegment.IsConstant)
    //    {
    //        var dbFieldValue = sqlSegment.Value;
    //        //TODO: 常量和变量不应该放在这里处理
    //        if (dbFieldValue is string strFieldValue && strFieldValue == "*")
    //        {
    //            sqlSegment.Body = strFieldValue;
    //            return sqlSegment.Body;
    //        }
    //        string body = null;
    //        if (sqlSegment.TypeHandler != null)
    //            body = sqlSegment.TypeHandler.GetQuotedValue(dbFieldValue);
    //        else
    //        {
    //            if (sqlSegment.ExpectType != null && sqlSegment.SegmentType != sqlSegment.ExpectType)
    //            {
    //                if (sqlSegment.ExpectType.IsEnum)
    //                    dbFieldValue = Enum.ToObject(sqlSegment.ExpectType, dbFieldValue);
    //                else dbFieldValue = Convert.ChangeType(dbFieldValue, sqlSegment.ExpectType);
    //                sqlSegment.SegmentType = sqlSegment.ExpectType;
    //            }
    //            var targetType = sqlSegment.SegmentType;
    //            if (sqlSegment.NativeDbType != null)
    //            {
    //                targetType = sqlSegment.MappedTargetType;
    //                if (sqlSegment.SegmentType != targetType)
    //                {
    //                    var valueGetter = this.OrmProvider.GetParameterValueGetter(sqlSegment.SegmentType, targetType, false, this.DbContext.Options);
    //                    dbFieldValue = valueGetter.Invoke(dbFieldValue);
    //                }
    //            }
    //            //枚举类型常量，无法确定数据库是什么类型，取默认配置类型，通常是SELECT场景
    //            else if (targetType.IsEnum)
    //            {
    //                targetType = this.DbContext.Options.DefaultEnumMapDbType;
    //                dbFieldValue = Convert.ChangeType(dbFieldValue, targetType);
    //            }
    //            body = this.OrmProvider.GetQuotedValue(targetType, dbFieldValue);
    //        }
    //        sqlSegment.Body = body;
    //        return body;
    //    }
    //    //带有参数或字段的表达式或函数调用、或是只有参数或字段
    //    //本地函数调用返回值，非常量、变量、字段、SQL函数调用
    //    if (isNeedExprWrap && sqlSegment.IsExpression)
    //    {
    //        sqlSegment.Body = $"({sqlSegment.Body})";
    //        sqlSegment.IsExpression = false;
    //        sqlSegment.IsMethodCall = true;
    //        return sqlSegment.Body;
    //    }
    //    return sqlSegment.Body;
    //}
    /// <summary>
    /// 在已知需要的类型，获取原值，只能是常量或变量，不做参数化处理，后需要运算操作，操作后再做参数化处理
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="sqlSegment"></param>
    /// <returns></returns>
    public virtual TValue GetQuotedValue<TValue>(SqlSegment sqlSegment)
    {
        if (sqlSegment.Value == null || sqlSegment.Value == DBNull.Value)
            return default(TValue);

        var expectType = typeof(TValue);
        var dbFieldValue = sqlSegment.Value;
        if (sqlSegment.TypeHandler != null)
            dbFieldValue = sqlSegment.TypeHandler.ToFieldValue(dbFieldValue);
        else
        {
            if (expectType.IsEnum && !sqlSegment.SegmentType.IsEnum)
                dbFieldValue = Enum.ToObject(expectType, dbFieldValue);
            if (!expectType.IsEnum && sqlSegment.SegmentType.IsEnum)
            {
                var underlyingType = Enum.GetUnderlyingType(sqlSegment.SegmentType);
                dbFieldValue = Convert.ChangeType(dbFieldValue, underlyingType);
            }
        }
        return (TValue)Convert.ChangeType(dbFieldValue, expectType);
    }
    public virtual string GetQuotedValue(object elementValue, SqlSegment arraySegment, SqlSegment elementSegment)
    {
        if (elementValue is DBNull || elementValue == null)
            return "NULL";
        if (arraySegment.IsVariable || (this.IsConstantParameterized || arraySegment.IsParameterized) && arraySegment.IsConstant)
        {
            var dbParameters = this.DbParameters;
            if (this.IsIncludeMany)
            {
                this.NextDbParameters ??= new TheaDbParameterCollection();
                dbParameters = this.NextDbParameters;
            }
            var parameterName = this.OrmProvider.ParameterPrefix + this.UserParameterPrefix + dbParameters.Count.ToString();

            if (elementValue == null || elementValue == DBNull.Value)
                dbParameters.Add(this.OrmProvider.CreateParameter(parameterName, DBNull.Value));
            else
            {
                var dbFieldValue = elementValue;
                var expectType = elementSegment.ExpectType;
                var segmentType = elementSegment.SegmentType;
                var nativeDbType = elementSegment.NativeDbType;
                var typeHandler = elementSegment.TypeHandler;

                if (typeHandler != null)
                {
                    dbFieldValue = typeHandler.ToFieldValue(dbFieldValue);
                    if (nativeDbType != null)
                        dbParameters.Add(this.OrmProvider.CreateParameter(parameterName, nativeDbType, dbFieldValue));
                    else dbParameters.Add(this.OrmProvider.CreateParameter(parameterName, dbFieldValue));
                }
                else
                {
                    if (expectType != null && segmentType != expectType)
                    {
                        dbFieldValue = Enum.ToObject(expectType, dbFieldValue);
                        segmentType = expectType;
                    }
                    if (nativeDbType != null)
                    {
                        var targetType = elementSegment.MappedTargetType;
                        if (segmentType != targetType)
                        {
                            var valueGetter = this.OrmProvider.GetParameterValueGetter(segmentType, targetType, false, this.DbContext.Options);
                            dbFieldValue = valueGetter.Invoke(dbFieldValue);
                        }
                        dbParameters.Add(this.OrmProvider.CreateParameter(parameterName, nativeDbType, dbFieldValue));
                    }
                    else dbParameters.Add(this.OrmProvider.CreateParameter(parameterName, dbFieldValue));
                }
            }
            return parameterName;
        }
        if (arraySegment.IsConstant)
        {
            var dbFieldValue = elementValue;
            var expectType = elementSegment.ExpectType;
            var segmentType = elementSegment.SegmentType;
            var nativeDbType = elementSegment.NativeDbType;
            var typeHandler = elementSegment.TypeHandler;

            if (typeHandler != null)
                return typeHandler.GetQuotedValue(dbFieldValue);
            else
            {
                if (expectType != null && segmentType != expectType)
                {
                    dbFieldValue = Enum.ToObject(expectType, dbFieldValue);
                    segmentType = expectType;
                }
                var targetType = segmentType;
                if (nativeDbType != null)
                {
                    targetType = elementSegment.MappedTargetType;
                    if (segmentType != targetType)
                    {
                        var valueGetter = this.OrmProvider.GetParameterValueGetter(segmentType, targetType, false, this.DbContext.Options);
                        dbFieldValue = valueGetter.Invoke(dbFieldValue);
                    }
                }
                //枚举类型常量，无法确定数据库是什么类型，取默认配置类型，通常是SELECT场景
                else if (targetType.IsEnum)
                {
                    targetType = this.DbContext.Options.DefaultEnumMapDbType;
                    dbFieldValue = Convert.ChangeType(dbFieldValue, targetType);
                }
                return this.OrmProvider.GetQuotedValue(targetType, dbFieldValue);
            }
        }
        //此场景走不到，通常是常量和变量
        return this.OrmProvider.GetQuotedValue(elementValue);
    }
    public virtual string ChangeParameterValue(SqlSegment sqlSegment, Type targetType)
    {
        var dbParameter = this.DbParameters[sqlSegment.Body] as IDbDataParameter;
        this.OrmProvider.ChangeParameter(dbParameter, targetType, sqlSegment.Value);
        return sqlSegment.Body;
    }
    public virtual IQueryVisitor CreateQueryVisitor(char? tableAsStart = null)
    {
        //Union的时候，tableAsStart会传入'a'，表示从'a'开始
        //Join的时候，tableAsStart不传值，使用当前Visitor中的
        var queryVisitor = this.OrmProvider.NewQueryVisitor(this.DbContext, tableAsStart ?? this.TableAsStart, this.Command);
        queryVisitor.RefQueries = this.RefQueries;
        queryVisitor.ShardingTables = this.ShardingTables;
        queryVisitor.RefTableAliases = this.RefTableAliases;
        queryVisitor.IncludeTables = this.IncludeTables;
        queryVisitor.NextDbParameters = this.NextDbParameters;
        queryVisitor.IsRecursive = this.IsRecursive;
        queryVisitor.CteQueryObj = this.CteQueryObj;
        queryVisitor.RefFrom = this;
        return queryVisitor;
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
            string leftArgument = sqlSegment.Body;
            if (sqlSegment.IsConstant || sqlSegment.IsVariable)
                leftArgument = this.GetQuotedValue(sqlSegment);
            if (deferredSegment == SqlSegment.Null)
                strExpression = $"{leftArgument} {strOperator} {deferredSegment.Body}";
            else strExpression = $"{leftArgument}{strOperator}{this.OrmProvider.GetQuotedValue(typeof(bool), deferredSegment.Value)}";
        }
        else strExpression = sqlSegment.Body;
        if (this.IsSelect || (this.IsWhere && !isExpectBooleanType))
            strExpression = $"CASE WHEN {strExpression} THEN {ifTrueValue} ELSE {ifFalseValue} END";
        return sqlSegment.Change(strExpression);
    }
    public List<ReaderField> FlattenTableFields(TableSegment tableSegment, bool isNeedAlias = true)
    {
        var targetFields = new List<ReaderField>();
        if (tableSegment.Mapper != null)
        {
            //Select参数时，Flatten实体表
            foreach (var memberMapper in tableSegment.Mapper.MemberMaps)
            {
                if (memberMapper.IsIgnore || memberMapper.IsNavigation)
                    continue;
                var fieldName = this.OrmProvider.GetFieldName(memberMapper.FieldName);
                if (isNeedAlias) fieldName = tableSegment.AliasName + "." + fieldName;
                targetFields.Add(new ReaderField
                {
                    FieldType = SqlFieldType.Field,
                    TableSegment = tableSegment,
                    ReaderType = memberMapper.MemberType,
                    FieldName = memberMapper.FieldName,
                    TypeHandler = memberMapper.TypeHandler,
                    TargetMember = memberMapper.Member,
                    Value = fieldName
                });
            }
        }
        //Select参数时，Flatten子查询表
        else targetFields.AddRange(tableSegment.Fields);
        return targetFields;
    }
    public virtual bool IsStringConcatOperator(SqlSegment sqlSegment, BinaryExpression binaryExpr, out SqlSegment result)
    {
        if (binaryExpr.NodeType == ExpressionType.Add && (binaryExpr.Left.Type == typeof(string) || binaryExpr.Right.Type == typeof(string)))
        {
            //调用拼接方法Concat,每个数据库Provider都实现了这个方法
            var methodInfo = typeof(string).GetMethod(nameof(string.Concat), [typeof(object), typeof(object)]);
            var methodCallExpr = Expression.Call(methodInfo, binaryExpr.Left, binaryExpr.Right);
            sqlSegment.Expression = methodCallExpr;
            this.OrmProvider.TryGetMethodCallSqlFormatter(methodCallExpr, out var formater);
            //返回的SQL表达式中直接拼接好
            result = formater.Invoke(this, methodCallExpr, null, null, binaryExpr.Left, binaryExpr.Right);
            return true;
        }
        result = null;
        return false;
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
    public bool IsDateTimeOperator(SqlSegment sqlSegment, BinaryExpression binaryExpr, out SqlSegment result)
    {
        if (binaryExpr.Left.Type != typeof(DateTime))
        {
            result = default;
            return false;
        }
        if (binaryExpr.NodeType == ExpressionType.Add)
        {
            var methodInfo = typeof(DateTime).GetMethod(nameof(DateTime.Add), [binaryExpr.Right.Type]);
            var operatorExpr = Expression.Call(binaryExpr.Left, methodInfo, binaryExpr.Right);
            result = this.VisitMethodCall(sqlSegment.Next(operatorExpr));
            return true;
        }
        if (binaryExpr.NodeType == ExpressionType.Subtract)
        {
            var methodInfo = typeof(DateTime).GetMethod(nameof(DateTime.Subtract), [binaryExpr.Right.Type]);
            var operatorExpr = Expression.Call(binaryExpr.Left, methodInfo, binaryExpr.Right);
            result = this.VisitMethodCall(sqlSegment.Next(operatorExpr));
            return true;
        }
        result = default;
        return false;
    }
    public bool IsTimeSpanOperator(SqlSegment sqlSegment, BinaryExpression binaryExpr, out SqlSegment result)
    {
        if (binaryExpr.Left.Type != typeof(TimeSpan))
        {
            result = default;
            return false;
        }

        if (binaryExpr.NodeType == ExpressionType.Add)
        {
            var methodInfo = typeof(TimeSpan).GetMethod(nameof(TimeSpan.Add), [binaryExpr.Right.Type]);
            var operatorExpr = Expression.Call(binaryExpr.Left, methodInfo, binaryExpr.Right);
            result = this.VisitMethodCall(sqlSegment.Next(operatorExpr));
            return true;
        }
        if (binaryExpr.NodeType == ExpressionType.Subtract)
        {
            var methodInfo = typeof(TimeSpan).GetMethod(nameof(TimeSpan.Subtract), [binaryExpr.Right.Type]);
            var operatorExpr = Expression.Call(binaryExpr.Left, methodInfo, binaryExpr.Right);
            result = this.VisitMethodCall(sqlSegment.Next(operatorExpr));
            return true;
        }
#if NETCOREAPP2_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        if (binaryExpr.NodeType == ExpressionType.Multiply)
        {
            var rightExpr = binaryExpr.Right;
            if (binaryExpr.Right.Type != typeof(double))
                rightExpr = Expression.Convert(rightExpr, typeof(double));
            var methodInfo = typeof(TimeSpan).GetMethod(nameof(TimeSpan.Multiply), [typeof(double)]);
            var operatorExpr = Expression.Call(binaryExpr.Left, methodInfo, rightExpr);
            result = this.VisitMethodCall(sqlSegment.Next(operatorExpr));
            return true;
        }
        if (binaryExpr.NodeType == ExpressionType.Divide)
        {
            Type divideType = null;
            if (binaryExpr.Right.Type == typeof(TimeSpan))
                divideType = typeof(TimeSpan);
            else divideType = typeof(double);
            var methodInfo = typeof(TimeSpan).GetMethod(nameof(TimeSpan.Divide), [divideType]);
            var rightExpr = binaryExpr.Right;
            if (divideType == typeof(double) && binaryExpr.Right.Type != typeof(double))
                rightExpr = Expression.Convert(rightExpr, typeof(double));
            var operatorExpr = Expression.Call(binaryExpr.Left, methodInfo, rightExpr);
            result = this.VisitMethodCall(sqlSegment.Next(operatorExpr));
            return true;
        }
#endif
        result = default;
        return false;
    }
    public void Swap<T>(ref T left, ref T right) => (left, right) = (right, left);
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
            else throw new NotSupportedException($"不支持的表达式解析:{currentExpr}");
        }
        return currentExpr as LambdaExpression;
    }
    public bool IsGroupingMember(MemberExpression memberExpr)
    {
        if (memberExpr == null) return false;
        return memberExpr.Member.Name == "Grouping" && memberExpr.Member.DeclaringType.FullName.StartsWith("Trolley.IGroupingObject");
    }
    public List<ICteQuery> FlattenRefCteTables(List<IQuery> cteQueries)
    {
        var result = new List<ICteQuery>();
        AddRefCteTables(result, cteQueries);
        return result;
    }
    private void AddRefCteTables(List<ICteQuery> result, List<IQuery> fromCteQueries)
    {
        foreach (var subQueryObj in fromCteQueries)
        {
            if (subQueryObj.Visitor.RefQueries.Count > 0 && !fromCteQueries.Equals(subQueryObj.Visitor.RefQueries))
                this.AddRefCteTables(result, subQueryObj.Visitor.RefQueries);
            if (!result.Contains(subQueryObj) && subQueryObj is ICteQuery cteQueryObj)
                result.Add(cteQueryObj);
        }
    }
    public DataTable ToDataTable(string tableName, IEnumerable entities, List<MemberMap> memberMappers, List<Func<object, object>> valueGetters)
    {
        var result = new DataTable(tableName);
        foreach (var memberMapper in memberMappers)
            result.Columns.Add(memberMapper.FieldName, memberMapper.MappedTargetType);
        foreach (var entity in entities)
        {
            var row = new object[memberMappers.Count];
            for (var i = 0; i < valueGetters.Count; i++)
            {
                row[i] = valueGetters[i].Invoke(entity);
            }
            result.Rows.Add(row);
        }
        return result;
    }
    public (List<MemberMap>, List<Func<object, object>>) GetRefMemberMappers(Type parameterType, EntityMap entityMapper, object parameterSample, bool isUpdate)
    {
        var memberMappers = new List<MemberMap>();
        var valueGetters = new List<Func<object, object>>();
        if (parameterSample is IDictionary<string, object> dict)
        {
            foreach (var memberMapper in entityMapper.MemberMaps)
            {
                if (memberMapper.IsIgnore || memberMapper.IsNavigation
                    || memberMapper.IsAutoIncrement || memberMapper.IsRowVersion)
                    continue;
                if (!isUpdate && memberMapper.IsIgnoreInsert) continue;
                if (isUpdate && memberMapper.IsIgnoreUpdate) continue;

                Func<object, object> valueGetter = null;
                if (!dict.TryGetKeyIgnoreCase(memberMapper.MemberName, out var itemKey))
                {
                    if (isUpdate) continue;
                    else valueGetter = value => memberMapper.DefaultValue;
                }
                else
                {
                    if (memberMapper.TypeHandler != null) valueGetter = value =>
                    {
                        var dict = value as IDictionary<string, object>;
                        return memberMapper.TypeHandler.ToFieldValue(dict[itemKey]);
                    };
                    else
                    {
                        Func<object, object> typedValueGetter = null;
                        var fieldValue = dict[itemKey];
                        if (fieldValue == null)
                        {
                            if (memberMapper.IsRequired)
                                throw new InvalidOperationException($"参数字典中成员{memberMapper.MemberName}对应的值不能为空");

                            typedValueGetter = this.OrmProvider.GetParameterValueGetter(
                                fieldValue.GetType(), memberMapper.MappedTargetType, true, this.DbContext.Options);
                            valueGetter = value =>
                            {
                                var dict = value as IDictionary<string, object>;
                                var fieldValue = dict[itemKey];
                                if (fieldValue == null) return DBNull.Value;
                                return typedValueGetter.Invoke(fieldValue);
                            };
                        }
                        else
                        {
                            typedValueGetter = this.OrmProvider.GetParameterValueGetter(dict[itemKey].GetType(),
                                memberMapper.MappedTargetType, !memberMapper.IsRequired, this.DbContext.Options);
                            valueGetter = value =>
                            {
                                var dict = value as IDictionary<string, object>;
                                return typedValueGetter.Invoke(dict[itemKey]);
                            };
                        }
                    }
                }

                memberMappers.Add(memberMapper);
                valueGetters.Add(valueGetter);
            }
        }
        else
        {
            var memberInfos = RepositoryHelper.GetMembers(parameterType);
            foreach (var memberMapper in entityMapper.MemberMaps)
            {
                if (memberMapper.IsIgnore || memberMapper.IsNavigation
                    || memberMapper.IsAutoIncrement || memberMapper.IsRowVersion)
                    continue;
                if (!isUpdate && memberMapper.IsIgnoreInsert) continue;
                if (isUpdate && memberMapper.IsIgnoreUpdate) continue;

                Func<object, object> valueGetter = null;

                if (!memberInfos.TryFind(memberMapper.MemberName, out var memberInfo))
                {
                    if (isUpdate) continue;
                    else valueGetter = value => memberMapper.DefaultValue;
                }
                else
                {
                    if (memberMapper.TypeHandler != null) valueGetter = value =>
                    {
                        var fieldValue = memberInfo.Evaluate(value);
                        return memberMapper.TypeHandler.ToFieldValue(fieldValue);
                    };
                    else
                    {
                        Func<object, object> typedValueGetter = null;
                        typedValueGetter = this.OrmProvider.GetParameterValueGetter(memberInfo.GetMemberType(),
                            memberMapper.MappedTargetType, !memberMapper.IsRequired, this.DbContext.Options);
                        valueGetter = value =>
                        {
                            var fieldValue = memberInfo.Evaluate(value);
                            return typedValueGetter.Invoke(fieldValue);
                        };
                    }
                }

                memberMappers.Add(memberMapper);
                valueGetters.Add(valueGetter);
            }
        }
        return (memberMappers, valueGetters);
    }
    /// <summary>
    /// 查询、更新、删除都会使用，带有占位符的表名
    /// </summary>
    /// <param name="tableSegment"></param>
    /// <returns></returns>
    public string GetFormatTableName(TableSegment tableSegment)
    {
        string tableName = null;
        if (tableSegment.TableShardingInfo != null)
        {
            if (tableSegment.IsSharding)
            {
                //当单个ShardingTables时，只有一个分表的情况下，会移除ShardingTables中的表，存在多个分表的表时，不做移除
                if (tableSegment.ShardingType > ShardingTableType.SingleTable
                    && (tableSegment.TableType == TableType.Entity || tableSegment.TableType == TableType.Include))
                    tableName = $"__SHARDING_{tableSegment.ShardingId}_{tableSegment.Mapper.TableName}";
                //单个明确分表或是有分表的子查询
                else tableName = tableSegment.Body;
            }
            else tableName = tableSegment.Body;
        }
        else tableName = tableSegment.Body ?? tableSegment.Mapper.TableName;
        if (tableSegment.TableType != TableType.FromQuery)
        {
            if (!string.IsNullOrEmpty(tableSegment.TableSchema))
                tableName = $"{this.OrmProvider.GetTableName(tableSegment.TableSchema)}.{this.OrmProvider.GetTableName(tableName)}";
            else tableName = this.OrmProvider.GetTableName(tableName);
        }
        return tableName;
    }
    public virtual SqlSegment VisitDeferredSqlSegment(SqlSegment sqlSegment)
    {
        if (!this.IsSelect)
            throw new NotSupportedException($"只有在Select子句中，才支持延迟方法访问，表达式：{sqlSegment.Expression}");

        ICollection<Expression> fieldExprs = null;
        switch (sqlSegment.Expression.NodeType)
        {
            case ExpressionType.MemberAccess:
                var visitor = new MemberVisitor();
                visitor.Visit(sqlSegment.Expression);
                fieldExprs = visitor.Members;
                break;
            case ExpressionType.Call:
                var methodCallExpr = sqlSegment.Expression as MethodCallExpression;
                fieldExprs = methodCallExpr.Arguments;
                break;
        }
        string sql = "NULL";
        List<ReaderField> readerFields = null;
        if (fieldExprs != null && fieldExprs.Count > 0)
        {
            readerFields = new List<ReaderField>();
            var builder = new StringBuilder();
            foreach (var argsExpr in fieldExprs)
            {
                var argumentSegment = this.VisitAndDeferred(new SqlSegment { Expression = argsExpr });
                if (argumentSegment.SqlType == SqlType.OnlyField)
                {
                    var fieldName = argumentSegment.Value.ToString();
                    readerFields.Add(new ReaderField
                    {
                        Value = fieldName,
                        ReaderType = argsExpr.Type,
                        TypeHandler = argumentSegment.MemberMapper.TypeHandler
                    });
                    if (builder.Length > 0)
                        builder.Append(',');
                    builder.Append(fieldName);
                }
            }
            if (readerFields.Count > 0)
                sql = builder.ToString();
        }
        sqlSegment.SqlType = SqlType.ReaderField;
        sqlSegment.Value = new ReaderField
        {
            Value = sql,
            IsDeferredFields = true,
            Expression = sqlSegment.Expression,
            Fields = readerFields
        };
        return sqlSegment;
    }
    public TableSegment UseQuery(Type targetType, IQuery subQueryObj, bool isCopyRefParameters)
    {
        TableSegment tableSegment = null;
        var readerFields = new List<ReaderField>();

        //包含该查询对象引用，就说明当前visitor对象已经包含了该子查询引用到的参数，只需要添加表即可
        if (!this.RefQueries.Contains(subQueryObj))
        {
            this.CopyRefParametersFromQueryVisitor(subQueryObj.Visitor);
            this.RefQueries.Add(subQueryObj);
        }

        if (subQueryObj is ICteQuery cteQueryObj)
        {
            cteQueryObj.ReaderFields.ForEach(f => readerFields.Add(f.Clone()));
            tableSegment = this.AddJoinTable(targetType, null, TableType.CteSelfRef, cteQueryObj.TableName, readerFields);
        }
        else
        {
            var sql = subQueryObj.Visitor.BuildSql(false, out var myReaderFields);
            myReaderFields.ForEach(f => readerFields.Add(f.Clone()));
            tableSegment = this.AddJoinTable(targetType, null, TableType.FromQuery, $"({sql})", readerFields);
        }
        this.InitUseQueryReaderFields(tableSegment, readerFields);
        this.CopyShardingFromQueryVisitor(subQueryObj.Visitor);
        return tableSegment;
    }
    public TableSegment AddJoinTable(Type entityType, string joinType = null, TableType tableType = TableType.Entity, string body = null, List<SqlSegment> readerFields = null)
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
            Fields = readerFields,
            IsMaster = true
        });
    }
    public virtual TableSegment AddTable(TableSegment tableSegment)
    {
        //Union后，有加新表，要把前一个UnionSql设置完整
        this.ClearUnionSql();
        this.Tables.Add(tableSegment);
        if (this.ReaderFields != null && !this.IsUnion)
            this.ReaderFields = null;
        return tableSegment;
    }
    public virtual void ClearUnionSql()
    {
        if (this.UnionSql == null) return;

        //有union操作的visitor，都是新New的，前面只有一个表
        this.Tables.Last().Body = $"({this.UnionSql})";
        this.Tables.Last().TableType = TableType.FromQuery;
        this.UnionSql = null;
    }
    public void InitUseQueryReaderFields(TableSegment tableSegment, List<ReaderField> readerFields)
    {
        foreach (var readerField in readerFields)
        {
            //子查询中，访问了实体类对象，比如：Grouping分组对象或是匿名对象
            if (readerField.FieldType == SqlFieldType.Entity)
                this.InitUseQueryReaderFields(tableSegment, readerField.Fields);
            else
            {
                //已经变成子查询了，原表字段名已经没意义了，直接变成新的字段名
                readerField.TableSegment = tableSegment;
                readerField.TargetMember = readerField.TargetMember;
                //重新设置body内容，表别名变更，字段名也可能变更
                if (readerField.TargetMember != null)
                {
                    readerField.FieldName = readerField.TargetMember.Name;
                    readerField.Value = tableSegment.AliasName + "." + this.OrmProvider.GetFieldName(readerField.TargetMember.Name);
                }
                //更改原SQL中原始字段解析属性，防止在GetQuotedValue中使用原始字段名,导致SQL解析错误
                //readerField.IsNeedAlias = false;
                //readerField.IsConstant = false;
                //readerField.IsVariable = false;
                //readerField.IsExpression = false;
                //readerField.IsMethodCall = false;
            }
        }
    }
    public void CopyShardingFromQueryVisitor(IQueryVisitor visitor)
    {
        // 把subQueryObj的Visitor状态属性拷贝到当前Visitor中
        // 因为会生成新的子查询SQL，subQueryObj的Visitor状态已经通过新表更新到tableSegment.Body中，无需同步
        // 所以，只需要同步Sharding分表信息和IncludeTables表信息，这两个会影响到后面SQL的生成
        if (this.Equals(visitor)) return;
        if (visitor.IsNeedUnionShardingTables)
            this.IsNeedUnionShardingTables = true;
        if (visitor.IsNeedFormatShardingTables)
            this.IsNeedFormatShardingTables = true;
        if (visitor.IsManyShardingTables)
            this.IsManyShardingTables = true;
    }
    public void CopyRefParametersFromQueryVisitor(IQueryVisitor visitor)
    {
        if (this.Equals(visitor) || this.Equals(visitor.RefFrom)) return;

        if (visitor.NextDbParameters != null && visitor.NextDbParameters.Count > 0)
        {
            this.NextDbParameters ??= new TheaDbParameterCollection();
            foreach (var dbParameter in visitor.NextDbParameters)
            {
                if (this.NextDbParameters.Contains(dbParameter)) continue;
                this.NextDbParameters.Add(dbParameter);
            }
        }
        if (visitor.ShardingTables != null && visitor.ShardingTables.Count > 0)
        {
            this.ShardingTables ??= new();
            foreach (var shardingTable in visitor.ShardingTables)
            {
                if (this.ShardingTables.Contains(shardingTable)) continue;
                this.ShardingTables.Add(shardingTable);
            }
        }
        if (visitor.RefQueries != null && visitor.RefQueries.Count > 0)
        {
            this.RefQueries ??= new();
            foreach (var refQuery in visitor.RefQueries)
            {
                if (this.RefQueries.Contains(refQuery)) continue;
                this.RefQueries.Add(refQuery);
            }
        }
    }
    public virtual SqlSegment ToEnumString(SqlSegment sqlSegment)
    {
        if (sqlSegment.HasField)
        {
            var targetType = sqlSegment.MappedTargetType;
            if (targetType != typeof(string))
            {
                var enumValues = Enum.GetValues(sqlSegment.SegmentType);
                var enumUnderlyingType = Enum.GetUnderlyingType(sqlSegment.SegmentType);
                var enumBuilder = new StringBuilder($"CASE {sqlSegment.Body}");
                foreach (var enumValue in enumValues)
                {
                    var enumName = Enum.GetName(sqlSegment.SegmentType, enumValue);
                    var numberValue = Convert.ChangeType(enumValue, enumUnderlyingType);
                    enumBuilder.Append($" WHEN {numberValue} THEN '{enumName}'");
                }
                enumBuilder.Append(" END");
                sqlSegment.IsExpression = true;
                sqlSegment.Body = enumBuilder.ToString();
            }
        }
        else if (sqlSegment.IsConstant || sqlSegment.IsVariable)
            sqlSegment.Value = Enum.GetName(sqlSegment.SegmentType, sqlSegment.Value);
        sqlSegment.SegmentType = typeof(string);
        return sqlSegment;
    }
    public Expression EnsureMemberVisit(Expression expr)
    {
        var myExpr = expr;
        while (myExpr is not MemberExpression memberExpr)
        {
            if (myExpr is UnaryExpression unaryExpr)
                myExpr = unaryExpr.Operand;
            else throw new NotSupportedException($"不支持的表达式解析:{myExpr}->MemberExpression");
        }
        return myExpr;
    }
    public bool IsMemberVisit(Expression expr)
    {
        var myExpr = expr;
        while (myExpr is not MemberExpression memberExpr)
        {
            if (myExpr is UnaryExpression unaryExpr)
                myExpr = unaryExpr.Operand;
            else throw new NotSupportedException($"不支持的表达式解析:{myExpr}->MemberExpression");
        }
        return myExpr.NodeType == ExpressionType.MemberAccess;
    }
    public Stack<MemberExpression> GetMemberExprs(MemberExpression memberExpr)
    {
        Expression currentExpr = memberExpr;
        var memberExprs = new Stack<MemberExpression>();
        while (currentExpr != null)
        {
            if (currentExpr is UnaryExpression unaryExpr)
            {
                currentExpr = unaryExpr.Operand;
                continue;
            }
            switch (currentExpr.NodeType)
            {
                case ExpressionType.Parameter:
                    currentExpr = null;
                    break;
                case ExpressionType.MemberAccess:
                    var parentExpr = currentExpr as MemberExpression;
                    if (currentExpr == null) break;

                    memberExprs.Push(parentExpr);
                    currentExpr = parentExpr.Expression;
                    break;
                default: throw new NotSupportedException($"不支持的成员访问表达式，访问路径：{currentExpr.ToString()}");
            }
        }
        return memberExprs;
    }

    public Dictionary<string, List<object>> SplitShardingParameters(TableShardingInfo tableShardingInfo, Type paramterType, IEnumerable parameters, object parameterSample, IDictionary<string, object> shardingValues)
    {
        var tableSegment = this.Tables[0];
        var tableNameGetter = tableSegment.ShardingTableGetter ?? RepositoryHelper.BuildShardingTableNameGetter(this.DbContext, tableShardingInfo, tableSegment.EntityType, paramterType, parameterSample, shardingValues);
        var result = new Dictionary<string, List<object>>();
        foreach (var parameter in parameters)
        {
            var tableName = tableNameGetter.Invoke(parameter);
            if (!result.ContainsKey(tableName))
                result[tableName] = new List<object>();
            result[tableName].Add(parameter);
        }
        return result;
    }
    public virtual void Dispose()
    {
        if (this.isDisposed)
            return;
        this.isDisposed = true;

        this.Tables = null;
        this.TableAliases = null;
        this.RefTableAliases = null;
        this.ReaderFields = null;
        this.WhereBuilder = null;
        this.IncludeTables = null;

        //设置null，不能清空，以免给返回的参数丢失
        this.DbParameters = null;
        this.NextDbParameters = null;
        this.DbContext = null;

        //应用子查询表，只删除元素，不能dispose，后续操作可能还会用到子查询
        this.RefQueries.Clear();
    }
    private void VisitLogicBinaryExpr(StringBuilder builder, Expression expr)
    {
        if (expr.NodeType == ExpressionType.AndAlso || expr.NodeType == ExpressionType.OrElse)
        {
            var binaryExpr = expr as BinaryExpression;
            string op = expr.NodeType == ExpressionType.AndAlso ? " AND " : " OR ";
            bool needParensLeft = binaryExpr.Left.NodeType == ExpressionType.OrElse && expr.NodeType == ExpressionType.AndAlso;
            if (needParensLeft) builder.Append('(');
            this.VisitLogicBinaryExpr(builder, binaryExpr.Left);
            if (needParensLeft) builder.Append(')');
            builder.Append(op);
            bool needParensRight = binaryExpr.Right.NodeType == ExpressionType.OrElse && expr.NodeType == ExpressionType.AndAlso;
            if (needParensRight) builder.Append('(');
            this.VisitLogicBinaryExpr(builder, binaryExpr.Right);
            if (needParensRight) builder.Append(')');
        }
        else
        {
            var sqlSegment = this.VisitAndDeferred(this.CreateConditionSegment(expr));
            builder.Append(this.GetQuotedValue(sqlSegment));
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
}