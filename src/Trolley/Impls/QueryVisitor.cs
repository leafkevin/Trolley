using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley;

public class QueryVisitor : SqlVisitor, IQueryVisitor
{
    protected static readonly ConcurrentDictionary<int, (string, Action<StringBuilder, IOrmProvider, object>)> includeSqlGetterCache = new();
    protected static readonly ConcurrentDictionary<int, Action<object, object>> targetIncludeValuesSetters = new();
    private bool isDisposed;

    protected int? offset;
    protected int? limit;
    protected int pageNumber;

    protected string GroupBySql { get; set; }
    protected string HavingSql { get; set; }
    protected bool IsDistinct { get; set; }


    public List<ReaderField> IncludeReaderFields { get; set; }
    public TableSegment LastIncludeSegment { get; set; }
    public List<ReaderField> GroupByFields { get; set; }
    public List<ReaderField> HavingFields { get; set; }
    public List<OrderByField> OrderByFields { get; set; }
    public bool IsCteTable { get; set; }
    public int PageNumber => this.pageNumber;
    public int PageSize => this.limit ?? 0;
    public bool IsNeedPaging { get; set; }
    public bool IsScalar { get; set; }

    public QueryVisitor(DbContext dbContext, char tableAliasStart = 'a', ITheaCommand command = null)
    {
        this.DbContext = dbContext;
        this.TableAliasStart = tableAliasStart;
        this.Command = command;
        if (command != null)
        {
            this.Connection = command.Connection;
            this.DbParameters = this.Command.Parameters;
        }
        this.IsNeedTableAlias = true;
    }

