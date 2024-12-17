using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Trolley.MySqlConnector;

public class MySqlCreateVisitor : CreateVisitor
{
    public bool IsUseIgnoreInto { get; set; }
    public StringBuilder UpdateBuilder { get; set; }
    public bool IsUseSetAlias { get; set; }
    public string SetRowAlias { get; set; } = "newRow";
    public string OutputSql { get; set; }

    public MySqlCreateVisitor(DbContext dbContext, char tableAsStart = 'a')
        : base(dbContext, tableAsStart) { }

    public override string BuildCommand(ITheaCommand command, bool isReturnIdentity, out List<SqlFieldSegment> readerFields)
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
                        this.UpdateBuilder ??= new();
                        this.VisitSetObject(deferredSegment.Value);
                        break;
                    case "SetExpression":
                        this.UpdateBuilder ??= new();
                        this.VisitSetExpression(deferredSegment.Value as LambdaExpression);
                        break;
                    case "OutputFields":
                        this.VisitOutputFields(deferredSegment.Value);
                        break;
                    case "OutputExpression":
                        this.VisitOutputExpression(deferredSegment.Value);
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
            if (this.ShardingProvider != null && this.ShardingProvider.TryGetTableSharding(entityType, out _))
                tableName = this.GetShardingTableName();
            else tableName = entityMapper.TableName;
        }
        var tableSchema = tableSegment.TableSchema;
        if (!string.IsNullOrEmpty(tableSegment.TableSchema))
            tableName = $"{this.OrmProvider.GetTableName(tableSegment.TableSchema)}.{this.OrmProvider.GetTableName(tableName)}";
        tableName = this.OrmProvider.GetTableName(tableName);

        if (this.IsReturnIdentity && (this.UpdateBuilder != null || this.OutputSql != null))
            throw new NotSupportedException("返回Identity，不支持同时OnDuplicateKeyUpdate、Returning操作");

        string tailSql = null;
        if (this.UpdateBuilder != null)
            tailSql = this.UpdateBuilder.ToString();

        if (this.OutputSql != null)
        {
            tailSql += this.OutputSql;
            readerFields = this.ReaderFields;
        }

        if (this.IsReturnIdentity)
        {
            if (!entityMapper.IsAutoIncrementKey)
                throw new NotSupportedException($"实体{entityMapper.EntityType.FullName}表未配置自增长字段，无法返回Identity值");
            tailSql = this.OrmProvider.GetIdentitySql(null);
        }
        return $"{this.BuildHeadSql()} {tableName} ({this.FieldsBuilder}) VALUES ({this.ValuesBuilder}){tailSql}";
    }
    public override (bool, string, IEnumerable, int, Action<IDataParameterCollection, StringBuilder, string>,
        Action<IDataParameterCollection, StringBuilder, DbContext, object, string>, string, List<SqlFieldSegment>) BuildWithBulk(ITheaCommand command)
    {
        bool isNeedSplit = false;
        object firstInsertObj = null;
        Type insertObjType = null;
        (var insertObjs, var bulkCount) = ((IEnumerable, int))this.deferredSegments[0].Value;
        foreach (var entity in insertObjs)
        {
            firstInsertObj = entity;
            insertObjType = entity.GetType();
            break;
        }
        var tableSegment = this.Tables[0];
        var tableName = tableSegment.Mapper.TableName;
        var entityType = tableSegment.EntityType;

        if (tableSegment.IsSharding)
            tableName = tableSegment.Body;
        else isNeedSplit = this.ShardingProvider != null && this.ShardingProvider.TryGetTableSharding(entityType, out _);

        string fixedSql = "(";
        List<IDbDataParameter> fixedDbParameters = null;
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
                    case "SetObject":
                        this.UpdateBuilder ??= new();
                        this.VisitSetObject(deferredSegment.Value);
                        break;
                    case "SetExpression":
                        this.UpdateBuilder ??= new();
                        this.VisitSetExpression(deferredSegment.Value as LambdaExpression);
                        break;
                    default: throw new NotSupportedException("批量插入后，只支持WithBy/IgnoreFields/OnlyFields/OnDuplicateKeyUpdate操作");
                }
                fixedDbParameters = this.DbParameters.Cast<IDbDataParameter>().ToList();
            }
            fixedSql = $"({this.ValuesBuilder}";
        }

        var entityMapper = tableSegment.Mapper;
        var fieldsSetter = RepositoryHelper.BuildCreateFieldsSqlPart(this.DbContext, entityType, insertObjType, false, this.OnlyFieldNames, this.IgnoreFieldNames);
        var valuesSetter = RepositoryHelper.BuildCreateValuesSqlPart(this.DbContext, entityType, insertObjType, true, false, this.OnlyFieldNames, this.IgnoreFieldNames);
        var typedValuesSetter = valuesSetter as Action<IDataParameterCollection, StringBuilder, DbContext, object, string>;

        string headSql = $"{this.BuildHeadSql()} ";
        if (!string.IsNullOrEmpty(tableSegment.TableSchema))
            headSql = $"{this.BuildHeadSql()} {this.OrmProvider.GetTableName(tableSegment.TableSchema)}.";

        //生成批量Fields SQL
        fieldsSetter.Invoke(this.FieldsBuilder, this.DbContext, firstInsertObj);

        string tailSql = null;
        List<SqlFieldSegment> readerFields = null;

        if (this.UpdateBuilder != null)
            tailSql = this.UpdateBuilder.ToString();
        if (this.OutputSql != null)
        {
            tailSql += this.OutputSql;
            readerFields = this.ReaderFields;
        }

        var fieldsSql = $" ({this.FieldsBuilder}) VALUES ";
        this.FieldsBuilder.Clear();
        this.ValuesBuilder.Clear();

        Action<IDataParameterCollection, StringBuilder, string> firstSqlSetter = null;
        if (this.deferredSegments.Count > 1)
        {
            firstSqlSetter = (dbParameters, builder, tableName) =>
            {
                builder.Append($"{headSql}{this.OrmProvider.GetTableName(tableName)}{fieldsSql}");
                fixedDbParameters.ForEach(f => dbParameters.Add(f));
            };
        }
        else firstSqlSetter = (dbParameters, builder, tableName) => builder.Append($"{headSql}{this.OrmProvider.GetTableName(tableName)}{fieldsSql}");

        Action<IDataParameterCollection, StringBuilder, DbContext, object, string> loopSqlSetter = null;
        loopSqlSetter = (dbParameters, builder, dbContext, insertObj, suffix) =>
        {
            builder.Append(fixedSql);
            typedValuesSetter.Invoke(dbParameters, builder, dbContext, insertObj, suffix);
            builder.Append(')');
        };
        this.DbParameters = command.Parameters;
        return (isNeedSplit, tableName, insertObjs, bulkCount, firstSqlSetter, loopSqlSetter, tailSql, readerFields);
    }
    public void Returning(Type targetType, string fieldNames)
    {
        this.deferredSegments.Add(new CommandSegment
        {
            Type = "OutputFields",
            Value = (targetType, fieldNames)
        });
    }
    public virtual void Returning(Type targetType, Expression fieldsSelector)
    {
        this.deferredSegments.Add(new CommandSegment
        {
            Type = "OutputExpression",
            Value = (targetType, fieldsSelector)
        });
    }
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
    public void OnDuplicateKeyUpdate(object updateObj)
    {
        if (this.ActionMode == ActionMode.Bulk)
            throw new NotSupportedException("批量插入时，不支持此方法的调用，请使用OnDuplicateKeyUpdate<TUpdateFields>(Expression<Func<IMySqlCreateDuplicateKeyUpdate<TEntity>, TUpdateFields>> fieldsAssignment)方法");
        this.deferredSegments.Add(new CommandSegment
        {
            Type = "SetObject",
            Value = updateObj
        });
    }
    public void OnDuplicateKeyUpdate(Expression updateExpr)
    {
        this.deferredSegments.Add(new CommandSegment
        {
            Type = "SetExpression",
            Value = updateExpr
        });
    }
    public string BuildHeadSql()
    {
        if (this.IsUseIgnoreInto) return "INSERT IGNORE INTO";
        return "INSERT INTO";
    }
    public void VisitSetObject(object updateObj)
    {
        if (this.ActionMode == ActionMode.Bulk)
            throw new NotSupportedException("批量插入时，不支持此方法的调用，请使用OnDuplicateKeyUpdate<TUpdateFields>(Expression<Func<IMySqlCreateDuplicateKeyUpdate<TEntity>, TUpdateFields>> fieldsAssignment)方法");

        var entityType = this.Tables[0].EntityType;
        var updateObjType = updateObj.GetType();
        var setFieldsSetter = RepositoryHelper.BuildFieldsSqlParametersPart(this.DbContext, entityType, updateObjType, 3, 1, 0, false, this.IsMultiple, false, this.OnlyFieldNames, this.IgnoreFieldNames);
        if (this.IsMultiple)
        {
            var typedSetFieldsSetter = setFieldsSetter as Action<IDataParameterCollection, StringBuilder, DbContext, object, string>;
            typedSetFieldsSetter.Invoke(this.DbParameters, this.UpdateBuilder, this.DbContext, updateObj, $"_m{this.CommandIndex}");
        }
        else
        {
            var typedSetFieldsSetter = setFieldsSetter as Action<IDataParameterCollection, StringBuilder, DbContext, object>;
            typedSetFieldsSetter.Invoke(this.DbParameters, this.UpdateBuilder, this.DbContext, updateObj);
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
        bool isNeedAlias = false;
        while (callStack.TryPop(out var callExpr))
        {
            var genericArguments = callExpr.Method.GetGenericArguments();
            switch (callExpr.Method.Name)
            {
                case "UseAlias":
                    this.IsUseSetAlias = true;
                    break;
                case "Set":
                    //var genericType = genericArguments[0].DeclaringType;
                    if (callExpr.Arguments.Count == 1)
                    {
                        //Set<TFields>(Expression<Func<TEntity, TFields>> fieldsAssignment)
                        if (callExpr.Arguments[0].Type.BaseType == typeof(LambdaExpression))
                        {
                            this.VisitAndDeferred(new SqlFieldSegment { Expression = callExpr.Arguments[0] });
                            isNeedAlias = true;
                        }
                        //Set<TUpdateObj>(TUpdateObj updateObj), 走参数
                        else this.VisitSetObject(this.Evaluate(callExpr.Arguments[0]));
                    }
                    else if (callExpr.Arguments.Count == 2)
                    {
                        //Set<TFields>(bool condition, Expression<Func<TEntity, TFields>> fieldsAssignment)
                        //Set<TField>(Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<TEntity, TField>> fieldValueSelector)
                        if (callExpr.Arguments[1].Type.BaseType == typeof(LambdaExpression))
                        {
                            if (callExpr.Arguments[0].Type == typeof(bool))
                            {
                                var condition = this.Evaluate<bool>(callExpr.Arguments[0]);
                                if (condition) this.VisitAndDeferred(new SqlFieldSegment { Expression = callExpr.Arguments[1] });
                            }
                            else this.VisitSetFieldExpression(callExpr.Arguments[0], callExpr.Arguments[1]);
                            isNeedAlias = true;
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
                            else this.VisitWithSetField(callExpr.Arguments[1], this.Evaluate(callExpr.Arguments[2]));
                        }
                    }
                    break;
            }
        }
        this.UpdateBuilder.Insert(0, " ON DUPLICATE KEY UPDATE ");
        if (this.IsUseSetAlias && isNeedAlias) this.UpdateBuilder.Insert(0, $" AS {this.SetRowAlias}");
    }
    public override SqlFieldSegment VisitNew(SqlFieldSegment sqlSegment)
    {
        //只有OnDuplicateKeyUpdate.Set时，才会走到此场景，如：.Set(f => new { TotalAmount = f.TotalAmount + x.Values(f.TotalAmount) })
        //INSERT INTO ... SELECT ... FROM ... 由FromCommand单独处理了，FromCommand走的是QueryVisitor的解析
        var newExpr = sqlSegment.Expression as NewExpression;
        if (newExpr.Type.Name.StartsWith("<>"))
        {
            var entityMapper = this.Tables[0].Mapper;
            for (int i = 0; i < newExpr.Arguments.Count; i++)
            {
                var memberInfo = newExpr.Members[i];
                if (!entityMapper.TryGetMemberMap(memberInfo.Name, out var memberMapper))
                    continue;
                sqlSegment = this.VisitAndDeferred(new SqlFieldSegment
                {
                    Expression = newExpr.Arguments[i],
                    NativeDbType = memberMapper.NativeDbType,
                    TypeHandler = memberMapper.TypeHandler
                });
                this.AddMemberElement(sqlSegment, memberMapper);
            }
            return sqlSegment;
        }
        return this.Evaluate(sqlSegment);
    }
    public override SqlFieldSegment VisitMemberInit(SqlFieldSegment sqlSegment)
    {
        var memberInitExpr = sqlSegment.Expression as MemberInitExpression;
        var entityMapper = this.Tables[0].Mapper;
        for (int i = 0; i < memberInitExpr.Bindings.Count; i++)
        {
            if (memberInitExpr.Bindings[i].BindingType != MemberBindingType.Assignment)
                throw new NotImplementedException($"不支持除MemberBindingType.Assignment类型外的成员绑定表达式, {memberInitExpr.Bindings[i]}");
            var memberAssignment = memberInitExpr.Bindings[i] as MemberAssignment;
            if (!entityMapper.TryGetMemberMap(memberAssignment.Member.Name, out var memberMapper))
                continue;
            sqlSegment = this.VisitAndDeferred(new SqlFieldSegment
            {
                Expression = memberAssignment.Expression,
                NativeDbType = memberMapper.NativeDbType,
                TypeHandler = memberMapper.TypeHandler
            });
            this.AddMemberElement(sqlSegment, memberMapper);
        }
        return this.Evaluate(sqlSegment);
    }
    public override IQueryVisitor CreateQueryVisitor()
    {
        var queryVisiter = new MySqlQueryVisitor(this.DbContext, this.TableAsStart, this.DbParameters)
        {
            IsMultiple = this.IsMultiple,
            CommandIndex = this.CommandIndex,
            RefQueries = this.RefQueries,
            ShardingTables = this.ShardingTables,
            IsUseIgnoreInto = this.IsUseIgnoreInto
        };
        return queryVisiter;
    }
    public void InitTableAlias(LambdaExpression lambdaExpr)
    {
        this.TableAliases.Clear();
        lambdaExpr.Body.GetParameters(out var parameters);
        if (parameters == null || parameters.Count == 0)
            return;
        foreach (var parameterExpr in parameters)
        {
            if (parameterExpr.Type == typeof(IMySqlCreateDuplicateKeyUpdate<>).MakeGenericType(this.Tables[0].EntityType))
                continue;
            if (this.TableAliases.ContainsKey(parameterExpr.Name))
                continue;
            this.TableAliases.Add(parameterExpr.Name, this.Tables[0]);
        }
    }
    public void AddMemberElement(SqlFieldSegment sqlSegment, MemberMap memberMapper)
    {
        if (this.UpdateBuilder.Length > 0) this.UpdateBuilder.Append(',');
        var fieldName = this.OrmProvider.GetFieldName(memberMapper.FieldName);

        if (sqlSegment == SqlFieldSegment.Null)
            this.UpdateBuilder.Append($"{fieldName}=NULL");
        else if (sqlSegment.IsConstant || sqlSegment.IsVariable)
        {
            var parameterName = this.OrmProvider.ParameterPrefix + this.ParameterPrefix + this.DbParameters.Count.ToString();
            if (this.IsMultiple) parameterName += $"_m{this.CommandIndex}";

            var dbFieldValue = sqlSegment.Value;
            if (memberMapper.TypeHandler != null)
                dbFieldValue = memberMapper.TypeHandler.ToFieldValue(this.OrmProvider, dbFieldValue);
            else
            {
                var targetType = this.OrmProvider.MapDefaultType(memberMapper);
                var valueGetter = this.OrmProvider.GetParameterValueGetter(dbFieldValue.GetType(), targetType, false, this.Options);
                dbFieldValue = valueGetter.Invoke(dbFieldValue);
            }

            this.DbParameters.Add(this.OrmProvider.CreateParameter(parameterName, memberMapper.NativeDbType, dbFieldValue));
            this.UpdateBuilder.Append($"{fieldName}={parameterName}");
        }
        //带有参数或字段的表达式或函数调用、或是只有参数或字段
        //.Set(true, f => f.TotalAmount))
        //.Set(f => new { TotalAmount = x.Values(f.TotalAmount) })
        else this.UpdateBuilder.Append($"{fieldName}={sqlSegment.Body}");
    }
    public void VisitSetFieldExpression(Expression fieldSelector, Expression fieldValueSelector)
    {
        var fieldSegment = this.VisitAndDeferred(new SqlFieldSegment { Expression = fieldSelector });
        var valueSegment = this.VisitAndDeferred(new SqlFieldSegment { Expression = fieldValueSelector });
        if (this.UpdateBuilder.Length > 0) this.UpdateBuilder.Append(',');
        this.UpdateBuilder.Append($"{fieldSegment.Body}={valueSegment.Body}");
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
        if (this.UpdateBuilder.Length > 0) this.UpdateBuilder.Append(',');
        this.UpdateBuilder.Append($"{this.OrmProvider.GetFieldName(memberMapper.FieldName)}={parameterName}");
    }
    public void VisitOutputFields(object deferredSegmentValue)
    {
        this.ReaderFields = new();
        (var targetType, var fieldNames) = ((Type, string))deferredSegmentValue;
        this.OutputSql = $" RETURNING {fieldNames}";
        var entityType = this.Tables[0].EntityType;
        if (fieldNames == "*" || targetType == entityType)
        {
            var entityMapper = this.Tables[0].Mapper;
            foreach (var memberMapper in entityMapper.MemberMaps)
            {
                if (memberMapper.IsIgnore || memberMapper.IsNavigation)
                    continue;
                this.ReaderFields.Add(new SqlFieldSegment
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
        }
        //TODO:
        //else
        //{
        //    var memberMapper = entityMapper.GetMemberMapByFieldName(fieldName);
        //    this.ReaderFields.Add(new SqlFieldSegment
        //    {
        //        FieldType = SqlFieldType.Field,
        //        FromMember = memberMapper.Member,
        //        TargetMember = memberMapper.Member,
        //        SegmentType = memberMapper.MemberType,
        //        NativeDbType = memberMapper.NativeDbType,
        //        TypeHandler = memberMapper.TypeHandler,
        //        Body = memberMapper.FieldName
        //    });
        //}
    }
    public void VisitOutputExpression(object deferredSegmentValue)
    {
        //RETURNING f(id1), UPPER(animal1) 不需要列名 
        this.ReaderFields = new();
        (var targetType, var fieldsSelector) = ((Type, Expression))deferredSegmentValue;
        var lambdaExpr = fieldsSelector as LambdaExpression;
        var entityMapper = this.Tables[0].Mapper;
        var builder = new StringBuilder(" RETURNING ");
        this.InitTableAlias(lambdaExpr);
        switch (lambdaExpr.Body.NodeType)
        {
            case ExpressionType.MemberAccess:
                {
                    var memberExpr = lambdaExpr.Body as MemberExpression;
                    var sqlSegment = this.VisitAndDeferred(new SqlFieldSegment { Expression = memberExpr });
                    this.GetQuotedValue(sqlSegment, true);
                    sqlSegment.TargetMember = memberExpr.Member;
                    sqlSegment.SegmentType = memberExpr.Type;
                    builder.Append(sqlSegment.Body);
                    this.ReaderFields.Add(sqlSegment);
                }
                break;
            case ExpressionType.New:
                var newExpr = lambdaExpr.Body as NewExpression;
                for (int i = 0; i < newExpr.Arguments.Count; i++)
                {
                    var memberInfo = newExpr.Members[i];
                    var sqlSegment = this.VisitAndDeferred(new SqlFieldSegment { Expression = newExpr.Arguments[i] });
                    this.GetQuotedValue(sqlSegment, true);
                    sqlSegment.TargetMember = memberInfo;
                    sqlSegment.SegmentType = memberInfo.GetMemberType();
                    if (i > 0) builder.Append(',');
                    builder.Append(sqlSegment.Body);
                    this.ReaderFields.Add(sqlSegment);
                }
                break;
            case ExpressionType.MemberInit:
                var memberInitExpr = lambdaExpr.Body as MemberInitExpression;
                for (int i = 0; i < memberInitExpr.Bindings.Count; i++)
                {
                    if (memberInitExpr.Bindings[i].BindingType != MemberBindingType.Assignment)
                        throw new NotSupportedException("暂时不支持除MemberBindingType.Assignment类型外的成员绑定表达式");

                    var memberAssignment = memberInitExpr.Bindings[i] as MemberAssignment;
                    var sqlSegment = this.VisitAndDeferred(new SqlFieldSegment { Expression = memberAssignment.Expression });
                    this.GetQuotedValue(sqlSegment, true);
                    sqlSegment.TargetMember = memberAssignment.Member;
                    sqlSegment.SegmentType = memberAssignment.Member.GetMemberType();
                    if (i > 0) builder.Append(',');
                    builder.Append(sqlSegment.Body);
                    this.ReaderFields.Add(sqlSegment);
                }
                break;
        }
        this.OutputSql = builder.ToString();
        builder.Clear();
    }
    public override void Dispose()
    {
        base.Dispose();
        this.UpdateBuilder = null;
        this.OutputSql = null;
    }
}