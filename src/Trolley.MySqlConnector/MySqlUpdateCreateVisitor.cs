using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Trolley.MySqlConnector;

class MySqlUpdateCreateVisitor : CreateVisitor, ICreateVisitor
{
    public MySqlUpdateCreateVisitor(string dbKey, IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType, bool isParameterized = false, char tableAsStart = 'a', string parameterPrefix = "p")
     : base(dbKey, ormProvider, mapProvider, entityType, isParameterized, tableAsStart, parameterPrefix)
    {
    }
    //public override string BuildSql(out List<IDbDataParameter> dbParameters)
    //{
    //    var entityTableName = this.OrmProvider.GetTableName(this.tables[0].Mapper.TableName);
    //    var builder = new StringBuilder("INSERT ");
    //    if (this.isUseIgnore) builder.Append(" IGNORE INTO");

    //    if (this.isFrom)
    //    {

    //    }

    //    if (!string.IsNullOrEmpty(this.whereSql))
    //        builder.Append(" WHERE " + this.whereSql);
    //    dbParameters = this.dbParameters;
    //    return builder.ToString();
    //}
    public override string BuildHeadSql()
    {
        if (this.isUseIgnore) return "INSERT IGNORE INTO";
        return "INSERT INTO";
    }
}

