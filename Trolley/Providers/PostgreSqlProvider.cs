using System.Data.Common;

namespace Trolley.Providers
{
    public class PostgreSqlProvider : BaseOrmProvider
    {
        public override DbConnection CreateConnection(string connString)
        {
            var factory = OrmProviderFactory.PostgreSqlFactory();
            var result = factory.CreateConnection();
            result.ConnectionString = connString;
            return result;
        }
    }
}

