using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace Trolley
{
    [DebuggerDisplay("SQL:{BuildSql()},Parameters:[Count={Parameters.Count}]")]
    [DebuggerTypeProxy(typeof(SqlBuilder.DebugView))]
    public class SqlBuilder
    {
        private static Regex HasWhereRegex = new Regex(@"\bWHERE\b((?<quote>\()[^\(\)]*)*((?<-quote>\))[^\(\)]*)*(?(quote)(?!))[^\(\)]", RegexOptions.IgnoreCase | RegexOptions.Multiline);
        private static Regex HasCommaRegex = new Regex(@",\s*$", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.RightToLeft);
        private bool hasWhere = false;
        private bool hasComma = false;
        private IOrmProvider provider = null;
        private StringBuilder sqlBuilder = new StringBuilder();
        private Dictionary<string, object> parameters = new Dictionary<string, object>();
        internal Dictionary<string, object> Parameters => this.parameters;
        /// <summary>
        /// 用默认的Provider生成SqlBuilder对象
        /// </summary>
        public SqlBuilder() => this.provider = OrmProviderFactory.DefaultProvider;
        /// <summary>
        /// 使用指定的Provider生成SqlBuilder对象
        /// </summary>
        /// <param name="provider">Provider对象</param>
        public SqlBuilder(IOrmProvider provider) => this.provider = provider;
        /// <summary>
        /// 追加sql语句
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public SqlBuilder RawSql(string sql) => this.RawSql(sql, null);
        /// <summary>
        /// 追加sql语句。
        /// 如果有参数，则按照先后顺序把sql中用到的参数追加到sql命令的参数列表中。
        /// </summary>
        /// <param name="sql">前面生成的参数</param>
        /// <param name="parameters">sql中使用的参数值列表，顺序按照sql中出现的顺序</param>
        /// <returns>返回SqlBuilder对象</returns>
        public SqlBuilder RawSql(string sql, params object[] parameters)
        {
            if (String.IsNullOrEmpty(sql)) throw new ArgumentNullException("sql", "sql不能为空字符串！");
            if (this.sqlBuilder.Length > 0) this.sqlBuilder.Append(" ");
            this.sqlBuilder.Append(sql.TrimEnd());
            this.AddParameters(sql, parameters);
            return this;
        }
        /// <summary>
        /// 追加AND子句。
        /// 如果有参数，则按照先后顺序把sql中用到的参数追加到sql命令的参数列表中。
        /// </summary>
        /// <param name="clause">前面生成的参数</param>
        /// <param name="parameters">sql中使用的参数值列表，顺序按照sql中出现的顺序</param>
        /// <returns>返回SqlBuilder对象</returns>
        public SqlBuilder AndWhere(string clause, params object[] parameters) => this.AndWhere(true, clause, parameters);
        /// <summary>
        /// 条件condition为true，追加AND子句，为false不追加AND子句。
        /// 如果有参数，则按照先后顺序把sql中用到的参数追加到sql命令的参数列表中。
        /// </summary>
        /// <param name="condition">条件</param>
        /// <param name="clause">前面生成的参数</param>
        /// <param name="parameters">sql中使用的参数值列表，顺序按照sql中出现的顺序</param>
        /// <returns>返回SqlBuilder对象</returns>
        public SqlBuilder AndWhere(bool condition, string clause, params object[] parameters)
        {
            if (condition)
            {
                if (String.IsNullOrEmpty(clause)) throw new ArgumentNullException("clause", "clause不能为空字符串！");
                var sql = this.sqlBuilder.ToString();
                if (!this.hasWhere) { if (HasWhereRegex.IsMatch(sql)) this.hasWhere = true; }
                else if (sql.LastIndexOf(";") > sql.ToUpper().LastIndexOf("WHERE")) this.hasWhere = false;
                if (this.hasWhere) this.sqlBuilder.Append(" AND " + clause);
                else { this.sqlBuilder.Append(" WHERE " + clause); hasWhere = true; }
                this.AddParameters(clause, parameters);
            }
            return this;
        }
        /// <summary>
        /// 追加以(起始的AND子句。如： AND (...
        /// 如果有参数，则按照先后顺序把sql中用到的参数追加到sql命令的参数列表中。
        /// </summary>
        /// <param name="clause">前面生成的参数</param>
        /// <param name="parameters">sql中使用的参数值列表，顺序按照sql中出现的顺序</param>
        /// <returns>返回SqlBuilder对象</returns>
        public SqlBuilder AndWithParenthesis(string clause, params object[] parameters) => this.AndWithParenthesis(true, clause, parameters);
        /// <summary>
        /// 条件condition为true，追加以(起始的AND子句，为false不追加。如： AND (...。
        /// 如果有参数，则按照先后顺序把sql中用到的参数追加到sql命令的参数列表中。
        /// </summary>
        /// <param name="condition">条件</param>
        /// <param name="clause">前面生成的参数</param>
        /// <param name="parameters">sql中使用的参数值列表，顺序按照sql中出现的顺序</param>
        /// <returns>返回SqlBuilder对象</returns>
        public SqlBuilder AndWithParenthesis(bool condition, string clause, params object[] parameters)
        {
            if (condition)
            {
                if (String.IsNullOrEmpty(clause)) throw new ArgumentNullException("clause", "clause不能为空字符串！");
                var sql = this.sqlBuilder.ToString();
                if (!this.hasWhere) { if (HasWhereRegex.IsMatch(sql)) this.hasWhere = true; }
                else if (sql.LastIndexOf(";") > sql.ToUpper().LastIndexOf("WHERE")) this.hasWhere = false;
                if (this.hasWhere) this.sqlBuilder.Append(" AND (" + clause);
                else { this.sqlBuilder.Append(" WHERE (" + clause); hasWhere = true; }
                this.AddParameters(clause, parameters);
            }
            return this;
        }
        /// <summary>
        /// 追加)
        /// </summary>
        /// <returns>返回SqlBuilder对象</returns>
        public SqlBuilder EndParenthesis()
        {
            this.sqlBuilder.Append(")");
            return this;
        }
        /// <summary>
        /// 追加OR子句。如果有参数，则按照先后顺序把sql中用到的参数追加到sql命令的参数列表中。
        /// </summary>
        /// <param name="clause">前面生成的参数</param>
        /// <param name="parameters">sql中使用的参数值列表，顺序按照sql中出现的顺序</param>
        /// <returns>返回SqlBuilder对象</returns>
        public SqlBuilder OrWhere(string clause, params object[] parameters) => this.OrWhere(true, clause, parameters);
        /// <summary>
        /// 条件condition为true，追加OR子句，为false不追加OR子句。
        /// 如果有参数，则按照先后顺序把sql中用到的参数追加到sql命令的参数列表中。
        /// </summary>
        /// <param name="clause">前面生成的参数</param>
        /// <param name="parameters">sql中使用的参数值列表，顺序按照sql中出现的顺序</param>
        /// <returns>返回SqlBuilder对象</returns>
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
        /// <summary>
        /// 追加以(起始的OR子句。如： OR (...。
        /// 如果有参数，则按照先后顺序把sql中用到的参数追加到sql命令的参数列表中。
        /// </summary>
        /// <param name="clause">前面生成的参数</param>
        /// <param name="parameters">sql中使用的参数值列表，顺序按照sql中出现的顺序</param>
        /// <returns>返回SqlBuilder对象</returns>
        public SqlBuilder OrWithParenthesis(string clause, params object[] parameters) => this.OrWithParenthesis(true, clause, parameters);
        /// <summary>
        /// 条件condition为true，追加以(起始的OR子句，为false不追加子句。如： OR (...。
        /// 如果有参数，则按照先后顺序把sql中用到的参数追加到sql命令的参数列表中。
        /// </summary>
        /// <param name="condition">条件</param>
        /// <param name="clause">前面生成的参数</param>
        /// <param name="parameters">sql中使用的参数值列表，顺序按照sql中出现的顺序</param>
        /// <returns>返回SqlBuilder对象</returns>
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
        /// <summary>
        /// 追加字段名或是更新用字段名赋值子句。如："Column1","Column1,Column2","Column1=@value1"
        /// </summary>
        /// <param name="field">一个或多个列名称，多个以,分隔</param>
        /// <param name="parameters">sql中使用的参数值列表，顺序按照sql中出现的顺序</param>
        /// <returns>返回SqlBuilder对象</returns>
        public SqlBuilder AddField(string field, params object[] parameters) => this.AddField(true, field, parameters);
        /// <summary>
        /// 条件condition为true，追加字段名或是更新用字段名赋值子句，为false不追加。如："Column1","Column1,Column2","Column1=@value1"
        /// </summary>
        /// <param name="condition">条件</param>
        /// <param name="field">一个或多个列名称，多个以,分隔</param>
        /// <param name="parameters">sql中使用的参数值列表，顺序按照sql中出现的顺序</param>
        /// <returns>返回SqlBuilder对象</returns>
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
        /// <summary>
        /// 追加sql参数值
        /// </summary>
        /// <param name="name">参数名，包含@:?</param>
        /// <param name="value">参数值</param>
        /// <returns>返回SqlBuilder对象</returns>
        public SqlBuilder AddParameter(string name, object value)
        {
            if (this.parameters.ContainsKey(name)) return this;
            this.parameters.Add(name, value);
            return this;
        }
        /// <summary>
        /// 根据前面生成的sql，进行生成分页的总条数语句。
        /// </summary>
        /// <param name="pageIndex">返回数据的页索引，从0开始</param>
        /// <param name="pageSize">每页的条数</param>
        /// <param name="orderBy">排序语句</param>
        /// <returns>返回生成的分页语句</returns>
        public string PagingTotal(int pageIndex, int pageSize, string orderBy = null)
        {
            var sql = this.sqlBuilder.ToString();
            return this.provider.GetPagingCountExpression(sql);
        }
        /// <summary>
        /// 根据前面生成的sql，进行生成分页语句，不包含分页的总条数语句。
        /// </summary>
        /// <param name="pageIndex">返回数据的页索引，从0开始</param>
        /// <param name="pageSize">每页的条数</param>
        /// <param name="orderBy">排序语句</param>
        /// <returns>返回生成的分页语句</returns>
        public string Paging(int pageIndex, int pageSize, string orderBy = null)
        {
            var sql = this.sqlBuilder.ToString();
            return this.provider.GetPagingExpression(sql, pageIndex * pageSize, pageSize, orderBy);
        }
        /// <summary>
        /// 清空前面生成sql
        /// </summary>
        /// <returns>返回生成的分页语句</returns>
        public SqlBuilder Clear()
        {
            this.sqlBuilder.Clear();
            return this;
        }
        /// <summary>
        /// 生成sql并返回
        /// </summary>
        /// <returns>返回生成的sql</returns>
        public string BuildSql() => this.sqlBuilder.ToString();
        private void AddParameters(string clause, params object[] parameters)
        {
            if (parameters != null && parameters.Length > 0)
            {
                var matches = Regex.Matches(clause, @"[?@:][a-z0-9_]+(?=[^a-z0-9_]|$)", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.CultureInvariant);
                var index = 0;
                for (int i = 0; i < matches.Count; i++)
                {
                    if (i >= parameters.Length) break;
                    if (this.Parameters.ContainsKey(matches[i].Value)) continue;
                    this.Parameters.Add(matches[i].Value, parameters[index]);
                    index++;
                }
            }
        }
        sealed class DebugView
        {
            private readonly SqlBuilder builder;
            public DebugView(SqlBuilder builder) => this.builder = builder;
            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public string SQL => this.builder.sqlBuilder.ToString();
            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public Dictionary<string, object> Parameters => this.builder.Parameters;
        }
    }
}
