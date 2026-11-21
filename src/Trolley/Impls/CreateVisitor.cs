using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Text;

namespace Trolley;

public class CreateVisitor : SqlVisitor, ICreateVisitor
{
    private bool isDisposed = false;
    protected List<CommandSegment> deferredSegments = new();

    public ITheaCommand Command { get; set; }

    public StringBuilder FieldsBuilder { get; set; } = new();
    public StringBuilder ValuesBuilder { get; set; }
    public StringBuilder UpdateBuilder { get; set; }
    public int UpdateIndex { get; set; }
    public ActionMode ActionMode { get; set; }
    public bool IsReturnIdentity { get; set; }
    public string FromSql { get; set; }
    public string OutputSql { get; set; }
    public bool IsUpdate { get; set; }

    public bool IsNeedShardingValues { get; set; }
    public Dictionary<string, object> ShardingValues { get; set; }
    public Dictionary<string, object> FieldValues { get; set; }

    public CreateVisitor(Type entityType, DbContext dbContext, char tableAsStart = 'a', ITheaCommand command = null)
    {
        this.DbContext = dbContext;
        this.TableAliasStart = tableAsStart;
        this.Command = command;
        if (command != null)
            this.DbParameters = command.Parameters;
        this.Tables = new()
        {
            new TableSegment
            {
                TableType = TableType.Entity,
                EntityType = entityType,
                AliasName = "a",
                Mapper = this.EntityMapProvider.GetEntityMap(entityType)
            }
        };
        if (this.TryGetTableShardingInfo(entityType, out var tableShardingInfo))
            this.Tables[0].TableShardingInfo = tableShardingInfo;
    }
    public virtual string BuildSql(out ReusableList<ReaderField> readerFields)
    {
        readerFields = this.ReaderFields;
        switch (this.ActionMode)
        {
            case ActionMode.Bulk:
                (var shardingType, var shardingTables, var insertObjs, _, var firstSqlSetter,
                    var loopSqlSetter, var tailSql, readerFields) = this.BuildWithBulk(this.Command);

                int index = 0;
                this.FieldsBuilder.Clear();
                if (shardingType == ShardingTableType.SplitTables)
                {
                    var tabledInsertObjs = shardingTables as Dictionary<string, List<object>>;
                    foreach (var shardingTableName in tabledInsertObjs.Keys)
                    {
                        firstSqlSetter.Invoke(this.DbParameters, this.FieldsBuilder, shardingTableName);
                        var tableParameters = tabledInsertObjs[shardingTableName];
                        foreach (var insertObj in tableParameters)
                        {
                            loopSqlSetter.Invoke(this.DbParameters, this.FieldsBuilder, this.DbContext, insertObj, index.ToString());
                            index++;
                        }
                    }
                }
                else
                {
                    firstSqlSetter.Invoke(this.DbParameters, this.FieldsBuilder, shardingTables as string);
                    foreach (var insertObj in insertObjs)
                    {
                        loopSqlSetter.Invoke(this.DbParameters, this.FieldsBuilder, this.DbContext, insertObj, index.ToString());
                        index++;
                    }
                }
                this.FieldsBuilder.Remove(this.FieldsBuilder.Length - 1, 1);
                if (!string.IsNullOrEmpty(tailSql))
                    this.FieldsBuilder.Append(tailSql);
                this.FromSql = this.FieldsBuilder.ToString();
                break;
            case ActionMode.Single:
                var tableSegment = this.Tables[0];
                if (string.IsNullOrEmpty(this.FromSql))
                {
                    this.ValuesBuilder = new();
                    if (tableSegment.TableShardingInfo != null && !tableSegment.IsSharding && tableSegment.ShardingTableGetter == null)
                    {
                        if (tableSegment.TableShardingInfo.DependOnMembers == null || tableSegment.TableShardingInfo.DependOnMembers.Count == 0)
                            throw new Exception($"实体表{tableSegment.EntityType.FullName}已设置分表，未指定分表，也未设置依赖字段无法确定分表，请使用UseTable/UseTableBy方法手动指定分表");
                        if (this.deferredSegments.Count > 1)
                        {
                            this.IsNeedShardingValues = true;
                            this.ShardingValues = new();
                        }
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
                            case "WithByFieldExpr":
                                this.VisitWithByFieldExpr(deferredSegment.Value);
                                break;
                            case "SetObject":
                                this.VisitSetObject(deferredSegment.Value);
                                break;
                            case "SetObjectExpr":
                                this.VisitSetObjectExpr(deferredSegment.Value);
                                break;
                            case "SetField":
                                this.VisitSetField(deferredSegment.Value);
                                break;
                            case "SetFieldExpr":
                                this.VisitSetFieldExpr(deferredSegment.Value);
                                break;
                            case "SetFieldExprs":
                                this.VisitSetFieldExprs(deferredSegment.Value);
                                break;
                        }
                    }
                    this.FromSql = $"({this.FieldsBuilder}) VALUES ({this.ValuesBuilder})";
                }
                var tableName = this.GetTableName(tableSegment);
                if (this.IsReturnIdentity && (this.UpdateBuilder != null || this.OutputSql != null))
                    throw new NotSupportedException("返回Identity，不支持同时Returning操作");
                this.FromSql = $"{this.BuildHeadSql()} {tableName} {this.FromSql}";
                this.FieldsBuilder.Clear();
                this.ValuesBuilder.Clear();
                this.FieldsBuilder.Append(this.FromSql);

                if (this.UpdateBuilder != null)
                    this.FieldsBuilder.Append(this.UpdateBuilder.ToString());

                if (this.OutputSql != null)
                {
                    this.FieldsBuilder.Append(this.OutputSql);
                    readerFields = this.ReaderFields;
                }
                if (this.IsReturnIdentity)
                {
                    var entityMapper = tableSegment.Mapper;
                    if (!entityMapper.IsAutoIncrementKey)
                        throw new NotSupportedException($"实体{entityMapper.EntityType.FullName}表未配置自增长字段，无法返回Identity值");
                    var keyFieldName = this.OrmProvider.GetFieldName(entityMapper.KeyMembers[0].FieldName);
                    this.FieldsBuilder.Append(this.OrmProvider.GetIdentitySql(keyFieldName));
                }
                this.FromSql = this.FieldsBuilder.ToString();
                break;
        }
        return this.FromSql;
    }
    public virtual string BuildHeadSql() => "INSERT INTO";
    public virtual void WithBy(object insertObj)
    {
        this.deferredSegments.Add(new CommandSegment
        {
            Type = "WithBy",
            Value = insertObj
        });
    }
    public virtual void WithByField(string fieldName, object fieldValue)
    {
        this.deferredSegments.Add(new CommandSegment
        {
            Type = "WithByField",
            Value = (fieldName, fieldValue)
        });
    }
    public virtual void WithByFieldExpr(Expression fieldSelector, object fieldValue)
    {
        this.deferredSegments.Add(new CommandSegment
        {
            Type = "WithByFieldExpr",
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
    public virtual void SetField(string fieldName, object fieldValue)
    {
        this.deferredSegments.Add(new CommandSegment
        {
            Type = "SetField",
            Value = (fieldName, fieldValue)
        });
    }
    public virtual void SetObject(object updateObj)
    {
        this.deferredSegments.Add(new CommandSegment
        {
            Type = "SetObject",
            Value = updateObj
        });
    }
    public virtual void SetFieldExpr(Expression fieldSelector, object fieldValue)
    {
        this.deferredSegments.Add(new CommandSegment
        {
            Type = "SetFieldExpr",
            Value = (fieldSelector, fieldValue)
        });
    }
    public virtual void SetFieldExprs(Expression fieldSelector, Expression valueGetter)
    {
        this.deferredSegments.Add(new CommandSegment
        {
            Type = "SetFieldExprs",
            Value = (fieldSelector, valueGetter)
        });
    }
    public virtual void SetObjectExpr(Expression fieldsAssignment)
    {
        this.deferredSegments.Add(new CommandSegment
        {
            Type = "SetObjectExpr",
            Value = fieldsAssignment
        });
    }
    public virtual (ShardingTableType, object, IEnumerable, int, Action<IDataParameterCollection, StringBuilder, string>,
        Action<IDataParameterCollection, StringBuilder, DbContext, object, string>, string, ReusableList<ReaderField>) BuildWithBulk(ITheaCommand command)
    {
        (var insertObjs, var bulkCount) = ((IEnumerable, int))this.deferredSegments[0].Value;

        object firstInsertObj = null;
        foreach (var insertObj in insertObjs)
        {
            firstInsertObj = insertObj;
            break;
        }
        var insertObjType = firstInsertObj.GetType();
        var tableSegment = this.Tables[0];
        var entityType = tableSegment.EntityType;
        this.FieldsBuilder.Append('(');

        var headSql = $"{this.BuildHeadSql()} ";
        if (!string.IsNullOrEmpty(tableSegment.TableSchema))
            headSql += this.OrmProvider.GetTableName(tableSegment.TableSchema) + ".";
        List<IDbDataParameter> fixedDbParameters = null;

        string fixedFieldsSql = null;
        string fixedValuesSql = "(";
        if (tableSegment.TableShardingInfo != null && !tableSegment.IsSharding && tableSegment.ShardingTableGetter == null)
        {
            if (tableSegment.TableShardingInfo.DependOnMembers == null || tableSegment.TableShardingInfo.DependOnMembers.Count == 0)
                throw new Exception($"实体表{tableSegment.EntityType.FullName}已设置分表，未指定分表，也未设置依赖字段无法确定分表，请使用UseTable/UseTableBy方法手动指定分表，或是设置分表依赖字段");
            if (this.deferredSegments.Count > 1)
            {
                this.IsNeedShardingValues = true;
                this.ShardingValues = new();
            }
        }
        if (this.deferredSegments.Count > 1)
        {
            this.ValuesBuilder = new StringBuilder("(");
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
                    case "WithByFieldExpr":
                        this.VisitWithByFieldExpr(deferredSegment.Value);
                        break;
                    case "SetObject":
                        this.VisitSetObject(deferredSegment.Value);
                        break;
                    case "SetObjectExpr":
                        this.VisitSetObjectExpr(deferredSegment.Value);
                        break;
                    case "SetField":
                        this.VisitSetField(deferredSegment.Value);
                        break;
                    case "SetFieldExpr":
                        this.VisitSetFieldExpr(deferredSegment.Value);
                        break;
                    case "SetFieldExprs":
                        this.VisitSetFieldExprs(deferredSegment.Value);
                        break;
                    default: throw new NotSupportedException("批量插入后，只支持WithBy/OnDuplicateKeyUpdate/Returning操作");
                }
            }
            if (this.FieldsBuilder.Length > 1)
            {
                this.FieldsBuilder.Append(',');
                this.ValuesBuilder.Append(',');
                fixedValuesSql = this.ValuesBuilder.ToString();
            }
            this.ValuesBuilder.Clear();
            if (this.DbParameters.Count > 0)
            {
                fixedDbParameters = this.DbParameters.ToList();
                this.DbParameters.Clear();
            }
        }

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
            (var fieldsSql, var sqlSetter) = ((string, Action<IDataParameterCollection, StringBuilder, DbContext, string, string, object, string>))
                RepositoryHelper.BuildTypedBulkCommandInitializer(this.DbContext, entityType, insertObjType, 1, null, null);
            this.FieldsBuilder.Append(fieldsSql);
            loopSqlSetter = (dbParameters, builder, dbContext, insertObj, suffix) => sqlSetter.Invoke(dbParameters, builder, dbContext, fixedValuesSql, "),", insertObj, suffix);
        }
        this.FieldsBuilder.Append(") VALUES ");
        fixedFieldsSql = this.FieldsBuilder.ToString();

        if (fixedDbParameters != null && fixedDbParameters.Count > 0)
        {
            firstSqlSetter = (dbParameters, builder, tableName) =>
            {
                builder.Append($"{headSql}{this.OrmProvider.GetTableName(tableName)}");
                builder.Append(fixedFieldsSql);
                builder.Append(fixedValuesSql);
                fixedDbParameters.ForEach(f => dbParameters.Add(f));
            };
        }
        else
        {
            firstSqlSetter = (dbParameters, builder, tableName) =>
            {
                builder.Append($"{headSql}{this.OrmProvider.GetTableName(tableName)} ");
                builder.Append(fixedFieldsSql);
                builder.Append(fixedValuesSql);
            };
        }
        var shardingType = tableSegment.ShardingType;
        object shardingTables = tableSegment.Mapper.TableName;
        if (tableSegment.TableShardingInfo != null)
        {
            if (tableSegment.IsSharding)
            {
                if (shardingType > ShardingTableType.SingleTable)
                    throw new NotSupportedException($"实体表{entityType.FullName}已设置分表，数据插入不能设置多个分表，原始表：{tableSegment.Mapper.TableName}");
                shardingTables = tableSegment.Body;
            }
            else
            {
                shardingType = ShardingTableType.SplitTables;
                shardingTables = this.SplitShardingParameters(tableSegment.TableShardingInfo, TableUsageMode.WriteOnly, insertObjType, insertObjs, firstInsertObj, this.ShardingValues);
            }
        }
        string tailSql = null;
        if (this.UpdateBuilder != null)
            tailSql = this.UpdateBuilder.ToString();
        if (this.OutputSql != null)
            tailSql += this.OutputSql;
        return (shardingType, shardingTables, insertObjs, bulkCount, firstSqlSetter, loopSqlSetter, tailSql, this.ReaderFields);
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
                if (memberMapper.IsIgnore || memberMapper.IsAutoIncrement
                    || memberMapper.IsNavigation || memberMapper.IsIgnoreInsert || memberMapper.IsRowVersion)
                    continue;

                var fieldValue = dict[key];
                if (this.FieldsBuilder.Length > 0)
                {
                    this.FieldsBuilder.Append(',');
                    this.ValuesBuilder.Append(',');
                }
                var parameterName = $"{this.OrmProvider.ParameterPrefix}{memberMapper.MemberName}";
                this.FieldsBuilder.Append(this.OrmProvider.GetFieldName(memberMapper.FieldName));
                this.ValuesBuilder.Append(parameterName);

                if (fieldValue == null)
                    fieldValue = memberMapper.DefaultValue;
                else if (memberMapper.TypeHandler != null)
                    fieldValue = memberMapper.TypeHandler.ToFieldValue(fieldValue);
                else
                {
                    var targetType = memberMapper.MappedTargetType;
                    var fieldValueType = fieldValue.GetType();
                    if (fieldValueType != targetType)
                    {
                        var myValueGetter = this.OrmProvider.GetParameterValueGetter(fieldValueType, targetType, !memberMapper.IsRequired, this.DbContext.Options);
                        fieldValue = myValueGetter.Invoke(fieldValue);
                    }
                }
                this.DbParameters.Add(this.OrmProvider.CreateParameter(parameterName, memberMapper.NativeDbType, fieldValue));
                if (this.IsNeedShardingValues && tableSegment.TableShardingInfo.DependOnMembers.Contains(memberMapper.MemberName))
                    this.ShardingValues[memberMapper.MemberName] = fieldValue;
            }
        }
        else
        {
            var commandInitializer = RepositoryHelper.BuildTypedCommandInitializer(this.DbContext, entityType, insertObjType, 1, false, false, null, null);
            var typedCommandInitializer = commandInitializer as Action<IDataParameterCollection, StringBuilder, StringBuilder, DbContext, object>;
            typedCommandInitializer.Invoke(this.DbParameters, this.FieldsBuilder, this.ValuesBuilder, this.DbContext, insertObj);
        }

        if (this.ActionMode == ActionMode.Single && tableSegment.TableShardingInfo != null && !tableSegment.IsSharding && tableSegment.ShardingTableGetter != null)
        {
            tableSegment.Body = tableSegment.ShardingTableGetter.Invoke(insertObj);
            tableSegment.ShardingType = ShardingTableType.SingleTable;
            tableSegment.IsSharding = true;
        }
        if (this.IsNeedShardingValues) RepositoryHelper.SetShardingValues(this.DbContext,
            tableSegment.TableShardingInfo, tableSegment.EntityType, insertObjType, insertObj, this.ShardingValues);
    }
    public virtual void VisitWithByField(object deferredSegmentValue)
    {
        (var fieldName, var fieldValue) = ((string, object))deferredSegmentValue;
        var tableSegment = this.Tables[0];
        var entityMapper = tableSegment.Mapper;
        if (!entityMapper.TryGetMemberMapByFieldName(fieldName, out var memberMapper))
            throw new NotSupportedException($"实体{entityMapper.EntityType.FullName}未包含字段{fieldName}，无法进行更新操作");
        if (memberMapper.IsIgnore || memberMapper.IsIgnoreInsert)
            throw new NotSupportedException($"当前字段{memberMapper.FieldName}被忽略插入，IsIgnore：{memberMapper.IsIgnore}，IsIgnoreInsert：{memberMapper.IsIgnoreInsert}");
        if (memberMapper.IsRowVersion)
            throw new NotSupportedException($"当前字段{memberMapper.FieldName}为RowVersion类型，不允许插入");

        if (memberMapper.TypeHandler != null)
            fieldValue = memberMapper.TypeHandler.ToFieldValue(fieldValue);
        else
        {
            var targetType = memberMapper.MappedTargetType;
            var valueGetter = this.OrmProvider.GetParameterValueGetter(fieldValue.GetType(), targetType, false, this.DbContext.Options);
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
        if (this.IsNeedShardingValues && tableSegment.TableShardingInfo.DependOnMembers.Contains(memberMapper.MemberName))
            this.ShardingValues[memberMapper.MemberName] = fieldValue;
    }
    public virtual void VisitWithByFieldExpr(object deferredSegmentValue)
    {
        (var fieldSelector, var fieldValue) = ((Expression, object))deferredSegmentValue;
        var lambdaExpr = fieldSelector as LambdaExpression;
        var memberExpr = this.EnsureMemberVisit(lambdaExpr.Body) as MemberExpression;
        var tableSegment = this.Tables[0];
        var entityMapper = tableSegment.Mapper;
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
            var valueGetter = this.OrmProvider.GetParameterValueGetter(fieldValue.GetType(), targetType, false, this.DbContext.Options);
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
        if (this.IsNeedShardingValues && tableSegment.TableShardingInfo.DependOnMembers.Contains(memberMapper.MemberName))
            this.ShardingValues[memberMapper.MemberName] = fieldValue;
    }
    public virtual void VisitSetObject(object updateObj)
    {
        var tableSegment = this.Tables[0];
        var entityType = tableSegment.EntityType;
        var updateObjType = updateObj.GetType();
        if (this.UpdateIndex > 0) this.UpdateBuilder.Append(',');

        if (updateObj is IDictionary<string, object> dict)
        {
            var entityMapper = this.DbContext.EntityMapProvider.GetEntityMap(entityType);
            foreach (var key in dict.Keys)
            {
                if (!entityMapper.TryGetMemberMap(key, out var memberMapper))
                    continue;

                var fieldValue = dict[key];
                if (memberMapper.IsIgnore || memberMapper.IsAutoIncrement || memberMapper.IsNavigation
                    || memberMapper.IsIgnoreUpdate || memberMapper.IsRowVersion)
                    continue;

                var parameterName = $"{this.OrmProvider.ParameterPrefix}p{memberMapper.MemberName}";
                //此前参数可能有添加，也可能没有添加过，此处需要判断是否需要添加过
                if (fieldValue == null)
                    fieldValue = memberMapper.DefaultValue;
                else if (memberMapper.TypeHandler != null)
                    fieldValue = memberMapper.TypeHandler.ToFieldValue(fieldValue);
                else
                {
                    var targetType = memberMapper.MappedTargetType;
                    var fieldValueType = fieldValue.GetType();
                    if (fieldValueType != targetType)
                    {
                        var myValueGetter = this.OrmProvider.GetParameterValueGetter(fieldValueType, targetType, !memberMapper.IsRequired, this.DbContext.Options);
                        fieldValue = myValueGetter.Invoke(fieldValue);
                    }
                }
                this.DbParameters.Add(this.OrmProvider.CreateParameter(parameterName, memberMapper.NativeDbType, fieldValue));
                if (this.UpdateBuilder.Length > 0) this.FieldsBuilder.Append(',');
                this.UpdateBuilder.Append($"{this.OrmProvider.GetFieldName(memberMapper.FieldName)}={parameterName}");
            }
        }
        else
        {
            var commandInitializer = RepositoryHelper.BuildTypedCommandInitializer(this.DbContext, entityType, updateObjType, 3, false, false, null, null);
            var typedCommandInitializer = commandInitializer as Action<IDataParameterCollection, StringBuilder, DbContext, object>;
            typedCommandInitializer.Invoke(this.DbParameters, this.UpdateBuilder, this.DbContext, updateObj);
        }
        this.UpdateIndex++;
    }
    public virtual void VisitSetObjectExpr(object deferredSegmentValue)
    {
        var fieldsAssignment = deferredSegmentValue as LambdaExpression;
        this.InitTableAlias(fieldsAssignment);
        if (this.UpdateIndex > 0) this.UpdateBuilder.Append(',');
        this.Visit(new SqlSegment { Expression = fieldsAssignment });
    }
    public virtual void VisitSetField(object deferredSegmentValue)
    {
        (var fieldName, var fieldValue) = ((string, object))deferredSegmentValue;
        var tableSegment = this.Tables[0];
        var entityMapper = tableSegment.Mapper;
        if (!entityMapper.TryGetMemberMapByFieldName(fieldName, out var memberMapper))
            throw new NotSupportedException($"实体{entityMapper.EntityType.FullName}未包含字段{fieldName}，无法进行更新操作");
        if (memberMapper.IsIgnore || memberMapper.IsIgnoreInsert)
            throw new NotSupportedException($"当前字段{memberMapper.FieldName}被忽略插入，IsIgnore：{memberMapper.IsIgnore}，IsIgnoreInsert：{memberMapper.IsIgnoreInsert}");
        if (memberMapper.IsRowVersion)
            throw new NotSupportedException($"当前字段{memberMapper.FieldName}为RowVersion类型，不允许插入");
        this.AddSetParameter(memberMapper, fieldValue);
    }
    public virtual void VisitSetFieldExpr(object deferredSegmentValue)
    {
        (var fieldSelector, var fieldValue) = ((Expression, object))deferredSegmentValue;
        var lambdaExpr = fieldSelector as LambdaExpression;
        var memberExpr = this.EnsureMemberVisit(lambdaExpr.Body) as MemberExpression;
        var tableSegment = this.Tables[0];
        var entityMapper = tableSegment.Mapper;
        var memberMapper = entityMapper.GetMemberMap(memberExpr.Member.Name);
        if (memberMapper.IsIgnore || memberMapper.IsIgnoreInsert)
            throw new NotSupportedException($"当前字段{memberMapper.FieldName}被忽略插入，IsIgnore：{memberMapper.IsIgnore}，IsIgnoreInsert：{memberMapper.IsIgnoreInsert}");
        if (memberMapper.IsRowVersion)
            throw new NotSupportedException($"当前字段{memberMapper.FieldName}为RowVersion类型，不允许插入");
        this.AddSetParameter(memberMapper, fieldValue);
    }
    public virtual void VisitSetFieldExprs(object deferredSegmentValue)
    {
        (var fieldSelector, var valueGetter) = ((Expression, Expression))deferredSegmentValue;
        this.InitTableAlias(fieldSelector as LambdaExpression);
        var fieldSegment = this.Visit(new SqlSegment { Expression = fieldSelector });
        this.InitTableAlias(valueGetter as LambdaExpression);
        var valueSegment = this.Visit(new SqlSegment { Expression = valueGetter });
        this.AddSetParameter(fieldSegment.MemberMapper, valueSegment);
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
    public override SqlSegment VisitMemberAccess(SqlSegment sqlSegment)
    {
        var memberExpr = sqlSegment.Expression as MemberExpression;
        var memberInfo = memberExpr.Member;
        Func<ISqlVisitor, SqlSegment, SqlSegment> formatter = null;
        if (memberExpr.Expression != null)
        {
            //Where(f=>... && !f.OrderId.HasValue && ...)
            //Where(f=>... f.OrderId.Value==10 && ...)
            //Select(f=>... ,f.OrderId.HasValue  ...)
            //Select(f=>... ,f.OrderId.Value==10  ...)
            if (memberInfo.DeclaringType.IsValueType && Nullable.GetUnderlyingType(memberInfo.DeclaringType) != null)
            {
                if (memberInfo.Name == "HasValue")
                {
                    sqlSegment.Push(DeferredOperation.IsNull);
                    sqlSegment.Push(DeferredOperation.Not);
                    return this.Visit(sqlSegment.Next(memberExpr.Expression));
                }
                if (memberInfo.Name == "Value")
                    return this.Visit(sqlSegment.Next(memberExpr.Expression));
            }

            //各种OrmProvider提供的类型实例成员访问，如：DateTime,TimeSpan,String.Length
            //Where(f=>... && f.CreatedAt.Month<5 && ...)
            //Where(f=>... && f.Order.OrderNo.Length==10 && ...)
            if (this.OrmProvider.TryGetMemberAccessSqlFormatter(memberExpr, out formatter))
            {
                //.CreatedAt.Date 会更改MemberName
                var result = formatter.Invoke(this, sqlSegment.Next(memberExpr.Expression));
                result.MemberName = memberInfo.Name;
                return result;
            }

            if (memberExpr.TryGetParameters(out var parameterExprs))
            {
                if (parameterExprs.Count > 1)
                    throw new NotSupportedException($"不支持多参数访问，{memberExpr.ToString()}");

                var parameterExpr = parameterExprs[0];
                var parameterName = parameterExpr.Name;

                //最后有效的成员访问，去除集合类导航属性访问，如：f.Order.Seller.Company.Products.Count，最后有效成员访问是Products，而不是Count
                var fromSegment = this.Tables[0];
                MemberMap memberMapper = null;
                //当访问x.Grouping.UserId或是x.Order.Buyer时，会有2个以上的成员访问
                var memberExprs = this.GetMemberExprs(memberExpr, out _);
                while (memberExprs.TryPop(out var lastMemberExpr))
                {
                    if (!fromSegment.Mapper.TryGetMemberMap(lastMemberExpr.Member.Name, out memberMapper))
                        throw new NotSupportedException($"类{fromSegment.EntityType.FullName}没有成员{lastMemberExpr.Member.Name}，无法访问");
                    if (memberMapper.IsIgnore)
                        throw new NotSupportedException($"类{fromSegment.EntityType.FullName}的成员{lastMemberExpr.Member.Name}是忽略成员无法访问");
                    if (memberMapper.IsNavigation)
                        throw new NotSupportedException($"插入语句不支持使用导航属性，表达式：{memberExpr}");
                }
                //简单字段访问
                sqlSegment.SqlType = SqlType.OnlyField;
                sqlSegment.MemberMapper = memberMapper;
                sqlSegment.MemberName = memberMapper.MemberName;
                sqlSegment.Value = this.OrmProvider.GetFieldName(memberMapper.FieldName);
                return sqlSegment;
            }
        }

        //各种静态成员访问，如：DateTime.Now,int.MaxValue,string.Empty
        if (memberExpr.Member.DeclaringType == typeof(DBNull))
            return SqlSegment.Null;

        if (this.OrmProvider.TryGetMemberAccessSqlFormatter(memberExpr, out formatter))
            return formatter.Invoke(this, sqlSegment);

        //访问局部变量或是成员变量，当作常量处理，直接计算，后面统一做参数化处理
        //var orderIds=new List<int>{1,2,3}; Where(f=>orderIds.Contains(f.OrderId));
        //private Order order; Where(f=>f.OrderId==this.Order.Id); this.Order.Id
        //var orderId=10; Select(f=>new {OrderId=orderId,...}
        //Select(f=>new {OrderId=this.Order.Id, ...}
        var value = ValueEvalutor.Evaluate(memberExpr);
        if (value == null) return SqlSegment.Null;
        return sqlSegment.Change(value, SqlType.Variable);
    }
    public override SqlSegment VisitNew(SqlSegment sqlSegment)
    {
        //更新场景
        //.Set(f => new { TotalAmount = f.TotalAmount + f.Values(f.TotalAmount) })
        //.Set(f => new { TotalAmount = f.TotalAmount + f.Excluded(f.TotalAmount) })
        //INSERT INTO ... SELECT ... FROM ... 由Query部分单独处理，表达式走QueryVisitor的解析
        if (this.IsUpdate)
        {
            var newExpr = sqlSegment.Expression as NewExpression;
            if (newExpr.Type.Name.StartsWith("<>"))
            {
                var entityMapper = this.Tables[0].Mapper;
                for (int i = 0; i < newExpr.Arguments.Count; i++)
                {
                    var memberInfo = newExpr.Members[i];
                    if (!entityMapper.TryGetMemberMap(memberInfo.Name, out var memberMapper))
                        continue;
                    this.AddMemberElement(newExpr.Arguments[i], memberMapper);
                    this.UpdateIndex++;
                }
                return sqlSegment;
            }
        }
        return sqlSegment.Change(sqlSegment.Expression.Evaluate(), SqlType.Constant);
    }
    public override SqlSegment VisitMemberInit(SqlSegment sqlSegment)
    {
        //更新场景
        //.Set(f => new xxx { TotalAmount = f.TotalAmount + f.Values(f.TotalAmount) })
        //.Set(f => new xxx { TotalAmount = f.TotalAmount + f.Excluded(f.TotalAmount) })
        //INSERT INTO ... SELECT ... FROM ... 由Query部分单独处理，表达式走QueryVisitor的解析   
        if (this.IsUpdate)
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
                this.AddMemberElement(memberAssignment.Expression, memberMapper);
                this.UpdateIndex++;
            }
        }
        return sqlSegment.Change(sqlSegment.Expression.Evaluate(), SqlType.Constant);
    }
    public virtual void AddMemberElement(Expression fieldExpr, MemberMap memberMapper)
    {
        //更新使用
        var sqlSegment = this.Visit(new SqlSegment { Expression = fieldExpr });
        if (this.UpdateIndex > 0) this.UpdateBuilder.Append(',');
        var fieldName = this.OrmProvider.GetFieldName(memberMapper.FieldName);

        if (sqlSegment.IsNull)
            this.UpdateBuilder.Append($"{fieldName}=NULL");
        else if (sqlSegment.IsValue)
        {
            var parameterName = this.OrmProvider.ParameterPrefix + this.UserParameterPrefix + this.DbParameters.Count.ToString();
            var dbFieldValue = sqlSegment.Value;
            if (memberMapper.TypeHandler != null)
                dbFieldValue = memberMapper.TypeHandler.ToFieldValue(dbFieldValue);
            else
            {
                var targetType = memberMapper.MappedTargetType;
                var valueGetter = this.OrmProvider.GetParameterValueGetter(dbFieldValue.GetType(), targetType, false, this.DbContext.Options);
                dbFieldValue = valueGetter.Invoke(dbFieldValue);
            }

            this.DbParameters.Add(this.OrmProvider.CreateParameter(parameterName, memberMapper.NativeDbType, dbFieldValue));
            this.UpdateBuilder.Append($"{fieldName}={parameterName}");
        }
        else
        {
            //TODO: 
            //带有参数或字段的表达式或函数调用、或是只有参数或字段
            //ON DUPLICATE KEY UPDATE `Name`=VALUES(`Name`)
            //ON CONFLICT ("Id") DO UPDATE SET "Name"=EXCLUDED."Name"
            //var fieldValue = this.GetSetFieldName(sqlSegment.Value.ToString());
            this.UpdateBuilder.Append($"{fieldName}={sqlSegment.Value}");
        }
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

            if (index > 0) this.FieldsBuilder.Append(',');
            this.FieldsBuilder.Append(this.OrmProvider.GetFieldName(memberMapper.FieldName));

            Func<IDictionary<string, object>, object> valueGetter = null;
            if (memberMapper.TypeHandler != null)
                valueGetter = insertObj => memberMapper.TypeHandler.ToFieldValue(insertObj[key]);
            else
            {
                var targetType = memberMapper.MappedTargetType;
                var isContainsKey = dict.TryGetValueIgnoreCase(key, out var fieldValue);
                if (memberMapper.IsRequired && (!isContainsKey || fieldValue == null))
                    throw new Exception($"实体{entityMapper.EntityType.FullName}表，字段{memberMapper.FieldName}为必填，值不能为空");

                if (memberMapper.IsRequired)
                {
                    var fieldValueType = fieldValue.GetType();
                    if (fieldValueType.ToUnderlyingType() != targetType)
                    {
                        var myValueGetter = this.OrmProvider.GetParameterValueGetter(fieldValueType, targetType, !memberMapper.IsRequired, this.DbContext.Options);
                        valueGetter = insertObj => myValueGetter.Invoke(insertObj[key]);
                    }
                    else valueGetter = insertObj => insertObj[key];
                }
                else
                {
                    if (fieldValue != null)
                    {
                        var fieldValueType = dict[key].GetType();
                        if (fieldValueType.ToUnderlyingType() != targetType)
                        {
                            var myValueGetter = this.OrmProvider.GetParameterValueGetter(fieldValueType, targetType, !memberMapper.IsRequired, this.DbContext.Options);
                            valueGetter = insertObj =>
                            {
                                var fieldValue = insertObj[key];
                                return fieldValue == null ? memberMapper.DefaultValue : myValueGetter.Invoke(fieldValue);
                            };
                        }
                        else valueGetter = insertObj => insertObj[key] ?? memberMapper.DefaultValue;
                    }
                    else valueGetter = insertObj => insertObj[key] ?? memberMapper.DefaultValue;
                }
            }

            Action<IDataParameterCollection, StringBuilder, IDictionary<string, object>, string> valueSetter = null;
            if (index > 0)
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
            else
            {
                valueSetter = (dbParameters, builder, insertObj, suffix) =>
                {
                    var fieldValue = valueGetter.Invoke(insertObj);
                    var parameterName = $"{this.OrmProvider.ParameterPrefix}{memberMapper.MemberName}{suffix}";
                    builder.Append(parameterName);
                    dbParameters.Add(this.OrmProvider.CreateParameter(parameterName, memberMapper.NativeDbType, fieldValue));
                };
            }
            valueSetters.Add(valueSetter);
            index++;
        }
        return valueSetters;
    }
    public string GetTableName(TableSegment tableSegment)
    {
        string tableName = null;
        if (tableSegment.TableShardingInfo != null)
        {
            if (tableSegment.IsSharding) tableName = tableSegment.Body;
            else tableName = RepositoryHelper.GetShardingTableName(this.DbContext, tableSegment.TableShardingInfo, TableUsageMode.WriteOnly, this.ShardingValues);
        }
        else tableName = tableSegment.Mapper.TableName;
        if (!string.IsNullOrEmpty(tableSegment.TableSchema))
            tableName = $"{this.OrmProvider.GetTableName(tableSegment.TableSchema)}.{this.OrmProvider.GetTableName(tableName)}";
        else tableName = this.OrmProvider.GetTableName(tableName);
        return tableName;
    }
    public override void Dispose()
    {
        if (this.isDisposed) return;
        this.deferredSegments = null;
        this.FieldsBuilder = null;
        this.ValuesBuilder = null;
        this.Command?.Dispose();
        this.Command = null;
        base.Dispose();
        this.isDisposed = true;
    }
    public virtual void InitTableAlias(LambdaExpression lambdaExpr)
    {
        this.TableAliases.Clear();
        this.TableAliases[lambdaExpr.Parameters[0].Name] = this.Tables[0];
    }
    public virtual void AddSetParameter(MemberMap memberMapper, object fieldValue)
    {
        string parameterValue = null;
        if (fieldValue == null || fieldValue is DBNull)
            parameterValue = "NULL";
        else
        {
            parameterValue = this.OrmProvider.ParameterPrefix + "p" + memberMapper.MemberName;
            if (memberMapper.TypeHandler != null)
                fieldValue = memberMapper.TypeHandler.ToFieldValue(fieldValue);
            else
            {
                var targetType = memberMapper.MappedTargetType;
                var valueGetter = this.OrmProvider.GetParameterValueGetter(fieldValue.GetType(), targetType, false, this.DbContext.Options);
                fieldValue = valueGetter.Invoke(fieldValue);
            }
        }
        this.DbParameters.Add(this.OrmProvider.CreateParameter(parameterValue, memberMapper.NativeDbType, fieldValue));
        if (this.UpdateIndex > 0) this.UpdateBuilder.Append(',');
        this.UpdateBuilder.Append($"{this.OrmProvider.GetFieldName(memberMapper.FieldName)}={parameterValue}");
        this.UpdateIndex++;
    }
    public virtual void AddSetParameter(MemberMap memberMapper, SqlSegment valueSegment)
    {
        if (valueSegment.HasField)
        {
            if (this.UpdateIndex > 0) this.UpdateBuilder.Append(',');
            this.UpdateBuilder.Append($"{this.OrmProvider.GetFieldName(memberMapper.FieldName)}={valueSegment.Value}");
            this.UpdateIndex++;
        }
        else this.AddSetParameter(memberMapper, valueSegment.Value);
    }
}