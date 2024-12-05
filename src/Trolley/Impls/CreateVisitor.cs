using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Trolley;

public class CreateVisitor : SqlVisitor, ICreateVisitor
{
    private static ConcurrentDictionary<int, Action<StringBuilder, DbContext, object>> withByFieldsCache = new();
    private static ConcurrentDictionary<int, object> withByValuesCache = new();

    protected List<CommandSegment> deferredSegments = new();
    public StringBuilder FieldsBuilder { get; set; } = new();
    public StringBuilder ValuesBuilder { get; set; } = new();

    public List<string> OnlyFieldNames { get; set; }
    public List<string> IgnoreFieldNames { get; set; }
    public ActionMode ActionMode { get; set; }
    public bool IsReturnIdentity { get; set; }

    public CreateVisitor(DbContext dbContext, char tableAsStart = 'a')
    {
        this.DbContext = dbContext;
        this.TableAsStart = tableAsStart;
    }
    public virtual void Initialize(Type entityType, bool isMultiple = false, bool isFirst = true)
    {
        if (!isMultiple)
        {
            this.Tables = new();
            this.TableAliases = new();
            this.Tables.Add(new TableSegment
            {
                EntityType = entityType,
                AliasName = "a",
                Mapper = this.MapProvider.GetEntityMap(entityType)
            });
        }
        if (!isFirst) this.Clear();
    }
    public virtual string BuildCommand(ITheaCommand command, bool isReturnIdentity, out List<SqlFieldSegment> readerFields)
    {
        string sql = null;
        readerFields = null;
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
                }
            }
            sql = this.BuildSql(out readerFields);
        }
        return sql;
    }
    public virtual MultipleCommand CreateMultipleCommand()
    {
        return new MultipleCommand
        {
            CommandType = MultipleCommandType.Insert,
            EntityType = this.Tables[0].EntityType,
            Body = this.deferredSegments,
            Tables = this.Tables,
            IgnoreFieldNames = this.IgnoreFieldNames,
            OnlyFieldNames = this.OnlyFieldNames,
            RefQueries = this.RefQueries,
            IsNeedTableAlias = this.IsNeedTableAlias
        };
    }
    public virtual void BuildMultiCommand(ITheaCommand command, StringBuilder sqlBuilder, MultipleCommand multiCommand, int commandIndex)
    {
        this.IsMultiple = true;
        this.CommandIndex = commandIndex;
        this.deferredSegments = multiCommand.Body as List<CommandSegment>;
        this.Tables = multiCommand.Tables;
        this.IgnoreFieldNames = multiCommand.IgnoreFieldNames;
        this.OnlyFieldNames = multiCommand.OnlyFieldNames;
        this.RefQueries = multiCommand.RefQueries;
        this.IsNeedTableAlias = multiCommand.IsNeedTableAlias;
        if (sqlBuilder.Length > 0) sqlBuilder.Append(';');
        if (this.deferredSegments.Count > 0 && this.deferredSegments[0].Type == "WithBulk")
            this.ActionMode = ActionMode.Bulk;
        sqlBuilder.Append(this.BuildCommand(command, false, out var readerFields));
        this.ReaderFields = readerFields;
    }
    public virtual string BuildSql(out List<SqlFieldSegment> readerFields)
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
        if (!string.IsNullOrEmpty(tableSegment.TableSchema))
            tableName = $"{this.OrmProvider.GetTableName(tableSegment.TableSchema)}.{this.OrmProvider.GetTableName(tableName)}";
        tableName = this.OrmProvider.GetTableName(tableName);

        if (this.IsReturnIdentity)
        {
            if (!entityMapper.IsAutoIncrementKey)
                throw new Exception($"实体{entityMapper.EntityType.FullName}表未配置自增长字段，无法返回Identity值");
            var keyFieldName = this.OrmProvider.GetFieldName(entityMapper.KeyMembers[0].FieldName);
            this.ValuesBuilder.Append(this.OrmProvider.GetIdentitySql(keyFieldName));
        }

        var sql = $"INSERT INTO {tableName}({this.FieldsBuilder}) VALUES({this.ValuesBuilder})";
        this.FieldsBuilder.Clear();
        this.ValuesBuilder.Clear();
        return sql;
    }
    public virtual void WithBy(object insertObj, ActionMode? actionMode = null)
    {
        if (actionMode.HasValue)
            this.ActionMode = actionMode.Value;
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
        this.IgnoreFieldNames.AddRange(fieldNames);
    }
    public virtual void IgnoreFields(Expression fieldsSelector)
        => this.IgnoreFieldNames = this.VisitFields(fieldsSelector);
    public virtual void OnlyFields(string[] fieldNames)
    {
        this.OnlyFieldNames ??= new();
        this.OnlyFieldNames.AddRange(fieldNames);
    }
    public virtual void OnlyFields(Expression fieldsSelector)
        => this.OnlyFieldNames = this.VisitFields(fieldsSelector);
    public virtual string BuildWithBulkSql(ITheaCommand command, out List<SqlFieldSegment> readerFields)
    {
        //多命令查询或是ToSql才会走到此分支
        //多语句执行，一次性不分批次
        var builder = new StringBuilder();
        (var isNeedSplit, var tableName, var insertObjs, _, var firstSqlSetter,
            var loopSqlSetter, _, readerFields) = this.BuildWithBulk(command);

        void Execute(string tableName, IEnumerable insertObjs)
        {
            firstSqlSetter.Invoke(command.Parameters, builder, tableName);
            int index = 0;
            foreach (var insertObj in insertObjs)
            {
                if (index > 0) builder.Append(',');
                loopSqlSetter.Invoke(command.Parameters, builder, this.DbContext, insertObj, index.ToString());
                index++;
            }
        }
        if (isNeedSplit)
        {
            var entityType = this.Tables[0].EntityType;
            var tabledInsertObjs = RepositoryHelper.SplitShardingParameters(this.MapProvider, this.ShardingProvider, entityType, insertObjs);
            int index = 0;
            foreach (var tabledInsertObj in tabledInsertObjs)
            {
                if (index > 0) builder.Append(';');
                Execute(tabledInsertObj.Key, tabledInsertObj.Value);
                index++;
            }
        }
        else Execute(tableName, insertObjs);
        var sql = builder.ToString();
        builder.Clear();
        return sql;
    }
    public virtual (bool, string, IEnumerable, int, Action<IDataParameterCollection, StringBuilder, string>,
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
                    default: throw new NotSupportedException("批量插入后，只支持WithBy/IgnoreFields/OnlyFields操作");
                }
            }
            fixedDbParameters = this.DbParameters.Cast<IDbDataParameter>().ToList();
            this.DbParameters = command.Parameters;
        }
        //多命令查询时，第二次以后，DbParameters有值，不能再赋值
        else this.DbParameters ??= command.Parameters;

        var entityMapper = tableSegment.Mapper;
        var fieldsSetter = this.GetFieldsSetter(entityType, insertObjType);
        var valuesSetter = this.GetValuesSetter(entityType, insertObjType, true);
        var typedValuesSetter = valuesSetter as Action<IDataParameterCollection, StringBuilder, DbContext, object, string>;

        string headSql = "INSERT INTO ";
        if (!string.IsNullOrEmpty(tableSegment.TableSchema))
            headSql = "INSERT INTO " + this.OrmProvider.GetTableName(tableSegment.TableSchema);

        fieldsSetter.Invoke(this.FieldsBuilder, this.DbContext, firstInsertObj);
        var fieldsSql = $"({this.FieldsBuilder}) VALUES({this.ValuesBuilder})";
        this.FieldsBuilder.Clear();
        this.ValuesBuilder.Clear();

        Action<IDataParameterCollection, StringBuilder, string> firstSqlSetter = null;
        if (this.deferredSegments.Count > 1)
        {
            firstSqlSetter = (dbParameters, builder, tableName) =>
            {
                builder.Append(headSql);
                builder.Append(this.OrmProvider.GetTableName(tableName));
                builder.Append(fieldsSql);
                fixedDbParameters.ForEach(f => dbParameters.Add(f));
            };
        }
        else
        {
            firstSqlSetter = (dbParameters, builder, tableName) =>
            {
                builder.Append(headSql);
                builder.Append(this.OrmProvider.GetTableName(tableName));
                builder.Append(fieldsSql);
            };
        }

        return (isNeedSplit, tableName, insertObjs, bulkCount, firstSqlSetter, typedValuesSetter, null, null);
    }
    public virtual void VisitWithBy(object insertObj)
    {
        var entityType = this.Tables[0].EntityType;
        var insertObjType = insertObj.GetType();
        var fielsSetter = this.GetFieldsSetter(entityType, insertObjType);
        var valuesSetter = this.GetValuesSetter(entityType, insertObjType, this.IsMultiple);
        if (this.FieldsBuilder.Length > 0)
        {
            this.FieldsBuilder.Append(',');
            this.ValuesBuilder.Append(',');
        }
        fielsSetter.Invoke(this.FieldsBuilder, this.DbContext, insertObj);
        if (this.IsMultiple)
        {
            var typedValuesSetter = valuesSetter as Action<IDataParameterCollection, StringBuilder, DbContext, object, string>;
            typedValuesSetter.Invoke(this.DbParameters, ValuesBuilder, this.DbContext, insertObj, $"_m{this.CommandIndex}");
        }
        else
        {
            var typedValuesSetter = valuesSetter as Action<IDataParameterCollection, StringBuilder, DbContext, object>;
            typedValuesSetter.Invoke(this.DbParameters, this.ValuesBuilder, this.DbContext, insertObj);
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
            throw new NotSupportedException($"当前字段{memberMapper.FieldName}不允许插入，IsRowVersion：{memberMapper.IsRowVersion}");

        var parameterName = this.OrmProvider.ParameterPrefix + memberMapper.MemberName;
        if (this.IsMultiple) parameterName += $"_m{this.CommandIndex}";

        if (memberMapper.TypeHandler != null)
            fieldValue = memberMapper.TypeHandler.ToFieldValue(this.OrmProvider, fieldValue);
        else
        {
            var targetType = this.OrmProvider.MapDefaultType(memberMapper);
            var valueGetter = this.OrmProvider.GetParameterValueGetter(fieldValue.GetType(), targetType, false, this.Options);
            fieldValue = valueGetter.Invoke(fieldValue);
        }
        if (this.FieldsBuilder.Length > 0)
        {
            this.FieldsBuilder.Append(',');
            this.ValuesBuilder.Append(',');
        }
        this.FieldsBuilder.Append(memberMapper.FieldName);
        this.ValuesBuilder.Append(parameterName);
        this.DbParameters.Add(this.OrmProvider.CreateParameter(parameterName, memberMapper.NativeDbType, fieldValue));
    }
    public virtual List<string> VisitFields(Expression fieldsSelector)
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
                    fieldNames.Add(memberMapper.FieldName);
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
                    fieldNames.Add(memberMapper.FieldName);
                }
                break;
        }
        if (fieldNames.Count > 0)
            return fieldNames;
        return null;
    }
    public virtual string GetShardingTableName()
    {
        var entityType = this.Tables[0].EntityType;
        var origTableName = this.Tables[0].Mapper.TableName;
        if (this.deferredSegments.Count == 1)
        {
            var insertObj = this.deferredSegments[0].Value;
            var insertObjType = insertObj.GetType();
            return this.DbContext.GetShardingTableName(entityType, insertObjType, insertObj);
        }
        else
        {
            if (!this.ShardingProvider.TryGetTableSharding(entityType, out var tableShardingInfo))
                throw new Exception($"实体表{entityType.FullName}没有配置分表，不能调用此方法UseAutoTable");
            if (tableShardingInfo.DependOnMembers.Count > 1)
            {
                var shardingRule = tableShardingInfo.Rule as Func<string, object, object, string>;
                var memberValue1 = this.GetShardingFieldValue(tableShardingInfo.DependOnMembers[0]);
                var memberValue2 = this.GetShardingFieldValue(tableShardingInfo.DependOnMembers[1]);
                return shardingRule.Invoke(origTableName, memberValue1, memberValue2);
            }
            else
            {
                var shardingRule = tableShardingInfo.Rule as Func<string, object, string>;
                var memberValue = this.GetShardingFieldValue(tableShardingInfo.DependOnMembers[0]);
                return shardingRule.Invoke(origTableName, memberValue);
            }
        }
    }
    public virtual Action<StringBuilder, DbContext, object> GetFieldsSetter(Type entityType, Type insertObjType)
    {
        var cacheKey = RepositoryHelper.GetCacheKey(this.OrmProvider.OrmProviderType, this.MapProvider, entityType, insertObjType, this.OnlyFieldNames, this.IgnoreFieldNames);
        return withByFieldsCache.GetOrAdd(cacheKey, f =>
        {
            var fieldsSetter = RepositoryHelper.BuildFieldsSqlParametersPart(this.DbContext, entityType, insertObjType, 1, false, false, false, false, false, this.OnlyFieldNames, this.IgnoreFieldNames);
            return fieldsSetter as Action<StringBuilder, DbContext, object>;
        });
    }
    public virtual object GetValuesSetter(Type entityType, Type insertObjType, bool hasSuffix)
    {
        var cacheKey = RepositoryHelper.GetCacheKey(this.OrmProvider.OrmProviderType, this.MapProvider, entityType, insertObjType, this.OnlyFieldNames, this.IgnoreFieldNames);
        return withByValuesCache.GetOrAdd(cacheKey, f => RepositoryHelper.BuildFieldsSqlParametersPart(
            this.DbContext, entityType, insertObjType, 2, false, false, false, hasSuffix, false, this.OnlyFieldNames, this.IgnoreFieldNames));
    }
    public virtual void Clear()
    {
        this.Tables?.Clear();
        this.TableAliases?.Clear();
        this.ReaderFields?.Clear();
        this.FieldsBuilder.Clear();
        this.ValuesBuilder.Clear();
        this.WhereSql = null;
        this.TableAsStart = 'a';
        this.IsNeedTableAlias = false;
        this.deferredSegments.Clear();
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
    private object GetShardingFieldValue(string memberName)
    {
        foreach (var deferredSegment in this.deferredSegments)
        {
            switch (deferredSegment.Type)
            {
                case "WithBy":
                    var insertObj = deferredSegment.Value;
                    var insertObjType = insertObj.GetType();
                    if (this.TryGetMemberValue(insertObjType, insertObj, memberName, out var memberValue))
                        return memberValue;
                    break;
                case "WithByField":
                    (var fieldSelector, var fieldValue) = ((Expression, object))deferredSegment.Value;
                    var lambdaExpr = fieldSelector as LambdaExpression;
                    var memberExpr = this.EnsureMemberVisit(lambdaExpr.Body) as MemberExpression;
                    if (memberExpr.Member.Name == memberName)
                        return fieldValue;
                    break;
            }
        }
        throw new Exception($"缺少分表规则依赖的成员{memberName}，无法确定分表");
    }
    private bool TryGetMemberValue(Type insertObjType, object insertObj, string memberName, out object memberValue)
    {
        var memberInfos = insertObjType.GetMember(memberName);
        if (memberInfos.Length > 0)
        {
            memberValue = FasterEvaluator.EvaluateAndCache(insertObj, memberInfos[0]);
            return true;
        }
        memberValue = null;
        return false;
    }
}
