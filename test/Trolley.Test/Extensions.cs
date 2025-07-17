using Microsoft.Data.SqlClient;
using MySqlConnector;
using Npgsql;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Reflection;
using System.Text;

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
    public static string ToMySqlParametersString(this IDataParameterCollection dbParameters)
    {
        if (dbParameters == null || dbParameters.Count == 0)
            return string.Empty;
        var builder = new StringBuilder();
        foreach (var parameter in dbParameters)
        {
            var dbParameter = parameter as MySqlParameter;
            builder.Append($"{dbParameter.ParameterName}:{{MySqlDbType={dbParameter.MySqlDbType}, Value={dbParameter.Value}}};  ");
        }
        return builder.ToString();
    }
    public static string ToPostgreSqlParametersString(this IDataParameterCollection dbParameters)
    {
        if (dbParameters == null || dbParameters.Count == 0)
            return string.Empty;
        var builder = new StringBuilder();
        foreach (var parameter in dbParameters)
        {
            var dbParameter = parameter as NpgsqlParameter;
            builder.Append($"{dbParameter.ParameterName}:{{NpgsqlDbType={dbParameter.NpgsqlDbType}, Value={dbParameter.Value}}};  ");
        }
        return builder.ToString();
    }
    public static string ToSqlServerParametersString(this IDataParameterCollection dbParameters)
    {
        if (dbParameters == null || dbParameters.Count == 0)
            return string.Empty;
        var builder = new StringBuilder();
        foreach (var parameter in dbParameters)
        {
            var dbParameter = parameter as SqlParameter;
            builder.Append($"{dbParameter.ParameterName}:{{SqlDbType={dbParameter.SqlDbType}, Value={dbParameter.Value}}};  ");
        }
        return builder.ToString();
    }
    public static string Slice(this string strValue, int index, int endIndex)
    {
        if (string.IsNullOrEmpty(strValue))
            return string.Empty;
        if (index < 0) index = strValue.Length + index - 1;
        if (endIndex > 0)
            return strValue.Substring(index, endIndex - index);
        else if (endIndex == 0)
            return strValue.Substring(index);
        return strValue.Substring(index, strValue.Length + endIndex - index - 1);
    }
}