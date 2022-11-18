//using System;
//using System.Collections.Generic;
//using System.Data;
//using System.Data.Common;
//using System.Linq.Expressions;
//using System.Threading;
//using System.Threading.Tasks;

//namespace Trolley;

//class SqlExpression<T>
//{
//    private readonly IOrmDbFactory dbFactory;
//    private readonly TheaConnection connection;
//    private readonly SqlExpressionVisitor visitor;
//    private bool hasSelect;

//    public SqlExpression(IOrmDbFactory dbFactory, TheaConnection connection, SqlExpressionVisitor visitor)
//    {
//        this.dbFactory = dbFactory;
//        this.connection = connection;
//        this.visitor = visitor;
//    }

//    public bool Exists(Expression<Func<T, bool>> predicate)
//    {
//        if (!this.hasSelect)
//            this.visitor.Select("COUNT(*)");
//        this.hasSelect = true;
//        var sql = this.visitor.BuildSql();
//        var command = this.connection.CreateCommand();
//        command.CommandText = sql;
//        command.CommandType = CommandType.Text;
//        if (this.visitor.Parameters != null && this.visitor.Parameters.Count > 0)
//            this.visitor.Parameters.ForEach(f => command.Parameters.Add(f));
//        this.connection.Open();
//        var result = command.ExecuteScalar();
//        return (long)result > 0;
//    }
//    public Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
//    {
//        throw new NotImplementedException();
//    }
//    /// <summary>
//    /// 导航属性的InnerJoin
//    /// </summary>
//    /// <param name="predicate"></param>
//    /// <returns></returns>
//    public ISqlExpression<T> InnerJoin(Expression<Func<T, bool>> predicate)
//    {
//        this.visitor.Join(typeof(T), predicate.Body, "INNER JOIN");
//        return this;
//    }
//    public ISqlExpression<T> LeftJoin(Expression<Func<T, bool>> predicate)
//    {
//        this.visitor.Join(typeof(T), predicate.Body, "LEFT JOIN");
//        return this;
//    }
//    public ISqlExpression<T> RightJoin(Expression<Func<T, bool>> predicate)
//    {
//        this.visitor.Join(typeof(T), predicate, "RIGHT JOIN");
//        return this;
//    }
//    public ISqlExpression<T> Include<TTarget>(Expression<Func<T, TTarget>> memberSelector)
//    {
//        this.visitor.Include(memberSelector);
//        return this;
//    }
//    public ISqlExpression<T> IncludeIf<TTarget>(bool condition, Expression<Func<T, TTarget>> memberSelector)
//    {
//        if (condition) this.visitor.Include(memberSelector);
//        return this;
//    }
//    public ISqlExpression<TTarget> Select<TTarget>(Expression<Func<T, TTarget>> fieldsExpr)
//    {
//        this.hasSelect = true;
//        this.visitor.Select(fieldsExpr.Body);
//        return new SqlExpression<TTarget>(this.dbFactory, this.connection, this.visitor);
//    }
//    public IGroupingQuery<TTarget> GroupBy<TTarget>(Expression<Func<T, TTarget>> fieldsExpr)
//    {
//        this.visitor.GroupBy(fieldsExpr.Body);
//        return new GroupBySqlExpression<TTarget>(this.visitor);
//    }
//    public ISqlExpression<T> OrderBy<TTarget>(Expression<Func<T, TTarget>> fieldsExpr)
//    {
//        throw new NotImplementedException();
//    }
//    public ISqlExpression<T> OrderByDescending<TTarget>(Expression<Func<T, TTarget>> fieldsExpr)
//    {
//        throw new NotImplementedException();
//    }
//    public int Count()
//    {
//        if (!this.hasSelect)
//            this.visitor.Select("COUNT(*)");
//        this.hasSelect = true;
//        var sql = this.visitor.BuildSql();
//        var command = this.connection.CreateCommand();
//        command.CommandText = sql;
//        command.CommandType = CommandType.Text;
//        if (this.visitor.Parameters != null && this.visitor.Parameters.Count > 0)
//            this.visitor.Parameters.ForEach(f => command.Parameters.Add(f));
//        this.connection.Open();
//        var result = command.ExecuteScalar();
//        return (int)result;
//    }
//    public long LongCount()
//    {
//        if (!this.hasSelect)
//            this.visitor.Select("COUNT(*)");
//        this.hasSelect = true;
//        var sql = this.visitor.BuildSql();
//        var command = this.connection.CreateCommand();
//        command.CommandText = sql;
//        command.CommandType = CommandType.Text;
//        if (this.visitor.Parameters != null && this.visitor.Parameters.Count > 0)
//            this.visitor.Parameters.ForEach(f => command.Parameters.Add(f));
//        this.connection.Open();
//        var result = command.ExecuteScalar();
//        return (long)result;
//    }
//    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
//    {
//        if (!this.hasSelect)
//            this.visitor.Select("COUNT(*)");
//        this.hasSelect = true;
//        var sql = this.visitor.BuildSql();
//        var cmd = this.connection.CreateCommand();
//        if (cmd is not DbCommand command)
//            throw new Exception("当前数据库驱动不支持异步SQL查询");
//        command.CommandText = sql;
//        command.CommandType = CommandType.Text;
//        if (this.visitor.Parameters != null && this.visitor.Parameters.Count > 0)
//            this.visitor.Parameters.ForEach(f => command.Parameters.Add(f));
//        await this.connection.OpenAsync(cancellationToken);
//        var result = await command.ExecuteScalarAsync(cancellationToken);
//        return (int)result;
//    }
//    public async Task<long> LongCountAsync(CancellationToken cancellationToken = default)
//    {
//        var sql = this.visitor.BuildSql("COUNT(*)");
//        var cmd = this.connection.CreateCommand();
//        if (cmd is not DbCommand command)
//            throw new Exception("当前数据库驱动不支持异步SQL查询");
//        command.CommandText = sql;
//        command.CommandType = CommandType.Text;
//        if (this.visitor.Parameters != null && this.visitor.Parameters.Count > 0)
//            this.visitor.Parameters.ForEach(f => command.Parameters.Add(f));
//        await this.connection.OpenAsync(cancellationToken);
//        var result = await command.ExecuteScalarAsync(cancellationToken);
//        return (long)result;
//    }
//    public T First()
//    {
//        if (!this.hasSelect)
//        {
//            Expression<Func<T, T>> defaultExpr = f => f;
//            this.visitor.Select(defaultExpr);
//        }
//        this.hasSelect = true;
//        var sql = this.visitor.BuildSql();
//        var command = this.connection.CreateCommand();
//        command.CommandText = sql;
//        command.CommandType = CommandType.Text;
//        if (this.visitor.Parameters != null && this.visitor.Parameters.Count > 0)
//            this.visitor.Parameters.ForEach(f => command.Parameters.Add(f));
//        T result = default;
//        this.connection.Open();
//        var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
//        var reader = command.ExecuteReader(behavior);
//        while (reader.Read())
//        {
//            result = reader.To<T>(this.connection, this.visitor.ReaderFields);
//        }
//        while (reader.NextResult()) { }
//        reader.Close();
//        reader.Dispose();
//        return result;
//    }
//    public async Task<T> FirstAsync(CancellationToken cancellationToken = default)
//    {
//        if (!this.hasSelect)
//        {
//            Expression<Func<T, T>> defaultExpr = f => f;
//            this.visitor.Select(defaultExpr);
//        }
//        this.hasSelect = true;
//        var sql = this.visitor.BuildSql();
//        var cmd = this.connection.CreateCommand();
//        if (cmd is not DbCommand command)
//            throw new Exception("当前数据库驱动不支持异步SQL查询");

