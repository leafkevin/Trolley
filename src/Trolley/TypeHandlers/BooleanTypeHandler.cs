using System;

namespace Trolley;

public class BooleanTypeHandler : ITypeHandler
{
    public virtual object Parse(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is bool)
            return value;
        return false;
    }
    public virtual object ToFieldValue(IOrmProvider ormProvider, Type underlyingType, object value) => value;
    public virtual string GetQuotedValue(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is bool bValue)
            return bValue.ToString();
        return "False";
    }
}
public class NullableBooleanTypeHandler : ITypeHandler
{
    public virtual object Parse(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is bool)
            return value;
        return DBNull.Value;
    }
    public virtual object ToFieldValue(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is bool)
            return value;
        return DBNull.Value;
    }
    public virtual string GetQuotedValue(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is bool bValue)
            return bValue.ToString();
        return "NULL";
    }
}
public class BooleanAsIntTypeHandler : ITypeHandler
{
    public virtual object Parse(IOrmProvider ormProvider, Type underlyingType, object value)
        => Convert.ToInt32(value) == 1;
    public virtual object ToFieldValue(IOrmProvider ormProvider, Type underlyingType, object value) => value;
    public virtual string GetQuotedValue(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is bool bValue)
            return bValue ? "1" : "0";
        return "0";
    }
}
public class NullableBooleanAsIntTypeHandler : ITypeHandler
{
    public virtual object Parse(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value == null) return null;
        return Convert.ToInt32(value) == 1;
    }
    public virtual object ToFieldValue(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is bool)
            return value;
        return DBNull.Value;
    }
    public virtual string GetQuotedValue(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is bool bValue)
            return bValue ? "1" : "0";
        return "NULL";
    }
}