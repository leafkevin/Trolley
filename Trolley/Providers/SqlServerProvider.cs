using System;
using System.Data.Common;
using System.Text;

namespace Trolley.Providers
{
    public class SqlServerProvider : BaseOrmProvider
    {
        public override DbConnection CreateConnection(string connString)
        {
#if COREFX
            var assemblyQualifiedName ="System.Data.SqlClient.SqlClientFactory, System.Data.SqlClient, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
            var assemblyFile = "System.Data.SqlClient.dll";
#else
            var assemblyQualifiedName = "System.Data.SqlClient.SqlClientFactory, System.Data, Culture=neutral, PublicKeyToken=b77a5c561934e089";
            var assemblyFile = "System.Data.dll";
#endif
            var factory = OrmProviderFactory.GetFactory(assemblyQualifiedName, assemblyFile);
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
