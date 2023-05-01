using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Trolley.MySqlConnector;

public class MySqlQueryVisitor : QueryVisitor, IQueryVisitor
{
    public MySqlQueryVisitor(string dbKey, IOrmProvider ormProvider, IEntityMapProvider mapProvider, bool isParameterized = false, char tableAsStart = 'a', string parameterPrefix = "p")
      : base(dbKey, ormProvider, mapProvider, isParameterized, tableAsStart, parameterPrefix)
    {
    }
    public override QueryVisitor WithCteTable(Type entityType, string cteTableName, bool isRecursive, string rawSql, List<IDbDataParameter> dbParameters = null, List<ReaderField> readerFields = null)
    {
        string withTable = cteTableName;
        if (isRecursive)
            withTable = "RECURSIVE " + cteTableName;

        var builder = new StringBuilder();
        if (string.IsNullOrEmpty(this.cteTableSql))
            builder.Append($"WITH {withTable}(");
        else
        {
            builder.Append(this.cteTableSql);
            builder.AppendLine(",");
            builder.Append($"{withTable}(");
        }

        int index = 0;
        foreach (var readerField in readerFields)
        {
            var memberInfo = readerField.FromMember;
            if (index > 0) builder.Append(',');
            builder.Append(memberInfo.Name);
            index++;
        }
        builder.AppendLine(") AS ");
        builder.AppendLine("(");
        builder.AppendLine(rawSql);
        builder.Append(')');
        this.cteTableSql = builder.ToString();

        var tableSegment = this.AddTable(entityType, string.Empty, TableType.FromQuery, cteTableName, readerFields);
        this.InitFromQueryReaderFields(tableSegment, readerFields);
        if (dbParameters != null)
        {
            if (this.dbParameters == null)
                this.dbParameters = dbParameters;
            else this.dbParameters.AddRange(dbParameters);
        }
        //清掉构建CTE表时Union产生的sql
        this.sql = null;
        return this;
    }
}
