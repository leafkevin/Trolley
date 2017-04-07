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
        protected void ReadNextResult()
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
    public class QueryReader<TEntity> : QueryReader
    {
        protected Type entityType;

        public QueryReader(int hashKey, Type entityType, DbCommand command, DbDataReader reader, bool isMappingIgnoreCase, bool isCloseConnection)
            : base(hashKey, command, reader, isMappingIgnoreCase, isCloseConnection)
        {
            this.entityType = entityType;
        }
        public TEntity Read()
        {
            TEntity result = default(TEntity);
            var func = RepositoryHelper.GetReader(this.hashKey, this.entityType, this.reader, this.isMappingIgnoreCase);
            while (reader.Read())
            {
                var objResult = func?.Invoke(reader);
                if (objResult == null || objResult is TEntity) result = (TEntity)objResult;
                else result = (TEntity)Convert.ChangeType(objResult, this.entityType, CultureInfo.InvariantCulture);
            }
            base.ReadNextResult();
            return result;
        }
    }
}
