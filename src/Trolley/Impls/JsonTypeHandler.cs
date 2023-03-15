using System;
using System.Data;
using System.Text.Json;

namespace Trolley;

public class JsonTypeHandler : ITypeHandler
{
    public virtual void SetValue(IOrmProvider ormProvider, IDbDataParameter parameter, object value)
    {
        if (value == null)
            parameter.Value = DBNull.Value;
        else parameter.Value = JsonSerializer.Serialize(value);
    }
    public virtual object Parse(IOrmProvider ormProvider, Type TargetType, object value)
    {
        if (value is DBNull) return null;
        return JsonSerializer.Deserialize(value as string, TargetType);
    }
}
