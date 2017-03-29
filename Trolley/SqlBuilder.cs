using System.Text;
using System.Text.RegularExpressions;

namespace Trolley
{
    public class SqlBuilder
    {
        private IOrmProvider provider = null;
        private SqlClauseBuilder sqlBuilder = null;
        public SqlBuilder(IOrmProvider provider)
        {
            this.provider = provider;
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
        private static Regex HasWhereRegex = new Regex(@"\s+WHERE\s+((?<quote>\))[^\(\)]*)*((?<-quote>\()[^\(\)]*)*(?(quote)(?!))\s*$", RegexOptions.IgnoreCase | RegexOptions.Multiline);
        private bool hasWhere = false;
        private StringBuilder sqlBuilder = new StringBuilder();
        public SqlClauseBuilder(string sql = null)
        {
            if (!string.IsNullOrEmpty(sql))
            {
                hasWhere = HasWhereRegex.IsMatch(sql);
                this.sqlBuilder.Append(sql);
            }
        }
        public SqlClauseBuilder Where(bool condition, string trueClause, string falseClause = null)
        {
            if (hasWhere) this.sqlBuilder.Append(" AND " + (condition ? trueClause : falseClause ?? ""));
            else { this.sqlBuilder.Append(" WHERE " + (condition ? trueClause : falseClause ?? "")); hasWhere = true; }
            return this;
        }
        public SqlClauseBuilder OrWhere(bool condition, string trueClause, string falseClause = null)
        {
            if (hasWhere) this.sqlBuilder.Append(" OR " + (condition ? trueClause : falseClause ?? ""));
            else { this.sqlBuilder.Append(" WHERE " + (condition ? trueClause : falseClause ?? "")); hasWhere = true; }
            return this;
        }
        public SqlClauseBuilder AddField(bool condition, string trueClause, string falseClause = null)
        {
            this.sqlBuilder.Append(condition ? trueClause : falseClause ?? "");
            return this;
        }
        public SqlClauseBuilder AddSql(string clause)
        {
            sqlBuilder.Append(" " + clause);
            return this;
        }
        public string BuildSql()
        {
            return this.sqlBuilder.ToString();
        }
    }
}
