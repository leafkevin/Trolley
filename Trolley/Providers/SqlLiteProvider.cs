using System.Data.Common;

namespace Trolley.Providers
{
    public class SqlLiteProvider : BaseOrmProvider
    {
        public SqlLiteProvider()
        {
        }
        public override DbConnection CreateConnection(string connString)
        {
            var factory = OrmProviderFactory.GetFactory("System.Data.SQLite.SQLiteFactory, System.Data.SQLite, Culture=neutral, PublicKeyToken=db937bc2d44ff139");
            var result = factory.CreateConnection();
            result.ConnectionString = connString;
            return result;
        }
    }
}

