using System;
using System.Data.Common;
using System.Text;
using System.Text.RegularExpressions;

namespace Trolley.Providers
{
    public abstract class BaseOrmProvider : IOrmProvider
    {
        protected static Regex HasUnionRegex = new Regex(@"\bFROM\b[^()]*?(?>[^()]+|\((?<quote>)|\)(?<-quote>))*?[^()]*?(?(quote)(?!))\bUNION\b", RegexOptions.IgnoreCase | RegexOptions.Multiline);
        protected static Regex PagingCountRegex = new Regex(@"\bSELECT\b[^()]*?(?>[^()]+|\((?<quote>)|\)(?<-quote>))*?[^()]*?(?(quote)(?!))\bFROM\b", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);
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
        public virtual string GetPagingCountExpression(string sql)
        {
            if (HasUnionRegex.IsMatch(sql))
            {
                return "SELECT COUNT(*) FROM (" + sql + ") PT ";
            }
            else
            {
                return PagingCountRegex.Replace(sql, "SELECT COUNT(*) FROM ");
            }
        }
    }
}
