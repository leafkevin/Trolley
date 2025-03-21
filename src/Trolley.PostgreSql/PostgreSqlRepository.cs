using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

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
            f.Query<ColumnInfo>($"select c.attname ColumnName,c.attndims ArrayDimens,case when c.atttypmod>0 and c.atttypmod<32767 then c.atttypmod-4 else c.attlen end Length,e.description,pg_get_expr(g.adbin,g.adrelid) DefaultValue,f.conname is not null,h.refobjid is not null,c.attnotnull IsNullable from pg_class a\r\n inner join pg_namespace b on a.relnamespace = b.oid inner join pg_attribute c on a.oid = c.attrelid and c.attnum>0\r\n inner join pg_type d on c.atttypid = d.oid\tleft join pg_description e on e.objoid = c.attrelid and e.objsubid = c.attnum\r\n left join pg_constraint f on a.oid=f.conrelid and f.contype='p' and f.conkey @> array[c.attnum]\r\n left join pg_attrdef g on a.oid=g.adrelid and c.attnum=g.adnum\r\n left join (select dp.refobjid,dp.refobjsubid from pg_depend dp,pg_class cs where dp.objid=cs.oid and cs.relkind='S') h on a.oid=h.refobjid and c.attnum=h.refobjsubid\r\n where a.relkind='r' and b.nspname='{{tableSchema}}' and a.relname='{{tableName}}' order by c.attnum asc")
             .Query<IndexInfo>($"SELECT NON_UNIQUE,INDEX_NAME,SEQ_IN_INDEX,COLUMN_NAME,COLLATION,INDEX_TYPE FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_NAME='{orgTableName}' AND TABLE_SCHEMA='{fromTableSchema}'");
        });
        var collationInfo = reader.ReadFirst<CollationInfo>();
        var columnInfos = reader.Read<ColumnInfo>();
        var indexInfos = reader.Read<IndexInfo>();

        var builder = new StringBuilder($"CREATE TABLE {this.OrmProvider.GetTableName(tableName)}");
        builder.AppendLine();
        builder.AppendLine("(");
        foreach (var columnInfo in columnInfos)
        {
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
            builder.AppendLine(",");
        }
        var indexNames = indexInfos.Select(f => f.IndexName).Distinct().ToList();
        for (int i = 0; i < indexNames.Count; i++)
        {
            if (i > 0) builder.AppendLine(",");
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
        builder.AppendLine($") ENGINE={collationInfo.Engine} CHARACTER SET={collationInfo.CharacterSetName} COLLATE={collationInfo.CollationName}");
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
            f.QueryFirst<CollationInfo>($"SELECT a.ENGINE,b.COLLATION_NAME,b.CHARACTER_SET_NAME FROM INFORMATION_SCHEMA.`TABLES` a,INFORMATION_SCHEMA.`COLLATION_CHARACTER_SET_APPLICABILITY` b WHERE a.TABLE_COLLATION=b.COLLATION_NAME AND a.TABLE_SCHEMA='{fromTableSchema}' AND a.TABLE_NAME='{orgTableName}' ")
             .Query<ColumnInfo>($"SELECT COLUMN_NAME,COLUMN_TYPE,COLUMN_COMMENT Description,COLUMN_DEFAULT DefaultValue,EXTRA IsIdentity,IS_NULLABLE FROM INFORMATION_SCHEMA.`COLUMNS` WHERE TABLE_SCHEMA='{fromTableSchema}' AND TABLE_NAME='{orgTableName}' ORDER BY ORDINAL_POSITION")
             .Query<IndexInfo>($"SELECT NON_UNIQUE,INDEX_NAME,SEQ_IN_INDEX,COLUMN_NAME,COLLATION,INDEX_TYPE FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_NAME='{orgTableName}' AND TABLE_SCHEMA='{fromTableSchema}'");
        }, cancellationToken);
        var collationInfo = await reader.ReadFirstAsync<CollationInfo>(cancellationToken);
        var columnInfos = await reader.ReadAsync<ColumnInfo>(cancellationToken);
        var indexInfos = await reader.ReadAsync<IndexInfo>(cancellationToken);

        var builder = new StringBuilder($"CREATE TABLE {this.OrmProvider.GetTableName(tableName)}");
        builder.AppendLine();
        builder.AppendLine("(");
        foreach (var columnInfo in columnInfos)
        {
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
            builder.AppendLine(",");
        }
        var indexNames = indexInfos.Select(f => f.IndexName).Distinct().ToList();
        for (int i = 0; i < indexNames.Count; i++)
        {
            if (i > 0) builder.AppendLine(",");
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
        builder.AppendLine($") ENGINE={collationInfo.Engine} CHARACTER SET={collationInfo.CharacterSetName} COLLATE={collationInfo.CollationName}");
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

    class CollationInfo
    {
        public string Engine { get; set; }
        public string CollationName { get; set; }
        public string CharacterSetName { get; set; }
    }
    class ColumnInfo
    {
        public string ColumnName { get; set; }
        public int ArrayDimens { get; set; }
        public string ColumnType { get; set; }
        public int Length { get; set; }
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