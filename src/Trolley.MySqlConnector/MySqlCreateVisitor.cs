﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Trolley.MySqlConnector;

public class MySqlCreateVisitor : CreateVisitor
{
    public bool IsUseIgnoreInto { get; set; }
    public StringBuilder UpdateFields { get; set; }
    public bool IsUpdate { get; set; }
    public bool IsSetAlias { get; set; }
    public string SetRowAlias { get; set; } = "newRow";
    public List<string> OutputFieldNames { get; set; }

    public MySqlCreateVisitor(string dbKey, IOrmProvider ormProvider, IEntityMapProvider mapProvider, IShardingProvider shardingProvider, bool isParameterized = false, char tableAsStart = 'a', string parameterPrefix = "p")
        : base(dbKey, ormProvider, mapProvider, shardingProvider, isParameterized, tableAsStart, parameterPrefix) { }

    public override string BuildCommand(IDbCommand command, bool isReturnIdentity, out List<ReaderField> readerFields)
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
    public override string BuildSql(out List<ReaderField> readerFields)
    {
        readerFields = null;
        var entityType = this.Tables[0].EntityType;
        var entityMapper = this.Tables[0].Mapper;
        var tableName = entityMapper.TableName;
        if (this.ShardingProvider.TryGetShardingTable(entityType, out _))
        {
            if (string.IsNullOrEmpty(this.Tables[0].Body))
                throw new Exception($"实体表{entityType.FullName}有配置分表，当前操作未指定分表，请调用UseTable或UseTableBy方法指定分表");
            tableName = this.Tables[0].Body;
        }
        tableName = this.OrmProvider.GetTableName(tableName);

        var fieldsBuilder = new StringBuilder($"{this.BuildHeadSql()} {tableName} (");
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
        string outputSql = null;
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
            valuesBuilder.Append(this.OrmProvider.GetIdentitySql(entityMapper.EntityType));
        }

