using System.Data.Common;

namespace Trolley.Providers
{
    public class MySqlProvider : BaseOrmProvider
    {
        public override DbConnection CreateConnection(string connString)
        {
            var assemblyQualifiedName = "MySql.Data.MySqlClient.MySqlClientFactory, MySql.Data, Culture=neutral, PublicKeyToken=c5687fc88969c44d";
            var factory = OrmProviderFactory.GetFactory(assemblyQualifiedName, "MySql.Data.dll");
            var result = factory.CreateConnection();
            result.ConnectionString = connString;
            return result;
        }
        public override string GetPropertyName(string propertyName)
        {
            return "`" + propertyName + "`";
        }
        public override string GetTableName(string entityName)
        {
            return "`" + entityName + "`";
        }
        public override string GetColumnName(string propertyName)
        {
            return "`" + propertyName + "`";
        }
    }
}
