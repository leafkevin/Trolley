using MySqlConnector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
    public (ShardingTableType, object, Type, EntityMap, IEnumerable, int?) BuildWithBulkCopy()
    {
        (var updateObjs, int? timeoutSeconds) = ((IEnumerable, int?))this.deferredSegments[0].Value;
        object firstUpdateObj = null;
        Type updateObjType = null;
        foreach (var insertObj in updateObjs)
        {
            firstUpdateObj = insertObj;
            updateObjType = insertObj.GetType();
            break;
        }
        var tableSegment = this.Tables[0];
        var entityType = tableSegment.EntityType;
        var entityMapper = tableSegment.Mapper;
        this.FieldsBuilder.Append('(');

        var shardingType = ShardingTableType.None;
        object shardingTables = entityMapper.TableName;
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
                shardingTables = this.SplitShardingParameters(updateObjType, updateObjs, this.ShardingValues);
            }
        }
        (var memberMappers, var valueGetters) = this.GetRefMemberMappers(updateObjType, entityMapper, true); 
        return (shardingType, shardingTables, updateObjType, tableSegment.Mapper, updateObjs, timeoutSeconds);
    }
}