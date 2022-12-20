using Microsoft.Extensions.Primitives;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley;

class Update<TEntity> : IUpdate<TEntity>
{
    private static ConcurrentDictionary<int, object> commandInitializerCache = new();
    private readonly IOrmDbFactory dbFactory;
    private readonly TheaConnection connection;
    private readonly IDbTransaction transaction;
    private EntityMap entityMapper = null;
    private QueryVisitor visitor = null;

    private bool isByKey = false;
    private object updateObj = null;
    private int bulkCount = 500;
    private IQuery<TEntity> query = null;
    private List<IDbDataParameter> dbParameters = null;
    private StringBuilder sqlBuilder = null;
    private string fromSql = null;


    public Update(IOrmDbFactory dbFactory, TheaConnection connection, IDbTransaction transaction)
    {
        this.dbFactory = dbFactory;
        this.connection = connection;
        this.transaction = transaction;
        this.entityMapper = dbFactory.GetEntityMap(typeof(TEntity));
    }
    public IUpdate<TEntity> RawSql(string rawSql)
    {
        if (this.sqlBuilder.Length > 0)
            this.sqlBuilder.Append(' ');
        this.sqlBuilder.Append(rawSql);
        return this;
    }
    public IUpdate<TEntity> SetByKey<TUpdateObject>(TUpdateObject updateObj, int bulkCount = 500)
    {
        if (updateObj == null)
            throw new ArgumentNullException(nameof(updateObj));
        this.isByKey = true;
        this.bulkCount = bulkCount;
        return this;
    }
    public IUpdate<TEntity> Set<TMember>(Expression<Func<TEntity, TMember>> fieldExpr, TMember fieldValue = default)
    {
        if (fieldExpr == null)
            throw new ArgumentNullException(nameof(fieldExpr));

        if (fieldExpr.NodeType != ExpressionType.MemberAccess || fieldExpr.NodeType != ExpressionType.New || fieldExpr.NodeType != ExpressionType.MemberInit)
            throw new Exception($"表达式{nameof(fieldExpr)},只支持MemberAccess和New、MemberInit三种类型");
        var ormProvider = this.connection.OrmProvider;
        var nextExpr = fieldExpr.Body;
        if (this.sqlBuilder == null)
            this.sqlBuilder = new StringBuilder();
        if (this.dbParameters == null)
            this.dbParameters = new List<IDbDataParameter>();
        MemberMap memberMapper = null;
        string parameterName = null;

        switch (nextExpr.NodeType)
        {
            case ExpressionType.MemberAccess:
                if (fieldValue == null)
                    throw new ArgumentNullException(nameof(fieldValue));

                var memberExpr = nextExpr as MemberExpression;
                memberMapper = this.entityMapper.GetMemberMap(memberExpr.Member.Name);
                parameterName = ormProvider.ParameterPrefix + memberMapper.MemberName;
                if (this.sqlBuilder.Length > 0)
                    this.sqlBuilder.Append(',');
                this.sqlBuilder.Append($"{ormProvider.GetFieldName(memberMapper.FieldName)}={parameterName}");
                this.dbParameters.Add(ormProvider.CreateParameter(parameterName, fieldValue));
                break;
            case ExpressionType.New:
                var newExpr = nextExpr as NewExpression;
                var visitor = new UpdateVisitor(this.dbFactory, ormProvider, this.entityMapper.EntityType);
                for (int i = 0; i < newExpr.Arguments.Count; i++)
                {
                    var memberInfo = newExpr.Members[i];
                    memberMapper = this.entityMapper.GetMemberMap(memberInfo.Name);
                    if (memberMapper == null) continue;
                    var sqlSegment = visitor.Visit(new SqlSegment { Expression = newExpr.Arguments[i] });
                    if (this.sqlBuilder.Length > 0)
                        this.sqlBuilder.Append(',');
                    parameterName = ormProvider.ParameterPrefix + memberMapper.MemberName;
                    this.sqlBuilder.Append($"{ormProvider.GetFieldName(memberMapper.FieldName)}={parameterName}");
                    this.dbParameters.Add(ormProvider.CreateParameter(parameterName, sqlSegment.Value));
                }
                break;
            case ExpressionType.MemberInit:
                //var newExpr = nextExpr as NewExpression;
                //var visitor = new QueryVisitor(this.dbFactory, ormProvider);
                //for (int i = 0; i < newExpr.Arguments.Count; i++)
                //{
                //    var memberInfo = newExpr.Members[i];
                //    memberMapper = this.entityMapper.GetMemberMap(memberInfo.Name);
                //    if (memberMapper == null) continue;
                //    var sqlSegment = visitor.Visit(new SqlSegment { Expression = newExpr.Arguments[i] });
                //    if (this.sqlBuilder.Length > 0)
                //        this.sqlBuilder.Append(',');
                //    parameterName = ormProvider.ParameterPrefix + memberMapper.MemberName;
                //    this.sqlBuilder.Append($"{ormProvider.GetFieldName(memberMapper.FieldName)}={parameterName}");
                //    this.dbParameters.Add(ormProvider.CreateParameter(parameterName, sqlSegment.Value));
                //}
                break;
        }
        return this;
    }

