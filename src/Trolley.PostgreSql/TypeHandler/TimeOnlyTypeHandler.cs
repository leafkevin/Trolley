using System;

namespace Trolley;

public class PostgreSqlTimeOnlyTypeHandler : TimeOnlyTypeHandler, ITypeHandler
{
    public override string GetQuotedValue(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is TimeOnly toValue)
            return $"TIME {toValue.ToString(this.Format)}";
        return $"TIME {TimeOnly.MinValue.ToString(this.Format)}";
    }
}
public class PostgreSqlNullableTimeOnlyTypeHandler : NullableTimeOnlyTypeHandler, ITypeHandler
{
    public override string GetQuotedValue(IOrmProvider ormProvider, Type underlyingType, object value)
    {
        if (value is TimeOnly toValue)
            return $"TIME {toValue.ToString(this.Format)}";
        return "NULL";
    }
}