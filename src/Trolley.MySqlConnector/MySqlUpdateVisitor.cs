using System;
using System.Collections.Generic;
using System.Data;

namespace Trolley.MySqlConnector;

class MySqlUpdateVisitor : UpdateVisitor, IUpdateVisitor
{
    public MySqlUpdateVisitor(string dbKey, IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType, bool isParameterized = false, char tableAsStart = 'a', string parameterPrefix = "p", List<IDbDataParameter> dbParameters = null)
      : base(dbKey, ormProvider, mapProvider, entityType, isParameterized, tableAsStart, parameterPrefix, dbParameters)
    {
    }
    public override IUpdateVisitor From(params Type[] entityTypes)
        => throw new NotSupportedException("MySql不支持Update From语法，支持Update InnerJoin/LeftJoin语法");
}
