using System;
using System.Diagnostics;

namespace Trolley;

public class EnumTypeHandler<TField> : ITypeHandler
{
    protected Type fieldType = typeof(TField);
    public virtual object Parse(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is not DBNull)
            return Enum.ToObject(underlyingType, value);
        return Enum.ToObject(underlyingType, 0);
    }
    public virtual object ToFieldValue(IOrmProvider ormProvider, Type underlyingType, object value)
        => Convert.ChangeType(value, fieldType);
    public virtual string GetQuotedValue(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        var numberValue = Convert.ChangeType(value, fieldType);
        return numberValue.ToString();
    }
}
public class NullableEnumTypeHandler<TField> : ITypeHandler
{
    protected Type fieldType = typeof(TField);
    public virtual object Parse(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is DBNull)
            return null;
        return Enum.ToObject(underlyingType, value);
    }
    public virtual object ToFieldValue(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value != null)
            return Convert.ChangeType(value, fieldType);
        return DBNull.Value;
    }
    public virtual string GetQuotedValue(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value != null)
        {
            var numberValue = Convert.ChangeType(value, fieldType);
            return numberValue.ToString();
        }
        return "NULL";
    }
}
public class ConvertEnumTypeHandler<TField> : ITypeHandler
{
    protected Type fieldType = typeof(TField);
    public virtual object Parse(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is not DBNull)
        {
            value = Convert.ChangeType(value, fieldType);
            return Enum.ToObject(underlyingType, value);
        }
        return Enum.ToObject(underlyingType, 0);
    }
    public virtual object ToFieldValue(IOrmProvider ormProvider, Type underlyingType, object value)
        => Convert.ChangeType(value, fieldType);
    public virtual string GetQuotedValue(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        var numberValue = Convert.ChangeType(value, fieldType);
        return numberValue.ToString();
    }
}
public class NullableConvertEnumTypeHandler<TField> : ITypeHandler
{
    protected Type fieldType = typeof(TField);
    public virtual object Parse(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is not DBNull)
        {
            value = Convert.ChangeType(value, fieldType);
            return Enum.ToObject(underlyingType, value);
        }
        return null;
    }
    public virtual object ToFieldValue(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value != null)
            return Convert.ChangeType(value, fieldType);
        return DBNull.Value;
    }
    public virtual string GetQuotedValue(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value != null)
        {
            var numberValue = Convert.ChangeType(value, fieldType);
            return numberValue.ToString();
        }
        return "NULL";
    }
}
public class EnumAsStringTypeHandler : ITypeHandler
{
    public virtual object Parse(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is string strValue && !string.IsNullOrEmpty(strValue))
            return Enum.Parse(underlyingType, strValue);
        return Enum.ToObject(underlyingType, 0);
    }
    public virtual object ToFieldValue(IOrmProvider ormProvider, Type underlyingType, object value)
        => Enum.GetName(underlyingType, value);
    public virtual string GetQuotedValue(IOrmProvider ormProvider, Type underlyingType, object value)
        => $"'{Enum.GetName(underlyingType, value)}'";
}
public class NullableEnumAsStringTypeHandler : ITypeHandler
{
    public virtual object Parse(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is string strValue && !string.IsNullOrEmpty(strValue))
            return Enum.Parse(underlyingType, strValue);
        return null;
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