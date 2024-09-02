using System.Collections.Generic;

namespace Trolley.Test;

public class TableShardingConfiguration : ITableShardingConfiguration
{
    public void OnModelCreating(TableShardingBuilder builder)
    {
        //按照租户+时间分表
        builder
            .Table<Order>(t => t
                .DependOn(d => d.TenantId).DependOn(d => d.CreatedAt)
                .UseRule((origName, tenantId, createdAt) => tenantId.Length >= 3 ? $"{origName}_{tenantId}_{createdAt:yyyyMM}" : origName, "^sys_order_\\d{1,4}_[1,2]\\d{3}[0,1][0-9]$")
                //时间分表，通常都是支持范围查询
                .UseRangeRule((origName, tenantId, beginTime, endTime) =>
                {
                    if (tenantId.Length < 3)
                        return new List<string> { origName };
                    var tableNames = new List<string>();
                    var current = beginTime.AddDays(1 - beginTime.Day);
                    while (current <= endTime)
                    {
                        var tableName = $"{origName}_{tenantId}_{current:yyyyMM}";
                        if (tableNames.Contains(tableName))
                        {
                            current = current.AddMonths(1);
                            continue;
                        }
                        tableNames.Add(tableName);
                        current = current.AddMonths(1);
                    }
                    return tableNames;
                }))
            //按照租户+时间分表
            .Table<OrderDetail>(t => t
                .DependOn(d => d.TenantId).DependOn(d => d.CreatedAt)
                .UseRule((origName, tenantId, createdAt) => tenantId.Length >= 3 ? $"{origName}_{tenantId}_{createdAt:yyyyMM}" : origName, "^sys_order_detail_\\d{1,4}_[1,2]\\d{3}[0,1][0-9]$")
                //时间分表，通常都是支持范围查询
                .UseRangeRule((origName, tenantId, beginTime, endTime) =>
                {
                    if (tenantId.Length < 3)
                        return new List<string> { origName };
                    var tableNames = new List<string>();
                    var current = beginTime.AddDays(1 - beginTime.Day);
                    while (current <= endTime)
                    {
                        var tableName = $"{origName}_{tenantId}_{current:yyyyMM}";
                        if (tableNames.Contains(tableName))
                        {
                            current = current.AddMonths(1);
                            continue;
                        }
                        tableNames.Add(tableName);
                        current = current.AddMonths(1);
                    }
                    return tableNames;
                }))
            //按租户分表
            //.UseTable<Order>(t => t.DependOn(d => d.TenantId).UseRule((origName, tenantId) => $"{origName}_{tenantId}", "^sys_order_\\d{1,4}$"))
            ////按照Id字段分表，Id字段是带有时间属性的ObjectId
            //.UseTable<Order>(t => t.DependOn(d => d.Id).UseRule((origName, id) => $"{origName}_{new DateTime(ObjectId.Parse(id).Timestamp):yyyyMM}", "^sys_order_\\S{24}$"))
            ////按照Id字段哈希取模分表
            //.UseTable<Order>(t => t.DependOn(d => d.Id).UseRule((origName, id) => $"{origName}_{HashCode.Combine(id) % 5}", "^sys_order_\\S{24}$"))
            //按照租户ID分表
            .Table<User>(t => t.DependOn(d => d.TenantId).UseRule((origName, tenantId) => tenantId.Length >= 3 ? $"{origName}_{tenantId}" : origName, "^sys_user_\\d{1,4}$"));
    }
}
