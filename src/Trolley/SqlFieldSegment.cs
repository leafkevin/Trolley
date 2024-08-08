using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Net.NetworkInformation;
using System.Reflection;

namespace Trolley;

public enum SqlFieldType : byte
{
    /// <summary>
    /// 字段
    /// </summary>
    Field,
    /// <summary>
    /// 实体类型，三种场景：参数访问，直接主表的Include导航属性成员访问，Grouping分组对象成员，返回的类型是ReaderField列表
    /// </summary>
    Entity,
    /// <summary>
    /// Include子表引用，场景: .Select(x => new { Order = x, CompanyInfo = x.Buyer.Company })
    /// </summary>
    IncludeRef,
    /// <summary>
    /// 先从数据库中查询连续的一个或多个字段，再执行函数调用返回一个字段
    /// </summary>
    DeferredFields
}

public class SqlFieldSegment
{
    private bool isFixValue = false;
    public static readonly SqlFieldSegment True = new SqlFieldSegment { isFixValue = true, IsConstant = true, Value = true, Body = "True" };
    public static readonly SqlFieldSegment Null = new SqlFieldSegment { isFixValue = true, Value = "NULL", Body = "NULL" };

    /// <summary>
    /// 操作符:None,Equal,Not,And,Or
    /// </summary>
    public OperationType OperationType { get; set; } = OperationType.None;
    /// <summary>
    /// 延迟表达式操作，通常是非或是不等于等操作
    /// </summary>
    public Stack<DeferredExpr> DeferredExprs { get; set; }

    /// <summary>
    /// 字段类型
    /// </summary>
    public SqlFieldType FieldType { get; set; }
    /// <summary>
    /// 所属的TableSegment，如：User表
    /// </summary>
    public TableSegment TableSegment { get; set; }
    /// <summary>
    /// 是否有字段
    /// </summary>
    public bool HasField { get; set; }
    /// <summary>
    /// 是否有SQL参数，如：@p1,@p2
    /// </summary>
    public bool HasParameter { get; set; }
    /// <summary>
    /// 是否是常量值
    /// </summary>
    public bool IsConstant { get; set; }
    /// <summary>
    /// 是否是变量
    /// </summary>
    public bool IsVariable { get; set; }
    /// <summary>
    /// 是否是表达式，二元表达式、字符串拼接等
    /// </summary>
    public bool IsExpression { get; set; }
    /// <summary>
    /// 是否方法调用
    /// </summary>
    public bool IsMethodCall { get; set; }
    /// <summary>
    /// 是否参数化当前值，本次解析有效
    /// </summary>
    public bool IsParameterized { get; set; }
    /// <summary>
    /// 是否是数组
    /// </summary>
    public bool IsArray { get; set; }
    /// <summary>
    /// 去掉Nullable后的枚举类型，此外无值
    /// </summary>
    public Type ExpectType { get; set; }
    /// <summary>
    /// 每次Visit时，当前表达式的Type，当发生强制转换，或是类型变更时，才会随之变化
    /// 单个字段访问时，就是这个字段的成员类型
    /// </summary>
    public Type SegmentType { get; set; }
    /// <summary>
    /// 是否可以做字段类型处理，用在a??b场景
    /// </summary>
    public bool IsFieldType { get; set; }
    /// <summary>
    /// 是否是最外层目标类型，通常用判断第一个字段是否是参数访问，并且只有一个字段，可以有include操作
    /// </summary>
    public bool IsTargetType { get; set; }
    /// <summary>
    /// 强制改变参数名称，会使用到
    /// </summary>
    public string ParameterName { get; set; }
    /// <summary>
    /// 原TableSegment表中的成员，Include子表的场景时，父亲对象中的成员，如：Order.Buyer成员，根据此成员信息设置主表属性值
    /// 每变更一次子查询，都会更改此成员值，用于最外层与TargetMember比较，是否AS别名
    /// </summary>
    public MemberInfo FromMember { get; set; }
    /// <summary>
    /// 最外层返回实体要设置的成员
    /// </summary>
    public MemberInfo TargetMember { get; set; }
    /// <summary>
    /// 单字段访问时，数据库字段的类型
    /// </summary>
    public object NativeDbType { get; set; }
    /// <summary>
    /// 单字段访问时，映射的TypeHandler
    /// </summary>
    public ITypeHandler TypeHandler { get; set; }
    /// <summary>
    /// 字段值、字段名称、方法调用或是表达式SQL片段，只有是字段值的时候，值不做处理，只有到最后一步才处理
    /// </summary>
    public object Value { get; set; }
    /// <summary>
    /// Value转变成后的SQL
    /// </summary>
    public string Body { get; set; }
    /// <summary>
    /// Include子表的主表SqlFieldSegment引用
    /// </summary>
    public SqlFieldSegment Parent { get; set; }
    /// <summary>
    /// 是否有后续的Include表，当前是主表SqlFieldSegment时且有Include表，此值为true
    /// </summary>
    public bool HasNextInclude { get; set; }
    /// <summary>
    /// 实体表或是子查询表的所有字段，FieldType为Entity时有值
    /// </summary>
    public List<SqlFieldSegment> Fields { get; set; }
    /// <summary>
    /// 是否DeferredFields,延迟方法调用
    /// </summary>
    public bool IsDeferredFields { get; set; }
    /// <summary>
    /// 延迟调用的委托
    /// </summary>
    public Delegate DeferredDelegate { get; set; }
    /// <summary>
    /// 延迟方法调用委托的类型
    /// </summary>
    public Type DeferredDelegateType { get; set; }
    /// <summary>
    /// 最外层Select时，原参数访问的路径，如：.Select(x => new { Order = x, x.Seller.Company })中的x, x.Seller.Company
    /// 当有Include导航属性成员访问时，查找其主表在Select返回的实体中的属性值，构造延迟属性设置方法
    /// 此处获取Company表字段信息，在Order属性中已经存在了，直接取里面的值，不再查询数据库，只做延迟属性值设置
    /// </summary>
    public string Path { get; set; }
    /// <summary>
    /// 是否需要alias别名
    /// </summary>
    public bool IsNeedAlias { get; set; }

