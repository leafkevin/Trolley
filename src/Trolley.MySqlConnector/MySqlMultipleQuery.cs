using System;

namespace Trolley.MySqlConnector;

public class MySqlMultipleQuery : MultipleQuery
{
    #region Constructor
    public MySqlMultipleQuery(DbContext dbContext)
        : base(dbContext) { }
    #endregion

    #region GetShardingTableNames
    public override IMultipleQuery GetShardingTableNames<TEntity>(Func<string, bool> tableNameSelector, string tableSchema = null)
    {
        var entityMapper = this.DbContext.MapProvider.GetEntityMap(typeof(TEntity));
        var orgTableName = entityMapper.TableName;
        tableSchema ??= this.DbContext.DefaultTableSchema;
        var sql = $"SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE' AND TABLE_NAME LIKE '{orgTableName}_%' AND TABLE_SCHEMA='{tableSchema}'";
        this.AddReader(typeof(string), sql, false);
        return this;
    }
    #endregion
}