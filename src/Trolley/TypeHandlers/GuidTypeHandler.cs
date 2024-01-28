using System;

namespace Trolley;

public class GuidTypeHandler : ITypeHandler
{
    public object Parse(IOrmProvider ormProvider, Type targetType, object value)
    {
        if (value is Guid)
            return value;
        return Guid.Empty;
    }
    public object ToFieldValue(IOrmProvider ormProvider, Type expectType, object value) => value;
    public virtual string GetQuotedValue(IOrmProvider ormProvider, Type expectType, object value)
    {
        if (value is Guid gVaue)
            return gVaue.ToString();
        return Guid.Empty.ToString();
    }
}
public class GuidAsStringTypeHandler : ITypeHandler
{
    public object Parse(IOrmProvider ormProvider, Type targetType, object value)
    {
        if (value is Guid)
            return value;
        if (value is string strValue && !string.IsNullOrEmpty(strValue))
            return Guid.Parse(strValue);
        return Guid.Empty;
    }
    public object ToFieldValue(IOrmProvider ormProvider, Type expectType, object value) => value.ToString();
    public virtual string GetQuotedValue(IOrmProvider ormProvider, Type expectType, object value)
    {
        if (value is Guid gVaue)
            return gVaue.ToString();
        return Guid.Empty.ToString();
    }
}
public class NullableGuidTypeHandler : ITypeHandler
{
    public object Parse(IOrmProvider ormProvider, Type targetType, object value)
    {
        if (value is Guid)
            return value;
        return null;
    }
    public object ToFieldValue(IOrmProvider ormProvider, Type expectType, object value)
    {
        if (value is Guid)
            return value;
        return DBNull.Value;
    }
    public virtual string GetQuotedValue(IOrmProvider ormProvider, Type expectType, object value)
    {
        if (value is Guid gVaue)
            return gVaue.ToString();
        return "NULL";
    }
}
public class NullableGuidAsStringTypeHandler : ITypeHandler
{
    public object Parse(IOrmProvider ormProvider, Type targetType, object value)
    {
        if (value is Guid gValue)
            return gValue;
        if (value is string strValue && !string.IsNullOrEmpty(strValue))
            return Guid.Parse(strValue);
        return null;
    }
    public object ToFieldValue(IOrmProvider ormProvider, Type expectType, object value)
    {
        if (value is Guid gValue)
            return gValue.ToString();
        return DBNull.Value;
    }
    public virtual string GetQuotedValue(IOrmProvider ormProvider, Type expectType, object value)
    {
        if (value is Guid gVaue)
            return gVaue.ToString();
        return "NULL";
    }
}