    public Expression Expression { get; set; }
    public Expression OriginalExpression { get; set; }
    public bool HasDeferred => this.DeferredExprs != null && this.DeferredExprs.Count > 0;
    /// <summary>
    /// 常量、变量会调用此方法，默认是常量
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public SqlFieldSegment ChangeValue(object value)
    {
        this.Value = value;
        return this;
    }
    /// <summary>
    /// 常量、变量会调用此方法，默认是常量
    /// </summary>
    /// <param name="value"></param>
    /// <param name="isConstant"></param>
    /// <returns></returns>
    public SqlFieldSegment ChangeValue(object value, bool isConstant)
    {
        this.IsConstant = isConstant;
        this.IsVariable = !isConstant;
        this.Value = value;
        return this;
    }
    /// <summary>
    /// 在解析函数时使用，明确调用后的结果，表达式和方法调用，默认是表达式
    /// </summary>
    /// <param name="body"></param>
    /// <param name="isExpression"></param>
    /// <param name="isMethodCall"></param>
    /// <returns></returns>
    public SqlFieldSegment Change(string body, bool isExpression = true, bool isMethodCall = false)
    {
        this.IsConstant = false;
        this.IsVariable = false;
        this.IsExpression = isExpression;
        this.IsMethodCall = isMethodCall;
        this.Body = body;
        return this;
    }
    /// <summary>
    /// 在解析函数时使用，明确是常量或是变量，需要Merge一下IsConstant和IsVariable
    /// </summary>
    /// <param name="rightSegment"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public SqlFieldSegment MergeValue(SqlFieldSegment rightSegment, object value)
    {
        this.IsConstant = this.IsConstant && rightSegment.IsConstant;
        this.IsVariable = this.IsVariable || rightSegment.IsVariable;
        this.Value = value;
        return this;
    }
    /// <summary>
    /// 在解析函数时使用，明确是常量或是变量，需要Merge一下IsConstant和IsVariable
    /// </summary>
    /// <param name="leftSegment"></param>
    /// <param name="rightSegment"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public SqlFieldSegment MergeValue(SqlFieldSegment leftSegment, SqlFieldSegment rightSegment, object value)
    {
        this.IsConstant = this.IsConstant && leftSegment.IsConstant && rightSegment.IsConstant;
        this.IsVariable = this.IsVariable || leftSegment.IsVariable || rightSegment.IsVariable;
        this.Value = value;
        return this;
    }
    /// <summary>
    /// 在解析函数时使用，明确是常量或是变量，需要Merge一下IsConstant和IsVariable
    /// </summary>
    /// <param name="rightSegment"></param> 
    /// <returns></returns>
    public SqlFieldSegment Merge(SqlFieldSegment rightSegment)
    {
        this.IsConstant = this.IsConstant && rightSegment.IsConstant;
        this.IsVariable = this.IsVariable || rightSegment.IsVariable;
        return this;
    }
    /// <summary>
    /// 在解析函数时使用，明确调用后的结果，表达式或是方法调用，需要Merge一下HasField和IsParameter，默认是表达式
    /// </summary>
    /// <param name="rightSegment"></param>
    /// <param name="body"></param>
    /// <param name="isExpression"></param>
    /// <param name="isMethodCall"></param>
    /// <returns></returns>
    public SqlFieldSegment Merge(SqlFieldSegment rightSegment, string body, bool isExpression = true, bool isMethodCall = false)
    {
        this.IsConstant = false;
        this.IsVariable = false;
        this.HasField = this.HasField || rightSegment.HasField;
        this.HasParameter = this.HasParameter || rightSegment.HasParameter;
        this.IsExpression = isExpression;
        this.IsMethodCall = isMethodCall;
        this.Body = body;
        return this;
    }
    /// <summary>
    /// 在解析函数时使用，明确调用后的结果，表达式或是方法调用，需要Merge一下HasField和IsParameter，默认是表达式
    /// </summary>
    /// <param name="leftSegment"></param>
    /// <param name="rightSegment"></param>
    /// <param name="body"></param>
    /// <param name="isExpression"></param>
    /// <param name="isMethodCall"></param>
    /// <returns></returns>
    public SqlFieldSegment Merge(SqlFieldSegment leftSegment, SqlFieldSegment rightSegment, string body, bool isExpression = true, bool isMethodCall = false)
    {
        this.IsConstant = false;
        this.IsVariable = false;
        this.HasField = this.HasField || leftSegment.HasField || rightSegment.HasField;
        this.HasParameter = this.HasParameter || leftSegment.HasParameter || rightSegment.HasParameter;
        this.IsExpression = isExpression;
        this.IsMethodCall = isMethodCall;
        this.Body = body;
        return this;
    }
    public string ToExprWrap()
    {
        if (this.IsExpression)
            return $"({this.Body})";
        return this.Body;
    }
    public SqlFieldSegment Next(Expression nextExpr)
    {
        this.Expression = nextExpr;
        return this;
    }
    public bool HasDeferrdNot() => this.DeferredExprs.IsDeferredNot();
    public void Push(DeferredExpr deferredExpr)
    {
        this.DeferredExprs ??= new();
        this.DeferredExprs.Push(deferredExpr);
    }
    public bool TryPop(out DeferredExpr deferredExpr)
    {
        if (!this.HasDeferred)
        {
            deferredExpr = default;
            return false;
        }
        return this.DeferredExprs.TryPop(out deferredExpr);
    }
    /// <summary>
    /// CTE表被引用的时候才会使用克隆字段
    /// </summary>
    /// <returns></returns>
    public SqlFieldSegment Clone()
    {
        List<SqlFieldSegment> fields = null;
        if (this.Fields != null)
        {
            fields = new();
            this.Fields.ForEach(f => fields.Add(f.Clone()));
        }
        return new SqlFieldSegment
        {
            FieldType = this.FieldType,
            TableSegment = this.TableSegment,
            IsFieldType = this.IsFieldType,
            IsConstant = this.IsConstant,
            IsVariable = this.IsVariable,
            HasParameter = this.HasParameter,
            HasField = this.HasField,
            IsExpression = this.IsExpression,
            IsMethodCall = this.IsMethodCall,
            Fields = fields,
            Value = this.Value,
            Body = this.Body,
            IsDeferredFields = this.IsDeferredFields,
            DeferredDelegateType = this.DeferredDelegateType,
            DeferredDelegate = this.DeferredDelegate,
            SegmentType = this.SegmentType,
            TargetMember = this.TargetMember,
            FromMember = this.FromMember,
            IsTargetType = this.IsTargetType,
            HasNextInclude = this.HasNextInclude,
            NativeDbType = this.NativeDbType,
            TypeHandler = this.TypeHandler,
            Parent = this.Parent,
            Path = this.Path
        };
    }
    public override string ToString()
    {
        if (this.Value == null)
            throw new Exception("SqlFieldSegment.Value值不能为null，使用错误");
        return this.Value.ToString();
    }
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebugDisplayText => $"Value: {this.Value} Expression: {this.Expression}";
}