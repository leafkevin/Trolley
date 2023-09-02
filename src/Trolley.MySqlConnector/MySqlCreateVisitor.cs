using System;

namespace Trolley.MySqlConnector;

class MySqlCreateVisitor : CreateVisitor, ICreateVisitor
{
    public MySqlCreateVisitor(string dbKey, IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType, bool isParameterized = false, char tableAsStart = 'a', string parameterPrefix = "p")
     : base(dbKey, ormProvider, mapProvider, entityType, isParameterized, tableAsStart, parameterPrefix)
    {
    }
    public override bool IsSupportIgnore => true;
    public override string BuildHeadSql()
    {
        if (this.isUseIgnore) return "INSERT IGNORE INTO";
        return "INSERT INTO";
    }
}