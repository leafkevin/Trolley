using System;

namespace Trolley.MySqlConnector;

public class MySqlTimeSpanTypeHandler : TimeSpanTypeHandler
{
    public virtual string RestFormat { get; set; } = "\\:mm\\:ss\\.ffffff";
    public override object Parse(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is TimeSpan)
            return value;
        if (value is string strValue && !string.IsNullOrEmpty(strValue))
        {
            var hourEndIndex = strValue.IndexOf(':');
            var hour = int.Parse(strValue.Substring(0, hourEndIndex));
            if (hour > -24 && hour < 24)
                return TimeSpan.Parse(strValue);

            var hourValue = TimeSpan.FromHours(hour);
            var restValue = TimeSpan.Parse("00" + strValue.Substring(hourEndIndex));
            return hourValue.Add(restValue);
        }
        return TimeSpan.MinValue;
    }
    public override string GetQuotedValue(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is TimeSpan tsValue)
        {
            if (tsValue.TotalDays > 24 || tsValue.TotalDays < -24)
                return $"'{(int)tsValue.TotalHours}{tsValue.ToString(this.RestFormat)}'";
            return $"'{tsValue.ToString(this.Format)}'";
        }
        return TimeSpan.MinValue.ToString(this.Format);
    }
}
public class MySqlNullableTimeSpanTypeHandler : NullableTimeSpanTypeHandler
{
    public virtual string RestFormat { get; set; } = "\\:mm\\:ss\\.ffffff";
    public override object Parse(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is TimeSpan)
            return value;
        if (value is string strValue && !string.IsNullOrEmpty(strValue))
        {
            var hourEndIndex = strValue.IndexOf(':');
            var hour = int.Parse(strValue.Substring(0, hourEndIndex));
            if (hour > -24 && hour < 24)
                return TimeSpan.Parse(strValue);

            var hourValue = TimeSpan.FromHours(hour);
            var restValue = TimeSpan.Parse("00" + strValue.Substring(hourEndIndex));
            return hourValue.Add(restValue);
        }
        return null;
    }
    public override string GetQuotedValue(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is TimeSpan tsValue)
        {
            if (tsValue.TotalDays > 24 || tsValue.TotalDays < -24)
                return $"'{(int)tsValue.TotalHours}{tsValue.ToString(this.RestFormat)}'";
            return $"'{tsValue.ToString(this.Format)}'";
        }
        return "NULL";
    }
}
public class MySqlTimeSpanAsStringTypeHandler : TimeSpanAsStringTypeHandler
{
    public virtual string RestFormat { get; set; } = "\\:mm\\:ss\\.ffffff";
    public override object Parse(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is string strValue && !string.IsNullOrEmpty(strValue))
        {
            var hourEndIndex = strValue.IndexOf(':');
            var hour = int.Parse(strValue.Substring(0, hourEndIndex));
            if (hour > -24 && hour < 24)
                return TimeSpan.Parse(strValue);

            var hourValue = TimeSpan.FromHours(hour);
            var restValue = TimeSpan.Parse("00" + strValue.Substring(hourEndIndex));
            return hourValue.Add(restValue);
        }
        return TimeSpan.MinValue;
    }
    public override string GetQuotedValue(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is TimeSpan tsValue)
        {
            if (tsValue.TotalDays > 24 || tsValue.TotalDays < -24)
                return $"'{(int)tsValue.TotalDays}{tsValue.ToString(this.RestFormat)}'";
            return $"'{tsValue.ToString(this.Format)}'";
        }
        return TimeSpan.MinValue.ToString(this.Format);
    }
}
public class MySqlNullableTimeSpanAsStringTypeHandler : NullableTimeSpanAsStringTypeHandler
{
    public virtual string RestFormat { get; set; } = "\\:mm\\:ss\\.ffffff";
    public override object Parse(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is string strValue && !string.IsNullOrEmpty(strValue))
        {
            var hourEndIndex = strValue.IndexOf(':');
            var hour = int.Parse(strValue.Substring(0, hourEndIndex));
            if (hour > -24 && hour < 24)
                return TimeSpan.Parse(strValue);

            var hourValue = TimeSpan.FromHours(hour);
            var restValue = TimeSpan.Parse("00" + strValue.Substring(hourEndIndex));
            return hourValue.Add(restValue);
        }
        return null;
    }
    public override string GetQuotedValue(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is TimeSpan tsValue)
        {
            if (tsValue.TotalDays > 24 || tsValue.TotalDays < -24)
                return $"'{(int)tsValue.TotalDays}{tsValue.ToString(this.RestFormat)}'";
            return $"'{tsValue.ToString(this.Format)}'";
        }
        return "NULL";
    }
}