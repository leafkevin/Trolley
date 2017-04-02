using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace Trolley
{
    public class DbTypeMap
    {
        private static readonly Dictionary<Type, DbType> typeMap = new Dictionary<Type, DbType>();
        public const string LinqBinary = "System.Data.Linq.Binary";
        static DbTypeMap()
        {
            typeMap = new Dictionary<Type, DbType>();
            typeMap[typeof(byte)] = DbType.Byte;
            typeMap[typeof(sbyte)] = DbType.SByte;
            typeMap[typeof(short)] = DbType.Int16;
            typeMap[typeof(ushort)] = DbType.UInt16;
            typeMap[typeof(int)] = DbType.Int32;
            typeMap[typeof(uint)] = DbType.UInt32;
            typeMap[typeof(long)] = DbType.Int64;
            typeMap[typeof(ulong)] = DbType.UInt64;
            typeMap[typeof(float)] = DbType.Single;
            typeMap[typeof(double)] = DbType.Double;
            typeMap[typeof(decimal)] = DbType.Decimal;
            typeMap[typeof(bool)] = DbType.Boolean;
            typeMap[typeof(string)] = DbType.String;
            typeMap[typeof(char)] = DbType.StringFixedLength;
            typeMap[typeof(Guid)] = DbType.Guid;
            typeMap[typeof(DateTime)] = DbType.DateTime;
            typeMap[typeof(DateTimeOffset)] = DbType.DateTimeOffset;
            typeMap[typeof(TimeSpan)] = DbType.Time;
            typeMap[typeof(byte[])] = DbType.Binary;
            typeMap[typeof(byte?)] = DbType.Byte;
            typeMap[typeof(sbyte?)] = DbType.SByte;
            typeMap[typeof(short?)] = DbType.Int16;
            typeMap[typeof(ushort?)] = DbType.UInt16;
            typeMap[typeof(int?)] = DbType.Int32;
            typeMap[typeof(uint?)] = DbType.UInt32;
            typeMap[typeof(long?)] = DbType.Int64;
            typeMap[typeof(ulong?)] = DbType.UInt64;
            typeMap[typeof(float?)] = DbType.Single;
            typeMap[typeof(double?)] = DbType.Double;
            typeMap[typeof(decimal?)] = DbType.Decimal;
            typeMap[typeof(bool?)] = DbType.Boolean;
            typeMap[typeof(char?)] = DbType.StringFixedLength;
            typeMap[typeof(Guid?)] = DbType.Guid;
            typeMap[typeof(DateTime?)] = DbType.DateTime;
            typeMap[typeof(DateTimeOffset?)] = DbType.DateTimeOffset;
            typeMap[typeof(TimeSpan?)] = DbType.Time;
        }
        internal static bool ContainsKey(Type type)
        {
            var underlyingType = Nullable.GetUnderlyingType(type) ?? type;
            if (underlyingType.GetTypeInfo().IsEnum) return true;
            return typeMap.ContainsKey(underlyingType);
        }
        internal static DbType LookupDbType(Type type)
        {
            DbType dbType;
            var underlyingType = Nullable.GetUnderlyingType(type) ?? type;
            if (underlyingType.GetTypeInfo().IsEnum)
            {
                underlyingType = Enum.GetUnderlyingType(underlyingType);
            }
            if (typeMap.TryGetValue(underlyingType, out dbType))
            {
                return dbType;
            }
            if (underlyingType.FullName == LinqBinary)
            {
                return DbType.Binary;
            }
            throw new NotSupportedException(string.Format("没有设置类型{0}DbType的映射", type.FullName));
        }
    }
}
