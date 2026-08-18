using Microsoft.Extensions.DependencyInjection;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Trolley.MySqlConnector;
using Xunit;
using Xunit.Abstractions;

namespace Trolley.Test.MySqlConnector;

public class UnitTest6 : UnitTestBase
{
    private readonly ITestOutputHelper output;
    private int[] robinIndices = [0, 0, 0];
    public UnitTest6(ITestOutputHelper output)
    {
        this.output = output;
        var services = new ServiceCollection();
        services.AddSingleton(f =>
        {
            var connectionString = "Server=localhost;Database=fengling;Uid=root;password=123456;charset=utf8mb4;AllowLoadLocalInfile=true";
            var connectionString1 = "Server=localhost;Database=fengling1;Uid=root;password=123456;charset=utf8mb4;AllowLoadLocalInfile=true";
            var connectionString2 = "Server=localhost;Database=fengling2;Uid=root;password=123456;charset=utf8mb4;AllowLoadLocalInfile=true";
            var builder = new OrmDbFactoryBuilder()
                .Register(OrmProviderType.MySql, "fengling", f => f.Use([connectionString, connectionString1, connectionString2], values =>
                {
                    var tenantId = (int)values[0];
                    if (tenantId > 3) tenantId = 3;
                    var connectionStrings = new Dictionary<int, string[]>()
                    {
                        { 1, [connectionString, connectionString1, connectionString2] },
                        { 2, [connectionString, connectionString1, connectionString2] },
                        { 3, [connectionString, connectionString1, connectionString2] }
                    };
                    int index = Interlocked.Increment(ref robinIndices[0]) % 3;
                    if (Volatile.Read(ref robinIndices[0]) >= int.MaxValue - 1000)
                        Interlocked.Exchange(ref robinIndices[0], 0);
                    return connectionStrings[tenantId][Interlocked.Increment(ref robinIndices[0]) % 3];
                }).UseSlave(connectionString1, connectionString2), true)
                .Register(OrmProviderType.MySql, "fengling1", f => f.Use(connectionString1))
                .Register(OrmProviderType.MySql, "fengling2", f => f.Use(connectionString2))
                .UseMapping<ModelMappingConfiguration>(OrmProviderType.MySql)
                .UseTableSharding<TableShardingConfiguration>(OrmProviderType.MySql)
                .UseInterceptor(new MyDbInterceptor(output));
            return builder.Build();
        });
        services.AddTransient<IPassport>(f => new Passport { TenantId = "104", UserId = "1" });
        var serviceProvider = services.BuildServiceProvider();
        this.dbFactory = serviceProvider.GetService<IOrmDbFactory>();
    }
    private async Task InitSharding()
    {
        var repository = this.dbFactory.Create();
        await repository.Delete<User>()
            .UseTableBy("104")
            .UseTableBy("105")
            .WhereByIds(new[] { 101, 102, 103 })
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
#if NET6_0_OR_GREATER
                SomeTimes = TimeOnly.FromTimeSpan(TimeSpan.FromSeconds(4769)),
#else
                SomeTimes = TimeSpan.FromSeconds(4769),
#endif
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
                GuidField = Guid.NewGuid(),
#if NET6_0_OR_GREATER
                SomeTimes = TimeOnly.FromTimeSpan(TimeSpan.FromSeconds(5730)),
#else
                SomeTimes = TimeSpan.FromSeconds(5730),
#endif
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
                GuidField = Guid.NewGuid(),
#if NET6_0_OR_GREATER
                SomeTimes = TimeOnly.FromTimeSpan(TimeSpan.FromSeconds(5730)),
#else
                SomeTimes = TimeSpan.FromSeconds(5730),
#endif
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
    public async Task Create_WithBy_UseTable()
    {
        var repository = this.dbFactory.Create();
        await repository.Delete<User>()
            .UseTableBy("104")
            .WhereById(101)
            .ExecuteAsync();
        var count = repository.UseMaster()
            .From<User>()
            .UseTable("sys_user_104")
            .Where(f => f.Id == 101)
            .Count();
        Assert.Equal(0, count);

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
#if NET6_0_OR_GREATER
                SomeTimes = TimeOnly.FromTimeSpan(TimeSpan.FromSeconds(4769)),
#else
                SomeTimes = TimeSpan.FromSeconds(4769),
#endif
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
        Assert.Equal("104", result.TenantId);
    }
    [Fact]
    public async Task Create_WithBy_WithoutUseTable()
    {
        var repository = this.dbFactory.Create();
        await repository.Delete<User>()
            .UseTableBy("104")
            .WhereById(101)
            .ExecuteAsync();
        var count = repository.UseMaster()
            .From<User>()
            .UseTableBy("104")
            .Where(f => f.Id == 101)
            .Count();
        Assert.Equal(0, count);

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
#if NET6_0_OR_GREATER
                SomeTimes = TimeOnly.FromTimeSpan(TimeSpan.FromSeconds(4769)),
#else
                SomeTimes = TimeSpan.FromSeconds(4769),
#endif
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
        Assert.Equal("104", result.TenantId);
    }
    [Fact]
    public async Task Create_WithBulk_UseTable()
    {
        var repository = this.dbFactory.Create();
        await repository.Delete<User>()
            .UseTableBy("104")
            .WhereById(101)
            .ExecuteAsync();
        var count = repository.UseMaster()
            .From<User>()
            .UseTableBy("104")
            .Where(f => f.Id == 101)
            .Count();
        Assert.Equal(0, count);

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
#if NET6_0_OR_GREATER
                SomeTimes = TimeOnly.FromTimeSpan(TimeSpan.FromSeconds(4769)),
#else
                SomeTimes = TimeSpan.FromSeconds(4769),
#endif
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
        Assert.Equal("104", result.TenantId);
    }
    [Fact]
    public async Task Create_WithoutUseTable()
    {
        var repository = this.dbFactory.Create();
        await repository.Delete<User>()
            .UseTableBy("104")
            .UseTableBy("105")
            .WhereByIds(new object[] { 101, 102, 103 })
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
#if NET6_0_OR_GREATER
            SomeTimes = TimeOnly.FromTimeSpan(TimeSpan.FromSeconds(5730)),
#else
            SomeTimes = TimeSpan.FromSeconds(5730),
#endif
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
#if NET6_0_OR_GREATER
                SomeTimes = TimeOnly.FromTimeSpan(TimeSpan.FromSeconds(4769)),
#else
                SomeTimes = TimeSpan.FromSeconds(4769),
#endif
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
                GuidField = Guid.NewGuid(),
#if NET6_0_OR_GREATER
                SomeTimes = TimeOnly.FromTimeSpan(TimeSpan.FromSeconds(5730)),
#else
                SomeTimes = TimeSpan.FromSeconds(5730),
#endif
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
        Assert.Equal("104", result.TenantId);

        result = await repository.From<User>()
           .UseTableBy("105")
           .Where(f => f.Id == 102)
           .FirstAsync();
        Assert.NotNull(result);
        Assert.Equal("105", result.TenantId);

        result = await repository.From<User>()
           .UseTableBy("105")
           .Where(f => f.Id == 103)
           .FirstAsync();
        Assert.NotNull(result);
        Assert.Equal("105", result.TenantId);
    }
    [Fact]
    public async Task Create_WithBulk_WithoutUseTable()
    {
        var repository = this.dbFactory.Create();
        var userIds = new[] { 101, 102, 103 };
        await repository.Delete<User>()
            .UseTable("sys_user", "sys_user_104", "sys_user_105")
            .WhereByIds(userIds)
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
#if NET6_0_OR_GREATER
                SomeTimes = TimeOnly.FromTimeSpan(TimeSpan.FromSeconds(4769)),
#else
                SomeTimes = TimeSpan.FromSeconds(4769),
#endif
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
                GuidField = Guid.NewGuid(),
#if NET6_0_OR_GREATER
                SomeTimes = TimeOnly.FromTimeSpan(TimeSpan.FromSeconds(5730)),
#else
                SomeTimes = TimeSpan.FromSeconds(5730),
#endif
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
                GuidField = Guid.NewGuid(),
#if NET6_0_OR_GREATER
                SomeTimes = TimeOnly.FromTimeSpan(TimeSpan.FromSeconds(5730)),
#else
                SomeTimes = TimeSpan.FromSeconds(5730),
#endif
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
        Assert.Equal("104", result.TenantId);

        var result1 = await repository.From<User>()
            .UseTableBy("105")
            .Where(f => userIds.Contains(f.Id))
            .FirstAsync();
        Assert.NotNull(result1);
        Assert.Equal("105", result1.TenantId);
    }
    [Fact]
    public async Task Create_BulkCopy_UseTable()
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
        var repository = this.dbFactory.Create();
        var removeIds = orders.Select(f => f.Id).ToList();

        await repository.BeginTransactionAsync();
        repository.Delete<Order>()
           .UseTableBy("104", createdAt)
           .Where(f => removeIds.Contains(f.Id))
           .Execute();
        repository.Delete<OrderDetail>()
           .UseTableBy("104", createdAt)
           .Where(f => removeIds.Contains(f.OrderId))
           .Execute();
        var count1 = await repository.Create<Order>()
            .UseTableBy("104", createdAt)
            .WithBulkCopy(orders)
            .ExecuteAsync();
        var count2 = await repository.Create<OrderDetail>()
             .UseTableBy("104", createdAt)
             .WithBulkCopy(orderDetails)
             .ExecuteAsync();
        await repository.CommitAsync();
        Assert.Equal(1000, count1);
        Assert.Equal(2000, count2);

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
        Assert.Equal(1000, count1);
        Assert.Equal(2000, count2);
    }
    [Fact]
    public async Task Create_BulkCopy_WithoutUseTable()
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

