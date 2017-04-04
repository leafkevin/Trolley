using System.Data.Common;

namespace Trolley.Providers
{
    public class SqLiteProvider : BaseOrmProvider
    {
        public override DbConnection CreateConnection(string connString)
        {
            var factory = OrmProviderFactory.GetFactory("Microsoft.Data.Sqlite.SqliteFactory, Microsoft.Data.Sqlite, Culture=neutral, PublicKeyToken=adb9793829ddae60");
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

