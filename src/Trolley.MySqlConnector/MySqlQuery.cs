using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley.MySqlConnector;

public class MySqlQuery<T> : Query<T>
{
    #region Fields
    private MySqlProvider dialectProvider => this.DbContext.OrmProvider as MySqlProvider;
    #endregion

    #region Constructor
    public MySqlQuery(DbContext dbContext, IQueryVisitor visitor)
        : base(dbContext, visitor) { }
    #endregion

    #region GetShardingTableNames
    public override List<string> GetShardingTableNames(Func<string, bool> tableNameSelector)
    {
        var tableSchema = this.Visitor.Tables[0].TableSchema;
        return this.dialectProvider.GetShardingTableNames<T>(this.DbContext, tableNameSelector, tableSchema);
    }
    public override async Task<List<string>> GetShardingTableNamesAsync<TEntity>(Func<string, bool> tableNameSelector, CancellationToken cancellationToken = default)
    {
        var tableSchema = this.Visitor.Tables[0].TableSchema;
        return await this.dialectProvider.GetShardingTableNamesAsync<T>(this.DbContext, tableNameSelector, tableSchema);
    }
    #endregion
}