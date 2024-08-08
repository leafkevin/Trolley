using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Trolley.SqlServer;
using Xunit;

namespace Trolley.Test.SqlServer;

public class UnitTest6 : UnitTestBase
{
    public UnitTest6()
    {
        var services = new ServiceCollection();
        services.AddSingleton(f =>
        {
            var builder = new OrmDbFactoryBuilder()
            .Register<SqlServerProvider>("fengling", "Server=172.16.30.190;Database=fengling;Uid=sa;password=SQLserverSA123456;TrustServerCertificate=true", true)
            .Register<SqlServerProvider>("fengling_tenant1", "Server=127.0.0.1;Database=fengling_tenant1;Uid=sa;password=SQLserverSA123456;TrustServerCertificate=true", false)
            .Register<SqlServerProvider>("fengling_tenant2", "Server=127.0.0.1;Database=fengling_tenant2;Uid=sa;password=SQLserverSA123456;TrustServerCertificate=true", false)
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
                    .UseRule((dbKey, origName, tenantId, createdAt) => $"{origName}_{tenantId}_{createdAt:yyyyMM}", "^sys_order_\\d{1,4}_[1,2]\\d{3}[0,1][0-9]$")
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
                    .UseRule((dbKey, origName, tenantId, createdAt) => $"{origName}_{tenantId}_{createdAt:yyyyMM}", "^sys_order_detail_\\d{1,4}_[1,2]\\d{3}[0,1][0-9]$")
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
                .UseTable<User>(t => t.DependOn(d => d.TenantId).UseRule((dbKey, origName, tenantId) => $"{origName}_{tenantId}", "^sys_user_\\d{1,4}$"));
            })
            .Configure<SqlServerProvider, ModelConfiguration>();
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
    private async Task InitSharding()
    {
        using var repository = this.dbFactory.Create();
        await repository.Delete<User>()
            .UseTableBy("104")
            .UseTableBy("105")
            .Where(new[] { 101, 102, 103 })
            .ExecuteAsync();
        repository.Create<User>(new[]
        {
            new User
            {
                Id = 101,
                TenantId ="104",
                Name = "leafkevin",
                Age = 25,
                CompanyId = 1,
                Gender = Gender.Male,
                GuidField = Guid.NewGuid(),
                SomeTimes = TimeOnly.FromTimeSpan(TimeSpan.FromSeconds(4769)),
                SourceType = UserSourceType.Douyin,
                IsEnabled = true,
                CreatedAt = DateTime.Parse("2023-03-10 06:07:08"),
                CreatedBy = 1,
                UpdatedAt = DateTime.Parse("2023-03-15 16:27:38"),
                UpdatedBy = 1
            },
            new User
            {
                Id = 102,
                TenantId ="105",
                Name = "cindy",
                Age = 21,
                CompanyId = 2,
                Gender = Gender.Female,
                GuidField= Guid.NewGuid(),
                SomeTimes= TimeOnly.FromTimeSpan(TimeSpan.FromSeconds(5730)),
                SourceType = UserSourceType.Taobao,
                IsEnabled = true,
                CreatedAt = DateTime.Parse($"{DateTime.Today.AddDays(-1):yyyy-MM-dd} 06:07:08"),
                CreatedBy = 1,
                UpdatedAt = DateTime.Now,
                UpdatedBy = 1
            },
            new User
            {
                Id = 103,
                TenantId ="105",
                Name = "xiyuan",
                Age = 17,
                CompanyId = 3,
                Gender = Gender.Female,
                GuidField= Guid.NewGuid(),
                SomeTimes= TimeOnly.FromTimeSpan(TimeSpan.FromSeconds(5730)),
                SourceType = UserSourceType.Taobao,
                IsEnabled = true,
                CreatedAt = DateTime.Parse($"{DateTime.Today.AddDays(-1):yyyy-MM-dd} 06:07:08"),
                CreatedBy = 1,
                UpdatedAt = DateTime.Now,
                UpdatedBy = 1
            }
        });

        var createdAt = DateTime.Parse("2024-05-24");
        var orders = new List<Order>();
        var orderDetails = new List<OrderDetail>();
        for (int i = 1000; i < 2000; i++)
        {
            var orderId = $"ON_{i + 1}";
            orders.Add(new Order
            {
                Id = orderId,
                TenantId = "104",
                OrderNo = $"ON-{i + 1}",
                BuyerId = 101,
                SellerId = 2,
                TotalAmount = 420,
                ProductCount = 2,
                Products = new List<int> { 1, 2 },
                Disputes = new Dispute
                {
                    Id = i + 1,
                    Content = "无良商家",
                    Result = "同意退款",
                    Users = "Buyer2,Seller2",
                    CreatedAt = createdAt
                },
                IsEnabled = true,
                CreatedAt = createdAt,
                CreatedBy = 1,
                UpdatedAt = createdAt,
                UpdatedBy = 1
            });
            orderDetails.Add(new OrderDetail
            {
                Id = $"OND_{1000 + (i - 1000) * 2 + 1}",
                TenantId = "104",
                Amount = 240,
                OrderId = orderId,
                Price = 120,
                ProductId = 11,
                Quantity = 2,
                IsEnabled = true,
                CreatedAt = createdAt,
                CreatedBy = 1,
                UpdatedAt = createdAt,
                UpdatedBy = 1
            });
            orderDetails.Add(new OrderDetail
            {
                Id = $"OND_{1000 + (i - 1000) * 2 + 2}",
                TenantId = "104",
                Amount = 180,
                OrderId = orderId,
                Price = 180,
                ProductId = 12,
                Quantity = 1,
                IsEnabled = true,
                CreatedAt = createdAt,
                CreatedBy = 1,
                UpdatedAt = createdAt,
                UpdatedBy = 1
            });
        }
        for (int i = 2000; i < 3000; i++)
        {
            var orderId = $"ON_{i + 1}";
            orders.Add(new Order
            {
                Id = orderId,
                TenantId = "105",
                OrderNo = $"ON-{i + 1}",
                BuyerId = 102,
                SellerId = 2,
                TotalAmount = 630,
                ProductCount = 2,
                Products = new List<int> { 1, 2 },
                Disputes = new Dispute
                {
                    Id = i + 1,
                    Content = "无良商家",
                    Result = "同意退款",
                    Users = "Buyer2,Seller2",
                    CreatedAt = DateTime.Now
                },
                IsEnabled = true,
                CreatedAt = createdAt,
                CreatedBy = 1,
                UpdatedAt = createdAt,
                UpdatedBy = 1
            });
            orderDetails.Add(new OrderDetail
            {
                Id = $"OND_{2000 + (i - 2000) * 2 + 1}",
                TenantId = "105",
                Amount = 230,
                OrderId = orderId,
                Price = 230,
                ProductId = 13,
                Quantity = 1,
                IsEnabled = true,
                CreatedAt = createdAt,
                CreatedBy = 1,
                UpdatedAt = createdAt,
                UpdatedBy = 1
            });
            orderDetails.Add(new OrderDetail
            {
                Id = $"OND_{2000 + (i - 2000) * 2 + 2}",
                TenantId = "105",
                Amount = 400,
                OrderId = orderId,
                Price = 200,
                ProductId = 14,
                Quantity = 2,
                IsEnabled = true,
                CreatedAt = createdAt,
                CreatedBy = 1,
                UpdatedAt = createdAt,
                UpdatedBy = 1
            });
        }
        var removeIds = orders.Select(f => f.Id).ToList();

        await repository.BeginTransactionAsync();
        var count = await repository.Delete<Order>()
            .UseTableBy("104", createdAt)
            .UseTableBy("105", createdAt)
            .Where(f => removeIds.Contains(f.Id))
            .ExecuteAsync();
        count = await repository.Delete<OrderDetail>()
            .UseTableBy("104", createdAt)
            .UseTableBy("105", createdAt)
            .Where(f => removeIds.Contains(f.OrderId))
            .ExecuteAsync();

        var count1 = await repository.Create<Order>()
            .WithBulkCopy(orders)
            .ExecuteAsync();
        var count2 = await repository.Create<OrderDetail>()
            .WithBulkCopy(orderDetails)
            .ExecuteAsync();
        await repository.CommitAsync();
    }
    [Fact]
    public async void Create_WithBy_UseTable()
    {
        using var repository = this.dbFactory.Create();
        await repository.Delete<User>()
            .UseTableBy("104")
            .Where(101)
            .ExecuteAsync();
        var count = repository.From<User>()
            .UseTable("sys_user_104")
            .Where(f => f.Id == 101)
            .Count();
        Assert.True(count == 0);

        repository.Create<User>()
            .UseTable("sys_user_104")
            .WithBy(new
            {
                Id = 101,
                TenantId = "104",
                Name = "leafkevin",
                Age = 25,
                CompanyId = 1,
                Gender = Gender.Male,
                GuidField = Guid.NewGuid(),
                SomeTimes = TimeOnly.FromTimeSpan(TimeSpan.FromSeconds(4769)),
                SourceType = UserSourceType.Douyin,
                IsEnabled = true,
                CreatedAt = DateTime.Parse("2024-05-10 06:07:08"),
                CreatedBy = 1,
                UpdatedAt = DateTime.Parse("2024-05-15 16:27:38"),
                UpdatedBy = 1
            })
            .Execute();
        var result = repository.From<User>()
            .UseTableBy("104")
            .Where(f => f.Id == 101)
            .First();
        Assert.NotNull(result);
        Assert.True(result.TenantId == "104");
    }
    [Fact]
    public async void Create_WithBy_WithoutUseTable()
    {
        using var repository = this.dbFactory.Create();
        await repository.Delete<User>()
            .UseTableBy("104")
            .Where(101)
            .ExecuteAsync();
        var count = repository.From<User>()
            .UseTableBy("104")
            .Where(f => f.Id == 101)
            .Count();
        Assert.True(count == 0);

        await repository.Create<User>()
            .WithBy(new
            {
                Id = 101,
                TenantId = "104",
                Name = "leafkevin",
                Age = 25,
                CompanyId = 1,
                Gender = Gender.Male,
                GuidField = Guid.NewGuid(),
                SomeTimes = TimeOnly.FromTimeSpan(TimeSpan.FromSeconds(4769)),
                SourceType = UserSourceType.Douyin,
                IsEnabled = true,
                CreatedAt = DateTime.Parse("2023-03-10 06:07:08"),
                CreatedBy = 1,
                UpdatedAt = DateTime.Parse("2023-03-15 16:27:38"),
                UpdatedBy = 1
            })
            .ExecuteAsync();
        var result = await repository.From<User>()
            .UseTableBy("104")
            .Where(f => f.Id == 101)
            .FirstAsync();
        Assert.NotNull(result);
        Assert.True(result.TenantId == "104");
    }
    [Fact]
    public async void Create_WithBulk_UseTable()
    {
        using var repository = this.dbFactory.Create();
        await repository.Delete<User>()
            .UseTableBy("104")
            .Where(101)
            .ExecuteAsync();
        var count = repository.From<User>()
            .UseTableBy("104")
            .Where(f => f.Id == 101)
            .Count();
        Assert.True(count == 0);

        repository.Create<User>()
            .UseTableBy("104")
            .WithBulk(new[]{new
            {
                Id = 101,
                TenantId = "104",
                Name = "leafkevin",
                Age = 25,
                CompanyId = 1,
                Gender = Gender.Male,
                GuidField = Guid.NewGuid(),
                SomeTimes = TimeOnly.FromTimeSpan(TimeSpan.FromSeconds(4769)),
                SourceType = UserSourceType.Douyin,
                IsEnabled = true,
                CreatedAt = DateTime.Parse("2023-03-10 06:07:08"),
                CreatedBy = 1,
                UpdatedAt = DateTime.Parse("2023-03-15 16:27:38"),
                UpdatedBy = 1
            }})
            .Execute();
        var result = repository.From<User>()
            .UseTableBy("104")
            .Where(f => f.Id == 101)
            .First();
        Assert.NotNull(result);
        Assert.True(result.TenantId == "104");
    }
    [Fact]
    public async void Create_WithoutUseTable()
    {
        using var repository = this.dbFactory.Create();
        await repository.Delete<User>()
            .UseTableBy("104")
            .UseTableBy("105")
            .Where(new object[] { 101, 102, 103 })
            .ExecuteAsync();
        await repository.CreateAsync<User>(new
        {
            Id = 102,
            TenantId = "105",
            Name = "cindy",
            Age = 21,
            CompanyId = 2,
            Gender = Gender.Female,
            GuidField = Guid.NewGuid(),
            SomeTimes = TimeOnly.FromTimeSpan(TimeSpan.FromSeconds(5730)),
            SourceType = UserSourceType.Taobao,
            IsEnabled = true,
            CreatedAt = DateTime.Parse($"{DateTime.Today.AddDays(-1):yyyy-MM-dd} 06:07:08"),
            CreatedBy = 1,
            UpdatedAt = DateTime.Now,
            UpdatedBy = 1
        });

        await repository.CreateAsync<User>(new[]
        {
            new
            {
                Id = 101,
                TenantId ="104",
                Name = "leafkevin",
                Age = 25,
                CompanyId = 1,
                Gender = Gender.Male,
                GuidField = Guid.NewGuid(),
                SomeTimes = TimeOnly.FromTimeSpan(TimeSpan.FromSeconds(4769)),
                SourceType = UserSourceType.Douyin,
                IsEnabled = true,
                CreatedAt = DateTime.Parse("2023-03-10 06:07:08"),
                CreatedBy = 1,
                UpdatedAt = DateTime.Parse("2023-03-15 16:27:38"),
                UpdatedBy = 1
            },
            new
            {
                Id = 103,
                TenantId ="105",
                Name = "xiyuan",
                Age = 17,
                CompanyId = 3,
                Gender = Gender.Female,
                GuidField= Guid.NewGuid(),
                SomeTimes= TimeOnly.FromTimeSpan(TimeSpan.FromSeconds(5730)),
                SourceType = UserSourceType.Taobao,
                IsEnabled = true,
                CreatedAt = DateTime.Parse($"{DateTime.Today.AddDays(-1):yyyy-MM-dd} 06:07:08"),
                CreatedBy = 1,
                UpdatedAt = DateTime.Now,
                UpdatedBy = 1
            }
        });
        var result = await repository.From<User>()
            .UseTableBy("104")
            .Where(f => f.Id == 101)
            .FirstAsync();
        Assert.NotNull(result);
        Assert.True(result.TenantId == "104");

        result = await repository.From<User>()
           .UseTableBy("105")
           .Where(f => f.Id == 102)
           .FirstAsync();
        Assert.NotNull(result);
        Assert.True(result.TenantId == "105");

        result = await repository.From<User>()
           .UseTableBy("105")
           .Where(f => f.Id == 103)
           .FirstAsync();
        Assert.NotNull(result);
        Assert.True(result.TenantId == "105");
    }
    [Fact]
    public async void Create_WithBulk_WithoutUseTable()
    {
        using var repository = this.dbFactory.Create();
        var userIds = new[] { 101, 102, 103 };
        await repository.Delete<User>()
            .UseTable(f => f.Contains("104") || f.Contains("105"))
            .Where(userIds)
            .ExecuteAsync();
        repository.Create<User>(new[]
        {
            new User
            {
                Id = 101,
                TenantId ="104",
                Name = "leafkevin",
                Age = 25,
                CompanyId = 1,
                Gender = Gender.Male,
                GuidField = Guid.NewGuid(),
                SomeTimes = TimeOnly.FromTimeSpan(TimeSpan.FromSeconds(4769)),
                SourceType = UserSourceType.Douyin,
                IsEnabled = true,
                CreatedAt = DateTime.Parse("2023-03-10 06:07:08"),
                CreatedBy = 1,
                UpdatedAt = DateTime.Parse("2023-03-15 16:27:38"),
                UpdatedBy = 1
            },
            new User
            {
                Id = 102,
                TenantId ="105",
                Name = "cindy",
                Age = 21,
                CompanyId = 2,
                Gender = Gender.Female,
                GuidField= Guid.NewGuid(),
                SomeTimes= TimeOnly.FromTimeSpan(TimeSpan.FromSeconds(5730)),
                SourceType = UserSourceType.Taobao,
                IsEnabled = true,
                CreatedAt = DateTime.Parse($"{DateTime.Today.AddDays(-1):yyyy-MM-dd} 06:07:08"),
                CreatedBy = 1,
                UpdatedAt = DateTime.Now,
                UpdatedBy = 1
            },
            new User
            {
                Id = 103,
                TenantId ="105",
                Name = "xiyuan",
                Age = 17,
                CompanyId = 3,
                Gender = Gender.Female,
                GuidField= Guid.NewGuid(),
                SomeTimes= TimeOnly.FromTimeSpan(TimeSpan.FromSeconds(5730)),
                SourceType = UserSourceType.Taobao,
                IsEnabled = true,
                CreatedAt = DateTime.Parse($"{DateTime.Today.AddDays(-1):yyyy-MM-dd} 06:07:08"),
                CreatedBy = 1,
                UpdatedAt = DateTime.Now,
                UpdatedBy = 1
            }
        });

        var result = await repository.From<User>()
            .UseTableBy("104")
            .Where(f => f.Id == 101)
            .FirstAsync();
        Assert.NotNull(result);
        Assert.True(result.TenantId == "104");

        var result1 = await repository.From<User>()
            .UseTableBy("105")
            .Where(f => userIds.Contains(f.Id))
            .FirstAsync();
        Assert.NotNull(result);
        Assert.True(result.TenantId == "104");
    }
    [Fact]
    public async void Create_BulkCopy_UseTable()
    {
        var createdAt = DateTime.Parse("2024-05-24");
        var orders = new List<Order>();
        var orderDetails = new List<OrderDetail>();
        for (int i = 1000; i < 2000; i++)
        {
            var orderId = $"ON_{i + 1}";
            orders.Add(new Order
            {
                Id = orderId,
                TenantId = "104",
                OrderNo = $"ON-{i + 1}",
                BuyerId = 101,
                SellerId = 2,
                TotalAmount = 420,
                ProductCount = 2,
                Products = new List<int> { 1, 2 },
                Disputes = new Dispute
                {
                    Id = i + 1,
                    Content = "无良商家",
                    Result = "同意退款",
                    Users = "Buyer2,Seller2",
                    CreatedAt = createdAt
                },
                IsEnabled = true,
                CreatedAt = createdAt,
                CreatedBy = 1,
                UpdatedAt = createdAt,
                UpdatedBy = 1
            });
            orderDetails.Add(new OrderDetail
            {
                Id = $"OND_{1000 + (i - 1000) * 2 + 1}",
                TenantId = "104",
                Amount = 240,
                OrderId = orderId,
                Price = 120,
                ProductId = 11,
                Quantity = 2,
                IsEnabled = true,
                CreatedAt = createdAt,
                CreatedBy = 1,
                UpdatedAt = createdAt,
                UpdatedBy = 1
            });
            orderDetails.Add(new OrderDetail
            {
                Id = $"OND_{1000 + (i - 1000) * 2 + 2}",
                TenantId = "104",
                Amount = 180,
                OrderId = orderId,
                Price = 180,
                ProductId = 12,
                Quantity = 1,
                IsEnabled = true,
                CreatedAt = createdAt,
                CreatedBy = 1,
                UpdatedAt = createdAt,
                UpdatedBy = 1
            });
        }
        using var repository = this.dbFactory.Create();
        var removeIds = orders.Select(f => f.Id).ToList();

        await repository.BeginTransactionAsync();
        var deleteOrders = repository.Delete<Order>()
           .UseTableBy("104", createdAt)
           .Where(f => removeIds.Contains(f.Id))
           .ToMultipleCommand();
        var deleteOrderDetails = repository.Delete<OrderDetail>()
           .UseTableBy("104", createdAt)
           .Where(f => removeIds.Contains(f.OrderId))
           .ToMultipleCommand();
        await repository.MultipleExecuteAsync(new List<MultipleCommand>
        {
            deleteOrders, deleteOrderDetails
        });
        var count1 = await repository.Create<Order>()
            .UseTableBy("104", createdAt)
            .WithBulkCopy(orders)
            .ExecuteAsync();
        var count2 = await repository.Create<OrderDetail>()
             .UseTableBy("104", createdAt)
             .WithBulkCopy(orderDetails)
             .ExecuteAsync();
        await repository.CommitAsync();
        Assert.True(count1 == 1000);
        Assert.True(count2 == 2000);

        orders.Clear();
        orderDetails.Clear();
        for (int i = 2000; i < 3000; i++)
        {
            var orderId = $"ON_{i + 1}";
            orders.Add(new Order
            {
                Id = orderId,
                TenantId = "105",
                OrderNo = $"ON-{i + 1}",
                BuyerId = 102,
                SellerId = 2,
                TotalAmount = 630,
                ProductCount = 2,
                Products = new List<int> { 1, 2 },
                Disputes = new Dispute
                {
                    Id = i + 1,
                    Content = "无良商家",
                    Result = "同意退款",
                    Users = "Buyer2,Seller2",
                    CreatedAt = DateTime.Now
                },
                IsEnabled = true,
                CreatedAt = createdAt,
                CreatedBy = 1,
                UpdatedAt = createdAt,
                UpdatedBy = 1
            });
            orderDetails.Add(new OrderDetail
            {
                Id = $"OND_{2000 + (i - 2000) * 2 + 1}",
                TenantId = "105",
                Amount = 230,
                OrderId = orderId,
                Price = 230,
                ProductId = 13,
                Quantity = 1,
                IsEnabled = true,
                CreatedAt = createdAt,
                CreatedBy = 1,
                UpdatedAt = createdAt,
                UpdatedBy = 1
            });
            orderDetails.Add(new OrderDetail
            {
                Id = $"OND_{2000 + (i - 2000) * 2 + 2}",
                TenantId = "105",
                Amount = 400,
                OrderId = orderId,
                Price = 200,
                ProductId = 14,
                Quantity = 2,
                IsEnabled = true,
                CreatedAt = createdAt,
                CreatedBy = 1,
                UpdatedAt = createdAt,
                UpdatedBy = 1
            });
        }

        removeIds = orders.Select(f => f.Id).ToList();
        await repository.BeginTransactionAsync();
        await repository.Delete<Order>()
            .UseTableBy("105", createdAt)
            .Where(f => removeIds.Contains(f.Id))
            .ExecuteAsync();
        await repository.Delete<OrderDetail>()
            .UseTableBy("105", createdAt)
            .Where(f => removeIds.Contains(f.OrderId))
            .ExecuteAsync();

        count1 = await repository.Create<Order>()
            .UseTableBy("105", createdAt)
            .WithBulkCopy(orders)
            .ExecuteAsync();
        count2 = await repository.Create<OrderDetail>()
            .UseTableBy("105", createdAt)
            .WithBulkCopy(orderDetails)
            .ExecuteAsync();
        await repository.CommitAsync();
        Assert.True(count1 == 1000);
        Assert.True(count2 == 2000);
    }
    [Fact]
    public async void Create_BulkCopy_WithoutUseTable()
    {
        var createdAt = DateTime.Parse("2024-05-24");
        var orders = new List<Order>();
        var orderDetails = new List<OrderDetail>();
        for (int i = 1000; i < 2000; i++)
        {
            var orderId = $"ON_{i + 1}";
            orders.Add(new Order
            {
                Id = orderId,
                TenantId = "104",
                OrderNo = $"ON-{i + 1}",
                BuyerId = 101,
                SellerId = 2,
                TotalAmount = 420,
                ProductCount = 2,
                Products = new List<int> { 1, 2 },
                Disputes = new Dispute
                {
                    Id = i + 1,
                    Content = "无良商家",
                    Result = "同意退款",
                    Users = "Buyer2,Seller2",
                    CreatedAt = createdAt
                },
                IsEnabled = true,
                CreatedAt = createdAt,
                CreatedBy = 1,
                UpdatedAt = createdAt,
                UpdatedBy = 1
            });
            orderDetails.Add(new OrderDetail
            {
                Id = $"OND_{1000 + (i - 1000) * 2 + 1}",
                TenantId = "104",
                Amount = 240,
                OrderId = orderId,
                Price = 120,
                ProductId = 11,
                Quantity = 2,
                IsEnabled = true,
                CreatedAt = createdAt,
                CreatedBy = 1,
                UpdatedAt = createdAt,
                UpdatedBy = 1
            });
            orderDetails.Add(new OrderDetail
            {
                Id = $"OND_{1000 + (i - 1000) * 2 + 2}",
                TenantId = "104",
                Amount = 180,
                OrderId = orderId,
                Price = 180,
                ProductId = 12,
                Quantity = 1,
                IsEnabled = true,
                CreatedAt = createdAt,
                CreatedBy = 1,
                UpdatedAt = createdAt,
                UpdatedBy = 1
            });
        }
        for (int i = 2000; i < 3000; i++)
        {
            var orderId = $"ON_{i + 1}";
            orders.Add(new Order
            {
                Id = orderId,
                TenantId = "105",
                OrderNo = $"ON-{i + 1}",
                BuyerId = 102,
                SellerId = 2,
                TotalAmount = 630,
                ProductCount = 2,
                Products = new List<int> { 1, 2 },
                Disputes = new Dispute
                {
                    Id = i + 1,
                    Content = "无良商家",
                    Result = "同意退款",
                    Users = "Buyer2,Seller2",
                    CreatedAt = DateTime.Now
                },
                IsEnabled = true,
                CreatedAt = createdAt,
                CreatedBy = 1,
                UpdatedAt = createdAt,
                UpdatedBy = 1
            });
            orderDetails.Add(new OrderDetail
            {
                Id = $"OND_{2000 + (i - 2000) * 2 + 1}",
                TenantId = "105",
                Amount = 230,
                OrderId = orderId,
                Price = 230,
                ProductId = 13,
                Quantity = 1,
                IsEnabled = true,
                CreatedAt = createdAt,
                CreatedBy = 1,
                UpdatedAt = createdAt,
                UpdatedBy = 1
            });
            orderDetails.Add(new OrderDetail
            {
                Id = $"OND_{2000 + (i - 2000) * 2 + 2}",
                TenantId = "105",
                Amount = 400,
                OrderId = orderId,
                Price = 200,
                ProductId = 14,
                Quantity = 2,
                IsEnabled = true,
                CreatedAt = createdAt,
                CreatedBy = 1,
                UpdatedAt = createdAt,
                UpdatedBy = 1
            });
        }
        var removeIds = orders.Select(f => f.Id).ToList();

        using var repository = this.dbFactory.Create();
        await repository.BeginTransactionAsync();
        await repository.Delete<Order>()
            .UseTableBy("104", createdAt)
            .Where(f => removeIds.Contains(f.Id))
            .ExecuteAsync();
        await repository.Delete<Order>()
            .UseTableBy("105", createdAt)
            .Where(f => removeIds.Contains(f.Id))
            .ExecuteAsync();
        await repository.Delete<OrderDetail>()
            .UseTableBy("104", createdAt)
            .Where(f => removeIds.Contains(f.OrderId))
            .ExecuteAsync();
        await repository.Delete<OrderDetail>()
            .UseTableBy("105", createdAt)
            .Where(f => removeIds.Contains(f.OrderId))
            .ExecuteAsync();

        var count1 = await repository.Create<Order>()
            .WithBulkCopy(orders)
            .ExecuteAsync();
        var count2 = await repository.Create<OrderDetail>()
            .WithBulkCopy(orderDetails)
            .ExecuteAsync();
        await repository.CommitAsync();
        Assert.True(count1 == 2000);
        Assert.True(count2 == 4000);
    }
    [Fact]
    public async void Query_ManySharding_SingleTable()
    {
        await this.InitSharding();
        var productCount = 1;
        using var repository = dbFactory.Create();
        var sql = repository.From<Order>()
            .UseTable("sys_order_104_202405", "sys_order_105_202405")
            .Where(f => f.ProductCount > productCount)
            .ToSql(out _);
        Assert.True(sql == "SELECT a.[Id],a.[TenantId],a.[OrderNo],a.[ProductCount],a.[TotalAmount],a.[BuyerId],a.[BuyerSource],a.[SellerId],a.[Products],a.[Disputes],a.[IsEnabled],a.[CreatedAt],a.[CreatedBy],a.[UpdatedAt],a.[UpdatedBy] FROM [sys_order_104_202405] a WHERE a.[ProductCount]>@p0 UNION ALL SELECT a.[Id],a.[TenantId],a.[OrderNo],a.[ProductCount],a.[TotalAmount],a.[BuyerId],a.[BuyerSource],a.[SellerId],a.[Products],a.[Disputes],a.[IsEnabled],a.[CreatedAt],a.[CreatedBy],a.[UpdatedAt],a.[UpdatedBy] FROM [sys_order_105_202405] a WHERE a.[ProductCount]>@p0");

        var result = repository.From<Order>()
            .UseTable("sys_order_104_202405", "sys_order_105_202405")
            .Where(f => f.ProductCount > productCount)
            .ToList();
        if (result.Count > 0)
        {
            var tenantIds = result.Select(f => f.TenantId).ToList();
            Assert.True(tenantIds.Exists(f => "104,105".Contains(f)));
        }
    }
    [Fact]
    public async void Query_SingleSharding_Value()
    {
        await this.InitSharding();
        var orderId = "ON_1015";
        using var repository = dbFactory.Create();
        var sql = repository.From<Order>()
            .UseTableBy("104", DateTime.Parse("2024-05-01"))
            .InnerJoin<User>((x, y) => x.BuyerId == y.Id)
            .UseTableBy("104")
            .Where((x, y) => x.Id == orderId)
            .Select((x, y) => new { x.Id, x.OrderNo, x.TenantId, x.BuyerId, BuyerName = y.Name })
            .ToSql(out _);
        Assert.True(sql == "SELECT a.[Id],a.[OrderNo],a.[TenantId],a.[BuyerId],b.[Name] AS [BuyerName] FROM [sys_order_104_202405] a INNER JOIN [sys_user_104] b ON a.[BuyerId]=b.[Id] WHERE a.[Id]=@p0");

        var result = await repository.From<Order>()
            .UseTableBy("104", DateTime.Parse("2024-05-01"))
            .InnerJoin<User>((x, y) => x.BuyerId == y.Id)
            .UseTableBy("104")
            .Where((x, y) => x.Id == orderId)
            .Select((x, y) => new { x.Id, x.OrderNo, x.TenantId, x.BuyerId, BuyerName = y.Name })
            .FirstAsync();
        if (result != null)
        {
            Assert.True(result.TenantId == "104");
        }
    }
    [Fact]
    public async void Query_ManySharding_SingleTable_SubQuery()
    {
        await this.InitSharding();
        using var repository = dbFactory.Create();
        var sql = repository
            .From(f => f.From<OrderDetail>()
                .UseTable("sys_order_detail_104_202405", "sys_order_detail_105_202405")
                .InnerJoin<Order>((x, y) => x.OrderId == y.Id)
                .UseTable<OrderDetail>((dbKey, orderOrigName, userOrigName, orderTableName) => orderTableName.Replace(orderOrigName, userOrigName))
                .GroupBy((a, b) => new { OrderId = b.Id, b.BuyerId })
                .Select((x, a, b) => new { Group = x.Grouping, ProductCount = x.CountDistinct(a.ProductId) }))
            .InnerJoin<User>((x, y) => x.Group.BuyerId == y.Id)
            .UseTable<OrderDetail>((dbKey, orderOrigName, userOrigName, orderTableName) =>
            {
                var tableName = orderTableName.Replace(orderOrigName, userOrigName);
                return tableName.Substring(0, tableName.Length - 7);
            })
            .Where((a, b) => a.ProductCount > 1)
            .Select((x, y) => new
            {
                x.Group,
                y.TenantId,
                Buyer = y,
                x.ProductCount
            })
            .ToSql(out _);
        Assert.True(sql == "SELECT a.[OrderId],a.[BuyerId],b.[TenantId],b.[Id],b.[TenantId],b.[Name],b.[Gender],b.[Age],b.[CompanyId],b.[GuidField],b.[SomeTimes],b.[SourceType],b.[IsEnabled],b.[CreatedAt],b.[CreatedBy],b.[UpdatedAt],b.[UpdatedBy],a.[ProductCount] FROM (SELECT b.[Id] AS [OrderId],b.[BuyerId],COUNT(DISTINCT a.[ProductId]) AS [ProductCount] FROM [sys_order_detail_104_202405] a INNER JOIN [sys_order_104_202405] b ON a.[OrderId]=b.[Id] GROUP BY b.[Id],b.[BuyerId]) a INNER JOIN [sys_user_104] b ON a.[BuyerId]=b.[Id] WHERE a.[ProductCount]>1 UNION ALL SELECT a.[OrderId],a.[BuyerId],b.[TenantId],b.[Id],b.[TenantId],b.[Name],b.[Gender],b.[Age],b.[CompanyId],b.[GuidField],b.[SomeTimes],b.[SourceType],b.[IsEnabled],b.[CreatedAt],b.[CreatedBy],b.[UpdatedAt],b.[UpdatedBy],a.[ProductCount] FROM (SELECT b.[Id] AS [OrderId],b.[BuyerId],COUNT(DISTINCT a.[ProductId]) AS [ProductCount] FROM [sys_order_detail_105_202405] a INNER JOIN [sys_order_105_202405] b ON a.[OrderId]=b.[Id] GROUP BY b.[Id],b.[BuyerId]) a INNER JOIN [sys_user_105] b ON a.[BuyerId]=b.[Id] WHERE a.[ProductCount]>1");

        var result = await repository
            .From(f => f.From<OrderDetail>()
                .UseTable("sys_order_detail_104_202405", "sys_order_detail_105_202405")
                .InnerJoin<Order>((x, y) => x.OrderId == y.Id)
                .UseTable<OrderDetail>((dbKey, orderOrigName, userOrigName, orderTableName) => orderTableName.Replace(orderOrigName, userOrigName))
                .GroupBy((a, b) => new { OrderId = b.Id, b.BuyerId })
                .Select((x, a, b) => new { Group = x.Grouping, ProductCount = x.CountDistinct(a.ProductId) }))
            .InnerJoin<User>((x, y) => x.Group.BuyerId == y.Id)
            .UseTable<OrderDetail>((dbKey, orderOrigName, userOrigName, orderTableName) =>
            {
                var tableName = orderTableName.Replace(orderOrigName, userOrigName);
                return tableName.Substring(0, tableName.Length - 7);
            })
            .Where((a, b) => a.ProductCount > 1)
            .Select((x, y) => new
            {
                x.Group,
                y.TenantId,
                Buyer = y,
                x.ProductCount
            })
            .ToListAsync();
        if (result.Count > 0)
        {
            Assert.NotNull(result[0]);
            Assert.NotNull(result[0].Group);
            Assert.NotNull(result[0].Buyer);
            Assert.True(result[0].ProductCount > 1);
            var tenantIds = result.Select(f => f.TenantId).Distinct().ToList();
            Assert.True(!tenantIds.Exists(f => f != "104" && f != "105"));
        }
    }
    [Fact]
    public async void Query_ManySharding_MultiTable1()
    {
        await this.InitSharding();
        var productCount = 1;
        using var repository = dbFactory.Create();
        var sql = repository.From<Order>()
            .UseTable("sys_order_104_202405", "sys_order_105_202405")
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
        Assert.True(sql == "SELECT a.[Id],a.[TenantId],a.[OrderNo],a.[ProductCount],a.[TotalAmount],a.[BuyerId],a.[BuyerSource],a.[SellerId],a.[Products],a.[Disputes],a.[IsEnabled],a.[CreatedAt],a.[CreatedBy],a.[UpdatedAt],a.[UpdatedBy],b.[Id],b.[TenantId],b.[Name],b.[Gender],b.[Age],b.[CompanyId],b.[GuidField],b.[SomeTimes],b.[SourceType],b.[IsEnabled],b.[CreatedAt],b.[CreatedBy],b.[UpdatedAt],b.[UpdatedBy] FROM [sys_order_104_202405] a INNER JOIN [sys_user_104] b ON a.[BuyerId]=b.[Id] WHERE a.[ProductCount]>@p0 UNION ALL SELECT a.[Id],a.[TenantId],a.[OrderNo],a.[ProductCount],a.[TotalAmount],a.[BuyerId],a.[BuyerSource],a.[SellerId],a.[Products],a.[Disputes],a.[IsEnabled],a.[CreatedAt],a.[CreatedBy],a.[UpdatedAt],a.[UpdatedBy],b.[Id],b.[TenantId],b.[Name],b.[Gender],b.[Age],b.[CompanyId],b.[GuidField],b.[SomeTimes],b.[SourceType],b.[IsEnabled],b.[CreatedAt],b.[CreatedBy],b.[UpdatedAt],b.[UpdatedBy] FROM [sys_order_105_202405] a INNER JOIN [sys_user_105] b ON a.[BuyerId]=b.[Id] WHERE a.[ProductCount]>@p0");

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
            Assert.True(tenantIds.Exists(f => "104,105".Contains(f)));
        }
    }
    [Fact]
    public async void Query_ManySharding_MultiTable2()
    {
        await this.InitSharding();
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
        Assert.True(sql == "SELECT a.[Id],a.[TenantId],a.[OrderNo],a.[ProductCount],a.[TotalAmount],a.[BuyerId],a.[BuyerSource],a.[SellerId],a.[Products],a.[Disputes],a.[IsEnabled],a.[CreatedAt],a.[CreatedBy],a.[UpdatedAt],a.[UpdatedBy],b.[Id],b.[TenantId],b.[Name],b.[Gender],b.[Age],b.[CompanyId],b.[GuidField],b.[SomeTimes],b.[SourceType],b.[IsEnabled],b.[CreatedAt],b.[CreatedBy],b.[UpdatedAt],b.[UpdatedBy] FROM [sys_order_104_202405] a INNER JOIN [sys_user_104] b ON a.[BuyerId]=b.[Id] WHERE a.[ProductCount]>@p0 UNION ALL SELECT a.[Id],a.[TenantId],a.[OrderNo],a.[ProductCount],a.[TotalAmount],a.[BuyerId],a.[BuyerSource],a.[SellerId],a.[Products],a.[Disputes],a.[IsEnabled],a.[CreatedAt],a.[CreatedBy],a.[UpdatedAt],a.[UpdatedBy],b.[Id],b.[TenantId],b.[Name],b.[Gender],b.[Age],b.[CompanyId],b.[GuidField],b.[SomeTimes],b.[SourceType],b.[IsEnabled],b.[CreatedAt],b.[CreatedBy],b.[UpdatedAt],b.[UpdatedBy] FROM [sys_order_105_202405] a INNER JOIN [sys_user_105] b ON a.[BuyerId]=b.[Id] WHERE a.[ProductCount]>@p0");

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
            Assert.True(tenantIds.Exists(f => "104,105".Contains(f)));
        }
    }
    [Fact]
    public async void Query_ManySharding_MultiTable3()
    {
        await this.InitSharding();
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
        Assert.True(sql == "SELECT a.[Id],a.[TenantId],a.[OrderNo],a.[ProductCount],a.[TotalAmount],a.[BuyerId],a.[BuyerSource],a.[SellerId],a.[Products],a.[Disputes],a.[IsEnabled],a.[CreatedAt],a.[CreatedBy],a.[UpdatedAt],a.[UpdatedBy],b.[Id],b.[TenantId],b.[OrderId],b.[ProductId],b.[Price],b.[Quantity],b.[Amount],b.[IsEnabled],b.[CreatedAt],b.[CreatedBy],b.[UpdatedAt],b.[UpdatedBy] FROM [sys_order_104_202405] a INNER JOIN [sys_order_detail_104_202405] b ON a.[Id]=b.[OrderId] WHERE a.[ProductCount]>@p0 UNION ALL SELECT a.[Id],a.[TenantId],a.[OrderNo],a.[ProductCount],a.[TotalAmount],a.[BuyerId],a.[BuyerSource],a.[SellerId],a.[Products],a.[Disputes],a.[IsEnabled],a.[CreatedAt],a.[CreatedBy],a.[UpdatedAt],a.[UpdatedBy],b.[Id],b.[TenantId],b.[OrderId],b.[ProductId],b.[Price],b.[Quantity],b.[Amount],b.[IsEnabled],b.[CreatedAt],b.[CreatedBy],b.[UpdatedAt],b.[UpdatedBy] FROM [sys_order_105_202405] a INNER JOIN [sys_order_detail_105_202405] b ON a.[Id]=b.[OrderId] WHERE a.[ProductCount]>@p0");

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
            var tenantIds = result.Select(f => f.Order.TenantId).Distinct().ToList();
            Assert.True(tenantIds.Exists(f => "104,105".Contains(f)));
        }
    }
    [Fact]
    public async void Query_SingleSharding_Exists1()
    {
        await this.InitSharding();
        using var repository = dbFactory.Create();
        var sql = repository.From<Order>()
            .UseTableBy("104", DateTime.Parse("2024-05-24"))
            .Where(f => repository.From<User>('b')
                .UseTableBy("104", DateTime.Parse("2024-05-24"))
                .Exists(t => t.Id == f.BuyerId && t.Age < 25))
            .ToSql(out _);
        Assert.True(sql == "SELECT a.[Id],a.[TenantId],a.[OrderNo],a.[ProductCount],a.[TotalAmount],a.[BuyerId],a.[BuyerSource],a.[SellerId],a.[Products],a.[Disputes],a.[IsEnabled],a.[CreatedAt],a.[CreatedBy],a.[UpdatedAt],a.[UpdatedBy] FROM [sys_order_104_202405] a WHERE EXISTS(SELECT * FROM [sys_user_104] b WHERE b.[Id]=a.[BuyerId] AND b.[Age]<25)");

        var result = repository.From<Order>()
            .UseTableBy("104", DateTime.Parse("2024-05-24"))
            .Where(f => repository.From<User>('b')
                .UseTableBy("104", DateTime.Parse("2024-05-24"))
                .Exists(t => t.Id == f.BuyerId && t.Age < 25))
            .ToList();
        if (result.Count > 0)
        {
            var tenantIds = result.Select(f => f.TenantId).Distinct().ToList();
            Assert.Contains("104", tenantIds);
        }
    }
    [Fact]
    public async void Query_SingleSharding_Exists2()
    {
        await this.InitSharding();
        using var repository = dbFactory.Create();
        var sql = repository.From<Order>()
            .UseTableBy("104", DateTime.Parse("2024-05-24"))
            .Where(f => repository.From<User>('b')
                .UseTableBy("104", DateTime.Parse("2024-05-24"))
                .InnerJoin<OrderDetail>((x, y) => f.Id == y.OrderId)
                .UseTableBy("104", DateTime.Parse("2024-05-24"))
                .Exists((x, y) => x.Id == f.BuyerId && x.Age <= 25 && y.Price > 100))
            .ToSql(out _);
        Assert.True(sql == "SELECT a.[Id],a.[TenantId],a.[OrderNo],a.[ProductCount],a.[TotalAmount],a.[BuyerId],a.[BuyerSource],a.[SellerId],a.[Products],a.[Disputes],a.[IsEnabled],a.[CreatedAt],a.[CreatedBy],a.[UpdatedAt],a.[UpdatedBy] FROM [sys_order_104_202405] a WHERE EXISTS(SELECT * FROM [sys_user_104] b INNER JOIN [sys_order_detail_104_202405] c ON a.[Id]=c.[OrderId] WHERE b.[Id]=a.[BuyerId] AND b.[Age]<=25 AND c.[Price]>100)");

        sql = repository.From<Order>()
            .UseTableBy("104", DateTime.Parse("2024-05-24"))
            .Where(f => repository.From<User>('b')
                .UseTableBy("104", DateTime.Parse("2024-05-24"))
                .InnerJoin<OrderDetail>((x, y) => f.Id == y.OrderId)
                .UseTableBy("104", DateTime.Parse("2024-05-24"))
                .Exists((x, y) => x.Id == f.BuyerId && x.Age <= 25 && y.Price > 100))
            .ToSql(out _);
        Assert.True(sql == "SELECT a.[Id],a.[TenantId],a.[OrderNo],a.[ProductCount],a.[TotalAmount],a.[BuyerId],a.[BuyerSource],a.[SellerId],a.[Products],a.[Disputes],a.[IsEnabled],a.[CreatedAt],a.[CreatedBy],a.[UpdatedAt],a.[UpdatedBy] FROM [sys_order_104_202405] a WHERE EXISTS(SELECT * FROM [sys_user_104] b INNER JOIN [sys_order_detail_104_202405] c ON a.[Id]=c.[OrderId] WHERE b.[Id]=a.[BuyerId] AND b.[Age]<=25 AND c.[Price]>100)");

        var result = repository.From<Order>()
            .UseTableBy("104", DateTime.Parse("2024-05-24"))
            .Where(f => repository.From<User>('b')
                .UseTableBy("104", DateTime.Parse("2024-05-24"))
                .InnerJoin<OrderDetail>((x, y) => f.Id == y.OrderId)
                .UseTableBy("104", DateTime.Parse("2024-05-24"))
                .Exists((x, y) => x.Id == f.BuyerId && x.Age <= 25 && y.Price > 100))
            .ToList();
        if (result.Count > 0)
        {
            var tenantIds = result.Select(f => f.TenantId).Distinct().ToList();
            Assert.Contains("104", tenantIds);
        }
    }
    [Fact]
    public async void Update_SingleSharding()
    {
        await this.InitSharding();
        using var repository = dbFactory.Create();
        var orderIds = new string[] { "ON_1001", "ON_1002", "ON_1003", "ON_1004" };
        var sql = repository.Update<Order>()
            .UseTableBy("104", DateTime.Parse("2024-05-24"))
            .Set(new { TotalAmount = 400 })
            .Where(f => orderIds.Contains(f.Id))
            .ToSql(out var dbParameters);
        Assert.True(sql == "UPDATE [sys_order_104_202405] SET [TotalAmount]=@TotalAmount WHERE [Id] IN (@p1,@p2,@p3,@p4)");
        Assert.True((double)dbParameters[0].Value == 400);
        Assert.True(((SqlParameter)dbParameters[0]).SqlDbType == SqlDbType.Float);
        Assert.True((string)dbParameters[1].Value == orderIds[0]);
        Assert.True((string)dbParameters[2].Value == orderIds[1]);
        Assert.True((string)dbParameters[3].Value == orderIds[2]);
        Assert.True((string)dbParameters[4].Value == orderIds[3]);

        var result = await repository.Update<Order>()
            .UseTableBy("104", DateTime.Parse("2024-05-24"))
            .Set(new { TotalAmount = 400 })
            .Where(f => orderIds.Contains(f.Id))
            .ExecuteAsync();
        Assert.True(result > 0);
    }
    [Fact]
    public async void Update_ManySharding1()
    {
        await this.InitSharding();
        using var repository = dbFactory.Create();
        var orderIds = new string[] { "ON_1001", "ON_1002", "ON_2003", "ON_2004" };
        var sql = repository.Update<Order>()
            .UseTable("sys_order_104_202405", "sys_order_105_202405")
            .Set(new { TotalAmount = 400 })
            .Where(f => orderIds.Contains(f.Id))
            .ToSql(out var dbParameters);
        Assert.True(sql == "UPDATE [sys_order_104_202405] SET [TotalAmount]=@TotalAmount WHERE [Id] IN (@p1,@p2,@p3,@p4);UPDATE [sys_order_105_202405] SET [TotalAmount]=@TotalAmount WHERE [Id] IN (@p1,@p2,@p3,@p4)");
        Assert.True((double)dbParameters[0].Value == 400);
        Assert.True(((SqlParameter)dbParameters[0]).SqlDbType == SqlDbType.Float);
        Assert.True((string)dbParameters[1].Value == orderIds[0]);
        Assert.True((string)dbParameters[2].Value == orderIds[1]);
        Assert.True((string)dbParameters[3].Value == orderIds[2]);
        Assert.True((string)dbParameters[4].Value == orderIds[3]);

        await repository.BeginTransactionAsync();
        var result = await repository.Update<Order>()
            .UseTable("sys_order_104_202405", "sys_order_105_202405")
            .Set(new { TotalAmount = 400 })
            .Where(f => orderIds.Contains(f.Id))
            .ExecuteAsync();
        var orders = await repository.From<Order>()
            .UseTable("sys_order_104_202405", "sys_order_105_202405")
            .Where(f => orderIds.Contains(f.Id))
            .ToListAsync();
        await repository.CommitAsync();

        Assert.True(result > 0);
        foreach (var order in orders)
        {
            Assert.True(order.TotalAmount == 400);
            Assert.True(order.TenantId == "104" || order.TenantId == "105");
            Assert.Contains(order.Id, orderIds);
        }
    }
    [Fact]
    public async void Update_ManySharding2()
    {
        await this.InitSharding();
        using var repository = dbFactory.Create();
        var orderIds = new string[] { "ON_1001", "ON_1002", "ON_2003", "ON_2004" };
        var sql = repository.Update<Order>()
            .UseTable(f => (f.Contains("_104_") || f.Contains("_105_")) && int.Parse(f.Substring(f.Length - 6)) > 202001)
            .Set(new { TotalAmount = 400 })
            .Where(f => orderIds.Contains(f.Id))
            .ToSql(out var dbParameters);
        Assert.True(sql == "SELECT name FROM sys.sysobjects WHERE xtype='U' AND name LIKE 'sys_order%';UPDATE [sys_order_104_202405] SET [TotalAmount]=@TotalAmount WHERE [Id] IN (@p1,@p2,@p3,@p4);UPDATE [sys_order_105_202405] SET [TotalAmount]=@TotalAmount WHERE [Id] IN (@p1,@p2,@p3,@p4)");
        Assert.True((double)dbParameters[0].Value == 400);
        Assert.True(((SqlParameter)dbParameters[0]).SqlDbType == SqlDbType.Float);
        Assert.True((string)dbParameters[1].Value == orderIds[0]);
        Assert.True((string)dbParameters[2].Value == orderIds[1]);
        Assert.True((string)dbParameters[3].Value == orderIds[2]);
        Assert.True((string)dbParameters[4].Value == orderIds[3]);

        await repository.BeginTransactionAsync();
        var result = await repository.Update<Order>()
            .UseTable(f => (f.Contains("_104_") || f.Contains("_105_")) && int.Parse(f.Substring(f.Length - 6)) > 202001)
            .Set(new { TotalAmount = 400 })
            .Where(f => orderIds.Contains(f.Id))
            .ExecuteAsync();
        var orders = await repository.From<Order>()
            .UseTable("sys_order_104_202405", "sys_order_105_202405")
            .Where(f => orderIds.Contains(f.Id))
            .ToListAsync();
        await repository.CommitAsync();

        Assert.True(result > 0);
        foreach (var order in orders)
        {
            Assert.True(order.TotalAmount == 400);
            Assert.True(order.TenantId == "104" || order.TenantId == "105");
            Assert.Contains(order.Id, orderIds);
        }
    }
    [Fact]
    public async void Update_SetBulk_ManySharding()
    {
        await this.InitSharding();
        var createdAt = DateTime.Parse("2024-05-24");
        using var repository = dbFactory.Create();
        var orders = repository.From<Order>()
            .UseTable(f => (f.Contains("_104_") || f.Contains("_105_")) && int.Parse(f.Substring(f.Length - 6)) > 202001)
            .Select(f => new
            {
                f.Id,
                f.OrderNo,
                f.BuyerId,
                f.SellerId,
                TotalAmount = f.TotalAmount + 50,
                ProductCount = 3,
                UpdatedAt = DateTime.Now
            })
            .OrderByDescending(f => f.Id)
            .Take(20)
            .ToList();
        var orderIds = orders.Select(f => f.Id).ToList();

        var sql = repository.Update<Order>()
            .UseTable("sys_order_104_202405", "sys_order_105_202405")
            .SetBulk(orders, 10)
            .Set(f => f.BuyerSource, UserSourceType.Wechat)
            .IgnoreFields(f => new { f.OrderNo, f.BuyerId, f.SellerId })
            .ToSql(out var dbParameters);

        Assert.True(sql == "UPDATE [sys_order_104_202405] SET [BuyerSource]=@BuyerSource,[TotalAmount]=@TotalAmount0,[ProductCount]=@ProductCount0,[UpdatedAt]=@UpdatedAt0 WHERE [Id]=@kId0;UPDATE [sys_order_105_202405] SET [BuyerSource]=@BuyerSource,[TotalAmount]=@TotalAmount0,[ProductCount]=@ProductCount0,[UpdatedAt]=@UpdatedAt0 WHERE [Id]=@kId0;UPDATE [sys_order_104_202405] SET [BuyerSource]=@BuyerSource,[TotalAmount]=@TotalAmount1,[ProductCount]=@ProductCount1,[UpdatedAt]=@UpdatedAt1 WHERE [Id]=@kId1;UPDATE [sys_order_105_202405] SET [BuyerSource]=@BuyerSource,[TotalAmount]=@TotalAmount1,[ProductCount]=@ProductCount1,[UpdatedAt]=@UpdatedAt1 WHERE [Id]=@kId1;UPDATE [sys_order_104_202405] SET [BuyerSource]=@BuyerSource,[TotalAmount]=@TotalAmount2,[ProductCount]=@ProductCount2,[UpdatedAt]=@UpdatedAt2 WHERE [Id]=@kId2;UPDATE [sys_order_105_202405] SET [BuyerSource]=@BuyerSource,[TotalAmount]=@TotalAmount2,[ProductCount]=@ProductCount2,[UpdatedAt]=@UpdatedAt2 WHERE [Id]=@kId2;UPDATE [sys_order_104_202405] SET [BuyerSource]=@BuyerSource,[TotalAmount]=@TotalAmount3,[ProductCount]=@ProductCount3,[UpdatedAt]=@UpdatedAt3 WHERE [Id]=@kId3;UPDATE [sys_order_105_202405] SET [BuyerSource]=@BuyerSource,[TotalAmount]=@TotalAmount3,[ProductCount]=@ProductCount3,[UpdatedAt]=@UpdatedAt3 WHERE [Id]=@kId3;UPDATE [sys_order_104_202405] SET [BuyerSource]=@BuyerSource,[TotalAmount]=@TotalAmount4,[ProductCount]=@ProductCount4,[UpdatedAt]=@UpdatedAt4 WHERE [Id]=@kId4;UPDATE [sys_order_105_202405] SET [BuyerSource]=@BuyerSource,[TotalAmount]=@TotalAmount4,[ProductCount]=@ProductCount4,[UpdatedAt]=@UpdatedAt4 WHERE [Id]=@kId4;UPDATE [sys_order_104_202405] SET [BuyerSource]=@BuyerSource,[TotalAmount]=@TotalAmount5,[ProductCount]=@ProductCount5,[UpdatedAt]=@UpdatedAt5 WHERE [Id]=@kId5;UPDATE [sys_order_105_202405] SET [BuyerSource]=@BuyerSource,[TotalAmount]=@TotalAmount5,[ProductCount]=@ProductCount5,[UpdatedAt]=@UpdatedAt5 WHERE [Id]=@kId5;UPDATE [sys_order_104_202405] SET [BuyerSource]=@BuyerSource,[TotalAmount]=@TotalAmount6,[ProductCount]=@ProductCount6,[UpdatedAt]=@UpdatedAt6 WHERE [Id]=@kId6;UPDATE [sys_order_105_202405] SET [BuyerSource]=@BuyerSource,[TotalAmount]=@TotalAmount6,[ProductCount]=@ProductCount6,[UpdatedAt]=@UpdatedAt6 WHERE [Id]=@kId6;UPDATE [sys_order_104_202405] SET [BuyerSource]=@BuyerSource,[TotalAmount]=@TotalAmount7,[ProductCount]=@ProductCount7,[UpdatedAt]=@UpdatedAt7 WHERE [Id]=@kId7;UPDATE [sys_order_105_202405] SET [BuyerSource]=@BuyerSource,[TotalAmount]=@TotalAmount7,[ProductCount]=@ProductCount7,[UpdatedAt]=@UpdatedAt7 WHERE [Id]=@kId7;UPDATE [sys_order_104_202405] SET [BuyerSource]=@BuyerSource,[TotalAmount]=@TotalAmount8,[ProductCount]=@ProductCount8,[UpdatedAt]=@UpdatedAt8 WHERE [Id]=@kId8;UPDATE [sys_order_105_202405] SET [BuyerSource]=@BuyerSource,[TotalAmount]=@TotalAmount8,[ProductCount]=@ProductCount8,[UpdatedAt]=@UpdatedAt8 WHERE [Id]=@kId8;UPDATE [sys_order_104_202405] SET [BuyerSource]=@BuyerSource,[TotalAmount]=@TotalAmount9,[ProductCount]=@ProductCount9,[UpdatedAt]=@UpdatedAt9 WHERE [Id]=@kId9;UPDATE [sys_order_105_202405] SET [BuyerSource]=@BuyerSource,[TotalAmount]=@TotalAmount9,[ProductCount]=@ProductCount9,[UpdatedAt]=@UpdatedAt9 WHERE [Id]=@kId9;UPDATE [sys_order_104_202405] SET [BuyerSource]=@BuyerSource,[TotalAmount]=@TotalAmount10,[ProductCount]=@ProductCount10,[UpdatedAt]=@UpdatedAt10 WHERE [Id]=@kId10;UPDATE [sys_order_105_202405] SET [BuyerSource]=@BuyerSource,[TotalAmount]=@TotalAmount10,[ProductCount]=@ProductCount10,[UpdatedAt]=@UpdatedAt10 WHERE [Id]=@kId10;UPDATE [sys_order_104_202405] SET [BuyerSource]=@BuyerSource,[TotalAmount]=@TotalAmount11,[ProductCount]=@ProductCount11,[UpdatedAt]=@UpdatedAt11 WHERE [Id]=@kId11;UPDATE [sys_order_105_202405] SET [BuyerSource]=@BuyerSource,[TotalAmount]=@TotalAmount11,[ProductCount]=@ProductCount11,[UpdatedAt]=@UpdatedAt11 WHERE [Id]=@kId11;UPDATE [sys_order_104_202405] SET [BuyerSource]=@BuyerSource,[TotalAmount]=@TotalAmount12,[ProductCount]=@ProductCount12,[UpdatedAt]=@UpdatedAt12 WHERE [Id]=@kId12;UPDATE [sys_order_105_202405] SET [BuyerSource]=@BuyerSource,[TotalAmount]=@TotalAmount12,[ProductCount]=@ProductCount12,[UpdatedAt]=@UpdatedAt12 WHERE [Id]=@kId12;UPDATE [sys_order_104_202405] SET [BuyerSource]=@BuyerSource,[TotalAmount]=@TotalAmount13,[ProductCount]=@ProductCount13,[UpdatedAt]=@UpdatedAt13 WHERE [Id]=@kId13;UPDATE [sys_order_105_202405] SET [BuyerSource]=@BuyerSource,[TotalAmount]=@TotalAmount13,[ProductCount]=@ProductCount13,[UpdatedAt]=@UpdatedAt13 WHERE [Id]=@kId13;UPDATE [sys_order_104_202405] SET [BuyerSource]=@BuyerSource,[TotalAmount]=@TotalAmount14,[ProductCount]=@ProductCount14,[UpdatedAt]=@UpdatedAt14 WHERE [Id]=@kId14;UPDATE [sys_order_105_202405] SET [BuyerSource]=@BuyerSource,[TotalAmount]=@TotalAmount14,[ProductCount]=@ProductCount14,[UpdatedAt]=@UpdatedAt14 WHERE [Id]=@kId14;UPDATE [sys_order_104_202405] SET [BuyerSource]=@BuyerSource,[TotalAmount]=@TotalAmount15,[ProductCount]=@ProductCount15,[UpdatedAt]=@UpdatedAt15 WHERE [Id]=@kId15;UPDATE [sys_order_105_202405] SET [BuyerSource]=@BuyerSource,[TotalAmount]=@TotalAmount15,[ProductCount]=@ProductCount15,[UpdatedAt]=@UpdatedAt15 WHERE [Id]=@kId15;UPDATE [sys_order_104_202405] SET [BuyerSource]=@BuyerSource,[TotalAmount]=@TotalAmount16,[ProductCount]=@ProductCount16,[UpdatedAt]=@UpdatedAt16 WHERE [Id]=@kId16;UPDATE [sys_order_105_202405] SET [BuyerSource]=@BuyerSource,[TotalAmount]=@TotalAmount16,[ProductCount]=@ProductCount16,[UpdatedAt]=@UpdatedAt16 WHERE [Id]=@kId16;UPDATE [sys_order_104_202405] SET [BuyerSource]=@BuyerSource,[TotalAmount]=@TotalAmount17,[ProductCount]=@ProductCount17,[UpdatedAt]=@UpdatedAt17 WHERE [Id]=@kId17;UPDATE [sys_order_105_202405] SET [BuyerSource]=@BuyerSource,[TotalAmount]=@TotalAmount17,[ProductCount]=@ProductCount17,[UpdatedAt]=@UpdatedAt17 WHERE [Id]=@kId17;UPDATE [sys_order_104_202405] SET [BuyerSource]=@BuyerSource,[TotalAmount]=@TotalAmount18,[ProductCount]=@ProductCount18,[UpdatedAt]=@UpdatedAt18 WHERE [Id]=@kId18;UPDATE [sys_order_105_202405] SET [BuyerSource]=@BuyerSource,[TotalAmount]=@TotalAmount18,[ProductCount]=@ProductCount18,[UpdatedAt]=@UpdatedAt18 WHERE [Id]=@kId18;UPDATE [sys_order_104_202405] SET [BuyerSource]=@BuyerSource,[TotalAmount]=@TotalAmount19,[ProductCount]=@ProductCount19,[UpdatedAt]=@UpdatedAt19 WHERE [Id]=@kId19;UPDATE [sys_order_105_202405] SET [BuyerSource]=@BuyerSource,[TotalAmount]=@TotalAmount19,[ProductCount]=@ProductCount19,[UpdatedAt]=@UpdatedAt19 WHERE [Id]=@kId19;UPDATE [sys_order_104_202405] SET [BuyerSource]=@BuyerSource,[TotalAmount]=@TotalAmount20,[ProductCount]=@ProductCount20,[UpdatedAt]=@UpdatedAt20 WHERE [Id]=@kId20;UPDATE [sys_order_105_202405] SET [BuyerSource]=@BuyerSource,[TotalAmount]=@TotalAmount20,[ProductCount]=@ProductCount20,[UpdatedAt]=@UpdatedAt20 WHERE [Id]=@kId20;UPDATE [sys_order_104_202405] SET [BuyerSource]=@BuyerSource,[TotalAmount]=@TotalAmount21,[ProductCount]=@ProductCount21,[UpdatedAt]=@UpdatedAt21 WHERE [Id]=@kId21;UPDATE [sys_order_105_202405] SET [BuyerSource]=@BuyerSource,[TotalAmount]=@TotalAmount21,[ProductCount]=@ProductCount21,[UpdatedAt]=@UpdatedAt21 WHERE [Id]=@kId21;UPDATE [sys_order_104_202405] SET [BuyerSource]=@BuyerSource,[TotalAmount]=@TotalAmount22,[ProductCount]=@ProductCount22,[UpdatedAt]=@UpdatedAt22 WHERE [Id]=@kId22;UPDATE [sys_order_105_202405] SET [BuyerSource]=@BuyerSource,[TotalAmount]=@TotalAmount22,[ProductCount]=@ProductCount22,[UpdatedAt]=@UpdatedAt22 WHERE [Id]=@kId22;UPDATE [sys_order_104_202405] SET [BuyerSource]=@BuyerSource,[TotalAmount]=@TotalAmount23,[ProductCount]=@ProductCount23,[UpdatedAt]=@UpdatedAt23 WHERE [Id]=@kId23;UPDATE [sys_order_105_202405] SET [BuyerSource]=@BuyerSource,[TotalAmount]=@TotalAmount23,[ProductCount]=@ProductCount23,[UpdatedAt]=@UpdatedAt23 WHERE [Id]=@kId23;UPDATE [sys_order_104_202405] SET [BuyerSource]=@BuyerSource,[TotalAmount]=@TotalAmount24,[ProductCount]=@ProductCount24,[UpdatedAt]=@UpdatedAt24 WHERE [Id]=@kId24;UPDATE [sys_order_105_202405] SET [BuyerSource]=@BuyerSource,[TotalAmount]=@TotalAmount24,[ProductCount]=@ProductCount24,[UpdatedAt]=@UpdatedAt24 WHERE [Id]=@kId24;UPDATE [sys_order_104_202405] SET [BuyerSource]=@BuyerSource,[TotalAmount]=@TotalAmount25,[ProductCount]=@ProductCount25,[UpdatedAt]=@UpdatedAt25 WHERE [Id]=@kId25;UPDATE [sys_order_105_202405] SET [BuyerSource]=@BuyerSource,[TotalAmount]=@TotalAmount25,[ProductCount]=@ProductCount25,[UpdatedAt]=@UpdatedAt25 WHERE [Id]=@kId25;UPDATE [sys_order_104_202405] SET [BuyerSource]=@BuyerSource,[TotalAmount]=@TotalAmount26,[ProductCount]=@ProductCount26,[UpdatedAt]=@UpdatedAt26 WHERE [Id]=@kId26;UPDATE [sys_order_105_202405] SET [BuyerSource]=@BuyerSource,[TotalAmount]=@TotalAmount26,[ProductCount]=@ProductCount26,[UpdatedAt]=@UpdatedAt26 WHERE [Id]=@kId26;UPDATE [sys_order_104_202405] SET [BuyerSource]=@BuyerSource,[TotalAmount]=@TotalAmount27,[ProductCount]=@ProductCount27,[UpdatedAt]=@UpdatedAt27 WHERE [Id]=@kId27;UPDATE [sys_order_105_202405] SET [BuyerSource]=@BuyerSource,[TotalAmount]=@TotalAmount27,[ProductCount]=@ProductCount27,[UpdatedAt]=@UpdatedAt27 WHERE [Id]=@kId27;UPDATE [sys_order_104_202405] SET [BuyerSource]=@BuyerSource,[TotalAmount]=@TotalAmount28,[ProductCount]=@ProductCount28,[UpdatedAt]=@UpdatedAt28 WHERE [Id]=@kId28;UPDATE [sys_order_105_202405] SET [BuyerSource]=@BuyerSource,[TotalAmount]=@TotalAmount28,[ProductCount]=@ProductCount28,[UpdatedAt]=@UpdatedAt28 WHERE [Id]=@kId28;UPDATE [sys_order_104_202405] SET [BuyerSource]=@BuyerSource,[TotalAmount]=@TotalAmount29,[ProductCount]=@ProductCount29,[UpdatedAt]=@UpdatedAt29 WHERE [Id]=@kId29;UPDATE [sys_order_105_202405] SET [BuyerSource]=@BuyerSource,[TotalAmount]=@TotalAmount29,[ProductCount]=@ProductCount29,[UpdatedAt]=@UpdatedAt29 WHERE [Id]=@kId29;UPDATE [sys_order_104_202405] SET [BuyerSource]=@BuyerSource,[TotalAmount]=@TotalAmount30,[ProductCount]=@ProductCount30,[UpdatedAt]=@UpdatedAt30 WHERE [Id]=@kId30;UPDATE [sys_order_105_202405] SET [BuyerSource]=@BuyerSource,[TotalAmount]=@TotalAmount30,[ProductCount]=@ProductCount30,[UpdatedAt]=@UpdatedAt30 WHERE [Id]=@kId30;UPDATE [sys_order_104_202405] SET [BuyerSource]=@BuyerSource,[TotalAmount]=@TotalAmount31,[ProductCount]=@ProductCount31,[UpdatedAt]=@UpdatedAt31 WHERE [Id]=@kId31;UPDATE [sys_order_105_202405] SET [BuyerSource]=@BuyerSource,[TotalAmount]=@TotalAmount31,[ProductCount]=@ProductCount31,[UpdatedAt]=@UpdatedAt31 WHERE [Id]=@kId31;UPDATE [sys_order_104_202405] SET [BuyerSource]=@BuyerSource,[TotalAmount]=@TotalAmount32,[ProductCount]=@ProductCount32,[UpdatedAt]=@UpdatedAt32 WHERE [Id]=@kId32;UPDATE [sys_order_105_202405] SET [BuyerSource]=@BuyerSource,[TotalAmount]=@TotalAmount32,[ProductCount]=@ProductCount32,[UpdatedAt]=@UpdatedAt32 WHERE [Id]=@kId32;UPDATE [sys_order_104_202405] SET [BuyerSource]=@BuyerSource,[TotalAmount]=@TotalAmount33,[ProductCount]=@ProductCount33,[UpdatedAt]=@UpdatedAt33 WHERE [Id]=@kId33;UPDATE [sys_order_105_202405] SET [BuyerSource]=@BuyerSource,[TotalAmount]=@TotalAmount33,[ProductCount]=@ProductCount33,[UpdatedAt]=@UpdatedAt33 WHERE [Id]=@kId33;UPDATE [sys_order_104_202405] SET [BuyerSource]=@BuyerSource,[TotalAmount]=@TotalAmount34,[ProductCount]=@ProductCount34,[UpdatedAt]=@UpdatedAt34 WHERE [Id]=@kId34;UPDATE [sys_order_105_202405] SET [BuyerSource]=@BuyerSource,[TotalAmount]=@TotalAmount34,[ProductCount]=@ProductCount34,[UpdatedAt]=@UpdatedAt34 WHERE [Id]=@kId34;UPDATE [sys_order_104_202405] SET [BuyerSource]=@BuyerSource,[TotalAmount]=@TotalAmount35,[ProductCount]=@ProductCount35,[UpdatedAt]=@UpdatedAt35 WHERE [Id]=@kId35;UPDATE [sys_order_105_202405] SET [BuyerSource]=@BuyerSource,[TotalAmount]=@TotalAmount35,[ProductCount]=@ProductCount35,[UpdatedAt]=@UpdatedAt35 WHERE [Id]=@kId35;UPDATE [sys_order_104_202405] SET [BuyerSource]=@BuyerSource,[TotalAmount]=@TotalAmount36,[ProductCount]=@ProductCount36,[UpdatedAt]=@UpdatedAt36 WHERE [Id]=@kId36;UPDATE [sys_order_105_202405] SET [BuyerSource]=@BuyerSource,[TotalAmount]=@TotalAmount36,[ProductCount]=@ProductCount36,[UpdatedAt]=@UpdatedAt36 WHERE [Id]=@kId36;UPDATE [sys_order_104_202405] SET [BuyerSource]=@BuyerSource,[TotalAmount]=@TotalAmount37,[ProductCount]=@ProductCount37,[UpdatedAt]=@UpdatedAt37 WHERE [Id]=@kId37;UPDATE [sys_order_105_202405] SET [BuyerSource]=@BuyerSource,[TotalAmount]=@TotalAmount37,[ProductCount]=@ProductCount37,[UpdatedAt]=@UpdatedAt37 WHERE [Id]=@kId37;UPDATE [sys_order_104_202405] SET [BuyerSource]=@BuyerSource,[TotalAmount]=@TotalAmount38,[ProductCount]=@ProductCount38,[UpdatedAt]=@UpdatedAt38 WHERE [Id]=@kId38;UPDATE [sys_order_105_202405] SET [BuyerSource]=@BuyerSource,[TotalAmount]=@TotalAmount38,[ProductCount]=@ProductCount38,[UpdatedAt]=@UpdatedAt38 WHERE [Id]=@kId38;UPDATE [sys_order_104_202405] SET [BuyerSource]=@BuyerSource,[TotalAmount]=@TotalAmount39,[ProductCount]=@ProductCount39,[UpdatedAt]=@UpdatedAt39 WHERE [Id]=@kId39;UPDATE [sys_order_105_202405] SET [BuyerSource]=@BuyerSource,[TotalAmount]=@TotalAmount39,[ProductCount]=@ProductCount39,[UpdatedAt]=@UpdatedAt39 WHERE [Id]=@kId39");

        await repository.BeginTransactionAsync();
        var result = await repository.Update<Order>()
            .UseTable("sys_order_104_202405", "sys_order_105_202405")
            .SetBulk(orders, 10)
            .Set(f => f.BuyerSource, UserSourceType.Wechat)
            .IgnoreFields(f => new { f.OrderNo, f.BuyerId, f.SellerId })
            .ExecuteAsync();
        var updatedOrders = await repository.From<Order>()
            .UseTable("sys_order_104_202405", "sys_order_105_202405")
            .Where(f => orderIds.Contains(f.Id))
            .ToListAsync();
        await repository.CommitAsync();
        orders.Sort((x, y) => x.Id.CompareTo(y.Id));
        updatedOrders.Sort((x, y) => x.Id.CompareTo(y.Id));
        Assert.True(result > 0);
        for (int i = 0; i < orders.Count; i++)
        {
            Assert.True(orders[i].TotalAmount == updatedOrders[i].TotalAmount);
            Assert.True(orders[i].ProductCount == updatedOrders[i].ProductCount);

            Assert.True(orders[i].OrderNo == updatedOrders[i].OrderNo);
            Assert.True(orders[i].BuyerId == updatedOrders[i].BuyerId);
            Assert.True(orders[i].SellerId == updatedOrders[i].SellerId);
            Assert.True(updatedOrders[i].TenantId == "104" || updatedOrders[i].TenantId == "105");
        }
    }
    [Fact]
    public async void Update_BulkCopy_ManySharding()
    {
        await this.InitSharding();
        var createdAt = DateTime.Parse("2024-05-24");
        using var repository = dbFactory.Create();
        var orders = repository.From<Order>()
            .UseTable(f => (f.Contains("_104_") || f.Contains("_105_")) && int.Parse(f.Substring(f.Length - 6)) > 202001)
            .Select(f => new
            {
                f.Id,
                f.TenantId,
                TotalAmount = f.TotalAmount + 50,
                ProductCount = 3,
                UpdatedAt = DateTime.Now
            })
            .OrderByDescending(f => f.Id)
            .Take(20)
            .ToList();
        var orderIds = orders.Select(f => f.Id).ToList();

        var sql = repository.Update<Order>()
            .UseTable("sys_order_104_202405", "sys_order_105_202405")
            .SetBulkCopy(orders)
            .ToSql(out var dbParameters);
        //Assert.True(sql == "CREATE TEMPORARY TABLE `sys_order_0c0f27d1c0224df38030d8e78b03f8c4`(\r\n`Id` varchar(50) NOT NULL,\r\n`TotalAmount` double,\r\n`ProductCount` int,\r\n`UpdatedAt` datetime,\r\nPRIMARY KEY(`Id`)\r\n);\r\nUPDATE `sys_order_104_202405` a INNER JOIN `sys_order_0c0f27d1c0224df38030d8e78b03f8c4` b ON a.`Id`=b.`Id` SET a.`TotalAmount`=b.`TotalAmount`,a.`ProductCount`=b.`ProductCount`,a.`UpdatedAt`=b.`UpdatedAt`;UPDATE `sys_order_105_202405` a INNER JOIN `sys_order_0c0f27d1c0224df38030d8e78b03f8c4` b ON a.`Id`=b.`Id` SET a.`TotalAmount`=b.`TotalAmount`,a.`ProductCount`=b.`ProductCount`,a.`UpdatedAt`=b.`UpdatedAt`;DROP TABLE `sys_order_0c0f27d1c0224df38030d8e78b03f8c4`");

        await repository.BeginTransactionAsync();
        var result = await repository.Update<Order>()
            .UseTable("sys_order_104_202405", "sys_order_105_202405")
            .SetBulkCopy(orders)
            .ExecuteAsync();
        var updatedOrders = await repository.From<Order>()
            .UseTable("sys_order_104_202405", "sys_order_105_202405")
            .Where(f => orderIds.Contains(f.Id))
            .ToListAsync();
        await repository.CommitAsync();
        orders.Sort((x, y) => x.Id.CompareTo(y.Id));
        updatedOrders.Sort((x, y) => x.Id.CompareTo(y.Id));
        Assert.True(result == orders.Count);
        for (int i = 0; i < orders.Count; i++)
        {
            Assert.True(orders[i].TotalAmount == updatedOrders[i].TotalAmount);
            Assert.True(orders[i].ProductCount == updatedOrders[i].ProductCount);
            Assert.True(updatedOrders[i].TenantId == "104" || updatedOrders[i].TenantId == "105");
        }
    }
}
