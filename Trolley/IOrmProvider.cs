using System.Data.Common;

namespace Trolley
{
    public interface IOrmProvider
    {
        /// <summary>
        /// 参数名前导字符，如：@:?
        /// </summary>
        string ParamPrefix { get; }
        /// <summary>
        /// 实体映射是否忽略大小写，对于Postgresql很有用
        /// </summary>
        bool IsMappingIgnoreCase { get; }
        DbConnection CreateConnection(string ConnString);
        string GetPropertyName(string propertyName);
        string GetTableName(string entityName);
        string GetColumnName(string propertyName);
        string GetPagingExpression(string sql, int skip, int? limit, string orderBy = null);
        string GetPagingCountExpression(string sql);
    }
}
