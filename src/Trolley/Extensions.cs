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
    private static readonly HashSet<Type> valueTypes = new HashSet<Type>
    {
        typeof(byte),typeof(sbyte),typeof(short),typeof(ushort),
        typeof(int),typeof(uint),typeof(long),typeof(ulong),typeof(float),typeof(double),typeof(decimal),
        typeof(bool),typeof(string),typeof(char),typeof(Guid),typeof(DateTime),typeof(DateTimeOffset),
        typeof(TimeSpan),
#if NET6_0_OR_GREATER
        typeof(DateOnly),typeof(TimeOnly),
#endif
        typeof(BitArray),typeof(DBNull)
    };

    private static readonly ConcurrentDictionary<int, Func<ITheaDataReader, object>> valueTupleReaderDeserializerCache = new();
    private static readonly ConcurrentDictionary<int, Func<ITheaDataReader, object>> typeReaderDeserializerCache = new();
    private static readonly ConcurrentDictionary<int, Func<ITheaDataReader, object>> queryReaderDeserializerCache = new();
    private static readonly ConcurrentDictionary<int, Func<ITheaDataReader, object>> deferredValueReaderDeserializerCache = new();

    extension(OrmDbFactoryBuilder builder)
    {
        public OrmDbFactoryBuilder UseMapping<TModelConfiguration>(OrmProviderType ormProviderType) where TModelConfiguration : class, IModelMappingConfiguration, new()
            => builder.UseMapping(ormProviderType, new TModelConfiguration());
        public OrmDbFactoryBuilder UseMapping<TModelConfiguration>(string dbKey) where TModelConfiguration : class, IModelMappingConfiguration, new()
            => builder.UseMapping(dbKey, new TModelConfiguration());
        public OrmDbFactoryBuilder UseTableSharding<TTableShardingConfiguration>(OrmProviderType ormProviderType) where TTableShardingConfiguration : class, ITableShardingConfiguration, new()
            => builder.UseTableSharding(ormProviderType, new TTableShardingConfiguration());
        public OrmDbFactoryBuilder UseTableSharding<TTableShardingConfiguration>(string dbKey) where TTableShardingConfiguration : class, ITableShardingConfiguration, new()
            => builder.UseTableSharding(dbKey, new TTableShardingConfiguration());
    }
    extension(IEntityMapProvider entityMapProvider)
    {
        public bool IsCanMapTo(MemberInfo fromName, MemberInfo toName)
        {
            if (fromName == null || toName == null)
                return false;
            return entityMapProvider.IsCanMapTo(fromName.Name, toName.Name);
        }
        public bool TryMapMember(string fieldName, List<MemberMap> memberMappers, out MemberMap memberMapper)
        {
            if (string.IsNullOrEmpty(fieldName))
                throw new ArgumentNullException(nameof(fieldName));
            memberMapper = memberMappers.Find(f => entityMapProvider.IsCanMapTo(fieldName, f.MemberName));
            return memberMapper != null;
        }
        public bool TryMapMember(string fieldName, List<MemberInfo> memberInfos, out MemberInfo memberInfo)
        {
            if (string.IsNullOrEmpty(fieldName))
                throw new ArgumentNullException(nameof(fieldName));
            memberInfo = memberInfos.Find(f => entityMapProvider.IsCanMapTo(fieldName, f.Name));
            return memberInfo != null;
        }
        public EntityMap GetEntityMap(Type entityType)
        {
            if (!entityMapProvider.TryGetEntityMap(entityType, out var mapper))
                throw new Exception($"实体类型{entityType.FullName}没有配置映射，请在IModelConfiguration.OnModelCreating方法中配置映射");
            return mapper;
        }
        public EntityMap GetEntityMap(Type targetType, Type mapToType)
        {
            if (!entityMapProvider.TryGetEntityMap(mapToType, out var mapper))
                throw new Exception($"实体类型{mapToType.FullName}没有配置映射，请在IModelConfiguration.Configure方法中配置映射");
            var entityMapper = mapper.CreateDefaultMap(targetType);
            entityMapProvider.UseEntityMap(targetType, entityMapper);
            return entityMapper;
        }
    }
    extension(Type type)
    {
        public bool IsNullableType(out Type underlyingType)
        {
            if (!type.IsValueType)
            {
                underlyingType = type;
                return true;
            }
            underlyingType = Nullable.GetUnderlyingType(type);
            if (underlyingType == null)
            {
                underlyingType = type;
                return false;
            }
            return true;
        }
        public Type ToUnderlyingType() => Nullable.GetUnderlyingType(type) ?? type;
        public bool IsEnumType(out Type underlyingType, out Type enumUnderlyingType)
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
        public bool IsEnumType(out Type enumUnderlyingType)
        {
            if (type.IsEnum)
            {
                enumUnderlyingType = type.GetEnumUnderlyingType();
                return true;
            }
            enumUnderlyingType = null;
            return false;
        }
        /// <summary>
        /// 只要当前对象是存在多个成员(字段或是属性)的结构或是类对象，都属于属于实体类型
        /// </summary>
        /// <param name="underlyingType"></param>
        /// <returns></returns>
        public bool IsEntityType(out Type underlyingType)
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
        public bool TryGetMember(string memberName, out MemberInfo memberInfo)
        {
            var memberInfos = RepositoryHelper.GetMembers(type);
            return memberInfos.TryFind(memberName, out memberInfo);
        }
    }
    extension(MemberInfo member)
    {
        public Type GetMemberType()
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
        public bool CanWrite
        {
            get
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
        }
    }
    extension(IOrmProvider ormProvider)
    {
        public string GetQuotedValue(object value)
            => ormProvider.GetQuotedValue(value.GetType(), value);
    }
    extension(Expression expr)
    {
        //public bool HasParameter()
        //{
        //    var visitor = new HasParameterVisitor();
        //    visitor.Visit(expr);
        //    return visitor.HasParameter;
        //}
        public bool TryGetParameters(out List<ParameterExpression> parameters)
        {
            var visitor = new HasParameterVisitor();
            visitor.Visit(expr);
            if (visitor.HasParameter)
            {
                parameters = visitor.Parameters;
                return true;
            }
            parameters = null;
            return false;
        }
        public bool TryGetParameterNames(out List<string> parameterNames)
        {
            var visitor = new HasParameterVisitor();
            visitor.Visit(expr);
            if (visitor.HasParameter)
            {
                parameterNames = visitor.Parameters.Select(f => f.Name).ToList();
                return true;
            }
            parameterNames = null;
            return false;
        }
    }
    extension(ITheaDataReader reader)
    {
        /// <summary>
        /// 返回的是单个基础值类型数据，如：int,DateTime等基础类型的数据
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dbContext"></param>
        /// <returns></returns>
        public TValue ToValue<TValue>(DbContext dbContext)
        {
            var targetType = typeof(TValue);
            var fieldType = reader.GetFieldType(0);
            if (fieldType == targetType)
                return (TValue)reader.GetValue(0);

            var valueGetter = dbContext.OrmProvider.GetReaderValueGetter(targetType, fieldType, dbContext.Options);
            return (TValue)valueGetter.Invoke(reader.GetValue(0));
        }
        public Func<ITheaDataReader, object> GetReaderDeserializer(Type targetType, DbContext dbContext)
        {
            if (reader.FieldCount == 1 && !targetType.IsEntityType(out _))
            {
                var fieldType = reader.GetFieldType(0);
                if (fieldType == targetType)
                    return reader => reader.GetValue(0);
                else
                {
                    var valueGetter = dbContext.OrmProvider.GetReaderValueGetter(targetType, fieldType, dbContext.Options);
                    return reader => valueGetter.Invoke(reader.GetValue(0));
                }
            }
            var ormProviderType = dbContext.OrmProvider.OrmProviderType;
            var cacheKey = GetTypeReaderKey(targetType, ormProviderType, reader);
            if (targetType.FullName.StartsWith("System.ValueTuple`"))
            {
                if (!valueTupleReaderDeserializerCache.TryGetValue(cacheKey, out var deserializer))
                    valueTupleReaderDeserializerCache.TryAdd(cacheKey, deserializer = RepositoryHelper.CreateReaderValueTupleDeserializer(targetType, dbContext, reader));
                return deserializer;
            }
            else if (typeof(IDictionary<string, object>).IsAssignableFrom(targetType))
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
                    typeReaderDeserializerCache.TryAdd(cacheKey, deserializer = RepositoryHelper.CreateReaderEntityDeserializer(targetType, dbContext, reader));
                return deserializer;
            }
        }
        public Func<ITheaDataReader, object> GetReaderDeserializer(Type targetType, DbContext dbContext, List<ReaderField> readerFields)
        {
            if (readerFields == null)
                return GetReaderDeserializer(reader, targetType, dbContext);

            int cacheKey = 0;
            var ormProviderType = dbContext.OrmProvider.OrmProviderType;
            if (reader.FieldCount == 1 && !targetType.IsEntityType(out _))
            {
                if (readerFields.Exists(f => f.IsDeferredFields))
                {
                    cacheKey = GetTypeReaderKey(targetType, ormProviderType, reader, readerFields);
                    if (!deferredValueReaderDeserializerCache.TryGetValue(cacheKey, out var deserializer))
                        deferredValueReaderDeserializerCache.TryAdd(cacheKey, deserializer = RepositoryHelper.CreateReaderDeferredValueDeserializer(targetType, dbContext, reader, readerFields));
                    return deserializer;
                }

                var fieldType = reader.GetFieldType(0);
                if (fieldType != targetType)
                {
                    var typeHandler = readerFields[0].TypeHandler;
                    if (typeHandler != null)
                        return reader => typeHandler.Parse(readerFields[0].ReaderType, reader.GetValue(0));
                    else
                    {
                        var valueGetter = dbContext.OrmProvider.GetReaderValueGetter(targetType, fieldType, dbContext.Options);
                        return reader => valueGetter.Invoke(reader.GetValue(0));
                    }
                }
                return reader => reader.GetValue(0);
            }

            cacheKey = GetTypeReaderKey(targetType, ormProviderType, reader, readerFields);
            if (targetType.FullName.StartsWith("System.ValueTuple`"))
            {
                if (!valueTupleReaderDeserializerCache.TryGetValue(cacheKey, out var deserializer))
                    valueTupleReaderDeserializerCache.TryAdd(cacheKey, deserializer = RepositoryHelper.CreateReaderValueTupleDeserializer(targetType, dbContext, reader));
                return deserializer;
            }
            else if (typeof(IDictionary<string, object>).IsAssignableFrom(targetType))
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
                    queryReaderDeserializerCache.TryAdd(cacheKey, deserializer = RepositoryHelper.CreateReaderEntityDeserializer(targetType, dbContext, reader, readerFields));
                return deserializer;
            }
        }
    }
    extension(IDataReader reader)
    {
        public T ToFieldValue<T>(int index)
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
    }
    extension(string value)
    {
        public string ToCamel()
        {
            if (string.IsNullOrEmpty(value)) return value;
            if (value.Length > 1)
            {
#if NETCOREAPP2_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER
                return string.Create(value.Length, value, (span, str) =>
                {
                    str.AsSpan().CopyTo(span);
                    span[0] = char.ToLower(span[0]);
                });
#else
                return string.Concat(char.ToLower(value[0]), value.Substring(1));
#endif
            }
            else return value.ToLower();
        }
    }
    extension(IDataParameterCollection dbParameters)
    {
        public void CopyTo(IDataParameterCollection other)
        {
            if (dbParameters == null || dbParameters.Count == 0)
                return;
            if (dbParameters.Equals(other)) return;
            foreach (var dbParameter in dbParameters)
            {
                other.Add(dbParameter);
            }
        }
        public List<IDbDataParameter> ToList()
        {
            if (dbParameters == null || dbParameters.Count == 0)
                return null;
            if (dbParameters is TheaDbParameterCollection theaDbParameters)
                return theaDbParameters.ToList();
            return dbParameters.Cast<IDbDataParameter>().ToList();
            //var result = new List<IDbDataParameter>(dbParameters.Count);
            //foreach (var dbParameter in dbParameters)
            //{
            //    result.Add((IDbDataParameter)dbParameter);
            //}
            //return result;
        }
    }
