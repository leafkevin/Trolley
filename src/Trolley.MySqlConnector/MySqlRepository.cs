using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley.MySqlConnector;

public class MySqlRepository : Repository, IMySqlRepository
{
    #region fields
    private MySqlProvider dialectProvider => this.OrmProvider as MySqlProvider;
    #endregion

    #region Constructor
    public MySqlRepository(DbContext dbContext) :
        base(dbContext)
    { }
    #endregion

    #region Create
    public new IMySqlCreate<TEntity> Create<TEntity>()
        => this.OrmProvider.NewCreate<TEntity>(this.DbContext) as IMySqlCreate<TEntity>;
    #endregion

    #region Update
    public new IMySqlUpdate<TEntity> Update<TEntity>()
        => this.OrmProvider.NewUpdate<TEntity>(this.DbContext) as IMySqlUpdate<TEntity>;
    #endregion

    #region Delete
    public new IMySqlDelete<TEntity> Delete<TEntity>()
        => this.OrmProvider.NewDelete<TEntity>(this.DbContext) as IMySqlDelete<TEntity>;
    #endregion

    #region ShardingTable
    public override List<string> GetShardingTableNames<TEntity>(Func<string, bool> tableNameSelector = null, string tableSchema = null)
        => this.dialectProvider.GetShardingTableNames<TEntity>(this.DbContext, tableNameSelector, tableSchema);
    public override async Task<List<string>> GetShardingTableNamesAsync<TEntity>(Func<string, bool> tableNameSelector = null, string tableSchema = null, CancellationToken cancellationToken = default)
        => await this.dialectProvider.GetShardingTableNamesAsync<TEntity>(this.DbContext, tableNameSelector, tableSchema, cancellationToken);
    public override void CreateShardingTable<TEntity>(string tableName, string tableSchema = null, string fromTableSchema = null)
    {
        var entityType = typeof(TEntity);
        if (!this.MapProvider.TryGetEntityMap(entityType, out var entityMapper))
            throw new Exception($"未找到{entityType.FullName}实体映射");

        fromTableSchema ??= this.DbContext.DefaultTableSchema;
        var orgTableName = entityMapper.TableName;
        var shardingPart = tableName.Substring(orgTableName.Length);
        using var reader = this.QueryMultiple(f =>
        {
            f.QueryFirst<CollationInfo>($"select a.engine,b.collation_name,b.character_set_name from information_schema.tables a,information_schema.collation_character_set_applicability b where a.table_collation=b.collation_name and a.table_schema='{fromTableSchema}' and a.table_name='{orgTableName}'")
             .Query<ColumnInfo>($"select column_name,column_type,column_comment description,column_default default_value,extra,is_nullable from information_schema.columns where table_schema='{fromTableSchema}' and table_name='{orgTableName}' order by ordinal_position")
             .Query<IndexInfo>(@$"select a.non_unique,a.index_name,a.seq_in_index,a.column_name,a.collation,a.index_type,b.constraint_type from information_schema.statistics a left join information_schema.table_constraints b 
on a.table_schema=b.table_schema and a.table_name=b.table_name and a.index_name=b.constraint_name where IFNULL(b.constraint_type,'')<>'FOREIGN KEY' and a.table_schema='{fromTableSchema}' and a.table_name='{orgTableName}'")
             .Query<ForeignKeyInfo>($@"select a.constraint_name,a.column_name,a.referenced_table_name ref_table,a.referenced_column_name ref_column_name,b.update_rule update_rule,b.delete_rule delete_rule from information_schema.key_column_usage a inner join 
information_schema.referential_constraints b on a.table_schema=b.constraint_schema and a.table_name=b.table_name and a.constraint_name=b.constraint_name and b.referenced_table_name IS NOT NULL where a.table_schema='{fromTableSchema}' and a.table_name='{orgTableName}'");
        });
        var collationInfo = reader.ReadFirst<CollationInfo>();
        var columnInfos = reader.Read<ColumnInfo>();
        var indexInfos = reader.Read<IndexInfo>();
        var foreignKeyInfos = reader.Read<ForeignKeyInfo>();

        var builder = new StringBuilder($"CREATE TABLE IF NOT EXISTS {this.OrmProvider.GetTableName(tableName)}");
        builder.AppendLine();
        builder.AppendLine("(");
        for (int i = 0; i < columnInfos.Count; i++)
        {
            if (i > 0) builder.AppendLine(",");
            var columnInfo = columnInfos[i];
            builder.Append($"{this.OrmProvider.GetFieldName(columnInfo.ColumnName)} {columnInfo.ColumnType}");
            if (columnInfo.IsNullable == "NO")
                builder.Append(" NOT");
            builder.Append(" NULL");
            if (!string.IsNullOrEmpty(columnInfo.Extra) && columnInfo.Extra.ToLower().Contains("auto_increment"))
                builder.Append(" AUTO_INCREMENT");
            if (!string.IsNullOrEmpty(columnInfo.DefaultValue))
                builder.Append($" DEFAULT {columnInfo.DefaultValue}");
            if (!string.IsNullOrEmpty(columnInfo.Description))
                builder.Append($" COMMENT '{columnInfo.Description}'");
        }
        var indexNames = indexInfos.Select(f => f.IndexName).Distinct().ToList();
        foreach (var indexName in indexNames)
        {
            builder.AppendLine(",");
            var indexInfo = indexInfos.First(f => f.IndexName == indexName);
            if (indexInfo.ConstraintType == "PRIMARY KEY")
                builder.Append($"{indexInfo.ConstraintType}");
            else
            {
                if (!indexInfo.NonUnique)
                    builder.Append("UNIQUE ");
                var myIndexName = indexName + shardingPart;
                builder.Append($"INDEX {this.OrmProvider.GetFieldName(myIndexName)}");
            }
            builder.Append($" USING {indexInfo.IndexType}");
            builder.Append('(');
            var myIndexInfos = indexInfos.Where(f => f.IndexName == indexName)
                .OrderBy(f => f.SeqInIndex).ToList();
            for (int j = 0; j < myIndexInfos.Count; j++)
            {
                if (j > 0) builder.Append(',');
                var myIndexInfo = myIndexInfos[j];
                builder.Append(this.OrmProvider.GetFieldName(myIndexInfo.ColumnName));
                if (myIndexInfo.Collation == "D")
                    builder.Append($" DESC");
            }
            builder.Append(')');
        }
        indexNames = foreignKeyInfos.Select(f => f.ConstraintName).Distinct().ToList();
        foreach (var indexName in indexNames)
        {
            builder.AppendLine(",");
            var indexInfo = foreignKeyInfos.First(f => f.ConstraintName == indexName);
            builder.Append($"FOREIGN KEY ({this.OrmProvider.GetFieldName(indexInfo.ColumnName)}) REFERENCES {this.OrmProvider.GetTableName(indexInfo.RefTable)}");
            builder.Append($"({this.OrmProvider.GetFieldName(indexInfo.RefColumnName)}) ON DELETE {indexInfo.DeleteRule} ON UPDATE {indexInfo.UpdateRule}");
        }
        builder.AppendLine();
        builder.Append($") ENGINE={collationInfo.Engine} CHARACTER SET={collationInfo.CharacterSetName} COLLATE={collationInfo.CollationName}");
        if (!string.IsNullOrEmpty(collationInfo.TableComment))
            builder.Append($" COMMENT '{collationInfo.TableComment}'");
        this.Execute(builder.ToString());
    }
    public override async Task CreateShardingTableAsync<TEntity>(string tableName, string tableSchema = null, string fromTableSchema = null, CancellationToken cancellationToken = default)
    {
        var entityType = typeof(TEntity);
        if (!this.MapProvider.TryGetEntityMap(entityType, out var entityMapper))
            throw new Exception($"未找到{entityType.FullName}实体映射");

        fromTableSchema ??= this.DbContext.DefaultTableSchema;
        var orgTableName = entityMapper.TableName;
        var shardingPart = tableName.Substring(orgTableName.Length);
        using var reader = await this.QueryMultipleAsync(f =>
        {
            f.QueryFirst<CollationInfo>($"select a.engine,b.collation_name,b.character_set_name from information_schema.tables a,information_schema.collation_character_set_applicability b where a.table_collation=b.collation_name and a.table_schema='{fromTableSchema}' and a.table_name='{orgTableName}'")
             .Query<ColumnInfo>($"select column_name,column_type,column_comment description,column_default default_value,extra,is_nullable from information_schema.columns where table_schema='{fromTableSchema}' and table_name='{orgTableName}' order by ordinal_position")
             .Query<IndexInfo>(@$"select a.non_unique,a.index_name,a.seq_in_index,a.column_name,a.collation,a.index_type,b.constraint_type from information_schema.statistics a left join information_schema.table_constraints b 
on a.table_schema=b.table_schema and a.table_name=b.table_name and a.index_name=b.constraint_name where IFNULL(b.constraint_type,'')<>'FOREIGN KEY' and a.table_schema='{fromTableSchema}' and a.table_name='{orgTableName}'")
             .Query<ForeignKeyInfo>($@"select a.constraint_name,a.column_name,a.referenced_table_name ref_table,a.referenced_column_name ref_column_name,b.update_rule update_rule,b.delete_rule delete_rule from information_schema.key_column_usage a inner join 
information_schema.referential_constraints b on a.table_schema=b.constraint_schema and a.table_name=b.table_name and a.constraint_name=b.constraint_name and b.referenced_table_name IS NOT NULL where a.table_schema='{fromTableSchema}' and a.table_name='{orgTableName}'");
        }, cancellationToken);
        var collationInfo = await reader.ReadFirstAsync<CollationInfo>(cancellationToken);
        var columnInfos = await reader.ReadAsync<ColumnInfo>(cancellationToken);
        var indexInfos = await reader.ReadAsync<IndexInfo>(cancellationToken);
        var foreignKeyInfos = await reader.ReadAsync<ForeignKeyInfo>(cancellationToken);

        var builder = new StringBuilder($"CREATE TABLE IF NOT EXISTS {this.OrmProvider.GetTableName(tableName)}");
        builder.AppendLine();
        builder.AppendLine("(");
        for (int i = 0; i < columnInfos.Count; i++)
        {
            if (i > 0) builder.AppendLine(",");
            var columnInfo = columnInfos[i];
            builder.Append($"{this.OrmProvider.GetFieldName(columnInfo.ColumnName)} {columnInfo.ColumnType}");
            if (columnInfo.IsNullable == "NO")
                builder.Append(" NOT");
            builder.Append(" NULL");
            if (!string.IsNullOrEmpty(columnInfo.Extra) && columnInfo.Extra.ToLower().Contains("auto_increment"))
                builder.Append(" AUTO_INCREMENT");
            if (!string.IsNullOrEmpty(columnInfo.DefaultValue))
                builder.Append($" DEFAULT {columnInfo.DefaultValue}");
            if (!string.IsNullOrEmpty(columnInfo.Description))
                builder.Append($" COMMENT '{columnInfo.Description}'");
        }
        var indexNames = indexInfos.Select(f => f.IndexName).Distinct().ToList();
        foreach (var indexName in indexNames)
        {
            builder.AppendLine(",");
            var indexInfo = indexInfos.First(f => f.IndexName == indexName);
            if (indexInfo.ConstraintType == "PRIMARY KEY")
                builder.Append($"{indexInfo.ConstraintType}");
            else
            {
                if (!indexInfo.NonUnique)
                    builder.Append("UNIQUE ");
                var myIndexName = indexName + shardingPart;
                builder.Append($"INDEX {this.OrmProvider.GetFieldName(myIndexName)}");
            }
            builder.Append($" USING {indexInfo.IndexType}");
            builder.Append('(');
            var myIndexInfos = indexInfos.Where(f => f.IndexName == indexName)
                .OrderBy(f => f.SeqInIndex).ToList();
            for (int j = 0; j < myIndexInfos.Count; j++)
            {
                if (j > 0) builder.Append(',');
                var myIndexInfo = myIndexInfos[j];
                builder.Append(this.OrmProvider.GetFieldName(myIndexInfo.ColumnName));
                if (myIndexInfo.Collation == "D")
                    builder.Append($" DESC");
            }
            builder.Append(')');
        }
        indexNames = foreignKeyInfos.Select(f => f.ConstraintName).Distinct().ToList();
        foreach (var indexName in indexNames)
        {
            builder.AppendLine(",");
            var indexInfo = foreignKeyInfos.First(f => f.ConstraintName == indexName);
            builder.Append($"FOREIGN KEY ({this.OrmProvider.GetFieldName(indexInfo.ColumnName)}) REFERENCES {this.OrmProvider.GetTableName(indexInfo.RefTable)}");
            builder.Append($"({this.OrmProvider.GetFieldName(indexInfo.RefColumnName)}) ON DELETE {indexInfo.DeleteRule} ON UPDATE {indexInfo.UpdateRule}");
        }
        builder.AppendLine();
        builder.Append($") ENGINE={collationInfo.Engine} CHARACTER SET={collationInfo.CharacterSetName} COLLATE={collationInfo.CollationName}");
        if (!string.IsNullOrEmpty(collationInfo.TableComment))
            builder.Append($" COMMENT '{collationInfo.TableComment}'");
        await this.ExecuteAsync(builder.ToString(), null, CommandType.Text, cancellationToken);
    }
    #endregion

    class CollationInfo
    {
        public string TableComment { get; set; }
        public string Engine { get; set; }
        public string CollationName { get; set; }
        public string CharacterSetName { get; set; }
    }
    class ColumnInfo
    {
        public string ColumnName { get; set; }
        public string ColumnType { get; set; }
        public string Extra { get; set; }
        public string IsNullable { get; set; }
        public string Description { get; set; }
        public string DefaultValue { get; set; }
    }
    class IndexInfo
    {
        public bool NonUnique { get; set; }
        public string IndexName { get; set; }
        public int SeqInIndex { get; set; }
        public string ColumnName { get; set; }
        public string IndexType { get; set; }
        public string Collation { get; set; }
        public string ConstraintType { get; set; }
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