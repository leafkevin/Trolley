using System.Text;
using System.Text.RegularExpressions;

namespace Trolley
{
    public class SqlBuilder
    {
        private SqlClauseBuilder sqlBuilder = null;
        public SqlBuilder()
        {
        }
        public SqlClauseBuilder RawSql(string sql = null)
        {
            return this.sqlBuilder = new SqlClauseBuilder(sql);
        }
        public string BuildSql()
        {
            return this.sqlBuilder.BuildSql();
        }
    }
    public class SqlClauseBuilder
    {
        private static Regex HasWhereRegex = new Regex(@"\bWHERE\b((?<quote>\()[^\(\)]*)*((?<-quote>\))[^\(\)]*)*(?(quote)(?!))[^\(\)]", RegexOptions.IgnoreCase | RegexOptions.Multiline);
        private static Regex HasCommaRegex = new Regex(@",\s*$", RegexOptions.IgnoreCase | RegexOptions.Multiline);
        private bool hasWhere = false;
        private bool hasComma = false;
        private StringBuilder sqlBuilder = new StringBuilder();
        public SqlClauseBuilder(string sql = null)
        {
            if (!string.IsNullOrEmpty(sql))
            {
                sql = sql.Trim();
                this.hasWhere = HasWhereRegex.IsMatch(sql);
                this.hasComma = HasCommaRegex.IsMatch(sql);
                this.sqlBuilder.Append(sql);
            }
        }
        public SqlClauseBuilder Where(bool condition, string trueClause, string falseClause = null)
        {
            if (!this.hasWhere)
            {
                if (HasWhereRegex.IsMatch(this.sqlBuilder.ToString())) this.hasWhere = true;
            }
            if (this.hasWhere) this.sqlBuilder.Append(" AND " + (condition ? trueClause : falseClause ?? ""));
            else { this.sqlBuilder.Append(" WHERE " + (condition ? trueClause : falseClause ?? "")); hasWhere = true; }
            return this;
        }
        public SqlClauseBuilder OrWhere(bool condition, string trueClause, string falseClause = null)
        {
            this.sqlBuilder.Append(" OR " + (condition ? trueClause : falseClause ?? ""));
            return this;
        }
        public SqlClauseBuilder AddField(bool condition, string trueClause, string falseClause = null)
        {
            if (!this.hasComma)
            {
                if (HasCommaRegex.IsMatch(this.sqlBuilder.ToString())) this.hasComma = true;
            }
            if (!this.hasComma) this.sqlBuilder.Append(",");
            this.sqlBuilder.Append(condition ? trueClause.Trim().TrimStart(',') : (string.IsNullOrEmpty(falseClause) ? "" : falseClause.Trim().TrimStart(',')));
            return this;
        }
        public SqlClauseBuilder AddSql(string clause)
        {
            clause = clause.Trim();
            this.sqlBuilder.Append(" " + clause);
            return this;
        }
        public string BuildSql()
        {
            return this.sqlBuilder.ToString();
        }
    }
}
