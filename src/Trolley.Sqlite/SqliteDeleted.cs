using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley.Sqlite;

public class SqliteDeleted<TEntity, TResult> : Deleted<TEntity>, ISqliteDeleted<TEntity, TResult>
{
    #region Properties
    public SqliteDeleteVisitor DialectVisitor { get; protected set; }
    public IOrmProvider OrmProvider => this.Visitor.OrmProvider;
    #endregion

    #region Constructor
    public SqliteDeleted(DbContext dbContext, IDeleteVisitor visitor)
        : base(dbContext, visitor)
    {
        this.DialectVisitor = this.Visitor as SqliteDeleteVisitor;
    }
    #endregion

    #region Execute
    public new List<TResult> Execute()
    {
        if (!this.Visitor.HasWhere)
            throw new InvalidOperationException("缺少where条件，请使用Where/And方法完成where条件");
        if (this.Visitor.IsNeedFetchShardingTables)
            this.DbContext.FetchShardingTables(this.Visitor as SqlVisitor);

        var result = new List<TResult>();
        (var isNeedClose, var connection, var command) = this.DbContext.UseMasterCommand();
        command.CommandText = this.Visitor.BuildCommand(command, out var readerFields);
        connection.Open();

        using var reader = command.ExecuteReader(CommandSqlType.Delete, CommandBehavior.SequentialAccess);
        while (reader.Read())
        {
            result.Add(reader.ToEntity<TResult>(this.DbContext, readerFields));
        }
        while (reader.NextResult())
        {
            while (reader.Read())
            {
                result.Add(reader.ToEntity<TResult>(this.DbContext, readerFields));
            }
        }

        reader.Dispose();
        command.Dispose();
        if (isNeedClose) connection.Close();
		this.Visitor.Dispose();
        return result;
    }
    public new async Task<List<TResult>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        if (!this.Visitor.HasWhere)
            throw new InvalidOperationException("缺少where条件，请使用Where/And方法完成where条件");
        if (this.Visitor.IsNeedFetchShardingTables)
            await this.DbContext.FetchShardingTablesAsync(this.Visitor as SqlVisitor, cancellationToken);

        var result = new List<TResult>();
        (var isNeedClose, var connection, var command) = this.DbContext.UseMasterCommand();
        command.CommandText = this.Visitor.BuildCommand(command, out var readerFields);
        await connection.OpenAsync(cancellationToken);
        using var reader = await command.ExecuteReaderAsync(CommandSqlType.Delete, CommandBehavior.SequentialAccess, cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(reader.ToEntity<TResult>(this.DbContext, readerFields));
        }
        while (await reader.NextResultAsync(cancellationToken))
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                result.Add(reader.ToEntity<TResult>(this.DbContext, readerFields));
            }
        }

        await reader.DisposeAsync();
        await command.DisposeAsync();
        if (isNeedClose) await connection.CloseAsync();
		this.Visitor.Dispose();
        return result;
    }
    #endregion
}