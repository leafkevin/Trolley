using System.Data.Common;

namespace Trolley.Providers
{
    public class NpgsqlProvider : BaseOrmProvider
    {
        public override bool IsMappingIgnoreCase { get { return true; } }
        public override DbConnection CreateConnection(string connString)
        {
            var factory = OrmProviderFactory.GetFactory("Npgsql.NpgsqlFactory, Npgsql, Culture=neutral, PublicKeyToken=5d8b90d52f46fda7");
            var result = factory.CreateConnection();
            result.ConnectionString = connString;
            return result;
        }
        public override string GetPropertyName(string propertyName)
        {
            if (this.IsMappingIgnoreCase) return propertyName;
            return "\"" + propertyName + "\"";
        }
    }
}

