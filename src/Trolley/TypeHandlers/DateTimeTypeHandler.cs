using System;

namespace Trolley;

public class DateTimeTypeHandler : ITypeHandler
{
    public virtual string Format { get; set; } = "yyyy-MM-dd hh:mm:ss.fff";
    public virtual object Parse(IOrmProvider ormProvider, Type targetType, object value)
    {
        if (value is DateTime)
            return value;
        return DateTime.MinValue;
    }
    public virtual object ToFieldValue(IOrmProvider ormProvider, Type expectType, object value) => value;
    public virtual string GetQuotedValue(IOrmProvider ormProvider, Type expectType, object value)
    {
        if (value is DateTimeOffset toVaue)
            return toVaue.ToString(this.Format);
        return DateTimeOffset.MinValue.ToString(this.Format);
    }
}
public class NullableDateTimeTypeHandler : ITypeHandler
{
    public virtual string Format { get; set; } = "yyyy-MM-dd hh:mm:ss.fff";
    public virtual object Parse(IOrmProvider ormProvider, Type targetType, object value)
    {
        if (value is DateTime)
            return value;
        return null;
    }
    public virtual object ToFieldValue(IOrmProvider ormProvider, Type expectType, object value)
    {
        if (value is DateTime dtValue)
            return dtValue;
        return DBNull.Value;
    }
    public virtual string GetQuotedValue(IOrmProvider ormProvider, Type expectType, object value)
    {
        if (value is DateTimeOffset toVaue)
            return toVaue.ToString(this.Format);
        return "NULL";
    }
}
public class DateTimeAsStringTypeHandler : ITypeHandler
{
    public virtual string Format { get; set; } = "yyyy-MM-dd hh:mm:ss.fff";
    public virtual object Parse(IOrmProvider ormProvider, Type targetType, object value)
    {
        if (value is DateTime)
            return value;
        if (value is string strValue)
        {
            if (string.IsNullOrEmpty(strValue))
                return DateTime.MinValue;
            return DateTime.Parse(strValue);
        }
        return DateTime.MinValue;
    }
    public virtual object ToFieldValue(IOrmProvider ormProvider, Type expectType, object value)
    {
        if (value is DateTime dtValue)
            return dtValue.ToString(this.Format);
        return DateTime.MinValue.ToString(this.Format);
    }
    public virtual string GetQuotedValue(IOrmProvider ormProvider, Type expectType, object value)
    {
        if (value is DateTime dtValue)
            return dtValue.ToString(this.Format);
        return DateTime.MinValue.ToString(this.Format);
    }
}
public class NullableDateTimeAsStringTypeHandler : ITypeHandler
{
    public virtual string Format { get; set; } = "yyyy-MM-dd hh:mm:ss.fff";
    public virtual object Parse(IOrmProvider ormProvider, Type targetType, object value)
    {
        if (value is DateTime)
            return value;
        if (value is string strValue)
        {
            if (string.IsNullOrEmpty(strValue))
                return null;
            return DateTime.Parse(strValue);
        }
        return null;
    }
    public virtual object ToFieldValue(IOrmProvider ormProvider, Type expectType, object value)
    {
        if (value is DateTime dtValue)
            return dtValue.ToString(this.Format);
        return DBNull.Value;
    }
    public virtual string GetQuotedValue(IOrmProvider ormProvider, Type expectType, object value)
    {
        if (value is DateTime toVaue)
            return toVaue.ToString(this.Format);
        return "NULL";
    }
}
