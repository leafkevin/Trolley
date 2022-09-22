using System;
using System.Collections.Generic;

namespace Trolley;

public class TheaDatabase
{
    public string DbKey { get; set; }
    public bool IsDefault { get; set; }
    public List<TheaConnectionInfo> ConnectionStrings { get; set; }

    public TheaConnectionInfo GetConnectionInfo(int? tenantId = null)
    {
        TheaConnectionInfo result = null;
        if (tenantId.HasValue)
        {
            result = this.ConnectionStrings.Find(f => f.TenantIds != null && f.TenantIds.Contains(tenantId.Value));
            if (result != null) return result;
        }
        result = this.ConnectionStrings.Find(f => f.IsDefault);
        if (result == null)
            throw new Exception($"dbKey:{this.DbKey}数据库未配置默认连接串");

        return result;
    }
}
