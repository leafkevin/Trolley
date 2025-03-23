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
            f.QueryFirst<CollationInfo>($"SELECT a.ENGINE,b.COLLATION_NAME,b.CHARACTER_SET_NAME FROM INFORMATION_SCHEMA.`TABLES` a,INFORMATION_SCHEMA.`COLLATION_CHARACTER_SET_APPLICABILITY` b WHERE a.TABLE_COLLATION=b.COLLATION_NAME AND a.TABLE_SCHEMA='{fromTableSchema}' AND a.TABLE_NAME='{orgTableName}' ")
             .Query<ColumnInfo>($"SELECT COLUMN_NAME,COLUMN_TYPE,COLUMN_COMMENT Description,COLUMN_DEFAULT DefaultValue,EXTRA IsIdentity,IS_NULLABLE FROM INFORMATION_SCHEMA.`COLUMNS` WHERE TABLE_SCHEMA='{fromTableSchema}' AND TABLE_NAME='{orgTableName}' ORDER BY ORDINAL_POSITION")
             .Query<IndexInfo>($"SELECT NON_UNIQUE,INDEX_NAME,SEQ_IN_INDEX,COLUMN_NAME,COLLATION,INDEX_TYPE FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_NAME='{orgTableName}' AND TABLE_SCHEMA='{fromTableSchema}'");
        });
        var collationInfo = reader.ReadFirst<CollationInfo>();
        var columnInfos = reader.Read<ColumnInfo>();
        var indexInfos = reader.Read<IndexInfo>();

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
            if (columnInfo.IsIdentity == "auto_increment")
                builder.Append(" AUTO_INCREMENT");
            if (!string.IsNullOrEmpty(columnInfo.DefaultValue))
                builder.Append($" DEFAULT {columnInfo.DefaultValue}");
            if (!string.IsNullOrEmpty(columnInfo.Description))
                builder.Append($" COMMENT {columnInfo.Description}");
        }
        var indexNames = indexInfos.Select(f => f.IndexName).Distinct().ToList();
        for (int i = 0; i < indexNames.Count; i++)
        {
            builder.AppendLine(",");
            var indexName = indexNames[i];
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
            f.QueryFirst<CollationInfo>($"SELECT a.TABLE_COMMENT,a.ENGINE,b.COLLATION_NAME,b.CHARACTER_SET_NAME FROM INFORMATION_SCHEMA.`TABLES` a,INFORMATION_SCHEMA.`COLLATION_CHARACTER_SET_APPLICABILITY` b WHERE a.TABLE_COLLATION=b.COLLATION_NAME AND a.TABLE_SCHEMA='{fromTableSchema}' AND a.TABLE_NAME='{orgTableName}' ")
             .Query<ColumnInfo>($"SELECT COLUMN_NAME,COLUMN_TYPE,COLUMN_COMMENT Description,COLUMN_DEFAULT DefaultValue,EXTRA IsIdentity,IS_NULLABLE FROM INFORMATION_SCHEMA.`COLUMNS` WHERE TABLE_SCHEMA='{fromTableSchema}' AND TABLE_NAME='{orgTableName}' ORDER BY ORDINAL_POSITION")
             .Query<IndexInfo>($"SELECT NON_UNIQUE,INDEX_NAME,SEQ_IN_INDEX,COLUMN_NAME,COLLATION,INDEX_TYPE FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_NAME='{orgTableName}' AND TABLE_SCHEMA='{fromTableSchema}'");
        }, cancellationToken);
        var collationInfo = await reader.ReadFirstAsync<CollationInfo>(cancellationToken);
        var columnInfos = await reader.ReadAsync<ColumnInfo>(cancellationToken);
        var indexInfos = await reader.ReadAsync<IndexInfo>(cancellationToken);

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
            if (columnInfo.IsIdentity == "auto_increment")
                builder.Append(" AUTO_INCREMENT");
            if (!string.IsNullOrEmpty(columnInfo.DefaultValue))
                builder.Append($" DEFAULT {columnInfo.DefaultValue}");
            if (!string.IsNullOrEmpty(columnInfo.Description))
                builder.Append($" COMMENT '{columnInfo.Description}'");
        }
        var indexNames = indexInfos.Select(f => f.IndexName).Distinct().ToList();
        for (int i = 0; i < indexNames.Count; i++)
        {
            builder.AppendLine(",");
            var indexName = indexNames[i];
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
        public string IsIdentity { get; set; }
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
}