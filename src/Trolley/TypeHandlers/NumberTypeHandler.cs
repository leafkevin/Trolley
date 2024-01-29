using System;

namespace Trolley;

public class NumberTypeHandler : ITypeHandler
{
    public virtual object Parse(IOrmProvider ormProvider, Type underlyingType, object value) => value;
    public virtual object ToFieldValue(IOrmProvider ormProvider, Type underlyingType, object value) => value;
    public virtual string GetQuotedValue(IOrmProvider ormProvider, Type underlyingType, object value) => value.ToString();
}
public class NullableNumberTypeHandler : ITypeHandler
{
    public virtual object Parse(IOrmProvider ormProvider, Type underlyingType, object value) => value;
    public virtual object ToFieldValue(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value != null)
            return value;
        return DBNull.Value;
    }
    public virtual string GetQuotedValue(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value != null)
            return value.ToString();
        return "NULL";
    }
}
public class ConvertNumberTypeHandler : ITypeHandler
{
    public virtual object Parse(IOrmProvider ormProvider, Type underlyingType, object value)
        => Convert.ChangeType(value, underlyingType);
    public virtual object ToFieldValue(IOrmProvider ormProvider, Type underlyingType, object value)
        => Convert.ChangeType(value, underlyingType);
    public virtual string GetQuotedValue(IOrmProvider ormProvider, Type underlyingType, object value) => value.ToString();
}
public class NullableConvertNumberTypeHandler : ITypeHandler
{
    public virtual object Parse(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value != null)
            return Convert.ChangeType(value, underlyingType);
        return null;
    }
    public virtual object ToFieldValue(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value != null)
            return Convert.ChangeType(value, underlyingType);
        return DBNull.Value;
    }
    public virtual string GetQuotedValue(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value != null)
            return value.ToString();
        return "NULL";
    }
}
