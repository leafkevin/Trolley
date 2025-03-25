using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley.PostgreSql;

public class PostgreSqlRepository : Repository, IPostgreSqlRepository
{
    #region Constructor
    public PostgreSqlRepository(DbContext dbContext) :
        base(dbContext)
    { }
    #endregion

    #region From
    public new IPostgreSqlQuery<T> From<T>(char tableAsStart = 'a')
        => base.From<T>(tableAsStart) as IPostgreSqlQuery<T>;
    public new IPostgreSqlQuery<T1, T2> From<T1, T2>(char tableAsStart = 'a')
        => base.From<T1, T2>(tableAsStart) as IPostgreSqlQuery<T1, T2>;
    public new IPostgreSqlQuery<T1, T2, T3> From<T1, T2, T3>(char tableAsStart = 'a')
        => base.From<T1, T2, T3>(tableAsStart) as IPostgreSqlQuery<T1, T2, T3>;
    public new IPostgreSqlQuery<T1, T2, T3, T4> From<T1, T2, T3, T4>(char tableAsStart = 'a')
        => base.From<T1, T2, T3, T4>(tableAsStart) as IPostgreSqlQuery<T1, T2, T3, T4>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5> From<T1, T2, T3, T4, T5>(char tableAsStart = 'a')
        => base.From<T1, T2, T3, T4, T5>(tableAsStart) as IPostgreSqlQuery<T1, T2, T3, T4, T5>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6> From<T1, T2, T3, T4, T5, T6>(char tableAsStart = 'a')
        => base.From<T1, T2, T3, T4, T5, T6>(tableAsStart) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7> From<T1, T2, T3, T4, T5, T6, T7>(char tableAsStart = 'a')
        => base.From<T1, T2, T3, T4, T5, T6, T7>(tableAsStart) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8> From<T1, T2, T3, T4, T5, T6, T7, T8>(char tableAsStart = 'a')
        => base.From<T1, T2, T3, T4, T5, T6, T7, T8>(tableAsStart) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> From<T1, T2, T3, T4, T5, T6, T7, T8, T9>(char tableAsStart = 'a')
        => base.From<T1, T2, T3, T4, T5, T6, T7, T8, T9>(tableAsStart) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9>;
    public new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> From<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(char tableAsStart = 'a')
        => base.From<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(tableAsStart) as IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>;
    #endregion

    #region From SubQuery
    public new IPostgreSqlQuery<T> From<T>(IQuery<T> subQuery)
        => base.From<T>(subQuery) as IPostgreSqlQuery<T>;
    public new IPostgreSqlQuery<T> From<T>(Func<IFromQuery, IQuery<T>> subQuery)
        => base.From<T>(subQuery) as IPostgreSqlQuery<T>;
    #endregion

    #region Create
    public new IPostgreSqlCreate<TEntity> Create<TEntity>()
        => this.OrmProvider.NewCreate<TEntity>(this.DbContext) as IPostgreSqlCreate<TEntity>;
    #endregion

    #region Update
    public new IPostgreSqlUpdate<TEntity> Update<TEntity>()
        => this.OrmProvider.NewUpdate<TEntity>(this.DbContext) as IPostgreSqlUpdate<TEntity>;
    #endregion

    #region Delete
    public new IPostgreSqlDelete<TEntity> Delete<TEntity>()
        => this.OrmProvider.NewDelete<TEntity>(this.DbContext) as IPostgreSqlDelete<TEntity>;
    #endregion

