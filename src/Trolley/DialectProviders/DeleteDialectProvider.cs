using System;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley;

public class DeleteDialectProvider : DialectProvider
{
    #region Delete
    public int Delete<TEntity>(object whereObjs, bool isUseKey, bool isBulk)
    {
        (var isNeedClose, var connection, var command) = this.CreateDeleteCommand(typeof(TEntity), whereObjs, isUseKey, isBulk);
        return this.Execute(isNeedClose, connection, command);
    }
    public async Task<int> DeleteAsync<TEntity>(object whereObjs, bool isUseKey, bool isBulk, CancellationToken cancellationToken = default)
    {
        (var isNeedClose, var connection, var command) = this.CreateDeleteCommand(typeof(TEntity), whereObjs, isUseKey, isBulk);
        await connection.OpenAsync(cancellationToken);
        var result = await command.ExecuteNonQueryAsync(cancellationToken);

        await command.DisposeAsync();
        if (isNeedClose) await connection.CloseAsync();
        return result;
    }
    private (bool, ITheaConnection, ITheaCommand) CreateDeleteCommand(Type entityType, object whereObjs, bool isUseKey, bool isBulk)
    {
        if (whereObjs == null)
            throw new ArgumentNullException(nameof(whereObjs));
        (var isNeedClose, var connection, var command) = this.UseMasterCommand();
        var commandInitializer = RepositoryHelper.BuildWhereCommandInitializer(this.DbContext, entityType, whereObjs, 3, isUseKey, false, isBulk);
        command.CommandText = commandInitializer.Invoke(command.Parameters, this.DbContext, whereObjs);
        return (isNeedClose, connection, command);
    }
    #endregion
}