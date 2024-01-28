using System;

namespace Trolley;

public class BooleanTypeHandler : ITypeHandler
{
    public virtual object Parse(IOrmProvider ormProvider, Type targetType, object value)
    {
        if (value is bool)
            return value;
        return false;
    }
    public virtual object ToFieldValue(IOrmProvider ormProvider, Type expectType, object value) => value;
    public virtual string GetQuotedValue(IOrmProvider ormProvider, Type expectType, object value)
    {
        if (value is bool bValue)
            return bValue.ToString();
        return "False";
    }
}
public class BooleanAsIntTypeHandler : ITypeHandler
{
    public virtual object Parse(IOrmProvider ormProvider, Type targetType, object value)
    {
        if (value is bool)
            return value;
        if (value != null)
            return Convert.ToInt32(value) == 1;
        return false;
    }
    public virtual object ToFieldValue(IOrmProvider ormProvider, Type expectType, object value) => value;
    public virtual string GetQuotedValue(IOrmProvider ormProvider, Type expectType, object value)
    {
        if (value is bool bValue)
            return bValue ? "1" : "0";
        return "0";
    }
}
public class NullableBooleanTypeHandler : ITypeHandler
{
    public virtual object Parse(IOrmProvider ormProvider, Type targetType, object value)
    {
        if (value is bool)
            return value;
        return DBNull.Value;
    }
    public virtual object ToFieldValue(IOrmProvider ormProvider, Type expectType, object value)
    {
        if (value is bool)
            return value;
        return DBNull.Value;
    }
    public virtual string GetQuotedValue(IOrmProvider ormProvider, Type expectType, object value)
    {
        if (value is bool bValue)
            return bValue.ToString();
        return "NULL";
    }
}
public class NullableBooleanAsIntTypeHandler : ITypeHandler
{
    public virtual object Parse(IOrmProvider ormProvider, Type targetType, object value)
    {
        if (value is bool)
            return value;
        if (value != null)
            return Convert.ToInt32(value) == 1;
        return null;
    }
    public virtual object ToFieldValue(IOrmProvider ormProvider, Type expectType, object value) => value;
    public virtual string GetQuotedValue(IOrmProvider ormProvider, Type expectType, object value)
    {
        if (value is bool bValue)
            return bValue ? "1" : "0";
        return "NULL";
    }
}