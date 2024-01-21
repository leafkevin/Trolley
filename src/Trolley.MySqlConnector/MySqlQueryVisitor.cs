using System;
using System.Data;
using System.Text;

namespace Trolley.MySqlConnector;

public class MySqlQueryVisitor : QueryVisitor
{
    public bool IsUseIgnoreInto { get; set; }
    public MySqlQueryVisitor(string dbKey, IOrmProvider ormProvider, IEntityMapProvider mapProvider, bool isParameterized = false, char tableAsStart = 'a', string parameterPrefix = "p", IDataParameterCollection dbParameters = null)
        : base(dbKey, ormProvider, mapProvider, isParameterized, tableAsStart, parameterPrefix, dbParameters) { }

    public override string BuildCommandSql(Type targetType, out IDataParameterCollection dbParameters)
    {
        var builder = new StringBuilder();
        var entityMapper = this.MapProvider.GetEntityMap(targetType);
        if (this.IsUseIgnoreInto)
            builder.Append("INSERT IGNORE INTO");
        else builder.Append("INSERT INTO");
        builder.Append($" {this.OrmProvider.GetTableName(entityMapper.TableName)} (");
        int index = 0;
        foreach (var readerField in this.ReaderFields)
        {
            //Union后，如果没有select语句时，通常实体类型或是select分组对象
            if (!entityMapper.TryGetMemberMap(readerField.TargetMember.Name, out var propMapper)
                || propMapper.IsIgnore || propMapper.IsNavigation || propMapper.IsAutoIncrement
                || (propMapper.MemberType.IsEntityType(out _) && propMapper.TypeHandler == null))
                continue;
            if (index > 0) builder.Append(',');
            builder.Append($"{this.OrmProvider.GetFieldName(propMapper.FieldName)}");
            index++;
        }
        builder.Append(") ");
        //有CTE表
        if (this.IsUseCteTable && this.CteQueries != null && this.CteQueries.Count > 0)
        {
            bool isRecursive = false;
            var cteQueries = this.FlattenRefCteTables(this.CteQueries);
            for (int i = 0; i < cteQueries.Count; i++)
            {
                if (i > 0) builder.AppendLine(",");
                builder.Append(cteQueries[i].Body);
                builder.AppendLine();
                if (cteQueries[i].IsRecursive)
                    isRecursive = true;
            }
            if (isRecursive)
                builder.Insert(0, "WITH RECURSIVE ");
            else builder.Insert(0, "WITH ");
        }
        dbParameters = this.DbParameters;
        if (!string.IsNullOrEmpty(this.UnionSql))
        {
            builder.Append($"SELECT * FROM ({this.UnionSql}) t");
            var sql = builder.ToString();
            builder.Clear();
            return sql;
        }
        var headSql = builder.ToString();
        builder.Clear();
        //生成sql时，include表的字段，一定要紧跟着主表字段后面，方便赋值主表实体的属性中，所以，要排序或是插入时候就排好序
        //方案：在buildSql时确定，ReaderFields要重新排好序，include字段放到对应主表字段后面，表别名顺序不变
        builder.Append(this.BuildSelectSql(this.ReaderFields));

        string selectSql = null;
        if (this.IsDistinct)
            selectSql = "DISTINCT " + builder.ToString();
        else selectSql = builder.ToString();

        builder.Clear();
        string tableSql = null;
        if (this.Tables.Count > 0)
        {
            foreach (var tableSegment in this.Tables)
            {
                string tableName = string.Empty;
                tableName = tableSegment.Body;
                if (string.IsNullOrEmpty(tableName))
                    tableName = this.OrmProvider.GetTableName(tableSegment.Mapper.TableName);

                if (builder.Length > 0)
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
        if (!string.IsNullOrEmpty(this.WhereSql))
        {
            this.WhereSql = $" WHERE {this.WhereSql}";
            builder.Append(this.WhereSql);
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
            builder.Append($"{pageSql}");
        }
        else builder.Append($"SELECT {selectSql} FROM {tableSql}{others}");

        //UNION的子查询中有OrderBy或是Limit，就要包一下SELECT * FROM，否则数据结果不对
        if (this.IsUnion && (!string.IsNullOrEmpty(this.OrderBySql) || this.limit.HasValue))
        {
            builder.Insert(0, "SELECT * FROM (");
            builder.Append($") a");
        }
        return builder.ToString();
    }
}
