using System.Collections.Generic;
using System.Data.Common;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley;

class MultiQueryReader : IMultiQueryReader
{
    private readonly string dbKey;
    private readonly IDbCommand command;
    private readonly IDataReader reader;
    private readonly IOrmProvider ormProvider;
    private int readerIndex = 0;
    public MultiQueryReader(string dbKey, IDbCommand command, IDataReader reader, IOrmProvider ormProvider)
    {
        this.dbKey = dbKey;
        this.command = command;
        this.reader = reader;
        this.ormProvider = ormProvider;
    }
    public dynamic ReadFirst()
    {
        return null;
    }
    public T ReadFirst<T>()
    {
        //if (this.reader.Read())
        //{
        //    var entityType = typeof(T);
        //    if (entityType.IsEntityType())
        //        return this.reader.To<T>(this.dbKey, this.ormProvider, readerFields);
        //    else return this.reader.To<T>();
        //}
        return default(T);
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

    public void Dispose()
    {
        throw new System.NotImplementedException();
    }

    private void ReadNextResult()
    {
        if (!this.reader.NextResult())
        {
            this.reader.Close();
            this.reader.Dispose();
            var conn = this.command.Connection;
            conn.Close();
            conn.Dispose();
            this.command.Dispose();
        }
    }
}
