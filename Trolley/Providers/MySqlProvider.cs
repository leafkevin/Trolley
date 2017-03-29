using System.Data.Common;

namespace Trolley.Providers
{
    public class MySqlProvider : BaseOrmProvider
    {
        public override DbConnection CreateConnection(string connString)
        {
            var factory = OrmProviderFactory.GetFactory("MySql.Data.MySqlClient.MySqlClientFactory, MySql.Data, Culture=neutral, PublicKeyToken=c5687fc88969c44d");
            var result = factory.CreateConnection();
            result.ConnectionString = connString;
            return result;
        }
    }
}
