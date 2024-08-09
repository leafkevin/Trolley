using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Trolley.SqlServer;

public class SqlServerQueryVisitor : QueryVisitor, IQueryVisitor
{
    public SqlServerQueryVisitor(string dbKey, IOrmProvider ormProvider, IEntityMapProvider mapProvider, IShardingProvider shardingProvider, bool isParameterized = false, char tableAsStart = 'a', string parameterPrefix = "p", IDataParameterCollection dbParameters = null)
     : base(dbKey, ormProvider, mapProvider, shardingProvider, isParameterized, tableAsStart, parameterPrefix, dbParameters) { }

    public override string BuildSql(out List<SqlFieldSegment> readerFields)
    {
        var builder = new StringBuilder();
        if (this.IsUseCteTable && this.RefQueries != null && this.RefQueries.Count > 0)
        {
            var cteQueries = this.FlattenRefCteTables(this.RefQueries);
            if (cteQueries.Count > 0)
            {
                for (int i = 0; i < cteQueries.Count; i++)
                {
                    if (i > 0) builder.AppendLine(",");
                    builder.Append(cteQueries[i].Body);
                }
                builder.Insert(0, "WITH ");
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
            builder = null;
            return sql;
        }
        var headSql = builder.ToString();
        builder.Clear();

        //先判断表是否有多分表isManySharding
        string tableSql = null;
        bool isManySharding = false;
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
                if (tableSegment.TableNames != null && tableSegment.TableNames.Count > 1)
                    isManySharding = true;
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

        //判断是否需要SELECT * FROM包装，UNION的子查询中有OrderBy或是Limit，就要包一下SELECT * FROM，否则数据结果不对
        //SqlServer数据库，Union子句在SELECT * FROM包装后，每个列都需要有一个明确的列名，没有则需要增加as别名
        bool isNeedWrap = (this.IsUnion || this.IsSecondUnion || isManySharding) && (!string.IsNullOrEmpty(this.OrderBySql) || this.limit.HasValue);
        this.AddSelectFieldsSql(builder, this.ReaderFields, isNeedWrap);

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

        if (!string.IsNullOrEmpty(this.GroupBySql))
            builder.Append($" GROUP BY {this.GroupBySql}");
        if (!string.IsNullOrEmpty(this.HavingSql))
            builder.Append($" HAVING {this.HavingSql}");

        string orderBy = null;
        if (!string.IsNullOrEmpty(this.OrderBySql))
        {
            orderBy = $"ORDER BY {this.OrderBySql}";
            if (!this.skip.HasValue && !this.limit.HasValue)
                builder.Append(" " + orderBy);
        }
        string others = builder.ToString();

        builder.Clear();
        if (!string.IsNullOrEmpty(headSql))
            builder.Append(headSql);

        if (this.skip.HasValue || this.limit.HasValue)
        {
            //SQL TEMPLATE:SELECT /**fields**/ FROM /**tables**/ /**others**/
            var pageSql = this.OrmProvider.GetPagingTemplate(this.skip, this.limit, orderBy);
            pageSql = pageSql.Replace("/**fields**/", selectSql);
            pageSql = pageSql.Replace("/**tables**/", tableSql);
            pageSql = pageSql.Replace(" /**others**/", others);

            if (this.skip.HasValue && this.limit.HasValue)
                builder.Append($"SELECT COUNT(*) FROM {tableSql}{whereSql};");
            builder.Append($"{pageSql}");
        }
        else builder.Append($"SELECT {selectSql} FROM {tableSql}{others}");
        if (isNeedWrap)
        {
            builder.Insert(0, "SELECT * FROM (");
            builder.Append($") a");
        }
        sql = builder.ToString();
        builder.Clear();
        builder = null;
        return sql;
    }
    public override string BuildCommandSql(out IDataParameterCollection dbParameters)
    {
        var builder = new StringBuilder();
        var entityMapper = this.Tables[0].Mapper;
        builder.Append($"INSERT INTO {this.GetTableName(this.Tables[0])} (");
        int index = 0;
        if (this.ReaderFields == null && this.IsFromQuery)
            this.ReaderFields = this.Tables[1].Fields;
        foreach (var readerField in this.ReaderFields)
        {
            //Union后，如果没有select语句时，通常实体类型或是select分组对象
            if (!entityMapper.TryGetMemberMap(readerField.TargetMember.Name, out var memberMapper)
                || memberMapper.IsIgnore || memberMapper.IsIgnoreInsert
                || memberMapper.IsNavigation || memberMapper.IsAutoIncrement || memberMapper.IsRowVersion
                || (memberMapper.MemberType.IsEntityType(out _) && memberMapper.TypeHandler == null))
                continue;
            if (index > 0) builder.Append(',');
            builder.Append($"{this.OrmProvider.GetFieldName(memberMapper.FieldName)}");
            index++;
        }
        builder.Append(") ");
        //有CTE表
        if (this.IsUseCteTable && this.RefQueries != null && this.RefQueries.Count > 0)
        {
            var fieldsSql = builder.ToString();
            builder.Clear();
            var cteQueries = this.FlattenRefCteTables(this.RefQueries);
            if (cteQueries.Count > 0)
            {
                for (int i = 0; i < cteQueries.Count; i++)
                {
                    if (i > 0) builder.AppendLine(",");
                    builder.Append(cteQueries[i].Body);
                }
                builder.Insert(0, "WITH ");
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
            builder = null;
            return sql;
        }
        var headSql = builder.ToString();
        builder.Clear();

        //先判断表是否有多分表isManySharding
        string tableSql = null;
        bool isManySharding = false;
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
                if (tableSegment.TableNames != null && tableSegment.TableNames.Count > 1)
                    isManySharding = true;
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
        //SqlServer数据库，Union子句在SELECT * FROM包装后，每个列都需要有一个明确的列名，没有则需要增加as别名
        bool isNeedWrap = (this.IsUnion || this.IsSecondUnion || isManySharding) && (!string.IsNullOrEmpty(this.OrderBySql) || this.limit.HasValue);
        this.AddSelectFieldsSql(builder, this.ReaderFields, isNeedWrap);

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
        if (!string.IsNullOrEmpty(this.OrderBySql))
        {
            orderBy = $"ORDER BY {this.OrderBySql}";
            if (!this.skip.HasValue && !this.limit.HasValue)
                builder.Append(" " + orderBy);
        }
        string others = builder.ToString();

        builder.Clear();
        if (!string.IsNullOrEmpty(headSql))
            builder.Append(headSql);

        if (this.skip.HasValue || this.limit.HasValue)
        {
            //SQL TEMPLATE:SELECT /**fields**/ FROM /**tables**/ /**others**/
            var pageSql = this.OrmProvider.GetPagingTemplate(this.skip, this.limit, orderBy);
            pageSql = pageSql.Replace("/**fields**/", selectSql);
            pageSql = pageSql.Replace("/**tables**/", tableSql);
            pageSql = pageSql.Replace(" /**others**/", others);
            builder.Append($"{pageSql}");
        }
        else builder.Append($"SELECT {selectSql} FROM {tableSql}{others}");

        //判断是否需要SELECT * FROM包装，UNION的子查询中有OrderBy或是Limit，就要包一下SELECT * FROM，否则数据结果不正确
        if (isNeedWrap)
        {
            builder.Insert(0, "SELECT * FROM (");
            builder.Append($") a");
        }
        sql = builder.ToString();
        builder.Clear();
        builder = null;
        return sql;
    }
    public override string BuildShardingTablesSql(string tableSchema)
    {
        var count = this.ShardingTables.FindAll(f => f.ShardingType > ShardingTableType.MultiTable).Count;
        var builder = new StringBuilder($"SELECT name FROM sys.sysobjects WHERE xtype='U' AND ");
        if (count > 1)
        {
            builder.Append('(');
            int index = 0;
            foreach (var tableSegment in this.ShardingTables)
            {
                if (tableSegment.ShardingType > ShardingTableType.MultiTable)
                {
                    if (index > 0) builder.Append(" OR ");
                    builder.Append($"name LIKE '{tableSegment.Mapper.TableName}%'");
                    index++;
                }
            }
            builder.Append(')');
        }
        else
        {
            if (this.ShardingTables.Count > 1)
            {
                var tableSegment = this.ShardingTables.Find(f => f.ShardingType > ShardingTableType.MultiTable);
                builder.Append($"name LIKE '{tableSegment.Mapper.TableName}%'");
            }
            else builder.Append($"name LIKE '{this.ShardingTables[0].Mapper.TableName}%'");
        }
        return builder.ToString();
    }
    public virtual void AddSelectFieldsSql(StringBuilder builder, List<SqlFieldSegment> readerFields, bool isSecondUnionWrap)
    {
        int index = 0;
        string body = null;
        bool isOnlyField = readerFields.Count == 1 && readerFields[0].FieldType == SqlFieldType.Field;
        foreach (var readerField in readerFields)
        {
            if (index > 0) builder.Append(',');
            switch (readerField.FieldType)
            {
                case SqlFieldType.Entity:
                    this.AddSelectFieldsSql(builder, readerField.Fields, isSecondUnionWrap);
                    break;
                case SqlFieldType.DeferredFields:
                    if (readerField.Fields == null)
                        continue;
                    body = this.GetQuotedValue(readerField);
                    builder.Append(body);
                    //生成SQL的时候，才加上AS别名
                    if (this.IsNeedAlias(readerField, isOnlyField, isSecondUnionWrap))
                        builder.Append($" AS {this.OrmProvider.GetFieldName(readerField.TargetMember.Name)}");
                    break;
                default:
                    body = this.GetQuotedValue(readerField);
                    //CTE表字段是常量/变量/字段名称，都有可能和声明的字段不一致，所以需要获取CTE表的声明字段
                    //body里面的值，是原始的值或是字段名
                    if (readerField.TableSegment != null && readerField.TableSegment.TableType == TableType.CteSelfRef)
                        body = $"{readerField.TableSegment.AliasName}.{this.OrmProvider.GetFieldName(readerField.TargetMember.Name)}";
                    builder.Append(body);
                    //生成SQL的时候，才加上AS别名
                    if (this.IsNeedAlias(readerField, isOnlyField, isSecondUnionWrap))
                        builder.Append($" AS {this.OrmProvider.GetFieldName(readerField.TargetMember.Name)}");
                    break;
            }
            index++;
        }
    }
    public virtual bool IsNeedAlias(SqlFieldSegment readerField, bool isOnlyField, bool isSecondUnionWrap)
    {
        if (!isSecondUnionWrap && (this.IsSecondUnion || this.IsFromCommand || this.IsSecondUnion || this.IsCteTable)) return false;
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
}