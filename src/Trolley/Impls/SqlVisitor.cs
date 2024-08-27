﻿using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Trolley;

public class SqlVisitor : ISqlVisitor
{
    private static ConcurrentDictionary<int, Func<object, object>> memberGetterCache = new();
    private static string[] calcOps = new string[] { ">", ">=", "<", "<=", "+", "-", "*", "/", "%", "&", "|", "^", "<<", ">>" };
    private bool isDisposed;

    public DbContext DbContext { get; set; }
    public string DbKey => this.DbContext.DbKey;
    public IOrmProvider OrmProvider => this.DbContext.OrmProvider;
    public IEntityMapProvider MapProvider => this.DbContext.MapProvider;
    public ITableShardingProvider ShardingProvider => this.DbContext.ShardingProvider;
    public string DefaultTableSchema => this.DbContext.DefaultTableSchema;
    public bool IsParameterized => this.DbContext.IsParameterized;
    public string ParameterPrefix => this.DbContext.ParameterPrefix;
    public IDataParameterCollection DbParameters { get; set; }
    public IDataParameterCollection NextDbParameters { get; set; }
    public char TableAsStart { get; set; }
    public bool IsMultiple { get; set; }
    public int CommandIndex { get; set; }
    public bool IsUseMaster { get; set; }

    /// <summary>
    /// 所有表都是扁平化的，主表、1:1关系Include子表，也在这里
    /// </summary>
    public List<TableSegment> Tables { get; set; } = new();
    public Dictionary<string, TableSegment> TableAliases { get; set; } = new();
    /// <summary>
    /// 在解析子查询中，会用到父查询中的所有表，父查询中所有表别名引用
    /// </summary> 
    public Dictionary<string, TableSegment> RefTableAliases { get; set; }
    public bool IsSelect { get; set; }
    public bool IsWhere { get; set; }
    public bool IsNeedTableAlias { get; set; }
    public bool IsIncludeMany { get; set; }

    public List<SqlFieldSegment> ReaderFields { get; set; }
    public bool IsFromQuery { get; set; }
    public string WhereSql { get; set; }
    public OperationType LastWhereOperationType { get; set; } = OperationType.None;

    public List<SqlFieldSegment> GroupFields { get; set; }
    public List<TableSegment> IncludeTables { get; set; }
    public List<IQuery> RefQueries { get; set; } = new();
    public bool IsNeedFetchShardingTables { get; set; }
    public List<TableSegment> ShardingTables { get; set; }

    public void UseTable(bool isIncludeMany, params string[] tableNames)
    {
        if (tableNames == null) throw new ArgumentNullException(nameof(tableNames));

        var tableSegment = isIncludeMany ? this.IncludeTables.Last() : this.Tables.Last();
        //多个分表，才当作分表处理
        tableSegment.IsSharding = true;
        if (tableNames.Length > 1)
        {
            tableSegment.ShardingType = ShardingTableType.MultiTable;
            tableSegment.TableNames = new List<string>(tableNames);
            this.ShardingTables ??= new();
            if (this.ShardingTables.Exists(f => f.ShardingType < ShardingTableType.SubordinateMap))
                throw new NotSupportedException("一个查询语句中仅支持一个主表多个分表，其他表多个分表只能调用方法UseTable<TMasterSharding>(Func<string, string, string, string> tableNameGetter)构造与主表表名称映射实现多个分表");
            if (!this.ShardingTables.Contains(tableSegment))
            {
                tableSegment.ShardingId = Guid.NewGuid().ToString("N");
                this.ShardingTables.Add(tableSegment);
            }
        }
        //一个分表的，当作不分表处理
        else
        {
            tableSegment.ShardingType = ShardingTableType.SingleTable;
            tableSegment.Body = tableNames[0];
        }
    }
    public void UseTable(bool isIncludeMany, Func<string, bool> tableNamePredicate)
    {
        if (tableNamePredicate == null)
            throw new ArgumentNullException(nameof(tableNamePredicate));

        var tableSegment = isIncludeMany ? this.IncludeTables.Last() : this.Tables.Last();
        if (this.ShardingProvider == null || !this.ShardingProvider.TryGetTableSharding(tableSegment.EntityType, out var tableShardingInfo))
            throw new Exception($"实体表{tableSegment.EntityType.FullName}没有配置分表，无需调用此方法");
        if (tableShardingInfo.DependOnMembers == null || tableShardingInfo.DependOnMembers.Count == 0)
            throw new Exception($"实体表{tableSegment.EntityType.FullName}没有配置分表依赖的字段");

        tableSegment.IsSharding = true;
        tableSegment.ShardingType = ShardingTableType.MasterFilter;
        tableSegment.ShardingFilter = tableNamePredicate;
        this.ShardingTables ??= new();
        if (!this.ShardingTables.Contains(tableSegment))
        {
            tableSegment.ShardingId = Guid.NewGuid().ToString("N");
            this.ShardingTables.Add(tableSegment);
            this.IsNeedFetchShardingTables = true;
        }
    }
    public void UseTable(bool isIncludeMany, Type masterEntityType, Func<string, string, string, string> tableNameGetter)
    {
        var tableSegment = isIncludeMany ? this.IncludeTables.Last() : this.Tables.Last();
        if (this.ShardingProvider == null || !this.ShardingProvider.TryGetTableSharding(tableSegment.EntityType, out var shardingTable))
            throw new Exception($"实体表{tableSegment.EntityType.FullName}没有配置分表，无需调用此方法");
        if (shardingTable.DependOnMembers == null || shardingTable.DependOnMembers.Count == 0)
            throw new Exception($"实体表{tableSegment.EntityType.FullName}没有配置分表依赖的字段");

        tableSegment.IsSharding = true;
        tableSegment.ShardingType = ShardingTableType.SubordinateMap;
        if (this.ShardingTables == null)
            throw new NotSupportedException("当主表有多个分表时，才能使用此方法UseTable<TMasterSharding>(Func<string, string, string, string> tableNameGetter)");
        //分表依赖，只要取一个主表就行，因为分表规则都是一样的
        tableSegment.ShardingDependent = this.ShardingTables.Find(f => f.EntityType == masterEntityType);
        tableSegment.ShardingMapGetter = tableNameGetter;

        var masterTableSegment = this.ShardingTables.Find(f => f.ShardingType > ShardingTableType.SingleTable && f.ShardingType < ShardingTableType.SubordinateMap);
        if (masterTableSegment.EntityType != masterEntityType)
            throw new NotSupportedException($"实体表{tableSegment.EntityType.FullName}的映射实体应该选择第一个多个分表的表实体类型:{masterTableSegment.EntityType.FullName}");

        if (!this.ShardingTables.Contains(tableSegment))
        {
            tableSegment.ShardingId = Guid.NewGuid().ToString("N");
            this.ShardingTables.Add(tableSegment);
            this.IsNeedFetchShardingTables = true;
        }
    }
    public void UseTableBy(bool isIncludeMany, object field1Value, object field2Value = null)
    {
        var tableSegment = isIncludeMany ? this.IncludeTables.Last() : this.Tables.Last();
        if (this.ShardingProvider == null || !this.ShardingProvider.TryGetTableSharding(tableSegment.EntityType, out var shardingTable))
            throw new Exception($"实体表{tableSegment.EntityType.FullName}没有配置分表，无需调用此方法");
        if (shardingTable.DependOnMembers == null || shardingTable.DependOnMembers.Count == 0)
            throw new Exception($"实体表{tableSegment.EntityType.FullName}没有配置分表依赖的字段");

        tableSegment.IsSharding = true;
        var origTableName = tableSegment.Mapper.TableName;
        string tableName = null;

        if (field1Value == null)
            throw new ArgumentNullException($"实体{tableSegment.EntityType.FullName}的分表规则依赖字段，字段值field1Value不能为null");

        if (shardingTable.DependOnMembers.Count > 1)
        {
            if (field2Value == null)
                throw new ArgumentNullException($"实体{tableSegment.EntityType.FullName}的分表规则依赖2个字段，字段值field2Value不能为null");
            var shardingRule = shardingTable.Rule as Func<string, string, object, object, string>;
            tableName = shardingRule.Invoke(this.DbKey, origTableName, field1Value, field2Value);
        }
        else
        {
            var shardingRule = shardingTable.Rule as Func<string, string, object, string>;
            tableName = shardingRule.Invoke(this.DbKey, origTableName, field1Value);
        }
        //单个分表，直接设置body表名，当作不分表处理
        if (tableSegment.TableNames == null)
        {
            if (string.IsNullOrEmpty(tableSegment.Body))
                tableSegment.Body = tableName;
            else
            {
                tableSegment.TableNames = new List<string> { tableSegment.Body, tableName };
                tableSegment.Body = null;
                tableSegment.ShardingType = ShardingTableType.MultiTable;
                this.ShardingTables ??= new();
                if (!this.ShardingTables.Contains(tableSegment))
                    this.ShardingTables.Add(tableSegment);
            }
        }
        else tableSegment.TableNames.Add(tableName);
    }
    public void UseTableByRange(bool isIncludeMany, object beginFieldValue, object endFieldValue)
    {
        var tableSegment = isIncludeMany ? this.IncludeTables.Last() : this.Tables.Last();
        if (this.ShardingProvider == null || !this.ShardingProvider.TryGetTableSharding(tableSegment.EntityType, out var shardingTable))
            throw new Exception($"实体表{tableSegment.EntityType.FullName}没有配置分表，无需调用此方法");
        if (shardingTable.DependOnMembers == null || shardingTable.DependOnMembers.Count == 0)
            throw new Exception($"实体表{tableSegment.EntityType.FullName}没有配置分表依赖的字段");
        if (shardingTable.DependOnMembers.Count > 1)
            throw new NotSupportedException($"实体表{tableSegment.EntityType.FullName}的分表规则依赖2个字段，不能使用此方法");

        tableSegment.IsSharding = true;
        var origTableName = tableSegment.Mapper.TableName;
        var shardingRule = shardingTable.RangleRule as Func<string, string, object, object, List<string>>;
        var tableNames = shardingRule.Invoke(this.DbKey, origTableName, beginFieldValue, endFieldValue);
        if (tableNames.Count > 1)
        {
            tableSegment.ShardingType = ShardingTableType.TableRange;
            tableSegment.TableNames = new List<string>(tableNames);
            this.ShardingTables ??= new();
            if (this.ShardingTables.Exists(f => f.ShardingType < ShardingTableType.SubordinateMap))
                throw new NotSupportedException("一个查询语句中仅支持一个主表多个分表，其他表多个分表只能调用方法UseTable<TMasterSharding>(Func<string, string, string, string> tableNameGetter)构造与主表表名称映射实现多个分表");
            if (!this.ShardingTables.Contains(tableSegment))
            {
                tableSegment.ShardingId = Guid.NewGuid().ToString("N");
                this.ShardingTables.Add(tableSegment);
            }
        }
        //一个分表的，当作不分表处理
        else tableSegment.Body = tableNames[0];
        this.IsNeedFetchShardingTables = true;
    }
    public void UseTableByRange(bool isIncludeMany, object fieldValue1, object fieldValue2, object fieldValue3)
    {
        var tableSegment = isIncludeMany ? this.IncludeTables.Last() : this.Tables.Last();
        if (this.ShardingProvider == null || !this.ShardingProvider.TryGetTableSharding(tableSegment.EntityType, out var shardingTable))
            throw new Exception($"实体表{tableSegment.EntityType.FullName}没有配置分表，无需调用此方法");
        if (shardingTable.DependOnMembers == null || shardingTable.DependOnMembers.Count == 0)
            throw new Exception($"实体表{tableSegment.EntityType.FullName}没有配置分表依赖的字段");
        if (shardingTable.DependOnMembers.Count == 1)
            throw new NotSupportedException($"实体{tableSegment.EntityType.FullName}的分表规则依赖1个字段，不能使用此方法");

        tableSegment.IsSharding = true;
        var origTableName = tableSegment.Mapper.TableName;
        var shardingRule = shardingTable.RangleRule as Func<string, string, object, object, object, List<string>>;
        var tableNames = shardingRule.Invoke(this.DbKey, origTableName, fieldValue1, fieldValue2, fieldValue3);
        if (tableNames.Count > 1)
        {
            tableSegment.ShardingType = ShardingTableType.TableRange;
            tableSegment.TableNames = new List<string>(tableNames);
            this.ShardingTables ??= new();
            if (this.ShardingTables.Exists(f => f.ShardingType < ShardingTableType.SubordinateMap))
                throw new NotSupportedException("一个查询语句中仅支持一个主表多个分表，其他表多个分表只能调用方法UseTable<TMasterSharding>(Func<string, string, string, string> tableNameGetter)构造与主表表名称映射实现多个分表");
            if (!this.ShardingTables.Contains(tableSegment))
            {
                tableSegment.ShardingId = Guid.NewGuid().ToString("N");
                this.ShardingTables.Add(tableSegment);
            }
        }
        //一个分表的，当作不分表处理
        else tableSegment.Body = tableNames[0];
        this.IsNeedFetchShardingTables = true;
    }
    public void UseTableSchema(bool isIncludeMany, string tableSchema)
    {
        if (tableSchema == this.DefaultTableSchema) return;

        var tableSegment = isIncludeMany ? this.IncludeTables.Last() : this.Tables.Last();
        tableSegment.TableSchema = tableSchema;
    }
    public void UseMaster(bool isUseMaster = true) => this.IsUseMaster = isUseMaster;
    public virtual string BuildTableShardingsSql() => null;
    public void SetShardingTables(List<string> shardingTables)
    {
        List<string> tableNames = null;
        if (this.ShardingTables.Count > 1)
        {
            var needQueryTables = this.ShardingTables.FindAll(f => f.ShardingType > ShardingTableType.MultiTable);
            foreach (var tableSegment in needQueryTables)
            {
                var entityType = tableSegment.EntityType;
                var tableName = tableSegment.Mapper.TableName;
                if (this.ShardingProvider == null || !this.ShardingProvider.TryGetTableSharding(entityType, out var shardingTable))
                    throw new Exception($"实体{entityType.FullName}表没有配置分表信息");

                switch (tableSegment.ShardingType)
                {
                    case ShardingTableType.TableRange:
                        var oldTableNames = tableSegment.TableNames;
                        tableSegment.TableNames = shardingTables.FindAll(f => oldTableNames.Contains(f) && Regex.IsMatch(f, shardingTable.ValidateRegex));
                        break;
                    case ShardingTableType.MasterFilter:
                        tableSegment.TableNames = shardingTables.FindAll(f => f.Contains(tableName) && Regex.IsMatch(f, shardingTable.ValidateRegex) && tableSegment.ShardingFilter.Invoke(f));
                        break;
                    case ShardingTableType.SubordinateMap:
                        //此处只是把所有可能的分表名称设置一下，在执行前，再做过滤                      
                        tableSegment.TableNames = shardingTables.FindAll(f => Regex.IsMatch(f, shardingTable.ValidateRegex));
                        break;
                }
            }
        }
        else
        {
            var tableSegment = this.ShardingTables[0];
            var entityType = tableSegment.EntityType;
            if (this.ShardingProvider == null || !this.ShardingProvider.TryGetTableSharding(entityType, out var shardingTable))
                throw new Exception($"实体{entityType.FullName}表有配置分表信息");

            tableNames = shardingTables.FindAll(f =>
            {
                var result = Regex.IsMatch(f, shardingTable.ValidateRegex);
                if (tableSegment.ShardingType == ShardingTableType.MasterFilter)
                    result = result && tableSegment.ShardingFilter.Invoke(f);
                else if (tableSegment.ShardingType == ShardingTableType.TableRange && tableSegment.TableNames != null)
                    result = result && tableSegment.TableNames.Contains(f);
                return result;
            });
            //只有一个分表时，会移除ShardingTables里面元素，生成SQL时候，直接取tableSegment.Body
            if (tableNames.Count > 1)
                tableSegment.TableNames = tableNames;
            else
            {
                tableSegment.Body = tableNames[0];
                tableSegment.TableNames = null;
                this.ShardingTables.Remove(tableSegment);
            }
        }
    }

