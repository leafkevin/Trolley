using System;

namespace Trolley;

public class GuidTypeHandler : ITypeHandler
{
    public virtual object Parse(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is Guid)
            return value;
        return Guid.Empty;
    }
    public virtual object ToFieldValue(IOrmProvider ormProvider, Type underlyingType, object value) => value;
    public virtual string GetQuotedValue(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is Guid gVaue)
            return gVaue.ToString();
        return Guid.Empty.ToString();
    }
}
public class GuidAsStringTypeHandler : ITypeHandler
{
    public virtual object Parse(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is Guid)
            return value;
        if (value is string strValue && !string.IsNullOrEmpty(strValue))
            return Guid.Parse(strValue);
        return Guid.Empty;
    }
    public virtual object ToFieldValue(IOrmProvider ormProvider, Type underlyingType, object value) => value.ToString();
    public virtual string GetQuotedValue(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is Guid gVaue)
            return gVaue.ToString();
        return Guid.Empty.ToString();
    }
}
public class NullableGuidTypeHandler : ITypeHandler
{
    public virtual object Parse(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is Guid)
            return value;
        return null;
    }
    public virtual object ToFieldValue(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is Guid)
            return value;
        return DBNull.Value;
    }
    public virtual string GetQuotedValue(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is Guid gVaue)
            return gVaue.ToString();
        return "NULL";
    }
}
public class NullableGuidAsStringTypeHandler : ITypeHandler
{
    public virtual object Parse(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is Guid gValue)
            return gValue;
        if (value is string strValue && !string.IsNullOrEmpty(strValue))
            return Guid.Parse(strValue);
        return null;
    }
    public virtual object ToFieldValue(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is Guid gValue)
            return gValue.ToString();
        return DBNull.Value;
    }
    public virtual string GetQuotedValue(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is Guid gVaue)
            return gVaue.ToString();
        return "NULL";
    }
}