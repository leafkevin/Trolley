using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Trolley;

public static class Extensions
{
    private static readonly Type[] valueTypes = [typeof(byte),typeof(sbyte),typeof(short),typeof(ushort),
        typeof(int),typeof(uint),typeof(long),typeof(ulong),typeof(float),typeof(double),typeof(decimal),
        typeof(bool),typeof(string),typeof(char),typeof(Guid),typeof(DateTime),typeof(DateTimeOffset),
        typeof(TimeSpan),
#if NET6_0_OR_GREATER
        typeof(DateOnly),typeof(TimeOnly),
#endif
        typeof(BitArray),typeof(DBNull)];

    private static readonly ConcurrentDictionary<int, Func<ITheaDataReader, object>> valueTupleReaderDeserializerCache = new();
    private static readonly ConcurrentDictionary<int, Func<ITheaDataReader, object>> typeReaderDeserializerCache = new();
    private static readonly ConcurrentDictionary<int, Func<ITheaDataReader, object>> queryReaderDeserializerCache = new();


    public static OrmDbFactoryBuilder Configure<TModelConfiguration>(this OrmDbFactoryBuilder builder, OrmProviderType ormProviderType) where TModelConfiguration : class, IModelConfiguration, new()
        => builder.Configure(ormProviderType, new TModelConfiguration());
    public static OrmDbFactoryBuilder Configure<TModelConfiguration>(this OrmDbFactoryBuilder builder, string dbKey) where TModelConfiguration : class, IModelConfiguration, new()
        => builder.Configure(dbKey, new TModelConfiguration());
    public static OrmDbFactoryBuilder UseTableSharding<TTableShardingConfiguration>(this OrmDbFactoryBuilder builder, OrmProviderType ormProviderType) where TTableShardingConfiguration : class, ITableShardingConfiguration, new()
        => builder.UseTableSharding(ormProviderType, new TTableShardingConfiguration());
    public static OrmDbFactoryBuilder UseTableSharding<TTableShardingConfiguration>(this OrmDbFactoryBuilder builder, string dbKey) where TTableShardingConfiguration : class, ITableShardingConfiguration, new()
        => builder.UseTableSharding(dbKey, new TTableShardingConfiguration());
    public static OrmDbFactoryBuilder UseFieldMapHandler<TFieldMapHandler>(this OrmDbFactoryBuilder builder) where TFieldMapHandler : class, IFieldMapHandler, new()
        => builder.UseFieldMapHandler(new TFieldMapHandler());
    public static void Configure(this IOrmDbFactory dbFactory, OrmProviderType ormProviderType, IModelConfiguration configuration)
    {
        if (!dbFactory.TryGetMapProvider(ormProviderType, out var mapProvider))
            dbFactory.AddMapProvider(ormProviderType, mapProvider = new EntityMapProvider(dbFactory.Options.FieldMapHandler));
        configuration.OnModelCreating(new ModelBuilder(mapProvider));
    }
    public static void Configure<TModelConfiguration>(this IOrmDbFactory dbFactory, OrmProviderType ormProviderType) where TModelConfiguration : class, IModelConfiguration, new()
       => dbFactory.Configure(ormProviderType, new TModelConfiguration());
    public static void Configure(this IOrmDbFactory dbFactory, string dbKey, IModelConfiguration configuration)
    {
        if (!dbFactory.TryGetMapProvider(dbKey, out var mapProvider))
            dbFactory.AddMapProvider(dbKey, mapProvider = new EntityMapProvider(dbFactory.Options.FieldMapHandler));
        configuration.OnModelCreating(new ModelBuilder(mapProvider));
    }
    public static void Configure<TModelConfiguration>(this IOrmDbFactory dbFactory, string dbKey) where TModelConfiguration : class, IModelConfiguration, new()
       => dbFactory.Configure(dbKey, new TModelConfiguration());
    public static string GetQuotedValue(this IOrmProvider ormProvider, object value)
        => ormProvider.GetQuotedValue(value.GetType(), value);
    public static EntityMap GetEntityMap(this IEntityMapProvider mapProvider, Type entityType)
    {
        if (!mapProvider.TryGetEntityMap(entityType, out var mapper))
            throw new Exception($"实体类型{entityType.FullName}没有配置映射，请在IModelConfiguration.OnModelCreating方法中配置映射");
        return mapper;
    }
    public static EntityMap GetEntityMap(this IEntityMapProvider mapProvider, Type entityType, Type mapToType)
    {
        if (!mapProvider.TryGetEntityMap(mapToType, out var mapper))
            throw new Exception($"实体类型{mapToType.FullName}没有配置映射，请在IModelConfiguration.OnModelCreating方法中配置映射");
        var entityMapper = mapper.CreateDefaultMap(entityType);
        mapProvider.AddEntityMap(entityType, entityMapper);
        return entityMapper;
    }
    internal static bool IsNullableType(this Type type, out Type underlyingType)
    {
        if (type.IsValueType)
        {
            underlyingType = Nullable.GetUnderlyingType(type);
            if (underlyingType == null)
            {
                underlyingType = type;
                return false;
            }
            return true;
        }
        underlyingType = type;
        return false;
    }
    internal static Type ToUnderlyingType(this Type type) => Nullable.GetUnderlyingType(type) ?? type;
    internal static bool IsEnumType(this Type type, out Type underlyingType, out Type enumUnderlyingType)
    {
        type.IsNullableType(out underlyingType);
        if (underlyingType.IsEnum)
        {
            enumUnderlyingType = underlyingType.GetEnumUnderlyingType();
            return true;
        }
        enumUnderlyingType = null;
        return false;
    }
    public static bool IsEnumType(this Type enumType, out Type enumUnderlyingType)
    {
        if (enumType.IsEnum)
        {
            enumUnderlyingType = enumType.GetEnumUnderlyingType();
            return true;
        }
        enumUnderlyingType = null;
        return false;
    }
    public static Type GetMemberType(this MemberInfo member)
    {
        switch (member.MemberType)
        {
            case MemberTypes.Property:
                var propertyInfo = member as PropertyInfo;
                return propertyInfo.PropertyType;
            case MemberTypes.Field:
                var fieldInfo = member as FieldInfo;
                return fieldInfo.FieldType;
        }
        throw new NotSupportedException("成员member，不是属性也不是字段");
    }
    public static bool CanWrite(this MemberInfo member)
    {
        switch (member.MemberType)
        {
            case MemberTypes.Property:
                var propertyInfo = member as PropertyInfo;
                return propertyInfo.CanWrite;
            case MemberTypes.Field:
                return true;
        }
        return false;
    }
    public static bool IsParameter(this Expression expr, out string parameterName)
    {
        var visitor = new IsParameterVisitor();
        visitor.Visit(expr);
        if (visitor.IsParameter)
        {
            parameterName = visitor.LastParameterName;
            return visitor.IsParameter;
        }
        parameterName = null;
        return false;
    }
    public static bool GetParameters(this Expression expr, out List<ParameterExpression> parameters)
    {
        var visitor = new IsParameterVisitor();
        visitor.Visit(expr);
        if (visitor.IsParameter)
        {
            parameters = visitor.Parameters;
            return visitor.IsParameter;
        }
        parameters = null;
        return false;
    }
    public static bool GetParameterNames(this Expression expr, out List<string> parameterNames)
    {
        var visitor = new IsParameterVisitor();
        visitor.Visit(expr);
        if (visitor.IsParameter)
        {
            parameterNames = visitor.Parameters.Select(f => f.Name).ToList();
            return visitor.IsParameter;
        }
        parameterNames = null;
        return false;
    }
    public static string NextReplace(this string content, string oldValue, string newValue)
    {
        if (!content.Contains(oldValue))
            return content;
        return content.Replace(oldValue, newValue);
    }
    /// <summary>
    /// 只要当前对象是存在多个成员(字段或是属性)的结构或是类对象，都属于属于实体类型
    /// </summary>
    /// <param name="type"></param>
    /// <param name="underlyingType"></param>
    /// <returns></returns>
    public static bool IsEntityType(this Type type, out Type underlyingType)
    {
        underlyingType = type;
        if (valueTypes.Contains(type) || type.FullName == "System.Data.Linq.Binary")
            return false;
        underlyingType = type.ToUnderlyingType();
        if (valueTypes.Contains(underlyingType) || underlyingType.FullName == "System.Data.Linq.Binary" || underlyingType.IsEnum)
            return false;
        if (type.IsArray)
        {
            var elementType = type.GetElementType();
            return elementType!.IsEntityType(out underlyingType);
        }
        if (type.IsGenericType)
        {
            if (type.FullName.StartsWith("System.ValueTuple`") && type.GenericTypeArguments.Length == 1)
                return false;
            if (typeof(IEnumerable).IsAssignableFrom(type))
            {
                if (typeof(IDictionary).IsAssignableFrom(type))
                    return true;
                foreach (var elementType in type.GenericTypeArguments)
                {
                    if (elementType.IsEntityType(out underlyingType))
                        return true;
                }
                return false;
            }
        }
        return true;
    }
    /// <summary>
    /// 返回的是单个基础值类型数据，如：int,DateTime等基础类型的数据
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="reader"></param>
    /// <param name="dbContext"></param>
    /// <returns></returns>
    public static TValue ToValue<TValue>(this ITheaDataReader reader, DbContext dbContext)
    {
        var targetType = typeof(TValue);
        var fieldType = reader.GetFieldType(0);
        if (fieldType == targetType)
            return (TValue)reader.GetValue(0);

        var valueGetter = dbContext.OrmProvider.GetReaderValueGetter(targetType, fieldType, dbContext.Options);
        return (TValue)valueGetter.Invoke(reader.GetValue(0));
    }
    public static Func<ITheaDataReader, object> GetReaderDeserializer(this ITheaDataReader reader, Type entityType, DbContext dbContext)
    {
        if (reader.FieldCount == 1 && !entityType.IsEntityType(out _))
        {
            var fieldType = reader.GetFieldType(0);
            if (fieldType == entityType)
                return reader => reader.GetValue(0);
            else
            {
                var valueGetter = dbContext.OrmProvider.GetReaderValueGetter(entityType, fieldType, dbContext.Options);
                return reader => valueGetter.Invoke(reader.GetValue(0));
            }
        }
        var ormProviderType = dbContext.OrmProvider.OrmProviderType;
        var cacheKey = GetTypeReaderKey(entityType, ormProviderType, reader);
        if (entityType.FullName.StartsWith("System.ValueTuple`"))
        {
            if (!valueTupleReaderDeserializerCache.TryGetValue(cacheKey, out var deserializer))
                valueTupleReaderDeserializerCache.TryAdd(cacheKey, deserializer = RepositoryHelper.CreateReaderValueTupleDeserializer(entityType, dbContext, reader));
            return deserializer;
        }
        //else if (entityType == typeof(object))
        //{
        //    return reader =>
        //    {
        //        var row = new List<object>();
        //        for (var i = 0; i < reader.FieldCount; i++)
        //        {
        //            var dbValue = reader.GetValue(i);
        //            row.Add(dbValue is DBNull ? null : dbValue);
        //        }
        //        return (TEntity)(object)row;
        //    };
        //}
        else if (typeof(IDictionary<string, object>).IsAssignableFrom(entityType))
        {
            return reader =>
            {
                var row = new Dictionary<string, object>();
                for (var i = 0; i < reader.FieldCount; i++)
                {
                    var dbValue = reader.GetValue(i);
                    row[reader.GetName(i).Trim()] = dbValue is DBNull ? null : dbValue;
                }
                return row;
            };
        }
        else
        {
            if (!typeReaderDeserializerCache.TryGetValue(cacheKey, out var deserializer))
                typeReaderDeserializerCache.TryAdd(cacheKey, deserializer = RepositoryHelper.CreateReaderEntityDeserializer(entityType, dbContext, reader));
            return deserializer;
        }
    }
    public static Func<ITheaDataReader, object> GetReaderDeserializer(this ITheaDataReader reader, Type entityType, DbContext dbContext, List<SqlFieldSegment> readerFields)
    {
        if (readerFields == null)
            return GetReaderDeserializer(reader, entityType, dbContext);

        if (reader.FieldCount == 1 && !entityType.IsEntityType(out _))
        {
            var fieldType = reader.GetFieldType(0);
            if (fieldType != entityType)
            {
                var typeHandler = readerFields[0].TypeHandler;
                if (typeHandler != null)
                    return reader => typeHandler.Parse(dbContext.OrmProvider, readerFields[0].SegmentType, reader.GetValue(0));
                else
                {
                    var valueGetter = dbContext.OrmProvider.GetReaderValueGetter(entityType, fieldType, dbContext.Options);
                    return reader => valueGetter.Invoke(reader.GetValue(0));
                }
            }
            return reader => reader.GetValue(0);
        }
        var ormProviderType = dbContext.OrmProvider.OrmProviderType;
        var cacheKey = GetTypeReaderKey(entityType, ormProviderType, reader, readerFields);
        if (entityType.FullName.StartsWith("System.ValueTuple`"))
        {
            if (!valueTupleReaderDeserializerCache.TryGetValue(cacheKey, out var deserializer))
                valueTupleReaderDeserializerCache.TryAdd(cacheKey, deserializer = RepositoryHelper.CreateReaderValueTupleDeserializer(entityType, dbContext, reader));
            return deserializer;
        }
        //else if (entityType == typeof(object))
        //{
        //    var valueGetters = new List<Func<ITheaDataReader, object>>();
        //    for (var i = 0; i < reader.FieldCount; i++)
        //    {
        //        var fieldType = reader.GetFieldType(i);
        //        var readerField = readerFields[i];
        //        if (fieldType != entityType)
        //        {
        //            if (readerField.TypeHandler != null)
        //            {
        //                valueGetters.Add(reader =>
        //                {
        //                    var dbValue = reader.GetValue(i);
        //                    dbValue = readerField.TypeHandler.Parse(dbContext.OrmProvider, readerField.SegmentType, dbValue);
        //                    return dbValue is DBNull ? null : dbValue;
        //                });
        //            }
        //            else
        //            {
        //                var valueGetter = dbContext.OrmProvider.GetReaderValueGetter(entityType, readerField.SegmentType, dbContext.Options);
        //                valueGetters.Add(reader =>
        //                {
        //                    var dbValue = valueGetter.Invoke(reader.GetValue(i));
        //                    return dbValue is DBNull ? null : dbValue;
        //                });
        //            }
        //        }
        //        else valueGetters.Add(reader =>
        //        {
        //            var dbValue = reader.GetValue(i);
        //            return dbValue is DBNull ? null : dbValue;
        //        });
        //    }
        //    return reader =>
        //    {
        //        var row = new List<object>();
        //        valueGetters.ForEach(f => row.Add(f.Invoke(reader)));
        //        return (TEntity)(object)row;
        //    };
        //}
        else if (typeof(IDictionary<string, object>).IsAssignableFrom(entityType))
        {
            return reader =>
            {
                var row = new Dictionary<string, object>();
                for (var i = 0; i < reader.FieldCount; i++)
                {
                    var dbValue = reader.GetValue(i);
                    row[reader.GetName(i)] = dbValue is DBNull ? null : dbValue;
                }
                return row;
            };
        }
        else
        {
            //TEntity类型与Target类型，不一定一致，可能是dynamic或是object类型，内部还是它真正的Target类型
            if (!queryReaderDeserializerCache.TryGetValue(cacheKey, out var deserializer))
                queryReaderDeserializerCache.TryAdd(cacheKey, deserializer = RepositoryHelper.CreateReaderEntityDeserializer(entityType, dbContext, reader, readerFields));
            return deserializer;
        }
    }
    /// <summary>
    /// 用在方法调用中，判断!=,NOT IN,NOT LIKE三种情况
    /// </summary>
    /// <param name="deferExprs"></param>
    /// <returns></returns>
    public static bool IsDeferredNot(this Stack<DeferredExpr> deferExprs)
    {
        int notIndex = 0;
        if (deferExprs != null && deferExprs.Count > 0)
        {
            while (deferExprs.Count > 0)
            {
                var deferredExpr = deferExprs.Pop();
                switch (deferredExpr.OperationType)
                {
                    case OperationType.Equal:
                        break;
                    case OperationType.Not:
                        notIndex++;
                        break;
                }
            }
            return notIndex % 2 > 0;
        }
        return false;
    }
    public static void CopyTo(this IQuery subQuery, SqlVisitor visitor)
    {
        if (subQuery == null || visitor.Equals(subQuery.Visitor)) return;
        if (!visitor.RefQueries.Contains(subQuery))
            visitor.RefQueries.Add(subQuery);
        if (visitor.DbParameters.Equals(subQuery.Visitor.DbParameters))
            return;
        if (subQuery.Visitor.DbParameters?.Count > 0)
            subQuery.Visitor.DbParameters.CopyTo(visitor.DbParameters);
    }
    public static T ToFieldValue<T>(this IDataReader reader, int index)
    {
        var readerValue = reader.GetValue(index);
        if (readerValue == null || readerValue is DBNull)
            return default;
        if (readerValue is IConvertible convertible)
            return (T)Convert.ChangeType(readerValue, typeof(T));

        var targetType = typeof(T);
        //兼容某些分布式数据库，byte[]类型转换为string类型
        if (readerValue is byte[] bytes && targetType == typeof(string))
            return (T)(object)UTF8Encoding.UTF8.GetString(bytes);
        var fieldType = readerValue.GetType();
        throw new NotSupportedException($"不支持的类型转换，{fieldType.FullName}->{targetType.FullName}");
    }
#if !NETCOREAPP2_0_OR_GREATER || !NETSTANDARD2_1_OR_GREATER
    public static bool TryPop<TElement>(this Stack<TElement> stack, out TElement element)
    {
        if (stack.Count > 0)
        {
            element = stack.Pop();
            return true;
        }
        element = default;
        return false;
    }
    public static bool TryPeek<TElement>(this Stack<TElement> stack, out TElement element)
    {
        if (stack.Count > 0)
        {
            element = stack.Peek();
            return true;
        }
        element = default;
        return false;
    }
#endif
    public static (bool, object) ContainsLowerKey(this IDictionary<string, object> dict, string lowerKey)
    {
        bool isContainsKey = false;
        object value = null;
        foreach (var dictKey in dict.Keys)
        {
            if (dictKey.ToLower() == lowerKey)
            {
                value = dict[dictKey];
                isContainsKey = true;
                break;
            }
        }
        return (isContainsKey, value);
    }
    public static (bool, string) ContainsLower(this List<string> keys, string lowerKey)
    {
        bool isContainsKey = false;
        string value = null;
        foreach (var key in keys)
        {
            if (key.ToLower() == lowerKey)
            {
                value = key;
                isContainsKey = true;
                break;
            }
        }
        return (isContainsKey, value);
    }
    internal static void CopyTo(this IDataParameterCollection dbParameters, IDataParameterCollection other)
    {
        if (dbParameters == null || dbParameters.Count == 0)
            return;
        if (dbParameters.Equals(other)) return;
        foreach (var dbParameter in dbParameters)
        {
            other.Add(dbParameter);
        }
    }
    internal static string ToCamel(this string value)
    {
        if (string.IsNullOrEmpty(value)) return value;
        if (value.Length > 1) return value.Substring(0, 1).ToLower() + value.Substring(1);
        else return value.ToLower();
    }
    private static int GetTypeReaderKey(Type entityType, OrmProviderType ormProviderType, ITheaDataReader reader, List<SqlFieldSegment> readerFields)
    {
#if NETCOREAPP2_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        var hashCode = new HashCode();
        hashCode.Add(ormProviderType);
        hashCode.Add(entityType);
        hashCode.Add(reader.FieldCount);
        for (int i = 0; i < reader.FieldCount; i++)
        {
            hashCode.Add(reader.GetFieldType(i));
        }
        hashCode.Add(readerFields.Count);
        foreach (var readerField in readerFields)
        {
            hashCode.Add(readerField.FieldType);
            if (readerField.FieldType == SqlFieldType.Entity && readerField.IsTargetType)
                hashCode.Add("TargetEntity");
            else if (readerField.FieldType == SqlFieldType.RawSql)
                hashCode.Add($"RawSql:{readerField.Body}");
            else if (readerField.FieldType == SqlFieldType.DeferredFields)
                hashCode.Add($"{readerField.TargetMember.Name}:{readerField.DeferredExpression.ToString()}");
            else hashCode.Add(readerField.TargetMember.Name);
        }
        return hashCode.ToHashCode();
#else
        int hashCode = 17;
        unchecked
        {
            hashCode = hashCode * 23 + ormProviderType.GetHashCode();
            hashCode = hashCode * 23 + entityType.GetHashCode();
            hashCode = hashCode * 23 + reader.FieldCount.GetHashCode();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                hashCode = hashCode * 23 + reader.GetFieldType(i).GetHashCode();
            }
            hashCode = hashCode * 23 + readerFields.Count.GetHashCode();
            foreach (var readerField in readerFields)
            {
                hashCode = hashCode * 23 + readerField.FieldType.GetHashCode();
                if (readerField.FieldType == SqlFieldType.Entity && readerField.IsTargetType)
                    hashCode = hashCode * 23 + "TargetEntity".GetHashCode();
                else if (readerField.FieldType == SqlFieldType.RawSql)
                    hashCode = hashCode * 23 + $"RawSql:{readerField.Body}".GetHashCode();
                else if (readerField.FieldType == SqlFieldType.DeferredFields)
                    hashCode = hashCode * 23 + $"{readerField.TargetMember.Name}:{readerField.DeferredExpression.ToString()}".GetHashCode();
                else hashCode = hashCode * 23 + readerField.TargetMember.Name.GetHashCode();
            }
        }
        return hashCode;
#endif
    }
    private static int GetTypeReaderKey(Type entityType, OrmProviderType ormProviderType, ITheaDataReader reader)
    {
#if NETCOREAPP2_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        var hashCode = new HashCode();
        hashCode.Add(ormProviderType);
        hashCode.Add(entityType);
        hashCode.Add(reader.FieldCount);
        for (int i = 0; i < reader.FieldCount; i++)
        {
            hashCode.Add(reader.GetFieldType(i));
            hashCode.Add(reader.GetName(i));
        }
        return hashCode.ToHashCode();
#else
        int hashCode = 17;
        unchecked
        {
            hashCode = hashCode * 23 + ormProviderType.GetHashCode();
            hashCode = hashCode * 23 + entityType.GetHashCode();
            hashCode = hashCode * 23 + reader.FieldCount.GetHashCode();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                hashCode = hashCode * 23 + reader.GetFieldType(i).GetHashCode();
                hashCode = hashCode * 23 + reader.GetName(i).GetHashCode();
            }
        }
        return hashCode;
#endif
    }
}