#if !NETCOREAPP2_0_OR_GREATER || !NETSTANDARD2_1_OR_GREATER
    extension<TElement>(Stack<TElement> stack)
    {
        public bool TryPop(out TElement element)
        {
            if (stack.Count > 0)
            {
                element = stack.Pop();
                return true;
            }
            element = default;
            return false;
        }
        public bool TryPeek(out TElement element)
        {
            if (stack.Count > 0)
            {
                element = stack.Peek();
                return true;
            }
            element = default;
            return false;
        }
    }
#endif

    /// <summary>
    /// 用在方法调用中，判断!=,NOT IN,NOT LIKE三种情况
    /// </summary>
    /// <param name="deferExprs"></param>
    /// <returns></returns>
    //public static bool IsDeferredNot(this Stack<DeferredExpr> deferExprs)
    //{
    //    if (deferExprs != null && deferExprs.Count > 0)
    //    {
    //        int notIndex = 0;
    //        while (deferExprs.Count > 0)
    //        {
    //            var deferredExpr = deferExprs.Pop();
    //            switch (deferredExpr.OperationType)
    //            {
    //                case OperationType.Equal:
    //                    break;
    //                case OperationType.Not:
    //                    notIndex++;
    //                    break;
    //            }
    //        }
    //        return notIndex % 2 > 0;
    //    }
    //    return false;
    //}

    public static bool TryGetKeyIgnoreCase(this IDictionary<string, object> dict, string memberName, out string itemKey)
    {
        itemKey = null;
        foreach (var dictKey in dict.Keys)
        {
            if (string.Equals(memberName, dictKey, StringComparison.OrdinalIgnoreCase))
            {
                itemKey = dictKey;
                return true;
            }
        }
        return false;
    }
    public static bool TryFind(this List<MemberInfo> memberInfos, string memberName, out MemberInfo memberInfo)
    {
        foreach (var myMemberInfo in memberInfos)
        {
            if (string.Equals(myMemberInfo.Name, memberName, StringComparison.OrdinalIgnoreCase))
            {
                memberInfo = myMemberInfo;
                return true;
            }
        }
        memberInfo = null;
        return false;
    }
    private static int GetTypeReaderKey(Type entityType, OrmProviderType ormProviderType, ITheaDataReader reader, List<ReaderField> readerFields)
    {
        var hashCode = new HashCode();
        hashCode.Add(ormProviderType);
        hashCode.Add(entityType);
        hashCode.Add(reader.FieldCount);
        for (int i = 0; i < reader.FieldCount; i++)
        {
            hashCode.Add(reader.GetFieldType(i));
        }
        hashCode.Add(readerFields.Count);
        int index = 0;
        foreach (var readerField in readerFields)
        {
            hashCode.Add(readerField.FieldType);
            if (readerField.FieldType == SqlFieldType.Entity && readerField.IsTargetType)
                hashCode.Add("TargetEntity");
            else if (readerField.FieldType == SqlFieldType.RawSql)
                hashCode.Add($"RawSql:{readerField.Value}");
            else if (readerField.IsDeferredFields)
            {
                string fieldName = readerField.TargetMember?.Name ?? $"DeferredField{index++}";
                hashCode.Add($"{fieldName}:{readerField.Expression.ToString()}");
            }
            else hashCode.Add(readerField.TargetMember.Name);
        }
        return hashCode.ToHashCode();
    }
    private static int GetTypeReaderKey(Type entityType, OrmProviderType ormProviderType, ITheaDataReader reader)
    {
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
    }
}