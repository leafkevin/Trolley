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
    public virtual string BuildCommand(ITheaCommand command, out List<SqlFieldSegment> readerFields)
    {
        string sql = null;
        readerFields = null;
        this.hasOnlyFields = this.OnlyFieldNames != null && this.OnlyFieldNames.Count > 0;
        this.hasIgnoreFields = this.IgnoreFieldNames != null && this.IgnoreFieldNames.Count > 0;

        switch (this.ActionMode)
        {
            case ActionMode.Bulk:
                (var shardingType, var shardingTables, var insertObjs, _, var firstSqlSetter,
                    var loopSqlSetter, var tailSql, readerFields) = this.BuildWithBulk(command);

                int index = 0;
                var builder = new StringBuilder();
                if (shardingType == ShardingTableType.SplitTables)
                {
                    var tabledInsertObjs = shardingTables as Dictionary<string, List<object>>;
                    foreach (var tableName in tabledInsertObjs.Keys)
                    {
                        firstSqlSetter.Invoke(command.Parameters, builder, tableName);
                        var tableParameters = tabledInsertObjs[tableName];
                        foreach (var insertObj in tableParameters)
                        {
                            loopSqlSetter.Invoke(command.Parameters, builder, this.DbContext, insertObj, index.ToString());
                            index++;
                        }
                    }
                }
                else
                {
                    firstSqlSetter.Invoke(command.Parameters, builder, shardingTables as string);
                    foreach (var insertObj in insertObjs)
                    {
                        loopSqlSetter.Invoke(command.Parameters, builder, this.DbContext, insertObj, index.ToString());
                        index++;
                    }
                }
                sql = builder.ToString();
                break;
            case ActionMode.Single:
                sql = this.BuildSql(command, out readerFields);
                break;
        }
        this.FieldsBuilder.Clear();
        this.ValuesBuilder.Clear();
        return sql;
    }
    public virtual string BuildSql(ITheaCommand command, out List<SqlFieldSegment> readerFields)
    {
        readerFields = null;
        this.DbParameters = command.Parameters;
        this.ValuesBuilder = new();
        var tableSegment = this.Tables[0];
        if (tableSegment.TableShardingInfo != null && !tableSegment.IsSharding)
        {
            this.IsNeedSplitShardingTables = true;
            this.ShardingValues = new();
        }
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

        var tableName = this.GetTableName(tableSegment);
        var sql = $"INSERT INTO {tableName} ({this.FieldsBuilder}) VALUES ({this.ValuesBuilder})";
        if (this.IsReturnIdentity)
        {
            var entityMapper = this.Tables[0].Mapper;
            if (!entityMapper.IsAutoIncrementKey)
                throw new NotSupportedException($"实体{entityMapper.EntityType.FullName}表未配置自增长字段，无法返回Identity值");
            var keyFieldName = this.OrmProvider.GetFieldName(entityMapper.KeyMembers[0].FieldName);
            sql += this.OrmProvider.GetIdentitySql(keyFieldName);
        }
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
    public virtual (ShardingTableType, object, IEnumerable, int, Action<IDataParameterCollection, StringBuilder, string>,
        Action<IDataParameterCollection, StringBuilder, DbContext, object, string>, string, List<SqlFieldSegment>) BuildWithBulk(ITheaCommand command)
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
        this.FieldsBuilder.Append('(');
        this.ValuesBuilder = new StringBuilder("(");

        var headSql = "INSERT INTO ";
        if (!string.IsNullOrEmpty(tableSegment.TableSchema))
            headSql += this.OrmProvider.GetTableName(tableSegment.TableSchema) + ".";
        List<IDbDataParameter> fixedDbParameters = null;

        if (this.deferredSegments.Count > 1)
        {
            if (tableSegment.TableShardingInfo != null && !tableSegment.IsSharding
                && tableSegment.ShardingTableGetter == null && !tableSegment.IsUseOtherValuesTableSharding)
            {
                this.IsNeedSplitShardingTables = true;
                this.ShardingValues = new();
            }
            var tempDbParameters = new TheaDbParameterCollection();
            this.DbParameters = tempDbParameters;
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
            this.FieldsBuilder.Append(',');
            this.ValuesBuilder.Append(',');
            if (this.DbParameters.Count > 0)
                fixedDbParameters = tempDbParameters.ToList();
            this.DbParameters = command.Parameters;
        }

        string fixedFieldsSql = null;
        string fixedValuesSql = null;
        Action<IDataParameterCollection, StringBuilder, string> firstSqlSetter = null;
        Action<IDataParameterCollection, StringBuilder, DbContext, object, string> loopSqlSetter = null;
        List<Action<IDataParameterCollection, StringBuilder, IDictionary<string, object>, string>> valueSetters = null;
        if (firstInsertObj is IDictionary<string, object> dict)
        {
            var entityMapper = this.DbContext.EntityMapProvider.GetEntityMap(entityType);
            valueSetters = this.BuildDictBulkCommandInitializer(entityMapper, dict);
            loopSqlSetter = (dbParameters, builder, dbContext, insertObj, suffix) =>
            {
                var typedInsertObj = insertObj as IDictionary<string, object>;
                builder.Append(fixedValuesSql);
                foreach (var valueSetter in valueSetters)
                    valueSetter.Invoke(dbParameters, builder, typedInsertObj, suffix);
                builder.Append("),");
            };
        }
        else
        {
            (var fieldsSql, var sqlSetter) = ((string, Action<IDataParameterCollection, StringBuilder, DbContext, string, object, string>))
                RepositoryHelper.BuildTypedBulkCommandInitializer(this.DbContext, entityType, insertObjType, 1, this.OnlyFieldNames, this.IgnoreFieldNames);
            this.FieldsBuilder.Append(fieldsSql);
            loopSqlSetter = (dbParameters, builder, dbContext, insertObj, suffix) =>
            {
                sqlSetter.Invoke(dbParameters, builder, dbContext, fixedValuesSql, insertObj, suffix);
                builder.Append("),");
            };
        }
        this.FieldsBuilder.Append(") VALUES ");
        fixedFieldsSql = this.FieldsBuilder.ToString();
        fixedValuesSql = this.ValuesBuilder.ToString();

        if (fixedDbParameters != null && fixedDbParameters.Count > 0)
        {
            firstSqlSetter = (dbParameters, builder, tableName) =>
            {
                builder.Append(headSql);
                builder.Append(this.OrmProvider.GetTableName(tableName));
                builder.Append(fixedFieldsSql);
                builder.Append(fixedValuesSql);
                fixedDbParameters.ForEach(f => dbParameters.Add(f));
            };
        }
        else
        {
            firstSqlSetter = (dbParameters, builder, tableName) =>
            {
                builder.Append(headSql);
                builder.Append(this.OrmProvider.GetTableName(tableName));
                builder.Append(fixedFieldsSql);
                builder.Append(fixedValuesSql);
            };
        }
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
                shardingTables = this.SplitShardingParameters(insertObjType, insertObjs, this.ShardingValues);
            }
        }

        return (shardingType, shardingTables, insertObjs, bulkCount, firstSqlSetter, loopSqlSetter, null, null);
    }
    public virtual void VisitWithBy(object insertObj)
    {
        var tableSegment = this.Tables[0];
        var entityType = tableSegment.EntityType;
        var insertObjType = insertObj.GetType();
        if (insertObj is IDictionary<string, object> dict)
        {
            var entityMapper = this.DbContext.EntityMapProvider.GetEntityMap(entityType);
            foreach (var key in dict.Keys)
            {
                if (!entityMapper.TryGetMemberMap(key, out var memberMapper))
                    continue;

                var fieldValue = dict[key];
                if (this.IsNeedSplitShardingTables && tableSegment.TableShardingInfo.DependOnMembers.Contains(memberMapper.MemberName))
                    this.ShardingValues[memberMapper.MemberName] = fieldValue;

                if (memberMapper.IsIgnore || memberMapper.IsAutoIncrement
                    || memberMapper.IsNavigation || memberMapper.IsIgnoreInsert || memberMapper.IsRowVersion)
                    continue;

                if (this.hasOnlyFields || this.hasOnlyFields)
                {
                    var lowerMemberName = memberMapper.MemberName.ToLower();
                    if (this.hasOnlyFields && !this.OnlyFieldNames.Contains(lowerMemberName)
                        || this.hasIgnoreFields && this.IgnoreFieldNames.Contains(lowerMemberName))
                        continue;
                }

                if (this.FieldsBuilder.Length > 0)
                {
                    this.FieldsBuilder.Append(',');
                    this.ValuesBuilder.Append(',');
                }
                var parameterName = $"{this.OrmProvider.ParameterPrefix}{memberMapper.MemberName}";
                this.FieldsBuilder.Append(this.OrmProvider.GetFieldName(memberMapper.FieldName));
                this.ValuesBuilder.Append(parameterName);

                if (memberMapper.TypeHandler != null)
                    fieldValue = memberMapper.TypeHandler.ToFieldValue(fieldValue);
                else
                {
                    var targetType = memberMapper.MappedTargetType;
                    var fieldValueType = fieldValue.GetType();
                    if (fieldValueType != targetType)
                    {
                        var myValueGetter = this.OrmProvider.GetParameterValueGetter(fieldValueType, targetType, !memberMapper.IsRequired, this.DbContext);
                        fieldValue = myValueGetter.Invoke(fieldValue);
                    }
                }
                this.DbParameters.Add(this.OrmProvider.CreateParameter(parameterName, memberMapper.NativeDbType, fieldValue));
            }
        }
        else
        {
            var commandInitializer = RepositoryHelper.BuildTypedCommandInitializer(this.DbContext, entityType, insertObjType, 1, false, this.IsNeedSplitShardingTables, false, this.OnlyFieldNames, this.IgnoreFieldNames);
            if (this.IsNeedSplitShardingTables)
            {
                var typedCommandInitializer = commandInitializer as Action<IDataParameterCollection, StringBuilder, StringBuilder, IDictionary<string, object>, DbContext, object>;
                typedCommandInitializer.Invoke(this.DbParameters, this.FieldsBuilder, this.ValuesBuilder, this.ShardingValues, this.DbContext, insertObj);
            }
            else
            {
                var typedCommandInitializer = commandInitializer as Action<IDataParameterCollection, StringBuilder, StringBuilder, DbContext, object>;
                typedCommandInitializer.Invoke(this.DbParameters, this.FieldsBuilder, this.ValuesBuilder, this.DbContext, insertObj);
            }
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
            fieldValue = memberMapper.TypeHandler.ToFieldValue(fieldValue);
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
    public List<Action<IDataParameterCollection, StringBuilder, IDictionary<string, object>, string>> BuildDictBulkCommandInitializer(EntityMap entityMapper, IDictionary<string, object> dict)
    {
        var valueSetters = new List<Action<IDataParameterCollection, StringBuilder, IDictionary<string, object>, string>>();
        int index = 0;
        foreach (var key in dict.Keys)
        {
            if (!entityMapper.TryGetMemberMap(key, out var memberMapper) || memberMapper.IsIgnore
               || memberMapper.IsAutoIncrement || memberMapper.IsNavigation
               || memberMapper.IsIgnoreInsert || memberMapper.IsRowVersion)
                continue;

            if (this.hasOnlyFields || this.hasIgnoreFields)
            {
                var lowerMemberName = memberMapper.MemberName.ToLower();
                if (this.hasOnlyFields && !this.OnlyFieldNames.Contains(lowerMemberName)
                    || this.hasIgnoreFields && this.IgnoreFieldNames.Contains(lowerMemberName))
                    continue;
            }

            if (index > 0) this.FieldsBuilder.Append(',');
            this.FieldsBuilder.Append(this.OrmProvider.GetFieldName(memberMapper.FieldName));

            Func<IDictionary<string, object>, object> valueGetter = null;
            if (memberMapper.TypeHandler != null)
                valueGetter = insertObj => memberMapper.TypeHandler.ToFieldValue(insertObj[key]);
            else
            {
                var targetType = memberMapper.MappedTargetType;
                var fieldValueType = dict[key].GetType();
                if (fieldValueType.ToUnderlyingType() != targetType)
                {
                    var myValueGetter = this.OrmProvider.GetParameterValueGetter(fieldValueType, targetType, !memberMapper.IsRequired, this.DbContext);
                    valueGetter = insertObj => myValueGetter.Invoke(insertObj[key]);
                }
                else valueGetter = insertObj => insertObj[key];
            }

            Action<IDataParameterCollection, StringBuilder, IDictionary<string, object>, string> valueSetter = null;
            if (index > 0)
            {
                valueSetter = (dbParameters, builder, insertObj, suffix) =>
                {
                    var fieldValue = valueGetter.Invoke(insertObj);
                    var parameterName = $"{this.OrmProvider.ParameterPrefix}{memberMapper.MemberName}{suffix}";
                    builder.Append(parameterName);
                    dbParameters.Add(this.OrmProvider.CreateParameter(parameterName, memberMapper.NativeDbType, fieldValue));
                };
            }
            else
            {
                valueSetter = (dbParameters, builder, insertObj, suffix) =>
                {
                    var fieldValue = valueGetter.Invoke(insertObj);
                    var parameterName = $"{this.OrmProvider.ParameterPrefix}{memberMapper.MemberName}{suffix}";
                    builder.Append(',');
                    builder.Append(parameterName);
                    dbParameters.Add(this.OrmProvider.CreateParameter(parameterName, memberMapper.NativeDbType, fieldValue));
                };
            }
            valueSetters.Add(valueSetter);
            index++;
        }
        return valueSetters;
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