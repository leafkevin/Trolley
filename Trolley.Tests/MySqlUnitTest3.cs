using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using Xunit;

namespace Trolley.Tests;

public class MySqlUnitTest3
{
    private readonly IOrmDbFactory dbFactory;
    public MySqlUnitTest3()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IOrmProvider, MySqlProvider>();
        services.AddSingleton(f =>
        {
            var connectionString = "Server=localhost;Database=fengling;Uid=root;password=123456;charset=utf8mb4;";
            var ormProvider = f.GetService<IOrmProvider>();
            var builder = new OrmDbFactoryBuilder();
            builder.Register("fengling", true, f => f.Add(connectionString, ormProvider, true))
                .Configure(f => new ModelConfiguration().OnModelCreating(f));
            return builder.Build();
        });
        var serviceProvider = services.BuildServiceProvider();
        this.dbFactory = serviceProvider.GetService<IOrmDbFactory>();

        using var repository = this.dbFactory.Create();
        repository.Delete<User>(new[] { 1, 2 });
        repository.Create<User>(new[]
        {
            new User
            {
                Id = 1,
                Name = "leafkevin",
                Age = 25,
                CompanyId = 1,
                Gender = Gender.Male,
                IsEnabled = true,
                CreatedAt = DateTime.Now,
                CreatedBy = 1,
                UpdatedAt = DateTime.Now,
                UpdatedBy = 1
            },
            new User
            {
                Id = 2,
                Name = "cindy",
                Age = 21,
                CompanyId = 2,
                Gender = Gender.Male,
                IsEnabled = true,
                CreatedAt = DateTime.Now,
                CreatedBy = 1,
                UpdatedAt = DateTime.Now,
                UpdatedBy = 1
            }
        });

        repository.Delete<Company>(new[] { 1, 2 });
        repository.Create<Company>(new[]
        {
            new Company
            {
                Id = 1,
                Name = "微软",
                IsEnabled = true,
                CreatedAt = DateTime.Now,
                CreatedBy = 1,
                UpdatedAt = DateTime.Now,
                UpdatedBy = 1
            },
            new Company
            {
                Id = 2,
                Name = "谷歌",
                IsEnabled = true,
                CreatedAt = DateTime.Now,
                CreatedBy = 1,
                UpdatedAt = DateTime.Now,
                UpdatedBy = 1
            }
        });

        repository.Delete<Brand>(new[] { 1, 2, 3 });
        repository.Create<Brand>(new[]
        {
            new Brand
            {
                Id = 1,
                BrandNo = "BN-001",
                Name = "波司登",
                IsEnabled = true,
                CreatedAt = DateTime.Now,
                CreatedBy = 1,
                UpdatedAt = DateTime.Now,
                UpdatedBy = 1
            },
            new Brand
            {
                Id = 2,
                BrandNo = "BN-002",
                Name = "雪中飞",
                IsEnabled = true,
                CreatedAt = DateTime.Now,
                CreatedBy = 1,
                UpdatedAt = DateTime.Now,
                UpdatedBy = 1
            },
            new Brand
            {
                Id = 3,
                BrandNo = "BN-003",
                Name = "优衣库",
                IsEnabled = true,
                CreatedAt = DateTime.Now,
                CreatedBy = 1,
                UpdatedAt = DateTime.Now,
                UpdatedBy = 1
            }
        });

        repository.Delete<Product>(new[] { 1, 2, 3 });
        repository.Create<Product>(new[]
        {
            new Product
            {
                Id = 1,
                ProductNo="PN-001",
                Name = "波司登羽绒服",
                BrandId = 1,
                CategoryId = 1,
                IsEnabled = true,
                CreatedAt = DateTime.Now,
                CreatedBy = 1,
                UpdatedAt = DateTime.Now,
                UpdatedBy = 1
            },
            new Product
            {
                Id = 2,
                ProductNo="PN-002",
                Name = "雪中飞羽绒裤",
                BrandId = 2,
                CategoryId = 2,
                IsEnabled = true,
                CreatedAt = DateTime.Now,
                CreatedBy = 1,
                UpdatedAt = DateTime.Now,
                UpdatedBy = 1
            },
            new Product
            {
                Id = 3,
                ProductNo="PN-003",
                Name = "优衣库保暖内衣",
                BrandId = 3,
                CategoryId = 3,
                IsEnabled = true,
                CreatedAt = DateTime.Now,
                CreatedBy = 1,
                UpdatedAt = DateTime.Now,
                UpdatedBy = 1
            }
        });

