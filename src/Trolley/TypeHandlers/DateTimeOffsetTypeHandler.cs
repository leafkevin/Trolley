using System;

namespace Trolley;

public class DateTimeOffsetTypeHandler : ITypeHandler
{
    /// <summary>
    /// 有几种格式：yyyy-MM-ddTHH:mm:ssZ、yyyy-MM-ddTHH:mm:ss.fffZ、yyyy-MM-dd HH:mm:ss zzz、yyyy-MM-dd HH:mm:ss.fff zzz
    /// </summary>
    public virtual string Format { get; set; } = "\\'yyyy-MM-ddTHH:mm:ss.fffZ\\'";
    public virtual object Parse(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is DateTimeOffset)
            return value;
        if (value is DateTime dtValue)
            return new DateTimeOffset(dtValue);
        return DateTimeOffset.MinValue;
    }
    public virtual object ToFieldValue(IOrmProvider ormProvider, Type underlyingType, object value) => value;
    public virtual string GetQuotedValue(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is DateTimeOffset dtoValue)
            return dtoValue.ToString(this.Format);
        return DateTimeOffset.MinValue.ToString(this.Format);
    }
}
public class NullableDateTimeOffsetTypeHandler : ITypeHandler
{
    /// <summary>
    /// 有几种格式：yyyy-MM-ddTHH:mm:ssZ、yyyy-MM-ddTHH:mm:ss.fffZ、yyyy-MM-dd HH:mm:ss zzz、yyyy-MM-dd HH:mm:ss.fff zzz
    /// </summary>
    public virtual string Format { get; set; } = "\\'yyyy-MM-ddTHH:mm:ss.fffZ\\'";
    public virtual object Parse(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is DateTimeOffset)
            return value;
        if (value is DateTime dtValue)
            return new DateTimeOffset(dtValue);
        return null;
    }
    public virtual object ToFieldValue(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is DateTimeOffset)
            return value;
        return DBNull.Value;
    }
    /// <summary>
    /// 有几种格式：yyyy-MM-ddThh:mm:ssZ、yyyy-MM-ddThh:mm:ss.fffZ、yyyy-MM-dd HH:mm:ss zzz、yyyy-MM-dd HH:mm:ss.fff zzz
    /// </summary>
    /// <param name="ormProvider"></param>
    /// <param name="fieldType"></param>
    /// <param name="value"></param>
    /// <param name="format"></param>
    /// <returns></returns>
    public virtual string GetQuotedValue(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is DateTimeOffset dtoValue)
            return dtoValue.ToString(this.Format);
        return "NULL";
    }
}
public class DateTimeOffsetAsStringTypeHandler : ITypeHandler
{
    /// <summary>
    /// 有几种格式：yyyy-MM-ddTHH:mm:ssZ、yyyy-MM-ddTHH:mm:ss.fffZ、yyyy-MM-dd HH:mm:ss zzz、yyyy-MM-dd HH:mm:ss.fff zzz
    /// </summary>
    public virtual string Format { get; set; } = "\\'yyyy-MM-ddTHH:mm:ss.fffZ\\'";
    public virtual object Parse(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is string strValue && !string.IsNullOrEmpty(strValue))
            return DateTimeOffset.Parse(strValue);
        return DateTimeOffset.MinValue;
    }
    public virtual object ToFieldValue(IOrmProvider ormProvider, Type underlyingType, object value)
        => this.GetQuotedValue(ormProvider, underlyingType, value);
    public virtual string GetQuotedValue(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is DateTimeOffset dtoValue)
            return dtoValue.ToString(this.Format);
        return DateTimeOffset.MinValue.ToString(this.Format);
    }
}
public class NullableDateTimeOffsetAsStringTypeHandler : ITypeHandler
{
    /// <summary>
    /// 有几种格式：yyyy-MM-ddTHH:mm:ssZ、yyyy-MM-ddTHH:mm:ss.fffZ、yyyy-MM-dd HH:mm:ss zzz、yyyy-MM-dd HH:mm:ss.fff zzz
    /// </summary>
    public virtual string Format { get; set; } = "\\'yyyy-MM-ddTHH:mm:ss.fffZ\\'";
    public virtual object Parse(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is string strValue && !string.IsNullOrEmpty(strValue))
            return DateTimeOffset.Parse(strValue);
        return null;
    }
    public virtual object ToFieldValue(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is DateTimeOffset dtoValue)
            return dtoValue.ToString(this.Format);
        return DBNull.Value;
    }
    /// <summary>
    /// 有几种格式：yyyy-MM-ddThh:mm:ssZ、yyyy-MM-ddThh:mm:ss.fffZ、yyyy-MM-dd HH:mm:ss zzz、yyyy-MM-dd HH:mm:ss.fff zzz
    /// </summary>
    /// <param name="ormProvider"></param>
    /// <param name="fieldType"></param>
    /// <param name="value"></param>
    /// <param name="format"></param>
    /// <returns></returns>
    public virtual string GetQuotedValue(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is DateTimeOffset dtoValue)
            return dtoValue.ToString(this.Format);
        return "NULL";
    }
}