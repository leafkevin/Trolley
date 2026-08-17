using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Text;

namespace Trolley.PostgreSql;

public class PostgreSqlUpdateVisitor : UpdateVisitor, IUpdateVisitor
{
    public string OutputSql { get; set; }

    public PostgreSqlUpdateVisitor(Type entityType, DbContext dbContext, char tableAsStart = 'a')
        : base(entityType, dbContext, tableAsStart) { }
    public override string BuildSql(ITheaCommand command, out List<ReaderField> readerFields)
    {
        string sql = null;
        readerFields = null;
        this.hasOnlyFields = this.OnlyFieldNames != null && this.OnlyFieldNames.Count > 0;
        this.hasIgnoreFields = this.IgnoreFieldNames != null && this.IgnoreFieldNames.Count > 0;
        if (this.HasWhere) this.WhereBuilder = new();

        var builder = new StringBuilder();
        var tableSegment = this.Tables[0];
        var shardingType = tableSegment.ShardingType;
        object shardingTables = tableSegment.Mapper.TableName;
        switch (this.ActionMode)
        {
            case ActionMode.Bulk:
                {
                    (shardingType, shardingTables, var updateObjs, _, var fixedSqlSetter,
                        var loopSqlSetter, readerFields) = this.BuildSetBulk(command);

                    int index = 0;
                    fixedSqlSetter?.Invoke(command.Parameters);
                    if (shardingType == ShardingTableType.SplitTables)
                    {
                        var tabledUpdateObjs = shardingTables as Dictionary<string, List<object>>;
                        foreach (var tableName in tabledUpdateObjs.Keys)
                        {
                            var tableParameters = tabledUpdateObjs[tableName];
                            foreach (var updateObj in tableParameters)
                            {
                                loopSqlSetter.Invoke(command.Parameters, builder, this.DbContext, tableName, updateObj, index.ToString());
                                index++;
                            }
                        }
                    }
                    else
                    {
                        foreach (var updateObj in updateObjs)
                        {
                            switch (shardingType)
                            {
                                case ShardingTableType.None:
                                case ShardingTableType.SingleTable:
                                    loopSqlSetter.Invoke(command.Parameters, builder, this.DbContext, shardingTables as string, updateObj, index.ToString());
                                    break;
                                case ShardingTableType.MultiTable:
                                case ShardingTableType.ShardingTableMap:
                                    var tableNames = shardingTables as List<string>;
                                    foreach (var tableName in tableNames)
                                    {
                                        loopSqlSetter.Invoke(command.Parameters, builder, this.DbContext, tableName, updateObj, index.ToString());
                                    }
                                    break;
                            }
                            index++;
                        }
                    }
                    sql = builder.ToString();
                }
                break;
            case ActionMode.Single:
                {
                    this.FieldsBuilder = new();
                    this.DbParameters = command.Parameters;
                    var entityType = tableSegment.EntityType;
                    Func<IDataParameterCollection, DbContext, object, string> whereSqlInitializer = null;
                    foreach (var deferredSegment in this.deferredSegments)
                    {
                        switch (deferredSegment.Type)
                        {
                            case "Set":
                                this.VisitSet(deferredSegment.Value as Expression);
                                break;
                            case "SetFrom":
                                this.VisitSet(deferredSegment.Value as Expression);
                                break;
                            case "SetField":
                                this.VisitSetField(deferredSegment.Value);
                                break;
                            case "SetFieldExpr":
                                this.VisitSetFieldExpr(deferredSegment.Value);
                                break;
                            case "SetWith":
                                this.VisitSetWith(deferredSegment.Value);
                                break;
                            case "SetFromField":
                                this.VisitSetFromField(deferredSegment.Value);
                                break;
                            case "AndBy":
                                whereSqlInitializer = RepositoryHelper.BuildWhereCommandInitializer(this.DbContext, entityType, deferredSegment.Value, 4, false, false, false);
                                this.VisitAndSql(whereSqlInitializer.Invoke(this.DbParameters, this.DbContext, deferredSegment.Value));
                                break;
                            case "AndById":
                                whereSqlInitializer = RepositoryHelper.BuildWhereCommandInitializer(this.DbContext, entityType, deferredSegment.Value, 4, true, false, false);
                                this.VisitAndSql(whereSqlInitializer.Invoke(this.DbParameters, this.DbContext, deferredSegment.Value));
                                break;
                            case "AndByIds":
                                whereSqlInitializer = RepositoryHelper.BuildWhereCommandInitializer(this.DbContext, entityType, deferredSegment.Value, 4, true, false, true);
                                this.VisitAndSql(whereSqlInitializer.Invoke(this.DbParameters, this.DbContext, deferredSegment.Value));
                                break;
                            case "And":
                                this.VisitAnd(deferredSegment.Value as Expression);
                                break;
                            case "OrBy":
                                whereSqlInitializer = RepositoryHelper.BuildWhereCommandInitializer(this.DbContext, entityType, deferredSegment.Value, 4, false, false, false);
                                this.VisitOrSql(whereSqlInitializer.Invoke(this.DbParameters, this.DbContext, deferredSegment.Value));
                                break;
                            case "OrById":
                                whereSqlInitializer = RepositoryHelper.BuildWhereCommandInitializer(this.DbContext, entityType, deferredSegment.Value, 4, true, false, false);
                                this.VisitOrSql(whereSqlInitializer.Invoke(this.DbParameters, this.DbContext, deferredSegment.Value));
                                break;
                            case "OrByIds":
                                whereSqlInitializer = RepositoryHelper.BuildWhereCommandInitializer(this.DbContext, entityType, deferredSegment.Value, 4, true, false, true);
                                this.VisitOrSql(whereSqlInitializer.Invoke(this.DbParameters, this.DbContext, deferredSegment.Value));
                                break;
                            case "Or":
                                this.VisitOr(deferredSegment.Value as Expression);
                                break;
                        }
                    }
                    builder.Append($"UPDATE {this.GetFormatTableName(tableSegment)}");
                    if (this.IsNeedTableAlias) builder.Append($" AS {this.Tables[0].AliasName}");
                    builder.Append(" SET ");
                    if (this.FieldsBuilder.Length > 0)
                        builder.Append(this.FieldsBuilder.ToString());

                    var whereSql = string.Empty;
                    if (this.IsJoin)
                    {
                        builder.Append(" FROM ");
                        if (this.HasWhere) this.WhereBuilder.Append(" AND ");
                        else
                        {
                            this.WhereBuilder = new();
                            this.HasWhere = true;
                        }

                        for (var i = 1; i < this.Tables.Count; i++)
                        {
                            var myTableSegment = this.Tables[i];
                            var tableName = this.GetFormatTableName(myTableSegment);
                            builder.Append($"{tableName} AS {myTableSegment.AliasName}");
                            this.WhereBuilder.Append(tableSegment.OnExpr);
                        }
                    }
                    if (this.HasWhere)
                        builder.Append($" WHERE {this.WhereBuilder.ToString()}");
                    if (!string.IsNullOrEmpty(this.OutputSql))
                        builder.Append(this.OutputSql);
                    sql = builder.ToString();
                    if (this.ShardingTables != null && this.ShardingTables.Count > 0)
                        sql = this.DbContext.BuildShardingTablesSqlByFormat(this, sql, ";");
                }
                break;
        }
        builder.Clear();
        return sql;
    }
    public override (ShardingTableType, object, IEnumerable, int, Action<IDataParameterCollection>,
        Action<IDataParameterCollection, StringBuilder, DbContext, string, object, string>, List<ReaderField>) BuildSetBulk(ITheaCommand command)
    {
        (var updateObjs, var bulkCount) = ((IEnumerable, int))this.deferredSegments[0].Value;
        object firstUpdateObj = null;
        foreach (var updateObj in updateObjs)
        {
            firstUpdateObj = updateObj;
            break;
        }
        var updateObjType = firstUpdateObj.GetType();
        var tableSegment = this.Tables[0];
        var entityType = tableSegment.EntityType;

        var headSql = "UPDATE";
        if (!string.IsNullOrEmpty(tableSegment.TableSchema))
            headSql += $" {this.OrmProvider.GetTableName(tableSegment.TableSchema)}.";
        var fixedHeadSql = "SET ";
        var fixedTailSql = ";";

        List<IDbDataParameter> fixedDbParameters = null;
        Action<IDataParameterCollection> firstSqlSetter = null;
        Action<IDataParameterCollection, StringBuilder, DbContext, string, object, string> loopSqlSetter = null;
        if (this.deferredSegments.Count > 1)
        {
            this.FieldsBuilder = new();
            var tempDbParameters = new TheaDbParameterCollection();
            this.DbParameters = tempDbParameters;
            Func<IDataParameterCollection, DbContext, object, string> whereSqlInitializer = null;
            for (int i = 1; i < this.deferredSegments.Count; i++)
            {
                var deferredSegment = this.deferredSegments[i];
                switch (deferredSegment.Type)
                {
                    case "Set":
                        this.VisitSet(deferredSegment.Value as Expression);
                        break;
                    case "SetFrom":
                        this.VisitSet(deferredSegment.Value as Expression);
                        break;
                    case "SetField":
                        this.VisitSetField(deferredSegment.Value);
                        break;
                    case "SetFieldExpr":
                        this.VisitSetFieldExpr(deferredSegment.Value);
                        break;
                    case "SetWith":
                        this.VisitSetWith(deferredSegment.Value);
                        break;
                    case "SetFromField":
                        this.VisitSetFromField(deferredSegment.Value);
                        break;
                    case "AndBy":
                        whereSqlInitializer = RepositoryHelper.BuildWhereCommandInitializer(this.DbContext, entityType, deferredSegment.Value, 4, false, false, false);
                        this.VisitAndSql(whereSqlInitializer.Invoke(this.DbParameters, this.DbContext, deferredSegment.Value));
                        break;
                    case "AndById":
                        whereSqlInitializer = RepositoryHelper.BuildWhereCommandInitializer(this.DbContext, entityType, deferredSegment.Value, 4, true, false, false);
                        this.VisitAndSql(whereSqlInitializer.Invoke(this.DbParameters, this.DbContext, deferredSegment.Value));
                        break;
                    case "AndByIds":
                        whereSqlInitializer = RepositoryHelper.BuildWhereCommandInitializer(this.DbContext, entityType, deferredSegment.Value, 4, true, false, true);
                        this.VisitAndSql(whereSqlInitializer.Invoke(this.DbParameters, this.DbContext, deferredSegment.Value));
                        break;
                    case "And":
                        this.VisitAnd(deferredSegment.Value as Expression);
                        break;
                    case "OrBy":
                        whereSqlInitializer = RepositoryHelper.BuildWhereCommandInitializer(this.DbContext, entityType, deferredSegment.Value, 4, false, false, false);
                        this.VisitOrSql(whereSqlInitializer.Invoke(this.DbParameters, this.DbContext, deferredSegment.Value));
                        break;
                    case "OrById":
                        whereSqlInitializer = RepositoryHelper.BuildWhereCommandInitializer(this.DbContext, entityType, deferredSegment.Value, 4, true, false, false);
                        this.VisitOrSql(whereSqlInitializer.Invoke(this.DbParameters, this.DbContext, deferredSegment.Value));
                        break;
                    case "OrByIds":
                        whereSqlInitializer = RepositoryHelper.BuildWhereCommandInitializer(this.DbContext, entityType, deferredSegment.Value, 4, true, false, true);
                        this.VisitOrSql(whereSqlInitializer.Invoke(this.DbParameters, this.DbContext, deferredSegment.Value));
                        break;
                    case "Or":
                        this.VisitOr(deferredSegment.Value as Expression);
                        break;
                    default: throw new NotSupportedException("SetBulk操作后，只支持Set/IgnoreFields/OnlyFields/Returning操作");
                }
            }
            if (this.DbParameters.Count > 0)
            {
                fixedDbParameters = tempDbParameters.ToList();
                firstSqlSetter = dbParameters => fixedDbParameters.ForEach(f => dbParameters.Add(f));
            }
            if (this.FieldsBuilder.Length > 0)
                fixedHeadSql = $"SET {this.FieldsBuilder.ToString()},";
            if (this.WhereBuilder.Length > 0)
                fixedTailSql = $" AND {this.WhereBuilder.ToString()};";
            this.DbParameters = command.Parameters;
        }
        if (firstUpdateObj is IDictionary<string, object> dict)
        {
            var entityMapper = this.DbContext.EntityMapProvider.GetEntityMap(entityType);
            (var valueSetters, var whereSetters) = this.BuildDictBulkCommandInitializer(entityMapper, dict);
            loopSqlSetter = (dbParameters, builder, dbContext, tableName, updateObj, index) =>
            {
                var dictObj = updateObj as IDictionary<string, object>;
                builder.Append($"{headSql}{this.OrmProvider.GetTableName(tableName)} {fixedHeadSql}");
                foreach (var valueSetter in valueSetters)
                    valueSetter.Invoke(dbParameters, builder, dictObj, index.ToString());
                builder.Append(" WHERE ");
                foreach (var valueSetter in whereSetters)
                    valueSetter.Invoke(dbParameters, builder, dictObj, index.ToString());
                builder.Append(fixedTailSql);
            };
        }
        else
        {
            var commandInitializer = RepositoryHelper.BuildTypedBulkCommandInitializer(this.DbContext, entityType, updateObjType, 2, this.OnlyFieldNames, this.IgnoreFieldNames)
                as Action<IDataParameterCollection, StringBuilder, DbContext, object, string>;
            loopSqlSetter = (dbParameters, builder, dbContext, tableName, updateObj, index) =>
            {
                builder.Append($"{headSql}{this.OrmProvider.GetTableName(tableName)} {fixedHeadSql}");
                commandInitializer.Invoke(dbParameters, builder, dbContext, updateObj, index.ToString());
                builder.Append(fixedTailSql);
            };
        }

        var shardingType = ShardingTableType.None;
        object shardingTables = tableSegment.Mapper.TableName;
        if (tableSegment.TableShardingInfo != null)
        {
            if (tableSegment.IsSharding)
            {
                shardingTables = shardingType switch
                {
                    ShardingTableType.SingleTable => tableSegment.Body,
                    ShardingTableType.MultiTable => tableSegment.TableNames,
                    _ => tableSegment.Mapper.TableName,
                };
            }
            else
            {
                shardingType = ShardingTableType.SplitTables;
                shardingTables = this.SplitShardingParameters(tableSegment.TableShardingInfo, updateObjType, updateObjs, firstUpdateObj, this.ShardingValues);
            }
        }
        return (shardingType, shardingTables, updateObjs, bulkCount, firstSqlSetter, loopSqlSetter, this.ReaderFields);
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
                this.ReaderFields.Add(new SqlSegment
                {
                    FieldType = ReaderFieldType.Field,
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
            this.ReaderFields.Add(new SqlSegment
            {
                FieldType = ReaderFieldType.RawSql,
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
                    var sqlSegment = this.VisitAndDeferred(new SqlSegment { Expression = memberExpr });
                    this.WrapSql(sqlSegment, true);
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
                    var sqlSegment = this.VisitAndDeferred(new SqlSegment { Expression = newExpr.Arguments[i] });
                    this.WrapSql(sqlSegment, true);
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
                    var sqlSegment = this.VisitAndDeferred(new SqlSegment { Expression = memberAssignment.Expression });
                    this.WrapSql(sqlSegment, true);
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
                    this.ReaderFields.Add(new SqlSegment
                    {
                        FieldType = ReaderFieldType.Field,
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
                this.VisitAndDeferred(new SqlSegment { Expression = fieldsSelector });
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
    public virtual void SetBulkCopy(IEnumerable updateObjs)
    {
        this.ActionMode = ActionMode.BulkCopy;
        this.deferredSegments.Add(new CommandSegment
        {
            Type = "WithBulkCopy",
            Value = updateObjs
        });
    }
    public (ShardingTableType, object, IEnumerable, List<MemberMap>, List<Func<object, object>>) BuildSetBulkCopy()
    {
        var updateObjs = this.deferredSegments[0].Value as IEnumerable;
        object firstUpdateObj = null;
        foreach (var updateObj in updateObjs)
        {
            firstUpdateObj = updateObj;
            break;
        }
        var updateObjType = firstUpdateObj.GetType();
        var tableSegment = this.Tables[0];
        var entityMapper = tableSegment.Mapper;

        var shardingType = ShardingTableType.None;
        object shardingTables = tableSegment.Mapper.TableName;
        if (tableSegment.TableShardingInfo != null)
        {
            if (tableSegment.IsSharding)
            {
                shardingTables = shardingType switch
                {
                    ShardingTableType.SingleTable => tableSegment.Body,
                    ShardingTableType.MultiTable => tableSegment.TableNames,
                    _ => tableSegment.Mapper.TableName,
                };
            }
            else
            {
                shardingType = ShardingTableType.SplitTables;
                shardingTables = this.SplitShardingParameters(tableSegment.TableShardingInfo, updateObjType, updateObjs, firstUpdateObj, this.ShardingValues);
            }
        }
        (var memberMappers, var valueGetters) = this.GetRefMemberMappers(updateObjType, entityMapper, firstUpdateObj, true);
        return (shardingType, shardingTables, updateObjs, memberMappers, valueGetters);
    }
}