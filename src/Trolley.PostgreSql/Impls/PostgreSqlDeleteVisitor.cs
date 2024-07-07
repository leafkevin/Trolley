﻿using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Trolley.PostgreSql;

public class PostgreSqlDeleteVisitor : DeleteVisitor
{
    public PostgreSqlDeleteVisitor(string dbKey, IOrmProvider ormProvider, IEntityMapProvider mapProvider, IShardingProvider shardingProvider, bool isParameterized = false, char tableAsStart = 'a', string parameterPrefix = "p", List<IDbDataParameter> dbParameters = null)
      : base(dbKey, ormProvider, mapProvider, shardingProvider, isParameterized, tableAsStart, parameterPrefix, dbParameters) { }

    public override string BuildShardingTablesSql(string tableSchema)
    {
        var count = this.ShardingTables.FindAll(f => f.ShardingType > ShardingTableType.MultiTable).Count;
        var builder = new StringBuilder($"SELECT a.relname FROM pg_class a,pg_namespace b WHERE a.relnamespace=b.oid AND b.nspname='{tableSchema}' AND a.relkind='r' AND ");
        if (count > 1)
        {
            builder.Append('(');
            int index = 0;
            foreach (var tableSegment in this.ShardingTables)
            {
                if (tableSegment.ShardingType > ShardingTableType.MultiTable)
                {
                    if (index > 0) builder.Append(" OR ");
                    builder.Append($"a.relname LIKE '{tableSegment.Mapper.TableName}%'");
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
                builder.Append($"a.relname LIKE '{tableSegment.Mapper.TableName}%'");
            }
            else builder.Append($"a.relname LIKE '{this.ShardingTables[0].Mapper.TableName}%'");
        }
        return builder.ToString();
    }
}
