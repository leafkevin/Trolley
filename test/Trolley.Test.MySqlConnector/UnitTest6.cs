using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using Trolley.MySqlConnector;
using Xunit;

namespace Trolley.Test.MySqlConnector
{
    public class UnitTest6 : UnitTestBase
    {
        public UnitTest6()
        {
            var services = new ServiceCollection();
            services.AddSingleton(f =>
            {
                var builder = new OrmDbFactoryBuilder()
                .Register<MySqlProvider>("fengling", "Server=localhost;Database=fengling;Uid=root;password=123456;charset=utf8mb4;", true)
                .Register<MySqlProvider>("fengling_tenant1", "Server=localhost;Database=fengling_tenant1;Uid=root;password=123456;charset=utf8mb4;", false)
                .Register<MySqlProvider>("fengling_tenant2", "Server=localhost;Database=fengling_tenant2;Uid=root;password=123456;charset=utf8mb4;", false)
                .UseSharding(s =>
                {
                    s.UseDatabase(() =>
                    {
                        //可以硬编码分库，也可以使用redis，映射表 ...，其他方式等
                        var passport = f.GetService<IPassport>();
                        return passport.TenantId switch
                        {
                            "200" => "fengling_tenant1",
                            "300" => "fengling_tenant2",
                            _ => "fengling"
                        };
                    })
                    //按照租户+时间分表
                    .UseTable<Order>(t =>
                    {
                        t.DependOn(d => d.TenantId).DependOn(d => d.CreatedAt)
                        .UseRule((dbKey, origName, tenantId, createdAt) => $"{origName}_{tenantId}_{createdAt:yyyMM}", "^sys_order_\\d{1,4}_[1,2]\\d{3}[0,1][0-9]$")
                        //时间分表，通常都是支持范围查询
                        .UseRangeRule((dbKey, origName, tenantId, beginTime, endTime) =>
                        {
                            var tableNames = new List<string>();
                            var current = beginTime;
                            while (current <= endTime)
                            {
                                var tableName = $"{origName}_{tenantId}_{current:yyyyMM}";
                                if (tableNames.Contains(tableName))
                                    continue;
                                tableNames.Add(tableName);
                            }
                            return tableNames;
                        });
                    })
                    //按照租户+时间分表
                    .UseTable<OrderDetail>(t =>
                    {
                        t.DependOn(d => d.TenantId).DependOn(d => d.CreatedAt)
                        .UseRule((dbKey, origName, tenantId, createdAt) => $"{origName}_{tenantId}_{createdAt:yyyMM}", "^sys_order_detail_\\d{1,4}_[1,2]\\d{3}[0,1][0-9]$")
                        //时间分表，通常都是支持范围查询
                        .UseRangeRule((dbKey, origName, tenantId, beginTime, endTime) =>
                        {
                            var tableNames = new List<string>();
                            var current = beginTime;
                            while (current <= endTime)
                            {
                                var tableName = $"{origName}_{tenantId}_{current:yyyyMM}";
                                if (tableNames.Contains(tableName))
                                    continue;
                                tableNames.Add(tableName);
                            }
                            return tableNames;
                        });
                    })
                    //按租户分表
                    //.UseTable<Order>(t => t.DependOn(d => d.TenantId).UseRule((dbKey, origName, tenantId) => $"{origName}_{tenantId}", "^sys_order_\\d{1,4}$"))
                    ////按照Id字段分表，Id字段是带有时间属性的ObjectId
                    //.UseTable<Order>(t => t.DependOn(d => d.Id).UseRule((dbKey, origName, id) => $"{origName}_{new DateTime(ObjectId.Parse(id).Timestamp):yyyyMM}", "^sys_order_\\S{24}$"))
                    ////按照Id字段哈希取模分表
                    //.UseTable<Order>(t => t.DependOn(d => d.Id).UseRule((dbKey, origName, id) => $"{origName}_{HashCode.Combine(id) % 5}", "^sys_order_\\S{24}$"))
                    //当数据库dbKey是fengling主库时，才取模分表
                    .UseTable<User>(t => t.DependOn(d => d.Id).UseRule((dbKey, origName, id) => $"{origName}_{HashCode.Combine(id) % 5}", "^sys_user_\\d{1,4}$"));
                })
                .Configure<MySqlProvider, ModelConfiguration>();
                return builder.Build();
            });
            services.AddTransient<IPassport>(f => new Passport { TenantId = "104", UserId = "1" });
            var serviceProvider = services.BuildServiceProvider();
            this.dbFactory = serviceProvider.GetService<IOrmDbFactory>();
        }
        public interface IPassport
        {
            //只用于演示，实际使用中要与ASP.NET CORE中间件或是IOC组件相结合，赋值此对象
            string TenantId { get; set; }
            string UserId { get; set; }
        }
        class Passport : IPassport
        {
            public string TenantId { get; set; }
            public string UserId { get; set; }
        }
        [Fact]
        public void Query_ManySharding_SingleTable()
        {
            //Initialize();
            var productCount = 1;
            using var repository = dbFactory.Create();
            var sql = repository.From<Order>()
                .UseTable("sys_order_104_202405", "sys_order_105_202405")
                .Where(f => f.ProductCount > productCount)
                .ToSql(out _);
            Assert.True(sql == "SELECT a.`Id`,a.`TenantId`,a.`OrderNo`,a.`ProductCount`,a.`TotalAmount`,a.`BuyerId`,a.`BuyerSource`,a.`SellerId`,a.`Products`,a.`Disputes`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy` FROM `sys_order_104_202405` a WHERE a.`ProductCount`>@p0\r\nUNION ALL\r\nSELECT a.`Id`,a.`TenantId`,a.`OrderNo`,a.`ProductCount`,a.`TotalAmount`,a.`BuyerId`,a.`BuyerSource`,a.`SellerId`,a.`Products`,a.`Disputes`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy` FROM `sys_order_105_202405` a WHERE a.`ProductCount`>@p0");

            var result = repository.From<Order>()
                .UseTable("sys_order_104_202405", "sys_order_105_202405")
                .Where(f => f.ProductCount > productCount)
                .ToList();
            if (result.Count > 0)
            {
                var tenantIds = result.Select(f => f.TenantId).ToList();
                Assert.True(tenantIds.Exists(f => f == "104" || f == "105"));
                Assert.True(!tenantIds.Exists(f => f != "104" && f != "105"));
            }
        }
        [Fact]
        public void Query_ManySharding_MultiTable1()
        {
            //Initialize();
            var productCount = 1;
            using var repository = dbFactory.Create();
            var sql = repository.From<Order>()
                .UseTable(f => (f.Contains("_104_") || f.Contains("_105_")) && int.Parse(f.Substring(f.Length - 6)) > 202001)
                .InnerJoin<User>((x, y) => x.BuyerId == y.Id)
                .UseTable<Order>((dbKey, orderOrigName, userOrigName, orderTableName) =>
                {
                    var tableName = orderTableName.Replace(orderOrigName, userOrigName);
                    return tableName.Substring(0, tableName.Length - 7);
                })
                .Where((a, b) => a.ProductCount > productCount)
                .Select((x, y) => new
                {
                    Order = x,
                    Buyer = y
                })
                .ToSql(out _);
            Assert.True(sql == "SELECT a.`Id`,a.`TenantId`,a.`OrderNo`,a.`ProductCount`,a.`TotalAmount`,a.`BuyerId`,a.`BuyerSource`,a.`SellerId`,a.`Products`,a.`Disputes`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy`,b.`Id`,b.`Name`,b.`Gender`,b.`Age`,b.`CompanyId`,b.`GuidField`,b.`SomeTimes`,b.`SourceType`,b.`IsEnabled`,b.`CreatedAt`,b.`CreatedBy`,b.`UpdatedAt`,b.`UpdatedBy` FROM `sys_order_104_202405` a INNER JOIN `sys_user_104` b ON a.`BuyerId`=b.`Id` WHERE a.`ProductCount`>@p0\r\nUNION ALL\r\nSELECT a.`Id`,a.`TenantId`,a.`OrderNo`,a.`ProductCount`,a.`TotalAmount`,a.`BuyerId`,a.`BuyerSource`,a.`SellerId`,a.`Products`,a.`Disputes`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy`,b.`Id`,b.`Name`,b.`Gender`,b.`Age`,b.`CompanyId`,b.`GuidField`,b.`SomeTimes`,b.`SourceType`,b.`IsEnabled`,b.`CreatedAt`,b.`CreatedBy`,b.`UpdatedAt`,b.`UpdatedBy` FROM `sys_order_105_202405` a INNER JOIN `sys_user_105` b ON a.`BuyerId`=b.`Id` WHERE a.`ProductCount`>@p0");

            var result = repository.From<Order>()
                .UseTable(f => (f.Contains("_104_") || f.Contains("_105_")) && int.Parse(f.Substring(f.Length - 6)) > 202001)
                .InnerJoin<User>((x, y) => x.BuyerId == y.Id)
                .UseTable<Order>((dbKey, orderOrigName, userOrigName, orderTableName) =>
                {
                    var tableName = orderTableName.Replace(orderOrigName, userOrigName);
                    return tableName.Substring(0, tableName.Length - 7);
                })
                .Where((a, b) => a.ProductCount > productCount)
                .Select((x, y) => new
                {
                    Order = x,
                    Buyer = y
                })
                .ToList();
            if (result.Count > 0)
            {
                var tenantIds = result.Select(f => f.Order.TenantId).ToList();
                Assert.True(tenantIds.Exists(f => f == "104" || f == "105"));
                Assert.True(!tenantIds.Exists(f => f != "104" && f != "105"));
            }
        }
        [Fact]
        public void Query_ManySharding_MultiTable2()
        {
            //Initialize();
            var productCount = 1;
            using var repository = dbFactory.Create();
            var sql = repository.From<Order>()
                .UseTable(f => (f.Contains("_104_") || f.Contains("_105_")) && int.Parse(f.Substring(f.Length - 6)) > 202001)
                .InnerJoin<OrderDetail>((x, y) => x.Id == y.OrderId)
                .UseTable<Order>((dbKey, orderOrigName, orderDetailOrigName, orderTableName) => orderTableName.Replace(orderOrigName, orderDetailOrigName))
                .Where((a, b) => a.ProductCount > productCount)
                .Select((x, y) => new
                {
                    Order = x,
                    Detail = y
                })
                .ToSql(out _);
            Assert.True(sql == "SELECT a.`Id`,a.`TenantId`,a.`OrderNo`,a.`ProductCount`,a.`TotalAmount`,a.`BuyerId`,a.`BuyerSource`,a.`SellerId`,a.`Products`,a.`Disputes`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy`,b.`Id`,b.`TenantId`,b.`OrderId`,b.`ProductId`,b.`Price`,b.`Quantity`,b.`Amount`,b.`IsEnabled`,b.`CreatedAt`,b.`CreatedBy`,b.`UpdatedAt`,b.`UpdatedBy` FROM `sys_order_104_202405` a INNER JOIN `sys_order_detail_104_202405` b ON a.`Id`=b.`OrderId` WHERE a.`ProductCount`>@p0\r\nUNION ALL\r\nSELECT a.`Id`,a.`TenantId`,a.`OrderNo`,a.`ProductCount`,a.`TotalAmount`,a.`BuyerId`,a.`BuyerSource`,a.`SellerId`,a.`Products`,a.`Disputes`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy`,b.`Id`,b.`TenantId`,b.`OrderId`,b.`ProductId`,b.`Price`,b.`Quantity`,b.`Amount`,b.`IsEnabled`,b.`CreatedAt`,b.`CreatedBy`,b.`UpdatedAt`,b.`UpdatedBy` FROM `sys_order_105_202405` a INNER JOIN `sys_order_detail_105_202405` b ON a.`Id`=b.`OrderId` WHERE a.`ProductCount`>@p0");

            var result = repository.From<Order>()
                .UseTable(f => (f.Contains("_104_") || f.Contains("_105_")) && int.Parse(f.Substring(f.Length - 6)) > 202001)
                .InnerJoin<OrderDetail>((x, y) => x.Id == y.OrderId)
                .UseTable<Order>((dbKey, orderOrigName, orderDetailOrigName, orderTableName) => orderTableName.Replace(orderOrigName, orderDetailOrigName))
                .Where((a, b) => a.ProductCount > productCount)
                .Select((x, y) => new
                {
                    Order = x,
                    Detail = y
                })
                .ToList();
            if (result.Count > 0)
            {
                var tenantIds = result.Select(f => f.Order.TenantId).ToList();
                Assert.True(tenantIds.Exists(f => f == "104" || f == "105"));
                Assert.True(!tenantIds.Exists(f => f != "104" && f != "105"));
            }
        }
    }
}
