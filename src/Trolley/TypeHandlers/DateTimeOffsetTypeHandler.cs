using System;

namespace Trolley;

public class DateTimeOffsetTypeHandler : ITypeHandler
{
    /// <summary>
    /// 有几种格式：yyyy-MM-ddThh:mm:ssZ、yyyy-MM-ddThh:mm:ss.fffZ、yyyy-MM-dd HH:mm:ss zzz、yyyy-MM-dd HH:mm:ss.fff zzz
    /// </summary>
    public virtual string Format { get; set; } = "yyyy-MM-ddThh:mm:ss.fffZ";
    public virtual object Parse(IOrmProvider ormProvider, Type targetType, object value)
    {
        if (value is DateTimeOffset)
            return value;
        return DateTimeOffset.MinValue;
    }
    public virtual object ToFieldValue(IOrmProvider ormProvider, Type expectType, object value) => value;
    public virtual string GetQuotedValue(IOrmProvider ormProvider, Type expectType, object value)
    {
        if (value is DateTimeOffset toVaue)
            return toVaue.ToString(this.Format);
        return DateTimeOffset.MinValue.ToString(this.Format);
    }
}
public class NullableDateTimeOffsetTypeHandler : ITypeHandler
{
    /// <summary>
    /// 有几种格式：yyyy-MM-ddThh:mm:ssZ、yyyy-MM-ddThh:mm:ss.fffZ、yyyy-MM-dd HH:mm:ss zzz、yyyy-MM-dd HH:mm:ss.fff zzz
    /// </summary>
    public virtual string Format { get; set; } = "yyyy-MM-ddThh:mm:ss.fffZ";
    public virtual object Parse(IOrmProvider ormProvider, Type targetType, object value)
    {
        if (value is DateTimeOffset)
            return value;
        return null;
    }
    public virtual object ToFieldValue(IOrmProvider ormProvider, Type expectType, object value)
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
    public virtual string GetQuotedValue(IOrmProvider ormProvider, Type expectType, object value)
    {
        if (value is DateTimeOffset toVaue)
            return toVaue.ToString(this.Format);
        return "NULL";
    }
}