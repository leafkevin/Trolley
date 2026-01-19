using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Trolley.MySqlConnector;

public class MySqlUpdateVisitor : UpdateVisitor, IUpdateVisitor
{
    private MySqlProvider dialectProvider => this.OrmProvider as MySqlProvider;

    public MySqlUpdateVisitor(Type entityType, DbContext dbContext, char tableAsStart = 'a')
        : base(entityType, dbContext, tableAsStart) { }

    public override void UseTableSchema(bool isIncludeMany, string tableSchema)
    {
        var defaultSchemaName = this.dialectProvider.GetDefaultSchemaName(this.DbContext);
        if (tableSchema == defaultSchemaName) return;
        var tableSegment = isIncludeMany ? this.IncludeTables.Last() : this.Tables.Last();
        tableSegment.TableSchema = tableSchema;
    }
    public void WithBulkCopy(IEnumerable updateObjs, int? timeoutSeconds)
    {
        this.ActionMode = ActionMode.BulkCopy;
        this.deferredSegments.Add(new CommandSegment
        {
            Type = "WithBulkCopy",
            Value = (updateObjs, timeoutSeconds)
        });
    }
    public (ShardingTableType, object, IEnumerable, int?, List<MemberMap>, List<Func<object, object>>) BuildWithBulkCopy()
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
                    ShardingTableType.SingleTable => tableSegment.Body,
                    ShardingTableType.MultiTable => tableSegment.TableNames,
                    _ => tableSegment.Mapper.TableName,
                };
            }
            else
            {
                shardingType = ShardingTableType.SplitTables;
                shardingTables = this.SplitShardingParameters(tableSegment.TableShardingInfo, updateObjType, updateObjs, firstUpdateObj, this.shardingValues);
            }
        }
        (var memberMappers, var valueGetters) = this.GetRefMemberMappers(updateObjType, entityMapper, firstUpdateObj, true);
        return (shardingType, shardingTables, updateObjs, timeoutSeconds, memberMappers, valueGetters);
    }
}