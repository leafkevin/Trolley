using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
                .Register<MySqlProvider>("fengling", "Server=localhost;Database=fengling;Uid=root;password=123456;charset=utf8mb4;AllowLoadLocalInfile=true", true)
                .Register<MySqlProvider>("fengling_tenant1", "Server=localhost;Database=fengling_tenant1;Uid=root;password=123456;charset=utf8mb4;AllowLoadLocalInfile=true", false)
                .Register<MySqlProvider>("fengling_tenant2", "Server=localhost;Database=fengling_tenant2;Uid=root;password=123456;charset=utf8mb4;AllowLoadLocalInfile=true", false)
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
        private async Task InitSharding()
        {
            using var repository = this.dbFactory.Create();
            await repository.Delete<User>()
                .UseTableBy("104")
                .Where(101)
                .ExecuteAsync();
            await repository.Delete<User>()
               .UseTableBy("105")
               .Where(new[] { 102, 103 })
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
                .Where(101)
                .ExecuteAsync();
            await repository.Delete<User>()
                .UseTableBy("105")
                .Where(102)
                .ExecuteAsync();
            await repository.Delete<User>()
                .UseTableBy("105")
                .Where(103)
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
            await repository.Delete<User>()
                .UseTableBy("104")
                .Where(101)
                .ExecuteAsync();
            var userIds = new[] { 102, 103 };
            await repository.Delete<User>()
                .UseTableBy("105")
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
            Assert.True(sql == "SELECT a.`Id`,a.`TenantId`,a.`OrderNo`,a.`ProductCount`,a.`TotalAmount`,a.`BuyerId`,a.`BuyerSource`,a.`SellerId`,a.`Products`,a.`Disputes`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy` FROM `sys_order_104_202405` a WHERE a.`ProductCount`>@p0 UNION ALL SELECT a.`Id`,a.`TenantId`,a.`OrderNo`,a.`ProductCount`,a.`TotalAmount`,a.`BuyerId`,a.`BuyerSource`,a.`SellerId`,a.`Products`,a.`Disputes`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy` FROM `sys_order_105_202405` a WHERE a.`ProductCount`>@p0");

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
            Assert.True(sql == "SELECT a.`Id`,a.`OrderNo`,a.`TenantId`,a.`BuyerId`,b.`Name` AS `BuyerName` FROM `sys_order_104_202405` a INNER JOIN `sys_user_104` b ON a.`BuyerId`=b.`Id` WHERE a.`Id`=@p0");

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
            Assert.True(sql == "SELECT a.`OrderId`,a.`BuyerId`,b.`TenantId`,b.`Id`,b.`TenantId`,b.`Name`,b.`Gender`,b.`Age`,b.`CompanyId`,b.`GuidField`,b.`SomeTimes`,b.`SourceType`,b.`IsEnabled`,b.`CreatedAt`,b.`CreatedBy`,b.`UpdatedAt`,b.`UpdatedBy`,a.`ProductCount` FROM (SELECT b.`Id` AS `OrderId`,b.`BuyerId`,COUNT(DISTINCT a.`ProductId`) AS `ProductCount` FROM `sys_order_detail_104_202405` a INNER JOIN `sys_order_104_202405` b ON a.`OrderId`=b.`Id` GROUP BY b.`Id`,b.`BuyerId`) a INNER JOIN `sys_user_104` b ON a.`BuyerId`=b.`Id` WHERE a.`ProductCount`>1 UNION ALL SELECT a.`OrderId`,a.`BuyerId`,b.`TenantId`,b.`Id`,b.`TenantId`,b.`Name`,b.`Gender`,b.`Age`,b.`CompanyId`,b.`GuidField`,b.`SomeTimes`,b.`SourceType`,b.`IsEnabled`,b.`CreatedAt`,b.`CreatedBy`,b.`UpdatedAt`,b.`UpdatedBy`,a.`ProductCount` FROM (SELECT b.`Id` AS `OrderId`,b.`BuyerId`,COUNT(DISTINCT a.`ProductId`) AS `ProductCount` FROM `sys_order_detail_105_202405` a INNER JOIN `sys_order_105_202405` b ON a.`OrderId`=b.`Id` GROUP BY b.`Id`,b.`BuyerId`) a INNER JOIN `sys_user_105` b ON a.`BuyerId`=b.`Id` WHERE a.`ProductCount`>1");

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
            Assert.True(sql == "SELECT a.`Id`,a.`TenantId`,a.`OrderNo`,a.`ProductCount`,a.`TotalAmount`,a.`BuyerId`,a.`BuyerSource`,a.`SellerId`,a.`Products`,a.`Disputes`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy`,b.`Id`,b.`TenantId`,b.`Name`,b.`Gender`,b.`Age`,b.`CompanyId`,b.`GuidField`,b.`SomeTimes`,b.`SourceType`,b.`IsEnabled`,b.`CreatedAt`,b.`CreatedBy`,b.`UpdatedAt`,b.`UpdatedBy` FROM `sys_order_104_202405` a INNER JOIN `sys_user_104` b ON a.`BuyerId`=b.`Id` WHERE a.`ProductCount`>@p0 UNION ALL SELECT a.`Id`,a.`TenantId`,a.`OrderNo`,a.`ProductCount`,a.`TotalAmount`,a.`BuyerId`,a.`BuyerSource`,a.`SellerId`,a.`Products`,a.`Disputes`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy`,b.`Id`,b.`TenantId`,b.`Name`,b.`Gender`,b.`Age`,b.`CompanyId`,b.`GuidField`,b.`SomeTimes`,b.`SourceType`,b.`IsEnabled`,b.`CreatedAt`,b.`CreatedBy`,b.`UpdatedAt`,b.`UpdatedBy` FROM `sys_order_105_202405` a INNER JOIN `sys_user_105` b ON a.`BuyerId`=b.`Id` WHERE a.`ProductCount`>@p0");

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
            Assert.True(sql == "SELECT a.`Id`,a.`TenantId`,a.`OrderNo`,a.`ProductCount`,a.`TotalAmount`,a.`BuyerId`,a.`BuyerSource`,a.`SellerId`,a.`Products`,a.`Disputes`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy`,b.`Id`,b.`TenantId`,b.`Name`,b.`Gender`,b.`Age`,b.`CompanyId`,b.`GuidField`,b.`SomeTimes`,b.`SourceType`,b.`IsEnabled`,b.`CreatedAt`,b.`CreatedBy`,b.`UpdatedAt`,b.`UpdatedBy` FROM `sys_order_104_202405` a INNER JOIN `sys_user_104` b ON a.`BuyerId`=b.`Id` WHERE a.`ProductCount`>@p0 UNION ALL SELECT a.`Id`,a.`TenantId`,a.`OrderNo`,a.`ProductCount`,a.`TotalAmount`,a.`BuyerId`,a.`BuyerSource`,a.`SellerId`,a.`Products`,a.`Disputes`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy`,b.`Id`,b.`TenantId`,b.`Name`,b.`Gender`,b.`Age`,b.`CompanyId`,b.`GuidField`,b.`SomeTimes`,b.`SourceType`,b.`IsEnabled`,b.`CreatedAt`,b.`CreatedBy`,b.`UpdatedAt`,b.`UpdatedBy` FROM `sys_order_105_202405` a INNER JOIN `sys_user_105` b ON a.`BuyerId`=b.`Id` WHERE a.`ProductCount`>@p0");

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
            Assert.True(sql == "SELECT a.`Id`,a.`TenantId`,a.`OrderNo`,a.`ProductCount`,a.`TotalAmount`,a.`BuyerId`,a.`BuyerSource`,a.`SellerId`,a.`Products`,a.`Disputes`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy`,b.`Id`,b.`TenantId`,b.`OrderId`,b.`ProductId`,b.`Price`,b.`Quantity`,b.`Amount`,b.`IsEnabled`,b.`CreatedAt`,b.`CreatedBy`,b.`UpdatedAt`,b.`UpdatedBy` FROM `sys_order_104_202405` a INNER JOIN `sys_order_detail_104_202405` b ON a.`Id`=b.`OrderId` WHERE a.`ProductCount`>@p0 UNION ALL SELECT a.`Id`,a.`TenantId`,a.`OrderNo`,a.`ProductCount`,a.`TotalAmount`,a.`BuyerId`,a.`BuyerSource`,a.`SellerId`,a.`Products`,a.`Disputes`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy`,b.`Id`,b.`TenantId`,b.`OrderId`,b.`ProductId`,b.`Price`,b.`Quantity`,b.`Amount`,b.`IsEnabled`,b.`CreatedAt`,b.`CreatedBy`,b.`UpdatedAt`,b.`UpdatedBy` FROM `sys_order_105_202405` a INNER JOIN `sys_order_detail_105_202405` b ON a.`Id`=b.`OrderId` WHERE a.`ProductCount`>@p0");

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
            Assert.True(sql == "SELECT a.`Id`,a.`TenantId`,a.`OrderNo`,a.`ProductCount`,a.`TotalAmount`,a.`BuyerId`,a.`BuyerSource`,a.`SellerId`,a.`Products`,a.`Disputes`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy` FROM `sys_order_104_202405` a WHERE EXISTS(SELECT * FROM `sys_user_104` b WHERE b.`Id`=a.`BuyerId` AND b.`Age`<25)");

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
            Assert.True(sql == "SELECT a.`Id`,a.`TenantId`,a.`OrderNo`,a.`ProductCount`,a.`TotalAmount`,a.`BuyerId`,a.`BuyerSource`,a.`SellerId`,a.`Products`,a.`Disputes`,a.`IsEnabled`,a.`CreatedAt`,a.`CreatedBy`,a.`UpdatedAt`,a.`UpdatedBy` FROM `sys_order_104_202405` a WHERE EXISTS(SELECT * FROM `sys_user_104` b,`sys_order_detail_104_202405` c WHERE b.`Id`=a.`BuyerId` AND b.`Age`<=25 AND c.`Price`>100)");

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
    }
}
