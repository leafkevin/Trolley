﻿using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using Xunit;

namespace Trolley.Tests;

public class MySqlUnitTest2
{
    private readonly IOrmDbFactory dbFactory;
    public MySqlUnitTest2()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IOrmProvider, MySqlProvider>();
        services.AddSingleton<IOrmDbFactory, OrmDbFactory>(f =>
        {
            var dbFactory = new OrmDbFactory(f);
            var connString = "Server=localhost;Database=fengling;Uid=root;password=123456;charset=utf8mb4;";
            dbFactory.Register("mysql", true, f => f.Add<MySqlProvider>(connString, true));
            dbFactory.Configure(f => new ModelConfiguration().OnModelCreating(f));
            return dbFactory;
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
    public async void QueryFirst()
    {
        using var repository = this.dbFactory.Create();
        var result = repository.QueryFirst<User>(f => f.Id == 1);
        Assert.True(result.Name == "leafkevin");
        result = await repository.QueryFirstAsync<User>(f => f.Name == "leafkevin");
        Assert.True(result.Id == 1);
    }
    [Fact]
    public async void Query()
    {
        using var repository = this.dbFactory.Create();
        var result = await repository.QueryAsync<Product>(f => f.ProductNo.Contains("PN-00"));
        Assert.True(result.Count >= 3);
    }
    [Fact]
    public async void FromQuery_Include()
    {
        using var repository = this.dbFactory.Create();
        var result = await repository.From<Product>()
            .Include(f => f.Brand)
            .Where(f => f.ProductNo.Contains("PN-00"))
            .ToListAsync();
        Assert.True(result.Count >= 3);
        Assert.True(result[0].Brand.BrandNo == "BN-001");
        Assert.True(result[1].Brand.BrandNo == "BN-002");
    }
    [Fact]
    public async void FromQuery_IncludeMany()
    {
        using var repository = this.dbFactory.Create();
        var result = repository.From<Order>()
            .Include(f => f.Details)
            .InnerJoin<User>((a, b) => a.BuyerId == b.Id)
            .Where((a, b) => a.TotalAmount > 300)
            .Select((x, y) => new { Order = x, Buyer = y })
            .ToList();
        Assert.True(result.Count == 2);
    }
    //[Fact]
    //public async void Insert_RawSql()
    //{
    //    using var repository = this.dbFactory.Create();
    //    repository.Delete<Brand>().Where(new[] { new { Id = 1 }, new { Id = 2 }, new { Id = 3 } }).Execute();
    //    var rawSql = "INSERT INTO sys_brand(Id,BrandNo,Name,IsEnabled,CreatedAt,CreatedBy,UpdatedAt,UpdatedBy) VALUES(@Id,@BrandNo,@Name,1,NOW(),@User,NOW(),@User)";
    //    var count = await repository.Create<Brand>().RawSql(rawSql, new
    //    {
    //        Id = 1,
    //        BrandNo = "BN-001",
    //        Name = "波司登",
    //        User = 1
    //    }).ExecuteAsync();
    //    Assert.Equal(1, count);
    //    count = await repository.Create<Brand>().RawSql(rawSql, new
    //    {
    //        Id = 2,
    //        BrandNo = "BN-002",
    //        Name = "雪中飞",
    //        User = 1
    //    }).ExecuteAsync();
    //    Assert.Equal(1, count);
    //    count = await repository.Create<Brand>().RawSql(rawSql, new
    //    {
    //        Id = 3,
    //        BrandNo = "BN-003",
    //        Name = "优衣库",
    //        User = 1
    //    }).ExecuteAsync();
    //    Assert.Equal(1, count);
    //}
}