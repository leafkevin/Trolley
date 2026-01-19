using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Trolley.MySqlConnector;

public class MySqlCreateVisitor : CreateVisitor
{
    private MySqlProvider dialectProvider => this.OrmProvider as MySqlProvider;
    public bool IsUseIgnoreInto { get; set; }
    public StringBuilder UpdateBuilder { get; set; }
    public bool IsUseSetAlias { get; set; }
    public string SetRowAlias { get; set; } = "newRow";
    public string FromSql { get; set; }
    public string OutputSql { get; set; }

    public MySqlCreateVisitor(Type entityType, DbContext dbContext, char tableAsStart = 'a')
        : base(entityType, dbContext, tableAsStart) { }

    public override string BuildSql(ITheaCommand command, out List<SqlFieldSegment> readerFields)
    {
        string tailSql = null;
        readerFields = this.ReaderFields;

        switch (this.ActionMode)
        {
            case ActionMode.Bulk:
                (var shardingType, var shardingTables, var insertObjs, _, var firstSqlSetter,
                   var loopSqlSetter, tailSql, readerFields) = this.BuildWithBulk(command);

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
                builder.Remove(builder.Length - 1, 1);
                this.FromSql = builder.ToString();
                break;
            case ActionMode.Single:
                //当Insert Select From操作时，DbParameters也有值，但不是command.Parameters，需要赋值到command.Parameters
                if (this.DbParameters != null && this.DbParameters != command.Parameters)
                {
                    foreach (var dbParameter in this.DbParameters)
                    {
                        command.Parameters.Add(dbParameter);
                    }
                    this.DbParameters = command.Parameters;
                }
                else this.DbParameters = command.Parameters;
                this.ValuesBuilder = new();
                var tableSegment = this.Tables[0];

                if (tableSegment.TableShardingInfo != null && !tableSegment.IsSharding && tableSegment.ShardingTableGetter == null)
                {
                    if (tableSegment.TableShardingInfo.DependOnMembers == null || tableSegment.TableShardingInfo.DependOnMembers.Count == 0)
                        throw new Exception($"实体表{tableSegment.EntityType.FullName}已设置分表，未指定分表，也未设置依赖字段无法确定分表，请使用UseTable/UseTableBy方法手动指定分表");
                    if (this.deferredSegments.Count > 1)
                    {
                        this.isNeedShardingValues = true;
                        this.shardingValues = new();
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
                    }
                }
                var entityType = tableSegment.EntityType;
                if (string.IsNullOrEmpty(this.FromSql))
                {
                    var tableName = this.GetTableName(tableSegment);
                    if (this.IsReturnIdentity && (this.UpdateBuilder != null || this.OutputSql != null))
                        throw new NotSupportedException("返回Identity，不支持同时Returning操作");
                    this.FromSql = $"{this.BuildHeadSql()} {tableName} ({this.FieldsBuilder}) VALUES ({this.ValuesBuilder})";
                }
                this.ValuesBuilder.Clear();
                if (this.UpdateBuilder != null)
                    tailSql = this.UpdateBuilder.ToString();

                if (this.OutputSql != null)
                {
                    tailSql += this.OutputSql;
                    readerFields = this.ReaderFields;
                }
                if (this.IsReturnIdentity)
                {
                    var entityMapper = tableSegment.Mapper;
                    if (!entityMapper.IsAutoIncrementKey)
                        throw new NotSupportedException($"实体{entityMapper.EntityType.FullName}表未配置自增长字段，无法返回Identity值");
                    tailSql = this.OrmProvider.GetIdentitySql(null);
                }
                this.ValuesBuilder.Clear();
                break;
        }
        this.FieldsBuilder.Clear();
        return $"{this.FromSql}{tailSql}";
    }
    public override void UseTableSchema(bool isIncludeMany, string tableSchema)
    {
        var defaultSchemaName = this.dialectProvider.GetDefaultSchemaName(this.DbContext);
        if (tableSchema == defaultSchemaName) return;

        var tableSegment = isIncludeMany ? this.IncludeTables.Last() : this.Tables.Last();
        tableSegment.TableSchema = tableSchema;
    }
    public override (ShardingTableType, object, IEnumerable, int, Action<IDataParameterCollection, StringBuilder, string>,
        Action<IDataParameterCollection, StringBuilder, DbContext, object, string>, string, List<SqlFieldSegment>) BuildWithBulk(ITheaCommand command)
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
                this.isNeedShardingValues = true;
                this.shardingValues = new();
            }
        }
        if (this.deferredSegments.Count > 1)
        {
            this.ValuesBuilder = new StringBuilder("(");
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
                    default: throw new NotSupportedException("批量插入后，只支持WithBy/OnDuplicateKeyUpdate/Returning操作");
                }
            }
            this.FieldsBuilder.Append(',');
            this.ValuesBuilder.Append(',');
            fixedValuesSql = this.ValuesBuilder.ToString();
            this.ValuesBuilder.Clear();
            if (this.DbParameters.Count > 0)
                fixedDbParameters = tempDbParameters.ToList();
            this.DbParameters = command.Parameters;
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
            (var fieldsSql, var sqlSetter) = ((string, Action<IDataParameterCollection, StringBuilder, DbContext, string, object, string>))
                RepositoryHelper.BuildTypedBulkCommandInitializer(this.DbContext, entityType, insertObjType, 1, null, null);
            this.FieldsBuilder.Append(fieldsSql);
            loopSqlSetter = (dbParameters, builder, dbContext, insertObj, suffix) =>
            {
                sqlSetter.Invoke(dbParameters, builder, dbContext, fixedValuesSql, insertObj, suffix);
                builder.Append("),");
            };
        }
        this.FieldsBuilder.Append(") VALUES ");
        fixedFieldsSql = this.FieldsBuilder.ToString();

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
                shardingTables = this.SplitShardingParameters(tableSegment.TableShardingInfo, insertObjType, insertObjs, firstInsertObj, this.shardingValues);
            }
        }
        string tailSql = null;
        if (this.UpdateBuilder != null)
            tailSql = this.UpdateBuilder.ToString();
        if (this.OutputSql != null)
            tailSql += this.OutputSql;
        return (shardingType, shardingTables, insertObjs, bulkCount, firstSqlSetter, loopSqlSetter, tailSql, this.ReaderFields);
    }
    public void Returning(string fieldNames)
    {
        this.ReaderFields = new();
        this.OutputSql = $" RETURNING {fieldNames}";
        var entityType = this.Tables[0].EntityType;
        if (fieldNames == "*")
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
                    MappedTargetType = memberMapper.MappedTargetType,
                    TypeHandler = memberMapper.TypeHandler,
                    Body = memberMapper.FieldName
                });
            }
        }
        else
        {
            this.ReaderFields.Add(new SqlFieldSegment
            {
                FieldType = SqlFieldType.RawSql,
                Body = fieldNames
            });
        }
    }
    public virtual void Returning(LambdaExpression fieldsSelector)
    {
        this.ReaderFields = new();
        var entityMapper = this.Tables[0].Mapper;
        var builder = new StringBuilder(" RETURNING ");
        this.InitTableAlias(fieldsSelector);
        switch (fieldsSelector.Body.NodeType)
        {
            case ExpressionType.MemberAccess:
                {
                    var memberExpr = fieldsSelector.Body as MemberExpression;
                    var sqlSegment = this.VisitAndDeferred(new SqlFieldSegment { Expression = memberExpr });
                    this.GetQuotedValue(sqlSegment, true);
                    sqlSegment.TargetMember = memberExpr.Member;
                    sqlSegment.SegmentType = memberExpr.Type;
                    builder.Append(sqlSegment.Body);
                    if (sqlSegment.IsNeedAlias || sqlSegment.IsConstant || sqlSegment.IsVariable || sqlSegment.HasParameter || sqlSegment.IsExpression || sqlSegment.IsMethodCall
                        || sqlSegment.FromMember != null && sqlSegment.FromMember.Name != sqlSegment.TargetMember.Name)
                        builder.Append($" AS {this.OrmProvider.GetFieldName(memberExpr.Member.Name)}");
                    this.ReaderFields.Add(sqlSegment);
                }
                break;
            case ExpressionType.New:
                var newExpr = fieldsSelector.Body as NewExpression;
                for (int i = 0; i < newExpr.Arguments.Count; i++)
                {
                    var memberInfo = newExpr.Members[i];
                    var sqlSegment = this.VisitAndDeferred(new SqlFieldSegment { Expression = newExpr.Arguments[i] });
                    this.GetQuotedValue(sqlSegment, true);
                    sqlSegment.TargetMember = memberInfo;
                    sqlSegment.SegmentType = memberInfo.GetMemberType();
                    if (i > 0) builder.Append(',');
                    builder.Append(sqlSegment.Body);
                    if (sqlSegment.IsNeedAlias || sqlSegment.IsConstant || sqlSegment.IsVariable || sqlSegment.HasParameter || sqlSegment.IsExpression || sqlSegment.IsMethodCall)
                        builder.Append($" AS {this.OrmProvider.GetFieldName(memberInfo.Name)}");
                    this.ReaderFields.Add(sqlSegment);
                }
                break;
            case ExpressionType.MemberInit:
                var memberInitExpr = fieldsSelector.Body as MemberInitExpression;
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
                    if (sqlSegment.IsNeedAlias || sqlSegment.IsConstant || sqlSegment.IsVariable || sqlSegment.HasParameter || sqlSegment.IsExpression || sqlSegment.IsMethodCall)
                        builder.Append($" AS {this.OrmProvider.GetFieldName(memberAssignment.Member.Name)}");
                    this.ReaderFields.Add(sqlSegment);
                }
                break;
            case ExpressionType.Parameter:
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
                        MappedTargetType = memberMapper.MappedTargetType,
                        TypeHandler = memberMapper.TypeHandler,
                        Body = memberMapper.FieldName
                    });
                }
                builder.Append('*');
                break;
            default:
                this.VisitAndDeferred(new SqlFieldSegment { Expression = fieldsSelector });
                for (int i = 0; i < this.ReaderFields.Count; i++)
                {
                    var readerField = this.ReaderFields[i];
                    if (i > 0) builder.Append(',');
                    builder.Append(readerField.Body);
                    if (readerField.IsNeedAlias || readerField.IsConstant || readerField.IsVariable || readerField.HasParameter || readerField.IsExpression || readerField.IsMethodCall)
                        builder.Append($" AS {this.OrmProvider.GetFieldName(readerField.TargetMember.Name)}");
                }
                break;
        }
        this.OutputSql = builder.ToString();
        builder.Clear();
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
    public (ShardingTableType, object, IEnumerable, int?, List<MemberMap>, List<Func<object, object>>) BuildWithBulkCopy()
    {
        (var insertObjs, int? timeoutSeconds) = ((IEnumerable, int?))this.deferredSegments[0].Value;
        object firstInsertObj = null;
        foreach (var insertObj in insertObjs)
        {
            firstInsertObj = insertObj;
            break;
        }
        var insertObjType = firstInsertObj.GetType();
        var tableSegment = this.Tables[0];

        var shardingType = tableSegment.ShardingType;
        object shardingTables = tableSegment.Mapper.TableName;
        if (tableSegment.TableShardingInfo != null)
        {
            if (tableSegment.IsSharding)
            {
                if (!string.IsNullOrEmpty(tableSegment.Body))
                    shardingTables = tableSegment.Body;
                else if (tableSegment.TableNames != null && tableSegment.TableNames.Count > 0)
                {
                    var entityType = tableSegment.EntityType;
                    throw new NotSupportedException($"实体表{entityType.FullName}已设置分表，数据插入不能设置多个分表，原始表：{tableSegment.Mapper.TableName}");
                }
            }
            else
            {
                shardingType = ShardingTableType.SplitTables;
                shardingTables = this.SplitShardingParameters(tableSegment.TableShardingInfo, insertObjType, insertObjs, firstInsertObj, this.shardingValues);
            }
        }
        (var memberMappers, var valueGetters) = this.GetRefMemberMappers(insertObjType, tableSegment.Mapper, firstInsertObj, false);
        return (shardingType, shardingTables, insertObjs, timeoutSeconds, memberMappers, valueGetters);
    }
    public void OnDuplicateKeyUpdate(object updateObj)
    {
        if (this.ActionMode == ActionMode.Bulk)
            throw new NotSupportedException("批量插入时，不支持此方法的调用，请使用OnDuplicateKeyUpdate<TUpdateFields>(Expression<Func<IMySqlCreateDuplicateKeyUpdate<TEntity>, TUpdateFields>> fieldsAssignment)方法");

        this.UpdateBuilder = new();
        this.VisitSetObject(updateObj);
    }
    public void OnDuplicateKeyUpdate(LambdaExpression updateExpr)
    {
        this.UpdateBuilder = new();
        this.VisitSetExpression(updateExpr);
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

        var tableSegment = this.Tables[0];
        var entityType = tableSegment.EntityType;
        var updateObjType = updateObj.GetType();

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

                var parameterName = $"{this.OrmProvider.ParameterPrefix}{memberMapper.MemberName}";
                if (this.UpdateBuilder.Length > 0) this.FieldsBuilder.Append(',');
                this.UpdateBuilder.Append($"{this.OrmProvider.GetFieldName(memberMapper.FieldName)}={parameterName}");

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
            var commandInitializer = RepositoryHelper.BuildTypedCommandInitializer(this.DbContext, entityType, updateObjType, 2, false, false, null, null);
            var typedCommandInitializer = commandInitializer as Action<IDataParameterCollection, StringBuilder, DbContext, object>;
            typedCommandInitializer.Invoke(this.DbParameters, this.UpdateBuilder, this.DbContext, updateObj);
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
                    MappedTargetType = memberMapper.MappedTargetType,
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
                MappedTargetType = memberMapper.MappedTargetType,
                TypeHandler = memberMapper.TypeHandler
            });
            this.AddMemberElement(sqlSegment, memberMapper);
        }
        return this.Evaluate(sqlSegment);
    }
    public override IQueryVisitor CreateQueryVisitor(char? tableAsStart = null)
    {
        var queryVisitor = this.OrmProvider.NewQueryVisitor(this.DbContext, tableAsStart ?? this.TableAsStart, this.DbParameters) as MySqlQueryVisitor;
        queryVisitor.RefQueries = this.RefQueries;
        queryVisitor.ShardingTables = this.ShardingTables;
        queryVisitor.RefTableAliases = this.RefTableAliases;
        queryVisitor.IncludeTables = this.IncludeTables;
        queryVisitor.IsRecursive = this.IsRecursive;
        queryVisitor.CteQueryObj = this.CteQueryObj;
        queryVisitor.RefFrom = this;
        queryVisitor.Tables = this.Tables;

        queryVisitor.IsUseIgnoreInto = this.IsUseIgnoreInto;
        return queryVisitor;
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
            var parameterName = this.OrmProvider.ParameterPrefix + this.UserParameterPrefix + this.DbParameters.Count.ToString();

            var dbFieldValue = sqlSegment.Value;
            if (memberMapper.TypeHandler != null)
                dbFieldValue = memberMapper.TypeHandler.ToFieldValue(dbFieldValue);
            else
            {
                var targetType = memberMapper.MappedTargetType;
                var valueGetter = this.OrmProvider.GetParameterValueGetter(dbFieldValue.GetType(), targetType, false, this.DbContext);
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
        //在前面insert的时候，参数有可能已经添加过了，此处需要判断是否需要添加
        if (!this.DbParameters.Contains(parameterName))
        {
            if (memberMapper.TypeHandler != null)
                fieldValue = memberMapper.TypeHandler.ToFieldValue(fieldValue);
            else
            {
                var targetType = memberMapper.MappedTargetType;
                var valueGetter = this.OrmProvider.GetParameterValueGetter(fieldValue.GetType(), targetType, false, this.DbContext);
                fieldValue = valueGetter.Invoke(fieldValue);
            }
            this.DbParameters.Add(this.OrmProvider.CreateParameter(parameterName, memberMapper.NativeDbType, fieldValue));
        }
        if (this.UpdateBuilder.Length > 0) this.UpdateBuilder.Append(',');
        this.UpdateBuilder.Append($"{this.OrmProvider.GetFieldName(memberMapper.FieldName)}={parameterName}");
    }
    public override void Dispose()
    {
        base.Dispose();
        this.UpdateBuilder = null;
        this.SetRowAlias = null;
        this.FromSql = null;
        this.OutputSql = null;
    }
}