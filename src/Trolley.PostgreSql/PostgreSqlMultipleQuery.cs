using System;

namespace Trolley.PostgreSql;

public class PostgreSqlMultipleQuery : MultipleQuery
{
    #region Constructor
    public PostgreSqlMultipleQuery(DbContext dbContext)
        : base(dbContext) { }
    #endregion

    #region GetShardingTableNames
    public override IMultipleQuery GetShardingTableNames<TEntity>(Func<string, bool> tableNameSelector, string tableSchema = null)
    {
        var entityMapper = this.DbContext.MapProvider.GetEntityMap(typeof(TEntity));
        var orgTableName = entityMapper.TableName;
        tableSchema ??= this.DbContext.DefaultTableSchema;
        var sql = $"SELECT a.relname FROM pg_class a,pg_namespace b WHERE a.relnamespace=b.oid AND a.relkind='r' AND a.relname LIKE '{orgTableName}_%' AND b.nspname='{tableSchema}'";
        Func<ITheaDataReader, object> readerGetter = reader => reader.ToValue<string>(this.DbContext);
        this.AddReader(typeof(string), sql, readerGetter, false);
        return this;
    }
    #endregion
}