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
    protected string OrderBySql { get; set; }
    protected bool IsDistinct { get; set; }
    protected bool IsSelectMember { get; set; }
    public bool IsNeedCommandTableAlias { get; set; }

    public TableSegment LastIncludeSegment { get; set; }
    public List<SqlFieldSegment> GroupByFields { get; set; }
    public List<OrderByField> OrderByFields { get; set; }
    public bool IsCteTable { get; set; }
    public int PageNumber => this.pageNumber;
    public int PageSize => this.limit ?? 0;
    public bool IsNeedPaging { get; set; }

    public QueryVisitor(DbContext dbContext) => this.DbContext = dbContext;
    public QueryVisitor(DbContext dbContext, char tableAsStart, IDataParameterCollection dbParameters = null)
    {
        this.DbContext = dbContext;
        this.TableAsStart = tableAsStart;
        this.DbParameters = dbParameters ?? new TheaDbParameterCollection();
        this.IsNeedTableAlias = true;
    }
    public virtual string BuildSql(bool isBuildCteSql, out List<SqlFieldSegment> readerFields)
    {
        var builder = new StringBuilder();
        if (isBuildCteSql && this.RefQueries != null && this.RefQueries.Count > 0)
        {
            bool isRecursive = false;
            var cteQueries = this.FlattenRefCteTables(this.RefQueries);
            if (cteQueries.Count > 0)
            {
                for (int i = 0; i < cteQueries.Count; i++)
                {
                    if (i > 0) builder.AppendLine(",");
                    builder.Append(cteQueries[i].Body);
                    if (cteQueries[i].IsRecursive)
                        isRecursive = true;
                }
                if (isRecursive)
                    builder.Insert(0, "WITH RECURSIVE ");
                else builder.Insert(0, "WITH ");
                builder.AppendLine();
            }
        }
        readerFields = this.ReaderFields;

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

        //先判断表是否有多分表IsManyShardingTables
        string tableSql = null;
        var hasShardingTables = this.ShardingTables != null && this.ShardingTables.Count > 0;
        if (this.Tables.Count > 0)
        {
            //每个表都要有单独的GUID值，否则有类似的表前缀名，也会被替换导致表名替换错误
            for (int i = 0; i < this.Tables.Count; i++)
            {
                var tableSegment = this.Tables[i];
                string tableName = this.GetTableName(tableSegment);
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
                if (hasShardingTables && this.ShardingTables[0] == tableSegment
                    && tableSegment.TableNames != null && tableSegment.TableNames.Count > 1)
                    this.IsManyShardingTables = true;
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

        if (this.IsManyShardingTables)
        {
            if (!string.IsNullOrEmpty(this.GroupBySql))
            {
                //当有多分表时，有分组，Select字段中，没有完全的分组字段，则需要补全所有分组字段
                foreach (var groupByField in this.GroupByFields)
                {
                    var memberInfo = groupByField.TargetMember ?? groupByField.FromMember;
                    if (this.ReaderFields.Exists(f => f.IsGroupByField && f.TargetMember.Name == memberInfo.Name || f.IsGroupingField))
                        continue;
                    this.ReaderFields.Add(groupByField);
                }
            }
            if (!string.IsNullOrEmpty(this.OrderBySql))
            {
                //当有多分表时，有排序，Select字段中，没有完全的排序字段，则需要补全所有排序字段
                var hasGrouping = this.ReaderFields.Exists(f => f.IsGroupingField);
                foreach (var orderByField in this.OrderByFields)
                {
                    var memberInfo = orderByField.Field.TargetMember ?? orderByField.Field.FromMember;
                    if (this.ReaderFields.Exists(f => f.TargetMember.Name == memberInfo.Name || f.FromMember == memberInfo))
                        continue;
                    if (hasGrouping && this.GroupByFields.Exists(f => f.TargetMember.Name == memberInfo.Name || f.FromMember == memberInfo))
                        continue;
                    this.ReaderFields.Add(orderByField.Field);
                }
            }
        }

        this.AddSelectFieldsSql(builder, this.ReaderFields);
        if (this.IsManyShardingTables && this.IsNeedFormatShardingTables && this.AggFieldAlias != null)
            builder.Append($" AS {this.AggFieldAlias},COUNT(*) AS AVG_COUNT");

        string selectSql = null;
        if (this.IsDistinct)
            selectSql = "DISTINCT " + builder.ToString();
        else selectSql = builder.ToString();

        builder.Clear();
        string whereSql = null;
        if (!string.IsNullOrEmpty(this.WhereSql))
        {
            whereSql = $" WHERE {this.WhereSql}";
            builder.Append(whereSql);
        }
        //有多分表还有Group By操作，每个分表语句中做Group By操作，Union All语句后，还要再做Group By操作
        if (!string.IsNullOrEmpty(this.GroupBySql))
            builder.Append($" GROUP BY {this.GroupBySql}");
        //有多分表还有Group By+Having操作，每个分表语句中只做Group By操作，不做Having操作，在Union All语句后，再做Group By+Having操作
        if (!this.IsManyShardingTables && !string.IsNullOrEmpty(this.HavingSql))
            builder.Append($" HAVING {this.HavingSql}");

        string orderBy = null;
        if (!string.IsNullOrEmpty(this.OrderBySql) && (!this.IsManyShardingTables
            || (this.IsManyShardingTables && !this.offset.HasValue && this.limit.HasValue)))
        {
            orderBy = $"ORDER BY {this.OrderBySql}";
            if (!this.offset.HasValue && !this.limit.HasValue)
                builder.Append(" " + orderBy);
        }
        string others = builder.ToString();

        builder.Clear();
        if (!string.IsNullOrEmpty(headSql))
            builder.Append(headSql);

        if (!this.IsManyShardingTables && (this.offset.HasValue || this.limit.HasValue)
            || (this.IsManyShardingTables && !this.offset.HasValue && this.limit.HasValue))
        {
            //SQL TEMPLATE:SELECT /**fields**/ FROM /**tables**/ /**others**/
            var pageSql = this.OrmProvider.GetPagingTemplate(this.offset, this.limit, orderBy);
            pageSql = pageSql.Replace("/**fields**/", selectSql);
            pageSql = pageSql.Replace("/**tables**/", tableSql);
            pageSql = pageSql.Replace(" /**others**/", others);

            if (this.IsNeedPaging && this.offset.HasValue && this.limit.HasValue)
            {
                var myTableSql = $"{tableSql}{others}";
                if (this.HasAggFields || !string.IsNullOrEmpty(this.GroupBySql))
                    myTableSql = $"(SELECT {selectSql} FROM {tableSql}{others}) a";
                builder.Append($"SELECT COUNT(*) FROM {myTableSql};");
            }
            builder.Append($"{pageSql}");
        }
        else builder.Append($"SELECT {selectSql} FROM {tableSql}{others}");

        if (this.IsManyShardingTables && (!string.IsNullOrEmpty(this.GroupBySql)
            || !string.IsNullOrEmpty(this.OrderBySql) || this.offset.HasValue || this.limit.HasValue || this.HasAggFields))
            this.IsNeedUnionShardingTables = true;

        //判断是否需要SELECT * FROM包装，UNION的子查询中有OrderBy或是Limit，就要包一下SELECT * FROM，否则数据结果不正确
        bool isNeedWrap = ((this.IsUnion || this.IsSecondUnion) && (!string.IsNullOrEmpty(this.OrderBySql) || this.limit.HasValue))
            || (this.IsManyShardingTables && !this.offset.HasValue && this.limit.HasValue);
        if (isNeedWrap)
        {
            builder.Insert(0, "SELECT * FROM (");
            builder.Append($") a");
        }
        sql = builder.ToString();
        builder.Clear();
        return sql;
    }
    public virtual string BuildCommandSql(bool isBuildCteSql, out IDataParameterCollection dbParameters)
    {
        var builder = new StringBuilder();
        var entityMapper = this.Tables[0].Mapper;
        builder.Append($"INSERT INTO {this.GetTableName(this.Tables[0])} (");
        int index = 0;
        //如果ReaderFields没有设置，通常是从Query中来的，ReaderFields是从Query中获取的
        if (this.ReaderFields == null && this.IsFromQuery)
            this.ReaderFields = this.Tables[1].Fields;
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
        if (isBuildCteSql && this.RefQueries != null && this.RefQueries.Count > 0)
        {
            var fieldsSql = builder.ToString();
            builder.Clear();
            bool isRecursive = false;
            var cteQueries = this.FlattenRefCteTables(this.RefQueries);
            if (cteQueries.Count > 0)
            {
                for (int i = 0; i < cteQueries.Count; i++)
                {
                    if (i > 0) builder.AppendLine(",");
                    builder.Append(cteQueries[i].Body);
                    if (cteQueries[i].IsRecursive)
                        isRecursive = true;
                }
                if (isRecursive)
                    builder.Insert(0, "WITH RECURSIVE ");
                else builder.Insert(0, "WITH ");
                builder.AppendLine();
            }
            builder.Append(fieldsSql);
        }
        dbParameters = this.DbParameters;
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

        //先判断表是否有多分表isManySharding
        string tableSql = null;
        var hasShardingTables = this.ShardingTables != null && this.ShardingTables.Count > 0;
        if (this.Tables.Count > 0)
        {
            for (int i = 1; i < this.Tables.Count; i++)
            {
                var tableSegment = this.Tables[i];
                string tableName = this.GetTableName(tableSegment);
                if (i > 1)
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
                if (hasShardingTables && this.ShardingTables[0] == tableSegment
                    && tableSegment.TableNames != null && tableSegment.TableNames.Count > 1)
                    this.IsManyShardingTables = true;
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

        if (this.IsManyShardingTables)
        {
            if (!string.IsNullOrEmpty(this.GroupBySql))
            {
                //当有多分表时，有分组，Select字段中，没有完全的分组字段，则需要补全所有分组字段
                foreach (var groupByField in this.GroupByFields)
                {
                    var memberInfo = groupByField.TargetMember ?? groupByField.FromMember;
                    if (this.ReaderFields.Exists(f => f.IsGroupByField && f.TargetMember.Name == memberInfo.Name || f.IsGroupingField))
                        continue;
                    this.ReaderFields.Add(groupByField);
                }
            }
            if (!string.IsNullOrEmpty(this.OrderBySql))
            {
                //当有多分表时，有排序，Select字段中，没有完全的排序字段，则需要补全所有排序字段
                var hasGrouping = this.ReaderFields.Exists(f => f.IsGroupingField);
                foreach (var orderByField in this.OrderByFields)
                {
                    var memberInfo = orderByField.Field.TargetMember ?? orderByField.Field.FromMember;
                    if (this.ReaderFields.Exists(f => f.TargetMember.Name == memberInfo.Name || f.FromMember == memberInfo))
                        continue;
                    if (hasGrouping && this.GroupByFields.Exists(f => f.TargetMember.Name == memberInfo.Name || f.FromMember == memberInfo))
                        continue;
                    this.ReaderFields.Add(orderByField.Field);
                }
            }
        }

        this.AddSelectFieldsSql(builder, this.ReaderFields);
        if (this.IsManyShardingTables && this.IsNeedFormatShardingTables && this.AggFieldAlias != null)
            builder.Append($" AS {this.AggFieldAlias},COUNT(*) AS AVG_COUNT");

        string selectSql = null;
        if (this.IsDistinct)
            selectSql = "DISTINCT " + builder.ToString();
        else selectSql = builder.ToString();

        builder.Clear();
        if (!string.IsNullOrEmpty(this.WhereSql))
            builder.Append($" WHERE {this.WhereSql}");

        if (!string.IsNullOrEmpty(this.GroupBySql))
            builder.Append($" GROUP BY {this.GroupBySql}");
        if (!string.IsNullOrEmpty(this.HavingSql))
            builder.Append($" HAVING {this.HavingSql}");

        string orderBy = null;
        if (!string.IsNullOrEmpty(this.OrderBySql) && (!this.IsManyShardingTables
            || (this.IsManyShardingTables && !this.offset.HasValue && this.limit.HasValue)))
        {
            orderBy = $"ORDER BY {this.OrderBySql}";
            if (!this.offset.HasValue && !this.limit.HasValue)
                builder.Append(" " + orderBy);
        }
        string others = builder.ToString();

        builder.Clear();
        if (!string.IsNullOrEmpty(headSql))
            builder.Append(headSql);

        if (!this.IsManyShardingTables && (this.offset.HasValue || this.limit.HasValue)
            || (this.IsManyShardingTables && !this.offset.HasValue && this.limit.HasValue))
        {
            //SQL TEMPLATE:SELECT /**fields**/ FROM /**tables**/ /**others**/
            var pageSql = this.OrmProvider.GetPagingTemplate(this.offset, this.limit, orderBy);
            pageSql = pageSql.Replace("/**fields**/", selectSql);
            pageSql = pageSql.Replace("/**tables**/", tableSql);
            pageSql = pageSql.Replace(" /**others**/", others);
            builder.Append($"{pageSql}");
        }
        else builder.Append($"SELECT {selectSql} FROM {tableSql}{others}");

        if (this.IsManyShardingTables && (!string.IsNullOrEmpty(this.GroupBySql)
            || !string.IsNullOrEmpty(this.OrderBySql) || this.offset.HasValue || this.limit.HasValue || this.HasAggFields))
            this.IsNeedUnionShardingTables = true;

        //判断是否需要SELECT * FROM包装，UNION的子查询中有OrderBy或是Limit，就要包一下SELECT * FROM，否则数据结果不正确
        bool isNeedWrap = ((this.IsUnion || this.IsSecondUnion) && (!string.IsNullOrEmpty(this.OrderBySql) || this.limit.HasValue))
            || (this.IsManyShardingTables && !this.offset.HasValue && this.limit.HasValue);
        if (isNeedWrap)
        {
            builder.Insert(0, "SELECT * FROM (");
            builder.Append($") a");
        }
        sql = builder.ToString();
        builder.Clear();
        return sql;
    }
    public virtual string BuildShardingSql(string formatSql)
    {
        var sql = formatSql;
        string groupBy = null;
        string orderBy = null;
        string selectSql = "*";
        var builder = new StringBuilder();
        Func<SqlFieldSegment, string> FieldNameFetcher = readerField =>
        {
            string fieldName = null;
            if (readerField.IsNeedAlias || readerField.IsConstant || readerField.IsVariable
                || readerField.HasParameter || readerField.IsExpression || readerField.IsMethodCall
                || (readerField.TargetMember != null && readerField.FromMember != null
                && readerField.TargetMember.Name != readerField.FromMember.Name))
            {
                fieldName = readerField.TargetMember.Name;
                fieldName = this.OrmProvider.GetFieldName(fieldName);
            }
            else
            {
                fieldName = readerField.Body;
                var startIndex = fieldName.IndexOf('.');
                if (startIndex > 0)
                    fieldName = fieldName.Substring(startIndex + 1);
            }
            return fieldName;
        };
        if (this.GroupByFields != null && this.GroupByFields.Count > 0)
        {
            for (int i = 0; i < this.GroupByFields.Count; i++)
            {
                var fieldName = FieldNameFetcher(this.GroupByFields[i]);
                if (i > 0) builder.Append(',');
                builder.Append(fieldName);
            }
            groupBy = "GROUP BY " + builder.ToString();

            builder.Clear();
            for (int i = 0; i < this.ReaderFields.Count; i++)
            {
                var readerField = this.ReaderFields[i];
                string fieldName = null;
                if (readerField.IsGroupingField)
                {
                    for (int j = 0; j < readerField.Fields.Count; j++)
                    {
                        fieldName = FieldNameFetcher(readerField.Fields[j]);
                        if (j > 0) builder.Append(',');
                        builder.Append(fieldName);
                    }
                    continue;
                }
                fieldName = FieldNameFetcher(readerField);
                if (readerField.IsAggField)
                    fieldName = $"{readerField.ShardingAggFunc}({fieldName}) AS {fieldName}";
                if (i > 0) builder.Append(',');
                builder.Append(fieldName);
            }
            selectSql = builder.ToString();
        }
        if (this.OrderByFields != null && this.OrderByFields.Count > 0)
        {
            builder.Clear();
            for (int i = 0; i < this.OrderByFields.Count; i++)
            {
                var orderByField = this.OrderByFields[i];
                var fieldName = FieldNameFetcher(orderByField.Field);
                if (i > 0) builder.Append(',');
                builder.Append(fieldName);
                if (!string.IsNullOrEmpty(orderByField.OrderSuffix))
                    builder.Append(orderByField.OrderSuffix);
            }
            orderBy = "ORDER BY " + builder.ToString();
        }

        builder.Clear();
        bool isFormated = false;
        if (!string.IsNullOrEmpty(groupBy))
        {
            builder.Append($"SELECT {selectSql} FROM ({formatSql}) a");
            if (!string.IsNullOrEmpty(groupBy))
                builder.Append($" {groupBy}");
            //TODO:有Having操作，要添加Having操作
            sql = builder.ToString();
            isFormated = true;
        }
        //TODO:此处的ReaderFields的字段，如果有join表，需要添加alias表名前缀
        if (this.offset.HasValue || this.limit.HasValue)
        {
            //SQL TEMPLATE:SELECT /**fields**/ FROM /**tables**/ /**others**/
            var pageSql = this.OrmProvider.GetPagingTemplate(this.offset, this.limit, orderBy);
            pageSql = pageSql.Replace("/**fields**/", "*");
            pageSql = pageSql.Replace("/**tables**/", $"({sql}) b");
            pageSql = pageSql.Replace(" /**others**/", "");

            builder.Clear();
            if (this.IsNeedPaging && this.offset.HasValue && this.limit.HasValue)
                builder.Append($"SELECT COUNT(*) FROM ({sql}) a;");
            builder.Append($"{pageSql}");
            sql = builder.ToString();
        }
        else if (!string.IsNullOrEmpty(orderBy))
        {
            builder.Clear();
            if (isFormated) builder.Append(sql);
            else builder.Append($"SELECT * FROM ({sql}) a");
            builder.Append($" {orderBy}");
            sql = builder.ToString();
        }
        else if (this.HasAggFields)
        {
            builder.Clear();
            for (int i = 0; i < this.ReaderFields.Count; i++)
            {
                var readerField = this.ReaderFields[i];
                string fieldName = null;
                if (readerField.IsAggField)
                {
                    fieldName = FieldNameFetcher(readerField);
                    fieldName = $"{readerField.ShardingAggFunc}({fieldName}) AS {fieldName}";
                }
                else fieldName = readerField.Body;
                if (i > 0) builder.Append(',');
                builder.Append(fieldName);
            }
            selectSql = builder.ToString();
            builder.Clear();
            builder.Append($"SELECT {selectSql} FROM ({sql}) a");
            sql = builder.ToString();
        }
        builder.Clear();
        return sql;
    }
    public virtual string BuildCteTableSql(string tableName, out List<SqlFieldSegment> readerFields)
    {
        tableName = this.OrmProvider.GetTableName(tableName);
        var rawSql = this.BuildSql(false, out readerFields);
        var builder = new StringBuilder($"{tableName}(");
        int index = 0;
        foreach (var readerField in readerFields)
        {
            if (readerField.FieldType == SqlFieldType.Field)
            {
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
            //在引用CTE表时，会更新FromMember，此处无需更新
            //readerField.FromMember = readerField.TargetMember; 
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
        this.TableAsStart = tableAsStart;
        foreach (var entityType in entityTypes)
        {
            int tableIndex = tableAsStart + this.Tables.Count;
            this.AddTable(new TableSegment
            {
                EntityType = entityType,
                Mapper = this.MapProvider.GetEntityMap(entityType),
                AliasName = $"{(char)tableIndex}",
                Path = $"{(char)tableIndex}",
                TableType = TableType.Entity,
                IsMaster = true
            });
        }
    }
    public virtual void AddTable(params Type[] entityTypes)
    {
        int tableIndex = this.TableAsStart + this.Tables.Count;
        foreach (var entityType in entityTypes)
        {
            if (entityType == null) continue;
            this.AddTable(new TableSegment
            {
                EntityType = entityType,
                Mapper = this.MapProvider.GetEntityMap(entityType),
                AliasName = $"{(char)(tableIndex++)}",
                Path = $"{(char)tableIndex}",
                TableType = TableType.Entity,
                IsMaster = true
            });
        }
    }
    public TableSegment UseNewQuery(Type targetType, Expression subQueryExpr, bool isFirstTable)
    {
        //repository.FromQuery(f => ... ) 或是 repository.WithQuery(f => ... )，具体参数如下：
        //f => f.From<Order>().Where(o=>o.Id==1) ... 或是 f => cteOrders 或是 f => myRefOrders等
        //或是 f => myCteOrders.Where(o=>o.Id==1) ... 或是 f => myRefOrders.Where(o=>o.Id==1)等
        //都是从引用现有子查询、CTE表、新建子查询生成一个子查询加入到当前Tables中，后续会有Join/Where...等操作，子查询中表别名从'a'开始
        //必须新建一个QueryVisitor对象，不能使用已有表，后续的字段引用只存在于新表中
        IQueryVisitor queryVisiter = null;
        if (isFirstTable) queryVisiter = this;
        else
        {
            queryVisiter = this.CreateQueryVisitor();
            queryVisiter.CteQueryObj = null;
            queryVisiter.IsRecursive = false;
        }
        var fromQuery = new FromQuery(this.DbContext, queryVisiter);
        (var sql, var tableSegment, var readerFields) = this.VisitFromQuery(subQueryExpr, fromQuery);
        if (tableSegment != null) return tableSegment;
        //CTE子查询：
        //如果是直接引用，无后续操作，直接添加新的引用CTE表名，无需生成SQL和build，把CTE子查询对象添加到当前RefQueries中
        //如果引用并有后续操作，在queryVisitor中，添加新的引用CTE表，进行后续操作，最后再build和生成SQL，把CTE子查询对象添加到当前RefQueries中
        //子查询：
        //如果是直接引用，无后续操作，直接在原来的子查询上进行build，只需要拷贝dbParameters、nextDbParameters
        //如果引用并有后续操作，需要在新的queryVisitor上进行build和生成SQL，并把原有所有数据和参数拷贝到新的queryVisitor上

        //从FromQuery对象开始的场景，直接build和生成SQL，就可以，正常逻辑
        if (isFirstTable) this.Clear();
        tableSegment = this.AddJoinTable(targetType, null, TableType.FromQuery, $"({sql})", readerFields);
        this.InitUseQueryReaderFields(tableSegment, readerFields);
        return tableSegment;
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
        var visitor = this.CreateQueryVisitor();
        var fromQuery = new FromQuery(this.DbContext, visitor);
        visitor.IsSecondUnion = true;
        (var sql, _, _) = this.VisitFromQuery(subQueryExpr, fromQuery);
        this.Union(union, targetType, sql);
    }
    private void Union(string union, Type targetType, string subQuerySql)
    {
        //解析第一个UNION子句，需要AS别名
        this.IsUnion = true;
        var rawSql = this.BuildSql(false, out var readerFields);
        rawSql += union + Environment.NewLine + subQuerySql;
        this.Clear();
        this.AddJoinTable(targetType, null, TableType.FromQuery, $"({rawSql})", readerFields);
        //先放到UnionSql中，在AsCteTable方法中，BuildCteTableSql时能得到这个SQL
        this.UnionSql = rawSql;
        this.IsUnion = false;
    }
    public virtual void UnionRecursive(string union, ICteQuery selfQueryObj, Expression subQueryExpr)
    {
        this.IsUnion = true;
        var rawSql = this.BuildSql(false, out var readerFields);
        this.Clear();
        //此时产生的queryObj是一个新的对象，只能用于解析sql，与传进来的queryObj不是同一个对象，舍弃
        //临时产生一个随机表名，在后面的AsCteTable时，再做替换
        var tempTableName = $"__CTE_TABLE_{Guid.NewGuid():N}__";
        selfQueryObj.TableName = tempTableName;
        selfQueryObj.ReaderFields = readerFields;
        selfQueryObj.IsRecursive = true;
        this.CteQueryObj = selfQueryObj;
        this.IsRecursive = true;

        var visitor = this.CreateQueryVisitor();
        var fromQuery = new FromQuery(this.DbContext, visitor);
        visitor.IsSecondUnion = true;
        (var sql, _, _) = this.VisitFromQuery(subQueryExpr, fromQuery, selfQueryObj);
        rawSql += union + Environment.NewLine + sql;
        //先放到UnionSql中，在AsCteTable方法中，BuildCteTableSql时能得到这个SQL
        this.UnionSql = rawSql;
        this.IsUnion = false;
    }
    public virtual void Join(string joinType, Expression joinOn)
       => this.Join(joinType, joinOn, f => this.InitTableAlias(f));
    public virtual void Join(string joinType, Type newEntityType, Expression joinOn)
        => this.Join(joinType, joinOn, f => { this.From(this.TableAsStart, newEntityType); return this.InitTableAlias(f); });
    public virtual void Join(string joinType, Type newEntityType, IQuery subQuery, Expression joinOn)
        => this.Join(joinType, joinOn, f => { this.UseQuery(newEntityType, subQuery, true); return this.InitTableAlias(f); });
    public virtual void Join(string joinType, Type newEntityType, Expression subQueryExpr, Expression joinOn)
        => this.Join(joinType, joinOn, f => { this.UseNewQuery(newEntityType, subQueryExpr, false); return this.InitTableAlias(f); });
    private void Join(string joinType, Expression joinOn, Func<LambdaExpression, TableSegment> joinTableSegmentGetter = null)
    {
        var lambdaExpr = joinOn as LambdaExpression;
        if (!lambdaExpr.Body.GetParameters(out var parameters))
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
    public virtual bool BuildIncludeSql(Type targetType, object target, bool isSingle, out string sql)
    {
        sql = null;
        if (target == null) return false;
        ICollection targets = null;
        if (!isSingle)
        {
            targets = target as ICollection;
            if (targets.Count == 0)
                return false;
        }
        if (this.IncludeTables == null) return false;

        Action<StringBuilder, Action<StringBuilder, IOrmProvider, object>> sqlBuilderInitializer = null;
        if (isSingle)
        {
            sqlBuilderInitializer = (builder, foreignKeysSetter) =>
            {
                foreignKeysSetter.Invoke(builder, this.OrmProvider, target);
                builder.Append(')');
            };
        }
        else
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
        var builder = new StringBuilder();
        for (int i = 0; i < this.IncludeTables.Count; i++)
        {
            if (i > 0 && builder.Length > 0) builder.Append(';');
            var includeTableSegment = this.IncludeTables[i];
            var rootPath = includeTableSegment.Path.Substring(0, 1);
            var rootReaderField = this.ReaderFields.Find(f => f.Path == rootPath);
            if (rootReaderField == null)
                continue;
            //throw new NotSupportedException("Include导航属性成员，一定要Select对应的实体表，如：\r\nrepository.From<Order>()\r\n    .InnerJoin<User>((x, y) => x.SellerId == y.Id)\r\n    .Include((x, y) => x.Buyer)\r\n    .Include((x, y) => y.Company)\r\n    .Select((x, y) => new { Order = x, Seller = y, ... })");
            var firstMember = rootReaderField.TargetMember;

            (var headSql, var sqlInitializer) = this.BuildIncludeSqlGetter(targetType, firstMember, includeTableSegment);
            headSql = string.Format(headSql, this.GetTableName(includeTableSegment));

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
    public virtual void SetIncludeValues(Type targetType, object target, ITheaDataReader reader, bool isSingle)
    {
        var deferredInitializers = new List<(object, Action<object>)>();
        foreach (var includeSegment in this.IncludeTables)
        {
            var navigationType = includeSegment.FromMember.NavigationType;
            var rootPath = includeSegment.Path.Substring(0, 1);
            var rootReaderField = this.ReaderFields.Find(f => f.Path == rootPath);
            //当最外层实体是参数访问时，此值为null
            var firstMember = rootReaderField.TargetMember;
            var includeValues = RepositoryHelper.ReadList(navigationType, reader, this.DbContext);
            Action<object> includeValuesSetter = f => this.SetIncludeValueToTarget(targetType, firstMember, includeSegment, f, includeValues);
            deferredInitializers.Add((includeValues, includeValuesSetter));
        }
        if (isSingle)
        {
            foreach ((var includeValues, var valueSetter) in deferredInitializers)
            {
                if (includeValues is ICollection collection && collection.Count > 0)
                    valueSetter(target);
            }
        }
        else
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
        reader.NextResult();
    }
    public virtual async Task SetIncludeValuesAsync(Type targetType, object target, ITheaDataReader reader, bool isSingle, CancellationToken cancellationToken = default)
    {
        var deferredInitializers = new List<(object, Action<object>)>();
        foreach (var includeSegment in this.IncludeTables)
        {
            var navigationType = includeSegment.FromMember.NavigationType;
            var rootPath = includeSegment.Path.Substring(0, 1);
            var rootReaderField = this.ReaderFields.Find(f => f.Path == rootPath);
            var firstMember = rootReaderField.TargetMember;
            var includeValues = await RepositoryHelper.ReadListAsync(navigationType, reader, this.DbContext, cancellationToken);
            Action<object> includeValuesSetter = f => this.SetIncludeValueToTarget(targetType, firstMember, includeSegment, f, includeValues);
            deferredInitializers.Add((includeValues, includeValuesSetter));
        }
        if (isSingle)
        {
            foreach ((var includeValues, var includeValuesSetter) in deferredInitializers)
            {
                if (includeValues is ICollection collection && collection.Count > 0)
                    includeValuesSetter(target);
            }
        }
        else
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
        lambdaExpr.Body.GetParameters(out var parameters);
        tableAliasInitializer.Invoke(lambdaExpr, parameters);
        (var includeSegment, var isIncludeMany) = this.AddIncludeTables(memberExpr);

        if (filter != null)
        {
            this.IsIncludeMany = true;
            var filterLambdaExpr = filter as LambdaExpression;
            var parameterName = filterLambdaExpr.Parameters[0].Name;
            this.TableAliases.Clear();
            this.TableAliases.Add(parameterName, includeSegment);
            var sqlSegment = this.Visit(new SqlFieldSegment { Expression = filter });
            includeSegment.Filter = sqlSegment.Body;
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
            fromSegment.Mapper ??= this.MapProvider.GetEntityMap(fromType);
            var memberMapper = fromSegment.Mapper.GetMemberMap(currentExpr.Member.Name);

            if (!memberMapper.IsNavigation)
                throw new Exception($"实体{fromType.FullName}的属性{currentExpr.Member.Name}未配置为导航属性");

            //实体类型是成员的声明类型，映射类型不一定是成员的声明类型，一定是成员的Map类型
            //如：成员是UserInfo类型，对应的模型是User类型，UserInfo类型只是User类型的一个子集，成员名称和映射关系完全一致
            var entityType = memberMapper.NavigationType;
            var entityMapper = this.MapProvider.GetEntityMap(entityType, memberMapper.MapType);
            if (entityMapper.KeyMembers.Count > 1)
                throw new Exception($"导航属性表，暂时不支持多个主键字段，实体：{memberMapper.MapType.FullName}");

            memberVisits.Add(currentExpr.Member);
            var rightAlias = $"{(char)(this.TableAsStart + this.Tables.Count)}";
            //path是从顶级级到子级的完整链路，用户查找TableSegment，如：a.Order.Seller.Company
            builder.Append("." + currentExpr.Member.Name);
            //在映射实体时，根据ParentIndex+FromMember值，设置到主表实体的属性中
            if (memberMapper.IsToOne)
            {
                this.Tables.Add(tableSegment = new TableSegment
                {
                    TableType = TableType.Include,
                    JoinType = "LEFT JOIN",
                    EntityType = entityType,
                    Mapper = entityMapper,
                    AliasName = rightAlias,
                    FromTable = fromSegment,
                    FromMember = memberMapper,
                    OnExpr = $"{fromSegment.AliasName}.{this.OrmProvider.GetFieldName(memberMapper.ForeignKey)}={rightAlias}.{this.OrmProvider.GetFieldName(entityMapper.KeyMembers[0].FieldName)}",
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
                    EntityType = entityType,
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
            foreignKeyValueExpr = Expression.Convert(foreignKeyValueExpr, typeof(object));
            var methedInfo = typeof(IOrmProvider).GetMethod(nameof(IOrmProvider.GetQuotedValue));
            var fieldTypeExpr = Expression.Constant(foreignKeyMember.MemberType);
            foreignKeyValueExpr = Expression.Call(ormProviderExpr, methedInfo, fieldTypeExpr, foreignKeyValueExpr);
            methedInfo = typeof(StringBuilder).GetMethod(nameof(StringBuilder.Append), [typeof(string)]);
            blockBodies.Add(Expression.Call(builderExpr, methedInfo, foreignKeyValueExpr));

            var foreignKey = this.OrmProvider.GetFieldName(includeSegment.FromMember.ForeignKey);
            var fields = RepositoryHelper.BuildSelectFieldsSqlPart(this.OrmProvider, includeSegment.Mapper, includeSegment.EntityType);
            var headSql = $"SELECT {fields} FROM {{0}} WHERE {foreignKey} IN (";
            var sqlInitializer = Expression.Lambda<Action<StringBuilder, IOrmProvider, object>>(Expression.Block(blockParameters, blockBodies), builderExpr, ormProviderExpr, targetExpr).Compile();
            return (headSql, sqlInitializer);
        });
    }

    public virtual void Where(Expression whereExpr)
    {
        //为了兼容，Where条件中的Exists，多个表联合查询时，有Where+Exists的场景
        if (!string.IsNullOrEmpty(this.WhereSql))
        {
            this.And(whereExpr);
            return;
        }
        this.IsWhere = true;
        var lambdaExpr = whereExpr as LambdaExpression;
        this.ClearUnionSql();
        this.InitTableAlias(lambdaExpr);
        //不能更改LastWhereOperationType，如果是引用已有子查询，LastWhereOperationType是有值的
        this.WhereSql = this.VisitConditionExpr(lambdaExpr.Body, out var operationType);
        this.LastWhereOperationType = operationType;
        this.IsWhere = false;
    }
    public virtual void And(Expression whereExpr)
    {
        this.IsWhere = true;
        var lambdaExpr = whereExpr as LambdaExpression;
        this.ClearUnionSql();
        this.InitTableAlias(lambdaExpr);
        var conditionSql = this.VisitConditionExpr(lambdaExpr.Body, out var operationType);
        if (string.IsNullOrEmpty(this.WhereSql))
        {
            this.WhereSql = conditionSql;
            this.LastWhereOperationType = operationType;
        }
        else
        {
            if (this.LastWhereOperationType == OperationType.Or)
                this.WhereSql = $"({this.WhereSql})";
            if (operationType == OperationType.Or)
                conditionSql = $"({conditionSql})";
            this.WhereSql += " AND " + conditionSql;
            this.LastWhereOperationType = OperationType.And;
        }
        this.IsWhere = false;
    }
    public virtual void Or(Expression whereExpr)
    {
        this.IsWhere = true;
        var lambdaExpr = whereExpr as LambdaExpression;
        this.ClearUnionSql();
        this.InitTableAlias(lambdaExpr);
        var conditionSql = this.VisitConditionExpr(lambdaExpr.Body, out var operationType);
        if (string.IsNullOrEmpty(this.WhereSql))
        {
            this.WhereSql = conditionSql;
            this.LastWhereOperationType = operationType;
        }
        else
        {
            if (this.LastWhereOperationType == OperationType.And)
                this.WhereSql = $"({this.WhereSql})";
            if (operationType == OperationType.And)
                conditionSql = $"({conditionSql})";
            this.WhereSql += " OR " + conditionSql;
            this.LastWhereOperationType = OperationType.Or;
        }
        this.IsWhere = false;
    }
    public virtual void GroupBy(Expression expr)
    {
        var lambdaExpr = expr as LambdaExpression;
        if (lambdaExpr.Body.NodeType != ExpressionType.New && lambdaExpr.Body.NodeType != ExpressionType.MemberAccess)
            throw new Exception("不支持的表达式访问，GroupBy只支持New或MemberAccess表达式");

        this.ClearUnionSql();
        this.InitTableAlias(lambdaExpr);
        this.GroupByFields = new();
        switch (lambdaExpr.Body.NodeType)
        {
            case ExpressionType.New:
                var builder = new StringBuilder();
                int index = 0;
                var newExpr = lambdaExpr.Body as NewExpression;
                foreach (var argumentExpr in newExpr.Arguments)
                {
                    var memberInfo = newExpr.Members[index];
                    var sqlSegment = this.VisitAndDeferred(new SqlFieldSegment { Expression = argumentExpr });
                    if (builder.Length > 0)
                        builder.Append(',');

                    var fieldName = sqlSegment.Body ?? sqlSegment.Value.ToString();
                    builder.Append(fieldName);
                    sqlSegment.TargetMember = memberInfo;
                    sqlSegment.SegmentType = memberInfo.GetMemberType();
                    this.GroupByFields.Add(sqlSegment);
                    index++;
                }
                //GroupBy SQL中不含别名
                this.GroupBySql = builder.ToString();
                break;
            case ExpressionType.MemberAccess:
                {
                    var memberExpr = lambdaExpr.Body as MemberExpression;
                    var sqlSegment = this.VisitAndDeferred(new SqlFieldSegment { Expression = memberExpr });
                    var fieldName = sqlSegment.Body ?? sqlSegment.Value.ToString();
                    var memberInfo = memberExpr.Member;
                    sqlSegment.TargetMember = memberInfo;
                    sqlSegment.SegmentType = memberInfo.GetMemberType();
                    this.GroupByFields.Add(sqlSegment);
                    this.GroupBySql = fieldName;
                }
                break;
        }
    }
    public virtual void OrderBy(string orderType, Expression expr)
    {
        var lambdaExpr = expr as LambdaExpression;
        this.ClearUnionSql();
        var builder = new StringBuilder();
        if (!string.IsNullOrEmpty(this.OrderBySql))
            builder.Append(this.OrderBySql + ",");

        this.OrderByFields ??= new();
        //能够访问Grouping属性的场景，通常是在最外层的Select子句或是OrderBy子句
        //访问Grouping字段，并且Grouping对象是一个字段
        if (this.IsGroupingMember(lambdaExpr.Body as MemberExpression))
        {
            for (int i = 0; i < this.GroupByFields.Count; i++)
            {
                if (i > 0) builder.Append(',');
                var readerField = this.GroupByFields[i];
                //order by 尽力取源字段值，不管是字段还是表达式，还是函数调用
                var fieldName = readerField.Body ?? readerField.Value.ToString();
                builder.Append(fieldName);
                var orderField = new OrderByField { Field = this.GroupByFields[i] };
                this.OrderByFields.Add(orderField);
                if (orderType == "DESC")
                {
                    builder.Append(" DESC");
                    orderField.OrderSuffix = " DESC";
                }
            }
        }
        else
        {
            this.InitTableAlias(lambdaExpr);
            switch (lambdaExpr.Body.NodeType)
            {
                case ExpressionType.New:
                    int index = 0;
                    var newExpr = lambdaExpr.Body as NewExpression;
                    foreach (var argumentExpr in newExpr.Arguments)
                    {
                        //OrderBy访问分组
                        if (this.IsGroupingMember(argumentExpr as MemberExpression))
                        {
                            for (int i = 0; i < this.GroupByFields.Count; i++)
                            {
                                if (i > 0) builder.Append(',');
                                var readerField = this.GroupByFields[i];
                                //order by 尽力取源字段值，不管是字段还是表达式，还是函数调用
                                var fieldName = readerField.Body ?? readerField.Value.ToString();
                                builder.Append(fieldName);
                                var orderField = new OrderByField { Field = this.GroupByFields[i] };
                                this.OrderByFields.Add(orderField);
                                if (orderType == "DESC")
                                {
                                    builder.Append(" DESC");
                                    orderField.OrderSuffix = " DESC";
                                }
                            }
                        }
                        else
                        {
                            var memberInfo = newExpr.Members[index];
                            var sqlSegment = this.VisitAndDeferred(new SqlFieldSegment { Expression = argumentExpr });
                            if (index > 0) builder.Append(',');
                            //order by 尽力取源字段值，不管是字段还是表达式，还是函数调用
                            builder.Append(sqlSegment.Body ?? sqlSegment.Value.ToString());
                            var orderField = new OrderByField { Field = sqlSegment };
                            this.OrderByFields.Add(orderField);
                            if (orderType == "DESC")
                            {
                                builder.Append(" DESC");
                                orderField.OrderSuffix = " DESC";
                            }
                        }
                        index++;
                    }
                    break;
                case ExpressionType.MemberAccess:
                    var memberExpr = lambdaExpr.Body as MemberExpression;
                    if (this.IsGroupingMember(memberExpr))
                    {
                        for (int i = 0; i < this.GroupByFields.Count; i++)
                        {
                            if (i > 0) builder.Append(',');
                            //order by 尽力取源字段值，不管是字段还是表达式，还是函数调用
                            builder.Append(this.GroupByFields[i].Body ?? this.GroupByFields[i].Value.ToString());
                            var orderField = new OrderByField { Field = this.GroupByFields[i] };
                            this.OrderByFields.Add(orderField);
                            if (orderType == "DESC")
                            {
                                builder.Append(" DESC");
                                orderField.OrderSuffix = " DESC";
                            }
                        }
                    }
                    else if (this.IsGroupingMember(memberExpr.Expression as MemberExpression))
                    {
                        var readerField = this.GroupByFields.Find(f => f.TargetMember.Name == memberExpr.Member.Name);
                        //order by 尽力取源字段值，不管是字段还是表达式，还是函数调用
                        var fieldName = readerField.Body ?? readerField.Value.ToString();
                        builder.Append(fieldName);
                        var orderField = new OrderByField { Field = readerField };
                        this.OrderByFields.Add(orderField);
                        if (orderType == "DESC")
                        {
                            builder.Append(" DESC");
                            orderField.OrderSuffix = " DESC";
                        }
                    }
                    else
                    {
                        var sqlSegment = this.VisitAndDeferred(new SqlFieldSegment { Expression = memberExpr });
                        //order by 尽力取源字段值，不管是字段还是表达式，还是函数调用
                        var fieldName = sqlSegment.Body ?? sqlSegment.Value.ToString();
                        builder.Append(fieldName);
                        var orderField = new OrderByField { Field = sqlSegment };
                        this.OrderByFields.Add(orderField);
                        if (orderType == "DESC")
                        {
                            builder.Append(" DESC");
                            orderField.OrderSuffix = " DESC";
                        }
                    }
                    break;
                default:
                    {
                        var sqlSegment = this.VisitAndDeferred(new SqlFieldSegment { Expression = expr });
                        //order by 尽力取源字段值，不管是字段还是表达式，还是函数调用
                        var fieldName = sqlSegment.Body ?? sqlSegment.Value.ToString();
                        builder.Append(fieldName);
                        if (orderType == "DESC")
                            builder.Append(" DESC");
                    }
                    break;
            }
        }
        this.OrderBySql = builder.ToString();
    }
    public virtual void Having(Expression havingExpr)
    {
        this.IsWhere = true;
        var lambdaExpr = havingExpr as LambdaExpression;
        this.InitTableAlias(lambdaExpr);
        this.HavingSql = this.VisitConditionExpr(lambdaExpr.Body, out _);
        this.IsWhere = false;
    }

    public virtual void SelectGrouping() => this.ReaderFields = this.GroupByFields;
    public virtual void SelectDefault(Expression defaultExpr)
    {
        if (this.ReaderFields != null && this.ReaderFields.Count > 0)
            return;
        this.Select(null, defaultExpr);
    }
    public virtual void Select(string sqlFormat, Expression selectExpr = null)
    {
        this.IsSelect = true;
        if (selectExpr != null)
        {
            var toTargetExpr = selectExpr as LambdaExpression;
            this.ClearUnionSql();
            this.InitTableAlias(toTargetExpr);
            var sqlSegment = new SqlFieldSegment { Expression = toTargetExpr.Body };
            switch (toTargetExpr.Body.NodeType)
            {
                case ExpressionType.Parameter:
                    sqlSegment = this.VisitParameter(sqlSegment);
                    this.ReaderFields = sqlSegment.Value as List<SqlFieldSegment>;
                    this.ReaderFields[0].IsTargetType = true;
                    break;
                case ExpressionType.New:
                case ExpressionType.MemberInit:
                    sqlSegment = this.VisitAndDeferred(sqlSegment);
                    this.ReaderFields = sqlSegment.Value as List<SqlFieldSegment>;
                    break;
                case ExpressionType.MemberAccess:
                    MemberInfo memberInfo = null;
                    if (toTargetExpr.Body is MemberExpression memberExpr)
                        memberInfo = memberExpr.Member;
                    sqlSegment = this.VisitAndDeferred(sqlSegment);
                    sqlSegment.TargetMember = memberInfo;
                    this.ReaderFields = new List<SqlFieldSegment> { sqlSegment };
                    break;
                default:
                    //单个字段或单个值，常量、方法调用、表达式计算场景
                    if (toTargetExpr.Body.NodeType == ExpressionType.Call)
                        sqlSegment.OriginalExpression = toTargetExpr;
                    sqlSegment = this.VisitAndDeferred(sqlSegment);
                    //延迟方法调用，参数可能有多个，返回的ReaderField只有一个
                    //不一定有成员名称，无需设置TableSegment/FromMember/TargetMember，如：.Select(f => f.Age / 10 * 10)
                    //sqlSegment.FieldType = SqlFieldType.Field;
                    sqlSegment.SegmentType ??= selectExpr.Type;
                    //常量和变量body没有值，最后BuildSql时，再进行设置
                    this.ReaderFields = new List<SqlFieldSegment> { sqlSegment };
                    break;
            }
        }
        if (!string.IsNullOrEmpty(sqlFormat))
        {
            //单值操作，SELECT COUNT(1)/*等
            if (this.ReaderFields == null)
                this.ReaderFields = new List<SqlFieldSegment> { new SqlFieldSegment { Body = sqlFormat } };
            else
            {
                //单值操作，SELECT COUNT(DISTINCT b.Id),MAX(b.Amount),COUNT(1)等
                var readerField = this.ReaderFields[0];
                if (this.IsNeedFormatShardingTables && this.AggFieldAlias == "AVG_VALUE")
                    readerField.Body = $"SUM({readerField.Body})";
                //当有多分表并且是AVG场景时，UNION之后，再做AVG操作
                else readerField.Body = string.Format(sqlFormat, readerField.Body);
            }
        }
        this.IsSelect = false;
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
                var sqlSegment = this.VisitAndDeferred(new SqlFieldSegment { Expression = lambdaExpr.Body });
                //特殊的已经确定的列
                this.ReaderFields = sqlSegment.Value as List<SqlFieldSegment>;
            }
            else this.ReaderFields = new();
            bool isExistsFields = false;
            List<string> existsMembers = null;
            if (this.ReaderFields.Count > 0)
            {
                existsMembers = this.ReaderFields.Select(f => f.TargetMember.Name).ToList();
                isExistsFields = true;
            }
            var targetMembers = targetType.GetMembers(BindingFlags.Public | BindingFlags.Instance)
                .Where(f => f.CanWrite()).ToList();

            foreach (var memberInfo in targetMembers)
            {
                if (isExistsFields && existsMembers.Contains(memberInfo.Name)) continue;
                if (this.TryFindReaderField(memberInfo, out var readerField))
                    this.ReaderFields.Add(readerField);
            }
        }
        else
        {
            this.ReaderFields = new();
            var targetMembers = targetType.GetMembers(BindingFlags.Public | BindingFlags.Instance)
                .Where(f => f.CanWrite()).ToList();

            foreach (var memberInfo in targetMembers)
            {
                if (this.TryFindReaderField(memberInfo, out var readerField))
                    this.ReaderFields.Add(readerField);
            }
        }
        this.IsFromQuery = false;
        this.IsSelect = false;
    }
    public virtual bool TryFindReaderField(MemberInfo memberInfo, out SqlFieldSegment readerField)
    {
        foreach (var tableSegment in this.Tables)
        {
            if (this.TryFindReaderField(tableSegment, memberInfo, out readerField))
                return true;
        }
        readerField = null;
        return false;
    }
    public virtual bool TryFindReaderField(TableSegment tableSegment, MemberInfo memberInfo, out SqlFieldSegment readerField)
    {
        readerField = null;
        if (tableSegment.Fields != null)
        {
            readerField = tableSegment.Fields.Find(f => f.FromMember.Name == memberInfo.Name);
            if (readerField == null) return false;
            readerField.TargetMember = memberInfo;
        }
        else
        {
            if (!tableSegment.Mapper.TryGetMemberMap(memberInfo.Name, out var memberMapper))
                return false;
            var segmentType = memberInfo.GetMemberType();
            Type expectType = null;
            if (segmentType.IsEnumType(out var underlyingType, out _))
                expectType = underlyingType;
            readerField = new SqlFieldSegment
            {
                FieldType = SqlFieldType.Field,
                HasField = true,
                FromMember = memberMapper.Member,
                TargetMember = memberInfo,
                SegmentType = segmentType,
                ExpectType = expectType,
                NativeDbType = memberMapper.NativeDbType,
                TypeHandler = memberMapper.TypeHandler,
                Body = tableSegment.AliasName + "." + this.OrmProvider.GetFieldName(memberMapper.FieldName)
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
    public override SqlFieldSegment VisitMemberAccess(SqlFieldSegment sqlSegment)
    {
        //Select场景，实体成员访问，返回ReaderField实体类型，ReaderFields并且有值，子ReaderFields的Body可无值
        //Select场景和Where场景，单个字段成员访(包括Json实体类型字段)，返回FromMember，TargetMember，字段类型，Body有值为带有别名的FieldName
        var memberExpr = sqlSegment.Expression as MemberExpression;
        var memberInfo = memberExpr.Member;

        if (sqlSegment.IsDeferredFields && this.IsSelect)
        {
            //延迟属性访问，两种场景：
            //主动延迟方法调用：如，把返回的枚举列转成描述，参数就是枚举列，返回值是对应的描述
            string fields = null;
            List<SqlFieldSegment> readerFields = null;
            var visitor = new MemberVisitor();
            visitor.Visit(memberExpr);
            //$"{f.OrderNo} : {f.TotalAmount.ToString("C")}"
            //f.TotalAmount.ToString("C")
            //"TotalAmount: " + (f.Price * f.Quantity).ToString("C")
            //this.DeferredInvoke(f.Price, f.Quantity)
            if (visitor.Members.Count > 0)
            {
                readerFields = new List<SqlFieldSegment>();
                var builder = new StringBuilder();
                foreach (var argsExpr in visitor.Members)
                {
                    var argumentSegment = this.VisitAndDeferred(new SqlFieldSegment { Expression = argsExpr });
                    if (argumentSegment.HasField)
                    {
                        sqlSegment.HasField = true;
                        var fieldName = argumentSegment.Body;
                        readerFields.Add(new SqlFieldSegment
                        {
                            SegmentType = argsExpr.Type,
                            TargetMember = argsExpr.Member,
                            NativeDbType = argumentSegment.NativeDbType,
                            TypeHandler = argumentSegment.TypeHandler
                        });
                        if (builder.Length > 0)
                            builder.Append(',');
                        builder.Append(fieldName);
                    }
                }
                if (readerFields.Count > 0)
                    fields = builder.ToString();
            }

            if (readerFields == null)
                fields = "NULL";
            sqlSegment.IsDeferredFields = true;
            sqlSegment.FieldType = SqlFieldType.DeferredFields;
            sqlSegment.Body = fields;
            sqlSegment.DeferredExpression = memberExpr;
            sqlSegment.Fields = readerFields;
            sqlSegment.IsMethodCall = true;
            return sqlSegment;
        }

        MemberAccessSqlFormatter formatter = null;
        if (memberExpr.Expression != null)
        {
            //Where(f=>... && !f.OrderId.HasValue && ...)
            //Where(f=>... f.OrderId.Value==10 && ...)
            //Select(f=>... ,f.OrderId.HasValue  ...)
            //Select(f=>... ,f.OrderId.Value==10  ...)
            if (Nullable.GetUnderlyingType(memberExpr.Member.DeclaringType) != null)
            {
                if (memberExpr.Member.Name == nameof(Nullable<bool>.HasValue))
                {
                    sqlSegment.Push(new DeferredExpr { OperationType = OperationType.Equal, Value = SqlFieldSegment.Null });
                    sqlSegment.Push(new DeferredExpr { OperationType = OperationType.Not });
                    return this.Visit(sqlSegment.Next(memberExpr.Expression));
                }
                else if (memberExpr.Member.Name == nameof(Nullable<bool>.Value))
                    return this.Visit(sqlSegment.Next(memberExpr.Expression));
                else throw new ArgumentException($"不支持的MemberAccess操作，表达式'{memberExpr}'返回值不是boolean类型");
            }

            //各种类型实例成员访问，如：DateTime,TimeSpan,String.Length,List.Count
            if (this.OrmProvider.TryGetMemberAccessSqlFormatter(memberExpr, out formatter))
            {
                //Where(f=>... && f.CreatedAt.Month<5 && ...)
                //Where(f=>... && f.Order.OrderNo.Length==10 && ...)
                var targetSegment = sqlSegment.Next(memberExpr.Expression);
                sqlSegment = formatter.Invoke(this, targetSegment);
                sqlSegment.TargetMember = memberExpr.Member;
                return sqlSegment;
            }

            //此场景一定是select
            //Select((x, a, b, ... ) => new { x.Grouping, ... });
            if (this.IsGroupingMember(memberExpr))
            {
                List<SqlFieldSegment> groupFields = new();
                //在子查询中，Select了Group分组对象，为了避免在Clear时，把GroupFields元素清掉，放到一个新列表中
                if (this.GroupByFields.Count > 1)
                {
                    this.GroupByFields.ForEach(f => groupFields.Add(f.Clone()));
                    sqlSegment.FieldType = SqlFieldType.Entity;
                    sqlSegment.HasField = true;
                    sqlSegment.FromMember = memberInfo;
                    sqlSegment.TargetMember = memberInfo;
                    sqlSegment.SegmentType = memberInfo.GetMemberType();
                    sqlSegment.Fields = groupFields;
                }
                else sqlSegment = this.GroupByFields[0].Clone();
                sqlSegment.IsGroupingField = true;
                return sqlSegment;
            }
            //Select((x, a, b, ... ) => new { x.Grouping.Id, x.Grouping.Name, ... });
            else if (this.IsGroupingMember(memberExpr.Expression as MemberExpression))
            {
                //此时是Grouping对象字段的引用，最外面可能会更改成员名称，要复制一份，防止更改Grouping对象中的字段
                var readerField = this.GroupByFields.Find(f => f.TargetMember.Name == memberInfo.Name);
                sqlSegment = readerField.Clone();
                sqlSegment.IsGroupByField = true;
                return sqlSegment;
            }
            if (memberExpr.IsParameter(out var parameterName))
            {
                string path = null;
                TableSegment fromSegment = null;

                var rootTableSegment = this.TableAliases[parameterName];
                if (rootTableSegment.TableType == TableType.Entity)
                {
                    var builder = new StringBuilder(rootTableSegment.AliasName);
                    var memberExprs = this.GetMemberExprs(memberExpr, out _);
                    if (memberExprs.Count > 1)
                    {
                        while (memberExprs.Count > 1)
                        {
                            var currentExpr = memberExprs.Pop();
                            builder.Append("." + currentExpr.Member.Name);
                        }
                        path = builder.ToString();
                        fromSegment = this.Tables.Find(f => f.TableType == TableType.Include && f.Path == path);
                    }
                    else fromSegment = rootTableSegment;
                }
                else fromSegment = rootTableSegment;

                if (memberExpr.Type.IsEntityType(out _))
                {
                    //TODO:匿名实体类型类似于Grouping对象，在子查询后续会支持
                    if (this.IsFromQuery && this.IsSelectMember)
                        throw new NotSupportedException("FROM子查询中不支持实体类型成员MemberAccess表达式访问，只支持基础字段访问");

                    //实体类型字段，三个场景：Json类型实体字段成员访问(包含实体表和子查询表)，Include导航实体类型成员访问(包括1:1,1:N关系)，
                    //Grouping分组对象的访问(包含当前查询中的和子查询表中的)                  
                    //子查询时，Mapper为null
                    if (fromSegment.Mapper != null)
                    {
                        //非子查询场景
                        var memberMapper = fromSegment.Mapper.GetMemberMap(memberExpr.Member.Name);
                        if (memberMapper.IsIgnore)
                            throw new NotSupportedException($"类{fromSegment.EntityType.FullName}的成员{memberExpr.Member.Name}是忽略成员无法访问");

                        if (memberMapper.IsNavigation)
                        {
                            //引用导航属性
                            if (this.IsWhere)
                                throw new NotSupportedException("不支持使用Include成员作为where条件，可以使用Join关联后再做where条件筛选");

                            path ??= fromSegment.Path.Replace(fromSegment.AliasName, parameterName);
                            path += "." + memberExpr.Member.Name;
                            var refReaderField = this.ReaderFields.Find(f => f.Path == path);
                            if (refReaderField == null)
                                throw new NotSupportedException("Select访问Include成员，要先Select访问Include成员的主表实体，如：.Select((x, y) =&gt; new { Order = x, x.Seller, x.Buyer, ... })");

                            //引用实体类型导航属性，当前导航属性可能还会有Include导航属性，所以构造时只给默认值
                            //在初始化完最外层实体后，再做赋值
                            if (!memberMapper.IsToOne) throw new NotSupportedException("暂时不支持引用1:N关系Include导航属性成员访问");
                            var readerField = this.ReaderFields.Find(f => f.Path == path);
                            //只有select场景才会Include对象
                            sqlSegment.HasField = true;
                            sqlSegment.FieldType = SqlFieldType.IncludeRef;
                            sqlSegment.FromMember = memberMapper.Member;
                            sqlSegment.Value = refReaderField;
                            return sqlSegment;
                        }
                        else
                        {
                            //引用Json实体类型字段
                            if (memberMapper.TypeHandler == null)
                                throw new NotSupportedException($"类{fromSegment.EntityType.FullName}的成员{memberExpr.Member.Name}是实体类型，未配置导航属性也没有配置TypeHandler");

                            var fieldName = this.OrmProvider.GetFieldName(memberMapper.FieldName);
                            if (this.IsNeedTableAlias) fieldName = fromSegment.AliasName + "." + fieldName;
                            sqlSegment.FieldType = SqlFieldType.Field;
                            sqlSegment.HasField = true;
                            sqlSegment.FromMember = memberMapper.Member;
                            sqlSegment.TargetMember = memberInfo;
                            sqlSegment.SegmentType = memberMapper.MemberType;
                            if (memberMapper.UnderlyingType.IsEnum)
                                sqlSegment.ExpectType = memberMapper.UnderlyingType;
                            sqlSegment.NativeDbType = memberMapper.NativeDbType;
                            sqlSegment.TypeHandler = memberMapper.TypeHandler;
                            sqlSegment.Body = fieldName;
                        }
                    }
                    else
                    {
                        //子查询和CTE子查询场景
                        //子查询和CTE子查询中，Select了Grouping分组对象或是临时匿名对象，目前子查询，只有分组对象才是实体类型，后续会支持匿名对象
                        //OrderBy中的实体类型对象访问已经单独处理了，包括Grouping对象
                        //fromSegment.TableType: TableType.FromQuery || TableType.CteSelfRef
                        var readerField = fromSegment.Fields.Find(f => f.TargetMember.Name == memberExpr.Member.Name);
                        sqlSegment.HasField = true;
                        sqlSegment.FieldType = readerField.FieldType;
                        sqlSegment.FromMember = readerField.TargetMember;
                        sqlSegment.TargetMember = readerField.TargetMember;
                        sqlSegment.SegmentType = readerField.SegmentType;
                        if (readerField.SegmentType.IsEnumType(out var underlyingType))
                            sqlSegment.ExpectType = underlyingType;
                        sqlSegment.NativeDbType = readerField.NativeDbType;
                        sqlSegment.TypeHandler = readerField.TypeHandler;
                        sqlSegment.Body = readerField.Body;
                        sqlSegment.Fields = readerField.Fields;
                    }
                }
                else
                {
                    //Where(f => f.Amount > 5)
                    //Select(f => new { f.OrderId, f.Disputes ...})                    
                    string fieldName = null;
                    sqlSegment.HasField = true;

                    if (fromSegment.Mapper != null)
                    {
                        var memberMapper = fromSegment.Mapper.GetMemberMap(memberExpr.Member.Name);
                        if (memberMapper.IsIgnore)
                            throw new Exception($"类{fromSegment.EntityType.FullName}的成员{memberMapper.MemberName}是忽略成员无法访问");
                        if (memberMapper.MemberType.IsEntityType(out _) && !memberMapper.IsNavigation && memberMapper.TypeHandler == null)
                            throw new Exception($"类{fromSegment.EntityType.FullName}的成员{memberExpr.Member.Name}不是值类型，未配置为导航属性也没有配置TypeHandler");

                        sqlSegment.FieldType = SqlFieldType.Field;
                        sqlSegment.FromMember = memberMapper.Member;
                        sqlSegment.TargetMember = memberMapper.Member;
                        if (memberMapper.UnderlyingType.IsEnum)
                            sqlSegment.ExpectType = memberMapper.UnderlyingType;
                        sqlSegment.SegmentType = memberMapper.MemberType;
                        sqlSegment.NativeDbType = memberMapper.NativeDbType;
                        sqlSegment.TypeHandler = memberMapper.TypeHandler;

                        //查询时，IsNeedAlias始终为true，新增、更新、删除时，引用联表操作时，才会为true
                        fieldName = this.OrmProvider.GetFieldName(memberMapper.FieldName);
                        //IncludeMany表时，fromSegment.AliasName为null
                        if (this.IsNeedTableAlias && !string.IsNullOrEmpty(fromSegment.AliasName))
                            fieldName = fromSegment.AliasName + "." + fieldName;

                        //设置是否是分组字段，以便后面分表添加分组字段处理
                        if (this.IsSelect && this.GroupByFields != null && this.GroupByFields.Count > 0)
                            sqlSegment.IsGroupByField = this.GroupByFields.Exists(f => f.FromMember == memberMapper.Member);
                        sqlSegment.Body = fieldName;
                    }
                    else
                    {
                        //if (fromSegment.TableType == TableType.FromQuery || fromSegment.TableType == TableType.CteSelfRef)
                        //访问子查询表的成员，子查询表没有Mapper，也不会有实体类型成员
                        //Json的实体类型字段                       
                        //子查询，Select了Grouping分组对象或是匿名对象，目前子查询中，只支持一层，匿名对象后续会做支持
                        //取AS后的字段名，与原字段名不一定一样，AS后的字段名与memberExpr.Member.Name一致
                        SqlFieldSegment readerField = null;
                        if (memberExpr.Expression.NodeType != ExpressionType.Parameter)
                        {
                            var parentMemberExpr = memberExpr.Expression as MemberExpression;
                            var parenetReaderField = fromSegment.Fields.Find(f => f.TargetMember.Name == parentMemberExpr.Member.Name);
                            readerField = parenetReaderField.Fields.Find(f => f.TargetMember.Name == memberExpr.Member.Name);
                        }
                        else
                        {
                            var fromReaderFields = fromSegment.Fields;
                            if (fromReaderFields.Count == 1 && fromReaderFields[0].FieldType != SqlFieldType.Field)
                                fromReaderFields = fromReaderFields[0].Fields;
                            readerField = fromReaderFields.Find(f => f.TargetMember.Name == memberExpr.Member.Name);
                        }
                        sqlSegment.FieldType = readerField.FieldType;
                        sqlSegment.FromMember = readerField.TargetMember ?? readerField.FromMember;
                        sqlSegment.TargetMember = readerField.TargetMember;
                        sqlSegment.SegmentType = readerField.SegmentType;
                        if (readerField.SegmentType.IsEnumType(out var underlyingType))
                            sqlSegment.ExpectType = underlyingType;

                        sqlSegment.NativeDbType = readerField.NativeDbType;
                        sqlSegment.TypeHandler = readerField.TypeHandler;
                        sqlSegment.TableSegment = fromSegment;
                        if (fromSegment.TableType == TableType.TempReaderFields)
                            fieldName = readerField.Body;
                        else
                        {
                            fieldName = this.OrmProvider.GetFieldName(memberExpr.Member.Name);
                            if (this.IsNeedTableAlias) fieldName = fromSegment.AliasName + "." + fieldName;
                        }
                        sqlSegment.Body = fieldName;
                        sqlSegment.Fields = readerField.Fields;
                        sqlSegment.IsNeedAlias = false;
                    }
                }
                return sqlSegment;
            }
        }

        if (memberExpr.Member.DeclaringType == typeof(DBNull))
            return SqlFieldSegment.Null;

        //各种静态成员访问，如：DateTime.Now,int.MaxValue,string.Empty
        if (this.OrmProvider.TryGetMemberAccessSqlFormatter(memberExpr, out formatter))
        {
            sqlSegment = formatter.Invoke(this, sqlSegment);
            sqlSegment.SegmentType = memberExpr.Type;
            sqlSegment.TargetMember = memberExpr.Member;
            sqlSegment.IsNeedAlias = true;
            return sqlSegment;
        }

        //访问局部变量或是成员变量，当作常量处理，直接计算，后面统一做参数化处理
        //var orderIds=new List<int>{1,2,3}; Where(f=>orderIds.Contains(f.OrderId)); orderIds
        //private Order order; Where(f=>f.OrderId==this.Order.Id); this.Order.Id
        //var orderId=10; Select(f=>new {OrderId=orderId,...}
        //Select(f=>new {OrderId=this.Order.Id, ...}
        this.Evaluate(sqlSegment);

        sqlSegment.IsConstant = false;
        sqlSegment.IsVariable = true;
        return sqlSegment;
    }
    public override SqlFieldSegment VisitNew(SqlFieldSegment sqlSegment)
    {
        var newExpr = sqlSegment.Expression as NewExpression;
        //Select场景
        if (this.IsSelect && newExpr.Type.Name.StartsWith("<>"))
        {
            this.IsSelectMember = true;
            var readerFields = new List<SqlFieldSegment>();
            //为给里面的成员访问提供数据，有参数访问、引用Include成员访问的场景提供数据参数访问的ReaderField查询
            this.ReaderFields = readerFields;
            for (int i = 0; i < newExpr.Arguments.Count; i++)
            {
                this.AddSelectElement(newExpr.Arguments[i], newExpr.Members[i], readerFields);
            }
            this.IsSelectMember = false;
            sqlSegment.FieldType = SqlFieldType.Entity;
            return sqlSegment.ChangeValue(readerFields);
        }
        return this.Evaluate(sqlSegment);
    }
    public override SqlFieldSegment VisitMemberInit(SqlFieldSegment sqlSegment)
    {
        var memberInitExpr = sqlSegment.Expression as MemberInitExpression;
        //Select场景
        if (this.IsSelect)
        {
            this.IsSelectMember = true;
            var readerFields = new List<SqlFieldSegment>();
            //为给里面的成员访问提供数据，有参数访问、引用Include成员访问的场景提供数据参数访问的ReaderField查询
            this.ReaderFields = readerFields;
            for (int i = 0; i < memberInitExpr.Bindings.Count; i++)
            {
                if (memberInitExpr.Bindings[i].BindingType != MemberBindingType.Assignment)
                    throw new NotSupportedException("暂时不支持除MemberBindingType.Assignment类型外的成员绑定表达式");
                var memberAssignment = memberInitExpr.Bindings[i] as MemberAssignment;
                this.AddSelectElement(memberAssignment.Expression, memberAssignment.Member, readerFields);
            }
            this.IsSelectMember = false;
            return sqlSegment.ChangeValue(readerFields);
        }
        return this.Evaluate(sqlSegment);
    }
    public virtual void AsCteTable(Type targetType, string tableName)
    {
        if (this.ShardingTables != null && this.ShardingTables.Count > 0)
            throw new NotSupportedException("CTE暂时不支持多分表，只支持单个分表");

        this.IsCteTable = true;
        //每次要新建一个CteQuery对象，避免多次使用同一个对象
        if (this.CteQueryObj != null && this.IsRecursive && !string.IsNullOrEmpty(this.UnionSql))
        {
            var tempTableName = this.CteQueryObj.TableName;
            this.UnionSql = this.UnionSql.Replace(tempTableName, tableName);
        }
        if (this.CteQueryObj == null)
        {
            var cteQueryType = typeof(CteQuery<>).MakeGenericType(targetType);
            this.CteQueryObj = RepositoryHelper.CreateInstance(cteQueryType,
                [typeof(DbContext), typeof(IQueryVisitor)], this.DbContext, this) as ICteQuery;
        }
        this.CteQueryObj.Body = this.BuildCteTableSql(tableName, out var readerFields);
        this.CteQueryObj.ReaderFields = readerFields;
        this.CteQueryObj.TableName = tableName;
    }
    public virtual void AddSelectElement(Expression elementExpr, MemberInfo memberInfo, List<SqlFieldSegment> readerFields)
    {
        var sqlSegment = new SqlFieldSegment { Expression = elementExpr };
        switch (elementExpr.NodeType)
        {
            case ExpressionType.Parameter:
                if (this.IsFromQuery)
                    throw new NotSupportedException("FROM子查询中不支持参数Parameter表达式访问，只支持基础字段访问访问");
                //两种场景：.Select((x, y) => new { Order = x, x.Seller, x.Buyer, ... }) 和 .Select((x, y) => x)，可能有include操作
                sqlSegment = this.VisitParameter(sqlSegment);
                var tableReaderFields = sqlSegment.Value as List<SqlFieldSegment>;
                tableReaderFields[0].FromMember = memberInfo;
                tableReaderFields[0].TargetMember = memberInfo;
                readerFields.AddRange(tableReaderFields);
                break;
            case ExpressionType.New:
            case ExpressionType.MemberInit:
                //为了简化SELECT操作，只支持一次New/MemberInit表达式操作
                throw new NotSupportedException("不支持的表达式访问，SELECT语句只支持一次New/MemberInit表达式访问操作");
            case ExpressionType.MemberAccess:
                sqlSegment = this.VisitAndDeferred(sqlSegment);
                if (sqlSegment.FieldType != SqlFieldType.IncludeRef)
                {
                    this.GetQuotedValue(sqlSegment, true);
                    if (sqlSegment.IsConstant || sqlSegment.IsVariable || sqlSegment.HasParameter || sqlSegment.IsExpression
                        || sqlSegment.IsMethodCall || sqlSegment.FromMember != null && sqlSegment.FromMember.Name != memberInfo.Name)
                        sqlSegment.IsNeedAlias = true;
                }
                sqlSegment.TargetMember = memberInfo;
                sqlSegment.SegmentType = memberInfo.GetMemberType();
                readerFields.Add(sqlSegment);
                break;
            case ExpressionType.AndAlso:
            case ExpressionType.OrElse:
                var trueExpr = this.OrmProvider.GetQuotedValue(typeof(bool), true);
                var falseExpr = this.OrmProvider.GetQuotedValue(typeof(bool), false);
                sqlSegment = this.VisitAndDeferred(sqlSegment);
                var boolExpr = this.GetQuotedValue(sqlSegment, false);
                sqlSegment.Body = $"(CASE WHEN {boolExpr} THEN {trueExpr} ELSE {falseExpr} END)";
                sqlSegment.TargetMember = memberInfo;
                sqlSegment.IsNeedAlias = true;
                sqlSegment.SegmentType = memberInfo.GetMemberType();
                readerFields.Add(sqlSegment);
                break;
            default:
                //常量或方法或表达式访问
                sqlSegment = this.VisitAndDeferred(sqlSegment);
                this.GetQuotedValue(sqlSegment, true);
                //DeferredFields场景
                //函数调用，参数引用多个字段
                //.SelectTo<DTO>((a, b ...) => new DTO
                //{
                //    ActivityTypeEnum = this.GetEmnuName(f.ActivityType)
                //})
                if (!sqlSegment.IsDeferredFields)
                {
                    sqlSegment.FieldType = SqlFieldType.Field;
                    //只有常量、方法调用、表达式计算，没有设置NativeDbType和TypeHandler，需要根据memberInfo类型获取
                    //常量和变量，暂时不做GetQuotedValue处理，在BuildSql时候，再进行处理，Value值保留
                    if (sqlSegment.IsConstant || sqlSegment.IsVariable || sqlSegment.HasParameter || sqlSegment.IsExpression
                        || sqlSegment.IsMethodCall || sqlSegment.FromMember != null && sqlSegment.FromMember.Name != memberInfo.Name)
                        sqlSegment.IsNeedAlias = true;
                }
                sqlSegment.TargetMember = memberInfo;
                //常量或变量场景，此值为null
                sqlSegment.SegmentType = memberInfo.GetMemberType();
                readerFields.Add(sqlSegment);
                break;
        }
    }
    public virtual TableSegment InitTableAlias(LambdaExpression lambdaExpr)
    {
        TableSegment tableSegment = null;
        this.TableAliases.Clear();
        lambdaExpr.Body.GetParameterNames(out var parameterNames);
        if (parameterNames == null || parameterNames.Count <= 0)
            return tableSegment;

        //为了实现Select之后，有的表达式计算、函数调用或是普通字段，都有可能改变了名字，为了之后select之后还可以OrderBy操作，
        //在解析字段的时候，如果ReaderFields有值说明已经select过了(Union除外)，就取ReaderFields中的字段，否则就取原表中的字段
        //有加新表操作或是Join操作就要清空ReaderFields，以免后续的解析字段时找不到字段
        if (this.ReaderFields != null && this.ReaderFields.Count > 0)
        {
            this.TableAliases.Add(parameterNames[0], tableSegment = new TableSegment
            {
                TableType = TableType.TempReaderFields,
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
    public virtual void AddSelectFieldsSql(StringBuilder builder, List<SqlFieldSegment> readerFields)
    {
        int index = 0;
        string body = null;
        bool isOnlyField = readerFields.Count == 1 && readerFields[0].FieldType == SqlFieldType.Field;
        foreach (var readerField in readerFields)
        {
            if (readerField.FieldType == SqlFieldType.IncludeRef)
                continue;
            if (index > 0) builder.Append(',');
            switch (readerField.FieldType)
            {
                case SqlFieldType.Entity:
                    this.AddSelectFieldsSql(builder, readerField.Fields);
                    break;
                case SqlFieldType.DeferredFields:
                    if (readerField.Fields == null)
                        continue;
                    body = this.GetQuotedValue(readerField);
                    builder.Append(body);
                    //延迟方法调用字段，不需要加别名
                    break;
                default:
                    body = this.GetQuotedValue(readerField);
                    //在前面select时，有可能是多分表并且是AVG操作时，没有包裹AVG函数，现在确认不是多分表，需要加上AVG函数包裹
                    if (this.IsNeedFormatShardingTables && this.AggFieldAlias == "AVG_VALUE" && !this.IsManyShardingTables)
                        body = $"AVG({body})";
                    builder.Append(body);
                    //生成SQL的时候，才加上AS别名
                    if (this.IsNeedAlias(readerField, isOnlyField))
                        builder.Append($" AS {this.OrmProvider.GetFieldName(readerField.TargetMember.Name)}");
                    break;
            }
            index++;
        }
    }
    public virtual void AddVisitedFieldsSqlWithoutAlias(StringBuilder builder, SqlFieldSegment readerField, string suffix = null)
    {
        switch (readerField.FieldType)
        {
            case SqlFieldType.Entity:
                var readerFields = readerField.Value as List<SqlFieldSegment>;
                for (int i = 0; i < readerFields.Count; i++)
                {
                    if (i > 0) builder.Append(',');
                    this.AddVisitedFieldsSqlWithoutAlias(builder, readerFields[i], suffix);
                }
                break;
            default:
                var body = this.GetQuotedValue(readerField);
                //CTE表字段是常量/变量/字段名称，都有可能和声明的字段不一致，所以需要获取CTE表的声明字段
                //body里面的值，是原始的值或是字段名
                if (readerField.TableSegment != null && readerField.TableSegment.TableType == TableType.CteSelfRef)
                    body = $"{readerField.TableSegment.AliasName}.{this.OrmProvider.GetFieldName(readerField.TargetMember.Name)}";
                builder.Append(body);
                if (suffix != null) builder.Append(suffix);
                break;
        }
    }
    public virtual bool IsNeedAlias(SqlFieldSegment readerField, bool isOnlyField)
    {
        if (this.IsFromCommand || this.IsSecondUnion || this.IsCteTable) return false;
        if (readerField.IsNeedAlias) return true;
        if (isOnlyField) return false;
        if (readerField.Fields != null && readerField.Fields.Count > 1)
            return false;
        //GroupFields中的ReaderField只设置了必须加as别名的情况，没有设置TargetMember.Name !=FromMember.Name的情况，这里把这种情况补上
        //PostgreSql时，DistinctOnFields中的ReaderField也是这个场景
        if (readerField.IsConstant || readerField.IsVariable || readerField.HasParameter
            || readerField.IsExpression || readerField.IsMethodCall) return true;
        if (readerField.TargetMember != null && readerField.FromMember != null)
            return readerField.TargetMember.Name != readerField.FromMember.Name;
        return false;
    }
    public virtual void Clear(bool isClearReaderFields = false)
    {
        this.Tables.Clear();
        if (isClearReaderFields)
            this.ReaderFields = null;
        this.WhereSql = null;
        this.TableAsStart = 'a';

        this.offset = null;
        this.limit = null;
        this.UnionSql = null;
        this.GroupBySql = null;
        this.HavingSql = null;
        this.OrderBySql = null;
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
        queryVisitor.IsMultiple = this.IsMultiple;
        queryVisitor.CommandIndex = this.CommandIndex;
        queryVisitor.RefTableAliases = this.RefTableAliases;
        queryVisitor.IsNeedTableAlias = this.IsNeedTableAlias;
        queryVisitor.WhereSql = this.WhereSql;
        queryVisitor.LastWhereOperationType = this.LastWhereOperationType;
        queryVisitor.IncludeTables = this.IncludeTables;
        queryVisitor.RefQueries = this.RefQueries;
        queryVisitor.IsNeedUnionShardingTables = this.IsNeedUnionShardingTables;
        queryVisitor.IsNeedFormatShardingTables = this.IsNeedFormatShardingTables;
        queryVisitor.IsManyShardingTables = this.IsManyShardingTables;
        queryVisitor.AggFieldAlias = this.AggFieldAlias;
        queryVisitor.HasAggFields = this.HasAggFields;
        queryVisitor.ShardingTables = this.ShardingTables;
        queryVisitor.GroupByFields = this.GroupByFields;
        queryVisitor.OrderByFields = this.OrderByFields;
        queryVisitor.UnionSql = this.UnionSql;
        queryVisitor.GroupBySql = this.GroupBySql;
        queryVisitor.HavingSql = this.HavingSql;
        queryVisitor.OrderBySql = this.OrderBySql;
        queryVisitor.IsDistinct = this.IsDistinct;
        queryVisitor.IsNeedCommandTableAlias = this.IsNeedCommandTableAlias;
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
        this.OrderBySql = null;

        this.LastIncludeSegment = null;
        this.GroupByFields = null;
        this.OrderByFields = null;
        this.CteQueryObj = null;

        base.Dispose();
    }
    /// <summary>
    /// Join/Union方法，使用子查询对象后，必须先调用UseQuery方法才能继续其他查询操作
    /// </summary>
    /// <exception cref="NotSupportedException"></exception>
    public Stack<MemberExpression> GetMemberExprs(MemberExpression memberExpr, out ParameterExpression parameterExpr)
    {
        Expression currentExpr = memberExpr;
        parameterExpr = null;
        var memberExprs = new Stack<MemberExpression>();
        while (currentExpr != null)
        {
            switch (currentExpr.NodeType)
            {
                case ExpressionType.Parameter:
                    parameterExpr = currentExpr as ParameterExpression;
                    currentExpr = null;
                    break;
                case ExpressionType.Convert:
                    var unaryExpr = currentExpr as UnaryExpression;
                    currentExpr = unaryExpr.Operand;
                    break;
                //case ExpressionType.Call:
                //    if(this.IsSelect)
                //    {
                //        //Select场景，方法调用表达式，可能是访问了Include导航属性
                //        this.Visit(new SqlFieldSegment { Expression = currentExpr });
                //        var methodCallExpr = currentExpr as MethodCallExpression;
                //        if (methodCallExpr.Method.Name == nameof(IncludeSegment.GetIncludeKey))
                //        {
                //            //获取IncludeKey的表达式
                //            var includeSegment = methodCallExpr.Arguments[2] as TableSegment;
                //            if (includeSegment != null)
                //            {
                //                parameterExpr = methodCallExpr.Arguments[0] as ParameterExpression;
                //                return this.GetMemberExprs(includeSegment.Expression as MemberExpression, out parameterExpr);
                //            }
                //        }
                //        else throw new NotSupportedException($"不支持的Select方法调用表达式，访问路径：{currentExpr.ToString()}");
                //    }
                //break;
                case ExpressionType.MemberAccess:
                    var parentExpr = currentExpr as MemberExpression;
                    memberExprs.Push(parentExpr);
                    currentExpr = parentExpr.Expression;
                    break;
                default: throw new NotSupportedException($"不支持的成员访问表达式，访问路径：{currentExpr.ToString()}");
            }
        }
        return memberExprs;
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
#if NETCOREAPP2_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        var hashCode = new HashCode();
        hashCode.Add(targetType);
        hashCode.Add(pathLength);
        hashCode.Add(path);
        return hashCode.ToHashCode();
#else
        int hashCode = 17;
        unchecked
        {
            hashCode = hashCode * 23 + this.OrmProvider.OrmProviderType.GetHashCode();
            hashCode = hashCode * 23 + targetType.GetHashCode();
            hashCode = hashCode * 23 + pathLength.GetHashCode();
            hashCode = hashCode * 23 + path.GetHashCode();
        }
        return hashCode;
#endif
    }
}
public class OrderByField
{
    public SqlFieldSegment Field { get; set; }
    public string OrderSuffix { get; set; }
}
