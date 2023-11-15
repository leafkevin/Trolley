using System;
using System.Data;
using System.Text.Json;

namespace Trolley;

public class JsonTypeHandler : ITypeHandler
{
    public virtual object Parse(IOrmProvider ormProvider, Type TargetType, object value)
    {
        if (value is DBNull) return null;
        return JsonSerializer.Deserialize(value as string, TargetType);
    }
    public virtual object ToFieldValue(IOrmProvider ormProvider, object value)
    {
        if (value != null)
            return JsonSerializer.Serialize(value);
        return null;
    }
}
