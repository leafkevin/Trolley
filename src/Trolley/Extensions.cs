using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Trolley;

public static class Extensions
{
    private static Type[] valueTypes = new Type[] {typeof(byte),typeof(sbyte),typeof(short),typeof(ushort),
        typeof(int),typeof(uint),typeof(long),typeof(ulong),typeof(float),typeof(double),typeof(decimal),
        typeof(bool),typeof(string),typeof(char),typeof(Guid),typeof(DateTime),typeof(DateTimeOffset),
        typeof(TimeSpan),typeof(TimeOnly),typeof(DateOnly),typeof(byte[]),typeof(byte?),typeof(sbyte?),
        typeof(short?),typeof(ushort?),typeof(int?),typeof(uint?),typeof(long?),typeof(ulong?),typeof(float?),
        typeof(double?),typeof(decimal?),typeof(bool?),typeof(char?),typeof(Guid?) ,typeof(DateTime?),
        typeof(DateTimeOffset?),typeof(TimeSpan?),typeof(TimeOnly?),typeof(DateOnly?)};
    private static readonly ConcurrentDictionary<int, Delegate> typeReaderDeserializerCache = new();
    private static readonly ConcurrentDictionary<int, Delegate> valueTupleReaderDeserializerCache = new();
    private static readonly ConcurrentDictionary<int, Delegate> queryReaderDeserializerCache = new();
    private static readonly ConcurrentDictionary<int, Delegate> readerValueConverterCache = new();

