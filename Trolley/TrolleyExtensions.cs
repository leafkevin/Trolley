using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley;

public static class TrolleyExtensions
{
    private static readonly ConcurrentDictionary<int, Delegate> typeReaderDeserializerCache = new();
    private static readonly ConcurrentDictionary<int, Delegate> queryReaderDeserializerCache = new();
    private static readonly ConcurrentDictionary<int, Delegate> readerValueConverterCache = new();

    public static TEntity QueryFirst<TEntity>(this IRepository repository, Expression<Func<TEntity, bool>> predicate = null)
        => repository.From<TEntity>().Where(predicate).First();
    public static async Task<TEntity> QueryFirstAsync<TEntity>(this IRepository repository, Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        => await repository.From<TEntity>().Where(predicate).FirstAsync(cancellationToken);
    public static List<TEntity> Query<TEntity>(this IRepository repository, Expression<Func<TEntity, bool>> predicate)
        => repository.From<TEntity>().Where(predicate).ToList();
    public static async Task<List<TEntity>> QueryAsync<TEntity>(this IRepository repository, Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        => await repository.From<TEntity>().Where(predicate).ToListAsync(cancellationToken);
    public static IPagedList<TEntity> QueryPage<TEntity>(this IRepository repository, int pageIndex, int pageSize, Expression<Func<TEntity, bool>> predicate = null)
        => repository.From<TEntity>().Where(predicate).ToPageList(pageIndex, pageSize);
    public static async Task<IPagedList<TEntity>> QueryPageAsync<TEntity>(this IRepository repository, int pageIndex, int pageSize, Expression<Func<TEntity, bool>> predicate = null, CancellationToken cancellationToken = default)
        => await repository.From<TEntity>().Where(predicate).ToPageListAsync(pageIndex, pageSize, cancellationToken);

    public static int Create<TEntity>(this IRepository repository, object parameter)
        => repository.Create<TEntity>().WithBy(parameter).Execute();
    public static async Task<int> CreateAsync<TEntity>(this IRepository repository, object parameter, CancellationToken cancellationToken = default)
        => await repository.Create<TEntity>().WithBy(parameter).ExecuteAsync(cancellationToken);
    public static int Create<TEntity>(this IRepository repository, string sql, object parameter)
        => repository.Create<TEntity>().RawSql(sql, parameter).Execute();
    public static async Task<int> CreateAsync<TEntity>(this IRepository repository, string sql, object parameter, CancellationToken cancellationToken = default)
        => await repository.Create<TEntity>().RawSql(sql, parameter).ExecuteAsync(cancellationToken);
    public static int Create<TEntity>(this IRepository repository, IEnumerable entities, int bulkCount = 500)
        => repository.Create<TEntity>().WithBy(entities, bulkCount).Execute();
    public static async Task<int> CreateAsync<TEntity>(this IRepository repository, IEnumerable entities, int bulkCount = 500, CancellationToken cancellationToken = default)
        => await repository.Create<TEntity>().WithBy(entities, bulkCount).ExecuteAsync(cancellationToken);

    public static int Update<TEntity>(this IRepository repository, object parameters)
        => repository.Update<TEntity>().WithBy(parameters).Execute();
    public static async Task<int> UpdateAsync<TEntity>(this IRepository repository, object parameters, CancellationToken cancellationToken = default)
        => await repository.Update<TEntity>().WithBy(parameters).ExecuteAsync(cancellationToken);
    public static int Update<TEntity>(this IRepository repository, IEnumerable entities, int bulkCount = 500)
        => repository.Update<TEntity>().WithBy(entities, bulkCount).Execute();
    public static async Task<int> UpdateAsync<TEntity>(this IRepository repository, IEnumerable entities, int bulkCount = 500, CancellationToken cancellationToken = default)
        => await repository.Update<TEntity>().WithBy(entities, bulkCount).ExecuteAsync(cancellationToken);
    public static int Update<TEntity, TFields>(this IRepository repository, Expression<Func<TEntity, TFields>> fieldsExpr, Expression<Func<TEntity, bool>> predicate)
        => repository.Update<TEntity>().Set(fieldsExpr).Where(predicate).Execute();
    public static async Task<int> UpdateAsync<TEntity, TFields>(this IRepository repository, Expression<Func<TEntity, TFields>> fieldsExpr, Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        => await repository.Update<TEntity>().Set(fieldsExpr).Where(predicate).ExecuteAsync(cancellationToken);
    public static int Update<TEntity, TField>(this IRepository repository, Expression<Func<TEntity, TField>> fieldExpr, object parameter)
        => repository.Update<TEntity>().WithBy(fieldExpr, parameter).Execute();
    public static async Task<int> UpdateAsync<TEntity, TMember>(this IRepository repository, Expression<Func<TEntity, TMember>> fieldExpr, object parameter, CancellationToken cancellationToken = default)
        => await repository.Update<TEntity>().WithBy(fieldExpr, parameter).ExecuteAsync(cancellationToken);
    public static int Update<TEntity, TFields>(this IRepository repository, Expression<Func<TEntity, TFields>> fieldsExpr, object parameters, int bulkCount = 500)
        => repository.Update<TEntity>().WithBy(fieldsExpr, parameters, bulkCount).Execute();
    public static async Task<int> UpdateAsync<TEntity, TFields>(this IRepository repository, Expression<Func<TEntity, TFields>> fieldsExpr, object parameters, int bulkCount = 500, CancellationToken cancellationToken = default)
        => await repository.Update<TEntity>().WithBy(fieldsExpr, parameters, bulkCount).ExecuteAsync(cancellationToken);

    public static int Delete<TEntity>(this IRepository repository, Expression<Func<TEntity, bool>> predicate)
        => repository.Delete<TEntity>().Where(predicate).Execute();
    public static async Task<int> DeleteAsync<TEntity>(this IRepository repository, Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        => await repository.Delete<TEntity>().Where(predicate).ExecuteAsync(cancellationToken);
    public static int Delete<TEntity>(this IRepository repository, object keys)
        => repository.Delete<TEntity>().Where(keys).Execute();
    public static async Task<int> DeleteAsync<TEntity>(this IRepository repository, object keys, CancellationToken cancellationToken = default)
        => await repository.Delete<TEntity>().Where(keys).ExecuteAsync(cancellationToken);

    public static string GetQuotedValue(this IOrmProvider ormProvider, object value)
    {
        if (value == null) return "NULL";
        return ormProvider.GetQuotedValue(value.GetType(), value);
    }
    public static EntityMap GetEntityMap(this IOrmDbFactory dbFactory, Type entityType)
    {
        if (!dbFactory.TryGetEntityMap(entityType, out var mapper))
        {
            mapper = EntityMap.CreateDefaultMap(entityType);
            dbFactory.AddEntityMap(entityType, mapper);
        }
        return mapper;
    }
    public static EntityMap GetEntityMap(this IOrmDbFactory dbFactory, Type entityType, Type mapToType)
    {
        if (!dbFactory.TryGetEntityMap(entityType, out var mapper))
        {
            var mapToMapper = dbFactory.GetEntityMap(mapToType);
            mapper = EntityMap.CreateDefaultMap(entityType, mapToMapper);
            dbFactory.AddEntityMap(entityType, mapper);
        }
        return mapper;
    }
    public static bool IsEntityType(this Type type)
    {
        var typeCode = Type.GetTypeCode(type);
        switch (typeCode)
        {
            case TypeCode.DBNull:
            case TypeCode.Boolean:
            case TypeCode.Char:
            case TypeCode.SByte:
            case TypeCode.Byte:
            case TypeCode.Int16:
            case TypeCode.UInt16:
            case TypeCode.Int32:
            case TypeCode.UInt32:
            case TypeCode.Int64:
            case TypeCode.UInt64:
            case TypeCode.Single:
            case TypeCode.Double:
            case TypeCode.Decimal:
            case TypeCode.DateTime:
            case TypeCode.String:
                return false;
        }
        if (type.IsClass) return true;
        if (type.IsValueType && !type.IsEnum && !type.IsPrimitive && type.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Count(f => f.MemberType == MemberTypes.Field || (f.MemberType == MemberTypes.Property && f is PropertyInfo propertyInfo && propertyInfo.GetIndexParameters().Length == 0)) > 1)
            return true;
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
    public static bool TryPop<T>(this Stack<T> stack, Func<T, bool> filter, out T element)
    {
        if (stack.TryPeek(out element) && filter.Invoke(element))
            return stack.TryPop(out _);
        return false;
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
    public static bool GetParameters(this Expression expr, out List<string> parameterNames)
    {
        var visitor = new TestVisitor();
        visitor.Visit(expr);
        if (visitor.IsParameter)
        {
            parameterNames = visitor.ParameterNames;
            return visitor.IsParameter;
        }
        parameterNames = null;
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
    internal static TEntity To<TEntity>(this IDataReader reader, IOrmDbFactory dbFactory, TheaConnection connection)
    {
        var entityType = typeof(TEntity);
        var cacheKey = GetReaderKey(entityType, connection, reader);
        if (!typeReaderDeserializerCache.TryGetValue(cacheKey, out var deserializer))
        {
            deserializer = CreateReaderDeserializer(reader, dbFactory, entityType);
            typeReaderDeserializerCache.TryAdd(cacheKey, deserializer);
        }
        return ((Func<IDataReader, TEntity>)deserializer).Invoke(reader);
    }
    internal static TEntity To<TEntity>(this IDataReader reader, TheaConnection connection, List<ReaderField> readerFields)
    {
        var entityType = typeof(TEntity);
        var cacheKey = GetReaderKey(entityType, connection, reader);
        if (!queryReaderDeserializerCache.TryGetValue(cacheKey, out var deserializer))
        {
            deserializer = CreateReaderDeserializer(reader, entityType, readerFields);
            queryReaderDeserializerCache.TryAdd(cacheKey, deserializer);
        }
        return ((Func<IDataReader, TEntity>)deserializer).Invoke(reader);
    }
    private static Delegate CreateReaderDeserializer(IDataReader reader, IOrmDbFactory dbFactory, Type entityType)
    {
        var blockBodies = new List<Expression>();
        var readerExpr = Expression.Parameter(typeof(IDataReader), "reader");
        var entityMapper = dbFactory.GetEntityMap(entityType);
        var index = 0;
        var target = NewBuildInfo(entityType);
        foreach (var memberMapper in entityMapper.MemberMaps)
        {
            if (memberMapper.IsIgnore || memberMapper.IsNavigation || memberMapper.MemberType.IsEntityType())
                continue;

            var fieldType = reader.GetFieldType(index);
            var readerValueExpr = GetReaderValue(readerExpr, Expression.Constant(index), memberMapper.MemberType, fieldType);

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
        blockBodies.Add(Expression.Label(resultLabelExpr, Expression.Constant(null, entityType)));
        return Expression.Lambda(returnExpr, readerExpr).Compile();
    }
    private static Delegate CreateReaderDeserializer(IDataReader reader, Type entityType, List<ReaderField> readerFields)
    {
        var blockBodies = new List<Expression>();
        var readerExpr = Expression.Parameter(typeof(IDataReader), "reader");

        int index = 0, readerIndex = 0;
        var root = NewBuildInfo(entityType);
        var current = root;
        var readerBuilders = new Dictionary<int, EntityBuildInfo>();
        var deferredBuilds = new Stack<EntityBuildInfo>();
        while (readerIndex < readerFields.Count)
        {
            var readerField = readerFields[readerIndex];
            if (readerField.Type == ReaderFieldType.Entity || readerField.Type == ReaderFieldType.AnonymousField)
            {
                //第一个是实体，如果是entityType类型，则是Parameter类型表达式访问
                //如果不是entityType类型，就是子类型，则是New或是MemeberInit类型表达式访问
                bool isTarget = false;
                if (readerIndex == 0)
                    isTarget = readerField.TableSegment.EntityType == entityType;

                EntityBuildInfo parent = null;
                if (!isTarget)
                {
                    if (readerField.ParentIndex.HasValue)
                        parent = readerBuilders[readerField.ParentIndex.Value];
                    else parent = root;
                    current = NewBuildInfo(readerField.TableSegment.EntityType, readerField.TargetMember, parent);
                }
                readerBuilders.Add(readerField.Index, current);

                if (readerField.Type == ReaderFieldType.AnonymousField)
                {
                    int endIndex = index + readerField.FieldCount;
                    while (index < endIndex)
                    {
                        var fieldType = reader.GetFieldType(index);
                        var readerValueExpr = GetReaderValue(readerExpr, Expression.Constant(index), readerFields[index].FromMember.GetMemberType(), fieldType);
                        if (current.IsDefault) current.Bindings.Add(Expression.Bind(readerFields[index].FromMember, readerValueExpr));
                        else current.Arguments.Add(readerValueExpr);
                        index++;
                    }
                    readerIndex += readerField.FieldCount - 1;
                }
                else
                {
                    var memberMappers = readerField.TableSegment.Mapper.MemberMaps;
                    foreach (var memberMapper in memberMappers)
                    {
                        if (memberMapper.IsIgnore || memberMapper.IsNavigation || memberMapper.MemberType.IsEntityType())
                            continue;
                        var fieldType = reader.GetFieldType(index);
                        var readerValueExpr = GetReaderValue(readerExpr, Expression.Constant(index), memberMapper.MemberType, fieldType);
                        if (current.IsDefault) current.Bindings.Add(Expression.Bind(memberMapper.Member, readerValueExpr));
                        else current.Arguments.Add(readerValueExpr);
                        index++;
                    }
                }

                if (!isTarget)
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
            }
            else
            {
                var fieldType = reader.GetFieldType(index);
                var readerValueExpr = GetReaderValue(readerExpr, Expression.Constant(index), readerField.FromMember.GetMemberType(), fieldType);
                if (root.IsDefault) root.Bindings.Add(Expression.Bind(readerField.FromMember, readerValueExpr));
                else root.Arguments.Add(readerValueExpr);
                index++;
            }
            readerIndex++;
        }
        var resultLabelExpr = Expression.Label(entityType);
        Expression returnExpr = null;
        if (root.IsDefault)
            returnExpr = Expression.MemberInit(Expression.New(root.Constructor), root.Bindings);
        else returnExpr = Expression.New(root.Constructor, root.Arguments);

        blockBodies.Add(Expression.Return(resultLabelExpr, returnExpr));
        blockBodies.Add(Expression.Label(resultLabelExpr, Expression.Constant(null, entityType)));
        return Expression.Lambda(returnExpr, readerExpr).Compile();
    }
    private static Expression GetReaderValue(ParameterExpression readerExpr, Expression indexExpr, Type targetType, Type fieldType)
    {
        var underlyingType = Nullable.GetUnderlyingType(targetType);
        bool isNullable = underlyingType != null;
        underlyingType ??= targetType;
        var methodInfo = typeof(IDataRecord).GetMethod(nameof(IDataRecord.GetValue), new Type[] { typeof(int) });
        var valueExpr = Expression.Call(readerExpr, methodInfo, indexExpr);
        Expression typedValueExpr = null;

        if (underlyingType.IsAssignableFrom(fieldType))
            typedValueExpr = Expression.Convert(valueExpr, underlyingType);
        else if (underlyingType == typeof(char))
        {
            if (fieldType == typeof(string))
            {
                typedValueExpr = Expression.Convert(valueExpr, typeof(string));
                var lengthExpr = Expression.Property(typedValueExpr, nameof(string.Length));
                var compareExpr = Expression.GreaterThan(lengthExpr, Expression.Constant(0, typeof(int)));
                methodInfo = typeof(string).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .Where(p => p.GetIndexParameters().Length > 0 && p.GetIndexParameters()[0].ParameterType == typeof(int))
                    .Select(p => p.GetGetMethod()).First();
                var getCharExpr = Expression.Call(typedValueExpr, methodInfo, Expression.Constant(0, typeof(int)));
                typedValueExpr = Expression.IfThenElse(compareExpr, getCharExpr, Expression.Default(underlyingType));
            }
            else throw new Exception($"暂时不支持的类型,FieldType:{fieldType.FullName},TargetType:{targetType.FullName}");
        }
        else if (underlyingType == typeof(Guid))
        {
            if (fieldType != typeof(string) && fieldType != typeof(byte[]))
                throw new Exception($"暂时不支持的类型,FieldType:{fieldType.FullName},TargetType:{targetType.FullName}");
            typedValueExpr = Expression.New(typeof(Guid).GetConstructor(new Type[] { fieldType }), Expression.Convert(valueExpr, fieldType));
        }
        else if (targetType.FullName == "System.Data.Linq.Binary")
        {
            methodInfo = typeof(Activator).GetMethod(nameof(Activator.CreateInstance), new Type[] { typeof(Type), typeof(object[]) });
            typedValueExpr = Expression.Call(methodInfo, Expression.Constant(targetType), Expression.Constant(new object[] { valueExpr }));
            typedValueExpr = Expression.Convert(typedValueExpr, targetType);
        }
        else
        {
            if (underlyingType.IsEnum)
            {
                if (fieldType == typeof(string))
                {
                    typedValueExpr = Expression.Convert(valueExpr, typeof(string));
                    methodInfo = typeof(Enum).GetMethod(nameof(Enum.Parse), new Type[] { typeof(Type), typeof(string), typeof(bool) });
                    var toEnumExpr = Expression.Call(methodInfo, Expression.Constant(underlyingType), typedValueExpr, Expression.Constant(true));
                    typedValueExpr = Expression.Convert(toEnumExpr, underlyingType);
                }
                else if (fieldType == typeof(byte) || fieldType == typeof(sbyte) || fieldType == typeof(short)
                    || fieldType == typeof(ushort) || fieldType == typeof(int) || fieldType == typeof(uint)
                    || fieldType == typeof(long) || fieldType == typeof(ulong))
                {
                    typedValueExpr = Expression.Convert(valueExpr, fieldType);
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
                    typedValueExpr = Expression.Call(methodInfo, valueExpr, Expression.Constant(CultureInfo.CurrentCulture));
                }
                else typedValueExpr = Expression.Convert(Expression.Convert(valueExpr, fieldType), underlyingType);
            }
        }
        if (underlyingType.IsValueType && isNullable)
            typedValueExpr = Expression.Convert(typedValueExpr, targetType);
        var isNullExpr = Expression.TypeIs(valueExpr, typeof(DBNull));
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
        var blockBodies = new List<Expression>();
        var readerExpr = Expression.Parameter(typeof(IDataReader), "reader");
        var indexExpr = Expression.Parameter(typeof(int), "index");
        var resultLabelExpr = Expression.Label(targetType);
        var bodyExpr = GetReaderValue(readerExpr, indexExpr, targetType, fieldType);
        blockBodies.Add(Expression.Return(resultLabelExpr, bodyExpr));
        blockBodies.Add(Expression.Label(resultLabelExpr, Expression.Default(targetType)));
        return Expression.Lambda(Expression.Block(blockBodies), readerExpr, indexExpr).Compile();
    }
    private static int GetReaderKey(Type entityType, TheaConnection connection, IDataReader reader)
    {
        var hashCode = new HashCode();
        hashCode.Add(entityType);
        hashCode.Add(connection);
        hashCode.Add(connection.OrmProvider);
        hashCode.Add(reader.FieldCount);
        for (int i = 0; i < reader.FieldCount; i++)
        {
            hashCode.Add(reader.GetName(i));
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
    }
}
