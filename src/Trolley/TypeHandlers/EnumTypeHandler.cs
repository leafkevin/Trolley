using System;

namespace Trolley;

public class EnumTypeHandler : ITypeHandler
{
    public virtual object Parse(IOrmProvider ormProvider, Type targetType, object value) => Enum.ToObject(targetType, value);
    public virtual object ToFieldValue(IOrmProvider ormProvider, Type expectType, object value)
        => Convert.ChangeType(value, Enum.GetUnderlyingType(expectType));
    public virtual string GetQuotedValue(IOrmProvider ormProvider, Type expectType, object value)
    {
        var numberValue = Convert.ChangeType(value, Enum.GetUnderlyingType(expectType));
        return numberValue.ToString();
    }
}
public class NullableEnumTypeHandler : ITypeHandler
{
    public virtual object Parse(IOrmProvider ormProvider, Type targetType, object value)
    {
        if (value is DBNull)
            return null;
        return Enum.ToObject(targetType, value);
    }
    public virtual object ToFieldValue(IOrmProvider ormProvider, Type expectType, object value)
    {
        if (value == null) return DBNull.Value;
        return Convert.ChangeType(value, Enum.GetUnderlyingType(expectType));
    }
    public virtual string GetQuotedValue(IOrmProvider ormProvider, Type expectType, object value)
    {
        if (value == null) return "NULL";
        var numberValue = Convert.ChangeType(value, Enum.GetUnderlyingType(expectType));
        return numberValue.ToString();
    }
}
public class EnumAsStringTypeHandler : ITypeHandler
{
    public virtual object Parse(IOrmProvider ormProvider, Type targetType, object value)
        => Enum.GetName(targetType, value);
    public virtual object ToFieldValue(IOrmProvider ormProvider, Type expectType, object value)
        => Enum.GetName(expectType, value);
    public virtual string GetQuotedValue(IOrmProvider ormProvider, Type expectType, object value)
        => Enum.GetName(expectType, value);
}
public class NullableEnumAsStringTypeHandler : ITypeHandler
{
    public virtual object Parse(IOrmProvider ormProvider, Type targetType, object value)
    {
        if (value is DBNull)
            return null;
        return Enum.GetName(targetType, value);
    }
    public virtual object ToFieldValue(IOrmProvider ormProvider, Type expectType, object value)
    {
        if (value == null) return DBNull.Value;
        return Enum.GetName(expectType, value);
    }
    public virtual string GetQuotedValue(IOrmProvider ormProvider, Type expectType, object value)
    {
        if (value != null)
            return Enum.GetName(expectType, value);
        return "NULL";
    }
}