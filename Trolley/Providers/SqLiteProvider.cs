using System.Data.Common;

namespace Trolley.Providers
{
    public class SqLiteProvider : BaseOrmProvider
    {
        public SqLiteProvider()
        {
        }
        public override DbConnection CreateConnection(string connString)
        {
            var factory = OrmProviderFactory.SQLiteFactory();
            var result = factory.CreateConnection();
            result.ConnectionString = connString;
            return result;
        }
    }
}

