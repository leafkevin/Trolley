using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Trolley;

public class SqlSegment
{
    public static SqlSegment None = new SqlSegment { isFixValue = true, Value = string.Empty };
    public static SqlSegment True = new SqlSegment { isFixValue = true, Value = true };
    public static SqlSegment False = new SqlSegment { isFixValue = true, Value = false };
    public static SqlSegment Null = new SqlSegment { isFixValue = true, Value = "NULL" };
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
    public bool IsExpression { get; set; }
    public string ParameterName { get; set; }
    public MemberInfo FromMember { get; set; }
    public TableSegment TableSegment { get; set; }
    public object Value { get; set; }
    public Expression Expression { get; set; }
    public bool HasDeferred => this.DeferredExprs != null && this.DeferredExprs.Count > 0;

    public SqlSegment Merge(SqlSegment right)
    {
        this.isFixValue = false;
        this.HasField = this.HasField || right.HasField;
        this.IsParameter = this.IsParameter || right.IsParameter;
        this.IsExpression = this.IsExpression || right.IsExpression;
        this.IsConstantValue = this.IsConstantValue && right.IsConstantValue;
        this.Value = right.Value;
        if (right.HasDeferred)
        {
            if (!this.HasDeferred)
                this.DeferredExprs = right.DeferredExprs;
            else
            {
                while (right.TryPop(out var deferredExpr))
                    this.Push(deferredExpr);
            }
        }
        return this;
    }
    public SqlSegment Merge(SqlSegment right, object value)
    {
        this.Merge(right);
        this.Value = value;
        return this;
    }
    public SqlSegment Change(object value, bool isConstantValue = true)
    {
        this.isFixValue = false;
        this.IsConstantValue = isConstantValue;
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
    public bool TryPop(out DeferredExpr deferredExpr)
    {
        if (!this.HasDeferred)
        {
            deferredExpr = default;
            return false;
        }
        return this.DeferredExprs.TryPop(out deferredExpr);
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
    public override int GetHashCode() => HashCode.Combine(this.isFixValue, this.Value);
}
