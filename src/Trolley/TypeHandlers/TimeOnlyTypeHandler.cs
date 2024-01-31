using System;

namespace Trolley;

public class TimeOnlyTypeHandler : ITypeHandler
{
    public virtual string Format { get; set; } = "\\'hh\\:mm\\:ss\\.ffffff\\'";
    public virtual object Parse(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is TimeOnly)
            return value;
        if (value is TimeSpan tsValue)
            return TimeOnly.FromTimeSpan(tsValue);
        return TimeOnly.MinValue;
    }
    public virtual object ToFieldValue(IOrmProvider ormProvider, Type underlyingType, object value) => value;
    public virtual string GetQuotedValue(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is TimeOnly toValue)
            return toValue.ToString(this.Format);
        return TimeOnly.MinValue.ToString(this.Format);
    }
}
public class NullableTimeOnlyTypeHandler : ITypeHandler
{
    public virtual string Format { get; set; } = "\\'hh\\:mm\\:ss\\.ffffff\\'";
    public virtual object Parse(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is TimeOnly)
            return value;
        if (value is TimeSpan tsValue)
            return TimeOnly.FromTimeSpan(tsValue);
        return null;
    }
    public virtual object ToFieldValue(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is TimeOnly)
            return value;
        return DBNull.Value;
    }
    public virtual string GetQuotedValue(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is TimeOnly toValue)
            return $"'{toValue.ToString(this.Format)}'";
        return "NULL";
    }
}
public class TimeOnlyAsStringTypeHandler : ITypeHandler
{
    public virtual string Format { get; set; } = "\\'hh\\:mm\\:ss\\.ffffff\\'";
    public virtual object Parse(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is string strValue && !string.IsNullOrEmpty(strValue))
            return TimeOnly.Parse(strValue);
        return TimeOnly.MinValue;
    }
    public virtual object ToFieldValue(IOrmProvider ormProvider, Type underlyingType, object value)
        => this.GetQuotedValue(ormProvider, underlyingType, value);
    public virtual string GetQuotedValue(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is TimeOnly toValue)
            return toValue.ToString(this.Format);
        return TimeOnly.MinValue.ToString(this.Format);
    }
}
public class NullableTimeOnlyAsStringTypeHandler : ITypeHandler
{
    public virtual string Format { get; set; } = "\\'hh\\:mm\\:ss\\.ffffff\\'";
    public virtual object Parse(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is string strValue && !string.IsNullOrEmpty(strValue))
            return TimeOnly.Parse(strValue);
        return null;
    }
    public virtual object ToFieldValue(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is TimeOnly toValue)
            return $"'{toValue.ToString(this.Format)}'";
        return DBNull.Value;
    }
    public virtual string GetQuotedValue(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is TimeOnly toValue)
            return $"'{toValue.ToString(this.Format)}'";
        return "NULL";
    }
}
public class TimeOnlyAsLongTypeHandler : ITypeHandler
{
    public virtual object Parse(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is long lValue)
            return new TimeOnly(lValue);
        return TimeOnly.MinValue;
    }
    public virtual object ToFieldValue(IOrmProvider ormProvider, Type underlyingType, object value)
        => this.GetQuotedValue(ormProvider, underlyingType, value);
    public virtual string GetQuotedValue(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is TimeOnly toValue)
            return toValue.Ticks.ToString();
        return "0";
    }
}
public class NullableTimeOnlyAsLongTypeHandler : ITypeHandler
{
    public virtual object Parse(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is long lValue)
            return new TimeOnly(lValue);
        return null;
    }
    public virtual object ToFieldValue(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is TimeOnly toValue)
            return toValue.Ticks;
        return DBNull.Value;
    }
    public virtual string GetQuotedValue(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is TimeOnly toValue)
            return toValue.Ticks.ToString();
        return "NULL";
    }
}