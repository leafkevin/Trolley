using System.Collections.Generic;
using System.Data.Common;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace Trolley;

class MultiQueryReader : IMultiQueryReader
{
    private readonly string dbKey;
    private readonly IDbCommand command;
    private readonly IDataReader reader;
    private readonly IOrmProvider ormProvider;
    private readonly IEntityMapProvider mapProvider;
    private int readerIndex = 0;
    private readonly List<Func<IDataReader, object>> readerGetters;
    public MultiQueryReader(string dbKey, IDbCommand command, IDataReader reader, IOrmProvider ormProvider, IEntityMapProvider mapProvider, List<Func<IDataReader, object>> readerGetters = null)
    {
        this.dbKey = dbKey;
        this.command = command;
        this.reader = reader;
        this.ormProvider = ormProvider;
        this.mapProvider = mapProvider;
        this.readerGetters = readerGetters;
    }
    public dynamic ReadFirst()
    {
        dynamic result = default;
        if (this.reader.Read())
        {
            if (this.readerGetters != null)
                result = (dynamic)this.readerGetters[readerIndex].Invoke(this.reader);
            else
            {
                var entityType = typeof(T);
                if (entityType.IsEntityType())
                    result = reader.To<T>(this.dbKey, this.ormProvider, this.mapProvider);
                else result = reader.To<T>();
            }
        }
        this.ReadNextResult();
        return result;
    }
    public T ReadFirst<T>()
    {
        T result = default;
        if (this.reader.Read())
        {
            if (this.readerGetters != null)
                result = (T)this.readerGetters[readerIndex].Invoke(this.reader);
            else
            {
                var entityType = typeof(T);
                if (entityType.IsEntityType())
                    result = reader.To<T>(this.dbKey, this.ormProvider, this.mapProvider);
                else result = reader.To<T>();
            }
        }
        this.ReadNextResult();
        this.readerIndex++;
        return result;
    }
    public List<dynamic> Read()
    {
        return null;
    }
    public List<T> Read<T>()
    {
        return null;
    }
    public IPagedList<dynamic> ReadPageList()
    {
        return null;
    }
    public IPagedList<T> ReadPageList<T>()
    {
        return null;
    }
    public Dictionary<TKey, TValue> ReadDictionary<TKey, TValue>()
    {
        return null;
    }
    public Task<dynamic> ReadFirstAsync(CancellationToken cancellationToken = default)
    {
        return null;
    }
    public Task<T> ReadFirstAsync<T>(CancellationToken cancellationToken = default)
    {
        return null;
    }
    public Task<List<dynamic>> ReadAsync(CancellationToken cancellationToken = default)
    {
        return null;
    }
    public Task<List<T>> ReadAsync<T>(CancellationToken cancellationToken = default)
    {
        return null;
    }
    public Task<IPagedList<dynamic>> ReadPageListAsync(CancellationToken cancellationToken = default)
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
        {
            var conn = this.command.Connection;
            this.reader.Dispose();
            this.command.Dispose();
            conn.Dispose();
        }
    }
    private async Task ReadNextResultAsync()
    {
        if (!this.reader.NextResult())
        {
            if (this.command is not DbCommand dbCommand)
                throw new NotSupportedException("当前数据库驱动不支持异步SQL查询");
            if (this.reader is not DbDataReader dbReader)
                throw new NotSupportedException("当前数据库驱动不支持异步SQL查询");

            var conn = dbCommand.Connection;
            await dbReader.DisposeAsync();
            await dbCommand.DisposeAsync();
            await conn.DisposeAsync();
        }
    }
    public void Dispose()
    {
        throw new System.NotImplementedException();
    }
    public ValueTask DisposeAsync()
    {
        throw new System.NotImplementedException();
    }
}
