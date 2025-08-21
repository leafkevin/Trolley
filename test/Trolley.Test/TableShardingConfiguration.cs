using System;
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
                .UseRule((origName, fieldValues) =>
                {
                    var tenantId = fieldValues[0] as string;
                    var createdAt = (DateTime)fieldValues[1];
                    return tenantId.Length >= 3 ? $"{origName}_{tenantId}_{createdAt: yyyyMM}" : origName;
                }, "^sys_order_[1-9]\\d{3}(0[1-9]|1[0-2])$")
                //时间分表，通常都是支持范围查询
                .UseRangeRule((origName, fieldValues) =>
                {
                    var tenantId = fieldValues[0] as string;
                    var beginTime = (DateTime)fieldValues[1];
                    var endTime = (DateTime)fieldValues[2];
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
                .UseRule((origName, fieldValues) =>
                {
                    var tenantId = fieldValues[0] as string;
                    var createdAt = (DateTime)fieldValues[1];
                    return tenantId.Length >= 3 ? $"{origName}_{tenantId}_{createdAt:yyyyMM}" : origName;
                }, "^sys_order_detail_[1-9]\\d{3}(0[1-9]|1[0-2])$")
                //时间分表，通常都是支持范围查询
                .UseRangeRule((origName, fieldValues) =>
                {
                    var tenantId = fieldValues[0] as string;
                    var beginTime = (DateTime)fieldValues[1];
                    var endTime = (DateTime)fieldValues[2];
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
            //.UseTableMap<Order>(t => t.DependOn(d => d.TenantId).UseRule((origName, tenantId) => $"{origName}_{tenantId}", "^sys_order_\\d{1,4}$"))
            ////按照Id字段分表，Id字段是带有时间属性的ObjectId
            //.UseTableMap<Order>(t => t.DependOn(d => d.Id).UseRule((origName, id) => $"{origName}_{new DateTime(ObjectId.Parse(id).Timestamp):yyyyMM}", "^sys_order_[1-9]\\d{3}$"))
            ////按照Id字段哈希取模分表
            //.UseTableMap<Order>(t => t.DependOn(d => d.Id).UseRule((origName, id) => $"{origName}_{RepositoryHelper.GetCacheKey(id) % 5}", "^sys_order_\\S{24}$"))
            //按照租户ID分表
            .Table<User>(t => t.DependOn(d => d.TenantId).UseRule((origName, tenantId) => tenantId.Length >= 3 ? $"{origName}_{tenantId}" : origName, "^sys_user_[1-9]\\d{3}$"));
    }
}