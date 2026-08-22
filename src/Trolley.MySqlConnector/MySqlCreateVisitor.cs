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
    public bool IsUseIgnoreInto { get; set; }
    public bool IsUseSetAlias { get; set; }
    public string RowAlias { get; set; } = "newRow";
    public bool IsSetValue { get; set; }

    public MySqlCreateVisitor(Type entityType, DbContext dbContext, char tableAsStart = 'a', ITheaCommand command = null)
        : base(entityType, dbContext, tableAsStart, command) { }

    public override string BuildSql(out List<ReaderField> readerFields)
    {
        string tailSql = null;
        readerFields = this.ReaderFields;

        switch (this.ActionMode)
        {
            case ActionMode.Bulk:
                (var shardingType, var shardingTables, var insertObjs, _, var firstSqlSetter,
                    var loopSqlSetter, tailSql, readerFields) = this.BuildWithBulk(this.Command);

                int index = 0;
                var builder = new StringBuilder();
                if (shardingType == ShardingTableType.SplitTables)
                {
                    var tabledInsertObjs = shardingTables as Dictionary<string, List<object>>;
                    foreach (var tableName in tabledInsertObjs.Keys)
                    {
                        firstSqlSetter.Invoke(this.DbParameters, builder, tableName);
                        var tableParameters = tabledInsertObjs[tableName];
                        foreach (var insertObj in tableParameters)
                        {
                            loopSqlSetter.Invoke(this.DbParameters, builder, this.DbContext, insertObj, index.ToString());
                            index++;
                        }
                    }
                }
                else
                {
                    firstSqlSetter.Invoke(this.DbParameters, builder, shardingTables as string);
                    foreach (var insertObj in insertObjs)
                    {
                        loopSqlSetter.Invoke(this.DbParameters, builder, this.DbContext, insertObj, index.ToString());
                        index++;
                    }
                }
                builder.Remove(builder.Length - 1, 1);
                this.FromSql = builder.ToString();
                break;
            case ActionMode.Single:
                //当Insert Select From操作时，DbParameters也有值，但不是command.Parameters，需要赋值到command.Parameters
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
                    var tableName = this.GetTableName(tableSegment);
                    if (this.IsReturnIdentity && (this.UpdateBuilder != null || this.OutputSql != null))
                        throw new NotSupportedException("返回Identity，不支持同时Returning操作");
                    this.FromSql = $"{this.BuildHeadSql()} {tableName} ({this.FieldsBuilder}) VALUES ({this.ValuesBuilder})";
                    this.ValuesBuilder.Clear();
                }
                if (this.UpdateBuilder != null)
                    tailSql = this.UpdateBuilder.ToString();

                if (this.OutputSql != null)
                {
                    tailSql += this.OutputSql;
                    readerFields = this.ReaderFields;
                }
                if (this.IsReturnIdentity)
                {
                    if (!tableSegment.Mapper.IsAutoIncrementKey)
                        throw new NotSupportedException($"实体{tableSegment.EntityType.FullName}表未配置自增长字段，无法返回Identity值");
                    tailSql = this.OrmProvider.GetIdentitySql(null);
                }
                break;
        }
        this.FieldsBuilder.Clear();
        return $"{this.FromSql}{tailSql}";
    }
    public override void UseTableSchema(bool isIncludeMany, string tableSchema)
    {
        if (tableSchema == this.DbContext.DefaultTableSchema) return;

        var tableSegment = isIncludeMany ? this.IncludeTables.Last() : this.Tables.Last();
        tableSegment.TableSchema = tableSchema;
    }
    public override (ShardingTableType, object, IEnumerable, int, Action<IDataParameterCollection, StringBuilder, string>,
        Action<IDataParameterCollection, StringBuilder, DbContext, object, string>, string, List<ReaderField>) BuildWithBulk(ITheaCommand command)
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
                fixedDbParameters.ForEach(f => dbParameters.Add(f));
            };
        }
        else
        {
            firstSqlSetter = (dbParameters, builder, tableName) =>
            {
                builder.Append($"{headSql}{this.OrmProvider.GetTableName(tableName)} ");
                builder.Append(fixedFieldsSql);
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
                shardingTables = this.SplitShardingParameters(tableSegment.TableShardingInfo, insertObjType, insertObjs, firstInsertObj, this.ShardingValues);
            }
        }
        string tailSql = null;
        if (this.UpdateBuilder != null)
            tailSql = this.UpdateBuilder.ToString();
        if (this.OutputSql != null)
            tailSql += this.OutputSql;
        return (shardingType, shardingTables, insertObjs, bulkCount, firstSqlSetter, loopSqlSetter, tailSql, this.ReaderFields);
    }
    public virtual void WithBulkCopy(IEnumerable insertObjs, int? timeoutSeconds)
    {
        this.ActionMode = ActionMode.BulkCopy;
        this.deferredSegments.Add(new CommandSegment
        {
            Type = "WithBulkCopy",
            Value = (insertObjs, timeoutSeconds)
        });
    }
    public virtual void Returning(string fieldNames)
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
                this.ReaderFields.Add(new ReaderField
                {
                    FieldType = ReaderFieldType.Field,
                    TargetMember = memberMapper.Member,
                    ReaderType = memberMapper.MemberType,
                    MemberMapper = memberMapper,
                    MemberName = memberMapper.FieldName,
                    //NativeDbType = memberMapper.NativeDbType,
                    //MappedTargetType = memberMapper.MappedTargetType,
                    //TypeHandler = memberMapper.TypeHandler,
                    Value = memberMapper.FieldName
                });
            }
        }
        else
        {
            this.ReaderFields.Add(new ReaderField
            {
                FieldType = ReaderFieldType.RawSql,
                Value = fieldNames
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
                    var sqlSegment = this.Visit(new SqlSegment { Expression = memberExpr });
                    var fieldName = this.WrapSql(sqlSegment);
                    builder.Append(fieldName);
                    var isNeedAlias = false;
                    if (sqlSegment.MemberName != memberExpr.Member.Name)
                    {
                        isNeedAlias = true;
                        builder.Append($" AS {this.OrmProvider.GetFieldName(memberExpr.Member.Name)}");
                    }
                    this.ReaderFields.Add(new ReaderField
                    {
                        FieldType = ReaderFieldType.Field,
                        TargetMember = memberExpr.Member,
                        ReaderType = memberExpr.Type,
                        MemberMapper = sqlSegment.MemberMapper,
                        MemberName = sqlSegment.MemberMapper?.FieldName,
                        //MappedTargetType = sqlSegment.MappedTargetType,
                        //TypeHandler = sqlSegment.TypeHandler,
                        Value = fieldName,
                        IsNeedAlias = isNeedAlias
                    });
                }
                break;
            case ExpressionType.New:
                var newExpr = fieldsSelector.Body as NewExpression;
                for (int i = 0; i < newExpr.Arguments.Count; i++)
                {
                    var memberInfo = newExpr.Members[i];
                    var sqlSegment = this.Visit(new SqlSegment { Expression = newExpr.Arguments[i] });
                    var fieldName = this.WrapSql(sqlSegment);
                    if (i > 0) builder.Append(',');
                    builder.Append(fieldName);

                    var isNeedAlias = false;
                    if (sqlSegment.SqlType > SqlType.OnlyField || sqlSegment.IsVariable || sqlSegment.MemberName != memberInfo.Name)
                    {
                        isNeedAlias = true;
                        builder.Append($" AS {this.OrmProvider.GetFieldName(memberInfo.Name)}");
                    }
                    this.ReaderFields.Add(new ReaderField
                    {
                        FieldType = ReaderFieldType.Field,
                        TargetMember = memberInfo,
                        ReaderType = memberInfo.GetMemberType(),
                        MemberMapper = sqlSegment.MemberMapper,
                        MemberName = sqlSegment.MemberMapper?.FieldName,
                        //MappedTargetType = sqlSegment.MappedTargetType,
                        //TypeHandler = sqlSegment.TypeHandler,
                        Value = fieldName,
                        IsNeedAlias = isNeedAlias
                    });
                }
                break;
            case ExpressionType.MemberInit:
                var memberInitExpr = fieldsSelector.Body as MemberInitExpression;
                for (int i = 0; i < memberInitExpr.Bindings.Count; i++)
                {
                    if (memberInitExpr.Bindings[i].BindingType != MemberBindingType.Assignment)
                        throw new NotSupportedException("暂时不支持除MemberBindingType.Assignment类型外的成员绑定表达式");

                    var memberAssignment = memberInitExpr.Bindings[i] as MemberAssignment;
                    var sqlSegment = this.Visit(new SqlSegment { Expression = memberAssignment.Expression });
                    var fieldName = this.WrapSql(sqlSegment);
                    if (i > 0) builder.Append(',');
                    builder.Append(fieldName);

                    var isNeedAlias = false;
                    if (sqlSegment.SqlType > SqlType.OnlyField || sqlSegment.IsVariable || sqlSegment.MemberName != memberAssignment.Member.Name)
                    {
                        isNeedAlias = true;
                        builder.Append($" AS {this.OrmProvider.GetFieldName(memberAssignment.Member.Name)}");
                    }
                    this.ReaderFields.Add(new ReaderField
                    {
                        FieldType = ReaderFieldType.Field,
                        TargetMember = memberAssignment.Member,
                        ReaderType = memberAssignment.Member.GetMemberType(),
                        MemberMapper = sqlSegment.MemberMapper,
                        MemberName = sqlSegment.MemberMapper?.FieldName,
                        //MappedTargetType = sqlSegment.MappedTargetType,
                        //TypeHandler = sqlSegment.TypeHandler,
                        Value = fieldName,
                        IsNeedAlias = isNeedAlias
                    });
                }
                break;
            case ExpressionType.Parameter:
                foreach (var memberMapper in entityMapper.MemberMaps)
                {
                    if (memberMapper.IsIgnore || memberMapper.IsNavigation)
                        continue;

                    this.ReaderFields.Add(new ReaderField
                    {
                        FieldType = ReaderFieldType.Field,
                        TargetMember = memberMapper.Member,
                        ReaderType = memberMapper.MemberType,
                        MemberMapper = memberMapper,
                        MemberName = memberMapper.FieldName,
                        //MappedTargetType = memberMapper.MappedTargetType,
                        //TypeHandler = memberMapper.TypeHandler,
                        Value = memberMapper.FieldName
                    });
                }
                builder.Append('*');
                break;
            default:
                {
                    var sqlSegment = this.Visit(new SqlSegment { Expression = fieldsSelector });
                    for (int i = 0; i < this.ReaderFields.Count; i++)
                    {
                        var readerField = this.ReaderFields[i];
                        if (i > 0) builder.Append(',');
                        builder.Append(readerField.Value);
                    }
                    this.ReaderFields.Add(new ReaderField
                    {
                        FieldType = ReaderFieldType.Field,
                        ReaderType = fieldsSelector.Type,
                        Value = sqlSegment.Value
                    });
                }
                break;
        }
        this.OutputSql = builder.ToString();
        builder.Clear();
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
                shardingTables = this.SplitShardingParameters(tableSegment.TableShardingInfo, insertObjType, insertObjs, firstInsertObj, this.ShardingValues);
            }
        }
        (var memberMappers, var valueGetters) = this.GetRefMemberMappers(insertObjType, tableSegment.Mapper, firstInsertObj, false);
        return (shardingType, shardingTables, insertObjs, timeoutSeconds, memberMappers, valueGetters);
    }
    public virtual string BuildHeadSql()
    {
        if (this.IsUseIgnoreInto) return "INSERT IGNORE INTO";
        return "INSERT INTO";
    }
    public override SqlSegment VisitMemberAccess(SqlSegment sqlSegment)
    {
        var mySqlSegment = base.VisitMemberAccess(sqlSegment);
        if (this.IsUseSetAlias && this.IsSetValue)
            mySqlSegment.Value = $"{this.RowAlias}.{mySqlSegment.Value}";
        return mySqlSegment;
    }
    public override SqlSegment VisitNew(SqlSegment sqlSegment)
    {
        //只有OnDuplicateKeyUpdate.Set时，才会走到此场景，如：.Set(f => new { TotalAmount = f.TotalAmount + f.Values(f.TotalAmount) })
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
                this.AddMemberElement(newExpr.Arguments[i], memberMapper);
            }
            return sqlSegment;
        }
        return sqlSegment.Change(sqlSegment.Expression.Evaluate(), SqlType.Constant);
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
            this.AddMemberElement(memberAssignment.Expression, memberMapper);
        }
        return sqlSegment.Change(sqlSegment.Expression.Evaluate(), SqlType.Constant);
    }
    public override void VisitSetFieldExprs(object deferredSegmentValue)
    {
        (var fieldSelector, var valueGetter) = ((Expression, Expression))deferredSegmentValue;
        this.InitTableAlias(fieldSelector as LambdaExpression);
        var fieldSegment = this.Visit(new SqlSegment { Expression = fieldSelector });
        this.IsSetValue = true;
        this.InitTableAlias(valueGetter as LambdaExpression);
        var valueSegment = this.Visit(new SqlSegment { Expression = valueGetter });
        this.IsSetValue = false;
        if (this.UpdateIndex > 0) this.UpdateBuilder.Append(',');
        this.UpdateBuilder.Append($"{fieldSegment.Value}={valueSegment.Value}");
        this.UpdateIndex++;
    }
    public override IQueryVisitor CreateQueryVisitor(char? tableAsStart = null)
    {
        var queryVisitor = this.OrmProvider.NewQueryVisitor(this.DbContext, tableAsStart ?? this.TableAliasStart, this.Command) as MySqlQueryVisitor;
        queryVisitor.RefQueries = this.RefQueries;
        queryVisitor.ShardingTables = this.ShardingTables;
        queryVisitor.RefTableAliases = this.RefTableAliases;
        queryVisitor.IncludeTables = this.IncludeTables;
        queryVisitor.IsRecursive = this.IsRecursive;
        queryVisitor.CteQueryObj = this.CteQueryObj;
        //queryVisitor.RefFrom = this;
        queryVisitor.Tables = this.Tables;

        queryVisitor.IsUseIgnoreInto = this.IsUseIgnoreInto;
        return queryVisitor;
    }
    public virtual void AddMemberElement(Expression fieldExpr, MemberMap memberMapper)
    {
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
        //带有参数或字段的表达式或函数调用、或是只有参数或字段
        //.Set(true, f => f.TotalAmount))
        //.Set(f => new { TotalAmount = x.Values(f.TotalAmount) })
        else
        {
            var fieldValue = sqlSegment.Value;
            if (this.IsUseSetAlias) fieldValue = $"{this.RowAlias}.{fieldValue}";
            this.UpdateBuilder.Append($"{fieldName}={fieldValue}");
        }
    }
    public override void Dispose()
    {
        base.Dispose();
        this.UpdateBuilder = null;
        this.RowAlias = null;
        this.FromSql = null;
        this.OutputSql = null;
    }
}