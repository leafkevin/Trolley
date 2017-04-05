using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;
#if ASYNC
using System.Threading.Tasks;
#endif

namespace Trolley
{
    public class QueryReader : IDisposable
    {
        private bool isMappingIgnoreCase = false;
        private bool isCloseConnection = true;
        private int hashKey;
        private DbDataReader reader;
        private DbCommand command;

        public QueryReader(int hashKey, DbCommand command, DbDataReader reader, bool isMappingIgnoreCase, bool isCloseConnection)
        {
            this.hashKey = hashKey;
            this.command = command;
            this.reader = reader;
            this.isMappingIgnoreCase = isMappingIgnoreCase;
            this.isCloseConnection = isCloseConnection;
        }
        public T Read<T>()
        {
            Type targetType = typeof(T);
            T result = default(T);
            var func = RepositoryHelper.GetReader(this.hashKey, targetType, this.reader, this.isMappingIgnoreCase);
            while (reader.Read())
            {
                var objResult = func?.Invoke(reader);
                if (objResult == null || objResult is T) result = (T)objResult;
                else result = (T)Convert.ChangeType(objResult, targetType, CultureInfo.InvariantCulture);
            }
            this.ReadNextResult();
            return result;
        }
        public List<T> ReadList<T>()
        {
            Type targetType = typeof(T);
            List<T> result = new List<T>();
            var func = RepositoryHelper.GetReader(this.hashKey, targetType, this.reader, this.isMappingIgnoreCase);
            while (reader.Read())
            {
                var objResult = func?.Invoke(reader);
                if (objResult == null) continue;
                if (objResult is T) result.Add((T)objResult);
                else result.Add((T)Convert.ChangeType(objResult, targetType, CultureInfo.InvariantCulture));
            }
            this.ReadNextResult();
            return result;
        }
        public PagedList<T> ReadPageList<T>()
        {
            Type targetType = typeof(T);
            PagedList<T> result = new PagedList<T>();
            var func = RepositoryHelper.GetReader(this.hashKey, targetType, this.reader, this.isMappingIgnoreCase);
            while (reader.Read())
            {
                var objResult = func?.Invoke(reader);
                if (objResult == null) continue;
                if (objResult is T) result.Add((T)objResult);
                else result.Add((T)Convert.ChangeType(objResult, targetType, CultureInfo.InvariantCulture));
            }
            this.ReadNextResult();
            return result;
        }
        private void ReadNextResult()
        {
            if (!this.reader.NextResult())
            {
#if COREFX
                try { this.command.Cancel(); } catch { }
#else
                this.reader.Close();
#endif
                this.reader.Dispose();
                this.reader = null;
                if (this.isCloseConnection)
                {
                    var conn = this.command.Connection;
                    conn.Close();
                    conn.Dispose();
                    this.command.Dispose();
                    this.command = null;
                }
            }
        }
#if ASYNC
        public async Task<T> ReadAsync<T>()
        {
            Type targetType = typeof(T);
            T result = default(T);
            var func = RepositoryHelper.GetReader(this.hashKey, targetType, this.reader, this.isMappingIgnoreCase);
            while (await reader.ReadAsync())
            {
                var objResult = func?.Invoke(reader);
                if (objResult == null || objResult is T) result = (T)objResult;
                else result = (T)Convert.ChangeType(objResult, targetType, CultureInfo.InvariantCulture);
            }
            await this.ReadNextResultAsync();
            return result;
        }
        public async Task<List<T>> ReadListAsync<T>()
        {
            Type targetType = typeof(T);
            List<T> result = new List<T>();
            var func = RepositoryHelper.GetReader(this.hashKey, targetType, this.reader, this.isMappingIgnoreCase);
            while (await reader.ReadAsync())
            {
                var objResult = func?.Invoke(reader);
                if (objResult == null) continue;
                if (objResult is T) result.Add((T)objResult);
                else result.Add((T)Convert.ChangeType(objResult, targetType, CultureInfo.InvariantCulture));
            }
            await this.ReadNextResultAsync();
            return result;
        }
        public async Task<PagedList<T>> ReadPageListAsync<T>()
        {
            Type targetType = typeof(T);
            PagedList<T> result = new PagedList<T>();
            var func = RepositoryHelper.GetReader(this.hashKey, targetType, this.reader, this.isMappingIgnoreCase);
            while (await reader.ReadAsync())
            {
                var objResult = func?.Invoke(reader);
                if (objResult == null) continue;
                if (objResult is T) result.Add((T)objResult);
                else result.Add((T)Convert.ChangeType(objResult, targetType, CultureInfo.InvariantCulture));
            }
            await this.ReadNextResultAsync();
            return result;
        }
        private async Task ReadNextResultAsync()
        {
            if (!await this.reader.NextResultAsync())
            {
#if COREFX
                try { this.command.Cancel(); } catch { }
#else
                this.reader.Close();
#endif
                this.reader.Dispose();
                this.reader = null;
                if (this.isCloseConnection)
                {
                    var conn = this.command.Connection;
                    conn.Close();
                    conn.Dispose();
                    this.command.Dispose();
                    this.command = null;
                }
            }
        }
#endif
        public void Dispose()
        {
            if (this.reader != null)
            {
                if (!reader.IsClosed)
                {
#if COREFX
                    try { this.command.Cancel(); } catch { }
#else
                    this.reader.Close();
#endif
                }
                this.reader.Dispose();
                this.reader = null;
            }
            if (this.command != null)
            {
                this.command.Dispose();
                this.command = null;
            }
        }
    }
}