    /// <summary>
    /// postgresql 使用from
    /// update sys_order a set "TotalAmount"=50
    /// from sys_user b,sys_company c
    /// where a."BuyerId"=b."Id" and b."CompanyId"=c."Id" and c."Id"=1;
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="joinOn"></param>
    /// <returns></returns>
    public IUpdate<TEntity, T> From<T>(Expression<Func<TEntity, T, bool>> joinOn)
    {
        this.visitor = new QueryVisitor(this.dbFactory, this.connection.OrmProvider);
        this.visitor.From(typeof(TEntity), typeof(T)).Where(joinOn);
        var ormProvider = this.connection.OrmProvider;
        var fromType = typeof(T);
        var fromEntityMapper = this.dbFactory.GetEntityMap(fromType);
        this.fromSql = $"{ormProvider.GetTableName(fromEntityMapper.TableName)} WHERE {whereSql}";
        return new Update<TEntity, T>(this.dbFactory, this.connection, this.transaction, this.entityMapper, this.visitor);
    }
    /// <summary>
    /// postgresql 使用from，多表关联
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <param name="joinOn"></param>
    /// <returns></returns>
    public IUpdate<TEntity, T1, T2> From<T1, T2>(Expression<Func<TEntity, T1, T2, bool>> joinOn)
    {
    }
    public IUpdate<TEntity, T1, T2, T3> From<T1, T2, T3>(Expression<Func<TEntity, T1, T2, T3, bool>> joinOn)
    {
    }
    public IUpdate<TEntity, T1, T2, T3, T4> From<T1, T2, T3, T4>(Expression<Func<TEntity, T1, T2, T3, T4, bool>> joinOn)
    {
    }
    public IUpdate<TEntity, T1, T2, T3, T4, T5> From<T1, T2, T3, T4, T5>(Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> joinOn)
    {
    }

    public IUpdate<TEntity, T> InnerJoin<T>(Expression<Func<TEntity, T, bool>> joinOn)
    {
    }
    public IUpdate<TEntity, T1, T2> InnerJoin<T1, T2>(Expression<Func<TEntity, T1, T2, bool>> joinOn)
    {
    }
    public IUpdate<TEntity, T1, T2, T3> InnerJoin<T1, T2, T3>(Expression<Func<TEntity, T1, T2, T3, bool>> joinOn)
    {
    }
    public IUpdate<TEntity, T1, T2, T3, T4> InnerJoin<T1, T2, T3, T4>(Expression<Func<TEntity, T1, T2, T3, T4, bool>> joinOn)
    {
    }
    public IUpdate<TEntity, T1, T2, T3, T4, T5> InnerJoin<T1, T2, T3, T4, T5>(Expression<Func<TEntity, T1, T2, T3, T4, T5, bool>> joinOn)
    {
    }
    public IUpdate<TEntity> UpdateFrom<TUpdateObject>(Func<TEntity, IFromQuery, IQuery<TUpdateObject>> subQuery)
    {
    }
    public IUpdate<TEntity> Where(Expression<Func<TEntity, bool>> predicate)
    {
    }
    public IUpdate<TEntity> Where(bool condition, Expression<Func<TEntity, bool>> predicate)
    {
    }

