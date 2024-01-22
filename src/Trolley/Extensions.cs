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
        typeof(TimeSpan),typeof(TimeOnly),typeof(DateOnly),typeof(DBNull)};
    private static readonly ConcurrentDictionary<int, Delegate> typeReaderDeserializerCache = new();
    private static readonly ConcurrentDictionary<int, Delegate> valueTupleReaderDeserializerCache = new();
    private static readonly ConcurrentDictionary<int, Delegate> queryReaderDeserializerCache = new();
    private static readonly ConcurrentDictionary<int, Delegate> readerValueConverterCache = new();
    private static readonly ConcurrentDictionary<int, Action<IDataParameterCollection, IOrmProvider, string, object>> addParametersCache = new();


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
    public static IOrmDbFactory Configure<TOrmProvider>(this IOrmDbFactory dbFactory, IModelConfiguration configuration) where TOrmProvider : class, IOrmProvider, new()
    {
        var ormProviderType = typeof(TOrmProvider);
        if (!dbFactory.TryGetMapProvider(ormProviderType, out var mapProvider))
        {
            if (!dbFactory.TryGetOrmProvider(ormProviderType, out _))
                dbFactory.AddOrmProvider(new TOrmProvider());
            dbFactory.AddMapProvider(ormProviderType, new EntityMapProvider { OrmProviderType = ormProviderType });
        }
        configuration.OnModelCreating(new ModelBuilder(mapProvider));
        return dbFactory;
    }
    public static IOrmDbFactory Configure<TOrmProvider, TModelConfiguration>(this IOrmDbFactory dbFactory)
        where TOrmProvider : class, IOrmProvider, new()
        where TModelConfiguration : class, IModelConfiguration, new()
    {
        dbFactory.Configure<TOrmProvider>(new TModelConfiguration());
        return dbFactory;
    }
    public static string GetQuotedValue(this IOrmProvider ormProvider, object value)
        => ormProvider.GetQuotedValue(value.GetType(), value);
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
    public static Type ToUnderlyingType(this Type type) => Nullable.GetUnderlyingType(type) ?? type;
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
    /// <param name="columnIndex"></param>
    /// <returns></returns>
    public static TValue To<TValue>(this IDataReader reader, int columnIndex = 0)
    {
        var targetType = typeof(TValue);
        var fieldType = reader.GetFieldType(columnIndex);
        var hashCode = HashCode.Combine(targetType, fieldType);
        if (!readerValueConverterCache.TryGetValue(hashCode, out var converter))
            readerValueConverterCache.TryAdd(hashCode, converter = CreateReaderValueConverter(targetType, fieldType));
        var deserializer = (Func<IDataReader, int, TValue>)converter;
        return deserializer.Invoke(reader, columnIndex);
    }
    public static TEntity To<TEntity>(this IDataReader reader, DbContext dbContext)
        => reader.To<TEntity>(dbContext.DbKey, dbContext.OrmProvider, dbContext.MapProvider);
    public static TEntity To<TEntity>(this IDataReader reader, DbContext dbContext, List<ReaderField> readerFields)
        => reader.To<TEntity>(dbContext.DbKey, dbContext.OrmProvider, readerFields);
    /// <summary>
    /// 使用SQL+参数查询,返回的是单纯的实体
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="reader"></param>
    /// <param name="dbKey"></param>
    /// <param name="ormProvider"></param>
    /// <param name="mapProvider"></param>
    /// <returns></returns>
    public static TEntity To<TEntity>(this IDataReader reader, string dbKey, IOrmProvider ormProvider, IEntityMapProvider mapProvider)
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
    /// <summary>
    /// 使用非SQL查询，返回的是可能包含Include对象，包含层次的实体对象
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="reader"></param>
    /// <param name="dbKey"></param>
    /// <param name="ormProvider"></param>
    /// <param name="readerFields"></param>
    /// <returns></returns>
    public static TEntity To<TEntity>(this IDataReader reader, string dbKey, IOrmProvider ormProvider, List<ReaderField> readerFields)
    {
        var entityType = typeof(TEntity);
        var ormProviderType = ormProvider.GetType();
        var cacheKey = GetTypeReaderKey(entityType, dbKey, ormProviderType, readerFields);
        if (!queryReaderDeserializerCache.TryGetValue(cacheKey, out var deserializer))
        {
            deserializer = CreateReaderDeserializer(ormProvider, reader, entityType, readerFields);
            queryReaderDeserializerCache.TryAdd(cacheKey, deserializer);
        }
        return ((Func<IDataReader, TEntity>)deserializer).Invoke(reader);
    }
    public static void AddDbParameter(this IOrmProvider ormProvider, string dbKey, IDataParameterCollection dbParameters, MemberMap memberMapper, string parameterName, object fieldValue)
    {
        var fieldVallueType = fieldValue.GetType();
        var cacheKey = HashCode.Combine(dbKey, ormProvider, memberMapper.Parent.EntityType, memberMapper, fieldVallueType);
        var AddParametersDelegate = addParametersCache.GetOrAdd(cacheKey, f =>
        {
            var dbParametersExpr = Expression.Parameter(typeof(IDataParameterCollection), "dbParameters");
            var ormProviderExpr = Expression.Parameter(typeof(IOrmProvider), "ormProvider");
            var parameterNameExpr = Expression.Parameter(typeof(string), "parameterName");
            var fieldValueExpr = Expression.Parameter(typeof(object), "fieldValue");

            var typedFieldValueExpr = Expression.Variable(fieldVallueType, "typedFieldValue");
            var blockParameters = new List<ParameterExpression>();
            var blockBodies = new List<Expression>();

            RepositoryHelper.AddValueParameter(dbParametersExpr, ormProviderExpr, parameterNameExpr, fieldValueExpr, memberMapper, blockParameters, blockBodies);
            if (blockParameters.Count > 0)
            {
                return Expression.Lambda<Action<IDataParameterCollection, IOrmProvider, string, object>>(
                    Expression.Block(blockParameters, blockBodies), dbParametersExpr, ormProviderExpr, parameterNameExpr, fieldValueExpr).Compile();
            }
            return Expression.Lambda<Action<IDataParameterCollection, IOrmProvider, string, object>>(
                Expression.Block(blockBodies), dbParametersExpr, ormProviderExpr, parameterNameExpr, fieldValueExpr).Compile();
        });
        AddParametersDelegate.Invoke(dbParameters, ormProvider, parameterName, fieldValue);
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
            while (deferExprs.TryPop(out var deferredExpr))
            {
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
        if (subQuery == null) return;
        if (subQuery is ICteQuery cteQuery)
        {
            visitor.CteQueries ??= new();
            if (!visitor.CteQueries.Contains(cteQuery))
                visitor.CteQueries.Add(cteQuery);
        }       
        subQuery.Visitor.CopyTo(visitor);
    }

    internal static void CopyTo(this IDataParameterCollection dbParameters, IDataParameterCollection other)
    {
        if (dbParameters == null || dbParameters.Count == 0)
            return;
        foreach (var dbParameter in dbParameters)
        {
            other.Add(dbParameter);
        }
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

        int index = 0, readerIndex = 0;
        var root = NewBuildInfo(entityType);
        var current = root;
        var parent = root;
        var readerBuilders = new Dictionary<ReaderField, EntityBuildInfo>();
        var deferredBuilds = new Stack<EntityBuildInfo>();
        while (readerIndex < readerFields.Count)
        {
            var readerField = readerFields[readerIndex];
            if (readerField.FieldType == ReaderFieldType.Field)
            {
                var fieldType = reader.GetFieldType(index);
                var typeHandler = readerField.MemberMapper?.TypeHandler;
                var readerValueExpr = GetReaderValue(ormProviderExpr, readerExpr, Expression.Constant(index),
                    readerField.TargetMember?.GetMemberType(), fieldType, typeHandler, blockParameters, blockBodies);
                if (root.IsDefault) root.Bindings.Add(Expression.Bind(readerField.TargetMember, readerValueExpr));
                else root.Arguments.Add(readerValueExpr);
                index++;
            }
            else
            {
                MemberInfo fieldMember = null;
                Expression readerValueExpr = null;
                ReaderField childReaderField = null;
                var childIndex = 0;
                var endIndex = index + readerField.ReaderFields.Count;

                if (readerField.FieldType == ReaderFieldType.DeferredFields)
                {
                    //TODO:测试
                    if (readerField.TargetMember != null && readerField.TargetMember.GetMemberType().IsEntityType(out _))
                    {
                        current = NewBuildInfo(readerField.TargetMember.GetMemberType(), readerField.TargetMember, parent);
                        readerBuilders.Add(readerField, current);
                    }
                    Expression executeExpr = null;
                    List<Expression> argsExprs = null;
                    if (endIndex > index)
                    {
                        argsExprs = new List<Expression>();
                        while (index < endIndex)
                        {
                            var fieldType = reader.GetFieldType(index);
                            //延迟的方法调用，有字段值作为方法参数就读取，没有什么也不做
                            childReaderField = readerField.ReaderFields[childIndex];
                            //本地函数调用
                            var memberMapper = childReaderField.MemberMapper;
                            readerValueExpr = GetReaderValue(ormProviderExpr, readerExpr, Expression.Constant(index),
                                memberMapper.MemberType, fieldType, memberMapper.TypeHandler, blockParameters, blockBodies);
                            argsExprs.Add(readerValueExpr);
                            childIndex++;
                            index++;
                        }
                        executeExpr = Expression.Invoke(readerField.DeferredDelegate, argsExprs.ToArray());
                    }
                    else executeExpr = Expression.Invoke(readerField.DeferredDelegate);
                    if (current.IsDefault)
                        current.Bindings.Add(Expression.Bind(readerField.TargetMember, executeExpr));
                    else current.Arguments.Add(executeExpr);
                }
                else if (readerField.FieldType == ReaderFieldType.IncludeRef)
                {
                    //Include导航属性引用不能单独Select，前面一定有Parameter访问
                    //Include导航属性引用单独处理，先设置默认值，在整个实体初始化完后，再设置具体值，初始化Action在成员访问的时候，已经构建好了
                    var instanceExpr = Expression.Default(readerField.TargetMember.GetMemberType());
                    if (parent.IsDefault)
                        parent.Bindings.Add(Expression.Bind(readerField.TargetMember, instanceExpr));
                    else parent.Arguments.Add(instanceExpr);
                    readerIndex++;
                    continue;
                }
                else
                {
                    //默认是目标类型，并且也只有第一个ReaderField才是目标类型
                    if (!readerField.IsTargetType)
                    {
                        if (readerField.Parent != null)
                            parent = readerBuilders[readerField.Parent];
                        else parent = root;
                        current = NewBuildInfo(readerField.TargetMember.GetMemberType(), readerField.TargetMember, parent);
                    }
                    while (index < endIndex)
                    {
                        var fieldType = reader.GetFieldType(index);
                        childReaderField = readerField.ReaderFields[childIndex];
                        fieldMember = childReaderField.TargetMember;
                        var typeHandler = childReaderField.MemberMapper?.TypeHandler;
                        readerValueExpr = GetReaderValue(ormProviderExpr, readerExpr, Expression.Constant(index),
                            fieldMember.GetMemberType(), fieldType, typeHandler, blockParameters, blockBodies);

                        if (current.IsDefault) current.Bindings.Add(Expression.Bind(fieldMember, readerValueExpr));
                        else current.Arguments.Add(readerValueExpr);

                        childIndex++;
                        index++;
                    }

                    //有include对象
                    if (readerField.HasNextInclude)
                    {
                        deferredBuilds.Push(current);
                        readerBuilders.Add(readerField, current);
                    }
                    else
                    {
                        do
                        {
                            //创建子对象，并赋值给父对象的属性,直到Select语句
                            Expression instanceExpr = null;
                            if (current.IsDefault)
                                instanceExpr = Expression.MemberInit(Expression.New(current.Constructor), current.Bindings);
                            else instanceExpr = Expression.New(current.Constructor, current.Arguments);
                            current.Instance = instanceExpr;

                            //赋值给父对象的属性
                            if (current.Parent == null)
                                break;
                            if (current.Parent.IsDefault)
                                current.Parent.Bindings.Add(Expression.Bind(current.FromMember, instanceExpr));
                            else current.Parent.Arguments.Add(instanceExpr);
                        }
                        while (deferredBuilds.TryPop(out current));
                    }
                }
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
        else if (underlyingType == typeof(DateTime))
        {
            if (fieldType == typeof(long) || fieldType == typeof(ulong))
                typedValueExpr = Expression.New(underlyingType.GetConstructor(new Type[] { fieldType }), Expression.Convert(objLocalExpr, fieldType));
            else if (fieldType == typeof(string))
            {
                methodInfo = underlyingType.GetMethod(nameof(DateTime.Parse), new Type[] { typeof(string) });
                typedValueExpr = Expression.Call(methodInfo, Expression.Convert(objLocalExpr, fieldType));
            }
            else if (fieldType == typeof(DateOnly))
            {
                methodInfo = underlyingType.GetMethod(nameof(DateOnly.ToDateTime), new Type[] { typeof(TimeOnly) });
                typedValueExpr = Expression.Call(Expression.Convert(objLocalExpr, fieldType), methodInfo, Expression.Constant(TimeOnly.MinValue));
            }
            else throw new NotSupportedException($"暂时不支持的类型,FieldType:{fieldType.FullName},TargetType:{targetType.FullName}");
        }
        else if (underlyingType == typeof(DateOnly))
        {
            if (fieldType == typeof(string))
            {
                methodInfo = underlyingType.GetMethod(nameof(DateOnly.Parse), new Type[] { typeof(string) });
                typedValueExpr = Expression.Call(methodInfo, Expression.Convert(objLocalExpr, fieldType));
            }
            else if (fieldType == typeof(DateTime))
            {
                methodInfo = underlyingType.GetMethod(nameof(DateOnly.FromDateTime), new Type[] { typeof(DateTime) });
                typedValueExpr = Expression.Call(methodInfo, Expression.Convert(objLocalExpr, fieldType));
            }
            else throw new NotSupportedException($"暂时不支持的类型,FieldType:{fieldType.FullName},TargetType:{targetType.FullName}");
        }
        else if (underlyingType == typeof(TimeSpan))
        {
            if (fieldType == typeof(long))
                typedValueExpr = Expression.New(underlyingType.GetConstructor(new Type[] { fieldType }), Expression.Convert(objLocalExpr, fieldType));
            else if (fieldType == typeof(string))
            {
                methodInfo = underlyingType.GetMethod(nameof(TimeSpan.Parse), new Type[] { typeof(string) });
                typedValueExpr = Expression.Call(methodInfo, Expression.Convert(objLocalExpr, fieldType));
            }
            else if (fieldType == typeof(TimeOnly))
            {
                methodInfo = underlyingType.GetMethod(nameof(TimeOnly.ToTimeSpan));
                typedValueExpr = Expression.Call(Expression.Convert(objLocalExpr, fieldType), methodInfo);
            }
            else throw new NotSupportedException($"暂时不支持的类型,FieldType:{fieldType.FullName},TargetType:{targetType.FullName}");
        }
        else if (underlyingType == typeof(TimeOnly))
        {
            if (fieldType == typeof(long))
                typedValueExpr = Expression.New(underlyingType.GetConstructor(new Type[] { fieldType }), Expression.Convert(objLocalExpr, fieldType));
            else if (fieldType == typeof(string))
            {
                methodInfo = underlyingType.GetMethod(nameof(TimeOnly.Parse), new Type[] { typeof(string) });
                typedValueExpr = Expression.Call(methodInfo, Expression.Convert(objLocalExpr, fieldType));
            }
            else if (fieldType == typeof(DateTime))
            {
                methodInfo = underlyingType.GetMethod(nameof(TimeOnly.FromDateTime));
                typedValueExpr = Expression.Call(methodInfo, Expression.Convert(objLocalExpr, fieldType));
            }
            else if (fieldType == typeof(TimeSpan))
            {
                methodInfo = underlyingType.GetMethod(nameof(TimeOnly.FromTimeSpan));
                typedValueExpr = Expression.Call(methodInfo, Expression.Convert(objLocalExpr, fieldType));
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
    private static int GetTypeReaderKey(Type entityType, string dbKey, Type ormProviderType, List<ReaderField> readerFields)
    {
        var hashCode = new HashCode();
        hashCode.Add(dbKey);
        hashCode.Add(ormProviderType);
        hashCode.Add(entityType);
        hashCode.Add(readerFields.Count);
        foreach (var readerField in readerFields)
        {
            hashCode.Add(readerField.FieldType);
            hashCode.Add(readerField.TargetMember);

            if (readerField.ReaderFields != null)
            {
                hashCode.Add(readerField.ReaderFields.Count);
                foreach (var childReaderField in readerField.ReaderFields)
                {
                    hashCode.Add(childReaderField.FieldType);
                    hashCode.Add(childReaderField.TargetMember);
                }
            }
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
    class EntityBuildInfo
    {
        public bool IsDefault { get; set; }
        public ConstructorInfo Constructor { get; set; }
        public List<MemberBinding> Bindings { get; set; }
        public List<Expression> Arguments { get; set; }
        public MemberInfo FromMember { get; set; }
        public EntityBuildInfo Parent { get; set; }
        public Expression Instance { get; set; }
        public EntityBuildInfo Clone(EntityBuildInfo parent, MemberInfo fromMember)
        {
            return new EntityBuildInfo
            {
                IsDefault = this.IsDefault,
                Constructor = this.Constructor,
                Bindings = this.Bindings,
                Arguments = this.Arguments,
                FromMember = fromMember,
                Parent = parent
            };
        }
    }
}
