using System;

namespace Trolley;

public class NumberTypeHandler : ITypeHandler
{
    public virtual object Parse(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is DBNull)
            return Convert.ChangeType(0, underlyingType);
        return value;
    }
    public virtual object ToFieldValue(IOrmProvider ormProvider, Type underlyingType, object value) => value;
    public virtual string GetQuotedValue(IOrmProvider ormProvider, Type underlyingType, object value) => value.ToString();
}
public class NullableNumberTypeHandler : ITypeHandler
{
    public virtual object Parse(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is DBNull) return null;
        return value;
    }
    public virtual object ToFieldValue(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value != null) return value;
        return DBNull.Value;
    }
    public virtual string GetQuotedValue(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value != null)
            return value.ToString();
        return "NULL";
    }
}
public class ConvertNumberTypeHandler<TField> : ITypeHandler
{
    protected Type fieldType = typeof(TField);
    public virtual object Parse(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is DBNull)
            return Convert.ChangeType(0, underlyingType);
        return Convert.ChangeType(value, underlyingType);
    }
    public virtual object ToFieldValue(IOrmProvider ormProvider, Type underlyingType, object value)
        => Convert.ChangeType(value, this.fieldType);
    public virtual string GetQuotedValue(IOrmProvider ormProvider, Type underlyingType, object value) => value.ToString();
}
public class NullableConvertNumberTypeHandler<TField> : ITypeHandler
{
    protected Type fieldType = typeof(TField);
    public virtual object Parse(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is DBNull) return null;
        return Convert.ChangeType(value, underlyingType);
    }
    public virtual object ToFieldValue(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value != null)
            return Convert.ChangeType(value, this.fieldType);
        return DBNull.Value;
    }
    public virtual string GetQuotedValue(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value != null)
            return value.ToString();
        return "NULL";
    }
}
