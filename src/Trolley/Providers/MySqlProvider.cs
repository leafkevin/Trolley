using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;

namespace Trolley;

public partial class MySqlProvider : BaseOrmProvider
{
    private static Func<string, IDbConnection> createNativeConnectonDelegate = null;
    private static Func<string, object, IDbDataParameter> createDefaultNativeParameterDelegate = null;
    private static Func<string, object, object, IDbDataParameter> createNativeParameterDelegate = null;
    private static ConcurrentDictionary<int, MemberAccessSqlFormatter> memberAccessSqlFormatterCahe = new();
    private static ConcurrentDictionary<int, MethodCallSqlFormatter> methodCallSqlFormatterCahe = new();
    private static Dictionary<object, Type> defaultMapTypes = new();
    private static Dictionary<Type, object> defaultDbTypes = new();
    private static Dictionary<int, object> nativeDbTypes = new();
    private static Dictionary<Type, string> castTos = new();

    public override DatabaseType DatabaseType => DatabaseType.MySql;
    public override string SelectIdentitySql => " RETURNING {0}";
    static MySqlProvider()
    {
        var connectionType = Type.GetType("MySqlConnector.MySqlConnection, MySqlConnector, Version=2.0.0.0, Culture=neutral, PublicKeyToken=d33d3e53aa5f8c92");
        createNativeConnectonDelegate = BaseOrmProvider.CreateConnectionDelegate(connectionType);
        var dbTypeType = Type.GetType("MySqlConnector.MySqlDbType, MySqlConnector, Version=2.0.0.0, Culture=neutral, PublicKeyToken=d33d3e53aa5f8c92");
        var dbParameterType = Type.GetType("MySqlConnector.MySqlParameter, MySqlConnector, Version=2.0.0.0, Culture=neutral, PublicKeyToken=d33d3e53aa5f8c92");
        var valuePropertyInfo = dbParameterType.GetProperty("Value");
        createDefaultNativeParameterDelegate = BaseOrmProvider.CreateDefaultParameterDelegate(dbParameterType);
        createNativeParameterDelegate = BaseOrmProvider.CreateParameterDelegate(dbTypeType, dbParameterType, valuePropertyInfo);


        defaultMapTypes[Enum.Parse(dbTypeType, "Bit")] = typeof(bool);
        defaultMapTypes[Enum.Parse(dbTypeType, "Bool")] = typeof(bool);
        defaultMapTypes[Enum.Parse(dbTypeType, "Byte")] = typeof(sbyte);
        defaultMapTypes[Enum.Parse(dbTypeType, "UByte")] = typeof(byte);
        defaultMapTypes[Enum.Parse(dbTypeType, "Int16")] = typeof(short);
        defaultMapTypes[Enum.Parse(dbTypeType, "UInt16")] = typeof(ushort);
        defaultMapTypes[Enum.Parse(dbTypeType, "Int24")] = typeof(int);
        defaultMapTypes[Enum.Parse(dbTypeType, "UInt24")] = typeof(uint);
        defaultMapTypes[Enum.Parse(dbTypeType, "Int32")] = typeof(int);
        defaultMapTypes[Enum.Parse(dbTypeType, "UInt32")] = typeof(uint);
        defaultMapTypes[Enum.Parse(dbTypeType, "Int64")] = typeof(long);
        defaultMapTypes[Enum.Parse(dbTypeType, "UInt64")] = typeof(ulong);
        defaultMapTypes[Enum.Parse(dbTypeType, "Float")] = typeof(float);
        defaultMapTypes[Enum.Parse(dbTypeType, "Double")] = typeof(double);
        defaultMapTypes[Enum.Parse(dbTypeType, "NewDecimal")] = typeof(decimal);
        defaultMapTypes[Enum.Parse(dbTypeType, "Decimal")] = typeof(decimal);
        defaultMapTypes[Enum.Parse(dbTypeType, "String")] = typeof(string);
        defaultMapTypes[Enum.Parse(dbTypeType, "VarString")] = typeof(string);
        defaultMapTypes[Enum.Parse(dbTypeType, "VarChar")] = typeof(string);
        defaultMapTypes[Enum.Parse(dbTypeType, "TinyText")] = typeof(string);
        defaultMapTypes[Enum.Parse(dbTypeType, "MediumText")] = typeof(string);
        defaultMapTypes[Enum.Parse(dbTypeType, "LongText")] = typeof(string);
        defaultMapTypes[Enum.Parse(dbTypeType, "Text")] = typeof(string);
        defaultMapTypes[Enum.Parse(dbTypeType, "JSON")] = typeof(string);
        defaultMapTypes[Enum.Parse(dbTypeType, "Date")] = typeof(DateTime);
        defaultMapTypes[Enum.Parse(dbTypeType, "Datetime")] = typeof(DateTime);
        defaultMapTypes[Enum.Parse(dbTypeType, "Newdate")] = typeof(DateTime);
        defaultMapTypes[Enum.Parse(dbTypeType, "Timestamp")] = typeof(DateTime);
        defaultMapTypes[Enum.Parse(dbTypeType, "Time")] = typeof(TimeSpan);
        defaultMapTypes[Enum.Parse(dbTypeType, "TinyBlob")] = typeof(byte[]);
        defaultMapTypes[Enum.Parse(dbTypeType, "MediumBlob")] = typeof(byte[]);
        defaultMapTypes[Enum.Parse(dbTypeType, "LongBlob")] = typeof(byte[]);
        defaultMapTypes[Enum.Parse(dbTypeType, "Blob")] = typeof(byte[]);
        defaultMapTypes[Enum.Parse(dbTypeType, "Binary")] = typeof(byte[]);
        defaultMapTypes[Enum.Parse(dbTypeType, "VarBinary")] = typeof(byte[]);
        defaultMapTypes[Enum.Parse(dbTypeType, "Guid")] = typeof(Guid);

        defaultDbTypes[typeof(bool)] = Enum.Parse(dbTypeType, "Bool");
        defaultDbTypes[typeof(sbyte)] = Enum.Parse(dbTypeType, "Byte");
        defaultDbTypes[typeof(byte)] = Enum.Parse(dbTypeType, "UByte");
        defaultDbTypes[typeof(short)] = Enum.Parse(dbTypeType, "Int16");
        defaultDbTypes[typeof(ushort)] = Enum.Parse(dbTypeType, "UInt16");
        defaultDbTypes[typeof(int)] = Enum.Parse(dbTypeType, "Int32");
        defaultDbTypes[typeof(uint)] = Enum.Parse(dbTypeType, "UInt32");
        defaultDbTypes[typeof(long)] = Enum.Parse(dbTypeType, "Int64");
        defaultDbTypes[typeof(ulong)] = Enum.Parse(dbTypeType, "UInt64");
        defaultDbTypes[typeof(float)] = Enum.Parse(dbTypeType, "Float");
        defaultDbTypes[typeof(double)] = Enum.Parse(dbTypeType, "Double");
        defaultDbTypes[typeof(decimal)] = Enum.Parse(dbTypeType, "Decimal");
        defaultDbTypes[typeof(string)] = Enum.Parse(dbTypeType, "VarChar");
        defaultDbTypes[typeof(TimeSpan)] = Enum.Parse(dbTypeType, "Time");
        defaultDbTypes[typeof(DateTime)] = Enum.Parse(dbTypeType, "DateTime");
        defaultDbTypes[typeof(Guid)] = Enum.Parse(dbTypeType, "Guid");
        defaultDbTypes[typeof(byte[])] = Enum.Parse(dbTypeType, "VarBinary");

        defaultDbTypes[typeof(bool?)] = Enum.Parse(dbTypeType, "Bool");
        defaultDbTypes[typeof(sbyte?)] = Enum.Parse(dbTypeType, "Byte");
        defaultDbTypes[typeof(byte?)] = Enum.Parse(dbTypeType, "UByte");
        defaultDbTypes[typeof(short?)] = Enum.Parse(dbTypeType, "Int16");
        defaultDbTypes[typeof(ushort?)] = Enum.Parse(dbTypeType, "UInt16");
        defaultDbTypes[typeof(int?)] = Enum.Parse(dbTypeType, "Int32");
        defaultDbTypes[typeof(uint?)] = Enum.Parse(dbTypeType, "UInt32");
        defaultDbTypes[typeof(long?)] = Enum.Parse(dbTypeType, "Int64");
        defaultDbTypes[typeof(ulong?)] = Enum.Parse(dbTypeType, "UInt64");
        defaultDbTypes[typeof(float?)] = Enum.Parse(dbTypeType, "Float");
        defaultDbTypes[typeof(double?)] = Enum.Parse(dbTypeType, "Double");
        defaultDbTypes[typeof(decimal?)] = Enum.Parse(dbTypeType, "Decimal");
        defaultDbTypes[typeof(TimeSpan?)] = Enum.Parse(dbTypeType, "Time");
        defaultDbTypes[typeof(DateTime?)] = Enum.Parse(dbTypeType, "DateTime");
        defaultDbTypes[typeof(Guid?)] = Enum.Parse(dbTypeType, "Guid");

        nativeDbTypes[(int)Enum.Parse(dbTypeType, "Decimal")] = Enum.Parse(dbTypeType, "Decimal");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "Byte")] = Enum.Parse(dbTypeType, "Byte");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "Int16")] = Enum.Parse(dbTypeType, "Int16");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "Int32")] = Enum.Parse(dbTypeType, "Int32");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "Float")] = Enum.Parse(dbTypeType, "Float");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "Double")] = Enum.Parse(dbTypeType, "Double");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "Null")] = Enum.Parse(dbTypeType, "Null");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "Timestamp")] = Enum.Parse(dbTypeType, "Timestamp");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "Int64")] = Enum.Parse(dbTypeType, "Int64");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "Int24")] = Enum.Parse(dbTypeType, "Int24");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "Date")] = Enum.Parse(dbTypeType, "Date");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "Time")] = Enum.Parse(dbTypeType, "Time");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "DateTime")] = Enum.Parse(dbTypeType, "DateTime");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "Datetime")] = Enum.Parse(dbTypeType, "Datetime");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "Year")] = Enum.Parse(dbTypeType, "Year");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "Newdate")] = Enum.Parse(dbTypeType, "Newdate");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "VarString")] = Enum.Parse(dbTypeType, "VarString");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "Bit")] = Enum.Parse(dbTypeType, "Bit");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "JSON")] = Enum.Parse(dbTypeType, "JSON");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "NewDecimal")] = Enum.Parse(dbTypeType, "NewDecimal");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "Enum")] = Enum.Parse(dbTypeType, "Enum");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "Set")] = Enum.Parse(dbTypeType, "Set");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "TinyBlob")] = Enum.Parse(dbTypeType, "TinyBlob");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "MediumBlob")] = Enum.Parse(dbTypeType, "MediumBlob");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "LongBlob")] = Enum.Parse(dbTypeType, "LongBlob");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "Blob")] = Enum.Parse(dbTypeType, "Blob");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "VarChar")] = Enum.Parse(dbTypeType, "VarChar");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "String")] = Enum.Parse(dbTypeType, "String");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "Geometry")] = Enum.Parse(dbTypeType, "Geometry");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "UByte")] = Enum.Parse(dbTypeType, "UByte");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "UInt16")] = Enum.Parse(dbTypeType, "UInt16");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "UInt32")] = Enum.Parse(dbTypeType, "UInt32");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "UInt64")] = Enum.Parse(dbTypeType, "UInt64");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "UInt24")] = Enum.Parse(dbTypeType, "UInt24");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "Binary")] = Enum.Parse(dbTypeType, "Binary");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "VarBinary")] = Enum.Parse(dbTypeType, "VarBinary");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "TinyText")] = Enum.Parse(dbTypeType, "TinyText");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "MediumText")] = Enum.Parse(dbTypeType, "MediumText");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "LongText")] = Enum.Parse(dbTypeType, "LongText");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "Text")] = Enum.Parse(dbTypeType, "Text");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "Guid")] = Enum.Parse(dbTypeType, "Guid");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "Bool")] = Enum.Parse(dbTypeType, "Bool");

        castTos[typeof(string)] = "CHAR";
        castTos[typeof(bool)] = "SIGNED";
        castTos[typeof(byte)] = "UNSIGNED";
        castTos[typeof(sbyte)] = "SIGNED";
        castTos[typeof(short)] = "SIGNED";
        castTos[typeof(ushort)] = "UNSIGNED";
        castTos[typeof(int)] = "SIGNED";
        castTos[typeof(uint)] = "UNSIGNED";
        castTos[typeof(long)] = "SIGNED";
        castTos[typeof(ulong)] = "UNSIGNED";
        castTos[typeof(decimal)] = "DECIMAL(36,18)";
        castTos[typeof(DateTime)] = "DATETIME";

        castTos[typeof(bool?)] = "SIGNED";
        castTos[typeof(byte?)] = "UNSIGNED";
        castTos[typeof(sbyte?)] = "SIGNED";
        castTos[typeof(short?)] = "SIGNED";
        castTos[typeof(ushort?)] = "UNSIGNED";
        castTos[typeof(int?)] = "SIGNED";
        castTos[typeof(uint?)] = "UNSIGNED";
        castTos[typeof(long?)] = "SIGNED";
        castTos[typeof(ulong?)] = "UNSIGNED";
        castTos[typeof(decimal?)] = "DECIMAL(36,18)";
        castTos[typeof(DateTime?)] = "DATETIME";
    }
    public override IDbConnection CreateConnection(string connectionString)
        => createNativeConnectonDelegate.Invoke(connectionString);
    public override IDbDataParameter CreateParameter(string parameterName, object value)
        => createDefaultNativeParameterDelegate.Invoke(parameterName, value);
    public override IDbDataParameter CreateParameter(string parameterName, object nativeDbType, object value)
        => createNativeParameterDelegate.Invoke(parameterName, nativeDbType, value);
    public override string GetTableName(string entityName) => "`" + entityName + "`";
    public override string GetFieldName(string propertyName) => "`" + propertyName + "`";
    public override object GetNativeDbType(Type fieldType)
    {
        if (!defaultDbTypes.TryGetValue(fieldType, out var dbType))
            throw new Exception($"类型{fieldType.FullName}没有对应的MySqlConnector.MySqlDbType映射类型");
        return dbType;
    }
    public override object GetNativeDbType(int nativeDbType)
    {
        if (nativeDbTypes.TryGetValue(nativeDbType, out var result))
            return result;
        var dbTypeType = Type.GetType("MySqlConnector.MySqlDbType, MySqlConnector, Version=2.0.0.0, Culture=neutral, PublicKeyToken=d33d3e53aa5f8c92");
        result = Enum.ToObject(dbTypeType, nativeDbType);
        if (result != null)
        {
            lock (this)
            {
                if (nativeDbTypes.TryGetValue(nativeDbType, out result))
                    return result;
                result = Enum.ToObject(dbTypeType, nativeDbType);
                nativeDbTypes.TryAdd(nativeDbType, result);
            }
            return result;
        }
        throw new Exception($"数值{nativeDbType}没有对应的MySqlConnector.MySqlDbType映射类型");
    }
    public override Type MapDefaultType(object nativeDbType)
    {
        if (defaultMapTypes.TryGetValue(nativeDbType, out var result))
            return result;
        return typeof(object);
    }
    public override string GetQuotedValue(Type expectType, object value)
    {
        if (expectType == typeof(TimeSpan) && value is TimeSpan timeSpan)
            return $"TIME('{timeSpan.ToString("d\\ hh\\:mm\\:ss\\.fffffff")}')";
        if (expectType == typeof(TimeOnly) && value is TimeOnly timeOnly)
            return $"TIME('{timeOnly.ToString("hh\\:mm\\:ss\\.fffffff")}')";
        return base.GetQuotedValue(expectType, value);
    }
    public override string CastTo(Type type, object value)
       => $"CAST({value} AS {castTos[type]})";
    public override bool TryGetMemberAccessSqlFormatter(MemberExpression memberExpr, out MemberAccessSqlFormatter formatter)
    {
        var memberInfo = memberExpr.Member;
        var cacheKey = HashCode.Combine(memberInfo.DeclaringType, memberInfo);
        if (!memberAccessSqlFormatterCahe.TryGetValue(cacheKey, out formatter))
        {
            bool result = false;
            if (memberInfo.DeclaringType == typeof(DateTime) && this.TryGetDateTimeMemberAccessSqlFormatter(memberExpr, out formatter))
                return true;
            if (memberInfo.DeclaringType == typeof(TimeSpan) && this.TryGetTimeSpanMemberAccessSqlFormatter(memberExpr, out formatter))
                return true;
            return result;
        }
        return true;
    }
    public override bool TryGetMethodCallSqlFormatter(MethodCallExpression methodCallExpr, out MethodCallSqlFormatter formatter)
    {
        var methodInfo = methodCallExpr.Method;
        var parameterInfos = methodInfo.GetParameters();
        var cacheKey = HashCode.Combine(methodInfo.DeclaringType, methodInfo);
        if (!methodCallSqlFormatterCahe.TryGetValue(cacheKey, out formatter))
        {
            bool result = false;
            if (methodInfo.DeclaringType == typeof(string) && this.TryGetStringMethodCallSqlFormatter(methodCallExpr, out formatter))
                return true;
            if (methodInfo.DeclaringType == typeof(DateTime) && this.TryGetDateTimeMethodCallSqlFormatter(methodCallExpr, out formatter))
                return true;
            if (methodInfo.DeclaringType == typeof(TimeSpan) && this.TryGetTimeSpanMethodCallSqlFormatter(methodCallExpr, out formatter))
                return true;
            if (methodInfo.DeclaringType == typeof(Convert) && this.TryGetConvertMethodCallSqlFormatter(methodCallExpr, out formatter))
                return true; if (this.TryGetIEnumerableMethodCallSqlFormatter(methodCallExpr, out formatter))
                return true;
            if (methodInfo.DeclaringType == typeof(Math) && this.TryGetMathMethodCallSqlFormatter(methodCallExpr, out formatter))
                return true;
            switch (methodInfo.Name)
            {
                case "Equals":
                    if (!methodInfo.IsStatic && parameterInfos.Length == 1)
                    {
                        methodCallSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(target);
                            var rightSegment = visitor.VisitAndDeferred(target);
                            targetSegment.Merge(rightSegment);
                            return targetSegment.Change($"{this.GetQuotedValue(targetSegment)}={this.GetQuotedValue(rightSegment)}", false, true);
                        });
                        result = true;
                    }
                    break;
                case "Compare":
                    if (methodInfo.IsStatic && parameterInfos.Length == 2)
                    {
                        methodCallSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                        {
                            var leftSegment = visitor.VisitAndDeferred(args[0]);
                            var rightSegment = visitor.VisitAndDeferred(args[1]);

                            leftSegment.Merge(rightSegment);
                            return leftSegment.Change($"(CASE WHEN {this.GetQuotedValue(leftSegment)}={this.GetQuotedValue(rightSegment)} THEN 0 WHEN {this.GetQuotedValue(leftSegment)}>{this.GetQuotedValue(rightSegment)} THEN 1 ELSE -1 END)", false, true);
                        });
                        result = true;
                    }
                    break;
                case "CompareTo":
                    if (!methodInfo.IsStatic && parameterInfos.Length == 1)
                    {
                        methodCallSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(target);
                            var rightSegment = visitor.VisitAndDeferred(args[0]);

                            targetSegment.Merge(rightSegment);
                            return targetSegment.Change($"(CASE WHEN {this.GetQuotedValue(targetSegment)}={this.GetQuotedValue(rightSegment)} THEN 0 WHEN {this.GetQuotedValue(targetSegment)}>{this.GetQuotedValue(rightSegment)} THEN 1 ELSE -1 END)", false, true);
                        });
                        result = true;
                    }
                    break;
                case "ToString":
                    if (!methodInfo.IsStatic && parameterInfos.Length == 0)
                    {
                        methodCallSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(target);
                            if (targetSegment.IsConstantValue)
                                return targetSegment.Change( targetSegment.ToString() );
                            return targetSegment.Change(this.CastTo(typeof(string), this.GetQuotedValue(targetSegment)), false, true);
                        });
                        result = true;
                    }
                    break;
                case "Parse":
                case "TryParse":
                    if (!methodInfo.IsStatic && parameterInfos.Length == 1)
                    {
                        methodCallSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                        {
                            args[0] = visitor.VisitAndDeferred(args[0]);
                            if (args[0].IsConstantValue)
                                return args[0].Change(this.GetQuotedValue(methodInfo.DeclaringType, args[0]));
                            return args[0].Change(this.CastTo(methodInfo.DeclaringType, this.GetQuotedValue(args[0])), false, true);
                        });
                        result = true;
                    }
                    break;
                case "get_Item":
                    if (!methodInfo.IsStatic && parameterInfos.Length > 0)
                    {
                        methodCallSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(target);
                            var isConstantValue = targetSegment.IsConstantValue;
                            for (int i = 0; i < args.Length; i++)
                            {
                                args[i] = visitor.VisitAndDeferred(args[i]);
                                isConstantValue = isConstantValue && args[i].IsConstantValue;
                                targetSegment.Merge(args[i]);
                            }
                            if (isConstantValue)
                                return targetSegment.Change(methodInfo.Invoke(targetSegment.Value, args.Select(f => f.Value).ToArray()));

                            throw new NotSupportedException($"不支持的方法调用,{methodInfo.DeclaringType.FullName}.{methodInfo.Name}");
                        });
                        result = true;
                    }
                    break;
            }
            return result;
        }
        return true;
    }
}