    public int Execute()
    {
        if (this.isByKey)
        {
            bool isMulti = false;
            bool isDictionary = false;
            Type updateObjType = null;
            IEnumerable entities = null;

            if (this.updateObj is Dictionary<string, object> dict)
                isDictionary = true;
            else if (updateObj is IEnumerable)
            {
                isMulti = true;
                entities = updateObj as IEnumerable;
                foreach (var entity in entities)
                {
                    if (entity is Dictionary<string, object>)
                        isDictionary = true;
                    else updateObjType = entity.GetType();
                    break;
                }
            }
            else updateObjType = typeof(TEntity);

            if (isMulti)
            {
                Action<IDbCommand, IOrmProvider, StringBuilder, int, object> commandInitializer = null;
                if (isDictionary) commandInitializer = this.BuildBatchCommandInitializer(this.entityMapper.EntityType);
                else commandInitializer = this.BuildBatchCommandInitializer(this.entityMapper.EntityType, updateObjType);

                int result = 0, index = 0;
                var sqlBuilder = new StringBuilder();
                var command = this.connection.CreateCommand();
                foreach (var entity in entities)
                {
                    commandInitializer.Invoke(command, this.connection.OrmProvider, sqlBuilder, index, entity);
                    if (index >= this.bulkCount)
                    {
                        command.CommandText = sqlBuilder.ToString();
                        command.CommandType = CommandType.Text;
                        this.connection.Open();
                        result += command.ExecuteNonQuery();
                        sqlBuilder.Clear();
                        index = 0;
                        continue;
                    }
                    index++;
                }
                if (index > 0)
                {
                    command.CommandText = sqlBuilder.ToString();
                    command.CommandType = CommandType.Text;
                    this.connection.Open();
                    result += command.ExecuteNonQuery();
                }
            }
            else
            {
                Func<IDbCommand, IOrmProvider, object, string> commandInitializer = null;
                if (isDictionary) commandInitializer = this.BuildCommandInitializer(this.entityMapper.EntityType);
                else commandInitializer = this.BuildCommandInitializer(this.entityMapper.EntityType, updateObjType);

                var command = this.connection.CreateCommand();
                command.CommandText = commandInitializer?.Invoke(command, this.connection.OrmProvider, this.parameters);
                command.CommandType = CommandType.Text;
                command.Transaction = this.transaction;

                connection.Open();
                return command.ExecuteNonQuery();
            }
        }
        else
        {

        }



    }
    public Task<int> ExecuteAsync(CancellationToken cancellationToken = default)
    {
    }
    public string ToSql()
    {
    }


