using System;

namespace Trolley.PostgreSql;

public class PostgreSqlDateOnlyTypeHandler : DateOnlyTypeHandler, ITypeHandler
{
    public override string GetQuotedValue(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is DateOnly doValue)
            return $"DATE {doValue.ToString(this.Format)}";
        return $"DATE {DateOnly.MinValue.ToString(this.Format)}";
    }
}
public class PostgreSqlNullableDateOnlyTypeHandler : NullableDateOnlyTypeHandler, ITypeHandler
{
    public override string GetQuotedValue(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is DateOnly doValue)
            return $"DATE {doValue.ToString(this.Format)}";
        return "NULL";
    }
}