        repository.Delete<Order>(new[] { 1, 2, 3 });
        repository.Create<Order>(new[]
        {
            new Order
            {
                Id = 1,
                OrderNo = "ON-001",
                BuyerId = 1,
                SellerId = 2,
                TotalAmount = 500,
                IsEnabled = true,
                CreatedAt = DateTime.Now,
                CreatedBy = 1,
                UpdatedAt = DateTime.Now,
                UpdatedBy = 1
            },
            new Order
            {
                Id = 2,
                OrderNo = "ON-002",
                BuyerId = 2,
                SellerId = 1,
                TotalAmount = 350,
                IsEnabled = true,
                CreatedAt = DateTime.Now,
                CreatedBy = 1,
                UpdatedAt = DateTime.Now,
                UpdatedBy = 1
            },
            new Order
            {
                Id = 3,
                OrderNo = "ON-003",
                BuyerId = 1,
                SellerId = 2,
                TotalAmount = 199,
                IsEnabled = true,
                CreatedAt = DateTime.Now,
                CreatedBy = 1,
                UpdatedAt = DateTime.Now,
                UpdatedBy = 1
            }
        });

        repository.Delete<OrderDetail>(new[] { 1, 2, 3, 4, 5, 6 });
        repository.Create<OrderDetail>(new[]
        {
            new OrderDetail
            {
                Id = 1,
                OrderId = 1,
                ProductId = 1,
                Price = 299,
                Quantity = 1,
                Amount = 299,
                IsEnabled = true,
                CreatedAt = DateTime.Now,
                CreatedBy = 1,
                UpdatedAt = DateTime.Now,
                UpdatedBy = 1
            },
            new OrderDetail
            {
                Id = 2,
                OrderId = 1,
                ProductId = 2,
                Price = 159,
                Quantity = 1,
                Amount = 159,
                IsEnabled = true,
                CreatedAt = DateTime.Now,
                CreatedBy = 1,
                UpdatedAt = DateTime.Now,
                UpdatedBy = 1
            },
            new OrderDetail
            {
                Id = 3,
                OrderId = 1,
                ProductId = 3,
                Price = 69,
                Quantity = 1,
                Amount = 69,
                IsEnabled = true,
                CreatedAt = DateTime.Now,
                CreatedBy = 1,
                UpdatedAt = DateTime.Now,
                UpdatedBy = 1
            },
            new OrderDetail
            {
                Id = 4,
                OrderId = 2,
                ProductId = 1,
                Price = 299,
                Quantity = 1,
                Amount = 299,
                IsEnabled = true,
                CreatedAt = DateTime.Now,
                CreatedBy = 1,
                UpdatedAt = DateTime.Now,
                UpdatedBy = 1
            },
            new OrderDetail
            {
                Id = 5,
                OrderId = 2,
                ProductId = 3,
                Price = 69,
                Quantity = 1,
                Amount = 69,
                IsEnabled = true,
                CreatedAt = DateTime.Now,
                CreatedBy = 1,
                UpdatedAt = DateTime.Now,
                UpdatedBy = 1
            },
            new OrderDetail
            {
                Id = 6,
                OrderId = 3,
                ProductId = 2,
                Price = 199,
                Quantity = 1,
                Amount = 199,
                IsEnabled = true,
                CreatedAt = DateTime.Now,
                CreatedBy = 1,
                UpdatedAt = DateTime.Now,
                UpdatedBy = 1
            }
        });
    }
    [Fact]
    public void Update_Parameters_One()
    {
        using var repository = this.dbFactory.Create();
        var result = repository.Update<User>(new { Name = "leafkevin1", Id = 1 });
        var result1 = repository.Get<User>(new { Id = 1 });
        Assert.True(result > 0);
        Assert.NotNull(result1);
        Assert.True(result1.Id == 1);
        Assert.True(result1.Name == "leafkevin1");
    }
    [Fact]
    public async void UpdateAsync_Parameters_Multi()
    {
        using var repository = this.dbFactory.Create();
        var orderDetails = await repository.From<OrderDetail>().ToListAsync();
        var parameters = orderDetails.Select(f => new { f.Id, Quantity = f.Quantity + 1, Amount = f.Amount + 50 }).ToList();
        var sql = repository.Update<OrderDetail>().WithBy(parameters).ToSql(out _);
        Assert.True(sql == "UPDATE `sys_order_detail` SET `Quantity`=@Quantity0,`Amount`=@Amount0 WHERE `Id`=@kId0;UPDATE `sys_order_detail` SET `Quantity`=@Quantity1,`Amount`=@Amount1 WHERE `Id`=@kId1;UPDATE `sys_order_detail` SET `Quantity`=@Quantity2,`Amount`=@Amount2 WHERE `Id`=@kId2;UPDATE `sys_order_detail` SET `Quantity`=@Quantity3,`Amount`=@Amount3 WHERE `Id`=@kId3;UPDATE `sys_order_detail` SET `Quantity`=@Quantity4,`Amount`=@Amount4 WHERE `Id`=@kId4;UPDATE `sys_order_detail` SET `Quantity`=@Quantity5,`Amount`=@Amount5 WHERE `Id`=@kId5");
    }
    [Fact]
    public async void UpdateAsync_Parameters_WithBy()
    {
        using var repository = this.dbFactory.Create();
        var orders = await repository.From<Order>().ToListAsync();
        var sql = repository.Update<Order>().WithBy(f => new { BuyerId = DBNull.Value, OrderNo = "ON_" + f.OrderNo, f.TotalAmount }, orders).ToSql(out _);
        Assert.True(sql == "UPDATE `sys_order` SET `BuyerId`=NULL,`OrderNo`=CONCAT('ON_',`OrderNo`),`TotalAmount`=@TotalAmount0 WHERE `Id`=@kId0;UPDATE `sys_order` SET `BuyerId`=NULL,`OrderNo`=CONCAT('ON_',`OrderNo`),`TotalAmount`=@TotalAmount1 WHERE `Id`=@kId1;UPDATE `sys_order` SET `BuyerId`=NULL,`OrderNo`=CONCAT('ON_',`OrderNo`),`TotalAmount`=@TotalAmount2 WHERE `Id`=@kId2");
    }
    //[Fact]
    //public async void UpdateAsync_Parameters_WithBy()
    //{
    //    using var repository = this.dbFactory.Create();
    //    var orders = await repository.From<Order>().ToListAsync();
    //    var sql = repository.Update<Order>().WithBy(f => new { BuyerId = DBNull.Value, OrderNo = "ON_" + f.OrderNo, f.TotalAmount }, orders).ToSql(out _);
    //    Assert.True(sql == "UPDATE `sys_order` SET `BuyerId`=NULL,`OrderNo`=CONCAT('ON_',`OrderNo`),`TotalAmount`=@TotalAmount0 WHERE `Id`=@kId0;UPDATE `sys_order` SET `BuyerId`=NULL,`OrderNo`=CONCAT('ON_',`OrderNo`),`TotalAmount`=@TotalAmount1 WHERE `Id`=@kId1;UPDATE `sys_order` SET `BuyerId`=NULL,`OrderNo`=CONCAT('ON_',`OrderNo`),`TotalAmount`=@TotalAmount2 WHERE `Id`=@kId2");
    //}
}
