using System;

namespace Trolley;

public class PostgreSqlDateTimeOffsetTypeHandler : DateTimeOffsetTypeHandler, ITypeHandler
{
    public override string GetQuotedValue(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is DateTimeOffset dtoValue)
            return $"TIMESTAMPTZ {dtoValue.ToString(this.Format)}";
        return $"TIMESTAMPTZ {DateTimeOffset.MinValue.ToString(this.Format)}";
    }
}
public class PostgreSqlNullableDateTimeOffsetTypeHandler : NullableDateTimeOffsetTypeHandler, ITypeHandler
{
     public override string GetQuotedValue(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is DateTimeOffset dtoValue)
            return $"TIMESTAMPTZ {dtoValue.ToString(this.Format)}";
        return "NULL";
    }
}