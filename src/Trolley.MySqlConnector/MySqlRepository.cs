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
    public override List<string> GetShardingTableNames<TEntity>(Func<string, bool> tableNameSelector, string tableSchema = null)
    {
        var entityMapper = this.MapProvider.GetEntityMap(typeof(TEntity));
        var orgTableName = entityMapper.TableName;
        tableSchema ??= this.DbContext.DefaultTableSchema;
        var sql = $"SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE' AND TABLE_NAME LIKE '{orgTableName}_%' AND TABLE_SCHEMA='{tableSchema}'";
        return this.Query<string>(sql);
    }
    public override async Task<List<string>> GetShardingTableNamesAsync<TEntity>(Func<string, bool> tableNameSelector, string tableSchema = null, CancellationToken cancellationToken = default)
    {
        var entityMapper = this.MapProvider.GetEntityMap(typeof(TEntity));
        var orgTableName = entityMapper.TableName;
        tableSchema ??= this.DbContext.DefaultTableSchema;
        var sql = $"SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE' AND TABLE_NAME LIKE '{orgTableName}_%' AND TABLE_SCHEMA='{tableSchema}'";
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
            f.QueryFirst<CollationInfo>($"select a.engine,b.collation_name,b.character_set_name from information_schema.tables a,information_schema.collation_character_set_applicability b where a.table_collation=b.collation_name and a.table_schema='{fromTableSchema}' and a.table_name='{orgTableName}' ")
             .Query<ColumnInfo>($"select column_name,column_type,column_comment description,column_default default_value,extra,is_nullable from information_schema.columns where table_schema='{fromTableSchema}' and table_name='{orgTableName}' order by ordinal_position")
             .Query<IndexInfo>($"select non_unique,index_name,seq_in_index,column_name,collation,index_type from information_schema.statistics where table_schema='{fromTableSchema}' and table_name='{orgTableName}'")
             .Query<ForeignKeyInfo>(@$"select a.constraint_name,a.column_name,a.referenced_table_name ref_table,a.referenced_column_name ref_column_name,c.update_rule,c.delete_rule from information_schema.key_column_usage a inner join information_schema.table_constraints b on a.constraint_name=
b.constraint_name and a.table_schema=b.table_schema and a.table_name=b.table_name inner join information_schema.referential_constraints c on a.table_schema=c.constraint_schema and a.constraint_name=c.constraint_name where b.constraint_type='FOREIGN KEY' and a.table_schema='{fromTableSchema}' and a.table_name='{orgTableName}'");
        });
        var collationInfo = reader.ReadFirst<CollationInfo>();
        var columnInfos = reader.Read<ColumnInfo>();
        var indexInfos = reader.Read<IndexInfo>();
        var foreignKeyInfos = reader.Read<ForeignKeyInfo>();

        var builder = new StringBuilder($"CREATE TABLE {this.OrmProvider.GetTableName(tableName)}");
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
                builder.Append($" COMMENT {columnInfo.Description}");
        }
        var indexNames = indexInfos.Select(f => f.IndexName).Distinct().ToList();
        foreach (var indexName in indexNames)
        {
            builder.AppendLine(",");
            var indexInfo = indexInfos.First(f => f.IndexName == indexName);
            if (indexInfo.IndexName == "PRIMARY")
                builder.Append($"CONSTRAINT `pk_{tableName}` PRIMARY KEY");
            else
            {
                if (!indexInfo.NonUnique)
                    builder.Append("UNIQUE ");
                builder.Append("INDEX ");
                var myIndexName = indexName + shardingPart;
                builder.Append(this.OrmProvider.GetFieldName(myIndexName));
            }
            builder.Append('(');
            var myIndexInfos = indexInfos.Where(f => f.IndexName == indexName)
                .OrderBy(f => f.SeqInIndex).ToList();
            for (int j = 0; j < myIndexInfos.Count; j++)
            {
                if (j > 0) builder.Append(',');
                var myIndexInfo = myIndexInfos[j];
                builder.Append(this.OrmProvider.GetFieldName(myIndexInfo.ColumnName));
                var orderBy = myIndexInfo.Collation == "A" ? "ASC" : "DESC";
                builder.Append($" {orderBy}");
            }
            builder.Append($") USING {indexInfo.IndexType}");
        }
        foreach (var foreignKeyInfo in foreignKeyInfos)
        {
            builder.AppendLine(",");
            builder.Append($"FOREIGN KEY ({this.OrmProvider.GetFieldName(foreignKeyInfo.ColumnName)}) REFERENCES {this.OrmProvider.GetTableName(foreignKeyInfo.RefTable)}(");
            builder.Append($"{this.OrmProvider.GetFieldName(foreignKeyInfo.RefColumnName)}) ON DELETE {foreignKeyInfo.DeleteRule} ON UPDATE {foreignKeyInfo.UpdateRule}");
        }
        builder.AppendLine();
        builder.Append($") ENGINE={collationInfo.Engine} CHARACTER SET={collationInfo.CharacterSetName} COLLATE={collationInfo.CollationName}");
        if (!string.IsNullOrEmpty(collationInfo.TableComment))
        {
            builder.AppendLine(";");
            builder.Append($"ALTER TABLE {this.OrmProvider.GetTableName(tableName)} COMMENT '{collationInfo.TableComment}'");
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
            f.QueryFirst<CollationInfo>($"select a.engine,b.collation_name,b.character_set_name from information_schema.tables a,information_schema.collation_character_set_applicability b where a.table_collation=b.collation_name and a.table_schema='{fromTableSchema}' and a.table_name='{orgTableName}' ")
            .Query<ColumnInfo>($"select column_name,column_type,column_comment description,column_default default_value,extra,is_nullable from information_schema.columns where table_schema='{fromTableSchema}' and table_name='{orgTableName}' order by ordinal_position")
            .Query<IndexInfo>($"select non_unique,index_name,seq_in_index,column_name,collation,index_type from information_schema.statistics where table_schema='{fromTableSchema}' and table_name='{orgTableName}'")
            .Query<ForeignKeyInfo>(@$"select a.constraint_name,a.column_name,a.referenced_table_name ref_table,a.referenced_column_name ref_column_name,c.update_rule,c.delete_rule from information_schema.key_column_usage a inner join information_schema.table_constraints b on a.constraint_name=
b.constraint_name and a.table_schema=b.table_schema inner join information_schema.referential_constraints c on a.table_schema=c.constraint_schema and a.constraint_name=c.constraint_name where b.constraint_type='FOREIGN KEY' and a.table_schema='{fromTableSchema}' and a.table_name='{orgTableName}'");
        }, cancellationToken);
        var collationInfo = await reader.ReadFirstAsync<CollationInfo>(cancellationToken);
        var columnInfos = await reader.ReadAsync<ColumnInfo>(cancellationToken);
        var indexInfos = await reader.ReadAsync<IndexInfo>(cancellationToken);
        var foreignKeyInfos =await  reader.ReadAsync<ForeignKeyInfo>();

        var builder = new StringBuilder($"CREATE TABLE {this.OrmProvider.GetTableName(tableName)}");
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
                builder.Append($" COMMENT {columnInfo.Description}");
        }
        var indexNames = indexInfos.Select(f => f.IndexName).Distinct().ToList();
        foreach (var indexName in indexNames)
        {
            builder.AppendLine(",");
            var indexInfo = indexInfos.First(f => f.IndexName == indexName);
            if (indexInfo.IndexName == "PRIMARY")
                builder.Append($"CONSTRAINT `pk_{tableName}` PRIMARY KEY");
            else
            {
                if (!indexInfo.NonUnique)
                    builder.Append("UNIQUE ");
                builder.Append("INDEX ");
                var myIndexName = indexName + shardingPart;
                builder.Append(this.OrmProvider.GetFieldName(myIndexName));
            }
            builder.Append('(');
            var myIndexInfos = indexInfos.Where(f => f.IndexName == indexName)
                .OrderBy(f => f.SeqInIndex).ToList();
            for (int j = 0; j < myIndexInfos.Count; j++)
            {
                if (j > 0) builder.Append(',');
                var myIndexInfo = myIndexInfos[j];
                builder.Append(this.OrmProvider.GetFieldName(myIndexInfo.ColumnName));
                var orderBy = myIndexInfo.Collation == "A" ? "ASC" : "DESC";
                builder.Append($" {orderBy}");
            }
            builder.Append($") USING {indexInfo.IndexType}");
        }
        foreach (var foreignKeyInfo in foreignKeyInfos)
        {
            builder.AppendLine(",");
            builder.Append($"FOREIGN KEY ({this.OrmProvider.GetFieldName(foreignKeyInfo.ColumnName)}) REFERENCES {this.OrmProvider.GetTableName(foreignKeyInfo.RefTable)}(");
            builder.Append($"{this.OrmProvider.GetFieldName(foreignKeyInfo.RefColumnName)}) ON DELETE {foreignKeyInfo.DeleteRule} ON UPDATE {foreignKeyInfo.UpdateRule}");
        }
        builder.AppendLine();
        builder.Append($") ENGINE={collationInfo.Engine} CHARACTER SET={collationInfo.CharacterSetName} COLLATE={collationInfo.CollationName}");
        if (!string.IsNullOrEmpty(collationInfo.TableComment))
        {
            builder.AppendLine(";");
            builder.Append($"ALTER TABLE {this.OrmProvider.GetTableName(tableName)} COMMENT '{collationInfo.TableComment}'");
        }
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