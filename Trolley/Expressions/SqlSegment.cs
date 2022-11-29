using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Trolley;

public class SqlSegment
{
    public static SqlSegment None = new SqlSegment { isFixValue = true, Value = string.Empty };
    public static SqlSegment True = new SqlSegment { isFixValue = true, Value = true };
    public static SqlSegment False = new SqlSegment { isFixValue = true, Value = false };
    public static SqlSegment Null = new SqlSegment { isFixValue = true, Value = "NULL" };
    private bool isFixValue = false;

    /// <summary>
    /// 表达返回类型，同时代表了可能的操作
    /// </summary>
    public SqlSegmentType NodeType { get; set; }
    /// <summary>
    /// 操作符:And/Or/Concat/Equals/NotEquals/Convert/,
    /// </summary>
    public OperationType OperationType { get; set; } = OperationType.None;
    public Stack<DeferredExpr> DeferredExprs { get; set; } = new Stack<DeferredExpr>();
    public int Deep { get; set; }
    public bool IsCompleted { get; set; }
    /// <summary>
    /// 是否有字段
    /// </summary>
    public bool HasField { get; set; }
    /// <summary>
    /// 是否有SQL参数，如：@p1,@p2
    /// </summary>
    public bool IsParameter { get; set; }
    /// <summary>
    /// 是否有函数调用
    /// </summary>
    public bool IsMethodCall { get; set; }
    public MemberMap MemberMapper { get; set; }
    public TableSegment TableSegment { get; set; }
    public object Value { get; set; }
    public Expression Expression { get; set; }
    public bool HasDeferred => this.DeferredExprs.Count > 0;

    public SqlSegment Merge(SqlSegment right)
    {
        this.isFixValue = false;
        this.HasField = this.HasField || right.HasField;
        this.IsParameter = this.IsParameter || right.IsParameter;
        this.IsMethodCall = this.IsMethodCall || right.IsMethodCall;
        this.Value = right.Value;
        if (right.HasDeferred)
        {
            while (right.TryPop(out var deferredExpr))
                this.Push(deferredExpr);
        }
        return this;
    }
    public SqlSegment Merge(SqlSegment right, object value)
    {
        this.Merge(right);
        this.Value = value;
        return this;
    }
    public SqlSegment Change(object value)
    {
        this.isFixValue = false;
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
    public SqlSegment Complete(object value)
    {
        this.Expression = null;
        this.IsCompleted = true;
        this.isFixValue = false;
        this.Value = value;
        return this;
    }
    public void Push(DeferredExpr deferredExpr) => this.DeferredExprs.Push(deferredExpr);
    public bool TryPop(out DeferredExpr deferredExpr) => this.DeferredExprs.TryPop(out deferredExpr);
    public bool TryPop(OperationType[] operationTypes, out DeferredExpr deferredExpr)
        => this.DeferredExprs.TryPop(f => operationTypes.Contains(f.OperationType), out deferredExpr);

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
