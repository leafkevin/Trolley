using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Trolley;

class RepositoryHelper
{
    public static ConcurrentDictionary<int, string> querySqlCache = new();
    public static ConcurrentDictionary<int, object> queryCommandInitializerCache = new();
    public static ConcurrentDictionary<int, object> queryRawSqlCommandInitializerCache = new();

    public static ConcurrentDictionary<int, object> createRawSqlCommandInitializerCache = new();
    public static ConcurrentDictionary<int, object> createBatchCommandInitializerCache = new();
    public static ConcurrentDictionary<int, object> createCommandInitializerCache = new();
    public static ConcurrentDictionary<int, object> createWithBiesCommandInitializerCache = new();

    public static void AddKeyValueParameter(ParameterExpression commandExpr, ParameterExpression ormProviderExpr, Expression parameterNameExpr,
        Expression parameterValueExpr, MemberMap memberMapper, IOrmProvider ormProvider, List<Expression> blockBodies)
    {
        //var parameter = ormProvider.CreateParameter("@Parameter", value);
        //if(value==null)objLocal=DBNull.Value;
        //else objLocal=value;
        //var parameter = ormProvider.CreateParameter("@Parameter", objLocal);
        Expression valueExpr = parameterValueExpr;
        Expression memberMapperExpr = null;
        MethodInfo methodInfo = null;

        Expression dbParameterExpr = null;
        if (memberMapper != null)
        {
            methodInfo = typeof(IOrmProvider).GetMethod(nameof(IOrmProvider.ToFieldValue), new Type[] { typeof(MemberMap), typeof(object) });
            memberMapperExpr = Expression.Constant(memberMapper);
            valueExpr = Expression.Call(ormProviderExpr, methodInfo, memberMapperExpr, valueExpr);

            if (memberMapper.NativeDbType != null)
            {
                //var dbParameter = ormProvider.CreateParameter("@Name", SqlDbType.VarChar, "kevin");
                methodInfo = typeof(IOrmProvider).GetMethod(nameof(IOrmProvider.CreateParameter), new Type[] { typeof(string), typeof(object), typeof(object) });
                var nativeDbTypeExpr = Expression.Property(memberMapperExpr, nameof(MemberMap.NativeDbType));
                dbParameterExpr = Expression.Call(ormProviderExpr, methodInfo, parameterNameExpr, nativeDbTypeExpr, valueExpr);
            }
            else
            {
                //var dbParameter = ormProvider.CreateParameter("@Name", "kevin");
                methodInfo = typeof(IOrmProvider).GetMethod(nameof(IOrmProvider.CreateParameter), new Type[] { typeof(string), typeof(object) });
                valueExpr = Expression.Convert(valueExpr, typeof(object));
                dbParameterExpr = Expression.Call(ormProviderExpr, methodInfo, parameterNameExpr, valueExpr);
            }
            dbParameterExpr = Expression.Convert(dbParameterExpr, typeof(object));
        }
        //command.Parameters.Add(parameter);
        var propertyInfo = typeof(IDbCommand).GetProperty(nameof(IDbCommand.Parameters));
        var parametersExpr = Expression.MakeMemberAccess(commandExpr, propertyInfo);
        methodInfo = typeof(IList).GetMethod(nameof(IDbCommand.Parameters.Add));
        var addParameterExpr = Expression.Call(parametersExpr, methodInfo, dbParameterExpr);
        blockBodies.Add(addParameterExpr);
    }
    public static void AddKeyMemberParameter(ParameterExpression commandExpr, ParameterExpression ormProviderExpr, Expression parameterNameExpr,
        Expression typedParameterExpr, MemberMap memberMapper, IOrmProvider ormProvider, List<Expression> blockBodies)
    {
        //var parameter = ormProvider.CreateParameter("@Parameter", value);
        Expression valueExpr = Expression.PropertyOrField(typedParameterExpr, memberMapper.MemberName);
        Expression memberMapperExpr = null;
        MethodInfo methodInfo = null;

        Expression dbParameterExpr = null;
        if (memberMapper != null)
        {
            //var objValue = ormProvider.ToFieldValue(memberMapper, "kevin");
            methodInfo = typeof(IOrmProvider).GetMethod(nameof(IOrmProvider.ToFieldValue), new Type[] { typeof(MemberMap), typeof(object) });
            memberMapperExpr = Expression.Constant(memberMapper);
            if (valueExpr.Type != typeof(object))
                valueExpr = Expression.Convert(valueExpr, typeof(object));
            valueExpr = Expression.Call(ormProviderExpr, methodInfo, memberMapperExpr, valueExpr);

            if (memberMapper.NativeDbType != null)
            {
                //var dbParameter = ormProvider.CreateParameter("@Name", SqlDbType.VarChar, objValue);
                methodInfo = typeof(IOrmProvider).GetMethod(nameof(IOrmProvider.CreateParameter), new Type[] { typeof(string), typeof(object), typeof(object) });
                var nativeDbTypeExpr = Expression.Property(memberMapperExpr, nameof(MemberMap.NativeDbType));
                dbParameterExpr = Expression.Call(ormProviderExpr, methodInfo, parameterNameExpr, nativeDbTypeExpr, valueExpr);
            }
            else
            {
                //var dbParameter = ormProvider.CreateParameter("@Name", objValue);
                methodInfo = typeof(IOrmProvider).GetMethod(nameof(IOrmProvider.CreateParameter), new Type[] { typeof(string), typeof(object) });
                valueExpr = Expression.Convert(valueExpr, typeof(object));
                dbParameterExpr = Expression.Call(ormProviderExpr, methodInfo, parameterNameExpr, valueExpr);
            }
            dbParameterExpr = Expression.Convert(dbParameterExpr, typeof(object));
        }
        //command.Parameters.Add(parameter);
        var propertyInfo = typeof(IDbCommand).GetProperty(nameof(IDbCommand.Parameters));
        var parametersExpr = Expression.MakeMemberAccess(commandExpr, propertyInfo);
        methodInfo = typeof(IList).GetMethod(nameof(IDbCommand.Parameters.Add));
        var addParameterExpr = Expression.Call(parametersExpr, methodInfo, dbParameterExpr);
        blockBodies.Add(addParameterExpr);
    }
    public static void AddMemberParameter(ParameterExpression commandExpr, ParameterExpression ormProviderExpr, Expression parameterNameExpr,
        Expression typedParameterExpr, MemberMap memberMapper, IOrmProvider ormProvider, List<Expression> blockBodies)
    {
        //var parameter = ormProvider.CreateParameter("@Parameter", value);
        //if(value==null)objLocal=DBNull.Value;
        //else objLocal=value;
        //var parameter = ormProvider.CreateParameter("@Parameter", objLocal);
        var memberValueExpr = Expression.PropertyOrField(typedParameterExpr, memberMapper.MemberName);
        Expression valueExpr = memberValueExpr;
        Expression memberMapperExpr = null;
        MethodInfo methodInfo = null;

        Expression dbParameterExpr = null;
        if (memberMapper != null)
        {
            methodInfo = typeof(IOrmProvider).GetMethod(nameof(IOrmProvider.ToFieldValue), new Type[] { typeof(MemberMap), typeof(object) });
            memberMapperExpr = Expression.Constant(memberMapper);
            if (valueExpr.Type != typeof(object))
                valueExpr = Expression.Convert(valueExpr, typeof(object));
            valueExpr = Expression.Call(ormProviderExpr, methodInfo, memberMapperExpr, valueExpr);

            if (memberMapper.NativeDbType != null)
            {
                //var dbParameter = ormProvider.CreateParameter("@Name", SqlDbType.VarChar, "kevin");
                methodInfo = typeof(IOrmProvider).GetMethod(nameof(IOrmProvider.CreateParameter), new Type[] { typeof(string), typeof(object), typeof(object) });
                var nativeDbTypeExpr = Expression.Property(memberMapperExpr, nameof(MemberMap.NativeDbType));
                dbParameterExpr = Expression.Call(ormProviderExpr, methodInfo, parameterNameExpr, nativeDbTypeExpr, valueExpr);
            }
            else
            {
                //var dbParameter = ormProvider.CreateParameter("@Name", "kevin");
                methodInfo = typeof(IOrmProvider).GetMethod(nameof(IOrmProvider.CreateParameter), new Type[] { typeof(string), typeof(object) });
                valueExpr = Expression.Convert(valueExpr, typeof(object));
                dbParameterExpr = Expression.Call(ormProviderExpr, methodInfo, parameterNameExpr, valueExpr);
            }
            dbParameterExpr = Expression.Convert(dbParameterExpr, typeof(object));
        }
        //command.Parameters.Add(parameter);
        var propertyInfo = typeof(IDbCommand).GetProperty(nameof(IDbCommand.Parameters));
        var parametersExpr = Expression.MakeMemberAccess(commandExpr, propertyInfo);
        methodInfo = typeof(IList).GetMethod(nameof(IDbCommand.Parameters.Add));
        var addParameterExpr = Expression.Call(parametersExpr, methodInfo, dbParameterExpr);
        blockBodies.Add(addParameterExpr);
    }
    public static void AddMemberParameter(ParameterExpression commandExpr, ParameterExpression ormProviderExpr, Expression parameterNameExpr, Expression typedParameterExpr, bool isNullable,
        MemberMap memberMapper, IOrmProvider ormProvider, Dictionary<string, int> localParameters, List<ParameterExpression> blockParameters, List<Expression> blockBodies)
    {
        //var parameter = ormProvider.CreateParameter("@Parameter", value);
        //if(value==null)objLocal=DBNull.Value;
        //else objLocal=value;
        //var parameter = ormProvider.CreateParameter("@Parameter", objLocal);
        var memberValueExpr = Expression.PropertyOrField(typedParameterExpr, memberMapper.MemberName);
        Expression valueExpr = memberValueExpr;
        Expression memberMapperExpr = null;
        MethodInfo methodInfo = null;

        Expression dbParameterExpr = null;
        if (memberMapper != null)
        {
            if (memberMapper.TypeHandler != null)
            {
                dbParameterExpr = DefineLocalParameter("dbParameter", typeof(IDbDataParameter), localParameters, blockParameters);
                if (valueExpr.Type != typeof(object))
                    valueExpr = Expression.Convert(valueExpr, typeof(object));
                if (memberMapper.NativeDbType != null)
                {
                    //var dbParameter = ormProvider.CreateParameter("@Name", SqlDbType.VarChar, value);
                    var nativeDbTypeExpr = Expression.Convert(Expression.Constant(memberMapper.NativeDbType), typeof(object));
                    methodInfo = typeof(IOrmProvider).GetMethod(nameof(IOrmProvider.CreateParameter), new Type[] { typeof(string), typeof(object), typeof(object) });
                    var newParameterExpr = Expression.Call(ormProviderExpr, methodInfo, parameterNameExpr, nativeDbTypeExpr, valueExpr);
                    blockBodies.Add(Expression.Assign(dbParameterExpr, newParameterExpr));
                }
                else
                {
                    //var dbParameter = ormProvider.CreateParameter("@Name", value);
                    methodInfo = typeof(IOrmProvider).GetMethod(nameof(IOrmProvider.CreateParameter), new Type[] { typeof(string), typeof(object) });
                    var newParameterExpr = Expression.Call(ormProviderExpr, methodInfo, parameterNameExpr, valueExpr);
                    blockBodies.Add(Expression.Assign(dbParameterExpr, newParameterExpr));
                }

                //typeHandler.SetValue(ormProvider, dbParameter, value);
                var typeHandlerExpr = Expression.Constant(memberMapper.TypeHandler);
                methodInfo = typeof(ITypeHandler).GetMethod(nameof(ITypeHandler.SetValue), new Type[] { typeof(IOrmProvider), typeof(IDbDataParameter), typeof(object) });
                Expression assignValueExpr = Expression.Call(typeHandlerExpr, methodInfo, ormProviderExpr, dbParameterExpr, valueExpr);

                //INSERT场景
                if (isNullable && memberMapper.MemberType.IsNullableType(out _)
                    || memberMapper.MemberType == typeof(string) || memberMapper.MemberType.IsEntityType())
                {
                    //var dbParameter = ormProvider.CreateParameter("@Name", SqlDbType.VarChar, value);
                    //if(gender == null)
                    //  dbParameter.Value = DBNull.Value;
                    //else typeHandler.SetValue(ormProvider, dbParameter, value);
                    var isNullExpr = Expression.Equal(valueExpr, Expression.Constant(null));
                    methodInfo = typeof(IDataParameter).GetProperty(nameof(IDataParameter.Value)).GetSetMethod();
                    var assignNullExpr = Expression.Call(dbParameterExpr, methodInfo, Expression.Constant(DBNull.Value));
                    assignValueExpr = Expression.IfThenElse(isNullExpr, assignNullExpr, assignValueExpr);
                }
                blockBodies.Add(assignValueExpr);
            }
            else
            {
                methodInfo = typeof(IOrmProvider).GetMethod(nameof(IOrmProvider.ToFieldValue), new Type[] { typeof(MemberMap), typeof(object) });
                memberMapperExpr = Expression.Constant(memberMapper);
                if (valueExpr.Type != typeof(object))
                    valueExpr = Expression.Convert(valueExpr, typeof(object));
                valueExpr = Expression.Call(ormProviderExpr, methodInfo, memberMapperExpr, valueExpr);

                //INSERT场景                
                if (isNullable && memberMapper.MemberType.IsNullableType(out _)
                    || memberMapper.MemberType == typeof(string) || memberMapper.MemberType.IsEntityType())
                {
                    //object localValue;
                    //if(gender == null)
                    //  localValue = DBNull.Value;
                    //else localValue = (object)gender.Value;
                    var isNullExpr = Expression.Equal(valueExpr, Expression.Constant(null));
                    var objLocalExpr = DefineLocalParameter("objLocal", typeof(object), localParameters, blockParameters);
                    var assignNullExpr = Expression.Assign(objLocalExpr, Expression.Constant(DBNull.Value));
                    var assignValueExpr = Expression.Assign(objLocalExpr, valueExpr);
                    blockBodies.Add(Expression.IfThenElse(isNullExpr, assignNullExpr, assignValueExpr));
                    valueExpr = objLocalExpr;
                }
                if (memberMapper.NativeDbType != null)
                {
                    //var dbParameter = ormProvider.CreateParameter("@Name", SqlDbType.VarChar, "kevin");
                    methodInfo = typeof(IOrmProvider).GetMethod(nameof(IOrmProvider.CreateParameter), new Type[] { typeof(string), typeof(object), typeof(object) });
                    var nativeDbTypeExpr = Expression.Property(memberMapperExpr, nameof(MemberMap.NativeDbType));
                    dbParameterExpr = Expression.Call(ormProviderExpr, methodInfo, parameterNameExpr, nativeDbTypeExpr, valueExpr);
                }
                else
                {
                    //var dbParameter = ormProvider.CreateParameter("@Name", "kevin");
                    methodInfo = typeof(IOrmProvider).GetMethod(nameof(IOrmProvider.CreateParameter), new Type[] { typeof(string), typeof(object) });
                    valueExpr = Expression.Convert(valueExpr, typeof(object));
                    dbParameterExpr = Expression.Call(ormProviderExpr, methodInfo, parameterNameExpr, valueExpr);
                }
            }
            dbParameterExpr = Expression.Convert(dbParameterExpr, typeof(object));
        }
        //command.Parameters.Add(parameter);
        var propertyInfo = typeof(IDbCommand).GetProperty(nameof(IDbCommand.Parameters));
        var parametersExpr = Expression.MakeMemberAccess(commandExpr, propertyInfo);
        methodInfo = typeof(IList).GetMethod(nameof(IDbCommand.Parameters.Add));
        var addParameterExpr = Expression.Call(parametersExpr, methodInfo, dbParameterExpr);
        blockBodies.Add(addParameterExpr);
    }

