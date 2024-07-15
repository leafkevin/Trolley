using System;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Trolley;

public class JsonTypeHandler : ITypeHandler
{
    public virtual JsonSerializerOptions SerializerOptions { get; set; } = new JsonSerializerOptions
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        ReferenceHandler = ReferenceHandler.IgnoreCycles
    };
    public virtual object Parse(IOrmProvider ormProvider, Type targetType, object value)
    {
        if (value is DBNull) return null;
        return JsonSerializer.Deserialize(value as string, targetType, SerializerOptions);
    }
    public virtual object ToFieldValue(IOrmProvider ormProvider, object value)
    {
        if (value != null)
            return JsonSerializer.Serialize(value, SerializerOptions);
        return DBNull.Value;
    }
    public virtual string GetQuotedValue(IOrmProvider ormProvider, object value)
    {
        if (value != null)
            return $"'{JsonSerializer.Serialize(value, SerializerOptions)}'";
        return "NULL";
    }
}
