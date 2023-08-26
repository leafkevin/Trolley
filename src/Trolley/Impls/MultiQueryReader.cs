using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley;

class MultiQueryReader : IMultiQueryReader
{
    private readonly string dbKey;
    private readonly IDbCommand command;
    private readonly IDataReader reader;
    private readonly IOrmProvider ormProvider;
    private readonly IEntityMapProvider mapProvider;
    private int readerIndex = 0;
    private readonly List<ReaderAfter> readerAfters;
    public MultiQueryReader(string dbKey, IDbCommand command, IDataReader reader, IOrmProvider ormProvider, IEntityMapProvider mapProvider, List<ReaderAfter> readerAfters = null)
    {
        this.dbKey = dbKey;
        this.command = command;
        this.reader = reader;
        this.ormProvider = ormProvider;
        this.mapProvider = mapProvider;
        this.readerAfters = readerAfters;
    }
    public T ReadFirst<T>()
    {
        T result = default;
        if (this.reader.Read())
        {
            if (this.readerAfters != null)
                result = (T)this.readerAfters[this.readerIndex].ReaderGetter.Invoke(this.reader);
            else
            {
                var targetType = typeof(T);
                if (targetType.IsEntityType(out _))
                    result = this.reader.To<T>(this.dbKey, this.ormProvider, this.mapProvider);
                else result = this.reader.To<T>();
            }
        }
        this.ReadNextResult();
        return result;
    }
    public List<T> Read<T>()
    {
        var result = new List<T>();
        if (this.readerAfters != null)
        {
            var readerAfter = this.readerAfters[this.readerIndex];
            while (this.reader.Read())
            {
                result.Add((T)readerAfter.ReaderGetter.Invoke(this.reader));
            }
        }
        else
        {
            var targetType = typeof(T);
            if (targetType.IsEntityType(out _))
            {
                while (this.reader.Read())
                {
                    result.Add(this.reader.To<T>(this.dbKey, this.ormProvider, this.mapProvider));
                }
            }
            else
            {
                while (this.reader.Read())
                {
                    result.Add(this.reader.To<T>());
                }
            }
        }
        this.ReadNextResult();
        return result;
    }
    public IPagedList<T> ReadPageList<T>()
    {
        this.Read<T>();
        return null;
    }
    public Dictionary<TKey, TValue> ReadDictionary<TKey, TValue>()
    {
        return null;
    }
    public Task<T> ReadFirstAsync<T>(CancellationToken cancellationToken = default)
    {
        return null;
    }
    public Task<List<T>> ReadAsync<T>(CancellationToken cancellationToken = default)
    {
        return null;
    }
    public Task<IPagedList<T>> ReadPageListAsync<T>(CancellationToken cancellationToken = default)
    {
        return null;
    }
    public Task<Dictionary<TKey, TValue>> ReadDictionaryAsync<TKey, TValue>(CancellationToken cancellationToken = default)
    {
        return null;
    }
    private void ReadNextResult()
    {
        if (!this.reader.NextResult())
            this.Dispose();
        else this.readerIndex++;
    }
    private async Task ReadNextResultAsync(CancellationToken cancellationToken)
    {
        if (this.command is not DbCommand dbCommand)
            throw new NotSupportedException("当前数据库驱动不支持异步SQL查询");
        if (this.reader is not DbDataReader dbReader)
            throw new NotSupportedException("当前数据库驱动不支持异步SQL查询");

        if (!await dbReader.NextResultAsync(cancellationToken))
        {
            var conn = dbCommand.Connection;
            await dbReader.DisposeAsync();
            await dbCommand.DisposeAsync();
            await conn.DisposeAsync();
        }
        else this.readerIndex++;
    }
    public void Dispose()
    {
        var connection = this.command?.Connection;
        this.reader?.Dispose();
        this.command?.Dispose();
        connection?.Dispose();
    }
    public async ValueTask DisposeAsync()
    {
        if (this.command is not DbCommand dbCommand)
            throw new NotSupportedException("当前数据库驱动不支持异步SQL查询");
        if (this.reader is not DbDataReader dbReader)
            throw new NotSupportedException("当前数据库驱动不支持异步SQL查询");

        var connection = dbCommand?.Connection;
        if (dbReader != null)
            await dbReader.DisposeAsync();
        if (dbCommand != null)
            await dbCommand.DisposeAsync();
        if (connection != null)
            await connection.DisposeAsync();
    }
}
