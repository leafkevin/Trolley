using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Trolley;

public partial class SqlServerProvider : BaseOrmProvider
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

    public override DatabaseType DatabaseType => DatabaseType.SqlServer;
    public override string SelectIdentitySql => ";SELECT SCOPE_IDENTITY()";
    static SqlServerProvider()
    {
        var connectionType = Type.GetType("Microsoft.Data.SqlClient.SqlConnection, Microsoft.Data.SqlClient, Culture=neutral, PublicKeyToken=23ec7fc2d6eaa4a5");
        createNativeConnectonDelegate = BaseOrmProvider.CreateConnectionDelegate(connectionType);
        var dbTypeType = Type.GetType("System.Data.SqlDbType, System.Data.Common, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
        var dbParameterType = Type.GetType("Microsoft.Data.SqlClient.SqlParameter, Microsoft.Data.SqlClient, Culture=neutral, PublicKeyToken=23ec7fc2d6eaa4a5");
        var valuePropertyInfo = dbParameterType.GetProperty("Value");
        createDefaultNativeParameterDelegate = BaseOrmProvider.CreateDefaultParameterDelegate(dbParameterType);
        createNativeParameterDelegate = BaseOrmProvider.CreateParameterDelegate(dbTypeType, dbParameterType, valuePropertyInfo);

        defaultMapTypes[Enum.Parse(dbTypeType, "Bit")] = typeof(bool);
        defaultMapTypes[Enum.Parse(dbTypeType, "TinyInt")] = typeof(byte);
        defaultMapTypes[Enum.Parse(dbTypeType, "SmallInt")] = typeof(short);
        defaultMapTypes[Enum.Parse(dbTypeType, "Int")] = typeof(int);
        defaultMapTypes[Enum.Parse(dbTypeType, "BigInt")] = typeof(long);
        defaultMapTypes[Enum.Parse(dbTypeType, "Real")] = typeof(float);
        defaultMapTypes[Enum.Parse(dbTypeType, "Float")] = typeof(double);
        defaultMapTypes[Enum.Parse(dbTypeType, "Decimal")] = typeof(decimal);
        defaultMapTypes[Enum.Parse(dbTypeType, "Money")] = typeof(decimal);
        defaultMapTypes[Enum.Parse(dbTypeType, "SmallMoney")] = typeof(decimal);
        defaultMapTypes[Enum.Parse(dbTypeType, "Char")] = typeof(string);
        defaultMapTypes[Enum.Parse(dbTypeType, "NChar")] = typeof(string);
        defaultMapTypes[Enum.Parse(dbTypeType, "VarChar")] = typeof(string);
        defaultMapTypes[Enum.Parse(dbTypeType, "NVarChar")] = typeof(string);
        defaultMapTypes[Enum.Parse(dbTypeType, "Text")] = typeof(string);
        defaultMapTypes[Enum.Parse(dbTypeType, "NText")] = typeof(string);
        defaultMapTypes[Enum.Parse(dbTypeType, "Date")] = typeof(DateTime);
        defaultMapTypes[Enum.Parse(dbTypeType, "SmallDateTime")] = typeof(DateTime);
        defaultMapTypes[Enum.Parse(dbTypeType, "DateTime")] = typeof(DateTime);
        defaultMapTypes[Enum.Parse(dbTypeType, "Timestamp")] = typeof(DateTime);
        defaultMapTypes[Enum.Parse(dbTypeType, "DateTime2")] = typeof(DateTime);
        defaultMapTypes[Enum.Parse(dbTypeType, "DateTimeOffset")] = typeof(DateTimeOffset);
        defaultMapTypes[Enum.Parse(dbTypeType, "Time")] = typeof(TimeOnly);
        defaultMapTypes[Enum.Parse(dbTypeType, "Image")] = typeof(byte[]);
        defaultMapTypes[Enum.Parse(dbTypeType, "Binary")] = typeof(byte[]);
        defaultMapTypes[Enum.Parse(dbTypeType, "VarBinary")] = typeof(byte[]);
        defaultMapTypes[Enum.Parse(dbTypeType, "UniqueIdentifier")] = typeof(Guid);

        defaultDbTypes[typeof(bool)] = Enum.Parse(dbTypeType, "Bit");
        defaultDbTypes[typeof(byte)] = Enum.Parse(dbTypeType, "TinyInt");
        defaultDbTypes[typeof(short)] = Enum.Parse(dbTypeType, "SmallInt");
        defaultDbTypes[typeof(int)] = Enum.Parse(dbTypeType, "Int");
        defaultDbTypes[typeof(long)] = Enum.Parse(dbTypeType, "BigInt");
        defaultDbTypes[typeof(float)] = Enum.Parse(dbTypeType, "Real");
        defaultDbTypes[typeof(double)] = Enum.Parse(dbTypeType, "Float");
        defaultDbTypes[typeof(decimal)] = Enum.Parse(dbTypeType, "Decimal");
        defaultDbTypes[typeof(string)] = Enum.Parse(dbTypeType, "NVarChar");
        defaultDbTypes[typeof(DateTime)] = Enum.Parse(dbTypeType, "DateTime");
        defaultDbTypes[typeof(DateTimeOffset)] = Enum.Parse(dbTypeType, "DateTimeOffset");
        defaultDbTypes[typeof(TimeOnly)] = Enum.Parse(dbTypeType, "Time");
        defaultDbTypes[typeof(byte[])] = Enum.Parse(dbTypeType, "VarBinary");
        defaultDbTypes[typeof(Guid)] = Enum.Parse(dbTypeType, "UniqueIdentifier");

        defaultDbTypes[typeof(bool?)] = Enum.Parse(dbTypeType, "Bit");
        defaultDbTypes[typeof(byte?)] = Enum.Parse(dbTypeType, "TinyInt");
        defaultDbTypes[typeof(short?)] = Enum.Parse(dbTypeType, "SmallInt");
        defaultDbTypes[typeof(int?)] = Enum.Parse(dbTypeType, "Int");
        defaultDbTypes[typeof(long?)] = Enum.Parse(dbTypeType, "BigInt");
        defaultDbTypes[typeof(float?)] = Enum.Parse(dbTypeType, "Real");
        defaultDbTypes[typeof(double?)] = Enum.Parse(dbTypeType, "Float");
        defaultDbTypes[typeof(decimal?)] = Enum.Parse(dbTypeType, "Decimal");
        defaultDbTypes[typeof(DateTime?)] = Enum.Parse(dbTypeType, "DateTime");
        defaultDbTypes[typeof(DateTimeOffset?)] = Enum.Parse(dbTypeType, "DateTimeOffset");
        defaultDbTypes[typeof(TimeOnly?)] = Enum.Parse(dbTypeType, "Time");
        defaultDbTypes[typeof(Guid?)] = Enum.Parse(dbTypeType, "UniqueIdentifier");

        nativeDbTypes[(int)Enum.Parse(dbTypeType, "BigInt")] = Enum.Parse(dbTypeType, "BigInt");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "Binary")] = Enum.Parse(dbTypeType, "Binary");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "Bit")] = Enum.Parse(dbTypeType, "Bit");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "Char")] = Enum.Parse(dbTypeType, "Char");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "DateTime")] = Enum.Parse(dbTypeType, "DateTime");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "Decimal")] = Enum.Parse(dbTypeType, "Decimal");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "Float")] = Enum.Parse(dbTypeType, "Float");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "Image")] = Enum.Parse(dbTypeType, "Image");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "Int")] = Enum.Parse(dbTypeType, "Int");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "Money")] = Enum.Parse(dbTypeType, "Money");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "NChar")] = Enum.Parse(dbTypeType, "NChar");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "NText")] = Enum.Parse(dbTypeType, "NText");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "NVarChar")] = Enum.Parse(dbTypeType, "NVarChar");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "Real")] = Enum.Parse(dbTypeType, "Real");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "UniqueIdentifier")] = Enum.Parse(dbTypeType, "UniqueIdentifier");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "SmallDateTime")] = Enum.Parse(dbTypeType, "SmallDateTime");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "SmallInt")] = Enum.Parse(dbTypeType, "SmallInt");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "SmallMoney")] = Enum.Parse(dbTypeType, "SmallMoney");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "Text")] = Enum.Parse(dbTypeType, "Text");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "Timestamp")] = Enum.Parse(dbTypeType, "Timestamp");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "TinyInt")] = Enum.Parse(dbTypeType, "TinyInt");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "VarBinary")] = Enum.Parse(dbTypeType, "VarBinary");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "VarChar")] = Enum.Parse(dbTypeType, "VarChar");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "Variant")] = Enum.Parse(dbTypeType, "Variant");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "Xml")] = Enum.Parse(dbTypeType, "Xml");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "Udt")] = Enum.Parse(dbTypeType, "Udt");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "Structured")] = Enum.Parse(dbTypeType, "Structured");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "Date")] = Enum.Parse(dbTypeType, "Date");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "Time")] = Enum.Parse(dbTypeType, "Time");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "DateTime2")] = Enum.Parse(dbTypeType, "DateTime2");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "DateTimeOffset")] = Enum.Parse(dbTypeType, "DateTimeOffset");


        castTos[typeof(string)] = "NVARCHAR(MAX)";
        castTos[typeof(byte)] = "TINYINT";
        castTos[typeof(sbyte)] = "TINYINT";
        castTos[typeof(short)] = "SMALLINT";
        castTos[typeof(ushort)] = "SMALLINT";
        castTos[typeof(int)] = "INT";
        castTos[typeof(uint)] = "INT";
        castTos[typeof(long)] = "BIGINT";
        castTos[typeof(ulong)] = "BIGINT";
        castTos[typeof(float)] = "REAL";
        castTos[typeof(double)] = "FLOAT";
        castTos[typeof(decimal)] = "DECIMAL(36,18)";
        castTos[typeof(bool)] = "BIT";
        castTos[typeof(DateTime)] = "DATETIME";
        castTos[typeof(TimeOnly)] = "TIME";
        castTos[typeof(Guid)] = "UNIQUEIDENTIFIER";

        castTos[typeof(string)] = "NVARCHAR(MAX)";
        castTos[typeof(byte?)] = "TINYINT";
        castTos[typeof(sbyte?)] = "TINYINT";
        castTos[typeof(short?)] = "SMALLINT";
        castTos[typeof(ushort?)] = "SMALLINT";
        castTos[typeof(int?)] = "INT";
        castTos[typeof(uint?)] = "INT";
        castTos[typeof(long?)] = "BIGINT";
        castTos[typeof(ulong?)] = "BIGINT";
        castTos[typeof(float?)] = "REAL";
        castTos[typeof(double?)] = "FLOAT";
        castTos[typeof(decimal?)] = "DECIMAL(36,18)";
        castTos[typeof(bool?)] = "BIT";
        castTos[typeof(DateTime?)] = "DATETIME";
        castTos[typeof(TimeOnly?)] = "TIME";
        castTos[typeof(Guid?)] = "UNIQUEIDENTIFIER";
    }
    public override IDbConnection CreateConnection(string connectionString)
        => createNativeConnectonDelegate.Invoke(connectionString);
    public override IDbDataParameter CreateParameter(string parameterName, object value)
        => createDefaultNativeParameterDelegate.Invoke(parameterName, value);
    public override IDbDataParameter CreateParameter(string parameterName, object nativeDbType, object value)
        => createNativeParameterDelegate.Invoke(parameterName, nativeDbType, value);
    public override string GetFieldName(string propertyName) => "[" + propertyName + "]";
    public override string GetTableName(string entityName) => "[" + entityName + "]";
    public override string GetPagingTemplate(int? skip, int? limit, string orderBy = null)
    {
        var builder = new StringBuilder("SELECT ");
        if (skip.HasValue && limit.HasValue)
        {
            if (string.IsNullOrEmpty(orderBy)) throw new ArgumentNullException("orderBy");
            builder.Append("/**fields**/ FROM /**tables**/ /**others**/");
            if (!String.IsNullOrEmpty(orderBy)) builder.Append($" {orderBy}");
            builder.Append($" OFFSET {skip} ROWS");
            builder.AppendFormat($" FETCH NEXT {limit} ROWS ONLY", limit);
        }
        else if (!skip.HasValue && limit.HasValue)
        {
            builder.Append($"TOP {limit} ");
            builder.Append("/**fields**/ FROM /**tables**/ /**others**/");
            if (!String.IsNullOrEmpty(orderBy)) builder.Append($" {orderBy}");
        }
        return builder.ToString();
    }
    public override object GetNativeDbType(Type type)
    {
        if (!defaultDbTypes.TryGetValue(type, out var dbType))
            throw new Exception($"类型{type.FullName}没有对应的System.Data.SqlDbType映射类型");
        return dbType;
    }
    public override object GetNativeDbType(int nativeDbType)
    {
        if (nativeDbTypes.TryGetValue(nativeDbType, out var result))
            return result;
        var dbTypeType = Type.GetType("System.Data.SqlDbType, System.Data.Common, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
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
        throw new Exception($"数值{nativeDbType}没有对应的System.Data.SqlDbType映射类型");
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
            return $"'{timeSpan.ToString("d\\ hh\\:mm\\:ss\\.fffffff")}'";
        if (expectType == typeof(TimeOnly) && value is TimeOnly timeOnly)
            return $"'{timeOnly.ToString("hh\\:mm\\:ss\\.fffffff")}'";
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
                return true;
            if (this.TryGetIEnumerableMethodCallSqlFormatter(methodCallExpr, out formatter))
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
                                return targetSegment.Change(targetSegment.ToString());
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
