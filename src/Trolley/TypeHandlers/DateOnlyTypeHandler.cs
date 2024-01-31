using System;

namespace Trolley;

public class DateOnlyTypeHandler : ITypeHandler
{
    public virtual string Format { get; set; } = "\\'yyyy-MM-dd\\'";
    public virtual object Parse(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is DateOnly)
            return value;
        if (value is DateTime dtValue)
            return DateOnly.FromDateTime(dtValue);
        return DateOnly.MinValue;
    }
    public virtual object ToFieldValue(IOrmProvider ormProvider, Type underlyingType, object value) => value;
    public virtual string GetQuotedValue(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is DateOnly doValue)
            return doValue.ToString(this.Format);
        return DateOnly.MinValue.ToString(this.Format);
    }
}
public class NullableDateOnlyTypeHandler : ITypeHandler
{
    public virtual string Format { get; set; } = "\\'yyyy-MM-dd\\'";
    public virtual object Parse(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is DateOnly)
            return value;
        if (value is DateTime dtValue)
            return DateOnly.FromDateTime(dtValue);
        return null;
    }
    public virtual object ToFieldValue(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is DateOnly)
            return value;
        return DBNull.Value;
    }
    public virtual string GetQuotedValue(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is DateOnly doValue)
            return doValue.ToString(this.Format);
        return "NULL";
    }
}
public class DateOnlyAsStringTypeHandler : ITypeHandler
{
    public virtual string Format { get; set; } = "\\'yyyy-MM-dd\\'";
    public virtual object Parse(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is string strValue && !string.IsNullOrEmpty(strValue))
            return DateOnly.Parse(strValue);
        return DateOnly.MinValue;
    }
    public virtual object ToFieldValue(IOrmProvider ormProvider, Type underlyingType, object value)
        => this.GetQuotedValue(ormProvider, underlyingType, value);
    public virtual string GetQuotedValue(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is DateOnly doValue)
            return doValue.ToString(this.Format);
        return DateOnly.MinValue.ToString(this.Format);
    }
}
public class NullableDateOnlyAsStringTypeHandler : ITypeHandler
{
    public virtual string Format { get; set; } = "\\'yyyy-MM-dd\\'";
    public virtual object Parse(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is string strValue && !string.IsNullOrEmpty(strValue))
            return DateOnly.Parse(strValue);
        return null;
    }
    public virtual object ToFieldValue(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is DateOnly)
            return value;
        return DBNull.Value;
    }
    public virtual string GetQuotedValue(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is DateOnly doValue)
            return doValue.ToString(this.Format);
        return "NULL";
    }
}