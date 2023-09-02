using System;

namespace Trolley.SqlServer;

class SqlServerCreateVisitor : CreateVisitor, ICreateVisitor
{
    public SqlServerCreateVisitor(string dbKey, IOrmProvider ormProvider, IEntityMapProvider mapProvider, Type entityType, bool isParameterized = false, char tableAsStart = 'a', string parameterPrefix = "p")
     : base(dbKey, ormProvider, mapProvider, entityType, isParameterized, tableAsStart, parameterPrefix)
    {
    }
    public override bool IsSupportIgnore => true;
    public override string BuildTailSql()
    {
        if (this.isUseIgnore)
        {
            //if (string.IsNullOrEmpty(this.whereSql))
            //    valuesBuilder.Append(" WHERE NOT EXISTS(" + this.whereSql);
        }
        return base.BuildTailSql();
    }
}
