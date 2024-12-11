using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Trolley.Sqlite;

public class SqliteCreateVisitor : CreateVisitor, ICreateVisitor
{
    public string OrExpr { get; set; }
    public StringBuilder UpdateFields { get; set; }
    public bool IsUpdate { get; set; }
    public List<string> OutputFieldNames { get; set; }
    public SqliteCreateVisitor(DbContext dbContext, char tableAsStart = 'a')
        : base(dbContext, tableAsStart) { }

    public override string BuildCommand(IDbCommand command, bool isReturnIdentity, out List<SqlFieldSegment> readerFields)
    {
        string sql = null;
        this.IsReturnIdentity = isReturnIdentity;
        if (this.ActionMode == ActionMode.Bulk)
            sql = this.BuildWithBulkSql(command, out readerFields);
        else
        {
            this.DbParameters ??= command.Parameters;
            foreach (var deferredSegment in this.deferredSegments)
            {
                switch (deferredSegment.Type)
                {
                    case "WithBy":
                        this.VisitWithBy(deferredSegment.Value);
                        break;
                    case "WithByField":
                        this.VisitWithByField(deferredSegment.Value);
                        break;
                    case "SetObject":
                        this.UpdateFields ??= new();
                        this.VisitSetObject(deferredSegment.Value);
                        break;
                    case "SetExpression":
                        this.UpdateFields ??= new();
                        this.VisitSetExpression(deferredSegment.Value as LambdaExpression);
                        break;
                }
            }
            sql = this.BuildSql(out readerFields);
        }
        return sql;
    }
    public override string BuildSql(out List<SqlFieldSegment> readerFields)
    {
        readerFields = null;
        var tableSegment = this.Tables[0];
        var entityType = tableSegment.EntityType;
        var entityMapper = tableSegment.Mapper;
        string tableName;
        if (tableSegment.IsSharding)
            tableName = tableSegment.Body;
        else
        {
            if (this.ShardingProvider != null && this.ShardingProvider.TryGetTableSharding(entityType, out var tableShardingInfo))
                tableName = this.GetShardingTableName();
            else tableName = entityMapper.TableName;
        }
        var tableSchema = tableSegment.TableSchema;
        if (!string.IsNullOrEmpty(tableSegment.TableSchema))
            tableName = tableSegment.TableSchema + "." + tableName;
        tableName = this.OrmProvider.GetTableName(tableName);

        var builder = new StringBuilder("INSERT");
        if (!string.IsNullOrEmpty(this.OrExpr))
            builder.Append(this.OrExpr);

        builder.Append($" INTO {tableName} (");
        for (int i = 0; i < this.InsertFields.Count; i++)
        {
            var insertField = this.InsertFields[i];
            if (i > 0) builder.Append(',');
            builder.Append(insertField.Fields);
        }
        builder.Append(')');
        string outputSql = null;
        if (this.OutputFieldNames != null && this.OutputFieldNames.Count > 0)
        {
            (outputSql, readerFields) = this.BuildOutputSqlReaderFields();
            builder.Append(outputSql);
        }
        builder.Append(" VALUES (");
        for (int i = 0; i < this.InsertFields.Count; i++)
        {
            var insertField = this.InsertFields[i];
            if (i > 0) builder.Append(',');
            builder.Append(insertField.Values);
        }
        builder.Append(')');
        if (this.IsReturnIdentity)
        {
            if (!entityMapper.IsAutoIncrementKey)
                throw new NotSupportedException($"实体{entityMapper.EntityType.FullName}表未配置自增长字段，无法返回Identity值");
            builder.Append(this.OrmProvider.GetIdentitySql(null));
        }
        var sql = builder.ToString();
        builder.Clear();
        return sql;
    }
    public override (bool, string, IEnumerable, int, Action<IDataParameterCollection, StringBuilder, string>,
        Action<IDataParameterCollection, StringBuilder, object, string>, List<SqlFieldSegment>) BuildWithBulk(IDbCommand command)
    {
        bool isNeedSplit = false;
        object firstInsertObj = null;
        Type insertObjType = null;

        (var insertObjs, var bulkCount) = ((IEnumerable, int))this.deferredSegments[0].Value;
        foreach (var entity in insertObjs)
        {
            firstInsertObj = entity;
            break;
        }
        insertObjType = firstInsertObj.GetType();
        var tableSegment = this.Tables[0];
        var entityType = tableSegment.EntityType;
        string tableName = tableSegment.Mapper.TableName;
        if (tableSegment.IsSharding)
            tableName = tableSegment.Body;
        else isNeedSplit = this.ShardingProvider != null && this.ShardingProvider.TryGetTableSharding(entityType, out _);

        var fieldsSqlPartSetter = RepositoryHelper.BuildCreateFieldsSqlPart(this.OrmProvider, this.MapProvider, entityType, insertObjType, this.OnlyFieldNames, this.IgnoreFieldNames);
        var valuesSqlPartSetter = RepositoryHelper.BuildCreateValuesSqlParametes(this.DbContext, entityType, insertObjType, this.OnlyFieldNames, this.IgnoreFieldNames, true);
        bool isDictionary = typeof(IDictionary<string, object>).IsAssignableFrom(insertObjType);

        Action<IDataParameterCollection, StringBuilder, string> firstSqlSetter = null;
        Action<IDataParameterCollection, StringBuilder, object, string> loopSqlSetter = null;

        string outputSql = null;
        List<SqlFieldSegment> readerFields = null;
        if (this.OutputFieldNames != null && this.OutputFieldNames.Count > 0)
            (outputSql, readerFields) = this.BuildOutputSqlReaderFields();

        if (this.deferredSegments.Count > 1)
        {
            this.DbParameters = new TheaDbParameterCollection();
            for (int i = 1; i < this.deferredSegments.Count; i++)
            {
                var deferredSegment = this.deferredSegments[i];
                switch (deferredSegment.Type)
                {
                    case "WithBy":
                        this.VisitWithBy(deferredSegment.Value);
                        break;
                    case "WithByField":
                        this.VisitWithByField(deferredSegment.Value);
                        break;
                    default: throw new NotSupportedException("批量插入后，只支持WithBy/IgnoreFields/OnlyFields操作");
                }
            }

            var fixedDbParameters = this.DbParameters.Cast<IDbDataParameter>().ToList();
            if (isDictionary)
            {
                var typedFieldsSqlPartSetter = fieldsSqlPartSetter as Func<StringBuilder, object, List<MemberMap>>;
                var typedValuesSqlPartSetter = valuesSqlPartSetter as Action<IDataParameterCollection, StringBuilder, DbContext, List<MemberMap>, object, string>;

                var builder = new StringBuilder();
                for (int i = 0; i < this.InsertFields.Count; i++)
                {
                    var insertField = this.InsertFields[i];
                    if (i > 0) builder.Append(',');
                    builder.Append(insertField.Fields);
                }
                var memberMappers = typedFieldsSqlPartSetter.Invoke(builder, firstInsertObj);
                builder.Append(')');
                if (outputSql != null)
                    builder.Append(outputSql);
                builder.Append(" VALUES ");
                var firstHeadSql = builder.ToString();
                builder.Clear();
                builder = null;

                if (!string.IsNullOrEmpty(tableSegment.TableSchema))
                {
                    firstSqlSetter = (dbParameters, builder, tableName) =>
                    {
                        builder.Append($"INSERT INTO {this.OrmProvider.GetTableName(tableSegment + "." + tableName)} (");
                        builder.Append(firstHeadSql);
                        if (fixedDbParameters.Count > 0)
                            fixedDbParameters.ForEach(f => dbParameters.Add(f));
                    };
                }
                else
                {
                    firstSqlSetter = (dbParameters, builder, tableName) =>
                    {
                        builder.Append($"INSERT INTO {this.OrmProvider.GetTableName(tableName)} (");
                        builder.Append(firstHeadSql);
                        if (fixedDbParameters.Count > 0)
                            fixedDbParameters.ForEach(f => dbParameters.Add(f));
                    };
                }
                loopSqlSetter = (dbParameters, builder, insertObj, suffix) =>
                {
                    builder.Append('(');
                    for (int i = 0; i < this.InsertFields.Count; i++)
                    {
                        var insertField = this.InsertFields[i];
                        if (i > 0) builder.Append(',');
                        builder.Append(insertField.Values);
                    }
                    typedValuesSqlPartSetter.Invoke(dbParameters, builder, this.DbContext, memberMappers, insertObj, suffix);
                    builder.Append(')');
                };
            }
            else
            {
                var typedFieldsSqlPartSetter = fieldsSqlPartSetter as Action<StringBuilder>;
                var typedValuesSqlPartSetter = valuesSqlPartSetter as Action<IDataParameterCollection, StringBuilder, DbContext, object, string>;

                if (!string.IsNullOrEmpty(tableSegment.TableSchema))
                {
                    firstSqlSetter = (dbParameters, builder, tableName) =>
                    {
                        builder.Append($"INSERT INTO {this.OrmProvider.GetTableName(tableSegment + "." + tableName)} (");
                        for (int i = 0; i < this.InsertFields.Count; i++)
                        {
                            var insertField = this.InsertFields[i];
                            if (i > 0) builder.Append(',');
                            builder.Append(insertField.Fields);
                        }
                        typedFieldsSqlPartSetter.Invoke(builder);
                        builder.Append(')');
                        if (outputSql != null)
                            builder.Append(outputSql);
                        builder.Append(" VALUES ");
                        if (fixedDbParameters.Count > 0)
                            fixedDbParameters.ForEach(f => dbParameters.Add(f));
                    };
                }
                else
                {
                    firstSqlSetter = (dbParameters, builder, tableName) =>
                    {
                        builder.Append($"INSERT INTO {this.OrmProvider.GetTableName(tableName)} (");
                        for (int i = 0; i < this.InsertFields.Count; i++)
                        {
                            var insertField = this.InsertFields[i];
                            if (i > 0) builder.Append(',');
                            builder.Append(insertField.Fields);
                        }
                        typedFieldsSqlPartSetter.Invoke(builder);
                        builder.Append(')');
                        if (outputSql != null)
                            builder.Append(outputSql);
                        builder.Append(" VALUES ");
                        if (fixedDbParameters.Count > 0)
                            fixedDbParameters.ForEach(f => dbParameters.Add(f));
                    };
                }
                loopSqlSetter = (dbParameters, builder, insertObj, suffix) =>
                {
                    builder.Append('(');
                    for (int i = 0; i < this.InsertFields.Count; i++)
                    {
                        var insertField = this.InsertFields[i];
                        if (i > 0) builder.Append(',');
                        builder.Append(insertField.Values);
                    }
                    typedValuesSqlPartSetter.Invoke(dbParameters, builder, this.DbContext, insertObj, suffix);
                    builder.Append(')');
                };
            }
            this.DbParameters = command.Parameters;
        }
        else
        {
            if (isDictionary)
            {
                var typedFieldsSqlPartSetter = fieldsSqlPartSetter as Func<StringBuilder, object, List<MemberMap>>;
                var typedValuesSqlPartSetter = valuesSqlPartSetter as Action<IDataParameterCollection, StringBuilder, DbContext, List<MemberMap>, object, string>;

                var builder = new StringBuilder();
                var memberMappers = typedFieldsSqlPartSetter.Invoke(builder, firstInsertObj);
                builder.Append(')');
                if (outputSql != null)
                    builder.Append(outputSql);
                builder.Append(" VALUES ");
                var firstHeadSql = builder.ToString();
                builder.Clear();
                builder = null;

                if (!string.IsNullOrEmpty(tableSegment.TableSchema))
                {
                    firstSqlSetter = (dbParameters, builder, tableName) =>
                    {
                        builder.Append($"INSERT INTO {this.OrmProvider.GetTableName(tableSegment + "." + tableName)} (");
                        builder.Append(firstHeadSql);
                    };
                }
                else
                {
                    firstSqlSetter = (dbParameters, builder, tableName) =>
                    {
                        builder.Append($"INSERT INTO {this.OrmProvider.GetTableName(tableName)} (");
                        builder.Append(firstHeadSql);
                    };
                }
                loopSqlSetter = (dbParameters, builder, insertObj, suffix) =>
                {
                    builder.Append('(');
                    typedValuesSqlPartSetter.Invoke(dbParameters, builder, this.DbContext, memberMappers, insertObj, suffix);
                    builder.Append(')');
                };
            }
            else
            {
                var typedFieldsSqlPartSetter = fieldsSqlPartSetter as Action<StringBuilder>;
                var typedValuesSqlPartSetter = valuesSqlPartSetter as Action<IDataParameterCollection, StringBuilder, DbContext, object, string>;

                if (!string.IsNullOrEmpty(tableSegment.TableSchema))
                {
                    firstSqlSetter = (dbParameters, builder, tableName) =>
                    {
                        builder.Append($"INSERT INTO {this.OrmProvider.GetTableName(tableSegment + "." + tableName)} (");
                        typedFieldsSqlPartSetter.Invoke(builder);
                        builder.Append(')');
                        if (outputSql != null)
                            builder.Append(outputSql);
                        builder.Append(" VALUES ");
                    };
                }
                else
                {
                    firstSqlSetter = (dbParameters, builder, tableName) =>
                    {
                        builder.Append($"INSERT INTO {this.OrmProvider.GetTableName(tableName)} (");
                        typedFieldsSqlPartSetter.Invoke(builder);
                        builder.Append(')');
                        if (outputSql != null)
                            builder.Append(outputSql);
                        builder.Append(" VALUES ");
                    };
                }
                loopSqlSetter = (dbParameters, builder, insertObj, suffix) =>
                {
                    builder.Append('(');
                    typedValuesSqlPartSetter.Invoke(dbParameters, builder, this.DbContext, insertObj, suffix);
                    builder.Append(')');
                };
            }
        }
        return (isNeedSplit, tableName, insertObjs, bulkCount, firstSqlSetter, loopSqlSetter, readerFields);
    }
    public void OrExpression(string orExpr) => this.OrExpr = orExpr;
    public void OnConflict(Expression updateExpr)
    {
        this.deferredSegments.Add(new CommandSegment
        {
            Type = "SetExpression",
            Value = updateExpr
        });
    }
    public void VisitSetObject(object updateObj)
    {
        var entityType = this.Tables[0].EntityType;
        var updateObjType = updateObj.GetType();
        (var isDictionary, var setFieldsInitializer) = RepositoryHelper.BuildSqlParametersPart(this.DbContext, entityType, updateObjType, true, false, true, false, false, false, this.IsMultiple, false, this.OnlyFieldNames, this.IgnoreFieldNames, ",", null);
        if (isDictionary)
        {
            var entityMapper = this.Tables[0].Mapper;
            if (this.IsMultiple)
            {
                var typedSetFieldsInitializer = setFieldsInitializer as Action<StringBuilder, DbContext, EntityMap, object, string>;
                typedSetFieldsInitializer.Invoke(this.UpdateFields, this.DbContext, entityMapper, updateObj, $"_m{this.CommandIndex}");
            }
            else
            {
                var typedSetFieldsInitializer = setFieldsInitializer as Action<StringBuilder, DbContext, EntityMap, object>;
                typedSetFieldsInitializer.Invoke(this.UpdateFields, this.DbContext, entityMapper, updateObj);
            }
        }
        else
        {
            if (this.IsMultiple)
            {
                var typedSetFieldsInitializer = setFieldsInitializer as Action<StringBuilder, DbContext, object, string>;
                typedSetFieldsInitializer.Invoke(this.UpdateFields, this.DbContext, updateObj, $"_m{this.CommandIndex}");
            }
            else
            {
                var typedSetFieldsInitializer = setFieldsInitializer as Action<StringBuilder, DbContext, object>;
                typedSetFieldsInitializer.Invoke(this.UpdateFields, this.DbContext, updateObj);
            }
        }
    }
    public void VisitSetExpression(LambdaExpression lambdaExpr)
    {
        var currentExpr = lambdaExpr.Body;
        var callStack = new Stack<MethodCallExpression>();
        while (true)
        {
            if (currentExpr.NodeType == ExpressionType.Parameter)
                break;

            if (currentExpr is MethodCallExpression callExpr)
            {
                callStack.Push(callExpr);
                currentExpr = callExpr.Object;
            }
        }
        this.InitTableAlias(lambdaExpr);
        var builder = new StringBuilder(" ON CONFLICT ");
        while (callStack.TryPop(out var callExpr))
        {
            switch (callExpr.Method.Name)
            {
                case "DoNothing":
                    builder.Append("DO NOTHING");
                    break;
                case "UseKeys":
                    builder.Append('(');
                    foreach (var keyMapper in this.Tables[0].Mapper.KeyMembers)
                    {
                        builder.Append(this.OrmProvider.GetFieldName(keyMapper.FieldName));
                    }
                    builder.Append(") DO UPDATE SET ");
                    break;
                case "UseConstraint":
                    var constraintName = this.Evaluate<string>(callExpr.Arguments[0]);
                    if (string.IsNullOrEmpty(constraintName))
                        throw new ArgumentNullException("参数constraintName不能为null");
                    builder.Append($" {constraintName} DO UPDATE SET ");
                    break;
                case "Set":
                    //var genericType = genericArguments[0].DeclaringType;
                    if (callExpr.Arguments.Count == 1)
                    {
                        this.IsUpdate = true;
                        //Set<TFields>(Expression<Func<TEntity, TFields>> fieldsAssignment)
                        if (callExpr.Arguments[0].Type.BaseType == typeof(LambdaExpression))
                        {
                            this.VisitAndDeferred(new SqlFieldSegment { Expression = callExpr.Arguments[0] });
                        }
                        //Set<TUpdateObj>(TUpdateObj updateObj), 走参数
                        else this.VisitSetObject(this.Evaluate(callExpr.Arguments[0]));
                        this.IsUpdate = false;
                    }
                    else if (callExpr.Arguments.Count == 2)
                    {
                        //Set<TFields>(bool condition, Expression<Func<TEntity, TFields>> fieldsAssignment)
                        if (callExpr.Arguments[1].Type.BaseType == typeof(LambdaExpression))
                        {
                            if (callExpr.Arguments[0].Type == typeof(bool))
                            {
                                var condition = this.Evaluate<bool>(callExpr.Arguments[0]);
                                if (condition)
                                {
                                    this.IsUpdate = true;
                                    this.VisitAndDeferred(new SqlFieldSegment { Expression = callExpr.Arguments[1] });
                                    this.IsUpdate = false;
                                }
                            }
                            else
                            {
                                //Set<TField>(Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<TEntity, TField>> fieldValueSelector)
                                this.VisitSetFieldExpression(callExpr.Arguments[0], callExpr.Arguments[1]);
                            }
                        }
                        else
                        {
                            //Set<TUpdateObj>(bool condition, TUpdateObj updateObj)
                            if (callExpr.Arguments[0].Type == typeof(bool))
                            {
                                var condition = this.Evaluate<bool>(callExpr.Arguments[0]);
                                if (condition) this.VisitSetObject(this.Evaluate(callExpr.Arguments[1]));
                            }
                            //Set<TField>(Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
                            else this.VisitWithSetField(callExpr.Arguments[0], this.Evaluate(callExpr.Arguments[1]));
                        }
                    }
                    //Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)
                    //Set<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<TEntity, TField>> fieldValueSelector)
                    else
                    {
                        var condition = this.Evaluate<bool>(callExpr.Arguments[0]);
                        if (condition)
                        {
                            if (callExpr.Arguments[2].Type.BaseType == typeof(LambdaExpression))
                                this.VisitSetFieldExpression(callExpr.Arguments[1], callExpr.Arguments[2]);
                            else
                            {
                                this.VisitWithSetField(callExpr.Arguments[1], this.Evaluate(callExpr.Arguments[2]));
                            }
                        }
                    }
                    break;
            }
        }
        this.UpdateFields.Insert(0, builder.ToString());
        builder.Clear();
        this.IsUpdate = false;
    }
    public void Returning(params string[] fieldNames)
    {
        this.OutputFieldNames ??= new();
        this.OutputFieldNames.AddRange(fieldNames);
    }
    public virtual void Returning(Expression fieldsSelector)
        => this.OutputFieldNames = this.VisitFields(fieldsSelector, false);
    public void WithBulkCopy(IEnumerable insertObjs, int? timeoutSeconds)
    {
        this.ActionMode = ActionMode.BulkCopy;
        this.deferredSegments.Add(new CommandSegment
        {
            Type = "WithBulkCopy",
            Value = (insertObjs, timeoutSeconds)
        });
    }
    public (IEnumerable, int?) BuildWithBulkCopy() => ((IEnumerable, int?))this.deferredSegments[0].Value;
    public void InitTableAlias(LambdaExpression lambdaExpr)
    {
        this.TableAliases.Clear();
        lambdaExpr.Body.GetParameters(out var parameters);
        if (parameters == null || parameters.Count == 0)
            return;
        foreach (var parameterExpr in parameters)
        {
            if (parameterExpr.Type == typeof(ISqliteCreateConflictDoUpdate<>).MakeGenericType(this.Tables[0].EntityType))
                continue;
            if (this.TableAliases.ContainsKey(parameterExpr.Name))
                continue;
            this.TableAliases.Add(parameterExpr.Name, this.Tables[0]);
        }
    }
    public void VisitSetFieldExpression(Expression fieldSelector, Expression fieldValueSelector)
    {
        var fieldSegment = this.VisitAndDeferred(new SqlFieldSegment { Expression = fieldSelector });
        this.IsUpdate = true;
        var valueSegment = this.VisitAndDeferred(new SqlFieldSegment { Expression = fieldValueSelector });
        this.IsUpdate = false;
        if (this.UpdateFields.Length > 0) this.UpdateFields.Append(',');
        this.UpdateFields.Append($"{fieldSegment.Body}={valueSegment.Body}");
    }
    public void VisitWithSetField(Expression fieldSelector, object fieldValue)
    {
        var lambdaExpr = this.EnsureLambda(fieldSelector);
        var memberExpr = this.EnsureMemberVisit(lambdaExpr.Body) as MemberExpression;
        var entityMapper = this.Tables[0].Mapper;
        var memberMapper = entityMapper.GetMemberMap(memberExpr.Member.Name);
        var parameterName = this.OrmProvider.ParameterPrefix + memberMapper.MemberName;
        if (this.IsMultiple) parameterName += $"_m{this.CommandIndex}";
        //在前面insert的时候，参数有可能已经添加过了，此处需要判断是否需要添加
        if (!this.DbParameters.Contains(parameterName))
        {
            if (memberMapper.TypeHandler != null)
                fieldValue = memberMapper.TypeHandler.ToFieldValue(this.OrmProvider, fieldValue);
            else
            {
                var targetType = this.OrmProvider.MapDefaultType(memberMapper);
                var valueGetter = this.OrmProvider.GetParameterValueGetter(fieldValue.GetType(), targetType, false, this.Options);
                fieldValue = valueGetter.Invoke(fieldValue);
            }
            this.DbParameters.Add(this.OrmProvider.CreateParameter(parameterName, memberMapper.NativeDbType, fieldValue));
        }
        if (this.UpdateFields.Length > 0) this.UpdateFields.Append(',');
        this.UpdateFields.Append($"{this.OrmProvider.GetFieldName(memberMapper.FieldName)}={parameterName}");
    }
    private (string, List<SqlFieldSegment>) BuildOutputSqlReaderFields()
    {
        var readerFields = new List<SqlFieldSegment>();
        var entityMapper = this.Tables[0].Mapper;
        var builder = new StringBuilder();
        void addReaderField(MemberMap memberMapper)
        {
            readerFields.Add(new SqlFieldSegment
            {
                FieldType = SqlFieldType.Field,
                FromMember = memberMapper.Member,
                TargetMember = memberMapper.Member,
                SegmentType = memberMapper.MemberType,
                NativeDbType = memberMapper.NativeDbType,
                TypeHandler = memberMapper.TypeHandler,
                Body = memberMapper.FieldName
            });
        }
        builder.Append(" OUTPUT ");
        for (int i = 0; i < this.OutputFieldNames.Count; i++)
        {
            var fieldName = this.OutputFieldNames[i];
            if (i > 0) builder.Append(',');
            builder.Append($"INSERTED.{fieldName}");

            if (fieldName == "*")
            {
                foreach (var memberMapper in entityMapper.MemberMaps)
                {
                    if (memberMapper.IsIgnore || memberMapper.IsNavigation)
                        continue;
                    addReaderField(memberMapper);
                }
            }
            else
            {
                var memberMapper = entityMapper.GetMemberMapByFieldName(fieldName);
                addReaderField(memberMapper);
            }
        }
        var sql = builder.ToString();
        builder.Clear();
        return (sql, readerFields);
    }
}
