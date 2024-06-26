using System;

namespace Trolley;

public class ByteArrayTypeHandler : ITypeHandler
{
    public virtual object Parse(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is byte[])
            return value;
        if (value is long lValue)
            return BitConverter.GetBytes(lValue);
        return null;
    }
    public virtual object ToFieldValue(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is byte[] bytes)
            return bytes;
        if (value is long lValue)
            return BitConverter.GetBytes(lValue);
        return null;
    }
    public virtual string GetQuotedValue(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is byte[] bytes)
            return BitConverter.ToInt64(bytes).ToString();
        if (value is long lValue)
            return lValue.ToString();
        return "0";
    }
}
public class ByteArrayAsLongTypeHandler : ITypeHandler
{
    public virtual object Parse(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is long lValue)
            return BitConverter.GetBytes(lValue);
        return value;
    }
    public virtual object ToFieldValue(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is long lValue)
            return BitConverter.GetBytes(lValue);
        return value;
    }
    public virtual string GetQuotedValue(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is byte[] bytes)
            return BitConverter.ToInt64(bytes).ToString();
        if (value is long lValue)
            return lValue.ToString();
        return "0";
    }
}
