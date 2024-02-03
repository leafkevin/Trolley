using System;

namespace Trolley.MySqlConnector;

class MySqlUpdateVisitor : UpdateVisitor, IUpdateVisitor
{
    public MySqlUpdateVisitor(IOrmProvider ormProvider, IEntityMapProvider mapProvider, bool isParameterized = false, char tableAsStart = 'a', string parameterPrefix = "p")
      : base(ormProvider, mapProvider, isParameterized, tableAsStart, parameterPrefix) { }
    public override IUpdateVisitor From(params Type[] entityTypes)
        => throw new NotSupportedException("MySql不支持Update From语法，支持Update InnerJoin/LeftJoin语法");
}
