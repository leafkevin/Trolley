using System;

namespace Trolley.SqlServer;

public class SqlServerMultipleQuery : MultipleQuery
{
    #region Constructor
    public SqlServerMultipleQuery(DbContext dbContext)
        : base(dbContext) { }
    #endregion

    #region GetShardingTableNames
    public override IMultipleQuery GetShardingTableNames<TEntity>(Func<string, bool> tableNameSelector, string tableSchema = null)
    {
        var entityMapper = this.DbContext.EntityMapProvider.GetEntityMap(typeof(TEntity));
        var orgTableName = entityMapper.TableName;
        tableSchema ??= this.DbContext.DefaultTableSchema;
        var sql = $"SELECT a.relname FROM pg_class a,pg_namespace b WHERE a.relnamespace=b.oid AND a.relkind='r' AND a.relname LIKE '{orgTableName}_%' AND b.nspname='{tableSchema}'";
        this.AddReader(typeof(string), sql, false);
        return this;
    }
    #endregion
}