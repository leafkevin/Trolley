using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;

namespace Trolley
{
    public class QueryReader : IDisposable
    {
        protected bool isMappingIgnoreCase = false;
        protected bool isCloseConnection = true;
        protected int hashKey;
        protected DbDataReader reader;
        protected DbCommand command;

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
        public Dictionary<TKey, TValue> ReadDictionary<TKey, TValue>()
        {
            Type targetType = typeof(KeyValuePair<TKey, TValue>);
            Dictionary<TKey, TValue> result = new Dictionary<TKey, TValue>();
            int keyIndex = reader.GetOrdinal(nameof(KeyValuePair<TKey, TValue>.Key));
            int valueIndex = reader.GetOrdinal(nameof(KeyValuePair<TKey, TValue>.Value));
            while (reader.Read())
            {
                result.Add(reader.GetFieldValue<TKey>(keyIndex), reader.GetFieldValue<TValue>(valueIndex));
            }
            this.ReadNextResult();
            return result;
        }
        public PagedList<T> ReadPageList<T>(int pageIndex, int pageSize, int recordsTotal)
        {
            Type targetType = typeof(T);
            PagedList<T> result = new PagedList<T>();
            result.Data = new List<T>();
            var func = RepositoryHelper.GetReader(this.hashKey, targetType, this.reader, this.isMappingIgnoreCase);
            while (reader.Read())
            {
                var objResult = func?.Invoke(reader);
                if (objResult == null) continue;
                if (objResult is T) result.Data.Add((T)objResult);
                else result.Data.Add((T)Convert.ChangeType(objResult, targetType, CultureInfo.InvariantCulture));
            }
            this.ReadNextResult();
            result.PageIndex = pageIndex;
            result.PageSize = pageSize;
            result.RecordsTotal = recordsTotal;
            if (pageSize <= 0) throw new ArgumentException("pageSize不能<=0");
            result.PageTotal = (int)Math.Ceiling((double)recordsTotal / pageSize);
            return result;
        }
        protected void ReadNextResult()
        {
            if (!this.reader.NextResult())
            {
                this.reader.Close();
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
        public void Dispose()
        {
            if (this.reader != null)
            {
                if (!reader.IsClosed) this.reader.Close();
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
