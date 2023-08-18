using System;
using System.Linq.Expressions;

namespace Trolley.SqlServer;

public class SqlServerUpdateVisitor : UpdateVisitor, IUpdateVisitor
{
    public SqlServerUpdateVisitor(string dbKey, IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType, bool isParameterized = false, char tableAsStart = 'a', string parameterPrefix = "p")
        : base(dbKey, ormProvider, mapProvider, entityType, isParameterized, tableAsStart, parameterPrefix)
    {
        this.tables[0].AliasName = this.OrmProvider.GetTableName(this.tables[0].Mapper.TableName);
    }
    public override IUpdateVisitor Join(string joinType, Type entityType, Expression joinOn)
        => throw new NotSupportedException("SqlServer不支持Update Join语法，支持Update From语法");
}
