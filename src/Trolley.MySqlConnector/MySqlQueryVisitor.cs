﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Trolley.MySqlConnector;

public class MySqlQueryVisitor : QueryVisitor
{
    public bool IsUseIgnoreInto { get; set; }
    public MySqlQueryVisitor(DbContext dbContext, char tableAsStart = 'a', IDataParameterCollection dbParameters = null)
        : base(dbContext, tableAsStart, dbParameters) { }

    public override string BuildCommandSql(out IDataParameterCollection dbParameters)
    {
        var builder = new StringBuilder();
        var entityMapper = this.Tables[0].Mapper;
        if (this.IsUseIgnoreInto)
            builder.Append("INSERT IGNORE INTO");
        else builder.Append("INSERT INTO");
        builder.Append($" {this.GetTableName(this.Tables[0])} (");
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
            bool isRecursive = false;
            var cteQueries = this.FlattenRefCteTables(this.RefQueries);
            if (cteQueries.Count > 0)
            {
                builder.AppendLine();
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
            builder.Insert(0, fieldsSql);
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
        this.AddSelectFieldsSql(builder, this.ReaderFields);

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
        if (isManySharding)
            isManySharding = this.ShardingTables != null && this.ShardingTables[0].TableNames != null && this.ShardingTables[0].TableNames.Count > 1;
        bool isNeedWrap = (this.IsUnion || this.IsSecondUnion || isManySharding) && (!string.IsNullOrEmpty(this.OrderBySql) || this.limit.HasValue);
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
    public override string BuildTableShardingsSql()
    {
        var builder = new StringBuilder($"SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE' AND ");
        var schemaBuilders = new Dictionary<string, StringBuilder>();
        foreach (var tableSegment in this.ShardingTables)
        {
            if (tableSegment.ShardingType > ShardingTableType.MultiTable)
            {
                var tableSchema = tableSegment.TableSchema ?? this.DefaultTableSchema;
                if (!schemaBuilders.TryGetValue(tableSchema, out var tableBuilder))
                    schemaBuilders.TryAdd(tableSchema, tableBuilder = new StringBuilder());

                if (tableBuilder.Length > 0) tableBuilder.Append(" OR ");
                tableBuilder.Append($"TABLE_NAME LIKE '{tableSegment.Mapper.TableName}%'");
            }
        }
        if (schemaBuilders.Count > 1)
            builder.Append('(');
        int index = 0;
        foreach (var schemaBuilder in schemaBuilders)
        {
            if (index > 0) builder.Append(" OR ");
            builder.Append($"TABLE_SCHEMA='{schemaBuilder.Key}' AND ({schemaBuilder.Value.ToString()})");
            index++;
        }
        if (schemaBuilders.Count > 1)
            builder.Append(')');
        return builder.ToString();
    }
}
