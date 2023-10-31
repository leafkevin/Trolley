using Microsoft.Data.SqlClient;
using System;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Trolley.Test;

class NpgSqlDatabaseGenerator
{
    private readonly string connectionString;

    public NpgSqlDatabaseGenerator(string connectionString)
        => this.connectionString = connectionString;

    public async void CreateTable(Type entityType)
    {
        var tableName = entityType.Name;
        var tableAttribute = entityType.GetCustomAttribute<TableAttribute>();
        if (tableAttribute != null)
            tableName = tableAttribute.TableName;

        var memberInfos = entityType.GetMembers(BindingFlags.Public | BindingFlags.Instance)
            .Where(f => f.MemberType == MemberTypes.Property | f.MemberType == MemberTypes.Field).ToList();
        var builder = new StringBuilder();
        builder.AppendLine($"CREATE TABLE IF NOT EXISTS \"{tableName}\"");
        builder.AppendLine("(");
        string pkFields = string.Empty;
        int index = 0;
        foreach (var memberInfo in memberInfos)
        {
            var referenceAttribute = memberInfo.GetCustomAttribute<ReferenceAttribute>();
            if (referenceAttribute != null)
                continue;
            var ignoreAttribute = memberInfo.GetCustomAttribute<IgnoreAttribute>();
            if (ignoreAttribute != null)
                continue;

            Type memberType = null;
            switch (memberInfo.MemberType)
            {
                case MemberTypes.Field:
                    var fieldInfo = memberInfo as FieldInfo;
                    memberType = fieldInfo.FieldType;
                    break;
                case MemberTypes.Property:
                    var propertyInfo = memberInfo as PropertyInfo;
                    memberType = propertyInfo.PropertyType;
                    break;
            }
            string fieldName = memberInfo.Name;
            string dbType = this.MapDefaultDbType(memberType);
            bool isAutoIncrement = false;
            bool isIdentity = false;
            bool isNullable = true;
            var keyAttribute = memberInfo.GetCustomAttribute<KeyAttribute>();
            if (keyAttribute != null)
            {
                if (!string.IsNullOrEmpty(keyAttribute.FieldName))
                    fieldName = keyAttribute.FieldName;
                if (!string.IsNullOrEmpty(keyAttribute.DbType))
                    dbType = keyAttribute.DbType.ToUpper();
                isIdentity = keyAttribute.IsIdentity;
                isNullable = false;
            }
            var fieldAttribute = memberInfo.GetCustomAttribute<FieldAttribute>();
            if (fieldAttribute != null)
            {
                if (!string.IsNullOrEmpty(fieldAttribute.FieldName))
                    fieldName = fieldAttribute.FieldName;
                if (!string.IsNullOrEmpty(fieldAttribute.DbType))
                    dbType = fieldAttribute.DbType.ToUpper();
                isAutoIncrement = isAutoIncrement || fieldAttribute.AutoIncrement;
            }
            if ((isIdentity || isAutoIncrement) && !dbType.Contains("SERIAL"))
            {
                switch (Type.GetTypeCode(memberType))
                {
                    case TypeCode.Byte:
                    case TypeCode.SByte:
                    case TypeCode.Int16:
                    case TypeCode.UInt16:
                        dbType = "SMALLSERIAL"; break;
                    case TypeCode.Int32:
                    case TypeCode.UInt32:
                        dbType = "SERIAL"; break;
                    case TypeCode.Int64:
                    case TypeCode.UInt64:
                        dbType = "BIGSERIAL"; break;
                    default: dbType = "SERIAL"; break;
                }
            }
            if (keyAttribute != null)
            {
                if (pkFields.Length > 0)
                    pkFields += ",";
                pkFields += $"\"{fieldName}\"";
            }
            if (index > 0) builder.AppendLine(",");
            builder.Append($"\"{fieldName}\" {dbType}");
            if (!isNullable) builder.Append(" NOT");
            builder.Append(" NULL");
        }
        if (pkFields.Length > 0)
        {
            builder.AppendLine(",");
            builder.AppendLine($"CONSTRAINT pk_{tableName} PRIMARY KEY ({pkFields})");
        }
        builder.AppendLine(");");
        var sql = builder.ToString();
        using var connection = new SqlConnection(this.connectionString);
        using var command = new SqlCommand(sql, connection);
        connection.Open();
        await command.ExecuteNonQueryAsync();
        connection.Close();
    }
    public string MapDefaultDbType(Type memberType)
    {
        var underlyingType = memberType;
        if (memberType.IsValueType)
        {
            underlyingType = Nullable.GetUnderlyingType(memberType);
            if (underlyingType == null) underlyingType = memberType;
        }
        switch (Type.GetTypeCode(underlyingType))
        {
            case TypeCode.Boolean: return "bool";
            case TypeCode.SByte:
            case TypeCode.Byte:
            case TypeCode.Int16:
            case TypeCode.UInt16: return "int2";
            case TypeCode.Int32:
            case TypeCode.UInt32: return "int4";
            case TypeCode.Int64:
            case TypeCode.UInt64: return "int8";
            case TypeCode.Single: return "float";
            case TypeCode.Double: return "double";
            case TypeCode.Decimal: return "decimal";
            case TypeCode.DateTime: return "datetime";

            case TypeCode.Char: return "char(1)";
            case TypeCode.String: return "varchar(100)";
        }
        if (memberType == typeof(Guid))
            return "uuid";
        if (memberType == typeof(byte[]))
            return "bytea";
        return "varchar(100)";
    }
}
