using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;

namespace Trolley;

[DebuggerDisplay("Value: {Value,nq}     Expression: {Expression,nq}")]
public class SqlSegment
{
    public static readonly SqlSegment True = new SqlSegment { isFixValue = true, OperationType = OperationType.None, IsConstant = true, Value = true };
    public static readonly SqlSegment Null = new SqlSegment { isFixValue = true, OperationType = OperationType.None, Value = "NULL" };
    private bool isFixValue = false;

    /// <summary>
    /// 操作符:None,Equal,Not,And,Or
    /// </summary>
    public OperationType OperationType { get; set; } = OperationType.None;
    public Stack<DeferredExpr> DeferredExprs { get; set; }
    public ReaderField Parent { get; set; }
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
    /// 是否必须添加别名
    /// </summary>
    public bool IsNeedAlias { get; set; }
    /// <summary>
    /// 是否DeferredFields,延迟方法调用
    /// </summary>
    public bool IsDeferredFields { get; set; }
    /// <summary>
    /// 是否参数化当前值，本次解析有效
    /// </summary>
    public bool IsParameterized { get; set; }
    public bool IsArray { get; set; }
    /// <summary>
    /// 是否是字段类型
    /// </summary>
    public bool IsFieldType { get; set; }
    /// <summary>
    /// 去掉Nullable后的枚举类型，此外无值
    /// </summary>
    public Type ExpectType { get; set; }
    /// <summary>
    /// 每次Visit时，当前Type，当发生强制转换，或是类型变更时，也随之变化
    /// </summary>
    public Type SegmentType { get; set; }
    public string ParameterName { get; set; }
    public TableSegment TableSegment { get; set; }
    public MemberInfo FromMember { get; set; }
    /// <summary>
    /// 当前是成员访问时，才有值，和FromMember是同一个栏位，是Mapper
    /// </summary>
    public MemberMap MemberMapper { get; set; }
    public object NativeDbType { get; set; }
    public ITypeHandler TypeHandler { get; set; }
    public object Value { get; set; }
    public Expression Expression { get; set; }
    public Expression OriginalExpression { get; set; }
    public bool HasDeferred => this.DeferredExprs != null && this.DeferredExprs.Count > 0;
    /// <summary>
    /// 只改变值
    /// </summary>
    /// <param name="value"></param>
    public SqlSegment Change(object value)
    {
        this.Value = value;
        return this;
    }
    /// <summary>
    /// 在解析函数时使用，明确调用后的结果，常量、变量、表达式和方法调用
    /// </summary>
    /// <param name="value"></param>
    /// <param name="isConstant"></param>
    /// <param name="isVariable"></param>
    /// <param name="isExpression"></param>
    /// <param name="isMethodCall"></param>
    /// <returns></returns>
    public SqlSegment Change(object value, bool isConstant, bool isVariable = false, bool isExpression = false, bool isMethodCall = false)
    {
        this.IsConstant = isConstant;
        this.IsVariable = isVariable;
        this.IsExpression = isExpression;
        this.IsMethodCall = isMethodCall;
        this.Value = value;
        return this;
    }
    /// <summary>
    /// 在解析函数时使用，明确是常量或是变量，需要Merge一下IsConstant和IsVariable
    /// </summary>
    /// <param name="rightSegment"></param> 
    /// <returns></returns>
    public SqlSegment Merge(SqlSegment rightSegment)
    {
        this.IsConstant = this.IsConstant && rightSegment.IsConstant;
        this.IsVariable = this.IsVariable || rightSegment.IsVariable;
        return this;
    }
    /// <summary>
    /// 在解析函数时使用，明确是常量或是变量，需要Merge一下IsConstant和IsVariable
    /// </summary>
    /// <param name="rightSegment"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public SqlSegment Merge(SqlSegment rightSegment, object value)
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
    public SqlSegment Merge(SqlSegment leftSegment, SqlSegment rightSegment, object value)
    {
        this.IsConstant = this.IsConstant && leftSegment.IsConstant && rightSegment.IsConstant;
        this.IsVariable = this.IsVariable || leftSegment.IsVariable || rightSegment.IsVariable;
        this.Value = value;
        return this;
    }
    /// <summary>
    /// 在解析函数时使用，明确调用后的结果，表达式或是方法调用，需要Merge一下HasField和IsParameter
    /// </summary>
    /// <param name="rightSegment"></param>
    /// <param name="value"></param>
    /// <param name="isConstant"></param>
    /// <param name="isVariable"></param>
    /// <param name="isExpression"></param>
    /// <param name="isMethodCall"></param>
    /// <returns></returns>
    public SqlSegment Merge(SqlSegment rightSegment, object value, bool isConstant, bool isVariable, bool isExpression, bool isMethodCall = false)
    {
        this.IsConstant = isConstant;
        this.IsVariable = isVariable;
        this.HasField = this.HasField || rightSegment.HasField;
        this.HasParameter = this.HasParameter || rightSegment.HasParameter;
        this.IsExpression = isExpression;
        this.IsMethodCall = isMethodCall;
        this.Value = value;
        return this;
    }
    /// <summary>
    /// 在解析函数时使用，明确调用后的结果，表达式或是方法调用，需要Merge一下HasField和IsParameter
    /// </summary>
    /// <param name="leftSegment"></param>
    /// <param name="rightSegment"></param>
    /// <param name="value"></param>
    /// <param name="isConstant"></param>
    /// <param name="isVariable"></param>
    /// <param name="isExpression"></param>
    /// <param name="isMethodCall"></param>
    /// <returns></returns>
    public SqlSegment Merge(SqlSegment leftSegment, SqlSegment rightSegment, object value, bool isConstant, bool isVariable, bool isExpression, bool isMethodCall = false)
    {
        this.IsConstant = isConstant;
        this.IsVariable = isVariable;
        this.HasField = this.HasField || leftSegment.HasField || rightSegment.HasField;
        this.HasParameter = this.HasParameter || leftSegment.HasParameter || rightSegment.HasParameter;
        this.IsExpression = isExpression;
        this.IsMethodCall = isMethodCall;
        this.Value = value;
        return this;
    }
    public SqlSegment Next(Expression nextExpr)
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
    public override string ToString()
    {
        if (this.Value == null)
            throw new Exception("SqlSegment.Value值不能为null，使用错误");
        return this.Value.ToString();
    }
    protected bool Equals(SqlSegment other)
    {
        if (isFixValue != other.isFixValue)
            return false;
        return Value == other.Value;
    }
    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((SqlSegment)obj);
    }
    public override int GetHashCode() => HashCode.Combine(this.isFixValue, this.OperationType, this.IsConstant, this.Value);

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebugDisplayText => $"Value: {this.Value} Expression: {this.Expression}";
}
