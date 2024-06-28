using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace Trolley.Test;

public static class Extensions
{
    private static ConcurrentDictionary<Type, Dictionary<object, string>> enumDescriptions = new();
    public static string ToDescription<TEnum>(this TEnum enumObj) where TEnum : struct, Enum
    {
        var enumType = typeof(TEnum);
        object enumValue = null;
        if (enumObj is TEnum typedValue)
            enumValue = typedValue;
        else enumValue = Enum.ToObject(enumType, enumObj);
        if (!enumDescriptions.TryGetValue(enumType, out var descriptions))
        {
            var enumValues = Enum.GetValues(enumType);
            descriptions = new Dictionary<object, string>();
            foreach (var value in enumValues)
            {
                string description = null;
                var enumName = Enum.GetName(enumType, value);
                var fieldInfo = enumType.GetField(enumName);
                if (fieldInfo != null)
                {
                    var descAttr = fieldInfo.GetCustomAttribute<DescriptionAttribute>();
                    if (descAttr != null)
                        description = descAttr.Description;
                }
                descriptions.Add(value, description ?? enumName);
            }
            enumDescriptions.TryAdd(enumType, descriptions);
        }
        return descriptions[enumValue];
    }
}
