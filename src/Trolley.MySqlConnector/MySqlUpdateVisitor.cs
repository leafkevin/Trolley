using System;
using System.Collections;

namespace Trolley.MySqlConnector;

public class MySqlUpdateVisitor : UpdateVisitor, IUpdateVisitor
{
    public MySqlUpdateVisitor(IOrmProvider ormProvider, IEntityMapProvider mapProvider, bool isParameterized = false, char tableAsStart = 'a', string parameterPrefix = "p")
      : base(ormProvider, mapProvider, isParameterized, tableAsStart, parameterPrefix) { }
    public override IUpdateVisitor From(params Type[] entityTypes)
        => throw new NotSupportedException("MySql不支持Update From语法，支持Update InnerJoin/LeftJoin语法");
    public void WithBulkCopy(IEnumerable insertObjs, int? timeoutSeconds)
    {
        this.ActionMode = ActionMode.BulkCopy;
        this.deferredSegments.Add(new CommandSegment
        {
            Type = "WithBulkCopy",
            Value = (insertObjs, timeoutSeconds)
        });
    }
    public (IEnumerable, int?) BuildWithBulkCopy() => ((IEnumerable, int?))this.deferredSegments[0].Value;
}
