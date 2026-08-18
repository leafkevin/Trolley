using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Trolley.MySqlConnector;

public class MySqlUpdateVisitor : UpdateVisitor, IUpdateVisitor
{
    private MySqlProvider dialectProvider => this.OrmProvider as MySqlProvider;

    public MySqlUpdateVisitor(Type entityType, DbContext dbContext, char tableAsStart = 'a', ITheaCommand command = null)
        : base(entityType, dbContext, tableAsStart, command) { }

    public override void UseTableSchema(bool isIncludeMany, string tableSchema)
    {
        var defaultSchemaName = this.DbContext.DefaultTableSchema;
        if (tableSchema == defaultSchemaName) return;
        var tableSegment = isIncludeMany ? this.IncludeTables.Last() : this.Tables.Last();
        tableSegment.TableSchema = tableSchema;
    }
    public override (ShardingTableType, object, IEnumerable, int, Action<IDataParameterCollection>,
        Action<IDataParameterCollection, StringBuilder, DbContext, string, object, string>, List<SqlSegment>) BuildSetBulk(ITheaCommand command)
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
                    case "SetExpr":
                        this.VisitSetExpr(deferredSegment.Value as Expression);
                        break;
                    case "SetFrom":
                        this.VisitSetExpr(deferredSegment.Value as Expression);
                        break;
                    case "SetField":
                        this.VisitSetField(deferredSegment.Value);
                        break;
                    case "SetFieldExpr":
                        this.VisitSetFieldExpr(deferredSegment.Value);
                        break;
                    case "SetObject":
                        this.VisitSetObject(deferredSegment.Value);
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
                    default: throw new NotSupportedException("SetBulk操作后，只支持Set/IgnoreFields/OnlyFields/Where/And/Or操作");
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
                as Action<IDataParameterCollection, StringBuilder, DbContext, string, string, object, string>;
            loopSqlSetter = (dbParameters, builder, dbContext, tableName, updateObj, index) =>
            {
                builder.Append($"{headSql}{this.OrmProvider.GetTableName(tableName)} ");
                commandInitializer.Invoke(dbParameters, builder, dbContext, fixedHeadSql, fixedTailSql, updateObj, index.ToString());
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
                    ShardingTableType.SingleTable => tableSegment.Value,
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
        return (shardingType, shardingTables, updateObjs, bulkCount, firstSqlSetter, loopSqlSetter, null);
    }
    public (ShardingTableType, object, IEnumerable, int?, List<MemberMap>, List<Func<object, object>>) BuildSetBulkCopy()
    {
        (var updateObjs, int? timeoutSeconds) = ((IEnumerable, int?))this.deferredSegments[0].Value;
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
                    ShardingTableType.SingleTable => tableSegment.Value,
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
        return (shardingType, shardingTables, updateObjs, timeoutSeconds, memberMappers, valueGetters);
    }
    public virtual void SetBulkCopy(IEnumerable updateObjs, int? timeoutSeconds)
    {
        this.ActionMode = ActionMode.BulkCopy;
        this.deferredSegments.Add(new CommandSegment
        {
            Type = "WithBulkCopy",
            Value = (updateObjs, timeoutSeconds)
        });
    }
}