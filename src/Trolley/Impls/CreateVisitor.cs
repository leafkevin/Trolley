using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Trolley;

public class CreateVisitor : SqlVisitor, ICreateVisitor
{
    protected List<CommandSegment> deferredSegments = new();
    public StringBuilder FieldsBuilder { get; set; } = new();
    public StringBuilder ValuesBuilder { get; set; }

    public List<string> OnlyFieldNames { get; set; }
    public List<string> IgnoreFieldNames { get; set; }
    public ActionMode ActionMode { get; set; }
    public bool IsReturnIdentity { get; set; }

    public CreateVisitor(DbContext dbContext, char tableAsStart = 'a')
    {
        this.DbContext = dbContext;
        this.TableAsStart = tableAsStart;
    }
    public virtual void Initialize(Type entityType)
    {
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
        (var isNeetSplit, var tableName, var tabledInsertObjs, var insertObjs, _,
            var firstSqlSetter, var loopSqlSetter, var tailSql, readerFields) = this.BuildWithBulk();
        var builder = new StringBuilder();
        if (isNeetSplit)
        {
            foreach (var tabledInsertObj in tabledInsertObjs)
            {
                firstSqlSetter.Invoke(command.Parameters, builder, tabledInsertObj.Key);
                var index = 0;
                foreach (var insertObj in tabledInsertObj.Value)
                {
                    loopSqlSetter.Invoke(command.Parameters, builder, this.DbContext, insertObj, index);
                    index++;
                }
            }
        }
        else
        {
            firstSqlSetter.Invoke(command.Parameters, builder, tableName);
            var index = 0;
            foreach (var insertObj in insertObjs)
            {
                loopSqlSetter.Invoke(command.Parameters, builder, this.DbContext, insertObj, index);
                index++;
            }
        }
        this.FieldsBuilder.Clear();
        this.ValuesBuilder.Clear();
        return builder.ToString();
    }
    public virtual (bool, string, Dictionary<string, List<object>>, IEnumerable, int, Action<IDataParameterCollection, StringBuilder, string>,
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
        var hasOnlyFields = this.OnlyFieldNames != null && this.OnlyFieldNames.Count > 0;
        var hasIgnoreFields = this.IgnoreFieldNames != null && this.IgnoreFieldNames.Count > 0;
        var valueFieldSegments = new List<ValueFieldSegment>();
        Action<IDataParameterCollection, StringBuilder, DbContext, object, int> loopSqlSetter = null;

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

        var entityMapper = tableSegment.Mapper;
        if (firstInsertObj is IDictionary<string, object> dict)
        {
            foreach (var key in dict.Keys)
            {
                var fieldName = key.ToLower();
                if (hasIgnoreFields && this.IgnoreFieldNames.Exists(f => f.ToLower() == fieldName))
                    continue;
                if (hasOnlyFields && !this.OnlyFieldNames.Exists(f => f.ToLower() == fieldName))
                    continue;
                if (!entityMapper.TryGetMemberMap(key, out var memberMapper))
                    continue;
                if (memberMapper.IsAutoIncrement || memberMapper.IsIgnore || memberMapper.IsIgnoreInsert) 
                    continue;

                Func<object, object> valueGetter = null;
                if (memberMapper.TypeHandler != null)
                {
                    valueGetter = f =>
                    {
                        var myDict = f as IDictionary<string, object>;
                        return memberMapper.TypeHandler.ToFieldValue(this.OrmProvider, myDict[key]);
                    };
                }
                else
                {
                    var targetType = this.OrmProvider.MapDefaultType(memberMapper);
                    var myValueGetter = this.OrmProvider.GetParameterValueGetter(dict[key]?.GetType(), targetType, !memberMapper.IsRequired, this.DbContext);
                    valueGetter = f =>
                    {
                        var myDict = f as IDictionary<string, object>;
                        return myValueGetter.Invoke(myDict[key]);
                    };
                }
                valueFieldSegments.Add(new ValueFieldSegment(memberMapper, valueGetter));
            }
        }
        else
        {
            var memberInfos = insertObjType.GetMembers(BindingFlags.Public | BindingFlags.Instance)
                .Where(f => f.MemberType == MemberTypes.Property || f.MemberType == MemberTypes.Field).ToList();
            foreach (var memberInfo in memberInfos)
            {
                var fieldName = memberInfo.Name.ToLower();
                if (hasIgnoreFields && this.IgnoreFieldNames.Exists(f => f.ToLower() == fieldName))
                    continue;
                if (hasOnlyFields && !this.OnlyFieldNames.Exists(f => f.ToLower() == fieldName))
                    continue;
                if (!entityMapper.TryGetMemberMap(fieldName, out var memberMapper))
                    continue;
                if (memberMapper.IsAutoIncrement || memberMapper.IsIgnore || memberMapper.IsIgnoreInsert)
                    continue;

                Func<object, object> valueGetter = null;
                if (memberMapper.TypeHandler != null)
                {
                    valueGetter = f =>
                    {
                        var fieldValue = FasterEvaluator.EvaluateAndCache(f, memberInfo);
                        return memberMapper.TypeHandler.ToFieldValue(this.OrmProvider, fieldValue);
                    };
                }
                else
                {
                    var targetType = this.OrmProvider.MapDefaultType(memberMapper);
                    var myValueGetter = this.OrmProvider.GetParameterValueGetter(memberInfo.GetMemberType(), targetType, !memberMapper.IsRequired, this.DbContext);
                    valueGetter = f =>
                    {
                        var fieldValue = FasterEvaluator.EvaluateAndCache(f, memberInfo);
                        return myValueGetter.Invoke(fieldValue);
                    };
                }
                valueFieldSegments.Add(new ValueFieldSegment(memberMapper, valueGetter));
            }
        }

        loopSqlSetter = (dbParameters, builder, dbContext, insertObj, index) =>
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
        var isNeedSplit = false;
        var tableName = tableSegment.Mapper.TableName;
        Dictionary<string, List<object>> tabledInsertObjs = null;
        if (this.ShardingProvider != null && this.ShardingProvider.TryGetTableSharding(entityType, out var tableShardingInfo))
        {
            if (tableSegment.IsSharding)
                tableName = tableSegment.Body;
            else isNeedSplit = true;
        }
        else tableName = tableSegment.Mapper.TableName;

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
        return (isNeedSplit, tableName, tabledInsertObjs, insertObjs, bulkCount, firstSqlSetter, loopSqlSetter, null, null);
    }
    public virtual void VisitWithBy(object insertObj)
    {
        var entityType = this.Tables[0].EntityType;
        var insertObjType = insertObj.GetType();
        var hasOnlyFields = this.OnlyFieldNames != null && this.OnlyFieldNames.Count > 0;
        var hasIgnoreFields = this.IgnoreFieldNames != null && this.IgnoreFieldNames.Count > 0;
        var sqlParametersSetter = RepositoryHelper.BuildCreateSqlParametersPart(this.DbContext, entityType, insertObjType, true, hasOnlyFields, hasIgnoreFields)
            as Action<IDataParameterCollection, StringBuilder, StringBuilder, DbContext, List<string>, List<string>, object>;
        if (this.FieldsBuilder.Length > 0)
        {
            this.FieldsBuilder.Append(',');
            this.ValuesBuilder.Append(',');
        }
        sqlParametersSetter.Invoke(this.DbParameters, this.FieldsBuilder, this.ValuesBuilder, this.DbContext, this.OnlyFieldNames, this.IgnoreFieldNames, insertObj);
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

        var parameterName = this.OrmProvider.ParameterPrefix + memberMapper.MemberName;
        if (memberMapper.TypeHandler != null)
            fieldValue = memberMapper.TypeHandler.ToFieldValue(this.OrmProvider, fieldValue);
        else
        {
            var targetType = this.OrmProvider.MapDefaultType(memberMapper);
            var valueGetter = this.OrmProvider.GetParameterValueGetter(fieldValue.GetType(), targetType, false, this.DbContext);
            fieldValue = valueGetter.Invoke(fieldValue);
        }
        if (this.FieldsBuilder.Length > 0)
        {
            this.FieldsBuilder.Append(',');
            this.ValuesBuilder.Append(',');
        }
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
        if (this.ShardingProvider != null && this.ShardingProvider.TryGetTableSharding(entityType, out var tableShardingInfo))
        {
            //已经手动设置过分表
            if (tableSegment.IsSharding)
                tableName = tableSegment.Body;
            else
            {
                if (tableShardingInfo.DependOnMembers == null || tableShardingInfo.DependOnMembers.Count == 0)
                    throw new InvalidOperationException($"实体表{entityType.FullName}已设置分表，但未指定分表名，也未指定依赖的成员，无法确定分表，原表名：{tableName}");

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
    public Dictionary<string, List<object>> SplitShardingParameters(Type insertObjType, TableShardingInfo tableShardingInfo, IEnumerable insertObjs, object sampleObj)
    {
        var result = new Dictionary<string, List<object>>();
        var origTableName = this.Tables[0].Mapper.TableName;

        //优先使用本次设置的分表名获取委托来获取分表名
        if (this.Tables[0].ShardingTableGetter != null)
        {
            var tableNameGetter = this.Tables[0].ShardingTableGetter;
            foreach (var insertObj in insertObjs)
            {
                var tableName = tableNameGetter.DynamicInvoke(insertObj) as string;
                if (string.IsNullOrEmpty(tableName))
                    throw new InvalidOperationException($"手动设置的分表名获取委托无法获取分表名，原表名：{origTableName}，当前参数：{this.DbContext.JsonTypeHandler.ToFieldValue(this.OrmProvider, insertObj)}");
                if (!result.TryGetValue(tableName, out var myParameters))
                    result.Add(tableName, myParameters = new List<object>());
                myParameters.Add(insertObj);
            }
        }
        else
        {
            //使用分表规则获取分表名，根据依赖的字段值执行分表规则委托获取分表名
            if (tableShardingInfo.DependOnMembers == null || tableShardingInfo.DependOnMembers.Count == 0)
                throw new InvalidOperationException($"实体表{tableShardingInfo.EntityType.FullName}已设置分表，但未指定分表名，也未指定依赖的成员，无法确定分表，原表名：{origTableName}");

            var fieldValueGetters = new List<Func<object, object>>();
            bool TryAddMemberGetter(string memberName)
            {
                for (int i = 1; i < this.deferredSegments.Count; i++)
                {
                    var deferredSegment = this.deferredSegments[i];
                    switch (deferredSegment.Type)
                    {
                        case "WithBy":
                            var insertObj = deferredSegment.Value;
                            var myInsertObjType = insertObj.GetType();
                            if (RepositoryHelper.TryGetMemberGetter(myInsertObjType, memberName.ToLower(), insertObj, out var memberGetter))
                            {
                                fieldValueGetters.Add(f => memberGetter.Invoke(insertObj));
                                return true;
                            }
                            break;
                        case "WithByField":
                            (var fieldSelector, var fieldValue) = ((Expression, object))deferredSegment.Value;
                            var lambdaExpr = fieldSelector as LambdaExpression;
                            var memberExpr = this.EnsureMemberVisit(lambdaExpr.Body) as MemberExpression;
                            if (memberExpr.Member.Name == memberName)
                            {
                                fieldValueGetters.Add(f => fieldValue);
                                return true;
                            }
                            break;
                    }
                }
                return false;
            }

            foreach (var memberName in tableShardingInfo.DependOnMembers)
            {
                if (RepositoryHelper.TryGetMemberGetter(insertObjType, memberName.ToLower(), sampleObj, out var memberGetter))
                {
                    fieldValueGetters.Add(memberGetter);
                    continue;
                }
                if (!TryAddMemberGetter(memberName))
                    throw new InvalidOperationException($"实体表{tableShardingInfo.EntityType.FullName}已设置分表，依赖的成员{memberName}在插入对象类型{insertObjType.FullName}中不存在，无法确定分表，原表名：{origTableName}");
            }
            Func<object, string> tableNameGetter = insertObj =>
            {
                var fieldValus = new List<object>();
                foreach (var fieldValueGetter in fieldValueGetters)
                    fieldValus.Add(fieldValueGetter.Invoke(insertObj));
                return tableShardingInfo.Rule.Invoke(origTableName, fieldValus.ToArray()) as string;
            };

            foreach (var insertObj in insertObjs)
            {
                var tableName = tableNameGetter.Invoke(insertObj);
                if (string.IsNullOrEmpty(tableName))
                    throw new InvalidOperationException($"分表规则无法获取分表名，原表名：{origTableName}，当前参数：{this.DbContext.JsonTypeHandler.ToFieldValue(this.OrmProvider, insertObj)}");
                if (!result.TryGetValue(tableName, out var myParameters))
                    result.Add(tableName, myParameters = new List<object>());
                myParameters.Add(insertObj);
            }
        }
        return result;
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