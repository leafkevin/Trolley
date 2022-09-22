using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Trolley;

public class SqlSegment
{
    public static SqlSegment None = new SqlSegment { isFixValue = true, Value = string.Empty };
    public static SqlSegment Null = new SqlSegment { isFixValue = true, Value = "NULL" };
    public static SqlSegment True = new SqlSegment { isFixValue = true, Value = true };
    private bool isFixValue = false;

    public bool HasField { get; set; }
    public bool IsParameter { get; set; }
    public bool IsMethodCall { get; set; }
    public MemberInfo Member { get; set; }
    public object Value { get; set; }
    public Expression Expression { get; set; }

    public SqlSegment Merge(SqlSegment right)
    {
        this.isFixValue = false;
        this.HasField = this.HasField || right.HasField;
        this.IsParameter = this.IsParameter || right.IsParameter;
        this.IsMethodCall = this.IsMethodCall || right.IsMethodCall;
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
    public override string ToString()
    {
        if (Value == null)
            return string.Empty;
        return Value.ToString();
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