//        command.CommandText = sql;
//        command.CommandType = CommandType.Text;
//        if (this.visitor.Parameters != null && this.visitor.Parameters.Count > 0)
//            this.visitor.Parameters.ForEach(f => command.Parameters.Add(f));
//        T result = default;
//        var behavior = CommandBehavior.SequentialAccess | CommandBehavior.SingleResult | CommandBehavior.SingleRow;
//        await this.connection.OpenAsync(cancellationToken);
//        var reader = await command.ExecuteReaderAsync(behavior, cancellationToken);
//        while (await reader.ReadAsync(cancellationToken))
//        {
//            result = reader.To<T>(this.connection, this.visitor.ReaderFields);
//        }
//        while (await reader.NextResultAsync(cancellationToken)) { }
//        await reader.CloseAsync();
//        await reader.DisposeAsync();
//        return result;
//    }
//    public List<T> ToList()
//    {
//        if (!this.hasSelect)
//        {
//            Expression<Func<T, T>> defaultExpr = f => f;
//            this.visitor.Select(defaultExpr);
//        }
//        this.hasSelect = true;
//        var sql = this.visitor.BuildSql();
//        var command = this.connection.CreateCommand();
//        command.CommandText = sql;
//        command.CommandType = CommandType.Text;
//        if (this.visitor.Parameters != null && this.visitor.Parameters.Count > 0)
//            this.visitor.Parameters.ForEach(f => command.Parameters.Add(f));
//        var result = new List<T>();
//        this.connection.Open();
//        var reader = command.ExecuteReader(CommandBehavior.SequentialAccess);
//        while (reader.Read())
//        {
//            result.Add(reader.To<T>(this.connection, this.visitor.ReaderFields));
//        }
//        while (reader.NextResult()) { }
//        reader.Close();
//        reader.Dispose();
//        return result;
//    }
//    public async Task<List<T>> ToListAsync(CancellationToken cancellationToken = default)
//    {
//        if (!this.hasSelect)
//        {
//            Expression<Func<T, T>> defaultExpr = f => f;
//            this.visitor.Select(defaultExpr);
//        }
//        this.hasSelect = true;
//        var sql = this.visitor.BuildSql();
//        var cmd = this.connection.CreateCommand();
//        if (cmd is not DbCommand command)
//            throw new Exception("当前数据库驱动不支持异步SQL查询");

