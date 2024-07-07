using System;

namespace Trolley;

public class PostgreSqlDateTimeTypeHandler : DateTimeTypeHandler, ITypeHandler
{
    public override string GetQuotedValue(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is DateTimeOffset dtoValue)
            return $"TIMESTAMP {dtoValue.ToString(this.Format)}";
        return $"TIMESTAMP {DateTimeOffset.MinValue.ToString(this.Format)}";
    }
}
public class PostgreSqlNullableDateTimeTypeHandler : NullableDateTimeTypeHandler, ITypeHandler
{
    public override string GetQuotedValue(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is DateTimeOffset dtoValue)
            return $"TIMESTAMP {dtoValue.ToString(this.Format)}";
        return "NULL";
    }
}
