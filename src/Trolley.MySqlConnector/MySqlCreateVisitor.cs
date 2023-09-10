using System;
using System.Collections.Generic;
using System.Data;

namespace Trolley.MySqlConnector;

class MySqlCreateVisitor : CreateVisitor, ICreateVisitor
{
    public MySqlCreateVisitor(string dbKey, IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType, bool isParameterized = false, char tableAsStart = 'a', string parameterPrefix = "p", List<IDbDataParameter> dbParameters = null)
        : base(dbKey, ormProvider, mapProvider, entityType, isParameterized, tableAsStart, parameterPrefix, dbParameters) { }

    public override string BuildHeadSql()
    {
        if (this.IsUseIgnore) return "INSERT IGNORE INTO";
        return "INSERT INTO";
    }
    public override string BuildTailSql()
    {
        if (!this.IsBulk && this.Tables[0].Mapper.IsAutoIncrement)
            return ";SELECT LAST_INSERT_ID()";
        return string.Empty;
    }
}