    public override string BuildSql(out List<ReaderField> readerFields)
    {
        string sql = null;
        if (this.IsScalar)
        {
            sql = this.BuildSql(true, out readerFields);
            if (this.IsManyShardingTables)
            {
                sql = this.BuildShardingTablesSqlByFormat(sql, this.ShardingTableJointMark);
                sql = this.BuildShardingScalarSql(sql);
            }
        }
        else
        {
            sql = this.BuildSql(true, out readerFields);
            if (this.IsManyShardingTables)
            {
                sql = this.BuildShardingTablesSqlByFormat(sql, this.ShardingTableJointMark);
                if (this.IsNeedChangeUnionShardingTables)
                    sql = this.BuildShardingSql(sql);
            }
        }
        return sql;
    }
    public virtual string BuildSql(bool isBuildCteSql, out List<ReaderField> readerFields)
    {
        var builder = new StringBuilder();
        if (isBuildCteSql && this.RefQueries != null && this.RefQueries.Count > 0)
        {
            bool isRecursive = false;
            int index = 0;
            foreach (var refQueryObj in this.RefQueries)
            {
                if (!refQueryObj.IsCteTable || refQueryObj is not ICteQuery cteQueryObj)
                    continue;
                if (index > 0) builder.AppendLine(",");
                builder.Append(cteQueryObj.Body);
                if (cteQueryObj.IsRecursive)
                    isRecursive = true;
                index++;
            }
            if (index > 0)
            {
                if (isRecursive)
                    builder.Insert(0, "WITH RECURSIVE ");
                else builder.Insert(0, "WITH ");
                builder.AppendLine();
            }
        }
        readerFields = this.ReaderFields;

        if (this.IsUnion && this.IsManyShardingTables)
            throw new NotSupportedException("多分表场景下不支持Union操作");

        string sql = null;
        if (!string.IsNullOrEmpty(this.UnionSql))
        {
            builder.Append(this.UnionSql);
            sql = builder.ToString();
            builder.Clear();
            return sql;
        }
        var headSql = builder.ToString();
        builder.Clear();

        string tableSql = null;
        if (this.Tables.Count > 0)
        {
            for (int i = 0; i < this.Tables.Count; i++)
            {
                var tableSegment = this.Tables[i];
                string tableName = this.GetFormatTableName(tableSegment);
                if (i > 0)
                {
                    if (!string.IsNullOrEmpty(tableSegment.JoinType))
                    {
                        builder.Append(' ');
                        builder.Append($"{tableSegment.JoinType} ");
                    }
                    else builder.Append(',');
                }
                builder.Append(tableName);
                //子查询要设置表别名
                builder.Append(" " + tableSegment.AliasName);
                if (!string.IsNullOrEmpty(tableSegment.SuffixRawSql))
                    builder.Append(" " + tableSegment.SuffixRawSql);
                if (!string.IsNullOrEmpty(tableSegment.OnExpr))
                    builder.Append($" ON {tableSegment.OnExpr}");
            }
            tableSql = builder.ToString();
        }
        builder.Clear();

        //各种单值查询，如：SELECT COUNT(*)/MAX(*)..等，都有SELECT操作     
        //如：From(f=>...).InnerJoin/UnionAll(f=>...)
        //生成sql时，include表的字段，一定要紧跟着主表字段后面，方便赋值主表实体的属性中，所以在插入时候就排好序
        //方案：在buildSql时确定，ReaderFields要重新排好序，include字段放到对应主表字段后面，表别名顺序不变
        if (this.ReaderFields == null)
            throw new Exception("缺少Select语句");

        var hasGroupBy = !string.IsNullOrEmpty(this.GroupBySql);
        var hasOrderBy = this.OrderByFields != null && this.OrderByFields.Count > 0;
        if (this.IsManyShardingTables)
        {
            if (hasGroupBy)
            {
                //当有多分表时，有分组，Select字段中，没有完全的分组字段，则需要补全所有分组字段
                foreach (var groupByField in this.GroupByFields)
                {
                    if (this.ReaderFields.Exists(f => f.IsGroupingField && f.FieldType == ReaderFieldType.Entity))
                        break;
                    var fieldName = groupByField.Value.ToString();
                    if (!this.TryFindReaderFieldByValue(this.ReaderFields, fieldName, out var readerField))
                        continue;
                    if (groupByField != readerField)
                    {
                        readerField.IsGroupingField = true;
                        groupByField.RefField = readerField;
                        continue;
                    }
                    groupByField.IsIgnore = true;
                    //只有一个分组字段并且还是非字段场景，添加别名方便后面使用
                    if (groupByField.TargetMember == null)
                        groupByField.AliasName = "Grouping";
                    this.ReaderFields.Add(groupByField);
                }
            }
            if (hasOrderBy)
            {
                //当有多分表时，有排序，Select字段中，没有完全的排序字段，则需要补全所有排序字段
                foreach (var orderByField in this.OrderByFields)
                {
                    var fieldName = orderByField.Field.Value.ToString();
                    if (!this.TryFindReaderFieldByValue(this.ReaderFields, fieldName, out var readerField))
                        continue;
                    if (orderByField.Field != readerField)
                    {
                        readerField.IsOrderingField = true;
                        orderByField.Field.RefField = readerField;
                        continue;
                    }
                    orderByField.Field.IsIgnore = true;
                    this.ReaderFields.Add(orderByField.Field);
                }
            }
        }

        this.AddSelectFieldsSql(builder, this.ReaderFields);

        string selectSql = null;
        if (this.IsDistinct)
            selectSql = "DISTINCT " + builder.ToString();
        else selectSql = builder.ToString();
        builder.Clear();

        if (this.WhereBuilder != null && this.WhereBuilder.Length > 0)
            builder.Append($"WHERE {this.WhereBuilder.ToString()}");
        //有多分表还有Group By操作，每个分表语句中做Group By操作，Union All语句后，还要再做Group By操作
        if (hasGroupBy)
        {
            if (builder.Length > 0) builder.Append(' ');
            builder.Append($"GROUP BY {this.GroupBySql}");
        }
        //有多分表还有Group By+Having操作，每个分表语句中只做Group By操作，不做Having操作，在Union All语句后，再做Group By+Having操作
        if (!this.IsManyShardingTables && !string.IsNullOrEmpty(this.HavingSql))
            builder.Append($" HAVING {this.HavingSql}");

        //包含Where+GroupBy语句，不包含OrderBy语句
        var others = builder.ToString();
        string orderBy = null;

        //多分表场景只要没有Limit语句，不添加OrderBy语句，也不添加Offset语句
        //多分表场景有Limit语句，没有Offset语句，正常添加OrderBy语句
        if (hasOrderBy && (!this.IsManyShardingTables || this.limit.HasValue))
        {
            builder.Clear();
            //当有多分表时，有排序，Select字段中，没有完全的排序字段，则需要补全所有排序字段
            foreach (var orderByField in this.OrderByFields)
            {
                var fieldName = orderByField.Field.Value.ToString();
                if (!this.TryFindReaderFieldByValue(this.ReaderFields, fieldName, out var readerField))
                    continue;
                //OrderBy字段，优先使用SELECT字段别名
                if (readerField.IsNeedAlias)
                    builder.Append(readerField.AliasName);
                else builder.Append(fieldName);
            }
            orderBy = $"ORDER BY {builder.ToString()}";
            //if (!this.IsManyShardingTables || (this.IsManyShardingTables && !this.offset.HasValue && this.limit.HasValue))
            //{
            //    orderBy = $"ORDER BY {orderBy}";
            //    if (!this.offset.HasValue && !this.limit.HasValue)
            //        builder.Append(" " + orderBy);
            //}
        }
        builder.Clear();
        if (!string.IsNullOrEmpty(headSql))
            builder.Append(headSql);

        //多分表场景同时有Offset和Limit语句分页，正常添加OrderBy语句，不添加Offset，Limit设置为Offset+Limit
        //多分表场景下，offset有值，limit没有值，不添加offset语句，在最外层UNION ALL后，再添加offset语句
        //多分表场景下有分页，offset/limit都有值，limit要加上offset的值，防止丢失数据，在最外层UNION ALL后，再添加offset语句

        int offset = 0, limit = 0;
        if (this.limit.HasValue) limit = this.limit.Value;
        if (this.offset.HasValue || this.limit.HasValue)
        {
            if (this.offset.HasValue && this.limit.HasValue)
            {
                if (this.IsManyShardingTables)
                    limit = this.offset.Value + this.limit.Value;

                //生成分页COUNT语句，不需要添加OrderBy语句
                if (this.IsNeedPaging && !this.IsManyShardingTables)
                {
                    var fromSql = others.Length > 0 ? $"{tableSql} {others}" : tableSql;
                    if (!string.IsNullOrEmpty(this.GroupBySql))
                        fromSql = $"(SELECT {selectSql} FROM {fromSql}) a";
                    builder.Append($"SELECT COUNT(*) FROM {fromSql};");
                }
            }
            //SQL TEMPLATE:SELECT /**fields**/ FROM /**tables**/ /**others**/
            var pageSql = this.OrmProvider.GetPagingTemplate(offset, limit, orderBy);
            pageSql = pageSql.Replace("/**fields**/", selectSql);
            pageSql = pageSql.Replace("/**tables**/", tableSql);
            pageSql = pageSql.Replace("/**others**/", others);
            builder.Append($"{pageSql}");
        }
        else
        {
            builder.Append($"SELECT {selectSql} FROM {tableSql}");
            if (others.Length > 0)
            {
                builder.Append(' ');
                builder.Append(others);
            }
            if (!string.IsNullOrEmpty(orderBy))
            {
                builder.Append(' ');
                builder.Append(orderBy);
            }
        }

        if (this.IsManyShardingTables && (hasGroupBy || hasOrderBy || this.offset.HasValue || this.limit.HasValue))
            this.IsNeedChangeUnionShardingTables = true;

        //UNION的子查询中有OrderBy/Offset/Limit，就需要包装一下SELECT * FROM，否则数据结果不正确
        //多分表场景下，在处理多分表时再做包装，这里不包装，包装就错了
        if (this.IsUnion && (hasOrderBy || this.offset.HasValue || this.limit.HasValue))
        {
            builder.Insert(0, "SELECT * FROM (");
            builder.Append($") a");
        }
        sql = builder.ToString();
        builder.Clear();
        return sql;
    }
    public virtual string BuildCommandSql(Type entityType, out IDataParameterCollection dbParameters)
    {
        var builder = new StringBuilder("(");
        var entityMapper = this.Tables[0].Mapper;
        int index = 0;
        foreach (var readerField in this.ReaderFields)
        {
            //Union后，如果没有select语句时，通常实体类型或是select分组对象
            var memberName = readerField.TargetMember.Name;
            if (!entityMapper.TryGetMemberMap(memberName, out var memberMapper)
                || memberMapper.IsIgnore || memberMapper.IsIgnoreInsert
                || memberMapper.IsNavigation || memberMapper.IsAutoIncrement || memberMapper.IsRowVersion)
                continue;
            if (index > 0) builder.Append(',');
            builder.Append($"{this.OrmProvider.GetFieldName(memberMapper.FieldName)}");
            index++;
        }
        builder.Append(") ");
        //有CTE表
        if (this.RefQueries != null && this.RefQueries.Count > 0)
        {
            var fieldsSql = builder.ToString();
            builder.Clear();
            bool isRecursive = false;

            index = 0;
            foreach (var refQueryObj in this.RefQueries)
            {
                if (!refQueryObj.IsCteTable || refQueryObj is not ICteQuery cteQueryObj)
                    continue;
                if (index > 0) builder.AppendLine(",");
                builder.Append(cteQueryObj.Body);
                if (cteQueryObj.IsRecursive)
                    isRecursive = true;
                index++;
            }

            if (isRecursive)
                builder.Insert(0, "WITH RECURSIVE ");
            else builder.Insert(0, "WITH ");
            builder.AppendLine();
            builder.Append(fieldsSql);
        }
        dbParameters = this.DbParameters;

        if (this.IsUnion && this.IsManyShardingTables)
            throw new NotSupportedException("多分表场景下不支持Union操作");

        string sql = null;
        if (!string.IsNullOrEmpty(this.UnionSql))
        {
            builder.Append(this.UnionSql);
            sql = builder.ToString();
            builder.Clear();
            return sql;
        }
        var headSql = builder.ToString();
        builder.Clear();

        string tableSql = null;
        if (this.Tables.Count > 0)
        {
            for (int i = 0; i < this.Tables.Count; i++)
            {
                var tableSegment = this.Tables[i];
                string tableName = this.GetFormatTableName(tableSegment);
                if (i > 0)
                {
                    if (!string.IsNullOrEmpty(tableSegment.JoinType))
                    {
                        builder.Append(' ');
                        builder.Append($"{tableSegment.JoinType} ");
                    }
                    else builder.Append(',');
                }
                builder.Append(tableName);
                //子查询要设置表别名
                builder.Append(" " + tableSegment.AliasName);
                if (!string.IsNullOrEmpty(tableSegment.SuffixRawSql))
                    builder.Append(" " + tableSegment.SuffixRawSql);
                if (!string.IsNullOrEmpty(tableSegment.OnExpr))
                    builder.Append($" ON {tableSegment.OnExpr}");
            }
            tableSql = builder.ToString();
        }
        builder.Clear();

        //各种单值查询，如：SELECT COUNT(*)/MAX(*)..等，都有SELECT操作     
        //如：From(f=>...).InnerJoin/UnionAll(f=>...)
        //生成sql时，include表的字段，一定要紧跟着主表字段后面，方便赋值主表实体的属性中，所以在插入时候就排好序
        if (this.ReaderFields == null)
            throw new Exception("缺少Select语句");

        var hasGroupBy = !string.IsNullOrEmpty(this.GroupBySql);
        var hasOrderBy = this.OrderByFields != null && this.OrderByFields.Count > 0;
        if (this.IsManyShardingTables)
        {
            if (hasGroupBy)
            {
                //当有多分表时，有分组，Select字段中，没有完全的分组字段，则需要补全所有分组字段
                foreach (var groupByField in this.GroupByFields)
                {
                    if (this.ReaderFields.Exists(f => f.IsGroupingField && f.FieldType == ReaderFieldType.Entity))
                        break;
                    var fieldName = groupByField.Value.ToString();
                    if (!this.TryFindReaderFieldByValue(this.ReaderFields, fieldName, out var readerField))
                        continue;
                    if (groupByField != readerField)
                    {
                        readerField.IsGroupingField = true;
                        groupByField.RefField = readerField;
                        continue;
                    }
                    groupByField.IsIgnore = true;
                    this.ReaderFields.Add(groupByField);
                }
            }
            if (hasOrderBy)
            {
                //当有多分表时，有排序，Select字段中，没有完全的排序字段，则需要补全所有排序字段
                foreach (var orderByField in this.OrderByFields)
                {
                    var fieldName = orderByField.Field.Value.ToString();
                    if (!this.TryFindReaderFieldByValue(this.ReaderFields, fieldName, out var readerField))
                        continue;
                    if (orderByField.Field != readerField)
                    {
                        readerField.IsOrderingField = true;
                        orderByField.Field.RefField = readerField;
                        continue;
                    }
                    orderByField.Field.IsIgnore = true;
                    this.ReaderFields.Add(orderByField.Field);
                }
            }
        }

        this.AddSelectFieldsSql(builder, this.ReaderFields);

        string selectSql = null;
        if (this.IsDistinct)
            selectSql = "DISTINCT " + builder.ToString();
        else selectSql = builder.ToString();
        builder.Clear();

        if (this.WhereBuilder != null && this.WhereBuilder.Length > 0)
            builder.Append($" WHERE {this.WhereBuilder.ToString()}");
        //有多分表还有Group By操作，每个分表语句中做Group By操作，Union All语句后，还要再做Group By操作
        if (hasGroupBy)
        {
            if (this.IsManyShardingTables) this.IsNeedChangeUnionShardingTables = true;
            builder.Append($" GROUP BY {this.GroupBySql}");
        }
        //有多分表还有Group By+Having操作，每个分表语句中只做Group By操作，不做Having操作，在Union All语句后，再做Group By+Having操作
        if (!this.IsManyShardingTables && !string.IsNullOrEmpty(this.HavingSql))
            builder.Append($" HAVING {this.HavingSql}");

        //包含Where+GroupBy语句，不包含OrderBy语句
        var others = builder.ToString();
        string orderBy = null;

        //多分表场景只要没有Limit语句，不添加OrderBy语句，也不添加Offset语句
        //多分表场景有Limit语句，没有Offset语句，正常添加OrderBy语句
        if (hasOrderBy && (!this.IsManyShardingTables || this.limit.HasValue))
        {
            builder.Clear();
            //当有多分表时，有排序，Select字段中，没有完全的排序字段，则需要补全所有排序字段
            foreach (var orderByField in this.OrderByFields)
            {
                var fieldName = orderByField.Field.Value.ToString();
                if (!this.TryFindReaderFieldByValue(this.ReaderFields, fieldName, out var readerField))
                    continue;
                //OrderBy字段，优先使用SELECT字段别名
                if (readerField.IsNeedAlias)
                    builder.Append(readerField.AliasName);
                else builder.Append(fieldName);
            }
            orderBy = $" ORDER BY {builder.ToString()}";
            //if (!this.IsManyShardingTables || (this.IsManyShardingTables && !this.offset.HasValue && this.limit.HasValue))
            //{
            //    orderBy = $"ORDER BY {orderBy}";
            //    if (!this.offset.HasValue && !this.limit.HasValue)
            //        builder.Append(" " + orderBy);
            //}
        }
        builder.Clear();
        if (!string.IsNullOrEmpty(headSql))
            builder.Append(headSql);

        //多分表场景同时有Offset和Limit语句分页，正常添加OrderBy语句，不添加Offset，Limit设置为Offset+Limit
        //多分表场景下，offset有值，limit没有值，不添加offset语句，在最外层UNION ALL后，再添加offset语句
        //多分表场景下有分页，offset/limit都有值，limit要加上offset的值，防止丢失数据，在最外层UNION ALL后，再添加offset语句

        int offset = 0, limit = 0;
        if (this.limit.HasValue) limit = this.limit.Value;
        if (this.offset.HasValue || this.limit.HasValue)
        {
            if (this.offset.HasValue && this.limit.HasValue)
            {
                if (this.IsManyShardingTables)
                    limit = this.offset.Value + this.limit.Value;

                //生成分页COUNT语句，不需要添加OrderBy语句
                if (this.IsNeedPaging)
                {
                    var fromSql = $"{tableSql}{others}";
                    if (!string.IsNullOrEmpty(this.GroupBySql))
                        fromSql = $"(SELECT {selectSql} FROM {tableSql}{others}) a";
                    builder.Append($"SELECT COUNT(*) FROM {fromSql};");
                }
            }

            //生成查询数据语句时，需要添加OrderBy语句
            if (!string.IsNullOrEmpty(orderBy))
                others += orderBy;

            //SQL TEMPLATE:SELECT /**fields**/ FROM /**tables**/ /**others**/
            var pageSql = this.OrmProvider.GetPagingTemplate(offset, limit, orderBy);
            pageSql = pageSql.Replace("/**fields**/", selectSql);
            pageSql = pageSql.Replace("/**tables**/", tableSql);
            pageSql = pageSql.Replace(" /**others**/", others);
            builder.Append($"{pageSql}");
        }
        else
        {
            //生成查询数据语句时，需要添加OrderBy语句
            if (!string.IsNullOrEmpty(orderBy))
                others += orderBy;
            builder.Append($"SELECT {selectSql} FROM {tableSql}{others}");
        }
        if (this.IsManyShardingTables && (hasGroupBy || hasOrderBy || this.offset.HasValue || this.limit.HasValue))
            this.IsNeedChangeUnionShardingTables = true;

        //UNION的子查询中有OrderBy/Offset/Limit，就需要包装一下SELECT * FROM，否则数据结果不正确
        //多分表场景下，在处理多分表时再做包装，这里不包装，包装就错了
        //if (this.IsUnion && (hasOrderBy || this.offset.HasValue || this.limit.HasValue))
        //{
        //    builder.Insert(0, "SELECT * FROM (");
        //    builder.Append($") a");
        //}
        sql = builder.ToString();
        builder.Clear();
        return sql;
    }
    public virtual string BuildShardingSql(string formatSql)
    {
        string GetFieldName(object fieldString)
        {
            var fieldName = fieldString.ToString();
            var index = fieldName.IndexOf('.');
            return fieldName.Substring(index + 1);
        }
        var builder = new StringBuilder();
        for (int i = 0; i < this.ReaderFields.Count; i++)
        {
            var readerField = this.ReaderFields[i];
            //跳过后加的字段
            if (readerField.IsIgnore) continue;

            string fieldName = null;
            if (readerField.IsAggField)
            {
                if (readerField.IsAvgField)
                {
                    var fieldName1 = $"{readerField.Fields[0].AggFunc}({readerField.Fields[0].AliasName})";
                    var fieldName2 = $"{readerField.Fields[1].AggFunc}({readerField.Fields[1].AliasName})";
                    fieldName = $"{fieldName1}/{fieldName2} AS {readerField.AliasName}";
                }
                else fieldName = $"{readerField.AggFunc}({readerField.Value}) AS {readerField.AliasName}";
            }
            else fieldName = readerField.IsNeedAlias ? readerField.AliasName : GetFieldName(readerField.Value);
            if (i > 0) builder.Append(',');
            builder.Append(fieldName);
        }
        var selectSql = builder.ToString();
        string groupBy = null;
        if (this.GroupByFields != null && this.GroupByFields.Count > 0)
        {
            builder.Clear();
            for (int i = 0; i < this.GroupByFields.Count; i++)
            {
                var groupByField = this.GroupByFields[i];
                var myReaderField = groupByField.RefField ?? groupByField;
                var fieldName = myReaderField.IsNeedAlias ? myReaderField.AliasName : GetFieldName(myReaderField.Value);
                if (i > 0) builder.Append(',');
                builder.Append(fieldName);
            }
            groupBy = "GROUP BY " + builder.ToString();
            //TODO: 需要添加Having字段
        }
        string orderBy = null;
        if (this.OrderByFields != null && this.OrderByFields.Count > 0)
        {
            builder.Clear();
            for (int i = 0; i < this.OrderByFields.Count; i++)
            {
                var orderByField = this.OrderByFields[i];
                var myReaderField = orderByField.Field.RefField ?? orderByField.Field;
                if (i > 0) builder.Append(',');
                builder.Append(myReaderField.Value.ToString());
                if (!string.IsNullOrEmpty(orderByField.Suffix))
                    builder.Append(orderByField.Suffix);
            }
            orderBy = "ORDER BY " + builder.ToString();
        }
        builder.Clear();

        //多分表场景同时有Offset和Limit语句分页，正常添加OrderBy语句，不添加Offset，Limit设置为Offset+Limit
        //多分表场景下，offset有值，limit没有值，不添加offset语句，在最外层UNION ALL后，再添加offset语句
        //多分表场景下有分页，offset/limit都有值，limit要加上offset的值，防止丢失数据，在最外层UNION ALL后，再添加offset语句

        var tableSql = $"({formatSql})";
        if (this.offset.HasValue || this.limit.HasValue)
        {
            //生成分页COUNT语句，不需要添加OrderBy语句
            if (this.IsNeedPaging)
                builder.Append($"SELECT COUNT(*) FROM {tableSql}");
            //SQL TEMPLATE:SELECT /**fields**/ FROM /**tables**/ /**others**/
            var pageSql = this.OrmProvider.GetPagingTemplate(this.offset, this.limit, orderBy);
            pageSql = pageSql.Replace("/**fields**/", selectSql);
            pageSql = pageSql.Replace("/**tables**/", tableSql);
            pageSql = pageSql.Replace(" /**others**/", groupBy);
            //TODO:有Having操作，要添加Having操作
            builder.Append($"{pageSql}");
        }
        else
        {
            //生成查询数据语句时，需要添加OrderBy语句
            builder.Append($"SELECT {selectSql} FROM {tableSql}");
            if (!string.IsNullOrEmpty(groupBy))
                builder.Append($" {groupBy}");
            //TODO:有Having操作，要添加Having操作

            if (!string.IsNullOrEmpty(orderBy))
                builder.Append($" {orderBy}");
        }
        var sql = builder.ToString();
        return sql;
    }
    public virtual string BuildShardingScalarSql(string formatSql)
    {
        string aggFields = null;
        var readerField = this.ReaderFields[0];
        switch (readerField.AggFunc)
        {
            case "COUNT":
                aggFields = "SUM(COUNT_VALUE)";
                break;
            case "SUM":
                aggFields = "SUM(SUM_VALUE)";
                break;
            case "AVG":
                aggFields = "SUM(SUM_VALUE)/SUM(COUNT_VALUE)";
                break;
            case "MAX":
                aggFields = "MAX(MAX_VALUE)";
                break;
            case "MIN":
                aggFields = "MIN(MIN_VALUE)";
                break;
        }
        return $"SELECT {aggFields} FROM ({formatSql}) AS t";
    }
    public virtual string BuildCteTableSql(string tableName, out List<ReaderField> readerFields)
    {
        tableName = this.OrmProvider.GetTableName(tableName);
        var rawSql = this.BuildSql(false, out readerFields);
        var builder = new StringBuilder($"{tableName}(");
        int index = 0;
        foreach (var readerField in readerFields)
        {
            if (readerField.FieldType == ReaderFieldType.Field)
            {
                if (readerField.IsDeferredFields)
                    throw new NotSupportedException($"CTE表不支持延迟字段，Field: {readerField.TargetMember.Name}");
                if (index > 0) builder.Append(',');
                builder.Append(this.OrmProvider.GetFieldName(readerField.TargetMember.Name));
                index++;
            }
            else
            {
                foreach (var childReaderField in readerField.Fields)
                {
                    if (index > 0) builder.Append(',');
                    builder.Append(this.OrmProvider.GetFieldName(childReaderField.TargetMember.Name));
                    index++;
                }
            }
        }
        builder.AppendLine(") AS ");
        builder.AppendLine("(");
        builder.AppendLine(rawSql);
        builder.Append(')');
        var sql = builder.ToString();
        builder.Clear();
        return sql;
    }
    public virtual void From(char tableAsStart = 'a', params Type[] entityTypes)
    {
        this.TableAliasStart = tableAsStart;
        foreach (var entityType in entityTypes)
        {
            int tableIndex = tableAsStart + this.Tables.Count;
            var tableSegment = new TableSegment
            {
                EntityType = entityType,
                Mapper = this.EntityMapProvider.GetEntityMap(entityType),
                AliasName = $"{(char)tableIndex}",
                Path = $"{(char)tableIndex}",
                TableType = TableType.Entity,
                IsMaster = true
            };
            this.AddTable(tableSegment);
            if (this.TryGetTableShardingInfo(entityType, TableShardingUsageMode.ReadOnly, out var tableShardingInfo))
                tableSegment.TableShardingInfo = tableShardingInfo;
        }
    }
    public virtual void AddTable(params Type[] entityTypes)
    {
        int tableIndex = this.TableAliasStart + this.Tables.Count;
        foreach (var entityType in entityTypes)
        {
            if (entityType == null) continue;
            var tableSegment = new TableSegment
            {
                EntityType = entityType,
                Mapper = this.EntityMapProvider.GetEntityMap(entityType),
                AliasName = $"{(char)(tableIndex++)}",
                Path = $"{(char)tableIndex}",
                TableType = TableType.Entity,
                IsMaster = true
            };
            this.AddTable(tableSegment);
            if (this.TryGetTableShardingInfo(entityType, TableShardingUsageMode.ReadOnly, out var tableShardingInfo))
                tableSegment.TableShardingInfo = tableShardingInfo;
        }
    }
    public void UseNewQuery(Type targetType, Expression subQueryExpr, bool isClearTables)
    {
        //repository.FromQuery(f => ... ) 或是 ... .WithTable(f => ... )，具体参数如下：
        //f => f.From<Order>().Where(o=>o.Id==1) ... 或是 f => cteOrders 或是 f => myRefOrders等
        //或是 f => myCteOrders.Where(o=>o.Id==1) ... 或是 f => myRefOrders.Where(o=>o.Id==1)等
        //都是从引用现有子查询、CTE表、新建子查询生成一个子查询加入到当前Tables中，后续会有Join/Where...等操作，子查询中表别名从'a'开始
        //必须新建一个QueryVisitor对象，不能使用已有表，后续的字段引用只存在于新表中

        var lambdaExpr = this.EnsureLambda(subQueryExpr);
        if (lambdaExpr.Body.NodeType == ExpressionType.MemberAccess)
        {
            //直接引用子查询，也可以是CTE表
            var subQueryObj = lambdaExpr.Body.Evaluate() as IQuery;
            this.UseQuery(targetType, subQueryObj, isClearTables);
            return;
        }
        (var sql, var readerFields) = this.VisitFromQuery(lambdaExpr.Body);

        //CTE表，在VisitFromQuery中已经做了处理
        if (typeof(ICteQuery).IsAssignableFrom(lambdaExpr.Body.Type))
            return;

        //TODO:子查询中，有多分表并且还有Group By + Having/Count(Distinct)操作，出子查询后，
        //需要把所有多分表都打开UNION ALL起来，合成新的子查询，并去掉分表属性，以单表处理后续操作
        //除此之外，其他场景，还需要继续保留多分表属性，以便后面映射多分表
        //变成子查询了，不再需要UnionAll操作了
        this.IsNeedChangeUnionShardingTables = false;
        this.IsManyShardingTables = false;

        if (isClearTables)
        {
            this.Clear();
            this.Tables.Clear();
        }
        var tableSegment = this.AddJoinTable(targetType, null, TableType.FromQuery, $"({sql})", readerFields);
        //从FromQuery对象开始的场景，直接build和生成SQL，就可以，正常逻辑    
        this.InitUseQueryReaderFields(tableSegment, readerFields);
    }
    public virtual void Union(string union, Type targetType, IQuery subQuery)
    {
        string rawSql = null;
        if (subQuery is ICteQuery cteQuery)
            rawSql = $"SELECT * FROM {this.OrmProvider.GetTableName(cteQuery.TableName)}";
        else rawSql = subQuery.Visitor.BuildSql(false, out _);
        this.Union(union, targetType, rawSql);
    }
    public virtual void Union(string union, Type targetType, Expression subQueryExpr)
    {
        var lambdaExpr = this.EnsureLambda(subQueryExpr);
        if (lambdaExpr.Body.NodeType == ExpressionType.MemberAccess)
        {
            var subQueryObj = lambdaExpr.Body.Evaluate() as IQuery;
            this.UseQuery(targetType, subQueryObj, true);
            return;
        }
        (var sql, _) = this.VisitFromQuery(lambdaExpr.Body);
        this.Union(union, targetType, sql);
    }
    private void Union(string union, Type targetType, string subQuerySql)
    {
        //解析第一个UNION子句，需要AS别名
        this.IsUnion = true;
        var rawSql = this.BuildSql(false, out var readerFields);
        rawSql += union + Environment.NewLine + subQuerySql;
        this.Clear();
        this.Tables.RemoveAt(this.Tables.Count - 1);
        var tableSegment = this.AddJoinTable(targetType, null, TableType.FromQuery, $"({rawSql})", readerFields);
        this.InitUseQueryReaderFields(tableSegment, readerFields);
        //先放到UnionSql中，在AsCteTable方法中，BuildCteTableSql时能得到这个SQL
        this.UnionSql = rawSql;
        this.IsUnion = false;
    }
    public virtual void UnionRecursive(string union, Type targetType, Expression subQueryExpr)
    {
        this.IsUnion = true;
        var rawSql = this.BuildSql(false, out var readerFields);
        this.Clear();
        this.Tables.Clear();
        //此时产生的queryObj是一个新的对象，只能用于解析sql，与传进来的queryObj不是同一个对象，舍弃
        //临时產生一個隨機表名，在後面的AsCteTable時，再做替換
        var entityType = typeof(CteQuery<>).MakeGenericType(targetType);
        var selfQueryObj = RepositoryHelper.CreateInstance(entityType,
            [typeof(DbContext), typeof(IQueryVisitor)], this.DbContext, this) as ICteQuery;
        selfQueryObj.TableName = $"__CTE_TABLE_{Guid.NewGuid():N}__";
        selfQueryObj.ReaderFields = readerFields;
        selfQueryObj.IsRecursive = true;
        this.CteQueryObj = selfQueryObj;
        this.IsRecursive = true;

        (var sql, _) = this.VisitFromQuery(subQueryExpr, selfQueryObj);
        rawSql += union + Environment.NewLine + sql;
        //先放到UnionSql中，在AsCteTable方法中，BuildCteTableSql时能得到这个SQL
        this.UnionSql = rawSql;
        this.IsUnion = false;
    }
    public virtual void Join(string joinType, Expression joinOn)
       => this.Join(joinType, joinOn, f => this.InitTableAlias(f));
    public virtual void Join(string joinType, Type newEntityType, Expression joinOn)
        => this.Join(joinType, joinOn, f => { this.From(this.TableAliasStart, newEntityType); return this.InitTableAlias(f); });
    public virtual void Join(string joinType, Type newEntityType, IQuery subQuery, Expression joinOn)
        => this.Join(joinType, joinOn, f => { this.UseQuery(newEntityType, subQuery, false); return this.InitTableAlias(f); });
    public virtual void Join(string joinType, Type newEntityType, Expression subQueryExpr, Expression joinOn)
        => this.Join(joinType, joinOn, f => { this.UseNewQuery(newEntityType, subQueryExpr, false); return this.InitTableAlias(f); });
    private void Join(string joinType, Expression joinOn, Func<LambdaExpression, TableSegment> joinTableSegmentGetter = null)
    {
        var lambdaExpr = joinOn as LambdaExpression;
        if (!lambdaExpr.Body.TryGetParameters(out var parameters))
            throw new NotSupportedException("当前Join操作，没有表关联");
        if (parameters.Count != 2)
            throw new NotSupportedException("Join操作，只支持两个表进行关联，但可以多次Join操作");

        var joinTableSegment = joinTableSegmentGetter.Invoke(lambdaExpr);
        joinTableSegment.JoinType = joinType;
        this.IsWhere = true;
        joinTableSegment.OnExpr = this.VisitConditionExpr(lambdaExpr.Body, out _);
        this.IsWhere = false;
    }
    public virtual bool Include(Expression memberSelector, Expression filter = null)
        => this.Include(memberSelector, (a, b) => this.InitTableAlias(a), filter);
    public virtual bool ThenInclude(Expression memberSelector, Expression filter = null)
        => this.Include(memberSelector, (a, b) =>
        {
            this.TableAliases.Clear();
            this.TableAliases.Add(b[0].Name, this.LastIncludeSegment);
        }, filter);
    public virtual bool HasIncludeTables() => this.IncludeTables != null && this.IncludeTables.Count > 0;
    public virtual bool BuildIncludeSql(Type targetType, object target, bool isMultiResult, out string sql)
    {
        sql = null;
        if (this.IncludeTables == null || this.IncludeTables.Count == 0)
            return false;

        if (target == null) return false;
        ICollection targets = null;
        if (isMultiResult)
        {
            targets = target as ICollection;
            if (targets.Count == 0)
                return false;
        }

        Action<StringBuilder, Action<StringBuilder, IOrmProvider, object>> sqlBuilderInitializer = null;
        if (isMultiResult)
        {
            sqlBuilderInitializer = (builder, foreignKeysSetter) =>
            {
                int index = 0;
                foreach (var target in targets)
                {
                    if (index > 0) builder.Append(',');
                    foreignKeysSetter.Invoke(builder, this.OrmProvider, target);
                    index++;
                }
                builder.Append(')');
            };
        }
        else
        {
            sqlBuilderInitializer = (builder, foreignKeysSetter) =>
            {
                foreignKeysSetter.Invoke(builder, this.OrmProvider, target);
                builder.Append(')');
            };
        }
        var builder = new StringBuilder();
        for (int i = 0; i < this.IncludeTables.Count; i++)
        {
            if (i > 0 && builder.Length > 0) builder.Append(';');
            var includeTableSegment = this.IncludeTables[i];
            var rootPath = includeTableSegment.Path.Substring(0, 1);
            var rootReaderField = this.ReaderFields.Find(f => f.Path == rootPath);
            if (rootReaderField == null)
                throw new NotSupportedException("Include导航属性成员，必须先Select对应的实体表，如：.Include((x, y) => x.Buyer).Select((x, y) => new { Order = x, ... })");

            var firstMember = rootReaderField.TargetMember;
            (var headSql, var sqlInitializer) = this.BuildIncludeSqlGetter(targetType, firstMember, includeTableSegment);
            if (includeTableSegment.IsSharding && includeTableSegment.TableNames.Count > 0)
            {
                var sqlBuilder = new StringBuilder();
                sqlBuilderInitializer.Invoke(sqlBuilder, sqlInitializer);
                if (!string.IsNullOrEmpty(includeTableSegment.Filter))
                    sqlBuilder.Append($" AND {includeTableSegment.Filter}");
                var afterSql = sqlBuilder.ToString();
                var origName = includeTableSegment.Mapper.TableName;
                var formatName = $"__SHARDING_{includeTableSegment.ShardingId}_{origName}";
                int index = 0;
                foreach (var tableName in includeTableSegment.TableNames)
                {
                    if (index > 0) builder.Append(" UNION ALL ");
                    builder.Append(headSql.Replace(formatName, tableName));
                    builder.Append(afterSql);
                    index++;
                }
            }
            else
            {
                builder.Append(headSql);
                sqlBuilderInitializer.Invoke(builder, sqlInitializer);
                if (!string.IsNullOrEmpty(includeTableSegment.Filter))
                    builder.Append($" AND {includeTableSegment.Filter}");
            }
        }
        if (builder.Length > 0)
        {
            sql = builder.ToString();
            return true;
        }
        return false;
    }
    public virtual void SetIncludeValues(Type targetType, object target, ITheaDataReader reader, bool isMultiResult)
    {
        var deferredInitializers = new List<(object, Action<object>)>();
        foreach (var includeSegment in this.IncludeTables)
        {
            //NavigationType不一定是includeSegment.EntityType，有可能是瘦身版类型
            var navigationType = includeSegment.FromMember.NavigationType;
            var rootPath = includeSegment.Path.Substring(0, 1);
            var rootReaderField = this.ReaderFields.Find(f => f.Path == rootPath);
            //当最外层实体是参数访问时，此值为null
            var firstMember = rootReaderField.TargetMember;
            var includeValues = RepositoryHelper.ReadList(navigationType, includeSegment.EntityType, reader, this.DbContext);
            Action<object> includeValuesSetter = f => this.SetIncludeValueToTarget(targetType, firstMember, includeSegment, f, includeValues);
            deferredInitializers.Add((includeValues, includeValuesSetter));
        }
        if (isMultiResult)
        {
            var targets = target as ICollection;
            foreach ((var includeValues, var includeValuesSetter) in deferredInitializers)
            {
                if (includeValues is ICollection collection && collection.Count > 0)
                {
                    foreach (var targetItem in targets)
                        includeValuesSetter(targetItem);
                }
            }
        }
        else
        {
            foreach ((var includeValues, var valueSetter) in deferredInitializers)
            {
                if (includeValues is ICollection collection && collection.Count > 0)
                    valueSetter(target);
            }
        }
        reader.NextResult();
    }
    public virtual async Task SetIncludeValuesAsync(Type targetType, object target, ITheaDataReader reader, bool isMultiResult, CancellationToken cancellationToken = default)
    {
        var deferredInitializers = new List<(object, Action<object>)>();
        foreach (var includeSegment in this.IncludeTables)
        {
            var navigationType = includeSegment.FromMember.NavigationType;
            var rootPath = includeSegment.Path.Substring(0, 1);
            var rootReaderField = this.ReaderFields.Find(f => f.Path == rootPath);
            //当最外层实体是参数访问时，此值为null
            var firstMember = rootReaderField.TargetMember;
            var includeValues = await RepositoryHelper.ReadListAsync(navigationType, includeSegment.EntityType, reader, this.DbContext, cancellationToken);
            Action<object> includeValuesSetter = f => this.SetIncludeValueToTarget(targetType, firstMember, includeSegment, f, includeValues);
            deferredInitializers.Add((includeValues, includeValuesSetter));
        }
        if (isMultiResult)
        {
            var targets = target as ICollection;
            foreach ((var includeValues, var includeValuesSetter) in deferredInitializers)
            {
                if (includeValues is ICollection collection && collection.Count > 0)
                {
                    foreach (var targetItem in targets)
                        includeValuesSetter(targetItem);
                }
            }
        }
        else
        {
            foreach ((var includeValues, var includeValuesSetter) in deferredInitializers)
            {
                if (includeValues is ICollection collection && collection.Count > 0)
                    includeValuesSetter(target);
            }
        }
        await reader.NextResultAsync(cancellationToken);
    }
    private void SetIncludeValueToTarget(Type targetType, MemberInfo firstMember, TableSegment includeSegment, object target, object includeValues)
    {
        var cacheKey = this.GetIncludeKey(targetType, firstMember, includeSegment);
        var includeValuesSetter = targetIncludeValuesSetters.GetOrAdd(cacheKey, f =>
        {
            var targetExpr = Expression.Parameter(typeof(object), "target");
            var anonListExpr = Expression.Parameter(typeof(object), "anonObjs");
            var blockParameters = new List<ParameterExpression>();
            var blockBodies = new List<Expression>();

            var elementType = includeSegment.FromMember.NavigationType;
            var listType = typeof(List<>).MakeGenericType(elementType);
            var typedListExpr = Expression.Variable(listType, "typedList");
            var typedTargetExpr = Expression.Variable(targetType, "typedTarget");
            blockParameters.AddRange([typedListExpr, typedTargetExpr]);
            blockBodies.Add(Expression.Assign(typedListExpr, Expression.Convert(anonListExpr, listType)));
            blockBodies.Add(Expression.Assign(typedTargetExpr, Expression.Convert(targetExpr, targetType)));

            //order.Seller.Company.Products
            //var foreignKeyValue = target.Seller.Company.Id;
            //target.Order.Seller.Company.Products或是target.Details
            Expression parentExpr = typedTargetExpr;
            if (firstMember != null)
            {
                //target.Order.Seller.Company.Products
                parentExpr = Expression.PropertyOrField(typedTargetExpr, firstMember.Name);
                for (int i = 0; i < includeSegment.ParentMemberVisits.Count - 1; i++)
                {
                    //取父亲对象的完整访问路径：target.Order.Seller.Company
                    var memberInfo = includeSegment.ParentMemberVisits[i];
                    parentExpr = Expression.PropertyOrField(parentExpr, memberInfo.Name);
                }
            }
            var foreignKeyMember = includeSegment.FromTable.Mapper.KeyMembers[0];
            Expression foreignKeyValueExpr = Expression.PropertyOrField(parentExpr, foreignKeyMember.MemberName);
            var memberName = includeSegment.FromMember.MemberName;
            for (int i = 0; i < includeSegment.ParentMemberVisits.Count - 1; i++)
            {
                var memberInfo = includeSegment.ParentMemberVisits[i];
                parentExpr = Expression.PropertyOrField(parentExpr, memberInfo.Name);
            }
            var keyMember = includeSegment.FromTable.Mapper.KeyMembers[0];
            var includeMemberExpr = Expression.PropertyOrField(parentExpr, memberName);

            //var myIncludeValues = includeValues.FindAll(f => f.CompanyId == target.Seller.Company.Id);
            var predicateType = typeof(Predicate<>).MakeGenericType(elementType);
            var parameterExpr = Expression.Parameter(elementType, "f");
            var foreignKey = includeSegment.FromMember.ForeignKey;
            var equalExpr = Expression.Equal(Expression.PropertyOrField(parameterExpr, foreignKey), foreignKeyValueExpr);

            var predicateExpr = Expression.Lambda(predicateType, equalExpr, parameterExpr);
            var methodInfo = listType.GetMethod("FindAll", [predicateType]);
            var filterValuesExpr = Expression.Call(typedListExpr, methodInfo, predicateExpr);

            var myIncludeValuesExpr = Expression.Variable(listType, "myIncludeValues");
            blockParameters.Add(myIncludeValuesExpr);
            blockBodies.Add(Expression.Assign(myIncludeValuesExpr, filterValuesExpr));

            //target.Seller.Company.Products = myIncludeValues;
            Expression setValueExpr = null;
            switch (includeSegment.FromMember.Member.MemberType)
            {
                case MemberTypes.Field:
                    setValueExpr = Expression.Assign(Expression.Field(parentExpr, memberName), myIncludeValuesExpr);
                    break;
                case MemberTypes.Property:
                    methodInfo = (includeSegment.FromMember.Member as PropertyInfo).GetSetMethod();
                    setValueExpr = Expression.Call(parentExpr, methodInfo, myIncludeValuesExpr);
                    break;
                default: throw new NotSupportedException("目前只支持Field或是Property两种成员访问");
            }

            //if(myIncludeValues.Count>0)
            //  target.Seller.Company.Products = myIncludeValues;
            var greaterThanExpr = Expression.GreaterThan(Expression.Property(myIncludeValuesExpr, "Count"), Expression.Constant(0));
            blockBodies.Add(Expression.IfThen(greaterThanExpr, setValueExpr));
            return Expression.Lambda<Action<object, object>>(Expression.Block(blockParameters, blockBodies), targetExpr, anonListExpr).Compile();
        });
        includeValuesSetter.Invoke(target, includeValues);
    }
    protected bool Include(Expression memberSelector, Action<LambdaExpression, List<ParameterExpression>> tableAliasInitializer, Expression filter = null)
    {
        //if (!string.IsNullOrEmpty(this.WhereSql) || !string.IsNullOrEmpty(this.GroupBySql) || !string.IsNullOrEmpty(this.OrderBySql)
        //    || string.IsNullOrEmpty(this.UnionSql) && this.ReaderFields != null && this.ReaderFields.Count > 0)
        //    throw new NotSupportedException("Include/ThenInclude操作必须要在Where/And/GroupBy/OrderBy/Select等操作之前完成，紧跟From/Join等操作之后");

        var lambdaExpr = memberSelector as LambdaExpression;
        var memberExpr = lambdaExpr.Body as MemberExpression;
        lambdaExpr.Body.TryGetParameters(out var parameters);
        tableAliasInitializer.Invoke(lambdaExpr, parameters);
        (var includeSegment, var isIncludeMany) = this.AddIncludeTables(memberExpr);

        if (filter != null)
        {
            this.IsIncludeMany = true;
            var filterLambdaExpr = filter as LambdaExpression;
            var parameterName = filterLambdaExpr.Parameters[0].Name;
            this.TableAliases.Clear();
            this.TableAliases.Add(parameterName, includeSegment);
            var sqlSegment = this.Visit(new SqlSegment { Expression = filter });
            includeSegment.Filter = sqlSegment.Value.ToString();
            this.IsIncludeMany = false;
        }
        this.LastIncludeSegment = includeSegment;
        return isIncludeMany;
    }
    protected (TableSegment, bool) AddIncludeTables(MemberExpression memberExpr)
    {
        TableSegment tableSegment = null;
        bool isIncludeMany = false;
        var memberType = memberExpr.Member.GetMemberType();
        if (!memberType.IsEntityType(out _))
            throw new NotSupportedException($"Include方法只支持实体属性，{memberExpr.Member.DeclaringType.FullName}.{memberExpr.Member.Name}不是实体，Path:{memberExpr}");

        //支持N级成员访问，如：.Include((x, y) => x.Seller.Company.Products)
        //TODO:IncludeMany后的ThenInclude未实现
        //.IncludeMany(x => x.Orders).ThenInclude(x => x.Buyer)
        var memberExprs = this.GetMemberExprs(memberExpr, out var parameterExpr);
        var fromSegment = this.TableAliases[parameterExpr.Name];
        var fromType = fromSegment.EntityType;
        var builder = new StringBuilder(fromSegment.AliasName);
        //1:N关系，需要记录访问路径，为后面结果赋值做准备
        var memberVisits = new List<MemberInfo>();
        while (memberExprs.TryPop(out var currentExpr))
        {
            //多级成员访问，fromSegment.Mapper可能为null，如：f.Order.Seller.Company
            //fromSegment.Mapper ??= this.EntityMapProvider.GetEntityMap(fromType);
            var memberMapper = fromSegment.Mapper.GetMemberMap(currentExpr.Member.Name);
            if (!memberMapper.IsNavigation)
                throw new NotSupportedException($"实体{fromType.FullName}的属性{currentExpr.Member.Name}未配置为导航属性");

            //实体类型是成员的声明类型，映射类型不一定是成员的声明类型，一定是成员的Map类型
            //如：成员是UserInfo类型，对应的模型是User类型，UserInfo类型只是User类型的一个子集，成员名称和映射关系完全一致
            var targetType = memberMapper.NavigationType;
            var entityMapper = this.EntityMapProvider.GetEntityMap(targetType, memberMapper.MapEntityType);
            if (entityMapper.KeyMembers.Count > 1)
                throw new NotSupportedException($"导航属性表，暂时不支持多个主键字段，实体：{memberMapper.MapEntityType.FullName}");

            memberVisits.Add(currentExpr.Member);
            var tableAlias = $"{(char)(this.TableAliasStart + this.Tables.Count)}";
            //path是从顶级级到子级的完整链路，用户查找TableSegment，如：a.Order.Seller.Company
            builder.Append("." + currentExpr.Member.Name);
            //在映射实体时，根据ParentIndex+FromMember值，设置到主表实体的属性中
            if (memberMapper.IsToOne)
            {
                this.Tables.Add(tableSegment = new TableSegment
                {
                    TableType = TableType.Include,
                    JoinType = "LEFT JOIN",
                    EntityType = targetType,
                    Mapper = entityMapper,
                    AliasName = tableAlias,
                    FromTable = fromSegment,
                    FromMember = memberMapper,
                    OnExpr = $"{fromSegment.AliasName}.{this.OrmProvider.GetFieldName(memberMapper.ForeignKey)}={tableAlias}.{this.OrmProvider.GetFieldName(entityMapper.KeyMembers[0].FieldName)}",
                    Path = builder.ToString()
                });
            }
            else
            {
                if (fromSegment.Mapper.KeyMembers.Count > 1)
                    throw new NotSupportedException($"导航属性表，暂时不支持多个主键字段，实体：{fromSegment.EntityType.FullName}");

                this.IncludeTables ??= new();
                this.IncludeTables.Add(tableSegment = new TableSegment
                {
                    TableType = TableType.Include,
                    JoinType = "LEFT JOIN",
                    EntityType = targetType,
                    Mapper = entityMapper,
                    FromTable = fromSegment,
                    FromMember = memberMapper,
                    Path = builder.ToString(),
                    ParentMemberVisits = memberVisits
                });
                isIncludeMany = true;
            }
            fromSegment = tableSegment;
            fromType = memberMapper.NavigationType;
        }
        builder.Clear();
        return (tableSegment, isIncludeMany);
    }
    private (string, Action<StringBuilder, IOrmProvider, object>) BuildIncludeSqlGetter(Type targetType, MemberInfo firstMember, TableSegment includeSegment)
    {
        var cacheKey = this.GetIncludeKey(targetType, firstMember, includeSegment);
        return includeSqlGetterCache.GetOrAdd(cacheKey, f =>
        {
            var targetExpr = Expression.Parameter(typeof(object), "target");
            var builderExpr = Expression.Parameter(typeof(StringBuilder), "builder");
            var ormProviderExpr = Expression.Parameter(typeof(IOrmProvider), "ormProvider");
            var blockParameters = new List<ParameterExpression>();
            var blockBodies = new List<Expression>();

            var typedTargetExpr = Expression.Variable(targetType, "typedTarget");
            blockParameters.Add(typedTargetExpr);
            blockBodies.Add(Expression.Assign(typedTargetExpr, Expression.Convert(targetExpr, targetType)));

            //target.Order.Seller.Company.Products或是target.Details           
            Expression parentExpr = typedTargetExpr;
            if (firstMember != null)
            {
                //target.Order.Seller.Company.Products
                parentExpr = Expression.PropertyOrField(typedTargetExpr, firstMember.Name);
                for (int i = 0; i < includeSegment.ParentMemberVisits.Count - 1; i++)
                {
                    //取父亲对象的完整访问路径：target.Order.Seller.Company
                    var memberInfo = includeSegment.ParentMemberVisits[i];
                    parentExpr = Expression.PropertyOrField(parentExpr, memberInfo.Name);
                }
            }
            var foreignKeyMember = includeSegment.FromTable.Mapper.KeyMembers[0];
            Expression foreignKeyValueExpr = Expression.PropertyOrField(parentExpr, foreignKeyMember.MemberName);
            //includeMany支持分表，UNION SQL处理
            if (foreignKeyMember.MemberType != typeof(object))
                foreignKeyValueExpr = Expression.Convert(foreignKeyValueExpr, typeof(object));
            var methedInfo = typeof(IOrmProvider).GetMethod(nameof(IOrmProvider.GetQuotedValue));
            var fieldTypeExpr = Expression.Constant(foreignKeyMember.UnderlyingType);
            foreignKeyValueExpr = Expression.Call(ormProviderExpr, methedInfo, fieldTypeExpr, foreignKeyValueExpr);
            methedInfo = typeof(StringBuilder).GetMethod(nameof(StringBuilder.Append), [typeof(string)]);
            blockBodies.Add(Expression.Call(builderExpr, methedInfo, foreignKeyValueExpr));

            var foreignKey = this.OrmProvider.GetFieldName(includeSegment.FromMember.ForeignKey);
            var tableName = this.OrmProvider.GetTableName(includeSegment.Mapper.TableName);
            var fields = RepositoryHelper.BuildSelectFieldsSqlPart(this.DbContext, includeSegment.Mapper, includeSegment.EntityType);


            var headSql = $"SELECT {fields} FROM {tableName} WHERE {foreignKey} IN (";
            var sqlInitializer = Expression.Lambda<Action<StringBuilder, IOrmProvider, object>>(Expression.Block(blockParameters, blockBodies), builderExpr, ormProviderExpr, targetExpr).Compile();
            return (headSql, sqlInitializer);
        });
    }
    public virtual void AndBy(object whereObj)
    {
        var commandInitializer = RepositoryHelper.BuildWhereCommandInitializer(this.DbContext, this.Tables[0].EntityType, whereObj, 4, false, false, false);
        this.VisitAndSql(commandInitializer.Invoke(this.DbParameters, this.DbContext, whereObj));
    }
    public virtual void AndById(object whereKey)
    {
        var commandInitializer = RepositoryHelper.BuildWhereCommandInitializer(this.DbContext, this.Tables[0].EntityType, whereKey, 4, true, false, false);
        this.VisitAndSql(commandInitializer.Invoke(this.DbParameters, this.DbContext, whereKey));
    }
    public virtual void AndByIds(object whereKeys)
    {
        var commandInitializer = RepositoryHelper.BuildWhereCommandInitializer(this.DbContext, this.Tables[0].EntityType, whereKeys, 4, true, false, true);
        this.VisitAndSql(commandInitializer.Invoke(this.DbParameters, this.DbContext, whereKeys));
    }
    public virtual void And(Expression whereExpr)
    {
        this.IsWhere = true;
        var lambdaExpr = whereExpr as LambdaExpression;
        this.ClearUnionSql();
        this.InitTableAlias(lambdaExpr);
        //不能更改LastWhereOperationType，如果是引用已有子查询，LastWhereOperationType是有值的
        var whereSql = this.VisitConditionExpr(lambdaExpr.Body, out var operationType);
        this.IsWhere = false;
        this.VisitAndSql(whereSql, operationType);
    }
    public virtual void OrBy(object whereObj)
    {
        var commandInitializer = RepositoryHelper.BuildWhereCommandInitializer(this.DbContext, this.Tables[0].EntityType, whereObj, 4, false, false, false);
        this.VisitOrSql(commandInitializer.Invoke(this.DbParameters, this.DbContext, whereObj));
    }
    public virtual void OrById(object whereKey)
    {
        var commandInitializer = RepositoryHelper.BuildWhereCommandInitializer(this.DbContext, this.Tables[0].EntityType, whereKey, 4, true, false, false);
        this.VisitOrSql(commandInitializer.Invoke(this.DbParameters, this.DbContext, whereKey));
    }
    public virtual void OrByIds(object whereKeys)
    {
        var commandInitializer = RepositoryHelper.BuildWhereCommandInitializer(this.DbContext, this.Tables[0].EntityType, whereKeys, 4, true, false, true);
        this.VisitOrSql(commandInitializer.Invoke(this.DbParameters, this.DbContext, whereKeys));
    }
    public virtual void Or(Expression whereExpr)
    {
        this.IsWhere = true;
        var lambdaExpr = whereExpr as LambdaExpression;
        this.ClearUnionSql();
        this.InitTableAlias(lambdaExpr);
        var whereSql = this.VisitConditionExpr(lambdaExpr.Body, out var operationType);
        this.IsWhere = false;
        this.VisitOrSql(whereSql, operationType);
    }
    public virtual void GroupBy(Expression expr)
    {
        var lambdaExpr = expr as LambdaExpression;
        this.ClearUnionSql();
        this.InitTableAlias(lambdaExpr);
        this.GroupByFields = new();
        //分组字段都设置为ReaderFieldType.Expression类型，以便于在多分表情况下后加的字段使用别名，
        switch (lambdaExpr.Body.NodeType)
        {
            case ExpressionType.New:
                var builder = new StringBuilder();
                int index = 0;
                var newExpr = lambdaExpr.Body as NewExpression;
                foreach (var argumentExpr in newExpr.Arguments)
                {
                    var memberInfo = newExpr.Members[index];
                    var sqlSegment = this.Visit(new SqlSegment { Expression = argumentExpr });
                    var readerFieldType = sqlSegment.SqlType == SqlType.OnlyField ? ReaderFieldType.Field : ReaderFieldType.Expression;
                    var fieldName = this.WrapSql(sqlSegment);
                    if (builder.Length > 0) builder.Append(',');
                    //生成GroupBy语句时，使用原始FieldName，需要在数据库中执行
                    builder.Append(fieldName);
                    this.GroupByFields.Add(new ReaderField
                    {
                        IsGroupingField = true,
                        FieldType = readerFieldType,
                        ReaderType = argumentExpr.Type,
                        MemberName = sqlSegment.MemberName,
                        Value = fieldName,
                        TargetMember = memberInfo
                    });
                    index++;
                }
                //GroupBy SQL中不含别名
                this.GroupBySql = builder.ToString();
                break;
            case ExpressionType.MemberAccess:
                {
                    var memberExpr = lambdaExpr.Body as MemberExpression;
                    var sqlSegment = this.Visit(new SqlSegment { Expression = memberExpr });
                    var fieldName = this.WrapSql(sqlSegment);
                    this.GroupByFields.Add(new ReaderField
                    {
                        IsGroupingField = true,
                        FieldType = ReaderFieldType.Field,
                        ReaderType = memberExpr.Type,
                        MemberName = sqlSegment.MemberName,
                        Value = fieldName,
                        TargetMember = memberExpr.Member
                    });
                    this.GroupBySql = fieldName;
                }
                break;
            default:
                {
                    var sqlSegment = this.Visit(new SqlSegment { Expression = lambdaExpr.Body });
                    var fieldName = this.WrapSql(sqlSegment);
                    this.GroupByFields.Add(new ReaderField
                    {
                        IsGroupingField = true,
                        FieldType = ReaderFieldType.Expression,
                        ReaderType = lambdaExpr.Body.Type,
                        Value = fieldName
                    });
                    this.GroupBySql = fieldName;
                }
                break;
        }
    }
    public virtual void OrderBy(string orderType, Expression expr)
    {
        var lambdaExpr = expr as LambdaExpression;
        this.ClearUnionSql();
        this.OrderByFields ??= new();
        this.InitTableAlias(lambdaExpr);
        this.IsOrderBy = true;
        var sqlSegment = this.Visit(new SqlSegment { Expression = expr });
        switch (sqlSegment.SqlType)
        {
            case SqlType.ReaderField:
                var readerField = sqlSegment.Value as ReaderField;
                //Grouping分组字段
                if (readerField.FieldType == ReaderFieldType.Entity)
                    readerField.Fields.ForEach(f => this.AddOrderByField(f, orderType));
                else this.AddOrderByField(readerField, orderType);
                break;
            case SqlType.ReaderFields:
                var readerFields = sqlSegment.Value as List<ReaderField>;
                readerFields.ForEach(f => this.AddOrderByField(f, orderType));
                break;
            default:
                //成员访问、表达式、方法调用、原始SQL等场景
                var readerFieldType = sqlSegment.SqlType == SqlType.OnlyField ? ReaderFieldType.Field : ReaderFieldType.Expression;
                this.AddOrderByField(new ReaderField
                {
                    FieldType = readerFieldType,
                    Value = this.WrapSql(sqlSegment),
                    TargetMember = sqlSegment.TargetMember
                }, orderType);
                break;
        }
        this.IsOrderBy = false;
    }
    private void AddOrderByField(ReaderField readerField, string orderType)
    {
        string suffix = null;
        if (orderType == "DESC")
            suffix = " DESC";
        readerField.IsOrderingField = true;
        this.OrderByFields.Add(new OrderByField { Field = readerField, Suffix = suffix });
    }
    public virtual void Having(Expression havingExpr)
    {
        this.IsHaving = true;
        var lambdaExpr = havingExpr as LambdaExpression;
        this.InitTableAlias(lambdaExpr);
        this.HavingSql = this.VisitConditionExpr(lambdaExpr.Body, out _);
        this.IsHaving = false;
    }

