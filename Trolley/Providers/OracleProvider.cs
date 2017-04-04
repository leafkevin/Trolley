using System;
using System.Data.Common;
using System.Text.RegularExpressions;

namespace Trolley.Providers
{
    public class OracleProvider : BaseOrmProvider
    {
        private static Regex SelectRegex = new Regex(@"^\s*SELECT\s+", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);
        public override string ParamPrefix { get { return ":"; } }
        public override DbConnection CreateConnection(string connString)
        {
            //TODO:Oracle官方暂时还没有提供.NET Core版本驱动，无法实现跨平台
            var factory = OrmProviderFactory.GetFactory("Oracle.ManagedDataAccess.Client.OracleClientFactory, Oracle.ManagedDataAccess, Culture=neutral, PublicKeyToken=89b483f429c47342");
            var result = factory.CreateConnection();
            result.ConnectionString = connString;
            return result;
        }
        public override string GetPagingExpression(string sql, int skip, int? limit, string orderBy = null)
        {
            if (String.IsNullOrEmpty(orderBy)) throw new ArgumentNullException("orderBy");
            var pagingSql = String.Format("SELECT * FROM (SELECT ROW_NUMBER() OVER({0}) _PagingRowIndex,", orderBy);
            sql = SelectRegex.Replace(sql, pagingSql);
            sql += ") PagingList WHERE _PagingRowIndex>" + skip.ToString();
            sql += limit.HasValue ? " AND _PagingRowIndex<" + (skip + limit.Value).ToString() : "";
            return sql;
        }
    }
}
