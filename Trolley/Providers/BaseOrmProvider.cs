using System;
using System.Data.Common;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Trolley.Providers
{
    public abstract class BaseOrmProvider : IOrmProvider
    {
        private static Regex HasUnionRegex = new Regex(@"FROM\s+((?<quote>\()[^\(\)]*)+((?<-quote>\))[^\(\)]*)+(?(quote)(?!))\s+UNION", RegexOptions.IgnoreCase | RegexOptions.Multiline);
        public virtual string ParamPrefix { get { return "@"; } }
        public virtual bool IsMappingIgnoreCase { get { return false; } }
        public abstract DbConnection CreateConnection(string ConnString);
        public virtual string GetPropertyName(string propertyName)
        {
            return propertyName;
        }
        public virtual string GetTableName(string entityName)
        {
            return entityName;
        }
        public virtual string GetColumnName(string propertyName)
        {
            return propertyName;
        }
        public virtual string GetPagingExpression(string sql, int skip, int? limit, string orderBy = null)
        {
            StringBuilder buidler = new StringBuilder();
            buidler.Append(this.GetPagingSql(sql));
            if (!String.IsNullOrEmpty(orderBy)) buidler.Append(" " + orderBy);
            if (limit.HasValue) buidler.AppendFormat(" LIMIT {0}", limit);
            buidler.AppendFormat(" OFFSET {0}", skip);
            return buidler.ToString();
        }
        protected string GetPagingSql(string sql)
        {
            return HasUnionRegex.IsMatch(sql) ? "SELECT * FROM (" + sql + ") PagingList" : sql;
        }
    }
}
