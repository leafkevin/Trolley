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
    public override List<string> GetShardingTableNames<TEntity>(Func<string, bool> tableNameSelector)
    {
        var entityMapper = this.MapProvider.GetEntityMap(typeof(TEntity));
        var orgTableName = entityMapper.TableName;
        var sql = $"SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE' AND TABLE_NAME LIKE '{orgTableName}_%'";
        return this.Query<string>(sql);
    }
    public override async Task<List<string>> GetShardingTableNamesAsync<TEntity>(Func<string, bool> tableNameSelector, CancellationToken cancellationToken = default)
    {
        var entityMapper = this.MapProvider.GetEntityMap(typeof(TEntity));
        var orgTableName = entityMapper.TableName;
        var sql = $"SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE' AND TABLE_NAME LIKE '{orgTableName}_%'";
        return await this.QueryAsync<string>(sql);
    }
    public override void CreateShardingTable<TEntity>(string tableName, string fromTableSchema = null)
    {
        var entityType = typeof(TEntity);
        if (!this.MapProvider.TryGetEntityMap(entityType, out var entityMapper))
            throw new Exception($"未找到{entityType.FullName}实体映射");

        var orgTableName = entityMapper.TableName;
        var builder = new StringBuilder();
        builder.Append($"CREATE TABLE {tableName} SELECT * FROM {orgTableName} WHERE 1=2;")
            .Append($"SELECT NON_UNIQUE,INDEX_NAME,SEQ_IN_INDEX,COLUMN_NAME,INDEX_TYPE FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_NAME='sts_game_ranking'");
        if (!string.IsNullOrEmpty(fromTableSchema))
            builder.Append($" AND TABLE_SCHEMA='{fromTableSchema}'");
        builder.Append(';');
        var indexInfos = this.Query<IndexInfo>(builder.ToString());
        var shardingPart = tableName.Substring(orgTableName.Length);
        var indexNames = indexInfos.Select(f => f.IndexName).Distinct().ToList();
        builder.Clear();

        foreach (var indexInfo in indexInfos)
        {
            var indexName = indexInfo.IndexName + shardingPart;
            builder.Append($"ALTER TABLE {tableName} ADD ");
            if (indexInfo.IndexName == "PRIMARY")
                builder.Append($"PRIMARY KEY ");
            else
            {
                if (!indexInfo.NonUnique)
                    builder.Append("UNIQUE ");
                builder.Append("INDEX ");
            }
            builder.Append($"{indexName} (");

            var columnNames = indexInfos.Where(f => f.IndexName == indexName)
                .OrderBy(f => f.SeqInIndex)
                .Select(f => f.ColumnName).ToList();
            for (int i = 0; i < columnNames.Count; i++)
            {
                if (i > 0) builder.Append(',');
                builder.Append(columnNames[i]);
            }
            if (indexInfo.IndexName != "PRIMARY")
                builder.Append($" USING {indexInfo.IndexType} ");
            builder.Append(");");
        }
        this.Execute(builder.ToString());
    }
    public override async Task CreateShardingTableAsync<TEntity>(string tableName, string fromTableSchema = null, CancellationToken cancellationToken = default)
    {
        var entityType = typeof(TEntity);
        if (!this.MapProvider.TryGetEntityMap(entityType, out var entityMapper))
            throw new Exception($"未找到{entityType.FullName}实体映射");

        var orgTableName = entityMapper.TableName;
        var builder = new StringBuilder();
        builder.Append($"CREATE TABLE {tableName} SELECT * FROM {orgTableName} WHERE 1=2;")
            .Append($"SELECT NON_UNIQUE,INDEX_NAME,SEQ_IN_INDEX,COLUMN_NAME,INDEX_TYPE FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_NAME='sts_game_ranking'");
        if (!string.IsNullOrEmpty(fromTableSchema))
            builder.Append($" AND TABLE_SCHEMA='{fromTableSchema}'");
        builder.Append(';');
        var indexInfos = await this.QueryAsync<IndexInfo>(builder.ToString(), cancellationToken);
        var shardingPart = tableName.Substring(orgTableName.Length);
        var indexNames = indexInfos.Select(f => f.IndexName).Distinct().ToList();
        builder.Clear();

        foreach (var indexInfo in indexInfos)
        {
            var indexName = indexInfo.IndexName + shardingPart;
            builder.Append($"ALTER TABLE {tableName} ADD ");
            if (indexInfo.IndexName == "PRIMARY")
                builder.Append($"PRIMARY KEY ");
            else
            {
                if (!indexInfo.NonUnique)
                    builder.Append("UNIQUE ");
                builder.Append("INDEX ");
            }
            builder.Append($"{indexName} (");

            var columnNames = indexInfos.Where(f => f.IndexName == indexName)
                .OrderBy(f => f.SeqInIndex)
                .Select(f => f.ColumnName).ToList();
            for (int i = 0; i < columnNames.Count; i++)
            {
                if (i > 0) builder.Append(',');
                builder.Append(columnNames[i]);
            }
            if (indexInfo.IndexName != "PRIMARY")
                builder.Append($" USING {indexInfo.IndexType} ");
            builder.Append(");");
        }
        await this.ExecuteAsync(builder.ToString());
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

    class IndexInfo
    {
        public bool NonUnique { get; set; }
        public string IndexName { get; set; }
        public int SeqInIndex { get; set; }
        public string ColumnName { get; set; }
        public string IndexType { get; set; }
    }
}