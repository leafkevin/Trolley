using System;
using System.Collections.Generic;
using System.Linq;

namespace Trolley;

public class TheaDatabase
{
    public string DbKey { get; set; }
    public bool IsDefault { get; set; }
    public List<TheaConnectionInfo> ConnectionInfos { get; set; }

    public TheaConnectionInfo GetConnectionInfo(int? tenantId = null)
    {
        TheaConnectionInfo result = null;
        if (tenantId.HasValue)
        {
            result = this.ConnectionInfos.FirstOrDefault(f => f.TenantIds != null && f.TenantIds.Contains(tenantId.Value));
            if (result != null) return result;
        }
        result = this.ConnectionInfos.FirstOrDefault(f => f.IsDefault);
        if (result == null)
            throw new Exception($"dbKey:{this.DbKey}数据库未配置默认连接串");

        return result;
    }
}
