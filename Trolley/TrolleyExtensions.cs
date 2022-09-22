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
    private static readonly ConcurrentDictionary<int, Func<IDataReader, object>> readerCache = new();

    public static string GetQuotedValue(this IOrmProvider ormProvider, object value)
    {
        if (value == null) return "null";
        return ormProvider.GetQuotedValue(value.GetType(), value);
    }
    public static EntityMap GetEntityMap(this IOrmDbFactory dbFactory, Type entityType)
    {
        if (!dbFactory.TryGetEntityMap(entityType, out var mapper))
            mapper = EntityMap.CreateDefaultMap(entityType);

        return mapper;
    }
    public static void AddEntityMap<TEntity>(this IOrmDbFactory dbFactory)
    {
        var entityType = typeof(TEntity);
        dbFactory.AddEntityMap(entityType, new EntityMap(entityType));
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

    public static TEntity To<TEntity>(this IDataReader reader, IOrmDbFactory dbFactory, TheaConnection connection)
    {
        var entityType = typeof(TEntity);
        var entityMapper = dbFactory.GetEntityMap(entityType);
        var func = GetReader(connection, reader, entityMapper);
        return (TEntity)func.Invoke(reader);
    }
    public static TEntity To<TEntity>(this IDataReader reader, TheaConnection connection, List<ReaderFieldInfo> readerFields)
    {
        var entityType = typeof(TEntity);
        var func = GetReader(connection, reader, entityType, readerFields);
        return (TEntity)func.Invoke(reader);
    }
    private static Func<IDataReader, object> GetReader(TheaConnection connection, IDataReader reader, EntityMap entityMapper)
    {
        var cacheKey = GetReaderKey(entityMapper.EntityType, connection, reader);
        if (!readerCache.TryGetValue(cacheKey, out var readerFunc))
        {
            var blockParameters = new List<ParameterExpression>();
            var blockBodies = new List<Expression>();
            var readerExpr = Expression.Parameter(typeof(IDataReader), "reader");
            var resultLabelExpr = Expression.Label(typeof(object));
            Expression returnExpr = null;

            var ctor = entityMapper.EntityType.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
            if (ctor != null)
            {
                var entityExpr = Expression.Variable(entityMapper.EntityType, "entity");
                blockParameters.Add(entityExpr);
                blockBodies.Add(Expression.Assign(entityExpr, Expression.New(ctor)));

                for (int i = 0; i < reader.FieldCount; i++)
                {
                    var fieldName = reader.GetName(i);
                    var memberMapper = entityMapper.GetMemberMap(fieldName);
                    if (memberMapper.IsIgnore || (memberMapper.Member is PropertyInfo propertyInfo && propertyInfo.SetMethod == null))
                        continue;

                    var valueExpr = GetReaderValue(reader, readerExpr, i, fieldName, memberMapper, blockParameters, blockBodies);
                    blockBodies.Add(Expression.Assign(Expression.PropertyOrField(entityExpr, memberMapper.MemberName), valueExpr));
                }
                returnExpr = entityExpr;
            }
            else
            {
                ctor = entityMapper.EntityType.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).OrderBy(f => f.IsPublic ? 0 : (f.IsPrivate ? 2 : 1)).First();
                var valuesExprs = new List<Expression>();
                var ctorParameters = ctor.GetParameters();

                for (int i = 0; i < ctorParameters.Length; i++)
                {
                    var fieldName = reader.GetName(i);
                    var memberMapper = entityMapper.GetMemberMap(fieldName);
                    if (memberMapper.IsIgnore || (memberMapper.Member is PropertyInfo propertyInfo && propertyInfo.SetMethod == null))
                        continue;
                    var valueExpr = GetReaderValue(reader, readerExpr, i, fieldName, memberMapper, blockParameters, blockBodies);
                    valuesExprs.Add(valueExpr);
                }
                returnExpr = Expression.New(ctor, valuesExprs);
            }

            blockBodies.Add(Expression.Return(resultLabelExpr, Expression.Convert(returnExpr, typeof(object))));
            blockBodies.Add(Expression.Label(resultLabelExpr, Expression.Constant(null, typeof(object))));

            readerFunc = Expression.Lambda<Func<IDataReader, object>>(Expression.Block(blockParameters, blockBodies), readerExpr).Compile();
            readerCache.TryAdd(cacheKey, readerFunc);
        }
        return readerFunc;
    }
    private static Func<IDataReader, object> GetReader(TheaConnection connection, IDataReader reader, Type entityType, List<ReaderFieldInfo> readerFields)
    {
        var cacheKey = GetReaderKey(entityType, connection, reader);
        if (!readerCache.TryGetValue(cacheKey, out var readerFunc))
        {
            var blockParameters = new List<ParameterExpression>();
            var blockBodies = new List<Expression>();
            var readerExpr = Expression.Parameter(typeof(IDataReader), "reader");

            bool isDefaultCtor = false;
            NewExpression entityExpr = null;
            List<MemberBinding> bindings = null;
            List<Expression> ctorArguments = null;

            var entityExprs = new Dictionary<string, EntityBuildInfo>();
            var deferredBuildInfos = new Stack<EntityBuildInfo>();
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

            var rootBuildInfo = new EntityBuildInfo
            {
                IsDefault = isDefaultCtor,
                Constructor = ctor,
                Bindings = bindings,
                Arguments = ctorArguments
            };

            int index = 0;
            ReaderFieldInfo lastReaderInfo = null;

            while (index < reader.FieldCount)
            {
                var fieldName = reader.GetName(index);
                var readerFieldInfo = readerFields[index];
                if (isDefaultCtor && readerFieldInfo.Member is PropertyInfo propertyInfo && propertyInfo.SetMethod == null)
                {
                    lastReaderInfo = readerFieldInfo;
                    index++;
                    continue;
                }
                if (readerFieldInfo.IsTarget)
                {
                    var readerValueExpr = GetReaderValue(reader, readerExpr, index, fieldName, readerFieldInfo.Member, blockParameters, blockBodies);
                    if (isDefaultCtor) bindings.Add(Expression.Bind(readerFieldInfo.Member, readerValueExpr));
                    else ctorArguments.Add(readerValueExpr);

                    lastReaderInfo = readerFieldInfo;
                    index++;
                    continue;
                }
                //不相等说明是一个新实体
                if (readerFieldInfo.Member != lastReaderInfo.Member)
                {
                    //第一次不相等直接赋值，再次不相等就是更换实体了
                    bool isChildDefaultCtor = false;
                    List<MemberBinding> childBindings = null;
                    List<Expression> childCtorArguments = null;

                    lastReaderInfo = readerFieldInfo;
                    var childEntityType = readerFieldInfo.Member.GetMemberType();
                    var childCtor = childEntityType.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
                    if (childCtor != null)
                    {
                        childBindings = new List<MemberBinding>();
                        isChildDefaultCtor = true;
                    }
                    else
                    {
                        childCtor = childEntityType.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).OrderBy(f => f.IsPublic ? 0 : (f.IsPrivate ? 2 : 1)).First();
                        childCtorArguments = new List<Expression>();
                    }
                    while (true)
                    {
                        if (index >= reader.FieldCount) break;
                        readerFieldInfo = readerFields[index];

                        if (readerFieldInfo.Member != lastReaderInfo.Member) break;
                        var childMember = readerFieldInfo.RefMapper.GetMemberMap(readerFieldInfo.MemberName);
                        var readerValueExpr = GetReaderValue(reader, readerExpr, index, fieldName, childMember, blockParameters, blockBodies);
                        if (isChildDefaultCtor) childBindings.Add(Expression.Bind(childMember.Member, readerValueExpr));
                        else childCtorArguments.Add(readerValueExpr);
                        index++;
                    }
                    var readerMemberExpr = lastReaderInfo.Expression as MemberExpression;
                    EntityBuildInfo parentInfo = null;
                    if (readerMemberExpr.Expression.NodeType == ExpressionType.Parameter)
                        parentInfo = rootBuildInfo;
                    else
                    {
                        var parentPath = readerMemberExpr.Expression.ToString();
                        if (!entityExprs.TryGetValue(parentPath, out parentInfo))
                            throw new Exception($"{readerMemberExpr}未找到到父亲实体的Expression");
                    }

                    Expression childExpr = null;
                    if (isChildDefaultCtor)
                        childExpr = Expression.MemberInit(Expression.New(childCtor), childBindings);
                    else childExpr = Expression.New(childCtor, childCtorArguments);

                    var currentPath = lastReaderInfo.Expression.ToString();
                    var childBuildInfo = new EntityBuildInfo
                    {
                        IsDefault = isChildDefaultCtor,
                        Constructor = childCtor,
                        Bindings = childBindings,
                        Arguments = childCtorArguments,
                        Parent = parentInfo
                    };
                    entityExprs.TryAdd(currentPath, childBuildInfo);

                    if (parentInfo.IsDefault)
                    {
                        childBuildInfo.ParentIndex = parentInfo.Bindings.Count;
                        parentInfo.Bindings.Add(Expression.Bind(lastReaderInfo.Member, childExpr));
                    }
                    else
                    {
                        childBuildInfo.ParentIndex = parentInfo.Arguments.Count;
                        parentInfo.Arguments.Add(childExpr);
                    }
                    if (parentInfo.Parent != null)
                        deferredBuildInfos.Push(parentInfo);
                }
            }
            while (deferredBuildInfos.TryPop(out var buildInfo))
            {
                EntityBuildInfo current = buildInfo;
                while (current != null)
                {
                    if (current.Parent == null) break;
                    Expression instanceExpr = null;
                    if (buildInfo.IsDefault)
                        instanceExpr = Expression.MemberInit(Expression.New(current.Constructor), current.Bindings);
                    else instanceExpr = Expression.New(current.Constructor, current.Arguments);

                    if (current.Parent.IsDefault)
                    {
                        var parentBinding = current.Parent.Bindings[current.ParentIndex];
                        parentBinding = Expression.Bind(parentBinding.Member, instanceExpr);
                    }
                    else current.Parent.Arguments[current.ParentIndex] = instanceExpr;
                    current = buildInfo.Parent;
                }
            }

            var resultLabelExpr = Expression.Label(typeof(object));
            Expression returnExpr = null;
            if (isDefaultCtor) returnExpr = Expression.MemberInit(Expression.New(ctor), bindings);
            else returnExpr = Expression.New(ctor, ctorArguments);

            blockBodies.Add(Expression.Return(resultLabelExpr, Expression.Convert(returnExpr, typeof(object))));
            blockBodies.Add(Expression.Label(resultLabelExpr, Expression.Constant(null, typeof(object))));

            readerFunc = Expression.Lambda<Func<IDataReader, object>>(Expression.Block(blockParameters, blockBodies), readerExpr).Compile();
            readerCache.TryAdd(cacheKey, readerFunc);
        }
        return readerFunc;
    }
    private static Expression GetReaderValue(IDataReader reader, ParameterExpression readerExpr, int index, string fieldName,
        MemberMap memberMapper, List<ParameterExpression> blockParameters, List<Expression> blockBodies)
    {
        MethodInfo methodInfo = null;
        Expression typedValueExpr = null;

        var readerType = reader.GetFieldType(index);
        //reader.GetValue(index);
        methodInfo = typeof(IDataRecord).GetMethod("GetValue", BindingFlags.Public | BindingFlags.Instance, new Type[] { typeof(int) });
        var readerValueExpr = Expression.Call(readerExpr, methodInfo, Expression.Constant(index, typeof(int)));

        //null或default(int)
        Expression defaultValueExpr = Expression.Default(memberMapper.MemberType);

        if (memberMapper.MemberType.IsAssignableFrom(readerType))
            typedValueExpr = Expression.Convert(readerValueExpr, memberMapper.MemberType);
        else if (memberMapper.UnderlyingType == typeof(Guid))
        {
            if (readerType != typeof(string) && readerType != typeof(byte[]))
                throw new Exception($"数据库字段{fieldName}，无法初始化{memberMapper.Parent.EntityType.FullName}类型的Guid类型{memberMapper.MemberName}成员");

            typedValueExpr = Expression.New(typeof(Guid).GetConstructor(new Type[] { readerType }), Expression.Convert(readerValueExpr, readerType));
            if (!memberMapper.IsNullable) defaultValueExpr = Expression.Constant(Guid.Empty, typeof(Guid));
        }
        else
        {
            //else propValue=Convert.ToInt32(reader[index]);
            var typeCode = Type.GetTypeCode(memberMapper.UnderlyingType);
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

            methodInfo = typeof(Convert).GetMethod(toTypeMethod, new Type[] { typeof(object), typeof(IFormatProvider) });
            typedValueExpr = Expression.Call(methodInfo, readerValueExpr, Expression.Constant(CultureInfo.CurrentCulture));
            if (memberMapper.IsEnum)
            {
                methodInfo = typeof(Enum).GetMethod(nameof(Enum.ToObject), new Type[] { typeof(Type), memberMapper.UnderlyingType });
                var toEnumExpr = Expression.Call(methodInfo, Expression.Constant(memberMapper.EnumUnderlyingType), typedValueExpr);
                typedValueExpr = Expression.Convert(toEnumExpr, memberMapper.EnumUnderlyingType);
            }
            if (memberMapper.IsNullable) typedValueExpr = Expression.Convert(typedValueExpr, memberMapper.MemberType);
        }

        //if(localValue is DBNull)
        var isNullExpr = Expression.TypeIs(readerValueExpr, typeof(DBNull));
        return Expression.Condition(isNullExpr, defaultValueExpr, typedValueExpr);
    }
    private static Expression GetReaderValue(IDataReader reader, ParameterExpression readerExpr, int index, string fieldName,
        MemberInfo member, List<ParameterExpression> blockParameters, List<Expression> blockBodies)
    {
        MethodInfo methodInfo = null;
        Expression typedValueExpr = null;

        var readerType = reader.GetFieldType(index);
        //reader.GetValue(index);
        methodInfo = typeof(IDataRecord).GetMethod("GetValue", BindingFlags.Public | BindingFlags.Instance, new Type[] { typeof(int) });
        var readerValueExpr = Expression.Call(readerExpr, methodInfo, Expression.Constant(index, typeof(int)));

        //null或default(int)
        var entityType = member.DeclaringType;
        var memberType = member.GetMemberType();

        var underlyingType = Nullable.GetUnderlyingType(memberType);
        bool isNullable = underlyingType != null;
        if (!isNullable) underlyingType = memberType;

        Expression defaultValueExpr = Expression.Default(memberType);

        if (memberType.IsAssignableFrom(readerType))
            typedValueExpr = Expression.Convert(readerValueExpr, memberType);
        else if (underlyingType == typeof(Guid))
        {
            if (readerType != typeof(string) && readerType != typeof(byte[]))
                throw new Exception($"数据库字段{fieldName}，无法初始化{entityType.FullName}类型的Guid类型{member.Name}成员");

            typedValueExpr = Expression.New(typeof(Guid).GetConstructor(new Type[] { readerType }), Expression.Convert(readerValueExpr, readerType));
            if (!isNullable) defaultValueExpr = Expression.Constant(Guid.Empty, typeof(Guid));
        }
        else
        {
            //else propValue=Convert.ToInt32(reader[index]);
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

            methodInfo = typeof(Convert).GetMethod(toTypeMethod, new Type[] { typeof(object), typeof(IFormatProvider) });
            typedValueExpr = Expression.Call(methodInfo, readerValueExpr, Expression.Constant(CultureInfo.CurrentCulture));
            if (underlyingType.IsEnum)
            {
                var enumUnderlyingType = underlyingType.GetEnumUnderlyingType();
                methodInfo = typeof(Enum).GetMethod(nameof(Enum.ToObject), new Type[] { typeof(Type), underlyingType });
                var toEnumExpr = Expression.Call(methodInfo, Expression.Constant(enumUnderlyingType), typedValueExpr);
                typedValueExpr = Expression.Convert(toEnumExpr, enumUnderlyingType);
            }
            if (isNullable) typedValueExpr = Expression.Convert(typedValueExpr, memberType);
        }

        //if(localValue is DBNull)
        var isNullExpr = Expression.TypeIs(readerValueExpr, typeof(DBNull));
        return Expression.Condition(isNullExpr, defaultValueExpr, typedValueExpr);
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
    private static string GetRootPath(Expression expr)
    {
        var parentExpr = expr;
        while (parentExpr.NodeType != ExpressionType.Parameter)
        {
            if (parentExpr is MemberExpression memberExpr)
                parentExpr = memberExpr.Expression;
        }
        return parentExpr.ToString();
    }

    class EntityBuildInfo
    {
        public bool IsDefault { get; set; }
        public ConstructorInfo Constructor { get; set; }
        public List<MemberBinding> Bindings { get; set; }
        public List<Expression> Arguments { get; set; }
        public EntityBuildInfo Parent { get; set; }
        public int ParentIndex { get; set; }
    }
}
