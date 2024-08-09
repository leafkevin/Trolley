using System.Collections;
using System.Text;

namespace Trolley.MySqlConnector;

public class MySqlUpdateVisitor : UpdateVisitor, IUpdateVisitor
{
    public MySqlUpdateVisitor(string dbKey, IOrmProvider ormProvider, IEntityMapProvider mapProvider, IShardingProvider shardingProvider, bool isParameterized = false, char tableAsStart = 'a', string parameterPrefix = "p")
      : base(dbKey, ormProvider, mapProvider, shardingProvider, isParameterized, tableAsStart, parameterPrefix) { }
    public override string BuildShardingTablesSql(string tableSchema)
    {
        var count = this.ShardingTables.FindAll(f => f.ShardingType > ShardingTableType.MultiTable).Count;
        var builder = new StringBuilder($"SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE' AND TABLE_SCHEMA='{tableSchema}' AND ");
        if (count > 1)
        {
            builder.Append('(');
            int index = 0;
            foreach (var tableSegment in this.ShardingTables)
            {
                if (tableSegment.ShardingType > ShardingTableType.MultiTable)
                {
                    if (index > 0) builder.Append(" OR ");
                    builder.Append($"TABLE_NAME LIKE '{tableSegment.Mapper.TableName}%'");
                    index++;
                }
            }
            builder.Append(')');
        }
        else
        {
            if (this.ShardingTables.Count > 1)
            {
                var tableSegment = this.ShardingTables.Find(f => f.ShardingType > ShardingTableType.MultiTable);
                builder.Append($"TABLE_NAME LIKE '{tableSegment.Mapper.TableName}%'");
            }
            else builder.Append($"TABLE_NAME LIKE '{this.ShardingTables[0].Mapper.TableName}%'");
        }
        return builder.ToString();
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
    public (IEnumerable, int?) BuildWithBulkCopy() => ((IEnumerable, int?))this.deferredSegments[0].Value;
}
