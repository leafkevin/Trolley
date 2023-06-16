using System;

namespace Trolley.MySqlConnector;

public class MySqlUpdateVisitor : UpdateVisitor, IUpdateVisitor
{
    public MySqlUpdateVisitor(string dbKey, IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType, bool isParameterized = false, char tableAsStart = 'a', string parameterPrefix = "p")
      : base(dbKey, ormProvider, mapProvider, entityType, isParameterized, tableAsStart, parameterPrefix)
    {
    }
    public override IUpdateVisitor From(params Type[] entityTypes)
        => throw new NotSupportedException("MySql不支持Update From语法，支持Update InnerJoin/LeftJoin语法");
}
