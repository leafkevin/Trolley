using System;

namespace Trolley;

public class NumberTypeHandler : ITypeHandler
{
    public virtual object Parse(IOrmProvider ormProvider, Type targetType, object value) => value;
    public virtual object ToFieldValue(IOrmProvider ormProvider, Type expectType, object value) => value;
    public virtual string GetQuotedValue(IOrmProvider ormProvider, Type expectType, object value) => value.ToString();
}
public class NullableNumberTypeHandler : ITypeHandler
{
    public virtual object Parse(IOrmProvider ormProvider, Type targetType, object value) => value;
    public virtual object ToFieldValue(IOrmProvider ormProvider, Type expectType, object value) => value;
    public virtual string GetQuotedValue(IOrmProvider ormProvider, Type expectType, object value)
    {
        if (value != null)
            return value.ToString();
        return "NULL";
    }
}
