using System;

namespace Trolley;

public class DateTimeTypeHandler : ITypeHandler
{
    public virtual string Format { get; set; } = "\\'yyyy-MM-dd HH:mm:ss.fff\\'";
    public virtual object Parse(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is DateTime)
            return value;
        return DateTime.MinValue;
    }
    public virtual object ToFieldValue(IOrmProvider ormProvider, Type underlyingType, object value) => value;
    public virtual string GetQuotedValue(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is DateTime dtValue)
            return dtValue.ToString(this.Format);
        return DateTime.MinValue.ToString(this.Format);
    }
}
public class NullableDateTimeTypeHandler : ITypeHandler
{
    public virtual string Format { get; set; } = "\\'yyyy-MM-dd HH:mm:ss.fff\\'";
    public virtual object Parse(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is DateTime)
            return value;
        return null;
    }
    public virtual object ToFieldValue(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is DateTime dtValue)
            return dtValue;
        return DBNull.Value;
    }
    public virtual string GetQuotedValue(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is DateTime dtValue)
            return dtValue.ToString(this.Format);
        return "NULL";
    }
}
public class DateTimeAsStringTypeHandler : ITypeHandler
{
    public virtual string Format { get; set; } = "\\'yyyy-MM-dd HH:mm:ss.fff\\'";
    public virtual object Parse(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is string strValue && !string.IsNullOrEmpty(strValue))
            return DateTime.Parse(strValue);
        return DateTime.MinValue;
    }
    public virtual object ToFieldValue(IOrmProvider ormProvider, Type underlyingType, object value)
        => this.GetQuotedValue(ormProvider, underlyingType, value);
    public virtual string GetQuotedValue(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is DateTime dtValue)
            return dtValue.ToString(this.Format);
        return DateTime.MinValue.ToString(this.Format);
    }
}
public class NullableDateTimeAsStringTypeHandler : ITypeHandler
{
    public virtual string Format { get; set; } = "\\'yyyy-MM-dd HH:mm:ss.fff\\'";
    public virtual object Parse(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is string strValue && !string.IsNullOrEmpty(strValue))
            return DateTime.Parse(strValue);
        return null;
    }
    public virtual object ToFieldValue(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is DateTime dtValue)
            return dtValue;
        return DBNull.Value;
    }
    public virtual string GetQuotedValue(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is DateTime dtValue)
            return dtValue.ToString(this.Format);
        return "NULL";
    }
}