    #region ShardingTable
    public override List<string> GetShardingTableNames<TEntity>(Func<string, bool> tableNameSelector, string tableSchema = null)
    {
        var entityMapper = this.MapProvider.GetEntityMap(typeof(TEntity));
        var orgTableName = entityMapper.TableName;
        tableSchema ??= this.DbContext.DefaultTableSchema;
        var sql = $"SELECT a.relname FROM pg_class a,pg_namespace b WHERE a.relnamespace=b.oid AND a.relkind='r' AND a.relname LIKE '{orgTableName}_%' AND b.nspname='{tableSchema}'";
        return this.Query<string>(sql);
    }
    public override async Task<List<string>> GetShardingTableNamesAsync<TEntity>(Func<string, bool> tableNameSelector, string tableSchema = null, CancellationToken cancellationToken = default)
    {
        var entityMapper = this.MapProvider.GetEntityMap(typeof(TEntity));
        var orgTableName = entityMapper.TableName;
        tableSchema ??= this.DbContext.DefaultTableSchema;
        var sql = $"SELECT a.relname FROM pg_class a,pg_namespace b WHERE a.relnamespace=b.oid AND a.relkind='r' AND a.relname LIKE '{orgTableName}_%' AND b.nspname='{tableSchema}'";
        return await this.QueryAsync<string>(sql);
    }
    public override void CreateShardingTable<TEntity>(string tableName, string fromTableSchema = null)
    {
        var entityType = typeof(TEntity);
        if (!this.MapProvider.TryGetEntityMap(entityType, out var entityMapper))
            throw new Exception($"未找到{entityType.FullName}实体映射");

        fromTableSchema ??= this.DbContext.DefaultTableSchema;
        var orgTableName = entityMapper.TableName;
        var shardingPart = tableName.Substring(orgTableName.Length);
        using var reader = this.QueryMultiple(f =>
        {
            f.Query<TableInfo>($"select cast(obj_description(a.oid) as varchar) description,c.spcname tablespace from pg_class a inner join pg_namespace b on a.relnamespace=b.oid left join pg_tablespace c on a.reltablespace=c.oid where a.relkind='r' and b.nspname='{fromTableSchema}' and a.relname='{orgTableName}'")
             .Query<ColumnInfo>(@$"select c.attnum ColumnIndex,c.attname ColumnName,c.attndims ArrayDimens,concat_ws('',d.typname,SUBSTRING(format_type(c.atttypid,c.atttypmod) from '\(.*\)')) columnType,e.description,pg_get_expr(f.adbin,f.adrelid) DefaultValue,g.refobjid IsIdentity,c.attnotnull IsRequired 
from pg_class a inner join pg_namespace b on a.relnamespace=b.oid inner join pg_attribute c on a.oid=c.attrelid and c.attnum>0 inner join pg_type d on c.atttypid=d.oid left join pg_description e on e.objoid=c.attrelid and e.objsubid=c.attnum left join pg_attrdef f on a.oid=f.adrelid 
and c.attnum=f.adnum left join (select dp.refobjid,dp.refobjsubid from pg_depend dp,pg_class cs where dp.objid=cs.oid and cs.relkind='S') g on a.oid=g.refobjid and c.attnum=g.refobjsubid where a.relkind='r' and b.nspname='{fromTableSchema}' and a.relname='{orgTableName}' order by c.attnum asc")
             .Query<IndexInfo>($"select c.attname ColumnName,b.relname IndexName,a.indisunique IsUnique,a.indisprimary IsPrimary,not a.indisclustered IsClustered,pg_index_column_has_property(b.oid,c.attnum,'desc') IsDesc,d.amname IndexType from pg_index a inner join pg_class b " +
                $"on b.oid=a.indexrelid inner join pg_attribute c on c.attnum>0 and c.attrelid=b.oid inner join pg_am d ON b.relam=d.oid inner join pg_namespace e on e.oid=b.relnamespace inner join pg_class f on f.oid=a.indrelid WHERE f.relname='{orgTableName}' and e.nspname='{fromTableSchema}'");
        });
        var tableInfo = reader.ReadFirst<TableInfo>();
        var columnInfos = reader.Read<ColumnInfo>();
        var indexInfos = reader.Read<IndexInfo>();

        var builder = new StringBuilder($"CREATE TABLE IF NOT EXISTS {this.OrmProvider.GetTableName(tableName)}");
        builder.AppendLine();
        builder.AppendLine("(");
        var commentBuilder = new StringBuilder();
        if (tableInfo != null && !string.IsNullOrEmpty(tableInfo.Description))
            commentBuilder.AppendLine($"COMMENT ON TABLE {this.OrmProvider.GetTableName(tableName)} IS '{tableInfo.Description}';");
        for (int i = 0; i < columnInfos.Count; i++)
        {
            if (i > 0) builder.AppendLine(",");
            var columnInfo = columnInfos[i];
            builder.Append($"{this.OrmProvider.GetFieldName(columnInfo.ColumnName)} {columnInfo.ColumnType}");
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
                commentBuilder.AppendLine($"COMMENT ON COLUMN {tableName}.{this.OrmProvider.GetFieldName(columnInfo.ColumnName)} IS '{columnInfo.Description}';");
        }
        var myIndexInfos = indexInfos.FindAll(f => f.IsPrimary);
        if (myIndexInfos.Count > 0)
        {
            builder.AppendLine(",");
            builder.Append($"CONSTRAINT {this.OrmProvider.GetFieldName($"pk_{tableName}")} PRIMARY KEY(");
            for (int j = 0; j < myIndexInfos.Count; j++)
            {
                if (j > 0) builder.Append(',');
                var columnInfo = myIndexInfos[j];
                builder.Append(this.OrmProvider.GetFieldName(columnInfo.ColumnName));
                if (columnInfo.IsDesc)
                    builder.Append(" DESC");
            }
            builder.AppendLine(")");
        }
        builder.Append(')');
        if (tableInfo != null && !string.IsNullOrEmpty(tableInfo.TableSpace))
            builder.Append($" TABLESPACE {tableInfo.TableSpace}");

        if (indexInfos.Exists(f => !f.IsPrimary))
        {
            var indexNames = indexInfos.Where(f => !f.IsPrimary).Select(f => f.IndexName).Distinct().ToList();
            for (int i = 0; i < indexNames.Count; i++)
            {
                builder.AppendLine(";");
                builder.Append("CREATE ");
                var indexName = indexNames[i];
                var indexInfo = indexInfos.First(f => f.IndexName == indexName);
                if (indexInfo.IsUnique)
                    builder.Append("UNIQUE ");
                builder.Append("INDEX IF NOT EXISTS ");
                var myIndexName = indexInfo.IndexName + shardingPart;
                builder.Append(this.OrmProvider.GetFieldName(myIndexName));
                builder.Append($" ON {this.OrmProvider.GetTableName(tableName)}");
                if (!string.IsNullOrEmpty(indexInfo.IndexType))
                    builder.Append($" USING {indexInfo.IndexType}");
                builder.Append('(');
                myIndexInfos = indexInfos.FindAll(f => f.IndexName == indexName);
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

        fromTableSchema ??= this.DbContext.DefaultTableSchema;
        var orgTableName = entityMapper.TableName;
        var shardingPart = tableName.Substring(orgTableName.Length);
        using var reader = await this.QueryMultipleAsync(f =>
        {
            f.Query<TableInfo>($"select cast(obj_description(a.oid) as varchar) description,c.spcname tablespace from pg_class a inner join pg_namespace b on a.relnamespace=b.oid left join pg_tablespace c on a.reltablespace=c.oid where a.relkind='r' and b.nspname='{fromTableSchema}' and a.relname='{orgTableName}'")
             .Query<ColumnInfo>(@$"select c.attnum ColumnIndex,c.attname ColumnName,c.attndims ArrayDimens,concat_ws('',d.typname,SUBSTRING(format_type(c.atttypid,c.atttypmod) from '\(.*\)')) columnType,e.description,pg_get_expr(f.adbin,f.adrelid) DefaultValue,g.refobjid IsIdentity,c.attnotnull IsRequired 
from pg_class a inner join pg_namespace b on a.relnamespace=b.oid inner join pg_attribute c on a.oid=c.attrelid and c.attnum>0 inner join pg_type d on c.atttypid=d.oid left join pg_description e on e.objoid=c.attrelid and e.objsubid=c.attnum left join pg_attrdef f on a.oid=f.adrelid 
and c.attnum=f.adnum left join (select dp.refobjid,dp.refobjsubid from pg_depend dp,pg_class cs where dp.objid=cs.oid and cs.relkind='S') g on a.oid=g.refobjid and c.attnum=g.refobjsubid where a.relkind='r' and b.nspname='{fromTableSchema}' and a.relname='{orgTableName}' order by c.attnum asc")
             .Query<IndexInfo>($"select c.attname ColumnName,b.relname IndexName,a.indisunique IsUnique,a.indisprimary IsPrimary,not a.indisclustered IsClustered,pg_index_column_has_property(b.oid,c.attnum,'desc') IsDesc,d.amname IndexType from pg_index a inner join pg_class b " +
                $"on b.oid=a.indexrelid inner join pg_attribute c on c.attnum>0 and c.attrelid=b.oid inner join pg_am d ON b.relam=d.oid inner join pg_namespace e on e.oid=b.relnamespace inner join pg_class f on f.oid=a.indrelid WHERE f.relname='{orgTableName}' and e.nspname='{fromTableSchema}'")
             .Query<IndexInfo>(@$"SELECT c.conname AS constraint_name,d.attname column_name,e.relname ref_table,f.attname ref_column_name,CASE c.confdeltype WHEN 'a' THEN 'NO ACTION' WHEN 'r' THEN 'RESTRICT' WHEN 'c' THEN 'CASCADE' WHEN 'n' THEN 'SET NULL' WHEN 'd' THEN 'SET DEFAULT' END AS delete_action,
CASE c.confupdtype WHEN 'a' THEN 'NO ACTION' WHEN 'r' THEN 'RESTRICT' WHEN 'c' THEN 'CASCADE' WHEN 'n' THEN 'SET NULL' WHEN 'd' THEN 'SET DEFAULT' END AS update_action FROM pg_class a INNER JOIN pg_namespace b ON a.relnamespace=b.oid INNER JOIN pg_constraint c ON a.oid=c.conrelid and 
c.contype='f' INNER JOIN pg_attribute d ON d.attnum=ANY(c.conkey) AND d.attrelid=c.conrelid INNER JOIN pg_class e ON c.confrelid=e.oid INNER JOIN pg_attribute f ON f.attnum=ANY(c.confkey) AND f.attrelid=c.confrelid WHERE b.nspname='{fromTableSchema}' and a.relname='{orgTableName}'");
        });
        var tableInfo = await reader.ReadFirstAsync<TableInfo>();
        var columnInfos = await reader.ReadAsync<ColumnInfo>();
        var indexInfos = await reader.ReadAsync<IndexInfo>();

        var builder = new StringBuilder($"CREATE TABLE IF NOT EXISTS {this.OrmProvider.GetTableName(tableName)}");
        builder.AppendLine();
        builder.AppendLine("(");
        var commentBuilder = new StringBuilder();
        if (tableInfo != null && !string.IsNullOrEmpty(tableInfo.Description))
            commentBuilder.AppendLine($"COMMENT ON TABLE {this.OrmProvider.GetTableName(tableName)} IS '{tableInfo.Description}';");
        for (int i = 0; i < columnInfos.Count; i++)
        {
            if (i > 0) builder.AppendLine(",");
            var columnInfo = columnInfos[i];
            builder.Append($"{this.OrmProvider.GetFieldName(columnInfo.ColumnName)} {columnInfo.ColumnType}");
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
                commentBuilder.AppendLine($"COMMENT ON COLUMN {tableName}.{this.OrmProvider.GetFieldName(columnInfo.ColumnName)} IS '{columnInfo.Description}';");
        }
        var myIndexInfos = indexInfos.FindAll(f => f.IsPrimary);
        if (myIndexInfos.Count > 0)
        {
            builder.AppendLine(",");
            builder.Append($"CONSTRAINT {this.OrmProvider.GetFieldName($"pk_{tableName}")} PRIMARY KEY(");
            for (int j = 0; j < myIndexInfos.Count; j++)
            {
                if (j > 0) builder.Append(',');
                var columnInfo = myIndexInfos[j];
                builder.Append(this.OrmProvider.GetFieldName(columnInfo.ColumnName));
                if (columnInfo.IsDesc)
                    builder.Append(" DESC");
            }
            builder.AppendLine(")");
        }
        builder.Append(')');
        if (tableInfo != null && !string.IsNullOrEmpty(tableInfo.TableSpace))
            builder.Append($" TABLESPACE {tableInfo.TableSpace}");

        if (indexInfos.Exists(f => !f.IsPrimary))
        {
            var indexNames = indexInfos.Where(f => !f.IsPrimary).Select(f => f.IndexName).Distinct().ToList();
            for (int i = 0; i < indexNames.Count; i++)
            {
                builder.AppendLine(";");
                builder.Append("CREATE ");
                var indexName = indexNames[i];
                var indexInfo = indexInfos.First(f => f.IndexName == indexName);
                if (indexInfo.IsUnique)
                    builder.Append("UNIQUE ");
                builder.Append("INDEX IF NOT EXISTS ");
                var myIndexName = indexInfo.IndexName + shardingPart;
                builder.Append(this.OrmProvider.GetFieldName(myIndexName));
                builder.Append($" ON {this.OrmProvider.GetTableName(tableName)}");
                if (!string.IsNullOrEmpty(indexInfo.IndexType))
                    builder.Append($" USING {indexInfo.IndexType}");
                builder.Append('(');
                myIndexInfos = indexInfos.FindAll(f => f.IndexName == indexName);
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
        }
        if (commentBuilder.Length > 0)
        {
            builder.AppendLine(";");
            builder.AppendLine(commentBuilder.ToString());
        }
        await this.ExecuteAsync(builder.ToString(), cancellationToken);
    }
    public override string GetShardingTableNameBy<TEntity>(object field1Value, object field2Value = null)
    {
        var entityMapper = this.MapProvider.GetEntityMap(typeof(TEntity));
        return this.DbContext.GetShardingTableBy(entityMapper, field1Value, field2Value);
    }
    public override void CreateShardingTableBy<TEntity>(object field1Value, object field2Value = null)
    {
        var entityMapper = this.MapProvider.GetEntityMap(typeof(TEntity));
        var tableName = this.DbContext.GetShardingTableBy(entityMapper, field1Value, field2Value);
        this.CreateShardingTable<TEntity>(tableName);
    }
    public override async Task CreateShardingTableByAsync<TEntity>(object field1Value, object field2Value = null, CancellationToken cancellationToken = default)
    {
        var entityMapper = this.MapProvider.GetEntityMap(typeof(TEntity));
        var tableName = this.DbContext.GetShardingTableBy(entityMapper, field1Value, field2Value);
        await this.CreateShardingTableAsync<TEntity>(tableName, null, cancellationToken);
    }
    #endregion

    class TableInfo
    {
        public string Description { get; set; }
        public string TableSpace { get; set; }
    }
    class ColumnInfo
    {
        public int ColumnIndex { get; set; }
        public string ColumnName { get; set; }
        public int ArrayDimens { get; set; }
        public string ColumnType { get; set; }
        public int Length { get; set; }
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
        public bool IsClustered { get; set; }
        public bool IsDesc { get; set; }
    }
    class ForeignKeyInfo
    {
        public string ConstraintName { get; set; }
        public string ColumnName { get; set; }
        public string RefTable { get; set; }
        public string RefColumnName { get; set; }
        public string DeleteAction { get; set; }
        public string UpdateAction { get; set; }
    }
}