    public virtual void SelectGrouping() => this.ReaderFields = this.GroupByFields;
    public virtual void SelectDefault(Expression defaultExpr)
    {
        if (this.ReaderFields != null && this.ReaderFields.Count > 0)
            return;
        this.Select(defaultExpr);
    }
    public virtual void SelectRaw(Type targetType, string rawFields, string aggFunc = null)
    {
        //原始SQL，SELECT COUNT(*)，SELECT * 等
        var readerField = new ReaderField
        {
            FieldType = ReaderFieldType.RawSql,
            ReaderType = targetType,
            Value = rawFields
        };
        if (!string.IsNullOrEmpty(aggFunc))
        {
            //聚合字段，都是Expression类型
            readerField.FieldType = ReaderFieldType.Expression;
            readerField.IsAggField = true;
            readerField.AggFunc = aggFunc;
        }
        this.ReaderFields = [readerField];
    }
    public virtual void Select(Expression selectExpr)
    {
        this.IsSelect = true;
        var toTargetExpr = selectExpr as LambdaExpression;
        this.ClearUnionSql();
        this.InitTableAlias(toTargetExpr);
        //常量、变量、表达式、方法调用、成员访问、原始SQL、延迟属性、延迟方法调用等场景
        //.Select((x, y) => new { MaxValue = int.MaxValue, x.Seller, x.Buyer, Now = DateTime.UtcNow })
        //.SelectTo((a, b...) => new DTO{ ActivityTypeEnum = this.GetEmnuName(f.ActivityType) })
        //会有延迟成员访问，静态成员访问，还有方法调用等访问，所以需要VisitAndDeferred
        //延迟方法调用，参数可能有多个，返回的ReaderField只有一个
        //不一定有成员名称，无需设置TargetMember，如：.Select(f => f.Age / 10 * 10)
        var sqlSegment = this.Visit(new SqlSegment { Expression = toTargetExpr.Body });
        switch (sqlSegment.SqlType)
        {
            //成员访问
            case SqlType.OnlyField:
                this.ReaderFields = [new ReaderField
                {
                    FieldType = ReaderFieldType.Field,
                    ReaderType = toTargetExpr.Body.Type,
                    MemberMapper = sqlSegment.MemberMapper,
                    //MappedTargetType = sqlSegment.MappedTargetType,
                    //TypeHandler = sqlSegment.TypeHandler,
                    MemberName = sqlSegment.MemberName,
                    Value = sqlSegment.Value
                }];
                break;
            //成员访问，多个字段实体类型的原始SQL，聚合字段
            case SqlType.ReaderField:
                var readerField = sqlSegment.Value as ReaderField;
                readerField.ReaderType = toTargetExpr.Body.Type;
                this.ReaderFields = [readerField];
                break;
            case SqlType.ReaderFields:
                this.ReaderFields = sqlSegment.Value as List<ReaderField>;
                break;
            default:
                //常量、变量、表达式、方法调用、静态成员访问、原始SQL、延迟属性、延迟方法调用等场景
                //原始SQL，当个字段当作方法调用处理
                this.ReaderFields = [new ReaderField
                {
                    FieldType = ReaderFieldType.Expression,
                    ReaderType = toTargetExpr.Body.Type,
                    Value = this.WrapSql(sqlSegment)
                }];
                break;
        }
        this.IsSelect = false;
    }
    public virtual void Select(string sqlFormat, Expression selectExpr)
    {
        this.Select(selectExpr);
        //带字段的单值操作，SELECT COUNT(DISTINCT b.Id),MAX(b.Amount)等
        foreach (var readerField in this.ReaderFields)
        {
            readerField.Value = string.Format(sqlFormat, readerField.Value);
        }
    }
    public virtual void SelectTo(Type targetType, Expression specialMemberSelector = null)
    {
        this.IsSelect = true;
        if (specialMemberSelector != null)
        {
            var lambdaExpr = specialMemberSelector as LambdaExpression;
            this.ClearUnionSql();
            this.InitTableAlias(lambdaExpr);
            if (lambdaExpr.Body.NodeType == ExpressionType.MemberInit)
            {
                var sqlSegment = this.Visit(new SqlSegment { Expression = lambdaExpr.Body });
                //特殊的已经确定的列
                this.ReaderFields = sqlSegment.Fields;
            }
            else this.ReaderFields = new();
            bool isExistsFields = false;
            List<string> existsMembers = null;
            if (this.ReaderFields.Count > 0)
            {
                existsMembers = this.ReaderFields.Select(f => f.TargetMember.Name).ToList();
                isExistsFields = true;
            }
            var targetMembers = RepositoryHelper.GetMembers(targetType).FindAll(f => f.CanWrite);
            foreach (var memberInfo in targetMembers)
            {
                if (isExistsFields && existsMembers.Contains(memberInfo.Name)) continue;
                if (this.TryFindReaderFieldByValue(memberInfo, out var readerField))
                    this.ReaderFields.Add(readerField);
            }
        }
        else
        {
            this.ReaderFields = new();
            var targetMembers = RepositoryHelper.GetMembers(targetType).FindAll(f => f.CanWrite);
            foreach (var memberInfo in targetMembers)
            {
                if (this.TryFindReaderFieldByValue(memberInfo, out var readerField))
                    this.ReaderFields.Add(readerField);
            }
        }
        this.IsSelect = false;
    }
    public virtual bool TryFindReaderFieldByValue(MemberInfo memberInfo, out ReaderField readerField)
    {
        foreach (var tableSegment in this.Tables)
        {
            if (this.TryFindReaderFieldByValue(tableSegment, memberInfo, out readerField))
                return true;
        }
        readerField = null;
        return false;
    }
    public virtual bool TryFindReaderFieldByValue(TableSegment tableSegment, MemberInfo memberInfo, out ReaderField readerField)
    {
        readerField = null;
        if (tableSegment.Fields != null)
        {
            readerField = tableSegment.Fields.Find(f => f.TargetMember.Name == memberInfo.Name);
            if (readerField == null) return false;
            readerField.TargetMember = memberInfo;
        }
        else
        {
            if (!tableSegment.Mapper.TryGetMemberMap(memberInfo.Name, out var memberMapper))
                return false;
            readerField = new ReaderField
            {
                FieldType = ReaderFieldType.Field,
                TargetMember = memberInfo,
                ReaderType = memberInfo.GetMemberType(),
                MemberMapper = memberMapper,
                //MappedTargetType = memberMapper.MappedTargetType,
                //TypeHandler = memberMapper.TypeHandler,
                Value = tableSegment.AliasName + "." + this.OrmProvider.GetFieldName(memberMapper.FieldName)
            };
        }
        return true;
    }
    public virtual void Distinct() => this.IsDistinct = true;
    public virtual void Page(int pageNumber, int pageSize)
    {
        this.pageNumber = pageNumber;
        if (pageNumber > 0) pageNumber--;
        this.offset = pageNumber * pageSize;
        this.limit = pageSize;
        this.ClearUnionSql();
    }
    public virtual void Skip(int skip)
    {
        this.offset = skip;
        if (this.limit.HasValue && this.limit.Value > 0)
            this.pageNumber = (int)Math.Ceiling((double)offset / this.limit.Value) + 1;
        this.ClearUnionSql();
    }
    public virtual void Take(int limit)
    {
        this.limit = limit;
        if (this.offset.HasValue && this.offset.Value > 0)
            this.pageNumber = (int)Math.Ceiling((double)offset / this.limit.Value) + 1;
        this.ClearUnionSql();
    }
    public override SqlSegment VisitMemberAccess(SqlSegment sqlSegment)
    {
        if (sqlSegment.IsDeferredFields)
            return this.VisitDeferredSqlSegment(sqlSegment);

        //Select场景，实体成员访问，返回ReaderField实体类型，ReaderFields并且有值，子ReaderFields的Body可无值
        //Select场景和Where场景，单个字段成员访(包括Json实体类型字段)，返回FromMember，TargetMember，字段类型，Body有值为带有别名的FieldName
        var memberExpr = sqlSegment.Expression as MemberExpression;
        var memberInfo = memberExpr.Member;
        Func<ISqlVisitor, SqlSegment, SqlSegment> formatter = null;
        if (memberExpr.Expression != null)
        {
            //Where(f=>... && !f.OrderId.HasValue && ...)
            //Where(f=>... f.OrderId.Value==10 && ...)
            //Select(f=>... ,f.OrderId.HasValue  ...)
            //Select(f=>... ,f.OrderId.Value==10  ...)
            if (memberInfo.DeclaringType.IsValueType && Nullable.GetUnderlyingType(memberInfo.DeclaringType) != null)
            {
                if (memberInfo.Name == "HasValue")
                {
                    sqlSegment.Push(DeferredOperation.IsNull);
                    sqlSegment.Push(DeferredOperation.Not);
                    return this.Visit(sqlSegment.Next(memberExpr.Expression));
                }
                if (memberInfo.Name == "Value")
                    return this.Visit(sqlSegment.Next(memberExpr.Expression));
            }

            //各种OrmProvider提供的类型实例成员访问，如：DateTime,TimeSpan,String.Length
            if (this.OrmProvider.TryGetMemberAccessSqlFormatter(memberExpr, out formatter))
            {
                //Where(f=>... && f.CreatedAt.Month<5 && ...)
                //Where(f=>... && f.Order.OrderNo.Length==10 && ...)
                var targetSegment = sqlSegment.Next(memberExpr.Expression);
                sqlSegment = formatter.Invoke(this, targetSegment);
                //sqlSegment.TargetMember = memberInfo;
                return sqlSegment;
            }

            //此场景一定是select
            //Select((x, a, b, ... ) => new { x.Grouping, ... });
            if (this.IsGroupingMember(memberExpr))
            {
                ReaderField readerField = null;
                //在子查询中，Select了Group分组对象，为了避免在Clear时，把GroupFields元素清掉，放到一个新列表中
                if (this.GroupByFields.Count > 1)
                {
                    readerField = new ReaderField
                    {
                        IsGroupingField = true,
                        FieldType = ReaderFieldType.Entity,
                        //后续的OrderBy子句不需要，Select子句会设置
                        //ReaderType = memberInfo.GetMemberType(),
                        Fields = new List<ReaderField>()
                    };
                    this.GroupByFields.ForEach(f => readerField.Fields.Add(f));
                }
                else readerField = this.GroupByFields[0];
                return sqlSegment.Change(readerField, SqlType.ReaderField);
            }
            //Select((x, a, b, ... ) => new { x.Grouping.Id, x.Grouping.Name, ... });
            else if (this.IsGroupingMember(memberExpr.Expression as MemberExpression))
            {
                //此时是Grouping对象字段的引用，最外面可能会更改成员名称，要复制一份，防止更改Grouping对象中的字段
                var readerField = this.GroupByFields.Find(f => f.TargetMember.Name == memberInfo.Name);
                return sqlSegment.Change(readerField.Clone(), SqlType.ReaderField);
            }

            //支持多级成员访问，如：f.Order.Seller.Company.Name，f.Order.Seller.Company.Products.Count
            if (memberExpr.TryGetParameters(out var parameterExprs))
            {
                if (parameterExprs.Count > 1)
                    throw new NotSupportedException($"不支持多参数访问，{memberExpr.ToString()}");

                //可能是多级导航属性，也可能是实体类字段访问，如：
                //Select(f => new { f.Disputes.Length, f.Products.Count ...})
                //var isCollectionMember = false;
                //string[] collectionMemberNames = ["Length", "Count"];
                //if (collectionMemberNames.Contains(memberInfo.Name)
                //    && (memberExpr.Expression.Type.IsArray || memberExpr.Expression.Type.IsGenericType
                //    && memberExpr.Expression.Type.GetGenericTypeDefinition() == typeof(ICollection<>)))
                //    isCollectionMember = true;

                string path = null;
                var parameterExpr = parameterExprs[0];
                var parameterName = parameterExpr.Name;

                //最后有效的成员访问，去除集合类导航属性访问，如：f.Order.Seller.Company.Products.Count，最后有效成员访问是Products，而不是Count
                var fromSegment = this.TableAliases[parameterName];
                ReaderField lastReaderField = null;
                MemberMap memberMapper = null;
                //当访问x.Grouping.UserId或是x.Order.Buyer时，会有2个以上的成员访问
                var memberExprs = this.GetMemberExprs(memberExpr, out _);

                var builder = new StringBuilder(fromSegment.AliasName);
                while (memberExprs.TryPop(out var lastMemberExpr))
                {
                    //子查询表通常只有下转1层，只有导航属性才有N层下转
                    if (lastReaderField != null && lastReaderField.Fields != null && lastReaderField.Fields.Count > 0)
                        lastReaderField = lastReaderField.Fields.Find(f => f.TargetMember.Name == lastMemberExpr.Member.Name);
                    //只有正常实体表和Include表才有Mapper，子查询表没有Mapper
                    else if (fromSegment.Mapper != null)
                    {
                        if (!fromSegment.Mapper.TryGetMemberMap(lastMemberExpr.Member.Name, out memberMapper))
                            throw new NotSupportedException($"类{fromSegment.EntityType.FullName}没有成员{lastMemberExpr.Member.Name}，无法访问");
                        if (memberMapper.IsIgnore)
                            throw new NotSupportedException($"类{fromSegment.EntityType.FullName}的成员{lastMemberExpr.Member.Name}是忽略成员无法访问");
                        if (memberMapper.IsNavigation)
                        {
                            if (this.IsWhere)
                                throw new NotSupportedException("不支持使用Include成员作为where条件，可以使用Join关联后再做where条件筛选");

                            //if (memberExprs.Count > 0 && !memberMapper.IsToOne)
                            //    throw new NotSupportedException("暂时不支持引用1:N关系Include导航属性成员访问");

                            //最后一级成员访问，如果是导航属性，且没有下转成员访问，如：f.Order.Seller.Company.Products，直接访问Products集合对象
                            var myTables = memberMapper.IsToOne ? this.Tables : this.IncludeTables;
                            //不是最后一个成员访问
                            //if (memberExprs.Count > 0)
                            builder.Append("." + lastMemberExpr.Member.Name);
                            path = builder.ToString();
                            var nextTableSegment = myTables.Find(f => f.TableType == TableType.Include && f.Path == path);
                            if (nextTableSegment == null)
                                throw new NotSupportedException($"无法访问成员{lastMemberExpr.Member.Name}，请确认是否已经使用Include访问了导航属性{lastMemberExpr.Member.Name}的主表实体");
                            fromSegment = nextTableSegment;
                        }
                    }
                    //子查询和CTE子查询场景，fromSegment.TableType: TableType.FromQuery || TableType.CteSelfRef
                    //OrderBy Select字段
                    else lastReaderField = fromSegment.Fields.Find(f => f.TargetMember.Name == lastMemberExpr.Member.Name);
                }
                //子查询临时表字段访问或是OrderBy使用Select字段访问
                if (lastReaderField != null)
                {
                    if (this.IsOrderBy && fromSegment.TableType == TableType.SelectReaderFields)
                    {
                        var fieldName = lastReaderField.Value.ToString();
                        if (this.IsNeedAlias(lastReaderField))
                            fieldName = lastReaderField.TargetMember.Name;
                        lastReaderField = new ReaderField
                        {
                            FieldType = ReaderFieldType.Field,
                            Value = fieldName
                        };
                    }
                    //子查询中的字段，后续操作可能会更改MemberName，先标识是引用已有ReaderField字段，需要时再做克隆副本                    
                    else lastReaderField.IsRefField = true;
                    sqlSegment.Change(lastReaderField, SqlType.ReaderField);
                }
                else
                {
                    //真实表字段访问
                    if (memberMapper.IsNavigation)
                    {
                        ReaderField refReaderField = null;
                        if (memberMapper.IsToOne)
                        {
                            //有参数访问过，ReaderFields中包含引用的readerField                         
                            refReaderField = this.ReaderFields.Find(f => f.Path == path);
                            if (refReaderField == null)
                            {
                                //没参数访问过，找不到readerField，直接引用实体中的导航属性，需要构造一个readerField
                                refReaderField = new ReaderField
                                {
                                    FieldType = ReaderFieldType.IncludeRef,
                                    Fields = this.FlattenTableFields(fromSegment),
                                    Expression = memberExpr,
                                    Path = path
                                };
                            }
                        }
                        else
                        {
                            //直接引用 Select(f => new { f.Orders, f.Products })
                            refReaderField = new ReaderField
                            {
                                FieldType = ReaderFieldType.DeferredIncludeRef,
                                Fields = this.FlattenTableFields(fromSegment),
                                Expression = memberExpr,
                                Path = path
                            };
                        }
                        sqlSegment.Change(refReaderField, SqlType.ReaderField);
                    }
                    else
                    {
                        //简单字段访问
                        var fieldName = this.OrmProvider.GetFieldName(memberMapper.FieldName);
                        sqlSegment.SqlType = SqlType.OnlyField;
                        sqlSegment.MemberMapper = memberMapper;
                        //sqlSegment.MappedTargetType = memberMapper.MappedTargetType;
                        //sqlSegment.TypeHandler = memberMapper.TypeHandler;
                        sqlSegment.MemberName = memberMapper.MemberName;
                        //Include 1:N时，fromSegment.AliasName为null
                        if (!string.IsNullOrEmpty(fromSegment.AliasName))
                            fieldName = fromSegment.AliasName + "." + fieldName;
                        sqlSegment.Value = fieldName;
                        sqlSegment.IsEnum = memberMapper.UnderlyingType.IsEnum;
                    }
                }
                return sqlSegment;
            }
        }

        //各种静态成员访问，如：DateTime.Now,int.MaxValue,string.Empty
        if (memberExpr.Member.DeclaringType == typeof(DBNull))
            return SqlSegment.Null;

        if (this.OrmProvider.TryGetMemberAccessSqlFormatter(memberExpr, out formatter))
            return formatter.Invoke(this, sqlSegment);

        //访问局部变量或是成员变量，当作常量处理，直接计算，后面统一做参数化处理
        //var orderIds=new List<int>{1,2,3}; Where(f=>orderIds.Contains(f.OrderId));
        //private Order order; Where(f=>f.OrderId==this.Order.Id); this.Order.Id
        //var orderId=10; Select(f=>new {OrderId=orderId,...}
        //Select(f=>new {OrderId=this.Order.Id, ...}
        var value = ValueEvalutor.Evaluate(memberExpr);
        if (value == null) return SqlSegment.Null;
        return sqlSegment.Change(value, SqlType.Variable);
    }
    public override SqlSegment VisitNew(SqlSegment sqlSegment)
    {
        var newExpr = sqlSegment.Expression as NewExpression;
        //Select场景
        if (this.IsSelect && newExpr.Type.Name.StartsWith("<>"))
        {
            this.IsSelectMember = true;
            var readerFields = new List<ReaderField>();
            //为给里面的成员访问提供数据，有参数访问、引用Include成员访问的场景提供数据参数访问的ReaderField查询
            for (int i = 0; i < newExpr.Arguments.Count; i++)
            {
                var mySelectObj = this.AddSelectElement(newExpr.Arguments[i], newExpr.Members[i]);
                if (mySelectObj is ReaderField myReaderField)
                    readerFields.Add(myReaderField);
                else readerFields.AddRange(mySelectObj as List<ReaderField>);
            }
            this.IsSelectMember = false;
            return sqlSegment.Change(readerFields, SqlType.ReaderFields);
        }
        var sqlType = newExpr.HasVariable() ? SqlType.Variable : SqlType.Constant;
        return sqlSegment.Change(ValueEvalutor.Evaluate(newExpr), sqlType);
    }
    public override SqlSegment VisitMemberInit(SqlSegment sqlSegment)
    {
        var memberInitExpr = sqlSegment.Expression as MemberInitExpression;
        //Select场景
        if (this.IsSelect)
        {
            this.IsSelectMember = true;
            var readerFields = new List<ReaderField>();
            //为给里面的成员访问提供数据，有参数访问、引用Include成员访问的场景提供数据参数访问的ReaderField查询
            for (int i = 0; i < memberInitExpr.Bindings.Count; i++)
            {
                if (memberInitExpr.Bindings[i].BindingType != MemberBindingType.Assignment)
                    throw new NotSupportedException("暂时不支持除MemberBindingType.Assignment类型外的成员绑定表达式");
                var memberAssignment = memberInitExpr.Bindings[i] as MemberAssignment;
                var mySelectObj = this.AddSelectElement(memberAssignment.Expression, memberAssignment.Member);
                if (mySelectObj is ReaderField myReaderField)
                    readerFields.Add(myReaderField);
                else readerFields.AddRange(mySelectObj as List<ReaderField>);
            }
            this.IsSelectMember = false;
            return sqlSegment.Change(readerFields, SqlType.ReaderFields);
        }
        var sqlType = memberInitExpr.HasVariable() ? SqlType.Variable : SqlType.Constant;
        return sqlSegment.Change(ValueEvalutor.Evaluate(memberInitExpr), sqlType);
    }
    public virtual void AsCteTable(Type targetType, string tableName)
    {
        if (this.ShardingTables != null && this.ShardingTables.Count > 0)
            throw new NotSupportedException("CTE暂时不支持多分表，只支持单个分表");

        this.IsCteTable = true;
        //每次要新建一个CteQuery对象，避免多次使用同一个对象
        if (this.CteQueryObj == null)
        {
            var cteQueryType = typeof(CteQuery<>).MakeGenericType(targetType);
            this.CteQueryObj = RepositoryHelper.CreateInstance(cteQueryType,
                [typeof(DbContext), typeof(IQueryVisitor)], this.DbContext, this) as ICteQuery;
        }
        if (this.IsRecursive)
        {
            var tempTableName = this.CteQueryObj.TableName;
            this.UnionSql = this.UnionSql.Replace(tempTableName, tableName);
        }
        this.CteQueryObj.Body = this.BuildCteTableSql(tableName, out var readerFields);
        this.CteQueryObj.ReaderFields = readerFields;
        this.CteQueryObj.TableName = tableName;
    }
    public virtual object AddSelectElement(Expression elementExpr, MemberInfo memberInfo)
    {
        SqlSegment sqlSegment = default;
        //生成的SQL，都不加AS子句，在buildSql时再决定是否使用AS子句，因为buildSql时候，Alias别名才能真正确定下来
        //这里的AS子句，到buildSql时候，Alias别名可能会发生变化
        switch (elementExpr.NodeType)
        {
            case ExpressionType.Parameter:
                //两种场景：.Select((x, y) => new { Order = x, x.Seller, x.Buyer, ... }) 和 .Select((x, y) => x)，可能有include操作
                sqlSegment = this.VisitParameter(new SqlSegment { Expression = elementExpr });
                var readerFields = sqlSegment.Value as List<ReaderField>;
                readerFields[0].TargetMember = memberInfo;
                return readerFields;
            case ExpressionType.New:
            case ExpressionType.MemberInit:
                //为了简化SELECT操作，只支持一次New/MemberInit表达式操作
                throw new NotSupportedException("不支持的表达式访问，SELECT语句只支持一次New/MemberInit表达式访问操作");
            case ExpressionType.AndAlso:
            case ExpressionType.OrElse:
                sqlSegment = this.Visit(new SqlSegment { Expression = elementExpr });
                return new ReaderField
                {
                    //当作非字段处理
                    FieldType = ReaderFieldType.Expression,
                    ReaderType = memberInfo.GetMemberType(),
                    Value = this.WrapSql(sqlSegment),
                    TargetMember = memberInfo
                };
            default:
                //常量、变量、表达式、方法调用、成员访问、原始SQL、延迟属性、延迟方法调用等场景
                //.Select((x, y) => new { MaxValue = int.MaxValue, x.Seller, x.Buyer, Now = DateTime.UtcNow })
                //.SelectTo((a, b...) => new DTO{ ActivityTypeEnum = this.GetEmnuName(f.ActivityType) })
                //会有延迟成员访问，静态成员访问，还有方法调用等访问，所以需要VisitAndDeferred
                sqlSegment = this.Visit(new SqlSegment { Expression = elementExpr });
                switch (sqlSegment.SqlType)
                {
                    //成员访问
                    case SqlType.OnlyField:
                        return new ReaderField
                        {
                            FieldType = ReaderFieldType.Field,
                            ReaderType = memberInfo.GetMemberType(),
                            MemberMapper = sqlSegment.MemberMapper,
                            //MappedTargetType = sqlSegment.MappedTargetType,
                            //TypeHandler = sqlSegment.TypeHandler,
                            MemberName = sqlSegment.MemberName,
                            Value = this.WrapSql(sqlSegment),
                            TargetMember = memberInfo
                        };
                    //成员访问，多个字段实体类型的原始SQL，聚合字段
                    case SqlType.ReaderField:
                        var readerField = sqlSegment.Value as ReaderField;
                        //引用已有字段，需要更名，先克隆副本再更名，以免影响原字段后续使用
                        if (!readerField.IsRefField)
                        {
                            readerField.ReaderType = memberInfo.GetMemberType();
                            readerField.TargetMember = memberInfo;
                        }
                        else if (memberInfo.Name != readerField.TargetMember.Name)
                        {
                            var orgReaderField = readerField;
                            readerField = orgReaderField.Clone();
                            readerField.RefField = orgReaderField;
                            orgReaderField.IsRefField = false;
                            readerField.TargetMember = memberInfo;
                        }
                        return readerField;
                    default:
                        //常量、变量、表达式、方法调用、静态成员访问、原始SQL、延迟属性、延迟方法调用等场景
                        //原始SQL，当个字段当作方法调用处理
                        return new ReaderField
                        {
                            FieldType = ReaderFieldType.Expression,
                            ReaderType = memberInfo.GetMemberType(),
                            Value = this.WrapSql(sqlSegment),
                            TargetMember = memberInfo
                        };
                }
        }
    }
    public virtual TableSegment InitTableAlias(LambdaExpression lambdaExpr)
    {
        TableSegment tableSegment = null;
        this.TableAliases.Clear();
        lambdaExpr.Body.TryGetParameterNames(out var parameterNames);
        if (parameterNames == null || parameterNames.Count <= 0)
            return tableSegment;

        //有加新表操作或是Join操作就要清空ReaderFields，以免后续的解析字段时找不到字段
        //OrderBy操作，可以选择Select别名字段，此时优先使用别名
        if (this.ReaderFields != null && this.ReaderFields.Count > 0)
        {
            this.TableAliases.Add(parameterNames[0], tableSegment = new TableSegment
            {
                TableType = TableType.SelectReaderFields,
                Fields = this.ReaderFields,
            });
            return tableSegment;
        }
        var masterTables = this.Tables.FindAll(f => f.IsMaster);
        if (masterTables.Count > 0)
        {
            int index = 0;
            foreach (var parameterExpr in lambdaExpr.Parameters)
            {
                if (typeof(IAggregateSelect).IsAssignableFrom(parameterExpr.Type))
                    continue;
                if (typeof(IFromQuery).IsAssignableFrom(parameterExpr.Type))
                    continue;
                if (!parameterNames.Contains(parameterExpr.Name))
                {
                    index++;
                    continue;
                }
                if (this.TableAliases.ContainsKey(parameterExpr.Name))
                    continue;
                this.TableAliases.Add(parameterExpr.Name, tableSegment = masterTables[index]);
                index++;
            }
        }
        if (this.RefTableAliases != null && parameterNames.Count > this.TableAliases.Count)
        {
            foreach (var parameterName in parameterNames)
            {
                if (this.TableAliases.ContainsKey(parameterName))
                    continue;
                if (!this.RefTableAliases.ContainsKey(parameterName))
                    continue;
                this.TableAliases.Add(parameterName, this.RefTableAliases[parameterName]);
            }
        }
        return tableSegment;
    }
    public virtual void AddSelectFieldsSql(StringBuilder builder, List<ReaderField> readerFields)
    {
        if (readerFields.Count > 1)
        {
            int index = 0;
            foreach (var readerField in readerFields)
            {
                if (readerField.FieldType == ReaderFieldType.IncludeRef
                    || readerField.IsDeferredFields && readerField.Fields == null)
                    continue;

                if (index > 0) builder.Append(',');
                switch (readerField.FieldType)
                {
                    case ReaderFieldType.Entity:
                        this.AddSelectFieldsSql(builder, readerField.Fields);
                        break;
                    case ReaderFieldType.RawSql:
                        builder.Append(readerField.Value.ToString());
                        if (readerField.FieldsCount == 1)
                        {
                            readerField.IsNeedAlias = true;
                            readerField.AliasName = this.OrmProvider.GetFieldName(readerField.TargetMember.Name);
                            builder.Append($" AS {readerField.AliasName}");
                        }
                        break;
                    case ReaderFieldType.Expression:
                        readerField.IsNeedAlias = true;
                        readerField.AliasName = this.OrmProvider.GetFieldName(readerField.TargetMember.Name);
                        if (this.IsManyShardingTables && readerField.IsAggField)
                        {
                            var aliasName = readerField.TargetMember.Name;
                            if (readerField.IsAvgField)
                            {
                                var fieldName1 = readerField.Fields[0].Value.ToString();
                                var fieldNamd2 = readerField.Fields[1].Value.ToString();
                                var aliasName1 = this.OrmProvider.GetFieldName($"{aliasName}_SUM_VALUE");
                                var aliasName2 = this.OrmProvider.GetFieldName($"{aliasName}_COUNT_VALUE");
                                readerField.Fields[0].AliasName = aliasName1;
                                readerField.Fields[1].AliasName = aliasName2;
                                builder.Append($"{fieldName1} AS {aliasName1},{fieldNamd2} AS {aliasName2}");
                            }
                        }
                        else
                        {
                            builder.Append(readerField.Value.ToString());
                            builder.Append($" AS {readerField.AliasName}");
                        }
                        break;
                    default:
                        //延迟方法调用字段，不需要加别名
                        if (readerField.IsDeferredFields)
                            builder.Append(readerField.Value.ToString());
                        else
                        {
                            builder.Append(readerField.Value.ToString());
                            //生成SQL的时候，才加上AS别名
                            if (this.IsNeedAlias(readerField))
                            {
                                readerField.IsNeedAlias = true;
                                //多分表且单分组字段非字段场景，已经设置别名为Grouping
                                if (string.IsNullOrEmpty(readerField.AliasName))
                                    readerField.AliasName = this.OrmProvider.GetFieldName(readerField.TargetMember.Name);
                                builder.Append($" AS {readerField.AliasName}");
                            }
                        }
                        break;
                }
                index++;
            }
        }
        else
        {
            var readerField = readerFields[0];
            string body = null;
            switch (readerField.FieldType)
            {
                case ReaderFieldType.Field:
                    //不引用任何字段，没必要访问数据库
                    if (readerField.IsDeferredFields)
                    {
                        if (readerField.Fields == null)
                            break;
                        this.AddSelectFieldsSql(builder, readerField.Fields);
                    }
                    //TODO: 只有参数字段，不做数据库查询，否则，才使用参数
                    else builder.Append(readerField.Value.ToString());
                    break;
                case ReaderFieldType.Expression:
                    if (this.IsManyShardingTables && readerField.IsAggField)
                    {
                        readerField.IsNeedAlias = true;
                        readerField.AliasName = this.OrmProvider.GetFieldName($"{readerField.AggFunc}_VALUE");
                        if (readerField.IsAvgField)
                        {
                            var fieldName1 = readerField.Fields[0].Value.ToString();
                            var fieldNamd2 = readerField.Fields[1].Value.ToString();
                            var aliasName1 = this.OrmProvider.GetFieldName("SUM_VALUE");
                            var aliasName2 = this.OrmProvider.GetFieldName("COUNT_VALUE");
                            readerField.Fields[0].AliasName = aliasName1;
                            readerField.Fields[1].AliasName = aliasName2;
                            body = $"{fieldName1} AS {aliasName1},{fieldNamd2} AS {aliasName2}";
                        }
                        else body = $"{readerField.Value.ToString()} AS {readerField.AliasName}";
                    }
                    else body = readerField.Value.ToString();
                    builder.Append(body);
                    break;
                case ReaderFieldType.RawSql:
                    builder.Append(readerField.Value.ToString());
                    break;
                case ReaderFieldType.Entity:
                    this.AddSelectFieldsSql(builder, readerField.Fields);
                    break;
                default: throw new NotSupportedException($"不支持的字段类型{readerField.FieldType}");
            }
        }
    }
    public virtual void AddVisitedFieldsSqlWithoutAlias(StringBuilder builder, ReaderField readerField, string suffix = null)
    {
        switch (readerField.FieldType)
        {
            case ReaderFieldType.Entity:
                var readerFields = readerField.Fields;
                for (int i = 0; i < readerFields.Count; i++)
                {
                    if (i > 0) builder.Append(',');
                    this.AddVisitedFieldsSqlWithoutAlias(builder, readerFields[i], suffix);
                }
                break;
            default:
                var body = readerField.Value.ToString();
                //CTE表字段是常量/变量/字段名称，都有可能和声明的字段不一致，所以需要获取CTE表的声明字段
                //body里面的值，是原始的值或是字段名
                if (readerField.TableSegment != null && readerField.TableSegment.TableType == TableType.CteSelfRef)
                    body = $"{readerField.TableSegment.AliasName}.{this.OrmProvider.GetFieldName(readerField.TargetMember.Name)}";
                builder.Append(body);
                if (suffix != null) builder.Append(suffix);
                break;
        }
    }
    public virtual bool IsNeedAlias(ReaderField readerField)
    {
        if (this.IsSecondUnion || this.IsCteTable) return false;
        //单个字段RawSql场景，需要加别名，多个字段RawSql不需要加别名
        if (readerField.FieldType == ReaderFieldType.RawSql)
            return readerField.FieldsCount == 1;
        //GroupFields中的ReaderField只设置了必须加as别名的情况，没有设置TargetMember.Name !=FromMember.Name的情况，这里把这种情况补上
        //PostgreSql时，DistinctOnFields中的ReaderField也是这个场景
        if (readerField.FieldType == ReaderFieldType.Expression) return true;
        //在子查询中，readerField.MemberName和readerField.TargetMember.Name不同，就需要别名
        return readerField.MemberName != readerField.TargetMember.Name;
    }
    public virtual void Clear(bool isClearReaderFields = false)
    {
        if (isClearReaderFields)
            this.ReaderFields = null;
        this.WhereBuilder = null;
        this.TableAliasStart = 'a';

        this.offset = null;
        this.limit = null;
        this.UnionSql = null;
        this.GroupBySql = null;
        this.HavingSql = null;
        this.OrderByFields?.Clear();
        this.IsDistinct = false;
        this.LastIncludeSegment = null;
        this.GroupByFields?.Clear();
        this.IsSecondUnion = false;
        this.IsNeedTableAlias = true;
    }
    public virtual object Clone()
    {
        var visitor = new QueryVisitor(this.DbContext);
        this.CloneTo(visitor);
        return visitor;
    }
    public virtual void CloneTo(IQueryVisitor visitor)
    {
        var queryVisitor = visitor as QueryVisitor;
        queryVisitor.RefTableAliases = this.RefTableAliases;
        queryVisitor.IsNeedTableAlias = this.IsNeedTableAlias;
        queryVisitor.WhereBuilder = this.WhereBuilder;
        queryVisitor.LastWhereOperationType = this.LastWhereOperationType;
        queryVisitor.IncludeTables = this.IncludeTables;
        queryVisitor.RefQueries = this.RefQueries;
        queryVisitor.IsNeedChangeUnionShardingTables = this.IsNeedChangeUnionShardingTables;
        queryVisitor.IsManyShardingTables = this.IsManyShardingTables;
        queryVisitor.ShardingTables = this.ShardingTables;
        queryVisitor.GroupByFields = this.GroupByFields;
        queryVisitor.OrderByFields = this.OrderByFields;
        queryVisitor.UnionSql = this.UnionSql;
        queryVisitor.GroupBySql = this.GroupBySql;
        queryVisitor.HavingSql = this.HavingSql;
        queryVisitor.IsDistinct = this.IsDistinct;
        queryVisitor.IsCteTable = this.IsCteTable;
        queryVisitor.IsUnion = this.IsUnion;
        queryVisitor.IsSecondUnion = this.IsSecondUnion;
        queryVisitor.LastIncludeSegment = this.LastIncludeSegment;

        queryVisitor.IsRecursive = this.IsRecursive;
        queryVisitor.CteQueryObj = this.CteQueryObj;
        queryVisitor.IsNeedPaging = this.IsNeedPaging;

        if (this.DbParameters != null && this.DbParameters.Count > 0)
        {
            visitor.DbParameters = new TheaDbParameterCollection();
            foreach (var dbParameter in this.DbParameters)
            {
                if (dbParameter is ICloneable cloneable)
                    visitor.DbParameters.Add(cloneable.Clone());
            }
        }
        if (this.NextDbParameters != null && this.NextDbParameters.Count > 0)
        {
            visitor.NextDbParameters = new TheaDbParameterCollection();
            foreach (var dbParameter in this.NextDbParameters)
            {
                if (dbParameter is ICloneable cloneable)
                    visitor.NextDbParameters.Add(cloneable.Clone());
            }
        }
        if (this.ReaderFields != null && this.ReaderFields.Count > 0)
        {
            queryVisitor.ReaderFields = new();
            foreach (var readerField in this.ReaderFields)
                queryVisitor.ReaderFields.Add(readerField.Clone());
        }
    }
    public override void Dispose()
    {
        if (this.isDisposed)
            return;
        this.isDisposed = true;

        this.UnionSql = null;
        this.GroupBySql = null;
        this.HavingSql = null;

        this.LastIncludeSegment = null;
        this.GroupByFields = null;
        this.OrderByFields = null;

        base.Dispose();
    }
    public int GetIncludeKey(Type targetType, MemberInfo firstMember, TableSegment includeSegment)
    {
        int pathLength = includeSegment.ParentMemberVisits.Count;
        var builder = new StringBuilder();
        if (firstMember != null)
        {
            pathLength++;
            builder.Append($".{firstMember.Name}");
        }
        foreach (var memberInfo in includeSegment.ParentMemberVisits)
        {
            builder.Append($".{memberInfo.Name}");
        }
        var path = builder.ToString();
        return HashCode.Combine(this.OrmProvider.OrmProviderType, targetType, pathLength, path);
    }
}
public class OrderByField
{
    public ReaderField Field { get; set; }
    public string Suffix { get; set; }
}