    public static string BuildGetSql(IDbConnection connection, IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType)
    {
        var sqlCacheKey = HashCode.Combine("Get", connection, entityType, entityType);
        if (!querySqlCache.TryGetValue(sqlCacheKey, out var sql))
        {
            var index = 0;
            var entityMapper = mapProvider.GetEntityMap(entityType);

            var builder = new StringBuilder("SELECT ");
            foreach (var propMapper in entityMapper.MemberMaps)
            {
                if (propMapper.IsIgnore || propMapper.IsNavigation
                    || (propMapper.MemberType.IsEntityType() && propMapper.TypeHandler == null))
                    continue;

                if (index > 0) builder.Append(',');
                builder.Append(ormProvider.GetFieldName(propMapper.FieldName));
                if (propMapper.FieldName != propMapper.MemberName)
                    builder.Append(" AS " + ormProvider.GetFieldName(propMapper.MemberName));
                index++;
            }
            builder.Append($" FROM {ormProvider.GetTableName(entityMapper.TableName)} WHERE ");
            index = 0;
            foreach (var keyMapper in entityMapper.KeyMembers)
            {
                if (index > 0)
                    builder.Append(" AND ");
                var parameterName = ormProvider.ParameterPrefix + keyMapper.MemberName;
                builder.Append($"{ormProvider.GetFieldName(keyMapper.FieldName)}={parameterName}");
                index++;
            }
            sql = builder.ToString();
            querySqlCache.TryAdd(sqlCacheKey, sql);
        }
        return sql;
    }
    public static Action<IDbCommand, IOrmProvider, object> BuildGetParameters(IDbConnection connection, IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType, object whereObj)
    {
        Action<IDbCommand, IOrmProvider, object> commandInitializer = null;
        if (whereObj is Dictionary<string, object>)
        {
            commandInitializer = (command, ormProvider, whereObj) =>
            {
                var dict = whereObj as Dictionary<string, object>;
                var entityMapper = mapProvider.GetEntityMap(entityType);
                foreach (var item in dict)
                {
                    if (!entityMapper.TryGetMemberMap(item.Key, out var propMapper) || !propMapper.IsKey)
                        continue;

                    var parameterName = ormProvider.ParameterPrefix + item.Key;
                    if (propMapper.NativeDbType != null)
                        command.Parameters.Add(ormProvider.CreateParameter(parameterName, propMapper.NativeDbType, item.Value));
                    else command.Parameters.Add(ormProvider.CreateParameter(parameterName, item.Value));
                }
            };
        }
        else
        {
            var whereObjType = whereObj.GetType();
            var cacheKey = HashCode.Combine("Get", connection, entityType, whereObjType);
            if (!queryCommandInitializerCache.TryGetValue(cacheKey, out var commandInitializerDelegate))
            {
                var entityMapper = mapProvider.GetEntityMap(entityType);
                var whereObjMapper = mapProvider.GetEntityMap(whereObjType);
                var commandExpr = Expression.Parameter(typeof(IDbCommand), "cmd");
                var ormProviderExpr = Expression.Parameter(typeof(IOrmProvider), "ormProvider");
                var whereObjExpr = Expression.Parameter(typeof(object), "whereObj");
                ParameterExpression typedWhereObjExpr = null;

                var blockParameters = new List<ParameterExpression>();
                var blockBodies = new List<Expression>();
                bool isEntityType = false;

                if (whereObjType.IsEntityType())
                {
                    isEntityType = true;
                    typedWhereObjExpr = Expression.Variable(whereObjType, "typedWhereObj");
                    blockParameters.Add(typedWhereObjExpr);
                    blockBodies.Add(Expression.Assign(typedWhereObjExpr, Expression.Convert(whereObjExpr, whereObjType)));
                }
                else
                {
                    if (entityMapper.KeyMembers.Count > 1)
                        throw new NotSupportedException($"模型{entityType.FullName}有多个主键字段，不能使用单个值类型{whereObjType.FullName}作为参数");
                }

                var index = 0;
                foreach (var keyMapper in entityMapper.KeyMembers)
                {
                    if (isEntityType && !whereObjMapper.TryGetMemberMap(keyMapper.MemberName, out var whereObjPropMapper))
                        throw new ArgumentNullException($"参数类型{whereObjType.FullName}缺少主键字段{keyMapper.MemberName}", "whereObj");

                    var parameterName = $"{ormProvider.ParameterPrefix}{keyMapper.MemberName}";
                    var parameterNameExpr = Expression.Constant(parameterName, typeof(string));

                    if (isEntityType)
                        RepositoryHelper.AddKeyMemberParameter(commandExpr, ormProviderExpr, parameterNameExpr, typedWhereObjExpr, keyMapper, ormProvider, blockBodies);
                    else RepositoryHelper.AddKeyValueParameter(commandExpr, ormProviderExpr, parameterNameExpr, whereObjExpr, keyMapper, ormProvider, blockBodies);
                    index++;
                }
                commandInitializerDelegate = Expression.Lambda<Action<IDbCommand, IOrmProvider, object>>(Expression.Block(blockParameters, blockBodies), commandExpr, ormProviderExpr, whereObjExpr).Compile();
                queryCommandInitializerCache.TryAdd(cacheKey, commandInitializerDelegate);
            }
            commandInitializer = (Action<IDbCommand, IOrmProvider, object>)commandInitializerDelegate;
        }
        return commandInitializer;
    }
    public static string BuildQuerySqlPart(IDbConnection connection, IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType)
    {
        var sqlCacheKey = HashCode.Combine("Query", connection, entityType, Type.EmptyTypes);
        if (!querySqlCache.TryGetValue(sqlCacheKey, out var sql))
        {
            var index = 0;
            var entityMapper = mapProvider.GetEntityMap(entityType);
            var builder = new StringBuilder("SELECT ");
            foreach (var propMapper in entityMapper.MemberMaps)
            {
                if (propMapper.IsIgnore || propMapper.IsNavigation
                    || (propMapper.MemberType.IsEntityType() && propMapper.TypeHandler == null))
                    continue;

                if (index > 0) builder.Append(',');
                builder.Append(ormProvider.GetFieldName(propMapper.FieldName));
                if (propMapper.FieldName != propMapper.MemberName)
                    builder.Append(" AS " + ormProvider.GetFieldName(propMapper.MemberName));
                index++;
            }
            builder.Append($" FROM {ormProvider.GetTableName(entityMapper.TableName)}");
            sql = builder.ToString();
            querySqlCache.TryAdd(sqlCacheKey, sql);
        }
        return sql;
    }
    public static Func<IDbCommand, IOrmProvider, string, object, string> BuildQuerySqlParameters(IDbConnection connection, IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType, object whereObj)
    {
        Func<IDbCommand, IOrmProvider, string, object, string> commandInitializer = null;
        if (whereObj is Dictionary<string, object> dict)
        {
            commandInitializer = (command, ormProvider, sql, whereObj) =>
            {
                var dict = whereObj as Dictionary<string, object>;
                var entityMapper = mapProvider.GetEntityMap(entityType);
                var builder = new StringBuilder(" WHERE ");
                int index = 0;
                foreach (var item in dict)
                {
                    if (!entityMapper.TryGetMemberMap(item.Key, out var memberMapper)
                        || memberMapper.IsIgnore || memberMapper.IsNavigation
                        || (memberMapper.MemberType.IsEntityType() && memberMapper.TypeHandler == null))
                        continue;

                    var parameterName = ormProvider.ParameterPrefix + item.Key;
                    if (index > 0) builder.Append(" AND ");
                    builder.Append($"{ormProvider.GetFieldName(memberMapper.FieldName)}={parameterName}");

                    if (memberMapper.NativeDbType != null)
                        command.Parameters.Add(ormProvider.CreateParameter(parameterName, memberMapper.NativeDbType, item.Value));
                    else command.Parameters.Add(ormProvider.CreateParameter(parameterName, item.Value));
                    index++;
                }
                builder.Insert(0, sql);
                return builder.ToString();
            };
        }
        else
        {
            var whereObjType = whereObj.GetType();
            var cacheKey = HashCode.Combine("Query", connection, entityType, whereObjType);
            if (!queryCommandInitializerCache.TryGetValue(cacheKey, out var commandInitializerDelegate))
            {
                var entityMapper = mapProvider.GetEntityMap(entityType);
                var whereObjMapper = mapProvider.GetEntityMap(whereObjType);
                var commandExpr = Expression.Parameter(typeof(IDbCommand), "cmd");
                var ormProviderExpr = Expression.Parameter(typeof(IOrmProvider), "ormProvider");
                var sqlExpr = Expression.Parameter(typeof(string), "sql");
                var whereObjExpr = Expression.Parameter(typeof(object), "whereObj");

                var typedWhereObjExpr = Expression.Variable(whereObjType, "typedWhereObj");
                var blockParameters = new List<ParameterExpression>();
                var blockBodies = new List<Expression>();
                blockParameters.Add(typedWhereObjExpr);
                blockBodies.Add(Expression.Assign(typedWhereObjExpr, Expression.Convert(whereObjExpr, whereObjType)));

                var index = 0;
                var builder = new StringBuilder(" WHERE ");
                foreach (var whereObjPropMapper in whereObjMapper.MemberMaps)
                {
                    if (!entityMapper.TryGetMemberMap(whereObjPropMapper.MemberName, out var propMapper)
                        || propMapper.IsIgnore || propMapper.IsNavigation
                        || (propMapper.MemberType.IsEntityType() && propMapper.TypeHandler == null))
                        continue;

                    var parameterName = ormProvider.ParameterPrefix + propMapper.MemberName;
                    var parameterNameExpr = Expression.Constant(parameterName, typeof(string));
                    RepositoryHelper.AddMemberParameter(commandExpr, ormProviderExpr, parameterNameExpr, typedWhereObjExpr, propMapper, ormProvider, blockBodies);
                    if (index > 0) builder.Append(" AND ");
                    builder.Append($"{ormProvider.GetFieldName(propMapper.FieldName)}={parameterName}");
                    index++;
                }
                var methodInfo = typeof(string).GetMethod(nameof(string.Concat), new Type[] { typeof(string), typeof(string) });
                var returnExpr = Expression.Call(methodInfo, sqlExpr, Expression.Constant(builder.ToString(), typeof(string)));

                var resultLabelExpr = Expression.Label(typeof(string));
                blockBodies.Add(Expression.Return(resultLabelExpr, returnExpr));
                blockBodies.Add(Expression.Label(resultLabelExpr, Expression.Constant(null, typeof(string))));

                commandInitializerDelegate = Expression.Lambda<Func<IDbCommand, IOrmProvider, string, object, string>>(Expression.Block(blockParameters, blockBodies), commandExpr, ormProviderExpr, sqlExpr, whereObjExpr).Compile();
                queryCommandInitializerCache.TryAdd(cacheKey, commandInitializerDelegate);
            }
            commandInitializer = (Func<IDbCommand, IOrmProvider, string, object, string>)commandInitializerDelegate;
        }
        return commandInitializer;
    }
    public static Action<IDbCommand, IOrmProvider, object> BuildQueryRawSqlParameters(IDbConnection connection, IOrmProvider ormProvider, IEntityMapProvider mapProvider, string rawSql, object parameters)
    {
        Action<IDbCommand, IOrmProvider, object> commandInitializer = null;
        if (parameters != null)
        {
            if (parameters is Dictionary<string, object>)
            {
                commandInitializer = (command, ormProvider, parameter) =>
                {
                    var dict = parameter as Dictionary<string, object>;
                    foreach (var item in dict)
                    {
                        var parameterName = ormProvider.ParameterPrefix + item.Key;
                        if (!Regex.IsMatch(rawSql, parameterName + @"([^\p{L}\p{N}_]+|$)", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.CultureInvariant))
                            continue;
                        var dbParameter = ormProvider.CreateParameter(parameterName, dict[item.Key]);
                        command.Parameters.Add(dbParameter);
                    }
                };
            }
            else
            {
                var parameterType = parameters.GetType();
                var cacheKey = HashCode.Combine("QueryRaw", connection, rawSql, parameterType);
                if (!queryRawSqlCommandInitializerCache.TryGetValue(cacheKey, out var commandInitializerDelegate))
                {
                    var parameterMapper = mapProvider.GetEntityMap(parameterType);
                    var commandExpr = Expression.Parameter(typeof(IDbCommand), "cmd");
                    var ormProviderExpr = Expression.Parameter(typeof(IOrmProvider), "ormProvider");
                    var parameterExpr = Expression.Parameter(typeof(object), "parameter");

                    var typedParameterExpr = Expression.Variable(parameterType, "typedParameter");
                    var blockParameters = new List<ParameterExpression>();
                    var blockBodies = new List<Expression>();
                    blockParameters.Add(typedParameterExpr);
                    blockBodies.Add(Expression.Assign(typedParameterExpr, Expression.Convert(parameterExpr, parameterType)));

                    foreach (var memberMapper in parameterMapper.MemberMaps)
                    {
                        var parameterName = ormProvider.ParameterPrefix + memberMapper.MemberName;
                        if (!Regex.IsMatch(rawSql, parameterName + @"([^\p{L}\p{N}_]+|$)", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.CultureInvariant))
                            continue;
                        var parameterNameExpr = Expression.Constant(parameterName);
                        AddMemberParameter(commandExpr, ormProviderExpr, parameterNameExpr, typedParameterExpr, memberMapper, ormProvider, blockBodies);
                    }
                    commandInitializerDelegate = Expression.Lambda<Action<IDbCommand, IOrmProvider, object>>(Expression.Block(blockParameters, blockBodies), commandExpr, ormProviderExpr, parameterExpr).Compile();
                    queryRawSqlCommandInitializerCache.TryAdd(cacheKey, commandInitializerDelegate);
                }
                commandInitializer = commandInitializerDelegate as Action<IDbCommand, IOrmProvider, object>;
            }
        }
        return commandInitializer;
    }
    public static Func<IDbCommand, IOrmProvider, object, string> BuildExistsSqlParameters(IDbConnection connection, IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType, object whereObj)
    {
        Func<IDbCommand, IOrmProvider, object, string> commandInitializer = null;
        var whereObjType = whereObj.GetType();
        if (whereObj is Dictionary<string, object> dict)
        {
            commandInitializer = (command, ormProvider, whereObj) =>
            {
                var dict = whereObj as Dictionary<string, object>;
                var entityMapper = mapProvider.GetEntityMap(whereObjType);
                var builder = new StringBuilder($"SELECT COUNT(1) FROM {ormProvider.GetTableName(entityMapper.TableName)} WHERE ");
                int index = 0;
                foreach (var item in dict)
                {
                    if (!entityMapper.TryGetMemberMap(item.Key, out var propMapper)
                        || propMapper.IsIgnore || propMapper.IsNavigation
                        || (propMapper.MemberType.IsEntityType() && propMapper.TypeHandler == null))
                        continue;

                    var parameterName = ormProvider.ParameterPrefix + item.Key;
                    if (index > 0) builder.Append(" AND ");
                    builder.Append($"{ormProvider.GetFieldName(propMapper.FieldName)}={parameterName}");

                    if (propMapper.NativeDbType != null)
                        command.Parameters.Add(ormProvider.CreateParameter(parameterName, propMapper.NativeDbType, item.Value));
                    else command.Parameters.Add(ormProvider.CreateParameter(parameterName, item.Value));
                    index++;
                }
                return builder.ToString();
            };
        }
        else
        {
            var cacheKey = HashCode.Combine("Exists", connection, entityType, whereObjType);
            if (!queryCommandInitializerCache.TryGetValue(cacheKey, out var commandInitializerDelegate))
            {
                var entityMapper = mapProvider.GetEntityMap(entityType);
                var whereObjMapper = mapProvider.GetEntityMap(whereObjType);
                var commandExpr = Expression.Parameter(typeof(IDbCommand), "cmd");
                var ormProviderExpr = Expression.Parameter(typeof(IOrmProvider), "ormProvider");
                var whereObjExpr = Expression.Parameter(typeof(object), "whereObj");

                var typedWhereObjExpr = Expression.Variable(whereObjType, "typedWhereObj");
                var blockParameters = new List<ParameterExpression>();
                var blockBodies = new List<Expression>();
                blockParameters.Add(typedWhereObjExpr);
                blockBodies.Add(Expression.Assign(typedWhereObjExpr, Expression.Convert(whereObjExpr, whereObjType)));

                var index = 0;
                var builder = new StringBuilder($"SELECT COUNT(1) FROM {ormProvider.GetTableName(entityMapper.TableName)} WHERE ");
                foreach (var whereObjPropMapper in whereObjMapper.MemberMaps)
                {
                    if (!entityMapper.TryGetMemberMap(whereObjPropMapper.MemberName, out var propMapper)
                        || propMapper.IsIgnore || propMapper.IsNavigation
                        || (propMapper.MemberType.IsEntityType() && propMapper.TypeHandler == null))
                        continue;

                    var parameterName = ormProvider.ParameterPrefix + propMapper.MemberName;
                    var parameterNameExpr = Expression.Constant(parameterName, typeof(string));
                    RepositoryHelper.AddMemberParameter(commandExpr, ormProviderExpr, parameterNameExpr, typedWhereObjExpr, propMapper, ormProvider, blockBodies);
                    if (index > 0) builder.Append(" AND ");
                    builder.Append($"{ormProvider.GetFieldName(propMapper.FieldName)}={parameterName}");
                    index++;
                }
                if (index == 0)
                    throw new Exception($"{whereObjType.FullName}类中没有任何可以查询的字段");

                var returnExpr = Expression.Constant(builder.ToString());
                var resultLabelExpr = Expression.Label(typeof(string));
                blockBodies.Add(Expression.Return(resultLabelExpr, returnExpr));
                blockBodies.Add(Expression.Label(resultLabelExpr, Expression.Constant(null, typeof(string))));

                commandInitializerDelegate = Expression.Lambda<Func<IDbCommand, IOrmProvider, object, string>>(Expression.Block(blockParameters, blockBodies), commandExpr, ormProviderExpr, whereObjExpr).Compile();
                queryCommandInitializerCache.TryAdd(cacheKey, commandInitializerDelegate);
            }
            commandInitializer = (Func<IDbCommand, IOrmProvider, object, string>)commandInitializerDelegate;
        }
        return commandInitializer;
    }


