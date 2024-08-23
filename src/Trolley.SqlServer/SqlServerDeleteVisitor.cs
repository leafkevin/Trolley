using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Trolley.SqlServer;

public class SqlServerDeleteVisitor : DeleteVisitor
{
    public SqlServerDeleteVisitor(DbContext dbContext, char tableAsStart = 'a')
        : base(dbContext, tableAsStart) { }

    public override string BuildTableShardingsSql()
    {
        var builder = new StringBuilder($"SELECT a.name FROM sys.objects a,sys.schemas b WHERE a.schema_id=b.schema_id AND A.type='U' AND ");
        var schemaBuilders = new Dictionary<string, StringBuilder>();
        foreach (var tableSegment in this.ShardingTables)
        {
            if (tableSegment.ShardingType > ShardingTableType.MultiTable)
            {
                var tableSchema = tableSegment.TableSchema ?? this.DefaultTableSchema;
                if (!schemaBuilders.TryGetValue(tableSchema, out var tableBuilder))
                    schemaBuilders.TryAdd(tableSchema, tableBuilder = new StringBuilder());

                if (tableBuilder.Length > 0) tableBuilder.Append(" OR ");
                tableBuilder.Append($"a.name LIKE '{tableSegment.Mapper.TableName}%'");
            }
        }
        if (schemaBuilders.Count > 1)
            builder.Append('(');
        int index = 0;
        foreach (var schemaBuilder in schemaBuilders)
        {
            if (index > 0) builder.Append(" OR ");
            builder.Append($"b.name='{schemaBuilder.Key}' AND ({schemaBuilder.Value.ToString()})");
            index++;
        }
        if (schemaBuilders.Count > 1)
            builder.Append(')');
        return builder.ToString();
    }
}
