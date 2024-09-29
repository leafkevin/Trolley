using System.Collections.Generic;
using System.Text;

namespace Trolley.PostgreSql;

public class PostgreSqlDeleteVisitor : DeleteVisitor
{
    public PostgreSqlDeleteVisitor(DbContext dbContext, char tableAsStart = 'a')
        : base(dbContext, tableAsStart) { }

    public override string BuildTableShardingsSql()
    {
        var builder = new StringBuilder($"SELECT a.relname FROM pg_class a,pg_namespace b WHERE a.relnamespace=b.oid AND a.relkind='r' AND ");
        var schemaBuilders = new Dictionary<string, StringBuilder>();
        foreach (var tableSegment in this.ShardingTables)
        {
            if (tableSegment.ShardingType > ShardingTableType.MultiTable)
            {
                var tableSchema = tableSegment.TableSchema ?? this.DefaultTableSchema;
                if (!schemaBuilders.TryGetValue(tableSchema, out var tableBuilder))
                    schemaBuilders.Add(tableSchema, tableBuilder = new StringBuilder());

                if (tableBuilder.Length > 0) tableBuilder.Append(" OR ");
                tableBuilder.Append($"a.relname LIKE '{tableSegment.Mapper.TableName}%'");
            }
        }
        if (schemaBuilders.Count > 1)
            builder.Append('(');
        int index = 0;
        foreach (var schemaBuilder in schemaBuilders)
        {
            if (index > 0) builder.Append(" OR ");
            builder.Append($"b.nspname='{schemaBuilder.Key}' AND ({schemaBuilder.Value.ToString()})");
            index++;
        }
        if (schemaBuilders.Count > 1)
            builder.Append(')');
        return builder.ToString();
    }
}