        var repository = this.dbFactory.Create();
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
        Assert.Equal(2000, count1);
        Assert.Equal(4000, count2);
    }
    [Fact]
    public async Task Insert_Select_From_SubQuery_Returning()
    {
        var repository = this.dbFactory.Create();
        var sql = repository.From<OrderDetail>()
            .UseTableBy("104", DateTime.Parse("2024-05-01"))
            .Where(f => f.Id.Length < 1050)
            .GroupBy(f => f.OrderId)
            .Select((x, f) => new
            {
                Id = f.OrderId,
                f.TenantId,
                OrderNo = $"ON-{f.OrderId}",
                BuyerId = 1,
                SellerId = 1,
                BuyerSource = UserSourceType.Taobao.ToString(),
                ProductCount = 2,
                TotalAmount = x.Sum(f.Amount),
                IsEnabled = true,
                CreatedAt = DateTime.Now,
                CreatedBy = 1,
                UpdatedAt = DateTime.Now,
                UpdatedBy = 1
            })
            .ToCreate<Order>()
            .UseTableBy("104", DateTime.Parse("2024-05-01"))
            .Returning<OrderInfo>("BuyerId,TotalAmount")
            .ToSql(out var parameters);
        Assert.Equal("INSERT INTO `sys_order_104_202405` (`Id`,`TenantId`,`OrderNo`,`BuyerId`,`SellerId`,`BuyerSource`,`ProductCount`,`TotalAmount`,`IsEnabled`,`CreatedAt`,`CreatedBy`,`UpdatedAt`,`UpdatedBy`) SELECT b.`OrderId` AS `Id`,b.`TenantId`,CONCAT('ON-',b.`OrderId`) AS `OrderNo`,1 AS `BuyerId`,1 AS `SellerId`,'Taobao' AS `BuyerSource`,2 AS `ProductCount`,IFNULL(SUM(b.`Amount`),0) AS `TotalAmount`,1 AS `IsEnabled`,NOW() AS `CreatedAt`,1 AS `CreatedBy`,NOW() AS `UpdatedAt`,1 AS `UpdatedBy` FROM `sys_order_detail_104_202405` b WHERE CHAR_LENGTH(b.`Id`)<1050 GROUP BY b.`OrderId` RETURNING BuyerId,TotalAmount", sql);
        await repository.BeginTransactionAsync();
        await repository.Delete<Order>()
            .UseTableBy("104", DateTime.Parse("2024-05-01"))
            .Where(f => f.Id.Length < 10)
            .ExecuteAsync();
        var result = await repository.From<OrderDetail>()
            .UseTableBy("104", DateTime.Parse("2024-05-01"))
            .Where(f => f.Id.Length < 1050)
            .GroupBy(f => f.OrderId)
            .Select((x, f) => new
            {
                Id = f.OrderId,
                TenantId = "1",
                OrderNo = $"ON-{f.OrderId}",
                BuyerId = 1,
                SellerId = 1,
                BuyerSource = UserSourceType.Taobao.ToString(),
                ProductCount = 2,
                TotalAmount = x.Sum(f.Amount),
                IsEnabled = true,
                CreatedAt = DateTime.Now,
                CreatedBy = 1,
                UpdatedAt = DateTime.Now,
                UpdatedBy = 1
            })
            .ToCreate<Order>()
            .UseTableBy("104", DateTime.Parse("2024-05-01"))
            .Returning<OrderInfo>("BuyerId,TotalAmount")
            .ExecuteAsync();
        await repository.CommitAsync();
    }
    [Fact]
    public async Task Query_ManySharding_SingleTable()
    {
        await this.InitSharding();
        var productCount = 1;
        var repository = this.dbFactory.Create();
        var sql = repository.From<Order>()
            .UseTable("sys_order_104_202405", "sys_order_105_202405")
            .Where(f => f.ProductCount > productCount)
            .ToSql(out _);
        Assert.Equal("SELECT a.`Id`,a.`TenantId`,a.`OrderNo`,a.`ProductCount`,a.`TotalAmount`,a.`BuyerId`,a.`BuyerSource`,a.`SellerId`,a.`Products`,a.`Disputes`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy` FROM `sys_order_104_202405` a WHERE a.`ProductCount`>@p0 UNION ALL SELECT a.`Id`,a.`TenantId`,a.`OrderNo`,a.`ProductCount`,a.`TotalAmount`,a.`BuyerId`,a.`BuyerSource`,a.`SellerId`,a.`Products`,a.`Disputes`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy` FROM `sys_order_105_202405` a WHERE a.`ProductCount`>@p0", sql);

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
    public async Task Query_ManySharding_SingleTable_Include()
    {
        await this.InitSharding();
        var productCount = 1;
        var repository = this.dbFactory.Create();
        var sql = repository.From<Order>()
            .UseTable("sys_order_104_202405", "sys_order_105_202405")
            .Include(f => f.Details)
            .UseTableMap((origOrderName, origOrderDetailName, orderName) =>
                orderName.Replace(origOrderName, origOrderDetailName))
            .Where(f => f.ProductCount > productCount)
            .ToSql(out _);
        Assert.Equal("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE' AND TABLE_SCHEMA='fengling' AND (TABLE_NAME LIKE 'sys_order%' OR TABLE_NAME LIKE 'sys_order_detail%');SELECT a.`Id`,a.`TenantId`,a.`OrderNo`,a.`ProductCount`,a.`TotalAmount`,a.`BuyerId`,a.`BuyerSource`,a.`SellerId`,a.`Products`,a.`Disputes`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy` FROM `sys_order_104_202405` a WHERE a.`ProductCount`>@p0", sql);

        var result = repository.From<Order>()
            .UseTable("sys_order_104_202405", "sys_order_105_202405")
            .Include(f => f.Details)
            .UseTableMap((origOrderName, origOrderDetailName, orderName) =>
                orderName.Replace(origOrderName, origOrderDetailName))
            .Where(f => f.ProductCount > productCount)
            .ToList();
        if (result.Count > 0)
        {
            var tenantIds = result.Select(f => f.TenantId).Distinct().ToList();
            Assert.False(tenantIds.Exists(f => f != "104"));
            foreach (var order in result)
            {
                Assert.NotNull(order.Details);
                foreach (var orderDetail in order.Details)
                {
                    Assert.Equal("104", orderDetail.TenantId);
                }
            }
        }

        sql = repository.From<Order>()
            .UseTable("sys_order_104_202405", "sys_order_105_202405")
            .Include(f => f.Details)
            .UseTableMap((origOrderName, origOrderDetailName, orderName) =>
                orderName.Replace(origOrderName, origOrderDetailName))
            .Where(f => f.ProductCount > productCount)
            .ToSql(out _);
        Assert.Equal("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE' AND TABLE_SCHEMA='fengling' AND (TABLE_NAME LIKE 'sys_order_detail%');SELECT a.`Id`,a.`TenantId`,a.`OrderNo`,a.`ProductCount`,a.`TotalAmount`,a.`BuyerId`,a.`BuyerSource`,a.`SellerId`,a.`Products`,a.`Disputes`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy` FROM `sys_order_104_202405` a WHERE a.`ProductCount`>@p0 UNION ALL SELECT a.`Id`,a.`TenantId`,a.`OrderNo`,a.`ProductCount`,a.`TotalAmount`,a.`BuyerId`,a.`BuyerSource`,a.`SellerId`,a.`Products`,a.`Disputes`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy` FROM `sys_order_105_202405` a WHERE a.`ProductCount`>@p0", sql);

        result = repository.From<Order>()
            .UseTable("sys_order_104_202405", "sys_order_105_202405")
            .Include(f => f.Details)
            .UseTableMap((origOrderName, origOrderDetailName, orderName) =>
                orderName.Replace(origOrderName, origOrderDetailName))
            .Where(f => f.ProductCount > productCount)
            .ToList();
        if (result.Count > 0)
        {
            var tenantIds = result.Select(f => f.TenantId).ToList();
            Assert.True(tenantIds.Exists(f => "104,105".Contains(f)));
            foreach (var order in result)
            {
                Assert.NotNull(order.Details);
                foreach (var orderDetail in order.Details)
                {
                    Assert.Contains(orderDetail.TenantId, "104,105");
                }
            }
        }
        var beginTime = DateTime.Parse("2024-04-01");
        var endTime = DateTime.Parse("2024-06-05");
        result = repository.From<Order>()
            .UseTableByRange("104", beginTime, endTime)
            .Where(f => f.ProductCount > productCount)
            .ToList();
        Assert.True(result.Count > 0);
        result = repository.From<Order>()
            .UseTable("sys_order_104_202405", "sys_order_105_202405")
            .Where(f => f.ProductCount > productCount)
            .ToList();
        Assert.True(result.Count > 0);
    }
    [Fact]
    public async Task Query_SingleSharding_Value()
    {
        await this.InitSharding();
        var orderId = "ON_1015";
        var repository = this.dbFactory.Create();
        var sql = repository.From<Order>()
            .UseTableBy("104", DateTime.Parse("2024-05-01"))
            .InnerJoin<User>((x, y) => x.BuyerId == y.Id)
            .UseTableBy("104")
            .Where((x, y) => x.Id == orderId)
            .Select((x, y) => new { x.Id, x.OrderNo, x.TenantId, x.BuyerId, BuyerName = y.Name })
            .ToSql(out _);
        Assert.Equal("SELECT a.`Id`,a.`OrderNo`,a.`TenantId`,a.`BuyerId`,b.`Name` AS `BuyerName` FROM `sys_order_104_202405` a INNER JOIN `sys_user_104` b ON a.`BuyerId`=b.`Id` WHERE a.`Id`=@p0", sql);

        var result = await repository.From<Order>()
            .UseTableBy("104", DateTime.Parse("2024-05-01"))
            .InnerJoin<User>((x, y) => x.BuyerId == y.Id)
            .UseTableBy("104")
            .Where((x, y) => x.Id == orderId)
            .Select((x, y) => new { x.Id, x.OrderNo, x.TenantId, x.BuyerId, BuyerName = y.Name })
            .FirstAsync();
        if (result != null)
        {
            Assert.Equal("104", result.TenantId);
        }
    }
    [Fact]
    public async Task Query_ManySharding_SingleTable_SubQuery()
    {
        await this.InitSharding();
        var repository = this.dbFactory.Create();
        var sql = repository
            .FromQuery(f => f.From<OrderDetail>()
                .UseTable("sys_order_detail_104_202405", "sys_order_detail_105_202405")
                .InnerJoin<Order>((x, y) => x.OrderId == y.Id)
                .UseTableMap((orderOrigName, userOrigName, orderTableName) => orderTableName.Replace(orderOrigName, userOrigName))
                .GroupBy((a, b) => new { OrderId = b.Id, b.BuyerId })
                .Select((x, a, b) => new { Group = x.Grouping, ProductCount = x.CountDistinct(a.ProductId) }))
            .InnerJoin<User>((x, y) => x.Group.BuyerId == y.Id)
            .UseTableMap((orderOrigName, userOrigName, orderTableName) =>
                orderTableName.Replace(orderOrigName, userOrigName)[..^7])
            .Where((a, b) => a.ProductCount > 1)
            .Select((x, y) => new
            {
                x.Group,
                y.TenantId,
                Buyer = y,
                x.ProductCount
            })
            .ToSql(out _);
        Assert.Equal("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE' AND TABLE_SCHEMA='fengling' AND (TABLE_NAME LIKE 'sys_order%' OR TABLE_NAME LIKE 'sys_user%');SELECT a.`OrderId`,a.`BuyerId`,b.`TenantId`,b.`Id`,b.`TenantId`,b.`Name`,b.`Gender`,b.`Age`,b.`CompanyId`,b.`GuidField`,b.`SomeTimes`,b.`SourceType`,b.`IsEnabled`,b.`CreatedAt`,b.`CreatedBy`,b.`UpdatedAt`,b.`UpdatedBy`,a.`ProductCount` FROM (SELECT b.`Id` AS `OrderId`,b.`BuyerId`,COUNT(DISTINCT a.`ProductId`) AS `ProductCount` FROM `sys_order_detail_104_202405` a INNER JOIN `sys_order_104_202405` b ON a.`OrderId`=b.`Id` GROUP BY b.`Id`,b.`BuyerId`) a INNER JOIN `sys_user_104` b ON a.`BuyerId`=b.`Id` WHERE a.`ProductCount`>1 UNION ALL SELECT a.`OrderId`,a.`BuyerId`,b.`TenantId`,b.`Id`,b.`TenantId`,b.`Name`,b.`Gender`,b.`Age`,b.`CompanyId`,b.`GuidField`,b.`SomeTimes`,b.`SourceType`,b.`IsEnabled`,b.`CreatedAt`,b.`CreatedBy`,b.`UpdatedAt`,b.`UpdatedBy`,a.`ProductCount` FROM (SELECT b.`Id` AS `OrderId`,b.`BuyerId`,COUNT(DISTINCT a.`ProductId`) AS `ProductCount` FROM `sys_order_detail_105_202405` a INNER JOIN `sys_order_105_202405` b ON a.`OrderId`=b.`Id` GROUP BY b.`Id`,b.`BuyerId`) a INNER JOIN `sys_user_105` b ON a.`BuyerId`=b.`Id` WHERE a.`ProductCount`>1", sql);

        var result = await repository
            .FromQuery(f => f.From<OrderDetail>()
                .UseTable("sys_order_detail_104_202405", "sys_order_detail_105_202405")
                .InnerJoin<Order>((x, y) => x.OrderId == y.Id)
                .UseTableMap((orderOrigName, userOrigName, orderTableName) => orderTableName.Replace(orderOrigName, userOrigName))
                .GroupBy((a, b) => new { OrderId = b.Id, b.BuyerId })
                .Select((x, a, b) => new { Group = x.Grouping, ProductCount = x.CountDistinct(a.ProductId) }))
            .InnerJoin<User>((x, y) => x.Group.BuyerId == y.Id)
            .UseTableMap((orderOrigName, userOrigName, orderTableName) =>
            {
                var tableName = orderTableName.Replace(orderOrigName, userOrigName);
                return tableName[..^7];
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
            Assert.False(tenantIds.Exists(f => f != "104" && f != "105"));
        }
    }
    [Fact]
    public async Task Query_ManySharding_MultiTable1()
    {
        await this.InitSharding();
        var productCount = 1;
        var repository = this.dbFactory.Create();
        var sql = repository.From<Order>()
            .UseTable("sys_order_104_202405", "sys_order_105_202405")
            .InnerJoin<User>((x, y) => x.BuyerId == y.Id)
            .UseTableMap((orderOrigName, userOrigName, orderTableName) =>
            {
                var tableName = orderTableName.Replace(orderOrigName, userOrigName);
                return tableName[..^7];
            })
            .Where((a, b) => a.ProductCount > productCount)
            .Select((x, y) => new
            {
                Order = x,
                Buyer = y
            })
            .ToSql(out _);
        Assert.Equal("SELECT a.`Id`,a.`TenantId`,a.`OrderNo`,a.`ProductCount`,a.`TotalAmount`,a.`BuyerId`,a.`BuyerSource`,a.`SellerId`,a.`Products`,a.`Disputes`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy`,b.`Id`,b.`TenantId`,b.`Name`,b.`Gender`,b.`Age`,b.`CompanyId`,b.`GuidField`,b.`SomeTimes`,b.`SourceType`,b.`IsEnabled`,b.`CreatedAt`,b.`CreatedBy`,b.`UpdatedAt`,b.`UpdatedBy` FROM `sys_order_104_202405` a INNER JOIN `sys_user_104` b ON a.`BuyerId`=b.`Id` WHERE a.`ProductCount`>@p0 UNION ALL SELECT a.`Id`,a.`TenantId`,a.`OrderNo`,a.`ProductCount`,a.`TotalAmount`,a.`BuyerId`,a.`BuyerSource`,a.`SellerId`,a.`Products`,a.`Disputes`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy`,b.`Id`,b.`TenantId`,b.`Name`,b.`Gender`,b.`Age`,b.`CompanyId`,b.`GuidField`,b.`SomeTimes`,b.`SourceType`,b.`IsEnabled`,b.`CreatedAt`,b.`CreatedBy`,b.`UpdatedAt`,b.`UpdatedBy` FROM `sys_order_105_202405` a INNER JOIN `sys_user_105` b ON a.`BuyerId`=b.`Id` WHERE a.`ProductCount`>@p0", sql);

        var result = repository.From<Order>()
            .UseTable("sys_order_104_202405", "sys_order_105_202405")
            .InnerJoin<User>((x, y) => x.BuyerId == y.Id)
            .UseTableMap((orderOrigName, userOrigName, orderTableName) =>
            {
                var tableName = orderTableName.Replace(orderOrigName, userOrigName);
                return tableName[..^7];
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
    public async Task Query_ManySharding_MultiTable2()
    {
        await this.InitSharding();
        var productCount = 1;
        var repository = this.dbFactory.Create();
        var sql = repository.From<Order>()
            .UseTable("sys_order_104_202405", "sys_order_105_202405")
            .InnerJoin<User>((x, y) => x.BuyerId == y.Id)
            .UseTableMap((orderOrigName, userOrigName, orderTableName) =>
            {
                var tableName = orderTableName.Replace(orderOrigName, userOrigName);
                return tableName[..^7];
            })
            .Where((a, b) => a.ProductCount > productCount)
            .Select((x, y) => new
            {
                Order = x,
                Buyer = y
            })
            .ToSql(out _);
        Assert.Equal("SELECT a.`Id`,a.`TenantId`,a.`OrderNo`,a.`ProductCount`,a.`TotalAmount`,a.`BuyerId`,a.`BuyerSource`,a.`SellerId`,a.`Products`,a.`Disputes`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy`,b.`Id`,b.`TenantId`,b.`Name`,b.`Gender`,b.`Age`,b.`CompanyId`,b.`GuidField`,b.`SomeTimes`,b.`SourceType`,b.`IsEnabled`,b.`CreatedAt`,b.`CreatedBy`,b.`UpdatedAt`,b.`UpdatedBy` FROM `sys_order_104_202405` a INNER JOIN `sys_user_104` b ON a.`BuyerId`=b.`Id` WHERE a.`ProductCount`>@p0 UNION ALL SELECT a.`Id`,a.`TenantId`,a.`OrderNo`,a.`ProductCount`,a.`TotalAmount`,a.`BuyerId`,a.`BuyerSource`,a.`SellerId`,a.`Products`,a.`Disputes`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy`,b.`Id`,b.`TenantId`,b.`Name`,b.`Gender`,b.`Age`,b.`CompanyId`,b.`GuidField`,b.`SomeTimes`,b.`SourceType`,b.`IsEnabled`,b.`CreatedAt`,b.`CreatedBy`,b.`UpdatedAt`,b.`UpdatedBy` FROM `sys_order_104_202406` a INNER JOIN `sys_user_104` b ON a.`BuyerId`=b.`Id` WHERE a.`ProductCount`>@p0 UNION ALL SELECT a.`Id`,a.`TenantId`,a.`OrderNo`,a.`ProductCount`,a.`TotalAmount`,a.`BuyerId`,a.`BuyerSource`,a.`SellerId`,a.`Products`,a.`Disputes`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy`,b.`Id`,b.`TenantId`,b.`Name`,b.`Gender`,b.`Age`,b.`CompanyId`,b.`GuidField`,b.`SomeTimes`,b.`SourceType`,b.`IsEnabled`,b.`CreatedAt`,b.`CreatedBy`,b.`UpdatedAt`,b.`UpdatedBy` FROM `sys_order_105_202405` a INNER JOIN `sys_user_105` b ON a.`BuyerId`=b.`Id` WHERE a.`ProductCount`>@p0", sql);

        var result = repository.From<Order>()
            .UseTable("sys_order_104_202405", "sys_order_105_202405")
            .InnerJoin<User>((x, y) => x.BuyerId == y.Id)
            .UseTableMap((orderOrigName, userOrigName, orderTableName) =>
            {
                var tableName = orderTableName.Replace(orderOrigName, userOrigName);
                return tableName[..^7];
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
    public async Task Query_ManySharding_MultiTable3()
    {
        await this.InitSharding();
        var productCount = 1;
        var repository = this.dbFactory.Create();
        var sql = repository.From<Order>()
            .UseTable("sys_order_104_202405", "sys_order_105_202405")
            .InnerJoin<OrderDetail>((x, y) => x.Id == y.OrderId)
            .UseTableMap((orderOrigName, orderDetailOrigName, orderTableName) => orderTableName.Replace(orderOrigName, orderDetailOrigName))
            .Where((a, b) => a.ProductCount > productCount)
            .Select((x, y) => new
            {
                Order = x,
                Detail = y
            })
            .ToSql(out _);
        Assert.Equal("SELECT a.`Id`,a.`TenantId`,a.`OrderNo`,a.`ProductCount`,a.`TotalAmount`,a.`BuyerId`,a.`BuyerSource`,a.`SellerId`,a.`Products`,a.`Disputes`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy`,b.`Id`,b.`TenantId`,b.`OrderId`,b.`ProductId`,b.`Price`,b.`Quantity`,b.`Amount`,b.`IsEnabled`,b.`CreatedAt`,b.`CreatedBy`,b.`UpdatedAt`,b.`UpdatedBy` FROM `sys_order_104_202405` a INNER JOIN `sys_order_detail_104_202405` b ON a.`Id`=b.`OrderId` WHERE a.`ProductCount`>@p0 UNION ALL SELECT a.`Id`,a.`TenantId`,a.`OrderNo`,a.`ProductCount`,a.`TotalAmount`,a.`BuyerId`,a.`BuyerSource`,a.`SellerId`,a.`Products`,a.`Disputes`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy`,b.`Id`,b.`TenantId`,b.`OrderId`,b.`ProductId`,b.`Price`,b.`Quantity`,b.`Amount`,b.`IsEnabled`,b.`CreatedAt`,b.`CreatedBy`,b.`UpdatedAt`,b.`UpdatedBy` FROM `sys_order_105_202405` a INNER JOIN `sys_order_detail_105_202405` b ON a.`Id`=b.`OrderId` WHERE a.`ProductCount`>@p0", sql);

        var result = repository.From<Order>()
            .UseTable("sys_order_104_202405", "sys_order_105_202405")
            .InnerJoin<OrderDetail>((x, y) => x.Id == y.OrderId)
            .UseTableMap((orderOrigName, orderDetailOrigName, orderTableName)
                => orderTableName.Replace(orderOrigName, orderDetailOrigName))
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
    public async Task Query_SingleSharding_Exists1()
    {
        await this.InitSharding();
        var repository = this.dbFactory.Create();
        var sql = repository.From<Order>()
            .UseTableBy("104", DateTime.Parse("2024-05-24"))
            .Where(f => repository.From<User>('b')
                .UseTableBy("104")
                .Where(t => t.Id == f.BuyerId && t.Age < 25)
                .Exists())
            .ToSql(out _);
        Assert.Equal("SELECT a.`Id`,a.`TenantId`,a.`OrderNo`,a.`ProductCount`,a.`TotalAmount`,a.`BuyerId`,a.`BuyerSource`,a.`SellerId`,a.`Products`,a.`Disputes`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy` FROM `sys_order_104_202405` a WHERE EXISTS(SELECT * FROM `sys_user_104` b WHERE b.`Id`=a.`BuyerId` AND b.`Age`<25)", sql);

        var result = repository.From<Order>()
            .UseTableBy("104", DateTime.Parse("2024-05-24"))
            .Where(f => repository.From<User>('b')
                .UseTableBy("104")
                .Where(t => t.Id == f.BuyerId && t.Age < 25)
                .Exists())
            .ToList();
        if (result.Count > 0)
        {
            var tenantIds = result.Select(f => f.TenantId).Distinct().ToList();
            Assert.Contains("104", tenantIds);
        }
    }
    [Fact]
    public async Task Query_SingleSharding_Exists2()
    {
        await this.InitSharding();
        var repository = this.dbFactory.Create();
        var sql = repository.From<Order>()
            .UseTableBy("104", DateTime.Parse("2024-05-24"))
            .Where(f => repository.From<User>('b')
                .UseTableBy("104")
                .InnerJoin<OrderDetail>((x, y) => f.Id == y.OrderId)
                .UseTableBy("104", DateTime.Parse("2024-05-24"))
                .Where((x, y) => x.Id == f.BuyerId && x.Age <= 25 && y.Price > 100)
                .Exists())
            .ToSql(out _);
        Assert.Equal("SELECT a.`Id`,a.`TenantId`,a.`OrderNo`,a.`ProductCount`,a.`TotalAmount`,a.`BuyerId`,a.`BuyerSource`,a.`SellerId`,a.`Products`,a.`Disputes`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy` FROM `sys_order_104_202405` a WHERE EXISTS(SELECT * FROM `sys_user_104` b INNER JOIN `sys_order_detail_104_202405` c ON a.`Id`=c.`OrderId` WHERE b.`Id`=a.`BuyerId` AND b.`Age`<=25 AND c.`Price`>100)", sql);

        sql = repository.From<Order>()
            .UseTableBy("104", DateTime.Parse("2024-05-24"))
            .Where(f => repository.From<User>('b')
                .UseTableBy("104")
                .InnerJoin<OrderDetail>((x, y) => f.Id == y.OrderId)
                .UseTableBy("104", DateTime.Parse("2024-05-24"))
                .Where((x, y) => x.Id == f.BuyerId && x.Age <= 25 && y.Price > 100)
                .Exists())
            .ToSql(out _);
        Assert.Equal("SELECT a.`Id`,a.`TenantId`,a.`OrderNo`,a.`ProductCount`,a.`TotalAmount`,a.`BuyerId`,a.`BuyerSource`,a.`SellerId`,a.`Products`,a.`Disputes`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy` FROM `sys_order_104_202405` a WHERE EXISTS(SELECT * FROM `sys_user_104` b INNER JOIN `sys_order_detail_104_202405` c ON a.`Id`=c.`OrderId` WHERE b.`Id`=a.`BuyerId` AND b.`Age`<=25 AND c.`Price`>100)", sql);

        var result = repository.From<Order>()
            .UseTableBy("104", DateTime.Parse("2024-05-24"))
            .Where(f => repository.From<User>('b')
                .UseTableBy("104")
                .InnerJoin<OrderDetail>((x, y) => f.Id == y.OrderId)
                .UseTableBy("104", DateTime.Parse("2024-05-24"))
                .Where((x, y) => x.Id == f.BuyerId && x.Age <= 25 && y.Price > 100)
                .Exists())
            .ToList();
        if (result.Count > 0)
        {
            var tenantIds = result.Select(f => f.TenantId).Distinct().ToList();
            Assert.Contains("104", tenantIds);
        }
    }
    [Fact]
    public async Task Update_SingleSharding()
    {
        await this.InitSharding();
        var repository = this.dbFactory.Create();
        var orderIds = new string[] { "ON_1001", "ON_1002", "ON_1003", "ON_1004" };
        var sql = repository.Update<Order>()
            .UseTableBy("104", DateTime.Parse("2024-05-24"))
            .Set(new { TotalAmount = 400 })
            .Where(f => orderIds.Contains(f.Id))
            .ToSql(out var dbParameters);
        Assert.Equal("UPDATE `sys_order_104_202405` SET `TotalAmount`=@TotalAmount WHERE `Id` IN (@p1,@p2,@p3,@p4)", sql);
        Assert.Equal(400, (double)dbParameters[0].Value);
        Assert.Equal(MySqlDbType.Double, ((MySqlParameter)dbParameters[0]).MySqlDbType);
        Assert.Equal(orderIds[0], (string)dbParameters[1].Value);
        Assert.Equal(orderIds[1], (string)dbParameters[2].Value);
        Assert.Equal(orderIds[2], (string)dbParameters[3].Value);
        Assert.Equal(orderIds[3], (string)dbParameters[4].Value);

        var result = await repository.Update<Order>()
            .UseTableBy("104", DateTime.Parse("2024-05-24"))
            .Set(new { TotalAmount = 400 })
            .Where(f => orderIds.Contains(f.Id))
            .ExecuteAsync();
        Assert.True(result > 0);
    }
    [Fact]
    public async Task Update_ManySharding1()
    {
        await this.InitSharding();
        var repository = this.dbFactory.Create();
        var orderIds = new string[] { "ON_1001", "ON_1002", "ON_2003", "ON_2004" };
        var sql = repository.Update<Order>()
            .UseTable("sys_order_104_202405", "sys_order_105_202405")
            .Set(new { TotalAmount = 400 })
            .Where(f => orderIds.Contains(f.Id))
            .ToSql(out var dbParameters);
        Assert.Equal("UPDATE `sys_order_104_202405` SET `TotalAmount`=@TotalAmount WHERE `Id` IN (@p1,@p2,@p3,@p4);UPDATE `sys_order_105_202405` SET `TotalAmount`=@TotalAmount WHERE `Id` IN (@p1,@p2,@p3,@p4)", sql);
        Assert.Equal(400, (double)dbParameters[0].Value);
        Assert.Equal(MySqlDbType.Double, ((MySqlParameter)dbParameters[0]).MySqlDbType);
        Assert.Equal(orderIds[0], (string)dbParameters[1].Value);
        Assert.Equal(orderIds[1], (string)dbParameters[2].Value);
        Assert.Equal(orderIds[2], (string)dbParameters[3].Value);
        Assert.Equal(orderIds[3], (string)dbParameters[4].Value);

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
            Assert.Equal(400, order.TotalAmount);
            Assert.True(order.TenantId == "104" || order.TenantId == "105");
            Assert.Contains(order.Id, orderIds);
        }
    }
    [Fact]
    public async Task Update_ManySharding2()
    {
        await this.InitSharding();
        var repository = this.dbFactory.Create();
        var orderIds = new string[] { "ON_1001", "ON_1002", "ON_2003", "ON_2004" };
        var sql = repository.Update<Order>()
            .UseTable("sys_order_104_202405", "sys_order_105_202405")
            .Set(new { TotalAmount = 400 })
            .Where(f => orderIds.Contains(f.Id))
            .ToSql(out var dbParameters);
        Assert.True(sql == "UPDATE `sys_order_105_202405` SET `TotalAmount`=@TotalAmount WHERE `Id` IN (@p1,@p2,@p3,@p4);UPDATE `sys_order_104_202405` SET `TotalAmount`=@TotalAmount WHERE `Id` IN (@p1,@p2,@p3,@p4)");
        Assert.Equal(400, (double)dbParameters[0].Value);
        Assert.Equal(MySqlDbType.Double, ((MySqlParameter)dbParameters[0]).MySqlDbType);
        Assert.Equal(orderIds[0], (string)dbParameters[1].Value);
        Assert.Equal(orderIds[1], (string)dbParameters[2].Value);
        Assert.Equal(orderIds[2], (string)dbParameters[3].Value);
        Assert.Equal(orderIds[3], (string)dbParameters[4].Value);

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
            Assert.Equal(400, order.TotalAmount);
            Assert.True(order.TenantId == "104" || order.TenantId == "105");
            Assert.Contains(order.Id, orderIds);
        }
    }
    [Fact]
    public async Task Update_SetBulk_ManySharding()
    {
        await this.InitSharding();
        var createdAt = DateTime.Parse("2024-05-24");
        var repository = this.dbFactory.Create();
        var orders = repository.From<Order>()
            .UseTable("sys_order_104_202405", "sys_order_105_202405")
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
        Assert.Equal("UPDATE `sys_order_104_202405` SET `BuyerSource`=@BuyerSource,`ProductCount`=@ProductCount0,`TotalAmount`=@TotalAmount0,`UpdatedAt`=@UpdatedAt0 WHERE `Id`=@kId0;UPDATE `sys_order_105_202405` SET `BuyerSource`=@BuyerSource,`ProductCount`=@ProductCount0,`TotalAmount`=@TotalAmount0,`UpdatedAt`=@UpdatedAt0 WHERE `Id`=@kId0;UPDATE `sys_order_104_202405` SET `BuyerSource`=@BuyerSource,`ProductCount`=@ProductCount1,`TotalAmount`=@TotalAmount1,`UpdatedAt`=@UpdatedAt1 WHERE `Id`=@kId1;UPDATE `sys_order_105_202405` SET `BuyerSource`=@BuyerSource,`ProductCount`=@ProductCount1,`TotalAmount`=@TotalAmount1,`UpdatedAt`=@UpdatedAt1 WHERE `Id`=@kId1;UPDATE `sys_order_104_202405` SET `BuyerSource`=@BuyerSource,`ProductCount`=@ProductCount2,`TotalAmount`=@TotalAmount2,`UpdatedAt`=@UpdatedAt2 WHERE `Id`=@kId2;UPDATE `sys_order_105_202405` SET `BuyerSource`=@BuyerSource,`ProductCount`=@ProductCount2,`TotalAmount`=@TotalAmount2,`UpdatedAt`=@UpdatedAt2 WHERE `Id`=@kId2;UPDATE `sys_order_104_202405` SET `BuyerSource`=@BuyerSource,`ProductCount`=@ProductCount3,`TotalAmount`=@TotalAmount3,`UpdatedAt`=@UpdatedAt3 WHERE `Id`=@kId3;UPDATE `sys_order_105_202405` SET `BuyerSource`=@BuyerSource,`ProductCount`=@ProductCount3,`TotalAmount`=@TotalAmount3,`UpdatedAt`=@UpdatedAt3 WHERE `Id`=@kId3;UPDATE `sys_order_104_202405` SET `BuyerSource`=@BuyerSource,`ProductCount`=@ProductCount4,`TotalAmount`=@TotalAmount4,`UpdatedAt`=@UpdatedAt4 WHERE `Id`=@kId4;UPDATE `sys_order_105_202405` SET `BuyerSource`=@BuyerSource,`ProductCount`=@ProductCount4,`TotalAmount`=@TotalAmount4,`UpdatedAt`=@UpdatedAt4 WHERE `Id`=@kId4;UPDATE `sys_order_104_202405` SET `BuyerSource`=@BuyerSource,`ProductCount`=@ProductCount5,`TotalAmount`=@TotalAmount5,`UpdatedAt`=@UpdatedAt5 WHERE `Id`=@kId5;UPDATE `sys_order_105_202405` SET `BuyerSource`=@BuyerSource,`ProductCount`=@ProductCount5,`TotalAmount`=@TotalAmount5,`UpdatedAt`=@UpdatedAt5 WHERE `Id`=@kId5;UPDATE `sys_order_104_202405` SET `BuyerSource`=@BuyerSource,`ProductCount`=@ProductCount6,`TotalAmount`=@TotalAmount6,`UpdatedAt`=@UpdatedAt6 WHERE `Id`=@kId6;UPDATE `sys_order_105_202405` SET `BuyerSource`=@BuyerSource,`ProductCount`=@ProductCount6,`TotalAmount`=@TotalAmount6,`UpdatedAt`=@UpdatedAt6 WHERE `Id`=@kId6;UPDATE `sys_order_104_202405` SET `BuyerSource`=@BuyerSource,`ProductCount`=@ProductCount7,`TotalAmount`=@TotalAmount7,`UpdatedAt`=@UpdatedAt7 WHERE `Id`=@kId7;UPDATE `sys_order_105_202405` SET `BuyerSource`=@BuyerSource,`ProductCount`=@ProductCount7,`TotalAmount`=@TotalAmount7,`UpdatedAt`=@UpdatedAt7 WHERE `Id`=@kId7;UPDATE `sys_order_104_202405` SET `BuyerSource`=@BuyerSource,`ProductCount`=@ProductCount8,`TotalAmount`=@TotalAmount8,`UpdatedAt`=@UpdatedAt8 WHERE `Id`=@kId8;UPDATE `sys_order_105_202405` SET `BuyerSource`=@BuyerSource,`ProductCount`=@ProductCount8,`TotalAmount`=@TotalAmount8,`UpdatedAt`=@UpdatedAt8 WHERE `Id`=@kId8;UPDATE `sys_order_104_202405` SET `BuyerSource`=@BuyerSource,`ProductCount`=@ProductCount9,`TotalAmount`=@TotalAmount9,`UpdatedAt`=@UpdatedAt9 WHERE `Id`=@kId9;UPDATE `sys_order_105_202405` SET `BuyerSource`=@BuyerSource,`ProductCount`=@ProductCount9,`TotalAmount`=@TotalAmount9,`UpdatedAt`=@UpdatedAt9 WHERE `Id`=@kId9;UPDATE `sys_order_104_202405` SET `BuyerSource`=@BuyerSource,`ProductCount`=@ProductCount10,`TotalAmount`=@TotalAmount10,`UpdatedAt`=@UpdatedAt10 WHERE `Id`=@kId10;UPDATE `sys_order_105_202405` SET `BuyerSource`=@BuyerSource,`ProductCount`=@ProductCount10,`TotalAmount`=@TotalAmount10,`UpdatedAt`=@UpdatedAt10 WHERE `Id`=@kId10;UPDATE `sys_order_104_202405` SET `BuyerSource`=@BuyerSource,`ProductCount`=@ProductCount11,`TotalAmount`=@TotalAmount11,`UpdatedAt`=@UpdatedAt11 WHERE `Id`=@kId11;UPDATE `sys_order_105_202405` SET `BuyerSource`=@BuyerSource,`ProductCount`=@ProductCount11,`TotalAmount`=@TotalAmount11,`UpdatedAt`=@UpdatedAt11 WHERE `Id`=@kId11;UPDATE `sys_order_104_202405` SET `BuyerSource`=@BuyerSource,`ProductCount`=@ProductCount12,`TotalAmount`=@TotalAmount12,`UpdatedAt`=@UpdatedAt12 WHERE `Id`=@kId12;UPDATE `sys_order_105_202405` SET `BuyerSource`=@BuyerSource,`ProductCount`=@ProductCount12,`TotalAmount`=@TotalAmount12,`UpdatedAt`=@UpdatedAt12 WHERE `Id`=@kId12;UPDATE `sys_order_104_202405` SET `BuyerSource`=@BuyerSource,`ProductCount`=@ProductCount13,`TotalAmount`=@TotalAmount13,`UpdatedAt`=@UpdatedAt13 WHERE `Id`=@kId13;UPDATE `sys_order_105_202405` SET `BuyerSource`=@BuyerSource,`ProductCount`=@ProductCount13,`TotalAmount`=@TotalAmount13,`UpdatedAt`=@UpdatedAt13 WHERE `Id`=@kId13;UPDATE `sys_order_104_202405` SET `BuyerSource`=@BuyerSource,`ProductCount`=@ProductCount14,`TotalAmount`=@TotalAmount14,`UpdatedAt`=@UpdatedAt14 WHERE `Id`=@kId14;UPDATE `sys_order_105_202405` SET `BuyerSource`=@BuyerSource,`ProductCount`=@ProductCount14,`TotalAmount`=@TotalAmount14,`UpdatedAt`=@UpdatedAt14 WHERE `Id`=@kId14;UPDATE `sys_order_104_202405` SET `BuyerSource`=@BuyerSource,`ProductCount`=@ProductCount15,`TotalAmount`=@TotalAmount15,`UpdatedAt`=@UpdatedAt15 WHERE `Id`=@kId15;UPDATE `sys_order_105_202405` SET `BuyerSource`=@BuyerSource,`ProductCount`=@ProductCount15,`TotalAmount`=@TotalAmount15,`UpdatedAt`=@UpdatedAt15 WHERE `Id`=@kId15;UPDATE `sys_order_104_202405` SET `BuyerSource`=@BuyerSource,`ProductCount`=@ProductCount16,`TotalAmount`=@TotalAmount16,`UpdatedAt`=@UpdatedAt16 WHERE `Id`=@kId16;UPDATE `sys_order_105_202405` SET `BuyerSource`=@BuyerSource,`ProductCount`=@ProductCount16,`TotalAmount`=@TotalAmount16,`UpdatedAt`=@UpdatedAt16 WHERE `Id`=@kId16;UPDATE `sys_order_104_202405` SET `BuyerSource`=@BuyerSource,`ProductCount`=@ProductCount17,`TotalAmount`=@TotalAmount17,`UpdatedAt`=@UpdatedAt17 WHERE `Id`=@kId17;UPDATE `sys_order_105_202405` SET `BuyerSource`=@BuyerSource,`ProductCount`=@ProductCount17,`TotalAmount`=@TotalAmount17,`UpdatedAt`=@UpdatedAt17 WHERE `Id`=@kId17;UPDATE `sys_order_104_202405` SET `BuyerSource`=@BuyerSource,`ProductCount`=@ProductCount18,`TotalAmount`=@TotalAmount18,`UpdatedAt`=@UpdatedAt18 WHERE `Id`=@kId18;UPDATE `sys_order_105_202405` SET `BuyerSource`=@BuyerSource,`ProductCount`=@ProductCount18,`TotalAmount`=@TotalAmount18,`UpdatedAt`=@UpdatedAt18 WHERE `Id`=@kId18;UPDATE `sys_order_104_202405` SET `BuyerSource`=@BuyerSource,`ProductCount`=@ProductCount19,`TotalAmount`=@TotalAmount19,`UpdatedAt`=@UpdatedAt19 WHERE `Id`=@kId19;UPDATE `sys_order_105_202405` SET `BuyerSource`=@BuyerSource,`ProductCount`=@ProductCount19,`TotalAmount`=@TotalAmount19,`UpdatedAt`=@UpdatedAt19 WHERE `Id`=@kId19", sql);

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
    public async Task Update_BulkCopy_ManySharding()
    {
        await this.InitSharding();
        var createdAt = DateTime.Parse("2024-05-24");
        var repository = this.dbFactory.Create();
        var orders = repository.From<Order>()
            .UseTable("sys_order_104_202405", "sys_order_105_202405")
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
    [Fact]
    public async Task Update_ManySharding_Range()
    {
        await this.InitSharding();
        var beginTime = DateTime.Parse("2020-01-01");
        var endTime = DateTime.Parse("2024-12-31");
        var repository = this.dbFactory.Create();
        var sql = repository.From<Order>()
            .UseTableByRange("104", beginTime, endTime)
            .Select(f => new
            {
                f.Id,
                f.TenantId,
                f.OrderNo,
                f.TotalAmount
            })
            .OrderByDescending(f => f.Id)
            .ToSql(out _);
        Assert.Equal("SELECT * FROM (SELECT a.`Id`,a.`TenantId`,a.`OrderNo`,a.`TotalAmount` FROM `sys_order_104_202405` a UNION ALL SELECT a.`Id`,a.`TenantId`,a.`OrderNo`,a.`TotalAmount` FROM `sys_order_104_202406` a) a ORDER BY `Id` DESC", sql);
        var orders = repository.From<Order>()
            .UseTableByRange("104", beginTime, endTime)
            .Select(f => new
            {
                f.Id,
                f.TenantId,
                f.OrderNo,
                f.TotalAmount
            })
            .OrderByDescending(f => f.Id)
            .ToList();

        sql = repository.From<Order>()
           .UseTableByRange("104", beginTime, endTime)
           .InnerJoin<User>((x, y) => x.BuyerId == y.Id)
           .UseTableMap((orderOrigName, userOrigName, orderTableName) =>
           {
               var tableName = orderTableName.Replace(orderOrigName, userOrigName);
               return tableName[..^7];
           })
           .Select((x, y) => new
           {
               x.Id,
               x.TenantId,
               BuyerName = y.Name,
               x.TotalAmount
           })
           .OrderByDescending(f => f.Id)
           .ToSql(out _);
        Assert.Equal("SELECT * FROM (SELECT a.`Id`,a.`TenantId`,b.`Name` AS `BuyerName`,a.`TotalAmount` FROM `sys_order_104_202405` a INNER JOIN `sys_user_104` b ON a.`BuyerId`=b.`Id` UNION ALL SELECT a.`Id`,a.`TenantId`,b.`Name` AS `BuyerName`,a.`TotalAmount` FROM `sys_order_104_202406` a INNER JOIN `sys_user_104` b ON a.`BuyerId`=b.`Id`) a ORDER BY `Id` DESC", sql);
        var orderInfos = repository.From<Order>()
            .UseTableByRange("104", beginTime, endTime)
            .InnerJoin<User>((x, y) => x.BuyerId == y.Id)
            .UseTableMap((orderOrigName, userOrigName, orderTableName) =>
            {
                var tableName = orderTableName.Replace(orderOrigName, userOrigName);
                return tableName[..^7];
            })
            .Select((x, y) => new
            {
                x.Id,
                x.TenantId,
                BuyerName = y.Name,
                x.TotalAmount
            })
            .OrderByDescending(f => f.Id)
            .ToList();

        Assert.Equal(orders.Count, orderInfos.Count);
    }
    [Fact]
    public async Task ManySharding_FromQuery_SubQuery()
    {
        await this.InitSharding();
        var repository = this.dbFactory.Create();
        var count = 1;
        var amount = 50;
        var sql = repository
            .FromQuery(f => f.From<Order>()
                .UseTable("sys_order_104_202405", "sys_order_105_202405")
                .InnerJoin<User>((a, b) => a.BuyerId == b.Id)
                .UseTableMap((orderOrigName, userOrigName, orderTableName)
                    => orderTableName.Replace(orderOrigName, userOrigName).Slice(0, -6))
                .LeftJoin<OrderDetail>((a, b, c) => a.Id == c.OrderId)
                .UseTableMap((orderOrigName, orderDetailOrigName, orderTableName)
                    => orderTableName.Replace(orderOrigName, orderDetailOrigName))
                .GroupBy((a, b, c) => new { a.BuyerId, OrderId = a.Id, a.OrderNo })
                .Having((x, a, b, c) => x.CountDistinct(c.ProductId) > count)
                .Select((a, b, c, d) => new { a.Grouping.BuyerId, a.Grouping.OrderId, a.Grouping.OrderNo, ProductTotal = a.CountDistinct(d.ProductId) }))
            .InnerJoin<Order>((x, y) => x.OrderId == y.Id)
            .IncludeMany((a, b) => b.Details, f => f.Amount > amount)
            //.UseTableMap((orderOrigName, orderDetailOrigName, orderTableName)
            //    => orderTableName.Replace(orderOrigName, orderDetailOrigName))
            .Select((x, y) => new { y.Disputes, x.BuyerId, x.OrderId, x.OrderNo, x.ProductTotal, Order = y })
            .ToSql(out var dbParameters);
        Assert.Equal("SELECT b.`Disputes`,a.`BuyerId`,a.`OrderId`,a.`OrderNo`,a.`ProductTotal`,b.`Id`,b.`TenantId`,b.`OrderNo`,b.`ProductCount`,b.`TotalAmount`,b.`BuyerId`,b.`BuyerSource`,b.`SellerId`,b.`Products`,b.`Disputes`,b.`IsEnabled`,b.`CreatedAt`,b.`CreatedBy`,b.`UpdatedAt`,b.`UpdatedBy` FROM (SELECT a.`BuyerId`,a.`Id` AS `OrderId`,a.`OrderNo`,COUNT(DISTINCT c.`ProductId`) AS `ProductTotal` FROM `sys_order_104_202405` a INNER JOIN `sys_user_104` b ON a.`BuyerId`=b.`Id` LEFT JOIN `sys_order_detail_104_202405` c ON a.`Id`=c.`OrderId` GROUP BY a.`BuyerId`,a.`Id`,a.`OrderNo` HAVING COUNT(DISTINCT c.`ProductId`)>@p0) a INNER JOIN `sys_order` b ON a.`OrderId`=b.`Id` UNION ALL SELECT b.`Disputes`,a.`BuyerId`,a.`OrderId`,a.`OrderNo`,a.`ProductTotal`,b.`Id`,b.`TenantId`,b.`OrderNo`,b.`ProductCount`,b.`TotalAmount`,b.`BuyerId`,b.`BuyerSource`,b.`SellerId`,b.`Products`,b.`Disputes`,b.`IsEnabled`,b.`CreatedAt`,b.`CreatedBy`,b.`UpdatedAt`,b.`UpdatedBy` FROM (SELECT a.`BuyerId`,a.`Id` AS `OrderId`,a.`OrderNo`,COUNT(DISTINCT c.`ProductId`) AS `ProductTotal` FROM `sys_order_105_202405` a INNER JOIN `sys_user_105` b ON a.`BuyerId`=b.`Id` LEFT JOIN `sys_order_detail_105_202405` c ON a.`Id`=c.`OrderId` GROUP BY a.`BuyerId`,a.`Id`,a.`OrderNo` HAVING COUNT(DISTINCT c.`ProductId`)>@p0) a INNER JOIN `sys_order` b ON a.`OrderId`=b.`Id`", sql);
        Assert.Single(dbParameters);
        Assert.Equal((int)dbParameters[0].Value, count);

        var result = repository
            .FromQuery(f => f.From<Order>()
                .UseTable("sys_order_104_202405", "sys_order_105_202405")
                .InnerJoin<User>((a, b) => a.BuyerId == b.Id)
                .UseTableMap((orderOrigName, userOrigName, orderTableName)
                    => orderTableName.Replace(orderOrigName, userOrigName).Slice(0, -7))
                .LeftJoin<OrderDetail>((a, b, c) => a.Id == c.OrderId)
                .UseTableMap((orderOrigName, orderDetailOrigName, orderTableName)
                    => orderTableName.Replace(orderOrigName, orderDetailOrigName))
                .GroupBy((a, b, c) => new { a.BuyerId, OrderId = a.Id, a.OrderNo })
                .Having((x, a, b, c) => x.CountDistinct(c.ProductId) > count)
                .Select((a, b, c, d) => new { a.Grouping.BuyerId, a.Grouping.OrderId, a.Grouping.OrderNo, ProductTotal = a.CountDistinct(d.ProductId) }))
            .InnerJoin<Order>((x, y) => x.OrderId == y.Id)
            .IncludeMany((a, b) => b.Details, f => f.Amount > amount)
            .UseTableMap((orderOrigName, orderDetailOrigName, orderTableName)
                => orderTableName.Replace(orderOrigName, orderDetailOrigName))
            .Select((x, y) => new { y.Disputes, x.BuyerId, x.OrderId, x.OrderNo, x.ProductTotal, Order = y })
            .First();
        if (result != null)
        {
            Assert.NotNull(result.Disputes);
            Assert.NotNull(result.Order);
            Assert.NotNull(result.Order.Details);
            Assert.True(result.Order.Details.Count > 0);
            Assert.True(result.Order.Details[0].Amount > 0);
        }
    }
    [Fact]
    public async Task ManySharding_GroupBy()
    {
        await this.InitSharding();
        var repository = this.dbFactory.Create();
        var tenantId = "104";
        var beginTime = DateTime.Parse("2024-04-05");
        var endTime = DateTime.Parse("2024-06-05");
        var sql1 = repository.From<Order>()
            .UseTableByRange(tenantId, beginTime, endTime)
            .InnerJoin<User>((a, b) => a.BuyerId == b.Id)
            .UseTableMap((orderOrigName, userOrigName, orderTableName)
                => orderTableName.Replace(orderOrigName, userOrigName)[..^7])
            .GroupBy((a, b) => new { a.BuyerId, a.CreatedAt.Year })
            .Select((x, a, b) => new { a.BuyerId, a.CreatedAt.Year, Count = x.Count(a.Id) })
            .ToSql(out _);
        Assert.Equal("SELECT `BuyerId`,`Year`,SUM(`Count`) AS `Count` FROM (SELECT a.`BuyerId`,YEAR(a.`CreatedAt`) AS `Year`,COUNT(a.`Id`) AS `Count` FROM `sys_order_104_202405` a INNER JOIN `sys_user_104` b ON a.`BuyerId`=b.`Id` GROUP BY a.`BuyerId`,YEAR(a.`CreatedAt`) UNION ALL SELECT a.`BuyerId`,YEAR(a.`CreatedAt`) AS `Year`,COUNT(a.`Id`) AS `Count` FROM `sys_order_104_202406` a INNER JOIN `sys_user_104` b ON a.`BuyerId`=b.`Id` GROUP BY a.`BuyerId`,YEAR(a.`CreatedAt`)) a GROUP BY `BuyerId`,`Year`", sql1);

        var result1 = await repository.From<Order>()
            .UseTableByRange(tenantId, beginTime, endTime)
            .InnerJoin<User>((a, b) => a.BuyerId == b.Id)
            .UseTableMap((orderOrigName, userOrigName, orderTableName)
                => orderTableName.Replace(orderOrigName, userOrigName)[..^7])
            .GroupBy((a, b) => new { a.BuyerId, a.CreatedAt.Year })
            .Select((x, a, b) => new { a.BuyerId, a.CreatedAt.Year, Count = x.Count(a.Id) })
            .ToListAsync();
        Assert.NotEmpty(result1);

        var sql2 = repository.From<Order>()
            .UseTable("sys_order_104_202405", "sys_order_105_202405")
            .InnerJoin<User>((a, b) => a.BuyerId == b.Id)
            .UseTableMap((orderOrigName, userOrigName, orderTableName)
                => orderTableName.Replace(orderOrigName, userOrigName)[..^7])
            .GroupBy((a, b) => new { a.BuyerId, a.CreatedAt.Year })
            .OrderBy((x, a, b) => a.CreatedAt.Year)
            .Select((x, a, b) => new { a.BuyerId, a.CreatedAt.Year, Count = x.Count(a.Id) })
            .ToSql(out _);
        Assert.Equal("SELECT `BuyerId`,`Year`,SUM(`Count`) AS `Count` FROM (SELECT a.`BuyerId`,YEAR(a.`CreatedAt`) AS `Year`,COUNT(a.`Id`) AS `Count` FROM `sys_order_104_202405` a INNER JOIN `sys_user_104` b ON a.`BuyerId`=b.`Id` GROUP BY a.`BuyerId`,YEAR(a.`CreatedAt`) UNION ALL SELECT a.`BuyerId`,YEAR(a.`CreatedAt`) AS `Year`,COUNT(a.`Id`) AS `Count` FROM `sys_order_105_202405` a INNER JOIN `sys_user_105` b ON a.`BuyerId`=b.`Id` GROUP BY a.`BuyerId`,YEAR(a.`CreatedAt`)) a GROUP BY `BuyerId`,`Year` ORDER BY `Year`", sql2);

        var result2 = await repository.From<Order>()
            .UseTable("sys_order_104_202405", "sys_order_105_202405")
            .InnerJoin<User>((a, b) => a.BuyerId == b.Id)
            .UseTableMap((orderOrigName, userOrigName, orderTableName)
                => orderTableName.Replace(orderOrigName, userOrigName)[..^7])
            .GroupBy((a, b) => new { a.BuyerId, a.CreatedAt.Year })
            .OrderBy((x, a, b) => a.CreatedAt.Year)
            .Select((x, a, b) => new { a.BuyerId, a.CreatedAt.Year, Count = x.Count(a.Id) })
            .ToListAsync();
        Assert.NotEmpty(result2);

        var sql3 = repository.From<Order>()
            .UseTable("sys_order_104_202405", "sys_order_105_202405")
            .InnerJoin<User>((a, b) => a.BuyerId == b.Id)
            .UseTableMap((orderOrigName, userOrigName, orderTableName)
                => orderTableName.Replace(orderOrigName, userOrigName)[..^7])
            .GroupBy((a, b) => new { a.BuyerId, BuyerName = b.Name, a.CreatedAt.Year })
            .Having((x, a, b) => x.Count("*") > 1)
            .OrderBy((x, a, b) => x.Grouping.Year)
            .Select((x, a, b) => new { x.Grouping, Count = x.Count(a.Id) })
            .ToSql(out _);
        Assert.Equal("SELECT `BuyerId`,`BuyerName`,`Year`,SUM(`Count`) AS `Count` FROM (SELECT a.`BuyerId`,b.`Name` AS `BuyerName`,YEAR(a.`CreatedAt`) AS `Year`,COUNT(a.`Id`) AS `Count` FROM `sys_order_104_202405` a INNER JOIN `sys_user_104` b ON a.`BuyerId`=b.`Id` GROUP BY a.`BuyerId`,b.`Name`,YEAR(a.`CreatedAt`) UNION ALL SELECT a.`BuyerId`,b.`Name` AS `BuyerName`,YEAR(a.`CreatedAt`) AS `Year`,COUNT(a.`Id`) AS `Count` FROM `sys_order_105_202405` a INNER JOIN `sys_user_105` b ON a.`BuyerId`=b.`Id` GROUP BY a.`BuyerId`,b.`Name`,YEAR(a.`CreatedAt`)) a GROUP BY `BuyerId`,`BuyerName`,`Year` ORDER BY `Year`", sql3);

        var result3 = await repository.From<Order>()
            .UseTable("sys_order_104_202405", "sys_order_105_202405")
            .InnerJoin<User>((a, b) => a.BuyerId == b.Id)
            .UseTableMap((orderOrigName, userOrigName, orderTableName)
                => orderTableName.Replace(orderOrigName, userOrigName)[..^7])
            .GroupBy((a, b) => new { a.BuyerId, BuyerName = b.Name, a.CreatedAt.Year })
            .Having((x, a, b) => x.Count("*") > 1)
            .OrderBy((x, a, b) => x.Grouping.Year)
            .Select((x, a, b) => new { x.Grouping, Count = x.Count(a.Id) })
            .ToListAsync();
        Assert.NotEmpty(result3);
    }
    [Fact]
    public async Task ManySharding_Paging()
    {
        await this.InitSharding();
        var repository = this.dbFactory.Create();
        var tenantId = "104";
        var beginTime = DateTime.Parse("2024-04-05");
        var endTime = DateTime.Parse("2024-06-05");
        var sql1 = repository.From<Order>()
            .UseTableByRange(tenantId, beginTime, endTime)
            .InnerJoin<User>((a, b) => a.BuyerId == b.Id)
            .UseTableMap((orderOrigName, userOrigName, orderTableName)
                => orderTableName.Replace(orderOrigName, userOrigName)[..^7])
            .Select((a, b) => new { a.Id, a.BuyerId, a.TotalAmount, a.CreatedAt })
            .Page(1, 10)
            .ToSql(out _);
        Assert.Equal("SELECT * FROM (SELECT a.`Id`,a.`BuyerId`,a.`TotalAmount`,a.`CreatedAt` FROM `sys_order_104_202405` a INNER JOIN `sys_user_104` b ON a.`BuyerId`=b.`Id` UNION ALL SELECT a.`Id`,a.`BuyerId`,a.`TotalAmount`,a.`CreatedAt` FROM `sys_order_104_202406` a INNER JOIN `sys_user_104` b ON a.`BuyerId`=b.`Id`) b LIMIT 10", sql1);

        var result = await repository.From<Order>()
            .UseTableByRange(tenantId, beginTime, endTime)
            .InnerJoin<User>((a, b) => a.BuyerId == b.Id)
            .UseTableMap((orderOrigName, userOrigName, orderTableName)
                => orderTableName.Replace(orderOrigName, userOrigName)[..^7])
            .Select((a, b) => new { a.Id, a.BuyerId, a.TotalAmount, a.CreatedAt })
            .Page(1, 10)
            .ToPageListAsync();
        Assert.Equal(10, result.Count);

        var sql2 = repository.From<Order>()
            .UseTableByRange(tenantId, beginTime, endTime)
            .InnerJoin<User>((a, b) => a.BuyerId == b.Id)
            .UseTableMap((orderOrigName, userOrigName, orderTableName)
                => orderTableName.Replace(orderOrigName, userOrigName)[..^7])
            .Select((a, b) => new { a.Id, a.BuyerId, a.TotalAmount, a.CreatedAt })
            .Page(3, 10)
            .OrderBy(f => f.BuyerId).OrderByDescending(f => f.CreatedAt)
            .ToSql(out _);
        Assert.Equal("SELECT * FROM (SELECT a.`Id`,a.`BuyerId`,a.`TotalAmount`,a.`CreatedAt` FROM `sys_order_104_202405` a INNER JOIN `sys_user_104` b ON a.`BuyerId`=b.`Id` UNION ALL SELECT a.`Id`,a.`BuyerId`,a.`TotalAmount`,a.`CreatedAt` FROM `sys_order_104_202406` a INNER JOIN `sys_user_104` b ON a.`BuyerId`=b.`Id`) b ORDER BY `BuyerId`,`CreatedAt` DESC LIMIT 10 OFFSET 20", sql2);

        result = await repository.From<Order>()
            .UseTableByRange(tenantId, beginTime, endTime)
            .InnerJoin<User>((a, b) => a.BuyerId == b.Id)
            .UseTableMap((orderOrigName, userOrigName, orderTableName)
                => orderTableName.Replace(orderOrigName, userOrigName)[..^7])
            .Select((a, b) => new { a.Id, a.BuyerId, a.TotalAmount, a.CreatedAt })
            .Page(3, 10)
            .OrderBy(f => f.BuyerId).OrderByDescending(f => f.CreatedAt)
            .ToPageListAsync();
        Assert.Equal(10, result.Count);
    }
    [Fact]
    public async Task ManySharding_Aggregate()
    {
        //await this.InitSharding();
        var repository = this.dbFactory.Create();
        var tenantId = "104";
        var beginTime = DateTime.Parse("2024-05-05");
        var endTime = DateTime.Parse("2024-06-05");
        var result1 = await repository.From<Order>()
            .UseTableByRange(tenantId, beginTime, endTime)
            .InnerJoin<User>((a, b) => a.BuyerId == b.Id)
            .UseTableMap((orderOrigName, userOrigName, orderTableName)
                => orderTableName.Replace(orderOrigName, userOrigName)[..^7])
            .Where((a, b) => a.TotalAmount > 100)
            .CountAsync((a, b) => a.Id);
        var scalarValue1 = await repository.QueryScalarAsync<int>("SELECT SUM(COUNT_VALUE) FROM (SELECT COUNT(a.`Id`) AS COUNT_VALUE FROM `sys_order_104_202405` a INNER JOIN `sys_user_104` b ON a.`BuyerId`=b.`Id` WHERE a.`TotalAmount`>100 UNION ALL SELECT COUNT(a.`Id`) AS COUNT_VALUE FROM `sys_order_104_202406` a INNER JOIN `sys_user_104` b ON a.`BuyerId`=b.`Id` WHERE a.`TotalAmount`>100) AS t");
        Assert.Equal(scalarValue1, result1);

        var result2 = await repository.From<Order>()
            .UseTableByRange(tenantId, beginTime, endTime)
            .InnerJoin<User>((a, b) => a.BuyerId == b.Id)
            .UseTableMap((orderOrigName, userOrigName, orderTableName)
                => orderTableName.Replace(orderOrigName, userOrigName)[..^7])
            .SumAsync((a, b) => a.TotalAmount);
        var scalarValue2 = await repository.QueryScalarAsync<decimal>("SELECT SUM(SUM_VALUE) FROM (SELECT SUM(a.`TotalAmount`) AS SUM_VALUE FROM `sys_order_104_202405` a INNER JOIN `sys_user_104` b ON a.`BuyerId`=b.`Id` UNION ALL SELECT SUM(a.`TotalAmount`) AS SUM_VALUE FROM `sys_order_104_202406` a INNER JOIN `sys_user_104` b ON a.`BuyerId`=b.`Id`) AS t");
        Assert.Equal(scalarValue2, result2);

        var result3 = await repository.From<Order>()
            .UseTableByRange(tenantId, beginTime, endTime)
            .InnerJoin<User>((a, b) => a.BuyerId == b.Id)
            .UseTableMap((orderOrigName, userOrigName, orderTableName)
                => orderTableName.Replace(orderOrigName, userOrigName)[..^7])
            .MaxAsync((a, b) => a.TotalAmount);
        var scalarValue3 = await repository.QueryScalarAsync<double>("SELECT MAX(MAX_VALUE) FROM (SELECT MAX(a.`TotalAmount`) AS MAX_VALUE FROM `sys_order_104_202405` a INNER JOIN `sys_user_104` b ON a.`BuyerId`=b.`Id` UNION ALL SELECT MAX(a.`TotalAmount`) AS MAX_VALUE FROM `sys_order_104_202406` a INNER JOIN `sys_user_104` b ON a.`BuyerId`=b.`Id`) AS t");
        Assert.Equal(scalarValue3, result3);

        var result4 = await repository.From<Order>()
            .UseTableByRange(tenantId, beginTime, endTime)
            .InnerJoin<User>((a, b) => a.BuyerId == b.Id)
            .UseTableMap((orderOrigName, userOrigName, orderTableName)
                => orderTableName.Replace(orderOrigName, userOrigName)[..^7])
            .AvgAsync((a, b) => a.TotalAmount);
        var scalarValue4 = await repository.QueryScalarAsync<double>("SELECT AVG(AVG_VALUE) FROM (SELECT a.`TotalAmount` AS AVG_VALUE FROM `sys_order_104_202405` a INNER JOIN `sys_user_104` b ON a.`BuyerId`=b.`Id` UNION ALL SELECT a.`TotalAmount` AS AVG_VALUE FROM `sys_order_104_202406` a INNER JOIN `sys_user_104` b ON a.`BuyerId`=b.`Id`) AS t");
        Assert.Equal(scalarValue4, result4);
    }
    [Fact]
    public void TableSchema()
    {
        var repository = this.dbFactory.Create();
        var sql = repository
            .FromQuery(f => f.From<OrderDetail>()
                .UseTableSchema("myschema")
                .InnerJoin<Order>((x, y) => x.OrderId == y.Id)
                .UseTableSchema("myschema")
                .GroupBy((a, b) => new { OrderId = b.Id, b.BuyerId })
                .Select((x, a, b) => new { Group = x.Grouping, ProductCount = x.CountDistinct(a.ProductId) }))
            .InnerJoin<User>((x, y) => x.Group.BuyerId == y.Id)
            .UseTableSchema("myschema")
            .Where((a, b) => a.ProductCount > 1)
            .Select((x, y) => new
            {
                x.Group,
                Buyer = y,
                x.ProductCount
            })
            .ToSql(out _);
        Assert.Equal("SELECT a.`OrderId`,a.`BuyerId`,b.`Id`,b.`TenantId`,b.`Name`,b.`Gender`,b.`Age`,b.`CompanyId`,b.`GuidField`,b.`SomeTimes`,b.`SourceType`,b.`IsEnabled`,b.`CreatedAt`,b.`CreatedBy`,b.`UpdatedAt`,b.`UpdatedBy`,a.`ProductCount` FROM (SELECT b.`Id` AS `OrderId`,b.`BuyerId`,COUNT(DISTINCT a.`ProductId`) AS `ProductCount` FROM `myschema`.`sys_order_detail` a INNER JOIN `myschema`.`sys_order` b ON a.`OrderId`=b.`Id` GROUP BY b.`Id`,b.`BuyerId`) a INNER JOIN `myschema`.`sys_user` b ON a.`BuyerId`=b.`Id` WHERE a.`ProductCount`>1", sql);

        var result = repository
            .FromQuery(f => f.From<OrderDetail>()
                .UseTableSchema("fengling")
                .InnerJoin<Order>((x, y) => x.OrderId == y.Id)
                .UseTableSchema("fengling")
                .GroupBy((a, b) => new { OrderId = b.Id, b.BuyerId })
                .Select((x, a, b) => new { Group = x.Grouping, ProductCount = x.CountDistinct(a.ProductId) }))
            .InnerJoin<User>((x, y) => x.Group.BuyerId == y.Id)
            .UseTableSchema("fengling")
            .Where((a, b) => a.ProductCount > 1)
            .Select((x, y) => new
            {
                x.Group,
                Buyer = y,
                x.ProductCount
            })
            .ToList();
        if (result.Count > 0)
        {
            Assert.NotNull(result[0]);
            Assert.NotNull(result[0].Group);
            Assert.NotNull(result[0].Buyer);
            Assert.True(result[0].ProductCount > 1);
        }
    }
    [Fact]
    public async Task Query_ManySharding_SingleTable_Include_TableSchema()
    {
        await this.InitSharding();
        var productCount = 1;
        var repository = this.dbFactory.Create();
        var sql = repository.From<Order>()
            .UseTable("sys_order_104_202405", "sys_order_105_202405")
            .UseTableSchema("fengling")
            .Include(f => f.Details)
            .UseTableSchema("fengling")
            .UseTableMap((origOrderName, origOrderDetailName, orderName) =>
                orderName.Replace(origOrderName, origOrderDetailName))
            .Where(f => f.ProductCount > productCount)
            .ToSql(out _);
        Assert.Equal("SELECT a.`Id`,a.`TenantId`,a.`OrderNo`,a.`ProductCount`,a.`TotalAmount`,a.`BuyerId`,a.`BuyerSource`,a.`SellerId`,a.`Products`,a.`Disputes`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy` FROM `sys_order_104_202405` a WHERE a.`ProductCount`>@p0", sql);

        var result = repository.From<Order>()
            .UseTable("sys_order_104_202405", "sys_order_105_202405")
            .UseTableSchema("fengling")
            .Include(f => f.Details)
            .UseTableSchema("fengling")
            .UseTableMap((origOrderName, origOrderDetailName, orderName) =>
                orderName.Replace(origOrderName, origOrderDetailName))
            .Where(f => f.ProductCount > productCount)
            .ToList();
        if (result.Count > 0)
        {
            var tenantIds = result.Select(f => f.TenantId).Distinct().ToList();
            Assert.False(tenantIds.Exists(f => f != "104"));
            foreach (var order in result)
            {
                Assert.NotNull(order.Details);
                foreach (var orderDetail in order.Details)
                {
                    Assert.Equal("104", orderDetail.TenantId);
                }
            }
        }

        sql = repository.From<Order>()
            .UseTable("sys_order_104_202405", "sys_order_105_202405")
            .UseTableSchema("fengling")
            .Include(f => f.Details)
            .UseTableSchema("fengling")
            .UseTableMap((origOrderName, origOrderDetailName, orderName) =>
                orderName.Replace(origOrderName, origOrderDetailName))
            .Where(f => f.ProductCount > productCount)
            .ToSql(out _);
        Assert.Equal("SELECT a.`Id`,a.`TenantId`,a.`OrderNo`,a.`ProductCount`,a.`TotalAmount`,a.`BuyerId`,a.`BuyerSource`,a.`SellerId`,a.`Products`,a.`Disputes`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy` FROM `sys_order_104_202405` a WHERE a.`ProductCount`>@p0 UNION ALL SELECT a.`Id`,a.`TenantId`,a.`OrderNo`,a.`ProductCount`,a.`TotalAmount`,a.`BuyerId`,a.`BuyerSource`,a.`SellerId`,a.`Products`,a.`Disputes`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy` FROM `sys_order_105_202405` a WHERE a.`ProductCount`>@p0", sql);

        result = repository.From<Order>()
            .UseTable("sys_order_104_202405", "sys_order_105_202405")
            .UseTableSchema("fengling")
            .Include(f => f.Details)
            .UseTableSchema("fengling")
            .UseTableMap((origOrderName, origOrderDetailName, orderName) =>
                orderName.Replace(origOrderName, origOrderDetailName))
            .Where(f => f.ProductCount > productCount)
            .ToList();
        if (result.Count > 0)
        {
            var tenantIds = result.Select(f => f.TenantId).ToList();
            Assert.True(tenantIds.Exists(f => "104,105".Contains(f)));
            foreach (var order in result)
            {
                Assert.NotNull(order.Details);
                foreach (var orderDetail in order.Details)
                {
                    Assert.Contains(orderDetail.TenantId, "104,105");
                }
            }
        }
    }
    [Fact]
    public async Task Create_Without_Sharding()
    {
        var repository = this.dbFactory.Create();
        await repository.Delete<User>()
            .UseTableBy("104")
            .WhereById(11)
            .ExecuteAsync();
        repository.Create<User>()
            .WithBy(new
            {
                Id = 11,
                TenantId = "104",
                Name = "leafkevin",
                Age = 25,
                CompanyId = 1,
                Gender = Gender.Male,
                GuidField = Guid.NewGuid(),
#if NET6_0_OR_GREATER
                SomeTimes = TimeOnly.FromTimeSpan(TimeSpan.FromSeconds(4769)),
#else
                SomeTimes = TimeSpan.FromSeconds(4769),
#endif
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
            .Where(f => f.Id == 11)
            .First();
        Assert.NotNull(result);
        Assert.Equal("104", result.TenantId);
    }
    [Fact]
    public async Task CreateShardingTable()
    {
        var tenantId = "104";
        var now = DateTime.Now;
        var repository = this.dbFactory.Create();
        var tableName1 = repository.GetShardingTableName<OrderDetail>(tenantId, now);
        var tableName2 = repository.GetShardingTableName<OrderDetail>(tenantId, now.AddMonths(1));
        var sql = $"DROP TABLE {tableName1};DROP TABLE {tableName2}";
        await repository.ExecuteAsync(sql);
        repository.CreateShardingTable<OrderDetail>(tableName1);
        await repository.CreateShardingTableAsync<OrderDetail>(tableName2);
        sql = $"DROP TABLE {tableName1};DROP TABLE {tableName2}";
        await repository.ExecuteAsync(sql);
        repository.CreateShardingTable<OrderDetail>([tenantId, now]);
        await repository.CreateShardingTableAsync<OrderDetail>([tenantId, now.AddMonths(1)]);
    }
    [Fact]
    public async Task GetShardingTables()
    {
        var tenantId = "104";
        var now = DateTime.Now;
        var repository = this.dbFactory.Create();
        var tableName1 = repository.GetShardingTableName<OrderDetail>(tenantId, now);
        var tableName2 = repository.GetShardingTableName<OrderDetail>(tenantId, now.AddMonths(1));
        var sql = $"DROP TABLE {tableName1};DROP TABLE {tableName1};DROP TABLE {tableName2}";
        await repository.ExecuteAsync(sql);
        repository.CreateShardingTable<OrderDetail>(tableName1);
        await repository.CreateShardingTableAsync<OrderDetail>(tableName2);
        sql = $"DROP TABLE {tableName1};DROP TABLE {tableName2}";
        await repository.ExecuteAsync(sql);
        repository.CreateShardingTable<OrderDetail>([tenantId, now]);
        await repository.CreateShardingTableAsync<OrderDetail>([tenantId, now.AddMonths(1)]);
    }
    [Fact]
    public async Task ManyShardingCountDistinct()
    {
        var repository = this.dbFactory.Create();
        var result = await repository.FromQuery(t => t.From<User>()
            .UseTableBy("sys_user_104", "sys_user_105")
            .UseUnionShardingTable()
            .Select(f => f.Id).Distinct())
            .CountAsync();
        var sql = $"SELECT DISTINCT Id FROM sys_user_104 UNION SELECT DISTINCT Id FROM sys_user_105";
        var count = await repository.QueryScalarAsync<int>($"SELECT COUNT(DISTINCT Id) FROM ({sql}) AS t");
        Assert.Equal(result, count);
    }
}