    public static Action<IDbCommand, IOrmProvider, object> BuildCreateRawSqlParameters(IDbConnection connection, IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType, string rawSql, object parameters)
    {
        Action<IDbCommand, IOrmProvider, object> commandInitializer = null;
        if (parameters is Dictionary<string, object>)
        {
            commandInitializer = (command, ormProvider, parameter) =>
            {
                var dict = parameter as Dictionary<string, object>;
                foreach (var item in dict)
                {
                    var parameterName = ormProvider.ParameterPrefix + item.Key;
                    if (!Regex.IsMatch(rawSql, parameterName + @"([^\p{L}\p{N}_]+|$)", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.CultureInvariant))
                        continue;
                    var dbParameter = ormProvider.CreateParameter(parameterName, dict[item.Key]);
                    command.Parameters.Add(dbParameter);
                }
            };
        }
        else
        {
            var parameterType = parameters.GetType();
            var cacheKey = HashCode.Combine(connection, rawSql, entityType, parameterType);
            if (!createRawSqlCommandInitializerCache.TryGetValue(cacheKey, out var commandInitializerDelegate))
            {
                var parameterMapper = mapProvider.GetEntityMap(parameterType);
                var commandExpr = Expression.Parameter(typeof(IDbCommand), "cmd");
                var ormProviderExpr = Expression.Parameter(typeof(IOrmProvider), "ormProvider");
                var parameterExpr = Expression.Parameter(typeof(object), "parameter");

                var typedParameterExpr = Expression.Variable(parameterType, "typedParameter");
                var blockParameters = new List<ParameterExpression>();
                var blockBodies = new List<Expression>();
                var localParameters = new Dictionary<string, int>();
                blockParameters.Add(typedParameterExpr);
                blockBodies.Add(Expression.Assign(typedParameterExpr, Expression.Convert(parameterExpr, parameterType)));

                foreach (var memberMapper in parameterMapper.MemberMaps)
                {
                    var parameterName = ormProvider.ParameterPrefix + memberMapper.MemberName;
                    if (!Regex.IsMatch(rawSql, parameterName + @"([^\p{L}\p{N}_]+|$)", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.CultureInvariant))
                        continue;
                    var parameterNameExpr = Expression.Constant(parameterName);
                    //明确的SQL，传入的参数有可能为NULL,为NULL则插入NULL值
                    AddMemberParameter(commandExpr, ormProviderExpr, parameterNameExpr, typedParameterExpr, true, memberMapper, ormProvider, localParameters, blockParameters, blockBodies);
                }
                commandInitializerDelegate = Expression.Lambda<Action<IDbCommand, IOrmProvider, object>>(Expression.Block(blockParameters, blockBodies), commandExpr, ormProviderExpr, parameterExpr).Compile();
                createRawSqlCommandInitializerCache.TryAdd(cacheKey, commandInitializerDelegate);
            }
            commandInitializer = commandInitializerDelegate as Action<IDbCommand, IOrmProvider, object>;
        }
        return commandInitializer;
    }
    public static Action<IDbCommand, IOrmProvider, StringBuilder, int, object> BuildCreateBatchCommandInitializer(IDbConnection connection, IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType, object parameters)
    {
        Action<IDbCommand, IOrmProvider, StringBuilder, int, object> commandInitializer = null;
        if (parameters is Dictionary<string, object>)
        {
            commandInitializer = (command, ormProvider, builder, index, parameter) =>
            {
                int columnIndex = 0;
                var dict = parameter as Dictionary<string, object>;
                var entityMapper = mapProvider.GetEntityMap(entityType);
                if (index == 0)
                {
                    builder.Append($"INSERT INTO {ormProvider.GetTableName(entityMapper.TableName)} (");
                    foreach (var item in dict)
                    {
                        if (!entityMapper.TryGetMemberMap(item.Key, out var propMapper)
                            || propMapper.IsIgnore || propMapper.IsNavigation || propMapper.IsAutoIncrement
                            || (propMapper.MemberType.IsEntityType() && propMapper.TypeHandler == null))
                            continue;
                        if (columnIndex > 0)
                            builder.Append(',');
                        builder.Append(ormProvider.GetFieldName(propMapper.FieldName));
                        columnIndex++;
                    }
                    builder.Append(") VALUES ");
                }
                else builder.Append(',');
                columnIndex = 0;
                builder.Append('(');
                foreach (var item in dict)
                {
                    if (!entityMapper.TryGetMemberMap(item.Key, out var propMapper)
                        || propMapper.IsIgnore || propMapper.IsNavigation || propMapper.IsAutoIncrement
                        || (propMapper.MemberType.IsEntityType() && propMapper.TypeHandler == null))
                        continue;

                    if (columnIndex > 0)
                        builder.Append(',');
                    var parameterName = ormProvider.ParameterPrefix + propMapper.MemberName + index.ToString();
                    builder.Append(parameterName);

                    if (propMapper.NativeDbType != null)
                        command.Parameters.Add(ormProvider.CreateParameter(parameterName, propMapper.NativeDbType, item.Value));
                    else command.Parameters.Add(ormProvider.CreateParameter(parameterName, item.Value));
                    columnIndex++;
                }
                builder.Append(')');
            };
        }
        else
        {
            var parameterType = parameters.GetType();
            var cacheKey = HashCode.Combine(connection, entityType, parameterType);
            if (!createBatchCommandInitializerCache.TryGetValue(cacheKey, out var commandInitializerDelegate))
            {
                int columnIndex = 0;
                var entityMapper = mapProvider.GetEntityMap(entityType);
                var parameterMapper = mapProvider.GetEntityMap(parameterType);
                var commandExpr = Expression.Parameter(typeof(IDbCommand), "cmd");
                var ormProviderExpr = Expression.Parameter(typeof(IOrmProvider), "ormProvider");
                var builderExpr = Expression.Parameter(typeof(StringBuilder), "builder");
                var indexExpr = Expression.Parameter(typeof(int), "index");
                var parameterExpr = Expression.Parameter(typeof(object), "parameter");

                var typedParameterExpr = Expression.Variable(parameterType, "typedParameter");
                var blockParameters = new List<ParameterExpression>();
                var blockBodies = new List<Expression>();
                var localParameters = new Dictionary<string, int>();
                blockParameters.Add(typedParameterExpr);
                blockBodies.Add(Expression.Assign(typedParameterExpr, Expression.Convert(parameterExpr, parameterType)));

                var insertBuilder = new StringBuilder($"INSERT INTO {ormProvider.GetTableName(entityMapper.TableName)} (");
                foreach (var parameterMemberMapper in parameterMapper.MemberMaps)
                {
                    if (!entityMapper.TryGetMemberMap(parameterMemberMapper.MemberName, out var propMapper)
                        || propMapper.IsIgnore || propMapper.IsNavigation || propMapper.IsAutoIncrement
                        || (propMapper.MemberType.IsEntityType() && propMapper.TypeHandler == null))
                        continue;

                    if (columnIndex > 0)
                        insertBuilder.Append(',');
                    insertBuilder.Append(ormProvider.GetFieldName(propMapper.FieldName));
                    columnIndex++;
                }
                insertBuilder.Append(") VALUES ");

                var methodInfo1 = typeof(StringBuilder).GetMethod(nameof(StringBuilder.Append), new Type[] { typeof(char) });
                var methodInfo2 = typeof(StringBuilder).GetMethod(nameof(StringBuilder.Append), new Type[] { typeof(string) });
                var methodInfo3 = typeof(string).GetMethod(nameof(string.Concat), new Type[] { typeof(string), typeof(string) });

                var addInsertExpr = Expression.Call(builderExpr, methodInfo2, Expression.Constant(insertBuilder.ToString()));
                var addCommaExpr = Expression.Call(builderExpr, methodInfo1, Expression.Constant(','));
                var greatThenExpr = Expression.GreaterThan(indexExpr, Expression.Constant(0, typeof(int)));
                blockBodies.Add(Expression.IfThenElse(greatThenExpr, addCommaExpr, addInsertExpr));
                blockBodies.Add(Expression.Call(builderExpr, methodInfo1, Expression.Constant('(')));
                columnIndex = 0;
                foreach (var parameterMemberMapper in parameterMapper.MemberMaps)
                {
                    if (!entityMapper.TryGetMemberMap(parameterMemberMapper.MemberName, out var propMapper)
                        || propMapper.IsIgnore || propMapper.IsNavigation || propMapper.IsAutoIncrement
                        || (propMapper.MemberType.IsEntityType() && propMapper.TypeHandler == null))
                        continue;

                    if (columnIndex > 0)
                        blockBodies.Add(Expression.Call(builderExpr, methodInfo1, Expression.Constant(',')));

                    var parameterName = ormProvider.ParameterPrefix + propMapper.MemberName;
                    var suffixExpr = Expression.Call(indexExpr, typeof(int).GetMethod(nameof(int.ToString), Type.EmptyTypes));
                    var parameterNameExpr = Expression.Call(methodInfo3, Expression.Constant(parameterName), suffixExpr);
                    blockBodies.Add(Expression.Call(builderExpr, methodInfo2, parameterNameExpr));

                    RepositoryHelper.AddMemberParameter(commandExpr, ormProviderExpr, parameterNameExpr, typedParameterExpr, true, propMapper, ormProvider, localParameters, blockParameters, blockBodies);
                    columnIndex++;
                }
                blockBodies.Add(Expression.Call(builderExpr, methodInfo1, Expression.Constant(')')));

                commandInitializerDelegate = Expression.Lambda<Action<IDbCommand, IOrmProvider, StringBuilder, int, object>>(Expression.Block(blockParameters, blockBodies), commandExpr, ormProviderExpr, builderExpr, indexExpr, parameterExpr).Compile();
                createBatchCommandInitializerCache.TryAdd(cacheKey, commandInitializerDelegate);
            }
            commandInitializer = (Action<IDbCommand, IOrmProvider, StringBuilder, int, object>)commandInitializerDelegate;
        }
        return commandInitializer;
    }
    public static Func<IDbCommand, IOrmProvider, object, string> BuildCreateCommandInitializer(IDbConnection connection, IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType, object parameters)
    {
        Func<IDbCommand, IOrmProvider, object, string> commandInitializer = null;
        if (parameters is Dictionary<string, object>)
        {
            commandInitializer = (command, ormProvider, parameter) =>
            {
                int index = 0;
                var dict = parameter as Dictionary<string, object>;
                var entityMapper = mapProvider.GetEntityMap(entityType);
                var insertBuilder = new StringBuilder($"INSERT INTO {ormProvider.GetTableName(entityMapper.TableName)} (");
                var valuesBuilder = new StringBuilder(" VALUES(");
                foreach (var item in dict)
                {
                    if (!entityMapper.TryGetMemberMap(item.Key, out var propMapper)
                        || propMapper.IsIgnore || propMapper.IsNavigation || propMapper.IsAutoIncrement
                        || (propMapper.MemberType.IsEntityType() && propMapper.TypeHandler == null))
                        continue;

                    if (index > 0)
                    {
                        insertBuilder.Append(',');
                        valuesBuilder.Append(',');
                    }
                    var parameterName = ormProvider.ParameterPrefix + item.Key;
                    insertBuilder.Append(ormProvider.GetFieldName(propMapper.FieldName));
                    valuesBuilder.Append(parameterName);

                    if (propMapper.NativeDbType != null)
                        command.Parameters.Add(ormProvider.CreateParameter(parameterName, propMapper.NativeDbType, item.Value));
                    else command.Parameters.Add(ormProvider.CreateParameter(parameterName, item.Value));
                    index++;
                }
                insertBuilder.Append(')');
                valuesBuilder.Append(')');
                if (entityMapper.IsAutoIncrement)
                    valuesBuilder.AppendFormat(ormProvider.SelectIdentitySql, entityMapper.AutoIncrementField);
                return insertBuilder.ToString() + valuesBuilder.ToString();
            };
        }
        else
        {
            var parameterType = parameters.GetType();
            var cacheKey = HashCode.Combine(connection, entityType, parameterType);
            if (!createCommandInitializerCache.TryGetValue(cacheKey, out var commandInitializerDelegate))
            {
                int columnIndex = 0;
                var entityMapper = mapProvider.GetEntityMap(entityType);
                var parameterMapper = mapProvider.GetEntityMap(parameterType);
                var commandExpr = Expression.Parameter(typeof(IDbCommand), "cmd");
                var ormProviderExpr = Expression.Parameter(typeof(IOrmProvider), "ormProvider");
                var parameterExpr = Expression.Parameter(typeof(object), "parameter");

                var typedParameterExpr = Expression.Variable(parameterType, "typedParameter");
                var blockParameters = new List<ParameterExpression>();
                var blockBodies = new List<Expression>();
                var localParameters = new Dictionary<string, int>();
                blockParameters.Add(typedParameterExpr);
                blockBodies.Add(Expression.Assign(typedParameterExpr, Expression.Convert(parameterExpr, parameterType)));

                var insertBuilder = new StringBuilder($"INSERT INTO {ormProvider.GetTableName(entityMapper.TableName)} (");
                var valuesBuilder = new StringBuilder(" VALUES(");
                foreach (var parameterMemberMapper in parameterMapper.MemberMaps)
                {
                    if (!entityMapper.TryGetMemberMap(parameterMemberMapper.MemberName, out var propMapper)
                        || propMapper.IsIgnore || propMapper.IsNavigation || propMapper.IsAutoIncrement
                        || (propMapper.MemberType.IsEntityType() && propMapper.TypeHandler == null))
                        continue;

                    if (columnIndex > 0)
                    {
                        insertBuilder.Append(',');
                        valuesBuilder.Append(',');
                    }
                    insertBuilder.Append(ormProvider.GetFieldName(propMapper.FieldName));
                    var parameterName = ormProvider.ParameterPrefix + propMapper.MemberName;
                    valuesBuilder.Append(parameterName);

                    var parameterNameExpr = Expression.Constant(parameterName);
                    RepositoryHelper.AddMemberParameter(commandExpr, ormProviderExpr, parameterNameExpr, typedParameterExpr, true, propMapper, ormProvider, localParameters, blockParameters, blockBodies);
                    columnIndex++;
                }
                insertBuilder.Append(')');
                valuesBuilder.Append(')');

                if (entityMapper.IsAutoIncrement)
                    valuesBuilder.AppendFormat(ormProvider.SelectIdentitySql, entityMapper.AutoIncrementField);
                var sql = insertBuilder.ToString() + valuesBuilder.ToString();

                var returnExpr = Expression.Constant(sql, typeof(string));

                var resultLabelExpr = Expression.Label(typeof(string));
                blockBodies.Add(Expression.Return(resultLabelExpr, returnExpr));
                blockBodies.Add(Expression.Label(resultLabelExpr, Expression.Default(typeof(string))));

                commandInitializerDelegate = Expression.Lambda<Func<IDbCommand, IOrmProvider, object, string>>(Expression.Block(blockParameters, blockBodies), commandExpr, ormProviderExpr, parameterExpr).Compile();
                createCommandInitializerCache.TryAdd(cacheKey, commandInitializerDelegate);
            }
            commandInitializer = (Func<IDbCommand, IOrmProvider, object, string>)commandInitializerDelegate;
        }
        return commandInitializer;
    }
    public static Action<IDbCommand, IOrmProvider, object, StringBuilder, StringBuilder> BuildCreateWithBiesCommandInitializer(IDbConnection connection, IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType, object parameters)
    {
        Action<IDbCommand, IOrmProvider, object, StringBuilder, StringBuilder> commandInitializer = null;
        if (parameters is Dictionary<string, object>)
        {
            commandInitializer = (command, ormProvider, parameter, insertBuilder, valuesBuilder) =>
            {
                int index = 0;
                var dict = parameter as Dictionary<string, object>;
                var entityMapper = mapProvider.GetEntityMap(entityType);
                foreach (var item in dict)
                {
                    if (!entityMapper.TryGetMemberMap(item.Key, out var propMapper)
                        || propMapper.IsIgnore || propMapper.IsNavigation || propMapper.IsAutoIncrement
                        || (propMapper.MemberType.IsEntityType() && propMapper.TypeHandler == null))
                        continue;

                    if (index > 0)
                    {
                        insertBuilder.Append(',');
                        valuesBuilder.Append(',');
                    }
                    var parameterName = ormProvider.ParameterPrefix + item.Key;
                    insertBuilder.Append(ormProvider.GetFieldName(propMapper.FieldName));
                    valuesBuilder.Append(parameterName);

                    if (propMapper.NativeDbType != null)
                        command.Parameters.Add(ormProvider.CreateParameter(parameterName, propMapper.NativeDbType, item.Value));
                    else command.Parameters.Add(ormProvider.CreateParameter(parameterName, item.Value));
                    index++;
                }
            };
        }
        else
        {
            var parameterType = parameters.GetType();
            var cacheKey = HashCode.Combine(connection, ormProvider, parameterType);
            if (!parameterType.IsEntityType())
                throw new NotSupportedException("只支持类对象，不支持基础类型");

            if (!createWithBiesCommandInitializerCache.TryGetValue(cacheKey, out var commandInitializerDelegate))
            {
                int columnIndex = 0;
                var entityMapper = mapProvider.GetEntityMap(entityType);
                var parameterMapper = mapProvider.GetEntityMap(parameterType);
                var commandExpr = Expression.Parameter(typeof(IDbCommand), "cmd");
                var ormProviderExpr = Expression.Parameter(typeof(IOrmProvider), "ormProvider");
                var parameterExpr = Expression.Parameter(typeof(object), "parameter");
                var insertBuilderExpr = Expression.Parameter(typeof(StringBuilder), "insertBuilder");
                var valueBuilderExpr = Expression.Parameter(typeof(StringBuilder), "valueBuilder");

                var typedParameterExpr = Expression.Variable(parameterType, "typedParameter");
                var blockParameters = new List<ParameterExpression>();
                var blockBodies = new List<Expression>();
                var localParameters = new Dictionary<string, int>();
                blockParameters.Add(typedParameterExpr);
                blockBodies.Add(Expression.Assign(typedParameterExpr, Expression.Convert(parameterExpr, parameterType)));

                var insertBuilder = new StringBuilder();
                var valuesBuilder = new StringBuilder();
                var methodInfo1 = typeof(StringBuilder).GetMethod(nameof(StringBuilder.Append), new Type[] { typeof(char) });
                var methodInfo2 = typeof(StringBuilder).GetMethod(nameof(StringBuilder.Append), new Type[] { typeof(string) });
                foreach (var parameterMemberMapper in parameterMapper.MemberMaps)
                {
                    if (!entityMapper.TryGetMemberMap(parameterMemberMapper.MemberName, out var propMapper)
                        || propMapper.IsIgnore || propMapper.IsNavigation || propMapper.IsAutoIncrement
                        || (propMapper.MemberType.IsEntityType() && propMapper.TypeHandler == null))
                        continue;

                    if (columnIndex > 0)
                    {
                        blockBodies.Add(Expression.Call(insertBuilderExpr, methodInfo1, Expression.Constant(',')));
                        blockBodies.Add(Expression.Call(valueBuilderExpr, methodInfo1, Expression.Constant(',')));
                    }
                    var fieldName = ormProvider.GetFieldName(propMapper.FieldName);
                    var parameterName = ormProvider.ParameterPrefix + propMapper.MemberName;
                    var parameterNameExpr = Expression.Constant(parameterName);
                    blockBodies.Add(Expression.Call(insertBuilderExpr, methodInfo2, Expression.Constant(fieldName)));
                    blockBodies.Add(Expression.Call(valueBuilderExpr, methodInfo2, parameterNameExpr));

                    AddMemberParameter(commandExpr, ormProviderExpr, parameterNameExpr, typedParameterExpr, true, propMapper, ormProvider, localParameters, blockParameters, blockBodies);
                    columnIndex++;
                }
                commandInitializerDelegate = Expression.Lambda<Action<IDbCommand, IOrmProvider, object, StringBuilder, StringBuilder>>(Expression.Block(blockParameters, blockBodies), commandExpr, ormProviderExpr, parameterExpr, insertBuilderExpr, valueBuilderExpr).Compile();
                createWithBiesCommandInitializerCache.TryAdd(cacheKey, commandInitializerDelegate);
            }
            commandInitializer = (Action<IDbCommand, IOrmProvider, object, StringBuilder, StringBuilder>)commandInitializerDelegate;
        }
        return commandInitializer;
    }

    private static ParameterExpression DefineLocalParameter(string namePrefix, Type localVariableType, Dictionary<string, int> localParameters, List<ParameterExpression> blockParameters)
    {
        if (!localParameters.TryGetValue(namePrefix, out var index))
            index = 0;
        localParameters[namePrefix] = index + 1;
        var objLocalExpr = Expression.Variable(localVariableType, $"{namePrefix}{index + 1}");
        blockParameters.Add(objLocalExpr);
        return objLocalExpr;
    }
}