    private Action<IDbCommand, IOrmProvider, StringBuilder, int, object> BuildBatchCommandInitializer(Type entityType, Type parameterType)
    {
        var cacheKey = HashCode.Combine("UpdateBatch", connection.OrmProvider, entityType, parameterType);
        if (!commandInitializerCache.TryGetValue(cacheKey, out var commandInitializerDelegate))
        {
            int columnIndex = 0;
            var entityMapper = dbFactory.GetEntityMap(entityType);
            var parameterMapper = dbFactory.GetEntityMap(parameterType);
            var ormProvider = this.connection.OrmProvider;
            var commandExpr = Expression.Parameter(typeof(IDbCommand), "cmd");
            var ormProviderExpr = Expression.Parameter(typeof(IOrmProvider), "ormProvider");
            var builderExpr = Expression.Parameter(typeof(StringBuilder), "builder");
            var indexExpr = Expression.Parameter(typeof(int), "index");
            var parameterExpr = Expression.Parameter(typeof(object), "parameter");

            var typedParameterExpr = Expression.Variable(parameterType, "typedParameter");
            var blockParameters = new List<ParameterExpression>();
            var blockBodies = new List<Expression>();
            blockParameters.Add(typedParameterExpr);
            blockBodies.Add(Expression.Assign(typedParameterExpr, Expression.Convert(parameterExpr, parameterType)));

            var methodInfo1 = typeof(StringBuilder).GetMethod(nameof(StringBuilder.Append), new Type[] { typeof(char) });
            var methodInfo2 = typeof(StringBuilder).GetMethod(nameof(StringBuilder.Append), new Type[] { typeof(string) });
            var methodInfo3 = typeof(string).GetMethod(nameof(string.Concat), new Type[] { typeof(string), typeof(string) });

            var greatThenExpr = Expression.GreaterThan(Expression.Property(builderExpr, nameof(StringBuilder.Length)), Expression.Constant(0, typeof(int)));
            blockBodies.Add(Expression.IfThen(greatThenExpr, Expression.Call(builderExpr, methodInfo1, Expression.Constant(';'))));
            blockBodies.Add(Expression.Call(builderExpr, methodInfo2, Expression.Constant($"UPDATE {ormProvider.GetTableName(entityMapper.TableName)} SET ")));

            foreach (var parameterMemberMapper in parameterMapper.MemberMaps)
            {
                if (!entityMapper.TryGetMemberMap(parameterMemberMapper.MemberName, out var propMapper) || propMapper.IsIgnore || propMapper.IsNavigation || propMapper.IsKey)
                    continue;

                var parameterName = ormProvider.ParameterPrefix + propMapper.MemberName;
                var suffixExpr = Expression.Call(indexExpr, typeof(int).GetMethod(nameof(int.ToString), Type.EmptyTypes));
                var parameterNameExpr = Expression.Call(methodInfo3, Expression.Constant(parameterName), suffixExpr);

                //生成SQL
                if (columnIndex > 0)
                    blockBodies.Add(Expression.Call(builderExpr, methodInfo1, Expression.Constant(',')));
                blockBodies.Add(Expression.Call(builderExpr, methodInfo2, Expression.Constant(ormProvider.GetFieldName(propMapper.FieldName) + "=")));
                blockBodies.Add(Expression.Call(builderExpr, methodInfo2, parameterNameExpr));

                RepositoryHelper.AddParameter(commandExpr, ormProviderExpr, typedParameterExpr, parameterNameExpr, parameterMemberMapper.MemberName, blockBodies);
                columnIndex++;
            }
            columnIndex = 0;
            var whereBuilder = new StringBuilder(" WHERE ");
            foreach (var keyMemberMapper in entityMapper.KeyMembers)
            {
                if (!parameterMapper.TryGetMemberMap(keyMemberMapper.MemberName, out var parameterMemberMapper))
                    throw new Exception($"参数类型{parameterMapper.EntityType.FullName}，丢失{keyMemberMapper.MemberName}主键成员");

                var parameterName = ormProvider.ParameterPrefix + "k" + keyMemberMapper.MemberName;
                var suffixExpr = Expression.Call(indexExpr, typeof(int).GetMethod(nameof(int.ToString), Type.EmptyTypes));
                var parameterNameExpr = Expression.Call(methodInfo3, Expression.Constant(parameterName), suffixExpr);

                if (columnIndex > 0)
                    whereBuilder.Append(" AND ");
                whereBuilder.Append(ormProvider.GetFieldName(keyMemberMapper.FieldName) + "=");
                blockBodies.Add(Expression.Call(builderExpr, methodInfo2, Expression.Constant(whereBuilder.ToString())));
                blockBodies.Add(Expression.Call(builderExpr, methodInfo2, parameterNameExpr));

                RepositoryHelper.AddParameter(commandExpr, ormProviderExpr, typedParameterExpr, parameterNameExpr, parameterMemberMapper.MemberName, blockBodies);
                columnIndex++;
            }
            blockBodies.Add(Expression.Call(builderExpr, methodInfo2, Expression.Constant(whereBuilder.ToString())));

            commandInitializerDelegate = Expression.Lambda<Action<IDbCommand, IOrmProvider, StringBuilder, int, object>>(Expression.Block(blockParameters, blockBodies), commandExpr, ormProviderExpr, parameterExpr).Compile();
            commandInitializerCache.TryAdd(cacheKey, commandInitializerDelegate);
        }
        return (Action<IDbCommand, IOrmProvider, StringBuilder, int, object>)commandInitializerDelegate;
    }
    private Func<IDbCommand, IOrmProvider, object, string> BuildCommandInitializer(Type entityType, Type parameterType)
    {
        var cacheKey = HashCode.Combine("Update", connection.OrmProvider, entityType, parameterType);
        if (!commandInitializerCache.TryGetValue(cacheKey, out var commandInitializerDelegate))
        {
            int columnIndex = 0;
            var entityMapper = dbFactory.GetEntityMap(entityType);
            var parameterMapper = dbFactory.GetEntityMap(parameterType);
            var ormProvider = this.connection.OrmProvider;
            var commandExpr = Expression.Parameter(typeof(IDbCommand), "cmd");
            var ormProviderExpr = Expression.Parameter(typeof(IOrmProvider), "ormProvider");
            var parameterExpr = Expression.Parameter(typeof(object), "parameter");
            var typedParameterExpr = Expression.Parameter(parameterType, "typedParameter");

            var blockParameters = new List<ParameterExpression>();
            var blockBodies = new List<Expression>();
            blockParameters.Add(typedParameterExpr);
            blockBodies.Add(Expression.Assign(typedParameterExpr, Expression.Convert(parameterExpr, parameterType)));

            var methodInfo1 = typeof(StringBuilder).GetMethod(nameof(StringBuilder.Append), new Type[] { typeof(char) });
            var methodInfo2 = typeof(StringBuilder).GetMethod(nameof(StringBuilder.Append), new Type[] { typeof(string) });
            var methodInfo3 = typeof(string).GetMethod(nameof(string.Concat), new Type[] { typeof(string), typeof(string) });

            var sqlBuilder = new StringBuilder($"UPDATE {ormProvider.GetTableName(entityMapper.TableName)} SET ");
            foreach (var parameterMemberMapper in parameterMapper.MemberMaps)
            {
                if (!entityMapper.TryGetMemberMap(parameterMemberMapper.MemberName, out var propMapper) || propMapper.IsIgnore || propMapper.IsNavigation || propMapper.IsKey)
                    continue;

                //生成SQL
                var parameterName = ormProvider.ParameterPrefix + propMapper.MemberName;
                if (columnIndex > 0)
                    sqlBuilder.Append(',');
                sqlBuilder.Append($"{ormProvider.GetFieldName(propMapper.FieldName)}={parameterName}");
                var parameterNameExpr = Expression.Constant(parameterName);
                RepositoryHelper.AddParameter(commandExpr, ormProviderExpr, typedParameterExpr, parameterNameExpr, parameterMemberMapper.MemberName, blockBodies);
                columnIndex++;
            }
            columnIndex = 0;
            sqlBuilder.Append(" WHERE ");
            foreach (var keyMemberMapper in entityMapper.KeyMembers)
            {
                if (!parameterMapper.TryGetMemberMap(keyMemberMapper.MemberName, out var parameterMemberMapper))
                    throw new Exception($"参数类型{parameterMapper.EntityType.FullName}，丢失{keyMemberMapper.MemberName}主键成员");

                var parameterName = ormProvider.ParameterPrefix + "k" + keyMemberMapper.MemberName;
                var parameterNameExpr = Expression.Constant(parameterName);

                if (columnIndex > 0)
                    sqlBuilder.Append(" AND ");
                sqlBuilder.Append($"{ormProvider.GetFieldName(keyMemberMapper.FieldName)}={parameterName}");
                RepositoryHelper.AddParameter(commandExpr, ormProviderExpr, typedParameterExpr, parameterNameExpr, parameterMemberMapper.MemberName, blockBodies);
                columnIndex++;
            }
            var resultLabelExpr = Expression.Label(typeof(string));
            var returnExpr = Expression.Constant(sqlBuilder.ToString());
            blockBodies.Add(Expression.Return(resultLabelExpr, returnExpr));
            blockBodies.Add(Expression.Label(resultLabelExpr, Expression.Constant(null, typeof(string))));

            commandInitializerDelegate = Expression.Lambda<Func<IDbCommand, IOrmProvider, object, string>>(Expression.Block(blockParameters, blockBodies), commandExpr, ormProviderExpr, parameterExpr).Compile();
            commandInitializerCache.TryAdd(cacheKey, commandInitializerDelegate);
        }
        return (Func<IDbCommand, IOrmProvider, object, string>)commandInitializerDelegate;
    }
    private Action<IDbCommand, IOrmProvider, StringBuilder, int, object> BuildBatchCommandInitializer(Type entityType)
    {
        return (command, ormProvider, builder, index, parameter) =>
        {
            int updateIndex = 0, whereIndex = 0;
            var dict = parameter as Dictionary<string, object>;
            var entityMapper = dbFactory.GetEntityMap(entityType);
            var updateBuilder = new StringBuilder($"UPDATE {ormProvider.GetTableName(entityMapper.TableName)} SET ");
            var whereBuilder = new StringBuilder(" WHERE ");

            foreach (var item in dict)
            {
                if (!entityMapper.TryGetMemberMap(item.Key, out var propMapper))
                    continue;

                string parameterName = null;
                StringBuilder sqlBuilder = null;
                if (propMapper.IsKey)
                {
                    parameterName = ormProvider.ParameterPrefix + "k" + item.Key + index.ToString();
                    sqlBuilder = whereBuilder;
                    if (whereIndex > 0)
                        sqlBuilder.Append(',');
                    whereIndex++;
                }
                else
                {
                    parameterName = ormProvider.ParameterPrefix + item.Key + index.ToString();
                    sqlBuilder = updateBuilder;
                    if (updateIndex > 0)
                        sqlBuilder.Append(',');
                    updateIndex++;
                }
                var dbParameter = ormProvider.CreateParameter(parameterName, dict[item.Key]);
                sqlBuilder.Append($"{ormProvider.GetFieldName(propMapper.FieldName)}={parameterName}");
                command.Parameters.Add(dbParameter);
            }
            updateBuilder.Append(whereBuilder);
            if (builder.Length > 0)
                builder.Append(';');
            builder.Append(updateBuilder);
        };
    }
    private Func<IDbCommand, IOrmProvider, object, string> BuildCommandInitializer(Type entityType)
    {
        return (command, ormProvider, parameter) =>
        {
            int index = 0;
            var dict = parameter as Dictionary<string, object>;
            var entityMapper = dbFactory.GetEntityMap(entityType);
            var insertBuilder = new StringBuilder($"INSERT INTO {ormProvider.GetTableName(entityMapper.TableName)} (");
            var valuesBuilder = new StringBuilder(" VALUES(");
            foreach (var item in dict)
            {
                if (!entityMapper.TryGetMemberMap(item.Key, out var propMapper) || !propMapper.IsKey)
                    continue;

                var parameterName = ormProvider.ParameterPrefix + item.Key;
                var dbParameter = ormProvider.CreateParameter(parameterName, dict[item.Key]);
                if (index > 0)
                {
                    insertBuilder.Append(',');
                    valuesBuilder.Append(',');
                }
                insertBuilder.Append(ormProvider.GetFieldName(propMapper.FieldName));
                valuesBuilder.Append(parameterName);
                command.Parameters.Add(dbParameter);
                index++;
            }
            insertBuilder.Append(')');
            valuesBuilder.Append(')');
            if (entityMapper.IsAutoIncrement)
                valuesBuilder.AppendFormat(connection.OrmProvider.SelectIdentitySql, entityMapper.AutoIncrementField);
            return insertBuilder.ToString() + valuesBuilder.ToString();
        };
    }
}
class Update<TEntity, T> : IUpdate<TEntity, T>
{
    private readonly IOrmDbFactory dbFactory;
    private readonly IOrmProvider ormProvider;
    private EntityMap entityMapper = null;
    private UpdateVisitor visitor = null;

