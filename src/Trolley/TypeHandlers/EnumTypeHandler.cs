using System;

namespace Trolley;

public class EnumTypeHandler : ITypeHandler
{
    public virtual object Parse(IOrmProvider ormProvider, Type underlyingType, object value) => Enum.ToObject(underlyingType, value);
    public virtual object ToFieldValue(IOrmProvider ormProvider, Type underlyingType, object value)
        => Convert.ChangeType(value, Enum.GetUnderlyingType(underlyingType));
    public virtual string GetQuotedValue(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        var numberValue = Convert.ChangeType(value, Enum.GetUnderlyingType(underlyingType));
        return numberValue.ToString();
    }
}
public class NullableEnumTypeHandler : ITypeHandler
{
    public virtual object Parse(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is DBNull)
            return null;
        return Enum.ToObject(underlyingType, value);
    }
    public virtual object ToFieldValue(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value != null)
            return Convert.ChangeType(value, Enum.GetUnderlyingType(underlyingType));
        return DBNull.Value;
    }
    public virtual string GetQuotedValue(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value != null)
        {
            var numberValue = Convert.ChangeType(value, Enum.GetUnderlyingType(underlyingType));
            return numberValue.ToString();
        }
        return "NULL";
    }
}
public class EnumAsStringTypeHandler : ITypeHandler
{
    public virtual object Parse(IOrmProvider ormProvider, Type underlyingType, object value)
        => Enum.GetName(underlyingType, value);
    public virtual object ToFieldValue(IOrmProvider ormProvider, Type underlyingType, object value)
        => Enum.GetName(underlyingType, value);
    public virtual string GetQuotedValue(IOrmProvider ormProvider, Type underlyingType, object value)
        => $"'{Enum.GetName(underlyingType, value)}'";
}
public class NullableEnumAsStringTypeHandler : ITypeHandler
{
    public virtual object Parse(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is DBNull)
            return null;
        return Enum.GetName(underlyingType, value);
    }
    public virtual object ToFieldValue(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value == null) return DBNull.Value;
        return Enum.GetName(underlyingType, value);
    }
    public virtual string GetQuotedValue(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value != null)
            return $"'{Enum.GetName(underlyingType, value)}'";
        return "NULL";
    }
}