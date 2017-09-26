using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Trolley
{
    public class SqlBuilder
    {
        private static Regex HasWhereRegex = new Regex(@"\bWHERE\b((?<quote>\()[^\(\)]*)*((?<-quote>\))[^\(\)]*)*(?(quote)(?!))[^\(\)]", RegexOptions.IgnoreCase | RegexOptions.Multiline);
        private static Regex HasCommaRegex = new Regex(@",\s*$", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.RightToLeft);
        private bool hasWhere = false;
        private bool hasComma = false;
        private IOrmProvider provider = null;
        private StringBuilder sqlBuilder = new StringBuilder();
        private Dictionary<string, object> parameters = new Dictionary<string, object>();
        private List<object> entityParameters = new List<object>();
        internal Dictionary<string, object> Parameters => this.parameters;
        internal List<object> EntityParameters => this.entityParameters;
        public SqlBuilder()
        {
            this.provider = OrmProviderFactory.DefaultProvider;
        }
        public SqlBuilder(IOrmProvider provider)
        {
            this.provider = provider;
        }
        public SqlBuilder RawSql(string sql) => this.RawSql(sql, null);
        public SqlBuilder RawSql(string sql, params object[] parameters)
        {
            if (String.IsNullOrEmpty(sql)) throw new ArgumentNullException("sql", "sql不能为空字符串！");
            this.sqlBuilder.Append(sql);
            this.AddParameters(sql, parameters);
            return this;
        }
        public SqlBuilder AndWhere(string clause, params object[] parameters) => this.AndWhere(true, clause, parameters);
        public SqlBuilder AndWhere(bool condition, string clause, params object[] parameters)
        {
            if (condition)
            {
                if (String.IsNullOrEmpty(clause)) throw new ArgumentNullException("clause", "clause不能为空字符串！");
                if (!this.hasWhere)
                {
                    if (HasWhereRegex.IsMatch(this.sqlBuilder.ToString())) this.hasWhere = true;
                }
                if (this.hasWhere) this.sqlBuilder.Append(" AND " + clause);
                else { this.sqlBuilder.Append(" WHERE " + clause); hasWhere = true; }
                this.AddParameters(clause, parameters);
            }
            return this;
        }
        public SqlBuilder AndWithParenthesis(string clause, params object[] parameters) => this.AndWithParenthesis(true, clause, parameters);
        public SqlBuilder AndWithParenthesis(bool condition, string clause, params object[] parameters)
        {
            if (condition)
            {
                if (String.IsNullOrEmpty(clause)) throw new ArgumentNullException("clause", "clause不能为空字符串！");
                if (!this.hasWhere)
                {
                    if (HasWhereRegex.IsMatch(this.sqlBuilder.ToString())) this.hasWhere = true;
                }
                if (this.hasWhere) this.sqlBuilder.Append(" AND (" + clause);
                else { this.sqlBuilder.Append(" WHERE (" + clause); hasWhere = true; }
                this.AddParameters(clause, parameters);
            }
            return this;
        }
        public SqlBuilder EndParenthesis()
        {
            this.sqlBuilder.Append(")");
            return this;
        }
        public SqlBuilder OrWhere(string clause, params object[] parameters) => this.OrWhere(true, clause, parameters);
        public SqlBuilder OrWhere(bool condition, string clause, params object[] parameters)
        {
            if (condition)
            {
                if (String.IsNullOrEmpty(clause)) throw new ArgumentNullException("clause", "clause不能为空字符串！");
                this.sqlBuilder.Append(" OR " + clause);
                this.AddParameters(clause, parameters);
            }
            return this;
        }
        public SqlBuilder OrWithParenthesis(string clause, params object[] parameters) => this.OrWithParenthesis(true, clause, parameters);
        public SqlBuilder OrWithParenthesis(bool condition, string clause, params object[] parameters)
        {
            if (condition)
            {
                if (String.IsNullOrEmpty(clause)) throw new ArgumentNullException("clause", "clause不能为空字符串！");
                if (!this.hasWhere)
                {
                    if (HasWhereRegex.IsMatch(this.sqlBuilder.ToString())) this.hasWhere = true;
                }
                if (this.hasWhere) this.sqlBuilder.Append(" OR (" + clause);
                else { this.sqlBuilder.Append(" WHERE (" + clause); hasWhere = true; }
                this.AddParameters(clause, parameters);
            }
            return this;
        }
        public SqlBuilder AddField(string field, params object[] parameters) => this.AddField(true, field, parameters);
        public SqlBuilder AddField(bool condition, string field, params object[] parameters)
        {
            if (condition)
            {
                if (String.IsNullOrEmpty(field)) throw new ArgumentNullException("field", "field不能为空字符串！");
                if (!this.hasComma)
                {
                    if (HasCommaRegex.IsMatch(this.sqlBuilder.ToString())) this.hasComma = true;
                }
                if (!this.hasComma) this.sqlBuilder.Append(",");
                this.sqlBuilder.Append(field);
                this.AddParameters(field, parameters);
            }
            return this;
        }
        public SqlBuilder AddParameter(string name, object value)
        {
            if (this.parameters.ContainsKey(name)) return this;
            this.parameters.Add(name, value);
            return this;
        }
        public string PagingTotal(int pageIndex, int pageSize, string orderBy = null)
        {
            var sql = this.sqlBuilder.ToString();
            return this.provider.GetPagingCountExpression(sql);
        }
        public string Paging(int pageIndex, int pageSize, string orderBy = null)
        {
            var sql = this.sqlBuilder.ToString();
            return this.provider.GetPagingExpression(sql, pageIndex * pageSize, pageSize, orderBy);
        }
        public SqlBuilder Clear()
        {
            this.sqlBuilder.Clear();
            return this;
        }
        public string BuildSql() => this.sqlBuilder.ToString();
        private void AddParameters(string clause, params object[] parameters)
        {
            if (parameters != null && parameters.Length > 0)
            {
                var matches = Regex.Matches(clause, @"[?@:][a-z0-9_]+(?=[^a-z0-9_]|$)", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.CultureInvariant);
                var index = 0;
                for (int i = 0; i < matches.Count; i++)
                {
                    if (this.Parameters.ContainsKey(matches[i].Value)) continue;
                    this.Parameters.Add(matches[i].Value, parameters[index]);
                    index++;
                }
            }
        }
    }
}