//        command.CommandText = sql;
//        command.CommandType = CommandType.Text;
//        if (this.visitor.Parameters != null && this.visitor.Parameters.Count > 0)
//            this.visitor.Parameters.ForEach(f => command.Parameters.Add(f));
//        var result = new List<T>();
//        var behavior = CommandBehavior.SequentialAccess;
//        await this.connection.OpenAsync(cancellationToken);
//        var reader = await command.ExecuteReaderAsync(behavior, cancellationToken);
//        while (await reader.ReadAsync(cancellationToken))
//        {
//            result.Add(reader.To<T>(this.connection, this.visitor.ReaderFields));
//        }
//        while (await reader.NextResultAsync(cancellationToken)) { }
//        await reader.CloseAsync();
//        await reader.DisposeAsync();
//        return result;
//    }
//    public Dictionary<TKey, TElement> ToDictionary<TKey, TElement>(Func<T, TKey> keySelector, Func<T, TElement> valueSelector)
//    {
//        this.hasSelect = true;
//        throw new NotImplementedException();
//    }
//    public Task<Dictionary<TKey, TElement>> ToDictionaryAsync<TKey, TElement>(Func<T, TKey> keySelector, Func<T, TElement> valueSelector, CancellationToken cancellationToken = default)
//    {
//        this.hasSelect = true;
//        throw new NotImplementedException();
//    }
//    public List<T> ToPageList(int pageIndex, int pageSize)
//    {
//        if (!this.hasSelect)
//        {
//            Expression<Func<T, T>> defaultExpr = f => f;
//            this.visitor.Select(defaultExpr);
//        }
//        this.hasSelect = true;
//        var sql = this.visitor.BuildSql();



//        if (pageIndex >= 1) pageIndex = pageIndex - 1;
//        var skip = pageIndex * pageSize;
//        //SQL TEMPLATE:SELECT /**fields**/ FROM /**tables**/ WHERE /**conditions**/");
//        var pageSql = this.connection.OrmProvider.GetPagingTemplate(skip, pageSize);
//        pageSql = pageSql.Replace("/**fields**/", this.selectSql);
//        pageSql = pageSql.Replace("/**tables**/", tableSql);
//        pageSql = pageSql.Replace("WHERE /**conditions**/", string.Empty);

//        var sql = $"SELECT COUNT(*) FROM {tableSql};{pageSql}";
//        if (!string.IsNullOrEmpty(command.CommandText))
//            sql = command.CommandText + ";" + sql;
//        command.CommandText = sql;
//        command.CommandType = CommandType.Text;




//        var command = this.connection.CreateCommand();
//        command.CommandText = sql;
//        command.CommandType = CommandType.Text;
//        if (this.visitor.Parameters != null && this.visitor.Parameters.Count > 0)
//            this.visitor.Parameters.ForEach(f => command.Parameters.Add(f));
//        var result = new List<T>();
//        this.connection.Open();
//        var reader = command.ExecuteReader(CommandBehavior.SequentialAccess);
//        while (reader.Read())
//        {
//            result.Add(reader.To<T>(this.connection, this.visitor.ReaderFields));
//        }
//        while (reader.NextResult()) { }
//        reader.Close();
//        reader.Dispose();
//        return result;
//    }
//    public ISqlExpression<T> Skip(int offset)
//    {
//        return this;
//    }
//    public ISqlExpression<T> Take(int limit)
//    {
//        return this;
//    }
//    public ISqlExpression<T> Where(Expression<Func<T, bool>> predicate)
//    {
//        this.visitor.Where(predicate);
//        return this;
//    }
//    public ISqlExpression<T> WhereIf(bool condition, Expression<Func<T, bool>> predicate)
//    {
//        if (condition) this.visitor.Where(predicate);
//        return this;
//    }
//    public string ToSql() => this.visitor.BuildSql();
//}

