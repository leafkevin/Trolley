using System;

namespace Trolley;

public class ToStringTypeHandler : ITypeHandler
{
    public virtual object Parse(Type targetType, object value)
    {
        if (value is DBNull)
            return null;
        return value;
    }
    public virtual object ToFieldValue(object value)
    {
        if (value != null)
            return value.ToString();
        return DBNull.Value;
    }
    public virtual string GetQuotedValue(object value)
    {
        if (value != null)
            return value.ToString();
        return "NULL";
    }
}
