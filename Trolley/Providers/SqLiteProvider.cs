using System.Data.Common;

namespace Trolley.Providers
{
    public class SqLiteProvider : BaseOrmProvider
    {
        public override DbConnection CreateConnection(string connString)
        {
            var assemblyQualifiedName = "System.Data.SQLite.SQLiteFactory, System.Data.SqlClient, Culture=neutral, PublicKeyToken=db937bc2d44ff139";
            var factory = OrmProviderFactory.GetFactory(assemblyQualifiedName, "System.Data.SqlClient.dll");
            var result = factory.CreateConnection();
            result.ConnectionString = connString;
            return result;
        }
        public override string GetPropertyName(string propertyName)
        {
            return "\"" + propertyName + "\"";
        }
        public override string GetTableName(string entityName)
        {
            return "\"" + entityName + "\"";
        }
        public override string GetColumnName(string propertyName)
        {
            return "\"" + propertyName + "\"";
        }
    }
}

