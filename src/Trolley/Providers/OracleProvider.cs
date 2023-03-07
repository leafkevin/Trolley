using System;
using System.Data;
using System.Text;

namespace Trolley;.Providers;

public class OracleProvider : BaseOrmProvider
{
    public override string ParameterPrefix => ":";
    public override IDbConnection CreateConnection(string connString)
    {
        //TODO:Oracle官方暂时还没有提供.NET Core版本驱动，无法实现跨平台
        var assemblyQualifiedName = "Oracle.ManagedDataAccess.Client.OracleClientFactory, Oracle.ManagedDataAccess, Culture=neutral, PublicKeyToken=89b483f429c47342";
        var factory = this.GetFactory(assemblyQualifiedName);
        var result = factory.CreateConnection();
        result.ConnectionString = connString;
        return result;
    }
    public override string GetPagingTemplate(int skip, int? limit, string orderBy = null)
    {
        if (String.IsNullOrEmpty(orderBy)) throw new ArgumentNullException("orderBy");
        var builder = new StringBuilder($"SELECT * FROM (SELECT ROW_NUMBER() OVER({orderBy}) _RowIndex,");
        builder.Append($"/**fields**/ FROM /**tables**/ /**others**/) T WHERE _RowIndex>={skip}");
        if (limit.HasValue) builder.Append($" AND _RowIndex<{skip + limit.Value}");
        return builder.ToString();
    }
}
