using System;

namespace Trolley;

public class PostgreSqlTimeSpanTypeHandler : TimeSpanTypeHandler, ITypeHandler
{
    public override string Format { get; set; } = "hh\\:mm\\:ss";
    public override string GetQuotedValue(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is TimeSpan tsValue)
        {
            if (tsValue.TotalDays > 1 || tsValue.TotalDays < -1)
                return $"INTERVAL '{(int)tsValue.TotalDays}D {tsValue.ToString(Format)}'";
            return $"INTERVAL '{tsValue.ToString(this.Format)}'";
        }
        return $"INTERVAL '0S'";
    }
}
public class PostgreSqlNullableTimeSpanTypeHandler : NullableTimeSpanTypeHandler, ITypeHandler
{
    public override string Format { get; set; } = "hh\\:mm\\:ss";
    public override string GetQuotedValue(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is TimeSpan tsValue)
        {
            if (tsValue.TotalDays > 1 || tsValue.TotalDays < -1)
                return $"INTERVAL '{(int)tsValue.TotalDays}D {tsValue.ToString(Format)}'";
            return $"INTERVAL '{tsValue.ToString(this.Format)}'";
        }
        return "NULL";
    }
}
