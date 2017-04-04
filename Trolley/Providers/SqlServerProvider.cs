using System;
using System.Data.Common;
using System.Text;

namespace Trolley.Providers
{
    public class SqlServerProvider : BaseOrmProvider
    {
        public override DbConnection CreateConnection(string connString)
        {
            var factory =
#if COREFX
              OrmProviderFactory.GetFactory("System.Data.SqlClient.SqlClientFactory, System.Data.SqlClient, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
#else
              OrmProviderFactory.GetFactory("System.Data.SqlClient.SqlClientFactory, System.Data, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
#endif
            var result = factory.CreateConnection();
            result.ConnectionString = connString;
            return result;
        }
        public override string GetPropertyName(string propertyName)
        {
            return "[" + propertyName + "]";
        }
        public override string GetTableName(string entityName)
        {
            return "[" + entityName + "]";
        }
        public override string GetColumnName(string propertyName)
        {
            return "[" + propertyName + "]";
        }
        public override string GetPagingExpression(string sql, int skip, int? limit, string orderBy = null)
        {
            if (String.IsNullOrEmpty(orderBy)) throw new ArgumentNullException("orderBy");
            StringBuilder buidler = new StringBuilder();
            buidler.Append(this.GetPagingSql(sql));
            if (!String.IsNullOrEmpty(orderBy)) buidler.Append(" " + orderBy);
            buidler.AppendFormat(" OFFSET {0} ROWS", skip);
            if (limit.HasValue) buidler.AppendFormat(" FETCH NEXT {0} ROWS ONLY", limit);
            return buidler.ToString();
        }
    }
}
