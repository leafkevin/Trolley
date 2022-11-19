using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Trolley;

public static class TrolleyExtensions
{
    private static readonly ConcurrentDictionary<int, Delegate> readerDeserializerCache = new();
    private static readonly ConcurrentDictionary<int, Delegate> readerValueConverterCache = new();

    public static string GetQuotedValue(this IOrmProvider ormProvider, object value)
    {
        if (value == null) return "null";
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
        throw new Exception("成员member，不是属性也不是字段");
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
            parameterName = visitor.ParameterName;
            return visitor.IsParameter;
        }
        parameterName = null;
        return false;
    }
    public static bool IsConstant(this Expression expr)
    {
        var visitor = new TestVisitor();
        visitor.Visit(expr);
        return visitor.IsConstant;
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
    internal static TEntity To<TEntity>(this IDataReader reader, TheaConnection connection, List<MemberSegment> readerFields)
    {
        var entityType = typeof(TEntity);
        var cacheKey = GetReaderKey(entityType, connection, reader);
        if (!readerDeserializerCache.TryGetValue(cacheKey, out var deserializer))
        {
            deserializer = CreateReaderDeserializer(connection, reader, entityType, readerFields);
            readerDeserializerCache.TryAdd(cacheKey, deserializer);
        }
        return ((Func<IDataReader, TEntity>)deserializer).Invoke(reader);
    }
    private static Delegate CreateReaderDeserializer(TheaConnection connection, IDataReader reader, Type entityType, List<MemberSegment> readerFields)
    {
        var blockParameters = new List<ParameterExpression>();
        var blockBodies = new List<Expression>();
        var readerExpr = Expression.Parameter(typeof(IDataReader), "reader");

        bool isDefaultCtor = false;
        NewExpression entityExpr = null;
        List<MemberBinding> bindings = null;
        List<Expression> ctorArguments = null;

        var ctor = entityType.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
        if (ctor != null)
        {
            entityExpr = Expression.New(ctor);
            bindings = new List<MemberBinding>();
            isDefaultCtor = true;
        }
        else
        {
            ctor = entityType.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).OrderBy(f => f.IsPublic ? 0 : (f.IsPrivate ? 2 : 1)).First();
            ctorArguments = new List<Expression>();
        }

        int index = 0;
        MemberSegment lastReaderFieldInfo = null;
        var current = new EntityBuildInfo
        {
            IsDefault = isDefaultCtor,
            Constructor = ctor,
            Bindings = bindings,
            Arguments = ctorArguments
        };
        Type fieldType = null;
        Expression readerValueExpr = null;

        while (index < reader.FieldCount)
        {
            var readerFieldInfo = readerFields[index];
            //先处理上一个导航属性值
            if (lastReaderFieldInfo == null || readerFieldInfo.ReaderIndex != lastReaderFieldInfo.ReaderIndex)
            {
                if (current.Parent == null)
                {
                    //处理当前字段值
                    fieldType = reader.GetFieldType(index);
                    readerValueExpr = GetReaderValue(readerExpr, Expression.Constant(index), readerFieldInfo.MemberMapper.MemberType, fieldType);
                    if (current.IsDefault) current.Bindings.Add(Expression.Bind(readerFieldInfo.FromMember, readerValueExpr));
                    else current.Arguments.Add(readerValueExpr);

                    lastReaderFieldInfo = readerFieldInfo;
                    index++;
                    continue;
                }

                //Select语句，更换了一个新的导航属性，从最底层的导航属性一直往上层赋值，直到Select语句
                while (current.Parent != null)
                {
                    //创建子对象，并赋值给父对象的属性,直到Select语句
                    if (current.IsDefault)
                        readerValueExpr = Expression.MemberInit(Expression.New(current.Constructor), current.Bindings);
                    else readerValueExpr = Expression.New(current.Constructor, current.Arguments);

                    var parent = current.Parent;
                    //赋值给父对象的属性
                    if (parent.IsDefault)
                        parent.Bindings.Add(Expression.Bind(current.FromMember, readerValueExpr));
                    else parent.Arguments.Add(readerValueExpr);
                    current = parent;
                }

                lastReaderFieldInfo = readerFieldInfo;
                index++;
                continue;
            }
            if (readerFieldInfo.TableIndex != lastReaderFieldInfo.TableIndex)
            {
                //Select语句，更换了一个新的导航属性，从最底层的导航属性一直往上层赋值，直到Select语句
                while (current.Parent != null)
                {
                    //创建子对象，并赋值给父对象的属性,直到Select语句
                    if (current.IsDefault)
                        readerValueExpr = Expression.MemberInit(Expression.New(current.Constructor), current.Bindings);
                    else readerValueExpr = Expression.New(current.Constructor, current.Arguments);

                    //赋值给父对象的属性
                    if (current.Parent.IsDefault)
                        current.Parent.Bindings.Add(Expression.Bind(current.FromMember, readerValueExpr));
                    else current.Parent.Arguments.Add(readerValueExpr);
                    current = current.Parent;
                }

                var parent = current;
                var targetType = readerFieldInfo.TableSegment.EntityType;
                ctor = targetType.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
                if (ctor != null)
                {
                    entityExpr = Expression.New(ctor);
                    bindings = new List<MemberBinding>();
                    isDefaultCtor = true;
                }
                else
                {
                    ctor = targetType.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).OrderBy(f => f.IsPublic ? 0 : (f.IsPrivate ? 2 : 1)).First();
                    ctorArguments = new List<Expression>();
                }
                current = new EntityBuildInfo
                {
                    IsDefault = isDefaultCtor,
                    Constructor = ctor,
                    Bindings = bindings,
                    Arguments = ctorArguments,
                    FromMember = readerFieldInfo.FromMember,
                    Parent = parent
                };
            }

            //处理当前字段值
            fieldType = reader.GetFieldType(index);
            readerValueExpr = GetReaderValue(readerExpr, Expression.Constant(index), readerFieldInfo.MemberMapper.MemberType, fieldType);

            MemberInfo fromMember = null;
            if (readerFieldInfo.TableIndex > 1)
                fromMember = readerFieldInfo.MemberMapper.Member;
            else fromMember = readerFieldInfo.FromMember;

            if (current.IsDefault) current.Bindings.Add(Expression.Bind(fromMember, readerValueExpr));
            else current.Arguments.Add(readerValueExpr);

            lastReaderFieldInfo = readerFieldInfo;
            index++;
        }
        //Select语句，更换了一个新的导航属性，从最底层的导航属性一直往上层赋值，直到Select语句
        while (current.Parent != null)
        {
            //创建子对象，并赋值给父对象的属性,直到Select语句
            if (current.IsDefault)
                readerValueExpr = Expression.MemberInit(Expression.New(current.Constructor), current.Bindings);
            else readerValueExpr = Expression.New(current.Constructor, current.Arguments);

            //赋值给父对象的属性
            if (current.Parent.IsDefault)
                current.Parent.Bindings.Add(Expression.Bind(current.FromMember, readerValueExpr));
            else current.Parent.Arguments.Add(readerValueExpr);
            current = current.Parent;
        }

        var resultLabelExpr = Expression.Label(entityType);
        Expression returnExpr = null;
        if (current.IsDefault) returnExpr = Expression.MemberInit(Expression.New(current.Constructor), current.Bindings);
        else returnExpr = Expression.New(current.Constructor, current.Arguments);

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
    private static Delegate CreateReaderValueConverter(Type targetType, Type fieldType)
    {
        var blockBodies = new List<Expression>();
        var readerExpr = Expression.Parameter(typeof(IDataReader), "reader");
        var indexExpr = Expression.Parameter(typeof(int), "index");
        var resultLabelExpr = Expression.Label(fieldType);
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
