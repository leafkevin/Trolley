using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace Trolley.SqlServer;

public class SqlServerRepository : Repository, ISqlServerRepository
{
    #region fields
    private SqlServerProvider dialectProvider => this.OrmProvider as SqlServerProvider;
    #endregion

    #region Constructor
    public SqlServerRepository(DbContext dbContext) : base(dbContext) { }
    #endregion

    #region Create
    public new ISqlServerCreate<TEntity> Create<TEntity>()
        => this.OrmProvider.NewCreate<TEntity>(this.DbContext) as ISqlServerCreate<TEntity>;
    #endregion

    #region Update
    public new ISqlServerUpdate<TEntity> Update<TEntity>()
        => this.OrmProvider.NewUpdate<TEntity>(this.DbContext) as ISqlServerUpdate<TEntity>;
    #endregion

    #region Delete
    public new ISqlServerDelete<TEntity> Delete<TEntity>()
        => this.OrmProvider.NewDelete<TEntity>(this.DbContext) as ISqlServerDelete<TEntity>;
    #endregion

    #region ShardingTable
    public override List<string> GetShardingTableNames<TEntity>(Func<string, bool> tableNameSelector = null, string tableSchema = null)
        => this.dialectProvider.GetShardingTableNames<TEntity>(this.DbContext, tableNameSelector, tableSchema);
    public override async Task<List<string>> GetShardingTableNamesAsync<TEntity>(Func<string, bool> tableNameSelector = null, string tableSchema = null, CancellationToken cancellationToken = default)
        => await this.dialectProvider.GetShardingTableNamesAsync<TEntity>(this.DbContext, tableNameSelector, tableSchema, cancellationToken);
    public override void CreateShardingTable<TEntity>(string tableName, string tableSchema, string fromTableSchema = null)
    {
        var entityType = typeof(TEntity);
        if (!this.MapProvider.TryGetEntityMap(entityType, out var entityMapper))
            throw new Exception($"未找到{entityType.FullName}实体映射");

        tableSchema ??= this.DbContext.DefaultTableSchema;
        fromTableSchema ??= this.DbContext.DefaultTableSchema;
        var orgTableName = entityMapper.TableName;
        var shardingPart = tableName.Substring(orgTableName.Length);
        using var reader = this.QueryMultiple(f =>
        {
            f.Query<TableInfo>($"select c.value description from sys.sysobjects a INNER JOIN sys.schemas b ON a.schema_id=b.schema_id inner join sys.extended_properties cb on a.id=c.major_id and c.minor_id=0 and c.name='MS_Description' where a.xtype='U' and b.name='{fromTableSchema}' and a.name='{orgTableName}'")
             .Query<ColumnInfo>(@$"SELECT c.name AS column_name,ty.name AS data_type,c.max_length,c.precision,c.scale,c.is_nullable,c.is_identity,OBJECT_DEFINITION(c.default_object_id) AS default_value,ep.value AS description FROM sys.columns c INNER JOIN sys.tables t ON c.object_id=t.object_id 
INNER JOIN sys.schemas s ON t.schema_id=s.schema_id INNER JOIN sys.types ty ON c.user_type_id=ty.user_type_id LEFT JOIN sys.extended_properties ep ON c.object_id=ep.major_id AND c.column_id=ep.minor_id AND ep.name='MS_Description' WHERE s.name='{fromTableSchema}' AND t.name='{orgTableName}' ORDER BY c.column_id")
             .Query<IndexInfo>(@$"SELECT i.name AS index_name,i.type_desc AS index_type,i.is_unique,i.is_primary_key,c.name AS column_name,ic.is_descending_key AS is_desc FROM sys.tables t INNER JOIN sys.schemas s ON t.schema_id = s.schema_id INNER JOIN sys.indexes i ON 
t.object_id=i.object_id INNER JOIN sys.index_columns ic ON i.object_id=ic.object_id AND i.index_id=ic.index_id INNER JOIN sys.columns c ON ic.object_id=c.object_id AND ic.column_id=c.column_id WHERE s.name='{fromTableSchema}' AND t.name='{orgTableName}' ORDER BY i.name,ic.key_ordinal")
             .Query<ForeignKeyInfo>(@$"SELECT fk.name constraint_name,COL_NAME(fkc.parent_object_id,fkc.parent_column_id) column_name,SCHEMA_NAME(pt.schema_id)+'.'+OBJECT_NAME(fk.referenced_object_id) ref_table,COL_NAME(fkc.referenced_object_id,fkc.referenced_column_id) 
ref_column_name,fk.delete_referential_action_desc delete_rule,fk.update_referential_action_desc update_rule FROM sys.foreign_keys fk INNER JOIN sys.foreign_key_columns fkc ON fk.object_id=fkc.constraint_object_id INNER JOIN sys.tables pt ON 
fk.referenced_object_id=pt.object_id INNER JOIN sys.tables ft ON fk.parent_object_id=ft.object_id WHERE SCHEMA_NAME(ft.schema_id)='{fromTableSchema}' and OBJECT_NAME(fk.parent_object_id)='{orgTableName}' ORDER BY fk.name");
        });
        var tableInfo = reader.ReadFirst<TableInfo>();
        var columnInfos = reader.Read<ColumnInfo>();
        var indexInfos = reader.Read<IndexInfo>();
        var foreignKeyInfos = reader.Read<ForeignKeyInfo>();

        var builder = new StringBuilder();
        builder.AppendLine($"IF OBJECT_ID(N'{tableSchema}.{tableName}',N'U') IS NULL ");
        builder.AppendLine("BEGIN");
        builder.Append($"CREATE TABLE {this.OrmProvider.GetTableName(tableSchema)}.{this.OrmProvider.GetTableName(tableName)}");
        builder.AppendLine();
        builder.AppendLine("(");
        var commentBuilder = new StringBuilder();
        if (tableInfo != null && !string.IsNullOrEmpty(tableInfo.Description))
        {
            commentBuilder.AppendLine($"EXEC sys.sp_addextendedproperty @name=N'MS_Description',@value=N'{tableInfo.Description}',@level0type=N'SCHEMA',@level0name=N'{tableSchema}',@level1type=N'TABLE',@level1name=N'{tableName}'");
            commentBuilder.AppendLine("GO");
        }
        for (int i = 0; i < columnInfos.Count; i++)
        {
            if (i > 0) builder.AppendLine(",");
            var columnInfo = columnInfos[i];
            builder.Append($"{this.OrmProvider.GetFieldName(columnInfo.ColumnName)} {columnInfo.DataType}");

            string[] lenType = ["char", "varchar", "nchar", "nvarchar", "binary", "varbinary"];
            if (lenType.Contains(columnInfo.DataType))
            {
                string length = columnInfo.MaxLength.ToString();
                if (columnInfo.MaxLength == -1) length = "MAX";
                else
                {
                    length = columnInfo.DataType switch
                    {
                        "char" => $"{columnInfo.MaxLength}",
                        "varchar" => $"{columnInfo.MaxLength}",
                        "nchar" => $"{columnInfo.MaxLength / 2}",
                        "nvarchar" => $"{columnInfo.MaxLength / 2}",
                        "binary" => $"{columnInfo.MaxLength}",
                        "varbinary" => $"{columnInfo.MaxLength}",
                        _ => $"{columnInfo.MaxLength}"
                    };
                }
                builder.Append($"({length})");
            }
            switch (columnInfo.DataType.ToLower())
            {
                case "decimal":
                case "numeric":
                    builder.Append($"({columnInfo.Precision},{columnInfo.Scale})");
                    break;
            }
            if (!columnInfo.IsNullable)
                builder.Append(" NOT");
            builder.Append(" NULL");
            if (columnInfo.IsIdentity)
                builder.Append(" IDENTITY");
            if (!string.IsNullOrEmpty(columnInfo.DefaultValue)
                || !columnInfo.DefaultValue.StartsWith("(") && !columnInfo.DefaultValue.EndsWith(")"))
                builder.Append($" DEFAULT {columnInfo.DefaultValue}");
            if (!string.IsNullOrEmpty(columnInfo.Description))
            {
                commentBuilder.AppendLine($"EXEC sys.sp_addextendedproperty @name=N'MS_Description',@value=N'{columnInfo.Description}',@level0type=N'SCHEMA',@level0name=N'{tableSchema}',@level1type=N'TABLE',@level1name=N'{tableName}',@level2type=N'COLUMN',@level2name=N'{columnInfo.ColumnName}'");
                commentBuilder.AppendLine("GO");
            }
        }
        var myIndexInfos = indexInfos.FindAll(f => f.IsPrimary);
        if (myIndexInfos.Count > 0)
        {
            builder.AppendLine(",");
            builder.Append($"CONSTRAINT {this.OrmProvider.GetFieldName($"pk_{tableName}")} PRIMARY KEY {myIndexInfos[0].IndexType}(");
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

        if (indexInfos.Exists(f => !f.IsPrimary))
        {
            var indexNames = indexInfos.FindAll(f => !f.IsPrimary).Select(f => f.IndexName).Distinct().ToList();
            for (int i = 0; i < indexNames.Count; i++)
            {
                builder.AppendLine(",");
                var indexName = indexNames[i];
                var myIndexName = indexName + shardingPart;
                builder.Append($"CONSTRAINT {this.OrmProvider.GetFieldName(myIndexName)} ");
                var indexInfo = indexInfos.First(f => f.IndexName == indexName);
                if (indexInfo.IsUnique)
                    builder.Append("UNIQUE ");
                builder.Append($"{indexInfo.IndexType}(");
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
        if (foreignKeyInfos.Count > 0)
        {
            var indexNames = foreignKeyInfos.Select(f => f.ConstraintName).Distinct().ToList();
            for (int i = 0; i < foreignKeyInfos.Count; i++)
            {
                builder.AppendLine(",");
                var indexName = indexNames[i];
                var myIndexName = indexName + shardingPart;
                builder.Append($"CONSTRAINT {this.OrmProvider.GetFieldName(myIndexName)} FOREIGN KEY(");
                var myIndexInfo = foreignKeyInfos.First(f => f.ConstraintName == indexName);
                var myForeignKeyInfos = foreignKeyInfos.FindAll(f => f.ConstraintName == indexName);
                for (int j = 0; j < myForeignKeyInfos.Count; j++)
                {
                    if (j > 0) builder.Append(',');
                    var columnInfo = myForeignKeyInfos[j];
                    builder.Append(this.OrmProvider.GetFieldName(columnInfo.ColumnName));
                }
                builder.Append($") REFERENCES {this.OrmProvider.GetTableName(myIndexInfo.RefTable)}(");
                for (int j = 0; j < myForeignKeyInfos.Count; j++)
                {
                    if (j > 0) builder.Append(',');
                    var columnInfo = myForeignKeyInfos[j];
                    builder.Append(this.OrmProvider.GetFieldName(columnInfo.RefColumnName));
                }
                builder.Append($") ON DELETE {myIndexInfo.DeleteRule} ON UPDATE {myIndexInfo.UpdateRule}");
            }
        }
        builder.AppendLine(")");
        builder.AppendLine("GO");
        if (commentBuilder.Length > 0)
            builder.AppendLine(commentBuilder.ToString());
        builder.AppendLine("END");
        this.Execute(builder.ToString());
    }
    public override async Task CreateShardingTableAsync<TEntity>(string tableName, string tableSchema, string fromTableSchema = null, CancellationToken cancellationToken = default)
    {
        var entityType = typeof(TEntity);
        if (!this.MapProvider.TryGetEntityMap(entityType, out var entityMapper))
            throw new Exception($"未找到{entityType.FullName}实体映射");

        tableSchema ??= this.DbContext.DefaultTableSchema;
        fromTableSchema ??= this.DbContext.DefaultTableSchema;
        var orgTableName = entityMapper.TableName;
        var shardingPart = tableName.Substring(orgTableName.Length);
        using var reader = await this.QueryMultipleAsync(f =>
        {
            f.Query<TableInfo>($"select c.value description from sys.sysobjects a INNER JOIN sys.schemas b ON a.schema_id=b.schema_id inner join sys.extended_properties cb on a.id=c.major_id and c.minor_id=0 and c.name='MS_Description' where a.xtype='U' and b.name='{fromTableSchema}' and a.name='{orgTableName}'")
             .Query<ColumnInfo>(@$"SELECT c.name AS column_name,ty.name AS data_type,c.max_length,c.precision,c.scale,c.is_nullable,c.is_identity,OBJECT_DEFINITION(c.default_object_id) AS default_value,ep.value AS description FROM sys.columns c INNER JOIN sys.tables t ON c.object_id=t.object_id 
INNER JOIN sys.schemas s ON t.schema_id=s.schema_id INNER JOIN sys.types ty ON c.user_type_id=ty.user_type_id LEFT JOIN sys.extended_properties ep ON c.object_id=ep.major_id AND c.column_id=ep.minor_id AND ep.name='MS_Description' WHERE s.name='{fromTableSchema}' AND t.name='{orgTableName}' ORDER BY c.column_id")
             .Query<IndexInfo>(@$"SELECT i.name AS index_name,i.type_desc AS index_type,i.is_unique,i.is_primary_key,c.name AS column_name,ic.is_descending_key AS is_desc FROM sys.tables t INNER JOIN sys.schemas s ON t.schema_id = s.schema_id INNER JOIN sys.indexes i ON 
t.object_id=i.object_id INNER JOIN sys.index_columns ic ON i.object_id=ic.object_id AND i.index_id=ic.index_id INNER JOIN sys.columns c ON ic.object_id=c.object_id AND ic.column_id=c.column_id WHERE s.name='{fromTableSchema}' AND t.name='{orgTableName}' ORDER BY i.name,ic.key_ordinal")
             .Query<ForeignKeyInfo>(@$"SELECT fk.name constraint_name,COL_NAME(fkc.parent_object_id,fkc.parent_column_id) column_name,SCHEMA_NAME(pt.schema_id)+'.'+OBJECT_NAME(fk.referenced_object_id) ref_table,COL_NAME(fkc.referenced_object_id,fkc.referenced_column_id) 
ref_column_name,fk.delete_referential_action_desc delete_rule,fk.update_referential_action_desc update_rule FROM sys.foreign_keys fk INNER JOIN sys.foreign_key_columns fkc ON fk.object_id=fkc.constraint_object_id INNER JOIN sys.tables pt ON 
fk.referenced_object_id=pt.object_id INNER JOIN sys.tables ft ON fk.parent_object_id=ft.object_id WHERE SCHEMA_NAME(ft.schema_id)='{fromTableSchema}' and OBJECT_NAME(fk.parent_object_id)='{orgTableName}' ORDER BY fk.name");
        });
        var tableInfo = await reader.ReadFirstAsync<TableInfo>();
        var columnInfos = await reader.ReadAsync<ColumnInfo>();
        var indexInfos = await reader.ReadAsync<IndexInfo>();
        var foreignKeyInfos = await reader.ReadAsync<ForeignKeyInfo>();

        var builder = new StringBuilder();
        builder.AppendLine($"IF OBJECT_ID(N'{tableSchema}.{tableName}',N'U') IS NULL ");
        builder.AppendLine("BEGIN");
        builder.Append($"CREATE TABLE {this.OrmProvider.GetTableName(tableSchema)}.{this.OrmProvider.GetTableName(tableName)}");
        builder.AppendLine();
        builder.AppendLine("(");
        var commentBuilder = new StringBuilder();
        if (tableInfo != null && !string.IsNullOrEmpty(tableInfo.Description))
        {
            commentBuilder.AppendLine($"EXEC sys.sp_addextendedproperty @name=N'MS_Description',@value=N'{tableInfo.Description}',@level0type=N'SCHEMA',@level0name=N'{tableSchema}',@level1type=N'TABLE',@level1name=N'{tableName}'");
            commentBuilder.AppendLine("GO");
        }
        for (int i = 0; i < columnInfos.Count; i++)
        {
            if (i > 0) builder.AppendLine(",");
            var columnInfo = columnInfos[i];
            builder.Append($"{this.OrmProvider.GetFieldName(columnInfo.ColumnName)} {columnInfo.DataType}");

            string[] lenType = ["char", "varchar", "nchar", "nvarchar", "binary", "varbinary"];
            if (lenType.Contains(columnInfo.DataType))
            {
                string length = columnInfo.MaxLength.ToString();
                if (columnInfo.MaxLength == -1) length = "MAX";
                else
                {
                    length = columnInfo.DataType switch
                    {
                        "char" => $"{columnInfo.MaxLength}",
                        "varchar" => $"{columnInfo.MaxLength}",
                        "nchar" => $"{columnInfo.MaxLength / 2}",
                        "nvarchar" => $"{columnInfo.MaxLength / 2}",
                        "binary" => $"{columnInfo.MaxLength}",
                        "varbinary" => $"{columnInfo.MaxLength}",
                        _ => $"{columnInfo.MaxLength}"
                    };
                }
                builder.Append($"({length})");
            }
            switch (columnInfo.DataType.ToLower())
            {
                case "decimal":
                case "numeric":
                    builder.Append($"({columnInfo.Precision},{columnInfo.Scale})");
                    break;
            }
            if (!columnInfo.IsNullable)
                builder.Append(" NOT");
            builder.Append(" NULL");
            if (columnInfo.IsIdentity)
                builder.Append(" IDENTITY");
            if (!string.IsNullOrEmpty(columnInfo.DefaultValue)
                || !columnInfo.DefaultValue.StartsWith("(") && !columnInfo.DefaultValue.EndsWith(")"))
                builder.Append($" DEFAULT {columnInfo.DefaultValue}");
            if (!string.IsNullOrEmpty(columnInfo.Description))
            {
                commentBuilder.AppendLine($"EXEC sys.sp_addextendedproperty @name=N'MS_Description',@value=N'{columnInfo.Description}',@level0type=N'SCHEMA',@level0name=N'{tableSchema}',@level1type=N'TABLE',@level1name=N'{tableName}',@level2type=N'COLUMN',@level2name=N'{columnInfo.ColumnName}'");
                commentBuilder.AppendLine("GO");
            }
        }
        var myIndexInfos = indexInfos.FindAll(f => f.IsPrimary);
        if (myIndexInfos.Count > 0)
        {
            builder.AppendLine(",");
            builder.Append($"CONSTRAINT {this.OrmProvider.GetFieldName($"pk_{tableName}")} PRIMARY KEY {myIndexInfos[0].IndexType}(");
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

        if (indexInfos.Exists(f => !f.IsPrimary))
        {
            var indexNames = indexInfos.FindAll(f => !f.IsPrimary).Select(f => f.IndexName).Distinct().ToList();
            for (int i = 0; i < indexNames.Count; i++)
            {
                builder.AppendLine(",");
                var indexName = indexNames[i];
                var myIndexName = indexName + shardingPart;
                builder.Append($"CONSTRAINT {this.OrmProvider.GetFieldName(myIndexName)} ");
                var indexInfo = indexInfos.First(f => f.IndexName == indexName);
                if (indexInfo.IsUnique)
                    builder.Append("UNIQUE ");
                builder.Append($"{indexInfo.IndexType}(");
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
        if (foreignKeyInfos.Count > 0)
        {
            var indexNames = foreignKeyInfos.Select(f => f.ConstraintName).Distinct().ToList();
            for (int i = 0; i < foreignKeyInfos.Count; i++)
            {
                builder.AppendLine(",");
                var indexName = indexNames[i];
                var myIndexName = indexName + shardingPart;
                builder.Append($"CONSTRAINT {this.OrmProvider.GetFieldName(myIndexName)} FOREIGN KEY(");
                var myIndexInfo = foreignKeyInfos.First(f => f.ConstraintName == indexName);
                var myForeignKeyInfos = foreignKeyInfos.FindAll(f => f.ConstraintName == indexName);
                for (int j = 0; j < myForeignKeyInfos.Count; j++)
                {
                    if (j > 0) builder.Append(',');
                    var columnInfo = myForeignKeyInfos[j];
                    builder.Append(this.OrmProvider.GetFieldName(columnInfo.ColumnName));
                }
                builder.Append($") REFERENCES {this.OrmProvider.GetTableName(myIndexInfo.RefTable)}(");
                for (int j = 0; j < myForeignKeyInfos.Count; j++)
                {
                    if (j > 0) builder.Append(',');
                    var columnInfo = myForeignKeyInfos[j];
                    builder.Append(this.OrmProvider.GetFieldName(columnInfo.RefColumnName));
                }
                builder.Append($") ON DELETE {myIndexInfo.DeleteRule} ON UPDATE {myIndexInfo.UpdateRule}");
            }
        }
        builder.AppendLine(")");
        builder.AppendLine("GO");
        if (commentBuilder.Length > 0)
            builder.AppendLine(commentBuilder.ToString());
        builder.AppendLine("END");
        await this.ExecuteAsync(builder.ToString(), cancellationToken);
    }
    #endregion

    class TableInfo
    {
        public string Description { get; set; }
    }
    class ColumnInfo
    {
        public string ColumnName { get; set; }
        public string DataType { get; set; }
        public int MaxLength { get; set; }
        public int Precision { get; set; }
        public int Scale { get; set; }
        public bool IsIdentity { get; set; }
        public bool IsNullable { get; set; }
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