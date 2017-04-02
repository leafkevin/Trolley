using System.Data.Common;

namespace Trolley.Providers
{
    public class MySqlProvider : BaseOrmProvider
    {
        public override DbConnection CreateConnection(string connString)
        {
            var factory = OrmProviderFactory.MySqlFactory();
            var result = factory.CreateConnection();
            result.ConnectionString = connString;
            return result;
        }
    }
}