        fieldsBuilder.Append(valuesBuilder);
        valuesBuilder.Clear();
        var sql = fieldsBuilder.ToString();
        fieldsBuilder.Clear();
        fieldsBuilder = null;
        valuesBuilder = null;
        return sql;
    }
    public override string BuildWithBulkSql(IDbCommand command, out List<ReaderField> readerFields)
    {
        //多命令查询或是ToSql才会走到此分支
        //多语句执行，一次性不分批次
        var builder = new StringBuilder();
        (var isNeedSplit, var tableName, var insertObjs, _, var firstInsertObj,
            var headSqlSetter, var valuesSqlSetter, readerFields) = this.BuildWithBulk(command);

        string outputSql = null;
        if (this.OutputFieldNames != null && this.OutputFieldNames.Count > 0)
            (outputSql, readerFields) = this.BuildOutputSqlReaderFields();

        Action<string, IEnumerable> executor = (tableName, insertObjs) =>
        {
            headSqlSetter.Invoke(command.Parameters, builder, tableName, firstInsertObj);
            int index = 0;
            foreach (var insertObj in insertObjs)
            {
                if (index > 0) builder.Append(',');
                valuesSqlSetter.Invoke(command.Parameters, builder, insertObj, index.ToString());
                index++;
            }
            if (outputSql != null)
                builder.Append(outputSql);
        };
        if (isNeedSplit)
        {
            var entityType = this.Tables[0].EntityType;
            var tabledInsertObjs = RepositoryHelper.SplitShardingParameters(this.DbKey, this.MapProvider, this.ShardingProvider, entityType, insertObjs);
            int index = 0;
            foreach (var tabledInsertObj in tabledInsertObjs)
            {
                if (index > 0) builder.Append(';');
                executor.Invoke(tabledInsertObj.Key, tabledInsertObj.Value);
                index++;
            }
        }
        else executor.Invoke(tableName, insertObjs);
        var sql = builder.ToString();
        builder.Clear();
        builder = null;
        return sql;
    }
    public virtual (bool, string, IEnumerable, int, object, Action<IDataParameterCollection, StringBuilder, string, object>,
        Action<IDataParameterCollection, StringBuilder, object, string>, List<ReaderField>) BuildWithBulk(IDbCommand command)
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
        var tableName = this.Tables[0].Mapper.TableName;
        var entityType = this.Tables[0].EntityType;

        if (this.ShardingProvider.TryGetShardingTable(entityType, out _))
        {
            //有设置分表，优先使用分表，没有设置分表，则根据数据的字段确定分表
            if (!string.IsNullOrEmpty(this.Tables[0].Body))
                tableName = this.Tables[0].Body;
            //未指定分表，需要根据数据字段确定分表
            else isNeedSplit = true;
        }
        Action<IDataParameterCollection, StringBuilder, string, object> headSqlSetter = null;
        Action<IDataParameterCollection, StringBuilder, object, string> valuesSqlSetter = null;
        var fieldsSqlPartSetter = RepositoryHelper.BuildCreateFieldsSqlPart(this.OrmProvider, this.MapProvider, entityType, insertObjType, this.OnlyFieldNames, this.IgnoreFieldNames);
        var valuesSqlPartSetter = RepositoryHelper.BuildCreateValuesSqlParametes(this.OrmProvider, this.MapProvider, entityType, insertObjType, this.OnlyFieldNames, this.IgnoreFieldNames, true);

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
            headSqlSetter = (dbParameters, builder, tableName, insertObj) =>
            {
                builder.Append($"INSERT INTO {this.OrmProvider.GetFieldName(tableName)} (");
                for (int i = 0; i < this.InsertFields.Count; i++)
                {
                    var insertField = this.InsertFields[i];
                    if (i > 0) builder.Append(',');
                    builder.Append(insertField.Fields);
                }
                fieldsSqlPartSetter.Invoke(builder, insertObj);
                builder.Append(" VALUES ");
                if (fixedDbParameters.Count > 0)
                    fixedDbParameters.ForEach(f => dbParameters.Add(f));
            };
            var typedValuesSqlPartSetter = valuesSqlPartSetter as Action<IDataParameterCollection, StringBuilder, IOrmProvider, object, string>;

            valuesSqlSetter = (dbParameters, builder, insertObj, suffix) =>
            {
                builder.Append('(');
                for (int i = 0; i < this.InsertFields.Count; i++)
                {
                    var insertField = this.InsertFields[i];
                    if (i > 0) builder.Append(',');
                    builder.Append(insertField.Values);
                }
                typedValuesSqlPartSetter.Invoke(dbParameters, builder, this.OrmProvider, insertObj, suffix);
                builder.Append(')');
            };
            this.DbParameters = command.Parameters;
        }
        else
        {
            (_, var typedHeadSqlSetter, var sqlSetter) = RepositoryHelper.BuildCreateSqlParameters(this.OrmProvider, this.MapProvider, entityType, insertObjType, this.OnlyFieldNames, this.IgnoreFieldNames, true, false);
            headSqlSetter = (dbParameters, builder, tableName, insertObj) =>
            {
                typedHeadSqlSetter.Invoke(builder, tableName, insertObj);
                builder.Append(" VALUES ");
            };
            valuesSqlSetter = sqlSetter as Action<IDataParameterCollection, StringBuilder, object, string>;
        }
        return (isNeedSplit, tableName, insertObjs, bulkCount, firstInsertObj, headSqlSetter, valuesSqlSetter, null);
    }
    public void Returning(params string[] fieldNames)
    {
        this.OutputFieldNames ??= new();
        this.OutputFieldNames.AddRange(fieldNames);
    }
    public virtual void Returning(Expression fieldsSelector)
        => this.OutputFieldNames = this.VisitFields(fieldsSelector);
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
        var entityType = this.Tables[0].EntityType;
        var updateObjType = updateObj.GetType();
        var setFieldsInitializer = RepositoryHelper.BuildSqlParametersPart(this.OrmProvider, this.MapProvider, entityType, updateObjType, true, false, true, false, false, false, this.IsMultiple, false, this.OnlyFieldNames, this.IgnoreFieldNames, ",", null);
        if (this.IsMultiple)
        {
            var typedSetFieldsInitializer = setFieldsInitializer as Action<StringBuilder, IOrmProvider, object, string>;
            typedSetFieldsInitializer.Invoke(this.UpdateFields, this.OrmProvider, updateObj, $"_m{this.CommandIndex}");
        }
        else
        {
            var typedSetFieldsInitializer = setFieldsInitializer as Action<StringBuilder, IOrmProvider, object>;
            typedSetFieldsInitializer.Invoke(this.UpdateFields, this.OrmProvider, updateObj);
        }
    }
    public void VisitSetExpression(LambdaExpression lambdaExpr)
    {
        this.IsUpdate = true;
        var currentExpr = lambdaExpr.Body;
        var entityType = this.Tables[0].EntityType;
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
                    //if (callExpr.Arguments.Count > 0)
                    //    this.SetRowAlias = this.Evaluate<string>(callExpr.Arguments[0]);
                    this.IsSetAlias = true;
                    break;
                case "Set":
                    //var genericType = genericArguments[0].DeclaringType;
                    if (callExpr.Arguments.Count == 1)
                    {
                        //Set<TFields>(Expression<Func<TEntity, TFields>> fieldsAssignment)
                        if (callExpr.Arguments[0].Type.BaseType == typeof(LambdaExpression))
                        {
                            var argumentExpr = callExpr.Arguments[0];
                            this.VisitAndDeferred(new SqlSegment { Expression = callExpr.Arguments[0] });
                            isNeedAlias = true;
                        }
                        //Set<TUpdateObj>(TUpdateObj updateObj), 走参数
                        else this.VisitSetObject(this.Evaluate(callExpr.Arguments[0]));
                    }
                    else if (callExpr.Arguments.Count == 2)
                    {
                        //Set<TFields>(bool condition, Expression<Func<TEntity, TFields>> fieldsAssignment)
                        if (callExpr.Arguments[1].Type.BaseType == typeof(LambdaExpression))
                        {
                            var argumentExpr = callExpr.Arguments[1];
                            //if (argumentExpr.NodeType != ExpressionType.New && argumentExpr.NodeType != ExpressionType.MemberAccess)
                            //    throw new NotSupportedException($"不支持的表达式访问，类型{callExpr.Method.DeclaringType.FullName}.Set方法，只支持MemberAccess/New访问，如：.Set(true, f =&gt; f.TotalAmount) 或 .Set(true, f =&gt; new {{TotalAmount = f.TotalAmount + x.Values(f.TotalAmount)}})");
                            if (callExpr.Arguments[0].Type == typeof(bool))
                            {
                                var condition = this.Evaluate<bool>(callExpr.Arguments[0]);
                                if (condition) this.VisitAndDeferred(new SqlSegment { Expression = callExpr.Arguments[1] });
                            }
                            else
                            {
                                //Set<TField>(Expression<Func<TEntity, TField>> fieldSelector, Expression<Func<TEntity, TField>> fieldValueSelector)
                                this.VisitSetFieldExpression(callExpr.Arguments[0], callExpr.Arguments[1]);
                            }
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
                            else
                            {
                                var leftSegment = this.VisitAndDeferred(new SqlSegment { Expression = callExpr.Arguments[0] });
                                this.VisitWithSetField(callExpr.Arguments[1], this.Evaluate(callExpr.Arguments[2]));
                            }
                        }
                    }
                    break;
            }
        }
        this.UpdateFields.Insert(0, " ON DUPLICATE KEY UPDATE ").ToString();
        if (this.IsSetAlias && isNeedAlias) this.UpdateFields.Insert(0, $" AS {this.SetRowAlias}");
        this.IsUpdate = false;
    }
    public override SqlSegment VisitNew(SqlSegment sqlSegment)
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
                sqlSegment = this.VisitAndDeferred(new SqlSegment { Expression = newExpr.Arguments[i], MemberMapper = memberMapper });
                this.AddMemberElement(sqlSegment, memberMapper);
            }
            return sqlSegment;
        }
        return this.Evaluate(sqlSegment);
    }
    public override SqlSegment VisitMemberInit(SqlSegment sqlSegment)
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
            sqlSegment = this.VisitAndDeferred(new SqlSegment { Expression = memberAssignment.Expression, MemberMapper = memberMapper });
            this.AddMemberElement(sqlSegment, memberMapper);
        }
        return this.Evaluate(sqlSegment);
    }
    public override IQueryVisitor CreateQueryVisitor()
    {
        var queryVisiter = new MySqlQueryVisitor(this.DbKey, this.OrmProvider, this.MapProvider, this.ShardingProvider, this.IsParameterized, this.TableAsStart, this.ParameterPrefix, this.DbParameters);
        queryVisiter.IsMultiple = this.IsMultiple;
        queryVisiter.CommandIndex = this.CommandIndex;
        queryVisiter.RefQueries = this.RefQueries;
        queryVisiter.ShardingTables = this.ShardingTables;
        queryVisiter.IsUseIgnoreInto = this.IsUseIgnoreInto;
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
            this.TableAliases.TryAdd(parameterExpr.Name, this.Tables[0]);
        }
    }
    public void AddMemberElement(SqlSegment sqlSegment, MemberMap memberMapper)
    {
        if (this.UpdateFields.Length > 0) this.UpdateFields.Append(',');
        var fieldName = this.OrmProvider.GetFieldName(memberMapper.FieldName);

        if (sqlSegment == SqlSegment.Null)
            this.UpdateFields.Append($"{fieldName}=NULL");
        else if (sqlSegment.IsConstant || sqlSegment.IsVariable)
        {
            var parameterName = this.OrmProvider.ParameterPrefix + this.ParameterPrefix + this.DbParameters.Count.ToString();
            if (this.IsMultiple) parameterName += $"_m{this.CommandIndex}";
            //this.OrmProvider.AddDbParameter(this.DbKey, this.DbParameters, memberMapper, parameterName, sqlSegment.Value);
            var dbFieldValue = memberMapper.TypeHandler.ToFieldValue(this.OrmProvider, memberMapper.UnderlyingType, sqlSegment.Value);
            this.DbParameters.Add(this.OrmProvider.CreateParameter(parameterName, memberMapper.NativeDbType, dbFieldValue));
            this.UpdateFields.Append($"{fieldName}={parameterName}");
        }
        //带有参数或字段的表达式或函数调用、或是只有参数或字段
        //.Set(true, f => f.TotalAmount))
        //.Set(f => new { TotalAmount = x.Values(f.TotalAmount) })
        else this.UpdateFields.Append($"{fieldName}={sqlSegment.Value.ToString()}");
    }
    public void VisitSetFieldExpression(Expression fieldSelector, Expression fieldValueSelector)
    {
        var fieldSegment = this.VisitAndDeferred(new SqlSegment { Expression = fieldSelector });
        var valueSegment = this.VisitAndDeferred(new SqlSegment { Expression = fieldValueSelector });
        if (this.UpdateFields.Length > 0) this.UpdateFields.Append(',');
        this.UpdateFields.Append($"{fieldSegment}={valueSegment}");
    }
    public void VisitWithSetField(Expression fieldSelector, object fieldValue)
    {
        var lambdaExpr = this.EnsureLambda(fieldSelector);
        var memberExpr = lambdaExpr.Body as MemberExpression;
        var entityMapper = this.Tables[0].Mapper;
        var memberMapper = entityMapper.GetMemberMap(memberExpr.Member.Name);
        var parameterName = this.OrmProvider.ParameterPrefix + memberMapper.MemberName;
        if (this.IsMultiple) parameterName += $"_m{this.CommandIndex}";
        //在前面insert的时候，参数有可能已经添加过了，此处需要判断是否需要添加
        if (!this.DbParameters.Contains(parameterName))
        {
            //this.OrmProvider.AddDbParameter(this.DbKey, this.DbParameters, memberMapper, parameterName, fieldValue);
            var dbFieldValue = memberMapper.TypeHandler.ToFieldValue(this.OrmProvider, memberMapper.UnderlyingType, fieldValue);
            this.DbParameters.Add(this.OrmProvider.CreateParameter(parameterName, memberMapper.NativeDbType, dbFieldValue));
        }
        if (this.UpdateFields.Length > 0) this.UpdateFields.Append(',');
        this.UpdateFields.Append($"{this.OrmProvider.GetFieldName(memberMapper.FieldName)}={parameterName}");
    }
    private (string, List<ReaderField>) BuildOutputSqlReaderFields()
    {
        var readerFields = new List<ReaderField>();
        var entityMapper = this.Tables[0].Mapper;
        var builder = new StringBuilder();
        Action<MemberMap> addReaderField = memberMapper =>
        {
            readerFields.Add(new ReaderField
            {
                FieldType = ReaderFieldType.Field,
                FromMember = memberMapper.Member,
                TargetMember = memberMapper.Member,
                TargetType = memberMapper.MemberType,
                NativeDbType = memberMapper.NativeDbType,
                TypeHandler = memberMapper.TypeHandler,
                Body = memberMapper.FieldName
            });
        };
        builder.Append(" RETURNING ");
        for (int i = 0; i < this.OutputFieldNames.Count; i++)
        {
            var fieldName = this.OutputFieldNames[i];
            if (i > 0) builder.Append(',');
            builder.Append(fieldName);

            if (fieldName == "*")
            {
                foreach (var memberMapper in entityMapper.MemberMaps)
                {
                    if (memberMapper.IsIgnore || memberMapper.IsNavigation
                        || (memberMapper.MemberType.IsEntityType(out _) && memberMapper.TypeHandler == null))
                        continue;
                    addReaderField.Invoke(memberMapper);
                }
            }
            else
            {
                var memberMapper = entityMapper.GetMemberMapByFieldName(fieldName);
                addReaderField.Invoke(memberMapper);
            }
        }
        var sql = builder.ToString();
        builder.Clear();
        builder = null;
        return (sql, readerFields);
    }
}