    public static IOrmProvider GetOrmProvider(this IOrmDbFactory dbFactory, string dbKey, int? tenantId = null)
    {
        var dbProvider = dbFactory.GetDatabaseProvider(dbKey);
        var database = dbProvider.GetDatabase(tenantId);
        if (dbFactory.TryGetOrmProvider(database.OrmProviderType, out var ormProvider))
            return ormProvider;
        return null;
    }
    public static IEntityMapProvider GetEntityMapProvider(this IOrmDbFactory dbFactory, string dbKey, int? tenantId = null)
    {
        var dbProvider = dbFactory.GetDatabaseProvider(dbKey);
        var database = dbProvider.GetDatabase(tenantId);
        if (dbFactory.TryGetEntityMapProvider(database.OrmProviderType, out var entityMapProvider))
            return entityMapProvider;
        return null;
    }
    public static TenantDatabaseBuilder Add<TOrmProvider>(this TheaDatabaseBuilder builder, string connectionString, bool isDefault, params int[] tenantIds) where TOrmProvider : IOrmProvider, new()
    {
        return builder.Add(new TheaDatabase
        {
            ConnectionString = connectionString,
            IsDefault = isDefault,
            OrmProviderType = typeof(TOrmProvider),
            TenantIds = tenantIds
        });
    }
    public static OrmDbFactoryBuilder AddTypeHandler<TTypeHandler>(this OrmDbFactoryBuilder builder) where TTypeHandler : class, ITypeHandler, new()
       => builder.AddTypeHandler(new TTypeHandler());
    public static OrmDbFactoryBuilder Configure<TOrmProvider>(this OrmDbFactoryBuilder builder, IModelConfiguration configuration)
    {
        builder.Configure(typeof(TOrmProvider), configuration);
        return builder;
    }
    public static OrmDbFactoryBuilder Configure<TOrmProvider, TModelConfiguration>(this OrmDbFactoryBuilder builder) where TModelConfiguration : class, IModelConfiguration, new()
    {
        builder.Configure(typeof(TOrmProvider), new TModelConfiguration());
        return builder;
    }
    public static string GetQuotedValue(this IOrmProvider ormProvider, object value)
    {
        if (value == null) return "NULL";
        return ormProvider.GetQuotedValue(value.GetType(), value);
    }
    public static EntityMap GetEntityMap(this IEntityMapProvider mapProvider, Type entityType)
    {
        if (!mapProvider.TryGetEntityMap(entityType, out var mapper))
        {
            mapper = EntityMap.CreateDefaultMap(entityType);
            mapProvider.AddEntityMap(entityType, mapper);
        }
        return mapper;
    }
    public static EntityMap GetEntityMap(this IEntityMapProvider mapProvider, Type entityType, Type mapToType)
    {
        if (!mapProvider.TryGetEntityMap(entityType, out var mapper))
        {
            var mapToMapper = mapProvider.GetEntityMap(mapToType);
            mapper = EntityMap.CreateDefaultMap(entityType, mapToMapper);
            mapProvider.AddEntityMap(entityType, mapper);
        }
        return mapper;
    }
    public static T Parse<T>(this ITypeHandler typeHandler, IOrmProvider ormProvider, object value)
       => (T)typeHandler.Parse(ormProvider, typeof(T), value);
    public static bool IsNullableType(this Type type, out Type underlyingType)
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
    public static bool IsEnumType(this Type type, out Type underlyingType, out Type enumUnderlyingType)
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
    public static bool IsParameter(this Expression expr, out string parameterName)
    {
        var visitor = new TestVisitor();
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
        var visitor = new TestVisitor();
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
        var visitor = new TestVisitor();
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
    public static bool IsEntityType(this Type type)
    {
        if (type.IsEnum || valueTypes.Contains(type)) return false;
        if (type.FullName == "System.Data.Linq.Binary")
            return false;
        if (type.IsValueType)
        {
            var underlyingType = Nullable.GetUnderlyingType(type);
            if (underlyingType != null && underlyingType.IsEnum)
                return false;
        }
        if (type.IsArray)
        {
            var elementType = type.GetElementType();
            if (valueTypes.Contains(elementType) || elementType.IsEnum) return false;
            if (elementType.IsValueType)
            {
                var underlyingType = Nullable.GetUnderlyingType(elementType);
                if (underlyingType != null && underlyingType.IsEnum)
                    return false;
            }
        }
        if (typeof(IEnumerable).IsAssignableFrom(type))
        {
            foreach (var elementType in type.GenericTypeArguments)
            {
                if (elementType.IsEnum || valueTypes.Contains(elementType))
                    return false;
                if (elementType.IsValueType)
                {
                    var underlyingType = Nullable.GetUnderlyingType(elementType);
                    if (underlyingType != null && underlyingType.IsEnum)
                        return false;
                }
            }
        }
        return true;
    }
    public static bool TryPop<T>(this Stack<T> stack, Func<T, bool> filter, out T element)
    {
        if (stack.TryPeek(out element) && filter.Invoke(element))
            return stack.TryPop(out _);
        return false;
    }
    internal static TValue To<TValue>(this IDataReader reader, int columnIndex = 0)
    {
        var targetType = typeof(TValue);
        var fieldType = reader.GetFieldType(columnIndex);
        var hashCode = HashCode.Combine(targetType, fieldType);
        if (!readerValueConverterCache.TryGetValue(hashCode, out var converter))
            readerValueConverterCache.TryAdd(hashCode, converter = CreateReaderValueConverter(targetType, fieldType));
        var deserializer = (Func<IDataReader, int, TValue>)converter;
        return deserializer.Invoke(reader, columnIndex);
    }
    internal static TEntity To<TEntity>(this IDataReader reader, string dbKey, IOrmProvider ormProvider, IEntityMapProvider mapProvider)
    {
        var entityType = typeof(TEntity);
        var ormProviderType = ormProvider.GetType();
        var isValueTuple = entityType.FullName.StartsWith("System.ValueTuple`");

        int cacheKey = 0;
        ConcurrentDictionary<int, Delegate> deserializerCache = null;
        if (isValueTuple)
        {
            cacheKey = GetValueTupleReaderKey(entityType, dbKey, ormProviderType, reader);
            deserializerCache = valueTupleReaderDeserializerCache;
        }
        else
        {
            cacheKey = GetTypeReaderKey(entityType, dbKey, ormProviderType, reader);
            deserializerCache = typeReaderDeserializerCache;
        }
        if (!deserializerCache.TryGetValue(cacheKey, out var deserializer))
        {
            deserializer = CreateReaderDeserializer(ormProvider, mapProvider, reader, entityType, isValueTuple);
            deserializerCache.TryAdd(cacheKey, deserializer);
        }
        return ((Func<IDataReader, TEntity>)deserializer).Invoke(reader);
    }
    internal static TEntity To<TEntity>(this IDataReader reader, string dbKey, IOrmProvider ormProvider, List<ReaderField> readerFields)
    {
        var entityType = typeof(TEntity);
        var ormProviderType = ormProvider.GetType();
        var cacheKey = GetTypeReaderKey(entityType, dbKey, ormProviderType, reader);
        if (!queryReaderDeserializerCache.TryGetValue(cacheKey, out var deserializer))
        {
            deserializer = CreateReaderDeserializer(ormProvider, reader, entityType, readerFields);
            queryReaderDeserializerCache.TryAdd(cacheKey, deserializer);
        }
        return ((Func<IDataReader, TEntity>)deserializer).Invoke(reader);
    }
    private static Delegate CreateReaderDeserializer(IOrmProvider ormProvider, IEntityMapProvider mapProvider, IDataReader reader, Type entityType, bool isValueTuple)
    {
        var readerExpr = Expression.Parameter(typeof(IDataReader), "reader");
        var ormProviderExpr = Expression.Constant(ormProvider);
        var entityMapper = mapProvider.GetEntityMap(entityType);
        var index = 0;
        var target = NewBuildInfo(entityType);
        var blockParameters = new List<ParameterExpression>();
        var blockBodies = new List<Expression>();

        while (index < reader.FieldCount)
        {
            var memberName = isValueTuple ? $"Item{index + 1}" : reader.GetName(index);
            //使用原始SQL才有可能SQL中的字段名与成员名不一致，或是没有加 AS成员名
            if (!entityMapper.TryGetMemberMap(memberName, out var memberMapper))
                throw new Exception($"SQL中字段{memberName}映射不到模型{entityType.FullName}任何栏位,或者没有添加AS子句");

            var fieldType = reader.GetFieldType(index);
            var readerValueExpr = GetReaderValue(ormProviderExpr, readerExpr, Expression.Constant(index),
                memberMapper.MemberType, fieldType, memberMapper.TypeHandler, blockParameters, blockBodies);

            if (target.IsDefault)
                target.Bindings.Add(Expression.Bind(memberMapper.Member, readerValueExpr));
            else target.Arguments.Add(readerValueExpr);
            index++;
        }
        var resultLabelExpr = Expression.Label(entityType);
        Expression returnExpr = null;
        if (target.IsDefault) returnExpr = Expression.MemberInit(Expression.New(target.Constructor), target.Bindings);
        else returnExpr = Expression.New(target.Constructor, target.Arguments);

        blockBodies.Add(Expression.Return(resultLabelExpr, returnExpr));
        blockBodies.Add(Expression.Label(resultLabelExpr, Expression.Default(entityType)));
        return Expression.Lambda(Expression.Block(blockParameters, blockBodies), readerExpr).Compile();
    }
    private static Delegate CreateReaderDeserializer(IOrmProvider ormProvider, IDataReader reader, Type entityType, List<ReaderField> readerFields)
    {
        var blockParameters = new List<ParameterExpression>();
        var blockBodies = new List<Expression>();
        var readerExpr = Expression.Parameter(typeof(IDataReader), "reader");
        var ormProviderExpr = Expression.Constant(ormProvider);

        int? parentIndex = null;
        int index = 0, readerIndex = 0;
        var root = NewBuildInfo(entityType);
        var current = root;
        var parent = root;
        var readerBuilders = new Dictionary<int, EntityBuildInfo>();
        var deferredBuilds = new Stack<EntityBuildInfo>();

        while (readerIndex < readerFields.Count)
        {
            var readerField = readerFields[readerIndex];
            if (readerField.FieldType == ReaderFieldType.Field)
            {
                var fieldType = reader.GetFieldType(index);
                ITypeHandler typeHandler = null;
                if (readerField.IsOnlyField && readerField.TableSegment != null && readerField.TableSegment.TableType == TableType.Entity)
                {
                    var entityMapper = readerField.TableSegment.Mapper;
                    if (entityMapper.TryGetMemberMap(readerField.FromMember.Name, out var memberMapper))
                        typeHandler = memberMapper.TypeHandler;
                }
                var readerValueExpr = GetReaderValue(ormProviderExpr, readerExpr, Expression.Constant(index),
                    readerField.TargetMember.GetMemberType(), fieldType, typeHandler, blockParameters, blockBodies);
                if (root.IsDefault) root.Bindings.Add(Expression.Bind(readerField.TargetMember, readerValueExpr));
                else root.Arguments.Add(readerValueExpr);
                index++;
            }
            else
            {
                //readerIndex变化，就要构建一个NewBuildInfo对象，每一个readerIndex是不同的实体对象
                //根据ParentIndex的值，找到它的父亲NewBuildInfo对象               
                if (readerField.ParentIndex != parentIndex)
                {
                    if (readerField.ParentIndex.HasValue)
                        parent = readerBuilders[readerField.ParentIndex.Value];
                    else parent = root;
                }
                if (readerIndex == 0 && !IsTarget(readerFields))
                    parent = root;
                if (readerIndex == 0 && !IsTarget(readerFields) || readerIndex > 0)
                    current = NewBuildInfo(readerField.TargetMember.GetMemberType(), readerField.TargetMember, parent);

                readerBuilders.Add(readerField.Index, current);
                int endIndex = index + readerField.ReaderFields.Count;
                var childIndex = 0;
                while (index < endIndex)
                {
                    var fieldType = reader.GetFieldType(index);
                    var fieldMember = readerField.ReaderFields[childIndex].FromMember;
                    Expression readerValueExpr = null;

                    if (readerField.FieldType == ReaderFieldType.AnonymousObject)
                    {
                        readerValueExpr = GetReaderValue(ormProviderExpr, readerExpr, Expression.Constant(index),
                            fieldMember.GetMemberType(), fieldType, null, blockParameters, blockBodies);
                    }
                    else
                    {
                        var entityMapper = readerField.TableSegment.Mapper;
                        if (!entityMapper.TryGetMemberMap(fieldMember.Name, out var memberMapper))
                            break;
                        readerValueExpr = GetReaderValue(ormProviderExpr, readerExpr, Expression.Constant(index),
                            memberMapper.MemberType, fieldType, memberMapper.TypeHandler, blockParameters, blockBodies);
                    }

                    if (current.IsDefault) current.Bindings.Add(Expression.Bind(fieldMember, readerValueExpr));
                    else current.Arguments.Add(readerValueExpr);
                    childIndex++;
                    index++;
                }

                //不是第一个实体字段，都要构建当前实体，如果没有后续实体类型子属性，就一直构建到顶层的下一层
                if (readerIndex > 0 || readerIndex == 0 && !IsTarget(readerFields))
                {
                    if (readerField.HasNextInclude)
                        deferredBuilds.Push(current);
                    else
                    {
                        do
                        {
                            //创建子对象，并赋值给父对象的属性,直到Select语句
                            Expression instanceExpr = null;
                            if (current.IsDefault)
                                instanceExpr = Expression.MemberInit(Expression.New(current.Constructor), current.Bindings);
                            else instanceExpr = Expression.New(current.Constructor, current.Arguments);
                            //赋值给父对象的属性
                            if (current.Parent.IsDefault)
                                current.Parent.Bindings.Add(Expression.Bind(current.FromMember, instanceExpr));
                            else current.Parent.Arguments.Add(instanceExpr);
                        }
                        while (deferredBuilds.TryPop(out current));
                    }
                }
                parentIndex = readerField.ParentIndex;
            }
            readerIndex++;
        }
        var resultLabelExpr = Expression.Label(entityType);
        Expression returnExpr = null;
        if (root.IsDefault)
            returnExpr = Expression.MemberInit(Expression.New(root.Constructor), root.Bindings);
        else returnExpr = Expression.New(root.Constructor, root.Arguments);

        blockBodies.Add(Expression.Return(resultLabelExpr, returnExpr));
        blockBodies.Add(Expression.Label(resultLabelExpr, Expression.Default(entityType)));
        return Expression.Lambda(Expression.Block(blockParameters, blockBodies), readerExpr).Compile();
    }
    private static Expression GetReaderValue(ParameterExpression readerExpr, Expression indexExpr, Type targetType, Type fieldType, List<ParameterExpression> blockParameters, List<Expression> blockBodies)
        => GetReaderValue(null, readerExpr, indexExpr, targetType, fieldType, null, blockParameters, blockBodies);
    private static Expression GetReaderValue(Expression ormProviderExpr, ParameterExpression readerExpr, Expression indexExpr, Type targetType, Type fieldType, ITypeHandler typeHandler, List<ParameterExpression> blockParameters, List<Expression> blockBodies)
    {
        bool isNullable = targetType.IsNullableType(out var underlyingType);
        var methodInfo = typeof(IDataRecord).GetMethod(nameof(IDataRecord.GetValue), new Type[] { typeof(int) });
        var objLocalExpr = AssignLocalParameter(typeof(object), Expression.Call(readerExpr, methodInfo, indexExpr), blockParameters, blockBodies);
        Expression typedValueExpr = null;

        if (typeHandler != null)
        {
            methodInfo = typeof(ITypeHandler).GetMethod(nameof(ITypeHandler.Parse), new Type[] { typeof(IOrmProvider), typeof(Type), typeof(object) });
            var typeHandlerExpr = Expression.Constant(typeHandler);
            var typeExpr = Expression.Constant(underlyingType);
            var objTargetExpr = Expression.Call(typeHandlerExpr, methodInfo, ormProviderExpr, typeExpr, objLocalExpr);
            blockBodies.Add(Expression.Assign(objLocalExpr, objTargetExpr));
            typedValueExpr = Expression.Convert(objLocalExpr, targetType);
            var equalsNullExpr = Expression.Equal(objLocalExpr, Expression.Constant(null));
            return Expression.Condition(equalsNullExpr, Expression.Default(targetType), typedValueExpr);
        }
        if (underlyingType.IsAssignableFrom(fieldType))
            typedValueExpr = Expression.Convert(objLocalExpr, underlyingType);
        else if (underlyingType == typeof(char))
        {
            if (fieldType == typeof(string))
            {
                var strLocalExpr = AssignLocalParameter(typeof(string), Expression.Convert(objLocalExpr, typeof(string)), blockParameters, blockBodies);
                var lengthExpr = Expression.Property(strLocalExpr, nameof(string.Length));
                var compareExpr = Expression.GreaterThan(lengthExpr, Expression.Constant(0, typeof(int)));
                methodInfo = typeof(string).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .Where(p => p.GetIndexParameters().Length > 0 && p.GetIndexParameters()[0].ParameterType == typeof(int))
                    .Select(p => p.GetGetMethod()).First();
                var getCharExpr = Expression.Call(strLocalExpr, methodInfo, Expression.Constant(0, typeof(int)));
                typedValueExpr = Expression.IfThenElse(compareExpr, getCharExpr, Expression.Default(underlyingType));
            }
            else throw new NotSupportedException($"暂时不支持的类型,FieldType:{fieldType.FullName},TargetType:{targetType.FullName}");
        }
        else if (underlyingType == typeof(Guid))
        {
            if (fieldType != typeof(string) && fieldType != typeof(byte[]))
                throw new NotSupportedException($"暂时不支持的类型,FieldType:{fieldType.FullName},TargetType:{targetType.FullName}");
            typedValueExpr = Expression.New(typeof(Guid).GetConstructor(new Type[] { fieldType }), Expression.Convert(objLocalExpr, fieldType));
        }
        else if (underlyingType == typeof(TimeSpan) || underlyingType == typeof(TimeOnly))
        {
            if (fieldType == typeof(long))
                typedValueExpr = Expression.New(underlyingType.GetConstructor(new Type[] { fieldType }), Expression.Convert(objLocalExpr, fieldType));
            else if (fieldType != underlyingType && (fieldType == typeof(TimeSpan) || fieldType == typeof(TimeOnly)))
            {
                Expression fieldValueExpr = Expression.Convert(objLocalExpr, fieldType);
                fieldValueExpr = Expression.Property(fieldValueExpr, nameof(TimeSpan.Ticks));
                typedValueExpr = Expression.New(underlyingType.GetConstructor(new Type[] { typeof(long) }), fieldValueExpr);
            }
            else throw new NotSupportedException($"暂时不支持的类型,FieldType:{fieldType.FullName},TargetType:{targetType.FullName}");
        }
        else if (targetType.FullName == "System.Data.Linq.Binary")
        {
            methodInfo = typeof(Activator).GetMethod(nameof(Activator.CreateInstance), new Type[] { typeof(Type), typeof(object[]) });
            typedValueExpr = Expression.Call(methodInfo, Expression.Constant(targetType), Expression.Constant(new object[] { objLocalExpr }));
            typedValueExpr = Expression.Convert(typedValueExpr, targetType);
        }
        else
        {
            if (underlyingType.IsEnum)
            {
                if (fieldType == typeof(string))
                {
                    typedValueExpr = Expression.Convert(objLocalExpr, typeof(string));
                    methodInfo = typeof(Enum).GetMethod(nameof(Enum.Parse), new Type[] { typeof(Type), typeof(string), typeof(bool) });
                    var toEnumExpr = Expression.Call(methodInfo, Expression.Constant(underlyingType), typedValueExpr, Expression.Constant(true));
                    typedValueExpr = Expression.Convert(toEnumExpr, underlyingType);
                }
                else if (fieldType == typeof(byte) || fieldType == typeof(sbyte) || fieldType == typeof(short)
                    || fieldType == typeof(ushort) || fieldType == typeof(int) || fieldType == typeof(uint)
                    || fieldType == typeof(long) || fieldType == typeof(ulong))
                {
                    typedValueExpr = Expression.Convert(objLocalExpr, fieldType);
                    methodInfo = typeof(Enum).GetMethod(nameof(Enum.ToObject), new Type[] { typeof(Type), fieldType });
                    var toEnumExpr = Expression.Call(methodInfo, Expression.Constant(underlyingType), typedValueExpr);
                    typedValueExpr = Expression.Convert(toEnumExpr, underlyingType);
                }
                else throw new Exception($"暂时不支持的类型,FieldType:{fieldType.FullName},TargetType:{targetType.FullName}");
            }
            else
            {
                var typeCode = Type.GetTypeCode(underlyingType);
                string toTypeMethod = null;
                switch (typeCode)
                {
                    case TypeCode.Boolean: toTypeMethod = nameof(Convert.ToBoolean); break;
                    case TypeCode.Char: toTypeMethod = nameof(Convert.ToChar); break;
                    case TypeCode.Byte: toTypeMethod = nameof(Convert.ToByte); break;
                    case TypeCode.SByte: toTypeMethod = nameof(Convert.ToSByte); break;
                    case TypeCode.Int16: toTypeMethod = nameof(Convert.ToInt16); break;
                    case TypeCode.UInt16: toTypeMethod = nameof(Convert.ToUInt16); break;
                    case TypeCode.Int32: toTypeMethod = nameof(Convert.ToInt32); break;
                    case TypeCode.UInt32: toTypeMethod = nameof(Convert.ToUInt32); break;
                    case TypeCode.Int64: toTypeMethod = nameof(Convert.ToInt64); break;
                    case TypeCode.UInt64: toTypeMethod = nameof(Convert.ToUInt64); break;
                    case TypeCode.Single: toTypeMethod = nameof(Convert.ToSingle); break;
                    case TypeCode.Double: toTypeMethod = nameof(Convert.ToDouble); break;
                    case TypeCode.Decimal: toTypeMethod = nameof(Convert.ToDecimal); break;
                    case TypeCode.DateTime: toTypeMethod = nameof(Convert.ToDateTime); break;
                    case TypeCode.String: toTypeMethod = nameof(Convert.ToString); break;
                }
                if (!string.IsNullOrEmpty(toTypeMethod))
                {
                    methodInfo = typeof(Convert).GetMethod(toTypeMethod, new Type[] { typeof(object), typeof(IFormatProvider) });
                    typedValueExpr = Expression.Call(methodInfo, objLocalExpr, Expression.Constant(CultureInfo.CurrentCulture));
                }
                else typedValueExpr = Expression.Convert(Expression.Convert(objLocalExpr, fieldType), underlyingType);
            }
        }
        if (underlyingType.IsValueType && isNullable)
            typedValueExpr = Expression.Convert(typedValueExpr, targetType);

        var isNullExpr = Expression.TypeIs(objLocalExpr, typeof(DBNull));
        return Expression.Condition(isNullExpr, Expression.Default(targetType), typedValueExpr);
    }
    private static EntityBuildInfo NewBuildInfo(Type targetType, MemberInfo fromMember = null, EntityBuildInfo parent = null)
    {
        bool isDefaultCtor = false;
        List<MemberBinding> bindings = null;
        List<Expression> ctorArguments = null;

        var ctor = targetType.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
        if (ctor != null)
        {
            bindings = new List<MemberBinding>();
            isDefaultCtor = true;
        }
        else
        {
            ctor = targetType.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).OrderBy(f => f.IsPublic ? 0 : (f.IsPrivate ? 2 : 1)).First();
            ctorArguments = new List<Expression>();
        }
        return new EntityBuildInfo
        {
            IsDefault = isDefaultCtor,
            Constructor = ctor,
            Bindings = bindings,
            Arguments = ctorArguments,
            FromMember = fromMember,
            Parent = parent
        };
    }
    private static Delegate CreateReaderValueConverter(Type targetType, Type fieldType)
    {
        var blockParameters = new List<ParameterExpression>();
        var blockBodies = new List<Expression>();
        var readerExpr = Expression.Parameter(typeof(IDataReader), "reader");
        var indexExpr = Expression.Parameter(typeof(int), "index");

        var resultLabelExpr = Expression.Label(targetType);
        var readerValueExpr = GetReaderValue(readerExpr, indexExpr, targetType, fieldType, blockParameters, blockBodies);
        blockBodies.Add(Expression.Return(resultLabelExpr, readerValueExpr));
        blockBodies.Add(Expression.Label(resultLabelExpr, Expression.Default(targetType)));
        return Expression.Lambda(Expression.Block(blockParameters, blockBodies), readerExpr, indexExpr).Compile();
    }
    private static ParameterExpression AssignLocalParameter(Type type, Expression valueExpr, List<ParameterExpression> blockParameters, List<Expression> blockBodies)
    {
        var objLocalExpr = Expression.Variable(type, $"local{blockParameters.Count}");
        blockParameters.Add(objLocalExpr);
        blockBodies.Add(Expression.Assign(objLocalExpr, valueExpr));
        return objLocalExpr;
    }
    private static int GetTypeReaderKey(Type entityType, string dbKey, Type ormProviderType, IDataReader reader)
    {
        var hashCode = new HashCode();
        hashCode.Add(dbKey);
        hashCode.Add(ormProviderType);
        hashCode.Add(entityType);
        hashCode.Add(reader.FieldCount);
        for (int i = 0; i < reader.FieldCount; i++)
        {
            hashCode.Add(reader.GetName(i));
        }
        return hashCode.ToHashCode();
    }
    private static int GetValueTupleReaderKey(Type entityType, string dbKey, Type ormProviderType, IDataReader reader)
    {
        var hashCode = new HashCode();
        hashCode.Add(dbKey);
        hashCode.Add(ormProviderType);
        hashCode.Add(entityType);
        hashCode.Add(reader.FieldCount);
        for (int i = 0; i < reader.FieldCount; i++)
        {
            hashCode.Add(reader.GetName(i));
            hashCode.Add(reader.GetFieldType(i));
        }
        return hashCode.ToHashCode();
    }
    private static bool IsTarget(List<ReaderField> readerFields)
    {
        if (readerFields.Count == 1)
            return true;
        if (readerFields.Exists(f => f.FieldType == ReaderFieldType.Field
             || (f.Index > 0 && f.TableSegment.FromTable == null)))
            return false;
        return true;
    }
    class EntityBuildInfo
    {
        public bool IsDefault { get; set; }
        public ConstructorInfo Constructor { get; set; }
        public List<MemberBinding> Bindings { get; set; }
        public List<Expression> Arguments { get; set; }
        public MemberInfo FromMember { get; set; }
        public EntityBuildInfo Parent { get; set; }
    }
}
