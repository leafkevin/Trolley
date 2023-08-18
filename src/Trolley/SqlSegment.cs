﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;

namespace Trolley;

[DebuggerDisplay("Value: {Value,nq}     Expression: {Expression,nq}")]
public class SqlSegment
{
    public static readonly SqlSegment True = new SqlSegment { isFixValue = true, OperationType = OperationType.None, IsConstant = true, Value = true };
    public static readonly SqlSegment Null = new SqlSegment { isFixValue = true, OperationType = OperationType.None, IsConstant = true, Value = "NULL" };
    private bool isFixValue = false;
    private Type currentType = null;

    /// <summary>
    /// 操作符:None,Equal,Not,And,Or
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
    /// 当强制转换时，此字段值为转换后的类型，当枚举类型时，此字段值为枚举类型，其他场景就是当前表达式的类型
    /// </summary>
    public Type ExpectType
    {
        get
        {
            if (this.currentType != null)
                return this.currentType;
            return this.Expression.Type;
        }
        set { this.currentType = value; }
    }
    /// <summary>
    /// 当枚举类型成员访问时，且数据库为VARCHAR类型，此字段会有值，值为typeof(string)
    /// </summary>
    public Type TargetType { get; set; }
    public string ParameterName { get; set; }
    public TableSegment TableSegment { get; set; }
    public ReaderFieldType MemberType { get; set; }
    public MemberInfo FromMember { get; set; }
    /// <summary>
    /// 当前是成员访问时，才有值，和FromMember是同一个栏位，是Mapper
    /// </summary>
    public MemberMap MemberMapper { get; set; }
    public object Value { get; set; }
    public Expression Expression { get; set; }
    public Expression OriginalExpression { get; set; }
    public bool HasDeferred => this.DeferredExprs != null && this.DeferredExprs.Count > 0;

    /// <summary>
    /// 表达式的所有下属子表达式都解析完毕，把每个子表达式HasField，IsParameter栏位值合并一下，以便外层判断
    /// 通常是在二元操作后或是带有多个参数的方法调用后，进行此操作，并把Merge结果赋值到当前sqlSegment中
    /// </summary>
    /// <param name="rightSegment">右侧sqlSegment</param>
    /// <returns>返回合并后的结果，并赋值到当前sqlSegment中</returns>
    public SqlSegment Merge(SqlSegment rightSegment)
    {
        this.IsConstant = this.IsConstant && rightSegment.IsConstant;
        this.IsVariable = this.IsVariable || rightSegment.IsVariable;
        this.HasField = this.HasField || rightSegment.HasField;
        this.IsParameter = this.IsParameter || rightSegment.IsParameter;
        return this;
    }
    public SqlSegment Merge(SqlSegment rightSegment, object value)
    {
        this.IsConstant = this.IsConstant && rightSegment.IsConstant;
        this.IsVariable = this.IsVariable || rightSegment.IsVariable;
        this.HasField = this.HasField || rightSegment.HasField;
        this.IsParameter = this.IsParameter || rightSegment.IsParameter;
        this.Value = value;
        return this;
    }

    public SqlSegment Change(object value, bool isConstant = true, bool isExpression = false, bool isMethodCall = false)
    {
        this.isFixValue = false;
        this.IsConstant = isConstant;
        this.IsExpression = isExpression;
        this.IsMethodCall = isMethodCall;
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
    public SqlSegment ToExpectType(IOrmProvider ormProvider)
    {
        if (this.ExpectType != this.Expression.Type)
        {
            if (this.HasField || this.IsParameter)
                this.Value = ormProvider.CastTo(this.ExpectType, this.Value);
            if (this.IsConstant || this.IsVariable)
                this.Value = Convert.ChangeType(this.Value, this.ExpectType);
        }
        return this;
    }
    public SqlSegment ToParameter(ISqlVisitor visitor)
    {
        if (this.IsVariable || (this.IsParameterized || visitor.IsParameterized) && this.IsConstant)
        {
            this.IsConstant = false;
            this.IsVariable = false;
            this.IsParameter = true;
        }
        return this;
    }
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
    public bool TryPeek(out DeferredExpr deferredExpr)
    {
        if (!this.HasDeferred)
        {
            deferredExpr = default;
            return false;
        }
        return this.DeferredExprs.TryPeek(out deferredExpr);
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