    public virtual SqlFieldSegment VisitAndDeferred(SqlFieldSegment sqlSegment)
    {
        sqlSegment = this.Visit(sqlSegment);
        if (!sqlSegment.HasDeferred)
            return sqlSegment;

        //处理HasValue !逻辑取反操作，这种情况下是一元操作
        return this.VisitDeferredBoolConditional(sqlSegment, true, this.OrmProvider.GetQuotedValue(true), this.OrmProvider.GetQuotedValue(false));
    }
    public virtual SqlFieldSegment Visit(SqlFieldSegment sqlSegment)
    {
        SqlFieldSegment result = null;
        //初始值为表达式的类型
        sqlSegment.SegmentType = sqlSegment.Expression.Type;
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
    public virtual SqlFieldSegment VisitUnary(SqlFieldSegment sqlSegment)
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
                return sqlSegment.Change($"~{this.Visit(sqlSegment)}", true);
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
    public virtual SqlFieldSegment VisitBinary(SqlFieldSegment sqlSegment)
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
                if (leftSegment.IsDeferredFields) return sqlSegment;
                var rightSegment = this.Visit(new SqlFieldSegment { Expression = binaryExpr.Right });
                if (rightSegment.IsDeferredFields) return sqlSegment;

                //计算数组访问，a??b
                if (leftSegment.IsConstant && rightSegment.IsConstant)
                    return sqlSegment.ChangeValue(Expression.Lambda(binaryExpr).Compile().DynamicInvoke(), true);

                if ((leftSegment.IsConstant || leftSegment.IsVariable)
                    && (rightSegment.IsConstant || rightSegment.IsVariable))
                    return sqlSegment.ChangeValue(Expression.Lambda(binaryExpr).Compile().DynamicInvoke(), false);

                //下面都是带有参数的情况，带有参数表达式计算(常量、变量)、函数调用等共2种情况
                //bool类型的表达式，这里不做解析只做defer操作解析，到最外层select、where、having、joinOn子句中去解析合并
                if (binaryExpr.NodeType == ExpressionType.Equal || binaryExpr.NodeType == ExpressionType.NotEqual)
                {
                    //处理null != a.UserName和"kevin" == a.UserName情况
                    if (!leftSegment.HasField && rightSegment.HasField)
                        this.Swap(ref leftSegment, ref rightSegment);
                    if (leftSegment == SqlFieldSegment.Null && rightSegment != SqlFieldSegment.Null)
                        this.Swap(ref leftSegment, ref rightSegment);

                    //处理!(a.IsEnabled==true)情况，bool类型，最外层再做defer处理
                    if (binaryExpr.Left.Type == typeof(bool) && leftSegment.HasField && !rightSegment.HasField)
                    {
                        leftSegment.Push(new DeferredExpr { OperationType = OperationType.Equal, Value = SqlFieldSegment.True });
                        if (!(bool)rightSegment.Value)
                            leftSegment.Push(new DeferredExpr { OperationType = OperationType.Not });
                        if (binaryExpr.NodeType == ExpressionType.NotEqual)
                            leftSegment.Push(new DeferredExpr { OperationType = OperationType.Not });
                        return leftSegment;
                    }
                    if (rightSegment == SqlFieldSegment.Null)
                    {
                        leftSegment.Push(new DeferredExpr
                        {
                            OperationType = OperationType.Equal,
                            Value = SqlFieldSegment.Null
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

                //单个字段访问，才会设置nativeDbType和typeHandler
                if (leftSegment.HasField && (!leftSegment.IsMethodCall && !leftSegment.IsExpression || leftSegment.IsFieldType))
                {
                    rightSegment.ExpectType = leftSegment.ExpectType;
                    rightSegment.NativeDbType = leftSegment.NativeDbType;
                    rightSegment.TypeHandler = leftSegment.TypeHandler;
                }

                string strLeft = this.GetQuotedValue(leftSegment);
                string strRight = this.GetQuotedValue(rightSegment);

                if (binaryExpr.NodeType == ExpressionType.Coalesce)
                    return sqlSegment.Merge(leftSegment, rightSegment, $"{operators}({strLeft},{strRight})", false, true);

                if (leftSegment.IsExpression)
                    strLeft = $"({strLeft})";
                if (rightSegment.IsExpression)
                    strRight = $"({strRight})";

                return sqlSegment.Merge(leftSegment, rightSegment, $"{strLeft}{operators}{strRight}");
        }
        return sqlSegment;
    }
    public virtual SqlFieldSegment VisitMemberAccess(SqlFieldSegment sqlSegment)
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
                    sqlSegment.Push(new DeferredExpr { OperationType = OperationType.Equal, Value = SqlFieldSegment.Null });
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
                sqlSegment = formatter.Invoke(this, targetSegment);
                sqlSegment.SegmentType = memberExpr.Type;
                return sqlSegment;
            }

            if (memberExpr.IsParameter(out var parameterName))
            {
                //Where(f => f.Amount > 5)
                //Select(f => new { f.OrderId, f.Disputes ...})
                var tableSegment = this.TableAliases[parameterName];
                sqlSegment.HasField = true;
                sqlSegment.TableSegment = tableSegment;
                string fieldName = null;

                if (tableSegment.TableType == TableType.FromQuery || tableSegment.TableType == TableType.CteSelfRef)
                {
                    //访问子查询表的成员，子查询表没有Mapper，也不会有实体类型成员
                    //Json的实体类型字段
                    SqlFieldSegment readerField = null;
                    //子查询中，Select了Grouping分组对象，子查询中，只有一个分组对象才是实体类型，目前子查询，只支持一层
                    //取AS后的字段名，与原字段名不一定一样,AS后的字段名与memberExpr.Member.Name一致
                    if (memberExpr.Expression.NodeType != ExpressionType.Parameter)
                    {
                        var parentMemberExpr = memberExpr.Expression as MemberExpression;
                        var parenetReaderField = tableSegment.Fields.Count == 1 ? tableSegment.Fields.First()
                            : tableSegment.Fields.Find(f => f.TargetMember.Name == parentMemberExpr.Member.Name);
                        var fromReaderFields = parenetReaderField.Fields;
                        readerField = fromReaderFields.Count == 1 ? fromReaderFields.First()
                            : fromReaderFields.Find(f => f.TargetMember.Name == memberExpr.Member.Name);
                        fieldName = this.OrmProvider.GetFieldName(memberExpr.Member.Name);
                        if (this.IsNeedTableAlias) fieldName = tableSegment.AliasName + "." + fieldName;
                    }
                    else
                    {
                        readerField = tableSegment.Fields.Count == 1 ? tableSegment.Fields.First()
                          : tableSegment.Fields.Find(f => f.TargetMember.Name == memberExpr.Member.Name);
                        fieldName = readerField.Body;
                    }
                    sqlSegment.FromMember = readerField.TargetMember;
                    sqlSegment.SegmentType = readerField.SegmentType;
                    if (readerField.SegmentType.IsEnumType(out var underlyingType))
                        sqlSegment.ExpectType = underlyingType;
                    sqlSegment.NativeDbType = readerField.NativeDbType;
                    sqlSegment.TypeHandler = readerField.TypeHandler;
                    sqlSegment.Body = fieldName;
                }
                else
                {
                    var memberMapper = tableSegment.Mapper.GetMemberMap(memberExpr.Member.Name);
                    if (memberMapper.IsIgnore)
                        throw new Exception($"类{tableSegment.EntityType.FullName}的成员{memberMapper.MemberName}是忽略成员无法访问");
                    if (memberMapper.MemberType.IsEntityType(out _) && !memberMapper.IsNavigation && memberMapper.TypeHandler == null)
                        throw new Exception($"类{tableSegment.EntityType.FullName}的成员{memberExpr.Member.Name}不是值类型，未配置为导航属性也没有配置TypeHandler");
                    sqlSegment.FromMember = memberMapper.Member;
                    sqlSegment.SegmentType = memberMapper.MemberType;
                    if (memberMapper.UnderlyingType.IsEnum)
                        sqlSegment.ExpectType = memberMapper.UnderlyingType;
                    sqlSegment.NativeDbType = memberMapper.NativeDbType;
                    sqlSegment.TypeHandler = memberMapper.TypeHandler;
                    //查询时，IsNeedAlias始终为true，新增、更新、删除时，引用联表操作时，才会为true
                    fieldName = this.OrmProvider.GetFieldName(memberMapper.FieldName);
                    if (this.IsNeedTableAlias) fieldName = tableSegment.AliasName + "." + fieldName;
                    sqlSegment.Body = fieldName;
                }
                //.NET枚举类型总是解析成对应的UnderlyingType数值类型，如：a.Gender ?? Gender.Male == Gender.Male
                return sqlSegment;
            }
        }

        if (memberExpr.Member.DeclaringType == typeof(DBNull))
            return SqlFieldSegment.Null;

        //各种静态成员访问，如：DateTime.Now,int.MaxValue,string.Empty
        if (this.OrmProvider.TryGetMemberAccessSqlFormatter(memberExpr, out formatter))
        {
            sqlSegment = formatter.Invoke(this, sqlSegment);
            sqlSegment.SegmentType = memberExpr.Type;
            return sqlSegment;
        }

        //访问局部变量或是成员变量，当作常量处理，直接计算，后面统一做参数化处理
        //var orderIds=new List<int>{1,2,3}; Where(f=>orderIds.Contains(f.OrderId)); orderIds
        //private Order order; Where(f=>f.OrderId==this.Order.Id); this.Order.Id
        //var orderId=10; Select(f=>new {OrderId=orderId,...}
        //Select(f=>new {OrderId=this.Order.Id, ...}
        this.Evaluate(sqlSegment);

        sqlSegment.IsConstant = false;
        sqlSegment.IsVariable = true;
        sqlSegment.SegmentType = memberExpr.Type;
        return sqlSegment;
    }
    public virtual SqlFieldSegment VisitConstant(SqlFieldSegment sqlSegment)
    {
        var constantExpr = sqlSegment.Expression as ConstantExpression;
        if (constantExpr.Value == null)
            return SqlFieldSegment.Null;

        sqlSegment.Value = constantExpr.Value;
        sqlSegment.IsConstant = true;
        return sqlSegment;
    }
    public virtual SqlFieldSegment VisitMethodCall(SqlFieldSegment sqlSegment)
    {
        var methodCallExpr = sqlSegment.Expression as MethodCallExpression;
        if (this.IsSqlMethodCall(methodCallExpr))
        {
            sqlSegment = this.VisitSqlMethodCall(sqlSegment);
            sqlSegment.SegmentType = methodCallExpr.Type;
            return sqlSegment;
        }
        if (!sqlSegment.IsDeferredFields && this.OrmProvider.TryGetMethodCallSqlFormatter(methodCallExpr, out var formatter))
        {
            sqlSegment = formatter.Invoke(this, methodCallExpr, methodCallExpr.Object, sqlSegment.DeferredExprs, methodCallExpr.Arguments.ToArray());
            sqlSegment.SegmentType = methodCallExpr.Type;
            return sqlSegment;
        }
        if (this.IsSelect)
        {
            //延迟方法调用，两种场景：
            //1.主动延迟方法调用：如，把返回的枚举列转成描述，参数就是枚举列，返回值是对应的描述
            //2.Select子句中Include导航成员访问，主表数据已经查询了，此处成员访问只是多一个引用赋值动作，做成了延迟委托调用
            string fields = null;
            List<SqlFieldSegment> readerFields = null;
            LambdaExpression deferredFuncExpr = null;
            if (methodCallExpr.Arguments != null && methodCallExpr.Arguments.Count > 0)
            {
                readerFields = new List<SqlFieldSegment>();
                var builder = new StringBuilder();
                var visitor = new ReplaceParameterVisitor();
                var bodyExpr = visitor.Visit(methodCallExpr);
                //f.Balance.ToString("C")
                //args0.ToString("C")
                //(args0)=>{args0.ToString("C")}
                if (visitor.NewParameters.Count > 0)
                    deferredFuncExpr = Expression.Lambda(bodyExpr, visitor.NewParameters);
                foreach (var argsExpr in visitor.OrgMembers)
                {
                    var argumentSegment = this.VisitAndDeferred(new SqlFieldSegment { Expression = argsExpr });
                    if (argumentSegment.HasField)
                    {
                        sqlSegment.HasField = true;
                        var fieldName = argumentSegment.Body;
                        readerFields.Add(new SqlFieldSegment
                        {
                            SegmentType = argsExpr.Type,
                            TargetMember = argsExpr.Member,
                            NativeDbType = argumentSegment.NativeDbType,
                            TypeHandler = argumentSegment.TypeHandler
                        });
                        if (builder.Length > 0)
                            builder.Append(',');
                        builder.Append(fieldName);
                    }
                }
                if (readerFields.Count > 0)
                    fields = builder.ToString();
            }
            else deferredFuncExpr = Expression.Lambda(methodCallExpr);

            if (sqlSegment.IsDeferredFields || !string.IsNullOrEmpty(fields))
            {
                if (readerFields == null)
                    fields = "NULL";
                var deferredDelegate = deferredFuncExpr.Compile();
                sqlSegment.IsDeferredFields = true;
                sqlSegment.FieldType = SqlFieldType.DeferredFields;
                sqlSegment.Body = fields;
                sqlSegment.DeferredDelegateType = deferredFuncExpr.Type;
                sqlSegment.DeferredDelegate = deferredDelegate;
                sqlSegment.Fields = readerFields;
                sqlSegment.IsMethodCall = true;
                return sqlSegment;
            }
        }
        sqlSegment = this.Evaluate(sqlSegment);
        sqlSegment.SegmentType = methodCallExpr.Type;
        return sqlSegment;
    }
    public virtual SqlFieldSegment VisitParameter(SqlFieldSegment sqlSegment)
    {
        var parameterExpr = sqlSegment.Expression as ParameterExpression;
        //两种场景：.Select((x, y) => new { Order = x, x.Seller, x.Buyer, ... }) 和 .Select((x, y) => x)
        //参数访问通常都是SELECT语句的实体访问
        if (!this.IsSelect) throw new NotSupportedException($"不支持的参数表达式访问，只支持Select语句中，{parameterExpr}");
        var fromSegment = this.TableAliases[parameterExpr.Name];
        var readerField = new SqlFieldSegment
        {
            FieldType = SqlFieldType.Entity,
            TableSegment = fromSegment,
            SegmentType = fromSegment.EntityType,
            Fields = this.FlattenTableFields(fromSegment),
            Path = parameterExpr.Name
        };
        //include表的ReaderField字段，紧跟在主表ReaderField后面
        var readerFields = new List<SqlFieldSegment>() { readerField };
        this.AddIncludeTableReaderFields(readerField, readerFields);
        sqlSegment.Value = readerFields;
        return sqlSegment;
    }
    protected void AddIncludeTableReaderFields(SqlFieldSegment parent, List<SqlFieldSegment> readerFields)
    {
        var includedSegments = this.Tables.FindAll(f => f.TableType == TableType.Include && f.FromTable == parent.TableSegment);
        if (includedSegments.Count > 0)
        {
            parent.HasNextInclude = true;
            foreach (var includedSegment in includedSegments)
            {
                var childReaderFields = this.FlattenTableFields(includedSegment);
                var readerField = new SqlFieldSegment
                {
                    FieldType = SqlFieldType.Entity,
                    TableSegment = includedSegment,
                    FromMember = includedSegment.FromMember.Member,
                    TargetMember = includedSegment.FromMember.Member,
                    SegmentType = includedSegment.EntityType,
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
    public virtual SqlFieldSegment VisitNew(SqlFieldSegment sqlSegment)
    {
        throw new NotImplementedException();
    }
    public virtual SqlFieldSegment VisitMemberInit(SqlFieldSegment sqlSegment)
    {
        throw new NotImplementedException();
    }
    public virtual SqlFieldSegment VisitNewArray(SqlFieldSegment sqlSegment)
    {
        sqlSegment.IsArray = true;
        var newArrayExpr = sqlSegment.Expression as NewArrayExpression;
        var result = new List<object>();
        foreach (var elementExpr in newArrayExpr.Expressions)
        {
            var elementSegment = new SqlFieldSegment { Expression = elementExpr };
            elementSegment = this.VisitAndDeferred(elementSegment);
            result.Add(elementSegment.Value);
        }
        //走到这里肯定是常量，变量会走到成员访问
        return sqlSegment.ChangeValue(result, true);
    }
    public virtual SqlFieldSegment VisitIndexExpression(SqlFieldSegment sqlSegment)
    {
        if (sqlSegment.Expression.IsParameter(out _))
            throw new NotSupportedException("索引表达式不支持Parameter访问操作");
        return this.Evaluate(sqlSegment);
    }
    public virtual SqlFieldSegment VisitConditional(SqlFieldSegment sqlSegment)
    {
        var conditionalExpr = sqlSegment.Expression as ConditionalExpression;
        sqlSegment = this.Visit(sqlSegment.Next(conditionalExpr.Test));
        var ifTrueSegment = this.Visit(new SqlFieldSegment { Expression = conditionalExpr.IfTrue });
        var ifFalseSegment = this.Visit(new SqlFieldSegment { Expression = conditionalExpr.IfFalse });

        if (ifTrueSegment.HasField && (!ifTrueSegment.IsMethodCall && !ifTrueSegment.IsExpression || ifTrueSegment.IsFieldType))
        {
            ifFalseSegment.ExpectType = ifTrueSegment.ExpectType;
            ifFalseSegment.NativeDbType = ifTrueSegment.NativeDbType;
            ifFalseSegment.TypeHandler = ifTrueSegment.TypeHandler;
        }
        string leftArgument = this.GetQuotedValue(ifTrueSegment);
        string rightArgument = this.GetQuotedValue(ifFalseSegment);
        sqlSegment.IsFieldType = true;
        sqlSegment.ExpectType = ifTrueSegment.ExpectType;
        sqlSegment.NativeDbType = ifTrueSegment.NativeDbType;
        sqlSegment.TypeHandler = ifFalseSegment.TypeHandler;
        sqlSegment.SegmentType = ifFalseSegment.SegmentType;
        return this.VisitDeferredBoolConditional(sqlSegment, conditionalExpr.IfTrue.Type == typeof(bool), leftArgument, rightArgument);
    }
    public virtual SqlFieldSegment VisitListInit(SqlFieldSegment sqlSegment)
    {
        sqlSegment.IsArray = true;
        var listExpr = sqlSegment.Expression as ListInitExpression;
        var result = new List<object>();
        foreach (var elementInit in listExpr.Initializers)
        {
            if (elementInit.Arguments.Count == 0)
                continue;
            var elementSegment = new SqlFieldSegment { Expression = elementInit.Arguments[0] };
            elementSegment = this.VisitAndDeferred(elementSegment);
            if (elementSegment.HasField)
                throw new NotSupportedException("不支持的表达式访问，ListInitExpression表达式只支持常量和变量，不支持参数访问");
            result.Add(elementSegment.Value);
        }
        return sqlSegment.ChangeValue(result, true);
    }
    public virtual SqlFieldSegment VisitTypeIs(SqlFieldSegment sqlSegment)
    {
        var binaryExpr = sqlSegment.Expression as TypeBinaryExpression;
        if (!binaryExpr.Expression.IsParameter(out _))
            return this.Evaluate(sqlSegment);
        if (binaryExpr.TypeOperand == typeof(DBNull))
        {
            sqlSegment.Push(new DeferredExpr
            {
                OperationType = OperationType.Equal,
                Value = SqlFieldSegment.Null
            });
            return this.Visit(sqlSegment.Next(binaryExpr.Expression));
        }
        throw new NotSupportedException($"不支持的表达式操作，{sqlSegment.Expression}");
    }
    public virtual SqlFieldSegment Evaluate(SqlFieldSegment sqlSegment)
    {
        var objValue = sqlSegment.Expression.Evaluate();
        if (objValue == null)
            return SqlFieldSegment.Null;
        sqlSegment.Value = objValue;
        return sqlSegment;
    }
    public virtual T Evaluate<T>(Expression expr)
    {
        var objValue = this.Evaluate(expr);
        if (objValue == null)
            return default;
        return (T)objValue;
    }
    public virtual object Evaluate(Expression expr) => expr.Evaluate();
    public virtual SqlFieldSegment VisitSqlMethodCall(SqlFieldSegment sqlSegment)
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
                if (methodCallExpr.Arguments.Count > 1)
                {
                    if (!this.OrmProvider.TryGetMethodCallSqlFormatter(methodCallExpr, out var sqlFormatter))
                        throw new NotImplementedException($"当前Provider:{this.OrmProvider.GetType().FullName}未实现方法IsNull");
                    sqlSegment = sqlFormatter.Invoke(this, sqlSegment.OriginalExpression, null, null, methodCallExpr.Arguments.ToArray());
                }
                else
                {
                    sqlSegment.Push(new DeferredExpr { OperationType = OperationType.Equal, Value = SqlFieldSegment.Null });
                    sqlSegment = this.VisitAndDeferred(sqlSegment.Next(methodCallExpr.Arguments[0]));
                }
                break;
            case "ToParameter":
                sqlSegment.IsParameterized = true;
                sqlSegment.ParameterName = this.Evaluate<string>(methodCallExpr.Arguments[1]);
                sqlSegment = this.Visit(sqlSegment.Next(methodCallExpr.Arguments[0]));
                break;
            case "In":
                var elementType = methodCallExpr.Method.GetGenericArguments()[0];
                var type = methodCallExpr.Arguments[1].Type;
                var fieldSegment = this.Visit(new SqlFieldSegment { Expression = methodCallExpr.Arguments[0] });
                string inSql = null;
                if (type.IsArray || typeof(IEnumerable<>).MakeGenericType(elementType).IsAssignableFrom(type))
                {
                    var rightSegment = this.VisitAndDeferred(new SqlFieldSegment { Expression = methodCallExpr.Arguments[1] });
                    if (rightSegment == SqlFieldSegment.Null)
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
                    if (typeof(IQuery<>).MakeGenericType(elementType).IsAssignableFrom(type))
                    {
                        var queryObj = this.Evaluate(methodCallExpr.Arguments[1]) as IQuery;
                        if (queryObj is ICteQuery cteQuery)
                            queryObj.Visitor.IsUseCteTable = false;
                        inSql = queryObj.Visitor.BuildSql(out _);
                        queryObj.CopyTo(this);
                    }
                    else
                    {
                        lambdaExpr = this.EnsureLambda(methodCallExpr.Arguments[1]);
                        inSql = this.VisitFromQuery(lambdaExpr);
                    }
                }
                var fieldArgument = this.GetQuotedValue(fieldSegment);
                if (sqlSegment.HasDeferrdNot())
                    sqlSegment.Change($"{fieldArgument} NOT IN ({inSql})");
                else sqlSegment.Change($"{fieldArgument} IN ({inSql})");
                break;
            case "Exists":
            case "ExistsAsync":
                string existsSql = null;
                if (methodCallExpr.Method.DeclaringType == typeof(Sql))
                {
                    if (typeof(MulticastDelegate).IsAssignableFrom(methodCallExpr.Arguments[0].Type))
                    {
                        //Exists<TTarget>(Func<IFromQuery, IQuery<TTarget>> subQuery)
                        lambdaExpr = this.EnsureLambda(methodCallExpr.Arguments[0]);
                        existsSql = this.VisitFromQuery(lambdaExpr);
                    }
                    else
                    {
                        var genericArguments = methodCallExpr.Method.GetGenericArguments();
                        //保存现场，临时添加这几个新表及别名，解析之后再删除
                        var removeTables = new List<TableSegment>();
                        var builder = new StringBuilder("SELECT * FROM ");
                        int index = 0;
                        //Exists<T>(ICteQuery<T> subQuery, Expression<Func<T, bool>> predicate)
                        if (methodCallExpr.Arguments.Count > 1)
                        {
                            lambdaExpr = this.EnsureLambda(methodCallExpr.Arguments[1]);
                            var cteQuery = this.Evaluate(methodCallExpr.Arguments[0]) as ICteQuery;
                            methodCallExpr.Arguments[1].GetParameterNames(out var parameterNames);
                            foreach (var parameterName in parameterNames)
                            {
                                if (this.TableAliases.ContainsKey(parameterName))
                                    continue;
                                var tableSegment = new TableSegment
                                {
                                    TableType = TableType.CteSelfRef,
                                    EntityType = genericArguments[0],
                                    AliasName = parameterName,
                                    Fields = cteQuery.ReaderFields,
                                    Body = cteQuery.Body
                                };
                                this.TableAliases.TryAdd(parameterName, tableSegment);
                                removeTables.Add(tableSegment);
                                builder.Append(this.OrmProvider.GetTableName(cteQuery.TableName));
                                builder.Append($" {parameterName}");
                            }
                            cteQuery.CopyTo(this);
                        }
                        else
                        {
                            //Exists<T1, T2>(Expression<Func<T1, T2, bool>> predicate)
                            lambdaExpr = this.EnsureLambda(methodCallExpr.Arguments[0]);
                            foreach (var tableType in genericArguments)
                            {
                                var aliasName = lambdaExpr.Parameters[index].Name;
                                if (this.TableAliases.ContainsKey(aliasName))
                                    continue;

                                var tableMapper = this.MapProvider.GetEntityMap(tableType);
                                var tableSegment = new TableSegment
                                {
                                    EntityType = tableType,
                                    AliasName = aliasName,
                                    Mapper = tableMapper
                                };
                                this.Tables.Add(tableSegment);
                                this.TableAliases.TryAdd(aliasName, tableSegment);
                                removeTables.Add(tableSegment);
                                if (index > 0) builder.Append(',');
                                builder.Append(this.OrmProvider.GetTableName(tableMapper.TableName));
                                builder.Append($" {tableSegment.AliasName}");
                                index++;
                            }
                        }
                        builder.Append(" WHERE ");
                        builder.Append(this.VisitConditionExpr(lambdaExpr.Body, out _));

                        //恢复现场
                        if (removeTables.Count > 0)
                        {
                            removeTables.ForEach(f =>
                            {
                                this.Tables.Remove(f);
                                this.TableAliases.Remove(f.AliasName);
                            });
                        }
                        existsSql = builder.ToString();
                    }
                }
                else if (methodCallExpr.GetParameters(out var parameters))
                {
                    lambdaExpr = Expression.Lambda(methodCallExpr, parameters);
                    existsSql = this.VisitFromQuery(lambdaExpr);
                }
                if (sqlSegment.HasDeferrdNot())
                    sqlSegment.Change($"NOT EXISTS({existsSql})", false, true);
                else sqlSegment.Change($"EXISTS({existsSql})", false, true);
                break;
            case "Count":
            case "LongCount":
                if (methodCallExpr.Arguments != null && methodCallExpr.Arguments.Count == 1)
                {
                    sqlSegment = this.Visit(sqlSegment.Next(methodCallExpr.Arguments[0]));
                    sqlSegment.Change($"COUNT({sqlSegment.Body})", false, true);
                }
                else sqlSegment.Change("COUNT(1)", false, true);
                break;
            case "CountDistinct":
            case "LongCountDistinct":
                if (methodCallExpr.Arguments != null && methodCallExpr.Arguments.Count == 1)
                {
                    sqlSegment = this.Visit(sqlSegment.Next(methodCallExpr.Arguments[0]));
                    sqlSegment.Change($"COUNT(DISTINCT {sqlSegment.Body})", false, true);
                }
                break;
            case "Sum":
                if (methodCallExpr.Arguments != null && methodCallExpr.Arguments.Count == 1)
                {
                    sqlSegment = this.Visit(sqlSegment.Next(methodCallExpr.Arguments[0]));
                    sqlSegment.Change($"SUM({sqlSegment.Body})", false, true);
                }
                break;
            case "Avg":
                if (methodCallExpr.Arguments != null && methodCallExpr.Arguments.Count == 1)
                {
                    sqlSegment = this.Visit(sqlSegment.Next(methodCallExpr.Arguments[0]));
                    sqlSegment.Change($"AVG({sqlSegment.Body})", false, true);
                }
                break;
            case "Max":
                if (methodCallExpr.Arguments != null && methodCallExpr.Arguments.Count == 1)
                {
                    sqlSegment = this.Visit(sqlSegment.Next(methodCallExpr.Arguments[0]));
                    sqlSegment.Change($"MAX({sqlSegment.Body})", false, true);
                }
                break;
            case "Min":
                if (methodCallExpr.Arguments != null && methodCallExpr.Arguments.Count == 1)
                {
                    sqlSegment = this.Visit(sqlSegment.Next(methodCallExpr.Arguments[0]));
                    sqlSegment.Change($"MIN({sqlSegment.Body})", false, true);
                }
                break;
        }
        return sqlSegment;
    }
    public virtual bool IsStringConcatOperator(SqlFieldSegment sqlSegment, out SqlFieldSegment result)
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
    public virtual string VisitConditionExpr(Expression conditionExpr, out OperationType operationType)
    {
        operationType = OperationType.And;
        SqlFieldSegment sqlSegment = null;
        if (conditionExpr.NodeType == ExpressionType.AndAlso || conditionExpr.NodeType == ExpressionType.OrElse)
        {
            var completedExprs = this.VisitLogicBinaryExpr(conditionExpr);
            if (conditionExpr.NodeType == ExpressionType.OrElse)
                operationType = OperationType.Or;

            var builder = new StringBuilder();
            foreach (var completedExpr in completedExprs)
            {
                if (completedExpr.ExpressionType == ConditionType.OperatorType)
                {
                    builder.Append(completedExpr.Body);
                    continue;
                }
                sqlSegment = this.VisitAndDeferred(this.CreateConditionSegment(completedExpr.Body as Expression));
                builder.Append(this.GetQuotedValue(sqlSegment));
            }
            return builder.ToString();
        }
        sqlSegment = this.VisitAndDeferred(this.CreateConditionSegment(conditionExpr));
        return sqlSegment.Body;
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
        var callStack = new Stack<MethodCallExpression>();
        IQueryVisitor queryVisitor = null;
        FromQuery fromQuery = null;
        DbContext dbContext = null;
        IQuery queryObj = null;
        while (true)
        {
            if (currentExpr is not MethodCallExpression callExpr)
            {
                if (currentExpr.NodeType == ExpressionType.Parameter)
                {
                    queryVisitor = this.CreateQueryVisitor();
                    fromQuery = new FromQuery(this.OrmProvider, this.MapProvider, queryVisitor, this.IsParameterized);
                    dbContext = fromQuery.dbContext;
                    break;
                }
                if (currentExpr is MemberExpression memberExpr)
                {
                    var sqlSegment = this.VisitMemberAccess(new SqlFieldSegment { Expression = memberExpr });
                    if (sqlSegment.Value is IRepository)
                    {
                        queryVisitor = this.CreateQueryVisitor();
                        fromQuery = new FromQuery(this.OrmProvider, this.MapProvider, queryVisitor, this.IsParameterized);
                        dbContext = fromQuery.dbContext;
                    }
                    else
                    {
                        queryObj = sqlSegment.Value as IQuery;
                        queryVisitor = queryObj.Visitor;
                        queryVisitor.TableAsStart = (char)(this.TableAsStart + this.Tables.Count);
                    }
                }
                break;
            }
            callStack.Push(callExpr);
            currentExpr = callExpr.Object;
        }
        char tableAsStart = 'a';
        while (callStack.TryPop(out var callExpr))
        {
            var methodInfo = callExpr.Method;
            var genericArguments = methodInfo.GetGenericArguments();
            Type entityType = null;
            LambdaExpression lambdaArgsExpr = null;
            switch (methodInfo.Name)
            {
                case "From":
                    if (callExpr.Arguments.Count > 0)
                        tableAsStart = this.Evaluate<char>(callExpr.Arguments[0]);
                    queryVisitor.From(tableAsStart, genericArguments);
                    break;
                case "Union":
                case "UnionAll":
                    var unionParameters = this.Evaluate(callExpr.Arguments[0]);
                    if (unionParameters is Delegate subQueryGetter)
                        queryVisitor.Union(" " + callExpr.Method.Name.ToUpper(), genericArguments[0], dbContext, subQueryGetter);
                    else queryVisitor.Union(" " + callExpr.Method.Name.ToUpper(), genericArguments[0], unionParameters as IQuery);
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
                    lambdaArgsExpr = this.EnsureLambda(callExpr.Arguments[0]);
                    queryVisitor.RefTableAliases = this.TableAliases;
                    if (genericArguments.Length > 0)
                        queryVisitor.Join(joinType, genericArguments[0], lambdaArgsExpr);
                    else queryVisitor.Join(joinType, lambdaArgsExpr);
                    queryVisitor.RefTableAliases = null;
                    break;
                case "Where":
                case "And":
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
                        if (methodInfo.Name == "Where")
                            queryVisitor.Where(lambdaArgsExpr);
                        else queryVisitor.And(lambdaArgsExpr);
                        queryVisitor.RefTableAliases = null;
                    }
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
                case "Select":
                    if (callExpr.Arguments.Count > 0)
                    {
                        lambdaArgsExpr = this.EnsureLambda(callExpr.Arguments[0]);
                        queryVisitor.Select(null, lambdaArgsExpr);
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
                            var predicateExpr = Expression.Lambda(funcType, parameterExpr, parameterExpr);
                            lambdaArgsExpr = this.EnsureLambda(predicateExpr);
                            queryVisitor.Select(null, lambdaArgsExpr);
                        }
                    }
                    break;
                case "SelectAggregate":
                    lambdaArgsExpr = this.EnsureLambda(callExpr.Arguments[0]);
                    queryVisitor.Select(null, lambdaArgsExpr);
                    break;
                case "SelectAnonymous":
                    queryVisitor.Select("*");
                    break;
                case "SelectFlattenTo":
                    if (callExpr.Arguments.Count > 0)
                        lambdaArgsExpr = this.EnsureLambda(callExpr.Arguments[0]);
                    queryVisitor.SelectFlattenTo(genericArguments[0], lambdaArgsExpr);
                    break;
                case "Distinct":
                    queryVisitor.Distinct();
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
                case "UseTable":
                    entityType = methodInfo.DeclaringType.GetGenericArguments().Last();
                    var parameterInfos = methodInfo.GetParameters();
                    if (parameterInfos[0].ParameterType.IsArray)
                    {
                        var tableNames = this.Evaluate<string[]>(callExpr.Arguments[0]);
                        queryVisitor.UseTable(false, tableNames);
                    }
                    else throw new NotSupportedException("不支持的方法调用");
                    break;
                case "UseTableBy":
                    var args0 = this.Evaluate(callExpr.Arguments[0]);
                    object args1 = null;
                    if (callExpr.Arguments.Count > 1)
                        args1 = this.Evaluate(callExpr.Arguments[1]);
                    entityType = methodInfo.DeclaringType.GetGenericArguments().Last();
                    queryVisitor.UseTableBy(false, args0, args1);
                    break;
                case "Exists":
                case "ExistsAsync":
                    lambdaArgsExpr = this.EnsureLambda(callExpr.Arguments[0]);
                    var builder = new StringBuilder("SELECT * FROM ");
                    if (genericArguments.Length > 0)
                    {
                        foreach (var argsExpr in lambdaArgsExpr.Parameters)
                        {
                            queryVisitor.From(argsExpr.Name[0], argsExpr.Type);
                        }
                    }
                    queryVisitor.RefTableAliases = this.TableAliases;
                    queryVisitor.Where(lambdaArgsExpr);
                    queryVisitor.Select("*");
                    queryVisitor.RefTableAliases = null;
                    break;
                default:
                    throw new NotSupportedException("不支持的表达式解析");
            }
        }
        queryObj.CopyTo(this);
        return queryVisitor.BuildSql(out _);
    }
    public virtual string GetQuotedValue(SqlFieldSegment sqlSegment, bool isNeedExprWrap = false)
    {
        //默认只要是变量就设置为参数
        if (sqlSegment.IsVariable || (this.IsParameterized || sqlSegment.IsParameterized) && sqlSegment.IsConstant)
        {
            var dbParameters = this.DbParameters;
            if (this.IsIncludeMany)
            {
                this.NextDbParameters ??= new TheaDbParameterCollection();
                dbParameters = this.NextDbParameters;
            }
            var parameterName = sqlSegment.ParameterName ?? this.OrmProvider.ParameterPrefix + this.ParameterPrefix + dbParameters.Count.ToString();
            if (this.IsMultiple) parameterName += $"_m{this.CommandIndex}";

            if (sqlSegment.Value == null || sqlSegment.Value == DBNull.Value)
                dbParameters.Add(this.OrmProvider.CreateParameter(parameterName, DBNull.Value));
            else
            {
                var dbFieldValue = sqlSegment.Value;
                if (sqlSegment.ExpectType != null && sqlSegment.SegmentType != sqlSegment.ExpectType)
                {
                    if (sqlSegment.ExpectType.IsEnum)
                        dbFieldValue = Enum.ToObject(sqlSegment.ExpectType, dbFieldValue);
                    else dbFieldValue = Convert.ChangeType(dbFieldValue, sqlSegment.ExpectType);
                    sqlSegment.SegmentType = sqlSegment.ExpectType;
                }
                if (sqlSegment.TypeHandler != null)
                {
                    //枚举类型或是有强制转换时，要取sqlSegment.ExpectType值
                    //常量、方法调用、计算表达式时，sqlSegment.FromMember没有值，只能取Expression.Type值
                    dbFieldValue = sqlSegment.TypeHandler.ToFieldValue(this.OrmProvider, dbFieldValue);
                    if (sqlSegment.NativeDbType != null)
                        dbParameters.Add(this.OrmProvider.CreateParameter(parameterName, sqlSegment.NativeDbType, dbFieldValue));
                    else dbParameters.Add(this.OrmProvider.CreateParameter(parameterName, dbFieldValue));
                }
                //常量、方法调用、表达式计算等场景
                else
                {
                    if (sqlSegment.NativeDbType != null)
                    {
                        var targetType = this.OrmProvider.MapDefaultType(sqlSegment.NativeDbType);
                        if (sqlSegment.SegmentType != targetType)
                        {
                            var valueGetter = this.OrmProvider.GetParameterValueGetter(sqlSegment.SegmentType, targetType, false);
                            dbFieldValue = valueGetter.Invoke(dbFieldValue);
                            sqlSegment.SegmentType = targetType;
                        }
                        dbParameters.Add(this.OrmProvider.CreateParameter(parameterName, sqlSegment.NativeDbType, dbFieldValue));
                    }
                    else dbParameters.Add(this.OrmProvider.CreateParameter(parameterName, dbFieldValue));
                }
            }
            //清空指定的参数化名称
            if (sqlSegment.IsParameterized)
            {
                sqlSegment.ParameterName = null;
                sqlSegment.IsParameterized = false;
            }

            sqlSegment.Body = parameterName;
            sqlSegment.HasParameter = true;
            sqlSegment.IsVariable = false;
            sqlSegment.IsConstant = false;
            return parameterName;
        }
        else if (sqlSegment.IsConstant)
        {
            var dbFieldValue = sqlSegment.Value;
            if (dbFieldValue is string strFieldValue && strFieldValue == "*")
            {
                sqlSegment.Body = strFieldValue;
                return sqlSegment.Body;
            }
            if (sqlSegment.ExpectType != null && sqlSegment.SegmentType != sqlSegment.ExpectType)
            {
                if (sqlSegment.ExpectType.IsEnum)
                    dbFieldValue = Enum.ToObject(sqlSegment.ExpectType, dbFieldValue);
                else dbFieldValue = Convert.ChangeType(dbFieldValue, sqlSegment.ExpectType);
                sqlSegment.SegmentType = sqlSegment.ExpectType;
            }
            string body = null;
            if (sqlSegment.TypeHandler != null)
                body = sqlSegment.TypeHandler.GetQuotedValue(this.OrmProvider, dbFieldValue);
            else
            {
                var targetType = sqlSegment.SegmentType;
                if (sqlSegment.NativeDbType != null)
                {
                    targetType = this.OrmProvider.MapDefaultType(sqlSegment.NativeDbType);
                    if (sqlSegment.SegmentType != targetType)
                    {
                        var valueGetter = this.OrmProvider.GetParameterValueGetter(sqlSegment.SegmentType, targetType, false);
                        dbFieldValue = valueGetter.Invoke(dbFieldValue);
                    }
                }
                body = this.OrmProvider.GetQuotedValue(targetType, dbFieldValue);
            }
            sqlSegment.Body = body;
            return body;
        }
        //带有参数或字段的表达式或函数调用、或是只有参数或字段
        //本地函数调用返回值，非常量、变量、字段、SQL函数调用
        if (isNeedExprWrap && sqlSegment.IsExpression)
        {
            sqlSegment.Body = $"({sqlSegment.Body})";
            sqlSegment.IsExpression = false;
            sqlSegment.IsMethodCall = true;
            return sqlSegment.Body;
        }
        return sqlSegment.Body;
    }
    public virtual string GetQuotedValue(object elementValue, SqlFieldSegment arraySegment, SqlFieldSegment elementSegment)
    {
        if (elementValue is DBNull || elementValue == null)
            return "NULL";
        if (arraySegment.IsVariable || (this.IsParameterized || arraySegment.IsParameterized) && arraySegment.IsConstant)
        {
            var dbParameters = this.DbParameters;
            if (this.IsIncludeMany)
            {
                this.NextDbParameters ??= new TheaDbParameterCollection();
                dbParameters = this.NextDbParameters;
            }
            var parameterName = this.OrmProvider.ParameterPrefix + this.ParameterPrefix + dbParameters.Count.ToString();
            if (this.IsMultiple) parameterName += $"_m{this.CommandIndex}";

            if (elementValue == null || elementValue == DBNull.Value)
                dbParameters.Add(this.OrmProvider.CreateParameter(parameterName, DBNull.Value));
            else
            {
                var dbFieldValue = elementValue;
                var expectType = elementSegment.ExpectType;
                var segmentType = elementSegment.SegmentType;
                var nativeDbType = elementSegment.NativeDbType;
                var typeHandler = elementSegment.TypeHandler;

                if (expectType != null && segmentType != expectType)
                {
                    dbFieldValue = Enum.ToObject(expectType, dbFieldValue);
                    segmentType = expectType;
                }
                if (typeHandler != null)
                {
                    dbFieldValue = typeHandler.ToFieldValue(this.OrmProvider, dbFieldValue);
                    if (nativeDbType != null)
                        dbParameters.Add(this.OrmProvider.CreateParameter(parameterName, nativeDbType, dbFieldValue));
                    else dbParameters.Add(this.OrmProvider.CreateParameter(parameterName, dbFieldValue));
                }
                else
                {
                    if (nativeDbType != null)
                    {
                        var targetType = this.OrmProvider.MapDefaultType(nativeDbType);
                        if (segmentType != targetType)
                        {
                            var valueGetter = this.OrmProvider.GetParameterValueGetter(segmentType, targetType, false);
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

            if (expectType != null && segmentType != expectType)
            {
                dbFieldValue = Enum.ToObject(expectType, dbFieldValue);
                segmentType = expectType;
            }
            if (typeHandler != null)
                return typeHandler.GetQuotedValue(this.OrmProvider, dbFieldValue);
            //常量、方法调用、表达式计算等场景
            else
            {
                var targetType = segmentType;
                if (nativeDbType != null)
                {
                    targetType = this.OrmProvider.MapDefaultType(nativeDbType);
                    if (segmentType != targetType)
                    {
                        var valueGetter = this.OrmProvider.GetParameterValueGetter(segmentType, targetType, false);
                        dbFieldValue = valueGetter.Invoke(dbFieldValue);
                    }
                }
                return this.OrmProvider.GetQuotedValue(targetType, dbFieldValue);
            }
        }
        //此场景走不到，通常是常量和变量
        return this.OrmProvider.GetQuotedValue(elementValue);
    }
    public virtual string ChangeParameterValue(SqlFieldSegment sqlSegment, Type targetType)
    {
        var dbParameter = this.DbParameters[sqlSegment.Body] as IDbDataParameter;
        this.OrmProvider.ChangeParameter(dbParameter, targetType, sqlSegment.Value);
        return sqlSegment.Body;
    }
    public virtual IQueryVisitor CreateQueryVisitor()
    {
        var queryVisiter = this.OrmProvider.NewQueryVisitor(this.DbContext, this.TableAsStart, this.DbParameters);
        queryVisiter.IsMultiple = this.IsMultiple;
        queryVisiter.CommandIndex = this.CommandIndex;
        queryVisiter.RefQueries = this.RefQueries;
        queryVisiter.ShardingTables = this.ShardingTables;
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
    public SqlFieldSegment VisitDeferredBoolConditional(SqlFieldSegment sqlSegment, bool isExpectBooleanType, string ifTrueValue, string ifFalseValue)
    {
        //处理HasValue !逻辑取反操作，这种情况下是一元操作
        int notIndex = 0;
        SqlFieldSegment deferredSegment = null;
        //复杂bool条件判断，有IS NOT NULL, <> != 两种情况，只能在
        while (sqlSegment.TryPop(out var deferredExpr))
        {
            switch (deferredExpr.OperationType)
            {
                case OperationType.Equal:
                    deferredSegment = deferredExpr.Value as SqlFieldSegment;
                    break;
                case OperationType.Not:
                    notIndex++;
                    break;
            }
        }
        if (deferredSegment == null)
            deferredSegment = SqlFieldSegment.True;

        string strOperator = null;
        if (notIndex % 2 > 0)
            strOperator = deferredSegment == SqlFieldSegment.Null ? "IS NOT" : "<>";
        else strOperator = deferredSegment == SqlFieldSegment.Null ? "IS" : "=";

        string strExpression = null;
        if (!sqlSegment.IsExpression && (this.IsWhere || this.IsSelect))
        {
            string leftArgument = sqlSegment.Body;
            if (sqlSegment.IsConstant || sqlSegment.IsVariable)
                leftArgument = this.GetQuotedValue(sqlSegment);
            if (deferredSegment == SqlFieldSegment.Null)
                strExpression = $"{leftArgument} {strOperator} {deferredSegment.Body}";
            else strExpression = $"{leftArgument}{strOperator}{this.OrmProvider.GetQuotedValue(typeof(bool), deferredSegment.Value)}";
        }
        else strExpression = sqlSegment.Body;
        if (this.IsSelect || (this.IsWhere && !isExpectBooleanType))
            strExpression = $"CASE WHEN {strExpression} THEN {ifTrueValue} ELSE {ifFalseValue} END";
        return sqlSegment.Change(strExpression);
    }
    public List<SqlFieldSegment> FlattenTableFields(TableSegment tableSegment)
    {
        var targetFields = new List<SqlFieldSegment>();
        if (tableSegment.Mapper != null)
        {
            //Select参数时，Flatten实体表
            foreach (var memberMapper in tableSegment.Mapper.MemberMaps)
            {
                if (memberMapper.IsIgnore || memberMapper.IsNavigation
                    || (memberMapper.MemberType.IsEntityType(out _) && memberMapper.TypeHandler == null))
                    continue;
                targetFields.Add(new SqlFieldSegment
                {
                    FieldType = SqlFieldType.Field,
                    TableSegment = tableSegment,
                    FromMember = memberMapper.Member,
                    TargetMember = memberMapper.Member,
                    SegmentType = memberMapper.MemberType,
                    NativeDbType = memberMapper.NativeDbType,
                    TypeHandler = memberMapper.TypeHandler,
                    Body = tableSegment.AliasName + "." + this.OrmProvider.GetFieldName(memberMapper.FieldName)
                });
            }
        }
        else
        {
            //Select参数时，Flatten子查询表
            targetFields.AddRange(tableSegment.Fields);
        }
        return targetFields;
    }

    public bool IsDateTimeOperator(SqlFieldSegment sqlSegment, out SqlFieldSegment result)
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
    public bool IsTimeSpanOperator(SqlFieldSegment sqlSegment, out SqlFieldSegment result)
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
    public DataTable ToDataTable(Type parameterType, IEnumerable entities, List<(MemberMap RefMemberMapper, Func<object, object> ValueGetter)> memberMappers, string tableName = null)
    {
        var result = new DataTable();
        result.TableName = tableName;
        foreach (var memberMapper in memberMappers)
        {
            var refMemberMapper = memberMapper.RefMemberMapper;
            var targetType = this.OrmProvider.MapDefaultType(refMemberMapper.NativeDbType);
            result.Columns.Add(refMemberMapper.FieldName, targetType);
        }
        foreach (var entity in entities)
        {
            var row = new object[memberMappers.Count];
            for (var i = 0; i < memberMappers.Count; i++)
            {
                var memberMapper = memberMappers[i].RefMemberMapper;
                row[i] = memberMappers[i].ValueGetter.Invoke(entity);
            }
            result.Rows.Add(row);
        }
        return result;
    }
    public List<(MemberMap RefMemberMapper, Func<object, object> ValueGetter)> GetRefMemberMappers(Type parameterType, EntityMap refEntityMapper, bool isUpdate = false)
    {
        var memberInfos = parameterType.GetMembers(BindingFlags.Public | BindingFlags.Instance)
            .Where(f => f.MemberType == MemberTypes.Property | f.MemberType == MemberTypes.Field).ToList();
        var memberMappers = new List<(MemberMap, Func<object, object>)>();
        foreach (var refMemberMapper in refEntityMapper.MemberMaps)
        {
            if (refMemberMapper.IsIgnore || refMemberMapper.IsIgnoreInsert
                || refMemberMapper.IsNavigation || refMemberMapper.IsAutoIncrement || refMemberMapper.IsRowVersion
                || (refMemberMapper.MemberType.IsEntityType(out _) && refMemberMapper.TypeHandler == null))
                continue;
            var memberInfo = memberInfos.Find(f => f.Name == refMemberMapper.MemberName);
            if (isUpdate && memberInfo == null)
                continue;

            Func<object, object> valueGetter = null;
            var targetType = this.OrmProvider.MapDefaultType(refMemberMapper.NativeDbType);
            if (memberInfo == null) valueGetter = value => DBNull.Value;
            else
            {
                if (refMemberMapper.TypeHandler != null)
                {
                    valueGetter = value =>
                    {
                        var fieldValue = memberInfo.Evaluate(value);
                        return refMemberMapper.TypeHandler.ToFieldValue(this.OrmProvider, fieldValue);
                    };
                }
                else
                {
                    Func<object, object> typedValueGetter = null;
                    typedValueGetter = this.OrmProvider.GetParameterValueGetter(memberInfo.GetMemberType(), targetType, true);
                    valueGetter = value =>
                    {
                        var fieldValue = memberInfo.Evaluate(value);
                        return typedValueGetter.Invoke(fieldValue);
                    };
                }
            }
            memberMappers.Add((refMemberMapper, valueGetter));
        }
        return memberMappers;
    }
    public string GetTableName(TableSegment tableSegment)
    {
        string tableName = null;
        if (tableSegment.IsSharding)
        {
            //当单个ShardingTables时，只有一个分表的情况下，会移除ShardingTables中的表，存在多个分表的表时，不做移除
            if (tableSegment.ShardingType > ShardingTableType.SingleTable
                && (tableSegment.TableType == TableType.Entity || tableSegment.TableType == TableType.Include))
            {
                if (!this.ShardingTables.Contains(tableSegment))
                {
                    tableSegment.ShardingId = Guid.NewGuid().ToString("N");
                    this.ShardingTables.Add(tableSegment);
                }
                tableName = $"__SHARDING_{tableSegment.ShardingId}_{tableSegment.Mapper.TableName}";
            }
            //单个明确分表或是有分表的子查询
            else tableName = tableSegment.Body;
        }
        //子查询场景，tableSegment.Body有值
        else tableName = tableSegment.Body ?? tableSegment.Mapper.TableName;
        if (tableSegment.TableType != TableType.FromQuery)
        {
            //支持TableSchema
            if (!string.IsNullOrEmpty(tableSegment.TableSchema))
                tableName = tableSegment.TableSchema + "." + tableName;
            tableName = this.OrmProvider.GetTableName(tableName);
        }
        return tableName;
    }
    public virtual SqlFieldSegment BuildDeferredSqlSegment(MethodCallExpression methodCallExpr, SqlFieldSegment sqlSegment)
    {
        string fields = null;
        List<SqlFieldSegment> readerFields = null;
        LambdaExpression deferredFuncExpr = null;
        if (methodCallExpr.Arguments != null && methodCallExpr.Arguments.Count > 0)
        {
            readerFields = new List<SqlFieldSegment>();
            var builder = new StringBuilder();
            var visitor = new ReplaceParameterVisitor();
            var bodyExpr = visitor.Visit(methodCallExpr);

            //f.Balance.ToString("C")
            //args0.ToString("C")
            //(args0)=>{args0.ToString("C")}          
            if (visitor.NewParameters.Count > 0)
                deferredFuncExpr = Expression.Lambda(bodyExpr, visitor.NewParameters.ToArray());

            foreach (var argsExpr in visitor.OrgMembers)
            {
                var argumentSegment = this.VisitAndDeferred(new SqlFieldSegment { Expression = argsExpr });
                if (argumentSegment.HasField)
                {
                    sqlSegment.HasField = true;
                    var fieldName = argumentSegment.Body;
                    readerFields.Add(new SqlFieldSegment
                    {
                        SegmentType = argsExpr.Type,
                        TargetMember = argsExpr.Member,
                        NativeDbType = argumentSegment.NativeDbType,
                        TypeHandler = argumentSegment.TypeHandler
                    });
                    if (builder.Length > 0)
                        builder.Append(',');
                    builder.Append(fieldName);
                }
            }
            if (readerFields.Count > 0)
                fields = builder.ToString();
        }
        else deferredFuncExpr = Expression.Lambda(methodCallExpr);

        if (readerFields == null)
            fields = "NULL";
        var deferredDelegate = deferredFuncExpr.Compile();
        sqlSegment.IsDeferredFields = true;
        sqlSegment.FieldType = SqlFieldType.DeferredFields;
        sqlSegment.Body = fields;
        sqlSegment.DeferredDelegateType = deferredFuncExpr.Type;
        sqlSegment.DeferredDelegate = deferredDelegate;
        sqlSegment.Fields = readerFields;
        sqlSegment.IsMethodCall = true;
        return sqlSegment;
    }
    public virtual SqlFieldSegment ToEnumString(SqlFieldSegment sqlSegment)
    {
        if (sqlSegment.HasField)
        {
            var targetType = this.OrmProvider.MapDefaultType(sqlSegment.NativeDbType);
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
    public virtual void Dispose()
    {
        if (this.isDisposed)
            return;
        this.isDisposed = true;

        this.Tables = null;
        this.TableAliases = null;
        this.RefTableAliases = null;
        this.ReaderFields = null;
        this.WhereSql = null;
        this.GroupFields = null;
        this.IncludeTables = null;

        //设置null，不能清空，以免给返回的参数丢失
        this.DbParameters = null;
        this.NextDbParameters = null;
        this.DbContext = null;

        //应用子查询表，只删除元素，不能dispose，后续操作可能还会用到子查询
        this.RefQueries.Clear();
        this.IsUseMaster = false;
    }
    private List<ConditionExpression> VisitLogicBinaryExpr(Expression conditionExpr)
    {
        Func<Expression, bool> isConditionExpr = f => f.NodeType == ExpressionType.AndAlso || f.NodeType == ExpressionType.OrElse;

        int deep = 0, lastDeep = 0;
        var lastOperationTypes = new Stack<string>();
        string lastOperationType = string.Empty;
        var leftExprs = new Stack<ConditionExpression>();
        var completedExprs = new Stack<ConditionExpression>();
        var nextExpr = conditionExpr as BinaryExpression;
        lastOperationType = nextExpr.NodeType == ExpressionType.AndAlso ? " AND " : " OR ";

        while (nextExpr != null)
        {
            var operationType = nextExpr.NodeType == ExpressionType.AndAlso ? " AND " : " OR ";
            if (lastOperationType != operationType)
                deep++;

            //先从最右边解析，从右边第一个简单的条件开始
            //如果是复合条件，就把左半部分+当前操作符号压进leftExprs中，等待右边解析完后，再做解析
            //先计算有几个操作符变化，变化一次就deep就++
            if (isConditionExpr(nextExpr.Right))
            {
                //右边是复合条件，先把左侧表达式、操作符、deep都压进去，等待解析
                leftExprs.Push(new ConditionExpression
                {
                    ExpressionType = ConditionType.Expression,
                    Body = nextExpr.Left
                });
                leftExprs.Push(new ConditionExpression
                {
                    ExpressionType = ConditionType.OperatorType,
                    Body = (operationType, deep)
                });
                lastOperationType = operationType;
                lastDeep = deep;
                nextExpr = nextExpr.Right as BinaryExpression;
                continue;
            }
            //从左右边的符合条件
            //先压进右括号         
            for (int i = deep; i > lastDeep; i--)
            {
                completedExprs.Push(new ConditionExpression
                {
                    ExpressionType = ConditionType.OperatorType,
                    Body = ")"
                });
            }
            //再压进右侧表达式
            completedExprs.Push(new ConditionExpression
            {
                ExpressionType = ConditionType.Expression,
                Body = nextExpr.Right
            });
            //再压进当前操作符
            completedExprs.Push(new ConditionExpression
            {
                ExpressionType = ConditionType.OperatorType,
                Body = operationType
            });
            //计算左边表达式，如果是复杂条件，再重新解析左边的表达式
            if (isConditionExpr(nextExpr.Left))
            {
                nextExpr = nextExpr.Left as BinaryExpression;
                lastOperationType = operationType;
                lastDeep = deep;
                continue;
            }

            //预取下一个操作符和deep，用于判断要收尾几个左括号
            //如果取不到数据，说明到最后了，解析本轮就结束了
            int nextDeep = 0;
            if (leftExprs.TryPeek(out var deferredOperator))
                (_, nextDeep) = ((string, int))deferredOperator.Body;

            //左边也是简单表达式条件，先把左侧表达式压进去
            completedExprs.Push(new ConditionExpression
            {
                ExpressionType = ConditionType.Expression,
                Body = nextExpr.Left
            });

            //再压进左括号
            for (int i = deep; i > nextDeep; i--)
            {
                completedExprs.Push(new ConditionExpression
                {
                    ExpressionType = ConditionType.OperatorType,
                    Body = "("
                });
            }
            //当前表达式都解析完了，开始下一个新的表达式解析


            //如果有待处理的条件表达式解析，开始解析
            if (leftExprs.Count > 0)
            {
                //先更新lastOperationType，lastDeep
                lastOperationType = operationType;
                lastDeep = deep;

                //重新获取操作符，更新为当前操作符，deep
                if (leftExprs.TryPop(out deferredOperator))
                    (operationType, deep) = ((string, int))deferredOperator.Body;

                //先把当前的操作符压进去
                completedExprs.Push(new ConditionExpression
                {
                    ExpressionType = ConditionType.OperatorType,
                    Body = operationType
                });
                if (leftExprs.TryPop(out var deferredExpr))
                {
                    //更新操作符、deep
                    lastOperationType = operationType;
                    lastDeep = deep;
                    var typedDeferredExpr = deferredExpr.Body as Expression;

                    //继续解析当前表达式
                    if (isConditionExpr(typedDeferredExpr))
                    {
                        nextExpr = typedDeferredExpr as BinaryExpression;
                        continue;
                    }
                    completedExprs.Push(new ConditionExpression
                    {
                        ExpressionType = ConditionType.Expression,
                        Body = typedDeferredExpr
                    });
                    break;
                }
            }
            else break;
        }
        var conditionExprs = new List<ConditionExpression>();
        while (completedExprs.TryPop(out var completedExpr))
        {
            conditionExprs.Add(completedExpr);
        }
        return conditionExprs;
    }
    private SqlFieldSegment CreateConditionSegment(Expression conditionExpr)
    {
        var sqlSegment = new SqlFieldSegment { Expression = conditionExpr };
        if (conditionExpr.NodeType == ExpressionType.MemberAccess && conditionExpr.Type == typeof(bool))
        {
            sqlSegment.DeferredExprs ??= new();
            sqlSegment.DeferredExprs.Push(new DeferredExpr { OperationType = OperationType.Equal, Value = SqlFieldSegment.True });
        }
        return sqlSegment;
    }
    private bool IsSqlMethodCall(MethodCallExpression methodCallExpr)
    {
        if (methodCallExpr.Method.DeclaringType == typeof(Sql) || methodCallExpr.Method.DeclaringType == typeof(IRepository)
            || typeof(IAggregateSelect).IsAssignableFrom(methodCallExpr.Method.DeclaringType))
            return true;
        var methodInfo = methodCallExpr.Method;
        var declaringType = methodCallExpr.Method.DeclaringType;
        if (declaringType.IsGenericType && declaringType.FullName.StartsWith("Trolley.IQuery"))
            return true;
        return false;
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
