using System;

namespace Trolley;

public class TimeSpanTypeHandler : ITypeHandler
{
    public virtual string Format { get; set; } = "hh\\:mm\\:ss\\.ffffff";
    public object Parse(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is TimeSpan)
            return value;
        return TimeSpan.MinValue;
    }
    public object ToFieldValue(IOrmProvider ormProvider, Type underlyingType, object value) => value;
    public virtual string GetQuotedValue(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is TimeSpan tsValue)
        {
            if (tsValue > TimeSpan.FromDays(1) || tsValue < -TimeSpan.FromDays(1))
                return $"'{(int)tsValue.TotalDays}.{tsValue.ToString(Format)}'";
            return $"'{tsValue.ToString(this.Format)}'";
        }
        return TimeSpan.MinValue.ToString(this.Format);
    }
}
public class NullableTimeSpanTypeHandler : ITypeHandler
{
    public virtual string Format { get; set; } = "hh\\:mm\\:ss\\.ffffff";
    public object Parse(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is TimeSpan)
            return value;
        return null;
    }
    public object ToFieldValue(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is TimeSpan)
            return value;
        return DBNull.Value;
    }
    public virtual string GetQuotedValue(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is TimeSpan tsValue)
        {
            if (tsValue > TimeSpan.FromDays(1) || tsValue < -TimeSpan.FromDays(1))
                return $"'{(int)tsValue.TotalDays}.{tsValue.ToString(Format)}'";
            return $"'{tsValue.ToString(this.Format)}'";
        }
        return "NULL";
    }
}
public class TimeSpanAsStringTypeHandler : ITypeHandler
{
    public virtual string Format { get; set; } = "hh\\:mm\\:ss\\.ffffff";
    public object Parse(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is string strValue)
            return TimeSpan.Parse(strValue);
        return TimeSpan.MinValue;
    }
    public object ToFieldValue(IOrmProvider ormProvider, Type underlyingType, object value) => value;
    public virtual string GetQuotedValue(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is TimeSpan tsValue)
        {
            if (tsValue > TimeSpan.FromDays(1) || tsValue < -TimeSpan.FromDays(1))
                return $"'{(int)tsValue.TotalDays}.{tsValue.ToString(Format)}'";
            return $"'{tsValue.ToString(this.Format)}'";
        }
        return TimeSpan.MinValue.ToString(this.Format);
    }
}
public class NullableTimeSpanAsStringTypeHandler : ITypeHandler
{
    public virtual string Format { get; set; } = "hh\\:mm\\:ss\\.ffffff";
    public object Parse(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is string strValue)
            return TimeSpan.Parse(strValue);
        return null;
    }
    public object ToFieldValue(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is TimeSpan)
            return value;
        return DBNull.Value;
    }
    public virtual string GetQuotedValue(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is TimeSpan tsValue)
        {
            if (tsValue > TimeSpan.FromDays(1) || tsValue < -TimeSpan.FromDays(1))
                return $"'{(int)tsValue.TotalDays}.{tsValue.ToString(Format)}'";
            return $"'{tsValue.ToString(this.Format)}'";
        }
        return "NULL";
    }
}
public class TimeSpanAsLongTypeHandler : ITypeHandler
{
    public object Parse(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is TimeSpan)
            return value;
        if (value is long lValue)
            return TimeSpan.FromTicks(lValue);
        return TimeSpan.MinValue;
    }
    public object ToFieldValue(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is TimeSpan tsValue)
            return tsValue.Ticks;
        return 0;
    }
    public virtual string GetQuotedValue(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is TimeSpan tsValue)
            return tsValue.Ticks.ToString();
        return "0";
    }
}
public class NullableTimeSpanAsLongTypeHandler : ITypeHandler
{
    public object Parse(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is TimeSpan)
            return value;
        if (value is long lValue)
            return TimeSpan.FromTicks(lValue);
        return null;
    }
    public object ToFieldValue(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is TimeSpan tsValue)
            return tsValue.Ticks;
        return DBNull.Value;
    }
    public virtual string GetQuotedValue(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is TimeSpan tsValue)
            return tsValue.Ticks.ToString();
        return "NULL";
    }
}