    public Update(IOrmDbFactory dbFactory, IOrmProvider ormProvider, EntityMap entityMapper, UpdateVisitor visitor)
    {
        this.dbFactory = dbFactory;
        this.ormProvider = ormProvider;
        this.entityMapper = entityMapper;
        this.visitor = visitor;
    }
    public IUpdate<TEntity> Set<TSetObject>(Expression<Func<TEntity, T, TSetObject>> setExpr)
    {
        this.visitor.Set(setExpr.Body);
        return this;
    }
}
class Update<TEntity, T1, T2>
{
    public IUpdate<TEntity> Set<TSetObject>(Expression<Func<TEntity, T1, T2, TSetObject>> setExpr)
    {
    }
}
class Update<TEntity, T1, T2, T3>
{
    public IUpdate<TEntity> Set<TSetObject>(Expression<Func<TEntity, T1, T2, T3, TSetObject>> setExpr)
    {
    }
}
class Update<TEntity, T1, T2, T3, T4>
{
    public IUpdate<TEntity> Set<TSetObject>(Expression<Func<TEntity, T1, T2, T3, T4, TSetObject>> setExpr)
    {
    }
}

class Update<TEntity, T1, T2, T3, T4, T5>
{
    public IUpdate<TEntity> Set<TSetObject>(Expression<Func<TEntity, T1, T2, T3, T4, T5, TSetObject>> setExpr)
    {
    }
}

