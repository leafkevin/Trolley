using System.Text;
using System.Text.RegularExpressions;

namespace Trolley
{
    public class SqlBuilder
    {
        private static Regex HasWhereRegex = new Regex(@"\bWHERE\b((?<quote>\()[^\(\)]*)*((?<-quote>\))[^\(\)]*)*(?(quote)(?!))[^\(\)]", RegexOptions.IgnoreCase | RegexOptions.Multiline);
        private static Regex HasCommaRegex = new Regex(@",\s*$", RegexOptions.IgnoreCase | RegexOptions.Multiline);
        private bool hasWhere = false;
        private bool hasComma = false;
        private IOrmProvider provider = null;
        private StringBuilder sqlBuilder = new StringBuilder();

        public SqlBuilder()
        {
            this.provider = OrmProviderFactory.DefaultProvider;
        }
        public SqlBuilder(IOrmProvider provider)
        {
            this.provider = provider;
        }
        public SqlBuilder RawSql(string sql = null)
        {
            if (!string.IsNullOrEmpty(sql))
            {
                sql = sql.Trim();
                this.hasWhere = HasWhereRegex.IsMatch(sql);
                this.hasComma = HasCommaRegex.IsMatch(sql);
                this.sqlBuilder.Append(sql);
            }
            return this;
        }
        public SqlBuilder AndWhere(string clause)
        {
            if (!this.hasWhere)
            {
                if (HasWhereRegex.IsMatch(this.sqlBuilder.ToString())) this.hasWhere = true;
            }
            if (this.hasWhere) this.sqlBuilder.Append(" AND " + clause);
            else { this.sqlBuilder.Append(" WHERE " + clause); hasWhere = true; }
            return this;
        }
        public SqlBuilder AndWhere(bool condition, string trueClause, string falseClause = null)
        {
            if (!this.hasWhere)
            {
                if (HasWhereRegex.IsMatch(this.sqlBuilder.ToString())) this.hasWhere = true;
            }
            if (this.hasWhere) this.sqlBuilder.Append(condition ? " AND " + trueClause : falseClause ?? "");
            else { this.sqlBuilder.Append(condition ? " WHERE " + trueClause : falseClause ?? ""); hasWhere = true; }
            return this;
        }
        public SqlBuilder AndWithParenthesis(bool condition, string trueClause, string falseClause = null)
        {
            if (!this.hasWhere)
            {
                if (HasWhereRegex.IsMatch(this.sqlBuilder.ToString())) this.hasWhere = true;
            }
            if (this.hasWhere) this.sqlBuilder.Append(condition ? " AND (" + trueClause : falseClause ?? "");
            else { this.sqlBuilder.Append(condition ? " WHERE (" + trueClause : falseClause ?? ""); hasWhere = true; }
            return this;
        }
        public SqlBuilder AndWithParenthesis(string clause)
        {
            if (!this.hasWhere)
            {
                if (HasWhereRegex.IsMatch(this.sqlBuilder.ToString())) this.hasWhere = true;
            }
            if (this.hasWhere) this.sqlBuilder.Append(" AND (" + clause ?? "");
            else { this.sqlBuilder.Append(" WHERE (" + clause ?? ""); hasWhere = true; }
            return this;
        }
        public SqlBuilder EndParenthesis()
        {
            this.sqlBuilder.Append(")");
            return this;
        }
        public SqlBuilder OrWhere(string clause)
        {
            this.sqlBuilder.Append(" OR " + clause);
            return this;
        }
        public SqlBuilder OrWithParenthesis(string clause)
        {
            if (!this.hasWhere)
            {
                if (HasWhereRegex.IsMatch(this.sqlBuilder.ToString())) this.hasWhere = true;
            }
            if (this.hasWhere) this.sqlBuilder.Append(" OR (" + clause ?? "");
            else { this.sqlBuilder.Append(" WHERE (" + clause ?? ""); hasWhere = true; }
            return this;
        }
        public SqlBuilder OrWhere(bool condition, string trueClause, string falseClause = null)
        {
            this.sqlBuilder.Append(condition ? " OR " + trueClause : falseClause ?? "");
            return this;
        }
        public SqlBuilder AddField(string fieldClause)
        {
            if (!this.hasComma)
            {
                if (HasCommaRegex.IsMatch(this.sqlBuilder.ToString())) this.hasComma = true;
            }
            if (!this.hasComma) this.sqlBuilder.Append(",");
            this.sqlBuilder.Append(fieldClause.Trim().TrimStart(','));
            return this;
        }
        public SqlBuilder AddField(bool condition, string trueClause, string falseClause = null)
        {
            if (!this.hasComma)
            {
                if (HasCommaRegex.IsMatch(this.sqlBuilder.ToString())) this.hasComma = true;
            }
            if (!this.hasComma) this.sqlBuilder.Append(",");
            this.sqlBuilder.Append(condition ? trueClause.Trim().TrimStart(',') : (string.IsNullOrEmpty(falseClause) ? "" : falseClause.Trim().TrimStart(',')));
            return this;
        }
        public SqlBuilder AddSql(string clause)
        {
            clause = clause.Trim();
            this.sqlBuilder.Append(" " + clause);
            return this;
        }
        public SqlBuilder Paging(int pageIndex, int pageSize, string orderBy = null)
        {
            var sql = this.sqlBuilder.ToString();
            var pagingSql = this.provider.GetPagingExpression(sql, pageIndex * pageSize, pageSize, orderBy);
            this.sqlBuilder.Clear();
            this.sqlBuilder.Append(pagingSql);
            return this;
        }
        public SqlBuilder Clear()
        {
            this.sqlBuilder.Clear();
            return this;
        }
        public string BuildSql() => this.sqlBuilder.ToString();
    }
}
