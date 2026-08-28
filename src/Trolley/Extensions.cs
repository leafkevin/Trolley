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

    private static readonly ConcurrentDictionary<Type, ConcurrentDictionary<int, Func<ITheaDataReader, object>>> valueTupleReaderDeserializerCache = new();
    private static readonly ConcurrentDictionary<Type, ConcurrentDictionary<int, Func<ITheaDataReader, object>>> simpleReaderDeserializerCache = new();
    private static readonly ConcurrentDictionary<Type, ConcurrentDictionary<int, Func<ITheaDataReader, List<ReaderField>, object>>> queryReaderDeserializerCache = new();
    private static readonly ConcurrentDictionary<Type, ConcurrentDictionary<int, Func<ITheaDataReader, List<ReaderField>, object>>> deferredValueReaderDeserializerCache = new();

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
        public OrmDbFactoryBuilder UseInterceptor<TDbInterceptor>() where TDbInterceptor : class, IDbInterceptor, new()
            => builder.UseInterceptor(new TDbInterceptor());
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
        public EntityMap GetEntityMap(Type targetType, Type mapEntityType)
        {
            if (entityMapProvider.TryGetEntityMap(targetType, out var myEntityMapper))
                return myEntityMapper;
            if (!entityMapProvider.TryGetEntityMap(mapEntityType, out var entityMapper))
                throw new Exception($"实体类型{mapEntityType.FullName}没有配置映射，请在IModelConfiguration.Configure方法中配置映射");
            myEntityMapper = entityMapper.CreateDefaultMap(targetType);
            entityMapProvider.UseEntityMap(targetType, myEntityMapper);
            return myEntityMapper;
        }
    }
    extension(Type type)
    {
#if NETSTANDARD2_0
        public bool IsEnum => type.GetTypeInfo().IsEnum;
#endif
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
        //public bool IsEnumType(out Type underlyingType, out Type enumUnderlyingType)
        //{
        //    type.IsNullableType(out underlyingType);
        //    if (underlyingType.IsEnum)
        //    {
        //        enumUnderlyingType = underlyingType.GetEnumUnderlyingType();
        //        return true;
        //    }
        //    enumUnderlyingType = null;
        //    return false;
        //}
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
        public bool HasParameter()
        {
            var visitor = new HasParameterVisitor();
            visitor.Visit(expr);
            return visitor.HasParameter;
        }
        public bool HasVariable()
        {
            var visitor = new HasParameterVisitor();
            visitor.Visit(expr);
            return visitor.HasVariable;
        }
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
        /// <param name="isNullable"></param>
        /// <returns></returns>
        public TValue ToValue<TValue>(DbContext dbContext, bool isNullable)
        {
            var targetType = typeof(TValue);
            var fieldType = reader.GetFieldType(0);
            if (fieldType == targetType)
                return (TValue)reader.GetValue(0);
            var valueGetter = dbContext.OrmProvider.GetReaderValueGetter(targetType, fieldType, isNullable, dbContext.Options);
            return (TValue)valueGetter.Invoke(reader.GetValue(0));
        }
        public Func<ITheaDataReader, object> GetReaderDeserializer(Type targetType, DbContext dbContext)
        {
            var ormProviderType = dbContext.OrmProvider.OrmProviderType;
            var cacheKey = GetTypeReaderKey(targetType, ormProviderType, reader);
            if (reader.FieldCount == 1 && !targetType.IsEntityType(out _))
            {
                var fieldType = reader.GetFieldType(0);
                if (fieldType == targetType)
                    return reader => reader.GetValue(0);
                else
                {
                    var valueGetter = dbContext.OrmProvider.GetReaderValueGetter(targetType, fieldType, true, dbContext.Options);
                    return reader => valueGetter.Invoke(reader.GetValue(0));
                }
            }
            else if (targetType.FullName.StartsWith("System.ValueTuple`"))
            {
                var typedValueTupleReaderDeserializerCache = valueTupleReaderDeserializerCache.GetOrAdd(targetType, f => new());
                return typedValueTupleReaderDeserializerCache.GetOrAdd(cacheKey, f =>
                    RepositoryHelper.CreateReaderValueTupleDeserializer(targetType, dbContext, reader));
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
                var typedSimpleReaderDeserializerCache = simpleReaderDeserializerCache.GetOrAdd(targetType, f => new());
                return typedSimpleReaderDeserializerCache.GetOrAdd(cacheKey, f =>
                    RepositoryHelper.CreateReaderEntityDeserializer(targetType, dbContext, reader));
            }
        }
        public Func<ITheaDataReader, List<ReaderField>, object> GetReaderDeserializer(Type targetType, DbContext dbContext, List<ReaderField> readerFields)
        {
            if (typeof(IDictionary<string, object>).IsAssignableFrom(targetType))
            {
                return (reader, readerFields) =>
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
                var ormProviderType = dbContext.OrmProvider.OrmProviderType;
                var cacheKey = GetTypeReaderKey(targetType, ormProviderType, reader, readerFields);
                if (reader.FieldCount == 1 && !targetType.IsEntityType(out _))
                {
                    if (readerFields.Exists(f => f.IsDeferredFields))
                    {
                        var typedDeferredValueReaderDeserializerCache = deferredValueReaderDeserializerCache.GetOrAdd(targetType, f => new());
                        return typedDeferredValueReaderDeserializerCache.GetOrAdd(cacheKey, f =>
                            RepositoryHelper.CreateReaderDeferredValueDeserializer(dbContext, reader, readerFields));
                    }
                    else
                    {
                        var fieldType = reader.GetFieldType(0);
                        if (fieldType != targetType)
                        {
                            var memberMapper = readerFields[0].MemberMapper;
                            if (memberMapper != null)
                            {
                                var typeHandler = memberMapper.TypeHandler;
                                if (typeHandler != null)
                                    return (reader, readerFields) => typeHandler.Parse(readerFields[0].ReaderType, reader.GetValue(0));
                                else
                                {
                                    var valueGetter = dbContext.OrmProvider.GetReaderValueGetter(targetType, fieldType, !memberMapper.IsRequired, dbContext.Options);
                                    return (reader, readerFields) => valueGetter.Invoke(reader.GetValue(0));
                                }
                            }
                            else
                            {
                                var valueGetter = dbContext.OrmProvider.GetReaderValueGetter(targetType, fieldType, true, dbContext.Options);
                                return (reader, readerFields) => valueGetter.Invoke(reader.GetValue(0));
                            }
                        }
                        return (reader, readerFields) => reader.GetValue(0);
                    }
                }
                else
                {
                    //TEntity类型与Target类型，不一定一致，可能是dynamic或是object类型，内部还是它真正的Target类型
                    var typedQueryReaderDeserializerCache = queryReaderDeserializerCache.GetOrAdd(targetType, f => new());
                    return typedQueryReaderDeserializerCache.GetOrAdd(cacheKey, f =>
                        RepositoryHelper.CreateReaderEntityDeserializer(targetType, dbContext, reader, readerFields));
                }
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
                return (T)(object)Encoding.UTF8.GetString(bytes);
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
#if !NETCOREAPP2_0_OR_GREATER && !NETSTANDARD2_1_OR_GREATER
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
    extension<TKey, TValue>(Dictionary<TKey, TValue> dict)
    {
        public bool TryAdd(TKey key, TValue value)
        {
            if (dict.ContainsKey(key))
                return false;
            dict.Add(key, value);
            return true;
        }
    }
#endif
    public static bool HasNotOperation(this Stack<DeferredOperation> deferredOperations, out DeferredOperation lastOperation)
    {
        lastOperation = DeferredOperation.None;
        if (deferredOperations == null || deferredOperations.Count == 0)
            return false;

        int notIndex = 0;
        while (deferredOperations.Count > 0)
        {
            var operationType = deferredOperations.Pop();
            switch (operationType)
            {
                case DeferredOperation.IsNull:
                case DeferredOperation.IsTrue:
                    lastOperation = operationType;
                    break;
                case DeferredOperation.Not:
                    notIndex++;
                    break;
            }
        }
        return notIndex % 2 > 0;
    }

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
        var myMemberInfos = memberInfos.FindAll(f => string.Equals(f.Name, memberName, StringComparison.OrdinalIgnoreCase));
        if (myMemberInfos.Count > 1)
        {
            var myMemberInfo = myMemberInfos.Find(f => f.Name == memberName);
            if (myMemberInfo != null)
            {
                memberInfo = myMemberInfo;
                return true;
            }
        }
        else if (myMemberInfos.Count > 0)
        {
            memberInfo = myMemberInfos[0];
            return true;
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
            hashCode.Add(reader.GetName(i));
        }
        hashCode.Add(readerFields.Count);
        int index = 0;
        foreach (var readerField in readerFields)
        {
            hashCode.Add(readerField.FieldType);
            if (readerField.FieldType == ReaderFieldType.Entity)
                hashCode.Add(readerField.ReaderType);
            else if (readerField.FieldType == ReaderFieldType.RawSql)
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