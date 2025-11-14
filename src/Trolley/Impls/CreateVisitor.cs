using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Trolley;

public class CreateVisitor : SqlVisitor, ICreateVisitor
{
    protected List<CommandSegment> deferredSegments = new();
    protected bool isNeedSplitShardingTables = false;
    protected Dictionary<string, object> shardingValues = null;
    protected bool hasOnlyFields = false;
    protected bool hasIgnoreFields = false;

    public StringBuilder FieldsBuilder { get; set; } = new();
    public StringBuilder ValuesBuilder { get; set; }

    public List<string> OnlyFieldNames { get; set; }
    public List<string> IgnoreFieldNames { get; set; }
    public ActionMode ActionMode { get; set; }
    public bool IsReturnIdentity { get; set; }

    public CreateVisitor(Type entityType, DbContext dbContext, char tableAsStart = 'a')
    {
        this.DbContext = dbContext;
        this.TableAsStart = tableAsStart;
        this.Tables = new()
        {
            new TableSegment
            {
                TableType = TableType.Entity,
                EntityType = entityType,
                AliasName = "a",
                Mapper = this.MapProvider.GetEntityMap(entityType)
            }
        };
        if (this.TryGetTableShardingInfo(entityType, TableShardingUsageMode.WriteOnly, out var tableShardingInfo))
            this.Tables[0].TableShardingInfo = tableShardingInfo;
    }
    public virtual string BuildCommand(ITheaCommand command, bool isReturnIdentity, out List<SqlFieldSegment> readerFields)
    {
        string sql = null;
        readerFields = null;
        this.IsReturnIdentity = isReturnIdentity;
        if (this.ActionMode == ActionMode.Bulk)
            sql = this.BuildBulkSql(command, out readerFields);
        else
        {
            this.DbParameters ??= command.Parameters;
            this.ValuesBuilder = new();
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
                }
            }
            sql = this.BuildSql(out readerFields);
        }
        return sql;
    }
    public virtual string BuildSql(out List<SqlFieldSegment> readerFields)
    {
        readerFields = null;
        var tableName = this.GetTableName();
        var sql = $"INSERT INTO {tableName} ({this.FieldsBuilder}) VALUES ({this.ValuesBuilder})";
        if (this.IsReturnIdentity)
        {
            var entityMapper = this.Tables[0].Mapper;
            if (!entityMapper.IsAutoIncrementKey)
                throw new NotSupportedException($"实体{entityMapper.EntityType.FullName}表未配置自增长字段，无法返回Identity值");
            var keyFieldName = this.OrmProvider.GetFieldName(entityMapper.KeyMembers[0].FieldName);
            sql += this.OrmProvider.GetIdentitySql(keyFieldName);
        }
        this.FieldsBuilder.Clear();
        this.ValuesBuilder.Clear();
        return sql;
    }
    public virtual void WithBy(object insertObj)
    {
        this.ActionMode = ActionMode.Single;
        this.deferredSegments.Add(new CommandSegment
        {
            Type = "WithBy",
            Value = insertObj
        });
    }
    public virtual void WithByField(Expression fieldSelector, object fieldValue)
    {
        this.ActionMode = ActionMode.Single;
        this.deferredSegments.Add(new CommandSegment
        {
            Type = "WithByField",
            Value = (fieldSelector, fieldValue)
        });
    }
    public virtual void WithBulk(IEnumerable insertObjs, int bulkCount)
    {
        this.ActionMode = ActionMode.Bulk;
        this.deferredSegments.Add(new CommandSegment
        {
            Type = "WithBulk",
            Value = (insertObjs, bulkCount)
        });
    }
    public virtual void IgnoreFields(string[] fieldNames)
    {
        this.IgnoreFieldNames ??= new();
        this.IgnoreFieldNames.AddRange(fieldNames.Select(f => f.ToLower()));
    }
    public virtual void IgnoreFields(Expression fieldsSelector)
        => this.IgnoreFieldNames = this.VisitFields(fieldsSelector);
    public virtual void OnlyFields(string[] fieldNames)
    {
        this.OnlyFieldNames ??= new();
        this.OnlyFieldNames.AddRange(fieldNames.Select(f => f.ToLower()));
    }
    public virtual void OnlyFields(Expression fieldsSelector)
        => this.OnlyFieldNames = this.VisitFields(fieldsSelector);
    public virtual string BuildBulkSql(ITheaCommand command, out List<SqlFieldSegment> readerFields)
    {
        //多命令查询或是ToSql才会走到此分支
        //多语句执行，一次性不分批次
        (var shardingType, var shardingTables, var insertObjs, _, var firstSqlSetter,
            var loopSqlSetter, var tailSql, readerFields) = this.BuildWithBulk();
        var builder = new StringBuilder();

        int index = 0;
        if (shardingType == ShardingTableType.SplitTables)
        {
            var tabledInsertObjs = shardingTables as Dictionary<string, List<object>>;
            foreach (var tableName in tabledInsertObjs.Keys)
            {
                firstSqlSetter.Invoke(command.Parameters, builder, tableName);
                var tableParameters = tabledInsertObjs[tableName];
                foreach (var insertObj in tableParameters)
                {
                    loopSqlSetter.Invoke(command.Parameters, builder, this.DbContext, insertObj, index);
                    index++;
                }
            }
        }
        else
        {
            firstSqlSetter.Invoke(command.Parameters, builder, shardingTables as string);
            foreach (var insertObj in insertObjs)
            {
                loopSqlSetter.Invoke(command.Parameters, builder, this.DbContext, insertObj, index);
                index++;
            }
        }
        return builder.ToString();
    }
    public virtual (ShardingTableType, object, IEnumerable, int, Action<IDataParameterCollection, StringBuilder, string>,
        Action<IDataParameterCollection, StringBuilder, DbContext, object, int>, string, List<SqlFieldSegment>) BuildWithBulk()
    {
        (var insertObjs, var bulkCount) = ((IEnumerable, int))this.deferredSegments[0].Value;

        object firstInsertObj = null;
        Type insertObjType = null;
        foreach (var insertObj in insertObjs)
        {
            firstInsertObj = insertObj;
            insertObjType = insertObj.GetType();
            break;
        }

        var tableSegment = this.Tables[0];
        var entityType = tableSegment.EntityType;

        string headSql = "INSERT INTO ";
        if (!string.IsNullOrEmpty(tableSegment.TableSchema))
            headSql += this.OrmProvider.GetTableName(tableSegment.TableSchema) + ".";

        string fixedFieldsSql = "(", fixedValuesSql = ") VALUES (";
        List<IDbDataParameter> fixedDbParameters = null;
        Action<IDataParameterCollection, StringBuilder, string> firstSqlSetter = (dbParameters, builder, tableName) =>
        {
            builder.Append(headSql);
            builder.Append(this.OrmProvider.GetTableName(tableName));
            builder.Append(fixedFieldsSql);
            builder.Append(fixedValuesSql);
        };
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
            fixedFieldsSql += "," + this.FieldsBuilder.ToString();
            fixedValuesSql += this.ValuesBuilder.ToString() + ",";

            if (this.DbParameters.Count > 0)
            {
                fixedDbParameters = this.DbParameters.Cast<IDbDataParameter>().ToList();
                firstSqlSetter = (dbParameters, builder, tableName) =>
                {
                    builder.Append(headSql);
                    builder.Append(this.OrmProvider.GetTableName(tableName));
                    builder.Append(fixedFieldsSql);
                    builder.Append(fixedValuesSql);
                    fixedDbParameters.ForEach(f => dbParameters.Add(f));
                };
            }
        }

        this.hasOnlyFields = this.OnlyFieldNames != null && this.OnlyFieldNames.Count > 0;
        this.hasIgnoreFields = this.IgnoreFieldNames != null && this.IgnoreFieldNames.Count > 0;

        var valueSetters = new List<Action<IDataParameterCollection, StringBuilder, object, string>>();
        if (firstInsertObj is IDictionary<string, object> dict)
        {
            var entityMapper = this.DbContext.EntityMapProvider.GetEntityMap(entityType);
            var fieldBuilder = new StringBuilder("(");
            int index = 0;
            foreach (var key in dict.Keys)
            {
                if (!entityMapper.TryGetMemberMap(key, out var memberMapper) || memberMapper.IsIgnore
                   || memberMapper.IsAutoIncrement || memberMapper.IsNavigation || memberMapper.IsIgnoreInsert || memberMapper.IsRowVersion)
                    continue;
                var lowerMemberName = memberMapper.MemberName.ToLower();
                if (hasOnlyFields && !this.OnlyFieldNames.Contains(lowerMemberName) || hasIgnoreFields && this.IgnoreFieldNames.Contains(lowerMemberName))
                    continue;

                if (index > 0)
                {
                    this.FieldsBuilder.Append(',');
                    this.FieldsBuilder.Append(this.OrmProvider.GetFieldName(memberMapper.FieldName));
                }
                Func<IDictionary<string, object>, object> valueGetter = null;
                if (memberMapper.TypeHandler != null)
                    valueGetter = insertObj => memberMapper.TypeHandler.ToFieldValue(this.OrmProvider, insertObj[key]);
                else
                {
                    var targetType = memberMapper.MappedTargetType;
                    var fieldValueType = dict[key].GetType();
                    //使用闭包，避免每次获取Type类型
                    valueGetter = insertObj =>
                    {
                        var fieldValue = insertObj[key];
                        var myValueGetter = this.OrmProvider.GetParameterValueGetter(fieldValueType, targetType, !memberMapper.IsRequired, this.DbContext);
                        return myValueGetter.Invoke(fieldValue);
                    };
                }

                Action<IDataParameterCollection, StringBuilder, object, string> valueSetter = null;
                if (index > 0)
                {
                    valueSetter = (dbParameters, builder, insertObj, suffix) =>
                    {
                        var myDict = insertObj as IDictionary<string, object>;
                        var fieldValue = valueGetter.Invoke(myDict);
                        var parameterName = $"{this.OrmProvider.ParameterPrefix}{memberMapper.MemberName}{suffix}";
                        builder.Append(parameterName);
                        dbParameters.Add(this.OrmProvider.CreateParameter(parameterName, memberMapper.NativeDbType, fieldValue));
                    };
                }
                else
                {
                    valueSetter = (dbParameters, builder, insertObj, suffix) =>
                    {
                        var myDict = insertObj as IDictionary<string, object>;
                        var fieldValue = valueGetter.Invoke(myDict);
                        var parameterName = $"{this.OrmProvider.ParameterPrefix}{memberMapper.MemberName}{suffix}";
                        builder.Append(',');
                        builder.Append(parameterName);
                        dbParameters.Add(this.OrmProvider.CreateParameter(parameterName, memberMapper.NativeDbType, fieldValue));
                    };
                }
                valueSetters.Add(valueSetter);
                index++;
            }
        }
        var valueFieldsInitializer = RepositoryHelper.BuildWithBulkFilterFieldsCommandInitializer(this.DbContext, entityType, insertObjType, 1, hasOnlyFields, hasIgnoreFields)
            as Action<IDataParameterCollection, List<ValueFieldSegment>, DbContext, List<string>, List<string>, object>;
        valueFieldsInitializer.Invoke(this.DbParameters, valueFieldSegments, this.DbContext, this.OnlyFieldNames, this.IgnoreFieldNames, firstInsertObj);


        Func<DbContext, List<Action<IDataParameterCollection, StringBuilder, IDictionary<string, object>, string>>, Type, List<string>, List<string>, object, string> valueSetterFunc = null;
        valueSetterFunc = (dbContext, valueSetters, entityType, onlyFields, ignoreFields, parameter) =>
        {
            var ormProvider = dbContext.OrmProvider;
            var entityMapper = dbContext.EntityMapProvider.GetEntityMap(entityType);
            var typedParameter = parameter as IDictionary<string, object>;
            var fieldBuilder = new StringBuilder("(");
            int index = 0;
            foreach (var key in typedParameter.Keys)
            {
                if (!entityMapper.TryGetMemberMap(key, out var memberMapper) || memberMapper.IsIgnore
                   || memberMapper.IsAutoIncrement || memberMapper.IsNavigation || memberMapper.IsIgnoreInsert || memberMapper.IsRowVersion)
                    continue;
                var lowerMemberName = memberMapper.MemberName.ToLower();
                if (hasOnlyFields && !onlyFields.Contains(lowerMemberName) || hasIgnoreFields && ignoreFields.Contains(lowerMemberName))
                    continue;

                if (index > 0) fieldBuilder.Append(',');
                fieldBuilder.Append(ormProvider.GetFieldName(memberMapper.FieldName));
                Func<object, object> valueGetter = null;
                if (memberMapper.TypeHandler != null)
                    valueGetter = fieldValue => memberMapper.TypeHandler.ToFieldValue(ormProvider, fieldValue);
                else
                {
                    var targetType = memberMapper.MappedTargetType;
                    valueGetter = fieldValue =>
                    {
                        var myValueGetter = ormProvider.GetParameterValueGetter(fieldValue.GetType(), targetType, false, this.DbContext);
                        return valueGetter.Invoke(fieldValue);
                    };
                }

                Action<IDataParameterCollection, StringBuilder, IDictionary<string, object>, string> valueSetter = null;
                valueSetter = (dbParameters, builder, insertObj, suffix) =>
                {
                    var parameterName = $"{ormProvider.ParameterPrefix}{memberMapper.MemberName}{suffix}";
                    builder.Append(parameterName);
                    var fieldValue = valueGetter.Invoke(insertObj[key]);
                    dbParameters.Add(ormProvider.CreateParameter(parameterName, memberMapper.NativeDbType, fieldValue));
                };
                if (index > 0)
                {
                    valueSetter = (dbParameters, builder, insertObj, suffix) =>
                    {
                        builder.Append(',');
                        var parameterName = $"{ormProvider.ParameterPrefix}{memberMapper.MemberName}{suffix}";
                        builder.Append(parameterName);
                        var fieldValue = valueGetter.Invoke(insertObj[key]);
                        dbParameters.Add(ormProvider.CreateParameter(parameterName, memberMapper.NativeDbType, fieldValue));
                    };
                }
                valueSetters.Add(valueSetter);
                index++;
            }
            return fieldBuilder.ToString();
        };


        Action<IDataParameterCollection, StringBuilder, DbContext, object, int> loopSqlSetter = (dbParameters, builder, dbContext, insertObj, index) =>
            {
                if (index > 0) builder.Append(',');
                for (int i = 0; i < valueFieldSegments.Count; i++)
                {
                    if (i > 0) builder.Append(',');
                    var valueFieldSegment = valueFieldSegments[i];
                    var myParameterName = this.OrmProvider.ParameterPrefix + valueFieldSegment.MemberMapper.MemberName + index.ToString();
                    builder.Append(myParameterName);
                    var fieldValue = valueFieldSegment.ValueGetter.Invoke(insertObj);
                    dbParameters.Add(this.OrmProvider.CreateParameter(myParameterName, valueFieldSegment.MemberMapper.NativeDbType, fieldValue));
                }
            };


        var shardingType = ShardingTableType.None;
        object shardingTables = tableSegment.Mapper.TableName;
        if (tableSegment.TableShardingInfo != null)
        {
            if (tableSegment.IsSharding)
            {
                if (!string.IsNullOrEmpty(tableSegment.Body))
                {
                    shardingTables = tableSegment.Body;
                    shardingType = ShardingTableType.SingleTable;
                }
                else if (tableSegment.TableNames != null && tableSegment.TableNames.Count > 0)
                    throw new NotSupportedException($"实体表{entityType.FullName}已设置分表，数据插入不能设置多个分表，原始表：{tableSegment.Mapper.TableName}");
            }
            else
            {
                shardingType = ShardingTableType.SplitTables;
                shardingTables = this.SplitShardingParameters(insertObjType, insertObjs, this.shardingValues);
            }
        }
        return (shardingType, shardingTables, insertObjs, bulkCount, firstSqlSetter, loopSqlSetter, null, null);
    }
    public virtual void VisitWithBy(object insertObj)
    {
        var tableSegment = this.Tables[0];
        var entityType = tableSegment.EntityType;
        var insertObjType = insertObj.GetType();
        var hasOnlyFields = this.OnlyFieldNames != null && this.OnlyFieldNames.Count > 0;
        var hasIgnoreFields = this.IgnoreFieldNames != null && this.IgnoreFieldNames.Count > 0;
        if (insertObj is IDictionary<string, object> dict)
        {
            var entityMapper = this.DbContext.EntityMapProvider.GetEntityMap(entityType);
            var fieldBuilder = new StringBuilder("(");
            int index = 0;
            foreach (var key in dict.Keys)
            {
                if (!entityMapper.TryGetMemberMap(key, out var memberMapper) || memberMapper.IsIgnore
                   || memberMapper.IsAutoIncrement || memberMapper.IsNavigation || memberMapper.IsIgnoreInsert || memberMapper.IsRowVersion)
                    continue;
                var lowerMemberName = memberMapper.MemberName.ToLower();
                if (hasOnlyFields && !this.OnlyFieldNames.Contains(lowerMemberName) || hasIgnoreFields && this.IgnoreFieldNames.Contains(lowerMemberName))
                    continue;

                if (index > 0) fieldBuilder.Append(',');
                fieldBuilder.Append(this.OrmProvider.GetFieldName(memberMapper.FieldName));
                Func<object, object> valueGetter = null;
                if (memberMapper.TypeHandler != null)
                    valueGetter = fieldValue => memberMapper.TypeHandler.ToFieldValue(this.OrmProvider, fieldValue);
                else
                {
                    var targetType = memberMapper.MappedTargetType;
                    valueGetter = fieldValue =>
                    {
                        var myValueGetter = this.OrmProvider.GetParameterValueGetter(fieldValue.GetType(), targetType, false, this.DbContext);
                        return valueGetter.Invoke(fieldValue);
                    };
                }
                var fieldValue = dict[key];
                if (this.isNeedSplitShardingTables && tableSegment.TableShardingInfo.DependOnMembers.Contains(memberMapper.MemberName))
                    this.shardingValues[memberMapper.MemberName] = fieldValue;
                var parameterName = this.OrmProvider.ParameterPrefix + memberMapper.MemberName;
                fieldValue = valueGetter.Invoke(fieldValue);
                this.DbParameters.Add(this.OrmProvider.CreateParameter(parameterName, memberMapper.NavigationType, fieldValue));
                index++;
            }
        }
        else
        {

        }

        var commandInitializer = RepositoryHelper.BuildWithFilterFieldsCommandInitializer(this.DbContext, entityType, insertObjType, 1, this.isNeedSplitShardingTables, hasOnlyFields, hasIgnoreFields);
        if (this.isNeedSplitShardingTables)
        {
            var typedCommandInitializer = commandInitializer as Action<IDataParameterCollection, StringBuilder, StringBuilder, IDictionary<string, object>, DbContext, List<string>, List<string>, object>;
            typedCommandInitializer.Invoke(this.DbParameters, this.FieldsBuilder, this.ValuesBuilder, this.shardingDependOnValues, this.DbContext, this.OnlyFieldNames, this.IgnoreFieldNames, insertObj);
        }
        else
        {
            var typedCommandInitializer = commandInitializer as Action<IDataParameterCollection, StringBuilder, StringBuilder, DbContext, List<string>, List<string>, object>;
            typedCommandInitializer.Invoke(this.DbParameters, this.FieldsBuilder, this.ValuesBuilder, this.DbContext, this.OnlyFieldNames, this.IgnoreFieldNames, insertObj);
        }
    }
    public virtual void VisitWithByField(object deferredSegmentValue)
    {
        (var fieldSelector, var fieldValue) = ((Expression, object))deferredSegmentValue;
        var lambdaExpr = fieldSelector as LambdaExpression;
        var memberExpr = this.EnsureMemberVisit(lambdaExpr.Body) as MemberExpression;
        var entityMapper = this.Tables[0].Mapper;
        var memberMapper = entityMapper.GetMemberMap(memberExpr.Member.Name);
        if (memberMapper.IsIgnore || memberMapper.IsIgnoreInsert)
            throw new NotSupportedException($"当前字段{memberMapper.FieldName}被忽略插入，IsIgnore：{memberMapper.IsIgnore}，IsIgnoreInsert：{memberMapper.IsIgnoreInsert}");
        if (memberMapper.IsRowVersion)
            throw new NotSupportedException($"当前字段{memberMapper.FieldName}为RowVersion类型，不允许插入");

        if (memberMapper.TypeHandler != null)
            fieldValue = memberMapper.TypeHandler.ToFieldValue(this.OrmProvider, fieldValue);
        else
        {
            var targetType = memberMapper.MappedTargetType;
            var valueGetter = this.OrmProvider.GetParameterValueGetter(fieldValue.GetType(), targetType, false, this.DbContext);
            fieldValue = valueGetter.Invoke(fieldValue);
        }
        if (this.FieldsBuilder.Length > 0)
        {
            this.FieldsBuilder.Append(',');
            this.ValuesBuilder.Append(',');
        }
        var parameterName = this.OrmProvider.ParameterPrefix + memberMapper.MemberName;
        this.FieldsBuilder.Append(this.OrmProvider.GetFieldName(memberMapper.FieldName));
        this.ValuesBuilder.Append(parameterName);
        this.DbParameters.Add(this.OrmProvider.CreateParameter(parameterName, memberMapper.NativeDbType, fieldValue));
    }
    public virtual List<string> VisitFields(Expression fieldsSelector, bool isIgnoreCase = true)
    {
        var lambdaExpr = fieldsSelector as LambdaExpression;
        var entityMapper = this.Tables[0].Mapper;
        this.TableAliases.Clear();
        this.TableAliases.Add(lambdaExpr.Parameters[0].Name, this.Tables[0]);
        var fieldNames = new List<string>();
        switch (lambdaExpr.Body.NodeType)
        {
            case ExpressionType.New:
                var newExpr = lambdaExpr.Body as NewExpression;
                for (int i = 0; i < newExpr.Arguments.Count; i++)
                {
                    var memberInfo = newExpr.Members[i];
                    if (!entityMapper.TryGetMemberMap(memberInfo.Name, out var memberMapper))
                        continue;

                    if (newExpr.Arguments[i] is not MemberExpression memberExpr)
                        throw new NotSupportedException($"不支持的表达式访问，只支持MemberAccess访问，Path:{newExpr.Arguments[i]}");
                    var fieldName = memberMapper.FieldName;
                    if (isIgnoreCase) fieldName = fieldName.ToLower();
                    fieldNames.Add(fieldName);
                }
                break;
            case ExpressionType.MemberInit:
                var memberInitExpr = lambdaExpr.Body as MemberInitExpression;
                for (int i = 0; i < memberInitExpr.Bindings.Count; i++)
                {
                    var memberAssignment = memberInitExpr.Bindings[i] as MemberAssignment;
                    if (!entityMapper.TryGetMemberMap(memberAssignment.Member.Name, out var memberMapper))
                        continue;

                    if (memberAssignment.Expression is not MemberExpression memberExpr)
                        throw new NotSupportedException($"不支持的表达式访问，只支持MemberAccess访问，Path:{memberAssignment.Expression}");
                    var fieldName = memberMapper.FieldName;
                    if (isIgnoreCase) fieldName = fieldName.ToLower();
                    fieldNames.Add(fieldName);
                }
                break;
        }
        if (fieldNames.Count > 0)
            return fieldNames;
        return null;
    }
    public virtual string GetTableName()
    {
        var tableSegment = this.Tables[0];
        var entityType = tableSegment.EntityType;
        string tableName = tableSegment.Mapper.TableName;
        if (tableSegment.TableShardingInfo != null)
        {
            //已经手动设置过分表
            if (tableSegment.IsSharding)
                tableName = tableSegment.Body;
            else
            {
                if (tableSegment.ShardingValuesSelectorExpression == null)
                {
                    if (tableSegment.TableShardingInfo.DependOnMembers == null || tableSegment.TableShardingInfo.DependOnMembers.Count == 0)
                        throw new InvalidOperationException($"实体表{entityType.FullName}已设置分表，但未指定分表名，也未指定依赖成员，无法确定分表，原表名：{tableName}");
                    this.shardingValues = new();
                }
                //未设置，就要根据依赖字段确定分表
                var fieldValues = new List<object>();
                foreach (var dependOnMember in tableShardingInfo.DependOnMembers)
                {
                    for (int i = 0; i < this.deferredSegments.Count; i++)
                    {
                        var deferredSegment = this.deferredSegments[i];
                        switch (deferredSegment.Type)
                        {
                            case "WithBy":
                                var insertObj = deferredSegment.Value;
                                var insertObjType = insertObj.GetType();
                                if (RepositoryHelper.TryGetMemberGetter(insertObjType, dependOnMember.ToLower(), insertObj, out var memberGetter))
                                    fieldValues.Add(memberGetter.Invoke(insertObj));
                                break;
                            case "WithByField":
                                (var fieldSelector, var fieldValue) = ((Expression, object))deferredSegment.Value;
                                var lambdaExpr = fieldSelector as LambdaExpression;
                                var memberExpr = this.EnsureMemberVisit(lambdaExpr.Body) as MemberExpression;
                                if (memberExpr.Member.Name == dependOnMember)
                                    fieldValues.Add(fieldValue);
                                break;
                        }
                    }
                }
                tableName = tableShardingInfo.Rule.Invoke(tableName, fieldValues.ToArray()) as string;
            }
        }
        if (!string.IsNullOrEmpty(tableSegment.TableSchema))
            tableName = $"{this.OrmProvider.GetTableName(tableSegment.TableSchema)}.{this.OrmProvider.GetTableName(tableName)}";
        else tableName = this.OrmProvider.GetTableName(tableName);
        return tableName;
    }
    public virtual void BuildDictCommandIntializer(bool hasOnlyFields, bool hasIgnoreFields)
    {
        var valueSetters = new List<Action<IDataParameterCollection, StringBuilder, object, string>>();
        var entityType = this.Tables[0].EntityType;
        var entityMapper = this.DbContext.EntityMapProvider.GetEntityMap(entityType);

        Action<IDataParameterCollection, StringBuilder, object, string> dictCommandInitializer = (dbParameters, fieldBuilder, insertObj, suffix) =>
        {
            int index = 0;
            var dict = insertObj as IDictionary<string, object>;
            foreach (var key in dict.Keys)
            {
                if (!entityMapper.TryGetMemberMap(key, out var memberMapper) || memberMapper.IsIgnore
                   || memberMapper.IsAutoIncrement || memberMapper.IsNavigation || memberMapper.IsIgnoreInsert || memberMapper.IsRowVersion)
                    continue;
                var lowerMemberName = memberMapper.MemberName.ToLower();
                if (hasOnlyFields && !this.OnlyFieldNames.Contains(lowerMemberName))
                    continue;
                if (hasIgnoreFields && this.IgnoreFieldNames.Contains(lowerMemberName))
                    continue;

                if (index > 0) fieldBuilder.Append(',');
                fieldBuilder.Append(this.OrmProvider.GetFieldName(memberMapper.FieldName));
                Func<object, object> valueGetter = null;
                if (memberMapper.TypeHandler != null)
                    valueGetter = fieldValue => memberMapper.TypeHandler.ToFieldValue(this.OrmProvider, fieldValue);
                else
                {
                    var targetType = memberMapper.MappedTargetType;
                    var fieldValueType = dict[key].GetType();
                    //使用闭包，避免每次获取Type类型
                    valueGetter = fieldValue =>
                    {
                        var myValueGetter = this.OrmProvider.GetParameterValueGetter(fieldValueType, targetType, !memberMapper.IsRequired, this.DbContext);
                        return valueGetter.Invoke(fieldValue);
                    };
                }

                Action<IDataParameterCollection, StringBuilder, object, string> valueSetter = null;
                valueSetter = (dbParameters, builder, insertObj, suffix) =>
                {
                    var parameterName = $"{this.OrmProvider.ParameterPrefix}{memberMapper.MemberName}{suffix}";
                    builder.Append(parameterName);
                    var myDict = insertObj as IDictionary<string, object>;
                    var fieldValue = valueGetter.Invoke(myDict[key]);
                    dbParameters.Add(this.OrmProvider.CreateParameter(parameterName, memberMapper.NativeDbType, fieldValue));
                };
                if (index > 0)
                {
                    valueSetter = (dbParameters, builder, insertObj, suffix) =>
                    {
                        builder.Append(',');
                        var parameterName = $"{this.OrmProvider.ParameterPrefix}{memberMapper.MemberName}{suffix}";
                        builder.Append(parameterName);
                        var myDict = insertObj as IDictionary<string, object>;
                        var fieldValue = valueGetter.Invoke(myDict[key]);
                        dbParameters.Add(this.OrmProvider.CreateParameter(parameterName, memberMapper.NativeDbType, fieldValue));
                    };
                }
                valueSetters.Add(valueSetter);
                index++;
            }
        };


    }


    public override void Dispose()
    {
        base.Dispose();
        this.deferredSegments = null;
        this.FieldsBuilder = null;
        this.ValuesBuilder = null;
        this.OnlyFieldNames = null;
        this.IgnoreFieldNames = null;
    }
    public override IQueryVisitor CreateQueryVisitor(char? tableAsStart = null)
    {
        var queryVisitor = this.OrmProvider.NewQueryVisitor(this.DbContext, tableAsStart ?? this.TableAsStart, this.DbParameters);
        queryVisitor.RefQueries = this.RefQueries;
        queryVisitor.ShardingTables = this.ShardingTables;
        queryVisitor.RefTableAliases = this.RefTableAliases;
        queryVisitor.IncludeTables = this.IncludeTables;
        queryVisitor.IsRecursive = this.IsRecursive;
        queryVisitor.CteQueryObj = this.CteQueryObj;
        queryVisitor.RefFrom = this;

        queryVisitor.Tables = this.Tables;
        return queryVisitor;
    }
}