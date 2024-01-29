using System;

namespace Trolley;

public class StringTypeHandler : ITypeHandler
{
    public virtual object Parse(IOrmProvider ormProvider, Type underlyingType, object value) => value;
    public virtual object ToFieldValue(IOrmProvider ormProvider, Type underlyingType, object value) => value;
    public virtual string GetQuotedValue(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is string strValue && !string.IsNullOrEmpty(strValue))
            return $"'{strValue.Replace("'", @"\'")}'";
        return "''";
    }
}
public class NullableStringTypeHandler : ITypeHandler
{
    public virtual object Parse(IOrmProvider ormProvider, Type underlyingType, object value) => value;
    public virtual object ToFieldValue(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is string strValue && !string.IsNullOrEmpty(strValue))
            return value;
        return DBNull.Value;
    }
    public virtual string GetQuotedValue(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is string strValue && !string.IsNullOrEmpty(strValue))
            return $"'{strValue.Replace("'", @"\'")}'";
        return "NULL";
    }
}
