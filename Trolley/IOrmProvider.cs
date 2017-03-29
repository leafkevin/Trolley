using System.Data.Common;

namespace Trolley
{
    public interface IOrmProvider
    {
        string ParamPrefix { get; }
        DbConnection CreateConnection(string ConnString);
        string GetQuotedTableName(string tableName);
        string GetQuotedColumnName(string columnName);
        string GetPagingExpression(string sql, int skip, int? limit, string orderBy = null);
    }
}
