using System;

namespace Trolley;

public class CharTypeHandler : ITypeHandler
{
    public virtual object Parse(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is char)
            return value;
        else if (value is string strValue && !string.IsNullOrEmpty(strValue))
            return strValue[0];
        return default(char);
    }
    public virtual object ToFieldValue(IOrmProvider ormProvider, Type underlyingType, object value) => value;
    public virtual string GetQuotedValue(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        char cValue = default(char);
        if (value is char chValue)
            cValue = chValue;
        else if (value is string strValue && !string.IsNullOrEmpty(strValue))
            cValue = strValue[0];
        if (cValue == '\'')
            return "''";
        return cValue.ToString();
    }
}
public class NullableCharTypeHandler : ITypeHandler
{
    public virtual object Parse(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is char)
            return value;
        else if (value is string strValue && !string.IsNullOrEmpty(strValue))
            return strValue[0];
        return null;
    }
    public virtual object ToFieldValue(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is char)
            return value;
        else if (value is string strValue && !string.IsNullOrEmpty(strValue))
            return value;
        return DBNull.Value;
    }
    public virtual string GetQuotedValue(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        char? cValue = null;
        if (value is char chValue)
            cValue = chValue;
        else if (value is string strValue && !string.IsNullOrEmpty(strValue))
            cValue = strValue[0];
        if (cValue.HasValue)
        {
            if (cValue.Value == '\'')
                return "''";
            return cValue.Value.ToString();
        }
        return "NULL";
    }
}
