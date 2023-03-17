using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using Xunit;

namespace Trolley.Test;

public class MySqlWhereUnitTest
{
    private readonly IOrmDbFactory dbFactory;
    public MySqlWhereUnitTest()
    {
        var services = new ServiceCollection();
        services.AddSingleton(f =>
        {
            var builder = new OrmDbFactoryBuilder()
            .Register("fengling", true, f =>
            {
                var connectionString = "Server=localhost;Database=fengling;Uid=root;password=123456;charset=utf8mb4;";
                f.Add<MySqlProvider>(connectionString, true);
            })
            .AddTypeHandler<JsonTypeHandler>()
            .Configure<MySqlProvider, MySqlModelConfiguration>();
            return builder.Build();
        });
        var serviceProvider = services.BuildServiceProvider();
        this.dbFactory = serviceProvider.GetService<IOrmDbFactory>();
    }
    [Fact]
    public async void WhereMemberVisit()
    {
        this.Initialize();
        using var repository = this.dbFactory.Create();
        var result1 = await repository.QueryAsync<User>(f => f.IsEnabled);
        Assert.True(result1.Count > 0);
        var result2 = await repository.QueryAsync<User>(f => f.IsEnabled == true);
        Assert.True(result2.Count > 0);
        Assert.True(result1.Count == result2.Count);
    }



    private void Initialize()
    {
        using var repository = this.dbFactory.Create();
        repository.BeginTransaction();
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
                Price =550,
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
                Price =350,
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
                Price =180,
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
                Products = new List<int>{1, 2},
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
                Products = new List<int>{1, 3},
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
                Products = new List<int>{2},
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
        repository.Commit();
    }
}
