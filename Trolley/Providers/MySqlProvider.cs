using System.Data.Common;

namespace Trolley.Providers
{
    public class MySqlProvider : BaseOrmProvider
    {
        public override DbConnection CreateConnection(string connString)
        {
            //TODO:Oracle官方暂时还没有提供.NET Core版本驱动，无法实现跨平台
            var factory = OrmProviderFactory.GetFactory("MySql.Data.MySqlClient.MySqlClientFactory, MySql.Data, Culture=neutral, PublicKeyToken=c5687fc88969c44d");
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
