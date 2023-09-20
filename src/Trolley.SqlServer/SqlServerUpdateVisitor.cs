using System;
using System.Linq.Expressions;
using System.Text;

namespace Trolley.SqlServer;

public class SqlServerUpdateVisitor : UpdateVisitor, IUpdateVisitor
{
    public SqlServerUpdateVisitor(string dbKey, IOrmProvider ormProvider, IEntityMapProvider mapProvider, bool isParameterized = false, char tableAsStart = 'a', string parameterPrefix = "p")
        : base(dbKey, ormProvider, mapProvider, isParameterized, tableAsStart, parameterPrefix)
    {
        this.Tables[0].AliasName = this.OrmProvider.GetTableName(this.Tables[0].Mapper.TableName);
    }
    public override string BuildSql()
    {
        var entityTableName = this.OrmProvider.GetTableName(this.Tables[0].Mapper.TableName);
        var builder = new StringBuilder($"UPDATE {entityTableName} ");
        var aliasName = this.Tables[0].AliasName;
        int index = 0;
        bool hasWhere = false;
        builder.Append("SET ");
        if (this.UpdateFields != null && this.UpdateFields.Count > 0)
        {
            foreach (var setField in this.UpdateFields)
            {
                if (setField.Type == UpdateFieldType.Where)
                {
                    hasWhere = true;
                    continue;
                }
                if (index > 0) builder.Append(',');
                switch (setField.Type)
                {
                    case UpdateFieldType.SetField:
                    case UpdateFieldType.SetValue:
                        builder.Append($"{this.OrmProvider.GetFieldName(setField.MemberMapper.FieldName)}={setField.Value}");
                        break;
                    case UpdateFieldType.RawSql:
                        builder.Append(setField.Value);
                        break;
                }
                index++;
            }
        }

        if (this.IsFrom && this.Tables.Count > 1)
        {
            builder.Append(" FROM ");
            for (var i = 1; i < this.Tables.Count; i++)
            {
                var tableSegment = this.Tables[i];
                var tableName = tableSegment.Body;
                if (string.IsNullOrEmpty(tableName))
                {
                    tableSegment.Mapper ??= this.MapProvider.GetEntityMap(tableSegment.EntityType);
                    tableName = this.OrmProvider.GetTableName(tableSegment.Mapper.TableName);
                }
                builder.Append($"{tableName} {tableSegment.AliasName}");
            }
        }
        if (!string.IsNullOrEmpty(this.WhereSql) || hasWhere)
            builder.Append(" WHERE ");
        if (hasWhere)
        {
            index = 0;
            foreach (var setField in this.UpdateFields)
            {
                if (setField.Type != UpdateFieldType.Where)
                    continue;
                if (index > 0) builder.Append(" AND ");
                if (this.IsNeedAlias) builder.Append($"{aliasName}");
                builder.Append($"{this.OrmProvider.GetFieldName(setField.MemberMapper.FieldName)}={setField.Value}");
                index++;
            }
        }
        if (!string.IsNullOrEmpty(this.WhereSql))
        {
            if (hasWhere)
                builder.Append(" AND ");
            builder.Append(this.WhereSql);
        }

        return builder.ToString();
    }
    public override IUpdateVisitor Join(string joinType, Type entityType, Expression joinOn)
        => throw new NotSupportedException("SqlServer不支持Update Join语法，支持Update From语法");
}
