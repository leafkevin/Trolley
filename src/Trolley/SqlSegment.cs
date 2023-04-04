﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Trolley;

[DebuggerDisplay("Value: {Value,nq}     Expression: {Expression,nq}")]
public class SqlSegment
{
    public static SqlSegment True = new SqlSegment { isFixValue = true, OperationType = OperationType.None, IsConstantValue = true, Value = true };
    public static SqlSegment Null = new SqlSegment { isFixValue = true, OperationType = OperationType.None, IsConstantValue = true, Value = "NULL" };
    private bool isFixValue = false;

    /// <summary>
    /// 操作符:And/Or/Concat/Equals/NotEquals/Convert/,
    /// </summary>
    public OperationType OperationType { get; set; } = OperationType.None;
    public Stack<DeferredExpr> DeferredExprs { get; set; }
    public int Deep { get; set; }
    public int ReaderIndex { get; set; }
    /// <summary>
    /// 是否有字段
    /// </summary>
    public bool HasField { get; set; }
    /// <summary>
    /// 是否有SQL参数，如：@p1,@p2
    /// </summary>
    public bool IsParameter { get; set; }
    /// <summary>
    /// 是否是常量值
    /// </summary>
    public bool IsConstantValue { get; set; }
    /// <summary>
    /// 是否是表达式，通常方法调用，二元表达式都为true
    /// </summary>
    public bool IsExpression { get; set; }
    /// <summary>
    /// 是否需要在最外层添加括弧()，主要是在SELECT语句中，某个字段是表达式，用()包一下SQL看起来更优雅
    /// 通常是在解析Defer语句或是解析表达式后设置此值
    /// </summary>
    public bool IsNeedParentheses { get; set; }
    /// <summary>
    /// 是否参数化当前值，本次解析有效
    /// </summary>
    public bool IsParameterized { get; set; }
    public bool IsArray { get; set; }
    /// <summary>
    /// string.Concat,string.Format,string.Join，数据库VARCHAR类型的Enum实体成员，此字段会有值
    /// 做字符串连接时，此值为typeof(string)
    /// 数据库VARCHAR类型的Enum实体成员时，此值是对应的枚举类型，TargetType类型是typeof(string)
    /// </summary>
    public Type ExpectType { get; set; }
    /// <summary>
    /// 目标类型，与ExpectType类型，可能一致也可能不一致。
    /// 如：Enum类型，对应的数据库字段类型是VARCHAR时，就不一致，ExpectType是枚举类型，TargetType是字符串类型
    /// </summary>
    public Type TargetType { get; set; }
    public TableSegment TableSegment { get; set; }
    public ReaderFieldType MemberType { get; set; }
    public MemberInfo FromMember { get; set; }
    public object Value { get; set; }
    public Expression Expression { get; set; }
    public bool HasDeferred => this.DeferredExprs != null && this.DeferredExprs.Count > 0;

    /// <summary>
    /// 表达式的所有下属子表达式都解析完毕，把每个子表达式HasField，IsParameter栏位值合并一下，以便外层判断
    /// 通常是在二元操作后或是带有多个参数的方法调用后，进行此操作，并把Merge结果赋值到当前sqlSegment中
    /// </summary>
    /// <param name="rightSegment">右侧sqlSegment</param>
    /// <returns>返回合并后的结果，并赋值到当前sqlSegment中</returns>
    public SqlSegment Merge(SqlSegment rightSegment)
    {
        this.HasField = this.HasField || rightSegment.HasField;
        this.IsParameter = this.IsParameter || rightSegment.IsParameter;
        return this;
    }
    public SqlSegment Change(object value, bool isConstantValue = true, bool isExpression = false)
    {
        this.isFixValue = false;
        this.IsConstantValue = isConstantValue;
        this.IsExpression = isExpression;
        this.Value = value;
        return this;
    }
    public SqlSegment ChangeValue(object value)
    {
        this.Value = value;
        return this;
    }
    public SqlSegment Next(Expression nextExpr)
    {
        this.Expression = nextExpr;
        return this;
    }
    public SqlSegment Next(Expression nextExpr, object value)
    {
        this.Expression = nextExpr;
        this.isFixValue = false;
        this.Value = value;
        return this;
    }
    public void Push(DeferredExpr deferredExpr)
    {
        this.DeferredExprs ??= new();
        this.DeferredExprs.Push(deferredExpr);
    }
    public bool TryPop(OperationType[] operationTypes, out DeferredExpr deferredExpr)
    {
        if (!this.HasDeferred)
        {
            deferredExpr = default;
            return false;
        }
        return this.DeferredExprs.TryPop(f => operationTypes.Contains(f.OperationType), out deferredExpr);
    }
    public override string ToString()
    {
        if (this.Value == null)
            return string.Empty;
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
    public override int GetHashCode() => HashCode.Combine(this.isFixValue, this.OperationType, this.IsConstantValue, this.Value);

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string DebugDisplayText => $"Value: {this.Value} \r\nExpression: {this.Expression}";
}
