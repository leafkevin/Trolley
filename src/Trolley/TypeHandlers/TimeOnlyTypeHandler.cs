using System;

namespace Trolley;

public class TimeOnlyTypeHandler : ITypeHandler
{
    public virtual string Format { get; set; } = "hh\\:mm\\:ss\\.fff";
    public object Parse(IOrmProvider ormProvider, Type targetType, object value)
    {
        if (value is TimeOnly)
            return value;
        return TimeOnly.MinValue;
    }
    public object ToFieldValue(IOrmProvider ormProvider, Type expectType, object value) => value;
    public virtual string GetQuotedValue(IOrmProvider ormProvider, Type expectType, object value)
    {
        if (value is TimeOnly toValue)
            return toValue.ToString(this.Format);
        return TimeOnly.MinValue.ToString(this.Format);
    }
}
public class NullableTimeOnlyTypeHandler : ITypeHandler
{
    public virtual string Format { get; set; } = "hh\\:mm\\:ss\\.fff";
    public object Parse(IOrmProvider ormProvider, Type targetType, object value)
    {
        if (value is TimeOnly)
            return value;
        return null;
    }
    public object ToFieldValue(IOrmProvider ormProvider, Type expectType, object value)
    {
        if (value is TimeOnly)
            return value;
        return DBNull.Value;
    }
    public virtual string GetQuotedValue(IOrmProvider ormProvider, Type expectType, object value)
    {
        if (value is TimeOnly toValue)
            return toValue.ToString(this.Format);
        return "NULL";
    }
}
public class TimeOnlyAsLongTypeHandler : ITypeHandler
{
    public object Parse(IOrmProvider ormProvider, Type targetType, object value)
    {
        if (value is TimeOnly)
            return value;
        if (value is long lValue)
            return TimeOnly.FromTimeSpan(TimeSpan.FromTicks(lValue));
        return TimeOnly.MinValue;
    }
    public object ToFieldValue(IOrmProvider ormProvider, Type expectType, object value)
    {
        if (value is TimeOnly toValue)
            return toValue.Ticks;
        return 0;
    }
    public virtual string GetQuotedValue(IOrmProvider ormProvider, Type expectType, object value)
    {
        if (value is TimeOnly toValue)
            return toValue.Ticks.ToString();
        return "0";
    }
}
public class NullableTimeOnlyAsLongTypeHandler : ITypeHandler
{
    public object Parse(IOrmProvider ormProvider, Type targetType, object value)
    {
        if (value is TimeOnly)
            return value;
        if (value is long lValue)
            return TimeOnly.FromTimeSpan(TimeSpan.FromTicks(lValue));
        return null;
    }
    public object ToFieldValue(IOrmProvider ormProvider, Type expectType, object value)
    {
        if (value is TimeOnly toValue)
            return toValue.Ticks;
        return DBNull.Value;
    }
    public virtual string GetQuotedValue(IOrmProvider ormProvider, Type expectType, object value)
    {
        if (value is TimeOnly toValue)
            return toValue.Ticks.ToString();
        return "NULL";
    }
}