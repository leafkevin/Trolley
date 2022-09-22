using System.Data;

namespace Trolley;.Providers;

public class SqLiteProvider : BaseOrmProvider
{
    public override IDbConnection CreateConnection(string connString)
    {
        var assemblyQualifiedName = "System.Data.SQLite.SQLiteFactory, System.Data.SqlClient, Culture=neutral, PublicKeyToken=db937bc2d44ff139";
        var factory = this.GetFactory(assemblyQualifiedName);
        var result = factory.CreateConnection();
        result.ConnectionString = connString;
        return result;
    }
    public override string GetFieldName(string propertyName) => "\"" + propertyName + "\"";
    public override string GetTableName(string entityName) => "\"" + entityName + "\"";
}

