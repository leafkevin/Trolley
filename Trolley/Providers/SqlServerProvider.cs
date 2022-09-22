using System;
using System.Data;
using System.Text;

namespace Trolley;.Providers;

public class SqlServerProvider : BaseOrmProvider
{
    public override IDbConnection CreateConnection(string connString)
    {
        var assemblyQualifiedName = "System.Data.SqlClient.SqlClientFactory, System.Data.SqlClient, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
        var factory = this.GetFactory(assemblyQualifiedName);
        var result = factory.CreateConnection();
        result.ConnectionString = connString;
        return result;
    }
    public override string GetFieldName(string propertyName) => "[" + propertyName + "]";
    public override string GetTableName(string entityName) => "[" + entityName + "]";
    public override string GetPagingTemplate(int skip, int? limit, string orderBy = null)
    {
        if (String.IsNullOrEmpty(orderBy)) throw new ArgumentNullException("orderBy");
        var builder = new StringBuilder("SELECT /**fields**/ FROM /**tables**/ WHERE /**conditions**/");
        if (!String.IsNullOrEmpty(orderBy)) builder.Append($" {orderBy}");
        builder.Append($" OFFSET {skip} ROWS");
        if (limit.HasValue) builder.AppendFormat($" FETCH NEXT {limit} ROWS ONLY", limit);
        return builder.ToString();
    }
}
