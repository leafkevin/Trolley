using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

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
        typeof(DBNull)];
    private static readonly ConcurrentDictionary<int, Delegate> typeReaderDeserializerCache = new();
    private static readonly ConcurrentDictionary<int, Delegate> valueTupleReaderDeserializerCache = new();
    private static readonly ConcurrentDictionary<int, Delegate> queryReaderDeserializerCache = new();
    private static readonly ConcurrentDictionary<int, Delegate> readerValueConverterCache = new();

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
        var valueGetter = dbContext.OrmProvider.GetReaderValueGetter(targetType, fieldType, dbContext.Options);
        return (TValue)valueGetter.Invoke(reader.BaseDataReader[0]);
    }
    /// <summary>
    /// 使用SQL+参数查询，返回的是单纯的实体
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="reader"></param>
    /// <param name="dbContext"></param>
    /// <returns></returns>
    public static TEntity ToEntity<TEntity>(this ITheaDataReader reader, DbContext dbContext)
    {
        var dbReader = reader.BaseDataReader;
        var entityType = typeof(TEntity);
        var ormProviderType = dbContext.OrmProvider.OrmProviderType;
        var isValueTuple = entityType.FullName.StartsWith("System.ValueTuple`");

        int cacheKey;
        ConcurrentDictionary<int, Delegate> deserializerCache;
        if (isValueTuple)
        {
            cacheKey = GetTypeReaderKey(entityType, ormProviderType, dbReader);
            deserializerCache = valueTupleReaderDeserializerCache;
        }
        else
        {
            cacheKey = GetTypeReaderKey(entityType, ormProviderType, dbReader);
            deserializerCache = typeReaderDeserializerCache;
        }
        if (!deserializerCache.TryGetValue(cacheKey, out var deserializer))
        {
            deserializer = CreateReaderDeserializer(dbContext, dbReader, entityType, isValueTuple);
            deserializerCache.TryAdd(cacheKey, deserializer);
        }
        return ((Func<IDataReader, TEntity>)deserializer).Invoke(dbReader);
    }
    /// <summary>
    /// 使用非SQL查询，返回的是可能包含Include对象，包含层次的实体对象
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="reader"></param>
    /// <param name="dbContext"></param>
    /// <param name="readerFields"></param>
    /// <param name="isUseReaderOrder"></param>
    /// <returns></returns>
    public static TEntity ToEntity<TEntity>(this ITheaDataReader reader, DbContext dbContext, List<SqlFieldSegment> readerFields, bool isUseReaderOrder = false)
    {
        var dbReader = reader.BaseDataReader;
        var entityType = typeof(TEntity);
        var ormProviderType = dbContext.OrmProvider.OrmProviderType;
        var cacheKey = GetTypeReaderKey(entityType, ormProviderType, dbReader, readerFields, isUseReaderOrder, out var deferredFuncs);
        if (!queryReaderDeserializerCache.TryGetValue(cacheKey, out var deserializer))
        {
            deserializer = CreateReaderDeserializer(dbContext, dbReader, entityType, readerFields, isUseReaderOrder);
            queryReaderDeserializerCache.TryAdd(cacheKey, deserializer);
        }
        if (deferredFuncs != null && deferredFuncs.Count > 0)
        {
            deferredFuncs.Insert(0, dbReader);
            return (TEntity)deserializer.DynamicInvoke(deferredFuncs.ToArray());
        }
        return ((Func<IDataReader, TEntity>)deserializer).Invoke(dbReader);
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
        return (T)Convert.ChangeType(readerValue, typeof(T));
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
    internal static DateTime ToUtc(this DateTime dateTime)
    {
        if (dateTime.Kind == DateTimeKind.Local)
            return dateTime.ToUniversalTime();
        return dateTime;
    }
    internal static DateTimeOffset ToUtc(this DateTimeOffset dateTimeOffset)
    {
        if (dateTimeOffset.DateTime.Kind == DateTimeKind.Local)
            return dateTimeOffset.ToUniversalTime();
        return dateTimeOffset;
    }
    internal static DateTime ToLocal(this DateTime dateTime)
    {
        if (dateTime.Kind == DateTimeKind.Utc)
            return dateTime.ToLocalTime();
        return dateTime;
    }
    internal static DateTimeOffset ToLocal(this DateTimeOffset dateTimeOffset)
    {
        if (dateTimeOffset.DateTime.Kind == DateTimeKind.Utc)
            return dateTimeOffset.ToLocalTime();
        return dateTimeOffset;
    }
    private static Delegate CreateReaderDeserializer(DbContext dbContext, IDataReader reader, Type entityType, bool isValueTuple)
    {
        var readerExpr = Expression.Parameter(typeof(IDataReader), "reader");
        var ormProviderExpr = Expression.Constant(dbContext.OrmProvider);
        var entityMapper = dbContext.MapProvider.GetEntityMap(entityType);
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
            var readerValueExpr = GetReaderValue(dbContext, ormProviderExpr, readerExpr, Expression.Constant(index),
                memberMapper.MemberType, fieldType, memberMapper.TypeHandler, blockParameters, blockBodies);

            if (target.IsDefault)
                target.Bindings.Add(Expression.Bind(memberMapper.Member, readerValueExpr));
            else target.Arguments.Add(readerValueExpr);
            index++;
        }
        var resultLabelExpr = Expression.Label(entityType);
        Expression returnExpr;
        if (target.IsDefault) returnExpr = Expression.MemberInit(Expression.New(target.Constructor), target.Bindings);
        else returnExpr = Expression.New(target.Constructor, target.Arguments);

        blockBodies.Add(Expression.Return(resultLabelExpr, returnExpr));
        blockBodies.Add(Expression.Label(resultLabelExpr, Expression.Default(entityType)));
        return Expression.Lambda(Expression.Block(blockParameters, blockBodies), readerExpr).Compile();
    }
    private static Delegate CreateReaderDeserializer(DbContext dbContext, IDataReader reader, Type entityType, List<SqlFieldSegment> readerFields, bool isUseReaderOrder = false)
    {
        var blockParameters = new List<ParameterExpression>();
        var blockBodies = new List<Expression>();
        var readerExpr = Expression.Parameter(typeof(IDataReader), "reader");
        var parameterExprs = new List<ParameterExpression> { readerExpr };
        var ormProviderExpr = Expression.Constant(dbContext.OrmProvider);

        int index = 0, readerIndex = 0;
        var root = NewBuildInfo(entityType);
        var current = root;
        var parent = root;
        var readerBuilders = new Dictionary<SqlFieldSegment, EntityBuildInfo>();
        var deferredBuilds = new Stack<EntityBuildInfo>();
        while (readerIndex < readerFields.Count)
        {
            SqlFieldSegment readerField = null;
            //PostgreSql，使用RETURNING *返回的字段顺序不固定，需要根据reader的顺序来绑定接收实体
            //此时的字段都是基础类型
            if (isUseReaderOrder)
            {
                var fieldName = reader.GetName(index);
                readerField = readerFields.Find(f => f.Body == fieldName);
            }
            else readerField = readerFields[readerIndex];
            if (readerField.FieldType == SqlFieldType.Field)
            {
                var fieldType = reader.GetFieldType(index);
                var readerValueExpr = GetReaderValue(dbContext, ormProviderExpr, readerExpr, Expression.Constant(index),
                    readerField.SegmentType, fieldType, readerField.TypeHandler, blockParameters, blockBodies);
                if (root.IsDefault) root.Bindings.Add(Expression.Bind(readerField.TargetMember, readerValueExpr));
                else root.Arguments.Add(readerValueExpr);
                index++;
            }
            else
            {
                Expression readerValueExpr = null;
                SqlFieldSegment childReaderField = null;
                var childIndex = 0;
                var endIndex = index;
                //当无参数的Deferred函数调用，ReaderFields的值为null，也没有从数据库读取字段，count=0
                if (readerField.Fields != null)
                    endIndex += readerField.Fields.Count;

                if (readerField.FieldType == SqlFieldType.DeferredFields)
                {
                    if (readerField.SegmentType.IsEntityType(out _))
                    {
                        current = NewBuildInfo(readerField.SegmentType, readerField.TargetMember, parent);
                        readerBuilders.Add(readerField, current);
                    }

                    Expression executeExpr = null;

                    var deferredDelegateType = readerField.DeferredDelegateType;
                    var deferredFuncExpr = Expression.Parameter(typeof(object), $"deferredFunc{parameterExprs.Count}");
                    var typedDeferredFuncExpr = Expression.Convert(deferredFuncExpr, deferredDelegateType);
                    parameterExprs.Add(deferredFuncExpr);
                    //把延迟方法调用委托当作参数传进来，这样缓存才有效，相同key，不同的延迟方法
                    if (endIndex > index)
                    {
                        var argsExprs = new List<Expression>();
                        while (index < endIndex)
                        {
                            var fieldType = reader.GetFieldType(index);
                            //延迟的方法调用，有字段值作为方法参数就读取，没有什么也不做
                            childReaderField = readerField.Fields[childIndex];
                            readerValueExpr = GetReaderValue(dbContext, ormProviderExpr, readerExpr, Expression.Constant(index),
                                childReaderField.SegmentType, fieldType, childReaderField.TypeHandler, blockParameters, blockBodies);
                            argsExprs.Add(readerValueExpr);
                            childIndex++;
                            index++;
                        }
                        executeExpr = Expression.Invoke(typedDeferredFuncExpr, argsExprs);
                    }
                    else executeExpr = Expression.Invoke(typedDeferredFuncExpr);

                    if (current.IsDefault)
                        current.Bindings.Add(Expression.Bind(readerField.TargetMember, executeExpr));
                    else current.Arguments.Add(executeExpr);
                }
                else if (readerField.FieldType == SqlFieldType.IncludeRef)
                {
                    //Include导航属性引用不能单独Select，前面一定有Parameter访问
                    //Include导航属性引用单独处理，先设置默认值，在整个实体初始化完后，再设置具体值，初始化Action在成员访问的时候，已经构建好了
                    var refReaderField = readerField.Value as SqlFieldSegment;
                    var instanceExpr = readerBuilders[refReaderField].Instance;
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
                        current = NewBuildInfo(readerField.SegmentType, readerField.TargetMember, parent);
                    }
                    while (index < endIndex)
                    {
                        var fieldType = reader.GetFieldType(index);
                        childReaderField = readerField.Fields[childIndex];
                        readerValueExpr = GetReaderValue(dbContext, ormProviderExpr, readerExpr, Expression.Constant(index),
                            childReaderField.SegmentType, fieldType, childReaderField.TypeHandler, blockParameters, blockBodies);

                        if (current.IsDefault) current.Bindings.Add(Expression.Bind(childReaderField.TargetMember, readerValueExpr));
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
        return Expression.Lambda(Expression.Block(blockParameters, blockBodies), parameterExprs).Compile();
    }
    private static Expression GetReaderValue(DbContext dbContext, Expression ormProviderExpr, ParameterExpression readerExpr, Expression indexExpr, Type targetType, Type fieldType, ITypeHandler typeHandler, List<ParameterExpression> blockParameters, List<Expression> blockBodies)
    {
        var methodInfo = typeof(IDataRecord).GetMethod(nameof(IDataRecord.GetValue), [typeof(int)]);
        var readerValueExpr = AssignLocalParameter(typeof(object), Expression.Call(readerExpr, methodInfo, indexExpr), blockParameters, blockBodies);
        var isNullable = targetType.IsNullableType(out var underlyingType);
        Expression targetValueExpr = null;
        if (typeHandler != null)
        {
            methodInfo = typeof(ITypeHandler).GetMethod(nameof(ITypeHandler.Parse), [typeof(IOrmProvider), typeof(Type), typeof(object)]);
            var typeHandlerExpr = Expression.Constant(typeHandler);
            var underlyingTypeExpr = Expression.Constant(underlyingType);
            targetValueExpr = Expression.Call(typeHandlerExpr, methodInfo, ormProviderExpr, underlyingTypeExpr, readerValueExpr);
        }
        else
        {
            var valueGetter = dbContext.OrmProvider.GetReaderValueGetter(targetType, fieldType, dbContext.Options);
            targetValueExpr = Expression.Invoke(Expression.Constant(valueGetter), readerValueExpr);
        }
        blockBodies.Add(Expression.Assign(readerValueExpr, targetValueExpr));
        return Expression.Convert(readerValueExpr, targetType);
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
    private static ParameterExpression AssignLocalParameter(Type type, Expression valueExpr, List<ParameterExpression> blockParameters, List<Expression> blockBodies)
    {
        var objLocalExpr = Expression.Variable(type, $"local{blockParameters.Count}");
        blockParameters.Add(objLocalExpr);
        blockBodies.Add(Expression.Assign(objLocalExpr, valueExpr));
        return objLocalExpr;
    }
    private static int GetTypeReaderKey(Type entityType, OrmProviderType ormProviderType, IDataReader reader, List<SqlFieldSegment> readerFields, bool isUseReaderOrder, out List<object> deferredFuncs)
    {
        deferredFuncs = null;
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
            if (readerField.FieldType == SqlFieldType.DeferredFields)
            {
                deferredFuncs ??= new();
                deferredFuncs.Add(readerField.DeferredDelegate);
            }
            hashCode.Add(readerField.FieldType);
            if (readerField.FieldType == SqlFieldType.Entity && readerField.IsTargetType)
                hashCode.Add("TargetEntity");
            else hashCode.Add(readerField.TargetMember.Name);
        }
        hashCode.Add(isUseReaderOrder);
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
                if (readerField.FieldType == SqlFieldType.DeferredFields)
                {
                    deferredFuncs ??= new();
                    deferredFuncs.Add(readerField.DeferredDelegate);
                }
                hashCode = hashCode * 23 + readerField.FieldType.GetHashCode();
                if (readerField.FieldType == SqlFieldType.Entity && readerField.IsTargetType)
                    hashCode = hashCode * 23 + "TargetEntity".GetHashCode();
                else hashCode = hashCode * 23 + readerField.TargetMember.Name.GetHashCode();
            }
            hashCode = hashCode * 23 + isUseReaderOrder.GetHashCode();
        }
        return hashCode;
#endif
    }
    private static int GetTypeReaderKey(Type entityType, OrmProviderType ormProviderType, IDataReader reader)
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
