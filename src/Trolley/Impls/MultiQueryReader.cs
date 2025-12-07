using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley;

class MultiQueryReader : IMultiQueryReader
{
    private readonly bool isNeedClose;
    private readonly DbContext dbContext;
    private ITheaConnection connection;
    private ITheaCommand command;
    private ITheaDataReader reader;
    private List<ReaderAfter> readerAfters;

    private int readerIndex = 0;
    private List<NextReaderAfter> nextAfters;
    public MultiQueryReader(DbContext dbContext, ITheaConnection connection, ITheaCommand command, ITheaDataReader reader, List<ReaderAfter> readerAfters, bool isNeedClose)
    {
        this.dbContext = dbContext;
        this.connection = connection;
        this.command = command;
        this.reader = reader;
        this.readerAfters = readerAfters;
        this.isNeedClose = isNeedClose;
    }
    public T ReadFirst<T>()
    {
        T result = default;
        if (this.reader.Read())
        {
            var readerAfter = this.readerAfters[this.readerIndex];
            if (readerAfter.ResultType == ReaderResultType.Value)
            {
                object readerValue = this.reader.GetValue(0);
                if (readerAfter.IsExists)
                    readerValue = readerValue != null && readerValue is not DBNull;
                else this.reader.ToValue<T>(this.dbContext);
                result = (T)readerValue;
            }
            else
            {
                var deserializer = reader.GetReaderDeserializer(readerAfter.TargetType, this.dbContext, readerAfter.Visitor.ReaderFields);
                result = (T)deserializer.Invoke(this.reader);
            }
            this.NextReader(readerAfter, result);
        }
        else this.NextReader();
        return result;
    }
    public List<T> Read<T>()
    {
        var result = new List<T>();
        this.ReadList(result, false);
        return result;
    }
    public IPagedList<T> ReadPageList<T>()
    {
        int totalCount = 0;
        if (this.reader.Read())
            totalCount = reader.GetFieldValue<int>(0);
        this.reader.NextResult();
        var dataList = new List<T>();
        (var pageNumber, var pageSize) = this.ReadList<T>(dataList, true);
        return new PagedList<T>
        {
            Data = dataList,
            Count = dataList.Count,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }
    public async Task<T> ReadFirstAsync<T>(CancellationToken cancellationToken = default)
    {
        T result = default;
        if (await this.reader.ReadAsync(cancellationToken))
        {
            var readerAfter = this.readerAfters[this.readerIndex];
            if (readerAfter.ResultType == ReaderResultType.Value)
            {
                object readerValue = this.reader.GetValue(0);
                if (readerAfter.IsExists)
                    readerValue = readerValue != null && readerValue is not DBNull;
                else this.reader.ToValue<T>(this.dbContext);
                result = (T)readerValue;
            }
            else
            {
                var deserializer = reader.GetReaderDeserializer(readerAfter.TargetType, this.dbContext, readerAfter.Visitor.ReaderFields);
                result = (T)deserializer.Invoke(this.reader);
            }
            await this.NextReaderAsync(readerAfter, result, cancellationToken);
        }
        else await this.NextReaderAsync(cancellationToken);
        return result;
    }
    public async Task<List<T>> ReadAsync<T>(CancellationToken cancellationToken = default)
    {
        var result = new List<T>();
        await this.ReadListAsync(result, false, cancellationToken);
        return result;
    }
    public async Task<IPagedList<T>> ReadPageListAsync<T>(CancellationToken cancellationToken = default)
    {
        int totalCount = 0;
        if (await this.reader.ReadAsync(cancellationToken))
            totalCount = reader.GetFieldValue<int>(0);
        await this.reader.NextResultAsync(cancellationToken);

        var dataList = new List<T>();
        (var pageIndex, var pageSize) = await this.ReadListAsync(dataList, true, cancellationToken);
        return new PagedList<T>
        {
            Data = dataList,
            Count = dataList.Count,
            TotalCount = totalCount,
            PageNumber = pageIndex,
            PageSize = pageSize
        };
    }
    public string ToSql(out List<IDbDataParameter> dbParameters)
    {
        dbParameters = null;
        if (this.command.Parameters != null && this.command.Parameters.Count > 0)
            dbParameters = this.command.Parameters.Cast<IDbDataParameter>().ToList();
        return this.command.CommandText;
    }
    private (int, int) ReadList<T>(List<T> dataList, bool isPaged)
    {
        int pageIndex = 0;
        int pageSize = 0;
        var readerAfter = this.readerAfters[this.readerIndex];
        var deserializer = reader.GetReaderDeserializer(readerAfter.TargetType, this.dbContext, readerAfter.Visitor.ReaderFields);

        while (this.reader.Read())
            dataList.Add((T)deserializer.Invoke(this.reader));
        if (isPaged)
        {
            pageIndex = readerAfter.Visitor.PageNumber;
            pageSize = readerAfter.Visitor.PageSize;
        }
        this.NextReader(readerAfter, dataList);
        return (pageIndex, pageSize);
    }
    private async Task<(int, int)> ReadListAsync<T>(List<T> dataList, bool isPaged, CancellationToken cancellationToken = default)
    {
        int pageIndex = 0;
        int pageSize = 0;
        var readerAfter = this.readerAfters[this.readerIndex];
        var deserializer = reader.GetReaderDeserializer(readerAfter.TargetType, this.dbContext, readerAfter.Visitor.ReaderFields);

        var index = 0;
        while (index < this.reader.FieldCount)
        {
            this.reader.GetName(index);
            this.reader.GetValue(index);
        }
        while (await this.reader.ReadAsync(cancellationToken))
            dataList.Add((T)deserializer.Invoke(this.reader));
        if (isPaged)
        {
            pageIndex = readerAfter.Visitor.PageNumber;
            pageSize = readerAfter.Visitor.PageSize;
        }
        await this.NextReaderAsync(readerAfter, dataList, cancellationToken);
        return (pageIndex, pageSize);
    }
    public void Dispose()
    {
        if (this.readerAfters != null)
        {
            this.readerAfters.Clear();
            this.readerAfters = null;
        }
        if (this.nextAfters != null)
        {
            this.nextAfters.ForEach(f => f.Visitor.Dispose());
            this.nextAfters.Clear();
            this.nextAfters = null;
        }
        this.reader?.Dispose();
        this.reader = null;
        this.command?.Dispose();
        this.command = null;
        if (this.isNeedClose && this.connection != null)
            this.connection.Close();
    }
    public async ValueTask DisposeAsync()
    {
        if (this.readerAfters != null)
        {
            this.readerAfters.Clear();
            this.readerAfters = null;
        }
        if (this.nextAfters != null)
        {
            this.nextAfters.ForEach(f => f.Visitor.Dispose());
            this.nextAfters.Clear();
            this.nextAfters = null;
        }
        await this.reader.DisposeAsync();
        this.reader = null;
        await this.command.DisposeAsync();
        this.command = null;
        if (this.isNeedClose)
            await this.connection.CloseAsync();
    }
    private void NextReader()
    {
        this.reader.NextResult();
        this.readerIndex++;
        if (this.readerIndex == this.readerAfters.Count)
            this.Dispose();
    }
    private void NextReader(ReaderAfter readerAfter, object target)
    {
        var visitor = readerAfter.Visitor;
        if (visitor != null && visitor.BuildIncludeSql(readerAfter.TargetType, target, readerAfter.ResultType == ReaderResultType.List, out var sql))
        {
            this.nextAfters ??= new();
            this.nextAfters.Add(new NextReaderAfter
            {
                TargetType = readerAfter.TargetType,
                Sql = sql,
                Visitor = visitor,
                ResultType = readerAfter.ResultType,
                Target = target
            });
        }
        this.reader.NextResult();
        this.readerIndex++;

        if (this.readerIndex >= this.readerAfters.Count)
        {
            if (this.nextAfters != null && this.nextAfters.Count > 0)
            {
                var builder = new StringBuilder();
                foreach (var nextAfter in this.nextAfters)
                {
                    if (builder.Length > 0) builder.Append(';');
                    builder.Append(nextAfter.Sql);
                }
                //先关闭reader，才能继续查询
                this.reader.Dispose();

                this.command.CommandText = builder.ToString();
                this.command.Parameters.Clear();
                visitor.NextDbParameters.CopyTo(this.command.Parameters);
                using var includeReader = command.ExecuteReader(CommandSqlType.Select, CommandBehavior.SequentialAccess);
                foreach (var nextAfter in this.nextAfters)
                {
                    nextAfter.Visitor.SetIncludeValues(nextAfter.TargetType, nextAfter.Target, includeReader, nextAfter.ResultType == ReaderResultType.List);
                }
            }
            this.Dispose();
        }
    }
    private async Task NextReaderAsync(CancellationToken cancellationToken = default)
    {
        await this.reader.NextResultAsync(cancellationToken);
        this.readerIndex++;
        if (this.readerIndex >= this.readerAfters.Count)
            await this.DisposeAsync();
    }
    private async Task NextReaderAsync(ReaderAfter readerAfter, object target, CancellationToken cancellationToken = default)
    {
        var visitor = readerAfter.Visitor;
        if (visitor != null && visitor.BuildIncludeSql(readerAfter.TargetType, target, readerAfter.ResultType == ReaderResultType.List, out var sql))
        {
            this.nextAfters ??= new();
            this.nextAfters.Add(new NextReaderAfter
            {
                TargetType = readerAfter.TargetType,
                Sql = sql,
                Visitor = visitor,
                ResultType = readerAfter.ResultType,
                Target = target
            });
        }

        await this.reader.NextResultAsync(cancellationToken);
        this.readerIndex++;

        if (this.readerIndex >= this.readerAfters.Count)
        {
            if (this.nextAfters != null && this.nextAfters.Count > 0)
            {
                var builder = new StringBuilder();
                foreach (var nextAfter in this.nextAfters)
                {
                    if (builder.Length > 0) builder.Append(';');
                    builder.Append(nextAfter.Sql);
                }
                //先关闭reader，才能继续查询
                await this.reader.DisposeAsync();

                this.command.CommandText = builder.ToString();
                this.command.Parameters.Clear();
                visitor.NextDbParameters.CopyTo(this.command.Parameters);

                using var includeReader = await this.command.ExecuteReaderAsync(CommandSqlType.Select, CommandBehavior.SequentialAccess, cancellationToken);
                foreach (var nextAfter in this.nextAfters)
                {
                    nextAfter.Visitor.SetIncludeValues(nextAfter.TargetType, nextAfter.Target, includeReader, nextAfter.ResultType == ReaderResultType.List);
                }
                await includeReader.NextResultAsync(cancellationToken);
            }
            await this.DisposeAsync();
        }
    }
    struct NextReaderAfter
    {
        public Type TargetType { get; set; }
        public string Sql { get; set; }
        public IQueryVisitor Visitor { get; set; }
        public ReaderResultType ResultType { get; set; }
        public object Target { get; set; }
    }
}
