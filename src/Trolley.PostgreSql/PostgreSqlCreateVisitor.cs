﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Text;

namespace Trolley.PostgreSql;

public class PostgreSqlCreateVisitor : CreateVisitor
{
    public StringBuilder UpdateFields { get; set; }
    public bool IsUpdate { get; set; }
    public bool IsUseTableAlias { get; set; }
    public List<string> OutputFieldNames { get; set; }

    public PostgreSqlCreateVisitor(DbContext dbContext, char tableAsStart = 'a')
        : base(dbContext, tableAsStart) { }

    public override string BuildCommand(IDbCommand command, bool isReturnIdentity, out List<SqlFieldSegment> readerFields)
    {
        string sql;
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

        var fieldsBuilder = new StringBuilder($"INSERT INTO {tableName} ");
        //Set语句中，引用了原值，就需要使用别名
        if (this.IsUseTableAlias) fieldsBuilder.Append($"AS {tableSegment.AliasName} ");
        fieldsBuilder.Append('(');
        var valuesBuilder = new StringBuilder(" VALUES (");
        for (int i = 0; i < this.InsertFields.Count; i++)
        {
            var insertField = this.InsertFields[i];
            if (i > 0)
            {
                fieldsBuilder.Append(',');
                valuesBuilder.Append(',');
            }
            fieldsBuilder.Append(insertField.Fields);
            valuesBuilder.Append(insertField.Values);
        }
        fieldsBuilder.Append(')');
        valuesBuilder.Append(')');

        bool hasUpdateFields = false;
        if (this.UpdateFields != null && this.UpdateFields.Length > 0)
        {
            valuesBuilder.Append(this.UpdateFields);
            this.UpdateFields = null;
            hasUpdateFields = true;
        }
        string outputSql;
        if (this.OutputFieldNames != null && this.OutputFieldNames.Count > 0)
        {
            (outputSql, readerFields) = this.BuildOutputSqlReaderFields();
            valuesBuilder.Append(outputSql);
        }
        if (this.IsReturnIdentity)
        {
            if (!entityMapper.IsAutoIncrement)
                throw new NotSupportedException($"实体{entityMapper.EntityType.FullName}表未配置自增长字段，无法返回Identity值");
            if (hasUpdateFields) throw new NotSupportedException("包含更新子句，不支持返回Identity");
            var keyFieldName = this.OrmProvider.GetFieldName(entityMapper.KeyMembers[0].FieldName);
            valuesBuilder.Append(this.OrmProvider.GetIdentitySql(keyFieldName));
        }

        fieldsBuilder.Append(valuesBuilder);
        valuesBuilder.Clear();
        var sql = fieldsBuilder.ToString();
        fieldsBuilder.Clear();
        return sql;
    }
    public override string BuildWithBulkSql(IDbCommand command, out List<SqlFieldSegment> readerFields)
    {
        //多命令查询或是ToSql才会走到此分支
        //多语句执行，一次性不分批次
        var builder = new StringBuilder();
        (var isNeedSplit, var tableName, var insertObjs, _, var firstSqlSetter,
            var loopSqlSetter, readerFields) = this.BuildWithBulk(command);

        string outputSql = null;
        if (this.OutputFieldNames != null && this.OutputFieldNames.Count > 0)
            (outputSql, readerFields) = this.BuildOutputSqlReaderFields();

        void executor(string tableName, IEnumerable insertObjs)
        {
            firstSqlSetter.Invoke(command.Parameters, builder, tableName);
            int index = 0;
            foreach (var insertObj in insertObjs)
            {
                if (index > 0) builder.Append(',');
                loopSqlSetter.Invoke(command.Parameters, builder, insertObj, index.ToString());
                index++;
            }
            if (outputSql != null)
                builder.Append(outputSql);
        }
        if (isNeedSplit)
        {
            var entityType = this.Tables[0].EntityType;
            var tabledInsertObjs = RepositoryHelper.SplitShardingParameters(this.MapProvider, this.ShardingProvider, entityType, insertObjs);
            int index = 0;
            foreach (var tabledInsertObj in tabledInsertObjs)
            {
                if (index > 0) builder.Append(';');
                executor(tabledInsertObj.Key, tabledInsertObj.Value);
                index++;
            }
        }
        else executor(tableName, insertObjs);
        var sql = builder.ToString();
        builder.Clear();
        return sql;
    }
    public void Returning(params string[] fieldNames)
    {
        this.OutputFieldNames ??= new();
        this.OutputFieldNames.AddRange(fieldNames);
    }
    public virtual void Returning(Expression fieldsSelector)
        => this.OutputFieldNames = this.VisitFields(fieldsSelector);
    public void WithBulkCopy(IEnumerable insertObjs)
    {
        this.ActionMode = ActionMode.BulkCopy;
        this.deferredSegments.Add(new CommandSegment
        {
            Type = "WithBulkCopy",
            Value = insertObjs
        });
    }
    public IEnumerable BuildWithBulkCopy() => (IEnumerable)this.deferredSegments[0].Value;
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
    public override SqlFieldSegment VisitMemberAccess(SqlFieldSegment sqlSegment)
    {
        if (this.IsUpdate)
        {
            var memberExpr = sqlSegment.Expression as MemberExpression;
            if (!this.Tables[0].Mapper.TryGetMemberMap(memberExpr.Member.Name, out var memberMapper))
                throw new MissingMemberException($"类{this.Tables[0].EntityType.FullName}未找到成员{memberExpr.Member.Name}");

            //在解析过程中，引用原值时使用别名，最后再设置IsNeedTableAlias
            this.IsUseTableAlias = true;
            var fieldName = $"{this.Tables[0].AliasName}.{this.OrmProvider.GetFieldName(memberMapper.FieldName)}";
            return new SqlFieldSegment
            {
                HasField = true,
                FromMember = memberMapper.Member,
                NativeDbType = memberMapper.NativeDbType,
                TypeHandler = memberMapper.TypeHandler,
                Body = fieldName
            };
        }
        return base.VisitMemberAccess(sqlSegment);
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
        var queryVisiter = new PostgreSqlQueryVisitor(this.DbContext, this.TableAsStart, this.DbParameters)
        {
            IsMultiple = this.IsMultiple,
            CommandIndex = this.CommandIndex,
            RefQueries = this.RefQueries,
            ShardingTables = this.ShardingTables
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
            if (parameterExpr.Type == typeof(IPostgreSqlCreateConflictDoUpdate<>).MakeGenericType(this.Tables[0].EntityType))
                continue;
            if (this.TableAliases.ContainsKey(parameterExpr.Name))
                continue;
            this.TableAliases.Add(parameterExpr.Name, this.Tables[0]);
        }
    }
    public void AddMemberElement(SqlFieldSegment sqlSegment, MemberMap memberMapper)
    {
        if (this.UpdateFields.Length > 0) this.UpdateFields.Append(',');
        var fieldName = this.OrmProvider.GetFieldName(memberMapper.FieldName);

        if (sqlSegment == SqlFieldSegment.Null)
            this.UpdateFields.Append($"{fieldName}=NULL");
        else if (sqlSegment.IsConstant || sqlSegment.IsVariable)
        {
            var parameterName = this.OrmProvider.ParameterPrefix + this.ParameterPrefix + this.DbParameters.Count.ToString();
            if (this.IsMultiple) parameterName += $"_m{this.CommandIndex}";

            var dbFieldValue = sqlSegment.Value;
            if (memberMapper.TypeHandler != null)
                dbFieldValue = memberMapper.TypeHandler.ToFieldValue(this.OrmProvider, dbFieldValue);
            else
            {
                var targetType = this.OrmProvider.MapDefaultType(memberMapper.NativeDbType);
                var valueGetter = this.OrmProvider.GetParameterValueGetter(dbFieldValue.GetType(), targetType, false, this.Options);
                dbFieldValue = valueGetter.Invoke(dbFieldValue);
            }

            this.DbParameters.Add(this.OrmProvider.CreateParameter(parameterName, memberMapper.NativeDbType, dbFieldValue));
            this.UpdateFields.Append($"{fieldName}={parameterName}");
        }
        //带有参数或字段的表达式或函数调用、或是只有参数或字段
        //.Set(true, f => f.TotalAmount))
        //.Set(f => new { TotalAmount = x.Values(f.TotalAmount) })
        else this.UpdateFields.Append($"{fieldName}={sqlSegment.Body}");
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
                var targetType = this.OrmProvider.MapDefaultType(memberMapper.NativeDbType);
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
        builder.Append(" RETURNING ");
        for (int i = 0; i < this.OutputFieldNames.Count; i++)
        {
            var fieldName = this.OutputFieldNames[i];
            if (i > 0) builder.Append(',');
            if (fieldName == "*")
            {
                builder.Append(fieldName);
                foreach (var memberMapper in entityMapper.MemberMaps)
                {
                    if (memberMapper.IsIgnore || memberMapper.IsNavigation
                        || (memberMapper.MemberType.IsEntityType(out _) && memberMapper.TypeHandler == null))
                        continue;
                    addReaderField(memberMapper);
                }
            }
            else
            {
                builder.Append(this.OrmProvider.GetFieldName(fieldName));
                var memberMapper = entityMapper.GetMemberMapByFieldName(fieldName);
                addReaderField(memberMapper);
            }
        }
        var sql = builder.ToString();
        builder.Clear();
        return (sql, readerFields);
    }
}
