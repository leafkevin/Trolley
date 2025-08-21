using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley.MySqlConnector;

public class MySqlDeleted<TEntity, TResult> : Deleted<TEntity>, IMySqlDeleted<TEntity, TResult>
{
    #region Properties
    public MySqlDeleteVisitor DialectVisitor { get; protected set; }
    public IOrmProvider OrmProvider => this.Visitor.OrmProvider;
    #endregion

    #region Constructor
    public MySqlDeleted(DbContext dbContext, IDeleteVisitor visitor)
        : base(dbContext, visitor)
    {
        this.DialectVisitor = this.Visitor as MySqlDeleteVisitor;
    }
    #endregion

    #region Execute
    public new List<TResult> Execute()
    {
        if (!this.Visitor.HasWhere)
            throw new InvalidOperationException("缺少where条件，请使用Where/And/Or方法完成where条件");

        var result = new List<TResult>();
        (var isNeedClose, var connection, var command) = this.DbContext.UseMasterCommand();
        command.CommandText = this.Visitor.BuildCommand(command, out var readerFields);
        connection.Open();

        using var reader = command.ExecuteReader(CommandSqlType.Delete, CommandBehavior.SequentialAccess);
        var readerDeserializer = reader.GetReaderDeserializer(typeof(TResult), this.DbContext, readerFields);

        while (reader.Read())
            result.Add((TResult)readerDeserializer.Invoke(reader));
        while (reader.NextResult())
        {
            while (reader.Read())
                result.Add((TResult)readerDeserializer.Invoke(reader));
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
            throw new InvalidOperationException("缺少where条件，请使用Where/And/Or方法完成where条件");

        var result = new List<TResult>();
        (var isNeedClose, var connection, var command) = this.DbContext.UseMasterCommand();
        command.CommandText = this.Visitor.BuildCommand(command, out var readerFields);
        await connection.OpenAsync(cancellationToken);
        using var reader = await command.ExecuteReaderAsync(CommandSqlType.Delete, CommandBehavior.SequentialAccess, cancellationToken);
        var readerDeserializer = reader.GetReaderDeserializer(typeof(TResult), this.DbContext, readerFields);

        while (await reader.ReadAsync(cancellationToken))
            result.Add((TResult)readerDeserializer.Invoke(reader));
        while (await reader.NextResultAsync(cancellationToken))
        {
            while (await reader.ReadAsync(cancellationToken))
                result.Add((TResult)readerDeserializer.Invoke(reader));
        }

        await reader.DisposeAsync();
        await command.DisposeAsync();
        if (isNeedClose) await connection.CloseAsync();
        this.Visitor.Dispose();
        return result;
    }
    #endregion
}