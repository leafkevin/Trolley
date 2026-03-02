using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Text;

namespace Trolley.PostgreSql;

public class PostgreSqlCreateVisitor : CreateVisitor
{
    private bool isUseKeysOrConstraint = false;
    public bool IsUpdate { get; set; }
    /// <summary>
    /// 当有OnConflict更新操作时，引用原值时才会设置，使用IsNeedTableAlias会影响正常Set<TField>(Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue)场景的解析
    /// </summary>
    public bool IsUseTableAlias { get; set; }

    public PostgreSqlCreateVisitor(Type entityType, DbContext dbContext, char tableAsStart = 'a')
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
                if (this.DbParameters == null) this.DbParameters = command.Parameters;
                else if (this.DbParameters != command.Parameters)
                {
                    foreach (var dbParameter in this.DbParameters)
                    {
                        command.Parameters.Add(dbParameter);
                    }
                    this.DbParameters = command.Parameters;
                }
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
                            case "SetField":
                                this.VisitSetField(deferredSegment.Value);
                                break;
                            case "SetFieldExpr":
                                this.VisitSetFieldExpr(deferredSegment.Value);
                                break;
                            case "SetFieldExprs":
                                this.VisitSetFieldExprs(deferredSegment.Value);
                                break;
                            case "SetWhere":
                                this.VisitSetWhere(deferredSegment.Value);
                                break;
                        }
                    }
                    var tableName = this.GetTableName(tableSegment);
                    if (this.IsReturnIdentity && (this.UpdateBuilder != null || this.OutputSql != null))
                        throw new NotSupportedException("返回Identity，不支持同时Returning操作");
                    this.FromSql = $"INSERT INTO {tableName} ({this.FieldsBuilder}) VALUES ({this.ValuesBuilder})";
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
                    var entityMapper = tableSegment.Mapper;
                    if (!entityMapper.IsAutoIncrementKey)
                        throw new NotSupportedException($"实体{entityMapper.EntityType.FullName}表未配置自增长字段，无法返回Identity值");
                    var keyFieldName = this.OrmProvider.GetFieldName(entityMapper.KeyMembers[0].FieldName);
                    tailSql = this.OrmProvider.GetIdentitySql(keyFieldName);
                }
                break;
        }
        this.FieldsBuilder.Clear();
        return $"{this.FromSql}{tailSql}";
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

        var headSql = $"INSERT INTO ";
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
                    case "SetField":
                        this.VisitSetField(deferredSegment.Value);
                        break;
                    case "SetFieldExpr":
                        this.VisitSetFieldExpr(deferredSegment.Value);
                        break;
                    case "SetFieldExprs":
                        this.VisitSetFieldExprs(deferredSegment.Value);
                        break;
                    case "SetWhere":
                        this.VisitSetWhere(deferredSegment.Value);
                        break;
                    default: throw new NotSupportedException("批量插入后，只支持WithBy/OnConflict/Returning操作");
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
    public virtual void SetWhere(Expression whereExpr)
    {
        this.deferredSegments.Add(new CommandSegment
        {
            Type = "SetWhere",
            Value = whereExpr
        });
    }
    public (ShardingTableType, object, IEnumerable, List<MemberMap>, List<Func<object, object>>) BuildWithBulkCopy()
    {
        var insertObjs = this.deferredSegments[0].Value as IEnumerable;
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
        return (shardingType, shardingTables, insertObjs, memberMappers, valueGetters);
    }

    public void WithBulkCopy(IEnumerable insertObjs)
    {
        this.ActionMode = ActionMode.BulkCopy;
        this.deferredSegments.Add(new CommandSegment
        {
            Type = "WithBulkCopy",
            Value = insertObjs
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
    public override SqlFieldSegment VisitMemberAccess(SqlFieldSegment sqlSegment)
    {
        if (this.IsUpdate)
        {
            var memberExpr = sqlSegment.Expression as MemberExpression;
            if (!this.Tables[0].Mapper.TryGetMemberMap(memberExpr.Member.Name, out var memberMapper))
                throw new MissingMemberException($"类{this.Tables[0].EntityType.FullName}未找到成员{memberExpr.Member.Name}");

            //更新时的成员访问就是引用原值
            this.IsUseTableAlias = true;
            var fieldName = $"{this.Tables[0].AliasName}.{this.OrmProvider.GetFieldName(memberMapper.FieldName)}";

            sqlSegment.HasField = true;
            sqlSegment.SegmentType = memberMapper.MemberType;
            sqlSegment.FromMember = memberMapper.Member;
            sqlSegment.NativeDbType = memberMapper.NativeDbType;
            sqlSegment.MappedTargetType = memberMapper.MappedTargetType;
            sqlSegment.TypeHandler = memberMapper.TypeHandler;
            sqlSegment.Body = fieldName;
            return sqlSegment;
        }
        return base.VisitMemberAccess(sqlSegment);
    }
    public override SqlFieldSegment VisitNew(SqlFieldSegment sqlSegment)
    {
        //只有OnConflictDoUpdate.Set时，才会走到此场景，如：.Set(f => new { TotalAmount = f.TotalAmount + x.Excluded(f.TotalAmount) })
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
    public virtual void InitTableAlias(LambdaExpression lambdaExpr)
    {
        this.TableAliases.Clear();
        lambdaExpr.Body.TryGetParameters(out var parameters);
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
    public virtual void AddMemberElement(SqlFieldSegment sqlSegment, MemberMap memberMapper)
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

    public virtual void DoNothing() => this.UpdateBuilder.Append(" DO NOTHING");
    public virtual void UseKeys()
    {
        if (this.isUseKeysOrConstraint)
            throw new InvalidOperationException("已使用UseKeys或UseConstraint，不能重复使用");

        this.UpdateBuilder.Append(" (");
        var keyMappers = this.Tables[0].Mapper.KeyMembers;
        if (keyMappers.Count > 1)
        {
            for (int i = 0; i < keyMappers.Count; i++)
            {
                if (i > 0) this.UpdateBuilder.Append(',');
                var keyMapper = keyMappers[i];
                this.UpdateBuilder.Append(this.OrmProvider.GetFieldName(keyMapper.FieldName));
            }
        }
        else this.UpdateBuilder.Append(this.OrmProvider.GetFieldName(keyMappers[0].FieldName));
        this.UpdateBuilder.Append(") DO UPDATE SET");
        this.isUseKeysOrConstraint = true;
    }
    public void UseConstraint(string constraintName)
    {
        if (this.isUseKeysOrConstraint)
            throw new InvalidOperationException("已使用UseKeys或UseConstraint，不能重复使用");

        if (string.IsNullOrEmpty(constraintName))
            throw new ArgumentNullException("参数constraintName不可为null");
        this.UpdateBuilder.Append($" ON CONSTRAINT {constraintName} DO UPDATE SET");
        this.isUseKeysOrConstraint = true;
    }
    public virtual void VisitSetWhere(object deferredSegmentValue)
    {
        var whereExpr = deferredSegmentValue as Expression;
        var sqlSegment = this.VisitAndDeferred(new SqlFieldSegment { Expression = whereExpr });
        this.UpdateBuilder.Append($" WHERE {sqlSegment.Body}");
    }
    public override void Dispose()
    {
        base.Dispose();
        this.UpdateBuilder = null;
        this.FromSql = null;
        this.OutputSql = null;
    }
}