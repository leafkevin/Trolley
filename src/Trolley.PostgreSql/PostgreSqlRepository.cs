using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley.PostgreSql;

public class PostgreSqlRepository : Repository
{
    #region Constructor
    public PostgreSqlRepository(DbContext dbContext) : base(dbContext) { }
    #endregion

    #region ShardingTable
    public override void CreateShardingTable<TEntity>(string tableName, string fromTableSchema = null)
    {
        var entityType = typeof(TEntity);
        if (!this.MapProvider.TryGetEntityMap(entityType, out var entityMapper))
            throw new Exception($"未找到{entityType.FullName}实体映射");

        var tableSchema = this.DbContext.DefaultTableSchema;
        fromTableSchema ??= tableSchema;
        var orgTableName = entityMapper.TableName;
        var shardingPart = tableName.Substring(orgTableName.Length);
        using var reader = this.QueryMultiple(f =>
        {
            f.Query<TableInfo>($"select cast(obj_description(a.oid) as varchar) description,c.spcname tablespace from pg_class a inner join pg_namespace b on a.relnamespace=b.oid left join pg_tablespace c on a.reltablespace=c.oid where a.relkind='r' and b.nspname='{fromTableSchema}' and a.relname='{orgTableName}'")
             .Query<ColumnInfo>(@$"select c.attname ColumnName,c.attndims ArrayDimens,concat_ws('',d.typname,SUBSTRING(format_type(c.atttypid,c.atttypmod) from '\(.*\)')) columnType,e.description,pg_get_expr(f.adbin,f.adrelid) DefaultValue,g.refobjid IsIdentity,c.attnotnull IsRequired 
from pg_class a inner join pg_namespace b on a.relnamespace=b.oid inner join pg_attribute c on a.oid=c.attrelid and c.attnum>0 inner join pg_type d on c.atttypid=d.oid left join pg_description e on e.objoid=c.attrelid and e.objsubid=c.attnum left join pg_attrdef f on a.oid=f.adrelid 
and c.attnum=f.adnum left join (select dp.refobjid,dp.refobjsubid from pg_depend dp,pg_class cs where dp.objid=cs.oid and cs.relkind='S') g on a.oid=g.refobjid and c.attnum=g.refobjsubid where a.relkind='r' and b.nspname='{fromTableSchema}' and a.relname='{orgTableName}' order by c.attnum asc")
             .Query<IndexInfo>(@$"select c.attname ColumnName,b.relname IndexName,a.indisunique IsUnique,a.indisprimary IsPrimary,pg_index_column_has_property(b.oid,c.attnum,'desc') IsDesc,d.amname IndexType from pg_index a inner join pg_class b on b.oid=a.indexrelid 
inner join pg_attribute c on c.attnum>0 and c.attrelid=b.oid inner join pg_am d ON b.relam=d.oid inner join pg_namespace e on e.oid=b.relnamespace inner join pg_class f on f.oid=a.indrelid WHERE f.relname='{orgTableName}' and e.nspname='{fromTableSchema}'")
             .Query<ForeignKeyInfo>($@"SELECT c.conname AS constraint_name,d.attname column_name,e.relname ref_table,f.attname ref_column_name,CASE c.confdeltype WHEN 'a' THEN 'NO ACTION' WHEN 'r' THEN 'RESTRICT' WHEN 'c' THEN 'CASCADE' WHEN 'n' THEN 'SET NULL' WHEN 'd' THEN 'SET DEFAULT' END 
AS delete_rule,CASE c.confupdtype WHEN 'a' THEN 'NO ACTION' WHEN 'r' THEN 'RESTRICT' WHEN 'c' THEN 'CASCADE' WHEN 'n' THEN 'SET NULL' WHEN 'd' THEN 'SET DEFAULT' END AS update_rule FROM pg_class a INNER JOIN pg_namespace b ON a.relnamespace=b.oid INNER JOIN pg_constraint c ON a.oid=c.conrelid 
and c.contype='f' INNER JOIN pg_attribute d ON d.attnum=ANY(c.conkey) AND d.attrelid=c.conrelid INNER JOIN pg_class e ON c.confrelid=e.oid INNER JOIN pg_attribute f ON f.attnum=ANY(c.confkey) AND f.attrelid=c.confrelid WHERE b.nspname='{fromTableSchema}' and a.relname='{orgTableName}'");
        });
        var tableInfo = reader.ReadFirst<TableInfo>();
        var columnInfos = reader.Read<ColumnInfo>();
        var indexInfos = reader.Read<IndexInfo>();
        var foreignKeyInfos = reader.Read<ForeignKeyInfo>();

        var builder = new StringBuilder($"CREATE TABLE IF NOT EXISTS {this.OrmProvider.GetFieldName(tableSchema)}.{this.OrmProvider.GetTableName(tableName)}");
        builder.AppendLine();
        builder.AppendLine("(");
        var commentBuilder = new StringBuilder();
        if (tableInfo != null && !string.IsNullOrEmpty(tableInfo.Description))
            commentBuilder.AppendLine($"COMMENT ON TABLE {this.OrmProvider.GetFieldName(tableSchema)}.{this.OrmProvider.GetTableName(tableName)} IS '{tableInfo.Description}';");
        for (int i = 0; i < columnInfos.Count; i++)
        {
            if (i > 0) builder.AppendLine(",");
            var columnInfo = columnInfos[i];
            var columnType = columnInfo.ColumnType;
            if (columnType.StartsWith("_") && columnInfo.ArrayDimens > 0)
            {
                columnType = columnType.Substring(1);
                for (int j = 0; j < columnInfo.ArrayDimens; j++)
                    columnType += "[]";
            }
            builder.Append($"{this.OrmProvider.GetFieldName(columnInfo.ColumnName)} {columnType}");
            if (columnInfo.IsRequired)
                builder.Append(" NOT");
            builder.Append(" NULL");
            if (!string.IsNullOrEmpty(columnInfo.IsIdentity))
            {
                if (!string.IsNullOrEmpty(columnInfo.DefaultValue) && columnInfo.DefaultValue.Contains("nextval"))
                {
                    var dataType = columnInfo.ColumnType switch
                    {
                        "int2" => "SMALLSERIAL",
                        "int8" => "BIGSERIAL",
                        _ => "SERIAL"
                    };
                    builder.Append($" {dataType}");
                }
                else builder.Append(" GENERATED BY DEFAULT AS IDENTITY");
            }
            if (!string.IsNullOrEmpty(columnInfo.DefaultValue) && columnInfo.DefaultValue.Contains("nextval"))
            {
                builder.Append(" DEFAULT ");
                if (columnInfo.DefaultValue.StartsWith("NULL"))
                    builder.Append("NULL");
                else builder.Append(columnInfo.DefaultValue);
            }
            if (!string.IsNullOrEmpty(columnInfo.Description))
                commentBuilder.AppendLine($"COMMENT ON COLUMN {this.OrmProvider.GetFieldName(tableSchema)}.{this.OrmProvider.GetTableName(tableName)}.{this.OrmProvider.GetFieldName(columnInfo.ColumnName)} IS '{columnInfo.Description}';");
        }
        var indexNames = indexInfos.Where(f => f.IsPrimary || f.IsUnique)
            .Select(f => f.IndexName).Distinct().ToList();
        foreach (var indexName in indexNames)
        {
            builder.AppendLine(",");
            var indexInfo = indexInfos.First(f => f.IndexName == indexName);

            if (indexInfo.IsPrimary)
                builder.Append($"CONSTRAINT {this.OrmProvider.GetFieldName($"pk_{tableName}")} PRIMARY KEY");
            else
            {
                var myIndexName = indexName + shardingPart;
                builder.Append($"CONSTRAINT {this.OrmProvider.GetFieldName(myIndexName)} UNIQUE");
            }
            builder.Append('(');
            var myIndexInfos = indexInfos.Where(f => f.IndexName == indexName).ToList();
            for (int j = 0; j < myIndexInfos.Count; j++)
            {
                if (j > 0) builder.Append(',');
                var myIndexInfo = myIndexInfos[j];
                builder.Append(this.OrmProvider.GetFieldName(myIndexInfo.ColumnName));
                if (myIndexInfo.IsDesc)
                    builder.Append($" DESC");
            }
            builder.Append(')');
        }

        indexNames = foreignKeyInfos.Select(f => f.ConstraintName).Distinct().ToList();
        foreach (var indexName in indexNames)
        {
            builder.AppendLine(",");
            var myIndexName = indexName + shardingPart;
            var myIndexInfo = foreignKeyInfos.Find(f => f.ConstraintName == indexName);
            builder.Append($"CONSTRAINT {this.OrmProvider.GetFieldName(myIndexName)} FOREIGN KEY({this.OrmProvider.GetFieldName(myIndexInfo.ColumnName)}) ");
            builder.Append($"REFERENCES {this.OrmProvider.GetTableName(myIndexInfo.RefTable)}({this.OrmProvider.GetFieldName(myIndexInfo.RefColumnName)}) ");
            builder.Append($"ON DELETE {myIndexInfo.DeleteRule} ON UPDATE {myIndexInfo.UpdateRule}");
        }
        builder.AppendLine();
        builder.Append(')');
        if (tableInfo != null && !string.IsNullOrEmpty(tableInfo.TableSpace))
            builder.Append($" TABLESPACE {tableInfo.TableSpace}");

        indexNames = indexInfos.Where(f => !f.IsPrimary && !f.IsUnique)
            .Select(f => f.IndexName).Distinct().ToList();
        foreach (var indexName in indexNames)
        {
            builder.AppendLine(";");
            var myIndexName = indexName + shardingPart;
            var indexInfo = indexInfos.First(f => f.IndexName == indexName);
            builder.Append($"CREATE INDEX IF NOT EXISTS {this.OrmProvider.GetFieldName(myIndexName)} ON {tableSchema}.{this.OrmProvider.GetTableName(tableName)} USING {indexInfo.IndexType}");

            var myIndexInfos = indexInfos.FindAll(f => f.IsPrimary);
            builder.Append('(');
            for (int j = 0; j < myIndexInfos.Count; j++)
            {
                if (j > 0) builder.Append(',');
                var columnInfo = myIndexInfos[j];
                builder.Append(this.OrmProvider.GetFieldName(columnInfo.ColumnName));
                if (columnInfo.IsDesc)
                    builder.Append(" DESC");
            }
            builder.Append(')');
        }
        if (commentBuilder.Length > 0)
        {
            builder.AppendLine(";");
            builder.AppendLine(commentBuilder.ToString());
        }
        this.Execute(builder.ToString());
    }
    public override async Task CreateShardingTableAsync<TEntity>(string tableName, string fromTableSchema = null, CancellationToken cancellationToken = default)
    {
        var entityType = typeof(TEntity);
        if (!this.MapProvider.TryGetEntityMap(entityType, out var entityMapper))
            throw new Exception($"未找到{entityType.FullName}实体映射");

        var tableSchema = this.DbContext.DefaultTableSchema;
        fromTableSchema ??= tableSchema;
        var orgTableName = entityMapper.TableName;
        var shardingPart = tableName.Substring(orgTableName.Length);
        using var reader = await this.QueryMultipleAsync(f =>
        {
            f.Query<TableInfo>($"select cast(obj_description(a.oid) as varchar) description,c.spcname tablespace from pg_class a inner join pg_namespace b on a.relnamespace=b.oid left join pg_tablespace c on a.reltablespace=c.oid where a.relkind='r' and b.nspname='{fromTableSchema}' and a.relname='{orgTableName}'")
             .Query<ColumnInfo>(@$"select c.attname ColumnName,c.attndims ArrayDimens,concat_ws('',d.typname,SUBSTRING(format_type(c.atttypid,c.atttypmod) from '\(.*\)')) columnType,e.description,pg_get_expr(f.adbin,f.adrelid) DefaultValue,g.refobjid IsIdentity,c.attnotnull IsRequired 
from pg_class a inner join pg_namespace b on a.relnamespace=b.oid inner join pg_attribute c on a.oid=c.attrelid and c.attnum>0 inner join pg_type d on c.atttypid=d.oid left join pg_description e on e.objoid=c.attrelid and e.objsubid=c.attnum left join pg_attrdef f on a.oid=f.adrelid 
and c.attnum=f.adnum left join (select dp.refobjid,dp.refobjsubid from pg_depend dp,pg_class cs where dp.objid=cs.oid and cs.relkind='S') g on a.oid=g.refobjid and c.attnum=g.refobjsubid where a.relkind='r' and b.nspname='{fromTableSchema}' and a.relname='{orgTableName}' order by c.attnum asc")
             .Query<IndexInfo>(@$"select c.attname ColumnName,b.relname IndexName,a.indisunique IsUnique,a.indisprimary IsPrimary,pg_index_column_has_property(b.oid,c.attnum,'desc') IsDesc,d.amname IndexType from pg_index a inner join pg_class b on b.oid=a.indexrelid 
inner join pg_attribute c on c.attnum>0 and c.attrelid=b.oid inner join pg_am d ON b.relam=d.oid inner join pg_namespace e on e.oid=b.relnamespace inner join pg_class f on f.oid=a.indrelid WHERE f.relname='{orgTableName}' and e.nspname='{fromTableSchema}'")
             .Query<ForeignKeyInfo>($@"SELECT c.conname AS constraint_name,d.attname column_name,e.relname ref_table,f.attname ref_column_name,CASE c.confdeltype WHEN 'a' THEN 'NO ACTION' WHEN 'r' THEN 'RESTRICT' WHEN 'c' THEN 'CASCADE' WHEN 'n' THEN 'SET NULL' WHEN 'd' THEN 'SET DEFAULT' END 
AS delete_rule,CASE c.confupdtype WHEN 'a' THEN 'NO ACTION' WHEN 'r' THEN 'RESTRICT' WHEN 'c' THEN 'CASCADE' WHEN 'n' THEN 'SET NULL' WHEN 'd' THEN 'SET DEFAULT' END AS update_rule FROM pg_class a INNER JOIN pg_namespace b ON a.relnamespace=b.oid INNER JOIN pg_constraint c ON a.oid=c.conrelid 
and c.contype='f' INNER JOIN pg_attribute d ON d.attnum=ANY(c.conkey) AND d.attrelid=c.conrelid INNER JOIN pg_class e ON c.confrelid=e.oid INNER JOIN pg_attribute f ON f.attnum=ANY(c.confkey) AND f.attrelid=c.confrelid WHERE b.nspname='{fromTableSchema}' and a.relname='{orgTableName}'");
        });
        var tableInfo = await reader.ReadFirstAsync<TableInfo>(cancellationToken);
        var columnInfos = await reader.ReadAsync<ColumnInfo>(cancellationToken);
        var indexInfos = await reader.ReadAsync<IndexInfo>(cancellationToken);
        var foreignKeyInfos = await reader.ReadAsync<ForeignKeyInfo>(cancellationToken);

        var builder = new StringBuilder($"CREATE TABLE IF NOT EXISTS {this.OrmProvider.GetFieldName(tableSchema)}.{this.OrmProvider.GetTableName(tableName)}");
        builder.AppendLine();
        builder.AppendLine("(");
        var commentBuilder = new StringBuilder();
        if (tableInfo != null && !string.IsNullOrEmpty(tableInfo.Description))
            commentBuilder.AppendLine($"COMMENT ON TABLE {this.OrmProvider.GetFieldName(tableSchema)}.{this.OrmProvider.GetTableName(tableName)} IS '{tableInfo.Description}';");
        for (int i = 0; i < columnInfos.Count; i++)
        {
            if (i > 0) builder.AppendLine(",");
            var columnInfo = columnInfos[i];
            var columnType = columnInfo.ColumnType;
            if (columnType.StartsWith("_") && columnInfo.ArrayDimens > 0)
            {
                columnType = columnType.Substring(1);
                for (int j = 0; j < columnInfo.ArrayDimens; j++)
                    columnType += "[]";
            }
            builder.Append($"{this.OrmProvider.GetFieldName(columnInfo.ColumnName)} {columnType}");
            if (columnInfo.IsRequired)
                builder.Append(" NOT");
            builder.Append(" NULL");
            if (!string.IsNullOrEmpty(columnInfo.IsIdentity))
            {
                if (!string.IsNullOrEmpty(columnInfo.DefaultValue) && columnInfo.DefaultValue.Contains("nextval"))
                {
                    var dataType = columnInfo.ColumnType switch
                    {
                        "int2" => "SMALLSERIAL",
                        "int8" => "BIGSERIAL",
                        _ => "SERIAL"
                    };
                    builder.Append($" {dataType}");
                }
                else builder.Append(" GENERATED BY DEFAULT AS IDENTITY");
            }
            if (!string.IsNullOrEmpty(columnInfo.DefaultValue) && columnInfo.DefaultValue.Contains("nextval"))
            {
                builder.Append(" DEFAULT ");
                if (columnInfo.DefaultValue.StartsWith("NULL"))
                    builder.Append("NULL");
                else builder.Append(columnInfo.DefaultValue);
            }
            if (!string.IsNullOrEmpty(columnInfo.Description))
                commentBuilder.AppendLine($"COMMENT ON COLUMN {this.OrmProvider.GetFieldName(tableSchema)}.{this.OrmProvider.GetTableName(tableName)}.{this.OrmProvider.GetFieldName(columnInfo.ColumnName)} IS '{columnInfo.Description}';");
        }
        var indexNames = indexInfos.Where(f => f.IsPrimary || f.IsUnique)
            .Select(f => f.IndexName).Distinct().ToList();
        foreach (var indexName in indexNames)
        {
            builder.AppendLine(",");
            var indexInfo = indexInfos.First(f => f.IndexName == indexName);

            if (indexInfo.IsPrimary)
                builder.Append($"CONSTRAINT {this.OrmProvider.GetFieldName($"pk_{tableName}")} PRIMARY KEY");
            else
            {
                var myIndexName = indexName + shardingPart;
                builder.Append($"CONSTRAINT {this.OrmProvider.GetFieldName(myIndexName)} UNIQUE");
            }
            builder.Append('(');
            var myIndexInfos = indexInfos.Where(f => f.IndexName == indexName).ToList();
            for (int j = 0; j < myIndexInfos.Count; j++)
            {
                if (j > 0) builder.Append(',');
                var myIndexInfo = myIndexInfos[j];
                builder.Append(this.OrmProvider.GetFieldName(myIndexInfo.ColumnName));
                if (myIndexInfo.IsDesc)
                    builder.Append($" DESC");
            }
            builder.Append(')');
        }

        indexNames = foreignKeyInfos.Select(f => f.ConstraintName).Distinct().ToList();
        foreach (var indexName in indexNames)
        {
            builder.AppendLine(",");
            var myIndexName = indexName + shardingPart;
            var myIndexInfo = foreignKeyInfos.Find(f => f.ConstraintName == indexName);
            builder.Append($"CONSTRAINT {this.OrmProvider.GetFieldName(myIndexName)} FOREIGN KEY({this.OrmProvider.GetFieldName(myIndexInfo.ColumnName)}) ");
            builder.Append($"REFERENCES {this.OrmProvider.GetTableName(myIndexInfo.RefTable)}({this.OrmProvider.GetFieldName(myIndexInfo.RefColumnName)}) ");
            builder.Append($"ON DELETE {myIndexInfo.DeleteRule} ON UPDATE {myIndexInfo.UpdateRule}");
        }
        builder.AppendLine();
        builder.Append(')');
        if (tableInfo != null && !string.IsNullOrEmpty(tableInfo.TableSpace))
            builder.Append($" TABLESPACE {tableInfo.TableSpace}");

        indexNames = indexInfos.Where(f => !f.IsPrimary && !f.IsUnique)
            .Select(f => f.IndexName).Distinct().ToList();
        foreach (var indexName in indexNames)
        {
            builder.AppendLine(";");
            var myIndexName = indexName + shardingPart;
            var indexInfo = indexInfos.First(f => f.IndexName == indexName);
            builder.Append($"CREATE INDEX IF NOT EXISTS {this.OrmProvider.GetFieldName(myIndexName)} ON {tableSchema}.{this.OrmProvider.GetTableName(tableName)} USING {indexInfo.IndexType}");

            var myIndexInfos = indexInfos.FindAll(f => f.IsPrimary);
            builder.Append('(');
            for (int j = 0; j < myIndexInfos.Count; j++)
            {
                if (j > 0) builder.Append(',');
                var columnInfo = myIndexInfos[j];
                builder.Append(this.OrmProvider.GetFieldName(columnInfo.ColumnName));
                if (columnInfo.IsDesc)
                    builder.Append(" DESC");
            }
            builder.Append(')');
        }
        if (commentBuilder.Length > 0)
        {
            builder.AppendLine(";");
            builder.AppendLine(commentBuilder.ToString());
        }
        await this.ExecuteAsync(builder.ToString(), cancellationToken);
    }
    #endregion

    class TableInfo
    {
        public string Description { get; set; }
        public string TableSpace { get; set; }
    }
    class ColumnInfo
    {
        public string ColumnName { get; set; }
        public int ArrayDimens { get; set; }
        public string ColumnType { get; set; }
        public string IsIdentity { get; set; }
        public bool IsRequired { get; set; }
        public string Description { get; set; }
        public string DefaultValue { get; set; }
    }
    class IndexInfo
    {
        public string IndexName { get; set; }
        public string ColumnName { get; set; }
        public string IndexType { get; set; }
        public bool IsUnique { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsDesc { get; set; }
    }
    class ForeignKeyInfo
    {
        public string ConstraintName { get; set; }
        public string ColumnName { get; set; }
        public string RefTable { get; set; }
        public string RefColumnName { get; set; }
        public string DeleteRule { get; set; }
        public string UpdateRule { get; set; }
    }
}